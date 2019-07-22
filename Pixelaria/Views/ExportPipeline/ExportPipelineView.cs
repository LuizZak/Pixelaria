/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Forms;
using System.Windows.Threading;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Rendering;
using PixUI;
using PixUI.Controls;

using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Data.Persistence;
using Pixelaria.DXSupport;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps;
using Pixelaria.Properties;
using Pixelaria.Views.Direct2D;
using Pixelaria.Views.ExportPipeline.PipelineNodePanel;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI.Animation;
using Font = System.Drawing.Font;
using FontFamily = System.Drawing.FontFamily;

namespace Pixelaria.Views.ExportPipeline
{
    public partial class ExportPipelineView : PxlRenderForm
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
        
        private ExportPipelineNodesPanelManager _panelManager;
        private BitmapPreviewPipelineWindowManager _previewManager;

        private PropertiesPanel _propertiesPanel;

        private Direct2DRenderLoopManager _direct2DLoopManager;

        public ExportPipelineView()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release all Direct2D resources
                _direct2DLoopManager?.Dispose();

                _disposeBag.Dispose();
                _panelManager?.Dispose();
                _propertiesPanel?.Dispose();

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            StartDirect2DLoop();
        }

        public void StartDirect2DLoop()
        {
            if (DesignMode)
                return;

            _direct2DLoopManager = new Direct2DRenderLoopManager(exportPipelineControl, DxSupport.D2DFactory, DxSupport.D3DDevice);

            _direct2DLoopManager.Initialize();

            exportPipelineControl.InitializeRenderer(exportPipelineControl.RendererManager, _direct2DLoopManager.RenderingState);
            ConfigureForm();

            _direct2DLoopManager.InvalidatedState += (sender, args) =>
            {
                exportPipelineControl.InvalidateState();
            };

            _direct2DLoopManager.StartRenderLoop(state =>
            {
                var rects = exportPipelineControl.ClippingRegionRectangles;

                exportPipelineControl.Render(exportPipelineControl.RendererManager, _direct2DLoopManager.RenderingState);

                var redrawRects =
                    rects.Select(rect =>
                    {
                        int x = (int) Math.Floor(rect.X);
                        int y = (int) Math.Floor(rect.Y);

                        int width = (int) Math.Ceiling(rect.Width);
                        int height = (int) Math.Ceiling(rect.Height);

                        return new Rectangle(x, y, width, height);
                    }).ToArray();

                return new Direct2DRenderLoopResponse(redrawRects);
            });
        }

        #region Form Configuration

        private void ConfigureForm()
        {
            // InitTest();

            ControlView.TextLayoutRenderer = new Direct2DRenderer();

            ConfigurePipelineControl();
            ConfigureNodesPanel();
            ConfigurePropertiesPanel();
            ConfigurePreviewManager(_propertiesPanel);
        }

        private void ConfigurePipelineControl()
        {
            PipelineControlConfigurator.Configure(exportPipelineControl, _direct2DLoopManager.RenderingState);
        }

        private void ConfigureNodesPanel()
        {
            _panelManager = new ExportPipelineNodesPanelManager(exportPipelineControl);
            _panelManager.RegisterResizeEvent(exportPipelineControl);

            _panelManager.PipelineNodeSelected += PanelManagerOnPipelineNodeSelected;

            // Add nodes from the default provider
            var provider = new DefaultPipelineNodeSpecsProvider();

            _panelManager.LoadCreatablePipelineNodes(provider.GetNodeSpecs());
        }

        private void ConfigurePropertiesPanel()
        {
            _propertiesPanel = new PropertiesPanel(exportPipelineControl);
        }

        private void ConfigurePreviewManager([NotNull] PropertiesPanel propertiesPanel)
        {
            var manager = new BitmapPreviewPipelineWindowManager(exportPipelineControl)
            {
                ScreenInsetBounds = new InsetBounds(5, 5, 5, propertiesPanel.PanelWidth + 5)
            };

            exportPipelineControl.AddFeature(manager);

            _previewManager = manager;
        }

        #endregion
        
        private void PanelManagerOnPipelineNodeSelected(object sender, [NotNull] ExportPipelineNodesPanelManager.PipelineNodeSelectedEventArgs e)
        {
            exportPipelineControl.PipelineContainer.ClearSelection();
            AddPipelineNode(e.Node, e.ScreenPosition);
        }

        public void AddPipelineNode([NotNull] IPipelineNode node, Vector? screenPosition)
        {
            var container = exportPipelineControl.PipelineContainer;

            // Rename bitmap preview steps w/ numbers so they are easily identifiable
            if (node is BitmapPreviewPipelineStep bitmapPreview)
            {
                bool HasPreviewWithName(string name)
                {
                    return container.Nodes.OfType<BitmapPreviewPipelineStep>().Any(n => n.Name == name);
                }

                int count =
                    container.Nodes.OfType<BitmapPreviewPipelineStep>().Count() + 1;
                
                // Ensure unique names
                while (HasPreviewWithName($"Bitmap Preview #{count}"))
                    count += 1;

                bitmapPreview.Name = $"Bitmap Preview #{count}";
            }

            var view = PipelineNodeView.Create(node);
            view.Icon = ExportPipelineNodesPanelManager.IconForPipelineNode(node, exportPipelineControl.ImageResources);

            container.AddNodeView(view);
            container.AutoSizeNode(view);
            
            // Automatically adjust view to be on center of view port, if no location was informed
            if (screenPosition == null)
            {
                var center = exportPipelineControl.Bounds.Center();
                var centerCont = container.ContentsView.ConvertFrom(center, null);

                view.Location = centerCont - view.Size / 2;
            }
            else
            {
                view.Location = container.ContentsView.ConvertFrom(screenPosition.Value, null);
            }
        }

        public void InitTest()
        {
            var anim = new Animation("Anim 1", 48, 48);

            var animNodeView = PipelineNodeView.Create(new SingleAnimationPipelineStep(anim), node =>
            {
                node.Location = new Vector(0, 0);
                node.Icon = exportPipelineControl.ImageResources.GetImageResource("anim_icon");
            });
            var animJoinerNodeView = PipelineNodeView.Create(new AnimationJoinerStep(), node =>
            {
                node.Location = new Vector(350, 30);
            });
            var sheetNodeView = PipelineNodeView.Create(new SpriteSheetGenerationPipelineStep(), node =>
            {
                node.Location = new Vector(450, 30);
                node.Icon = exportPipelineControl.ImageResources.GetImageResource("sheet_new");
            });
            var fileExportView = PipelineNodeView.Create(new FileExportPipelineStep(), node =>
            {
                node.Location = new Vector(550, 30);
                node.Icon = exportPipelineControl.ImageResources.GetImageResource("sheet_save_icon");
            });
            var traspFilter = PipelineNodeView.Create(new TransparencyFilterPipelineStep(), node =>
            {
                node.Location = new Vector(550, 30);
                node.Icon = exportPipelineControl.ImageResources.GetImageResource("filter_transparency_icon");
            });

            exportPipelineControl.PipelineContainer.AddNodeView(animNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(animJoinerNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(sheetNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(fileExportView);
            exportPipelineControl.PipelineContainer.AddNodeView(traspFilter);

            exportPipelineControl.PipelineContainer.AutoSizeNodes();

            //TestThingsAndStuff();
        }

        void TestThingsAndStuff()
        {
            var bundle = new Bundle("abc");
            var anim1 = new Animation("Anim 1", 48, 48);
            var controller = new AnimationController(bundle, anim1);
            
            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var anim2 = new Animation("Anim 2", 64, 64);
            controller = new AnimationController(bundle, anim2);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var anim3 = new Animation("Anim 3", 80, 80);
            controller = new AnimationController(bundle, anim3);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var animSteps = new[]
            {
                new SingleAnimationPipelineStep(anim1),
                new SingleAnimationPipelineStep(anim2),
                new SingleAnimationPipelineStep(anim3)
            };

            var animJoinerStep = new AnimationJoinerStep();

            var exportSettings = new AnimationSheetExportSettings
            {
                FavorRatioOverArea = true, AllowUnorderedFrames = true, ExportJson = false, ForceMinimumDimensions = false, ForcePowerOfTwoDimensions = false,
                HighPrecisionAreaMatching = false, ReuseIdenticalFramesArea = false
            };

            var sheetSettingsOutput = new StaticPipelineOutput<AnimationSheetExportSettings>(exportSettings, "Sheet Export Settings");

            var sheetStep = new SpriteSheetGenerationPipelineStep();

            var finalStep = new FileExportPipelineStep();

            // Link stuff
            foreach (var step in animSteps)
            {
                step.ConnectTo(animJoinerStep);
            }

            animJoinerStep.ConnectTo(sheetStep);
            sheetStep.SheetSettingsInput.Connect(sheetSettingsOutput);

            sheetStep.ConnectTo(finalStep);

            finalStep.Begin();
        }

        private void tsb_sortSelected_Click(object sender, EventArgs e)
        {
            exportPipelineControl.PipelineContainer.PerformAction(new SortSelectedViewsAction(exportPipelineControl.AnimationsManager, true));
        }

        private void tab_open_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = @"Pixelaria files (*.pxl)|*.pxl"};

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;
            
            var bundle = PixelariaSaverLoader.LoadBundleFromDisk(ofd.FileName);
            Debug.Assert(bundle != null, "bundle != null");
            
            exportPipelineControl.SetPanAndZoom(Vector.Zero, Vector.Unit);

            exportPipelineControl.PipelineContainer.ContentsView.Scale = Vector.Unit;
            exportPipelineControl.PipelineContainer.ContentsView.Location = Vector.Zero;

            BundlePipelineLoader.Load(bundle, exportPipelineControl);
        }
    }

    internal class BundlePipelineLoader
    {
        public static void Load([NotNull] Bundle bundle, [NotNull] ExportPipelineControl exportPipelineControl)
        {
            exportPipelineControl.PipelineContainer.RemoveAllViews();
            
            exportPipelineControl.SuspendLayout();

            // Add export node from which all sheet steps will derive to
            var exportStep = new FileExportPipelineStep();
            exportPipelineControl.PipelineContainer.AddNodeView(PipelineNodeView.Create(exportStep, node =>
                {
                    node.Icon = exportPipelineControl.ImageResources.GetImageResource("sheet_save_icon");
                }));

            // Anim steps for animations w/ no owners
            foreach (var animation in bundle.Animations.Where(anim => bundle.GetOwningAnimationSheet(anim) == null))
            {
                var node = new SingleAnimationPipelineStep(animation);
                var step = PipelineNodeView.Create(node);
                step.Icon = exportPipelineControl.ImageResources.GetImageResource("anim_icon");

                exportPipelineControl.PipelineContainer.AddNodeView(step);
            }
                
            foreach (var sheet in bundle.AnimationSheets)
            {
                var sheetStep = new SpriteSheetGenerationPipelineStep();

                var animsStep = new AnimationsPipelineStep(sheet.Animations);
                    
                exportPipelineControl.PipelineContainer.AddNodeView(PipelineNodeView.Create(sheetStep, node =>
                    {
                        node.Icon = exportPipelineControl.ImageResources.GetImageResource("sheet_new");
                    }));
                exportPipelineControl.PipelineContainer.AddNodeView(PipelineNodeView.Create(animsStep, node =>
                    {
                        node.Icon = exportPipelineControl.ImageResources.GetImageResource("anim_icon");
                    }));

                exportPipelineControl.PipelineContainer.AddConnection(animsStep, sheetStep);
                exportPipelineControl.PipelineContainer.AddConnection(sheetStep, exportStep);
            }

            exportPipelineControl.PipelineContainer.AutoSizeNodes();
            exportPipelineControl.PipelineContainer.PerformAction(new SortSelectedViewsAction(exportPipelineControl.AnimationsManager, false));

            exportPipelineControl.ResumeLayout();
        }
    }

    internal class PipelineControlConfigurator
    {
        public static void Configure([NotNull] IExportPipelineControl control, [NotNull] IRenderLoopState state)
        {
            ConfigureLabelSizeProvider(control);
            RegisterIcons(control.ImageResources, state);
        }

        private static void ConfigureLabelSizeProvider([NotNull] IExportPipelineControl control)
        {
            LabelView.DefaultLabelViewSizeProvider = control.LabelViewSizeProvider;
        }

        public static void RegisterIcons([NotNull] IImageResourceManager manager, [NotNull] IRenderLoopState state)
        {
            void AddImage(Bitmap bitmap, string name)
            {
                manager.AddImageResource(state, bitmap, name);
            }

            AddImage(Resources.anim_icon, "anim_icon");
            AddImage(Resources.sheet_new, "sheet_new");
            AddImage(Resources.sheet_save_icon, "sheet_save_icon");
            AddImage(Resources.filter_transparency_icon, "filter_transparency_icon");
            AddImage(Resources.filter_hue, "filter_hue");
            AddImage(Resources.filter_saturation, "filter_saturation");
            AddImage(Resources.filter_lightness, "filter_lightness");
            AddImage(Resources.filter_offset_icon, "filter_offset_icon");
            AddImage(Resources.filter_scale_icon, "filter_scale_icon");
            AddImage(Resources.filter_rotation_icon, "filter_rotation_icon");
            AddImage(Resources.filter_stroke, "filter_stroke");
        }
    }

    /// <summary>
    /// Actions to be performed on a pipeline container's contents
    /// </summary>
    internal interface IExportPipelineAction
    {
        void Perform([NotNull] IPipelineContainer container);
    }

    internal class SortSelectedViewsAction : IExportPipelineAction
    {
        private readonly AnimationsManager _animationsManager;
        private readonly bool _animated;

        public SortSelectedViewsAction(AnimationsManager animationsManager, bool animated)
        {
            _animationsManager = animationsManager;
            _animated = animated;
        }

        public void Perform(IPipelineContainer container)
        {
            var selectedNodes = container.SelectionModel.NodeViews();
            var nodeViews = selectedNodes.Length > 0 ? selectedNodes : container.NodeViews;

            // No work to do
            if (nodeViews.Length == 0)
                return;

            // Create a quick graph for the views
            var root = new DirectedAcyclicNode<PipelineNodeView>(null);
            var nodes = nodeViews.Select(nodeView => new DirectedAcyclicNode<PipelineNodeView>(nodeView)).ToList();

            // Record geometric center so we can re-center later
            var originalCenter =
                nodeViews.Select(nv => nv.Center)
                    .Aggregate(Vector.Zero, (v1, v2) => v1 + v2) / nodeViews.Length;
            
            // Deal with connections, now
            foreach (var node in nodes)
            {
                var view = node.Value;
                if (view == null)
                    continue;

                var previous = container.GetNodesGoingTo(view);
                var next = container.GetNodesGoingFrom(view);

                foreach (var prevView in previous)
                {
                    var prevNode = nodes.FirstOrDefault(n => Equals(n.Value, prevView));
                    prevNode?.AddChild(node);
                }

                foreach (var nextView in next)
                {
                    var nextNode = nodes.FirstOrDefault(n => Equals(n.Value, nextView));
                    if (nextNode != null)
                        node.AddChild(nextNode);
                }

                // For nodes w/ no parents, add root as their parents
                if (node.Previous.Count == 0)
                {
                    root.AddChild(node);
                }
            }

            var sorted = root.TopologicalSorted().ToArray();
            
            var originalPositions = new Dictionary<PipelineNodeView, Vector>();
            foreach (var node in sorted)
            {
                var view = node.Value;
                if (view == null)
                    continue;

                originalPositions[view] = view.Location;
            }

            // Organize the sortings, now
            float x = 0;
            foreach (var node in sorted)
            {
                var view = node.Value;
                if (view == null)
                    continue;

                view.Location = new Vector(x, 0);
                x += view.GetFullBounds().Width + 40;
            }

            // Organize Y coordinates
            for (int i = sorted.Length - 1; i >= 0; i--)
            {
                var node = sorted[i];
                var view = node.Value;
                if (view == null)
                    continue;

                var conTo = container.GetNodesGoingFrom(view).FirstOrDefault();
                if (conTo == null || !sorted.Any(n => Equals(n.Value, conTo)))
                    continue;

                var connections = container.ConnectedLinkViewsBetween(view, conTo);
                if (connections.Length == 0)
                    continue;

                var (linkFrom, linkTo) = connections[0];

                float globY = linkTo.ConvertTo(linkTo.Bounds.Center, container.ContentsView).Y;

                float targetY = globY - linkFrom.Center.Y;

                view.Location = new Vector(view.Location.X, targetY);
            }
            
            // Recenter around common origin
            if (selectedNodes.Length > 0)
            {
                var newCenter =
                    nodeViews.Select(nv => nv.Center)
                        .Aggregate(Vector.Zero, (v1, v2) => v1 + v2) / nodeViews.Length;

                foreach (var view in nodeViews)
                {
                    view.Center += originalCenter - newCenter;
                }
            }

            if (_animated)
            {
                var positions = new Dictionary<PipelineNodeView, Vector>();
                foreach (var view in originalPositions.Keys)
                {
                    positions[view] = view.Location;
                    view.Location = originalPositions[view];
                }

                var animation = new ClosureAnimation(0.3, TimingFunctions.EaseInOut, t =>
                {
                    foreach (var pair in positions)
                    {
                        var view = pair.Key;
                        var target = pair.Value;

                        view.Location = Vector.Lerp(originalPositions[view], target, (float)t);
                    }

                    // Update link connections
                    foreach (var view in nodeViews)
                    {
                        container.UpdateConnectionViewsFor(view);
                    }
                });

                _animationsManager.AddAnimation(animation);
            }
            else
            {
                // Update link connections
                foreach (var view in nodeViews)
                {
                    container.UpdateConnectionViewsFor(view);
                }
            }
        }
    }

    /// <summary>
    /// A 'picture-in-picture' style manager for bitmap preview pipeline steps.
    /// </summary>
    internal class BitmapPreviewPipelineWindowManager : ExportPipelineUiFeature, IRenderListener
    {
        [CanBeNull]
        private IRenderLoopState _latestRenderState;

        private readonly Font _font = new Font(FontFamily.GenericSansSerif, 12);

        private readonly List<BitmapPreviewPipelineStep> _previewSteps = new List<BitmapPreviewPipelineStep>();
        private readonly Dictionary<BitmapPreviewPipelineStep, IManagedImageResource> _latestPreviews = new Dictionary<BitmapPreviewPipelineStep, IManagedImageResource>();
        private readonly List<PreviewBounds> _previewBounds = new List<PreviewBounds>();
        private readonly InsetBounds _titleInset = new InsetBounds(5, 5, 5, 5);
        private InsetBounds _screenInsetBounds = new InsetBounds(5, 5, 5, 5);

        public InsetBounds ScreenInsetBounds
        {
            get => _screenInsetBounds;
            set
            {
                if (_screenInsetBounds == value)
                    return;

                _screenInsetBounds = value;
                ReloadBoundsCache();
            }
        }

        public int RenderOrder => RenderOrdering.UserInterface;

        public BitmapPreviewPipelineWindowManager([NotNull] IExportPipelineControl control) : base(control)
        {
            control.PipelineContainer.NodeAdded += PipelineContainerOnNodeAdded;
            control.PipelineContainer.NodeRemoved += PipelineContainerOnNodeRemoved;
            control.SizeChanged += ControlOnSizeChanged;

            control.RendererManager.AddRenderListener(this);
        }

        private void ControlOnSizeChanged(object sender, EventArgs e)
        {
            ReloadBoundsCache();
        }

        ~BitmapPreviewPipelineWindowManager()
        {
            foreach (var bitmap in _latestPreviews.Values)
            {
                bitmap?.Dispose();
            }

            _font.Dispose();
        }
        
        private void PipelineContainerOnNodeAdded(object sender, [NotNull] PipelineNodeViewEventArgs e)
        {
            if (!(e.Node.PipelineNode is BitmapPreviewPipelineStep step))
                return;

            AddPreview(step, e.Control);
        }

        private void PipelineContainerOnNodeRemoved(object sender, [NotNull] PipelineNodeViewEventArgs e)
        {
            if (!(e.Node.PipelineNode is BitmapPreviewPipelineStep step))
                return;

            RemovePreview(step, e.Control);
        }

        private void OnBitmapStepOnRenamed(object sender, EventArgs args)
        {
            ReloadBoundsCache();
            Control.InvalidateAll();
        }

        private void AddPreview([NotNull] BitmapPreviewPipelineStep step, [NotNull] IExportPipelineControl control)
        {
            _previewSteps.Add(step);
            _latestPreviews[step] = null;

            ReloadBoundsCache();

            step.OnReceive = bitmap =>
            {
                UpdatePreview(step, bitmap, control);
            };

            step.Renamed += OnBitmapStepOnRenamed;

            control.InvalidateAll();
        }

        private void RemovePreview([NotNull] BitmapPreviewPipelineStep step, [NotNull] IExportPipelineControl control)
        {
            control.InvalidateAll();

            step.Renamed -= OnBitmapStepOnRenamed;

            _previewSteps.Remove(step);
            _latestPreviews[step]?.Dispose();
            _latestPreviews.Remove(step);

            ReloadBoundsCache();
        }

        private void UpdatePreview([NotNull] BitmapPreviewPipelineStep step, Bitmap bitmap, [NotNull] IExportPipelineControl control)
        {
            if (_latestRenderState == null)
                return;

            IManagedImageResource managedBitmap;

            if (_latestPreviews.TryGetValue(step, out var old))
            {
                managedBitmap = old;
                control.ImageResources.UpdateManagedImageResource(_latestRenderState, ref managedBitmap, bitmap);
            }
            else
            {
                managedBitmap = control.ImageResources.CreateManagedImageResource(_latestRenderState, bitmap);
            }

            _latestPreviews[step] = managedBitmap;
            
            ReloadBoundsCache();

            control.InvalidateAll();
        }

        public void RecreateState(IRenderLoopState state)
        {
            _latestRenderState = state;
        }

        public void Render(IRenderListenerParameters parameters)
        {
            var state = parameters.State;

            _latestRenderState = state;

            for (int i = 0; i < _previewSteps.Count; i++)
            {
                var step = _previewSteps[i];

                var bounds = BoundsForPreview(i);

                _latestPreviews.TryGetValue(step, out var bitmap);

                // Draw image, or opaque background
                if (bitmap != null)
                {
                    parameters.Renderer.DrawBitmap(bitmap, bounds.ImageBounds, 1, ImageInterpolationMode.Linear);
                }
                else
                {
                    parameters.Renderer.SetFillColor(Color.DimGray);
                    parameters.Renderer.FillArea(bounds.ImageBounds);
                }

                parameters.Renderer.SetStrokeColor(Color.Gray);
                parameters.Renderer.StrokeArea(bounds.ImageBounds);

                // Draw title
                var format = new TextFormatAttributes(_font.FontFamily.Name, _font.Size)
                {
                    TextEllipsisTrimming = new TextEllipsisTrimming
                    {
                        Granularity = TextTrimmingGranularity.Character
                    }
                };

                parameters.Renderer.SetFillColor(Color.Black);
                parameters.Renderer.FillArea(bounds.TitleBounds);

                parameters.TextRenderer.Draw(step.Name, format, bounds.TitleBounds.Inset(_titleInset), Color.White);
            }
        }

        private void ReloadBoundsCache()
        {
            _previewBounds.Clear();
            
            float y = 0;

            foreach (var step in _previewSteps)
            {
                string name = step.Name;

                var nameSize = Control.LabelViewSizeProvider.CalculateTextSize(name, _font);
                nameSize.Width += _titleInset.Left + _titleInset.Right;
                nameSize.Height += _titleInset.Top + _titleInset.Bottom;

                _latestPreviews.TryGetValue(step, out var bitmap);

                var size = new Vector(120, 90);
                if (bitmap != null)
                    size = new Vector(120 * ((float) bitmap.Width / bitmap.Height), 90);

                var availableBounds = 
                    AABB.FromRectangle(Vector.Zero, Control.Size)
                        .Inset(ScreenInsetBounds);

                var titleBounds = AABB.FromRectangle(availableBounds.Width - size.X,
                    availableBounds.Height - y - size.Y - nameSize.Height, 
                    size.X,
                    nameSize.Height);

                var bounds = AABB.FromRectangle(availableBounds.Width - size.X, 
                    availableBounds.Height - y - size.Y,
                    size.X,
                    size.Y);

                var previewBounds = new PreviewBounds(titleBounds, bounds);

                y += previewBounds.TotalBounds.Height + 5;

                _previewBounds.Add(previewBounds);
            }
        }

        private PreviewBounds BoundsForPreview(int index)
        {
            return _previewBounds[index];
        }

        private struct PreviewBounds
        {
            public AABB TotalBounds => TitleBounds.Union(ImageBounds);
            public AABB TitleBounds { get; }
            public AABB ImageBounds { get; }

            public PreviewBounds(AABB titleBounds, AABB imageBounds)
            {
                TitleBounds = titleBounds;
                ImageBounds = imageBounds;
            }
        }
    }
}

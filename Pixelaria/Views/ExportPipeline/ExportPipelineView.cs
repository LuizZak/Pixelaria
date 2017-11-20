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
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Forms;
using System.Windows.Threading;
using JetBrains.Annotations;

using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Factory = SharpDX.DXGI.Factory;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

using Resource = SharpDX.Direct3D11.Resource;
using Device = SharpDX.Direct3D11.Device;

using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Data.Persistence;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps;
using Pixelaria.Properties;
using Pixelaria.Utils;
using Pixelaria.Views.Direct2D;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Pixelaria.Views.ExportPipeline.PipelineView;
using Pixelaria.Views.ExportPipeline.PipelineView.Controls;

namespace Pixelaria.Views.ExportPipeline
{
    public partial class ExportPipelineView : PxlRenderForm
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
        
        private ExportPipelineNodesPanelManager _panelManager;
        private BitmapPreviewPipelineWindowManager _previewManager;

        private readonly Stopwatch _frameDeltaTimer = new Stopwatch();

        internal Direct2DRenderingState RenderingState { get; } = new Direct2DRenderingState();

        public ExportPipelineView()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            InitializeComponent();

            exportPipelineControl.Resize += ExportPipelineControlOnResize;
            exportPipelineControl.BackColor = exportPipelineControl.D2DRenderer.BackColor;
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
                if (RenderingState != null)
                {
                    // Release all resources
                    RenderingState.RenderTargetView?.Dispose();
                    RenderingState.BackBuffer?.Dispose();
                    RenderingState.Device?.ImmediateContext.ClearState();
                    RenderingState.Device?.ImmediateContext.Flush();
                    RenderingState.Device?.Dispose();
                    RenderingState.SwapChain?.Dispose();
                    RenderingState.Factory?.Dispose();
                }

                _disposeBag.Dispose();
                _panelManager?.Dispose();

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            InitializeDirect2D();
        }

        public void ConfigureForm()
        {
            ControlView.DirectWriteFactory = RenderingState.DirectWriteFactory;

            ConfigurePipelineControl();
            ConfigureNodesPanel();
            ConfigurePreviewManager();
        }

        public void ConfigurePipelineControl()
        {
            LabelView.DefaultLabelViewSizeProvider = exportPipelineControl.D2DRenderer;
            var imageResources = exportPipelineControl.D2DRenderer.ImageResources;

            void AddImage(System.Drawing.Bitmap bitmap, string name)
            {
                imageResources.AddImageResource(RenderingState, bitmap, name);
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

        public void ConfigureNodesPanel()
        {
            // Add controls
            var control = new ControlViewFeature(exportPipelineControl);
            exportPipelineControl.AddFeature(control);

            _panelManager = new ExportPipelineNodesPanelManager(exportPipelineControl, control);

            _panelManager.PipelineNodeSelected += PanelManagerOnPipelineNodeSelected;

            // Add nodes from the default provider
            var provider = new DefaultPipelineNodeSpecsProvider();

            _panelManager.LoadCreatablePipelineNodes(provider.GetNodeSpecs());
        }

        public void ConfigurePreviewManager()
        {
            var manager = new BitmapPreviewPipelineWindowManager(exportPipelineControl);
            exportPipelineControl.AddFeature(manager);

            _previewManager = manager;
        }

        private void ExportPipelineControlOnResize(object sender, EventArgs eventArgs)
        {
            ResetSwapChain();
        }

        private void PanelManagerOnPipelineNodeSelected(object sender, [NotNull] ExportPipelineNodesPanelManager.PipelineNodeSelectedEventArgs e)
        {
            AddPipelineNode(e.Node);
        }

        public void AddPipelineNode([NotNull] IPipelineNode node)
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

            var view = new PipelineNodeView(node)
            {
                Icon = ExportPipelineNodesPanelManager.IconForPipelineNode(node, exportPipelineControl.D2DRenderer.ImageResources)
            };

            container.AddNodeView(view);
            container.AutosizeNode(view);
            
            // Automatically adjust view to be on center of view port
            var center = exportPipelineControl.Bounds.Center();
            var centerCont = container.ContentsView.ConvertFrom(center, null);

            view.Location = centerCont - view.Size / 2;
        }

        public void InitTest()
        {
            var anim = new Animation("Anim 1", 48, 48);

            var animNodeView = new PipelineNodeView(new SingleAnimationPipelineStep(anim))
            {
                Location = new Vector(0, 0),
                Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("anim_icon")
            };
            var animJoinerNodeView = new PipelineNodeView(new AnimationJoinerStep())
            {
                Location = new Vector(350, 30)
            };
            var sheetNodeView = new PipelineNodeView(new SpriteSheetGenerationPipelineStep())
            {
                Location = new Vector(450, 30),
                Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("sheet_new")
            };
            var fileExportView = new PipelineNodeView(new FileExportPipelineStep())
            {
                Location = new Vector(550, 30),
                Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("sheet_save_icon")
            };
            var traspFilter = new PipelineNodeView(new TransparencyFilterPipelineStep())
            {
                Location = new Vector(550, 30),
                Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("filter_transparency_icon")
            };

            exportPipelineControl.PipelineContainer.AddNodeView(animNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(animJoinerNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(sheetNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(fileExportView);
            exportPipelineControl.PipelineContainer.AddNodeView(traspFilter);

            exportPipelineControl.PipelineContainer.AutosizeNodes();

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
            exportPipelineControl.PipelineContainer.PerformAction(new SortSelectedViewsAction());
        }

        private void tab_open_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = @"Pixelaria files (*.pxl)|*.pxl"};

            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            
            exportPipelineControl.PipelineContainer.RemoveAllViews();

            exportPipelineControl.SetPanAndZoom(Vector.Zero, Vector.Unit);

            exportPipelineControl.PipelineContainer.ContentsView.Scale = Vector.Unit;
            exportPipelineControl.PipelineContainer.ContentsView.Location = Vector.Zero;

            var bundle = PixelariaSaverLoader.LoadBundleFromDisk(ofd.FileName);

            Debug.Assert(bundle != null, "bundle != null");

            exportPipelineControl.SuspendLayout();

            // Add export node from which all sheet steps will derive to
            var exportStep = new FileExportPipelineStep();
            exportPipelineControl.PipelineContainer.AddNodeView(new PipelineNodeView(exportStep)
            {
                Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("sheet_save_icon")
            });

            // Anim steps for animations w/ no owners
            foreach (var animation in bundle.Animations.Where(anim => bundle.GetOwningAnimationSheet(anim) == null))
            {
                var node = new SingleAnimationPipelineStep(animation);
                var step = new PipelineNodeView(node)
                {
                    Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("anim_icon")
                };

                exportPipelineControl.PipelineContainer.AddNodeView(step);
            }
                
            foreach (var sheet in bundle.AnimationSheets)
            {
                var sheetStep = new SpriteSheetGenerationPipelineStep();

                var animsStep = new AnimationsPipelineStep(sheet.Animations);
                    
                exportPipelineControl.PipelineContainer.AddNodeView(new PipelineNodeView(sheetStep)
                {
                    Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("sheet_new")
                });
                exportPipelineControl.PipelineContainer.AddNodeView(new PipelineNodeView(animsStep)
                {
                    Icon = exportPipelineControl.D2DRenderer.ImageResources.PipelineNodeImageResource("anim_icon")
                });

                exportPipelineControl.PipelineContainer.AddConnection(animsStep, sheetStep);
                exportPipelineControl.PipelineContainer.AddConnection(sheetStep, exportStep);
            }

            exportPipelineControl.PipelineContainer.AutosizeNodes();

            exportPipelineControl.PipelineContainer.PerformAction(new SortSelectedViewsAction());
        }

        #region Direct2D Setup

        public void InitializeDirect2D()
        {
            if (DesignMode)
                return;

            // SwapChain description
            var desc = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(exportPipelineControl.Width, exportPipelineControl.Height,
                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = exportPipelineControl.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out RenderingState.Device, out RenderingState.SwapChain);

            RenderingState.D2DFactory = new SharpDX.Direct2D1.Factory();
            RenderingState.DirectWriteFactory = new SharpDX.DirectWrite.Factory();

            // Ignore all windows events
            RenderingState.Factory = RenderingState.SwapChain.GetParent<Factory>();
            RenderingState.Factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            RenderingState.BackBuffer = Resource.FromSwapChain<Texture2D>(RenderingState.SwapChain, 0);
            RenderingState.RenderTargetView = new RenderTargetView(RenderingState.Device, RenderingState.BackBuffer);

            RenderingState.DxgiSurface = RenderingState.BackBuffer.QueryInterface<Surface>();

            var settings = new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied));

            RenderingState.D2DRenderTarget =
                new RenderTarget(RenderingState.D2DFactory, RenderingState.DxgiSurface, settings)
                {
                    TextAntialiasMode = TextAntialiasMode.Cleartype
                };
            exportPipelineControl.InitializeDirect2DRenderer(RenderingState);

            ConfigureForm();
            //InitTest();

            using (var renderLoop = new RenderLoop(this) { UseApplicationDoEvents = true })
            {
                _frameDeltaTimer.Start();

                while (renderLoop.NextFrame())
                {
                    if (_frameDeltaTimer.ElapsedMilliseconds <= 16)
                        continue;

                    RenderingState.SetFrameDeltaTime(TimeSpan.FromTicks(_frameDeltaTimer.ElapsedTicks));
                    _frameDeltaTimer.Stop();
                    _frameDeltaTimer.Reset();
                    _frameDeltaTimer.Start();

                    RenderingState.D2DRenderTarget.BeginDraw();

                    exportPipelineControl.RenderDirect2D(RenderingState);

                    RenderingState.D2DRenderTarget.EndDraw();

                    RenderingState.SwapChain.Present(0, PresentFlags.None);
                }
            }
        }

        private void ResetSwapChain()
        {
            if (RenderingState.SwapChain == null)
                return;

            RenderingState.Device.ImmediateContext.ClearState();

            RenderingState.DxgiSurface.Dispose();
            RenderingState.D2DRenderTarget.Dispose();
            RenderingState.RenderTargetView.Dispose();
            RenderingState.BackBuffer.Dispose();

            RenderingState.SwapChain.ResizeBuffers(RenderingState.SwapChain.Description.BufferCount,
                exportPipelineControl.Width, exportPipelineControl.Height,
                RenderingState.SwapChain.Description.ModeDescription.Format,
                RenderingState.SwapChain.Description.Flags);

            RenderingState.BackBuffer = Resource.FromSwapChain<Texture2D>(RenderingState.SwapChain, 0);
            RenderingState.RenderTargetView = new RenderTargetView(RenderingState.Device, RenderingState.BackBuffer);
            RenderingState.DxgiSurface = RenderingState.BackBuffer.QueryInterface<Surface>();
            RenderingState.D2DRenderTarget = new RenderTarget(RenderingState.D2DFactory, RenderingState.DxgiSurface,
                new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
        }

        #endregion
    }

    /// <summary>
    /// Actions to be performed on a pipeline container's contents
    /// </summary>
    internal interface IExportPipelineAction
    {
        void Perform([NotNull] ExportPipelineControl.IPipelineContainer container);
    }

    internal class SortSelectedViewsAction : IExportPipelineAction
    {
        public void Perform(ExportPipelineControl.IPipelineContainer container)
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
            
            // Organize the sortings, now
            float x = 0;
            foreach (var node in sorted)
            {
                var view = node.Value;
                if (view == null)
                    continue;

                view.Location = new Vector(x, 0);
                x += view.GetFullBounds().Width + 40;

                container.UpdateConnectionViewsFor(view);
            }

            // Organize Y coordinates
            for (int i = sorted.Length - 1; i >= 0; i--)
            {
                var node = sorted[i];
                var view = node.Value;
                if (view == null)
                    continue;

                var conTo = container.GetNodesGoingFrom(view).FirstOrDefault();
                if (conTo == null)
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

            // Update link connections
            foreach (var view in nodeViews)
            {
                container.UpdateConnectionViewsFor(view);
            }
        }
    }

    /// <summary>
    /// A 'picture-in-picture' style manager for bitmap preview pipeline steps.
    /// </summary>
    internal class BitmapPreviewPipelineWindowManager : ExportPipelineUiFeature
    {
        [CanBeNull]
        private Direct2DRenderingState _latestRenderState;

        private readonly List<BitmapPreviewPipelineStep> _previewSteps = new List<BitmapPreviewPipelineStep>();
        private readonly Dictionary<BitmapPreviewPipelineStep, Bitmap> _latestPreviews = new Dictionary<BitmapPreviewPipelineStep, Bitmap>();

        public BitmapPreviewPipelineWindowManager([NotNull] ExportPipelineControl control) : base(control)
        {
            control.PipelineContainer.NodeAdded += PipelineContainerOnNodeAdded;
            control.PipelineContainer.NodeRemoved += PipelineContainerOnNodeRemoved;
        }

        ~BitmapPreviewPipelineWindowManager()
        {
            foreach (var bitmap in _latestPreviews.Values)
            {
                bitmap?.Dispose();
            }
        }
        
        private void PipelineContainerOnNodeAdded(object sender, [NotNull] PipelineNodeViewEventArgs e)
        {
            if (!(e.Node.PipelineNode is BitmapPreviewPipelineStep step))
                return;

            AddPreview(step);
        }

        private void PipelineContainerOnNodeRemoved(object sender, [NotNull] PipelineNodeViewEventArgs e)
        {
            if (!(e.Node.PipelineNode is BitmapPreviewPipelineStep step))
                return;

            RemovePreview(step);
        }

        private void AddPreview([NotNull] BitmapPreviewPipelineStep step)
        {
            _previewSteps.Add(step);
            _latestPreviews[step] = null;

            step.OnReceive = bitmap =>
            {
                UpdatePreview(step, bitmap);
            };
        }

        private void RemovePreview([NotNull] BitmapPreviewPipelineStep step)
        {
            _previewSteps.Remove(step);
            _latestPreviews.Remove(step);
        }

        private void UpdatePreview([NotNull] BitmapPreviewPipelineStep step, System.Drawing.Bitmap bitmap)
        {
            if (_latestRenderState == null)
                return;

            if(_latestPreviews.TryGetValue(step, out Bitmap old))
                old.Dispose();

            var newBit = Direct2DRenderer.CreateSharpDxBitmap(_latestRenderState.D2DRenderTarget, bitmap);

            _latestPreviews[step] = newBit;
        }

        public override void OnRender(Direct2DRenderingState state)
        {
            base.OnRender(state);

            _latestRenderState = state;

            float y = 0;

            foreach (var step in _previewSteps)
            {
                _latestPreviews.TryGetValue(step, out var bitmap);
                
                var size = new Vector(120, 90);
                if(bitmap != null)
                    size = new Vector(120, 120 * ((float)bitmap.PixelSize.Height / bitmap.PixelSize.Width));

                var availableBounds = 
                    AABB.FromRectangle(Vector.Zero, Control.Size)
                    .Inset(new InsetBounds(5, 5, 5, 5));

                var bounds = AABB.FromRectangle(availableBounds.Width - size.X, availableBounds.Height - y - size.Y, size.X, size.Y);

                // Draw image, or opaque background
                if (bitmap != null)
                {
                    state.D2DRenderTarget.DrawBitmap(bitmap, bounds, 1, BitmapInterpolationMode.Linear);
                }
                else
                {
                    using (var brush = new SolidColorBrush(state.D2DRenderTarget, System.Drawing.Color.DimGray.ToColor4()))
                    {
                        state.D2DRenderTarget.FillRectangle(bounds, brush);
                    }
                }

                using (var brush = new SolidColorBrush(state.D2DRenderTarget, System.Drawing.Color.Gray.ToColor4()))
                {
                    state.D2DRenderTarget.DrawRectangle(bounds, brush);
                }

                y += size.Y + 5;
            }
        }
    }
}

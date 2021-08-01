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
using PixDirectX.Rendering.DirectX;
using PixUI;
using PixUI.Controls;

using Pixelaria.Data.Persistence;
using Pixelaria.ExportPipeline;
using Pixelaria.Properties;
using Pixelaria.Utils;
using Pixelaria.Views.Direct2D;
using Pixelaria.Views.ExportPipeline.PipelineNodePanel;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixRendering;
using PixUI.Animation;

namespace Pixelaria.Views.ExportPipeline
{
    public partial class ExportPipelineView : PxlRenderForm
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();

        private ExportPipelineNodesPanelManager _panelManager;
        private BitmapPreviewPipelineWindowManager _previewManager;

        private PropertiesPanel _propertiesPanel;

        private IRendererStack _rendererStack;

        public ExportPipelineView()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release all Direct2D resources
                _disposeBag.Dispose();
                _panelManager?.Dispose();
                _propertiesPanel?.Dispose();

                components?.Dispose();

                _rendererStack?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Form Events

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            ConfigureAndRunRenderLoop();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            DetectMonitorRefreshRate();
        }

        #endregion

        /// <summary>
        /// Configures and executes a render loop on the current form.
        ///
        /// The method doesn't return while the run loop is active.
        /// </summary>
        public void ConfigureAndRunRenderLoop()
        {
            if (DesignMode)
                return;

            _rendererStack = new Direct2DRendererStack();
            var renderManager = _rendererStack.Initialize(exportPipelineControl);
            
            DetectMonitorRefreshRate();

            DefaultResources.LoadDefaultResources(renderManager.ImageResources);

            exportPipelineControl.InitializeRenderer(renderManager);
            exportPipelineControl.InvalidateAll();

            ConfigureForm(renderManager, _rendererStack.RenderingState ?? throw new InvalidOperationException("No initial rendering state available"));

            _rendererStack.ConfigureRenderLoop((state, clipping) =>
            {
                exportPipelineControl.UpdateFrameStep(state.FrameRenderDeltaTime);
                exportPipelineControl.FillRedrawRegion(clipping);
            });
        }

        public void DetectMonitorRefreshRate()
        {
            if (IsDisposed)
                return;

            var refreshRate = MonitorSettingsHelper.GetRefreshRateForForm(this);

            if (refreshRate != null)
            {
                _rendererStack?.ChangeRefreshRate(refreshRate.Value);
            }
        }

        #region Form Configuration

        private void ConfigureForm([NotNull] IRenderManager renderer, [NotNull] IRenderLoopState state)
        {
            // InitTest();

            ControlView.TextLayoutRenderer = new Direct2DRenderManager();

            ConfigurePipelineControl(state);
            ConfigureNodesPanel(renderer);
            ConfigurePropertiesPanel();
            ConfigurePreviewManager(renderer, _propertiesPanel);
        }

        private void ConfigurePipelineControl([NotNull] IRenderLoopState state)
        {
            PipelineControlConfigurator.Configure(exportPipelineControl, state);
        }

        private void ConfigureNodesPanel([NotNull] IRenderManager renderer)
        {
            _panelManager = new ExportPipelineNodesPanelManager(exportPipelineControl, renderer);
            _panelManager.RegisterResizeEvent(exportPipelineControl);

            _panelManager.PipelineNodeSelected += PanelManagerOnPipelineNodeSelected;
        }

        private void ConfigurePropertiesPanel()
        {
            _propertiesPanel = new PropertiesPanel(exportPipelineControl);
        }

        private void ConfigurePreviewManager([NotNull] IRenderManager renderer, [NotNull] PropertiesPanel propertiesPanel)
        {
            var manager = new BitmapPreviewPipelineWindowManager(exportPipelineControl, renderer)
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
            AddPipelineNode(e.NodeDescriptor, e.ScreenPosition);
        }

        public void AddPipelineNode([NotNull] PipelineNodeDescriptor descriptor, Vector? screenPosition)
        {
            var container = exportPipelineControl.PipelineContainer;

            var node = container.PipelineGraph.CreateNode(descriptor.NodeKind);
            if (!node.HasValue)
                return;
            var nodeView = container.PipelineGraph.GetViewForPipelineNode(node.Value);
            if (nodeView == null)
                return;

            var view = PipelineNodeView.Create(nodeView);
            view.Icon = ExportPipelineNodesPanelManager.IconForPipelineNodeKind(descriptor.NodeKind, exportPipelineControl.ImageResources);
            view.ManagedIcon = _panelManager.IconForPipelineNode(descriptor);

            // Rename bitmap preview steps w/ numbers so they are easily identifiable
            if (descriptor.NodeKind == PipelineNodeKinds.BitmapPreview)
            {
                var bitmapPreviewNodes = container.Nodes
                    .Select(n => container.PipelineGraph.GetViewForPipelineNode(n))
                    .Where(n => n != null)
                    .Where(n => n.NodeKind == PipelineNodeKinds.BitmapPreview)
                    .ToArray();

                bool HasPreviewWithName(string name)
                {
                    return bitmapPreviewNodes.Any(n => n.Title == name);
                }

                int count = bitmapPreviewNodes.Length + 1;

                // Ensure unique names
                while (HasPreviewWithName($"Bitmap Preview #{count}"))
                    count += 1;

                container.PipelineGraph.SetNodeTitle(node.Value, $"Bitmap Preview #{count}");
            }

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
            LabelView.defaultTextSizeProvider = control.TextSizeProvider;
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
}

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

using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixPipelineGraph;
using PixRendering;
using PixUI;
using PixUI.Animation;
using PixUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;

namespace Pixelaria.Views.ExportPipeline
{
    internal class ExportPipelineControl: UserControl, IExportPipelineControl
    {
        public AnimationsManager AnimationsManager { get; } = new AnimationsManager();

        private readonly InternalPipelineContainer _container;

        // Timer used to tick the fixed step OnFixedFrame method on each control feature added
        private readonly Timer _fixedTimer;

        private readonly InternalRenderListener _internalRenderer;

        private readonly List<ExportPipelineUiFeature> _features = new List<ExportPipelineUiFeature>();

        [CanBeNull]
        private ExportPipelineUiFeature _exclusiveControl;

        private readonly ClippingRegion _clippingRegion = new ClippingRegion();

        #region Intrinsic Features

        private SmoothViewPanAndZoomUiFeature _panAndZoom;
        private ControlViewFeature _controlViewFeature;

        #endregion

        /// <summary>
        /// Gets the default specs provider for this export pipeline
        /// </summary>
        public IPipelineGraphBodyProvider PipelineGraphBodyProvider { get; } = new DefaultPipelineGraphBodyProvider();

        /// <summary>
        /// Gets a set of rectangles that represent the invalidated redraw regions of this pipeline control.
        /// </summary>
        public IReadOnlyList<RectangleF> ClippingRegionRectangles => _clippingRegion.RedrawRegionRectangles(Size);

        /// <summary>
        /// Container for <see cref="ControlView"/>-based controls
        /// </summary>
        public IControlContainer ControlContainer => _controlViewFeature;

        /// <summary>
        /// Latest registered location of the mouse on this control.
        /// 
        /// Is updated on every mouse event handler.
        /// </summary>
        public Point MousePoint { get; private set; }

        /// <summary>
        /// Target for adding rendering decorators to
        /// </summary>
        public IRenderingDecoratorContainer RenderingDecoratorTarget => _internalRenderer;

        /// <summary>
        /// Gets the image resources provider for this pipeline control
        /// </summary>
        public IImageResourceManager ImageResources { get; set; }

        /// <summary>
        /// Gets the label size provider for this control
        /// </summary>
        public ITextSizeProvider TextSizeProvider { get; set; }

        /// <summary>
        /// Gets the label view metrics provider initialized for this control
        /// </summary>
        public ITextMetricsProvider TextMetricsProvider { get; set; }

        /// <summary>
        /// Gets the pipeline node and connections container for this control
        /// </summary>
        public IPipelineContainer PipelineContainer => _container;

        /// <summary>
        /// Gets or sets the sizer to apply to pipeline node views.
        /// </summary>
        public IPipelineNodeViewSizer PipelineNodeViewSizer { get; set; } = new DefaultPipelineNodeViewSizer();
        
        public ExportPipelineControl()
        {
            _fixedTimer = new Timer {Interval = 8};
            _fixedTimer.Tick += fixedTimer_Tick;
            _fixedTimer.Start();

            _container = new InternalPipelineContainer(this, PipelineGraphBodyProvider);

            _internalRenderer = new InternalRenderListener(_container, this);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            UpdateStyles();

            _internalRenderer.AddDecorator(new ConnectedLinksDecorator(_container));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fixedTimer.Tick -= fixedTimer_Tick;
                _fixedTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Adds a given region of invalidation to be rendered on the next frame.
        /// </summary>
        public void InvalidateRegion(RedrawRegion region)
        {
            if (region.IsEmpty())
                return;

            _clippingRegion.AddRegion(region);
        }

        /// <summary>
        /// Invalidates the entire draw region of this control
        /// </summary>
        public void InvalidateAll()
        {
            _clippingRegion.SetRectangle(new Rectangle(Point.Empty, Size));
        }

        public void SetPanAndZoom(Vector pan, Vector zoom)
        {
            _panAndZoom.SetTargetScale(zoom, pan);

            _clippingRegion.SetRectangle(new Rectangle(Point.Empty, Size));
        }

        /// <summary>
        /// Adds a feature to this export pipeline control.
        /// 
        /// Features that are added later are put at the start of the event
        /// handling queue when handling events.
        /// </summary>
        public void AddFeature([NotNull] ExportPipelineUiFeature feature)
        {
            _features.Insert(0, feature);
        }

        public void InitializeRenderer([NotNull] IRenderManager renderManager)
        {
            _panAndZoom = new SmoothViewPanAndZoomUiFeature(this);
            _controlViewFeature = new ControlViewFeature(this, renderManager);

            AddFeature(new NodeLinkHoverLabelFeature(this, renderManager));
            AddFeature(_panAndZoom);
            AddFeature(new DragAndDropUiFeature(this));
            AddFeature(new SelectionUiFeature(this));
            AddFeature(new PipelineLinkContextMenuFeature(this));
            AddFeature(_controlViewFeature);

            renderManager.AddRenderListener(_internalRenderer);
            BackColor = renderManager.BackColor;

            ImageResources = renderManager.ImageResources;
            TextSizeProvider = renderManager.TextSizeProvider;
            TextMetricsProvider = renderManager.TextMetricsProvider;
        }

        /// <summary>
        /// Performs frame-based step updates
        /// </summary>
        public void UpdateFrameStep(TimeSpan frameRenderDeltaTime)
        {
            // Update animations
            AnimationsManager.Update(frameRenderDeltaTime);
        }

        /// <summary>
        /// Requests that this control fill a clipping region with invalidated regions for redrawing.
        /// </summary>
        public void FillRedrawRegion([NotNull] ClippingRegion clippingRegion)
        {
            if (_clippingRegion.IsEmpty())
                return;

            clippingRegion.AddClippingRegion(_clippingRegion);
            _clippingRegion.Clear();
        }

        private void fixedTimer_Tick(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            if (InvokeRequired)
                return;

            foreach (var feature in _features)
            {
                feature.OnFixedFrame(e);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnMouseLeave(e);
            });
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            MousePoint = e.Location;

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnMouseClick(e);
            });
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            MousePoint = e.Location;

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnMouseDoubleClick(e);
            });
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            MousePoint = e.Location;

            //LoopFeaturesUntilConsumed(feature =>
            //{
            //    feature.OnMouseDown(e);
            //});

            for (int i = 0; i < _features.Count; i++)
            {
                var feature = _features[i];
                feature.OnMouseDown(e);

                if (!feature.IsEventConsumed) 
                    continue;

                for (int j = 0; j < _features.Count; j++)
                {
                    if (i == j)
                        continue;

                    _features[j].OtherFeatureConsumedMouseDown();
                }

                return;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            MousePoint = e.Location;

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnMouseUp(e);
            });
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            MousePoint = e.Location;

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnMouseMove(e);
            });
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            MousePoint = e.Location;

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnMouseWheel(e);
            });
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnKeyDown(e);
            });
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnKeyUp(e);
            });
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnKeyPress(e);
            });
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnPreviewKeyDown(e);
            });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            var args = new ResizeEventArgs(Size);

            foreach (var feature in _features)
                feature.OnResize(args);
        }

        /// <summary>
        /// Resets all export pipeline UI features registered on this pipeline control
        /// and runs a loop sequentially calling <see cref="executing"/> until one
        /// of the features sets its <see cref="ExportPipelineUiFeature.IsEventConsumed"/>
        /// to true, or all features are looped through instead.
        /// </summary>
        private void LoopFeaturesUntilConsumed(Action<ExportPipelineUiFeature> executing)
        {
            ResetEventConsumed();

            foreach (var feature in _features)
            {
                executing(feature);

                if (feature.IsEventConsumed)
                    return;
            }
        }

        /// <summary>
        /// Resets <see cref="ExportPipelineUiFeature.IsEventConsumed"/> to false for
        /// all UI features.
        /// </summary>
        private void ResetEventConsumed()
        {
            foreach (var feature in _features)
            {
                feature.IsEventConsumed = false;
            }
        }

        /// <summary>
        /// Called to grant a UI feature exclusive access to modifying UI views.
        /// 
        /// This is used only as a managing effort for concurrent features, and can be
        /// entirely bypassed by a feature.
        /// </summary>
        public bool FeatureRequestedExclusiveControl(ExportPipelineUiFeature feature)
        {
            // Check if any control has exclusive access first (and isn't the requesting one)
            var current = CurrentExclusiveControlFeature();
            if (current != null)
            {
                return ReferenceEquals(current, feature);
            }

            _exclusiveControl = feature;

            return true;
        }

        /// <summary>
        /// Returns the current feature under exclusive access, if any.
        /// </summary>
        [CanBeNull]
        public ExportPipelineUiFeature CurrentExclusiveControlFeature()
        {
            return _exclusiveControl;
        }

        /// <summary>
        /// If <see cref="feature"/> is under exclusive control, removes it back so no feature
        /// is marked as exclusive control anymore.
        /// 
        /// Does nothing if <see cref="feature"/> is not the current control under exclusive control.
        /// </summary>
        public void WaiveExclusiveControl(ExportPipelineUiFeature feature)
        {
            if (CurrentExclusiveControlFeature() == feature)
                _exclusiveControl = null;
        }

        /// <summary>
        /// Container for pipeline views.
        /// 
        /// Also aids in position calculations for rendering
        /// </summary>
        private class InternalPipelineContainer : IPipelineContainer, IFirstResponderDelegate<IEventHandler>, IInvalidateRegionDelegate
        {
            private readonly RootControlView _root;
            private readonly List<object> _selection = new List<object>();
            private readonly List<PipelineNodeView> _nodeViews = new List<PipelineNodeView>();
            private readonly List<PipelineNodeConnectionLineView> _connectionViews =
                new List<PipelineNodeConnectionLineView>();
            private readonly InternalSelection _sel;
            private readonly IExportPipelineControl _control;
            
            public event PipelineNodeViewEventHandler NodeAdded;

            public event PipelineNodeViewEventHandler NodeRemoved;

            public IPipelineSelection SelectionModel => _sel;

            public PipelineGraph PipelineGraph { get; }

            public object[] Selection => _selection.ToArray();

            public Size ScreenSize => _control.Size;

            public BaseView ContentsView { get; } = new BaseView();
            public BaseView UiContainerView { get; } = new BaseView();

            public PipelineNodeView[] NodeViews => _nodeViews.ToArray();
            public PipelineNodeId[] Nodes => _nodeViews.Where(n => n.NodeId.HasValue).Select(n => n.NodeId.Value).ToArray();
            
            public InternalPipelineContainer([NotNull] IExportPipelineControl control, [NotNull] IPipelineGraphBodyProvider bodyProvider)
            {
                PipelineGraph = new PipelineGraph(bodyProvider);
                PipelineGraph.ConnectionWasAdded += PipelineGraphOnConnectionWasAdded;
                PipelineGraph.ConnectionWillBeRemoved += PipelineGraphOnConnectionWillBeRemoved;

                _root = new RootControlView(this);
                _sel = new InternalSelection(this);

                _root.AddChild(ContentsView);
                _root.AddChild(UiContainerView);

                _control = control;

                _root.InvalidateRegionDelegate = this;
            }

            private void PipelineGraphOnConnectionWillBeRemoved(object sender, ConnectionEventArgs args)
            {
                var inpView = ViewForPipelineInput(args.Connection.End);
                var outView = ViewForPipelineOutput(args.Connection.Start);

                Debug.Assert(inpView != null, "inpView != null");
                Debug.Assert(outView != null, "outView != null");

                AddConnectionView(inpView, outView, args.Connection);
            }

            private void PipelineGraphOnConnectionWasAdded(object sender, ConnectionEventArgs args)
            {
                // Find view and remove it
                var view = ViewForPipelineConnection(args.Connection);
                if (view == null)
                    return;

                Deselect(view);
                view.RemoveFromParent();
            }

            public void RemoveAllViews()
            {
                ClearSelection();

                foreach (var view in ContentsView.Children.ToArray())
                {
                    view.RemoveFromParent();
                }
                foreach (var view in UiContainerView.Children.ToArray())
                {
                    view.RemoveFromParent();
                }

                foreach (var view in _root.Children.ToArray())
                {
                    view.RemoveFromParent();
                }

                _root.AddChild(ContentsView);
                _root.AddChild(UiContainerView);

                _nodeViews.Clear();
                _connectionViews.Clear();
            }

            public void AddNodeView(PipelineNodeView nodeView)
            {
                ContentsView.AddChild(nodeView);
                _nodeViews.Add(nodeView);

                NodeAdded?.Invoke(this, new PipelineNodeViewEventArgs(_control, nodeView));
            }

            public void RemoveNodeView(PipelineNodeView nodeView)
            {
                Deselect(nodeView);

                // Remove connections
                foreach (var connection in GetConnections(nodeView))
                {
                    RemoveConnection(connection.Connection);
                }

                nodeView.RemoveFromParent();

                _nodeViews.Remove(nodeView);

                NodeRemoved?.Invoke(this, new PipelineNodeViewEventArgs(_control, nodeView));
            }

            public void AttemptSelect(BaseView view)
            {
                switch (view)
                {
                    case PipelineNodeView nodeView:
                        if (nodeView.NodeId != null)
                            SelectNode(nodeView.NodeId.Value);
                        break;
                    case PipelineNodeInputLinkView linkView:
                        if (linkView.InputId != null)
                            SelectLink(linkView.InputId.Value);
                        break;
                    case PipelineNodeOutputLinkView linkView:
                        if (linkView.OutputId != null)
                            SelectLink(linkView.OutputId.Value);
                        break;
                    case PipelineNodeConnectionLineView connectionView:
                        SelectConnection(connectionView.Connection);
                        break;
                }
            }

            public bool IsSelectable(BaseView view)
            {
                switch (view)
                {
                    case PipelineNodeView _:
                        return true;
                    case PipelineNodeLinkView _:
                        return true;
                    case PipelineNodeConnectionLineView _:
                        return true;
                    default:
                        return false;
                }
            }

            public void SelectNode(IPipelineNode node)
            {
                if (_selection.Contains(node))
                    return;

                _selection.Add(node);

                var view = ViewForPipelineNode(node);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }

                _sel.FireOnSelectionChangedEvent();
            }

            public void SelectNode(PipelineNodeId node)
            {
                if (_selection.Contains(node))
                    return;

                _selection.Add(node);

                var view = ViewForPipelineNode(node);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }

                _sel.FireOnSelectionChangedEvent();
            }

            public void SelectLink(PipelineInput link)
            {
                if (_selection.Contains(link))
                    return;

                _selection.Add(link);

                var view = ViewForPipelineInput(link);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }

                _sel.FireOnSelectionChangedEvent();
            }

            public void SelectLink(PipelineOutput link)
            {
                if (_selection.Contains(link))
                    return;

                _selection.Add(link);

                var view = ViewForPipelineOutput(link);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }

                _sel.FireOnSelectionChangedEvent();
            }

            public void SelectConnection(IPipelineConnection connection)
            {
                if (_selection.Contains(connection))
                    return;

                _selection.Add(connection);

                var view = ViewForPipelineConnection(connection);
                if (view != null)
                {
                    view.StrokeWidth = 5;
                    view.StrokeColor = Color.OrangeRed;
                }

                _sel.FireOnSelectionChangedEvent();
            }

            public void ClearSelection()
            {
                foreach (var o in _selection.ToArray())
                {
                    switch (o)
                    {
                        case PipelineNodeId node:
                        {
                            var view = ViewForPipelineNode(node);
                            Deselect(view);
                            break;
                        }
                        case PipelineInput link:
                        {
                            // Invalidate view region
                            var view = ViewForPipelineInput(link);
                            Deselect(view);
                            break;
                        }
                        case PipelineOutput link:
                        {
                            // Invalidate view region
                            var view = ViewForPipelineOutput(link);
                            Deselect(view);
                            break;
                        }
                        case IPipelineConnection conn:
                        {
                            // Invalidate view region
                            var view = ViewForPipelineConnection(conn);
                            Deselect(view);
                            break;
                        }
                    }
                }

                _selection.Clear();
                _sel.FireOnSelectionChangedEvent();
            }
            
            public void Deselect([CanBeNull] BaseView view)
            {
                switch (view)
                {
                    case PipelineNodeView nodeView:
                        if (!_selection.Contains(nodeView.NodeId))
                            return;

                        _selection.Remove(nodeView.NodeId);
                        view.StrokeWidth = 1;
                        view.StrokeColor = PipelineNodeView.DefaultStrokeColorForPipelineStep(nodeView.NodeDescriptor);
                        _sel.FireOnSelectionChangedEvent();
                        break;

                    case PipelineNodeInputLinkView linkView:
                        if (!_selection.Contains(linkView.InputId))
                            return;

                        _selection.Remove(linkView.InputId);
                        view.StrokeWidth = 1;
                        view.StrokeColor = Color.Black;
                        _sel.FireOnSelectionChangedEvent();
                        break;

                    case PipelineNodeOutputLinkView linkView:
                        if (!_selection.Contains(linkView.OutputId))
                            return;

                        _selection.Remove(linkView.OutputId);
                        view.StrokeWidth = 1;
                        view.StrokeColor = Color.Black;
                        _sel.FireOnSelectionChangedEvent();
                        break;

                    case PipelineNodeConnectionLineView connView:
                        if (!_selection.Contains(connView.Connection))
                            return;

                        _selection.Remove(connView.Connection);
                        view.StrokeWidth = 2;
                        view.StrokeColor = Color.Orange;
                        _sel.FireOnSelectionChangedEvent();
                        break;
                }
            }

            private void AddConnectionView([NotNull] PipelineNodeLinkView start, [NotNull] PipelineNodeLinkView end, [NotNull] IPipelineConnection connection)
            {
                // Flip start/end to always match output/input
                if (start is PipelineNodeInputLinkView && end is PipelineNodeOutputLinkView)
                {
                    (start, end) = (end, start);
                }

                var view = PipelineNodeConnectionLineView.Create(start, end, connection);
                _connectionViews.Add(view);

                ContentsView.InsertChild(0, view);

                view.UpdateBezier();
            }

            public void AddConnection(IPipelineStep start, IPipelineNode end)
            {
                // Detect cycles
                if (PipelineGraph.AreDirectlyConnected(start.Id, end.Id))
                    return;

                if (!(end is IPipelineNodeWithInputs node))
                    return;

                PipelineGraph.Connect(start.Id, node.Id);
            }

            public bool AreConnected(PipelineNodeLinkView start, PipelineNodeLinkView end)
            {
                return _connectionViews.Any(view => Equals(view.Start, start) && Equals(view.End, end) ||
                                                    Equals(view.Start, end) && Equals(view.End, start));
            }

            public void AddConnection(IPipelineInput input, IPipelineOutput output)
            {
                if (!PipelineGraph.CanConnect(input.Id, output.Id))
                    return;

                var inpNode = input.NodeId;
                var outNode = output.NodeId;

                var inpView = ViewForPipelineInput(input.Id);
                var outView = ViewForPipelineOutput(output.Id);

                Debug.Assert(inpView != null, "inpView != null");
                Debug.Assert(outView != null, "outView != null");

                // Detect cycles
                if (PipelineGraph.AreDirectlyConnected(inpNode, outNode))
                    return;
                
                var con = PipelineGraph.Connect(output.Id, input.Id);

                if (con != null)
                    AddConnectionView(inpView, outView, con);
            }

            /// <summary>
            /// Adds a connection between the two given pipeline links.
            /// The connection is not made if input.CanConnect(output) returns false.
            /// </summary>
            public void AddConnection(PipelineInput input, PipelineOutput output)
            {
                var pipelineInput = PipelineGraph.GetInput(input);
                var pipelineOutput = PipelineGraph.GetOutput(output);

                if (pipelineInput != null && pipelineOutput != null)
                    AddConnection(pipelineInput, pipelineOutput);
            }

            public void RemoveConnection(IPipelineConnection connection)
            {
                PipelineGraph.Disconnect(connection);
            }

            /// <inheritdoc />
            public IEnumerable<PipelineNodeConnectionLineView> GetConnections(PipelineNodeView source)
            {
                return _connectionViews.Where(connection => connection.Start.NodeView.Equals(source) || connection.End.NodeView.Equals(source)).ToArray();
            }
            
            /// <inheritdoc />
            public IEnumerable<PipelineNodeConnectionLineView> GetConnections(PipelineNodeLinkView source)
            {
                return _connectionViews.Where(connection => connection.Start.Equals(source) || connection.End.Equals(source)).ToArray();
            }

            /// <inheritdoc />
            public IEnumerable<PipelineNodeView> GetNodesGoingFrom(PipelineNodeView source)
            {
                var output = new HashSet<PipelineNodeView>();

                foreach (var linkView in source.GetLinkViews())
                {
                    foreach (var connectionView in _connectionViews)
                    {
                        if (Equals(connectionView.Start, linkView))
                            output.Add(connectionView.End.NodeView);
                    }
                }

                return output.ToArray();
            }

            /// <inheritdoc />
            public IEnumerable<PipelineNodeView> GetNodesGoingTo(PipelineNodeView source)
            {
                var output = new HashSet<PipelineNodeView>();

                foreach (var linkView in source.GetLinkViews())
                {
                    foreach (var connectionView in _connectionViews)
                    {
                        if (Equals(connectionView.End, linkView))
                            output.Add(connectionView.Start.NodeView);
                    }
                }

                return output.ToArray();
            }
            
            /// <inheritdoc />
            public IEnumerable<PipelineNodeLinkView> GetLinksConnectedTo(PipelineNodeLinkView source)
            {
                return
                    _connectionViews.Select(connectionView =>
                        {
                            if (Equals(connectionView.Start, source))
                                return connectionView.End;

                            return Equals(connectionView.End, source) ? connectionView.Start : null;
                        }
                    ).Where(v => v != null);
            }

            /// <inheritdoc />
            public IEnumerable<PipelineNodeView> DirectlyConnectedNodeViews(PipelineNodeView source)
            {
                var output = new HashSet<PipelineNodeView>();

                foreach (var linkView in source.GetLinkViews())
                {
                    foreach (var connectionView in _connectionViews)
                    {
                        if (Equals(connectionView.Start, linkView))
                            output.Add(connectionView.End.NodeView);
                        else if (Equals(connectionView.End, linkView))
                            output.Add(connectionView.Start.NodeView);
                    }
                }

                return output.ToArray();
            }

            /// <inheritdoc />
            public IEnumerable<PipelineNodeView> NetworkForNodeView(PipelineNodeView source, IReadOnlyCollection<PipelineNodeView> except = null)
            {
                var output = new List<PipelineNodeView>();
                var queue = new Queue<PipelineNodeView>();

                queue.Enqueue(source);

                // Do a breadth-first search
                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();

                    output.Add(cur);

                    foreach (var connected in DirectlyConnectedNodeViews(cur))
                    {
                        if(!output.Contains(connected) && (except == null || !except.Contains(connected)))
                            queue.Enqueue(connected);
                    }
                }

                return output;
            }

            /// <inheritdoc />
            public (PipelineNodeOutputLinkView from, PipelineNodeInputLinkView to)[] ConnectedLinkViewsBetween(PipelineNodeView from, PipelineNodeView to)
            {
                return (from linkFrom in @from.OutputViews
                    from linkTo in to.InputViews
                    where AreConnected(linkFrom, linkTo)
                    select (linkFrom, linkTo)).ToArray();
            }

            /// <inheritdoc />
            public PipelineNodeView ViewForPipelineNode(IPipelineNode node)
            {
                return _nodeViews.FirstOrDefault(stepView => stepView.NodeId == node.Id);
            }

            /// <inheritdoc />
            public PipelineNodeView ViewForPipelineNode(PipelineNodeId nodeId)
            {
                return _nodeViews.FirstOrDefault(stepView => stepView.NodeId == nodeId);
            }

            /// <inheritdoc />
            public PipelineNodeLinkView ViewForPipelineInput(PipelineInput input)
            {
                return ViewForPipelineNode(input.NodeId)?.InputViews[input.Index];
            }

            /// <inheritdoc />
            public PipelineNodeLinkView ViewForPipelineOutput(PipelineOutput output)
            {
                return ViewForPipelineNode(output.NodeId)?.InputViews[output.Index];
            }

            /// <inheritdoc />
            public PipelineNodeConnectionLineView ViewForPipelineConnection(IPipelineConnection connection)
            {
                return _connectionViews.FirstOrDefault(view => view.Connection == connection);
            }

            /// <inheritdoc />
            public void UpdateConnectionViewsFor(PipelineNodeView nodeView)
            {
                foreach (var view in _connectionViews)
                {
                    if (Equals(view.Start.NodeView, nodeView) || Equals(view.End.NodeView, nodeView))
                        view.UpdateBezier();
                }
            }
            
            public PipelineNodeLinkView[] FindTargetsForLinkViews(IReadOnlyCollection<PipelineNodeLinkView> linkViews, Vector position)
            {
                return linkViews
                    .Select(view =>
                        PotentialLinkViewForLinking(view, position)
                    ).Select(found =>
                        // Remove links that where part of the input set.
                        linkViews.Contains(found) ? null : found
                    ).ToArray();
            }

            [CanBeNull]
            private PipelineNodeLinkView PotentialLinkViewForLinking([NotNull] PipelineNodeLinkView sourceLinkView, Vector position)
            {
                // Start searching through link views, then we look at node views under the mouse
                // and look through their own outputs instead
                var links =
                    ContentsView
                        .ViewsUnder(position, Vector.Zero)
                        .OfType<PipelineNodeLinkView>()
                        .Concat(
                            ContentsView.ViewsUnder(position, Vector.Zero)
                                .OfType<PipelineNodeView>()
                                .SelectMany(nodeView => nodeView.GetLinkViews())
                        )
                        .Where(view => !Equals(view, sourceLinkView));

                foreach (var targetLinkView in links)
                {
                    // Avoid links that belong to the same pipeline step
                    if (targetLinkView.NodeView.GetLinkViews().Contains(sourceLinkView))
                        continue;

                    // Avoid links that are already connected
                    if (AreConnected(sourceLinkView, targetLinkView))
                        continue;

                    // Check type validity
                    PipelineInput? input;
                    PipelineOutput? output;

                    if (sourceLinkView is PipelineNodeInputLinkView source1 && targetLinkView is PipelineNodeOutputLinkView target1)
                    {
                        input = source1.InputId;
                        output = target1.OutputId;
                    }
                    else if (sourceLinkView is PipelineNodeOutputLinkView target2 && targetLinkView is PipelineNodeInputLinkView source2)
                    {
                        input = source2.InputId;
                        output = target2.OutputId;
                    }
                    else
                    {
                        // Can only link inputs to outputs and outputs to inputs
                        continue;
                    }

                    if (input.HasValue && output.HasValue)
                    {
                        var pipelineInput = input.Value;
                        var pipelineOutput = output.Value;

                        // Verify connections won't result in a cycle in the node graphs
                        var node = pipelineOutput.NodeId;

                        if (PipelineGraph.AreDirectlyConnected(node, pipelineInput.NodeId))
                            continue;

                        if (PipelineGraph.CanConnect(pipelineInput, pipelineOutput))
                            return targetLinkView;
                    }
                }

                return null;
            }
            
            public void AutoSizeNodes()
            {
                foreach (var view in _nodeViews)
                {
                    AutoSizeNode(view);
                }
            }

            public void AutoSizeNode(PipelineNodeView view)
            {
                _control.PipelineNodeViewSizer.AutoSize(view, _control.TextSizeProvider);
            }

            public void PerformAction(IExportPipelineAction action)
            {
                action.Perform(this);
            }

            public bool SetAsFirstResponder(IEventHandler firstResponder, bool force)
            {
                return false;
            }

            public void RemoveCurrentResponder()
            {

            }

            public bool IsFirstResponder(IEventHandler handler)
            {
                return false;
            }

            public void DidInvalidate(RedrawRegion region, ISpatialReference reference)
            {
                _control.InvalidateRegion(region);
            }
            
            private class InternalSelection : IPipelineSelection
            {
                public event EventHandler OnSelectionChanged;

                private readonly InternalPipelineContainer _container;

                public InternalSelection(InternalPipelineContainer container)
                {
                    _container = container;
                }

                public PipelineNodeView[] NodeViews()
                {
                    return _container
                        .Selection.OfType<IPipelineNode>()
                        .Select(node => _container.ViewForPipelineNode(node))
                        .ToArray();
                }

                public PipelineNodeLinkView[] NodeLinkViews()
                {
                    return _container
                        .Selection.OfType<PipelineInput>()
                        .Select(link => _container.ViewForPipelineInput(link))
                        .Concat(_container
                            .Selection.OfType<PipelineOutput>()
                            .Select(link => _container.ViewForPipelineOutput(link)))
                        .ToArray();
                }

                public PipelineNodeConnectionLineView[] NodeLinkConnections()
                {
                    return _container
                        .Selection.OfType<IPipelineConnection>()
                        .Select(con => _container.ViewForPipelineConnection(con))
                        .ToArray();
                }

                public BaseView[] Views()
                {
                    return NodeViews().OfType<BaseView>().Concat(NodeLinkViews()).ToArray();
                }

                public bool Contains(BaseView view)
                {
                    switch (view)
                    {
                        case PipelineNodeView node:
                            return _container.Selection.Contains(node.NodeId);

                        case PipelineNodeInputLinkView link:
                            return _container.Selection.Contains(link.InputId);
                        case PipelineNodeOutputLinkView link:
                            return _container.Selection.Contains(link.OutputId);

                        case PipelineNodeConnectionLineView con:
                            return _container.Selection.Contains(con.Connection);
                    }

                    return false;
                }

                public void FireOnSelectionChangedEvent()
                {
                    OnSelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    /// <summary>
    /// Delegate for <see cref="IPipelineContainer.NodeAdded"/>/<see cref="IPipelineContainer.NodeRemoved"/> events.
    /// </summary>
    internal delegate void PipelineNodeViewEventHandler(object sender, [NotNull] PipelineNodeViewEventArgs e);

    /// <summary>
    /// A subclass of <see cref="ClippingRegion"/> that reports a full clipping region as available on the UI.
    /// </summary>
    internal class FullClippingRegion : ClippingRegion
    {
        public override RectangleF[] RedrawRegionRectangles(Size size)
        {
            return new[] { new RectangleF(PointF.Empty, size) };
        }

        public override bool IsVisibleInClippingRegion(Rectangle rectangle)
        {
            return true;
        }

        public override bool IsVisibleInClippingRegion(Point point)
        {
            return true;
        }

        public override bool IsVisibleInClippingRegion(AABB aabb)
        {
            return true;
        }

        public override bool IsVisibleInClippingRegion(Vector point)
        {
            return true;
        }

        public override bool IsVisibleInClippingRegion(AABB aabb, ISpatialReference reference)
        {
            return true;
        }

        public override bool IsVisibleInClippingRegion(Vector point, ISpatialReference reference)
        {
            return true;
        }
    }

    internal class ConnectedLinksDecorator : AbstractRenderingDecorator
    {
        private readonly IPipelineContainer _container;

        public ConnectedLinksDecorator(IPipelineContainer container)
        {
            _container = container;
        }

        public override void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {
            if (_container.GetLinksConnectedTo(link).Any())
                return;

            state.FillColor = Color.Black.WithTransparency(0.5f);
            state.StrokeColor = Color.DarkGray;
        }

        public override void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {
            if (_container.GetLinksConnectedTo(link).Any())
                return;

            state.FillColor = Color.Black.WithTransparency(0.5f);
            state.StrokeColor = Color.DarkGray;
        }
    }

    internal abstract class ExportPipelineControlEventArgs: EventArgs
    {
        [NotNull]
        public IExportPipelineControl Control { get; }

        protected ExportPipelineControlEventArgs([NotNull] IExportPipelineControl control)
        {
            Control = control;
        }
    }

    internal class PipelineNodeViewEventArgs: ExportPipelineControlEventArgs
    {
        [NotNull]
        public PipelineNodeView Node { get; }

        public PipelineNodeViewEventArgs([NotNull] IExportPipelineControl control, [NotNull] PipelineNodeView node) : base(control)
        {
            Node = node;
        }
    }
}
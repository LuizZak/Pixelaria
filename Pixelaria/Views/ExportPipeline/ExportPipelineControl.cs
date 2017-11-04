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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.ExportPipeline;
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Pixelaria.Views.ExportPipeline.PipelineView;

namespace Pixelaria.Views.ExportPipeline
{
    internal class ExportPipelineControl: Control
    {
        private readonly Timer _fixedTimer;

        private readonly SmoothViewPanAndZoomUiFeature _panAndZoom;

        private readonly Direct2DRenderer _d2DRenderer;

        private readonly InternalPipelineContainer _container;
        private readonly List<ExportPipelineUiFeature> _features = new List<ExportPipelineUiFeature>();
        
        [CanBeNull]
        private ExportPipelineUiFeature _exclusiveControl;

        public delegate void PipelineNodeViewEventHandler(object sender, [NotNull] PipelineNodeViewEventArgs e);

        public Point MousePoint { get; private set; }

        public IDirect2DRenderer D2DRenderer => _d2DRenderer;

        public IPipelineContainer PipelineContainer => _container;
        
        public ExportPipelineControl()
        {
            _fixedTimer = new Timer {Interval = 8};
            _fixedTimer.Tick += fixedTimer_Tick;
            _fixedTimer.Start();

            _container = new InternalPipelineContainer(this);

            _d2DRenderer = new Direct2DRenderer(_container, this);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            
            _panAndZoom = new SmoothViewPanAndZoomUiFeature(this);

            AddFeature(new NodeLinkHoverLabelFeature(this));
            AddFeature(_panAndZoom);
            AddFeature(new DragAndDropUiFeature(this));
            AddFeature(new SelectionUiFeature(this));

            _d2DRenderer.AddDecorator(new ConnectedLinksDecorator(_container));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _d2DRenderer.Dispose();

                _fixedTimer.Tick -= fixedTimer_Tick;
                _fixedTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        public void SetPanAndZoom(Vector pan, Vector zoom)
        {
            _panAndZoom.SetTargetScale(zoom, pan);
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

        public void InitializeDirect2DRenderer([NotNull] Direct2DRenderingState state)
        {
            _d2DRenderer.Initialize(state);
        }

        public void RenderDirect2D([NotNull] Direct2DRenderingState state)
        {
            if(_d2DRenderer == null)
                throw new InvalidOperationException("Direct2D renderer was not initialized");

            _d2DRenderer.Render(state);

            foreach (var feature in _features)
                feature.OnRender(state);
        }
        
        private void fixedTimer_Tick(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            if (!InvokeRequired)
            {
                foreach (var feature in _features)
                {
                    feature.OnFixedFrame(e);
                }
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

            LoopFeaturesUntilConsumed(feature =>
            {
                feature.OnMouseDown(e);
            });
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

            foreach (var feature in _features)
                feature.OnResize(e);
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
                return current == feature;
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
        /// Exposed interface for the pipeline step container of <see cref="ExportPipelineControl"/>
        /// </summary>
        public interface IPipelineContainer
        {
            /// <summary>
            /// Gets an array of all currently selected objects.
            /// </summary>
            [NotNull, ItemNotNull]
            object[] Selection { get; }

            /// <summary>
            /// Selection model for this pipeline container.
            /// 
            /// Encapsulate common selection querying operations.
            /// </summary>
            [NotNull]
            ISelection SelectionModel { get; }
            
            /// <summary>
            /// View contents that should be zoomable should be put on this container.
            /// 
            /// Usually all <see cref="PipelineNodeView"/> instances to be interacted with are added here.
            /// </summary>
            BaseView ContentsView { get; }

            /// <summary>
            /// View contents that should not scale with zooming of <see cref="ContentsView"/>.
            /// 
            /// Subviews on this view are shown over <see cref="ContentsView"/>.
            /// </summary>
            BaseView UiContainerView { get; }

            PipelineNodeView[] NodeViews { get; }

            /// <summary>
            /// Called when a node has been added to this container
            /// </summary>
            event PipelineNodeViewEventHandler NodeAdded;

            /// <summary>
            /// Called when a node has been removed from this container
            /// </summary>
            event PipelineNodeViewEventHandler NodeRemoved;

            /// <summary>
            /// Removes all views on this pipeline container
            /// </summary>
            void RemoveAllViews();

            /// <summary>
            /// Adds a new node view.
            /// </summary>
            void AddNodeView([NotNull] PipelineNodeView nodeView);

            /// <summary>
            /// Removes a given node view.
            /// 
            /// Removing a node also removes all of its connections.
            /// </summary>
            void RemoveNodeView([NotNull] PipelineNodeView nodeView);

            /// <summary>
            /// Selects a given pipeline node.
            /// </summary>
            void SelectNode([NotNull] IPipelineNode node);

            /// <summary>
            /// Selects a given pipeline link.
            /// </summary>
            void SelectLink([NotNull] IPipelineNodeLink link);

            /// <summary>
            /// Selects a given pipeline connection.
            /// </summary>
            void SelectConnection([NotNull] IPipelineLinkConnection link);

            /// <summary>
            /// De-selects all pipeline nodes.
            /// </summary>
            void ClearSelection();

            /// <summary>
            /// Tries to deselect the given view, if it represents a currently selected
            /// object.
            /// </summary>
            void Deselect(BaseView view);
            
            /// <summary>
            /// Selects the given view (if it's a valid selectable view).
            /// 
            /// Method may run through hierarchy to find a fit selectable view.
            /// </summary>
            void AttemptSelect(BaseView view);

            /// <summary>
            /// Returns if two link views are connected
            /// </summary>
            bool AreConnected([NotNull] PipelineNodeLinkView start, [NotNull] PipelineNodeLinkView end);

            /// <summary>
            /// Adds a connection to the first input/output that match on two pipeline nodes
            /// </summary>
            void AddConnection([NotNull] IPipelineStep start, [NotNull] IPipelineNode end);

            /// <summary>
            /// Adds a connection between the two given pipeline links.
            /// The connection is not made if input.CanConnect(output) returns false.
            /// </summary>
            void AddConnection([NotNull] IPipelineInput input, [NotNull] IPipelineOutput output);

            /// <summary>
            /// Removes a connection from the container's model
            /// </summary>
            void RemoveConnection([NotNull] IPipelineLinkConnection connection);

            /// <summary>
            /// Invalidates all connection line views connected to a given pipeline step
            /// </summary>
            void UpdateConnectionViewsFor([NotNull] PipelineNodeView nodeView);

            /// <summary>
            /// Returns all pipeline node view connections that are connected to the given node view's inputs and outputs
            /// </summary>
            IEnumerable<PipelineNodeConnectionLineView> GetConnections([NotNull] PipelineNodeView source);

            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's output.
            /// </summary>
            IEnumerable<PipelineNodeView> GetNodesGoingFrom([NotNull] PipelineNodeView source);
            
            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's input.
            /// </summary>
            IEnumerable<PipelineNodeView> GetNodesGoingTo([NotNull] PipelineNodeView source);

            /// <summary>
            /// Returns a list of node views that are directly connected to a given source node view.
            /// </summary>
            IEnumerable<PipelineNodeView> DirectlyConnectedNodeViews([NotNull] PipelineNodeView source);

            /// <summary>
            /// Returns all pipeline node link views that are connected to the given link views.
            /// </summary>
            IReadOnlyList<PipelineNodeLinkView> GetLinksConnectedTo([NotNull] PipelineNodeLinkView source);

            /// <summary>
            /// Retrieves the view that represents the given pipeline node within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeView ViewForPipelineNode([NotNull] IPipelineNode node);

            /// <summary>
            /// Retrieves the view that represents the given pipeline node within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeLinkView ViewForPipelineNodeLink([NotNull] IPipelineNodeLink node);

            /// <summary>
            /// Retrieves the view that represents the given pipeline connection
            /// </summary>
            [CanBeNull]
            PipelineNodeConnectionLineView ViewForPipelineConnection([NotNull] IPipelineLinkConnection node);

            /// <summary>
            /// Retrieves all combinations of link views between the two node views that are connected to one another.
            /// </summary>
            [NotNull]
            (PipelineNodeLinkView from, PipelineNodeLinkView to)[] ConnectedLinkViewsBetween(
                [NotNull] PipelineNodeView from, [NotNull] PipelineNodeView to);

            /// <summary>
            /// Returns a list of all node views that are connected to a given node view, including the view itself.
            /// 
            /// Returns nodes listed from nearest to farthest from the source node.
            /// </summary>
            IEnumerable<PipelineNodeView> NetworkForNodeView(PipelineNodeView source,
                IReadOnlyCollection<PipelineNodeView> except = null);

            /// <summary>
            /// With a given set of link views, combines them with links/nodes under the given absolute
            /// position (in ContentsView coordinates).
            /// 
            /// Given the array of link views, makes the proper guesses of the correct input/outputs
            /// to return a set of link views to connect them to.
            /// 
            /// Returns an array that maps 1-to-1 with each input link view, with either a link view to
            /// attach to, or null, if no fit was found.
            /// 
            /// Does not return links that are part of the input set.
            /// </summary>
            [NotNull]
            [ItemCanBeNull]
            PipelineNodeLinkView[] FindTargetsForLinkViews(
                [NotNull, ItemNotNull] IReadOnlyCollection<PipelineNodeLinkView> linkViews, Vector position);

            /// <summary>
            /// Performs automatic resizing of nodes
            /// </summary>
            void AutosizeNodes();

            /// <summary>
            /// Performs automatic resizing of a single node view
            /// </summary>
            void AutosizeNode([NotNull] PipelineNodeView view);

            void PerformAction([NotNull] IExportPipelineAction action);
        }

        /// <summary>
        /// For exposing selections
        /// </summary>
        public interface ISelection
        {
            /// <summary>
            /// Gets selected node views
            /// </summary>
            PipelineNodeView[] NodeViews();

            /// <summary>
            /// Gets selected node link views
            /// </summary>
            PipelineNodeLinkView[] NodeLinkViews();

            /// <summary>
            /// Gets the connection links between nodes
            /// </summary>
            PipelineNodeConnectionLineView[] NodeLinkConnections();

            /// <summary>
            /// Gets all selected objects that are currently represented as views.
            /// </summary>
            BaseView[] Views();

            /// <summary>
            /// Returns whether a given view is selected.
            /// 
            /// Only accepts selectable view types, other types return false.
            /// </summary>
            bool Contains([CanBeNull] BaseView view);
        }

        /// <summary>
        /// Container for pipeline views.
        /// 
        /// Also aids in position calculations for rendering
        /// </summary>
        private class InternalPipelineContainer : IPipelineContainer
        {
            private readonly BaseView _root = new BaseView();
            private readonly List<object> _selection = new List<object>();
            private readonly List<PipelineNodeView> _nodeViews = new List<PipelineNodeView>();
            private readonly List<PipelineNodeConnectionLineView> _connectionViews =
                new List<PipelineNodeConnectionLineView>();
            private readonly _Selection _sel;
            private readonly ExportPipelineControl _control;
            
            public event PipelineNodeViewEventHandler NodeAdded;

            public event PipelineNodeViewEventHandler NodeRemoved;

            public ISelection SelectionModel => _sel;

            public object[] Selection => _selection.ToArray();

            public BaseView ContentsView { get; } = new BaseView();
            public BaseView UiContainerView { get; } = new BaseView();

            public PipelineNodeView[] NodeViews => _nodeViews.ToArray();
            
            public InternalPipelineContainer(ExportPipelineControl control)
            {
                _sel = new _Selection(this);

                _root.AddChild(ContentsView);
                _root.AddChild(UiContainerView);

                _control = control;
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

                NodeAdded?.Invoke(this, new PipelineNodeViewEventArgs(nodeView));
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

                NodeRemoved?.Invoke(this, new PipelineNodeViewEventArgs(nodeView));
            }

            public void AttemptSelect(BaseView view)
            {
                if (view is PipelineNodeView nodeView)
                {
                    SelectNode(nodeView.PipelineNode);
                }
                else if (view is PipelineNodeLinkView linkView)
                {
                    SelectLink(linkView.NodeLink);
                }
                else if (view is PipelineNodeConnectionLineView connectionView)
                {
                    SelectConnection(connectionView.Connection);
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
            }

            public void SelectLink(IPipelineNodeLink link)
            {
                if (_selection.Contains(link))
                    return;

                _selection.Add(link);
                
                var view = ViewForPipelineNodeLink(link);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }
            }

            public void SelectConnection(IPipelineLinkConnection connection)
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
            }

            public void ClearSelection()
            {
                foreach (object o in _selection.ToArray())
                {
                    if (o is IPipelineNode node)
                    {
                        var view = ViewForPipelineNode(node);
                        Deselect(view);
                    }
                    else if (o is IPipelineNodeLink link)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineNodeLink(link);
                        Deselect(view);
                    }
                    else if (o is IPipelineLinkConnection conn)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineConnection(conn);
                        Deselect(view);
                    }
                }

                _selection.Clear();
            }
            
            public void Deselect(BaseView view)
            {
                if (view is PipelineNodeView nodeView)
                {
                    if (!_selection.Contains(nodeView.PipelineNode))
                        return;

                    _selection.Remove(nodeView.PipelineNode);
                    view.StrokeWidth = 1;
                    view.StrokeColor = Color.Black;
                }
                else if (view is PipelineNodeLinkView linkView)
                {
                    if (!_selection.Contains(linkView.NodeLink))
                        return;

                    _selection.Remove(linkView.NodeLink);
                    view.StrokeWidth = 1;
                    view.StrokeColor = Color.Black;
                }
                else if (view is PipelineNodeConnectionLineView connView)
                {
                    if (!_selection.Contains(connView.Connection))
                        return;

                    _selection.Remove(connView.Connection);
                    view.StrokeWidth = 2;
                    view.StrokeColor = Color.Orange;
                }
            }

            private void AddConnectionView([NotNull] PipelineNodeLinkView start, [NotNull] PipelineNodeLinkView end, [NotNull] IPipelineLinkConnection connection)
            {
                // Flip start/end to always match output/input
                if (start.NodeLink is IPipelineInput && end.NodeLink is IPipelineOutput)
                {
                    (start, end) = (end, start);
                }

                var view = new PipelineNodeConnectionLineView(start, end, connection);
                _connectionViews.Add(view);

                ContentsView.InsertChild(0, view);

                view.UpdateBezier();
            }

            public void AddConnection(IPipelineStep start, IPipelineNode end)
            {
                // Detect cycles
                if (start.IsDirectlyConnected(end))
                    return;

                if (end is IPipelineStep step)
                {
                    var con = start.ConnectTo(step);
                    if (con != null)
                    {
                        var input = step.Input.First(i => i.Connections.Any(start.Output.Contains));
                        var output = input.Connections.First(start.Output.Contains);

                        var inpView = ViewForPipelineNodeLink(input);
                        var outView = ViewForPipelineNodeLink(output);
                        
                        Debug.Assert(inpView != null, "inpView != null");
                        Debug.Assert(outView != null, "outView != null");

                        AddConnectionView(inpView, outView, con);
                    }
                }
                else if (end is IPipelineEnd endStep)
                {
                    var con = start.ConnectTo(endStep);
                    if (con != null)
                    {
                        var input = endStep.Input.First(i => i.Connections.Any(start.Output.Contains));
                        var output = input.Connections.First(start.Output.Contains);

                        var inpView = ViewForPipelineNodeLink(input);
                        var outView = ViewForPipelineNodeLink(output);

                        Debug.Assert(inpView != null, "inpView != null");
                        Debug.Assert(outView != null, "outView != null");
                        AddConnectionView(inpView, outView, con);
                    }
                }
            }

            public bool AreConnected(PipelineNodeLinkView start, PipelineNodeLinkView end)
            {
                return _connectionViews.Any(view => Equals(view.Start, start) && Equals(view.End, end) ||
                                                    Equals(view.Start, end) && Equals(view.End, start));
            }

            public void AddConnection(IPipelineInput input, IPipelineOutput output)
            {
                if (!input.CanConnect(output))
                    return;

                var inpNode = input.Node;
                var outNode = output.Node;
                if (inpNode == null || outNode == null)
                    return;

                var inpView = ViewForPipelineNodeLink(input);
                var outView = ViewForPipelineNodeLink(output);

                Debug.Assert(inpView != null, "inpView != null");
                Debug.Assert(outView != null, "outView != null");

                // Detect cycles
                if (inpNode.IsDirectlyConnected(outNode))
                    return;

                var con = input.Connect(output);

                if (con != null)
                    AddConnectionView(inpView, outView, con);
            }

            public void RemoveConnection(IPipelineLinkConnection connection)
            {
                if (!connection.Input.Connections.Contains(connection.Output))
                    return;

                // Find view and remove it
                var view = ViewForPipelineConnection(connection);
                if (view == null)
                    return;

                Deselect(view);
                connection.Disconnect();
                view.RemoveFromParent();

                _connectionViews.Remove(view);
            }

            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's output.
            /// </summary>
            public IEnumerable<PipelineNodeConnectionLineView> GetConnections(PipelineNodeView source)
            {
                return _connectionViews.Where(connection => connection.Start.NodeView.Equals(source) || connection.End.NodeView.Equals(source)).ToArray();
            }

            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's output.
            /// </summary>
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

            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's input.
            /// </summary>
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
            
            /// <summary>
            /// Returns all pipeline node link views that are connected to the given link views.
            /// </summary>
            public IReadOnlyList<PipelineNodeLinkView> GetLinksConnectedTo(PipelineNodeLinkView source)
            {
                var connections = new List<PipelineNodeLinkView>();

                foreach (var connectionView in _connectionViews)
                {
                    if (Equals(connectionView.Start, source))
                    {
                        connections.Add(connectionView.End);
                    }
                    else if (Equals(connectionView.End, source))
                    {
                        connections.Add(connectionView.Start);
                    }
                }

                return connections;
            }

            /// <summary>
            /// Returns a list of node views that are directly connected to a given source node view.
            /// </summary>
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

            /// <summary>
            /// Returns a list of all node views that are connected to a given node view, including the view itself.
            /// 
            /// Returns nodes listed from nearest to farthest from the source node.
            /// </summary>
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

            /// <summary>
            /// Retrieves all combinations of link views between the two node views that are connected to one another.
            /// </summary>
            public (PipelineNodeLinkView from, PipelineNodeLinkView to)[] ConnectedLinkViewsBetween(PipelineNodeView from, PipelineNodeView to)
            {
                var con = new List<(PipelineNodeLinkView from, PipelineNodeLinkView to)>();

                foreach (var linkFrom in from.GetOutputViews())
                {
                    foreach (var linkTo in to.GetInputViews())
                    {
                        if(AreConnected(linkFrom, linkTo))
                            con.Add((linkFrom, linkTo));
                    }
                }

                return con.ToArray();
            }

            /// <summary>
            /// Retrieves the view that represents the given pipeline step within this container
            /// </summary>
            public PipelineNodeView ViewForPipelineNode(IPipelineNode node)
            {
                return _nodeViews.FirstOrDefault(stepView => stepView.PipelineNode == node);
            }

            /// <summary>
            /// Retrieves the view that represents the given pipeline step within this container
            /// </summary>
            public PipelineNodeLinkView ViewForPipelineNodeLink(IPipelineNodeLink node)
            {
                if (node.Node == null)
                    return null;

                return ViewForPipelineNode(node.Node)?.GetLinkViews()
                    .FirstOrDefault(linkView => linkView.NodeLink == node);
            }

            /// <summary>
            /// Retrieves the view that represents the given pipeline connection
            /// </summary>
            public PipelineNodeConnectionLineView ViewForPipelineConnection(IPipelineLinkConnection connection)
            {
                return _connectionViews.FirstOrDefault(view => view.Connection == connection);
            }

            /// <summary>
            /// Invalidates all connection line views connected to a given pipeline step
            /// </summary>
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
                        linkViews.Contains(found) ? null : found
                    ) // Remove links that where part of the input set.
                    .ToArray();
            }

            [CanBeNull]
            private PipelineNodeLinkView PotentialLinkViewForLinking([NotNull] PipelineNodeLinkView linkView, Vector position)
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
                        .Where(view => !Equals(view, linkView));

                foreach (var nodeLinkView in links)
                {
                    var linkSource = linkView.NodeLink;
                    var linkTarget = nodeLinkView.NodeLink;

                    // Avoid links that belong to the same pipeline step
                    if (nodeLinkView.NodeView.GetLinkViews().Contains(linkView))
                        continue;

                    // Avoid links that are already connected
                    if (AreConnected(linkView, nodeLinkView))
                        continue;

                    // Can't link inputs to other inputs and outputs to other outputs
                    if (!(linkSource is IPipelineInput && linkTarget is IPipelineOutput) &&
                        !(linkSource is IPipelineOutput && linkTarget is IPipelineInput))
                        continue;
                    
                    // Check type validity
                    IPipelineInput input;
                    IPipelineOutput output;

                    if (linkSource is IPipelineInput source && linkTarget is IPipelineOutput)
                    {
                        input = source;
                        output = (IPipelineOutput)linkTarget;
                    }
                    else
                    {
                        input = (IPipelineInput)linkTarget;
                        output = (IPipelineOutput)linkSource;

                    }
                    
                    // Verify connections won't result in a cycle in the node graphs
                    var node = output.Node;
                    if (node == null)
                        continue;

                    if (input.Node != null && node.IsDirectlyConnected(input.Node))
                        continue;

                    if (input.CanConnect(output))
                        return nodeLinkView;
                }

                return null;
            }
            
            public void AutosizeNodes()
            {
                foreach (var view in _nodeViews)
                {
                    view.AutoSize(_control.D2DRenderer);
                }
            }

            public void AutosizeNode(PipelineNodeView view)
            {
                view.AutoSize(_control.D2DRenderer);
            }

            public void PerformAction(IExportPipelineAction action)
            {
                action.Perform(this);
            }

            // ReSharper disable once InconsistentNaming
            private class _Selection : ISelection
            {
                private readonly InternalPipelineContainer _container;

                public _Selection(InternalPipelineContainer container)
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
                        .Selection.OfType<IPipelineNodeLink>()
                        .Select(link => _container.ViewForPipelineNodeLink(link))
                        .ToArray();
                }

                public PipelineNodeConnectionLineView[] NodeLinkConnections()
                {
                    return _container
                        .Selection.OfType<IPipelineLinkConnection>()
                        .Select(con => _container.ViewForPipelineConnection(con))
                        .ToArray();
                }

                public BaseView[] Views()
                {
                    return NodeViews().OfType<BaseView>().Concat(NodeLinkViews()).ToArray();
                }

                public bool Contains(BaseView view)
                {
                    if (view is PipelineNodeView node)
                        return _container.Selection.Contains(node.PipelineNode);
                    if (view is PipelineNodeLinkView link)
                        return _container.Selection.Contains(link.NodeLink);
                    if (view is PipelineNodeConnectionLineView con)
                        return _container.Selection.Contains(con.Connection);

                    return false;
                }
            }
        }
    }

    /// <summary>
    /// Base class for managing UI states/functionalities based on current states of UI and keyboard/mouse.
    /// 
    /// Multiple states can be on simultaneously to satisfy multiple rendering needs.
    /// 
    /// Each state modifies the final rendering state using decorators.
    /// </summary>
    internal abstract class ExportPipelineUiFeature
    {
        /// <summary>
        /// Control to manage
        /// </summary>
        [NotNull]
        protected readonly ExportPipelineControl Control;

        /// <summary>
        /// When an <see cref="ExportPipelineControl"/> calls one of the event handlers
        /// On[...] (like OnMouseDown, OnMouseLeave, OnKeyDown, etc., except <see cref="OnRender"/>,
        /// <see cref="OnResize"/> and <see cref="OnFixedFrame"/>) this flag is reset to false on all pipeline UI 
        /// features, and after every feature's event handler call this flag is checked to 
        /// stop calling the event handler on further events.
        /// </summary>
        public bool IsEventConsumed { get; set; }

        /// <summary>
        /// States whether this pipeline feature has exclusive UI control.
        /// 
        /// Exclusive UI control should be requested whenever a UI feature wants to do
        /// work that adds/removes views, modifies the position/scaling of views on 
        /// screen etc., which could otherwise interfere with other features' functionalities
        /// if they happen concurrently.
        /// </summary>
        protected bool hasExclusiveControl => Control.CurrentExclusiveControlFeature() == this;

        protected ExportPipelineControl.IPipelineContainer container => Control.PipelineContainer;
        protected BaseView contentsView => container.ContentsView;
        protected BaseView uiContainerView => container.UiContainerView;

        protected ExportPipelineUiFeature([NotNull] ExportPipelineControl control)
        {
            Control = control;
        }

        /// <summary>
        /// Called on a fixed 8ms (on average) interval across all UI features to perform fixed-update operations.
        /// </summary>
        public virtual void OnFixedFrame([NotNull] EventArgs e) { }

        /// <summary>
        /// Called whenever a new Direct2D frame is being rendered.
        /// </summary>
        public virtual void OnRender([NotNull] Direct2DRenderingState state) { }

        public virtual void OnMouseLeave([NotNull] EventArgs e) { }
        public virtual void OnMouseClick([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseDoubleClick([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseDown([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseUp([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseMove([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseEnter([NotNull] EventArgs e) { }
        public virtual void OnMouseWheel([NotNull] MouseEventArgs e) { }

        public virtual void OnKeyDown([NotNull] KeyEventArgs e) { }
        public virtual void OnKeyUp([NotNull] KeyEventArgs e) { }
        public virtual void OnKeyPress([NotNull] KeyPressEventArgs e) { }
        public virtual void OnPreviewKeyDown([NotNull] PreviewKeyDownEventArgs e) { }
        public virtual void OnResize([NotNull] EventArgs e) { }

        /// <summary>
        /// Consumes the current event handler call such that no further UI features
        /// receive it on the current event loop.
        /// 
        /// See <see cref="IsEventConsumed"/> for more info.
        /// </summary>
        protected void ConsumeEvent()
        {
            IsEventConsumed = true;
        }

        /// <summary>
        /// Shortcut for <see cref="ExportPipelineControl.FeatureRequestedExclusiveControl"/>, returning whether
        /// exclusive control was granted.
        /// </summary>
        protected bool RequestExclusiveControl()
        {
            return Control.FeatureRequestedExclusiveControl(this);
        }

        /// <summary>
        /// Shortcut for <see cref="ExportPipelineControl.WaiveExclusiveControl"/>.
        /// 
        /// Returns exclusive control back for new requesters to pick on.
        /// </summary>
        protected void ReleaseExclusiveControl()
        {
            Control.WaiveExclusiveControl(this);
        }

        /// <summary>
        /// Returns true if any feature other than this one currently has exclusive control set.
        /// 
        /// Returns false, if no control currently has exclusive control.
        /// </summary>
        protected bool OtherFeatureHasExclusiveControl()
        {
            var feature = Control.CurrentExclusiveControlFeature();
            return feature != null && feature != this;
        }

        /// <summary>
        /// Executes an action under a temporary exclusive control lock.
        /// 
        /// If no lock could be obtained, the method returns without calling <see cref="action"/>.
        /// </summary>
        protected void ExecuteWithTemporaryExclusiveControl([InstantHandle] Action action)
        {
            if (!RequestExclusiveControl())
                return;

            action();

            ReleaseExclusiveControl();
        }
    }

    /// <summary>
    /// Decorator that modifies rendering of objects in the export pipeline view.
    /// </summary>
    internal interface IRenderingDecorator
    {
        void DecoratePipelineStep([NotNull] PipelineNodeView nodeView, ref PipelineStepViewState state);

        void DecoratePipelineStepInput([NotNull] PipelineNodeView nodeView, [NotNull] PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state);

        void DecoratePipelineStepOutput([NotNull] PipelineNodeView nodeView, [NotNull] PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state);

        void DecorateBezierPathView([NotNull] BezierPathView pathView, ref BezierPathViewState state);

        void DecorateLabelView([NotNull] LabelView pathView, ref LabelViewState state);
    }

    internal struct PipelineStepViewState
    {
        public int StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color TitleFillColor { get; set; }
        public Color StrokeColor { get; set; }
        public Color TitleFontColor { get; set; }
        public Color BodyFontColor { get; set; }
    }

    internal struct PipelineStepViewLinkState
    {
        public int StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color StrokeColor { get; set; }
    }

    internal struct BezierPathViewState
    {
        public int StrokeWidth { get; set; }
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
    }

    internal struct LabelViewState
    {
        public int StrokeWidth { get; set; }
        public Color StrokeColor { get; set; }
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
    }

    internal abstract class AbstractRenderingDecorator : IRenderingDecorator
    {
        public virtual void DecoratePipelineStep(PipelineNodeView nodeView, ref PipelineStepViewState state)
        {

        }

        public virtual void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {

        }

        public virtual void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {

        }

        public virtual void DecorateBezierPathView(BezierPathView pathView, ref BezierPathViewState state)
        {

        }

        public virtual void DecorateLabelView(LabelView pathView, ref LabelViewState state)
        {

        }
    }

    internal class ConnectedLinksDecorator : AbstractRenderingDecorator
    {
        private readonly ExportPipelineControl.IPipelineContainer _container;

        public ConnectedLinksDecorator(ExportPipelineControl.IPipelineContainer container)
        {
            _container = container;
        }

        public override void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {
            if (!_container.GetLinksConnectedTo(link).Any())
            {
                state.FillColor = Color.Transparent;
                state.StrokeColor = Color.Gray;
            }
        }

        public override void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {
            if (!_container.GetLinksConnectedTo(link).Any())
            {
                state.FillColor = Color.Transparent;
                state.StrokeColor = Color.Gray;
            }
        }
    }

    internal class PipelineNodeViewEventArgs: EventArgs
    {
        [NotNull]
        public PipelineNodeView Node { get; }

        public PipelineNodeViewEventArgs([NotNull] PipelineNodeView node)
        {
            Node = node;
        }
    }
}
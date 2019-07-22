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
using System.Drawing;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Rendering;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI;
using PixUI.Animation;
using PixUI.Controls;

namespace Pixelaria.Views.ExportPipeline
{
    internal interface IExportPipelineControl: IInvalidatableControl
    {
        /// <summary>
        /// Container for <see cref="ControlView"/>-based controls
        /// </summary>
        IControlContainer ControlContainer { get; }

        /// <summary>
        /// Latest registered location of the mouse on this control.
        /// 
        /// Is updated on every mouse event handler.
        /// </summary>
        Point MousePoint { get; }

        /// <summary>
        /// Target for adding rendering decorators to
        /// </summary>
        IRenderingDecoratorContainer RenderingDecoratorTarget { get; }

        /// <summary>
        /// Gets the image resources provider for this pipeline control
        /// </summary>
        IImageResourceManager ImageResources { get; }
        
        /// <summary>
        /// Gets the label size provider for this control
        /// </summary>
        ILabelViewSizeProvider LabelViewSizeProvider { get; }

        /// <summary>
        /// Gets the label view metrics provider initialized for this control
        /// </summary>
        ITextMetricsProvider TextMetricsProvider { get; }

        /// <summary>
        /// Gets the pipeline node and connections container for this control
        /// </summary>
        IPipelineContainer PipelineContainer { get; }

        /// <summary>
        /// Gets or sets the sizer to apply to pipeline node views.
        /// </summary>
        IPipelineNodeViewSizer PipelineNodeViewSizer { get; }

        /// <summary>
        /// Gets the animations manager that can be used to create animations.
        /// </summary>
        AnimationsManager AnimationsManager { get; }

        Size Size { get; }

        void SetPanAndZoom(Vector pan, Vector zoom);

        /// <summary>
        /// Invalidates the Direct2D renderer for this control.
        /// 
        /// The rendering context will be re-created on the next call to <see cref="ExportPipelineControl.Render"/>
        /// </summary>
        void InvalidateState();

        /// <summary>
        /// Called to grant a UI feature exclusive access to modifying UI views.
        /// 
        /// This is used only as a managing effort for concurrent features, and can be
        /// entirely bypassed by a feature.
        /// </summary>
        bool FeatureRequestedExclusiveControl(ExportPipelineUiFeature feature);

        /// <summary>
        /// Returns the current feature under exclusive access, if any.
        /// </summary>
        ExportPipelineUiFeature CurrentExclusiveControlFeature();

        /// <summary>
        /// If <see cref="feature"/> is under exclusive control, removes it back so no feature
        /// is marked as exclusive control anymore.
        /// 
        /// Does nothing if <see cref="feature"/> is not the current control under exclusive control.
        /// </summary>
        void WaiveExclusiveControl(ExportPipelineUiFeature feature);

        // TODO: This is only here because PropertiesPanel uses it; it's to keep compatibility with
        // the previous concrete ExportPipelineControl reference within PropertiesPanel; we can abstract
        // this away more nicely later, instead of just copy-pasting the Control event here.
        event EventHandler SizeChanged;
    }

    /// <summary>
    /// Specifies a control that has an invalidatable region for localized redrawing.
    /// </summary>
    internal interface IInvalidatableControl
    {
        /// <summary>
        /// Adds a given region of invalidation to be rendered on the next frame.
        /// </summary>
        void InvalidateRegion([NotNull] RedrawRegion region);

        /// <summary>
        /// Invalidates the entire draw region of this control
        /// </summary>
        void InvalidateAll();
    }

    /// <summary>
    /// Exposed interface for the pipeline step container of <see cref="ExportPipelineControl"/>
    /// </summary>
    internal interface IPipelineContainer
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
        IPipelineSelection SelectionModel { get; }

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

        /// <summary>
        /// Gets an array of all visible node views.
        /// 
        /// Each node from <see cref="Nodes"/> has a matching node view contained here.
        /// Ordering between the two arrays is not guaranteed.
        /// </summary>
        PipelineNodeView[] NodeViews { get; }

        /// <summary>
        /// Gets an array of all nodes from this container
        /// </summary>
        IPipelineNode[] Nodes { get; }

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
        /// Returns whether a given base view subclass is recognized as a selectable entity type
        /// by <see cref="AttemptSelect"/>.
        /// </summary>
        bool IsSelectable(BaseView view);

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
        /// Returns all pipeline node view connections that are connected to the given node link view's inputs and outputs
        /// </summary>
        IEnumerable<PipelineNodeConnectionLineView> GetConnections([NotNull] PipelineNodeLinkView source);

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
        IEnumerable<PipelineNodeLinkView> GetLinksConnectedTo([NotNull] PipelineNodeLinkView source);

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
        /// 
        /// Only return direct connections between the nodes.
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
        void AutoSizeNodes();

        /// <summary>
        /// Performs automatic resizing of a single node view
        /// </summary>
        void AutoSizeNode([NotNull] PipelineNodeView view);

        void PerformAction([NotNull] IExportPipelineAction action);
    }

    /// <summary>
    /// For exposing selections
    /// </summary>
    internal interface IPipelineSelection
    {
        /// <summary>
        /// An event fired whenever the contents of this selection change.
        /// </summary>
        event EventHandler OnSelectionChanged;

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
}
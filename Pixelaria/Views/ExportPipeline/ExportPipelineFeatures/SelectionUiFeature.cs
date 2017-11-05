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
using System.Linq;
using System.Windows.Forms;

using JetBrains.Annotations;

using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.PipelineView;

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    internal class SelectionUiFeature : ExportPipelineUiFeature, IRenderingDecorator
    {
        [CanBeNull]
        private MouseHoverState? _hovering;

        private bool _isDrawingSelection;
        private Vector _mouseDown;
        private bool _skipMouseUp;
        private HashSet<PipelineNodeView> _underSelectionArea = new HashSet<PipelineNodeView>();

        // For drawing the selection outline with
        private readonly BezierPathView _pathView = new BezierPathView
        {
            RenderOnTop = true,
            FillColor = Color.Orange.ToAhsl().WithTransparency(0.1f).ToColor()
        };

        public SelectionUiFeature([NotNull] ExportPipelineControl control) : base(control)
        {
            control.D2DRenderer.AddDecorator(this);
        }

        public override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hovering = null;

            SetHovering(null);
        }

        public override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left)
            {
                // Select a network of connected views
                var view =
                    contentsView
                        .ViewsUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5, 5))
                        .FirstOrDefault();

                if (view is PipelineNodeLinkView linkView)
                {
                    // Select all nodes connected to the given link view
                    var connected = container.GetLinksConnectedTo(linkView);

                    foreach (var v in connected)
                    {
                        var network = container.NetworkForNodeView(v.NodeView, new[] { linkView.NodeView });

                        foreach (var node in network)
                        {
                            container.AttemptSelect(node);
                        }

                        container.AttemptSelect(v.NodeView);
                    }

                    // Drop selection of link view
                    container.Deselect(linkView);

                    _skipMouseUp = true;
                }
                else
                {
                    // Select all nodes from network
                    var nodeView = (view as PipelineNodeConnectionLineView)?.Start.NodeView ?? view as PipelineNodeView;

                    if (nodeView != null)
                    {
                        var network = container.NetworkForNodeView(nodeView);

                        foreach (var node in network)
                        {
                            container.AttemptSelect(node);
                        }

                        _skipMouseUp = true;
                    }
                }
            }
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _skipMouseUp = false;

            if (OtherFeatureHasExclusiveControl() || e.Button != MouseButtons.Left)
                return;

            _mouseDown = e.Location;

            var closestView = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5), container.IsSelectable);
            var isInSelection = container.SelectionModel.Contains(closestView);

            if (!System.Windows.Forms.Control.ModifierKeys.HasFlag(Keys.Shift) && !isInSelection)
                Control.PipelineContainer.ClearSelection();

            // Selection
            if (System.Windows.Forms.Control.ModifierKeys.HasFlag(Keys.Shift) || closestView == null)
            {
                if (RequestExclusiveControl())
                {
                    _isDrawingSelection = true;

                    _pathView.ClearPath();
                    contentsView.AddChild(_pathView);
                }
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left && _isDrawingSelection)
            {
                // Draw selection square
                var area = new AABB(new[]
                {
                    contentsView.ConvertFrom(_mouseDown, null),
                    contentsView.ConvertFrom(e.Location, null)
                });

                _pathView.SetAsRectangle(area);

                // Highlight all views under the selected area
                var viewsInArea =
                    contentsView.ViewsUnder(area, Vector.Zero)
                        .OfType<PipelineNodeView>()
                        .ToArray();

                var removed =
                    _underSelectionArea
                        .Except(viewsInArea);

                var newViews =
                    viewsInArea
                        .Except(_underSelectionArea);

                _underSelectionArea = new HashSet<PipelineNodeView>(viewsInArea);

                // Create temporary strokes for displaying new selection
                foreach (var view in newViews)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }
                // Invalidate views that where removed from selection area as well
                foreach (var view in removed)
                {
                    view.StrokeWidth = 1;
                    view.StrokeColor = Color.Black;
                }
            }
            else if (e.Button == MouseButtons.None)
            {
                // Check hovering a link view
                var closest = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5), container.IsSelectable);

                if (closest != null)
                {
                    if (closest is PipelineNodeView stepView)
                        SetHovering(stepView);
                    else if (closest is PipelineNodeLinkView linkView)
                        SetHovering(linkView);
                    else if (closest is PipelineNodeConnectionLineView pathView)
                        SetHovering(pathView);
                }
                else
                {
                    SetHovering(null);
                }
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_skipMouseUp)
                return;

            if (hasExclusiveControl)
            {
                // Handle selection of new objects if user let the mouse go over a view
                // without moving the mouse much (click event)
                if (_isDrawingSelection)
                {
                    if (_mouseDown.Distance(e.Location) < 3)
                    {
                        var view = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5, 5), container.IsSelectable);
                        if (view != null)
                            container.AttemptSelect(view);
                    }

                    _isDrawingSelection = false;
                    _pathView.RemoveFromParent();

                    // Append selection
                    foreach (var nodeView in _underSelectionArea)
                    {
                        container.AttemptSelect(nodeView);
                    }

                    _underSelectionArea.Clear();

                    ReleaseExclusiveControl();
                }
            }
            else if (!OtherFeatureHasExclusiveControl())
            {
                if (_mouseDown.Distance(e.Location) < 3)
                {
                    var view = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5, 5), container.IsSelectable);
                    if (view != null)
                        container.AttemptSelect(view);
                }
            }

            _isDrawingSelection = false;
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!OtherFeatureHasExclusiveControl())
            {
                ExecuteWithTemporaryExclusiveControl(() =>
                {
                    if (e.KeyCode != Keys.Delete)
                        return;

                    var nodes = container.SelectionModel.NodeViews();
                    var cons = container.SelectionModel.NodeLinkConnections();

                    foreach (var node in nodes)
                    {
                        container.RemoveNodeView(node);
                    }
                    foreach (var con in cons)
                    {
                        container.RemoveConnection(con.Connection);
                    }

                    if (nodes.Length > 0 || cons.Length > 0)
                        container.ClearSelection();
                });
            }
        }

        private void SetHovering([CanBeNull] BaseView view)
        {
            if (Equals(_hovering?.View, view))
                return;

            if (view == null)
                _hovering = null;
            else
                _hovering = new MouseHoverState { View = view };
        }

        #region IRenderingDecorator

        public void DecoratePipelineStep(PipelineNodeView nodeView, ref PipelineStepViewState state)
        {
            if (Equals(_hovering?.View, nodeView) || Equals((_hovering?.View as PipelineNodeLinkView)?.NodeView, nodeView))
            {
                state.StrokeWidth = 3;
            }
        }

        public void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {
            if (Equals(_hovering?.View, link))
            {
                state.StrokeWidth = 3;
            }
        }

        public void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {
            if (Equals(_hovering?.View, link))
            {
                state.StrokeWidth = 3;
            }
        }

        public void DecorateBezierPathView(BezierPathView pathView, ref BezierPathViewState state)
        {
            if (Equals(_hovering?.View, pathView))
                state.StrokeWidth += 2;
        }

        public void DecorateLabelView(LabelView pathView, ref LabelViewState state)
        {

        }

        #endregion

        private struct MouseHoverState
        {
            [NotNull]
            public BaseView View { get; set; }
        }
    }
}
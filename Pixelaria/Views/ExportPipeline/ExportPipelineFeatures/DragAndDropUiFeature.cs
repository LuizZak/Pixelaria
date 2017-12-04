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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using JetBrains.Annotations;
using PixCore.Colors;
using PixUI;
using PixUI.Utils;

using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    internal class DragAndDropUiFeature : ExportPipelineUiFeature
    {
        /// <summary>
        /// List of on-going drag operations.
        /// </summary>
        private readonly List<IDragOperation> _operations = new List<IDragOperation>();

        private bool _isMouseDown;
        private bool _isDragging;

        private Vector _mouseDownPoint;

        public DragAndDropUiFeature([NotNull] ExportPipelineControl control) : base(control)
        {

        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                _mouseDownPoint = e.Location;
                _isDragging = false;
                _isMouseDown = true;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isMouseDown = false;

            if (e.Button == MouseButtons.Left)
            {
                ConcludeDragging(e.Location);
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isMouseDown && e.Button == MouseButtons.Left)
            {
                // Dragging happens when a minimum distance has been travelled
                if (!_isDragging && _mouseDownPoint.Distance(e.Location) > 3)
                {
                    if (RequestExclusiveControl())
                        StartDragging(_mouseDownPoint);
                }

                // Dragging
                if (_isDragging)
                {
                    foreach (var operation in _operations)
                    {
                        operation.Update(e.Location);
                    }
                }
            }
        }

        /// <summary>
        /// Starts dragging operations for any object selected
        /// </summary>
        private void StartDragging(Vector mousePosition)
        {
            CancelDragging();

            if (container.Selection.Length != 0)
            {
                var nodes = container.SelectionModel.NodeViews();
                var links = container.SelectionModel.NodeLinkViews();

                // Dragging nodes takes precedense over dragging links
                if (nodes.Length > 0)
                {
                    var operation = new NodeDragOperation(container, nodes, mousePosition);
                    _operations.Add(operation);
                }
                else
                {
                    var operation = new LinkConnectionDragOperation(container, links);
                    _operations.Add(operation);
                }
            }
            else
            {
                // No selection: find view under mouse and use that instead.
                var position = contentsView.ConvertFrom(mousePosition, null);

                var viewUnder = contentsView.ViewUnder(position, new Vector(5), view => view is PipelineNodeView || view is PipelineNodeLinkView);
                if (viewUnder is PipelineNodeView nodeView)
                {
                    var operation = new NodeDragOperation(container, new[] { nodeView }, mousePosition);
                    _operations.Add(operation);
                }
                else if (viewUnder is PipelineNodeLinkView linkView)
                {
                    var operation = new LinkConnectionDragOperation(container, new[] { linkView });
                    _operations.Add(operation);
                }
            }

            // Nothing to drag!
            if (_operations.Count == 0)
            {
                CancelDragging();
            }

            _isDragging = true;
        }

        /// <summary>
        /// Concludes all current dragging operations
        /// </summary>
        private void ConcludeDragging(Vector mousePosition)
        {
            if (!_isDragging)
                return;

            foreach (var operation in _operations)
            {
                operation.Finish(mousePosition);
            }

            _operations.Clear();

            _isDragging = false;

            ReleaseExclusiveControl();
        }

        /// <summary>
        /// Cancels on-going drag operations
        /// </summary>
        private void CancelDragging()
        {
            if (!_isDragging)
                return;

            foreach (var operation in _operations)
            {
                operation.Cancel();
            }

            _operations.Clear();

            _isDragging = false;

            ReleaseExclusiveControl();
        }

        private interface IDragOperation
        {
            IReadOnlyList<object> TargetObjects { get; }

            /// <summary>
            /// Updates the on-going drag operation
            /// </summary>
            void Update(Vector mousePosition);

            /// <summary>
            /// Notifies the mouse was released on a given position
            /// </summary>
            void Finish(Vector mousePosition);

            /// <summary>
            /// Cancels the operation and undoes any changes
            /// </summary>
            void Cancel();
        }

        /// <summary>
        /// Encapsulates a drag-and-drop operation so multiple can be made at the same time.
        /// </summary>
        private sealed class NodeDragOperation : IDragOperation
        {
            /// <summary>
            /// Container used to detect drop of link connections
            /// </summary>
            private readonly ExportPipelineControl.IPipelineContainer _container;

            private readonly Vector[] _startPositions;

            /// <summary>
            /// Target objects for dragging
            /// </summary>
            private PipelineNodeView[] Targets { get; }

            public IReadOnlyList<object> TargetObjects => Targets;

            /// <summary>
            /// The node view that when dragging started was under the mouse position.
            /// 
            /// Used to guide the grid-locking of other views when they are not aligned to the grid.
            /// 
            /// In case no view was under the mouse position, the top-left-most view is used instead.
            /// </summary>
            [NotNull]
            private PipelineNodeView DragMaster { get; }

            private readonly Vector _dragMasterOffset;

            /// <summary>
            /// The relative mouse offset off of <see cref="Targets"/> when dragging started
            /// </summary>
            private readonly Vector[] _targetOffsets;

            public NodeDragOperation(ExportPipelineControl.IPipelineContainer container, [NotNull, ItemNotNull] PipelineNodeView[] targets, Vector dragStartMousePosition)
            {
                Targets = targets;
                _container = container;

                // Find master for drag operation
                var master = Targets.FirstOrDefault(view => view.Contains(view.ConvertFrom(dragStartMousePosition, null)));
                DragMaster = master ?? Targets.OrderBy(view => view.Location).First();

                _dragMasterOffset = DragMaster.ConvertFrom(dragStartMousePosition, null);

                _startPositions = Targets.Select(view => view.Location).ToArray();
                _targetOffsets = Targets.Select(view => view.Location - DragMaster.Location).ToArray();
            }

            /// <summary>
            /// Updates the on-going drag operation
            /// </summary>
            public void Update(Vector mousePosition)
            {
                // Drag master target around
                var masterAbs = DragMaster.Parent?.ConvertFrom(mousePosition, null) ?? mousePosition;
                var masterPos = masterAbs - _dragMasterOffset;

                if (System.Windows.Forms.Control.ModifierKeys.HasFlag(Keys.Control))
                    masterPos = Vector.Round(masterPos / 10) * 10;

                DragMaster.Location = masterPos;

                _container.UpdateConnectionViewsFor(DragMaster);

                foreach (var (view, targetOffset) in Targets.Zip(_targetOffsets, (v, p) => (v, p)))
                {
                    if (Equals(view, DragMaster))
                        continue;

                    var position = DragMaster.Location + targetOffset;
                    view.Location = position;

                    _container.UpdateConnectionViewsFor(view);
                }
            }

            /// <summary>
            /// Notifies the mouse was released on a given position
            /// </summary>
            public void Finish(Vector mousePosition)
            {

            }

            /// <summary>
            /// Cancels the operation and undoes any changes
            /// </summary>
            public void Cancel()
            {
                foreach (var (view, position) in Targets.Zip(_startPositions, (v1, v2) => (v1, v2)))
                {
                    view.Location = position;
                }
            }
        }

        /// <summary>
        /// Encapsulates a drag-and-drop operation so multiple can be made at the same time.
        /// </summary>
        private sealed class LinkConnectionDragOperation : IDragOperation
        {
            /// <summary>
            /// Container used to detect drop of link connections
            /// </summary>
            private readonly ExportPipelineControl.IPipelineContainer _container;

            [NotNull, ItemNotNull]
            private readonly BezierPathView[] _linkDrawingPaths;
            [NotNull, ItemNotNull]
            private readonly BezierPathView[] _linkConnectingPaths;
            [NotNull, ItemNotNull]
            private readonly LabelView[] _linkConnectionLabels;

            /// <summary>
            /// Target objects for dragging
            /// </summary>
            [NotNull, ItemNotNull]
            private PipelineNodeLinkView[] LinkViews { get; }

            public IReadOnlyList<object> TargetObjects => LinkViews;

            public LinkConnectionDragOperation([NotNull] ExportPipelineControl.IPipelineContainer container, [NotNull] PipelineNodeLinkView[] linkViews)
            {
                LinkViews = linkViews;
                _container = container;

                _linkDrawingPaths = new BezierPathView[linkViews.Length];
                _linkConnectingPaths = new BezierPathView[linkViews.Length];
                _linkConnectionLabels = new LabelView[linkViews.Length];
                for (int i = 0; i < linkViews.Length; i++)
                {
                    var pathView = new BezierPathView();
                    container.ContentsView.AddChild(pathView);
                    _linkDrawingPaths[i] = pathView;

                    var connectionView = new BezierPathView { RenderOnTop = true };
                    container.ContentsView.AddChild(connectionView);
                    _linkConnectingPaths[i] = connectionView;

                    var label = new LabelView
                    {
                        TextColor = Color.White,
                        BackgroundColor = Color.Black.Faded(Color.Transparent, 0.1f, true),
                        Text = "",
                        Visible = false,
                        TextInsetBounds = new InsetBounds(5, 5, 5, 5)
                    };

                    container.ContentsView.AddChild(label);
                    _linkConnectionLabels[i] = label;
                }
            }

            private void UpdateLinkPreview([NotNull] PipelineNodeLinkView linkView, Vector mousePosition,
                [NotNull] BezierPathView pathView)
            {
                pathView.ClearPath();

                bool toRight = linkView.NodeLink is IPipelineOutput;

                var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.ContentsView);
                var pt4 = _container.ContentsView.ConvertFrom(mousePosition, null);
                var pt2 = new Vector(toRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                var pt3 = new Vector(pt1.X, pt4.Y);

                pathView.AddBezierPoints(pt1, pt2, pt3, pt4);
            }

            private void UpdateLinkPreview([NotNull] PipelineNodeLinkView linkView, [NotNull] PipelineNodeLinkView targetLinkView,
                [NotNull] BezierPathView pathView, [NotNull] BezierPathView connectView, [NotNull] LabelView labelView)
            {
                pathView.ClearPath();
                connectView.ClearPath();

                bool isStartToRight = linkView.NodeLink is IPipelineOutput;
                bool isEndToRight = targetLinkView.NodeLink is IPipelineOutput;

                var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.ContentsView);
                var pt4 = targetLinkView.ConvertTo(targetLinkView.Bounds.Center, _container.ContentsView);
                var pt2 = new Vector(isStartToRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                var pt3 = new Vector(isEndToRight ? pt4.X + 75 : pt4.X - 75, pt4.Y);

                pathView.AddBezierPoints(pt1, pt2, pt3, pt4);

                connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.Bounds, targetLinkView).Inflated(3, 3));
                connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.NodeView.GetTitleArea(), targetLinkView.NodeView).Inflated(3, 3));

                if (targetLinkView.NodeLink.Node != null)
                {
                    labelView.Text = targetLinkView.NodeLink.Name;

                    float xOffset = isEndToRight
                        ? targetLinkView.Bounds.Width / 2 + 5
                        : -targetLinkView.Bounds.Width / 2 - labelView.Bounds.Width - 5;

                    labelView.Location =
                        _container.ContentsView.ConvertFrom(targetLinkView.Bounds.Center, targetLinkView) +
                        new Vector(xOffset, -labelView.Bounds.Height / 2);
                }
            }

            /// <summary>
            /// Updates the on-going drag operation
            /// </summary>
            public void Update(Vector mousePosition)
            {
                var rootPosition = _container.ContentsView.ConvertFrom(mousePosition, null);

                // Search for possible drop positions to drop the links onto
                var targetLinks = _container.FindTargetsForLinkViews(LinkViews, rootPosition);

                for (int i = 0; i < LinkViews.Length; i++)
                {
                    var linkView = LinkViews[i];
                    var path = _linkDrawingPaths[i];
                    var connectView = _linkConnectingPaths[i];
                    var labelView = _linkConnectionLabels[i];
                    var target = targetLinks[i];

                    if (target != null)
                    {
                        labelView.Visible = true;

                        UpdateLinkPreview(linkView, target, path, connectView, labelView);
                    }
                    else
                    {
                        labelView.Visible = false;
                        labelView.Location = Vector.Zero;

                        connectView.ClearPath();
                        UpdateLinkPreview(linkView, mousePosition, path);
                    }
                }
            }

            /// <summary>
            /// Notifies the mouse was released on a given position
            /// </summary>
            public void Finish(Vector mousePosition)
            {
                RemoveAuxiliaryViews();

                var rootPosition = _container.ContentsView.ConvertFrom(mousePosition, null);

                // We pick any link that isn't one of the ones that we're dragging
                var targets = _container.FindTargetsForLinkViews(LinkViews, rootPosition);

                // Create links
                foreach (var (linkView, target) in LinkViews.Zip(targets, (lv, t) => (lv, t)))
                {
                    if (target == null)
                        continue;

                    IPipelineInput start;
                    IPipelineOutput end;

                    // Figure out direction of connection
                    if (linkView.NodeLink is IPipelineInput && target.NodeLink is IPipelineOutput)
                    {
                        start = (IPipelineInput)linkView.NodeLink;
                        end = (IPipelineOutput)target.NodeLink;
                    }
                    else if (linkView.NodeLink is IPipelineOutput && target.NodeLink is IPipelineInput)
                    {
                        start = (IPipelineInput)target.NodeLink;
                        end = (IPipelineOutput)linkView.NodeLink;
                    }
                    else
                    {
                        continue;
                    }

                    _container.AddConnection(start, end);
                }
            }

            /// <summary>
            /// Cancels the operation and undoes any changes
            /// </summary>
            public void Cancel()
            {
                RemoveAuxiliaryViews();
            }

            private void RemoveAuxiliaryViews()
            {
                for (int i = 0; i < _linkDrawingPaths.Length; i++)
                {
                    _linkDrawingPaths[i].RemoveFromParent();
                    _linkConnectingPaths[i].RemoveFromParent();
                    _linkConnectionLabels[i].RemoveFromParent();
                }
            }
        }
    }
}
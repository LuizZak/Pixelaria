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
using PixCore.Text;
using PixCore.Text.Attributes;
using Pixelaria.ExportPipeline;
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixPipelineGraph;
using PixUI;
using PixUI.Controls;
using PixUI.Controls.ContextMenu;
using PixUI.LayoutSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

        public DragAndDropUiFeature([NotNull] IExportPipelineControl control) : base(control)
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

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isMouseDown && e.Button == MouseButtons.Left)
            {
                // Dragging happens when a minimum distance has been traveled
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
        
        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isMouseDown = false;

            if (e.Button == MouseButtons.Left)
            {
                ConcludeDragging(e.Location);
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
                // If user is not holding Shift for multiple selection,
                // drop all selected views if they start dragging something
                // under the mouse.
                var viewUnder = contentsView.ViewUnder(contentsView.ConvertFrom(mousePosition, null), new Vector(5), container.IsSelectable);
                
                // Dragging by clicking area with no nodes: Cancel drag.
                if (viewUnder == null)
                {
                    return;
                }
                // Dragging by clicking a selected node while not holding shift: Erase selection
                if (!container.SelectionModel.Contains(viewUnder) && !System.Windows.Forms.Control.ModifierKeys.HasFlag(Keys.Shift))
                {
                    container.ClearSelection();
                }
                // Always re-select view before dragging, even if already selected (selection is idempotent)
                container.AttemptSelect(viewUnder);

                var nodes = container.SelectionModel.NodeViews();
                var links = container.SelectionModel.NodeLinkViews();

                // Dragging nodes takes precedence over dragging links
                if (nodes.Length > 0)
                {
                    var operation = new NodeDragOperation(container, nodes, mousePosition);
                    _operations.Add(operation);
                }
                else
                {
                    var operation = new LinkConnectionDragOperation(container, Control, links);
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
                    var operation = new LinkConnectionDragOperation(container, Control, new[] { linkView });
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
            private readonly IPipelineContainer _container;

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

            public NodeDragOperation(IPipelineContainer container, [NotNull, ItemNotNull] PipelineNodeView[] targets, Vector dragStartMousePosition)
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
            private readonly IPipelineContainer _container;
            private readonly IExportPipelineControl _control;

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

            public LinkConnectionDragOperation([NotNull] IPipelineContainer container, [NotNull] IExportPipelineControl control, [NotNull] PipelineNodeLinkView[] linkViews)
            {
                LinkViews = linkViews;
                _container = container;
                _control = control;
                _linkDrawingPaths = new BezierPathView[linkViews.Length];
                _linkConnectingPaths = new BezierPathView[linkViews.Length];
                _linkConnectionLabels = new LabelView[linkViews.Length];
                for (int i = 0; i < linkViews.Length; i++)
                {
                    var pathView = BezierPathView.Create();
                    container.ContentsView.AddChild(pathView);
                    _linkDrawingPaths[i] = pathView;

                    var connectionView = BezierPathView.Create();
                    connectionView.RenderOnTop = true;

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

            private void UpdateLinkPreview([NotNull] PipelineNodeLinkView linkView, Vector mousePosition, [NotNull] BezierPathView pathView)
            {
                pathView.ClearPath();

                bool toRight = linkView is PipelineNodeOutputLinkView;

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

                bool isStartToRight = linkView is PipelineNodeOutputLinkView;
                bool isEndToRight = targetLinkView is PipelineNodeOutputLinkView;

                var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.ContentsView);
                var pt4 = targetLinkView.ConvertTo(targetLinkView.Bounds.Center, _container.ContentsView);
                var pt2 = new Vector(isStartToRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                var pt3 = new Vector(isEndToRight ? pt4.X + 75 : pt4.X - 75, pt4.Y);

                pathView.AddBezierPoints(pt1, pt2, pt3, pt4);

                connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.Bounds, targetLinkView).Inflated(3, 3));
                connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.NodeView.GetTitleArea(), targetLinkView.NodeView).Inflated(3, 3));

                labelView.Text = targetLinkView.Title;

                float xOffset = isEndToRight
                    ? targetLinkView.Bounds.Width / 2 + 5
                    : -targetLinkView.Bounds.Width / 2 - labelView.Bounds.Width - 5;

                labelView.Location =
                    _container.ContentsView.ConvertFrom(targetLinkView.Bounds.Center, targetLinkView) +
                    new Vector(xOffset, -labelView.Bounds.Height / 2);
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
                var rootPosition = _container.ContentsView.ConvertFrom(mousePosition, null);

                // We pick any link that isn't one of the ones that we're dragging
                var targets = _container.FindTargetsForLinkViews(LinkViews, rootPosition);

                if (LinkViews.Length > 0 && targets.Count(e => e != null) == 0)
                {
                    // Find common type
                    Type commonType = LinkViews[0].LinkType;
                    bool isInput = LinkViews[0] is PipelineNodeInputLinkView;
                    foreach (PipelineNodeLinkView linkView in LinkViews)
                    {
                        // If any unmatching type is found, abort the operation and cancel.
                        if (linkView.LinkType != commonType || (linkView is PipelineNodeInputLinkView) != isInput)
                        {
                            RemoveAuxiliaryViews();
                            return;
                        }
                    }

                    var targetPoint = _control.MousePoint;
                    var potentialNodes = DefaultPipelineGraphNodeProvider.Instance.PotentialConnectionsForConnectionType(commonType, !isInput);

                    PipelineNodeView currentDisplayNode = null;
                    bool isApplied = false;

                    void createPreviewNode(int potentialNodeIndex)
                    {
                        var potentialNode = potentialNodes[potentialNodeIndex];

                        currentDisplayNode = _container.CreateNodeView(potentialNode.NodeKind, potentialNode.Icon, targetPoint);
                        BaseView targetView;

                        if (potentialNode.IsInput)
                        {
                            targetView = currentDisplayNode.InputViews[potentialNode.LinkIndex];
                        }
                        else
                        {
                            targetView = currentDisplayNode.OutputViews[potentialNode.LinkIndex];
                        }

                        var linkOffset = targetView.ConvertTo(targetView.Size / 2, currentDisplayNode);
                        currentDisplayNode.Location -= linkOffset;
                    }

                    void destroyPreviewNode()
                    {
                        if (!isApplied && currentDisplayNode != null)
                        {
                            _container.RemoveNodeView(currentDisplayNode);
                            currentDisplayNode = null;
                        }
                    }

                    void applyPreviewNode(int potentialNodeIndex)
                    {
                        var potentialNode = potentialNodes[potentialNodeIndex];

                        isApplied = true;

                        if (currentDisplayNode == null)
                        {
                            createPreviewNode(potentialNodeIndex);
                        }

                        // Create links
                        foreach (var linkView in LinkViews)
                        {
                            PipelineInput? start;
                            PipelineOutput? end;

                            // Figure out direction of connection
                            if (linkView is PipelineNodeInputLinkView input && !potentialNode.IsInput)
                            {
                                start = input.InputId;
                                end = currentDisplayNode.OutputViews[potentialNode.LinkIndex].OutputId;
                            }
                            else if (linkView is PipelineNodeOutputLinkView output && potentialNode.IsInput)
                            {
                                start = currentDisplayNode.InputViews[potentialNode.LinkIndex].InputId;
                                end = output.OutputId;
                            }
                            else
                            {
                                continue;
                            }

                            if (start.HasValue && end.HasValue)
                                _container.AddConnection(start.Value, end.Value);
                        }
                    }

                    /* TODO: Ideally will be managed in-engine with ContextMenuControl
                    */

                    var _dropDown = new ContextMenuDropDownItem("root");

                    var allItems = new List<ContextMenuDropDownItem>();
                    var visibleItems = new List<ContextMenuDropDownItem>();

                    var searchBox = TextField.Create(true);
                    searchBox.Layout();
                    searchBox.Size = new Vector(100, 26);
                    searchBox.TextChanged += (sender, args) =>
                    {
                        visibleItems.Clear();

                        if (string.IsNullOrEmpty(args.Text))
                        {
                            foreach (var item in allItems)
                            {
                                item.Visible = true;
                                item.AttributedName = new AttributedText(item.Name);
                                visibleItems.Add(item);
                            }
                        }
                        else
                        {
                            foreach (var item in allItems)
                            {
                                var index = item.Name.IndexOf(args.Text, StringComparison.InvariantCultureIgnoreCase);
                                var textBuilder = new AttributedTextBuilder(item.Name);

                                if (index != -1)
                                {
                                    item.Visible = true;
                                    textBuilder.SetAttributes(new TextRange(index, args.Text.Length), new ITextAttribute[]
                                    {
                                        new BackgroundColorAttribute(Color.Blue)
                                    });

                                    visibleItems.Add(item);
                                }
                                else
                                {
                                    item.Visible = false;
                                }

                                item.AttributedName = textBuilder.MakeAttributedText();
                            }
                        }
                    };
                    searchBox.KeyDown += (sender, args) =>
                    {
                        if (args.KeyCode == Keys.Down)
                        {
                            args.Handled = true;
                            args.SuppressKeyPress = true;

                            int selectedIndex = -1;

                            for (int i = 0; i < visibleItems.Count; i++)
                            {
                                if (visibleItems[i].Selected)
                                {
                                    selectedIndex = i;
                                    break;
                                }
                            }

                            if (selectedIndex < visibleItems.Count - 1)
                                selectedIndex++;

                            if (selectedIndex > -1 && selectedIndex < visibleItems.Count)
                            {
                                var item = visibleItems[selectedIndex];

                                item.Select();
                            }
                        }
                        else if (args.KeyCode == Keys.Up)
                        {
                            args.Handled = true;
                            args.SuppressKeyPress = true;
                            int selectedIndex = -1;

                            for (int i = 0; i < visibleItems.Count; i++)
                            {
                                if (visibleItems[i].Selected)
                                {
                                    selectedIndex = i;
                                    break;
                                }
                            }

                            if (selectedIndex > 0)
                                selectedIndex--;

                            if (selectedIndex > -1 && visibleItems.Count > 0)
                            {
                                var item = visibleItems[selectedIndex];

                                item.Select();
                            }
                        }
                        else if (args.KeyCode == Keys.Enter)
                        {
                            args.Handled = true;
                            args.SuppressKeyPress = true;

                            foreach (var item in allItems)
                            {
                                if (item.Visible && item.Selected)
                                {
                                    item.PerformClick();
                                    args.SuppressKeyPress = true;
                                    args.Handled = true;
                                    break;
                                }
                            }
                        }
                    };

                    _dropDown.DropDownItems.Add(new ContextMenuControlHostItem(searchBox) { CreateConstraints = false });

                    for (int i = 0; i < potentialNodes.Count; i++)
                    {
                        int index = i;
                        var potentialNode = potentialNodes[i];
                        var item = _dropDown.DropDownItems.Add(potentialNode.NodeDisplayName);

                        allItems.Add(item);
                        visibleItems.Add(item);

                        item.SelectChange += (sender, e) =>
                        {
                            if (item.Selected)
                            {
                                destroyPreviewNode();
                                createPreviewNode(index);
                            }
                        };
                        item.MouseEnter += (sender, e) =>
                        {
                            destroyPreviewNode();
                            createPreviewNode(index);
                        };
                        item.MouseLeave += (sender, e) =>
                        {
                            destroyPreviewNode();
                        };
                        item.Click += (sender, e) =>
                        {
                            applyPreviewNode(index);
                        };
                    }

                    var _contextMenu = ContextMenuControl.Create(_dropDown);
                    _contextMenu.AreaIntoConstraintsMask = BoundsConstraintMask.Size;
                    _contextMenu.Layout();
                    _contextMenu.Closed += (sender, e) =>
                    {
                        if (!isApplied)
                        {
                            destroyPreviewNode();
                        }

                        RemoveAuxiliaryViews();
                    };

                    _container.ShowAsDialog(_contextMenu);

                    searchBox.BecomeFirstResponder();

                    LayoutConstraint.Create(_contextMenu.Anchors.Left, _contextMenu.Parent.Anchors.Left, LayoutRelationship.GreaterThanOrEqual, priority: Cassowary.ClStrength.Strong);
                    LayoutConstraint.Create(_contextMenu.Anchors.Top, _contextMenu.Parent.Anchors.Top, LayoutRelationship.GreaterThanOrEqual, priority: Cassowary.ClStrength.Strong);
                    LayoutConstraint.Create(_contextMenu.Anchors.Bottom, _contextMenu.Parent.Anchors.Bottom, LayoutRelationship.LessThanOrEqual, priority: Cassowary.ClStrength.Strong);
                    LayoutConstraint.Create(_contextMenu.Anchors.Right, _contextMenu.Parent.Anchors.Right, LayoutRelationship.LessThanOrEqual, priority: Cassowary.ClStrength.Strong);

                    LayoutConstraint.Create(_contextMenu.Anchors.Left, priority: Cassowary.ClStrength.Weak, constant: targetPoint.X);
                    LayoutConstraint.Create(_contextMenu.Anchors.Top, priority: Cassowary.ClStrength.Weak, constant: targetPoint.Y);

                    return;

                    var itemNames = potentialNodes.Select(n => n.NodeDisplayName);

                    var contextMenuManager = new SearchContextMenuManager(itemNames);

                    contextMenuManager.ItemSelected += (sender, args) =>
                    {
                        destroyPreviewNode();
                        createPreviewNode(args.Index);
                    };
                    contextMenuManager.ItemMouseEnter += (sender, args) =>
                    {
                        destroyPreviewNode();
                        createPreviewNode(args.Index);
                    };
                    contextMenuManager.ItemMouseLeave += (sender, args) =>
                    {
                        destroyPreviewNode();
                    };
                    contextMenuManager.ItemClick += (sender, args) =>
                    {
                        applyPreviewNode(args.Index);
                    };

                    if (_control is Control control)
                    {
                        var menu = contextMenuManager.GenerateContextMenu();

                        menu.Closing += (sender, args) =>
                        {
                            if (!isApplied)
                            {
                                destroyPreviewNode();
                            }

                            RemoveAuxiliaryViews();
                        };

                        if (!isInput)
                        {
                            menu.Show(control, targetPoint, ToolStripDropDownDirection.BelowLeft);
                        }
                        else
                        {
                            menu.Show(control, targetPoint, ToolStripDropDownDirection.BelowRight);
                        }
                    }
                    else
                    {
                        RemoveAuxiliaryViews();
                    }

                    return;
                }

                RemoveAuxiliaryViews();

                // Create links
                foreach (var (linkView, target) in LinkViews.Zip(targets, (lv, t) => (lv, t)))
                {
                    if (target == null)
                        continue;

                    PipelineInput? start;
                    PipelineOutput? end;

                    // Figure out direction of connection
                    if (linkView is PipelineNodeInputLinkView input && target is PipelineNodeOutputLinkView output)
                    {
                        start = input.InputId;
                        end = output.OutputId;
                    }
                    else if (linkView is PipelineNodeOutputLinkView pipelineOutput && target is PipelineNodeInputLinkView pipelineInput)
                    {
                        start = pipelineInput.InputId;
                        end = pipelineOutput.OutputId;
                    }
                    else
                    {
                        continue;
                    }

                    if (start.HasValue && end.HasValue)
                        _container.AddConnection(start.Value, end.Value);
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
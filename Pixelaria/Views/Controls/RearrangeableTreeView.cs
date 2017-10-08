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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Pixelaria.Utils;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Describes a TreeView that can have its TreeNodes rearranged
    /// </summary>
    public class RearrangeableTreeView : TreeView
    {
        /// <summary>
        /// The node currently being dragged
        /// </summary>
        private TreeNode _draggedNode;

        /// <summary>
        /// The TreeNode currently being hovered with the currently dragged node
        /// </summary>
        private TreeNode _tempDropNode;

        /// <summary>
        /// Image list used for the drag operation
        /// </summary>
        private readonly ImageList _imageListDrag;

        /// <summary>
        /// Timer for scrolling
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Event handler fired when a drag operation has started or ended
        /// </summary>
        public delegate void DragOperationHandler(object sender, TreeViewNodeDragEventArgs e);
        /// <summary>
        /// Event fired when a drag operation has started or ended
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a drag operation has started or ended")]
        public event DragOperationHandler DragOperation;

        /// <summary>
        /// Creates a new RearrangeableTreeView
        /// </summary>
        public RearrangeableTreeView()
        {
            _imageListDrag = new ImageList();
            _timer = new Timer();
            _timer.Tick += timer_Tick;
        }

        protected override void OnCreateControl()
        {
            AllowDrop = true;
        }

        // 
        // Timer tick. Updates scrolling of the TreeView
        // 
        private void timer_Tick(object sender, EventArgs e)
        {
            // get node at mouse position
            Point pt = PointToClient(MousePosition);
            TreeNode node = GetNodeAt(pt);

            if (node == null) return;

            // if mouse is near to the top, scroll up
            if (pt.Y < 30)
            {
                // set actual node to the upper one
                if (node.PrevVisibleNode != null)
                {
                    node = node.PrevVisibleNode;

                    // hide drag image
                    UnsafeNativeMethods.ImageList_DragShowNolock(false);
                    // scroll and refresh
                    node.EnsureVisible();
                    Refresh();
                    // show drag image
                    UnsafeNativeMethods.ImageList_DragShowNolock(true);
                }
            }
            // if mouse is near to the bottom, scroll down
            else if (pt.Y > Size.Height - 30)
            {
                if (node.NextVisibleNode != null)
                {
                    node = node.NextVisibleNode;

                    UnsafeNativeMethods.ImageList_DragShowNolock(false);
                    node.EnsureVisible();
                    Refresh();
                    UnsafeNativeMethods.ImageList_DragShowNolock(true);
                }
            }
        }

        // 
        // OnItemDrag event handler. Starts dragging a node
        // 
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);

            SelectedNode = (TreeNode)e.Item;

            TreeViewNodeDragEventArgs evArgs = new TreeViewNodeDragEventArgs(TreeViewNodeDragEventType.DragStart, TreeViewNodeDragEventBehavior.PlaceInside, (TreeNode)e.Item, null);

            if(DragOperation != null)
            {
                DragOperation(this, evArgs);

                // Cancel the operation if the user specified so
                if (evArgs.Cancel)
                {
                    return;
                }
            }

            // Get drag node and select it
            _draggedNode = (TreeNode)e.Item;
            SelectedNode = evArgs.DraggedNode;

            // Reset image list used for drag image
            _imageListDrag.Images.Clear();
            _imageListDrag.ImageSize = new Size(_draggedNode.Bounds.Size.Width + Indent, _draggedNode.Bounds.Height);

            //// Create new bitmap

            // This bitmap will contain the tree node image to be dragged
            Bitmap bmp = new Bitmap(_draggedNode.Bounds.Width + Indent, _draggedNode.Bounds.Height);

            SolidBrush brush = new SolidBrush(ForeColor);

            // Get graphics from bitmap
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Draw node icon into the bitmap
            if (_draggedNode.ImageKey == "")
            {
                gfx.DrawImage(ImageList.Images[_draggedNode.ImageIndex], 0, 0);
            }
            else
            {
                if (ImageList?.Images[_draggedNode.ImageKey] != null)
                    gfx.DrawImage(ImageList.Images[_draggedNode.ImageKey], 0, 0);
            }

            // Draw node label into bitmap
            gfx.DrawString(_draggedNode.Text, Font, brush, Indent, 1.0f);

            gfx.Flush();
            gfx.Dispose();

            brush.Dispose();

            // Add bitmap to imagelist
            _imageListDrag.Images.Add(bmp);

            // Get mouse position in client coordinates
            Point p = PointToClient(MousePosition);

            // Compute delta between mouse position and node bounds
            int dx = p.X + Indent - _draggedNode.Bounds.Left;
            int dy = p.Y - _draggedNode.Bounds.Top;

            // Begin dragging image
            if (UnsafeNativeMethods.ImageList_BeginDrag(_imageListDrag.Handle, 0, dx, dy))
            {
                // Begin dragging
                DoDragDrop(bmp, DragDropEffects.Move);
                // End dragging image
                UnsafeNativeMethods.ImageList_EndDrag();
            }
        }

        // 
        // OnDragEnter event handler. Updates the dragged node's ghost image's position.
        // 
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            UnsafeNativeMethods.ImageList_DragEnter(Handle, drgevent.X - Left, drgevent.Y - Top);

            // Enable timer for scrolling dragged item
            _timer.Enabled = true;
        }

        // 
        // OnDragLeave event handler. Updates the dragged node's ghost image's position.
        // 
        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            UnsafeNativeMethods.ImageList_DragLeave(Handle);

            // Disable timer for scrolling dragged item
            _timer.Enabled = false;
        }

        // 
        // OnDragOver event handler. Updates the dragged node's ghost image's position
        // 
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            // Cancel if no node is being dragged
            if (_draggedNode == null)
                return;

            // Compute drag position and move image
            var findForm = FindForm();
            if (findForm != null)
            {
                Point formP = findForm.PointToClient(new Point(drgevent.X, drgevent.Y));
                UnsafeNativeMethods.ImageList_DragMove(formP.X - Left, formP.Y - Top);
            }

            // Get actual drop node
            TreeNode dropNode = GetNodeAt(PointToClient(new Point(drgevent.X, drgevent.Y)));
            if (dropNode == null)
            {
                drgevent.Effect = DragDropEffects.None;
                return;
            }

            TreeViewNodeDragEventArgs evArgs = new TreeViewNodeDragEventArgs(TreeViewNodeDragEventType.DragOver, TreeViewNodeDragEventBehavior.PlaceInside, _draggedNode, dropNode);

            if (DragOperation != null)
            {
                DragOperation(this, evArgs);

                // Cancel the operation if the user specified so
                if (evArgs.Cancel)
                {
                    drgevent.Effect = DragDropEffects.None;
                    OnDragDrop(drgevent);
                    return;
                }
                if (!evArgs.Allow)
                {
                    return;
                }
            }

            drgevent.Effect = DragDropEffects.Move;

            // Dissalow the drag here
            if (!evArgs.Allow)
            {
                drgevent.Effect = DragDropEffects.None;
            }

            // if mouse is on a new node select it
            if (_tempDropNode != dropNode)
            {
                UnsafeNativeMethods.ImageList_DragShowNolock(false);
                SelectedNode = dropNode;
                UnsafeNativeMethods.ImageList_DragShowNolock(true);
                _tempDropNode = dropNode;
            }

            // Avoid that drop node is child of drag node 
            TreeNode tmpNode = dropNode;
            while (tmpNode.Parent != null)
            {
                if (tmpNode.Parent == _draggedNode) drgevent.Effect = DragDropEffects.None;
                tmpNode = tmpNode.Parent;
            }
        }

        // 
        // OnGiveFeedback event handler. Occurs during a drag operation and updates the mouse cursor
        // 
        protected override void OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            base.OnGiveFeedback(gfbevent);

            if (gfbevent.Effect == DragDropEffects.Move)
            {
                // Show pointer cursor while dragging
                gfbevent.UseDefaultCursors = false;
                Cursor = Cursors.Default;
            }
            else
            {
                gfbevent.UseDefaultCursors = true;
            }
        }

        // 
        // OnDragDrop event handler. Ends a node dragging
        // 
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            // Cancel if no node is being dragged
            if (_draggedNode == null)
                return;

            // Unlock updates
            UnsafeNativeMethods.ImageList_DragLeave(Handle);

            if (drgevent.Effect == DragDropEffects.None)
            {
                // Set drag node and temp drop node to null
                _draggedNode = null;
                _tempDropNode = null;

                // Disable scroll timer
                _timer.Enabled = false;
                return;
            }

            // Get drop node
            TreeNode dropNode = GetNodeAt(PointToClient(new Point(drgevent.X, drgevent.Y)));

            // Launch the feedback for the drag operation
            TreeViewNodeDragEventArgs evArgs = new TreeViewNodeDragEventArgs(TreeViewNodeDragEventType.DragEnd, TreeViewNodeDragEventBehavior.PlaceInside, _draggedNode, dropNode);

            if (DragOperation != null)
            {
                DragOperation(this, evArgs);

                // Cancel the operation if the user specified so
                if (evArgs.Cancel)
                    return;
            }

            // If drop node isn't equal to drag node, add drag node as child of drop node
            if (_draggedNode != dropNode)
            {
                // Remove drag node from parent
                if (_draggedNode.Parent == null)
                {
                    Nodes.Remove(_draggedNode);
                }
                else
                {
                    _draggedNode.Parent.Nodes.Remove(_draggedNode);
                }

                // Place the dragged node before or after the target node depending on mouse position
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceBeforeOrAfterAuto)
                {
                    // Get drop node
                    Point mouseP = PointToClient(MousePosition);

                    // Figure out whether the node should be added uder or over the pointed item by checking if the mouse is under or over the middle of the item
                    evArgs.EventBehavior = mouseP.Y > dropNode.Bounds.Y + dropNode.Bounds.Height / 2
                        ? TreeViewNodeDragEventBehavior.PlaceAfter
                        : TreeViewNodeDragEventBehavior.PlaceBefore;
                }

                // Place the dragged node before the target node
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceBefore)
                {
                    // Add drag node before drop node
                    if (dropNode.Parent != null)
                    {
                        dropNode.Parent.Nodes.Insert(dropNode.Index, _draggedNode);
                    }
                    else
                    {
                        Nodes.Insert(dropNode.Index, _draggedNode);
                    }

                    SelectedNode = _draggedNode;
                }
                // Place the dragged node after the target node
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceAfter)
                {
                    // Add drag node after drop node
                    if (dropNode.Parent != null)
                    {
                        dropNode.Parent.Nodes.Insert(dropNode.Index + 1, _draggedNode);
                    }
                    else
                    {
                        Nodes.Insert(dropNode.Index + 1, _draggedNode);
                    }

                    SelectedNode = _draggedNode;
                }
                // Place the dragged node inside the target node
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceInside)
                {
                    // Add drag node to drop node
                    dropNode.Nodes.Add(_draggedNode);
                    dropNode.ExpandAll();
                }

                // Launch the feedback for the drag operation
                evArgs = new TreeViewNodeDragEventArgs(TreeViewNodeDragEventType.AfterDragEnd, evArgs.EventBehavior, _draggedNode, dropNode);

                DragOperation?.Invoke(this, evArgs);

                // Set drag node and temp drop node to null
                _draggedNode = null;
                _tempDropNode = null;

                // Disable scroll timer
                _timer.Enabled = false;
            }
        }
    }

    /// <summary>
    /// Class with util methods used by Rearrangeable* controls
    /// </summary>
    internal class DragHelper
    {
        static DragHelper()
        {
            UnsafeNativeMethods.InitCommonControls();
        }
    }

    /// <summary>
    /// Event arguments for a RearrangeableTreeView node drag
    /// </summary>
    public class TreeViewNodeDragEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the TreeViewNodeDragEventArgs class
        /// </summary>
        /// <param name="eventType">The type for this event</param>
        /// <param name="eventBehavior">The behavior of this event</param>
        /// <param name="draggedNode">The node being dragged</param>
        /// <param name="targetNode">The node that the dragged node was dropped at. If the value for the eventType is set to TreeViewNodeDragEventType.DragEnd, this field is automatically set to null.</param>
        public TreeViewNodeDragEventArgs(TreeViewNodeDragEventType eventType, TreeViewNodeDragEventBehavior eventBehavior, TreeNode draggedNode, TreeNode targetNode)
        {
            // The Cancel and Allow flags are set by the user and start with their default values
            Cancel = false;
            Allow = true;

            EventType = eventType;
            EventBehavior = eventBehavior;
            DraggedNode = draggedNode;
            TargetNode = (eventType == TreeViewNodeDragEventType.DragStart ? null : targetNode);
        }

        /// <summary>
        /// Gets the type of this event
        /// </summary>
        public TreeViewNodeDragEventType EventType { get; }

        /// <summary>
        /// Gets or sets the behavior of this event.
        /// This value will only be used when the EventType is TreeViewNodeDragEventType.DragEnd
        /// </summary>
        public TreeViewNodeDragEventBehavior EventBehavior { get; set; }

        /// <summary>
        /// Gets the node being dragged
        /// </summary>
        public TreeNode DraggedNode { get; }

        /// <summary>
        /// Gets the node that the dragged node was dropped at.
        /// If the EventType is set to TreeViewNodeDragEventType.DragEnd, this
        /// value is null
        /// </summary>
        public TreeNode TargetNode { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether the drag operation is to be canceled
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the drag operation is currently set to be allowed
        /// </summary>
        public bool Allow { get; set; }
    }

    /// <summary>
    /// Specifies the type of a TreeViewNodeDragEventArgs event
    /// </summary>
    public enum TreeViewNodeDragEventType
    {
        /// <summary>
        /// A Drag Start operation
        /// </summary>
        DragStart,
        /// <summary>
        /// A Drag Over operation, fired when the user hovers over another node with the current node being dragged
        /// </summary>
        DragOver,
        /// <summary>
        /// A Drag End operation
        /// </summary>
        DragEnd,
        /// <summary>
        /// Firead after a drag event has been successful
        /// </summary>
        AfterDragEnd
    }

    /// <summary>
    /// Specifies the type of behavior to apply to the dragged node once the drag operation is over
    /// </summary>
    public enum TreeViewNodeDragEventBehavior
    {
        /// <summary>
        /// Specifies that the dragged node should be placed as the target node's child node
        /// </summary>
        PlaceInside,
        /// <summary>
        /// Specifies that the dragged node should be placed before the target node
        /// </summary>
        PlaceBefore,
        /// <summary>
        /// Specifies that the dragged node should be placed after the target node
        /// </summary>
        PlaceAfter,
        /// <summary>
        /// Specifies that the dragged node should be placed before or after the target node depending on the mouse's position
        /// </summary>
        PlaceBeforeOrAfterAuto,
        /// <summary>
        /// Specifies that the dragged node and target node should switch places
        /// </summary>
        Switch
    }
}
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
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
        private TreeNode draggedNode;

        /// <summary>
        /// The TreeNode currently being hovered with the currently dragged node
        /// </summary>
        private TreeNode tempDropNode;

        /// <summary>
        /// Image list used for the drag operation
        /// </summary>
        private ImageList imageListDrag;

        /// <summary>
        /// Timer for scrolling
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Event handler fired when a drag operation has started or ended
        /// </summary>
        /// <param name="eventArgs">The TreeViewNodeDragEventArgs for the drag operation</param>
        public delegate void DragOperationHandler(TreeViewNodeDragEventArgs eventArgs);
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
            imageListDrag = new ImageList();
            timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);

            this.AllowDrop = true;
        }

        // 
        // Timer tick. Updates scrolling of the TreeView
        // 
        private void timer_Tick(object sender, EventArgs e)
        {
            // get node at mouse position
            Point pt = PointToClient(Control.MousePosition);
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
                    DragHelper.ImageList_DragShowNolock(false);
                    // scroll and refresh
                    node.EnsureVisible();
                    Refresh();
                    // show drag image
                    DragHelper.ImageList_DragShowNolock(true);
                }
            }
            // if mouse is near to the bottom, scroll down
            else if (pt.Y > Size.Height - 30)
            {
                if (node.NextVisibleNode != null)
                {
                    node = node.NextVisibleNode;

                    DragHelper.ImageList_DragShowNolock(false);
                    node.EnsureVisible();
                    Refresh();
                    DragHelper.ImageList_DragShowNolock(true);
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
                DragOperation(evArgs);

                // Cancel the operation if the user specified so
                if (evArgs.Cancel)
                {
                    return;
                }
            }

            // Get drag node and select it
            draggedNode = (TreeNode)e.Item;
            SelectedNode = evArgs.DraggedNode;

            // Reset image list used for drag image
            imageListDrag.Images.Clear();
            imageListDrag.ImageSize = new Size(draggedNode.Bounds.Size.Width + Indent, draggedNode.Bounds.Height);

            //// Create new bitmap

            // This bitmap will contain the tree node image to be dragged
            Bitmap bmp = new Bitmap(draggedNode.Bounds.Width + Indent, draggedNode.Bounds.Height);

            SolidBrush brush = new SolidBrush(ForeColor);

            // Get graphics from bitmap
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Draw node icon into the bitmap
            if (draggedNode.ImageKey == "")
            {
                gfx.DrawImage(ImageList.Images[draggedNode.ImageIndex], 0, 0);
            }
            else
            {
                gfx.DrawImage(ImageList.Images[draggedNode.ImageKey], 0, 0);
            }

            // Draw node label into bitmap
            gfx.DrawString(draggedNode.Text, Font, brush, Indent, 1.0f);

            gfx.Flush();
            gfx.Dispose();

            brush.Dispose();

            // Add bitmap to imagelist
            imageListDrag.Images.Add(bmp);

            // Get mouse position in client coordinates
            Point p = PointToClient(MousePosition);

            // Compute delta between mouse position and node bounds
            int dx = p.X + Indent - draggedNode.Bounds.Left;
            int dy = p.Y - draggedNode.Bounds.Top;

            // Begin dragging image
            if (DragHelper.ImageList_BeginDrag(imageListDrag.Handle, 0, dx, dy))
            {
                // Begin dragging
                DoDragDrop(bmp, DragDropEffects.Move);
                // End dragging image
                DragHelper.ImageList_EndDrag();
            }
        }

        // 
        // OnDragEnter event handler. Updates the dragged node's ghost image's position.
        // 
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            DragHelper.ImageList_DragEnter(Handle, drgevent.X - Left, drgevent.Y - Top);

            // Enable timer for scrolling dragged item
            this.timer.Enabled = true;
        }

        // 
        // OnDragLeave event handler. Updates the dragged node's ghost image's position.
        // 
        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            DragHelper.ImageList_DragLeave(Handle);

            // Disable timer for scrolling dragged item
            this.timer.Enabled = false;
        }

        // 
        // OnDragOver event handler. Updates the dragged node's ghost image's position
        // 
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            // Cancel if no node is being dragged
            if (draggedNode == null)
                return;

            // Compute drag position and move image
            Point formP = this.FindForm().PointToClient(new Point(drgevent.X, drgevent.Y));
            DragHelper.ImageList_DragMove(formP.X - Left, formP.Y - Top);

            // Get actual drop node
            TreeNode dropNode = GetNodeAt(PointToClient(new Point(drgevent.X, drgevent.Y)));
            if (dropNode == null)
            {
                drgevent.Effect = DragDropEffects.None;
                return;
            }

            TreeViewNodeDragEventArgs evArgs = new TreeViewNodeDragEventArgs(TreeViewNodeDragEventType.DragOver, TreeViewNodeDragEventBehavior.PlaceInside, draggedNode, dropNode);

            if (DragOperation != null)
            {
                DragOperation(evArgs);

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
            if (tempDropNode != dropNode)
            {
                DragHelper.ImageList_DragShowNolock(false);
                SelectedNode = dropNode;
                DragHelper.ImageList_DragShowNolock(true);
                tempDropNode = dropNode;
            }

            // Avoid that drop node is child of drag node 
            TreeNode tmpNode = dropNode;
            while (tmpNode.Parent != null)
            {
                if (tmpNode.Parent == draggedNode) drgevent.Effect = DragDropEffects.None;
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
            if (draggedNode == null)
                return;

            // Unlock updates
            DragHelper.ImageList_DragLeave(Handle);

            if (drgevent.Effect == DragDropEffects.None)
            {
                // Set drag node and temp drop node to null
                draggedNode = null;
                tempDropNode = null;

                // Disable scroll timer
                this.timer.Enabled = false;
                return;
            }

            // Get drop node
            TreeNode dropNode = GetNodeAt(PointToClient(new Point(drgevent.X, drgevent.Y)));

            // Launch the feedback for the drag operation
            TreeViewNodeDragEventArgs evArgs = new TreeViewNodeDragEventArgs(TreeViewNodeDragEventType.DragEnd, TreeViewNodeDragEventBehavior.PlaceInside, draggedNode, dropNode);

            if (DragOperation != null)
            {
                DragOperation(evArgs);

                // Cancel the operation if the user specified so
                if (evArgs.Cancel)
                    return;
            }

            // If drop node isn't equal to drag node, add drag node as child of drop node
            if (draggedNode != dropNode)
            {
                // Remove drag node from parent
                if (draggedNode.Parent == null)
                {
                    Nodes.Remove(draggedNode);
                }
                else
                {
                    draggedNode.Parent.Nodes.Remove(draggedNode);
                }

                // Place the dragged node before or after the target node depending on mouse position
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceBeforeOrAfterAuto)
                {
                    // Get drop node
                    Point mouseP = PointToClient(MousePosition);//new Point(drgevent.X, drgevent.Y));

                    if (mouseP.Y > dropNode.Bounds.Y + dropNode.Bounds.Height / 2)
                    {
                        evArgs.EventBehavior = TreeViewNodeDragEventBehavior.PlaceAfter;
                    }
                    else
                    {
                        evArgs.EventBehavior = TreeViewNodeDragEventBehavior.PlaceBefore;
                    }
                }

                // Place the dragged node before the target node
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceBefore)
                {
                    // Add drag node before drop node
                    if (dropNode.Parent != null)
                    {
                        dropNode.Parent.Nodes.Insert(dropNode.Index, draggedNode);
                    }
                    else
                    {
                        Nodes.Insert(dropNode.Index, draggedNode);
                    }

                    this.SelectedNode = draggedNode;
                }
                // Place the dragged node after the target node
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceAfter)
                {
                    // Add drag node after drop node
                    if (dropNode.Parent != null)
                    {
                        dropNode.Parent.Nodes.Insert(dropNode.Index + 1, draggedNode);
                    }
                    else
                    {
                        Nodes.Insert(dropNode.Index + 1, draggedNode);
                    }

                    this.SelectedNode = draggedNode;
                }
                // Place the dragged node inside the target node
                if (evArgs.EventBehavior == TreeViewNodeDragEventBehavior.PlaceInside)
                {
                    // Add drag node to drop node
                    dropNode.Nodes.Add(draggedNode);
                    dropNode.ExpandAll();
                }

                // Launch the feedback for the drag operation
                evArgs = new TreeViewNodeDragEventArgs(TreeViewNodeDragEventType.AfterDragEnd, evArgs.EventBehavior, draggedNode, dropNode);

                if (DragOperation != null)
                {
                    DragOperation(evArgs);
                }

                // Set drag node and temp drop node to null
                draggedNode = null;
                tempDropNode = null;

                // Disable scroll timer
                this.timer.Enabled = false;
            }
        }
    }

    /// <summary>
    /// Class with util methods used by Rearrangeable* cotrols
    /// </summary>
    public class DragHelper
    {
        [DllImport("comctl32.dll")]
        public static extern bool InitCommonControls();

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImageList_BeginDrag(
            IntPtr himlTrack, // Handler of the image list containing the image to drag
            int iTrack,       // Index of the image to drag 
            int dxHotspot,    // x-delta between mouse position and drag image
            int dyHotspot     // y-delta between mouse position and drag image
        );

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImageList_DragMove(
            int x,            // X-coordinate (relative to the form, not the treeview) at which to display the drag image.
            int y             // Y-coordinate (relative to the form, not the treeview) at which to display the drag image.
        );

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        public static extern void ImageList_EndDrag();

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImageList_DragEnter(
            IntPtr hwndLock,  // Handle to the control that owns the drag image.
            int x,            // X-coordinate (relative to the treeview) at which to display the drag image. 
            int y             // Y-coordinate (relative to the treeview) at which to display the drag image. 
        );

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImageList_DragLeave(
            IntPtr hwndLock  // Handle to the control that owns the drag image.
        );

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImageList_DragShowNolock(
            bool fShow       // False to hide, true to show the image
        );

        static DragHelper()
        {
            InitCommonControls();
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
            this.cancel = false;
            this.allow = true;

            this.eventType = eventType;
            this.eventBehavior = eventBehavior;
            this.draggedNode = draggedNode;
            this.targetNode = (eventType == TreeViewNodeDragEventType.DragStart ? null : targetNode);
        }

        /// <summary>
        /// The type of this event
        /// </summary>
        private TreeViewNodeDragEventType eventType;

        /// <summary>
        /// The behavior of this event
        /// </summary>
        private TreeViewNodeDragEventBehavior eventBehavior;

        /// <summary>
        /// The node being dragged
        /// </summary>
        private TreeNode draggedNode;

        /// <summary>
        /// The node that the dragged node was dropped at.
        /// If the EventType is set to TreeViewNodeDragEventType.DragEnd, this
        /// value is null
        /// </summary>
        private TreeNode targetNode;

        /// <summary>
        /// Value that specifies whether the drag operation is to be canceled
        /// </summary>
        private bool cancel;

        /// <summary>
        /// Value that specifies whether the drag operation is currently set to be allowed
        /// </summary>
        private bool allow;

        /// <summary>
        /// Gets the type of this event
        /// </summary>
        public TreeViewNodeDragEventType EventType { get { return eventType; } }

        /// <summary>
        /// Gets or sets the behavior of this event.
        /// This value will only be used when the EventType is TreeViewNodeDragEventType.DragEnd
        /// </summary>
        public TreeViewNodeDragEventBehavior EventBehavior { get { return eventBehavior; } set { eventBehavior = value; } }

        /// <summary>
        /// Gets the node being dragged
        /// </summary>
        public TreeNode DraggedNode { get { return draggedNode; } }

        /// <summary>
        /// Gets the node that the dragged node was dropped at.
        /// If the EventType is set to TreeViewNodeDragEventType.DragEnd, this
        /// value is null
        /// </summary>
        public TreeNode TargetNode { get { return targetNode; } }

        /// <summary>
        /// Gets or sets a value that specifies whether the drag operation is to be canceled
        /// </summary>
        public bool Cancel { get { return cancel; } set { cancel = value; } }

        /// <summary>
        /// Gets or sets a value that specifies whether the drag operation is currently set to be allowed
        /// </summary>
        public bool Allow { get { return allow; } set { allow = value; } }
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
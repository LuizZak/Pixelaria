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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// A list view that displays an item dragging behaviour
    /// </summary>
    public class RearrangeableListView : ListView
    {
        /// <summary>
        /// The items currently being dragged
        /// </summary>
        private List<ListViewItem> draggedItems;

        /// <summary>
        /// The ListViewItem currently being hovered with the currently dragged item
        /// </summary>
        private ListViewItem tempDropItem;

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
        /// <param name="eventArgs">The ListViewItemDragEventArgs for the drag operation</param>
        public delegate void DragOperationHandler(ListViewItemDragEventArgs eventArgs);
        /// <summary>
        /// Event fired when a drag operation has started or ended
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a drag operation has started or ended")]
        public event DragOperationHandler DragOperation;

        /// <summary>
        /// Initializes a new instance of the RearrangeableListView class
        /// </summary>
        public RearrangeableListView()
        {
            imageListDrag = new ImageList();
            timer = new Timer();
            timer.Interval = 1;
            timer.Tick += new EventHandler(timer_Tick);

            this.AllowDrop = true;

            this.ShowGroups = false;
            this.InsertionMark.AppearsAfterItem = true;
        }

        // 
        // Timer tick. Updates scrolling of the ListView
        // 
        private void timer_Tick(object sender, EventArgs e)
        {
            this.InsertionMark.Index = -1;
            ((Timer)sender).Stop();
        }

        // 
        // Item Drag event handler
        // 
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);

            base.OnItemDrag(e);

            draggedItems = new List<ListViewItem>();

            foreach (ListViewItem item in SelectedItems)
            {
                draggedItems.Add(item);
            }

            ListViewItemDragEventArgs evArgs = new ListViewItemDragEventArgs(ListViewItemDragEventType.DragStart, ListViewItemDragEventBehavior.PlaceBeforeOrAfterAuto, draggedItems, null);

            if (DragOperation != null)
            {
                DragOperation(evArgs);

                // Cancel the operation if the user specified so
                if (evArgs.Cancel)
                {
                    return;
                }
            }

            this.DoDragDrop(draggedItems, DragDropEffects.Move);

            this.InsertionMark.Index = 0;
        }

        // 
        // Drag Enter event handler
        // 
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            DragHelper.ImageList_DragEnter(Handle, drgevent.X - Left, drgevent.Y - Top);

            // Enable timer for scrolling dragged item
            this.timer.Enabled = true;

            drgevent.Effect = DragDropEffects.Move;
        }

        // 
        // OnDragLeave event handler. Updates the dragged items' ghost images' position.
        // 
        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            // Disable timer for scrolling dragged item
            this.timer.Enabled = false;

            this.InsertionMark.Index = -1;

            timer.Start();
        }

        // 
        // OnDragOver event handler. Updates the dragged node's ghost image's position
        // 
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            // Cancel if no node is being dragged
            if (draggedItems == null)
                return;

            // Get actual drop item
            Point controlP = PointToClient(new Point(drgevent.X, drgevent.Y));
            int index = this.InsertionMark.NearestIndex(controlP);

            if (index == -1)
                return;

            ListViewItem dropItem = Items[index];//GetItemAt(controlP.X, controlP.Y);

            if (dropItem != tempDropItem)
            {
                ListViewItemDragEventArgs evArgs = new ListViewItemDragEventArgs(ListViewItemDragEventType.DragOver, ListViewItemDragEventBehavior.PlaceBeforeOrAfterAuto, draggedItems, dropItem);

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

                tempDropItem = dropItem;
            }

            this.InsertionMark.Index = index;
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
            if (draggedItems == null)
            {
                timer.Start();

                return;
            }

            if (drgevent.Effect == DragDropEffects.None)
            {
                // Set drag node and temp drop node to null
                draggedItems = null;
                tempDropItem = null;

                // Disable scroll timer
                this.timer.Enabled = false;

                timer.Start();

                return;
            }

            if (this.InsertionMark.Index == -1)
            {
                timer.Start();

                return;
            }

            // Get drop item
            Point controlP = PointToClient(new Point(drgevent.X, drgevent.Y));
            ListViewItem dropItem = Items[this.InsertionMark.Index];

            // Launch the feedback for the drag operation
            ListViewItemDragEventArgs evArgs = new ListViewItemDragEventArgs(ListViewItemDragEventType.DragEnd, ListViewItemDragEventBehavior.PlaceBeforeOrAfterAuto, draggedItems, dropItem);

            if (DragOperation != null)
            {
                DragOperation(evArgs);

                // Cancel the operation if the user specified so
                if (evArgs.Cancel)
                {
                    timer.Start();

                    return;
                }
            }

            // If drop node isn't equal to drag node, add drag node as child of drop node
            if (draggedItems[0] != dropItem)
            {
                int index = InsertionMark.Index;

                this.SelectedItems.Clear();

                foreach (ListViewItem item in draggedItems)
                {
                    this.Items.Remove(item);
                    this.Items.Add(item);

                    item.Selected = true;
                }

                // Deal with a bug from the framework that adds all items to the end even though you insert
                // them at other indexes by also pushing all items after the current selection to the end

                for(int i = index; i < Items.Count - this.SelectedItems.Count; i++)
                {
                    ListViewItem item = Items[index];

                    this.Items.Remove(item);
                    this.Items.Add(item);
                }

                // Launch the feedback for the drag operation
                evArgs = new ListViewItemDragEventArgs(ListViewItemDragEventType.AfterDragEnd, evArgs.EventBehavior, draggedItems, dropItem);

                if (DragOperation != null)
                {
                    DragOperation(evArgs);
                }

                // Set drag node and temp drop node to null
                draggedItems = null;
                tempDropItem = null;

                // Disable scroll timer
                this.timer.Enabled = false;
            }

            timer.Start();
        }
    }

    /// <summary>
    /// Event arguments for a RearrangeableListView item drag
    /// </summary>
    public class ListViewItemDragEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the ListViewItemDragEventArgs class
        /// </summary>
        /// <param name="eventType">The type for this event</param>
        /// <param name="eventBehavior">The behavior of this event</param>
        /// <param name="draggedItems">The items being dragged</param>
        /// <param name="targetItem">The item that the dragged item was dropped at. If the value for the eventType is set to ListViewItemDragEventType.DragEnd, this field is automatically set to null.</param>
        public ListViewItemDragEventArgs(ListViewItemDragEventType eventType, ListViewItemDragEventBehavior eventBehavior, List<ListViewItem> draggedItems, ListViewItem targetItem)
        {
            // The Cancel and Allow flags are set by the user and start with their default values
            this.cancel = false;
            this.allow = true;

            this.eventType = eventType;
            this.eventBehavior = eventBehavior;
            this.draggedItems = draggedItems;
            this.targetItem = (eventType == ListViewItemDragEventType.DragStart ? null : targetItem);
        }

        /// <summary>
        /// The type of this event
        /// </summary>
        private ListViewItemDragEventType eventType;

        /// <summary>
        /// The behavior of this event
        /// </summary>
        private ListViewItemDragEventBehavior eventBehavior;

        /// <summary>
        /// The items being dragged
        /// </summary>
        private List<ListViewItem> draggedItems;

        /// <summary>
        /// The item that the dragged item was dropped at.
        /// If the EventType is set to ListViewItemDragEventType.DragEnd, this
        /// value is null
        /// </summary>
        private ListViewItem targetItem;

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
        public ListViewItemDragEventType EventType { get { return eventType; } }

        /// <summary>
        /// Gets or sets the behavior of this event.
        /// This value will only be used when the EventType is TreeViewNodeDragEventType.DragEnd
        /// </summary>
        public ListViewItemDragEventBehavior EventBehavior { get { return eventBehavior; } set { eventBehavior = value; } }

        /// <summary>
        /// Gets the items being dragged
        /// </summary>
        public List<ListViewItem> DraggedItems { get { return draggedItems; } }

        /// <summary>
        /// Gets the item that the dragged node was dropped at.
        /// If the EventType is set to TreeViewNodeDragEventType.DragEnd, this
        /// value is null
        /// </summary>
        public ListViewItem TargetItem { get { return targetItem; } }

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
    /// Specifies the type of a ListViewItemDragEventArgs event
    /// </summary>
    public enum ListViewItemDragEventType
    {
        /// <summary>
        /// A Drag Start operation
        /// </summary>
        DragStart,
        /// <summary>
        /// A Drag Over operation, fired when the user hovers over another item with the current item being dragged
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
    public enum ListViewItemDragEventBehavior
    {
        /// <summary>
        /// Specifies that the dragged item should be placed before the target item
        /// </summary>
        PlaceBefore,
        /// <summary>
        /// Specifies that the dragged item should be placed after the target item
        /// </summary>
        PlaceAfter,
        /// <summary>
        /// Specifies that the dragged item should be placed before or after the target item depending on the mouse's position
        /// </summary>
        PlaceBeforeOrAfterAuto,
        /// <summary>
        /// Specifies that the dragged item and target item should switch places
        /// </summary>
        Switch
    }
}
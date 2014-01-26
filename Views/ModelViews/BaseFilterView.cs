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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Filters;

using Pixelaria.Views.Controls.Filters;

using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Implements an interface that the user can use to tweak settings of and apply a filter to an image
    /// </summary>
    public partial class BaseFilterView : Form
    {
        /// <summary>
        /// The Bitmap the filter will be applied on when the user clicks 'Ok'
        /// </summary>
        Bitmap bitmapOriginal;

        /// <summary>
        /// The bitmap that will be used as a preview for the filters
        /// </summary>
        Bitmap bitmapPreview;

        /// <summary>
        /// The FilterContainer objects that are currently applying filters to the bitmap
        /// </summary>
        List<FilterContainer> filterContainers;

        /// <summary>
        /// Event handler for the FilterUpdated event
        /// </summary>
        EventHandler filterUpdatedHandler;

        /// <summary>
        /// Event handler for a filter item click event
        /// </summary>
        EventHandler filterItemClick;

        /// <summary>
        /// Event handler for the ContainerDragStart event
        /// </summary>
        EventHandler containerDraggedHandler;

        /// <summary>
        /// Event handler for the ContainerDragEnd event
        /// </summary>
        EventHandler containerDroppedHandler;

        /// <summary>
        /// Event handler for the ContainerDragMove event
        /// </summary>
        EventHandler containerDraggingHandler;

        /// <summary>
        /// Form used to display the current FilterContainer being dragged
        /// </summary>
        ContainerDragForm dragForm;

        /// <summary>
        /// Panel used to temporarely replace the current FilterContainer being dragged
        /// </summary>
        Panel containerReplacePanel;

        /// <summary>
        /// Gets the number of filters being applied to the image right now
        /// </summary>
        public int FilterCount { get { return filterContainers.Count; } }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="filters">The array of FilterControls to use as interface to mediate the interaction between the filters to be applied and the user</param>
        /// <param name="bitmap">A bitmap to apply the filter to</param>
        public BaseFilterView(FilterControl[] filters, Bitmap bitmap)
        {
            InitializeComponent();

            this.filterContainers = new List<FilterContainer>();

            this.bitmapOriginal = bitmap;
            this.bitmapPreview = bitmap.Clone() as Bitmap;

            this.filterUpdatedHandler = new EventHandler(FilterUpdated);
            this.filterItemClick = new EventHandler(tsm_filterItem_Click);
            this.containerDraggedHandler = new EventHandler(ContainerDragged);
            this.containerDroppedHandler = new EventHandler(ContainerDropped);
            this.containerDraggingHandler = new EventHandler(ContainerDragging);

            LoadFilters(filters);

            this.pnl_errorPanel.Visible = false;

            this.btn_ok.Enabled = true;

            this.zpb_original.Image = bitmap;
            this.zpb_preview.Image = this.bitmapPreview;

            if (bitmapOriginal.Width >= this.zpb_preview.Width || bitmapOriginal.Height >= this.zpb_preview.Height)
            {
                this.zpb_original.ImageLayout = ImageLayout.None;
                this.zpb_preview.ImageLayout = ImageLayout.None;
            }

            this.zpb_original.HookToForm(this);
            this.zpb_preview.HookToForm(this);

            this.ignoreZoomEvents = false;

            UpdateVisualization();

            UpdateFilterList();
        }

        /// <summary>
        /// Applies the filter to the image
        /// </summary>
        public void ApplyFilter()
        {
            foreach (FilterContainer container in filterContainers)
            {
                container.FilterControl.ApplyFilter(bitmapOriginal);
            }
        }

        /// <summary>
        /// Loads the given FilterControl array on this BaseFilterView
        /// </summary>
        /// <param name="filters"></param>
        public void LoadFilters(FilterControl[] filters)
        {
            foreach (FilterControl filter in filters)
            {
                LoadFilterControl(filter, false);
            }

            UpdateVisualization();
        }

        /// <summary>
        /// Loads the given FilterControl on this BaseFilterView
        /// </summary>
        /// <param name="filterControl">The filter control to load on this BaseFilterView</param>
        /// <param name="updateVisualization">Whether to update the 5+</param>
        public void LoadFilterControl(FilterControl filterControl, bool updateVisualization = true)
        {
            filterControl.Initialize(this.bitmapOriginal);

            FilterContainer filterContainer = new FilterContainer(this, filterControl);

            this.filterContainers.Add(filterContainer);

            filterControl.FilterUpdated += filterUpdatedHandler;
            filterContainer.ContainerDragStart += containerDraggedHandler;
            filterContainer.ContainerDragEnd += containerDroppedHandler;

            UpdateLayout();

            if (updateVisualization)
            {
                UpdateVisualization();
            }
        }

        /// <summary>
        /// Removes the given FilterContainer from this BaseFilterView
        /// </summary>
        /// <param name="filterContainer">The FilterContainer to remove from this BaseFilterView</param>
        public void RemoveFilterControl(FilterContainer filterContainer)
        {
            filterContainer.ContainerDragStart -= containerDraggedHandler;
            filterContainer.ContainerDragEnd -= containerDroppedHandler;

            filterContainers.Remove(filterContainer);

            filterContainer.DisposeThis();

            pnl_container.Controls.Remove(filterContainer);

            UpdateLayout();

            UpdateVisualization();
        }

        /// <summary>
        /// Updates the filter controls
        /// </summary>
        private void UpdateLayout()
        {
            int controlsHeight = 350;

            foreach (FilterContainer filterContainer in filterContainers)
            {
                pnl_container.Controls.Add(filterContainer);

                filterContainer.Width = this.pnl_container.Width - 23;
                filterContainer.Anchor = AnchorStyles.Right | AnchorStyles.Left;

                //controlsHeight += filterContainer.Height + 25;
            }

            this.gb_filterControlContainer.Height = controlsHeight;

            this.pnl_bottom.Location = new Point(this.pnl_bottom.Location.X, this.gb_filterControlContainer.Bounds.Bottom + 3);

            this.ClientSize = new Size(this.ClientSize.Width, this.pnl_bottom.Bounds.Bottom + 13);
        }

        /// <summary>
        /// Updates the preview visualization for this BaseFilterView
        /// </summary>
        private void UpdateVisualization()
        {
            FastBitmap.CopyPixels(bitmapOriginal, bitmapPreview);

            foreach (FilterContainer container in filterContainers)
            {
                container.FilterControl.ApplyFilter(bitmapPreview);
            }

            this.zpb_preview.Invalidate();
        }

        /// <summary>
        /// Updates the list of available filters
        /// </summary>
        private void UpdateFilterList()
        {
            cms_filters.Items.Clear();

            // Fetch the list of filters
            string[] filterNames = FilterStore.Instance.FiltersList;
            Image[] iconList = FilterStore.Instance.FilterIconList;

            //foreach (string filter in filterNames)
            for(int i = 0; i < iconList.Length; i++)
            {
                ToolStripMenuItem tsm_filterItem = new ToolStripMenuItem(filterNames[i], iconList[i]);

                tsm_filterItem.Tag = filterNames[i];
                tsm_filterItem.Click += filterItemClick;

                cms_filters.Items.Add(tsm_filterItem);
            }
        }

        // 
        // FilterControl Filter Updated event handler
        // 
        private void FilterUpdated(object sender, EventArgs e)
        {
            UpdateVisualization();
        }

        // 
        // Container Dragged event handler
        // 
        private void ContainerDragged(object sender, EventArgs e)
        {
            FilterContainer fc = (FilterContainer)sender;

            int scroll = pnl_container.VerticalScroll.Value;

            containerReplacePanel = new Panel();
            containerReplacePanel.BorderStyle = BorderStyle.FixedSingle;
            containerReplacePanel.Size = fc.Size;
            containerReplacePanel.PerformLayout();

            pnl_container.SuspendLayout();
            pnl_container.Controls.Add(containerReplacePanel);
            pnl_container.Controls.SetChildIndex(containerReplacePanel, pnl_container.Controls.GetChildIndex(fc));
            pnl_container.Controls.Remove(fc);
            pnl_container.ResumeLayout();

            pnl_container.VerticalScroll.Value = scroll;
            pnl_container.PerformLayout();

            dragForm = new ContainerDragForm(fc);
            dragForm.ContainerDragging += containerDraggingHandler;
            dragForm.Show();
        }

        // 
        // Container Dragging event handler
        // 
        private void ContainerDragging(object sender, EventArgs e)
        {
            Point toCont = pnl_container.PointToClient(MousePosition);

            if (toCont.Y < 30)
            {
                if (pnl_container.VerticalScroll.Value > 0)
                {
                    pnl_container.VerticalScroll.Value = Math.Max(0, pnl_container.VerticalScroll.Value - 15);
                }
            }
            else if (toCont.Y > pnl_container.Height - 30)
            {
                if (pnl_container.VerticalScroll.Value < pnl_container.VerticalScroll.Maximum)
                {
                    pnl_container.VerticalScroll.Value = Math.Min(pnl_container.VerticalScroll.Maximum, pnl_container.VerticalScroll.Value + 15);
                }
            }

            // Replace the container
            Control control = pnl_container.GetChildAtPoint(toCont);

            if (control is FilterContainer)
            {
                int pnlIndex = pnl_container.Controls.GetChildIndex(containerReplacePanel);

                pnl_container.Controls.SetChildIndex(containerReplacePanel, pnl_container.Controls.GetChildIndex(control));
                pnl_container.Controls.SetChildIndex(control, pnlIndex);
            }
        }

        // 
        // Container Dropped event handler
        // 
        private void ContainerDropped(object sender, EventArgs e)
        {
            FilterContainer fc = (FilterContainer)sender;

            dragForm.ContainerDragging -= containerDraggingHandler;
            dragForm.End();
            dragForm.Dispose();

            int index = pnl_container.Controls.GetChildIndex(containerReplacePanel);

            pnl_container.Controls.Add(fc);
            pnl_container.Controls.SetChildIndex(fc, index);
            pnl_container.Controls.Remove(containerReplacePanel);

            // Re-sort the filter's index
            filterContainers.Remove(fc);
            filterContainers.Insert(index, fc);

            UpdateVisualization();

            this.Focus();
            this.BringToFront();
        }

        // 
        // Form Closed event handler
        // 
        protected override void OnClosed(EventArgs e)
        {
            if (this.zpb_preview.Image != null)
            {
                this.zpb_preview.Image.Dispose();
            }

            foreach (FilterContainer container in filterContainers)
            {
                container.FilterControl.Dispose();
            }

            bitmapPreview.Dispose();
            bitmapOriginal = null;

            base.OnClosed(e);
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            ApplyFilter();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // 
        // Add Filter button click
        // 
        private void btn_addFilter_Click(object sender, EventArgs e)
        {
            cms_filters.Show(MousePosition);
        }

        // 
        // Filter menu item click
        // 
        private void tsm_filterItem_Click(object sender, EventArgs e)
        {
            // Creates the filter and adds it to the list of filters
            LoadFilterControl(FilterStore.Instance.CreateFilterControl(((ToolStripMenuItem)sender).Tag as string));
        }

        // 
        // Original ZPB zoom changed
        // 
        private void zpb_original_ZoomChanged(object sender, Controls.ZoomChangedEventArgs e)
        {
            if (ignoreZoomEvents)
                return;

            ignoreZoomEvents = true;
            zpb_preview.Zoom = new PointF(e.NewZoom, e.NewZoom);
            ignoreZoomEvents = false;
        }

        // 
        // Preview ZPB zoom changed
        // 
        private void zpb_preview_ZoomChanged(object sender, Controls.ZoomChangedEventArgs e)
        {
            if (ignoreZoomEvents)
                return;

            ignoreZoomEvents = true;
            zpb_original.Zoom = new PointF(e.NewZoom, e.NewZoom);
            ignoreZoomEvents = false;
        }

        /// <summary>
        /// Settings this flag to true ignores any zoom event fired by the zoomable picture boxes on the form
        /// </summary>
        bool ignoreZoomEvents;

        /// <summary>
        /// Form used to illustrate the drag operation
        /// </summary>
        private class ContainerDragForm : Form
        {
            /// <summary>
            /// The FilterContainer being displayed on this ContainerDragForm instance
            /// </summary>
            FilterContainer container;

            /// <summary>
            /// Timer used to drag this form
            /// </summary>
            Timer dragTimer;

            /// <summary>
            /// The size the container had when it was fed to this ContainerDragForm object
            /// </summary>
            Size containerStartSize;

            /// <summary>
            /// Occurs during the dragging operation whenever the container has been moved
            /// </summary>
            public event EventHandler ContainerDragging;

            /// <summary>
            /// Initializes a new instance of the ContainerDragForm class
            /// </summary>
            /// <param name="container">The container to display on this ContainerDragForm</param>
            public ContainerDragForm(FilterContainer container)
            {
                this.SuspendLayout();

                this.container = container;
                this.containerStartSize = container.Size;
                this.ShowInTaskbar = false;

                this.Size = new Size(this.container.Width + 1, this.container.Height + 1);
                this.FormBorderStyle = FormBorderStyle.None;
                this.container.Dock = DockStyle.Fill;
                this.Controls.Add(container);

                this.ResumeLayout();

                this.dragTimer = new Timer();
                this.dragTimer.Interval = 10;
                this.dragTimer.Tick += new EventHandler(dragTimer_Tick);
                this.dragTimer.Start();
            }

            /// <summary>
            /// Ends the dragging operation currently being handled by this ContainerDragForm
            /// </summary>
            public void End()
            {
                this.Controls.Remove(container);
                this.container.Dock = DockStyle.None;
                this.container.Size = this.containerStartSize;

                this.dragTimer.Stop();
            }

            /// <summary>
            /// Updates the drag position of this ContainerDragForm
            /// </summary>
            private void UpdateDrag()
            {
                Point newPos = new Point(MousePosition.X - container.MouseDownPoint.X, MousePosition.Y - container.MouseDownPoint.Y);

                //if(this.Location.X != newPos.X || this.Location.Y != newPos.Y)
                {
                    this.Location = newPos;

                    if (ContainerDragging != null)
                    {
                        ContainerDragging.Invoke(this, new EventArgs());
                    }
                }
            }

            // 
            // OnShown event handler
            // 
            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);

                UpdateDrag();
            }

            // 
            // Drag Update timer tick
            // 
            private void dragTimer_Tick(object sender, EventArgs e)
            {
                UpdateDrag();
            }
        }
    }
}
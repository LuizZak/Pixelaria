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
    }
}
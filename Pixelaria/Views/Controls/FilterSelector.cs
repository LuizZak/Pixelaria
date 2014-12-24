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
using System.Windows.Forms;

using Pixelaria.Filters;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Control that presents an interface for the user to compose a filter list
    /// </summary>
    public partial class FilterSelector : UserControl
    {
        /// <summary>
        /// The Bitmap the filter will be applied on when the user clicks 'Ok'
        /// </summary>
        Bitmap _bitmapOriginal;

        /// <summary>
        /// The bitmap that will be used as a preview for the filters
        /// </summary>
        Bitmap _bitmapPreview;

        /// <summary>
        /// The FilterContainer objects that are currently applying filters to the bitmap
        /// </summary>
        List<FilterContainer> _filterContainers;

        /// <summary>
        /// Event handler for the FilterUpdated event
        /// </summary>
        EventHandler _filterUpdatedHandler;

        /// <summary>
        /// Event handler for a filter item click event
        /// </summary>
        EventHandler _filterItemClick;

        /// <summary>
        /// Event handler for the ContainerDragStart event
        /// </summary>
        EventHandler _containerDraggedHandler;

        /// <summary>
        /// Event handler for the ContainerDragEnd event
        /// </summary>
        EventHandler _containerDroppedHandler;

        /// <summary>
        /// Event handler for the ContainerDragMove event
        /// </summary>
        EventHandler _containerDraggingHandler;

        /// <summary>
        /// Form used to display the current FilterContainer being dragged
        /// </summary>
        ContainerDragForm _dragForm;

        /// <summary>
        /// Panel used to temporarely replace the current FilterContainer being dragged
        /// </summary>
        Panel _containerReplacePanel;

        /// <summary>
        /// Gets the list of FilterContainer objects that are currently applying filters to the bitmap
        /// </summary>
        public FilterContainer[] FilterContainers { get { return _filterContainers.ToArray(); } }

        /// <summary>
        /// Initializes a new instance of the FilterSelector class
        /// </summary>
        public FilterSelector()
        {
            InitializeComponent();

            Init();
        }

        /// <summary>
        /// Initializes this FilterSelector control
        /// </summary>
        public void Init()
        {
            btn_addFilter.Click += btn_addFilter_Click;
            cb_filterPresets.TextChanged += cb_filterPresets_TextChanged;
            btn_savePreset.Click += btn_savePreset_Click;
            btn_deletePreset.Click += btn_deletePreset_Click;
            btn_loadPreset.Click += btn_loadPreset_Click;
            zpb_preview.ZoomChanged += zpb_preview_ZoomChanged;
            zpb_original.ZoomChanged += zpb_original_ZoomChanged;

            _filterContainers = new List<FilterContainer>();

            _filterUpdatedHandler = FilterUpdated;
            _filterItemClick = tsm_filterItem_Click;
            _containerDraggedHandler = ContainerDragged;
            _containerDroppedHandler = ContainerDropped;
            _containerDraggingHandler = ContainerDragging;

            zpb_original.Image = _bitmapOriginal;
            zpb_preview.Image = _bitmapPreview;

            zpb_original.HookToControl(this);
            zpb_preview.HookToControl(this);

            _ignoreZoomEvents = false;

            UpdateFilterList();
            UpdateFilterPresetList();
            UpdateFilterPresetButtons();
        }

        /// <summary>
        /// Sets the image to apply the filters to
        /// </summary>
        /// <param name="bitmap">The new bitmap to apply the filters to</param>
        public void SetImage(Bitmap bitmap)
        {
            if(_bitmapPreview != null)
                _bitmapPreview.Dispose();

            _bitmapOriginal = bitmap;
            _bitmapPreview = bitmap.Clone() as Bitmap;

            _ignoreZoomEvents = true;

            zpb_original.SetImage(_bitmapOriginal);
            zpb_preview.SetImage(_bitmapPreview);

            _ignoreZoomEvents = false;

            if (_bitmapOriginal.Width >= zpb_preview.Width || _bitmapOriginal.Height >= zpb_preview.Height)
            {
                zpb_original.ImageLayout = ImageLayout.None;
                zpb_preview.ImageLayout = ImageLayout.None;
            }

            foreach (FilterContainer container in _filterContainers)
            {
                container.FilterControl.Initialize(_bitmapOriginal);
            }

            UpdateVisualization();
        }

        /// <summary>
        /// Applies the filter to the image
        /// </summary>
        public void ApplyFilter()
        {
            foreach (FilterContainer container in _filterContainers)
            {
                container.ApplyFilter(_bitmapOriginal);
            }
        }

        /// <summary>
        /// Returns whether the current filter configuration can make any significant changes to the bitmap loaded
        /// </summary>
        /// <returns>Whether the current filter configuration can make any significant changes to the bitmap loaded</returns>
        public bool ChangesDetected()
        {
            bool changes = false;

            foreach (FilterContainer container in _filterContainers)
            {
                if (container.FilterEnabled && container.FilterControl.Filter.Modifying)
                {
                    changes = true;
                }
            }

            return changes;
        }

        /// <summary>
        /// Loads the given FilterControl array on this BaseFilterView
        /// </summary>
        /// <param name="filters">The array of filter controls to load into this BaseFilterView</param>
        public void LoadFilters(FilterControl[] filters)
        {
            foreach (FilterControl filter in filters)
            {
                LoadFilterControl(filter, false);
            }

            UpdateVisualization();
        }

        /// <summary>
        /// Loads the given FilterPreset on this BaseFilterView
        /// </summary>
        /// <param name="preset">A filter preset that contains data about filters to load on this BaseFilterView</param>
        public void LoadFilterPreset(FilterPreset preset)
        {
            RemoveAllFilterControls(false);

            cb_filterPresets.Text = preset.Name;

            LoadFilters(preset.MakeFilterControls());
        }

        /// <summary>
        /// Loads the given FilterControl on this BaseFilterView
        /// </summary>
        /// <param name="filterControl">The filter control to load on this BaseFilterView</param>
        /// <param name="updateVisualization">Whether to update the filter visualization at the end of the method</param>
        public void LoadFilterControl(FilterControl filterControl, bool updateVisualization = true)
        {
            filterControl.Initialize(_bitmapOriginal);

            FilterContainer filterContainer = new FilterContainer(this, filterControl);

            _filterContainers.Add(filterContainer);

            filterControl.FilterUpdated += _filterUpdatedHandler;
            filterContainer.ContainerDragStart += _containerDraggedHandler;
            filterContainer.ContainerDragEnd += _containerDroppedHandler;

            UpdateLayout();
            UpdateFilterPresetButtons();

            pnl_container.VerticalScroll.Value = pnl_container.VerticalScroll.Maximum;
            pnl_container.PerformLayout();

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
            RemoveFilterControl(filterContainer, true);
        }

        /// <summary>
        /// Removes the given FilterContainer from this BaseFilterView
        /// </summary>
        /// <param name="filterContainer">The FilterContainer to remove from this BaseFilterView</param>
        /// <param name="updateAfterRemoval">Whether to update the layout after this method call</param>
        private void RemoveFilterControl(FilterContainer filterContainer, bool updateAfterRemoval)
        {
            filterContainer.ContainerDragStart -= _containerDraggedHandler;
            filterContainer.ContainerDragEnd -= _containerDroppedHandler;

            _filterContainers.Remove(filterContainer);

            filterContainer.DisposeThis();

            pnl_container.Controls.Remove(filterContainer);

            if (updateAfterRemoval)
            {
                UpdateLayout();
                UpdateVisualization();
                UpdateFilterPresetButtons();
            }
        }

        /// <summary>
        /// Removes all the filter controls from this BaseFilterView
        /// </summary>
        /// <param name="updateAfterRemoval">Whether to update the layout after this method call</param>
        public void RemoveAllFilterControls(bool updateAfterRemoval)
        {
            while (_filterContainers.Count > 0)
            {
                RemoveFilterControl(_filterContainers[0], false);
            }

            if (updateAfterRemoval)
            {
                UpdateLayout();
                UpdateVisualization();
                UpdateFilterPresetButtons();
            }
        }

        /// <summary>
        /// Loads the filter preset currently selected on the presets combo box
        /// </summary>
        private void LoadSelectedFilterPreset()
        {
            if (cb_filterPresets.Text.Trim() == "")
                return;

            FilterPreset preset = FilterStore.Instance.GetFilterPresetByName(cb_filterPresets.Text);

            if (preset == null)
                return;

            LoadFilterPreset(preset);
        }

        /// <summary>
        /// Save the filter preset currently selected on the presets combo box
        /// </summary>
        private void SaveSelectedFilterPreset()
        {
            if (cb_filterPresets.Text.Trim() == "")
                return;

            if (_filterContainers.Count == 0)
                return;

            FilterStore.Instance.RecordFilterPreset(cb_filterPresets.Text, GetFilterArray());

            UpdateFilterPresetList();
        }

        /// <summary>
        /// Deletes the filter preset currently selected on the presets combo box
        /// </summary>
        private void DeleteSelectedFilterPreset()
        {
            if (cb_filterPresets.Text.Trim() == "")
                return;

            FilterStore.Instance.RemoveFilterPresetByName(cb_filterPresets.Text);

            cb_filterPresets.Text = @"New Preset";

            UpdateFilterPresetList();
        }

        /// <summary>
        /// Updates the filter controls
        /// </summary>
        private void UpdateLayout()
        {
            foreach (FilterContainer filterContainer in _filterContainers)
            {
                if (!pnl_container.Controls.Contains(filterContainer))
                    pnl_container.Controls.Add(filterContainer);

                filterContainer.Width = pnl_container.Width - 23;
                filterContainer.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            }
        }

        /// <summary>
        /// Updates the preview visualization for this BaseFilterView
        /// </summary>
        private void UpdateVisualization()
        {
            if (_bitmapOriginal == null)
                return;

            FastBitmap.CopyPixels(_bitmapOriginal, _bitmapPreview);

            foreach (FilterContainer container in _filterContainers)
            {
                container.ApplyFilter(_bitmapPreview);
            }

            zpb_preview.Invalidate();
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
            for (int i = 0; i < iconList.Length; i++)
            {
                ToolStripMenuItem filterItem = new ToolStripMenuItem(filterNames[i], iconList[i])
                {
                    Tag = filterNames[i]
                };

                filterItem.Click += _filterItemClick;

                cms_filters.Items.Add(filterItem);
            }
        }

        /// <summary>
        /// Updates the list of available filter presets
        /// </summary>
        private void UpdateFilterPresetList()
        {
            cb_filterPresets.Items.Clear();

            foreach (FilterPreset preset in FilterStore.Instance.FilterPrests)
            {
                cb_filterPresets.Items.Add(preset.Name);
            }
        }

        /// <summary>
        /// Updates the state of the filter presets buttons
        /// </summary>
        private void UpdateFilterPresetButtons()
        {
            if (cb_filterPresets.Text.Trim() == "")
            {
                btn_loadPreset.Enabled = btn_savePreset.Enabled = btn_deletePreset.Enabled = false;
                return;
            }

            if (_filterContainers.Count == 0)
            {
                btn_loadPreset.Enabled = true;
                btn_savePreset.Enabled = false;
                btn_deletePreset.Enabled = true;
                return;
            }

            btn_loadPreset.Enabled = btn_savePreset.Enabled = btn_deletePreset.Enabled = true;
        }

        /// <summary>
        /// Returns an array of IFilter objects that represent the filters currently loaded on this BaseFilterView
        /// </summary>
        /// <returns>An array of IFilter objects that represent the filters currently loaded on this BaseFilterView</returns>
        private IFilter[] GetFilterArray()
        {
            IFilter[] filters = new IFilter[_filterContainers.Count];

            for (int i = 0; i < filters.Length; i++)
            {
                filters[i] = _filterContainers[i].FilterControl.Filter;
            }

            return filters;
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

            _containerReplacePanel = new Panel { BorderStyle = BorderStyle.FixedSingle, Size = fc.Size };
            _containerReplacePanel.PerformLayout();

            pnl_container.SuspendLayout();
            pnl_container.Controls.Add(_containerReplacePanel);
            pnl_container.Controls.SetChildIndex(_containerReplacePanel, pnl_container.Controls.GetChildIndex(fc));
            pnl_container.Controls.Remove(fc);
            pnl_container.ResumeLayout();

            pnl_container.VerticalScroll.Value = scroll;
            pnl_container.PerformLayout();

            _dragForm = new ContainerDragForm(fc);
            _dragForm.ContainerDragging += _containerDraggingHandler;
            _dragForm.Show();
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
                int pnlIndex = pnl_container.Controls.GetChildIndex(_containerReplacePanel);

                pnl_container.Controls.SetChildIndex(_containerReplacePanel, pnl_container.Controls.GetChildIndex(control));
                pnl_container.Controls.SetChildIndex(control, pnlIndex);
            }
        }

        // 
        // Container Dropped event handler
        // 
        private void ContainerDropped(object sender, EventArgs e)
        {
            FilterContainer fc = (FilterContainer)sender;

            // Close the dragging form
            _dragForm.ContainerDragging -= _containerDraggingHandler;
            _dragForm.End();
            _dragForm.Dispose();

            // Re-add the filter container to the panel
            int index = pnl_container.Controls.GetChildIndex(_containerReplacePanel);

            int scroll = pnl_container.VerticalScroll.Value;

            pnl_container.SuspendLayout();
            pnl_container.Controls.Add(fc);
            pnl_container.Controls.SetChildIndex(fc, index);
            pnl_container.Controls.Remove(_containerReplacePanel);

            pnl_container.VerticalScroll.Value = scroll;
            pnl_container.ResumeLayout(true);

            // Re-sort the filter's index
            _filterContainers.Remove(fc);
            _filterContainers.Insert(index, fc);

            UpdateVisualization();

            Focus();
            BringToFront();
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
        private void zpb_original_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            if (_ignoreZoomEvents)
                return;

            _ignoreZoomEvents = true;
            zpb_preview.Zoom = new PointF(e.NewZoom, e.NewZoom);
            _ignoreZoomEvents = false;
        }

        // 
        // Preview ZPB zoom changed
        // 
        private void zpb_preview_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            if (_ignoreZoomEvents)
                return;

            _ignoreZoomEvents = true;
            zpb_original.Zoom = new PointF(e.NewZoom, e.NewZoom);
            _ignoreZoomEvents = false;
        }

        // 
        // Load Preset button click
        // 
        private void btn_loadPreset_Click(object sender, EventArgs e)
        {
            LoadSelectedFilterPreset();
        }
        // 
        // Save Preset button click
        // 
        private void btn_savePreset_Click(object sender, EventArgs e)
        {
            SaveSelectedFilterPreset();
        }
        // 
        // Delete Preset button click
        // 
        private void btn_deletePreset_Click(object sender, EventArgs e)
        {
            DeleteSelectedFilterPreset();
        }

        // 
        // Combobox text changed
        // 
        private void cb_filterPresets_TextChanged(object sender, EventArgs e)
        {
            UpdateFilterPresetButtons();
        }

        /// <summary>
        /// Settings this flag to true ignores any zoom event fired by the zoomable picture boxes on the form
        /// </summary>
        bool _ignoreZoomEvents;

        /// <summary>
        /// Form used to illustrate the drag operation
        /// </summary>
        private class ContainerDragForm : Form
        {
            /// <summary>
            /// The FilterContainer being displayed on this ContainerDragForm instance
            /// </summary>
            readonly FilterContainer _container;

            /// <summary>
            /// Timer used to drag this form
            /// </summary>
            readonly Timer _dragTimer;

            /// <summary>
            /// The size the container had when it was fed to this ContainerDragForm object
            /// </summary>
            Size _containerStartSize;

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
                _container = container;
                _dragTimer = new Timer();
            }

            /// <summary>
            /// Ends the dragging operation currently being handled by this ContainerDragForm
            /// </summary>
            public void End()
            {
                Controls.Remove(_container);
                _container.Dock = DockStyle.None;
                _container.Size = _containerStartSize;

                _dragTimer.Stop();
            }

            /// <summary>
            /// Updates the drag position of this ContainerDragForm
            /// </summary>
            private void UpdateDrag()
            {
                Point newPos = new Point(MousePosition.X - _container.MouseDownPoint.X, MousePosition.Y - _container.MouseDownPoint.Y);

                //if(this.Location.X != newPos.X || this.Location.Y != newPos.Y)
                {
                    Location = newPos;

                    if (ContainerDragging != null)
                    {
                        ContainerDragging.Invoke(this, new EventArgs());
                    }
                }
            }

            // 
            // OnLoad event handler
            // 
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                SuspendLayout();

                _containerStartSize = _container.Size;
                ShowInTaskbar = false;

                AutoScaleMode = AutoScaleMode.None;
                MinimumSize = new Size(0, 0);
                ClientSize = new Size(_container.Width + 1, _container.ClientSize.Height + 1);
                FormBorderStyle = FormBorderStyle.None;
                _container.Dock = DockStyle.Fill;
                Controls.Add(_container);

                ResumeLayout();

                _dragTimer.Interval = 10;
                _dragTimer.Tick += dragTimer_Tick;
                _dragTimer.Start();
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
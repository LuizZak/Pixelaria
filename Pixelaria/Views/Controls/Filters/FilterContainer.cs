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
using System.Drawing;
using System.Windows.Forms;

using Pixelaria.Filters;
using Pixelaria.Utils;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// A control used to store a FilterControl inside
    /// </summary>
    public partial class FilterContainer : UserControl
    {
        /// <summary>
        /// The FilterSelector that owns this FilterContainer
        /// </summary>
        readonly FilterSelector _owningSelector;

        /// <summary>
        /// The FilterControl currently held by this FilterContainer
        /// </summary>
        FilterControl _filterControl;

        /// <summary>
        /// Whether the filter currently contained on this FilterContainer is enabled
        /// </summary>
        bool _filterEnabled;

        /// <summary>
        /// Whether the mouse is currently held down on this control's drag area
        /// </summary>
        bool _mouseDown;

        /// <summary>
        /// Whether the user is currently dragging this FilterContainer
        /// </summary>
        bool _dragging;

        /// <summary>
        /// Where the mouse was held down on this control's drag area
        /// </summary>
        Point _mouseDownPoint;

        /// <summary>
        /// This filter container's state
        /// </summary>
        FilterContainerState _containerState;

        /// <summary>
        /// Gets the FilterControl currently held by this FilterContainer
        /// </summary>
        public FilterControl FilterControl { get { return _filterControl; } }

        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                // Adjust the buttons' colors
                AhslColor lightColor = value.ToAhsl();
                AhslColor darkColor = value.ToAhsl();

                lightColor = new AhslColor(lightColor.A, lightColor.H, lightColor.S, lightColor.L + 6);
                darkColor = new AhslColor(darkColor.A, darkColor.H, darkColor.S, darkColor.L - 19);

                btn_remove.FlatAppearance.MouseOverBackColor = btn_enable.FlatAppearance.MouseOverBackColor = lightColor.ToColor();
                btn_remove.FlatAppearance.MouseDownBackColor = btn_enable.FlatAppearance.MouseDownBackColor = darkColor.ToColor();

                base.BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the filter currently contained on this FilterContainer is enabled
        /// </summary>
        public bool FilterEnabled
        {
            get { return _filterEnabled; }
            set
            {
                _filterEnabled = value;

                if (_filterEnabled)
                {
                    btn_enable.Image = Properties.Resources.filter_enable_icon;
                    BackColor = Color.FromKnownColor(KnownColor.Control);
                }
                else
                {
                    btn_enable.Image = Properties.Resources.filter_disable_icon;

                    AhslColor newColor = Color.FromKnownColor(KnownColor.Control).ToAhsl();

                    newColor = new AhslColor(newColor.A, newColor.H, newColor.S, newColor.L - 10);

                    BackColor = newColor.ToColor();
                }
                
                _filterControl.FireFilterUpdated();
            }
        }

        /// <summary>
        /// Gets where the mouse was held down on this control's drag area
        /// </summary>
        public Point MouseDownPoint { get { return _mouseDownPoint; } }

        /// <summary>
        /// Gets this filter container's state
        /// </summary>
        public FilterContainerState ContainerState { get { return _containerState; } }

        /// <summary>
        /// Occurs whenever the user starts dragging the FilterControl
        /// </summary>
        public event EventHandler ContainerDragStart;

        /// <summary>
        /// Occurs whenever the user finishes dragging the FilterControl
        /// </summary>
        public event EventHandler ContainerDragEnd;

        /// <summary>
        /// Initializes a new instance of the FilterContainer class
        /// </summary>
        /// <param name="owningSelector">The view that will own this FilterContainer</param>
        /// <param name="filter">The filter to hold on this FilterContainer</param>
        public FilterContainer(FilterSelector owningSelector, FilterControl filter)
        {
            InitializeComponent();

            _containerState = FilterContainerState.Expanded;
            _mouseDown = false;
            _owningSelector = owningSelector;
            _filterEnabled = true;

            LoadFilter(filter);
        }

        /// <summary>
        /// Loads the given FilterControl on this FilterContainer
        /// </summary>
        /// <param name="filter">The FilterControl to hold on this FilterContainer</param>
        public void LoadFilter(FilterControl filter)
        {
            _filterControl = filter;

            lbl_filterName.Text = filter.FilterName;

            pnl_container.Controls.Add(filter);
            pnl_container.Height = filter.Height;

            filter.Width = pnl_container.Width;
            filter.Dock = DockStyle.Top;

            pb_filterIcon.Image = FilterStore.Instance.GetIconForFilter(filter.FilterName);

            ClientSize = new Size(Width, pnl_container.Bounds.Bottom);
        }

        /// <summary>
        /// Applies the filter settings to the given Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply the filter to</param>
        public void ApplyFilter(Bitmap bitmap)
        {
            if (_filterEnabled)
                _filterControl.ApplyFilter(bitmap);
        }

        /// <summary>
        /// Expands this filter's exhibition
        /// </summary>
        public void Expand()
        {
            _containerState = FilterContainerState.Expanded;

            btn_collapse.Image = Properties.Resources.minus_icon;

            ClientSize = new Size(ClientSize.Width, pnl_container.Bounds.Bottom);
        }

        /// <summary>
        /// Collapse this filter's exhibition
        /// </summary>
        public void Collapse()
        {
            _containerState = FilterContainerState.Collapsed;

            btn_collapse.Image = Properties.Resources.plus_icon;

            ClientSize = new Size(ClientSize.Width, 20);
        }

        /// <summary>
        /// Toggles this filter's exhibition
        /// </summary>
        public void Toggle()
        {
            if (_containerState == FilterContainerState.Expanded)
            {
                Collapse();
            }
            else
            {
                Expand();
            }
        }

        /// <summary>
        /// Disposes of this FilterContainer control
        /// </summary>
        public void DisposeThis()
        {
            _filterControl.Dispose();

            base.Dispose();
        }

        // 
        // OnMouseDown event handler
        // 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Location.X < 17 && e.Location.Y < 17)
            {
                _mouseDownPoint = e.Location;
                _mouseDown = true;
            }
        }

        // 
        // OnMouseMove event handler
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDown && _mouseDownPoint.Distance(e.Location) > 5 && !_dragging)
            {
                if (ContainerDragStart != null)
                {
                    ContainerDragStart.Invoke(this, new EventArgs());
                }

                _dragging = true;
            }
        }

        // 
        // OnMouseUp event handler
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_dragging)
            {
                if (ContainerDragEnd != null)
                {
                    ContainerDragEnd.Invoke(this, new EventArgs());
                }
            }

            _mouseDown = false;
            _dragging = false;
        }

        // 
        // OnPaint event handler
        // 
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the dragging region
            AhslColor lightColor = BackColor.ToAhsl();
            AhslColor darkColor = BackColor.ToAhsl();

            lightColor = new AhslColor(lightColor.A, lightColor.H, lightColor.S, lightColor.L + 6);
            darkColor = new AhslColor(darkColor.A, darkColor.H, darkColor.S, darkColor.L - 19);

            Pen lightPen = new Pen(lightColor.ToColor());
            Pen darkPen = new Pen(darkColor.ToColor());

            // Draw the light stripes
            for (int x = 3; x <= 15; x += 3)
            {
                e.Graphics.DrawLine(lightPen, x, 2, 2, x);
                e.Graphics.DrawLine(lightPen, 16, x + 1, x + 1, 16);
            }

            // Draw the dark stripes
            for (int x = 4; x <= 16; x += 3)
            {
                e.Graphics.DrawLine(darkPen, x, 2, 2, x);

                if (x <= 14)
                {
                    e.Graphics.DrawLine(darkPen, 16, x + 1, x + 1, 16);
                }
            }

            lightPen.Dispose();
            darkPen.Dispose();
        }

        // 
        // Remove Button click
        // 
        private void btn_remove_Click(object sender, EventArgs e)
        {
            if(_owningSelector != null)
                _owningSelector.RemoveFilterControl(this);
        }

        // 
        // Enable/Disable Button click
        // 
        private void btn_enable_Click(object sender, EventArgs e)
        {
            FilterEnabled = !FilterEnabled;
        }

        // 
        // Collapse/Expand Button click
        // 
        private void btn_collapse_Click(object sender, EventArgs e)
        {
            Toggle();
        }
    }

    /// <summary>
    /// Specifies one of the valid filter container states
    /// </summary>
    public enum FilterContainerState
    {
        /// <summary>
        /// Expanded state
        /// </summary>
        Expanded,
        /// <summary>
        /// Collapsed state
        /// </summary>
        Collapsed
    }
}
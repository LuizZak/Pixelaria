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

using Pixelaria.Utils;
using Pixelaria.Views.ModelViews;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// A control used to store a FilterControl inside
    /// </summary>
    public partial class FilterContainer : UserControl
    {
        /// <summary>
        /// The BaseFilterView that owns this FilterContainer
        /// </summary>
        BaseFilterView owningView;

        /// <summary>
        /// The FilterControl currently held by this FilterContainer
        /// </summary>
        FilterControl filterControl;

        /// <summary>
        /// Whether the mouse is currently held down on this control's drag area
        /// </summary>
        bool mouseDown;

        /// <summary>
        /// Whether the user is currently dragging this FilterContainer
        /// </summary>
        bool dragging;

        /// <summary>
        /// Where the mouse was held down on this control's drag area
        /// </summary>
        Point mouseDownPoint;

        /// <summary>
        /// Gets the FilterControl currently held by this FilterContainer
        /// </summary>
        public FilterControl FilterControl { get { return filterControl; } }

        /// <summary>
        /// Gets where the mouse was held down on this control's drag area
        /// </summary>
        public Point MouseDownPoint { get { return mouseDownPoint; } }

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
        /// <param name="owningView">The view that will own this FilterContainer</param>
        /// <param name="filter">The filter to hold on this FilterContainer</param>
        public FilterContainer(BaseFilterView owningView, FilterControl filter)
        {
            InitializeComponent();

            this.mouseDown = false;
            this.owningView = owningView;

            LoadFilter(filter);
        }

        /// <summary>
        /// Loads the given FilterControl on this FilterContainer
        /// </summary>
        /// <param name="filter">The FilterControl to hold on this FilterContainer</param>
        public void LoadFilter(FilterControl filter)
        {
            filterControl = filter;

            lbl_filterName.Text = filter.FilterName;

            pnl_container.Controls.Add(filter);
            pnl_container.Height = filter.Height;

            filter.Width = pnl_container.Width;
            filter.Dock = DockStyle.Top;

            this.ClientSize = new Size(this.Width, this.pnl_container.Bounds.Bottom);
        }

        /// <summary>
        /// Disposes of this FilterContainer control
        /// </summary>
        public void DisposeThis()
        {
            filterControl.Dispose();

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
                mouseDownPoint = e.Location;
                mouseDown = true;
            }
        }

        // 
        // OnMouseMove event handler
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown && mouseDownPoint.Distance(e.Location) > 5 && !dragging)
            {
                if (ContainerDragStart != null)
                {
                    ContainerDragStart.Invoke(this, new EventArgs());
                }

                dragging = true;
            }
        }

        // 
        // OnMouseUp event handler
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (dragging)
            {
                if (ContainerDragEnd != null)
                {
                    ContainerDragEnd.Invoke(this, new EventArgs());
                }
            }

            mouseDown = false;
            dragging = false;
        }

        // 
        // OnPaint event handler
        // 
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the dragging region
            Pen lightPen = Pens.White;
            Pen darkPen = new Pen(Color.FromArgb(255, 193, 193, 193));

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

            darkPen.Dispose();
        }

        // 
        // Remove Button click
        // 
        private void btn_remove_Click(object sender, EventArgs e)
        {
            owningView.RemoveFilterControl(this);
        }
    }
}
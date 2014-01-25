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
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        /// Gets the FilterControl currently held by this FilterContainer
        /// </summary>
        public FilterControl FilterControl { get { return filterControl; } }

        /// <summary>
        /// Initializes a new instance of the FilterContainer class
        /// </summary>
        /// <param name="owningView">The view that will own this FilterContainer</param>
        /// <param name="filter">The filter to hold on this FilterContainer</param>
        public FilterContainer(BaseFilterView owningView, FilterControl filter)
        {
            InitializeComponent();

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
        // Remove Button click
        // 
        private void btn_remove_Click(object sender, EventArgs e)
        {
            owningView.RemoveFilterControl(this);
        }
    }
}
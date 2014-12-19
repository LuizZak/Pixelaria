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

using Pixelaria.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a FilterControl that handles an OffsetFilter
    /// </summary>
    public partial class OffsetControl : FilterControl
    {
        /// <summary>
        /// Initializes a new instance of the OffsetControl class
        /// </summary>
        public OffsetControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this OffsetControl
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new OffsetFilter();
                ((OffsetFilter)filter).OffsetX = 0;
                ((OffsetFilter)filter).OffsetY = 0;
                ((OffsetFilter)filter).WrapHorizontal = false;
                ((OffsetFilter)filter).WrapVertical = false;
            }

            anud_offsetX.Minimum = -bitmap.Width;
            anud_offsetY.Minimum = -bitmap.Height;

            anud_offsetX.Maximum = bitmap.Width;
            anud_offsetY.Maximum = bitmap.Height;
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="referenceFilter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter referenceFilter)
        {
            if (!(referenceFilter is OffsetFilter))
                return;

            var offsetFilter = (referenceFilter as OffsetFilter);

            anud_offsetX.Value = (decimal)offsetFilter.OffsetX;
            anud_offsetY.Value = (decimal)offsetFilter.OffsetY;
            cb_wrapHorizontal.Checked = offsetFilter.WrapHorizontal;
            cb_wrapVertical.Checked = offsetFilter.WrapVertical;
        }

        // 
        // X offset nud
        // 
        private void anud_offsetX_ValueChanged(object sender, EventArgs e)
        {
            ((OffsetFilter)filter).OffsetX = (float)anud_offsetX.Value;

            FireFilterUpdated();
        }

        // 
        // Y offset nud
        // 
        private void anud_offsetY_ValueChanged(object sender, EventArgs e)
        {
            ((OffsetFilter)filter).OffsetY = (float)anud_offsetY.Value;

            FireFilterUpdated();
        }

        // 
        // Wrap Horizontal checkbox check
        // 
        private void cb_wrapHorizontal_CheckedChanged(object sender, EventArgs e)
        {
            ((OffsetFilter)filter).WrapHorizontal = cb_wrapHorizontal.Checked;

            FireFilterUpdated();
        }

        // 
        // Wrap Vertical checkbox checked
        // 
        private void cb_wrapVertical_CheckedChanged(object sender, EventArgs e)
        {
            ((OffsetFilter)filter).WrapVertical = cb_wrapVertical.Checked;

            FireFilterUpdated();
        }
    }
}
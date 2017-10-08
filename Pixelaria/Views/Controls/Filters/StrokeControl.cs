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

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a FilterControl that handles an OffsetFilter
    /// </summary>
    internal partial class StrokeControl : FilterControl
    {
        /// <summary>
        /// Initializes a new instance of the StrokeControl class
        /// </summary>
        public StrokeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this StrokeControl
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new StrokeFilter
                {
                    StrokeColor = Color.Red,
                    StrokeRadius = 1,
                    KnockoutImage = false,
                    Smooth = false
                };
            }
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="referenceFilter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter referenceFilter)
        {
            var strokeFilter = referenceFilter as StrokeFilter;

            if (strokeFilter == null)
                return;

            anud_strokeSize.Value = strokeFilter.StrokeRadius;
            cp_color.BackColor = strokeFilter.StrokeColor;
            cb_knockout.Checked = strokeFilter.KnockoutImage;
            cb_smooth.Checked = strokeFilter.Smooth;
        }

        // 
        // Color Panel click
        // 
        private void cp_color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog { AllowFullOpen = true };

            if (cd.ShowDialog(FindForm()) == DialogResult.OK)
            {
                cp_color.BackColor = cd.Color;

                ((StrokeFilter)filter).StrokeColor = cd.Color;

                FireFilterUpdated();
            }
        }

        // 
        // Stroke Size ANUD changed
        // 
        private void anud_strokeSize_ValueChanged(object sender, EventArgs e)
        {
            ((StrokeFilter)filter).StrokeRadius = (int)anud_strokeSize.Value;
            FireFilterUpdated();
        }

        // 
        // Knockout Image checkbox check
        // 
        private void cb_knockout_CheckedChanged(object sender, EventArgs e)
        {
            ((StrokeFilter)filter).KnockoutImage = cb_knockout.Checked;
            FireFilterUpdated();
        }

        // 
        // Smooth checkbox check
        // 
        private void cb_smooth_CheckedChanged(object sender, EventArgs e)
        {
            ((StrokeFilter)filter).Smooth = cb_smooth.Checked;
            FireFilterUpdated();
        }
    }
}
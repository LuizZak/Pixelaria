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
    /// Represents a <see cref="FilterControl{T}"/> that handles an <see cref="StrokeFilter"/>
    /// </summary>
    internal partial class StrokeControl : FilterControl<StrokeFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StrokeControl"/> class
        /// </summary>
        public StrokeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this <see cref="StrokeControl"/>
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
        /// Updates the fields from this <see cref="StrokeControl"/> based on the data from the
        /// given <see cref="StrokeFilter"/> instance
        /// </summary>
        /// <param name="referenceFilter">The <see cref="StrokeFilter"/> instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(StrokeFilter referenceFilter)
        {
            anud_strokeSize.Value = referenceFilter.StrokeRadius;
            cp_color.BackColor = referenceFilter.StrokeColor;
            cb_knockout.Checked = referenceFilter.KnockoutImage;
            cb_smooth.Checked = referenceFilter.Smooth;
        }

        // 
        // Color Panel click
        // 
        private void cp_color_Click(object sender, EventArgs e)
        {
            var cd = new ColorDialog { AllowFullOpen = true };

            if (cd.ShowDialog(FindForm()) != DialogResult.OK)
                return;

            cp_color.BackColor = cd.Color;

            filter.StrokeColor = cd.Color;

            FireFilterUpdated();
        }

        // 
        // Stroke Size ANUD changed
        // 
        private void anud_strokeSize_ValueChanged(object sender, EventArgs e)
        {
            filter.StrokeRadius = (int)anud_strokeSize.Value;
            FireFilterUpdated();
        }

        // 
        // Knockout Image checkbox check
        // 
        private void cb_knockout_CheckedChanged(object sender, EventArgs e)
        {
            filter.KnockoutImage = cb_knockout.Checked;
            FireFilterUpdated();
        }

        // 
        // Smooth checkbox check
        // 
        private void cb_smooth_CheckedChanged(object sender, EventArgs e)
        {
            filter.Smooth = cb_smooth.Checked;
            FireFilterUpdated();
        }
    }
}
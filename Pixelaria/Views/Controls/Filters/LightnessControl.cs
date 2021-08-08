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
using PixCore.Controls.ColorControls;
using PixLib.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a <see cref="FilterControl{T}"/> that handles a <see cref="LightnessFilter"/>
    /// </summary>
    internal partial class LightnessControl : FilterControl<LightnessFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightnessControl"/> class
        /// </summary>
        public LightnessControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this <see cref="LightnessControl"/>
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new LightnessFilter
                {
                    Lightness = 100,
                    Relative = false
                };
            }
        }

        /// <summary>
        /// Updates the fields from this <see cref="LightnessControl"/> based on the data from the
        /// given <see cref="LightnessFilter"/> instance
        /// </summary>
        /// <param name="referenceFilter">The <see cref="LightnessFilter"/> instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(LightnessFilter referenceFilter)
        {
            cs_lightness.CurrentValue = referenceFilter.Lightness / 100.0f;
            cb_relative.Checked = referenceFilter.Relative;
            cb_multiply.Checked = referenceFilter.Multiply;
        }

        // 
        // Lightness slider value changed
        // 
        private void cs_lightness_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            filter.Lightness = (int)(cs_lightness.CurrentValue * 100);

            FireFilterUpdated();
        }

        // 
        // Relative checkbox checked
        // 
        private void cb_relative_CheckedChanged(object sender, EventArgs e)
        {
            filter.Relative = cb_relative.Checked;

            FireFilterUpdated();
        }

        // 
        // Multiply checkbox checked
        // 
        private void cb_multiply_CheckedChanged(object sender, EventArgs e)
        {
            cb_relative.Enabled = !cb_multiply.Checked;

            filter.Multiply = cb_multiply.Checked;

            FireFilterUpdated();
        }
    }
}
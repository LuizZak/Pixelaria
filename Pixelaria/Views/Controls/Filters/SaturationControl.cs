﻿/*
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
    /// Represents a <see cref="FilterControl{T}"/> that handles a <see cref="SaturationFilter"/>
    /// </summary>
    internal partial class SaturationControl : FilterControl<SaturationFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaturationControl"/> class
        /// </summary>
        public SaturationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this <see cref="SaturationControl"/>
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new SaturationFilter {Saturation = 100, Relative = false, KeepGrays = true};
            }
        }

        /// <summary>
        /// Updates the fields from this <see cref="SaturationControl"/> based on the data from the
        /// given <see cref="SaturationFilter"/> instance
        /// </summary>
        /// <param name="referenceFilter">The <see cref="SaturationFilter"/> instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(SaturationFilter referenceFilter)
        {
            var saturationFilter = referenceFilter;

            cs_saturation.CurrentValue = saturationFilter.Saturation / 100.0f;
            cb_relative.Checked = saturationFilter.Relative;
            cb_keepGrays.Checked = saturationFilter.KeepGrays;
            cb_multiply.Checked = saturationFilter.Multiply;
        }

        // 
        // Saturation slider value changed
        // 
        private void cs_saturation_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            filter.Saturation = (int)(cs_saturation.CurrentValue * 100);

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
        // Keep Grays checkbox checked
        // 
        private void cb_keepGrays_CheckedChanged(object sender, EventArgs e)
        {
            filter.KeepGrays = cb_keepGrays.Checked;

            FireFilterUpdated();
        }

        // 
        // Multiply checkbox checked
        // 
        private void cb_multiply_CheckedChanged(object sender, EventArgs e)
        {
            cb_keepGrays.Enabled = cb_relative.Enabled = !cb_multiply.Checked;

            filter.Multiply = cb_multiply.Checked;

            FireFilterUpdated();
        }
    }
}
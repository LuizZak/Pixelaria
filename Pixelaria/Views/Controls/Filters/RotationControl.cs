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
    /// Represents a <see cref="FilterControl{T}"/> that handles a <see cref="RotationFilter"/>
    /// </summary>
    internal partial class RotationControl : FilterControl<RotationFilter>
    {
        /// <summary>
        /// Initializes a new class of the <see cref="RotationControl"/> class
        /// </summary>
        public RotationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this <see cref="RotationControl"/>
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new RotationFilter {Rotation = 0, RotateAroundCenter = false, PixelQuality = false};
            }
        }

        /// <summary>
        /// Updates the fields from this <see cref="RotationControl"/> based on the data from the
        /// given <see cref="RotationFilter"/> instance
        /// </summary>
        /// <param name="referenceFilter">The <see cref="RotationFilter"/> instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(RotationFilter referenceFilter)
        {
            anud_angle.Value = (decimal)referenceFilter.Rotation;

            cb_rotateAroundCenter.Checked = referenceFilter.RotateAroundCenter;
            cb_pixelQuality.Checked = referenceFilter.PixelQuality;
        }

        // 
        // Angle anud value changed
        // 
        private void anud_angle_ValueChanged(object sender, EventArgs e)
        {
            filter.Rotation = (float)anud_angle.Value;

            FireFilterUpdated();
        }

        // 
        // Rotate Around Center checkbox check
        // 
        private void cb_rotateAroundCenter_CheckedChanged(object sender, EventArgs e)
        {
            filter.RotateAroundCenter = cb_rotateAroundCenter.Checked;

            FireFilterUpdated();
        }

        // 
        // Pixel Quality checkbox check
        // 
        private void cb_pixelQuality_CheckedChanged(object sender, EventArgs e)
        {
            filter.PixelQuality = cb_pixelQuality.Checked;

            FireFilterUpdated();
        }
    }
}
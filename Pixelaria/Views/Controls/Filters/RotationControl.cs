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

using Pixelaria.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a FilterControl that handles a RotationFilter
    /// </summary>
    public partial class RotationControl : FilterControl
    {
        /// <summary>
        /// Initializes a new class of the RotationControl class
        /// </summary>
        public RotationControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes this RotationControl
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (this.filter == null)
            {
                this.filter = new RotationFilter();
                (filter as RotationFilter).Rotation = 0;
                (filter as RotationFilter).RotateAroundCenter = false;
                (filter as RotationFilter).PixelQuality = false;
            }
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="filter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter filter)
        {
            if (!(filter is RotationFilter))
                return;

            anud_angle.Value = (decimal)(filter as RotationFilter).Rotation;

            cb_rotateAroundCenter.Checked = (filter as RotationFilter).RotateAroundCenter;
            cb_pixelQuality.Checked = (filter as RotationFilter).PixelQuality;
        }

        // 
        // Angle anud value changed
        // 
        private void anud_angle_ValueChanged(object sender, EventArgs e)
        {
            (filter as RotationFilter).Rotation = (float)anud_angle.Value;

            FireFilterUpdated();
        }

        // 
        // Rotate Around Center checkbox check
        // 
        private void cb_rotateAroundCenter_CheckedChanged(object sender, EventArgs e)
        {
            (filter as RotationFilter).RotateAroundCenter = cb_rotateAroundCenter.Checked;

            FireFilterUpdated();
        }

        // 
        // Pixel Quality checkbox check
        // 
        private void cb_pixelQuality_CheckedChanged(object sender, EventArgs e)
        {
            (filter as RotationFilter).PixelQuality = cb_pixelQuality.Checked;

            FireFilterUpdated();
        }
    }
}
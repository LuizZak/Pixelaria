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
    /// Represents a <see cref="FilterControl{T}"/> that handles a <see cref="HueFilter"/>
    /// </summary>
    internal partial class HueControl : FilterControl<HueFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HueControl"/>
        /// </summary>
        public HueControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this <see cref="HueControl"/>
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new HueFilter {Hue = 0, Relative = false};
            }
        }

        /// <summary>
        /// Updates the fields from this <see cref="HueControl"/> based on the data from the
        /// given <see cref="HueFilter"/> instance
        /// </summary>
        /// <param name="referenceFilter">The <see cref="HueFilter"/> instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(HueFilter referenceFilter)
        {
            cs_hue.CurrentValue = referenceFilter.Hue / 360.0f;
            cb_relative.Checked = referenceFilter.Relative;
        }

        // 
        // Hue slider color changed
        // 
        private void cs_hue_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            filter.Hue = (int)(cs_hue.CurrentValue * 360);

            FireFilterUpdated();
        }

        // 
        // Relative checkbox check
        // 
        private void cb_relative_CheckedChanged(object sender, EventArgs e)
        {
            filter.Relative = cb_relative.Checked;

            FireFilterUpdated();
        }
    }
}
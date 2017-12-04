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
using PixCore.Colors;
using PixCore.Controls.ColorControls;

using Pixelaria.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a <see cref="FilterControl{T}"/> that handles a <see cref="FadeFilter"/>
    /// </summary>
    internal partial class FadeControl : FilterControl<FadeFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FadeControl"/> class
        /// </summary>
        public FadeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this <see cref="FadeControl"/>
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new FadeFilter {FadeAlpha = false, FadeColor = Color.White, FadeFactor = 0.5f};

                cs_factor.CurrentValue = 0.5f;
            }
        }

        /// <summary>
        /// Updates the fields from this <see cref="FadeControl"/> based on the data from the
        /// given <see cref="FadeFilter"/> instance
        /// </summary>
        /// <param name="referenceFilter">The <see cref="FadeFilter"/> instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(FadeFilter referenceFilter)
        {
            cp_color.BackColor = referenceFilter.FadeColor;
            cs_factor.CustomStartColor = cp_color.BackColor.ToAhsl().WithTransparency(0);
            cs_factor.CustomEndColor = cp_color.BackColor.ToAhsl().WithTransparency(1);

            cs_factor.CurrentValue = referenceFilter.FadeFactor;
        }

        // 
        // Color Panel click
        // 
        private void cp_color_Click(object sender, EventArgs e)
        {
            var cd = new ColorDialog
            {
                AllowFullOpen = true
            };

            if (cd.ShowDialog(FindForm()) == DialogResult.OK)
            {
                cp_color.BackColor = cd.Color;

                filter.FadeColor = cd.Color;

                cs_factor.CustomStartColor = cd.Color.ToAhsl().WithTransparency(0);
                cs_factor.CustomEndColor = cd.Color.ToAhsl().WithTransparency(1);

                FireFilterUpdated();
            }
        }

        // 
        // Factor slider changed
        // 
        private void cs_factor_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            filter.FadeFactor = cs_factor.CurrentValue;

            FireFilterUpdated();
        }
    }
}
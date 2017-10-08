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
    /// Represents a FilterControl that handles a FadeFilter
    /// </summary>
    internal partial class FadeControl : FilterControl
    {
        /// <summary>
        /// Initializes a new instance of the FadeControl class
        /// </summary>
        public FadeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this TransparencyControl
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new FadeFilter {FadeAlpha = false, FadeColor = Color.White, FadeFactor = 0.5f};
            }
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="referenceFilter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter referenceFilter)
        {
            if (!(referenceFilter is FadeFilter))
                return;

            cp_color.BackColor = ((FadeFilter)referenceFilter).FadeColor;
            anud_factor.Value = (decimal)((FadeFilter)referenceFilter).FadeFactor * 100;
        }

        // 
        // Color Panel click
        // 
        private void cp_color_Click(object sender, EventArgs e)
        {
            var cd = new ColorDialog {AllowFullOpen = true};

            if (cd.ShowDialog(FindForm()) == DialogResult.OK)
            {
                cp_color.BackColor = cd.Color;

                ((FadeFilter)filter).FadeColor = cd.Color;

                FireFilterUpdated();
            }
        }

        // 
        // Factor ANUD changed
        // 
        private void anud_factor_ValueChanged(object sender, EventArgs e)
        {
            ((FadeFilter)filter).FadeFactor = (float)(anud_factor.Value / 100);

            FireFilterUpdated();
        }
    }
}
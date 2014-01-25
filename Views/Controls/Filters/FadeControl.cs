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
    /// Represents a FilterControl that handles a FadeFilter
    /// </summary>
    public partial class FadeControl : FilterControl
    {
        /// <summary>
        /// Gets the name of this filter
        /// </summary>
        public override string FilterName { get { return "Fade Color"; } }

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

            this.filter = new FadeFilter();
            (filter as FadeFilter).FadeFactor = 0.5f;
            (filter as FadeFilter).FadeColor = Color.White;
            (filter as FadeFilter).FadeAlpha = false;

            this.updateRequired = true;
        }

        // 
        // Color Panel click
        // 
        private void cp_color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = true;

            if (cd.ShowDialog(this.FindForm()) == DialogResult.OK)
            {
                cp_color.BackColor = cd.Color;

                (filter as FadeFilter).FadeColor = cd.Color;

                this.updateRequired = true;

                this.FireFilterUpdated();
            }
        }

        // 
        // Factor ANUD changed
        // 
        private void anud_factor_ValueChanged(object sender, EventArgs e)
        {
            (filter as FadeFilter).FadeFactor = (float)(anud_factor.Value / 100);

            this.updateRequired = true;

            this.FireFilterUpdated();
        }
    }
}
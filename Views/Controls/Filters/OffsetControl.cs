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
    /// Represents a FilterControl that handles an OffsetFilter
    /// </summary>
    public partial class OffsetControl : FilterControl
    {
        /// <summary>
        /// Initializes a new instance of the OffsetControl class
        /// </summary>
        public OffsetControl()
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

            this.filter = new OffsetFilter();
            (filter as OffsetFilter).OffsetX = 1;
            (filter as OffsetFilter).OffsetY = 1;

            this.anud_offsetX.Minimum = -bitmap.Width;
            this.anud_offsetY.Minimum = -bitmap.Height;

            this.anud_offsetX.Maximum = bitmap.Width;
            this.anud_offsetY.Maximum = bitmap.Height;

            this.updateRequired = true;
        }

        // 
        // X offset nud
        // 
        private void anud_offsetX_ValueChanged(object sender, EventArgs e)
        {
            (filter as OffsetFilter).OffsetX = (float)anud_offsetX.Value;

            FireFilterUpdated();
        }

        // 
        // Y offset nud
        // 
        private void anud_offsetY_ValueChanged(object sender, EventArgs e)
        {
            (filter as OffsetFilter).OffsetY = (float)anud_offsetY.Value;

            FireFilterUpdated();
        }
    }
}
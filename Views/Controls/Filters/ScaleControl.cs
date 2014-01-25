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
    /// Represents a FilterControl that handles a ScaleFilter
    /// </summary>
    public partial class ScaleControl : FilterControl
    {
        /// <summary>
        /// Whether to ignore the next ANUD event
        /// </summary>
        private bool ignoreEvent;

        /// <summary>
        /// Gets the name of this filter
        /// </summary>
        public override string FilterName { get { return "Scale"; } }

        /// <summary>
        /// Initializes a new class of the ScaleControl class
        /// </summary>
        public ScaleControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes this TransparencyControl
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            this.filter = new ScaleFilter();
            (filter as ScaleFilter).ScaleX = 1;
            (filter as ScaleFilter).ScaleY = 1;

            this.updateRequired = true;

            this.ignoreEvent = false;
        }

        // 
        // Horizontal Scale nud changed
        // 
        private void anud_scaleX_ValueChanged(object sender, EventArgs e)
        {
            if (ignoreEvent)
                return;

            if (cb_keepAspect.Checked)
            {
                ignoreEvent = true;
                anud_scaleY.Value = anud_scaleX.Value;
                (filter as ScaleFilter).ScaleY = (float)anud_scaleY.Value;
                ignoreEvent = false;
            }

            updateRequired = true;

            (filter as ScaleFilter).ScaleX = (float)anud_scaleX.Value;

            FireFilterUpdated();
        }

        // 
        // Vertical Scale nud changed
        //
        private void anud_scaleY_ValueChanged(object sender, EventArgs e)
        {
            if (ignoreEvent)
                return;

            if (cb_keepAspect.Checked)
            {
                ignoreEvent = true;
                anud_scaleX.Value = anud_scaleY.Value;
                (filter as ScaleFilter).ScaleX = (float)anud_scaleX.Value;
                ignoreEvent = false;
            }
            
            updateRequired = true;

            (filter as ScaleFilter).ScaleY = (float)anud_scaleY.Value;

            FireFilterUpdated();
        }

        // 
        // Centered Checkbox checked
        // 
        private void cb_centered_CheckedChanged(object sender, EventArgs e)
        {
            updateRequired = true;

            (filter as ScaleFilter).Centered = cb_centered.Checked;

            FireFilterUpdated();
        }

        // 
        // Centered Checkbox checked
        // 
        private void cb_keepAspect_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_keepAspect.Checked)
            {
                ignoreEvent = true;
                anud_scaleY.Value = anud_scaleX.Value;
                ignoreEvent = false;

                updateRequired = true;

                (filter as ScaleFilter).ScaleX = (float)anud_scaleX.Value;
                (filter as ScaleFilter).ScaleY = (float)anud_scaleY.Value;

                FireFilterUpdated();
            }
        }

        // 
        // Pixel Quality checked
        // 
        private void cb_pixelQuality_CheckedChanged(object sender, EventArgs e)
        {
            updateRequired = true;

            (filter as ScaleFilter).PixelQuality = cb_pixelQuality.Checked;

            FireFilterUpdated();
        }
    }
}
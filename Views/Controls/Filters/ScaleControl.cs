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
    /// Represents a FilterControl that handles a ScaleFilter
    /// </summary>
    public partial class ScaleControl : FilterControl
    {
        /// <summary>
        /// Whether to ignore the next field updated event
        /// </summary>
        private bool ignoreEvent;

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

            if (this.filter == null)
            {
                this.filter = new ScaleFilter();
                (filter as ScaleFilter).ScaleX = 1;
                (filter as ScaleFilter).ScaleY = 1;
            }

            this.updateRequired = true;

            this.ignoreEvent = false;
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="filter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter filter)
        {
            if (!(filter is ScaleFilter))
                return;

            ignoreEvent = true;

            anud_scaleX.Value = (decimal)(filter as ScaleFilter).ScaleX;
            anud_scaleY.Value = (decimal)(filter as ScaleFilter).ScaleY;

            cb_centered.Checked = (filter as ScaleFilter).Centered;
            cb_pixelQuality.Checked = (filter as ScaleFilter).PixelQuality;

            ignoreEvent = false;
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
            if (ignoreEvent)
                return;

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
            if (ignoreEvent)
                return;

            updateRequired = true;

            (filter as ScaleFilter).PixelQuality = cb_pixelQuality.Checked;

            FireFilterUpdated();
        }
    }
}
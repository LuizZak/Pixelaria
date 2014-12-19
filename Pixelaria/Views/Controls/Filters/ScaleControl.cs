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
        private bool _ignoreEvent;

        /// <summary>
        /// Initializes a new class of the ScaleControl class
        /// </summary>
        public ScaleControl()
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
                filter = new ScaleFilter {ScaleX = 1, ScaleY = 1};
            }

            _ignoreEvent = false;
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="referenceFilter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter referenceFilter)
        {
            if (!(referenceFilter is ScaleFilter))
                return;

            var castFilter = (ScaleFilter)referenceFilter;

            _ignoreEvent = true;

            anud_scaleX.Value = (decimal)castFilter.ScaleX;
            anud_scaleY.Value = (decimal)castFilter.ScaleY;

            cb_centered.Checked = castFilter.Centered;
            cb_pixelQuality.Checked = castFilter.PixelQuality;

            _ignoreEvent = false;
        }

        // 
        // Horizontal Scale nud changed
        // 
        private void anud_scaleX_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreEvent)
                return;

            if (cb_keepAspect.Checked)
            {
                _ignoreEvent = true;
                anud_scaleY.Value = anud_scaleX.Value;
                ((ScaleFilter)filter).ScaleY = (float)anud_scaleY.Value;
                _ignoreEvent = false;
            }

            ((ScaleFilter)filter).ScaleX = (float)anud_scaleX.Value;

            FireFilterUpdated();
        }

        // 
        // Vertical Scale nud changed
        //
        private void anud_scaleY_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreEvent)
                return;

            if (cb_keepAspect.Checked)
            {
                _ignoreEvent = true;
                anud_scaleX.Value = anud_scaleY.Value;
                ((ScaleFilter)filter).ScaleX = (float)anud_scaleX.Value;
                _ignoreEvent = false;
            }

            ((ScaleFilter)filter).ScaleY = (float)anud_scaleY.Value;

            FireFilterUpdated();
        }

        // 
        // Centered Checkbox checked
        // 
        private void cb_centered_CheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreEvent)
                return;

            ((ScaleFilter)filter).Centered = cb_centered.Checked;

            FireFilterUpdated();
        }

        // 
        // Centered Checkbox checked
        // 
        private void cb_keepAspect_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_keepAspect.Checked)
            {
                _ignoreEvent = true;
                anud_scaleY.Value = anud_scaleX.Value;
                _ignoreEvent = false;

                ((ScaleFilter)filter).ScaleX = (float)anud_scaleX.Value;
                ((ScaleFilter)filter).ScaleY = (float)anud_scaleY.Value;

                FireFilterUpdated();
            }
        }

        // 
        // Pixel Quality checked
        // 
        private void cb_pixelQuality_CheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreEvent)
                return;

            ((ScaleFilter)filter).PixelQuality = cb_pixelQuality.Checked;

            FireFilterUpdated();
        }
    }
}
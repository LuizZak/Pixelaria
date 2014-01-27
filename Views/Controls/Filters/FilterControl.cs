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

using Pixelaria.Utils;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Base class for user controls that implement a visualization for twaking of filter parameters
    /// </summary>
    public class FilterControl : UserControl
    {
        /// <summary>
        /// The filter that applies the modifications to the bitmap
        /// </summary>
        protected IFilter filter;

        /// <summary>
        /// The original bitmap
        /// </summary>
        protected Bitmap originalBitmap;

        /// <summary>
        /// Whether there is an update pending
        /// </summary>
        protected bool updateRequired;

        /// <summary>
        /// The Bitmap that represents the preview for the filter
        /// </summary>
        protected Bitmap preview;

        /// <summary>
        /// Gets the name of this filter
        /// </summary>
        public virtual string FilterName { get { return filter.Name; } }

        /// <summary>
        /// Gets the filter loaded on this FilterControl
        /// </summary>
        public IFilter Filter { get { return filter; } }

        /// <summary>
        /// Initializes this FilterControl with the given Bitmap
        /// </summary>
        /// <param name="bitmap">The Bitmap to initialize the FilterControl with</param>
        public virtual void Initialize(Bitmap bitmap)
        {
            this.originalBitmap = bitmap;
        }

        /// <summary>
        /// Disposes of this FilterControl
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (preview != null)
            {
                preview.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Swaps the filter currently loaded on this FilterControl with the given filter
        /// </summary>
        /// <param name="filter">The new filter to load on this FilterControl</param>
        public virtual void SetFilter(IFilter filter)
        {
            this.filter = filter;

            UpdateFieldsFromFilter(filter);
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="filter">The IFilter instance to update the fields from</param>
        public virtual void UpdateFieldsFromFilter(IFilter filter)
        {

        }

        /// <summary>
        /// Returns a Bitmap visualization for this TransparencyControl
        /// </summary>
        /// <returns>A Bitmap visualization for this TransparencyControl</returns>
        public virtual Bitmap GetVisualization()
        {
            UpdateVisualization();

            return preview;
        }

        /// <summary>
        /// Applies the filter settings to the given Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply the filter to</param>
        public void ApplyFilter(Bitmap bitmap)
        {
            filter.ApplyToBitmap(bitmap);
        }

        /// <summary>
        /// Fires the FilterUpdated event
        /// </summary>
        public void FireFilterUpdated()
        {
            if (FilterUpdated != null)
            {
                FilterUpdated.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Updates the internal visualization of the FilterControl
        /// </summary>
        protected void UpdateVisualization()
        {
            if (updateRequired == false)
                return;

            if (preview != null)
            {
                this.preview = this.originalBitmap.Clone() as Bitmap;
            }

            FastBitmap.CopyPixels(originalBitmap, preview);

            filter.ApplyToBitmap(preview);

            updateRequired = false;
        }

        /// <summary>
        /// Occurs whenever changes have been made to the filter parameters and the visualization needs to be updated
        /// </summary>
        public event EventHandler FilterUpdated;
    }
}
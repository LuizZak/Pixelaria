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
using JetBrains.Annotations;

using PixLib.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Base interface for objects that implement a visualization for tweaking of filter parameters
    /// </summary>
    internal interface IFilterControl : IDisposable
    {
        /// <summary>
        /// Gets the filter loaded on this FilterControl
        /// </summary>
        IFilter Filter { get; }

        /// <summary>
        /// Gets the name of this filter
        /// </summary>
        string FilterName { get; }

        /// <summary>
        /// Occurs whenever changes have been made to the filter parameters and the visualization needs to be updated
        /// </summary>
        event EventHandler FilterUpdated;

        /// <summary>
        /// Swaps the filter currently loaded on this <see cref="IFilterControl"/> with the given filter
        /// </summary>
        /// <param name="newFilter">The new filter to load on this <see cref="IFilterControl"/></param>
        void SetFilter([NotNull] IFilter newFilter);

        /// <summary>
        /// Initializes this <see cref="IFilterControl"/> with the given Bitmap
        /// </summary>
        /// <param name="bitmap">The Bitmap to initialize the <see cref="IFilterControl"/> with</param>
        void Initialize([NotNull] Bitmap bitmap);
        
        /// <summary>
        /// Applies the filter settings to the given Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply the filter to</param>
        void ApplyFilter([NotNull] Bitmap bitmap);

        /// <summary>
        /// Fires the <see cref="FilterUpdated"/> event
        /// </summary>
        void FireFilterUpdated();

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="referenceFilter">The IFilter instance to update the fields from</param>
        void UpdateFieldsFromFilter([NotNull] IFilter referenceFilter);
    }
}
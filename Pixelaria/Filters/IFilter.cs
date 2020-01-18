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

using System.Drawing;
using System.IO;
using System.Reflection;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Interface to be implemented by filters.
    /// Specifies a filter that applies alterations to Bitmaps
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        bool Modifying { get; }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Applies this filter to an image
        /// </summary>
        /// <param name="target">The image to apply the filter to</param>
        void ApplyToBitmap(Bitmap target);

        /// <summary>
        /// Array of property infos from this <see cref="IFilter"/> that can be inspected and set using reflection.
        /// 
        /// Used by export pipeline UI for streamlining process of creating pipeline nodes based off of filters.
        /// </summary>
        PropertyInfo[] InspectableProperties();

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        void SaveToStream(Stream stream);

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        void LoadFromStream(Stream stream, int version);

        /// <summary>
        /// Returns whether this filter instance matches the given IFilter instance completely
        /// </summary>
        /// <param name="filter">The other filter to compare</param>
        /// <returns>true if this filter matches the given filter's type and parameters; false otherwise</returns>
        bool Equals(IFilter filter);
    }
}
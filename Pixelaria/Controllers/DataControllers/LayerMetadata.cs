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

namespace Pixelaria.Controllers.DataControllers
{
    /// <summary>
    /// Represents metadata for a frame's layer
    /// </summary>
    public struct LayerMetadata
    {
        /// <summary>
        /// Gets the size of this layer on its containing frame
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// Gets the index of this frame layer
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the display name for this layer.
        /// May be null, if no name was specified
        /// </summary>
        public string Name { get; }

        public LayerMetadata(Size size, int index, string name)
        {
            Size = size;
            Index = index;
            Name = name;
        }
    }
}
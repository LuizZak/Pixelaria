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

using System.Collections.Generic;
using System.Drawing;
using PixCore.Geometry;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// A brush for drawing with.
    /// </summary>
    public interface IBrush
    {
        
    }

    /// <summary>
    /// A solid color brush.
    /// </summary>
    public interface ISolidBrush : IBrush
    {
        /// <summary>
        /// This brush's color.
        /// </summary>
        Color Color { get; }
    }
    
    /// <summary>
    /// A gradient brush.
    /// </summary>
    public interface IGradientBrush : IBrush
    {
        /// <summary>
        /// Gets the list of gradient stops on this brush.
        /// </summary>
        IReadOnlyList<PixGradientStop> GradientStops { get; }
    }

    /// <summary>
    /// A linear gradient brush.
    /// </summary>
    public interface ILinearGradientBrush : IGradientBrush
    {
        /// <summary>
        /// The start of this linear brush
        /// </summary>
        Vector Start { get; }
        /// <summary>
        /// The end of this linear brush
        /// </summary>
        Vector End { get; }
    }

    /// <summary>
    /// Information about a gradient stop for a gradient brush.
    /// </summary>
    public struct PixGradientStop
    {
        /// <summary>
        /// The color for this gradient stop.
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// The position of this gradient stop, ranged [0 - 1]
        /// </summary>
        public float Position { get; set; }

        public PixGradientStop(Color color, float position)
        {
            Color = color;
            Position = position;
        }
    }
}

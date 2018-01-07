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
using System.Drawing.Drawing2D;

namespace Pixelaria.Algorithms.PaintOperations
{
    /// <summary>
    /// Specifies an interface to be implemented by objects that blend colors
    /// </summary>
    public interface IColorBlender
    {
        /// <summary>
        /// Returns a Color that represents the blend of the two provided background and foreground colors
        /// </summary>
        /// <param name="backColor">The background color to blend</param>
        /// <param name="foreColor">The foreground color to blend</param>
        /// <param name="compositingMode">The compositing mode to use when blending the colors</param>
        /// <returns>The blend result of the two colors</returns>
        Color BlendColors(Color backColor, Color foreColor, CompositingMode compositingMode);

        /// <summary>
        /// Returns a Color that represents the blend of the two provided background and foreground colors
        /// </summary>
        /// <param name="backColor">The background color to blend</param>
        /// <param name="foreColor">The foreground color to blend</param>
        /// <param name="compositingMode">The compositing mode to use when blending the colors</param>
        /// <returns>The blend result of the two colors</returns>
        uint BlendColors(uint backColor, uint foreColor, CompositingMode compositingMode);
    }
}
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
using PixelariaLib.Views.Controls.ColorControls;

namespace PixelariaLib.Views.Controls
{
    /// <summary>
    /// Specifies a color slider that has a very small vertical footprint and can be used in places where a minimal slider is needed
    /// </summary>
    public class TinyColorSlider : ColorSlider
    {
        /// <summary>
        /// Initializes a new instance of the TinySlider class
        /// </summary>
        public TinyColorSlider()
        {
            fixedControlHeight = 10;

            drawLabel = false;
            pnl_textHolder.Visible = false;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the slider's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider's bounds</returns>
        protected override Rectangle GetSliderRectangleBounds()
        {
            return new Rectangle(1, 1, Width - 3, Height - 3);
        }
    }
}
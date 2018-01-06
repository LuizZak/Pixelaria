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

namespace PixCore.Colors
{
    /// <summary>
    /// Specifies default color definitions
    /// </summary>
    public static class AhslColors
    {
        /// <summary>
        /// A: 1, H: 0, S: 1, L: 1
        /// </summary>
        public static AhslColor White = new AhslColor(1.0f, 0, 1, 1);

        /// <summary>
        /// A: 1, H: 0, S: 0, L: 0
        /// </summary>
        public static AhslColor Black = new AhslColor(1.0f, 0, 0, 0);

        /// <summary>
        /// A: 1, H: 0, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Red = new AhslColor(1.0f, 0, 1, 0.5f);

        /// <summary>
        /// A: 1, H: 0.333, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Green = AhslColor.FromArgb(1.0f, 0, 1, 0);

        /// <summary>
        /// A: 1, H: 0.666, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Blue = AhslColor.FromArgb(1.0f, 0, 0, 1);

        /// <summary>
        /// A: 1, H: 0.166, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Yellow = AhslColor.FromArgb(1.0f, 1, 1, 0);

        /// <summary>
        /// A: 1, H: 0.5, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Cyan = new AhslColor(1.0f, 0.5f, 1, 0.5f);
    }
}
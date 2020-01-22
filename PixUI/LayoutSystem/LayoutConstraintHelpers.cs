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

using Cassowary;

namespace PixUI.LayoutSystem
{
    public static class LayoutConstraintHelpers
    {
        /// <summary>
        /// Returns a <see cref="ClStrength"/> based on a priority value ranging
        /// from [0 - 1000].
        ///
        /// A value of 1000 always converts to <see cref="ClStrength.Required"/>.
        /// </summary>
        /// <param name="priority">A value ranging from 0 through 1000</param>
        public static ClStrength StrengthFromPriority(int priority)
        {
            if (priority >= 1000)
                return ClStrength.Required;

            int w1 = priority / 100;
            int w2 = priority / 10 % 10;
            int w3 = priority % 10;

            return new ClStrength("custom", w1 / 10.0f, w2 / 10.0f, w3 / 10.0f);
        }
    }
}
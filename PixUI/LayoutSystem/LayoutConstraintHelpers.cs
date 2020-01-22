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
using Cassowary;

namespace PixUI.LayoutSystem
{
    public static class LayoutConstraintHelpers
    {
        /// <summary>
        /// Returns a <see cref="ClStrength"/> based on a priority value ranging
        /// from [0 - 1000].
        ///
        /// A value of 1000 always converts to <see cref="ClStrength.Required"/>,
        /// 750 to <see cref="ClStrength.Strong"/>, 500 to <see cref="ClStrength.Medium"/>,
        /// and 250 <see cref="ClStrength.Weak"/>. Values in between return priorities
        /// in between, accordingly, growing in linear fashion.
        /// </summary>
        /// <param name="priority">A value ranging from 0 through 1000</param>
        public static ClStrength StrengthFromPriority(int priority)
        {
            if (priority >= 1000)
                return ClStrength.Required;
            if (priority == 750)
                return ClStrength.Strong;
            if (priority == 500)
                return ClStrength.Medium;
            if (priority == 250)
                return ClStrength.Weak;

            double upper = Math.Min(1, Math.Max(0, (priority - 500) / 250.0));
            double mid = Math.Min(1, Math.Max(0, (priority - 250) / 250.0 % 1));
            double lower = Math.Min(1, Math.Max(0, priority / 250.0 % 1));

            return new ClStrength("custom", upper, mid, lower);
        }
    }
}
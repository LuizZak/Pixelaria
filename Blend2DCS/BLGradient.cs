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

using Blend2DCS.Internal;

namespace Blend2DCS
{
    public class BLGradient
    {
        internal BLGradientCore Gradient;

        public BLGradient()
        {
            Gradient = new BLGradientCore();
            UnsafeGradientCore.blGradientInit(ref Gradient);
        }

        ~BLGradient()
        {
            UnsafeGradientCore.blGradientReset(ref Gradient);
        }
    }

    /// <summary>
    /// Gradient type.
    /// </summary>
    public enum BLGradientType: uint
    {
        /// <summary>
        /// Linear gradient type.
        /// </summary>
        Linear = 0,
        /// <summary>
        /// Radial gradient type.
        /// </summary>
        Radial = 1,
        /// <summary>
        /// Conical gradient type.
        /// </summary>
        Conical = 2,
    }

    /// <summary>
    /// Extend mode.
    /// </summary>
    public enum BLExtendMode : uint
    {
        /// <summary>
        /// Pad extend [default].
        /// </summary>
        Pad = 0,
        /// <summary>
        /// Repeat extend.
        /// </summary>
        Repeat = 1,
        /// <summary>
        /// Reflect extend.
        /// </summary>
        Reflect = 2,

        /// <summary>
        /// Alias to `PAD`.
        /// </summary>
        PadXPadY = 0,
        /// <summary>
        /// Alias to `REPEAT`.
        /// </summary>
        RepeatXRepeatY = 1,
        /// <summary>
        /// Alias to `REFLECT`.
        /// </summary>
        ReflectXReflectY = 2,
        /// <summary>
        /// Pad X and repeat Y.
        /// </summary>
        PadXRepeatY = 3,
        /// <summary>
        /// Pad X and reflect Y.
        /// </summary>
        PadXReflectY = 4,
        /// <summary>
        /// Repeat X and pad Y.
        /// </summary>
        RepeatXPadY = 5,
        /// <summary>
        /// Repeat X and reflect Y.
        /// </summary>
        RepeatXReflectY = 6,
        /// <summary>
        /// Reflect X and pad Y.
        /// </summary>
        ReflectXPadY = 7,
        /// <summary>
        /// Reflect X and repeat Y.
        /// </summary>
        ReflectXRepeatY = 8,
    }
}

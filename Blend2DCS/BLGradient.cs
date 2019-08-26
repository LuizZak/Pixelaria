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
using Blend2DCS.Geometry;
using Blend2DCS.Internal;

namespace Blend2DCS
{
    public class BLGradient : IDisposable
    {
        internal BLGradientCore Gradient;

        public BLGradientType Type => UnsafeGradientCore.blGradientGetType(ref Gradient);

        internal BLGradient()
        {
            Gradient = new BLGradientCore();
            UnsafeGradientCore.blGradientInit(ref Gradient);
        }

        ~BLGradient()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            UnsafeGradientCore.blGradientReset(ref Gradient);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        void SetType(BLGradientType type)
        {
            UnsafeGradientCore.blGradientSetType(ref Gradient, type);
        }

        public void SetValue(BLGradientValue index, double value)
        {
            UnsafeGradientCore.blGradientSetValue(ref Gradient, index, value);
        }

        public void AddStop(double offset, uint argb32)
        {
            UnsafeGradientCore.blGradientAddStopRgba32(ref Gradient, offset, argb32);
        }

        public static BLGradient Linear(BLPoint start, BLPoint end)
        {
            var gradient = new BLGradient();

            gradient.SetValue(BLGradientValue.CommonX0, start.X);
            gradient.SetValue(BLGradientValue.CommonY0, start.Y);
            gradient.SetValue(BLGradientValue.CommonX1, end.X);
            gradient.SetValue(BLGradientValue.CommonY1, end.Y);

            return gradient;
        }

    }

    /// <summary>
    /// Gradient type.
    /// </summary>
    public enum BLGradientType : uint
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
        /// Alias to <see cref="Pad"/>.
        /// </summary>
        PadXPadY = 0,

        /// <summary>
        /// Alias to <see cref="Repeat"/>.
        /// </summary>
        RepeatXRepeatY = 1,

        /// <summary>
        /// Alias to <see cref="Reflect"/>.
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
        ReflectXRepeatY = 8
    }

    /// <summary>
    /// Gradient data index.
    /// </summary>
    public enum BLGradientValue : uint
    {
        /// <summary>
        /// x0 - start 'x' for Linear/Radial and center 'x' for Conical.
        /// </summary>
        CommonX0 = 0,

        /// <summary>
        /// y0 - start 'y' for Linear/Radial and center 'y' for Conical.
        /// </summary>
        CommonY0 = 1,

        /// <summary>
        /// x1 - end 'x' for Linear/Radial.
        /// </summary>
        CommonX1 = 2,

        /// <summary>
        /// y1 - end 'y' for Linear/Radial.
        /// </summary>
        CommonY1 = 3,

        /// <summary>
        /// Radial gradient r0 radius.
        /// </summary>
        RadialR0 = 4,

        /// <summary>
        /// Conical gradient angle.
        /// </summary>
        ConicalAngle = 2
    }
}

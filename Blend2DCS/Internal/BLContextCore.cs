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
using System.Runtime.InteropServices;
using Blend2DCS.Geometry;

namespace Blend2DCS.Internal
{
    internal struct BLContextCore
    {
        internal IntPtr Impl;
    }

    // ReSharper disable InconsistentNaming
    internal static class UnsafeContextCore
    {
        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextInit(ref BLContextCore context);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern unsafe uint blContextInitAs(ref BLContextCore self, ref BLImageCore image, BLContextCreateInfo options);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextReset(ref BLContextCore context);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextGetUserMatrix(ref BLContextCore context, ref BLMatrix matrix);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextMatrixOp(ref BLContextCore context, uint opType, IntPtr opData);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextMatrixOp(ref BLContextCore context, uint opType, ref BLMatrix matrix);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextMatrixOp(ref BLContextCore context, uint opType, ref BLPoint point);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextSetFillStyleRgba32(ref BLContextCore context, uint color);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextSetFillStyle(ref BLContextCore context, ref BLGradientCore gradient);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextSetStrokeWidth(ref BLContextCore context, double width);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextSetStrokeStyleRgba32(ref BLContextCore context, uint color);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextSetStrokeStyle(ref BLContextCore context, ref BLGradientCore gradient);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextStrokePathD(ref BLContextCore context, ref BLPathCore path);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextStrokeGeometry(ref BLContextCore context, BLGeometryType type, ref BLCircle circle);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextStrokeGeometry(ref BLContextCore context, BLGeometryType type, ref BLRect rect);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextStrokeGeometry(ref BLContextCore context, BLGeometryType type, ref BLTriangle triangle);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextStrokeGeometry(ref BLContextCore context, BLGeometryType type, ref BLEllipse ellipse);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextStrokeGeometry(ref BLContextCore context, BLGeometryType type, ref BLRoundRect ellipse);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextFillPathD(ref BLContextCore context, ref BLPathCore path);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextFillGeometry(ref BLContextCore context, BLGeometryType type, ref BLCircle circle);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextFillGeometry(ref BLContextCore context, BLGeometryType type, ref BLRect rect);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextFillGeometry(ref BLContextCore context, BLGeometryType type, ref BLTriangle triangle);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextFillGeometry(ref BLContextCore context, BLGeometryType type, ref BLEllipse ellipse);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextFillGeometry(ref BLContextCore context, BLGeometryType type, ref BLRoundRect ellipse);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextClipToRectD(ref BLContextCore context, ref BLRect rect);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blContextRestoreClipping(ref BLContextCore context);
    }
}

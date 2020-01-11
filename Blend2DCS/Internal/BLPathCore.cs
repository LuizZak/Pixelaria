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
    internal struct BLPathCore
    {
        internal IntPtr Impl;
    }

    // ReSharper disable InconsistentNaming
    internal static class UnsafePathCore
    {
        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blPathInit(ref BLPathCore pathCore);
        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blPathReset(ref BLPathCore pathCore);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern int blPathGetSize(ref BLPathCore pathCore);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern int blPathGetCapacity(ref BLPathCore pathCore);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern int blPathMoveTo(ref BLPathCore pathCore, double x, double y);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern int blPathLineTo(ref BLPathCore pathCore, double x, double y);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern int blPathCubicTo(ref BLPathCore pathCore, double x1, double y1, double x2, double y2, double x3, double y3);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern int blPathAddRectD(ref BLPathCore pathCore, ref BLRect rect, BLGeometryDirection geometryDirection);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern int blPathClose(ref BLPathCore pathCore);
    }
}

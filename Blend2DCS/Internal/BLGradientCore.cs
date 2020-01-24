﻿/*
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

namespace Blend2DCS.Internal
{
    internal struct BLGradientCore
    {
        internal IntPtr Impl;
    }

    // ReSharper disable InconsistentNaming
    internal static class UnsafeGradientCore
    {
        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blGradientInit(ref BLGradientCore context);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blGradientReset(ref BLGradientCore context);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blGradientSetType(ref BLGradientCore context, BLGradientType type);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern BLGradientType blGradientGetType(ref BLGradientCore context);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern double blGradientGetValue(ref BLGradientCore context, BLGradientValue index);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blGradientSetValue(ref BLGradientCore context, BLGradientValue index, double value);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blGradientAddStopRgba32(ref BLGradientCore context, double offset, uint argb32);
    }
}
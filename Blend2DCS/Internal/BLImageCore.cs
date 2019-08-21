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
    internal struct BLImageCore
    {
        internal IntPtr Impl;
    }

    // ReSharper disable InconsistentNaming
    internal static class UnsafeImageCore
    {
        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageInit(ref BLImageCore self);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageInitAs(ref BLImageCore self, int w, int h, uint format);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageReset(ref BLImageCore self);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageGetData(ref BLImageCore self, ref BLImageData);
    }
}

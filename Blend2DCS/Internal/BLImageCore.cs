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
    internal struct BLImageCore
    {
        internal IntPtr Impl;

        internal BLImageImpl GetImplementation()
        {
            return Impl == IntPtr.Zero ? new BLImageImpl() : Marshal.PtrToStructure<BLImageImpl>(Impl);
        }
    }

    /// <summary>
    /// Image [C Interface - Impl].
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLImageImpl
    {
        /// <summary>
        /// Pixel data.
        /// </summary>
        public IntPtr PixelData;
        /// <summary>
        /// Image stride.
        /// </summary>
        public IntPtr Stride;
        /// <summary>
        /// Non-null if the image has a writer.
        /// </summary>
        public IntPtr Writer;

        /// <summary>
        /// Reference count.
        /// </summary>
        public IntPtr RefCount;
        /// <summary>
        /// Impl type.
        /// </summary>
        public byte ImplType;
        /// <summary>
        /// Impl traits.
        /// </summary>
        public byte ImplTraits;
        /// <summary>
        /// Memory pool data.
        /// </summary>
        public ushort MemPoolData;

        /// <summary>
        /// Image format.
        /// </summary>
        public byte Format;
        /// <summary>
        /// Image flags.
        /// </summary>
        public byte Flags;
        /// <summary>
        /// Image depth (in bits).
        /// </summary>
        public ushort Depth;
        /// <summary>
        /// Image size.
        /// </summary>
        public BLSizeI Size;
    }

    // ReSharper disable InconsistentNaming
    internal static class UnsafeImageCore
    {
        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageInit(ref BLImageCore self);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageInitAs(ref BLImageCore self, int w, int h, BLFormat format);
        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageInitAs(ref BLImageCore self, int w, int h, BLFormat format, IntPtr pixelData, int stride, IntPtr destroyFunc, IntPtr destroyData);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageReset(ref BLImageCore self);

        [DllImport("blend2d.dll", CharSet = CharSet.Unicode)]
        public static extern uint blImageGetData(ref BLImageCore self, ref BLImageData imageData);
    }
}

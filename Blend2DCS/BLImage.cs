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
    public class BLImage : IDisposable
    {
        internal BLImageCore Image;

        public BLSizeI Size => Image.GetImplementation().Size;

        public int Width => Size.Width;

        public int Height => Size.Height;

        public BLImage()
        {
            Image = new BLImageCore();
            UnsafeImageCore.blImageInit(ref Image);
        }

        public BLImage(int width, int height, BLFormat format)
        {
            Image = new BLImageCore();
            UnsafeImageCore.blImageInitAs(ref Image, width, height, format);
        }

        public BLImage(int width, int height, BLFormat format, IntPtr pixelData, int stride)
        {
            Image = new BLImageCore();
            UnsafeImageCore.blImageInitAsFromData(ref Image, width, height, (uint) format, pixelData, (IntPtr) stride, IntPtr.Zero, IntPtr.Zero);
        }

        ~BLImage()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            UnsafeImageCore.blImageReset(ref Image);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public BLImageData GetData()
        {
            var imageData = new BLImageData();
            UnsafeImageCore.blImageGetData(ref Image, ref imageData);
            return imageData;
        }
    }

    public enum BLFormat: uint
    {
        /// <summary>
        /// None or invalid pixel format.
        /// </summary>
        None = 0,
        /// <summary>
        /// 32-bit pre-multiplied ARGB pixel format (8-bit components).
        /// </summary>
        Prgb32 = 1,
        /// <summary>
        /// 32-bit (X)RGB pixel format (8-bit components, alpha ignored).
        /// </summary>
        Xrgb32 = 2,
        /// <summary>
        /// 8-bit alpha-only pixel format.
        /// </summary>
        A8 = 3
    }

    /// <summary>
    /// Pixel format flags.
    /// </summary>
    public enum BLFormatFlags : uint
    {
        /// <summary>
        /// Pixel format provides RGB components.
        /// </summary>
        Rgb = 0x00000001u,
        /// <summary>
        /// Pixel format provides only alpha component.
        /// </summary>
        Alpha = 0x00000002u,
        /// <summary>
        /// A combination of <see cref="Rgb"/> | <see cref="Alpha"/>.
        /// </summary>
        Rgba = 0x00000003u,
        /// <summary>
        /// Pixel format provides LUM component (and not RGB components).
        /// </summary>
        Lum = 0x00000004u,
        /// <summary>
        /// A combination of `<see cref="Lum"/> | <see cref="Alpha"/>`.
        /// </summary>
        Luma = 0x00000006u,
        /// <summary>
        /// Indexed pixel format the requres a palette (I/O only).
        /// </summary>
        Indexed = 0x00000010u,
        /// <summary>
        /// RGB components are premultiplied by alpha component.
        /// </summary>
        Premultiplied = 0x00000100u,
        /// <summary>
        /// Pixel format doesn't use native byte-order (I/O only).
        /// </summary>
        ByteSwap = 0x00000200u,

        // The following flags are only informative. They are part of `blFormatInfo[]`,
        // but doesn't have to be passed to `BLPixelConverter` as they can be easily
        // calculated.

        /// <summary>
        /// Pixel components are byte aligned (all 8bpp).
        /// </summary>
        ByteAligned = 0x00010000u
    }
}

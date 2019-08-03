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
using Blend2DCS.Internal;

namespace Blend2DCS
{
    public class BLImage
    {
        internal BLImageCore Image;

        public BLImage()
        {
            Image = new BLImageCore();
            UnsafeImageCore.blImageInit(ref Image);
        }

        public BLImage(int width, int height, BLFormat format)
        {
            Image = new BLImageCore();
            UnsafeImageCore.blImageInitAs(ref Image, width, height, (uint) format);
        }

        ~BLImage()
        {
            UnsafeImageCore.blImageReset(ref Image);
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
    };
}

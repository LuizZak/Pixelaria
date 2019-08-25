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
using JetBrains.Annotations;

namespace Blend2DCS
{
    public class BLFont : IDisposable
    {
        internal BLFontCore Font;

        public BLFont()
        {
            UnsafeFontCore.blFontInit(ref Font);
        }

        public BLFont([NotNull] BLFontFace face, float size)
        {
            Exceptions.ThrowOnError(UnsafeFontCore.blFontInit(ref Font));
            Exceptions.ThrowOnError(UnsafeFontCore.blFontCreateFromFace(ref Font, ref face.FontFace, size));
        }

        private void ReleaseUnmanagedResources()
        {
            UnsafeFontCore.blFontReset(ref Font);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~BLFont()
        {
            ReleaseUnmanagedResources();
        }
    }

    /// <summary>
    /// Text encoding.
    /// </summary>
    public enum BLTextEncoding : uint
    {
        /// <summary>
        /// UTF-8 encoding.
        /// </summary>
        UTF8 = 0,
        /// <summary>
        /// UTF-16 encoding (native endian).
        /// </summary>
        UTF16 = 1,
        /// <summary>
        /// UTF-32 encoding (native endian).
        /// </summary>
        UTF32 = 2,
        /// <summary>
        /// LATIN1 encoding (one byte per character).
        /// </summary>
        Latin1 = 3,
    };
}

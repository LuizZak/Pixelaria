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

        public BLFontMetrics GetMetrics()
        {
            var metrics = new BLFontMetrics();
            UnsafeFontCore.blFontGetMetrics(ref Font, ref metrics);
            return metrics;
        }

        public BLFontDesignMetrics GetDesignMetrics()
        {
            var metrics = new BLFontDesignMetrics();
            UnsafeFontCore.blFontGetDesignMetrics(ref Font, ref metrics);
            return metrics;
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
    }

    /// <summary>
    /// Scaled <see cref="BLFontDesignMetrics"/> based on font size and other properties.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BLFontMetrics
    {
        /// <summary>
        /// Font size.
        /// </summary>
        public float Size;

        /// <summary>
        /// Font ascent (horizontal orientation).
        /// </summary>
        public float Ascent;
        /// <summary>
        /// Font ascent (vertical orientation).
        /// </summary>
        public float VAscent;
        /// <summary>
        /// Font descent (horizontal orientation).
        /// </summary>
        public float Descent;
        /// <summary>
        /// Font descent (vertical orientation).
        /// </summary>
        public float VDescent;

        /// <summary>
        /// Line gap.
        /// </summary>
        public float LineGap;
        /// <summary>
        /// Distance between the baseline and the mean line of lower-case letters.
        /// </summary>
        public float XHeight;
        /// <summary>
        /// Maximum height of a capital letter above the baseline.
        /// </summary>
        public float CapHeight;

        /// <summary>
        /// Text underline position.
        /// </summary>
        public float UnderlinePosition;
        /// <summary>
        /// Text underline thickness.
        /// </summary>
        public float UnderlineThickness;
        /// <summary>
        /// Text strikethrough position.
        /// </summary>
        public float StrikethroughPosition;
        /// <summary>
        /// Text strikethrough thickness.
        /// </summary>
        public float StrikethroughThickness;
    }

    /// <summary>
    /// Design metrics of a font.
    ///
    /// Design metrics is information that <see cref="BLFontFace"/> collected directly from the
    /// font data. It means that all fields are measured in font design units.
    ///
    /// When a new <see cref="BLFont"/> instance is created a scaled metrics `BLFontMetrics` is
    /// automatically calculated from <see cref="BLFontDesignMetrics"/> including other members
    /// like transformation, etc...
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BLFontDesignMetrics
    {
        /// <summary>
        /// Units per EM square.
        /// </summary>
        public int UnitsPerEm;

        /// <summary>
        /// Line gap.
        /// </summary>
        public int LineGap;

        /// <summary>
        /// Distance between the baseline and the mean line of lower-case letters.
        /// </summary>
        public int XHeight;

        /// <summary>
        /// Maximum height of a capital letter above the baseline.
        /// </summary>
        public int CapHeight;

        /// <summary>
        /// Ascent (horizontal layout).
        /// </summary>
        public int Ascent;

        /// <summary>
        /// Ascent (vertical layout).
        /// </summary>
        public int VAscent;

        /// <summary>
        /// Descent (horizontal layout).
        /// </summary>
        public int Descent;

        /// <summary>
        /// Descent (vertical layout).
        /// </summary>
        public int VDescent;

        /// <summary>
        /// Minimum leading-side bearing (horizontal layout).
        /// </summary>
        public int HMinLsb;

        /// <summary>
        /// Minimum leading-side bearing (vertical layout).
        /// </summary>
        public int VMinLsb;

        /// <summary>
        /// Minimum trailing-side bearing (horizontal layout).
        /// </summary>
        public int HMinTsb;

        /// <summary>
        /// Minimum trailing-side bearing (vertical layout).
        /// </summary>
        public int VMinTsb;

        /// <summary>
        /// Maximum advance (horizontal layout).
        /// </summary>
        public int HMaxAdvance;

        /// <summary>
        /// Maximum advance (vertical layout).
        /// </summary>
        public int VMaxAdvance;

        /// <summary>
        /// Text underline position.
        /// </summary>
        public int UnderlinePosition;

        /// <summary>
        /// Text underline thickness.
        /// </summary>
        public int UnderlineThickness;

        /// <summary>
        /// Text strikethrough position.
        /// </summary>
        public int StrikethroughPosition;

        /// <summary>
        /// Text strikethrough thickness.
        /// </summary>
        public int StrikethroughThickness;
    }
}

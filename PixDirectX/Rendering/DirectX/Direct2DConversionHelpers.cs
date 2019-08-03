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
using PixRendering;
using SharpDX;
using SharpDX.DirectWrite;
using Color = System.Drawing.Color;

namespace PixDirectX.Rendering.DirectX
{
    /// <summary>
    /// Small set of helper methods for conversions to Direct2D types
    /// </summary>
    public static class Direct2DConversionHelpers
    {
        public static TextAlignment DirectWriteAlignmentFor(HorizontalTextAlignment alignment)
        {
            return (TextAlignment) alignment;
        }
        public static ParagraphAlignment DirectWriteAlignmentFor(VerticalTextAlignment alignment)
        {
            return (ParagraphAlignment) alignment;
        }
        public static WordWrapping DirectWriteWordWrapFor(TextWordWrap wordWrap)
        {
            switch (wordWrap)
            {
                case TextWordWrap.None:
                    return WordWrapping.NoWrap;
                case TextWordWrap.ByCharacter:
                    return WordWrapping.Character;
                case TextWordWrap.ByWord:
                    return WordWrapping.WholeWord;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wordWrap), wordWrap, null);
            }
        }
        public static TrimmingGranularity DirectWriteGranularityFor(TextTrimmingGranularity granularity)
        {
            return (TrimmingGranularity) granularity;
        }

        public static HorizontalTextAlignment HorizontalTextAlignmentFor(TextAlignment alignment)
        {
            return (HorizontalTextAlignment) alignment;
        }
        public static VerticalTextAlignment VerticalTextAlignmentFor(ParagraphAlignment alignment)
        {
            return (VerticalTextAlignment) alignment;
        }
        public static TextWordWrap TextWordWrapFor(WordWrapping wordWrapping)
        {
            switch (wordWrapping)
            {
                case WordWrapping.NoWrap:
                    return TextWordWrap.None;
                case WordWrapping.Character:
                    return TextWordWrap.ByCharacter;
                case WordWrapping.WholeWord:
                    return TextWordWrap.ByWord;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wordWrapping), wordWrapping, null);
            }
        }
        public static TextTrimmingGranularity TextTrimmingGranularityFor(TrimmingGranularity granularity)
        {
            return (TextTrimmingGranularity) granularity;
        }

        /// <summary>
        /// Converts a <see cref="Color"/> into a <see cref="Color4"/> structure for DirectX rendering.
        /// </summary>
        public static Color4 ToColor4(this Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;
            
            return new Color4(r, g, b, a);
        }
    }
}
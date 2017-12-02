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
using PixUI.Controls;
using SharpDX;
using SharpDX.DirectWrite;
using Color = System.Drawing.Color;

namespace PixUI.Rendering
{
    public static class Direct2DHelpers
    {
        public static TextAlignment DirectWriteAlignmentFor(HorizontalTextAlignment alignment)
        {
            TextAlignment horizontalAlign;

            switch (alignment)
            {
                case HorizontalTextAlignment.Leading:
                    horizontalAlign = TextAlignment.Leading;
                    break;
                case HorizontalTextAlignment.Center:
                    horizontalAlign = TextAlignment.Center;
                    break;
                case HorizontalTextAlignment.Trailing:
                    horizontalAlign = TextAlignment.Trailing;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return horizontalAlign;
        }

        public static ParagraphAlignment DirectWriteAlignmentFor(VerticalTextAlignment alignment)
        {
            ParagraphAlignment verticalAlign;

            switch (alignment)
            {
                case VerticalTextAlignment.Near:
                    verticalAlign = ParagraphAlignment.Near;
                    break;
                case VerticalTextAlignment.Center:
                    verticalAlign = ParagraphAlignment.Center;
                    break;
                case VerticalTextAlignment.Far:
                    verticalAlign = ParagraphAlignment.Far;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return verticalAlign;
        }

        public static WordWrapping DirectWriteWordWrapFor(TextWordWrap wordWrap)
        {
            WordWrapping verticalAlign;

            switch (wordWrap)
            {
                case TextWordWrap.None:
                    verticalAlign = WordWrapping.NoWrap;
                    break;
                case TextWordWrap.ByCharacter:
                    verticalAlign = WordWrapping.Character;
                    break;
                case TextWordWrap.ByWord:
                    verticalAlign = WordWrapping.Wrap;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return verticalAlign;
        }

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

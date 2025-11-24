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

using System.Drawing;
using JetBrains.Annotations;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixRendering;
using SharpDX.DirectWrite;
using Font = System.Drawing.Font;

namespace PixDirectX.Rendering.DirectX
{
    public class D2DTextSizeProvider : ITextSizeProvider
    {
        [CanBeNull]
        private readonly Factory _directWriteFactory;

        public D2DTextSizeProvider()
        {
            _directWriteFactory = new Factory();
        }

        public SizeF CalculateTextSize(string text, Font font)
        {
            return CalculateTextSize(new AttributedText(text), font);
        }

        public SizeF CalculateTextSize(AttributedText text, Font font)
        {
            return CalculateTextSize(text, font.Name, font.Size);
        }

        public SizeF CalculateTextSize(AttributedText text, string font, float fontSize)
        {
            var format = new TextFormat(_directWriteFactory, font, fontSize)
            {
                TextAlignment = TextAlignment.Leading,
                ParagraphAlignment = ParagraphAlignment.Center
            };

            using (var textFormat = format)
            using (var textLayout = new TextLayout(_directWriteFactory, text.String, textFormat, float.PositiveInfinity, float.PositiveInfinity))
            {
                foreach (var textSegment in text.GetTextSegments())
                {
                    if (!textSegment.HasAttribute<TextFontAttribute>())
                        continue;

                    var fontAttr = textSegment.GetAttribute<TextFontAttribute>();
                    var textRange = new SharpDX.DirectWrite.TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length);

                    ApplyFont(textLayout, fontAttr.Font, textRange);
                }

                return new SizeF(textLayout.Metrics.Width, textLayout.Metrics.Height);
            }
        }

        // TODO: Reduce duplication with InnerTextRenderer

        private void ApplyFont(TextLayout textLayout, Font font, SharpDX.DirectWrite.TextRange textRange)
        {
            textLayout.SetFontFamilyName(font.FontFamily.Name, textRange);
            textLayout.SetFontStyle(FontStyleFromSystemFontStyle(font.Style), textRange);
            textLayout.SetFontWeight(FontWeightFromFontStyle(font.Style), textRange);
            textLayout.SetFontSize(font.Size, textRange);

            if (font.Style.HasFlag(System.Drawing.FontStyle.Underline))
            {
                textLayout.SetUnderline(true, textRange);
            }
            if (font.Style.HasFlag(System.Drawing.FontStyle.Strikeout))
            {
                textLayout.SetStrikethrough(true, textRange);
            }
        }

        private SharpDX.DirectWrite.FontStyle FontStyleFromSystemFontStyle(System.Drawing.FontStyle fontStyle)
        {
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Italic))
                return SharpDX.DirectWrite.FontStyle.Italic;

            return SharpDX.DirectWrite.FontStyle.Normal;
        }

        private FontWeight FontWeightFromFontStyle(System.Drawing.FontStyle fontStyle)
        {
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Bold))
                return FontWeight.Bold;

            return FontWeight.Normal;
        }
    }
}
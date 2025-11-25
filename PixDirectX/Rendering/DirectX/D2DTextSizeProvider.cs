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
using Vortice.DirectWrite;
using Font = System.Drawing.Font;

using DXTextRange = Vortice.DirectWrite.TextRange;
using DXTextLayout = Vortice.DirectWrite.IDWriteTextLayout;
using DXFontStyle = Vortice.DirectWrite.FontStyle;
using DXFontWeight = Vortice.DirectWrite.FontWeight;

namespace PixDirectX.Rendering.DirectX
{
    public class D2DTextSizeProvider : ITextSizeProvider
    {
        [CanBeNull]
        private readonly IDWriteFactory _directWriteFactory;

        public D2DTextSizeProvider()
        {
            _directWriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();
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
            var format = _directWriteFactory.CreateTextFormat(font, fontSize);

            format.TextAlignment = TextAlignment.Leading;
            format.ParagraphAlignment = ParagraphAlignment.Center;

            using (var textFormat = format)
            using (var textLayout = _directWriteFactory.CreateTextLayout(text.String, format, float.PositiveInfinity, float.PositiveInfinity))
            {
                foreach (var textSegment in text.GetTextSegments())
                {
                    if (!textSegment.HasAttribute<TextFontAttribute>())
                        continue;

                    var fontAttr = textSegment.GetAttribute<TextFontAttribute>();
                    var textRange = new DXTextRange(textSegment.TextRange.Start, textSegment.TextRange.Length);

                    ApplyFont(textLayout, fontAttr.Font, textRange);
                }

                return new SizeF(textLayout.Metrics.Width, textLayout.Metrics.Height);
            }
        }

        // TODO: Reduce duplication with InnerTextRenderer

        private void ApplyFont(DXTextLayout textLayout, Font font, DXTextRange textRange)
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

        private DXFontStyle FontStyleFromSystemFontStyle(System.Drawing.FontStyle fontStyle)
        {
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Italic))
                return DXFontStyle.Italic;

            return DXFontStyle.Normal;
        }

        private DXFontWeight FontWeightFromFontStyle(System.Drawing.FontStyle fontStyle)
        {
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Bold))
                return DXFontWeight.Bold;

            return DXFontWeight.Normal;
        }
    }
}
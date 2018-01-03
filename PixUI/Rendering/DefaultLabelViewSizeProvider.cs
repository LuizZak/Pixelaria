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
using PixCore.Text;
using PixCore.Text.Attributes;
using PixDirectX.Rendering;
using SharpDX.DirectWrite;
using Font = System.Drawing.Font;

namespace PixUI.Rendering
{
    public class DefaultLabelViewSizeProvider : ILabelViewSizeProvider
    {
        private readonly IDirect2DRenderingStateProvider _renderingStateProvider;

        public DefaultLabelViewSizeProvider(IDirect2DRenderingStateProvider renderingStateProvider)
        {
            _renderingStateProvider = renderingStateProvider;
        }

        public SizeF CalculateTextSize(LabelView labelView)
        {
            return CalculateTextSize(labelView.AttributedText, labelView.TextFont);
        }

        public SizeF CalculateTextSize(string text, Font font)
        {
            return CalculateTextSize(new AttributedText(text), font);
        }

        public SizeF CalculateTextSize(IAttributedText text, Font font)
        {
            return CalculateTextSize(text, font.Name, font.Size);
        }

        public SizeF CalculateTextSize(IAttributedText text, string font, float fontSize)
        {
            var renderState = _renderingStateProvider.GetLatestValidRenderingState();
            if (renderState == null)
                return SizeF.Empty;

            using (var textFormat = new TextFormat(renderState.DirectWriteFactory, font, fontSize) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Center, WordWrapping = WordWrapping.WholeWord })
            using (var textLayout = new TextLayout(renderState.DirectWriteFactory, text.String, textFormat, float.PositiveInfinity, float.PositiveInfinity))
            {
                foreach (var textSegment in text.GetTextSegments())
                {
                    if (!textSegment.HasAttribute<TextFontAttribute>())
                        continue;

                    var fontAttr = textSegment.GetAttribute<TextFontAttribute>();

                    textLayout.SetFontFamilyName(fontAttr.Font.FontFamily.Name,
                        new SharpDX.DirectWrite.TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    textLayout.SetFontSize(fontAttr.Font.Size,
                        new SharpDX.DirectWrite.TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                }

                return new SizeF(textLayout.Metrics.Width, textLayout.Metrics.Height);
            }
        }
    }
}
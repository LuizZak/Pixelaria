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
using System.Drawing;
using PixCore.Geometry;
using PixCore.Text;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiTextRenderer : ITextRenderer
    {
        private readonly Graphics _graphics;
        private readonly Color _textColor;

        public GdiTextRenderer(Graphics graphics, Color textColor)
        {
            _graphics = graphics;
            _textColor = textColor;
        }

        public void Draw(ITextLayout textLayout, float x, float y)
        {
            Draw(textLayout.Text, textLayout.Attributes.TextFormatAttributes, AABB.FromRectangle(x, y, textLayout.Attributes.AvailableWidth, textLayout.Attributes.AvailableHeight), _textColor);
        }

        public void Draw(IAttributedText text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            var layoutRect = (RectangleF) area;

            foreach (var segment in text.GetTextSegments())
            {
                var segmentConsumer = new CollectingTextAttributeConsumer();
                segment.ConsumeAttributes(segmentConsumer);

                var foreColor = segmentConsumer.ForeColor ?? color;

                var font = segmentConsumer?.Font ?? new Font(textFormatAttributes.Font, textFormatAttributes.FontSize);

                var stringFormat = new StringFormat
                {
                    Alignment = ToStringAlignment(textFormatAttributes.HorizontalTextAlignment)
                };

                stringFormat.SetMeasurableCharacterRanges(new []{ new CharacterRange(segment.TextRange.Start, segment.TextRange.Length) });

                if (textFormatAttributes.TextEllipsisTrimming.HasValue)
                {
                    stringFormat.Trimming = ToStringTrimming(textFormatAttributes.TextEllipsisTrimming.Value.Granularity);
                }
                else
                {
                    stringFormat.Trimming = StringTrimming.None;
                }

                var regions = _graphics.MeasureCharacterRanges(text.String, font, layoutRect, stringFormat);

                foreach (var region in regions)
                {
                    using (var solidBrush = new SolidBrush(foreColor))
                    {
                        var rect = region.GetBounds(_graphics);
                        _graphics.DrawString(segment.Text, font, solidBrush, rect, stringFormat);
                    }
                }

                if (segmentConsumer.Font == null)
                {
                    font.Dispose();
                }
            }
        }

        public void Draw(string text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            using (var font = new Font(textFormatAttributes.Font, textFormatAttributes.FontSize))
            using (var solidBrush = new SolidBrush(color))
            {
                _graphics.DrawString(text, font, solidBrush, (RectangleF) area);
            }
        }

        private static StringAlignment ToStringAlignment(HorizontalTextAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalTextAlignment.Leading:
                    return StringAlignment.Near;
                case HorizontalTextAlignment.Trailing:
                    return StringAlignment.Far;
                case HorizontalTextAlignment.Center:
                    return StringAlignment.Center;
                case HorizontalTextAlignment.Justified:
                    return StringAlignment.Near;
                default:
                    return StringAlignment.Near;
            }
        }

        private static StringTrimming ToStringTrimming(TextTrimmingGranularity granularity)
        {
            switch (granularity)
            {
                case TextTrimmingGranularity.None:
                    return StringTrimming.None;
                case TextTrimmingGranularity.Character:
                    return StringTrimming.Character;
                case TextTrimmingGranularity.Word:
                    return StringTrimming.EllipsisWord;
                default:
                    return StringTrimming.None;
            }
        }
    }
}
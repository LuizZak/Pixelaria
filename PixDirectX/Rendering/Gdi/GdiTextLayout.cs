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
using PixCore.Geometry;
using PixCore.Text;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiTextLayout : ITextLayout
    {
        private readonly Bitmap _bitmap = new Bitmap(1, 1);

        public TextLayoutAttributes Attributes { get; }

        public IAttributedText Text { get; }

        public GdiTextLayout(TextLayoutAttributes attributes, IAttributedText text)
        {
            Attributes = attributes;
            Text = text;
        }

        public void Dispose()
        {
            _bitmap.Dispose();
        }

        public HitTestMetrics HitTestPoint(float x, float y, out bool isTrailingHit, out bool isInside)
        {
            using (var graphics = Graphics.FromImage(_bitmap))
            using (var font = CreateFont())
            {
                var stringFormat = new StringFormat();
                stringFormat.SetMeasurableCharacterRanges(new[] {new CharacterRange(0, Text.Length)});

                var regions = graphics.MeasureCharacterRanges(Text.String, font, CreateBounds(), stringFormat);
                for (int i = 0; i < regions.Length; i++)
                {
                    var region = regions[i];
                    var bounds = region.GetBounds(graphics);

                    if (!bounds.Contains(new PointF(x, y)))
                        continue;

                    isTrailingHit = bounds.Center().X < x;
                    isInside = true;
                    return new HitTestMetrics(i);
                }
            }

            isTrailingHit = false;
            isInside = false;

            return new HitTestMetrics(-1);
        }

        public HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit, out float x, out float y)
        {
            using (var graphics = Graphics.FromImage(_bitmap))
            using (var font = CreateFont())
            {
                var stringFormat = new StringFormat();
                stringFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(textPosition, 1) });

                var regions = graphics.MeasureCharacterRanges(Text.String, font, CreateBounds(), stringFormat);
                foreach (var region in regions)
                {
                    var bounds = region.GetBounds(graphics);

                    x = isTrailingHit ? bounds.Right : bounds.Left;
                    
                    y = bounds.Y;

                    return new HitTestMetrics(textPosition);
                }
            }

            x = 0;
            y = 0;

            return new HitTestMetrics(-1);
        }

        private Font CreateFont()
        {
            return new Font(Attributes.TextFormatAttributes.Font, Attributes.TextFormatAttributes.FontSize);
        }

        private RectangleF CreateBounds()
        {
            return new RectangleF(0, 0, Attributes.AvailableWidth, Attributes.AvailableHeight);
        }
    }
}
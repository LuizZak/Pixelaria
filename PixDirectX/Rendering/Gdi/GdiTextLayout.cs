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
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixRendering;

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
                Region closestRegion = null;
                int closestIndex = 0;
                float closestDist = float.PositiveInfinity;

                foreach (var (region, i) in GdiTextUtils.CharacterRegions(0, Text.Length, Text.String, font, graphics, CreateBounds()))
                {
                    var bounds = region.GetBounds(graphics);

                    if (bounds.Contains(new PointF(x, y)))
                    {
                        isTrailingHit = bounds.Center().X < x;
                        isInside = true;
                        return new HitTestMetrics(i);
                    }

                    var distance = bounds.Center().DistanceSquared(new PointF(x, y));
                    if (distance < closestDist)
                    {
                        closestRegion = region;
                        closestDist = distance;
                        closestIndex = i;
                    }
                }

                if (closestRegion != null)
                {
                    var region = closestRegion;
                    var bounds = region.GetBounds(graphics);

                    isTrailingHit = bounds.Center().X < x;
                    isInside = false;
                    return new HitTestMetrics(closestIndex);
                }
            }

            isTrailingHit = false;
            isInside = false;

            return new HitTestMetrics(0);
        }

        public HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit, out float x, out float y)
        {
            using (var graphics = Graphics.FromImage(_bitmap))
            using (var font = CreateFont())
            {
                var stringFormat = new StringFormat();
                stringFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(Math.Min(Text.Length - 1, textPosition), 1) });

                var regions = graphics.MeasureCharacterRanges(Text.String, font, CreateBounds(), stringFormat);
                foreach (var region in regions)
                {
                    var bounds = region.GetBounds(graphics);

                    x = isTrailingHit || textPosition == Text.Length ? bounds.Right : bounds.Left;
                    
                    y = bounds.Y;

                    return new HitTestMetrics(textPosition);
                }
            }

            x = 0;
            y = 0;

            return new HitTestMetrics(0);
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

    public static class GdiTextUtils
    {
        public static IEnumerable<(Region, int)> CharacterRegions(int start, int length, [NotNull] string text, [NotNull] Font font, [NotNull] Graphics graphics, RectangleF bounds)
        {
            using (var stringFormat = new StringFormat())
            {
                for (int charStart = start; charStart < length; charStart += 32)
                {
                    var charRange = new CharacterRange[Math.Min(text.Length - charStart, 32)];
                    for (int i = 0; i < charRange.Length; i++)
                    {
                        charRange[i].First = charStart + i;
                        charRange[i].Length = 1;
                    }

                    stringFormat.SetMeasurableCharacterRanges(charRange);

                    var regions = graphics.MeasureCharacterRanges(text, font, bounds, stringFormat);
                    for (int i = 0; i < regions.Length; i++)
                    {
                        var region = regions[i];

                        yield return (region, i);
                    }
                }
            }
        }
    }
}
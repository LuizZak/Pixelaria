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
using System.Linq;
using PixCore.Geometry;
using PixCore.Text;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiTextMetricsProvider : ITextMetricsProvider
    {
        private readonly Func<Graphics> _fetchGraphics;

        public GdiTextMetricsProvider(Func<Graphics> fetchGraphics)
        {
            _fetchGraphics = fetchGraphics;
        }

        public AABB LocationOfCharacter(int offset, IAttributedText text, TextLayoutAttributes textLayoutAttributes)
        {
            var graphics = _fetchGraphics();

            using (var format = new StringFormat())
            using (var font = new Font(textLayoutAttributes.TextFormatAttributes.Font, textLayoutAttributes.TextFormatAttributes.FontSize))
            {
                format.SetMeasurableCharacterRanges(new[] { new CharacterRange(offset, 1) });

                var layoutRect = new RectangleF(0, 0, textLayoutAttributes.AvailableWidth, textLayoutAttributes.AvailableHeight);

                return graphics.MeasureCharacterRanges(text.String, font, layoutRect, format).Select(r => (AABB)r.GetBounds(graphics)).First();
            }
        }

        public AABB[] LocationOfCharacters(int offset, int length, IAttributedText text, TextLayoutAttributes textLayoutAttributes)
        {
            var graphics = _fetchGraphics();

            using (var format = new StringFormat())
            using (var font = new Font(textLayoutAttributes.TextFormatAttributes.Font, textLayoutAttributes.TextFormatAttributes.FontSize))
            {
                format.SetMeasurableCharacterRanges(new [] { new CharacterRange(offset, length) });

                var layoutRect = new RectangleF(0, 0, textLayoutAttributes.AvailableWidth, textLayoutAttributes.AvailableHeight);

                return graphics.MeasureCharacterRanges(text.String, font, layoutRect, format).Select(r => (AABB) r.GetBounds(graphics)).ToArray();
            }
        }
    }
}
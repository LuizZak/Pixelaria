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
using PixCore.Text;
using PixRendering;
using SkiaSharp;

namespace PixDirectX.Rendering.Skia
{
    class SkiaTextLayout : ITextLayout
    {
        public TextLayoutAttributes Attributes { get; }
        public IAttributedText Text { get; }
        public SkiaTextLayout(TextLayoutAttributes attributes, IAttributedText text)
        {
            Attributes = attributes;
            Text = text;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public HitTestMetrics HitTestPoint(float x, float y, out bool isTrailingHit, out bool isInside)
        {
            using (var paint = PaintForLayout())
            {
                var path = paint.GetTextPath(Text.String, 0, 0);
                
            }
        }

        public HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit, out float x, out float y)
        {
            throw new NotImplementedException();
        }

        SKPaint PaintForLayout()
        {
            return new SKPaint
            {
                TextSize = Attributes.TextFormatAttributes.FontSize,
                Typeface = SKTypeface.FromFamilyName(Attributes.TextFormatAttributes.Font)
            };
        }
    }
}

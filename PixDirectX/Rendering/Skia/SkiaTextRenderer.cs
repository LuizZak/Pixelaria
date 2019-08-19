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
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixDirectX.Utils;
using PixRendering;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace PixDirectX.Rendering.Skia
{
    public class SkiaTextRenderer : ITextRenderer
    {
        private readonly SKCanvas _canvas;
        private readonly Color _textColor;

        public SkiaTextRenderer(SKCanvas canvas, Color textColor)
        {
            _canvas = canvas;
            _textColor = textColor;
        }

        public void Draw(ITextLayout textLayout, float x, float y)
        {
            using (var paint = PaintForLayout(textLayout))
            {
                _canvas.DrawText(textLayout.Text.String, x, y, paint);
            }
        }

        public void Draw(IAttributedText text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            using (var paint = new SKPaint())
            {
                ConfigurePaint(paint, textFormatAttributes);
                _canvas.DrawText(text.String, area.Left, area.Top, paint);
            }
        }

        public void Draw(string text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            using (var paint = new SKPaint())
            {
                ConfigurePaint(paint, textFormatAttributes);
                _canvas.DrawText(text, area.Left, area.Top, paint);
            }
        }

        internal SKPaint PaintForLayout([NotNull] ITextLayout textLayout)
        {
            var paint = new SKPaint();
            ConfigurePaint(paint, textLayout.Attributes.TextFormatAttributes);

            return paint;
        }

        internal void ConfigurePaint([NotNull] SKPaint paint, TextFormatAttributes attributes)
        {
            paint.TextSize = attributes.FontSize;
            paint.Typeface = SKTypeface.FromFamilyName(attributes.Font);
            paint.Color = _textColor.ToSKColor();
        }
    }
}

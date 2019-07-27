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
using PixCore.Text;

namespace PixDirectX.Rendering
{
    public class GdiTextSizeSizeProvider : ITextSizeProvider, IDisposable
    {
        private readonly Bitmap _dummy = new Bitmap(1, 1);

        public void Dispose()
        {
            _dummy.Dispose();
        }

        public SizeF CalculateTextSize(string text, Font font)
        {
            return CalculateTextSize(new AttributedText(text), font);
        }

        public SizeF CalculateTextSize(IAttributedText text, Font font)
        {
            using (var graphics = Graphics.FromImage(_dummy))
            {
                return graphics.MeasureString(text.String, font);
            }
        }

        public SizeF CalculateTextSize(IAttributedText text, string fontName, float size)
        {
            using (var graphics = Graphics.FromImage(_dummy))
            using (var font = new Font(fontName, size))
            {
                return graphics.MeasureString(text.String, font);
            }
        }
    }
}
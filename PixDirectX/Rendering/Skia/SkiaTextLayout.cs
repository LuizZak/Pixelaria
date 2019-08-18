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

using PixCore.Text;
using PixRendering;

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
            
        }

        public HitTestMetrics HitTestPoint(float x, float y, out bool isTrailingHit, out bool isInside)
        {
            isTrailingHit = false;
            isInside = false;
            return new HitTestMetrics(-1);
        }

        public HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit, out float x, out float y)
        {
            x = 0;
            y = 0;
            return new HitTestMetrics(-1);
        }
    }
}

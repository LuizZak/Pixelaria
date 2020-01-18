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
using PixRendering;
using SharpDX.Direct2D1;
using Bitmap = System.Drawing.Bitmap;

namespace PixDirectX.Rendering.DirectX
{
    public class DirectXBitmap : IManagedImageResource
    {
        internal RenderTarget renderTarget;
        internal SharpDX.Direct2D1.Bitmap bitmap;
        internal Bitmap original;

        public int Width => bitmap.PixelSize.Width;
        public int Height => bitmap.PixelSize.Height;
        public Size Size => new Size(Width, Height);

        public DirectXBitmap(SharpDX.Direct2D1.Bitmap bitmap, Bitmap original, RenderTarget renderTarget)
        {
            this.bitmap = bitmap;
            this.original = original;
            this.renderTarget = renderTarget;
        }

        public void Dispose()
        {
            bitmap.Dispose();
        }
    }
}
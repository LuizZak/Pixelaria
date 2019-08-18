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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixCore.Text;
using PixRendering;

namespace PixDirectX.Rendering.Skia
{
    public class SkiaRenderManager : IRenderManager, IDisposable
    {
        private readonly List<IRenderListener> _renderListeners = new List<IRenderListener>();
        public Color BackColor { get; set; }
        public IImageResourceManager ImageResources { get; }
        public ITextMetricsProvider TextMetricsProvider { get; }
        public ITextSizeProvider TextSizeProvider { get; }
        public IClippingRegion ClippingRegion { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize(IRenderLoopState state)
        {
            throw new NotImplementedException();
        }

        public void UpdateRenderingState(IRenderLoopState state, IClippingRegion clipping)
        {
            throw new NotImplementedException();
        }

        public void Render(IRenderLoopState renderLoopState, IClippingRegion clipping)
        {
            throw new NotImplementedException();
        }

        public void AddRenderListener(IRenderListener renderListener)
        {
            throw new NotImplementedException();
        }

        public void RemoveRenderListener(IRenderListener renderListener)
        {
            throw new NotImplementedException();
        }

        public void WithPreparedTextLayout(Color textColor, IAttributedText text, ref ITextLayout layout,
            TextLayoutAttributes attributes, Action<ITextLayout, ITextRenderer> perform)
        {
            throw new NotImplementedException();
        }

        public ITextLayout CreateTextLayout(IAttributedText text, TextLayoutAttributes attributes)
        {
            throw new NotImplementedException();
        }

    }
}

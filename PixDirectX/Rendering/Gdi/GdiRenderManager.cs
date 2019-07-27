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
using PixCore.Text;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiRenderManager : IRenderManager
    {
        private readonly List<IRenderListener> _renderListeners = new List<IRenderListener>();
        private readonly GdiImageResourceManager _imageResources;
        public Graphics Graphics { get; set; }

        public Color BackColor { get; set; }

        public IImageResourceManager ImageResources => _imageResources;

        public ITextMetricsProvider TextMetricsProvider { get; }

        public ITextSizeProvider TextSizeProvider { get; }

        public IClippingRegion ClippingRegion { get; set; }

        public GdiRenderManager(GdiImageResourceManager imageResources)
        {
            _imageResources = imageResources;
            TextMetricsProvider = new GdiTextMetricsProvider(() => Graphics);
        }

        public void Initialize(IRenderLoopState state)
        {
            Graphics = CastRenderLoopStateOrFail(state).Graphics;
        }

        public void UpdateRenderingState(IRenderLoopState state, IClippingRegion clipping)
        {
            Graphics = CastRenderLoopStateOrFail(state).Graphics;
            ClippingRegion = clipping;
        }

        public void Render(IRenderLoopState renderLoopState, IClippingRegion clipping)
        {
            Graphics = CastRenderLoopStateOrFail(renderLoopState).Graphics;
            ClippingRegion = clipping;

            // Clean background
            Graphics.Clear(BackColor);

            InvokeRenderListeners(renderLoopState);
        }

        private IRenderListenerParameters CreateRenderListenerParameters([NotNull] IRenderLoopState state)
        {
            var renderState = CastRenderLoopStateOrFail(state);

            var parameters = new RenderListenerParameters(ImageResources, ClippingRegion, state, this, TextMetricsProvider, new GdiRenderer(renderState.Graphics, _imageResources), new GdiTextRenderer(null));

            return parameters;
        }

        private void InvokeRenderListeners([NotNull] IRenderLoopState state)
        {
            var parameters = CreateRenderListenerParameters(state);

            foreach (var listener in _renderListeners)
            {
                listener.Render(parameters);
            }
        }

        public void AddRenderListener(IRenderListener renderListener)
        {
            _renderListeners.Add(renderListener);
        }

        public void RemoveRenderListener(IRenderListener renderListener)
        {
            _renderListeners.Remove(renderListener);
        }

        public void WithPreparedTextLayout(Color textColor, IAttributedText text, ref ITextLayout layout, TextLayoutAttributes attributes, Action<ITextLayout, ITextRenderer> perform)
        {
            if(!(layout is GdiTextLayout))
            {
                layout = new GdiTextLayout(attributes, text);
            }

            var textRenderer = new GdiTextRenderer(Graphics);
            perform(layout, textRenderer);
        }

        public ITextLayout CreateTextLayout(IAttributedText text, TextLayoutAttributes attributes)
        {
            return new GdiTextLayout(attributes, text);
        }

        private static GdiRenderLoopState CastRenderLoopStateOrFail([NotNull] IRenderLoopState state)
        {
            if (state is GdiRenderLoopState gdiRenderLoopState)
                return gdiRenderLoopState;

            throw new InvalidOperationException($"Expected a render loop state of type {typeof(GdiRenderLoopState)}");
        }
    }

    struct GdiRenderLoopState : IRenderLoopState
    {
        public Graphics Graphics { get; }
        public TimeSpan FrameRenderDeltaTime { get; }

        public GdiRenderLoopState(Graphics graphics, TimeSpan frameRenderDeltaTime)
        {
            Graphics = graphics;
            FrameRenderDeltaTime = frameRenderDeltaTime;
        }
    }
}
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
using PixDirectX.Rendering.DirectX;
using PixDirectX.Rendering.Gdi;
using PixRendering;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace PixDirectX.Rendering.Skia
{
    public class SkiaRenderManager : IRenderManager, IDisposable
    {
        private readonly List<IRenderListener> _renderListeners = new List<IRenderListener>();
        private readonly SkiaImageResources _imageResources;
        private IRenderLoopState _lastRenderingState;

        public SKCanvas Canvas { get; set; }
        public Color BackColor { get; set; }
        public IImageResourceManager ImageResources => _imageResources;
        public ITextMetricsProvider TextMetricsProvider => new GdiTextMetricsProvider();
        public ITextSizeProvider TextSizeProvider => new GdiTextSizeProvider();
        public IClippingRegion ClippingRegion { get; set; }

        public SkiaRenderManager(SkiaImageResources imageResources)
        {
            _imageResources = imageResources;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize(IRenderLoopState state)
        {
            _lastRenderingState = state;
            Canvas = CastRenderLoopStateOrFail(state).Canvas;
        }

        public void UpdateRenderingState(IRenderLoopState state, IClippingRegion clipping)
        {
            _lastRenderingState = state;
            Canvas = CastRenderLoopStateOrFail(state).Canvas;
            ClippingRegion = clipping;
        }

        public void Render(IRenderLoopState renderLoopState, IClippingRegion clipping)
        {
            _lastRenderingState = renderLoopState;
            var state = CastRenderLoopStateOrFail(renderLoopState);
            ClippingRegion = clipping;
            Canvas = state.Canvas;
            
            foreach (var rectangle in clipping.RedrawRegionRectangles(state.Size))
            {
                //Canvas.ClipRect(rectangle.ToSKRect());
            }

            Canvas.Clear(BackColor.ToSKColor());

            InvokeRenderListeners(renderLoopState);
        }

        private IRenderListenerParameters CreateRenderListenerParameters([NotNull] IRenderLoopState state)
        {
            var renderState = CastRenderLoopStateOrFail(state);

            var renderer = new SkiaRenderer(renderState.Canvas, _imageResources);
            var textRenderer = new SkiaTextRenderer(renderState.Canvas, Color.Black);
            var parameters = new RenderListenerParameters(ImageResources, ClippingRegion, state, this, TextMetricsProvider, renderer, textRenderer);

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
            bool inserted = false;

            // Use the render listener's rendering order value to figure out the correct insertion position
            for (int i = 0; i < _renderListeners.Count; i++)
            {
                var listener = _renderListeners[i];
                if (listener.RenderOrder > renderListener.RenderOrder)
                {
                    _renderListeners.Insert(i, renderListener);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                _renderListeners.Add(renderListener);
            }

            if (_lastRenderingState != null)
            {
                renderListener.RecreateState(_lastRenderingState);
            }
        }

        public void RemoveRenderListener(IRenderListener renderListener)
        {
            _renderListeners.Remove(renderListener);
        }

        public void WithPreparedTextLayout(Color textColor, IAttributedText text, ref ITextLayout layout, TextLayoutAttributes attributes, Action<ITextLayout, ITextRenderer> perform)
        {
            if (!(layout is SkiaTextLayout))
            {
                layout = new SkiaTextLayout(attributes, text);
            }

            var textRenderer = new SkiaTextRenderer(Canvas, textColor);
            perform(layout, textRenderer);
        }

        public ITextLayout CreateTextLayout(IAttributedText text, TextLayoutAttributes attributes)
        {
            return new SkiaTextLayout(attributes, text);
        }

        private static SkiaRenderLoopState CastRenderLoopStateOrFail([NotNull] IRenderLoopState state)
        {
            if (state is SkiaRenderLoopState renderLoopState)
                return renderLoopState;

            throw new InvalidOperationException($"Expected a render loop state of type {typeof(SkiaRenderLoopState)}");
        }
    }

    public struct SkiaRenderLoopState : IRenderLoopState
    {
        public SKCanvas Canvas { get; }
        public Size Size { get; }
        public TimeSpan FrameRenderDeltaTime { get; set; }

        public SkiaRenderLoopState(SKCanvas canvas, Size size, TimeSpan frameRenderDeltaTime)
        {
            Canvas = canvas;
            Size = size;
            FrameRenderDeltaTime = frameRenderDeltaTime;
        }
    }
}

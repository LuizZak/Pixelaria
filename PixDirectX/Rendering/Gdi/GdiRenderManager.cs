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
using System.Drawing.Drawing2D;
using JetBrains.Annotations;
using PixCore.Text;
using PixDirectX.Rendering.DirectX;
using PixRendering;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiRenderManager : IRenderManager, IDisposable
    {
        private readonly List<IRenderListener> _renderListeners = new List<IRenderListener>();
        private readonly GdiTextMetricsProvider _textMetricsProvider;
        private readonly GdiTextSizeProvider _textSizeProvider;
        private readonly GdiImageResourceManager _imageResources;
        private IRenderLoopState _lastRenderingState;
        public Graphics Graphics { get; set; }

        public Color BackColor { get; set; }

        public IImageResourceManager ImageResources => _imageResources;

        public ITextMetricsProvider TextMetricsProvider => _textMetricsProvider;

        public ITextSizeProvider TextSizeProvider => _textSizeProvider;

        public IClippingRegion ClippingRegion { get; set; }

        public GdiRenderManager(GdiImageResourceManager imageResources)
        {
            _imageResources = imageResources;
            _textMetricsProvider = new GdiTextMetricsProvider();
            _textSizeProvider = new GdiTextSizeProvider();
        }

        public void Dispose()
        {
            _textMetricsProvider?.Dispose();
            _textSizeProvider?.Dispose();
        }

        public void Initialize(IRenderLoopState state)
        {
            Graphics = CastRenderLoopStateOrFail(state).Graphics;
        }

        public void UpdateRenderingState(IRenderLoopState state, IClippingRegion clipping)
        {
            _lastRenderingState = state;

            Graphics = CastRenderLoopStateOrFail(state).Graphics;
            ClippingRegion = clipping;
        }

        public void Render(IRenderLoopState renderLoopState, IClippingRegion clipping)
        {
            _lastRenderingState = renderLoopState;

            var state = CastRenderLoopStateOrFail(renderLoopState);
            Graphics = state.Graphics;
            ClippingRegion = clipping;
            Graphics.CompositingMode = CompositingMode.SourceOver;
            Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Graphics.InterpolationMode = InterpolationMode.Bicubic;

            var redrawRegion = clipping.RedrawRegionRectangles(state.Size);

            foreach (var rect in redrawRegion)
            {
                Graphics.Clip.Intersect(rect);
            }

            // Clear background
            Graphics.Clear(BackColor);

            InvokeRenderListeners(renderLoopState);
        }

        private IRenderListenerParameters CreateRenderListenerParameters([NotNull] IRenderLoopState state)
        {
            var renderState = CastRenderLoopStateOrFail(state);

            var gdiRenderer = new GdiRenderer(renderState.Graphics, _imageResources);
            var gdiTextRenderer = new GdiTextRenderer(renderState.Graphics, Color.Black);
            var parameters = new RenderListenerParameters(ImageResources, ClippingRegion, state, this, TextMetricsProvider, gdiRenderer, gdiTextRenderer);

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

        #region IRenderListener handling

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

            if (Graphics != null)
            {
                renderListener.RecreateState(_lastRenderingState);
            }
        }

        public void RemoveRenderListener(IRenderListener renderListener)
        {
            _renderListeners.Remove(renderListener);
        }

        #endregion

        public void WithPreparedTextLayout(Color textColor, IAttributedText text, ref ITextLayout layout, TextLayoutAttributes attributes, Action<ITextLayout, ITextRenderer> perform)
        {
            if(!(layout is GdiTextLayout))
            {
                layout = new GdiTextLayout(attributes, text);
            }

            var textRenderer = new GdiTextRenderer(Graphics, textColor);
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

    public struct GdiRenderLoopState : IRenderLoopState
    {
        public Graphics Graphics { get; }
        public Size Size { get; }
        public TimeSpan FrameRenderDeltaTime { get; set; }

        public GdiRenderLoopState(Graphics graphics, Size size, TimeSpan frameRenderDeltaTime)
        {
            Graphics = graphics;
            Size = size;
            FrameRenderDeltaTime = frameRenderDeltaTime;
        }
    }
}
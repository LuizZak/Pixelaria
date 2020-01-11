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
using Blend2DCS;
using Blend2DCS.Geometry;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixDirectX.Rendering.DirectX;
using PixDirectX.Rendering.Gdi;
using PixDirectX.Utils;
using PixRendering;

namespace PixDirectX.Rendering.Blend2D
{
    public class Blend2DRenderManager : IRenderManager, IDisposable
    {
        private readonly List<IRenderListener> _renderListeners = new List<IRenderListener>();
        private readonly GdiTextMetricsProvider _textMetricsProvider;
        private readonly GdiTextSizeProvider _textSizeProvider;
        private readonly Blend2DImageResources _imageResources;
        private IRenderLoopState _lastRenderingState;
        public BLContext Context { get; set; }

        public Color BackColor { get; set; } = Color.Black;

        public IImageResourceManager ImageResources => _imageResources;

        public ITextMetricsProvider TextMetricsProvider => _textMetricsProvider;

        public ITextSizeProvider TextSizeProvider => _textSizeProvider;

        public IClippingRegion ClippingRegion { get; set; }

        public Blend2DRenderManager(Blend2DImageResources imageResources)
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
            Context = CastRenderLoopStateOrFail(state).Context;
        }

        public void UpdateRenderingState(IRenderLoopState state, IClippingRegion clipping)
        {
            _lastRenderingState = state;

            Context = CastRenderLoopStateOrFail(state).Context;
            ClippingRegion = clipping;
        }

        public void Render(IRenderLoopState renderLoopState, IClippingRegion clipping)
        {
            _lastRenderingState = renderLoopState;

            var state = CastRenderLoopStateOrFail(renderLoopState);
            Context = state.Context;
            ClippingRegion = clipping;

            var redrawRegion = clipping.RedrawRegionRectangles(state.Size);

            foreach (var rect in redrawRegion)
            {
                Context.ClipToRect(((AABB) rect).ToBLRect());
            }

            // Clear background
            Context.SetFillStyle(unchecked((uint) BackColor.ToArgb()));
            Context.FillRectangle(new BLRect(0, 0, state.Size.Width, state.Size.Height));

            InvokeRenderListeners(renderLoopState);
        }

        private IRenderListenerParameters CreateRenderListenerParameters([NotNull] IRenderLoopState state)
        {
            var renderState = CastRenderLoopStateOrFail(state);

            var gdiRenderer = new Blend2DRenderer(renderState.Context, _imageResources);
            var gdiTextRenderer = new GdiTextRenderer(null, Color.Black);
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

            if (Context != null)
            {
                renderListener.RecreateState(_lastRenderingState);
            }
        }

        public void RemoveRenderListener(IRenderListener renderListener)
        {
            _renderListeners.Remove(renderListener);
        }

        #endregion

        [CanBeNull]
        public ITextLayout CreateTextLayout(IAttributedText text, TextLayoutAttributes attributes)
        {
            return null;
        }

        private static Blend2DRenderLoopState CastRenderLoopStateOrFail([NotNull] IRenderLoopState state)
        {
            if (state is Blend2DRenderLoopState renderLoopState)
                return renderLoopState;

            throw new InvalidOperationException($"Expected a render loop state of type {typeof(Blend2DRenderLoopState)}");
        }
    }

    public struct Blend2DRenderLoopState : IRenderLoopState
    {
        public BLContext Context { get; }
        public Size Size { get; }
        public TimeSpan FrameRenderDeltaTime { get; set; }

        public Blend2DRenderLoopState(BLContext context, Size size, TimeSpan frameRenderDeltaTime)
        {
            Context = context;
            Size = size;
            FrameRenderDeltaTime = frameRenderDeltaTime;
        }
    }
}

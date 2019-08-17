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
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixDirectX.Utils;
using PixRendering;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.DirectWrite.Factory;
using HitTestMetrics = PixRendering.HitTestMetrics;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using TextRange = SharpDX.DirectWrite.TextRange;

namespace PixDirectX.Rendering.DirectX
{
    /// <inheritdoc cref="IRenderManager" />
    /// <summary>
    /// Base Direct2D renderer class that other packages may inherit from to provide custom rendering logic
    /// </summary>
    public class Direct2DRenderManager : IDisposable, IRenderManager, IDirect2DRenderingStateProvider
    {
        [CanBeNull]
        private IDirect2DRenderingState _lastRenderingState;
        private Direct2DRenderer _direct2DRenderer;
        private readonly D2DTextSizeProvider _textSizeProvider;

        private bool _isRefreshingState;

        protected readonly TextColorRenderer TextColorRenderer = new TextColorRenderer();
        protected readonly List<IRenderListener> RenderListeners = new List<IRenderListener>();
        
        private readonly D2DImageResources _imageResources;
        private readonly TextMetrics _textMetrics;

        [CanBeNull]
        protected virtual Factory directWriteFactory { get; } = new Factory();

        /// <inheritdoc />
        /// <summary>
        /// Control-space clip rectangle for current draw operation.
        /// </summary>
        public IClippingRegion ClippingRegion { get; set; }
        
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the background color that this <see cref="T:PixDirectX.Rendering.BaseDirect2DRenderer" /> uses to clear the display area
        /// </summary>
        public Color BackColor { get; set; } = Color.FromArgb(255, 25, 25, 25);

        public IImageResourceManager ImageResources => _imageResources;

        /// <inheritdoc />
        public ITextMetricsProvider TextMetricsProvider => _textMetrics;
        /// <inheritdoc />
        public ITextSizeProvider TextSizeProvider => _textSizeProvider;

        public Direct2DRenderManager()
        {
            _imageResources = new D2DImageResources();
            _textMetrics = new TextMetrics(this);
            _textSizeProvider = new D2DTextSizeProvider(this);
        }

        ~Direct2DRenderManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            
            TextColorRenderer.DefaultBrush?.Dispose();
            TextColorRenderer.Dispose();

            _imageResources.Dispose();
        }

        public virtual void Initialize(IRenderLoopState renderLoopState)
        {
            var state = (IDirect2DRenderingState)renderLoopState;

            _lastRenderingState = state;

            RecreateState(state);

            TextColorRenderer.AssignResources(state.D2DRenderTarget, new SolidColorBrush(state.D2DRenderTarget, Color4.White));
        }

        /// <summary>
        /// Invalidates this D2D renderer's state so it's re-created on the next call to <see cref="RecreateState"/>.
        /// </summary>
        public virtual void InvalidateState()
        {
            _direct2DRenderer?.Dispose();
            _direct2DRenderer = null;
            TextColorRenderer.DefaultBrush.Dispose();

            _isRefreshingState = true;
        }

        /// <summary>
        /// Called when the state has been invalidated and needs to be refreshed.
        /// </summary>
        protected virtual void RecreateState([NotNull] IDirect2DRenderingState state)
        {
            _direct2DRenderer = new Direct2DRenderer(state, _imageResources);

            foreach (var listener in RenderListeners)
            {
                listener.RecreateState(state);
            }
        }

        /// <summary>
        /// Updates the rendering state and clipping region of this Direct2D renderer instance to the ones specified.
        /// 
        /// Must be called whenever devices/surfaces/etc. have been invalidated or the clipping region has been changed.
        /// 
        /// Automatically called every time <see cref="Render"/> is called before any rendering occurs.
        /// 
        /// Calls <see cref="RecreateState"/> after calls to <see cref="InvalidateState"/> automatically.
        /// </summary>
        public void UpdateRenderingState(IRenderLoopState renderLoopState, IClippingRegion clipping)
        {
            var state = (IDirect2DRenderingState) renderLoopState;

            if (_isRefreshingState)
            {
                RecreateState(state);
                _isRefreshingState = false;
            }

            _lastRenderingState = state;
            
            // Update text renderer's references
            TextColorRenderer.DefaultBrush.Dispose();
            TextColorRenderer.AssignResources(state.D2DRenderTarget, new SolidColorBrush(state.D2DRenderTarget, Color4.White));
            
            ClippingRegion = clipping;
        }
        
        /// <summary>
        /// Renders all render listeners on this <see cref="Direct2DRenderManager"/> instance.
        ///
        /// If overriden, must be called to properly update the render state of the renderer.
        /// </summary>
        public virtual void Render(IRenderLoopState renderLoopState, IClippingRegion clipping)
        {
            var state = (IDirect2DRenderingState)renderLoopState;

            UpdateRenderingState(state, clipping);

            // Clean background
            state.D2DRenderTarget.Clear(BackColor.ToColor4());

            InvokeRenderListeners(state);
        }

        public IDirect2DRenderingState GetLatestValidRenderingState()
        {
            return _lastRenderingState;
        }

        public ITextLayout CreateTextLayout(IAttributedText text, TextLayoutAttributes attributes)
        {
            if (directWriteFactory == null)
                throw new InvalidOperationException("Direct2D renderer has no previous rendering state to derive a DirectWrite factory from.");

            var horizontalAlign =
                Direct2DConversionHelpers.DirectWriteAlignmentFor(attributes.TextFormatAttributes.HorizontalTextAlignment);
            var verticalAlign =
                Direct2DConversionHelpers.DirectWriteAlignmentFor(attributes.TextFormatAttributes.VerticalTextAlignment);
            var wordWrap =
                Direct2DConversionHelpers.DirectWriteWordWrapFor(attributes.TextFormatAttributes.WordWrap);

            var textFormat = new TextFormat(directWriteFactory, attributes.TextFormatAttributes.Font, attributes.TextFormatAttributes.FontSize)
            {
                TextAlignment = horizontalAlign,
                ParagraphAlignment = verticalAlign,
                WordWrapping = wordWrap
            };

            EllipsisTrimming ellipsisTrimming = null;

            if (attributes.TextFormatAttributes.TextEllipsisTrimming.HasValue)
            {
                var trimming = CreateTrimming(attributes.TextFormatAttributes.TextEllipsisTrimming.Value);
                ellipsisTrimming = new EllipsisTrimming(directWriteFactory, textFormat);
                textFormat.SetTrimming(trimming, ellipsisTrimming);
            }

            var textLayout = new TextLayout(directWriteFactory, text.String, textFormat,
                attributes.AvailableWidth, attributes.AvailableHeight);

            return new InnerTextLayout(textLayout, text, ellipsisTrimming, attributes);
        }

        private static Trimming CreateTrimming(TextEllipsisTrimming ellipsis)
        {
            var trimming = new Trimming
            {
                Granularity = Direct2DConversionHelpers.DirectWriteGranularityFor(ellipsis.Granularity),
                Delimiter = ellipsis.Delimiter,
                DelimiterCount = ellipsis.DelimiterCount
            };

            return trimming;
        }

        public void WithPreparedTextLayout(Color textColor, IAttributedText text, ref ITextLayout layout, TextLayoutAttributes attributes, Action<ITextLayout, ITextRenderer> perform)
        {
            if (!(layout is InnerTextLayout))
            {
                layout?.Dispose();
                layout = CreateTextLayout(text, attributes);
            }

            if (_lastRenderingState == null)
                throw new InvalidOperationException("Direct2D renderer has no previous rendering state to base this call on.");

            if (!(layout is InnerTextLayout innerLayout))
            {
                throw new InvalidOperationException($"Expected a text layout of type {typeof(InnerTextLayout)}");
            }

            using (var brush = new SolidColorBrush(_lastRenderingState.D2DRenderTarget, textColor.ToColor4()))
            {
                var disposes = new List<IDisposable>();
                
                // Apply text attributes
                foreach (var textSegment in text.GetTextSegments())
                {
                    if (textSegment.HasAttribute<ForegroundColorAttribute>())
                    {
                        var colorAttr = textSegment.GetAttribute<ForegroundColorAttribute>();

                        var segmentBrush =
                            new SolidColorBrush(_lastRenderingState.D2DRenderTarget,
                                colorAttr.ForeColor.ToColor4());

                        disposes.Add(segmentBrush);

                        innerLayout.TextLayout.SetDrawingEffect(segmentBrush,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    }
                    if (textSegment.HasAttribute<TextFontAttribute>())
                    {
                        var fontAttr = textSegment.GetAttribute<TextFontAttribute>();

                        innerLayout.TextLayout.SetFontFamilyName(fontAttr.Font.FontFamily.Name,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                        innerLayout.TextLayout.SetFontSize(fontAttr.Font.Size,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    }
                }

                var prev = TextColorRenderer.DefaultBrush;
                TextColorRenderer.DefaultBrush = brush;

                perform(layout, new InnerTextRenderer(TextColorRenderer, _lastRenderingState.DirectWriteFactory, _lastRenderingState.D2DRenderTarget));

                TextColorRenderer.DefaultBrush = prev;

                foreach (var disposable in disposes)
                {
                    disposable.Dispose();
                }
            }
        }

        #region IRenderListener invoking

        public IRenderListenerParameters CreateRenderListenerParameters([NotNull] IDirect2DRenderingState state)
        {
            var parameters = new RenderListenerParameters(ImageResources, ClippingRegion, state, this,
                TextMetricsProvider, _direct2DRenderer,
                new InnerTextRenderer(TextColorRenderer, state.DirectWriteFactory, state.D2DRenderTarget));

            return parameters;
        }

        protected void InvokeRenderListeners([NotNull] IDirect2DRenderingState state)
        {
            var parameters = CreateRenderListenerParameters(state);

            foreach (var listener in RenderListeners)
            {
                listener.Render(parameters);
            }
        }
        
        #endregion

        #region IRenderListener handling

        public void AddRenderListener(IRenderListener renderListener)
        {
            bool inserted = false;

            // Use the render listener's rendering order value to figure out the correct insertion position
            for (int i = 0; i < RenderListeners.Count; i++)
            {
                var listener = RenderListeners[i];
                if (listener.RenderOrder > renderListener.RenderOrder)
                {
                    RenderListeners.Insert(i, renderListener);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                RenderListeners.Add(renderListener);
            }

            if (_lastRenderingState != null)
            {
                renderListener.RecreateState(_lastRenderingState);
            }
        }

        public void RemoveRenderListener(IRenderListener renderListener)
        {
            RenderListeners.Remove(renderListener);
        }

        #endregion

        #region Static helpers

        [MustUseReturnValue]
        public static unsafe SharpDX.Direct2D1.Bitmap CreateSharpDxBitmap([NotNull] RenderTarget renderTarget, [NotNull] Bitmap bitmap)
        {
            var bitmapProperties =
                new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));

            var size = new Size2(bitmap.Width, bitmap.Height);

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
            {
                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                var data = (byte*)bitmapData.Scan0;

                Debug.Assert(data != null, "data != null");

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte b = data[offset++];
                        byte g = data[offset++];
                        byte r = data[offset++];
                        byte a = data[offset++];
                        int rgba = r | (g << 8) | (b << 16) | (a << 24);
                        tempStream.Write(rgba);
                    }
                }
                bitmap.UnlockBits(bitmapData);
                tempStream.Position = 0;

                return new SharpDX.Direct2D1.Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
            }
        }

        [MustUseReturnValue]
        public static SharpDX.Direct2D1.Bitmap CreateSharpDxBitmap([NotNull] RenderTarget renderTarget, [NotNull] SharpDX.WIC.Bitmap bitmap)
        {
            return SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, bitmap);
        }

        #endregion

        private class TextMetrics : ITextMetricsProvider
        {
            private readonly IDirect2DRenderingStateProvider _renderer;

            public TextMetrics(IDirect2DRenderingStateProvider renderer)
            {
                _renderer = renderer;
            }

            public AABB LocationOfCharacter(int offset, IAttributedText text, TextLayoutAttributes textLayoutAttributes)
            {
                var renderState = _renderer.GetLatestValidRenderingState();
                if (renderState == null)
                    return AABB.Empty;

                return
                    WithTemporaryTextFormat(renderState, text, textLayoutAttributes, (format, layout) =>
                    {
                        var metric = layout.HitTestTextPosition(offset, false, out float _, out float _);

                        return AABB.FromRectangle(metric.Left, float.IsInfinity(metric.Top) ? 0 : metric.Top, metric.Width, metric.Height);
                    });
            }

            public AABB[] LocationOfCharacters(int offset, int length, IAttributedText text, TextLayoutAttributes textLayoutAttributes)
            {
                var renderState = _renderer.GetLatestValidRenderingState();
                if (renderState == null)
                    return new AABB[0];

                return
                    WithTemporaryTextFormat(renderState, text, textLayoutAttributes, (format, layout) =>
                    {
                        var metrics = layout.HitTestTextRange(offset, length, 0, 0);
                        return metrics
                            .Select(range => AABB.FromRectangle(range.Left, range.Top, range.Width, range.Height))
                            .ToArray();
                    });
            }

            private static T WithTemporaryTextFormat<T>([NotNull] IDirect2DRenderingState renderState, [NotNull] IAttributedText text, TextLayoutAttributes textLayoutAttributes,
                [NotNull] Func<TextFormat, TextLayout, T> action)
            {
                var format = new TextFormat(renderState.DirectWriteFactory, textLayoutAttributes.TextFormatAttributes.Font,
                    textLayoutAttributes.TextFormatAttributes.FontSize)
                {
                    TextAlignment =
                        Direct2DConversionHelpers.DirectWriteAlignmentFor(textLayoutAttributes.TextFormatAttributes.HorizontalTextAlignment),
                    ParagraphAlignment =
                        Direct2DConversionHelpers.DirectWriteAlignmentFor(textLayoutAttributes.TextFormatAttributes.VerticalTextAlignment),
                    WordWrapping = Direct2DConversionHelpers.DirectWriteWordWrapFor(textLayoutAttributes.TextFormatAttributes.WordWrap)
                };

                using (var textFormat = format)
                using (var textLayout = new TextLayout(renderState.DirectWriteFactory, text.String, textFormat, textLayoutAttributes.AvailableWidth, textLayoutAttributes.AvailableHeight))
                {
                    foreach (var textSegment in text.GetTextSegments())
                    {
                        if (!textSegment.HasAttribute<TextFontAttribute>())
                            continue;

                        var fontAttr = textSegment.GetAttribute<TextFontAttribute>();

                        textLayout.SetFontFamilyName(fontAttr.Font.FontFamily.Name,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                        textLayout.SetFontSize(fontAttr.Font.Size,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    }

                    return action(textFormat, textLayout);
                }
            }
        }

        private class InnerTextLayout : ITextLayout
        {
            public TextLayout TextLayout { get; }
            [CanBeNull]
            public EllipsisTrimming EllipsisTrimming { get; }
            public TextLayoutAttributes Attributes { get; }
            public IAttributedText Text { get; }

            public InnerTextLayout(TextLayout textLayout, IAttributedText text, EllipsisTrimming ellipsisTrimming, TextLayoutAttributes attributes)
            {
                TextLayout = textLayout;
                EllipsisTrimming = ellipsisTrimming;
                Attributes = attributes;
                Text = text;
            }

            public void Dispose()
            {
                TextLayout?.Dispose();
                EllipsisTrimming?.Dispose();
            }

            public HitTestMetrics HitTestPoint(float x, float y, out bool isTrailingHit, out bool isInside)
            {
                var metrics = TextLayout.HitTestPoint(x, y, out var trailing, out var inside);
                isTrailingHit = trailing;
                isInside = inside;

                return MetricsFromDirectWrite(metrics);
            }

            public HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit, out float x, out float y)
            {
                var metrics = TextLayout.HitTestTextPosition(textPosition, isTrailingHit, out x, out y);
                return MetricsFromDirectWrite(metrics);
            }

            private static HitTestMetrics MetricsFromDirectWrite(SharpDX.DirectWrite.HitTestMetrics metrics)
            {
                return new HitTestMetrics(metrics.TextPosition);
            }
        }

        private class InnerTextRenderer : ITextRenderer
        {
            public TextColorRenderer TextColorRenderer { get; }
            private Factory directWriteFactory;
            private RenderTarget renderTarget;

            public InnerTextRenderer(TextColorRenderer textColorRenderer, Factory directWriteFactory, RenderTarget renderTarget)
            {
                TextColorRenderer = textColorRenderer;
                this.directWriteFactory = directWriteFactory;
                this.renderTarget = renderTarget;
            }

            public void Draw(ITextLayout textLayout, float x, float y)
            {
                if (!(textLayout is InnerTextLayout layout))
                    return;

                layout.TextLayout.Draw(TextColorRenderer, x, y);
            }

            public void Draw(IAttributedText text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
            {
                EllipsisTrimming trimming = null;

                using (var brush = new SolidColorBrush(renderTarget, color.ToColor4()))
                using (var textFormat = TextFormatForAttributes(textFormatAttributes, ref trimming))
                using (var textLayout = new TextLayout(directWriteFactory, text.String, textFormat, area.Width, area.Height))
                {
                    var disposes = new List<IDisposable>();

                    // Apply text attributes
                    foreach (var textSegment in text.GetTextSegments())
                    {
                        var consumer = new CollectingTextAttributeConsumer();
                        textSegment.ConsumeAttributes(consumer);

                        if (consumer.ForeColor.HasValue)
                        {
                            var segmentBrush =
                                new SolidColorBrush(renderTarget, consumer.ForeColor.Value.ToColor4());

                            disposes.Add(segmentBrush);

                            textLayout.SetDrawingEffect(segmentBrush,
                                new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                        }
                        if (consumer.Font != null)
                        {
                            textLayout.SetFontFamilyName(consumer.Font.FontFamily.Name,
                                new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                            textLayout.SetFontSize(consumer.Font.Size,
                                new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                        }
                    }

                    var prev = TextColorRenderer.DefaultBrush;
                    TextColorRenderer.DefaultBrush = brush;

                    textLayout.Draw(TextColorRenderer, area.Left, area.Top);

                    TextColorRenderer.DefaultBrush = prev;

                    foreach (var disposable in disposes)
                    {
                        disposable.Dispose();
                    }
                }
            }

            public void Draw(string text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
            {
                EllipsisTrimming trimming = null;

                using (var foreground = new SolidColorBrush(renderTarget, color.ToColor4()))
                using (var textFormat = TextFormatForAttributes(textFormatAttributes, ref trimming))
                {
                    renderTarget.DrawText(text, textFormat, area.ToRawRectangleF(), foreground);
                }

                trimming?.Dispose();
            }

            private TextFormat TextFormatForAttributes(TextFormatAttributes textFormatAttributes, ref EllipsisTrimming trimming)
            {
                var textFormat = new TextFormat(directWriteFactory, textFormatAttributes.Font, textFormatAttributes.FontSize)
                {
                    WordWrapping = WordWrapping.NoWrap
                };

                if (textFormatAttributes.TextEllipsisTrimming.HasValue)
                {
                    trimming = new EllipsisTrimming(directWriteFactory, textFormat);
                    textFormat.SetTrimming(CreateTrimming(textFormatAttributes.TextEllipsisTrimming.Value), trimming);
                }

                return textFormat;
            }
        }
    }

    public struct RenderListenerParameters : IRenderListenerParameters
    {
        public IImageResourceManager ImageResources { get; }
        public IClippingRegion ClippingRegion { get; }
        public IRenderer Renderer { get; }
        public IRenderLoopState State { get; }
        public ITextLayoutRenderer TextLayoutRenderer { get; }
        public ITextRenderer TextRenderer { get; }
        public ITextMetricsProvider TextMetricsProvider { get; }

        public RenderListenerParameters([NotNull] IImageResourceManager imageResources,
            [NotNull] IClippingRegion clippingRegion, [NotNull] IRenderLoopState state,
            [NotNull] ITextLayoutRenderer textLayoutRenderer,
            [NotNull] ITextMetricsProvider textMetricsProvider, [NotNull] IRenderer renderer, [NotNull] ITextRenderer textRenderer)
        {
            ImageResources = imageResources;
            ClippingRegion = clippingRegion;
            State = state;
            TextLayoutRenderer = textLayoutRenderer;
            TextMetricsProvider = textMetricsProvider;
            Renderer = renderer;
            TextRenderer = textRenderer;
        }
    }

    /// <summary>
    /// Interface for objects capable of providing a Direct2D rendering state on request.
    /// </summary>
    public interface IDirect2DRenderingStateProvider
    {
        /// <summary>
        /// Asks the object to return the latest available valid rendering state to the caller.
        /// 
        /// May return null in cases such as a rendering state not being available.
        /// </summary>
        [CanBeNull]
        IDirect2DRenderingState GetLatestValidRenderingState();
    }

    /// <summary>
    /// Basic implementation of <see cref="IDirect2DRenderingStateProvider"/> that always returns the same
    /// value specified at construction
    /// </summary>
    public class StaticDirect2DRenderingStateProvider : IDirect2DRenderingStateProvider
    {
        private readonly IDirect2DRenderingState _state;

        public StaticDirect2DRenderingStateProvider(IDirect2DRenderingState state)
        {
            _state = state;
        }

        public IDirect2DRenderingState GetLatestValidRenderingState()
        {
            return _state;
        }
    }
}
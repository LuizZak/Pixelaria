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
using System.Diagnostics.Contracts;
using System.Drawing.Imaging;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixDirectX.Rendering.Gdi;
using PixDirectX.Utils;
using PixRendering;

using PixVector = PixCore.Geometry.Vector;

using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using AlphaMode = Vortice.DCommon.AlphaMode;
using Brush = Vortice.Direct2D1.ID2D1Brush;
using Factory = Vortice.DirectWrite.IDWriteFactory;
using HitTestMetrics = PixRendering.HitTestMetrics;
using PixelFormat = Vortice.DCommon.PixelFormat;
using RectangleF = System.Drawing.RectangleF;
using TextRange = Vortice.DirectWrite.TextRange;
using TextHitTestMetrics = Vortice.DirectWrite.HitTestMetrics;

using Vortice.DirectWrite;
using Vortice.Mathematics;
using System.Numerics;
using Vortice.Direct2D1;

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
        private WrappedDirect2DRenderer _wrappedDirect2D;
        private readonly D2DTextSizeProvider _textSizeProvider;

        private bool _isRefreshingState;

        protected readonly TextColorRenderer TextColorRenderer = new TextColorRenderer();
        protected readonly List<IRenderListener> RenderListeners = new List<IRenderListener>();
        
        private readonly ImageResources _imageResources;
        private readonly TextMetrics _textMetrics;

        [CanBeNull]
        protected virtual Factory directWriteFactory { get; } = DWrite.DWriteCreateFactory<Factory>();

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
            _imageResources = new ImageResources(this);
            _textMetrics = new TextMetrics(this);
            _textSizeProvider = new D2DTextSizeProvider();
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

            TextColorRenderer.AssignResources(state.D2DRenderTarget, state.D2DRenderTarget.CreateSolidColorBrush(Colors.White));
        }

        /// <summary>
        /// Invalidates this D2D renderer's state so it's re-created on the next call to <see cref="RecreateState"/>.
        /// </summary>
        public virtual void InvalidateState()
        {
            _wrappedDirect2D?.Dispose();
            _wrappedDirect2D = null;
            TextColorRenderer.DefaultBrush.Dispose();

            _isRefreshingState = true;
        }

        /// <summary>
        /// Called when the state has been invalidated and needs to be refreshed.
        /// </summary>
        protected virtual void RecreateState([NotNull] IDirect2DRenderingState state)
        {
            _wrappedDirect2D = new WrappedDirect2DRenderer(state, _imageResources);

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
            TextColorRenderer.AssignResources(state.D2DRenderTarget, state.D2DRenderTarget.CreateSolidColorBrush(Colors.White));
            
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

        public ITextLayout CreateTextLayout(AttributedText text, TextLayoutAttributes attributes)
        {
            if (directWriteFactory == null)
                throw new InvalidOperationException("Direct2D renderer has no previous rendering state to derive a DirectWrite factory from.");

            var horizontalAlign =
                Direct2DConversionHelpers.DirectWriteAlignmentFor(attributes.TextFormatAttributes.HorizontalTextAlignment);
            var verticalAlign =
                Direct2DConversionHelpers.DirectWriteAlignmentFor(attributes.TextFormatAttributes.VerticalTextAlignment);
            var wordWrap =
                Direct2DConversionHelpers.DirectWriteWordWrapFor(attributes.TextFormatAttributes.WordWrap);

            var factory = DWrite.DWriteCreateFactory<Factory>();

            var textFormat = factory.CreateTextFormat(attributes.TextFormatAttributes.Font, attributes.TextFormatAttributes.FontSize);
            textFormat.TextAlignment = horizontalAlign;
            textFormat.ParagraphAlignment = verticalAlign;
            textFormat.WordWrapping = wordWrap;

            IDWriteInlineObject ellipsisTrimming = null;

            if (attributes.TextFormatAttributes.TextEllipsisTrimming.HasValue)
            {
                var trimming = CreateTrimming(attributes.TextFormatAttributes.TextEllipsisTrimming.Value);
                ellipsisTrimming = factory.CreateEllipsisTrimmingSign(textFormat);
                textFormat.SetTrimming(trimming, ellipsisTrimming);
            }

            var textLayout = directWriteFactory.CreateTextLayout(text.String, textFormat, attributes.AvailableWidth, attributes.AvailableHeight);

            return new InnerTextLayout(textLayout, text, ellipsisTrimming, attributes);
        }
        internal static Trimming CreateTrimming(TextEllipsisTrimming ellipsis)
        {
            var trimming = new Trimming
            {
                Granularity = Direct2DConversionHelpers.DirectWriteGranularityFor(ellipsis.Granularity),
                Delimiter = ellipsis.Delimiter,
                DelimiterCount = ellipsis.DelimiterCount
            };

            return trimming;
        }

        #region IRenderListener invoking

        public IRenderListenerParameters CreateRenderListenerParameters([NotNull] IDirect2DRenderingState state)
        {
            var parameters = new RenderListenerParameters(ImageResources, ClippingRegion, state, this,
                TextMetricsProvider, _wrappedDirect2D,
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
        public static unsafe ID2D1Bitmap CreateSharpDxBitmap([NotNull] ID2D1RenderTarget renderTarget, [NotNull] Bitmap bitmap)
        {
            var bitmapProperties =
                new BitmapProperties(new PixelFormat(Vortice.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));

            var size = new System.Drawing.Size(bitmap.Width, bitmap.Height);

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (var tempStream = new Vortice.DataStream(bitmap.Height * stride, true, true))
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

                return renderTarget.CreateBitmap(size, tempStream.BasePointer, stride, bitmapProperties);
            }
        }

        [MustUseReturnValue]
        public static ID2D1Bitmap CreateSharpDxBitmap([NotNull] ID2D1RenderTarget renderTarget, [NotNull] Vortice.WIC.IWICBitmap bitmap)
        {
            return renderTarget.CreateBitmapFromWicBitmap(bitmap);
        }

        #endregion

        private class TextMetrics : ITextMetricsProvider
        {
            private readonly IDirect2DRenderingStateProvider _renderer;

            public TextMetrics(IDirect2DRenderingStateProvider renderer)
            {
                _renderer = renderer;
            }

            public AABB LocationOfCharacter(int offset, AttributedText text, TextLayoutAttributes textLayoutAttributes)
            {
                var renderState = _renderer.GetLatestValidRenderingState();
                if (renderState == null)
                    return AABB.Empty;

                return
                    WithTemporaryTextFormat(renderState, text, textLayoutAttributes, (format, layout) =>
                    {
                        var metric = layout.HitTestTextPosition(offset, false, out Vector2 _);

                        return AABB.FromRectangle(metric.Left, float.IsInfinity(metric.Top) ? 0 : metric.Top, metric.Width, metric.Height);
                    });
            }

            public AABB[] LocationOfCharacters(int offset, int length, AttributedText text, TextLayoutAttributes textLayoutAttributes)
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

            private static T WithTemporaryTextFormat<T>([NotNull] IDirect2DRenderingState renderState, [NotNull] AttributedText text, TextLayoutAttributes textLayoutAttributes,
                [NotNull] Func<IDWriteTextFormat, IDWriteTextLayout, T> action)
            {
                var format = renderState.DirectWriteFactory.CreateTextFormat(
                    textLayoutAttributes.TextFormatAttributes.Font,
                    textLayoutAttributes.TextFormatAttributes.FontSize
                );
                format.TextAlignment = Direct2DConversionHelpers.DirectWriteAlignmentFor(textLayoutAttributes.TextFormatAttributes.HorizontalTextAlignment);
                format.ParagraphAlignment = Direct2DConversionHelpers.DirectWriteAlignmentFor(textLayoutAttributes.TextFormatAttributes.VerticalTextAlignment);
                format.WordWrapping = Direct2DConversionHelpers.DirectWriteWordWrapFor(textLayoutAttributes.TextFormatAttributes.WordWrap);

                using (var textFormat = format)
                using (var textLayout = renderState.DirectWriteFactory.CreateTextLayout(text.String, textFormat, textLayoutAttributes.AvailableWidth, textLayoutAttributes.AvailableHeight))
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

        internal class InnerTextLayout : ITextLayout
        {
            public IDWriteTextLayout TextLayout { get; }
            [CanBeNull]
            public IDWriteInlineObject EllipsisTrimming { get; }
            public TextLayoutAttributes Attributes { get; }
            public AttributedText Text { get; }

            public InnerTextLayout(IDWriteTextLayout textLayout, AttributedText text, IDWriteInlineObject ellipsisTrimming, TextLayoutAttributes attributes)
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
                var metrics = TextLayout.HitTestTextPosition(textPosition, isTrailingHit, out Vector2 point);
                x = point.X;
                y = point.Y;
                return MetricsFromDirectWrite(metrics);
            }

            private static HitTestMetrics MetricsFromDirectWrite(TextHitTestMetrics metrics)
            {
                return new HitTestMetrics(metrics.TextPosition);
            }
        }
    }

    public sealed class WrappedDirect2DRenderer : IRenderer, IDisposable
    {
        private InternalBrush _strokeBrush;
        private InternalBrush _fillBrush;

        private readonly IDirect2DRenderingState _state;
        private readonly ImageResources _imageResource;

        public Matrix2D Transform
        {
            get => _state.Transform.ToMatrix2D();
            set => _state.Transform = value.ToRawMatrix3X2();
        }

        public WrappedDirect2DRenderer([NotNull] IDirect2DRenderingState state, [NotNull] ImageResources imageResource)
        {
            _state = state;
            _imageResource = imageResource;
            _strokeBrush = new InternalSolidBrush(Color.Black);
            _fillBrush = new InternalSolidBrush(Color.Black);
        }

        public void Dispose()
        {
            _strokeBrush?.UnloadBrush();
            _fillBrush?.UnloadBrush();
        }

        private Brush BrushForStroke()
        {
            _strokeBrush.LoadBrush(_state.D2DRenderTarget);
            return _strokeBrush.Brush;
        }

        private Brush BrushForFill()
        {
            _fillBrush.LoadBrush(_state.D2DRenderTarget);
            return _fillBrush.Brush;
        }

        #region Stroke

        public void StrokeLine(PixVector start, PixVector end, float strokeWidth = 1)
        {
            _state.D2DRenderTarget.DrawLine(start.ToVector2(), end.ToVector2(), BrushForStroke(), strokeWidth);
        }

        public void StrokeCircle(PixVector center, float radius, float strokeWidth = 1)
        {
            StrokeEllipse(new AABB(center - new PixVector(radius) * 2, center + new PixVector(radius) * 2), strokeWidth);
        }

        public void StrokeEllipse(AABB ellipseArea, float strokeWidth = 1)
        {
            var ellipse = new Ellipse(ellipseArea.Center.ToVector2(), ellipseArea.Width / 2, ellipseArea.Height / 2);

            _state.D2DRenderTarget.DrawEllipse(ellipse, BrushForStroke(), strokeWidth);
        }

        public void StrokeRectangle(RectangleF rectangle, float strokeWidth = 1)
        {
            StrokeArea(new AABB(rectangle), strokeWidth);
        }

        public void StrokeArea(AABB area, float strokeWidth = 1)
        {
            _state.D2DRenderTarget.DrawRectangle(area.ToRawRectF(), BrushForStroke(), strokeWidth);
        }

        public void StrokeRoundedArea(AABB area, float radiusX, float radiusY, float strokeWidth = 1)
        {
            var roundedRect = new RoundedRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Rect = area.ToRawRectF()
            };

            _state.D2DRenderTarget.DrawRoundedRectangle(roundedRect, BrushForStroke(), strokeWidth);
        }

        public void StrokeGeometry(PolyGeometry geometry, float strokeWidth = 1)
        {
            using (var geom = _state.D2DFactory.CreatePathGeometry())
            {
                foreach (var polygon in geometry.Polygons())
                {
                    var sink = geom.Open();
                    sink.BeginFigure(polygon[0].ToVector2(), FigureBegin.Filled);
                    foreach (var vector in polygon.Skip(1))
                    {
                        sink.AddLine(vector.ToVector2());
                    }
                    sink.Close();
                }

                _state.D2DRenderTarget.DrawGeometry(geom, BrushForStroke(), strokeWidth);
            }
        }

        public void StrokePath(IPathGeometry path, float strokeWidth = 1)
        {
            var pathGeom = CastPathOrFail(path);

            _state.D2DRenderTarget.DrawGeometry(pathGeom.PathGeometry, BrushForStroke(), strokeWidth);
        }

        #endregion

        #region Fill

        public void FillCircle(PixVector center, float radius)
        {
            FillEllipse(new AABB(center - new PixVector(radius) * 2, center + new PixVector(radius) * 2));
        }

        public void FillEllipse(AABB ellipseArea)
        {
            var ellipse = new Ellipse(ellipseArea.Center.ToVector2(), ellipseArea.Width / 2, ellipseArea.Height / 2);

            _state.D2DRenderTarget.FillEllipse(ellipse, BrushForFill());
        }

        public void FillRectangle(RectangleF rectangle)
        {
            FillArea(new AABB(rectangle));
        }

        public void FillArea(AABB area)
        {
            _state.D2DRenderTarget.FillRectangle(area.ToRawRectF(), BrushForFill());
        }

        public void FillRoundedArea(AABB area, float radiusX, float radiusY)
        {
            var roundedRect = new RoundedRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Rect = area.ToRawRectF()
            };

            _state.D2DRenderTarget.FillRoundedRectangle(roundedRect, BrushForFill());
        }

        public void FillGeometry(PolyGeometry geometry)
        {
            using (var geom = _state.D2DFactory.CreatePathGeometry())
            {
                var sink = geom.Open();

                foreach (var polygon in geometry.Polygons())
                {
                    sink.BeginFigure(polygon[0].ToVector2(), FigureBegin.Filled);
                    foreach (var vector in polygon.Skip(1))
                    {
                        sink.AddLine(vector.ToVector2());
                    }
                    sink.EndFigure(FigureEnd.Closed);
                }

                sink.Close();

                _state.D2DRenderTarget.FillGeometry(geom, BrushForFill());
            }
        }

        public void FillPath(IPathGeometry path)
        {
            var pathGeom = CastPathOrFail(path);

            _state.D2DRenderTarget.FillGeometry(pathGeom.PathGeometry, BrushForFill());
        }

        #endregion

        #region Path Geometry

        /// <summary>
        /// Creates a path geometry by invoking path-drawing operations on an
        /// <see cref="IPathInputSink"/> provided within a closure.
        ///
        /// The path returned by this method can then be used in further rendering
        /// operations by this <see cref="IRenderer"/>.
        /// </summary>
        public IPathGeometry CreatePath(Action<IPathInputSink> execute)
        {
            var geom = _state.D2DFactory.CreatePathGeometry();

            var sink = geom.Open();

            var pathSink = new InternalPathSink(sink, FigureBegin.Filled);
            execute(pathSink);

            sink.Close();

            return new InternalPathGeometry(geom);
        }

        #endregion

        #region Bitmap

        public void DrawBitmap(ImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            var bitmap = _imageResource.BitmapForResource(image);
            Contract.Assert(bitmap != null, $"No bitmap found for image resource {image}. Make sure the bitmap is pre-loaded before using it.");

            DrawBitmap(bitmap, region, opacity, interpolationMode, tintColor);
        }

        public void DrawBitmap(ImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            DrawBitmap(image, (RectangleF)region, opacity, interpolationMode, tintColor);
        }

        public void DrawBitmap(IManagedImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            DrawBitmap(image, (RectangleF)region, opacity, interpolationMode, tintColor);
        }

        public void DrawBitmap(IManagedImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            var bitmap = CastBitmapOrFail(image);
            EnsureBitmapRenderTarget(bitmap);

            DrawBitmap(bitmap.bitmap, region, opacity, interpolationMode, tintColor);
        }

        private void DrawBitmap(ID2D1Bitmap bitmap, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            if (tintColor == null)
            {
                _state.D2DRenderTarget.DrawBitmap(bitmap, ((AABB)region).ToRawRectF(), opacity, ToBitmapInterpolation(interpolationMode), null);
            }
            else
            {
                using (var context = _state.D2DRenderTarget.QueryInterface<ID2D1DeviceContext>())
                using (var effect = new Vortice.Direct2D1.Effects.Tint(context))
                {
                    effect.SetInput(0, bitmap, true);

                    effect.Color = tintColor.Value.ToColor4();
                    effect.ClampOutput = true;

                    context.DrawImage(effect, ((AABB) region).Minimum.ToVector2(), ToInterpolation(interpolationMode));
                }
            }
        }

        private void EnsureBitmapRenderTarget([NotNull] DirectXBitmap bitmap)
        {
            if (bitmap.renderTarget == _state.D2DRenderTarget)
                return;

            Debug.WriteLine($"Attempted to render DirectXBitmap in a different RenderTarget w/ {nameof(WrappedDirect2DRenderer)}. Re-creating bitmap with current RenderTarget and continuing...");

            bitmap.renderTarget = _state.D2DRenderTarget;
            bitmap.bitmap.Dispose();
            bitmap.bitmap = Direct2DRenderManager.CreateSharpDxBitmap(_state.D2DRenderTarget, bitmap.original);
        }

        private static BitmapInterpolationMode ToBitmapInterpolation(ImageInterpolationMode imageInterpolationMode)
        {
            switch (imageInterpolationMode)
            {
                case ImageInterpolationMode.Linear:
                    return BitmapInterpolationMode.Linear;
                case ImageInterpolationMode.NearestNeighbor:
                    return BitmapInterpolationMode.NearestNeighbor;
                default:
                    return BitmapInterpolationMode.Linear;
            }
        }

        private static InterpolationMode ToInterpolation(ImageInterpolationMode imageInterpolationMode)
        {
            switch (imageInterpolationMode)
            {
                case ImageInterpolationMode.Linear:
                    return InterpolationMode.Linear;
                case ImageInterpolationMode.NearestNeighbor:
                    return InterpolationMode.NearestNeighbor;
                default:
                    return InterpolationMode.Linear;
            }
        }

        #endregion

        #region Clipping

        /// <summary>
        /// Pushes a clipping area where all further drawing operations will be constrained into.
        /// </summary>
        public void PushClippingArea(AABB area)
        {
            _state.D2DRenderTarget.PushAxisAlignedClip(area.ToRawRectF(), AntialiasMode.Aliased);
        }

        /// <summary>
        /// Pops the most recently pushed clipping area.
        /// </summary>
        public void PopClippingArea()
        {
            _state.D2DRenderTarget.PopAxisAlignedClip();
        }

        #endregion

        #region Transformation

        /// <summary>
        /// Pushes an Identity transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform()
        {
            _state.PushMatrix();
        }

        /// <summary>
        /// Pushes a 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform(Matrix2D matrix)
        {
            _state.PushMatrix(matrix.ToRawMatrix3X2());
        }

        /// <summary>
        /// Pops the top-most active transformation matrix.
        /// </summary>
        public void PopTransform()
        {
            _state.PopMatrix();
        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform(Matrix2D)"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Matrix2D matrix, Action execute)
        {
            PushTransform(matrix);
            execute();
            PopTransform();
        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform()"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Action execute)
        {
            PushTransform();
            execute();
            PopTransform();
        }

        #endregion

        #region Brush

        /// <summary>
        /// Sets the stroke color for this renderer
        /// </summary>
        public void SetStrokeColor(Color color)
        {
            _strokeBrush?.UnloadBrush();
            _strokeBrush = new InternalSolidBrush(color);
        }

        /// <summary>
        /// Sets the stroke brush for this renderer.
        /// </summary>
        public void SetStrokeBrush(IBrush brush)
        {
            if (brush == _strokeBrush)
                return;

            _strokeBrush?.UnloadBrush();
            _strokeBrush = CastBrushOrFail(brush);
        }

        /// <summary>
        /// Sets the fill color for this renderer
        /// </summary>
        public void SetFillColor(Color color)
        {
            _fillBrush?.UnloadBrush();
            _fillBrush = new InternalSolidBrush(color);
        }

        /// <summary>
        /// Sets the fill brush for this renderer.
        /// </summary>
        public void SetFillBrush(IBrush brush)
        {
            if (brush == _fillBrush)
                return;

            _fillBrush?.UnloadBrush();
            _fillBrush = CastBrushOrFail(brush);
        }

        /// <summary>
        /// Creates a linear gradient brush for drawing.
        /// </summary>
        public ILinearGradientBrush CreateLinearGradientBrush(IReadOnlyList<PixGradientStop> gradientStops, PixVector start, PixVector end)
        {
            return new InternalLinearBrush(gradientStops, start, end);
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        /// </summary>
        public IBrush CreateBitmapBrush(ImageResource image)
        {
            var bitmap = _imageResource.BitmapForResource(image);
            Contract.Assert(bitmap != null, $"No bitmap found for image resource {image}. Make sure the bitmap is pre-loaded before using it.");

            return new InternalBitmapBrush(bitmap);
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        /// </summary>
        public IBrush CreateBitmapBrush(IManagedImageResource image)
        {
            var bitmap = CastBitmapOrFail(image);
            return new InternalBitmapBrush(bitmap.bitmap);
        }

        #endregion

        #region Font and Text

        public IFontManager GetFontManager()
        {
            return new GdiFontManager();
        }

        public void DrawText(string text, IFont font, AABB area)
        {
            using (var textFormat = _state.DirectWriteFactory.CreateTextFormat(font.Name, font.FontSize))
            using (var textLayout = _state.DirectWriteFactory.CreateTextLayout(text, textFormat, area.Width, area.Height))
            {
                var renderer = new TextColorRenderer();
                renderer.AssignResources(_state.D2DRenderTarget, BrushForFill());

                textLayout.Draw(renderer, area.Left, area.Top);
            }
        }

        public void DrawAttributedText(AttributedText text, TextFormatAttributes attributes, AABB area)
        {
            var textRenderer = new TextColorRenderer();
            textRenderer.AssignResources(_state.D2DRenderTarget, BrushForFill());
            var renderer = new InnerTextRenderer(textRenderer, _state.DirectWriteFactory, _state.D2DRenderTarget)
            {
                Brush = BrushForFill()
            };

            renderer.Draw(text, attributes, area, Color.Black);
        }

        #endregion

        private static InternalBrush CastBrushOrFail([NotNull] IBrush brush)
        {
            if (brush is InternalBrush internalBrush)
                return internalBrush;

            throw new InvalidOperationException($"Expected a brush of type {typeof(InternalBrush)}");
        }

        private static InternalPathGeometry CastPathOrFail([NotNull] IPathGeometry path)
        {
            if (path is InternalPathGeometry internalPath)
                return internalPath;

            throw new InvalidOperationException($"Expected a path geometry of type {typeof(InternalPathGeometry)}");
        }

        private static DirectXBitmap CastBitmapOrFail([NotNull] IManagedImageResource bitmap)
        {
            if (bitmap is DirectXBitmap dxBitmap)
                return dxBitmap;

            throw new InvalidOperationException($"Expected a bitmap of type {typeof(DirectXBitmap)}");
        }

        private class InternalBrush : IBrush
        {
            internal bool IsLoaded { get; private set; }
            public Brush Brush { get; protected set; }

            public virtual void LoadBrush(ID2D1RenderTarget renderTarget)
            {
                IsLoaded = true;
            }

            public virtual void UnloadBrush()
            {
                if (!IsLoaded)
                    return;

                IsLoaded = false;
                Brush.Dispose();
            }
        }

        private class InternalSolidBrush : InternalBrush, ISolidBrush
        {
            public Color Color { get; }

            public InternalSolidBrush(Color color)
            {
                Color = color;
            }

            public override void LoadBrush(ID2D1RenderTarget renderTarget)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(renderTarget);

                Brush = renderTarget.CreateSolidColorBrush(Color.ToColor4());
            }
        }

        private class InternalLinearBrush : InternalBrush, ILinearGradientBrush
        {
            private ID2D1GradientStopCollection _stopCollection;
            public IReadOnlyList<PixGradientStop> GradientStops { get; }
            public PixVector Start { get; }
            public PixVector End { get; }

            public InternalLinearBrush([NotNull] IReadOnlyList<PixGradientStop> gradientStops, PixVector start, PixVector end)
            {
                GradientStops = gradientStops;
                Start = start;
                End = end;
            }

            public override void LoadBrush(ID2D1RenderTarget renderTarget)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(renderTarget);

                var stops = renderTarget.CreateGradientStopCollection(GradientStops.Select(ToGradientStop).ToArray());
                var properties = new LinearGradientBrushProperties
                {
                    StartPoint = Start.ToVector2(),
                    EndPoint = End.ToVector2()
                };

                Brush = renderTarget.CreateLinearGradientBrush(properties, stops);
                _stopCollection = stops;
            }

            public override void UnloadBrush()
            {
                if (!IsLoaded)
                    return;

                base.UnloadBrush();

                _stopCollection.Dispose();
            }

            private static GradientStop ToGradientStop(PixGradientStop stop)
            {
                return new GradientStop
                {
                    Color = stop.Color.ToColor4(),
                    Position = stop.Position
                };
            }
        }

        private class InternalBitmapBrush : InternalBrush
        {
            public ID2D1Bitmap Bitmap { get; }

            public InternalBitmapBrush(ID2D1Bitmap bitmap)
            {
                Bitmap = bitmap;
            }

            public override void LoadBrush(ID2D1RenderTarget renderTarget)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(renderTarget);

                var brush = renderTarget.CreateBitmapBrush(Bitmap);
                brush.ExtendModeX = ExtendMode.Wrap;
                brush.ExtendModeY = ExtendMode.Wrap;

                Brush = brush;
            }
        }

        private class InternalPathSink : IPathInputSink
        {
            private readonly ID2D1GeometrySink _geometrySink;
            private bool _startOfFigure = true;
            private PixVector _startLocation;
            private readonly FigureBegin _figureBegin;

            public InternalPathSink(ID2D1GeometrySink geometrySink, FigureBegin figureBegin)
            {
                _geometrySink = geometrySink;
                _figureBegin = figureBegin;
            }

            public void BeginFigure(PixVector location, bool filled)
            {
                _startOfFigure = false;
                _geometrySink.BeginFigure(location.ToVector2(), filled ? FigureBegin.Filled : FigureBegin.Hollow);
                _startLocation = location;
            }

            public void MoveTo(PixVector point)
            {
                if (!_startOfFigure)
                    _geometrySink.EndFigure(FigureEnd.Open);

                _startLocation = point;
                _startOfFigure = true;
            }

            public void LineTo(PixVector point)
            {
                EnsureBeginFigure();

                _geometrySink.AddLine(point.ToVector2());
                _startLocation = point;
            }

            public void MoveTo(float x, float y)
            {
                MoveTo(new PixVector(x, y));
            }

            public void LineTo(float x, float y)
            {
                LineTo(new PixVector(x, y));
            }

            public void BezierTo(PixVector anchor1, PixVector anchor2, PixVector endPoint)
            {
                EnsureBeginFigure();

                _geometrySink.AddBezier(new BezierSegment
                {
                    Point1 = anchor1.ToVector2(),
                    Point2 = anchor2.ToVector2(),
                    Point3 = endPoint.ToVector2(),
                });

                _startLocation = endPoint;
            }

            public void AddRectangle(AABB rectangle)
            {
                _geometrySink.AddLine(new PixVector(rectangle.Right, rectangle.Top).ToVector2());
                _geometrySink.AddLine(new PixVector(rectangle.Right, rectangle.Bottom).ToVector2());
                _geometrySink.AddLine(new PixVector(rectangle.Left, rectangle.Bottom).ToVector2());
            }

            public void EndFigure(bool closePath)
            {
                EndFigure(closePath ? FigureEnd.Closed : FigureEnd.Open);
            }

            private void EnsureBeginFigure()
            {
                if (!_startOfFigure)
                    return;

                _geometrySink.BeginFigure(_startLocation.ToVector2(), _figureBegin);
                _startOfFigure = false;
            }

            private void EndFigure(FigureEnd end)
            {
                if (_startOfFigure)
                    return;

                _geometrySink.EndFigure(end);
                _startOfFigure = true;
            }
        }

        private class InternalPathGeometry : IPathGeometry
        {
            public ID2D1PathGeometry PathGeometry { get; }

            public InternalPathGeometry(ID2D1PathGeometry pathGeometry)
            {
                PathGeometry = pathGeometry;
            }

            public void Dispose()
            {
                PathGeometry.Dispose();
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

    internal class InnerTextRenderer : ITextRenderer
    {
        public TextColorRenderer TextColorRenderer { get; }
        private readonly Factory _directWriteFactory;
        private readonly ID2D1RenderTarget _renderTarget;

        /// <summary>
        /// TODO: ITextRenderers should take in IBrush instances; for now, we set custom brushes this way here.
        /// </summary>
        public Brush Brush { get; set; }

        public InnerTextRenderer(TextColorRenderer textColorRenderer, Factory directWriteFactory, ID2D1RenderTarget renderTarget)
        {
            TextColorRenderer = textColorRenderer;
            _directWriteFactory = directWriteFactory;
            _renderTarget = renderTarget;
        }

        public void Draw(ITextLayout textLayout, float x, float y)
        {
            if (!(textLayout is Direct2DRenderManager.InnerTextLayout layout))
                return;

            // Render background segments
            var backSegments =
                textLayout.Text.GetTextSegments()
                    .Where(seg => seg.HasAttribute<BackgroundColorAttribute>());

            foreach (var segment in backSegments)
            {
                var attr = segment.GetAttribute<BackgroundColorAttribute>();

                var metrics = layout.TextLayout.HitTestTextRange(segment.TextRange.Start, segment.TextRange.Length, 0, 0);

                var bounds = metrics.Select(range => AABB.FromRectangle(range.Left, range.Top, range.Width, range.Height));

                using (var backBrush = _renderTarget.CreateSolidColorBrush(attr.BackColor.ToColor4()))
                {
                    foreach (var aabb in bounds)
                    {
                        _renderTarget.FillRectangle(aabb.Inflated(attr.Inflation).ToRawRectF(), backBrush);
                    }
                }
            }

            layout.TextLayout.Draw(TextColorRenderer, x, y);
        }

        public void Draw(AttributedText text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            IDWriteInlineObject trimming = null;

            using (var textFormat = TextFormatForAttributes(textFormatAttributes, ref trimming))
            using (var textLayout = _directWriteFactory.CreateTextLayout(text.String, textFormat, area.Width, area.Height))
            {
                var disposes = new List<IDisposable>();

                // Apply text attributes
                foreach (var textSegment in text.GetTextSegments())
                {
                    var consumer = new CollectingTextAttributeConsumer();
                    textSegment.ConsumeAttributes(consumer);

                    var textRange = new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length);

                    if (consumer.ForeColor.HasValue)
                    {
                        var segmentBrush = _renderTarget.CreateSolidColorBrush(consumer.ForeColor.Value.ToColor4());

                        disposes.Add(segmentBrush);

                        textLayout.SetDrawingEffect(segmentBrush, textRange);
                    }
                    if (consumer.Font != null)
                    {
                        ApplyFont(textLayout, consumer.Font, textRange);
                    }
                }

                // Render background segments
                var backSegments =
                    text.GetTextSegments()
                        .Where(seg => seg.HasAttribute<BackgroundColorAttribute>());

                foreach (var segment in backSegments)
                {
                    var attr = segment.GetAttribute<BackgroundColorAttribute>();

                    var metrics = textLayout.HitTestTextRange(segment.TextRange.Start, segment.TextRange.Length, 0, 0);

                    var bounds = metrics.Select(range => AABB.FromRectangle(range.Left, range.Top, range.Width, range.Height));

                    using (var backBrush = _renderTarget.CreateSolidColorBrush(attr.BackColor.ToColor4()))
                    {
                        foreach (var aabb in bounds)
                        {
                            _renderTarget.FillRectangle(aabb.Inflated(attr.Inflation).ToRawRectF(), backBrush);
                        }
                    }
                }

                var brush = Brush ?? _renderTarget.CreateSolidColorBrush(color.ToColor4());

                textLayout.Draw(brush.NativePointer, TextColorRenderer, area.Left, area.Top);

                foreach (var disposable in disposes)
                {
                    disposable.Dispose();
                }

                if (Brush == null)
                {
                    brush.Dispose();
                }
            }
        }

        // TODO: Reduce duplication with D2DTextSizeProvider

        private void ApplyFont(IDWriteTextLayout textLayout, System.Drawing.Font font, TextRange textRange)
        {
            textLayout.SetFontFamilyName(font.FontFamily.Name, textRange);
            textLayout.SetFontStyle(FontStyleFromSystemFontStyle(font.Style), textRange);
            textLayout.SetFontWeight(FontWeightFromFontStyle(font.Style), textRange);
            textLayout.SetFontSize(font.Size, textRange);

            if (font.Style.HasFlag(System.Drawing.FontStyle.Underline))
            {
                textLayout.SetUnderline(true, textRange);
            }
            if (font.Style.HasFlag(System.Drawing.FontStyle.Strikeout))
            {
                textLayout.SetStrikethrough(true, textRange);
            }
        }

        public void Draw(string text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            IDWriteInlineObject trimming = null;

            using (var foreground = _renderTarget.CreateSolidColorBrush(color.ToColor4()))
            using (var textFormat = TextFormatForAttributes(textFormatAttributes, ref trimming))
            {
                _renderTarget.DrawText(text, textFormat, area.ToRawRectF(), foreground);
            }

            trimming?.Dispose();
        }

        private IDWriteTextFormat TextFormatForAttributes(TextFormatAttributes textFormatAttributes, ref IDWriteInlineObject trimming)
        {
            var textFormat = _directWriteFactory.CreateTextFormat(textFormatAttributes.Font, textFormatAttributes.FontSize);

            textFormat.WordWrapping = Direct2DConversionHelpers.DirectWriteWordWrapFor(textFormatAttributes.WordWrap);
            textFormat.TextAlignment = Direct2DConversionHelpers.DirectWriteAlignmentFor(textFormatAttributes.HorizontalTextAlignment);
            textFormat.ParagraphAlignment = Direct2DConversionHelpers.DirectWriteAlignmentFor(textFormatAttributes.VerticalTextAlignment);

            if (textFormatAttributes.TextEllipsisTrimming.HasValue)
            {
                trimming = _directWriteFactory.CreateEllipsisTrimmingSign(textFormat);
                textFormat.SetTrimming(Direct2DRenderManager.CreateTrimming(textFormatAttributes.TextEllipsisTrimming.Value), trimming);
            }

            return textFormat;
        }

        private FontStyle FontStyleFromSystemFontStyle(System.Drawing.FontStyle fontStyle)
        {
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Italic))
                return FontStyle.Italic;

            return FontStyle.Normal;
        }

        private FontWeight FontWeightFromFontStyle(System.Drawing.FontStyle fontStyle)
        {
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Bold))
                return FontWeight.Bold;

            return FontWeight.Normal;
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
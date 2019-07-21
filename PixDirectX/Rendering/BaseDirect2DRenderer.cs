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
using PixDirectX.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using RectangleF = System.Drawing.RectangleF;
using TextRange = SharpDX.DirectWrite.TextRange;

namespace PixDirectX.Rendering
{
    /// <inheritdoc cref="IDirect2DRenderer" />
    /// <summary>
    /// Base Direct2D renderer class that other packages may inherit from to provide custom rendering logic
    /// </summary>
    public abstract class BaseDirect2DRenderer : IDisposable, IDirect2DRenderer, IDirect2DRenderingStateProvider
    {
        [CanBeNull]
        private IDirect2DRenderingState _lastRenderingState;

        private bool _isRefreshingState;

        protected readonly TextColorRenderer TextColorRenderer = new TextColorRenderer();
        protected readonly List<IRenderListener> RenderListeners = new List<IRenderListener>();
        
        private readonly D2DImageResources _imageResources;
        private readonly TextMetrics _textMetrics;
        
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

        public ID2DImageResourceManager ImageResources => _imageResources;

        /// <inheritdoc />
        public ITextMetricsProvider TextMetricsProvider => _textMetrics;
        
        protected BaseDirect2DRenderer()
        {
            _imageResources = new D2DImageResources();
            _textMetrics = new TextMetrics(this);
        }

        ~BaseDirect2DRenderer()
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

        public virtual void Initialize([NotNull] IDirect2DRenderingState state)
        {
            _lastRenderingState = state;

            RecreateState(state);

            TextColorRenderer.AssignResources(state.D2DRenderTarget, new SolidColorBrush(state.D2DRenderTarget, Color4.White));
        }

        /// <summary>
        /// Invalidates this D2D renderer's state so it's re-created on the next call to <see cref="RecreateState"/>.
        /// </summary>
        public virtual void InvalidateState()
        {
            TextColorRenderer.DefaultBrush.Dispose();

            _isRefreshingState = true;
        }

        /// <summary>
        /// Called when the state has been invalidated and needs to be refreshed.
        /// </summary>
        protected virtual void RecreateState([NotNull] IDirect2DRenderingState state)
        {
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
        public void UpdateRenderingState([NotNull] IDirect2DRenderingState state, [NotNull] IClippingRegion clipping)
        {
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
        /// Renders all render listeners on this <see cref="BaseDirect2DRenderer"/> instance.
        ///
        /// If overriden, must be called to properly update the render state of the renderer.
        /// </summary>
        public virtual void Render([NotNull] IDirect2DRenderingState state, [NotNull] IClippingRegion clipping)
        {
            UpdateRenderingState(state, clipping);

            // Clean background
            state.D2DRenderTarget.Clear(BackColor.ToColor4());

            InvokeRenderListeners(state);
        }

        public IDirect2DRenderingState GetLatestValidRenderingState()
        {
            return _lastRenderingState;
        }

        public void WithPreparedTextLayout(Color4 textColor, IAttributedText text, TextLayout layout, Action<TextLayout, TextRendererBase> perform)
        {
            if (_lastRenderingState == null)
                throw new InvalidOperationException("Direct2D renderer has no previous rendering state to base this call on.");

            using (var brush = new SolidColorBrush(_lastRenderingState.D2DRenderTarget, textColor))
            {
                var disposes = new List<IDisposable>();

                foreach (var textSegment in text.GetTextSegments())
                {
                    if (textSegment.HasAttribute<ForegroundColorAttribute>())
                    {
                        var colorAttr = textSegment.GetAttribute<ForegroundColorAttribute>();

                        var segmentBrush =
                            new SolidColorBrush(_lastRenderingState.D2DRenderTarget,
                                colorAttr.ForeColor.ToColor4());

                        disposes.Add(segmentBrush);

                        layout.SetDrawingEffect(segmentBrush,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    }
                    if (textSegment.HasAttribute<TextFontAttribute>())
                    {
                        var fontAttr = textSegment.GetAttribute<TextFontAttribute>();

                        layout.SetFontFamilyName(fontAttr.Font.FontFamily.Name,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                        layout.SetFontSize(fontAttr.Font.Size,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    }
                }

                var prev = TextColorRenderer.DefaultBrush;
                TextColorRenderer.DefaultBrush = brush;

                perform(layout, TextColorRenderer);

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
            var parameters = new RenderListenerParameters(ImageResources, ClippingRegion, state, TextColorRenderer, this, TextMetricsProvider);

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
                var format = new TextFormat(renderState.DirectWriteFactory, textLayoutAttributes.Font,
                    textLayoutAttributes.FontSize)
                {
                    TextAlignment =
                        Direct2DConversionHelpers.DirectWriteAlignmentFor(textLayoutAttributes.HorizontalTextAlignment),
                    ParagraphAlignment =
                        Direct2DConversionHelpers.DirectWriteAlignmentFor(textLayoutAttributes.VerticalTextAlignment),
                    WordWrapping = Direct2DConversionHelpers.DirectWriteWordWrapFor(textLayoutAttributes.WordWrap)
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
    }

    public sealed class WrappedDirect2DRenderer : IRenderer, IDisposable
    {
        private Brush _strokeBrush;
        private Brush _fillBrush;
        private Color _strokeColor;
        private Color _fillColor;

        private readonly IDirect2DRenderingState _state;
        private readonly ID2DImageResourceProvider _imageResource;

        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                RecreateFillBrush();
            }
        }

        public Color StrokeColor
        {
            get => _strokeColor;
            set
            {
                _strokeColor = value;
                RecreateStrokeBrush();
            }
        }

        public WrappedDirect2DRenderer([NotNull] IDirect2DRenderingState state, [NotNull] ID2DImageResourceProvider imageResource)
        {
            _state = state;
            _imageResource = imageResource;
            RecreateStrokeBrush();
            RecreateFillBrush();
        }

        public void Dispose()
        {
            _strokeBrush?.Dispose();
            _fillBrush?.Dispose();
        }

        private void RecreateStrokeBrush()
        {
            _strokeBrush?.Dispose();
            _strokeBrush = new SolidColorBrush(_state.D2DRenderTarget, StrokeColor.ToColor4());
        }

        private void RecreateFillBrush()
        {
            _fillBrush?.Dispose();
            _fillBrush = new SolidColorBrush(_state.D2DRenderTarget, FillColor.ToColor4());
        }

        private Brush BrushForStroke()
        {
            return _strokeBrush;
        }

        private Brush BrushForFill()
        {
            return _fillBrush;
        }

        #region Stroke

        public void StrokeLine(Vector start, Vector end)
        {
            _state.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), BrushForStroke());
        }

        public void StrokeCircle(Vector center, float radius)
        {
            StrokeEllipse(new AABB(center - new Vector(radius), center + new Vector(radius)));
        }

        public void StrokeEllipse(AABB ellipseArea)
        {
            var ellipse = new Ellipse(ellipseArea.Center.ToRawVector2(), ellipseArea.Width, ellipseArea.Height);

            _state.D2DRenderTarget.DrawEllipse(ellipse, _strokeBrush);
        }

        public void StrokeRectangle(RectangleF rectangle)
        {
            StrokeArea(new AABB(rectangle));
        }

        public void StrokeArea(AABB area)
        {
            _state.D2DRenderTarget.DrawRectangle(area.ToRawRectangleF(), BrushForStroke());
        }

        public void StrokeRoundedArea(AABB area, float radiusX, float radiusY)
        {
            var roundedRect = new RoundedRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Rect = area.ToRawRectangleF()
            };

            _state.D2DRenderTarget.DrawRoundedRectangle(roundedRect, BrushForStroke());
        }

        #endregion

        #region Fill

        public void FillCircle(Vector center, float radius)
        {
            FillEllipse(new AABB(center - new Vector(radius), center + new Vector(radius)));
        }

        public void FillEllipse(AABB ellipseArea)
        {
            var ellipse = new Ellipse(ellipseArea.Center.ToRawVector2(), ellipseArea.Width, ellipseArea.Height);

            _state.D2DRenderTarget.FillEllipse(ellipse, BrushForFill());
        }

        public void FillRectangle(RectangleF rectangle)
        {
            FillArea(new AABB(rectangle));
        }

        public void FillArea(AABB area)
        {
            _state.D2DRenderTarget.FillRectangle(area.ToRawRectangleF(), BrushForFill());
        }
        public void FillRoundedArea(AABB area, float radiusX, float radiusY)
        {
            var roundedRect = new RoundedRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Rect = area.ToRawRectangleF()
            };

            _state.D2DRenderTarget.FillRoundedRectangle(roundedRect, BrushForFill());
        }

        #endregion

        #region Bitmap

        public void DrawBitmap(ImageResource image, RectangleF region, float opacity, ImageInterpolation interpolation)
        {
            var bitmap = _imageResource.BitmapForResource(image);
            Contract.Assert(bitmap != null, $"No bitmap found for image resource {image}. Make sure the bitmap is pre-loaded before using it.");

            _state.D2DRenderTarget.DrawBitmap(bitmap, new AABB(region).ToRawRectangleF(), opacity, ToBitmapInterpolation(interpolation));
        }

        private static BitmapInterpolationMode ToBitmapInterpolation(ImageInterpolation imageInterpolation)
        {
            switch (imageInterpolation)
            {
                case ImageInterpolation.Linear:
                    return BitmapInterpolationMode.Linear;
                case ImageInterpolation.NearestNeighbor:
                    return BitmapInterpolationMode.NearestNeighbor;
                default:
                    return BitmapInterpolationMode.Linear;
            }
        }

        #endregion

        #region Clipping

        /// <summary>
        /// Pushes a clipping area where all further drawing operations will be constrained into.
        /// </summary>
        public void PushClippingArea(AABB area)
        {
            _state.D2DRenderTarget.PushAxisAlignedClip(area.ToRawRectangleF(), AntialiasMode.Aliased);
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

        #endregion
    }

    public struct RenderListenerParameters : IRenderListenerParameters
    {
        public ID2DImageResourceProvider ImageResources { get; }
        public IClippingRegion ClippingRegion { get; }
        public IDirect2DRenderingState State { get; }
        public TextColorRenderer TextColorRenderer { get; }
        public ITextLayoutRenderer TextLayoutRenderer { get; }
        public ITextMetricsProvider TextMetricsProvider { get; }

        public RenderListenerParameters([NotNull] ID2DImageResourceProvider imageResources,
            [NotNull] IClippingRegion clippingRegion, [NotNull] IDirect2DRenderingState state,
            [NotNull] TextColorRenderer textColorRenderer, [NotNull] ITextLayoutRenderer textLayoutRenderer,
            [NotNull] ITextMetricsProvider textMetricsProvider)
        {
            ImageResources = imageResources;
            ClippingRegion = clippingRegion;
            State = state;
            TextColorRenderer = textColorRenderer;
            TextLayoutRenderer = textLayoutRenderer;
            TextMetricsProvider = textMetricsProvider;
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
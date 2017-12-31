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
using System.Drawing.Imaging;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixDirectX.Rendering;
using PixUI;
using PixUI.Controls;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Rectangle = System.Drawing.Rectangle;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

using TextRange = SharpDX.DirectWrite.TextRange;

namespace PixUITests.TestUtils
{
    // TODO: Deal with duplication between this class and Pixelaria's Direct2DRenderer.

    /// <summary>
    /// Renders a pipeline export view
    /// </summary>
    internal class Direct2DRenderer : IDisposable, IDirect2DRenderer, ILabelViewSizeProvider
    {
        [CanBeNull]
        private IDirect2DRenderingState _lastRenderingState;

        private readonly TextColorRenderer _textColorRenderer = new TextColorRenderer();
        
        /// <summary>
        /// A small 32x32 box used to draw shadow boxes for labels.
        /// </summary>
        private SharpDX.Direct2D1.Bitmap _shadowBox;

        /// <summary>
        /// For rendering title of pipeline nodes
        /// </summary>
        private TextFormat _nodeTitlesTextFormat;
        
        private readonly D2DImageResources _imageResources;
        
        /// <summary>
        /// Control-space clip rectangle for current draw operation.
        /// </summary>
        public IClippingRegion ClippingRegion { get; set; }
        
        /// <summary>
        /// Gets or sets the background color that this <see cref="Direct2DRenderer"/> uses to clear the display area
        /// </summary>
        public Color BackColor { get; set; } = Color.FromArgb(255, 25, 25, 25);

        public ID2DImageResourceManager ImageResources => _imageResources;

        public ILabelViewTextMetricsProvider LabelViewTextMetricsProvider { get; }

        public Direct2DRenderer()
        {
            _imageResources = new D2DImageResources();
            
            LabelViewTextMetricsProvider = new TextMetrics(this);
        }

        ~Direct2DRenderer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _shadowBox?.Dispose();

            _nodeTitlesTextFormat?.Dispose();

            _textColorRenderer?.DefaultBrush?.Dispose();
            _textColorRenderer?.Dispose();

            _imageResources?.Dispose();
        }

        public void Initialize([NotNull] IDirect2DRenderingState state, [NotNull] IClippingRegion clipping)
        {
            _lastRenderingState = state;
            ClippingRegion = clipping;

            _textColorRenderer.AssignResources(state.D2DRenderTarget, new SolidColorBrush(state.D2DRenderTarget, Color4.White));

            _nodeTitlesTextFormat = new TextFormat(state.DirectWriteFactory, "Microsoft Sans Serif", 11)
            {
                TextAlignment = TextAlignment.Leading,
                ParagraphAlignment = ParagraphAlignment.Center
            };

            // Create shadow box image
            using (var bitmap = new Bitmap(32, 32))
            {
                _shadowBox = CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
            }
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

                var prev = _textColorRenderer.DefaultBrush;
                _textColorRenderer.DefaultBrush = brush;

                perform(layout, _textColorRenderer);

                _textColorRenderer.DefaultBrush = prev;

                foreach (var disposable in disposes)
                {
                    disposable.Dispose();
                }
            }
        }
        
        #region LabelView Size Provider

        public SizeF CalculateTextSize(LabelView labelView)
        {
            return CalculateTextSize(labelView.AttributedText, labelView.TextFont);
        }

        public SizeF CalculateTextSize(string text, Font font)
        {
            return CalculateTextSize(new AttributedText(text), font);
        }

        public SizeF CalculateTextSize(IAttributedText text, Font font)
        {
            return CalculateTextSize(text, font.Name, font.Size);
        }

        public SizeF CalculateTextSize(IAttributedText text, string font, float fontSize)
        {
            var renderState = _lastRenderingState;
            if (renderState == null)
                return SizeF.Empty;

            using (var textFormat = new TextFormat(renderState.DirectWriteFactory, font, fontSize) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Center, WordWrapping = WordWrapping.WholeWord })
            using (var textLayout = new TextLayout(renderState.DirectWriteFactory, text.String, textFormat, float.PositiveInfinity, float.PositiveInfinity))
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

                return new SizeF(textLayout.Metrics.Width, textLayout.Metrics.Height);
            }
        }

        #endregion
        
        #region Static helpers

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

        public static TextAlignment DirectWriteAlignmentFor(HorizontalTextAlignment alignment)
        {
            TextAlignment horizontalAlign;

            switch (alignment)
            {
                case HorizontalTextAlignment.Leading:
                    horizontalAlign = TextAlignment.Leading;
                    break;
                case HorizontalTextAlignment.Center:
                    horizontalAlign = TextAlignment.Center;
                    break;
                case HorizontalTextAlignment.Trailing:
                    horizontalAlign = TextAlignment.Trailing;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return horizontalAlign;
        }

        public static ParagraphAlignment DirectWriteAlignmentFor(VerticalTextAlignment alignment)
        {
            ParagraphAlignment verticalAlign;

            switch (alignment)
            {
                case VerticalTextAlignment.Near:
                    verticalAlign = ParagraphAlignment.Near;
                    break;
                case VerticalTextAlignment.Center:
                    verticalAlign = ParagraphAlignment.Center;
                    break;
                case VerticalTextAlignment.Far:
                    verticalAlign = ParagraphAlignment.Far;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return verticalAlign;
        }

        public static WordWrapping DirectWriteWordWrapFor(TextWordWrap wordWrap)
        {
            WordWrapping verticalAlign;

            switch (wordWrap)
            {
                case TextWordWrap.None:
                    verticalAlign = WordWrapping.NoWrap;
                    break;
                case TextWordWrap.ByCharacter:
                    verticalAlign = WordWrapping.Character;
                    break;
                case TextWordWrap.ByWord:
                    verticalAlign = WordWrapping.Wrap;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return verticalAlign;
        }

        #endregion

        private class TextMetrics : ILabelViewTextMetricsProvider
        {
            private readonly Direct2DRenderer _renderer;

            public TextMetrics(Direct2DRenderer renderer)
            {
                _renderer = renderer;
            }

            public AABB LocationOfCharacter(int offset, IAttributedText text, TextAttributes textAttributes)
            {
                var renderState = _renderer._lastRenderingState;
                if (renderState == null)
                    return AABB.Empty;

                return
                    WithTemporaryTextFormat(renderState, text, textAttributes, (format, layout) =>
                    {
                        var metric = layout.HitTestTextPosition(offset, false, out float _, out float _);

                        return AABB.FromRectangle(metric.Left, float.IsInfinity(metric.Top) ? 0 : metric.Top, metric.Width, metric.Height);
                    });
            }

            public AABB[] LocationOfCharacters(int offset, int length, IAttributedText text, TextAttributes textAttributes)
            {
                var renderState = _renderer._lastRenderingState;
                if (renderState == null)
                    return new AABB[0];

                return
                    WithTemporaryTextFormat(renderState, text, textAttributes, (format, layout) =>
                    {
                        var metrics = layout.HitTestTextRange(offset, length, 0, 0);
                        return metrics
                            .Select(range => AABB.FromRectangle(range.Left, range.Top, range.Width, range.Height))
                            .ToArray();
                    });
            }

            private static T WithTemporaryTextFormat<T>([NotNull] IDirect2DRenderingState renderState, [NotNull] IAttributedText text, TextAttributes textAttributes,
                [NotNull] Func<TextFormat, TextLayout, T> action)
            {
                using (var textFormat = new TextFormat(renderState.DirectWriteFactory, textAttributes.Font, textAttributes.FontSize)
                {
                    TextAlignment = DirectWriteAlignmentFor(textAttributes.HorizontalTextAlignment),
                    ParagraphAlignment = DirectWriteAlignmentFor(textAttributes.VerticalTextAlignment),
                    WordWrapping = DirectWriteWordWrapFor(textAttributes.WordWrap)
                })
                using (var textLayout = new TextLayout(renderState.DirectWriteFactory, text.String, textFormat, textAttributes.AvailableWidth, textAttributes.AvailableHeight))
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
    
    /// <summary>
    /// For rendering colored texts on a D2DRenderer
    /// </summary>
    internal class TextColorRenderer : TextRendererBase
    {
        private RenderTarget _renderTarget;
        public SolidColorBrush DefaultBrush { get; set; }

        public void AssignResources(RenderTarget renderTarget, SolidColorBrush defaultBrush)
        {
            _renderTarget = renderTarget;
            DefaultBrush = defaultBrush;
        }

        public override Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect)
        {
            var sb = DefaultBrush;
            if (clientDrawingEffect is SolidColorBrush brush)
                sb = brush;

            try
            {
                _renderTarget.DrawGlyphRun(new Vector2(baselineOriginX, baselineOriginY), glyphRun, sb, measuringMode);
                return Result.Ok;
            }
            catch
            {
                return Result.Fail;
            }
        }
    }
    
    internal static class DirectXHelpers
    {
        public static Color4 ToColor4(this Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;
            
            return new Color4(r, g, b, a);
        }
    }
}

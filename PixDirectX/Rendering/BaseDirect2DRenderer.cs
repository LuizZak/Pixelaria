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

using JetBrains.Annotations;

using PixCore.Text;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;

using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

using TextRange = SharpDX.DirectWrite.TextRange;

namespace PixDirectX.Rendering
{
    /// <inheritdoc cref="IDirect2DRenderer" />
    /// <summary>
    /// Base Direct2D renderer class that other packages may inherit from to provide custom rendering logic
    /// </summary>
    public class BaseDirect2DRenderer : IDisposable, IDirect2DRenderer
    {
        [CanBeNull]
        private IDirect2DRenderingState _lastRenderingState;
        
        protected readonly TextColorRenderer TextColorRenderer = new TextColorRenderer();
        
        private readonly D2DImageResources _imageResources;
        
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
        
        public BaseDirect2DRenderer()
        {
            _imageResources = new D2DImageResources();
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
            
            TextColorRenderer.DefaultBrush.Dispose();
            TextColorRenderer.Dispose();

            _imageResources.Dispose();
        }

        public virtual void Initialize([NotNull] IDirect2DRenderingState state)
        {
            _lastRenderingState = state;

            TextColorRenderer.AssignResources(state.D2DRenderTarget, new SolidColorBrush(state.D2DRenderTarget, Color4.White));
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
        
        #endregion
    }
}

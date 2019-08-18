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

using PixRendering;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using FastBitmapLib;
using JetBrains.Annotations;
using PixDirectX.Rendering.Skia;
using SkiaSharp;

namespace Pixelaria.Views.ExportPipeline
{
    class SkiaRendererStack: IRendererStack
    {
        private SKImageInfo _imageInfo;
        private Bitmap _bitmap;
        private SKSurface _surface;
        private bool _isControlDisposed;
        private readonly ClippingRegion _clippingRegion = new ClippingRegion();
        private readonly Control _control;
        private readonly SkiaRenderManager _renderManager;
        private readonly Stopwatch _frameDeltaTimer = new Stopwatch();
        private readonly SkiaImageResources _imageResource = new SkiaImageResources();
        private Action<IRenderLoopState, ClippingRegion> _renderAction;
        private SkiaRenderLoopState _renderState;
        public IRenderLoopState RenderingState => _renderState;

        public SkiaRendererStack([NotNull] Control control)
        {
            _control = control;
            _renderManager = new SkiaRenderManager(_imageResource);

            control.Disposed += ControlOnDisposed;
            control.Paint += ControlOnPaint;
            control.Resize += ControlOnResize;
        }

        private void ControlOnResize(object sender, EventArgs e)
        {
            RecreateState();
        }

        private void ControlOnDisposed(object sender, EventArgs e)
        {
            _isControlDisposed = true;
        }

        public void Dispose()
        {

        }

        private void RecreateState()
        {
            _bitmap?.Dispose();
            _surface?.Dispose();

            int width = _control.Size.Width;
            int height = _control.Size.Height;

            _bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

            _imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(_imageInfo, width * 4);
            
            _renderState = RenderStateFromCanvas(_surface.Canvas, TimeSpan.Zero);
        }

        public IRenderManager Initialize([NotNull] Control control)
        {
            RecreateState();

            _renderManager.Initialize(_renderState);

            return _renderManager;
        }

        private void ControlOnPaint(object sender, PaintEventArgs e)
        {
            if (_renderAction == null)
                return;

            var graphics = e.Graphics;

            _renderState = RenderStateFromCanvas(_surface.Canvas, _renderState.FrameRenderDeltaTime);

            if (!_clippingRegion.IsEmpty())
            {
                _renderManager.Render(_renderState, _clippingRegion);
            }

            using (var fastBitmap = _bitmap.FastLock())
            {
                _surface.ReadPixels(_imageInfo, fastBitmap.Scan0, _imageInfo.Width, 0, 0);
            }

            graphics.DrawImage(_bitmap, Point.Empty);
        }

        public void ConfigureRenderLoop(Action<IRenderLoopState, ClippingRegion> execute)
        {
            _renderAction = execute;

            _frameDeltaTimer.Start();
            while (!_isControlDisposed)
            {
                Application.DoEvents();

                _renderState.FrameRenderDeltaTime = _frameDeltaTimer.Elapsed;
                _frameDeltaTimer.Restart();

                _renderAction(RenderingState, _clippingRegion);

                if (!_clippingRegion.IsEmpty())
                {
                    foreach (var rectangle in _clippingRegion.RedrawRegionRectangles(_control.Size))
                    {
                        _control.Invalidate(new Region(rectangle));
                    }
                }

                Thread.Sleep(Math.Max(1, 16 - (int)_frameDeltaTimer.ElapsedMilliseconds));
            }
        }

        private SkiaRenderLoopState RenderStateFromCanvas(SKCanvas canvas, TimeSpan deltaTime)
        {
            return new SkiaRenderLoopState(canvas, _control.Size, deltaTime);
        }
    }
}

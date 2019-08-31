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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Blend2DCS;
using FastBitmapLib;
using JetBrains.Annotations;
using PixDirectX.Rendering.Blend2D;
using PixRendering;

namespace Pixelaria.Views.ExportPipeline
{
    public class Blend2DRendererStack : IRendererStack
    {
        private Bitmap _bitmap;
        private bool _isControlDisposed;
        private readonly ClippingRegion _clippingRegion = new ClippingRegion();
        private readonly Control _control;
        private readonly Blend2DRenderManager _renderManager;
        private readonly Stopwatch _frameDeltaTimer = new Stopwatch();
        private readonly Blend2DImageResources _imageResource = new Blend2DImageResources();
        private Action<IRenderLoopState, ClippingRegion> _renderAction;
        private Blend2DRenderLoopState _renderState;
        public IRenderLoopState RenderingState => _renderState;


        public Blend2DRendererStack([NotNull] Control control)
        {
            _control = control;
            _renderManager = new Blend2DRenderManager(_imageResource);

            control.Disposed += ControlOnDisposed;
            control.Paint += ControlOnPaint;
            control.Resize += ControlOnResize;

            _bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb);
        }

        private void ControlOnResize(object sender, EventArgs e)
        {
            if (_control.Width == 0 || _control.Height == 0)
                return;

            _bitmap.Dispose();
            _bitmap = new Bitmap(_control.Width, _control.Height, PixelFormat.Format32bppPArgb);
        }

        private void ControlOnDisposed(object sender, EventArgs e)
        {
            _isControlDisposed = true;
        }

        public void Dispose()
        {

        }

        public IRenderManager Initialize([NotNull] Control control)
        {
            WithRenderState(state =>
            {
                _renderState = state;
                _renderManager.Initialize(_renderState);
            });

            return _renderManager;
        }

        private void ControlOnPaint(object sender, PaintEventArgs e)
        {
            if (_renderAction == null)
                return;

            var graphics = e.Graphics;

            using (var fastBitmap = _bitmap.FastLock(FastBitmapLockFormat.Format32bppPArgb))
            using (var image = new BLImage(_bitmap.Width, _bitmap.Height, BLFormat.Prgb32, fastBitmap.Scan0, fastBitmap.StrideInBytes))
            using (var context = new BLContext(image))
            {
                _renderState = new Blend2DRenderLoopState(context, _control.Size, _renderState.FrameRenderDeltaTime);

                _renderManager.Render(_renderState, new ClippingRegion(new[] { (RectangleF)e.ClipRectangle }, true));

                context.Flush();
            }

            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImage(_bitmap, Point.Empty);
        }

        private void WithRenderState([NotNull] Action<Blend2DRenderLoopState> execute)
        {
            using (var fastBitmap = _bitmap.FastLock(FastBitmapLockFormat.Format32bppPArgb))
            using (var image = new BLImage(_bitmap.Width, _bitmap.Height, BLFormat.Prgb32, fastBitmap.Scan0, fastBitmap.StrideInBytes))
            using (var context = new BLContext(image))
            {
                _renderState = new Blend2DRenderLoopState(context, _control.Size, _renderState.FrameRenderDeltaTime);

                execute(_renderState);

                context.Flush();
            }
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
                    _clippingRegion.Clear();
                }

                Thread.Sleep(Math.Max(1, 16 - (int)_frameDeltaTimer.ElapsedMilliseconds));
            }
        }
    }
}
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
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixDirectX.Rendering;
using PixDirectX.Rendering.Gdi;
using PixRendering;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// A GDI+-based renderer stack.
    /// </summary>
    public class GdiRendererStack : IRendererStack
    {
        private bool _isControlDisposed;
        private readonly ClippingRegion _clippingRegion = new ClippingRegion();
        private readonly Control _control;
        private readonly GdiRenderManager _renderManager;
        private readonly Stopwatch _frameDeltaTimer = new Stopwatch();
        private readonly GdiImageResourceManager _imageResource = new GdiImageResourceManager();
        private Action<IRenderLoopState, ClippingRegion> _renderAction;
        private GdiRenderLoopState _renderState;
        public IRenderLoopState RenderingState => _renderState;


        public GdiRendererStack([NotNull] Control control)
        {
            _control = control;
            _renderManager = new GdiRenderManager(_imageResource);

            control.Disposed += ControlOnDisposed;
            control.Paint += ControlOnPaint;
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
            var graphics = control.CreateGraphics();
            _renderState = RenderStateFromGraphics(graphics, TimeSpan.Zero);
            _renderManager.Initialize(_renderState);

            return _renderManager;
        }

        private void ControlOnPaint(object sender, PaintEventArgs e)
        {
            if (_renderAction == null)
                return;

            var graphics = e.Graphics;

            _renderState = RenderStateFromGraphics(graphics, _renderState.FrameRenderDeltaTime);

            if (!_clippingRegion.IsEmpty())
            {
                _renderManager.Render(_renderState, _clippingRegion);
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
                }

                Thread.Sleep(Math.Max(1, 16 - (int)_frameDeltaTimer.ElapsedMilliseconds));
            }
        }

        private GdiRenderLoopState RenderStateFromGraphics(Graphics graphics, TimeSpan deltaTime)
        {
            return new GdiRenderLoopState(graphics, _control.Size, deltaTime);
        }
    }
}
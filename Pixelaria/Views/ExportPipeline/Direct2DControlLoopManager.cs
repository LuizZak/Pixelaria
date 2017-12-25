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
using System.Threading;
using System.Windows.Forms;
using PixUI.Rendering;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Windows;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Simple helper class to initialize and run a Direct2D loop on top of a specific Windows Forms control
    /// </summary>
    internal sealed class Direct2DControlLoopManager : IDisposable
    {
        private readonly Control _target;
        private readonly Stopwatch _frameDeltaTimer = new Stopwatch();
        private readonly Direct2DRenderingState _renderingState = new Direct2DRenderingState();

        /// <summary>
        /// Gets the public interface for the rendering state of this Direct2D manager
        /// </summary>
        internal IDirect2DRenderingState RenderingState => _renderingState;

        public Direct2DControlLoopManager(Control target)
        {
            _target = target;
            _target.Resize += target_Resize;
        }

        public void Dispose()
        {
            _frameDeltaTimer.Stop();
            _renderingState.Dispose();
        }
        
        /// <summary>
        /// Initializes the Direct2D rendering state, but do not start the render loop yet.
        /// </summary>
        public void InitializeDirect2D()
        {
            _renderingState.D2DFactory = new SharpDX.Direct2D1.Factory();
            _renderingState.DirectWriteFactory = new SharpDX.DirectWrite.Factory();

            // Direct2D Render Target
            var properties = new HwndRenderTargetProperties
            {
                Hwnd = _target.Handle,
                PixelSize = new Size2(_target.Width, _target.Height),
                PresentOptions = PresentOptions.RetainContents
            };

            _renderingState.WindowRenderTarget
                = new WindowRenderTarget(RenderingState.D2DFactory,
                    new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)), properties)
                {
                    TextAntialiasMode = TextAntialiasMode.Cleartype,
                    AntialiasMode = AntialiasMode.PerPrimitive
                };
        }

        /// <summary>
        /// Starts the render loop using a given closure as the actual content rendering delegate.
        /// 
        /// This method does not return after being called, and will continue processing Windows Form events
        /// internally until the application is closed.
        /// </summary>
        public void StartRenderLoop(Action<IDirect2DRenderingState> loop)
        {
            using (var renderLoop = new RenderLoop(_target) { UseApplicationDoEvents = true })
            {
                _frameDeltaTimer.Start();

                while (renderLoop.NextFrame())
                {
                    if (_frameDeltaTimer.ElapsedMilliseconds <= 16)
                        continue;

                    _renderingState.SetFrameDeltaTime(TimeSpan.FromTicks(_frameDeltaTimer.ElapsedTicks));
                    _frameDeltaTimer.Restart();

                    RenderingState.D2DRenderTarget.BeginDraw();

                    loop(RenderingState);
                    
                    RenderingState.D2DRenderTarget.EndDraw();

                    Thread.Sleep(15);
                }
            }
        }

        private void ResizeRenderTarget()
        {
            _renderingState.WindowRenderTarget?.Resize(new Size2(_target.Width, _target.Height));
        }

        private void target_Resize(object sender, EventArgs e)
        {
            ResizeRenderTarget();
        }
    }
}
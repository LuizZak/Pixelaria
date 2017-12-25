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
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixUI.Rendering;
using PixUI.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using Resource = SharpDX.Direct3D11.Resource;

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
            // SwapChain description
            var desc = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(_target.Width, _target.Height,
                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = _target.Handle,
                SampleDescription = new SampleDescription(2, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out _renderingState.Device, out _renderingState.SwapChain);

            _renderingState.D2DFactory = new Factory();
            _renderingState.DirectWriteFactory = new SharpDX.DirectWrite.Factory();

            // Ignore all windows events
            _renderingState.Factory = _renderingState.SwapChain.GetParent<SharpDX.DXGI.Factory>();
            _renderingState.Factory.MakeWindowAssociation(_target.FindForm()?.Handle ?? _target.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            _renderingState.BackBuffer = Resource.FromSwapChain<Texture2D>(_renderingState.SwapChain, 0);

            _renderingState.DxgiSurface = _renderingState.BackBuffer.QueryInterface<Surface>();
            
            var settings = new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied));
            _renderingState.D2DRenderTarget =
                new RenderTarget(RenderingState.D2DFactory, _renderingState.DxgiSurface, settings)
                {
                    TextAntialiasMode = TextAntialiasMode.Cleartype
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
            using (var renderLoop = new RenderLoop(_target) { UseApplicationDoEvents = false })
            {
                _frameDeltaTimer.Start();

                while (renderLoop.NextFrame())
                {
                    _renderingState.SetFrameDeltaTime(_frameDeltaTimer.Elapsed);
                    _frameDeltaTimer.Restart();

                    _renderingState.D2DRenderTarget.BeginDraw();

                    loop(RenderingState);

                    _renderingState.D2DRenderTarget.EndDraw();

                    // Sleep in case the screen is occluded so we don't waste cycles in this tight loop
                    // (Present doesn't wait for the next refresh in case the window is occluded)
                    if (_renderingState.SwapChain.Present(1, PresentFlags.None).Code == (int)DXGIStatus.Occluded)
                    {
                        Thread.Sleep(16);
                    }
                }
            }
        }

        private void ResizeRenderTarget()
        {
            _renderingState.D2DRenderTarget.Dispose();
            _renderingState.DxgiSurface.Dispose();
            _renderingState.BackBuffer.Dispose();

            _renderingState.SwapChain.ResizeBuffers(0, _target.Width, _target.Height, Format.Unknown, SwapChainFlags.None);

            _renderingState.BackBuffer = Resource.FromSwapChain<Texture2D>(_renderingState.SwapChain, 0);
            _renderingState.DxgiSurface = _renderingState.BackBuffer.QueryInterface<Surface>();
            var settings = new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied));
            _renderingState.D2DRenderTarget =
                new RenderTarget(RenderingState.D2DFactory, _renderingState.DxgiSurface, settings)
                {
                    TextAntialiasMode = TextAntialiasMode.Cleartype
                };
        }

        private void target_Resize(object sender, EventArgs e)
        {
            ResizeRenderTarget();
        }

        private class Direct2DRenderingState : IDirect2DRenderingState
        {
            private readonly Stack<Matrix3x2> _matrixStack = new Stack<Matrix3x2>();

            public SwapChain SwapChain;
            public Device Device;
            public SharpDX.DXGI.Factory Factory;

            public Surface DxgiSurface { set; get; }
            public Factory D2DFactory { set; get; }
            public Texture2D BackBuffer { set; get; }

            public RenderTarget D2DRenderTarget { set; get; }
            public SharpDX.DirectWrite.Factory DirectWriteFactory { get; set; }

            /// <summary>
            /// Gets the time span since the last frame rendered
            /// </summary>
            public TimeSpan FrameRenderDeltaTime { get; private set; }

            public void Dispose()
            {
                // Release all resources
                //RenderTargetView?.Dispose();
                BackBuffer?.Dispose();
                Device?.ImmediateContext.ClearState();
                Device?.ImmediateContext.Flush();
                Device?.Dispose();
                SwapChain?.Dispose();
                Factory?.Dispose();
            }

            public void SetFrameDeltaTime(TimeSpan frameDeltaTime)
            {
                FrameRenderDeltaTime = frameDeltaTime;
            }

            public void WithTemporaryClipping(AABB clipping, [InstantHandle] Action execute)
            {
                D2DRenderTarget.PushAxisAlignedClip(clipping.ToRawRectangleF(), AntialiasMode.Aliased);

                execute();

                D2DRenderTarget.PopAxisAlignedClip();
            }

            public void PushingTransform([InstantHandle] Action execute)
            {
                var transform = D2DRenderTarget.Transform;
                execute();
                D2DRenderTarget.Transform = transform;
            }

            public void PushMatrix(Matrix3x2 matrix)
            {
                _matrixStack.Push(D2DRenderTarget.Transform);

                D2DRenderTarget.Transform = D2DRenderTarget.Transform * matrix;
            }

            public void PopMatrix()
            {
                D2DRenderTarget.Transform = _matrixStack.Pop();
            }
        }
    }
}
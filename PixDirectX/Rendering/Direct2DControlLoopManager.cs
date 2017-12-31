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
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using Factory2 = SharpDX.DXGI.Factory2;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using Resource = SharpDX.Direct3D11.Resource;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Simple helper class to initialize and run a Direct2D loop on top of a specific Windows Forms control
    /// </summary>
    public sealed class Direct2DControlLoopManager : IDisposable
    {
        private readonly Control _target;
        private readonly Stopwatch _frameDeltaTimer = new Stopwatch();
        private readonly Direct2DRenderingState _renderingState = new Direct2DRenderingState();

        /// <summary>
        /// Event fired whenever the Direct2D state of this loop manager is invalidated (either due to context
        /// switches, resolution changes or window state changes).
        /// 
        /// Use this event handler to invalidate data related to screen rendering for a fresh rendering state.
        /// </summary>
        public event EventHandler InvalidatedState;

        /// <summary>
        /// Gets the public interface for the rendering state of this Direct2D manager
        /// </summary>
        public IDirect2DRenderingState RenderingState => _renderingState;

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
            var featureLevels = new[]
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0
            };
            const DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;

            var d3Device = new Device(DriverType.Hardware, creationFlags, featureLevels);
            var d3Device1 = d3Device.QueryInterface<SharpDX.Direct3D11.Device1>();

            var dxgiDevice = d3Device1.QueryInterface<SharpDX.DXGI.Device1>();
            var dxgiFactory = dxgiDevice.Adapter.GetParent<Factory2>();
            
            // This gives DXGI_ERROR_INVALID_CALL
            var swapChainDescription = new SwapChainDescription1
            {
                Width = _target.Width,
                Height = _target.Height,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                BufferCount = 1,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.Sequential,
                Flags = SwapChainFlags.AllowModeSwitch
            };
            
            var swapChain = new SwapChain1(dxgiFactory, d3Device1, _target.Handle, ref swapChainDescription);

            // Ignore all windows events
            var factory = swapChain.GetParent<Factory2>();
            factory.MakeWindowAssociation(_target.Handle, WindowAssociationFlags.IgnoreAll);

            var d2DFactory = new Factory();

            // New RenderTargetView from the backbuffer
            var backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);

            var dxgiSurface = backBuffer.QueryInterface<Surface>();

            var settings = new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied));
            var renderTarget =
                new RenderTarget(d2DFactory, dxgiSurface, settings)
                {
                    TextAntialiasMode = TextAntialiasMode.Cleartype
                };
            
            var directWriteFactory = new SharpDX.DirectWrite.Factory();

            _renderingState.D2DFactory = d2DFactory;
            _renderingState.D2DRenderTarget = renderTarget;
            _renderingState.SwapChain = swapChain;
            _renderingState.Factory = factory;
            _renderingState.DxgiSurface = dxgiSurface;
            _renderingState.BackBuffer = backBuffer;
            _renderingState.DirectWriteFactory = directWriteFactory;
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

        /// <summary>
        /// Starts the render loop using a given closure as the actual content rendering delegate.
        /// 
        /// The closure must return an array of Rectangle regions that specify the dirty regions updated during
        /// the loop.
        /// 
        /// This method does not return after being called, and will continue processing Windows Form events
        /// internally until the application is closed.
        /// </summary>
        public void StartRenderLoop(Func<IDirect2DRenderingState, System.Drawing.Rectangle[]> loop)
        {
            bool isOccluded = false;
            
            using (var renderLoop = new RenderLoop(_target) { UseApplicationDoEvents = false })
            {
                _frameDeltaTimer.Start();

                while (renderLoop.NextFrame())
                {
                    _renderingState.SetFrameDeltaTime(_frameDeltaTimer.Elapsed);
                    _frameDeltaTimer.Restart();

                    _renderingState.D2DRenderTarget.BeginDraw();

                    var rects = loop(RenderingState).Select(r => (RawRectangle)new Rectangle(r.X, r.Y, r.Width, r.Height)).ToArray();

                    _renderingState.D2DRenderTarget.EndDraw();

                    var parameters = new PresentParameters
                    {
                        DirtyRectangles = rects,
                        ScrollOffset = null,
                        ScrollRectangle = null
                    };

                    // Occluded state handling
                    if (isOccluded)
                    {
                        // Test if we're still in occluded state
                        if (_renderingState.SwapChain.Present(1, PresentFlags.Test) == (int)DXGIStatus.Occluded)
                        {
                            Thread.Sleep(16);
                            continue;
                        }

                        isOccluded = false;

                        InvalidatedState?.Invoke(this, EventArgs.Empty);

                        // Reset parameters for next redraw (backbuffer is empty when coming back from idle)
                        parameters = new PresentParameters();
                    }

                    // Sleep in case the screen is occluded so we don't waste cycles in this tight loop
                    // (Present doesn't wait for the next refresh in case the window is occluded)
                    if (_renderingState.SwapChain.Present(1, PresentFlags.None, parameters).Code == (int)DXGIStatus.Occluded)
                    {
                        isOccluded = true;
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

            public SwapChain1 SwapChain;
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
                BackBuffer?.Dispose();
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
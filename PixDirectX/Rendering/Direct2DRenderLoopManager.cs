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
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using Factory = SharpDX.Direct2D1.Factory;
using Factory2 = SharpDX.DXGI.Factory2;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using Resource = SharpDX.Direct3D11.Resource;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Simple helper class to initialize and run a Direct2D loop on top of a specific Windows Forms control
    /// </summary>
    public sealed class Direct2DRenderLoopManager : IDirect2DRenderManager
    {
        private readonly Control _target;
        private readonly Stopwatch _frameDeltaTimer = new Stopwatch();
        private readonly Direct2DRenderingState _renderingState = new Direct2DRenderingState();
        private readonly Factory _d2DFactory;
        private readonly Device _d3DDevice;

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

        public Direct2DRenderLoopManager(Control target, Factory d2DFactory, Device d3DDevice)
        {
            _d2DFactory = d2DFactory;
            _d3DDevice = d3DDevice;

            _target = target;
            _target.Resize += target_Resize;
        }

        public Direct2DRenderLoopManager(Control target, Factory d2DFactory)
        {
            _d2DFactory = d2DFactory;

            var featureLevels = new[]
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
                FeatureLevel.Level_9_3
            };
            var creationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= DeviceCreationFlags.Debug;
#endif

            _d3DDevice = new Device(DriverType.Hardware, creationFlags, featureLevels);

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
            var d3Device1 = _d3DDevice.QueryInterface<SharpDX.Direct3D11.Device1>();

            var dxgiDevice = d3Device1.QueryInterface<SharpDX.DXGI.Device1>();
            var dxgiFactory = dxgiDevice.Adapter.GetParent<Factory2>();
            var d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice);
            var d2dContext = new DeviceContext(d2dDevice, DeviceContextOptions.None);

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
            
            // New RenderTargetView from the back-buffer
            var backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);

            var dxgiSurface = backBuffer.QueryInterface<Surface>();
            
            var pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
            var settings = new RenderTargetProperties(pixelFormat)
            {
                Type = RenderTargetType.Hardware, 
                Usage = RenderTargetUsage.None
            };
            var renderTarget =
                new RenderTarget(_d2DFactory, dxgiSurface, settings)
                {
                    TextAntialiasMode = TextAntialiasMode.Cleartype
                };

            var directWriteFactory = new SharpDX.DirectWrite.Factory();

            var desktopScale =
                new Vector(_d2DFactory.DesktopDpi.Width, _d2DFactory.DesktopDpi.Height) / new Vector(96.0f, 96.0f);
            
            _renderingState.D2DFactory = _d2DFactory;
            _renderingState.D2DRenderTarget = renderTarget;
            _renderingState.SwapChain = swapChain;
            _renderingState.DxgiSurface = dxgiSurface;
            _renderingState.BackBuffer = backBuffer;
            _renderingState.DirectWriteFactory = directWriteFactory;
            _renderingState.DesktopDpiScaling = desktopScale;
            _renderingState.DeviceContext = d2dContext;
        }

        /// <summary>
        /// Starts the render loop using a given closure as the actual content rendering delegate.
        /// 
        /// This method does not return after being called, and will continue processing Windows Form events
        /// internally until the application is closed.
        /// </summary>
        public void StartRenderLoop([NotNull, InstantHandle] Action<IDirect2DRenderingState> loop)
        {
            StartRenderLoop(state =>
            {
                loop(state);

                return new Direct2DRenderLoopResponse(new System.Drawing.Rectangle[0], false);
            });
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
        public void StartRenderLoop([NotNull, InstantHandle] Func<IDirect2DRenderingState, Direct2DRenderLoopResponse> loop)
        {
            bool isOccluded = false;
            bool quitLoop = false;
            
            using (var renderLoop = new RenderLoop(_target) { UseApplicationDoEvents = false })
            {
                _frameDeltaTimer.Start();

                while (!quitLoop && renderLoop.NextFrame())
                {
                    _renderingState.SetFrameDeltaTime(_frameDeltaTimer.Elapsed);
                    _frameDeltaTimer.Restart();

                    /* TODO: Re-enable when desktop DPI-awareness is working again

                    var desktopScale = 
                        new Vector(_renderingState.D2DFactory.DesktopDpi.Width, _renderingState.D2DFactory.DesktopDpi.Height) / new Vector(96.0f, 96.0f);
                    
                    _renderingState.D2DRenderTarget.Transform = Matrix3x2.Scaling(desktopScale.X, desktopScale.Y);
                    // */

                    _renderingState.D2DRenderTarget.BeginDraw();

                    var results = loop(RenderingState);
                    var rects = results.RedrawRegions
                        .Select(r => (RawRectangle) new Rectangle(r.X, r.Y, r.Width, r.Height)).ToArray();

                    quitLoop = results.QuitRenderLoop;

                    _renderingState.D2DRenderTarget.EndDraw();

                    var parameters = new PresentParameters
                    {
                        DirtyRectangles = rects,
                        ScrollOffset = null,
                        ScrollRectangle = null
                    };

                    // Test if we're in occluded state
                    if (_renderingState.SwapChain.Present(0, PresentFlags.Test) == (int)DXGIStatus.Occluded)
                    {
                        isOccluded = true;
                        Thread.Sleep(Math.Max(1, 16 - (int)_frameDeltaTimer.ElapsedMilliseconds));
                        continue;
                    }

                    // Occluded state handling
                    if (isOccluded)
                    {
                        isOccluded = false;

                        InvalidatedState?.Invoke(this, EventArgs.Empty);

                        // Reset parameters for next redraw (back-buffer is empty when coming back from idle)
                        parameters = new PresentParameters();
                    }

                    _renderingState.SwapChain.Present(0, PresentFlags.None, parameters);
                    Thread.Sleep(Math.Max(1, 16 - (int)_frameDeltaTimer.ElapsedMilliseconds));
                }
            }
        }
        
        /// <inheritdoc />
        public void RenderSingleFrame(Action<IDirect2DRenderingState> render)
        {
            _frameDeltaTimer.Restart();

            RenderingState.D2DRenderTarget.BeginDraw();

            render(RenderingState);

            RenderingState.D2DRenderTarget.EndDraw();
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
                new RenderTarget(_d2DFactory, _renderingState.DxgiSurface, settings)
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

            public Surface DxgiSurface { set; get; }
            public Factory D2DFactory { set; get; }
            public Texture2D BackBuffer { set; get; }

            public RenderTarget D2DRenderTarget { set; get; }
            public DeviceContext DeviceContext { get; set; }
            public SharpDX.DirectWrite.Factory DirectWriteFactory { get; set; }

            /// <summary>
            /// Gets the time span since the last frame rendered
            /// </summary>
            public TimeSpan FrameRenderDeltaTime { get; private set; }

            public Vector DesktopDpiScaling { get; set; }

            public Matrix3x2 Transform
            {
                get => D2DRenderTarget.Transform;
                set => D2DRenderTarget.Transform = value;
            }

            public void Dispose()
            {
                // Release all resources
                BackBuffer?.Dispose();
                SwapChain?.Dispose();
                DirectWriteFactory?.Dispose();
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

            public void PushMatrix()
            {
                _matrixStack.Push(Transform);
            }

            public void PushMatrix(Matrix3x2 matrix)
            {
                _matrixStack.Push(Transform);

                Transform = Transform * matrix;
            }

            public void PopMatrix()
            {
                Transform = _matrixStack.Pop();
            }
        }
    }

    /// <summary>
    /// Encapsulates a response for the render loop method of <see cref="Direct2DRenderLoopManager.StartRenderLoop(Func{IDirect2DRenderingState,Direct2DRenderLoopResponse})"/>
    /// </summary>
    public readonly struct Direct2DRenderLoopResponse
    {
        /// <summary>
        /// Regions called rendering delegate has redrawn on a render loop call.
        /// 
        /// If set to a non-empty list, the caller <i>must</i> have drawn content on all pixels that where reported on all redraw regions.
        /// </summary>
        public IReadOnlyList<System.Drawing.Rectangle> RedrawRegions { get; }

        /// <summary>
        /// If set to true, <see cref="Direct2DRenderLoopManager"/> will stop its rendering loop and return control to the caller of
        /// <see cref="Direct2DRenderLoopManager.StartRenderLoop(Func{IDirect2DRenderingState,Direct2DRenderLoopResponse})"/>.
        /// 
        /// Defaults to false.
        /// </summary>
        public bool QuitRenderLoop { get; }
        
        public Direct2DRenderLoopResponse(IReadOnlyList<System.Drawing.Rectangle> redrawRegions)
        {
            RedrawRegions = redrawRegions;
            QuitRenderLoop = false;
        }

        public Direct2DRenderLoopResponse(IReadOnlyList<System.Drawing.Rectangle> redrawRegions, bool quitRenderLoop)
        {
            RedrawRegions = redrawRegions;
            QuitRenderLoop = quitRenderLoop;
        }
    }
}
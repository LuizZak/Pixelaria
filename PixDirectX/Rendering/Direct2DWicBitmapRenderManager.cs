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
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Bitmap = SharpDX.WIC.Bitmap;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using Factory = SharpDX.Direct2D1.Factory;
using Factory2 = SharpDX.DXGI.Factory2;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Renderer that renders to a backing WIC Bitmap.
    /// </summary>
    public sealed class Direct2DWicBitmapRenderManager : IDirect2DRenderManager
    {
        private readonly Factory _d2DFactory;
        private readonly Device _d3DDevice;
        private readonly Bitmap _target;
        private readonly Direct2DRenderingState _renderingState = new Direct2DRenderingState();
        
        public IDirect2DRenderingState RenderingState => _renderingState;

        public Direct2DWicBitmapRenderManager(Bitmap target, Factory d2DFactory, Device d3DDevice)
        {
            _target = target;
            _d2DFactory = d2DFactory;
            _d3DDevice = d3DDevice;
        }

        public Direct2DWicBitmapRenderManager(Bitmap target, Factory d2DFactory)
        {
            _target = target;
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
        }

        public void Dispose()
        {
            _renderingState.Dispose();
        }

        /// <inheritdoc />
        public void InitializeDirect2D()
        {
            var d3Device1 = _d3DDevice.QueryInterface<SharpDX.Direct3D11.Device1>();

            var dxgiDevice = d3Device1.QueryInterface<SharpDX.DXGI.Device1>();
            var dxgiFactory = dxgiDevice.Adapter.GetParent<Factory2>();
            var d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice);
            var d2dContext = new DeviceContext(d2dDevice, DeviceContextOptions.None);

            var pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);
            var renderTargetProperties = new RenderTargetProperties(pixelFormat)
            {
                Type = RenderTargetType.Software,
                Usage = RenderTargetUsage.None
            };

            var bitmapTarget = new WicRenderTarget(_d2DFactory, _target, renderTargetProperties)
            {
                TextAntialiasMode = TextAntialiasMode.Cleartype
            };

            var directWriteFactory = new SharpDX.DirectWrite.Factory();

            _renderingState.D2DFactory = _d2DFactory;
            _renderingState.WicRenderTarget = bitmapTarget;
            _renderingState.Factory = dxgiFactory;
            _renderingState.DirectWriteFactory = directWriteFactory;
            _renderingState.DeviceContext = d2dContext;
        }

        /// <inheritdoc />
        public void RenderSingleFrame(Action<IDirect2DRenderingState> render)
        {
            _renderingState.D2DRenderTarget.BeginDraw();

            render(_renderingState);

            _renderingState.D2DRenderTarget.EndDraw();
        }
        
        private class Direct2DRenderingState : IDirect2DRenderingState
        {
            private readonly Stack<Matrix3x2> _matrixStack = new Stack<Matrix3x2>();
            
            public SharpDX.DXGI.Factory Factory;
            
            public Factory D2DFactory { set; get; }
            public DeviceContext DeviceContext { get; set; }
            
            public WicRenderTarget WicRenderTarget { private get; set; }
            public RenderTarget D2DRenderTarget => WicRenderTarget;
            public SharpDX.DirectWrite.Factory DirectWriteFactory { get; set; }

            public TimeSpan FrameRenderDeltaTime => TimeSpan.Zero;

            public Vector DesktopDpiScaling { get; set; }

            public Matrix3x2 Transform
            {
                get => D2DRenderTarget.Transform;
                set => D2DRenderTarget.Transform = value;
            }

            public void Dispose()
            {
                // Release all resources
                DirectWriteFactory?.Dispose();
                WicRenderTarget?.Dispose();
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

                Transform *= matrix;
            }

            public void PopMatrix()
            {
                Transform = _matrixStack.Pop();
            }
        }
    }
}
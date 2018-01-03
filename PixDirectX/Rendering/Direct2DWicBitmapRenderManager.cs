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
        private readonly Bitmap _target;
        private readonly Direct2DRenderingState _renderingState = new Direct2DRenderingState();
        
        public IDirect2DRenderingState RenderingState => _renderingState;

        public Direct2DWicBitmapRenderManager(Bitmap target)
        {
            _target = target;
        }

        public void Dispose()
        {
            _renderingState.Dispose();
        }

        /// <inheritdoc />
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

            var d2DFactory = new SharpDX.Direct2D1.Factory();

            var pixelFormat = new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied);
            var renderTargetProperties = new RenderTargetProperties(pixelFormat);

            var bitmapTarget = new WicRenderTarget(d2DFactory, _target, renderTargetProperties);

            var directWriteFactory = new SharpDX.DirectWrite.Factory();

            _renderingState.D2DFactory = d2DFactory;
            _renderingState.WicRenderTarget = bitmapTarget;
            _renderingState.Factory = dxgiFactory;
            _renderingState.DirectWriteFactory = directWriteFactory;
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
            
            public SharpDX.Direct2D1.Factory D2DFactory { set; get; }
            
            public WicRenderTarget WicRenderTarget { private get; set; }
            public RenderTarget D2DRenderTarget => WicRenderTarget;
            public SharpDX.DirectWrite.Factory DirectWriteFactory { get; set; }
                
            public TimeSpan FrameRenderDeltaTime => TimeSpan.Zero;

            public void Dispose()
            {
                // Release all resources
                Factory?.Dispose();
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
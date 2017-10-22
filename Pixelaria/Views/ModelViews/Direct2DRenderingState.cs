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
using JetBrains.Annotations;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;

namespace Pixelaria.Views.ModelViews
{
    public class Direct2DRenderingState
    {
        public SwapChain SwapChain;
        public Surface DxgiSurface { set; get; }
        public RenderTarget D2DRenderTarget { set; get; }
        public SharpDX.Direct2D1.Factory D2DFactory { set; get; }
        public Texture2D BackBuffer { set; get; }
        public RenderTargetView RenderTargetView { set; get; }
        public Device Device;
        public Factory Factory;
        public SharpDX.DirectWrite.Factory DirectWriteFactory;

        public void PushingTransform([NotNull] Action execute)
        {
            var transform = D2DRenderTarget.Transform;
            execute();
            D2DRenderTarget.Transform = transform;
        }
    }
}
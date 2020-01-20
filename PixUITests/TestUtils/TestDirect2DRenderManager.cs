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
using System.Windows.Forms;
using JetBrains.Annotations;
using PixDirectX.Rendering.DirectX;
using PixRendering;
using PixUI;
using PixUI.Controls;

namespace PixUITests.TestUtils
{
    /// <summary>
    /// Basic rendering shim for snapshot tests
    /// </summary>
    public class TestDirect2DRenderManager : Direct2DRenderManager
    {
        public void Initialize([NotNull] IDirect2DRenderingState state, [NotNull] IClippingRegion clipping)
        {
            base.Initialize(state);

            ClippingRegion = clipping;
        }

        public static void CreateTemporary([NotNull] Action<TestDirect2DRenderManager> closure)
        {
            using (var factory = new SharpDX.Direct2D1.Factory())
            using (var renderLoop = new Direct2DRenderLoopManager(new Panel(), factory))
            using (var renderer = new TestDirect2DRenderManager())
            {
                ControlView.TextLayoutRenderer = renderer;

                var last = LabelView.defaultTextSizeProvider;
                LabelView.defaultTextSizeProvider = renderer.TextSizeProvider;

                renderLoop.Initialize();

                renderer.Initialize(renderLoop.D2DRenderState, new FullClippingRegion());

                closure(renderer);

                LabelView.defaultTextSizeProvider = last;
            }
        }
    }
}

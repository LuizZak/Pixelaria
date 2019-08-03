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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PixCore.Geometry;
using PixDirectX.Rendering;
using PixDirectX.Rendering.DirectX;
using PixDirectX.Utils;
using Pixelaria.DXSupport;
using PixRendering;
using SharpDX.Direct2D1;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// A full Direct2D renderer stack.
    /// </summary>
    public class Direct2DRendererStack : IRendererStack
    {
        private Direct2DRenderLoopManager _direct2DLoopManager;
        private readonly Direct2DRenderManager _renderManager;
        public IRenderLoopState RenderingState => _direct2DLoopManager.RenderingState;

        public bool RenderRedrawRegions { get; set; }

        public Direct2DRendererStack()
        {
            _renderManager = new Direct2DRenderManager();
        }

        public void Dispose()
        {
            _direct2DLoopManager?.Dispose();
        }

        public IRenderManager Initialize(Control control)
        {
            _direct2DLoopManager = new Direct2DRenderLoopManager(control, DxSupport.D2DFactory, DxSupport.D3DDevice);
            _direct2DLoopManager.Initialize();
            _renderManager.Initialize(_direct2DLoopManager.RenderingState);

            return _renderManager;
        }

        public void ConfigureRenderLoop(Action<IRenderLoopState, ClippingRegion> execute)
        {
            var clippingRegion = new ClippingRegion();

            _direct2DLoopManager.StartRenderLoop(state =>
            {
                clippingRegion.Clear();

                execute(state, clippingRegion);
                // Use clipping region
                var clipState = Direct2DClipping.PushDirect2DClipping((IDirect2DRenderingState)state, clippingRegion);

                _renderManager.Render(state, clippingRegion);
                
                Direct2DClipping.PopDirect2DClipping((IDirect2DRenderingState)state, clipState);

                var size = new Size(_direct2DLoopManager.D2DRenderState.D2DRenderTarget.PixelSize.Width, _direct2DLoopManager.D2DRenderState.D2DRenderTarget.PixelSize.Height);
                var rects = clippingRegion.RedrawRegionRectangles(size);

                if (RenderRedrawRegions)
                {
                    foreach (var rect in rects)
                    {
                        var renderState = (IDirect2DRenderingState) state;

                        using (var brush = new SolidColorBrush(renderState.D2DRenderTarget, Color.Red.ToColor4()))
                        {
                            renderState.D2DRenderTarget.DrawRectangle(((AABB) rect).ToRawRectangleF(), brush);
                        }
                    }
                }

                var redrawRects =
                    rects.Select(rect =>
                    {
                        int x = (int)Math.Floor(rect.X);
                        int y = (int)Math.Floor(rect.Y);

                        int width = (int)Math.Ceiling(rect.Width);
                        int height = (int)Math.Ceiling(rect.Height);

                        return new Rectangle(x, y, width, height);
                    }).ToArray();

                return new Direct2DRenderLoopResponse(redrawRects);
            });
        }
    }
}
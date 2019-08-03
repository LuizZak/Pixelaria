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
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Utils;
using PixRendering;
using SharpDX;
using SharpDX.Direct2D1;

namespace PixDirectX.Rendering.DirectX
{
    /// <summary>
    /// Helper class to work with clipping on Direct2D rendering contexts
    /// </summary>
    public class Direct2DClipping
    {
        /// <summary>
        /// Pushes a clipping region's area on top of a given Direct2D rendering state.
        /// </summary>
        public static IDirect2DClippingState PushDirect2DClipping([NotNull] IDirect2DRenderingState state, [NotNull] ClippingRegion clippingRegion)
        {
            var size = new Size((int) state.D2DRenderTarget.Size.Width, (int) state.D2DRenderTarget.Size.Height);

            var aabbClips = clippingRegion.RedrawRegionRectangles(size).Select(rect => (AABB) rect).ToArray();

            // If we're only working with a single rectangular clip, use a plain axis-aligned clip
            if (aabbClips.Length == 1)
            {
                state.D2DRenderTarget.PushAxisAlignedClip(aabbClips[0].ToRawRectangleF(), AntialiasMode.Aliased);

                return new Direct2DAxisAlignedClippingState();
            }

            var geom = new PathGeometry(state.D2DFactory);
            using (var sink = geom.Open())
            {
                sink.SetFillMode(FillMode.Winding);

                // Create geometry
                var poly = new PolyGeometry();
                foreach (var aabb in aabbClips)
                {
                    poly.Combine(aabb, GeometryOperation.Union);
                }

                foreach (var polygon in poly.Polygons())
                {
                    sink.BeginFigure(polygon[0].ToRawVector2(), FigureBegin.Filled);

                    foreach (var corner in polygon.Skip(1))
                    {
                        sink.AddLine(corner.ToRawVector2());
                    }

                    sink.EndFigure(FigureEnd.Closed);
                }

                sink.Close();
            }

            var layerParams = new LayerParameters
            {
                ContentBounds = SharpDX.RectangleF.Infinite,
                MaskAntialiasMode = AntialiasMode.Aliased,
                Opacity = 1f,
                GeometricMask = geom,
                MaskTransform = Matrix3x2.Identity,
                LayerOptions = LayerOptions.InitializeForCleartype
            };

            var layer = new Layer(state.D2DRenderTarget, state.D2DRenderTarget.Size);
            state.D2DRenderTarget.PushLayer(ref layerParams, layer);

            return new Direct2DGeometryClippingState(geom, layer);
        }

        /// <summary>
        /// Pops a clipping state that was created by this <see cref="Direct2DClipping"/> from a given Direct2D rendering state.
        /// </summary>
        public static void PopDirect2DClipping([NotNull] IDirect2DRenderingState state, [NotNull] IDirect2DClippingState clipState)
        {
            switch (clipState)
            {
                case Direct2DAxisAlignedClippingState _:
                    state.D2DRenderTarget.PopAxisAlignedClip();

                    break;
                case Direct2DGeometryClippingState d2DClip:
                    state.D2DRenderTarget.PopLayer();

                    d2DClip.Dispose();
                    break;
            }
        }

        /// <summary>
        /// Stores context about a Direct2D clipping operation.
        /// </summary>
        public interface IDirect2DClippingState
        {

        }

        private struct Direct2DGeometryClippingState : IDirect2DClippingState, IDisposable
        {
            private Geometry Geometry { get; }
            private Layer Layer { get; }

            public Direct2DGeometryClippingState(Geometry geometry, Layer layer)
            {
                Geometry = geometry;
                Layer = layer;
            }

            public void Dispose()
            {
                Geometry?.Dispose();
                Layer?.Dispose();
            }
        }

        private sealed class Direct2DAxisAlignedClippingState : IDirect2DClippingState
        {

        }
    }
}
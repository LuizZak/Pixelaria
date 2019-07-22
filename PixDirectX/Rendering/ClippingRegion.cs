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
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Geometry.Algorithms;
using PixDirectX.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// A clipping region backed by a list of individual <see cref="RectangleF"/> instances.
    /// </summary>
    public class ClippingRegion : IClippingRegion
    {
        private readonly List<RectangleF> _rectangles;
        private bool _needsDissect;

        public ClippingRegion()
        {
            _rectangles = new List<RectangleF>();
        }

        /// <summary>
        /// Returns a series of <see cref="RectangleF"/> instances that approximate the redraw region
        /// of this <see cref="ClippingRegion"/>, truncated to be within the given <see cref="Size"/>-d rectangle.
        /// </summary>
        public virtual RectangleF[] RedrawRegionRectangles(Size size)
        {
            if (_needsDissect)
            {
                Dissect();
            }

            var controlRect = new RectangleF(PointF.Empty, size);

            var rects = _rectangles;

            var clipped =
                rects
                    .Where(r => r.IntersectsWith(controlRect))
                    .Select(r =>
                    {
                        var rect = r;
                        rect.Intersect(controlRect);
                        return rect;
                    });

            return RectangleDissection.MergeRectangles(clipped.ToArray());
        }

        public virtual bool IsVisibleInClippingRegion(Rectangle rectangle)
        {
            return _rectangles.Any(r => r.IntersectsWith(rectangle));
        }

        public virtual bool IsVisibleInClippingRegion(Point point)
        {
            return _rectangles.Any(r => r.Contains(point));
        }

        public virtual bool IsVisibleInClippingRegion(AABB aabb)
        {
            return _rectangles.Any(r => r.IntersectsWith((RectangleF) aabb));
        }

        public virtual bool IsVisibleInClippingRegion(Vector point)
        {
            return _rectangles.Any(r => r.Contains(point));
        }

        public virtual bool IsVisibleInClippingRegion(AABB aabb, ISpatialReference reference)
        {
            var transformed = reference.ConvertTo(aabb, null);
            return _rectangles.Any(r => r.IntersectsWith((RectangleF) transformed));
        }

        public virtual bool IsVisibleInClippingRegion(Vector point, ISpatialReference reference)
        {
            var transformed = reference.ConvertTo(point, null);
            return _rectangles.Any(r => r.Contains(transformed));
        }

        public void AddRectangle(RectangleF rectangle)
        {
            if (IsEmpty())
            {
                _rectangles.Add(rectangle);
                return;
            }

            // If there are any rectangles available, check if we're not contained within other rectangles
            if (_rectangles.Any(rect => rect.Contains(rectangle)))
            {
                return;
            }

            // If no intersection is found, just add the rectangle right away
            if (_rectangles.All(rect => !rect.IntersectsWith(rectangle)))
            {
                _rectangles.Add(rectangle);
                return;
            }

            _needsDissect = true;

            _rectangles.Add(rectangle);
        }

        public void AddRegion([NotNull] IRegion region)
        {
            var scans = region.GetRectangles();

            foreach (var scan in scans)
            {
                AddRectangle((RectangleF) scan.Inflated(5, 5));
            }
        }

        public void SetRectangle(RectangleF rectangle)
        {
            Clear();
            AddRectangle(rectangle);
        }

        public void Clear()
        {
            _rectangles.Clear();
        }

        public virtual bool IsEmpty()
        {
            return _rectangles.Count == 0;
        }

        public virtual IDirect2DClippingState PushDirect2DClipping([NotNull] IDirect2DRenderingState state)
        {
            var size = new Size((int) state.D2DRenderTarget.Size.Width, (int) state.D2DRenderTarget.Size.Height);

            var aabbClips = RedrawRegionRectangles(size).Select(rect => (AABB) rect).ToArray();

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

        public virtual void PopDirect2DClipping([NotNull] IDirect2DRenderingState state,
            [NotNull] IDirect2DClippingState clipState)
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
        /// Stores context about a Direct2D clipping operation
        /// </summary>
        public interface IDirect2DClippingState
        {

        }

        private void Dissect()
        {
            var ret = RectangleDissection.Dissect(_rectangles);

            _rectangles.Clear();
            _rectangles.AddRange(ret);

            _needsDissect = false;
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
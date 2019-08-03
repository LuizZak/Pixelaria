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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Geometry.Algorithms;
using PixRendering;
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

        public ClippingRegion([NotNull] IEnumerable<RectangleF> rects, bool areDissected)
        {
            _needsDissect = areDissected;
            _rectangles.AddRange(rects);
        }

        /// <summary>
        /// Initializes a new <see cref="ClippingRegion"/> instance copying the state from another clipping
        /// region object.
        /// </summary>
        public ClippingRegion([NotNull] ClippingRegion copy)
        {
            _rectangles = new List<RectangleF>(copy._rectangles);
            _needsDissect = copy._needsDissect;
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

        /// <summary>
        /// Adds the regions of another clipping region to this clipping region instance.
        /// </summary>
        public void AddClippingRegion([NotNull] ClippingRegion region)
        {
            _rectangles.AddRange(region._rectangles);
            _needsDissect = true;
        }

        /// <summary>
        /// Applies a clip to the rectangles on this <see cref="ClippingRegion"/> so they are all contained within
        /// a given region.
        /// </summary>
        public void ApplyClip(AABB region, [CanBeNull] ISpatialReference reference)
        {
            ApplyClip((RectangleF)region, reference);
        }

        /// <summary>
        /// Applies a clip to the rectangles on this <see cref="ClippingRegion"/> so they are all contained within
        /// a given region.
        /// </summary>
        public void ApplyClip(RectangleF region, [CanBeNull] ISpatialReference reference)
        {
            var clipRegion = region;
            if (reference != null)
            {
                clipRegion = (RectangleF)reference.ConvertTo(region, null);
            }

            for (int i = _rectangles.Count - 1; i >= 0; i--)
            {
                var rect = _rectangles[i];
                rect = RectangleF.Intersect(rect, clipRegion);

                if (rect.IsEmpty)
                {
                    _rectangles.RemoveAt(i);
                }
                else
                {
                    _rectangles[i] = rect;
                }
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
            return _rectangles.Count == 0 || _rectangles.All(r => r.IsEmpty);
        }

        private void Dissect()
        {
            var ret = RectangleDissection.Dissect(_rectangles);

            _rectangles.Clear();
            _rectangles.AddRange(ret);

            _needsDissect = false;
        }
    }
}
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
using JetBrains.Annotations;
using PixCore.Geometry;

namespace PixUI.Controls
{
    /// <summary>
    /// Specifies a compounded region of axis-aligned rectangles that represent a redraw region request.
    /// </summary>
    public class RedrawRegion: IRegion
    {
        private readonly List<AABB> _rectangles = new List<AABB>();

        public RedrawRegion()
        {

        }

        public RedrawRegion(AABB region, [CanBeNull] ISpatialReference reference)
        {
            AddRectangle(region, reference);
        }

        public RedrawRegion Clone()
        {
            var clone = new RedrawRegion();
            
            clone._rectangles.AddRange(_rectangles);

            return clone;
        }

        /// <summary>
        /// Gets the list of rectangles that this redraw region represents
        /// </summary>
        public IReadOnlyList<AABB> GetRectangles()
        {
            return _rectangles;
        }

        /// <summary>
        /// Clears the contents of this redraw region
        /// </summary>
        public void Clear()
        {
            _rectangles.Clear();
        }

        /// <summary>
        /// Adds a rectangle to this invalidation region
        /// </summary>
        public void AddRectangle(AABB rect, [CanBeNull] ISpatialReference reference)
        {
            if (reference == null)
            {
                _rectangles.Add(rect);
                return;
            }

            _rectangles.Add(reference.ConvertTo(rect, null));
        }

        /// <summary>
        /// Applies a clip to the rectangles on this RedrawRegion so they are all contained within
        /// a given region.
        /// </summary>
        public void ApplyClip(AABB region, [CanBeNull] ISpatialReference reference)
        {
            var clipRegion = region;
            if (reference != null)
            {
                clipRegion = reference.ConvertTo(region, null);
            }

            for (int i = _rectangles.Count - 1; i >= 0; i--)
            {
                var rect = _rectangles[i];
                rect = rect.Intersect(clipRegion);

                if (rect.Validity == AABB.State.Invalid)
                {
                    _rectangles.RemoveAt(i);
                }
                else
                {
                    _rectangles[i] = rect;
                }
            }
        }

        /// <summary>
        /// Makes this redraw region  the union of itself and another redraw region by copying the other redraw
        /// region's rectangles inside this region.
        /// </summary>
        public void Combine([NotNull] RedrawRegion other)
        {
            _rectangles.AddRange(other._rectangles);
        }
    }
}

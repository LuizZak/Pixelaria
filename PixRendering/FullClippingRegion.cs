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

using System.Drawing;
using PixCore.Geometry;

namespace PixRendering
{
    /// <summary>
    /// A special clipping region that behaves as an infinitely large clipping region, where any point or area is
    /// considered within its infinite bounds.
    /// </summary>
    public class FullClippingRegion : IClippingRegion
    {
        public RectangleF TotalRedrawRegion(Size size)
        {
            return new RectangleF(PointF.Empty, size);
        }

        public RectangleF[] RedrawRegionRectangles(Size size)
        {
            return new[] { new RectangleF(PointF.Empty, size) };
        }

        public bool IsVisibleInClippingRegion(Rectangle rectangle)
        {
            return true;
        }

        public bool IsVisibleInClippingRegion(Point point)
        {
            return true;
        }

        public bool IsVisibleInClippingRegion(AABB aabb)
        {
            return true;
        }

        public bool IsVisibleInClippingRegion(Vector point)
        {
            return true;
        }

        public bool IsVisibleInClippingRegion(AABB aabb, ISpatialReference reference)
        {
            return true;
        }

        public bool IsVisibleInClippingRegion(Vector point, ISpatialReference reference)
        {
            return true;
        }
    }
}
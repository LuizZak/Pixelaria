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
using JetBrains.Annotations;

namespace PixCore.Geometry.Algorithms
{
    public class TotalRectanglesArea
    {
        private readonly List<RectangleF> _rectangles = new List<RectangleF>();

        public void AddRectangle(RectangleF rect)
        {
            _rectangles.Add(rect);
        }
        
        public float Calculate()
        {
            float area = 0;

            for (int rectangle = 0; rectangle < _rectangles.Count; rectangle++)
                area += Calculate(_rectangles[rectangle], 1, rectangle + 1);

            return area;
        }

        //A depth-first search for overlaps.
        //Each consecutive overlap alternates inclusionExclusion.
        private float Calculate(RectangleF currentRectangle, int inclusionExclusion, int nextRectangle)
        {
            float area = currentRectangle.Area() * inclusionExclusion;

            for (; nextRectangle < _rectangles.Count; nextRectangle++)
            {
                var other = _rectangles[nextRectangle];
                if(!currentRectangle.IntersectsWith(other))
                    continue;

                var overlap = currentRectangle.Intersection(_rectangles[nextRectangle]);
                area += Calculate(overlap, inclusionExclusion * -1, nextRectangle + 1);
            }

            return area;
        }

        public static float Calculate([NotNull] IEnumerable<RectangleF> items)
        {
            var rects = new TotalRectanglesArea();

            foreach (var item in items)
            {
                rects.AddRectangle(item);
            }

            return rects.Calculate();
        }
    }

    public static class RectangleExtensions
    {
        public static float Area(this RectangleF input)
        {
            return input.Width * input.Height;
        }

        public static RectangleF Intersection(this RectangleF input, RectangleF other)
        {
            return RectangleF.Intersect(input, other);
        }
    }
}

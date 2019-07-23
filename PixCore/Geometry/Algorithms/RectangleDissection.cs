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

namespace PixCore.Geometry.Algorithms
{
    /// <summary>
    /// Class that deals with dissection of two or more rectangles oriented with the same rotation
    /// on a 2D plane into smaller rectangles that fill the exact same area without overlapping.
    /// </summary>
    public class RectangleDissection
    {
        /// <summary>
        /// Performs dissection of two or more rectangles into an array of rectangles that occupy
        /// the same area, and do not intersect.
        /// 
        /// If the rectangles do not intersect, the same input rectangles are returned instead.
        /// </summary>
        public static RectangleF[] Dissect([NotNull] params RectangleF[] rects)
        {
            return Dissect((IEnumerable<RectangleF>)rects);
        }

        /// <summary>
        /// Performs dissection of two or more rectangles into an array of rectangles that occupy
        /// the same area, and do not intersect.
        /// 
        /// If the rectangles do not intersect, the same input rectangles are returned instead.
        /// </summary>
        public static RectangleF[] Dissect([NotNull] IEnumerable<RectangleF> rects)
        {
            var inputSet = PruneEnclosedRectangles(rects);
            if (inputSet.Count == 0)
                return new RectangleF[0];
            if (inputSet.Count == 1)
                return inputSet.ToArray();

            var totalSize = inputSet.Aggregate(inputSet[0], RectangleF.Union);

            // For faster querying of contained regions
            var quadTree = new QuadTree<RectangleF>(totalSize, 10, 5);
            foreach (var rect in inputSet)
            {
                quadTree.AddNode(new QuadTreeElement<RectangleF>(rect, rect));
            }

            var output = new List<RectangleF>();

            var hEdges = SortedHorizontalEdges(inputSet);
            var vEdges = SortedVerticalEdges(inputSet);

            for (int y = 0; y < vEdges.Length - 1; y++)
            {
                var top = vEdges[y];
                var bottom = vEdges[y + 1];

                for (int x = 0; x < hEdges.Length - 1; x++)
                {
                    var left = hEdges[x];
                    var right = hEdges[x + 1];

                    // Form a rectangle
                    var rect = RectangleF.FromLTRB(left.X, top.Y, right.X, bottom.Y);

                    if (quadTree.QueryAabbAny(r => r.Value.Contains(rect), rect))
                        output.Add(rect);
                }
            }
            
            output = new List<RectangleF>(MergeRectangles(output));

            return output.ToArray();
        }
        
        /// <summary>
        /// From a given list of rectangles, returns a new list where rectangles with shared edges
        /// (two shared vertices on either the top, left, right, or bottom sides are the same).
        /// </summary>
        public static RectangleF[] MergeRectangles([NotNull] params RectangleF[] rects)
        {
            return MergeRectangles((IReadOnlyList<RectangleF>)rects);
        }

        /// <summary>
        /// From a given list of rectangles, returns a new list where rectangles with shared edges
        /// (two shared vertices on either the top, left, right, or bottom sides are the same) are
        /// joined such that they form a single rectangle with the same area as the original rectangles.
        /// </summary>
        public static RectangleF[] MergeRectangles([NotNull] IReadOnlyList<RectangleF> rects)
        {
            if (rects.Count == 0)
                return new RectangleF[0];
            if (rects.Count == 1)
                return rects.ToArray();

            var totalSize = rects.Aggregate(rects[0], RectangleF.Union);

            var quadTree = new QuadTree<RectangleF>(totalSize, 10, 10);

            foreach (var rect in rects)
            {
                // Verify any existing rect on quad tree before adding
                var targetRect = rect;

                QuadTreeElement<RectangleF> found = null;
                quadTree.QueryAabb(element =>
                {
                    var other = element.Value;

                    // Check top and bottom
                    if (Math.Abs(rect.Top - other.Bottom) < float.Epsilon || Math.Abs(rect.Bottom - other.Top) < float.Epsilon)
                    {
                        if (Math.Abs(rect.Left - other.Left) < float.Epsilon &&
                            Math.Abs(rect.Right - other.Right) < float.Epsilon)
                        {
                            targetRect = RectangleF.Union(rect, element.Value);
                            found = element;
                            return false;
                        }
                    }

                    // Check left and right
                    if (Math.Abs(rect.Left - other.Right) < float.Epsilon || Math.Abs(rect.Right - other.Left) < float.Epsilon)
                    {
                        if (Math.Abs(rect.Top - other.Top) < float.Epsilon &&
                            Math.Abs(rect.Bottom - other.Bottom) < float.Epsilon)
                        {
                            targetRect = RectangleF.Union(rect, element.Value);
                            found = element;
                            return false;
                        }
                    }

                    return true;
                }, rect);

                if (found != null)
                    quadTree.RemoveNode(found);

                quadTree.AddNode(new QuadTreeElement<RectangleF>(targetRect, targetRect));
            }

            var elements = new List<QuadTreeElement<RectangleF>>();
            quadTree.GetAllNodesR(ref elements);

            return elements.Select(el => el.Value).ToArray();
        }
        
        /// <summary>
        /// Remove rectangles completely contained within other rectangles from the input set and
        /// returns the result a list of rectangles.
        /// </summary>
        /// <param name="rects"></param>
        /// <returns></returns>
        private static List<RectangleF> PruneEnclosedRectangles([NotNull] IEnumerable<RectangleF> rects)
        {
            var output = new List<RectangleF>();

            foreach (var rect in rects)
            {
                if (output.Any(r => r.Contains(rect)))
                    continue;

                output.Add(rect);
            }

            return output;
        }

        private static HorizontalEdge[] SortedHorizontalEdges([NotNull] IEnumerable<RectangleF> rects)
        {
            var set = new HashSet<HorizontalEdge>();
            foreach (var rect in rects)
            {
                set.Add(new HorizontalEdge(rect.Left));
                set.Add(new HorizontalEdge(rect.Right));
            }

            return set.OrderBy(edge => edge.X).ToArray();
        }

        private static VerticalEdge[] SortedVerticalEdges([NotNull] IEnumerable<RectangleF> rects)
        {
            var set = new HashSet<VerticalEdge>();
            foreach (var rect in rects)
            {
                set.Add(new VerticalEdge(rect.Top));
                set.Add(new VerticalEdge(rect.Bottom));
            }

            return set.OrderBy(edge => edge.Y).ToArray();
        }

        private struct HorizontalEdge : IEquatable<HorizontalEdge>
        {
            public float X { get; }

            public HorizontalEdge(float x)
            {
                X = x;
            }

            public bool Equals(HorizontalEdge other)
            {
                return X.Equals(other.X);
            }

            public override bool Equals(object obj)
            {
                return obj is HorizontalEdge other && Equals(other);
            }

            public override int GetHashCode()
            {
                return X.GetHashCode();
            }

            public static bool operator ==(HorizontalEdge left, HorizontalEdge right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(HorizontalEdge left, HorizontalEdge right)
            {
                return !left.Equals(right);
            }
        }

        private struct VerticalEdge : IEquatable<VerticalEdge>
        {
            public float Y { get; }

            public VerticalEdge(float y)
            {
                Y = y;
            }

            public bool Equals(VerticalEdge other)
            {
                return Y.Equals(other.Y);
            }

            public override bool Equals(object obj)
            {
                return obj is VerticalEdge other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Y.GetHashCode();
            }

            public static bool operator ==(VerticalEdge left, VerticalEdge right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(VerticalEdge left, VerticalEdge right)
            {
                return !left.Equals(right);
            }
        }
    }
}

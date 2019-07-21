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
using System.Linq;
using ClipperLib;
using JetBrains.Annotations;

using Poly = System.Collections.Generic.List<ClipperLib.IntPoint>;
using PolyList = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace PixCore.Geometry
{
    /// <summary>
    /// Represents an arbitrary geometric object.
    /// </summary>
    public class PathGeometry
    {
        private const float Scale = 1024;
        private List<Path> _paths = new List<Path>();

        /// <summary>
        /// Creates a new empty path geometry object.
        /// </summary>
        public PathGeometry()
        {

        }

        private PathGeometry(Path path)
        {
            _paths.Add(path);
        }

        /// <summary>
        /// Creates a new path geometry object with a given list of vertices.
        /// </summary>
        public PathGeometry([NotNull] IEnumerable<Vector> vertices)
        {
            _paths.Add(new Path(vertices));
        }

        /// <summary>
        /// Combines this path geometry with another path geometry using a given <see cref="GeometryOperation"/>.
        /// </summary>
        public void Combine([NotNull] PathGeometry other, GeometryOperation operation)
        {
            var clipper = new Clipper();
            
            clipper.AddPolygons(PolygonPoints(), PolyType.ptSubject);
            clipper.AddPolygons(other.PolygonPoints(), PolyType.ptClip);

            var solution = new PolyList();

            switch (operation)
            {
                case GeometryOperation.Union:
                    clipper.Execute(ClipType.ctUnion, solution);
                    break;
                case GeometryOperation.Intersect:
                    clipper.Execute(ClipType.ctIntersection, solution);
                    break;
                case GeometryOperation.Exclude:
                    clipper.Execute(ClipType.ctDifference, solution);
                    break;
                case GeometryOperation.Xor:
                    clipper.Execute(ClipType.ctXor, solution);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }

            _paths = solution.Select(p => new Path(p, Scale)).ToList();
        }

        /// <summary>
        /// Returns a list of polygons described as a list of vertices, which make up this path geometry.
        /// </summary>
        public Vector[][] Polygons()
        {
            return _paths.Select(p => p.vertices.ToArray()).ToArray();
        }

        private List<List<IntPoint>> PolygonPoints()
        {
            return _paths.Select(p => p.ToPoints(Scale)).ToList();
        }

        /// <summary>
        /// Creates a new path geometry object that represents a rectangle with the given dimensions.
        /// </summary>
        public static PathGeometry Rectangle(float x, float y, float width, float height)
        {
            return Rectangle(AABB.FromRectangle(x, y, width, height));
        }

        /// <summary>
        /// Creates a new path geometry object that represents a rectangle with the given area.
        /// </summary>
        public static PathGeometry Rectangle(AABB area)
        {
            return new PathGeometry(area.Corners);
        }

        /// <summary>
        /// Creates a new path geometry that represents a circle with the given center and radius,
        /// with a number of sides making up the polygon-based circle.
        /// </summary>
        public static PathGeometry Circle(Vector center, float radius, int sides)
        {
            var path = new Path();

            double arc = Math.PI * 2 / sides;

            for (int i = 0; i < sides; i++)
            {
                double x = center.X + Math.Cos(arc * i) * radius;
                double y = center.Y + Math.Sin(arc * i) * radius;

                path.vertices.Add(center + new Vector((float)x, (float)y));
            }

            return new PathGeometry(path);
        }

        /// <summary>
        /// Creates a new rounded rectangle geometry with a given area and corner radii.
        /// A parameter <see cref="arcSides"/> can be used to specify how detailed the arced
        /// corners of the rectangle should be.
        /// </summary>
        public static PathGeometry RoundedRectangle(AABB area, float radiusX, float radiusY, int arcSides)
        {
            void AddArc(Vector center, float radX, float radY, double startAngle, double endAngle, IList<Vector> list)
            {
                double step = (endAngle - startAngle) / arcSides;
                for (int i = 0; i < arcSides; i++)
                {
                    double x = center.X + Math.Cos(startAngle + step * i) * radX;
                    double y = center.Y + Math.Sin(startAngle + step * i) * radY;

                    list.Add(new Vector((float)x, (float)y));
                }
            }

            var path = new Path();

            path.vertices.Add(new Vector(area.Left + radiusX, area.Top));
            AddArc(new Vector(area.Right, area.Top) + new Vector(-radiusX, radiusY), radiusX, radiusY, -Math.PI / 2, 0, path.vertices);

            path.vertices.Add(new Vector(area.Right, area.Top + radiusY));
            AddArc(new Vector(area.Right, area.Bottom) + new Vector(-radiusX, -radiusY), radiusX, radiusY, 0, Math.PI / 2, path.vertices);

            path.vertices.Add(new Vector(area.Left + radiusX, area.Bottom));
            AddArc(new Vector(area.Left, area.Bottom) + new Vector(radiusX, -radiusY), radiusX, radiusY, Math.PI / 2, Math.PI, path.vertices);

            path.vertices.Add(new Vector(area.Left, area.Top + radiusY));
            AddArc(new Vector(area.Left, area.Top) + new Vector(radiusX, radiusY), radiusX, radiusY, Math.PI, Math.PI * 2 * 0.75, path.vertices);

            return new PathGeometry(path);
        }

        private class Path
        {
            public readonly List<Vector> vertices = new List<Vector>();

            public Path()
            {

            }

            public Path([NotNull] IEnumerable<Vector> vertices)
            {
                this.vertices = new List<Vector>(vertices);
            }

            public Path([NotNull] IEnumerable<IntPoint> points, float scale)
            {
                vertices = points.Select(p => new Vector(p.X / scale, p.Y / scale)).ToList();
            }

            public Poly ToPoints(float scale)
            {
                return vertices.Select(vertex => new IntPoint((long) (vertex.X * scale), (long) (vertex.Y * scale))).ToList();
            }
        }
    }

    public enum GeometryOperation
    {
        Union,
        Intersect,
        Exclude,
        Xor
    }
}

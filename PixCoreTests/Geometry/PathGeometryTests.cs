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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixSnapshot;

namespace PixCoreTests.Geometry
{
    [TestClass]
    public class PathGeometryTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestUnion()
        {
            // Apply an union operation between two polygons
            //
            //  ._______.
            //  |       |
            //  |       |
            //  !_______!
            //
            var poly1 = PolyGeometry.Rectangle(0, 0, 100, 100);
            //
            //
            //     ._______.
            //     |       |
            //     |       |
            //     !_______!
            //
            var poly2 = PolyGeometry.Rectangle(50, 50, 100, 100);

            poly1.Combine(poly2, GeometryOperation.Union);

            // Expected result:
            //
            //  ._______.
            //  |       !__.
            //  |          |
            //  !___.      |
            //      !______!
            //
            var result = poly1.Polygons();
            Assert.AreEqual(1, result.Length);

            AssertPolygonsMatch(result[0],
                new[]
                {
                    new Vector(0, 0),
                    new Vector(100, 0),
                    new Vector(100, 50),
                    new Vector(150, 50),
                    new Vector(150, 150),
                    new Vector(50, 150),
                    new Vector(50, 100),
                    new Vector(0, 100)
                });
        }

        [TestMethod]
        public void TestIntersect()
        {
            // Apply an union operation between two polygons
            //
            //  ._______.
            //  |       |
            //  |       |
            //  !_______!
            //
            var poly1 = PolyGeometry.Rectangle(0, 0, 100, 100);
            //
            //
            //     ._______.
            //     |       |
            //     |       |
            //     !_______!
            //
            var poly2 = PolyGeometry.Rectangle(50, 50, 100, 100);

            poly1.Combine(poly2, GeometryOperation.Intersect);

            // Expected result:
            //
            //    .____.
            //    |    |
            //    !____!
            //
            var result = poly1.Polygons();
            Assert.AreEqual(1, result.Length);

            AssertPolygonsMatch(result[0],
                new[]
                {
                    new Vector(50, 50),
                    new Vector(100, 50),
                    new Vector(100, 100),
                    new Vector(50, 100),
                });
        }

        [TestMethod]
        public void TestExclusion()
        {
            // Apply an exclusion operation between two polygons
            //
            //  ._______.
            //  |       |
            //  |       |
            //  !_______!
            //
            var poly1 = PolyGeometry.Rectangle(0, 0, 100, 100);
            //
            //
            //     ._______.
            //     |       |
            //     |       |
            //     !_______!
            //
            var poly2 = PolyGeometry.Rectangle(50, 50, 200, 200);

            poly1.Combine(poly2, GeometryOperation.Exclude);

            // Expected result:
            //
            //  ._______.
            //  |    ___!
            //  |   !
            //  !___!
            //
            var result = poly1.Polygons();
            Assert.AreEqual(1, result.Length);

            AssertPolygonsMatch(result[0],
                new[]
                {
                    new Vector(0, 0),
                    new Vector(100, 0),
                    new Vector(100, 50),
                    new Vector(50, 50),
                    new Vector(50, 100),
                    new Vector(0, 100)
                });
        }

        [TestMethod]
        public void TestExclusionThrough()
        {
            // Apply an exclusion operation between two polygons
            //
            //  ._______.
            //  |       |
            //  |       |
            //  !_______!
            //
            var poly1 = PolyGeometry.Rectangle(0, 0, 100, 100);
            //
            //
            //  ._________.
            //  !_________!
            //
            //
            var poly2 = PolyGeometry.Rectangle(0, 50, 200, 25);

            poly1.Combine(poly2, GeometryOperation.Exclude);

            // Expected result:
            //
            //  ._______.
            //  !_______!
            //  ._______.
            //  !_______!
            //
            var result = poly1.Polygons();
            Assert.AreEqual(2, result.Length);

            AssertPolygonsMatch(result[1],
                new[]
                {
                    new Vector(0, 0),
                    new Vector(100, 0),
                    new Vector(100, 50),
                    new Vector(0, 50),
                });

            AssertPolygonsMatch(result[0],
                new[]
                {
                    new Vector(0, 75),
                    new Vector(100, 75),
                    new Vector(100, 100),
                    new Vector(0, 100),
                });
        }

        [TestMethod]
        public void TestXor()
        {
            // Apply an union operation between two polygons
            //
            //  ._______.
            //  |       |
            //  |       |
            //  !_______!
            //
            var poly1 = PolyGeometry.Rectangle(0, 0, 100, 100);
            //
            //
            //     ._______.
            //     |       |
            //     |       |
            //     !_______!
            //
            var poly2 = PolyGeometry.Rectangle(50, 50, 100, 100);

            poly1.Combine(poly2, GeometryOperation.Xor);

            // Expected result:
            //
            //  ._______.
            //  |   .___!__.
            //  |   |xxx|  |
            //  !___!xxx!  |
            //      !______!
            //
            // (where XXX is an empty square
            var result = poly1.Polygons();
            Assert.AreEqual(2, result.Length);

            AssertPolygonsMatch(result[1],
                new[]
                {
                    new Vector(0, 0),
                    new Vector(100, 0),
                    new Vector(100, 50),
                    new Vector(50, 50),
                    new Vector(50, 100),
                    new Vector(0, 100)
                });
            AssertPolygonsMatch(result[0],
                new[]
                {
                    new Vector(100, 50),
                    new Vector(150, 50),
                    new Vector(150, 150),
                    new Vector(50, 150),
                    new Vector(50, 100),
                    new Vector(100, 100),
                });
        }

        [TestMethod]
        public void TestRectangle()
        {
            var path = PolyGeometry.Rectangle(20, 20, 40, 40);

            var result = path.Polygons();

            Assert.AreEqual(1, result.Length);

            AssertPolygonsMatch(result[0],
                new[]
                {
                    new Vector(20, 20),
                    new Vector(60, 20),
                    new Vector(60, 60),
                    new Vector(20, 60),
                });
        }

        [TestMethod]
        public void TestCircle()
        {
            var path = PolyGeometry.Circle(Vector.Zero, 10, 100);

            // Use the formula for the circle's area to figure out if we're off from the goal here
            AssertArea(path.Polygons()[0], (float)(Math.PI * 10 * 10), 1);
        }

        [TestMethod]
        public void TestRoundedRectangle()
        {
            var path = PolyGeometry.RoundedRectangle(AABB.FromRectangle(25, 25, 250, 250), 30, 30, 20);

            var bitmap = ToBitmap(path, 300, 300, Color.Transparent, Color.CornflowerBlue);

            BitmapSnapshot.Snapshot(bitmap, TestContext);
        }

        [TestMethod]
        public void TestRoundedRectangleSeparateRadii()
        {
            var path = PolyGeometry.RoundedRectangle(AABB.FromRectangle(25, 25, 250, 250), 50, 100, 20);

            var bitmap = ToBitmap(path, 300, 300, Color.Transparent, Color.CornflowerBlue);

            BitmapSnapshot.Snapshot(bitmap, TestContext);
        }

        [TestMethod]
        public void TestRoundedRectangleLargeRadius()
        {
            var path = PolyGeometry.RoundedRectangle(AABB.FromRectangle(25, 25, 250, 250), 200, 200, 20);

            var bitmap = ToBitmap(path, 300, 300, Color.Transparent, Color.CornflowerBlue);

            BitmapSnapshot.Snapshot(bitmap, TestContext);
        }

        /// <summary>
        /// Asserts that two list of polygons match. The assertion may shift the list of points,
        /// to allow for acceptance of polygons that are the same, but start on different point
        /// on the point list.
        /// </summary>
        public void AssertPolygonsMatch([NotNull] Vector[] actual, [NotNull] Vector[] expected)
        {
            for (int i = 0; i < actual.Length; i++)
            {
                var shifted = actual.Skip(i).Concat(actual.Take(i));

                if (shifted.SequenceEqual(expected))
                    return;
            }

            Assert.Fail($"List of polygons doesn't match, expected:\n{PolyToString(expected)}\nactual:\n{PolyToString(actual)}");
        }

        /// <summary>
        /// Asserts that a polygon with a given set of vertices has an expected area.
        /// </summary>
        public void AssertArea([NotNull] Vector[] polygon, float expected, float tolerance)
        {
            float area = 0;

            for (int i = 0; i < polygon.Length; i++)
            {
                var cur = polygon[i];
                var next = polygon[(i + 1) % polygon.Length];

                area += cur.Cross(next);
            }

            Assert.AreEqual(area / 2, expected, tolerance);
        }

        private static Bitmap ToBitmap([NotNull] PolyGeometry geometry, int width, int height, Color backColor, Color fillColor)
        {
            var bitmap = new Bitmap(width, height);

            var polygons = geometry.Polygons();

            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(fillColor))
            {
                graphics.Clear(backColor);

                foreach (var poly in polygons)
                {
                    var points = poly.Select(v => (PointF) v).ToArray();

                    graphics.FillPolygon(brush, points);
                }
            }

            return bitmap;
        }

        private static string PolyToString([NotNull] IEnumerable<Vector> points)
        {
            return string.Join(", ", points.Select(v => $"({v.X}, {v.Y})"));
        }
    }
}

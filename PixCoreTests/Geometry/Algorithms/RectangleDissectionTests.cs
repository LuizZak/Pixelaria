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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry.Algorithms;

namespace PixCoreTests.Geometry.Algorithms
{
    [TestClass]
    public class RectangleDissectionTests
    {
        [TestMethod]
        public void TestDissectSingleRectangle()
        {
            var rect1 = new RectangleF(10, 10, 50, 50);

            var ret = RectangleDissection.Dissect(rect1);

            Assert.AreEqual(rect1, ret[0]);
        }

        /// <summary>
        /// Tests dissection of two non-intersecting rectangles.
        /// </summary>
        [TestMethod]
        public void TestDissectNonIntersecting()
        {
            var rect1 = new RectangleF(10, 10, 50, 20);
            var rect2 = new RectangleF(15, 40, 50, 20);

            var ret = RectangleDissection.Dissect(rect1, rect2);
            
            AssertRectanglesDoNotIntersect(ret);
            AssertAreasMatch(new[] { rect1, rect2 }, ret);
        }

        /// <summary>
        /// Tests an intersection where a rectangle is contained within another rectangle,
        /// touching at the top and bottom edges
        /// 
        /// <code>
        ///    |---+------+---|
        ///    |   |      |   |
        ///    |---+------+---|
        /// </code>
        /// </summary>
        [TestMethod]
        public void TestDissectIntersectingTubeInTube()
        {
            var rect1 = new RectangleF(0, 0, 60, 10);
            var rect2 = new RectangleF(20, 0, 20, 10);

            var ret = RectangleDissection.Dissect(rect1, rect2);

            AssertRectanglesDoNotIntersect(ret);
            AssertAreasMatch(new[] { rect1, rect2 }, ret);
        }

        /// <summary>
        /// Tests intersection of rectangles with the following configuration:
        /// 
        /// <code>
        ///        |------|
        ///    |---+------+---|
        ///    |   |------|   |
        ///    |--------------|
        /// </code>
        /// </summary>
        [TestMethod]
        public void TestDissectIntersectingCarDrawing()
        {
            var rect1 = new RectangleF(0, 5, 60, 20);
            var rect2 = new RectangleF(15, 0, 20, 10);

            var ret = RectangleDissection.Dissect(rect1, rect2);
            
            AssertRectanglesDoNotIntersect(ret);
            AssertAreasMatch(new[] { rect1, rect2 }, ret);
        }

        /// <summary>
        /// Tests horizontal intersection of rectangles with the following configuration:
        /// 
        /// <code>
        ///        |------|
        ///    |---+---|  |
        ///    |   |   |  |
        ///    |---+---|  |
        ///        |------|
        /// </code>
        /// </summary>
        [TestMethod]
        public void TestDissectHorizontalIntersection()
        {
            var rect1 = new RectangleF(0, 25, 50, 25);
            var rect2 = new RectangleF(25, 0, 50, 75);

            var ret = RectangleDissection.Dissect(rect1, rect2);
            
            AssertRectanglesDoNotIntersect(ret);
            AssertAreaEquals(4375, ret);
        }

        public static void AssertRectanglesDoNotIntersect(IReadOnlyList<RectangleF> rectangles)
        {
            for (int i = 0; i < rectangles.Count; i++)
            {
                var r1 = rectangles[i];

                for (int j = i + 1; j < rectangles.Count; j++)
                {
                    var r2 = rectangles[j];

                    Assert.IsFalse(r1.IntersectsWith(r2), $"Found overlapping rectangles {r1} {r2}");
                }
            }
        }

        /// <summary>
        /// Makes an assertion that all rectangles combined have the same total area, exclusing overlapping regions.
        /// 
        /// If the assertion fails, an <see cref="Assert.Fail()"/> is raised.
        /// </summary>
        public static void AssertAreasMatch(IReadOnlyList<RectangleF> expected, IReadOnlyList<RectangleF> actual)
        {
            float area1 = TotalRectanglesArea.Calculate(expected);
            float area2 = TotalRectanglesArea.Calculate(actual);

            Assert.AreEqual(area1, area2);
        }

        /// <summary>
        /// Makes an assertion that all rectangles combined have the given area, exclusing overlapping regions.
        /// 
        /// If the assertion fails, an <see cref="Assert.Fail()"/> is raised.
        /// </summary>
        public static void AssertAreaEquals(float expected, IReadOnlyList<RectangleF> rects)
        {
            float area = TotalRectanglesArea.Calculate(rects);

            Assert.AreEqual(expected, area, "Area of rectangles did not match expected value.");
        }
    }
}

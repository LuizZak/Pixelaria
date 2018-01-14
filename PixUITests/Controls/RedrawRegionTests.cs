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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI.Controls;

namespace PixUITests.Controls
{
    [TestClass]
    public class RedrawRegionTests
    {
        [TestMethod]
        public void TestInitialState()
        {
            var sut = new RedrawRegion();

            Assert.AreEqual(0, sut.GetRectangles().Count);
        }

        [TestMethod]
        public void TestInitWithRectangle()
        {
            var rect = new AABB(5, 5, 10, 10);

            var sut = new RedrawRegion(rect, new SpatialReference());
            
            Assert.AreEqual(1, sut.GetRectangles().Count);
            Assert.AreEqual(rect, sut.GetRectangles()[0]);
        }
        
        [TestMethod]
        public void TestClone()
        {
            var rect = new AABB(5, 5, 10, 10);
            var sut = new RedrawRegion();
            sut.AddRectangle(rect, new SpatialReference());

            var clone = sut.Clone();

            Assert.AreNotSame(clone, sut);
            Assert.AreEqual(1, clone.GetRectangles().Count);
            Assert.AreEqual(rect, clone.GetRectangles()[0]);
        }

        [TestMethod]
        public void TestCloneIsSeparateFromOriginalObject()
        {
            var rect = new AABB(5, 5, 10, 10);
            var sut = new RedrawRegion();
            sut.AddRectangle(rect, new SpatialReference());
            var clone = sut.Clone();

            sut.Clear();

            Assert.AreEqual(1, clone.GetRectangles().Count);
            Assert.AreEqual(rect, clone.GetRectangles()[0]);
        }
        
        [TestMethod]
        public void TestAddRectangle()
        {
            var rect = new AABB(5, 5, 10, 10);
            var sut = new RedrawRegion();

            sut.AddRectangle(rect, new SpatialReference());

            Assert.AreEqual(1, sut.GetRectangles().Count);
            Assert.AreEqual(rect, sut.GetRectangles()[0]);
        }
        
        [TestMethod]
        public void TestAddRectangleUsesSpatialReference()
        {
            var rect = new AABB(5, 5, 10, 10);
            var transformedRect = new AABB(10, 10, 20, 20);
            var sut = new RedrawRegion();

            sut.AddRectangle(rect, new SpatialReference(transformedRect));

            Assert.AreEqual(1, sut.GetRectangles().Count);
            Assert.AreEqual(transformedRect, sut.GetRectangles()[0]);
        }

        [TestMethod]
        public void TestCombine()
        {
            var rect1 = new AABB(5, 5, 10, 10);
            var rect2 = new AABB(20, 20, 40, 40);
            var region1 = new RedrawRegion();
            var region2 = new RedrawRegion();
            region1.AddRectangle(rect1, null);
            region2.AddRectangle(rect2, null);

            region1.Combine(region2);
            
            Assert.AreEqual(2, region1.GetRectangles().Count);
            Assert.IsTrue(region1.GetRectangles().Contains(rect1));
            Assert.IsTrue(region1.GetRectangles().Contains(rect2));
        }

        [TestMethod]
        public void TestClear()
        {
            var sut = new RedrawRegion();
            sut.AddRectangle(new AABB(0, 0, 1, 1), null);
            sut.AddRectangle(new AABB(1, 1, 2, 2), null);

            sut.Clear();

            Assert.AreEqual(0, sut.GetRectangles().Count);
        }

        [TestMethod]
        public void TestApplyClip()
        {
            var clipRegion = new AABB(9, 9, 21, 21);
            var sut = new RedrawRegion();
            sut.AddRectangle(new AABB(0, 0, 5, 5), null);
            sut.AddRectangle(new AABB(0, 0, 10, 10), null);
            sut.AddRectangle(new AABB(10, 10, 20, 20), null);
            sut.AddRectangle(new AABB(20, 20, 30, 30), null);
            sut.AddRectangle(new AABB(25, 25, 30, 30), null);

            sut.ApplyClip(clipRegion, null);
            
            Assert.AreEqual(3, sut.GetRectangles().Count);
            Assert.IsTrue(sut.GetRectangles().All(r => clipRegion.Contains(r)));
        }

        private class SpatialReference : ISpatialReference
        {
            private readonly AABB? _aabb;

            public SpatialReference(AABB? aabb = null)
            {
                _aabb = aabb;
            }

            public Vector ConvertFrom(Vector point, ISpatialReference from)
            {
                return point;
            }

            public Vector ConvertTo(Vector point, ISpatialReference to)
            {
                return point;
            }

            public AABB ConvertFrom(AABB aabb, ISpatialReference from)
            {
                return _aabb ?? aabb;
            }

            public AABB ConvertTo(AABB aabb, ISpatialReference to)
            {
                return _aabb ?? aabb;
            }

            public Matrix2D GetAbsoluteTransform()
            {
                return new Matrix2D();
            }
        }
    }
}
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixDirectX.Utils;
using SharpDX.Mathematics.Interop;

namespace PixDirectXTests.Utils
{
    [TestClass]
    public class GeomExtensionsTests
    {
        [TestMethod]
        public void TestToVector()
        {
            var rawVector = new RawVector2(2, 3);

            var vector = rawVector.ToVector();

            Assert.AreEqual(2, vector.X);
            Assert.AreEqual(3, vector.Y);
        }

        [TestMethod]
        public void TestToRawVector2()
        {
            var vector = new Vector(2, 3);

            var rawVector = vector.ToRawVector2();

            Assert.AreEqual(2, rawVector.X);
            Assert.AreEqual(3, rawVector.Y);
        }
        
        // ReSharper disable once InconsistentNaming
        [TestMethod]
        public void TestToAABB()
        {
            var rawRectangleF = new RawRectangleF(2, 1, 4, 5);

            var aabb = rawRectangleF.ToAABB();

            Assert.AreEqual(2, aabb.Left, "Left");
            Assert.AreEqual(1, aabb.Top, "Top");
            Assert.AreEqual(4, aabb.Right, "Right");
            Assert.AreEqual(5, aabb.Bottom, "Bottom");
        }
        
        // ReSharper disable once InconsistentNaming
        [TestMethod]
        public void TestToRawRectangleF()
        {
            var aabb = new AABB(2, 1, 5, 4);

            var rawRectangleF = aabb.ToRawRectangleF();
            
            Assert.AreEqual(2, rawRectangleF.Left, "Left");
            Assert.AreEqual(1, rawRectangleF.Top, "Top");
            Assert.AreEqual(4, rawRectangleF.Right, "Right");
            Assert.AreEqual(5, rawRectangleF.Bottom, "Bottom");
        }
    }
}
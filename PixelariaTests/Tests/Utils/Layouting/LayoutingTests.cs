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
using Pixelaria.Utils;
using Pixelaria.Utils.Layouting;

namespace PixelariaTests.Tests.Utils.Layouting
{
    [TestClass]
    public class LayoutingTests
    {
        [TestMethod]
        public void TestWithCenterOn()
        {
            var aabb = new AABB(new Vector(0, 0), new Vector(10, 10));

            aabb = aabb.WithCenterOn(new Vector(20, 20));
            
            Assert.AreEqual(new Vector(15, 15), aabb.Minimum);
            Assert.AreEqual(new Vector(25, 25), aabb.Maximum);
        }

        [TestMethod]
        public void TestCenterWithinContainer()
        {
            var aabb = new AABB(new Vector(10, 10), new Vector(20, 20));
            var container = new AABB(new Vector(0, 0), new Vector(100, 50));

            var result1 = LayoutingHelper.CenterWithinContainer(aabb, container, LayoutDirection.Horizontal);
            var result2 = LayoutingHelper.CenterWithinContainer(aabb, container, LayoutDirection.Vertical);
            var result3 = LayoutingHelper.CenterWithinContainer(aabb, container, LayoutDirection.Horizontal | LayoutDirection.Vertical);

            Assert.AreEqual(new Vector(45, 10), result1.Minimum);
            Assert.AreEqual(new Vector(55, 20), result1.Maximum);
            
            Assert.AreEqual(new Vector(10, 20), result2.Minimum);
            Assert.AreEqual(new Vector(20, 30), result2.Maximum);

            Assert.AreEqual(new Vector(45, 20), result3.Minimum);
            Assert.AreEqual(new Vector(55, 30), result3.Maximum);
        }
    }
}

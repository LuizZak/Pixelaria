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

using Blend2DCS;
using Blend2DCS.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blend2DCSTests
{
    [TestClass]
    public class BLContextTests
    {
        [TestMethod]
        public void TestUserMatrix()
        {
            var sut = CreateContext();

            var matrix = sut.UserMatrix;

            Assert.AreEqual(new BLMatrix2D(1, 0, 0, 1, 0, 0), matrix);
        }

        [TestMethod]
        public void TestSetMatrix()
        {
            var matrix = new BLMatrix2D(3, 5, 5, 3, 10, 100);
            var sut = CreateContext();

            sut.SetMatrix(matrix);

            Assert.AreEqual(matrix, sut.UserMatrix);
        }

        private static BLContext CreateContext()
        {
            var image = new BLImage(32, 32, BLFormat.Prgb32);
            return new BLContext(image);
        }
    }
}

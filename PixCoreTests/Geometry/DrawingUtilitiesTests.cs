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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;

namespace PixCoreTests.Geometry
{
    [TestClass]
    public class DrawingUtilitiesTests
    {
        [TestMethod]
        public void TestCenterRectangle()
        {
            var rect = new Rectangle(5, 5, 20, 10);

            Assert.AreEqual(new Point(15, 10), rect.Center());
        }

        [TestMethod]
        public void TestCenterRectangleF()
        {
            var rect = new RectangleF(5.0f, 5.0f, 23.0f, 13.0f);

            Assert.AreEqual(new PointF(16.5F, 11.5F), rect.Center());
        }
    }
}
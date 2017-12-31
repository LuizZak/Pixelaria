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
using PixCore.Text;

namespace PixCoreTests.Text
{
    [TestClass]
    public class TextRangeTests
    {
        [TestMethod]
        public void TestEquals()
        {
            Assert.AreEqual(new TextRange(0, 0), new TextRange(0, 0));
            Assert.AreEqual(new TextRange(10, 0), new TextRange(10, 0));
            Assert.AreEqual(new TextRange(10, 10), new TextRange(10, 10));
            Assert.AreEqual(new TextRange(0, 10), new TextRange(0, 10));

            Assert.AreNotEqual(new TextRange(10, 0), new TextRange(0, 0));
            Assert.AreNotEqual(new TextRange(10, 10), new TextRange(10, 0));
            Assert.AreNotEqual(new TextRange(0, 10), new TextRange(10, 10));
            Assert.AreNotEqual(new TextRange(10, 0), new TextRange(0, 10));
        }

        [TestMethod]
        public void TestEqualsAsObjects()
        {
            Assert.AreEqual((object)new TextRange(0, 0), new TextRange(0, 0));
            Assert.AreEqual((object)new TextRange(10, 0), new TextRange(10, 0));
            Assert.AreEqual((object)new TextRange(10, 10), new TextRange(10, 10));
            Assert.AreEqual((object)new TextRange(0, 10), new TextRange(0, 10));

            Assert.AreNotEqual((object)new TextRange(10, 0), new TextRange(0, 0));
            Assert.AreNotEqual((object)new TextRange(10, 10), new TextRange(10, 0));
            Assert.AreNotEqual((object)new TextRange(0, 10), new TextRange(10, 10));
            Assert.AreNotEqual((object)new TextRange(10, 0), new TextRange(0, 10));
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            Assert.AreEqual(new TextRange(0, 0).GetHashCode(), new TextRange(0, 0).GetHashCode());
            Assert.AreEqual(new TextRange(10, 0).GetHashCode(), new TextRange(10, 0).GetHashCode());
            Assert.AreEqual(new TextRange(10, 10).GetHashCode(), new TextRange(10, 10).GetHashCode());
            Assert.AreEqual(new TextRange(0, 10).GetHashCode(), new TextRange(0, 10).GetHashCode());

            Assert.AreNotEqual(new TextRange(10, 0).GetHashCode(), new TextRange(0, 0).GetHashCode());
            Assert.AreNotEqual(new TextRange(10, 10).GetHashCode(), new TextRange(10, 0).GetHashCode());
            Assert.AreNotEqual(new TextRange(0, 10).GetHashCode(), new TextRange(10, 10).GetHashCode());
            Assert.AreNotEqual(new TextRange(10, 0).GetHashCode(), new TextRange(0, 10).GetHashCode());
        }

        [TestMethod]
        public void TestEnd()
        {
            Assert.AreEqual(0, new TextRange(0, 0).End);
            Assert.AreEqual(2, new TextRange(0, 2).End);
            Assert.AreEqual(4, new TextRange(2, 2).End);
            Assert.AreEqual(2, new TextRange(2, 0).End);
        }

        [TestMethod]
        public void TestIntersection()
        {
            Assert.AreEqual(null, new TextRange(0, 5).Intersection(new TextRange(5, 5)));
            Assert.AreEqual(null, new TextRange(5, 10).Intersection(new TextRange(0, 5)));

            Assert.AreEqual(new TextRange(0, 1), new TextRange(0, 1).Intersection(new TextRange(0, 1)));
            Assert.AreEqual(new TextRange(5, 5), new TextRange(5, 10).Intersection(new TextRange(5, 5)));

            Assert.AreEqual(new TextRange(0, 5), new TextRange(-5, 20).Intersection(new TextRange(0, 5)));
        }

        [TestMethod]
        public void TestOverlap()
        {
            Assert.AreEqual(null, new TextRange(0, 5).Overlap(new TextRange(6, 5)));
            Assert.AreEqual(null, new TextRange(5, 10).Overlap(new TextRange(0, 4)));

            Assert.AreEqual(new TextRange(5, 0), new TextRange(0, 5).Overlap(new TextRange(5, 5)));
            Assert.AreEqual(new TextRange(5, 0), new TextRange(5, 10).Overlap(new TextRange(0, 5)));

            Assert.AreEqual(new TextRange(0, 1), new TextRange(0, 1).Overlap(new TextRange(0, 1)));
            Assert.AreEqual(new TextRange(5, 5), new TextRange(5, 10).Overlap(new TextRange(5, 5)));

            Assert.AreEqual(new TextRange(0, 5), new TextRange(-5, 20).Overlap(new TextRange(0, 5)));
        }
    }
}

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
using PixCore.Text.Attributes;

namespace PixCoreTests.Text.Attributes
{
    [TestClass]
    public class ForegroundColorAttributeTests
    {
        [TestMethod]
        public void TestInstantiate()
        {
            var sut = new ForegroundColorAttribute(Color.AliceBlue);

            Assert.AreEqual(Color.AliceBlue, sut.ForeColor);
        }

        [TestMethod]
        public void TestClone()
        {
            var original = new ForegroundColorAttribute(Color.AliceBlue);
            var clone = (ForegroundColorAttribute)original.Clone();

            Assert.AreEqual(original.ForeColor, clone.ForeColor);
        }

        [TestMethod]
        public void TestEquals()
        {
            var attributeSame1 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeSame2 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeDifferent = new ForegroundColorAttribute(Color.Red);

            Assert.IsTrue(attributeSame1.Equals(attributeSame2));
            Assert.IsFalse(attributeSame1.Equals(attributeDifferent));
        }

        [TestMethod]
        public void TestEqualsObject()
        {
            var attributeSame1 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeSame2 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeDifferent = new ForegroundColorAttribute(Color.Red);

            Assert.IsTrue(attributeSame1.Equals((object)attributeSame2));
            Assert.IsFalse(attributeSame1.Equals((object)attributeDifferent));
            Assert.IsFalse(attributeSame1.Equals(null));
        }

        [TestMethod]
        public void TestEqualsOperator()
        {
            var attributeSame1 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeSame2 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeDifferent = new ForegroundColorAttribute(Color.Red);

            Assert.IsTrue(attributeSame1 == attributeSame2);
            Assert.IsFalse(attributeSame1 != attributeSame2);
            Assert.IsFalse(attributeSame1 == attributeDifferent);
            Assert.IsTrue(attributeSame1 != attributeDifferent);
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var attributeSame1 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeSame2 = new ForegroundColorAttribute(Color.AliceBlue);
            var attributeDifferent = new ForegroundColorAttribute(Color.Red);

            Assert.AreEqual(attributeSame1.GetHashCode(), attributeSame2.GetHashCode());
            Assert.AreNotEqual(attributeSame1.GetHashCode(), attributeDifferent.GetHashCode());
        }
    }
}
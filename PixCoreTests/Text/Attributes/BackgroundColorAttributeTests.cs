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
using PixCore.Text.Attributes;

namespace PixCoreTests.Text.Attributes
{
    [TestClass]
    public class BackgroundColorAttributeTests
    {
        [TestMethod]
        public void TestInstantiate()
        {
            var color = new BackgroundColorAttribute(Color.AliceBlue);
            var colorAndInflation = new BackgroundColorAttribute(Color.AliceBlue, Vector.Unit);

            Assert.AreEqual(Color.AliceBlue, color.BackColor);
            Assert.AreEqual(Vector.Zero, color.Inflation);
            Assert.AreEqual(Color.AliceBlue, colorAndInflation.BackColor);
            Assert.AreEqual(Vector.Unit, colorAndInflation.Inflation);
        }
        
        [TestMethod]
        public void TestClone()
        {
            var original = new BackgroundColorAttribute(Color.AliceBlue, Vector.Unit);
            var clone = (BackgroundColorAttribute)original.Clone();

            Assert.AreEqual(original.BackColor, clone.BackColor);
            Assert.AreEqual(original.Inflation, clone.Inflation);
        }

        [TestMethod]
        public void TestEquals()
        {
            var attributeSame1 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Zero);
            var attributeSame2 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Zero);
            var attributeDifferent1 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Unit);
            var attributeDifferent2 = new BackgroundColorAttribute(Color.Red, Vector.Zero);

            Assert.IsTrue(attributeSame1.Equals(attributeSame2));
            Assert.IsFalse(attributeSame1.Equals(attributeDifferent1));
            Assert.IsFalse(attributeSame1.Equals(attributeDifferent2));
        }

        [TestMethod]
        public void TestEqualsObject()
        {
            var attributeSame1 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Zero);
            var attributeSame2 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Zero);
            var attributeDifferent1 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Unit);
            var attributeDifferent2 = new BackgroundColorAttribute(Color.Red, Vector.Zero);

            Assert.IsTrue(attributeSame1.Equals((object)attributeSame2));
            Assert.IsFalse(attributeSame1.Equals((object)attributeDifferent1));
            Assert.IsFalse(attributeSame1.Equals((object)attributeDifferent2));
            Assert.IsFalse(attributeSame1.Equals(null));
        }

        [TestMethod]
        public void TestEqualsOperator()
        {
            var attributeSame1 = new BackgroundColorAttribute(Color.AliceBlue);
            var attributeSame2 = new BackgroundColorAttribute(Color.AliceBlue);
            var attributeDifferent1 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Unit);
            var attributeDifferent2 = new BackgroundColorAttribute(Color.Red, Vector.Zero);

            Assert.IsTrue(attributeSame1 == attributeSame2);
            Assert.IsFalse(attributeSame1 != attributeSame2);
            Assert.IsFalse(attributeSame1 == attributeDifferent1);
            Assert.IsTrue(attributeSame1 != attributeDifferent1);
            Assert.IsFalse(attributeSame1 == attributeDifferent2);
            Assert.IsTrue(attributeSame1 != attributeDifferent2);
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var attributeSame1 = new BackgroundColorAttribute(Color.AliceBlue);
            var attributeSame2 = new BackgroundColorAttribute(Color.AliceBlue);
            var attributeDifferent1 = new BackgroundColorAttribute(Color.AliceBlue, Vector.Unit);
            var attributeDifferent2 = new BackgroundColorAttribute(Color.Red, Vector.Zero);

            Assert.AreEqual(attributeSame1.GetHashCode(), attributeSame2.GetHashCode());
            Assert.AreNotEqual(attributeSame1.GetHashCode(), attributeDifferent1.GetHashCode());
            Assert.AreNotEqual(attributeSame1.GetHashCode(), attributeDifferent2.GetHashCode());
        }
    }
}
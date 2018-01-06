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
    public class TextFontAttributeTests
    {
        [TestMethod]
        public void TestInstantiate()
        {
            var font = new Font(FontFamily.GenericSerif, 12.0f);
            var sut = new TextFontAttribute(font);

            Assert.AreEqual(font, sut.Font);
        }

        [TestMethod]
        public void TestClone()
        {
            var font = new Font(FontFamily.GenericSerif, 12.0f);
            var original = new TextFontAttribute(font);
            var clone = (TextFontAttribute)original.Clone();

            Assert.AreEqual(original.Font, clone.Font);
        }

        [TestMethod]
        public void TestEquals()
        {
            var font1 = new Font(FontFamily.GenericSerif, 12.0f);
            var font2 = new Font(FontFamily.GenericSerif, 14.0f);
            var attributeSame1 = new TextFontAttribute(font1);
            var attributeSame2 = new TextFontAttribute(font1);
            var attributeDifferent = new TextFontAttribute(font2);

            Assert.IsTrue(attributeSame1.Equals(attributeSame2));
            Assert.IsFalse(attributeSame1.Equals(attributeDifferent));
        }

        [TestMethod]
        public void TestEqualsObject()
        {
            var font1 = new Font(FontFamily.GenericSerif, 12.0f);
            var font2 = new Font(FontFamily.GenericSerif, 14.0f);
            var attributeSame1 = new TextFontAttribute(font1);
            var attributeSame2 = new TextFontAttribute(font1);
            var attributeDifferent = new TextFontAttribute(font2);

            Assert.IsTrue(attributeSame1.Equals((object)attributeSame2));
            Assert.IsFalse(attributeSame1.Equals((object)attributeDifferent));
            Assert.IsFalse(attributeSame1.Equals(null));
        }

        [TestMethod]
        public void TestEqualsOperator()
        {
            var font1 = new Font(FontFamily.GenericSerif, 12.0f);
            var font2 = new Font(FontFamily.GenericSerif, 14.0f);
            var attributeSame1 = new TextFontAttribute(font1);
            var attributeSame2 = new TextFontAttribute(font1);
            var attributeDifferent = new TextFontAttribute(font2);

            Assert.IsTrue(attributeSame1 == attributeSame2);
            Assert.IsFalse(attributeSame1 != attributeSame2);
            Assert.IsFalse(attributeSame1 == attributeDifferent);
            Assert.IsTrue(attributeSame1 != attributeDifferent);
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var font1 = new Font(FontFamily.GenericSerif, 12.0f);
            var font2 = new Font(FontFamily.GenericSerif, 14.0f);
            var attributeSame1 = new TextFontAttribute(font1);
            var attributeSame2 = new TextFontAttribute(font1);
            var attributeDifferent = new TextFontAttribute(font2);

            Assert.AreEqual(attributeSame1.GetHashCode(), attributeSame2.GetHashCode());
            Assert.AreNotEqual(attributeSame1.GetHashCode(), attributeDifferent.GetHashCode());
        }
    }
}
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
using PixUI.Text;

namespace PixUITests.Text
{
    [TestClass]
    public class StringTextualBufferTests
    {
        [TestMethod]
        public void TestTextLength()
        {
            Assert.AreEqual(0, new StringTextualBuffer("").TextLength);
            Assert.AreEqual(2, new StringTextualBuffer("ab").TextLength);
            Assert.AreEqual(5, new StringTextualBuffer("abcde").TextLength);
        }

        [TestMethod]
        public void TestTextInRange()
        {
            Assert.AreEqual("ab", new StringTextualBuffer("abcdef").TextInRange(new TextRange(0, 2)));
            Assert.AreEqual("bc", new StringTextualBuffer("abcdef").TextInRange(new TextRange(1, 2)));
            Assert.AreEqual("abcdef", new StringTextualBuffer("abcdef").TextInRange(new TextRange(0, 6)));
        }

        [TestMethod]
        public void TestCharacterAtOffset()
        {
            Assert.AreEqual('a', new StringTextualBuffer("abcdef").CharacterAtOffset(0));
            Assert.AreEqual('c', new StringTextualBuffer("abcdef").CharacterAtOffset(2));
            Assert.AreEqual('f', new StringTextualBuffer("abcdef").CharacterAtOffset(5));
        }

        [TestMethod]
        public void TestAppend()
        {
            var sut = new StringTextualBuffer("abc");

            sut.Append("def");

            Assert.AreEqual("abcdef", sut.Text);
        }

        [TestMethod]
        public void TestInsert()
        {
            var sut = new StringTextualBuffer("abc");

            sut.Insert(1, "def");

            Assert.AreEqual("adefbc", sut.Text);
        }

        [TestMethod]
        public void TestReplace()
        {
            var sut = new StringTextualBuffer("abc");

            sut.Replace(1, 2, "def");

            Assert.AreEqual("adef", sut.Text);
        }

        [TestMethod]
        public void TestDelete()
        {
            var sut = new StringTextualBuffer("abc");

            sut.Delete(0, 3);

            Assert.AreEqual("", sut.Text);
        }

        [TestMethod]
        public void TestDeleteRange()
        {
            var sut = new StringTextualBuffer("abc");

            sut.Delete(1, 1);

            Assert.AreEqual("ac", sut.Text);
        }
    }
}

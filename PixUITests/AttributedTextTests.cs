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
using PixUI;
using PixUI.Controls.Text;

namespace PixUITests
{
    [TestClass]
    public class AttributedTextTests
    {
        [TestMethod]
        public void TestInitialState()
        {
            var sut = new AttributedText();

            Assert.AreEqual(0, sut.Length);
            Assert.AreEqual("", sut.String);
            Assert.IsFalse(sut.HasAttributes);
        }

        [TestMethod]
        public void TestAppend()
        {
            var sut = new AttributedText();

            sut.Append("abc");

            var segments = sut.GetTextSegments();

            Assert.AreEqual(3, sut.Length);
            Assert.AreEqual("abc", sut.String);
            Assert.IsFalse(sut.HasAttributes);

            Assert.AreEqual(1, segments.Length);
            Assert.AreEqual("abc", segments[0].Text);
            Assert.AreEqual(new TextRange(0, 3), segments[0].TextRange);
            Assert.AreEqual(0, segments[0].TextAttributes.Length);
        }

        [TestMethod]
        public void TestAppendSequential()
        {
            var sut = new AttributedText();

            sut.Append("abc");
            sut.Append("def");

            var segments = sut.GetTextSegments();

            Assert.AreEqual(6, sut.Length);
            Assert.AreEqual("abcdef", sut.String);
            Assert.IsFalse(sut.HasAttributes);

            Assert.AreEqual(2, segments.Length);
            Assert.AreEqual("abc", segments[0].Text);
            Assert.AreEqual("def", segments[1].Text);
            Assert.AreEqual(new TextRange(0, 3), segments[0].TextRange);
            Assert.AreEqual(new TextRange(3, 3), segments[1].TextRange);
            Assert.AreEqual(0, segments[0].TextAttributes.Length);
            Assert.AreEqual(0, segments[1].TextAttributes.Length);
        }

        [TestMethod]
        public void TestAppendWithAttribute()
        {
            var sut = new AttributedText();

            sut.Append("abc", new TestAttribute());

            var segments = sut.GetTextSegments();

            Assert.AreEqual(3, sut.Length);
            Assert.AreEqual("abc", sut.String);
            Assert.IsTrue(sut.HasAttributes);

            Assert.AreEqual(1, segments.Length);
            Assert.AreEqual("abc", segments[0].Text);
            Assert.AreEqual(new TextRange(0, 3), segments[0].TextRange);
            Assert.AreEqual(1, segments[0].TextAttributes.Length);
            Assert.IsInstanceOfType(segments[0].TextAttributes[0], typeof(TestAttribute));
        }

        [TestMethod]
        public void TestAppendWithAttributes()
        {
            var sut = new AttributedText();

            sut.Append("abc", new ITextAttribute[] {new TestAttribute(), new TestAttribute2()});

            var segments = sut.GetTextSegments();

            Assert.AreEqual(3, sut.Length);
            Assert.AreEqual("abc", sut.String);
            Assert.IsTrue(sut.HasAttributes);

            Assert.AreEqual(1, segments.Length);
            Assert.AreEqual("abc", segments[0].Text);
            Assert.AreEqual(new TextRange(0, 3), segments[0].TextRange);
            Assert.AreEqual(2, segments[0].TextAttributes.Length);
            Assert.IsInstanceOfType(segments[0].TextAttributes[0], typeof(TestAttribute));
            Assert.IsInstanceOfType(segments[0].TextAttributes[1], typeof(TestAttribute2));
        }

        [TestMethod]
        public void TestSetText()
        {
            var sut = new AttributedText();

            sut.Append("abc", new ITextAttribute[] { new TestAttribute(), new TestAttribute2() });

            sut.SetText("def");

            var segments = sut.GetTextSegments();

            Assert.AreEqual(3, sut.Length);
            Assert.AreEqual("def", sut.String);
            Assert.IsFalse(sut.HasAttributes);

            Assert.AreEqual(1, segments.Length);
            Assert.AreEqual("def", segments[0].Text);
            Assert.AreEqual(new TextRange(0, 3), segments[0].TextRange);
            Assert.AreEqual(0, segments[0].TextAttributes.Length);
        }

        [TestMethod]
        public void TestSetAttributes()
        {
            var sut = new AttributedText();

            sut.SetText("abcdef");

            sut.SetAttributes(new TextRange(3, 3), new ITextAttribute[] {new TestAttribute()});
            
            var segments = sut.GetTextSegments();

            Assert.AreEqual(6, sut.Length);
            Assert.AreEqual("abcdef", sut.String);
            Assert.IsTrue(sut.HasAttributes);

            Assert.AreEqual(2, segments.Length);
            Assert.AreEqual("abc", segments[0].Text);
            Assert.AreEqual("def", segments[1].Text);
            Assert.AreEqual(new TextRange(0, 3), segments[0].TextRange);
            Assert.AreEqual(new TextRange(3, 3), segments[1].TextRange);
            Assert.AreEqual(0, segments[0].TextAttributes.Length);
            Assert.AreEqual(1, segments[1].TextAttributes.Length);
            Assert.IsInstanceOfType(segments[1].TextAttributes[0], typeof(TestAttribute));
        }

        [TestMethod]
        public void TestSetAttributesRangeMiddle()
        {
            var sut = new AttributedText();

            sut.SetText("abcdef");
            
            // a b c d e f
            // 0 1 2 3 4 5
            //   1
            //     1 2 3
            sut.SetAttributes(new TextRange(1, 3), new ITextAttribute[] { new TestAttribute() });

            var segments = sut.GetTextSegments();

            Assert.AreEqual(6, sut.Length);
            Assert.AreEqual("abcdef", sut.String);
            Assert.IsTrue(sut.HasAttributes);

            Assert.AreEqual(3, segments.Length);
            Assert.AreEqual("a", segments[0].Text);
            Assert.AreEqual("bcd", segments[1].Text);
            Assert.AreEqual("ef", segments[2].Text);
            Assert.AreEqual(new TextRange(0, 1), segments[0].TextRange);
            Assert.AreEqual(new TextRange(1, 3), segments[1].TextRange);
            Assert.AreEqual(new TextRange(4, 2), segments[2].TextRange);
            Assert.AreEqual(0, segments[0].TextAttributes.Length);
            Assert.AreEqual(1, segments[1].TextAttributes.Length);
            Assert.AreEqual(0, segments[2].TextAttributes.Length);
            Assert.IsInstanceOfType(segments[1].TextAttributes[0], typeof(TestAttribute));
        }

        private class TestAttribute : ITextAttribute
        {
            public object Clone()
            {
                return new TestAttribute();
            }
        }

        private class TestAttribute2 : ITextAttribute
        {
            public object Clone()
            {
                return new TestAttribute2();
            }
        }
    }
}

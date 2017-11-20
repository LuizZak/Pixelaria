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

using Pixelaria.Views.ExportPipeline.PipelineView;
using Pixelaria.Views.ExportPipeline.PipelineView.Controls;
using Rhino.Mocks;

namespace PixelariaTests.Tests.Views.ExportPipeline.PipelineView.Controls
{
    [TestClass]
    public class TextEngineTests
    {
        [TestMethod]
        public void TestStartState()
        {
            var buffer = new TextBuffer("Test");
            var sut = new TextEngine(buffer);

            Assert.AreEqual(new TextRange(0, 0), sut.CaretRange, "Should start with caret at beginning of text");
            Assert.AreEqual(buffer, sut.TextBuffer, "Should properly assign passed in text buffer");
        }

        [TestMethod]
        public void TestMoveRight()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.MoveRight();

            Assert.AreEqual(new TextRange(1, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestMoveRightStopsAtEndOfText()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.MoveRight();
            sut.MoveRight();
            sut.MoveRight();
            sut.MoveRight(); // Should not move right any further

            Assert.AreEqual(new TextRange(3, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestMoveLeft()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(3);

            sut.MoveLeft();

            Assert.AreEqual(new TextRange(2, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestMoveLeftStopsAtBeginningOfText()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(3);

            sut.MoveLeft();
            sut.MoveLeft();
            sut.MoveLeft();
            sut.MoveLeft(); // Should not move right any further

            Assert.AreEqual(new TextRange(0, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestMoveToEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.MoveToEnd();

            Assert.AreEqual(new TextRange(3, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestMoveToEndIdempotent()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.MoveToEnd();
            sut.MoveToEnd();

            Assert.AreEqual(new TextRange(3, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestMoveToStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(3);

            sut.MoveToStart();

            Assert.AreEqual(new TextRange(0, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestMoveToStartIdempotent()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(3);

            sut.MoveToStart();
            sut.MoveToStart();

            Assert.AreEqual(new TextRange(0, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestSetCaret()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(1, 2));

            Assert.AreEqual(new TextRange(1, 2), sut.CaretRange);
        }

        [TestMethod]
        public void TestSetCaretOffset()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            
            sut.SetCaret(new TextRange(2, 2));

            sut.SetCaret(1);

            Assert.AreEqual(new TextRange(1, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestSetCaretOutOfBoundsStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(-5, 0));

            // Cap at start
            Assert.AreEqual(new TextRange(0, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestSetCaretOutOfBoundsEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(10, 5));

            // Cap at end
            Assert.AreEqual(new TextRange(3, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestSetCaretOutOfBounds()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(-5, 10));

            // Cap at whole available range
            Assert.AreEqual(new TextRange(0, 3), sut.CaretRange);
        }

        [TestMethod]
        public void TestInsertTextCaretAtEnd()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();

            stub.Stub(b => b.TextLength).Return(3);
            stub.Expect(b => b.Append("456"));
            
            var sut = new TextEngine(stub);

            sut.InsertText("456");

            stub.VerifyAllExpectations();
            Assert.AreEqual(new TextRange(3, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestInsertTextCaretNotAtEnd()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();

            stub.Stub(b => b.TextLength).Return(3);
            stub.Expect(b => b.Insert(0, "456"));

            var sut = new TextEngine(stub);

            sut.SetCaret(0);

            sut.InsertText("456");
            
            stub.VerifyAllExpectations();
            Assert.AreEqual(new TextRange(3, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestInsertTextWithSelection()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();

            stub.Stub(b => b.TextLength).Return(3);
            stub.Expect(b => b.Replace(1, 2, "456"));

            var sut = new TextEngine(stub);

            sut.SetCaret(new TextRange(1, 2));

            sut.InsertText("456");

            stub.VerifyAllExpectations();
            Assert.AreEqual(new TextRange(3, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestBackspace()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();

            stub.Stub(b => b.TextLength).Return(3);
            stub.Expect(b => b.Delete(2, 1));

            var sut = new TextEngine(stub);
            
            sut.SetCaret(3);

            sut.BackspaceText();

            stub.VerifyAllExpectations();
            Assert.AreEqual(new TextRange(2, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestBackspaceAtBeginningHasNoEffect()
        {
            var stub = MockRepository.GenerateStrictMock<ITextEngineTextualBuffer>();
            
            var sut = new TextEngine(stub);

            sut.BackspaceText();

            stub.VerifyAllExpectations();
            Assert.AreEqual(new TextRange(0, 0), sut.CaretRange);
        }

        [TestMethod]
        public void TestBackspaceWithRange()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();

            stub.Stub(b => b.TextLength).Return(3);
            stub.Expect(b => b.Delete(1, 2));

            var sut = new TextEngine(stub);

            sut.SetCaret(new TextRange(1, 2));

            sut.BackspaceText();

            stub.VerifyAllExpectations();
            Assert.AreEqual(new TextRange(1, 0), sut.CaretRange);
        }

        internal class TextBuffer : ITextEngineTextualBuffer
        {
            public string Text { get; set; }

            public int TextLength => Text.Length;

            public TextBuffer(string value)
            {
                Text = value;
            }

            public string TextInRange(TextRange range)
            {
                return Text.Substring(range.Start, range.Length);
            }

            public void Delete(int index, int length)
            {
                Text = Text.Remove(index, length);
            }

            public void Insert(int index, string text)
            {
                Text = Text.Insert(index, text);
            }

            public void Append(string text)
            {
                Text += text;
            }

            public void Replace(int index, int length, string text)
            {
                Text = Text.Remove(index, length).Insert(index, text);
            }
        }
    }
}

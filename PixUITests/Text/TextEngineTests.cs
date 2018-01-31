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
using PixUI.Text;
using Rhino.Mocks;

namespace PixUITests.Text
{
    [TestClass]
    public class TextEngineTests
    {
        [TestMethod]
        public void TestStartState()
        {
            var buffer = new TextBuffer("Test");

            var sut = new TextEngine(buffer);

            Assert.AreEqual(new Caret(0), sut.Caret, "Should start with caret at beginning of text");
            Assert.AreEqual(buffer, sut.TextBuffer, "Should properly assign passed in text buffer");
        }
        
        #region Selected Text
        
        [TestMethod]
        public void TestSelectedTextEmptyText()
        {
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer);

            string text = sut.SelectedText();

            Assert.AreEqual("", text);
        }

        [TestMethod]
        public void TestSelectedTextEmptyRange()
        {
            var buffer = new TextBuffer("Abcdef");
            var sut = new TextEngine(buffer);

            string text = sut.SelectedText();

            Assert.AreEqual("", text);
        }

        [TestMethod]
        public void TestSelectedTextPartialRange()
        {
            var buffer = new TextBuffer("Abcdef");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(2, 3));

            Assert.AreEqual("cde", sut.SelectedText());
        }

        [TestMethod]
        public void TestSelectedTextPartialRangeToEnd()
        {
            var buffer = new TextBuffer("Abcdef");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(2, 4));

            Assert.AreEqual("cdef", sut.SelectedText());
        }

        [TestMethod]
        public void TestSelectedTextFullRange()
        {
            var buffer = new TextBuffer("Abcdef");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(0, 6));

            Assert.AreEqual("Abcdef", sut.SelectedText());
        }

        [TestMethod]
        public void TestSelectedTextInvokesTextBuffer()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(6);
            var sut = new TextEngine(stub);
            sut.SetCaret(new TextRange(2, 3));

            sut.SelectedText();

            stub.AssertWasCalled(b => b.TextInRange(new TextRange(2, 3)));
        }

        #endregion

        #region Move

        [TestMethod]
        public void TestMoveRight()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.MoveRight();

            Assert.AreEqual(new Caret(1), sut.Caret);
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

            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestMoveRightWithSelectionAtEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(0, 2), CaretPosition.End));

            sut.MoveRight();

            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestMoveRightWithSelectionAtStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(0, 2), CaretPosition.Start));

            sut.MoveRight();

            Assert.AreEqual(new Caret(1), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeft()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.MoveLeft();

            Assert.AreEqual(new Caret(2), sut.Caret);
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

            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeftWithSelectionAtEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(1, 2), CaretPosition.End));

            sut.MoveLeft();

            Assert.AreEqual(new Caret(2), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeftWithSelectionAtStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(1, 2), CaretPosition.Start));

            sut.MoveLeft();

            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestMoveToEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.MoveToEnd();

            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestMoveToEndIdempotent()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.MoveToEnd();
            sut.MoveToEnd();

            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestMoveToStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.MoveToStart();

            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestMoveToStartIdempotent()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.MoveToStart();
            sut.MoveToStart();

            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        #endregion

        #region Selection Move

        [TestMethod]
        public void TestSelectRight()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SelectRight();

            Assert.AreEqual(new Caret(new TextRange(0, 1), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSelectRightStopsAtEndOfText()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SelectRight();
            sut.SelectRight();
            sut.SelectRight();
            sut.SelectRight(); // Should not move right any further

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSelectRightWithSelection()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(0, 2), CaretPosition.Start));

            sut.SelectRight();

            Assert.AreEqual(new Caret(new TextRange(1, 1), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectLeft()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.SelectLeft();

            Assert.AreEqual(new Caret(new TextRange(2, 1), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectLeftStopsAtBeginningOfText()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.SelectLeft();
            sut.SelectLeft();
            sut.SelectLeft();
            sut.SelectLeft(); // Should not move left any further

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectToEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SelectToEnd();

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSelectToEndIdempotent()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SelectToEnd();
            sut.SelectToEnd();

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSelectToEndWithSelectionAtStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(0, 2), CaretPosition.Start));

            sut.SelectToEnd();

            Assert.AreEqual(new Caret(new TextRange(2, 1), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSelectToStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.SelectToStart();
            
            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectToStartIdempotent()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.SelectToStart();
            sut.SelectToStart();

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectToStartWithSelectionAtEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(1, 2), CaretPosition.End));

            sut.SelectToStart();

            Assert.AreEqual(new Caret(new TextRange(0, 1), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestMoveCaretSelecting()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(1);

            sut.MoveCaretSelecting(2);

            Assert.AreEqual(new Caret(new TextRange(1, 1), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestMoveCaretSelectingLeft()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(2);

            sut.MoveCaretSelecting(1);

            Assert.AreEqual(new Caret(new TextRange(1, 1), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestMoveCaretSelectingSamePosition()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(1);

            sut.MoveCaretSelecting(1);

            Assert.AreEqual(new Caret(new TextRange(1, 0), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestMoveCaretSelectingSamePositionStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(1, 1), CaretPosition.Start));

            sut.MoveCaretSelecting(3);

            Assert.AreEqual(new Caret(new TextRange(2, 1), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestMoveCaretSelectingSamePositionEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(1, 2), CaretPosition.End));

            sut.MoveCaretSelecting(0);

            Assert.AreEqual(new Caret(new TextRange(0, 1), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectAll()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(1, 2), CaretPosition.End));

            sut.SelectAll();

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.End), sut.Caret);
        }

        #endregion

        #region Move Word

        [TestMethod]
        public void TestMoveRightWordEndOfWord()
        {
            var buffer = new TextBuffer("Abc Def");
            var sut = new TextEngine(buffer);

            sut.MoveRightWord();

            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestMoveRightWordBeginningOfNextWord()
        {
            var buffer = new TextBuffer("Abc   Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.MoveRightWord();

            Assert.AreEqual(new Caret(6), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeftWordBeginningOfWord()
        {
            var buffer = new TextBuffer("Abc Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(6);

            sut.MoveLeftWord();

            Assert.AreEqual(new Caret(4), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeftWordBeginningOfFirstWord()
        {
            var buffer = new TextBuffer("Abc Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.MoveLeftWord();

            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeftWordEndOfPreviousWord()
        {
            var buffer = new TextBuffer("Abc   Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(6);

            sut.MoveLeftWord();

            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeftWordBeginningOfWordCaretAtEnd()
        {
            // Tests moving to the previous word when the caret is currently just after the end
            // of a word

            var buffer = new TextBuffer("Abc def ghi");
            var sut = new TextEngine(buffer);
            sut.SetCaret(7);

            sut.MoveLeftWord();

            Assert.AreEqual(new Caret(new TextRange(4, 0), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestMoveLeftAtBeginningOfText()
        {
            // Tests moving a word to the left when at the beginning of the text stream

            var buffer = new TextBuffer("Abc");
            var sut = new TextEngine(buffer);
            sut.SetCaret(0);

            sut.MoveLeftWord();

            Assert.AreEqual(new Caret(new TextRange(0, 0), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestMoveRightAtEndOfText()
        {
            // Tests moving a word to the right when at the end of the text stream

            var buffer = new TextBuffer("Abc");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.MoveRightWord();

            Assert.AreEqual(new Caret(new TextRange(3, 0), CaretPosition.Start), sut.Caret);
        }

        #endregion

        #region Selection Move Word

        [TestMethod]
        public void TestSelectRightWordEndOfWord()
        {
            var buffer = new TextBuffer("Abc Def");
            var sut = new TextEngine(buffer);

            sut.SelectRightWord();

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSelectRightWordBeginningOfNextWord()
        {
            var buffer = new TextBuffer("Abc   Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.SelectRightWord();

            Assert.AreEqual(new Caret(new TextRange(3, 3), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSelectLeftWordBeginningOfWord()
        {
            var buffer = new TextBuffer("Abc Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(7);

            sut.SelectLeftWord();

            Assert.AreEqual(new Caret(new TextRange(4, 3), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectLeftWordBeginningOfFirstWord()
        {
            var buffer = new TextBuffer("Abc Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.SelectLeftWord();

            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectLeftWordBeginningOfPreviousWord()
        {
            var buffer = new TextBuffer("Abc   Def");
            var sut = new TextEngine(buffer);
            sut.SetCaret(6);

            sut.SelectLeftWord();

            Assert.AreEqual(new Caret(new TextRange(0, 6), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSelectLeftWordBeginningOfWordCaretAtEnd()
        {
            // Tests selecting the previous word when the caret is currently just after the end
            // of a word

            var buffer = new TextBuffer("Abc def ghi");
            var sut = new TextEngine(buffer);
            sut.SetCaret(7);

            sut.SelectLeftWord();

            Assert.AreEqual(new Caret(new TextRange(4, 3), CaretPosition.Start), sut.Caret);
        }

        #endregion

        #region Word Segment In

        [TestMethod]
        public void TestWordSegmentIn()
        {
            var buffer = new TextBuffer("Abc def ghi");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(5);

            Assert.AreEqual(new TextRange(4, 3), segment);
        }

        [TestMethod]
        public void TestWordSegmentInEmptyString()
        {
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(0);

            Assert.AreEqual(new TextRange(0, 0), segment);
        }

        [TestMethod]
        public void TestWordSegmentInAtStartOfWord()
        {
            var buffer = new TextBuffer("Abc def ghi");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(4);

            Assert.AreEqual(new TextRange(4, 3), segment);
        }

        [TestMethod]
        public void TestWordSegmentInAtEndOfWord()
        {
            var buffer = new TextBuffer("Abc def ghi");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(7);

            Assert.AreEqual(new TextRange(4, 3), segment);
        }

        [TestMethod]
        public void TestWordSegmentInOverWhitespace()
        {
            var buffer = new TextBuffer("Abc   ghi");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(4);

            Assert.AreEqual(new TextRange(3, 3), segment);
        }

        [TestMethod]
        public void TestWordSegmentInSingleWordText()
        {
            var buffer = new TextBuffer("Abcdef");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(3);

            Assert.AreEqual(new TextRange(0, 6), segment);
        }

        [TestMethod]
        public void TestWordSegmentInSingleWhitespaceText()
        {
            var buffer = new TextBuffer("      ");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(3);

            Assert.AreEqual(new TextRange(0, 6), segment);
        }

        [TestMethod]
        public void TestWordSegmentInBeginningOfString()
        {
            var buffer = new TextBuffer("Abcdef");
            var sut = new TextEngine(buffer);

            var segment = sut.WordSegmentIn(0);

            Assert.AreEqual(new TextRange(0, 6), segment);
        }

        #endregion

        #region Set Caret

        [TestMethod]
        public void TestSetCaret()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new Caret(new TextRange(1, 2), CaretPosition.End));

            Assert.AreEqual(new Caret(new TextRange(1, 2), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestSetCaretTextRange()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new TextRange(1, 2));

            Assert.AreEqual(new Caret(new TextRange(1, 2), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestSetCaretOffset()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new TextRange(2, 2));

            sut.SetCaret(1);

            Assert.AreEqual(new Caret(1), sut.Caret);
        }

        [TestMethod]
        public void TestSetCaretOutOfBoundsStart()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new Caret(new TextRange(-5, 0), CaretPosition.Start));

            // Cap at start
            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestSetCaretOutOfBoundsEnd()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new Caret(new TextRange(10, 5), CaretPosition.Start));

            // Cap at end
            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestSetCaretOutOfBounds()
        {
            var buffer = new TextBuffer("123");
            var sut = new TextEngine(buffer);

            sut.SetCaret(new Caret(new TextRange(-5, 10), CaretPosition.Start));

            // Cap at whole available range
            Assert.AreEqual(new Caret(new TextRange(0, 3), CaretPosition.Start), sut.Caret);
        }

        #endregion

        #region Update Caret From TextBuffer

        [TestMethod]
        public void TestUpdateCaretFromTextBuffer()
        {
            var buffer = new TextBuffer("123456");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(6, 0), CaretPosition.End));
            buffer.Text = "123";

            sut.UpdateCaretFromTextBuffer();

            Assert.AreEqual(new Caret(new TextRange(3, 0), CaretPosition.End), sut.Caret);
        }

        [TestMethod]
        public void TestUpdateCaretFromTextBufferWhileStillInBounds()
        {
            var buffer = new TextBuffer("123456");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new Caret(new TextRange(1, 2), CaretPosition.End));
            buffer.Text = "123";

            sut.UpdateCaretFromTextBuffer();

            Assert.AreEqual(new Caret(new TextRange(1, 2), CaretPosition.End), sut.Caret);
        }

        #endregion

        #region Insert Text

        [TestMethod]
        public void TestInsertTextCaretAtEnd()
        {
            int length = 0;
            var stub = MockRepository.GenerateStrictMock<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength)
                .WhenCalled(inv => inv.ReturnValue = length)
                .Return(0).TentativeReturn();
            stub.Stub(b => b.Append("456")).WhenCalled(_ => length = 3);
            var sut = new TextEngine(stub);
            
            sut.InsertText("456");

            stub.AssertWasCalled(b => b.Append("456"));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestInsertTextCaretNotAtEnd()
        {
            var stub = MockRepository.GenerateMock<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            sut.SetCaret(0);

            sut.InsertText("456");
            
            stub.AssertWasCalled(b => b.Insert(0, "456"));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestInsertTextWithSelection()
        {
            int length = 3;
            var stub = MockRepository.GenerateMock<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength)
                .WhenCalled(inv => inv.ReturnValue = length)
                .Return(0).TentativeReturn();
            stub.Stub(b => b.Replace(1, 2, "456")).WhenCalled(_ => length = 5);
            var sut = new TextEngine(stub);
            sut.SetCaret(new TextRange(1, 2));

            sut.InsertText("456");
            
            stub.AssertWasCalled(b => b.Replace(1, 2, "456"));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(4), sut.Caret);
        }

        #endregion

        #region Backspace

        [TestMethod]
        public void TestBackspace()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            sut.SetCaret(3);

            sut.BackspaceText();
            
            stub.AssertWasCalled(b => b.Delete(2, 1));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(2), sut.Caret);
        }

        [TestMethod]
        public void TestBackspaceAtBeginningHasNoEffect()
        {
            var stub = MockRepository.GenerateStrictMock<ITextEngineTextualBuffer>();
            var sut = new TextEngine(stub);

            sut.BackspaceText();

            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestBackspaceWithRange()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            sut.SetCaret(new TextRange(1, 2));

            sut.BackspaceText();

            stub.AssertWasCalled(b => b.Delete(1, 2));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(1), sut.Caret);
        }

        [TestMethod]
        public void TestBackspaceAtBeginningWithRange()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            sut.SetCaret(new TextRange(0, 3));

            sut.BackspaceText();

            stub.AssertWasCalled(b => b.Delete(0, 3));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        #endregion

        #region Delete

        [TestMethod]
        public void TestDelete()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            
            sut.DeleteText();

            stub.AssertWasCalled(b => b.Delete(0, 1));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        [TestMethod]
        public void TestDeleteAtEndHasNoEffect()
        {
            var stub = MockRepository.GenerateStrictMock<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            sut.SetCaret(3);

            sut.DeleteText();

            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(3), sut.Caret);
        }

        [TestMethod]
        public void TestDeleteWithRange()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            sut.SetCaret(new TextRange(1, 2));

            sut.DeleteText();

            stub.AssertWasCalled(b => b.Delete(1, 2));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(1), sut.Caret);
        }

        [TestMethod]
        public void TestDeleteAtEndWithRange()
        {
            var stub = MockRepository.GenerateStub<ITextEngineTextualBuffer>();
            stub.Stub(b => b.TextLength).Return(3);
            var sut = new TextEngine(stub);
            sut.SetCaret(new Caret(new TextRange(0, 3), CaretPosition.End));

            sut.DeleteText();

            stub.AssertWasCalled(b => b.Delete(0, 3));
            stub.VerifyAllExpectations();
            Assert.AreEqual(new Caret(0), sut.Caret);
        }

        #endregion

        #region Copy/Cut/Paste

        [TestMethod]
        public void TestCopy()
        {
            var mock = MockRepository.GenerateMock<ITextClipboard>();
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer) {TextClipboard = mock};
            sut.SetCaret(new TextRange(1, 2));

            sut.Copy();

            mock.AssertWasCalled(m => m.SetText("bc"));
        }

        [TestMethod]
        public void TestCopyNotCalledWhenNoSelectionRangeAvailable()
        {
            var mock = MockRepository.GenerateStrictMock<ITextClipboard>();
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer) { TextClipboard = mock };
            sut.SetCaret(new TextRange(1, 0));

            sut.Copy();

            mock.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestCut()
        {
            var mock = MockRepository.GenerateMock<ITextClipboard>();
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer) { TextClipboard = mock };
            sut.SetCaret(new TextRange(1, 2));

            sut.Cut();

            mock.AssertWasCalled(m => m.SetText("bc"));
            Assert.AreEqual(buffer.Text, "a");
        }

        [TestMethod]
        public void TestCutNotCalledWhenNoSelectionRangeAvailable()
        {
            var mock = MockRepository.GenerateStrictMock<ITextClipboard>();
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer) { TextClipboard = mock };
            sut.SetCaret(new TextRange(1, 0));

            sut.Cut();

            mock.VerifyAllExpectations();
            Assert.AreEqual(buffer.Text, "abc");
        }

        [TestMethod]
        public void TestPaste()
        {
            var stub = MockRepository.GenerateStub<ITextClipboard>();
            stub.Stub(s => s.ContainsText()).Return(true);
            stub.Stub(s => s.GetText()).Return("def");
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer) { TextClipboard = stub };
            sut.SetCaret(new TextRange(3, 0));

            sut.Paste();

            stub.AssertWasCalled(m => m.ContainsText());
            stub.AssertWasCalled(m => m.GetText());
            Assert.AreEqual(buffer.Text, "abcdef");
        }

        [TestMethod]
        public void TestPasteNotCalledWhenNoTextAvailable()
        {
            var stub = MockRepository.GenerateStub<ITextClipboard>();
            stub.Stub(s => s.ContainsText()).Return(false);
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer) { TextClipboard = stub };
            sut.SetCaret(new TextRange(3, 0));

            sut.Paste();

            stub.AssertWasCalled(m => m.ContainsText());
            stub.AssertWasNotCalled(m => m.GetText());
            Assert.AreEqual(buffer.Text, "abc");
        }

        [TestMethod]
        public void TestPasteReplacesSelectionRange()
        {
            var stub = MockRepository.GenerateStub<ITextClipboard>();
            stub.Stub(s => s.ContainsText()).Return(true);
            stub.Stub(s => s.GetText()).Return("def");
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer) { TextClipboard = stub };
            sut.SetCaret(new TextRange(1, 2));

            sut.Paste();
            
            Assert.AreEqual(buffer.Text, "adef");
        }

        #endregion

        #region Undo Operations
        
        /// <summary>
        /// Tests that multiple sequential 1-char long InsertText calls are properly undone
        /// as a single operation
        /// </summary>
        [TestMethod]
        public void TestInsertTextUndo()
        {
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer);
            sut.InsertText("a");
            sut.InsertText("b");
            sut.InsertText("c");

            sut.UndoSystem.Undo();

            Assert.AreEqual("", buffer.Text);
        }

        /// <summary>
        /// Tests that combining insert text undo operations into one only occur when the insertions
        /// are located one after another sequentially.
        /// </summary>
        [TestMethod]
        public void TestInsertTextUndoSequenceBreaksIfNotSequential()
        {
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer);
            sut.InsertText("a");
            sut.InsertText("b");
            sut.InsertText("c");
            sut.SetCaret(2);
            sut.InsertText("d");

            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }

        /// <summary>
        /// Sequential insertions should be chained even if calls to SetCaret (but only SetCaret) are 
        /// made between insertions, so long as the next text inserted is right after the previous inserted
        /// string.
        /// </summary>
        [TestMethod]
        public void TestInsertTextUndoSequenceDoesntBreakWhenCallingSetCaret()
        {
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer);
            sut.InsertText("a");
            sut.InsertText("b");
            sut.InsertText("c");
            sut.SetCaret(2);
            sut.SetCaret(3);
            sut.InsertText("d");

            sut.UndoSystem.Undo();

            Assert.AreEqual("", buffer.Text);
        }

        /// <summary>
        /// Tests that calling <see cref="TextEngine.Paste"/> interrupts insert undo sequences such 
        /// that it's considered a distinct input undo operation from the characters being input so
        /// far.
        /// </summary>
        [TestMethod]
        public void TestInsertTextUndoSequenceBreaksAfterPaste()
        {
            var clipboard = MockRepository.GenerateStub<ITextClipboard>();
            clipboard.Stub(c => c.GetText()).Return("d");
            clipboard.Stub(c => c.ContainsText()).Return(true);
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer) {TextClipboard = clipboard};
            sut.InsertText("a");
            sut.InsertText("b");
            sut.InsertText("c");
            sut.Paste();

            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }

        /// <summary>
        /// Tests that calling <see cref="TextEngine.DeleteText"/> interrupts insert undo sequences.
        /// </summary>
        [TestMethod]
        public void TestInsertTextUndoSequenceBreaksAfterDeleteText()
        {
            var buffer = new TextBuffer("e");
            var sut = new TextEngine(buffer);
            sut.InsertText("a");
            sut.InsertText("b");
            sut.InsertText("c");
            sut.DeleteText();
            sut.InsertText("d");

            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }

        /// <summary>
        /// Tests that calling <see cref="TextEngine.BackspaceText"/> interrupts insert undo sequences.
        /// </summary>
        [TestMethod]
        public void TestInsertTextUndoSequenceBreaksAfterBackspaceText()
        {
            var buffer = new TextBuffer("e");
            var sut = new TextEngine(buffer);
            sut.InsertText("a");
            sut.InsertText("b");
            sut.InsertText("c");
            sut.SetCaret(4);
            sut.BackspaceText();
            sut.InsertText("d");

            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }

        [TestMethod]
        public void TestBackspaceTextUndo()
        {
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer);
            sut.SetCaret(3);

            sut.BackspaceText();
            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }

        [TestMethod]
        public void TestBackspaceTextRangeUndo()
        {
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new TextRange(1, 2), CaretPosition.End);

            sut.BackspaceText();
            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }

        [TestMethod]
        public void TestDeleteTextUndo()
        {
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer);

            sut.DeleteText();
            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }

        [TestMethod]
        public void TestDeleteTextRangeUndo()
        {
            var buffer = new TextBuffer("abc");
            var sut = new TextEngine(buffer);
            sut.SetCaret(new TextRange(1, 2), CaretPosition.End);

            sut.DeleteText();
            sut.UndoSystem.Undo();

            Assert.AreEqual("abc", buffer.Text);
        }
        
        [TestMethod]
        public void TestBackwardsInsertionPlusDeleteUndo()
        {
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer);
            sut.InsertText("c");
            sut.SetCaret(0);
            sut.InsertText("b");
            sut.SetCaret(0);
            sut.InsertText("a");
            sut.SetCaret(0);
            sut.UndoSystem.Undo();
            sut.InsertText("a");
            sut.SetCaret(0);

            sut.UndoSystem.Undo();
            sut.UndoSystem.Undo();
            sut.UndoSystem.Undo();

            Assert.AreEqual("", buffer.Text);
        }

        [TestMethod]
        public void TestPasteUndoRespectsSelectedRegion()
        {
            var paste = new TestClipboard("test");
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer) {TextClipboard = paste};
            sut.InsertText("T");
            sut.InsertText("e");
            sut.SelectToStart();
            sut.Paste();
            
            sut.UndoSystem.Undo();

            Assert.AreEqual(new Caret(new TextRange(0, 2), CaretPosition.Start), sut.Caret);
        }

        [TestMethod]
        public void TestReplacingPasteUndoAndRedo()
        {
            var paste = new TestClipboard("test");
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer) { TextClipboard = paste };
            sut.InsertText("T");
            sut.InsertText("e");
            sut.SelectToStart();
            sut.Paste();

            sut.UndoSystem.Undo();
            sut.UndoSystem.Undo();
            sut.UndoSystem.Redo();
            sut.UndoSystem.Redo();

            Assert.AreEqual("test", buffer.Text);
        }

        [TestMethod]
        public void TestPasteBreaksTextInsertUndoChain()
        {
            var paste = new TestClipboard("test");
            var buffer = new TextBuffer("");
            var sut = new TextEngine(buffer) { TextClipboard = paste };
            sut.Paste();
            sut.InsertText("t");

            sut.UndoSystem.Undo();

            Assert.AreEqual("test", buffer.Text);
        }

        #region Text Insert Undo Task

        [TestMethod]
        public void TestTextInsertUndo()
        {
            const string text = "abc";
            var caret = new Caret(new TextRange(1, 3), CaretPosition.Start);
            var mock = MockRepository.GenerateStrictMock<ITextEngine>();
            mock.Expect(m => m.SetCaret(caret));
            mock.Expect(m => m.DeleteText());
            var sut = new TextInsertUndo(mock, caret, "", text);

            sut.Undo();

            mock.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestTextInsertRedo()
        {
            var caret = new Caret(new TextRange(1, 3), CaretPosition.Start);
            const string text = "abc";
            var mock = MockRepository.GenerateStrictMock<ITextEngine>();
            mock.Expect(m => m.SetCaret(caret));
            mock.Expect(m => m.InsertText(text));
            var sut = new TextInsertUndo(mock, caret, "", text);

            sut.Redo();

            mock.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestTextInsertUndoExpectedText()
        {
            const string beforeText = "abcdef";
            const string afterText = "agef";
            const string replacedText = "bcd";
            const string newText = "g";
            var buffer = new TextBuffer(afterText);
            var engine = new TextEngine(buffer);
            var caret = new Caret(new TextRange(1, 3), CaretPosition.Start);
            var sut = new TextInsertUndo(engine, caret, replacedText, newText);

            sut.Undo();

            Assert.AreEqual(beforeText, buffer.Text);
            Assert.AreEqual(caret, sut.Caret);
        }

        [TestMethod]
        public void TestTextInsertRedoExpectedText()
        {
            const string beforeText = "abcdef";
            const string afterText = "agef";
            const string replacedText = "bcd";
            const string newText = "g";
            var buffer = new TextBuffer(beforeText);
            var engine = new TextEngine(buffer);
            var caret = new Caret(new TextRange(1, 3), CaretPosition.End);
            var sut = new TextInsertUndo(engine, caret, replacedText, newText);

            sut.Redo();

            Assert.AreEqual(afterText, buffer.Text);
            Assert.AreEqual(new Caret(new TextRange(2, 0), CaretPosition.Start), engine.Caret);
        }

        #endregion

        #region Text Delete Undo Task

        [TestMethod]
        public void TestTextDeteleUndo()
        {
            const string text = "abc";
            var caret = new Caret(new TextRange(1, 3), CaretPosition.Start);
            var mock = MockRepository.GenerateStrictMock<ITextEngine>();
            mock.Expect(m => m.SetCaret(new Caret(caret.Location)));
            mock.Expect(m => m.InsertText(text));
            mock.Expect(m => m.SetCaret(caret));
            var sut = new TextDeleteUndo(mock, caret, caret.TextRange, text);

            sut.Undo();

            mock.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestTextRedoUndo()
        {
            const string text = "abc";
            var caret = new Caret(new TextRange(1, 3), CaretPosition.Start);
            var mock = MockRepository.GenerateStrictMock<ITextEngine>();
            mock.Expect(m => m.SetCaret(caret));
            mock.Expect(m => m.DeleteText());
            var sut = new TextDeleteUndo(mock, caret, caret.TextRange, text);

            sut.Redo();

            mock.VerifyAllExpectations();
        }
        
        [TestMethod]
        public void TestTextDeleteUndoExpectedTextCaretAtStart()
        {
            const string beforeText = "abcdef";
            const string afterText = "aef";
            const string deletedText = "bcd";
            var buffer = new TextBuffer(afterText);
            var engine = new TextEngine(buffer);
            var caret = new Caret(new TextRange(1, 3), CaretPosition.Start);
            var sut = new TextDeleteUndo(engine, caret, caret.TextRange, deletedText);

            sut.Undo();

            Assert.AreEqual(beforeText, buffer.Text);
            Assert.AreEqual(new Caret(new TextRange(1, 3), CaretPosition.Start), engine.Caret);
        }

        [TestMethod]
        public void TestTextDeleteUndoExpectedTextCaretAtEnd()
        {
            const string beforeText = "abcdef";
            const string afterText = "aef";
            const string deletedText = "bcd";
            var buffer = new TextBuffer(afterText);
            var engine = new TextEngine(buffer);
            var caret = new Caret(new TextRange(1, 3), CaretPosition.End);
            var sut = new TextDeleteUndo(engine, caret, caret.TextRange, deletedText);

            sut.Undo();

            Assert.AreEqual(beforeText, buffer.Text);
            Assert.AreEqual(new Caret(new TextRange(1, 3), CaretPosition.End), engine.Caret);
        }

        [TestMethod]
        public void TestTextDeleteRedoExpectedTextCaretAtStart()
        {
            const string beforeText = "abcdef";
            const string afterText = "aef";
            const string deletedText = "bcd";
            var buffer = new TextBuffer(beforeText);
            var engine = new TextEngine(buffer);
            var caret = new Caret(new TextRange(1, 3), CaretPosition.Start);
            var sut = new TextDeleteUndo(engine, caret, caret.TextRange, deletedText);

            sut.Redo();

            Assert.AreEqual(afterText, buffer.Text);
            Assert.AreEqual(new Caret(new TextRange(1, 0), CaretPosition.Start), engine.Caret);
        }

        [TestMethod]
        public void TestTextDeleteRedoExpectedTextCaretAtEnd()
        {
            const string beforeText = "abcdef";
            const string afterText = "aef";
            const string deletedText = "bcd";
            var buffer = new TextBuffer(beforeText);
            var engine = new TextEngine(buffer);
            var caret = new Caret(new TextRange(1, 3), CaretPosition.End);
            var sut = new TextDeleteUndo(engine, caret, caret.TextRange, deletedText);

            sut.Redo();

            Assert.AreEqual(afterText, buffer.Text);
            Assert.AreEqual(new Caret(new TextRange(1, 0), CaretPosition.Start), engine.Caret);
        }

        [TestMethod]
        public void TestTextDeleteUndoRespectsUndoCaretLocation()
        {
            const string beforeText = "abcdef";
            const string afterText = "abcde";
            const string deletedText = "f";
            var buffer = new TextBuffer(afterText);
            var engine = new TextEngine(buffer);
            var caretBefore = new Caret(new TextRange(6, 0), CaretPosition.Start);
            var rangeRemoved = new TextRange(5, 1);
            var sut = new TextDeleteUndo(engine, caretBefore, rangeRemoved, deletedText);

            sut.Undo();

            Assert.AreEqual(beforeText, buffer.Text);
            Assert.AreEqual(caretBefore, engine.Caret);
        }

        #endregion

        #endregion

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

            public char CharacterAtOffset(int offset)
            {
                return Text[offset];
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

        internal class TestClipboard : ITextClipboard
        {
            public string Value { get; set; }
            
            public TestClipboard(string value = "")
            {
                Value = value;
            }

            public string GetText()
            {
                return Value;
            }

            public void SetText(string text)
            {
                Value = text;
            }

            public bool ContainsText()
            {
                return true;
            }
        }
    }
}

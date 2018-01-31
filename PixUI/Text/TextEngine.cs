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

using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixCore.Text;
using PixCore.Undo;

namespace PixUI.Text
{
    /// <summary>
    /// A text + caret engine that handles manipulation of strings by insertion/removal of strings
    /// at locations that can be specified via a caret position.
    /// 
    /// Base text input engine backing for <see cref="Controls.TextField"/>'s.
    /// </summary>
    public class TextEngine : ITextEngine
    {
        /// <summary>
        /// Whenever sequential characters are input into the text engine via <see cref="InsertText"/>,
        /// this undo run is incremented so the undo operation for these operations occur as one single 
        /// undo for multiple characters.
        /// </summary>
        [CanBeNull]
        private TextInsertUndo _currentInputUndoRun;

        /// <summary>
        /// When undoing/redoing work with <see cref="_undoSystem"/>, this flag is temporarely set to true so no
        /// undo tasks are accidentally registered while another undo task performs changes to this text engine.
        /// </summary>
        private bool _isPerformingUndoRedo;

        private readonly UndoSystem _undoSystem;

        /// <summary>
        /// Event handler for <see cref="CaretChanged"/> event.
        /// </summary>
        public delegate void TextEngineCaretChangedEventHandler(object sender, TextEngineCaretChangedEventArgs e);

        /// <summary>
        /// Event fired whenever the current <see cref="Caret"/> value is changed.
        /// </summary>
        public event TextEngineCaretChangedEventHandler CaretChanged;

        /// <summary>
        /// Gets the internal undo system that this text engine records undo and redo operations in
        /// </summary>
        [NotNull]
        public IUndoSystem UndoSystem => _undoSystem;

        /// <summary>
        /// The text buffer that receives instructions to add/remove/replace text based on caret
        /// inputs handled by this text engine.
        /// </summary>
        public ITextEngineTextualBuffer TextBuffer { get; }

        /// <summary>
        /// Gets the text clipboard for this text engine to use during Copy/Cut/Paste operations.
        /// 
        /// Defaults to <see cref="WindowsTextClipboard"/>, but can be replaced at any time with any
        /// other implementation.
        /// </summary>
        [NotNull]
        public ITextClipboard TextClipboard { get; set; } = new WindowsTextClipboard();

        /// <summary>
        /// Gets the caret range.
        /// 
        /// To change the caret range, use one of the SetCaret() methods.
        /// </summary>
        public Caret Caret { get; private set; } = new Caret(new TextRange(0, 0), CaretPosition.Start);

        public TextEngine(ITextEngineTextualBuffer textBuffer)
        {
            TextBuffer = textBuffer;

            _undoSystem = new UndoSystem {MaximumTaskCount = 30};

            _undoSystem.WillPerformUndo += (sender, args) => { _isPerformingUndoRedo = true; };
            _undoSystem.WillPerformRedo += (sender, args) => { _isPerformingUndoRedo = true; };

            _undoSystem.UndoPerformed += (sender, args) => { _isPerformingUndoRedo = false; };
            _undoSystem.RedoPerformed += (sender, args) => { _isPerformingUndoRedo = false; };
        }

        /// <summary>
        /// Requests that this text engine reload its caret after a change to a text buffer's contents or length
        /// </summary>
        public void UpdateCaretFromTextBuffer()
        {
            if (Caret.Start > TextBuffer.TextLength || Caret.End > TextBuffer.TextLength)
            {
                SetCaret(Caret);
            }
        }

        private void RegisterUndo([NotNull] IUndoTask task)
        {
            if (_isPerformingUndoRedo)
                return;

            _undoSystem.RegisterUndo(task);
        }

        /// <summary>
        /// If any text insert undo is currently present under <see cref="_currentInputUndoRun"/>,
        /// this method flushes it into <see cref="_undoSystem"/> and resets the text undo run so new
        /// undo runs are started fresh.
        /// </summary>
        private void FlushTextInsertUndo()
        {
            if (_isPerformingUndoRedo)
                return;

            _undoSystem.FinishGroupUndo();
            _currentInputUndoRun = null;
        }

        /// <summary>
        /// Updates <see cref="_currentInputUndoRun"/> with a new character at a specified offset.
        /// If the offset is not exactly at the end of the current undo run, a new undo run is started
        /// and the current undo run is flushed (via <see cref="FlushTextInsertUndo"/>).
        /// </summary>
        private void UpdateTextInsertUndo(string replacing, string text, Caret caret)
        {
            if (_isPerformingUndoRedo)
                return;

            var current = _currentInputUndoRun;
            if (current == null)
            {
                _undoSystem.StartGroupUndo("Insert text");
            }
            else if (replacing.Length != 0 || current.Caret.Start + current.After.Length != caret.Start || current.Caret.Position != CaretPosition.Start)
            {
                FlushTextInsertUndo();
                
                _undoSystem.StartGroupUndo("Insert text");
            }

            _currentInputUndoRun = new TextInsertUndo(this, caret, replacing, text);
            RegisterUndo(_currentInputUndoRun);
        }

        /// <summary>
        /// Returns all text currently under selection of the caret.
        /// 
        /// Returns an empty string, if no ranged selection is present.
        /// </summary>
        public string SelectedText()
        {
            return TextBuffer.TextInRange(Caret.TextRange);
        }

        /// <summary>
        /// Moves the caret to the right
        /// </summary>
        public void MoveRight()
        {
            if (Caret.Start == TextBuffer.TextLength)
                return;

            SetCaret(new TextRange(Caret.Location + 1, 0));
        }

        /// <summary>
        /// Moves the caret to the left
        /// </summary>
        public void MoveLeft()
        {
            if (Caret.Start == 0)
                return;

            SetCaret(new TextRange(Caret.Location - 1, 0));
        }

        /// <summary>
        /// Moves the caret to the start of the text
        /// </summary>
        public void MoveToStart()
        {
            if (Caret.Start == 0)
                return;

            SetCaret(new TextRange(0, 0));
        }

        /// <summary>
        /// Moves the caret to just after the end of the text
        /// </summary>
        public void MoveToEnd()
        {
            if (Caret.Start == TextBuffer.TextLength)
                return;

            SetCaret(new TextRange(TextBuffer.TextLength, 0));
        }

        /// <summary>
        /// Moves right until the caret hits a word break.
        /// 
        /// From the current caret location, moves to the beginning of the next 
        /// word.
        /// If the caret is on top of a word, move to the end of that word.
        /// </summary>
        public void MoveRightWord()
        {
            if (Caret.Location == TextBuffer.TextLength)
                return;

            int offset = OffsetForRightWord();
            SetCaret(offset);
        }

        /// <summary>
        /// Moves left until the caret hits a word break.
        /// 
        /// From the current caret location, moves to the beginning of the previous
        /// word.
        /// If the caret is on top of a word, move to the beginning of that word.
        /// </summary>
        public void MoveLeftWord()
        {
            if (Caret.Location == 0)
                return;

            int offset = OffsetForLeftWord();
            SetCaret(offset);
        }

        /// <summary>
        /// Performs a selection to the right of the caret.
        /// 
        /// If the caret's position is Start, the caret selects one character to the 
        /// left, otherwise, it moves to the left and subtracts the character it 
        /// moved over from the selection area.
        /// </summary>
        public void SelectRight()
        {
            if (Caret.Start == TextBuffer.TextLength)
                return;

            MoveCaretSelecting(Caret.Location + 1);
        }

        /// <summary>
        /// Performs a selection to the left of the caret.
        /// 
        /// If the caret's position is End, the caret selects one character to the 
        /// left, otherwise, it moves to the left and subtracts the character it 
        /// moved over from the selection area.
        /// </summary>
        public void SelectLeft()
        {
            if (Caret.Start == 0)
                return;

            MoveCaretSelecting(Caret.Location - 1);
        }

        /// <summary>
        /// Moves the caret to the beginning of the text range, selecting
        /// any characters it moves over.
        /// </summary>
        public void SelectToStart()
        {
            if (Caret.Start == 0)
                return;
            
            MoveCaretSelecting(0);
        }

        /// <summary>
        /// Moves the caret to the end of the text range, selecting
        /// any characters it moves over.
        /// </summary>
        public void SelectToEnd()
        {
            if (Caret.Start == TextBuffer.TextLength)
                return;

            MoveCaretSelecting(TextBuffer.TextLength);
        }

        /// <summary>
        /// Selects right until the caret hits a word break.
        /// 
        /// From the current caret location, selects up to the beginning of the next 
        /// word.
        /// If the caret is on top of a word, selects up to the end of that word.
        /// </summary>
        public void SelectRightWord()
        {
            if (Caret.Location == TextBuffer.TextLength)
                return;

            int offset = OffsetForRightWord();
            MoveCaretSelecting(offset);
        }

        /// <summary>
        /// Selects to the left until the caret hits a word break.
        /// 
        /// From the current caret location, selects up to the beginning of the previous
        /// word.
        /// If the caret is on top of a word, selects up to the beginning of that word.
        /// </summary>
        public void SelectLeftWord()
        {
            if (Caret.Location == 0)
                return;

            int offset = OffsetForLeftWord();
            MoveCaretSelecting(offset);
        }

        /// <summary>
        /// Selects the entire text buffer available.
        /// </summary>
        public void SelectAll()
        {
            SetCaret(new Caret(new TextRange(0, TextBuffer.TextLength), CaretPosition.End));
        }

        /// <summary>
        /// Moves the caret position to a given location, while mantaining a pivot 
        /// over the other end of the selection.
        /// 
        /// If the caret's position is towards the End of its range, this method mantains 
        /// its Start location the same, otherwise, it keeps the End location the same, instead.
        /// </summary>
        /// <param name="offset">New offset to move caret to, pinning the current selection position.</param>
        public void MoveCaretSelecting(int offset)
        {
            int pivot = Caret.Position == CaretPosition.Start ? Caret.End : Caret.Start;
            int newPos = offset;

            var position = newPos > pivot ? CaretPosition.End : CaretPosition.Start;

            SetCaret(new Caret(TextRange.FromOffsets(pivot, newPos), position));
        }

        /// <summary>
        /// Inserts the specified text on top of the current caret position.
        /// 
        /// Replaces text if caret's range is > 0.
        /// </summary>
        public void InsertText(string text)
        {
            if (Caret.Start == TextBuffer.TextLength)
            {
                TextBuffer.Append(text);
                UpdateTextInsertUndo("", text, Caret);
            }
            else if (Caret.Length == 0)
            {
                TextBuffer.Insert(Caret.Start, text);
                UpdateTextInsertUndo("", text, Caret);
            }
            else
            {
                string replaced = TextBuffer.TextInRange(Caret.TextRange);

                TextBuffer.Replace(Caret.Start, Caret.Length, text);
                
                UpdateTextInsertUndo(replaced, text, Caret);
            }

            SetCaret(new TextRange(Caret.Start + text.Length, 0));
        }

        /// <summary>
        /// Deletes the text before the starting position of the caret.
        /// </summary>
        public void BackspaceText()
        {
            if (Caret.Location == 0 && Caret.Length == 0)
                return;

            FlushTextInsertUndo();

            if (Caret.Length == 0)
            {
                var caret = Caret;
                string removed = TextBuffer.TextInRange(new TextRange(Caret.Start - 1, 1));

                TextBuffer.Delete(Caret.Start - 1, 1);
                SetCaret(new TextRange(Caret.Start - 1, 0));

                _undoSystem.RegisterUndo(new TextDeleteUndo(this, caret, caret.TextRange, removed));
            }
            else
            {
                var caret = Caret;
                string removed = TextBuffer.TextInRange(Caret.TextRange);

                TextBuffer.Delete(Caret.Start, Caret.Length);
                SetCaret(Caret.Start);
                
                _undoSystem.RegisterUndo(new TextDeleteUndo(this, caret, caret.TextRange, removed));
            }
        }

        /// <summary>
        /// Deletes the text exactly on top of the caret.
        /// </summary>
        public void DeleteText()
        {
            if (Caret.Location == TextBuffer.TextLength && Caret.Length == 0)
                return;

            FlushTextInsertUndo();

            if (Caret.Length == 0)
            {
                var caret = Caret;
                string removed = TextBuffer.TextInRange(new TextRange(Caret.Start, 1));

                TextBuffer.Delete(Caret.Start, 1);

                RegisterUndo(new TextDeleteUndo(this, caret, caret.TextRange, removed));
            }
            else
            {
                var caret = Caret;
                string removed = TextBuffer.TextInRange(Caret.TextRange);

                TextBuffer.Delete(Caret.Start, Caret.Length);
                SetCaret(Caret.Start);

                RegisterUndo(new TextDeleteUndo(this, caret, caret.TextRange, removed));
            }
        }

        /// <summary>
        /// Copies the selected text content into <see cref="TextClipboard"/>.
        /// 
        /// If no text range is selected, nothing is done.
        /// </summary>
        public void Copy()
        {
            if (Caret.Length == 0)
                return;

            string text = SelectedText();
            TextClipboard.SetText(text);
        }

        /// <summary>
        /// Cuts the selected text content into <see cref="TextClipboard"/>, by
        /// copying and subsequently deleting the text range.
        /// 
        /// If no text range is selected, nothing is done.
        /// </summary>
        public void Cut()
        {
            if (Caret.Length == 0)
                return;

            Copy();
            DeleteText();
        }

        /// <summary>
        /// Pastes any text content from <see cref="TextClipboard"/> into this 
        /// text engine, replacing any selection range that is currently made.
        /// 
        /// If no text is available in the clipboard, nothing is done.
        /// </summary>
        public void Paste()
        {
            if (!TextClipboard.ContainsText())
                return;
            
            FlushTextInsertUndo();

            string text = TextClipboard.GetText();
            InsertText(text);

            FlushTextInsertUndo();
        }

        /// <summary>
        /// Returns a text range that covers an entire word segment at a given text position.
        /// 
        /// If the text under the position contains a word, the range from the begginning to the
        /// end of the word is returned, otherwise, the boundaries for the nearest word are given.
        /// 
        /// If no word is under or near the position, the non-word (white space)
        /// </summary>
        /// <param name="position">Position to get word segment under</param>
        public TextRange WordSegmentIn(int position)
        {
            if (position >= TextBuffer.TextLength)
                return new TextRange(TextBuffer.TextLength, 0);

            int start;
            int end;

            if (IsWord(TextBuffer.CharacterAtOffset(position)))
            {
                start = position;
                end = position;

                while (start > 0 && IsWord(TextBuffer.CharacterAtOffset(start)))
                {
                    start -= 1;
                }

                if (start > 0)
                    start += 1;

                while (end < TextBuffer.TextLength && IsWord(TextBuffer.CharacterAtOffset(end)))
                {
                    end += 1;
                }

                return TextRange.FromOffsets(start, end);
            }
            if (position > 0 && IsWord(TextBuffer.CharacterAtOffset(position - 1)))
            {
                start = position - 1;

                while (start > 0 && IsWord(TextBuffer.CharacterAtOffset(start)))
                {
                    start -= 1;
                }

                if (start > 0)
                    start += 1;
                
                return TextRange.FromOffsets(start, position);
            }

            start = position;
            end = position;

            while (start > 0 && !IsWord(TextBuffer.CharacterAtOffset(start)))
            {
                start -= 1;
            }

            if (start > 0)
                start += 1;

            while (end < TextBuffer.TextLength && !IsWord(TextBuffer.CharacterAtOffset(end)))
            {
                end += 1;
            }

            return TextRange.FromOffsets(start, end);
        }

        /// <summary>
        /// Sets the caret range for the text, with no selection length associated with it.
        /// 
        /// Calls to this method fire the <see cref="CaretChanged"/> event.
        /// </summary>
        public void SetCaret(int offset)
        {
            SetCaret(new Caret(offset));
        }

        /// <summary>
        /// Sets the caret range for the text.
        /// 
        /// If <see cref="range"/>.Length > 0, the caret is treated
        /// as a selection range.
        /// 
        /// Calls to this method fire the <see cref="CaretChanged"/> event.
        /// </summary>
        public void SetCaret(TextRange range, CaretPosition position = CaretPosition.Start)
        {
            SetCaret(new Caret(range, position));
        }

        /// <summary>
        /// Sets the caret range for the text.
        /// 
        /// If <see cref="caret"/>.Length > 0, the caret is treated
        /// as a selection range.
        /// 
        /// Calls to this method fire the <see cref="CaretChanged"/> event.
        /// </summary>
        public void SetCaret(Caret caret)
        {
            var oldCaret = Caret;

            // Overlap to keep caret within text bounds
            int total = TextBuffer.TextLength;

            var clampedRange =
                new TextRange(0, total).Overlap(caret.TextRange) ??
                (caret.Start < 0 ? new TextRange(0, 0) : new TextRange(total, 0));

            Caret = new Caret(clampedRange, caret.Position);

            CaretChanged?.Invoke(this, new TextEngineCaretChangedEventArgs(Caret, oldCaret));
        }

        private int OffsetForRightWord()
        {
            if (Caret.Location == TextBuffer.TextLength)
                return Caret.Location;

            if (IsWord(TextBuffer.CharacterAtOffset(Caret.Location)))
            {
                // Move to end of current word
                int newOffset = Caret.Location;
                while (newOffset < TextBuffer.TextLength && IsWord(TextBuffer.CharacterAtOffset(newOffset)))
                {
                    newOffset++;
                }

                return newOffset;
            }
            else
            {
                // Move to beginning of the next word
                int newOffset = Caret.Location;
                while (newOffset < TextBuffer.TextLength && !IsWord(TextBuffer.CharacterAtOffset(newOffset)))
                {
                    newOffset++;
                }

                return newOffset;
            }
        }

        private int OffsetForLeftWord()
        {
            if (Caret.Location == 0)
                return Caret.Location;

            if (IsWord(TextBuffer.CharacterAtOffset(Caret.Location - 1)))
            {
                // Move to beginning of current word
                int newOffset = Caret.Location - 1;
                while (newOffset > 0 && IsWord(TextBuffer.CharacterAtOffset(newOffset)))
                {
                    newOffset--;
                }

                // We stopped because we hit the beginning of the string
                if (newOffset == 0)
                    return newOffset;

                return newOffset + 1;
            }
            else
            {
                // Move to beginning of the previous word
                int newOffset = Caret.Location - 1;
                while (newOffset > 0 && !IsWord(TextBuffer.CharacterAtOffset(newOffset)))
                {
                    newOffset--;
                }
                while (newOffset > 0 && IsWord(TextBuffer.CharacterAtOffset(newOffset)))
                {
                    newOffset--;
                }

                // We stopped because we hit the beginning of the string
                if (newOffset == 0)
                    return newOffset;

                return newOffset + 1;
            }
        }

        /// <summary>
        /// Returns if a given character is recognized as a word char.
        /// </summary>
        private static bool IsWord(char character)
        {
            return char.IsLetterOrDigit(character);
        }
    }

    /// <summary>
    /// Basic text clipboard for a <see cref="TextEngine"/> to use during copy/cut/paste operations.
    /// </summary>
    public interface ITextClipboard
    {
        /// <summary>
        /// Returns text from this clipboard, if present.
        /// </summary>
        string GetText();

        /// <summary>
        /// Sets the textual content to this clipboard
        /// </summary>
        void SetText([NotNull] string text);

        /// <summary>
        /// Returns whether this clipboard contains any text content in it
        /// </summary>
        bool ContainsText();
    }

    /// <summary>
    /// Wraps Window's <see cref="Clipboard"/> into an <see cref="ITextClipboard"/> interface.
    /// </summary>
    internal class WindowsTextClipboard : ITextClipboard
    {
        public string GetText()
        {
            return Clipboard.GetText();
        }

        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }

        public bool ContainsText()
        {
            return Clipboard.ContainsText();
        }
    }

    /// <summary>
    /// Undo task for a text insert operation
    /// </summary>
    public class TextInsertUndo : IUndoTask
    {
        /// <summary>
        /// Text engine associated with this undo task
        /// </summary>
        public ITextEngine TextEngine;

        /// <summary>
        /// Position of caret when text was input
        /// </summary>
        public Caret Caret { get; }

        /// <summary>
        /// Text string that was replaced (if input replaced existing)
        /// </summary>
        public string Before { get; }

        /// <summary>
        /// Text string that replaced/was inserted into the buffer
        /// </summary>
        public string After { get; }

        public TextInsertUndo(ITextEngine textEngine, Caret caret, string before, string after)
        {
            TextEngine = textEngine;
            Caret = caret;
            Before = before;
            After = after;
        }

        public void Clear()
        {

        }

        public void Undo()
        {
            TextEngine.SetCaret(new Caret(new TextRange(Caret.Start, After.Length), CaretPosition.Start));
            TextEngine.DeleteText();

            if (Before.Length > 0)
            {
                TextEngine.InsertText(Before);
                TextEngine.SetCaret(Caret);
            }
        }

        public void Redo()
        {
            TextEngine.SetCaret(Caret);
            TextEngine.InsertText(After);
        }

        public string GetDescription()
        {
            return "Insert text";
        }
    }

    /// <summary>
    /// Undo task for a text delete operation
    /// </summary>
    public class TextDeleteUndo : IUndoTask
    {
        /// <summary>
        /// Text engine associated with this undo task
        /// </summary>
        public ITextEngine TextEngine;

        /// <summary>
        /// Position of caret to place when operation is undone
        /// </summary>
        public Caret BeforeCaret { get; }

        /// <summary>
        /// Range of text that was removed.
        /// 
        /// Must always have Length > 0.
        /// </summary>
        public TextRange DeletedRange { get; }

        /// <summary>
        /// Text string that was deleted
        /// </summary>
        public string Text { get; }
        
        public TextDeleteUndo(ITextEngine textEngine, Caret beforeCaret, TextRange deletedRange, string text)
        {
            TextEngine = textEngine;
            DeletedRange = deletedRange;
            Text = text;
            BeforeCaret = beforeCaret;
        }

        public void Clear()
        {
            
        }

        public void Undo()
        {
            TextEngine.SetCaret(new Caret(DeletedRange.Start));
            TextEngine.InsertText(Text);

            TextEngine.SetCaret(BeforeCaret);
        }

        public void Redo()
        {
            TextEngine.SetCaret(new Caret(DeletedRange, CaretPosition.Start));
            TextEngine.DeleteText();
        }

        public string GetDescription()
        {
            return "Delete text";
        }
    }

    /// <summary>
    /// Event arguments for events of <see cref="TextEngine"/> that involve the selection caret.
    /// </summary>
    public class TextEngineCaretChangedEventArgs : EventArgs
    {
        /// <summary>
        /// New caret range
        /// </summary>
        public Caret Caret { get; }

        /// <summary>
        /// Old caret range
        /// </summary>
        public Caret OldCaret { get; }

        public TextEngineCaretChangedEventArgs(Caret caret, Caret oldCaret)
        {
            Caret = caret;
            OldCaret = oldCaret;
        }
    }

    /// <summary>
    /// A textual buffer backing for a <see cref="TextEngine"/>.
    /// 
    /// Text buffers receive small text insertion/deletion/replacing operations, as well as text
    /// retrievals when necessary, and can hide complexity of the actual text backing as efficiently as needed.
    /// </summary>
    public interface ITextEngineTextualBuffer
    {
        /// <summary>
        /// Gets the textual length on this text buffer
        /// </summary>
        int TextLength { get; }

        /// <summary>
        /// Gets the string contents of a text at a given range on this text buffer.
        /// 
        /// If <see cref="range"/> is outside range 0 to <see cref="TextLength"/>, an exception is raised.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="range"/> is outside the range 0 to <see cref="TextLength"/>.</exception>
        string TextInRange(TextRange range);

        /// <summary>
        /// Gets the character at a given offset.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="offset"/> is outside the range 0 to <see cref="TextLength"/>.</exception>
        char CharacterAtOffset(int offset);

        /// <summary>
        /// Deletes <see cref="length"/> number of sequential characters starting at <see cref="index"/>.
        /// </summary>
        /// <param name="index">0-based string index.</param>
        /// <param name="length">Length of text to remove. Passing 1 removes a single character at <see cref="index"/>, passing 0 removes no text.</param>
        void Delete(int index, int length);

        /// <summary>
        /// Inserts a given string at <see cref="index"/> on the text buffer.
        /// </summary>
        /// <param name="index">Index to add text to</param>
        /// <param name="text">Text to insert</param>
        void Insert(int index, [NotNull] string text);

        /// <summary>
        /// Appends a given string at the end of this text buffer.
        /// </summary>
        /// <param name="text">Text to append</param>
        void Append([NotNull] string text);

        /// <summary>
        /// Replaces a run of text of <see cref="length"/>-count of characters on a given <see cref="index"/> with a given <see cref="text"/> value.
        /// </summary>
        /// <param name="index">0-based string index.</param>
        /// <param name="length">Length of text to replace. Passing 0 makes this method act as <see cref="Insert"/>, and no text removal is made.</param>
        /// <param name="text">
        /// Text to replace slice of text under index + length. Can be shorter or longer than <see cref="length"/>. 
        /// Passing an empty string makes this method act as <see cref="Delete"/>.
        /// </param>
        void Replace(int index, int length, [NotNull] string text);
    }

    /// <summary>
    /// Caret position of a <see cref="TextEngine"/>.
    /// </summary>
    public readonly struct Caret : IEquatable<Caret>
    {
        /// <summary>
        /// Range of text this caret covers on a text engine
        /// </summary>
        public TextRange TextRange { get; }

        /// <summary>
        /// Position of this text caret.
        /// 
        /// If <see cref="Position"/> is <see cref="CaretPosition.Start"/>, this value
        /// matches the value of <see cref="Start"/>, otherwise this value matches <see cref="End"/>,
        /// instead.
        /// </summary>
        public int Location
        {
            get
            {
                switch (Position)
                {
                    case CaretPosition.Start:
                        return Start;
                    case CaretPosition.End:
                        return End;
                    default:
                        return Start;
                }
            }
        }

        /// <summary>
        /// Position of the caret within the text range.
        /// </summary>
        public CaretPosition Position { get; }

        /// <summary>
        /// Start of text range this caret covers
        /// </summary>
        public int Start => TextRange.Start;

        /// <summary>
        /// Start of text range this caret covers
        /// </summary>
        public int End => TextRange.End;

        /// <summary>
        /// Length of text this caret covers
        /// </summary>
        public int Length => TextRange.Length;

        public Caret(int location)
        {
            TextRange = new TextRange(location, 0);
            Position = CaretPosition.Start;
        }

        public Caret(TextRange range, CaretPosition position)
        {
            TextRange = range;
            Position = position;
        }

        public bool Equals(Caret other)
        {
            return TextRange.Equals(other.TextRange) && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Caret caret && Equals(caret);
        }

        public static bool operator ==(Caret lhs, Caret rhs)
        {
            return lhs.TextRange == rhs.TextRange && lhs.Position == rhs.Position;
        }

        public static bool operator !=(Caret lhs, Caret rhs)
        {
            return lhs.TextRange != rhs.TextRange || lhs.Position != rhs.Position;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TextRange.GetHashCode() * 397) ^ (int) Position;
            }
        }

        public override string ToString()
        {
            return $"Caret: {TextRange} : {Position}";
        }
    }

    /// <summary>
    /// Specifies the position a <see cref="Caret"/> is located within its own range.
    /// 
    /// If start, <see cref="Caret.Location"/> == <see cref="Caret.Start"/>, if end,
    /// <see cref="Caret.Location"/> == <see cref="Caret.End"/>.
    /// </summary>
    public enum CaretPosition
    {
        Start,
        End
    }
}
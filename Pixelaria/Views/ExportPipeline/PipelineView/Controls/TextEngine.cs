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
using JetBrains.Annotations;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A text + caret engine that handles manipulation of a text value.
    /// 
    /// Base text input engine backing for <see cref="TextField"/>'s.
    /// </summary>
    internal class TextEngine
    {
        /// <summary>
        /// Event handler for <see cref="CaretChanged"/> event.
        /// </summary>
        public delegate void TextEngineCaretChangedEventHandler(object sender, TextEngineCaretChangedEventArgs e);

        /// <summary>
        /// Event fired whenever the current <see cref="Caret"/> value is changed.
        /// </summary>
        public event TextEngineCaretChangedEventHandler CaretChanged;

        /// <summary>
        /// The text buffer that receives instructions to add/remove/replace text based on caret
        /// inputs handled by this text engine.
        /// </summary>
        public ITextEngineTextualBuffer TextBuffer { get; }

        /// <summary>
        /// Gets the caret range.
        /// 
        /// To change the caret range, use <see cref="SetCaret(int)"/>/<see cref="SetCaret(Caret)"/>/<see cref="SetCaret(TextRange, CaretPosition)"/> methods.
        /// </summary>
        public Caret Caret { get; private set; } = new Caret(new TextRange(0, 0), CaretPosition.Start);

        public TextEngine(ITextEngineTextualBuffer textBuffer)
        {
            TextBuffer = textBuffer;
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
        /// Replaces text, if caret's length is > 0 and text is available on selection;
        /// </summary>
        public void InsertText([NotNull] string text)
        {
            if (Caret.Start == TextBuffer.TextLength)
            {
                TextBuffer.Append(text);
            }
            else
            {
                TextBuffer.Insert(Caret.Start, text);
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

            if (Caret.Length == 0)
            {
                TextBuffer.Delete(Caret.Start - 1, 1);
                SetCaret(new TextRange(Caret.Start - 1, 0));
            }
            else
            {
                TextBuffer.Delete(Caret.Start, Caret.Length);
                SetCaret(Caret.Start);
            }
        }

        /// <summary>
        /// Deletes the text exactly on top of the caret.
        /// </summary>
        public void DeleteText()
        {
            if (Caret.Location == TextBuffer.TextLength && Caret.Length == 0)
                return;

            if (Caret.Length == 0)
            {
                TextBuffer.Delete(Caret.Start, 1);
            }
            else
            {
                TextBuffer.Delete(Caret.Start, Caret.Length);
                SetCaret(Caret.Start);
            }
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

            var clampedRange = new TextRange(0, total).Overlap(caret.TextRange);
            if (clampedRange == null)
                clampedRange = caret.Start < 0 ? new TextRange(0, 0) : new TextRange(total, 0);

            Caret = new Caret(clampedRange.Value, caret.Position);

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

        /// <summary>
        /// Returns if a given character is recognized as a word break char.
        /// 
        /// Simply returns the inverse of <see cref="IsWord"/>.
        /// </summary>
        private static bool IsWordBreak(char character)
        {
            return !IsWord(character);
        }
    }

    /// <summary>
    /// Event arguments for events of <see cref="TextEngine"/> that involve the selection caret.
    /// </summary>
    internal class TextEngineCaretChangedEventArgs : EventArgs
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
    internal interface ITextEngineTextualBuffer
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
    internal struct Caret : IEquatable<Caret>
    {
        /// <summary>
        /// Range of text this caret covers on a text engine
        /// </summary>
        public TextRange TextRange { get; }

        /// <summary>
        /// Position of this text caret.
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
                        throw new ArgumentOutOfRangeException();
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
            return obj is Caret && Equals((Caret) obj);
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
    internal enum CaretPosition
    {
        Start,
        End
    }
}
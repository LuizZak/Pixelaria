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
        /// Event fired whenever the current <see cref="CaretRange"/> value is changed.
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
        /// To change the caret range, use <see cref="SetCaret(int)"/>/<see cref="SetCaret(TextRange)"/> methods.
        /// </summary>
        public TextRange CaretRange { get; private set; } = new TextRange(0, 0);

        public TextEngine(ITextEngineTextualBuffer textBuffer)
        {
            TextBuffer = textBuffer;
        }

        /// <summary>
        /// Moves the caret to the left
        /// </summary>
        public void MoveLeft()
        {
            if (CaretRange.Start == 0)
                return;

            SetCaret(new TextRange(CaretRange.Start - 1, 0));
        }

        /// <summary>
        /// Moves the caret to the right
        /// </summary>
        public void MoveRight()
        {
            if (CaretRange.Start == TextBuffer.TextLength)
                return;

            SetCaret(new TextRange(CaretRange.Start + 1, 0));
        }

        /// <summary>
        /// Moves the caret to the start of the text
        /// </summary>
        public void MoveToStart()
        {
            if (CaretRange.Start == 0)
                return;

            SetCaret(new TextRange(0, 0));
        }

        /// <summary>
        /// Moves the caret to just after the end of the text
        /// </summary>
        public void MoveToEnd()
        {
            if (CaretRange.Start == TextBuffer.TextLength)
                return;

            SetCaret(new TextRange(TextBuffer.TextLength, 0));
        }

        /// <summary>
        /// Inserts the specified text on top of the current caret position.
        /// 
        /// Replaces text, if caret's length is > 0 and text is available on selection;
        /// </summary>
        public void InsertText([NotNull] string text)
        {
            if (CaretRange.Start == TextBuffer.TextLength)
            {
                TextBuffer.Append(text);
            }
            else
            {
                TextBuffer.Insert(CaretRange.Start, text);
            }

            SetCaret(new TextRange(CaretRange.Start + text.Length, 0));
        }

        /// <summary>
        /// Deletes the text before the starting position of the caret.
        /// </summary>
        public void BackspaceText()
        {
            if (CaretRange.Start == 0)
                return;

            if (CaretRange.Length == 0)
            {
                TextBuffer.Delete(CaretRange.Start - 1, 1);
                SetCaret(new TextRange(CaretRange.Start - 1, 0));
            }
            else
            {
                TextBuffer.Delete(CaretRange.Start, CaretRange.Length);
                SetCaret(CaretRange.Start);
            }
        }

        /// <summary>
        /// Sets the caret range for the text, with no selection length associated with it.
        /// 
        /// Calls to this method fire the <see cref="CaretChanged"/> event.
        /// </summary>
        public void SetCaret(int offset)
        {
            SetCaret(new TextRange(offset, 0));
        }

        /// <summary>
        /// Sets the caret range for the text.
        /// 
        /// If <see cref="range"/>.Length > 0, the caret is treated
        /// as a selection range.
        /// 
        /// Calls to this method fire the <see cref="CaretChanged"/> event.
        /// </summary>
        public void SetCaret(TextRange range)
        {
            var oldCaret = CaretRange;

            // Overlap to keep caret within text bounds
            int total = TextBuffer.TextLength;

            var clampedRange = new TextRange(0, total).Overlap(range);
            if (clampedRange == null)
                clampedRange = range.Start < 0 ? new TextRange(0, 0) : new TextRange(total, 0);

            CaretRange = clampedRange.Value;

            CaretChanged?.Invoke(this, new TextEngineCaretChangedEventArgs(CaretRange, oldCaret));
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
        public TextRange CaretRange { get; }

        /// <summary>
        /// Old caret range
        /// </summary>
        public TextRange OldCaretRange { get; }

        public TextEngineCaretChangedEventArgs(TextRange caretRange, TextRange oldCaretRange)
        {
            CaretRange = caretRange;
            OldCaretRange = oldCaretRange;
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
}
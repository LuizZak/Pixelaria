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

namespace Pixelaria.PixUI.Controls.Text
{
    /// <summary>
    /// A simple string-backed <see cref="TextEngine"/> buffer.
    /// </summary>
    internal class StringTextualBuffer : ITextEngineTextualBuffer
    {
        public string Text { get; private set; }

        public int TextLength => Text.Length;

        public StringTextualBuffer(string text)
        {
            Text = text;
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
}
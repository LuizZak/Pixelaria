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

using JetBrains.Annotations;

namespace Pixelaria.PixUI.Controls.Text
{
    /// <summary>
    /// A minimal interface for a text engine.
    /// 
    /// Mainly implemented by <see cref="TextEngine"/>.
    /// </summary>
    internal interface ITextEngine
    {
        /// <summary>
        /// Gets the caret range.
        /// 
        /// To change the caret range, use one of the SetCaret() methods.
        /// </summary>
        Caret Caret { get; }

        /// <summary>
        /// Inserts the specified text on top of the current caret position.
        /// 
        /// Replaces text if caret's range is > 0.
        /// </summary>
        void InsertText([NotNull] string text);

        /// <summary>
        /// Deletes the text before the starting position of the caret.
        /// </summary>
        void BackspaceText();

        /// <summary>
        /// Deletes the text exactly on top of the caret.
        /// </summary>
        void DeleteText();

        /// <summary>
        /// Sets the caret range for the text.
        /// 
        /// If <see cref="caret"/>.Length > 0, the caret is treated
        /// as a selection range.
        /// </summary>
        void SetCaret(Caret caret);
    }
}
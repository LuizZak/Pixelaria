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

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Specifies an ellipsis trimming to use when a text's contents overflow the layout box.
    /// </summary>
    public struct TextEllipsisTrimming
    {
        public static TextEllipsisTrimming None = new TextEllipsisTrimming();

        /// <summary>
        /// The trimming granularity to apply
        /// </summary>
        public TextTrimmingGranularity Granularity { get; set; }

        /// <summary>
        /// A character code used as the delimiter that signals the beginning of the portion of text to be preserved.
        ///
        /// Text starting from the Nth occurence of the delimiter (where N equals delimiterCount) counting backwards from the end of the text block will be preserved.
        /// For example, given the text is a path like c:\A\B\C\D\file.txt and delimiter equal to '\' and delimiterCount equal to 1, the file.txt portion of the text would be preserved.
        /// Specifying a delimiterCount of 2 would preserve D\file.txt.
        /// </summary>
        public int Delimiter;

        /// <summary>
        /// The delimiter count, counting from the end of the text, to preserve text from.
        /// </summary>
        public int DelimiterCount;
    }

    /// <summary>
    /// Specifies the text granularity used to trim text overflowing the layout box.
    /// </summary>
    public enum TextTrimmingGranularity
    {
        /// <summary>
        /// No trimming occurs. Text flows beyond the layout width.
        /// </summary>
        None,
        /// <summary>
        /// Trimming occurs at a character cluster boundary.
        /// </summary>
        Character,
        /// <summary>
        /// Trimming occurs at a word boundary.
        /// </summary>
        Word,
    }
}
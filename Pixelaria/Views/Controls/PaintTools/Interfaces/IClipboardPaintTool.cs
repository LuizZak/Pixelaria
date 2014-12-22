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

namespace Pixelaria.Views.Controls.PaintTools.Interfaces
{
    /// <summary>
    /// Specifies a Paint Operation that has clipboard access capabilities
    /// </summary>
    public interface IClipboardPaintTool
    {
        /// <summary>
        /// Performs a Copy operation
        /// </summary>
        void Copy();

        /// <summary>
        /// Performs a Cut operation
        /// </summary>
        void Cut();

        /// <summary>
        /// Performs a Paste operation
        /// </summary>
        void Paste();

        /// <summary>
        /// Returns whether the paint operation can copy content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can copy content to the clipboard</returns>
        bool CanCopy();

        /// <summary>
        /// Returns whether the paint operation can cut content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can cut content to the clipboard</returns>
        bool CanCut();

        /// <summary>
        /// Returns whether the paint operation can paste content from the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can paste content from the clipboard</returns>
        bool CanPaste();
    }
}
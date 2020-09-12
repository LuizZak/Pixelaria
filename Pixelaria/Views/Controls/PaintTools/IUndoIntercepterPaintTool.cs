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

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Represents an interface for paint tool that can intercept the Undo/Redo system with custom actions
    /// </summary>
    internal interface IUndoIntercepterPaintTool
    {
        /// <summary>
        /// Forces this paint tool to intercept the undo operation, returning whether this Paint Tool has intercepted the undo operation successfully.
        /// While intercepting an undo, a paint tool might perform actions of its own
        /// </summary>
        /// <returns>Whether the current paint tool intercepted the undo task. When the return is true, no undo operation might be performed from an owning object</returns>
        bool InterceptUndo();

        /// <summary>
        /// Forces this paint tool to intercept the redo operation, returning whether this Paint Tool has intercepted the redo operation successfully.
        /// While intercepting a redo, a paint tool might perform actions of its own
        /// </summary>
        /// <returns>Whether the current paint tool intercepted the redo task. When the return is true, no redo operation might be performed from an owning object</returns>
        bool InterceptRedo();
    }
}
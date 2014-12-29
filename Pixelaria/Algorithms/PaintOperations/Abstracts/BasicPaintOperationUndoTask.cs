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

using System.Drawing;
using Pixelaria.Data.Undo;

namespace Pixelaria.Algorithms.PaintOperations.Abstracts
{
    /// <summary>
    /// Specifies a base class for any paint operation undo task
    /// </summary>
    public abstract class BasicPaintOperationUndoTask : IUndoTask
    {
        /// <summary>
        /// The bitmap to perform the undo/redo operations on
        /// </summary>
        protected Bitmap targetBitmap;

        /// <summary>
        /// Gets the target bitmap for this undo operation
        /// </summary>
        public Bitmap TargetBitmap
        {
            get { return targetBitmap; }
        }

        /// <summary>
        /// Initializes a new BasicPaintOperationUndoTask with the specified bitmap to perform the operations on
        /// </summary>
        /// <param name="targetBitmap">The target bitmap to perform the undo of the paint operation on</param>
        protected BasicPaintOperationUndoTask(Bitmap targetBitmap)
        {
            this.targetBitmap = targetBitmap;
        }

        /// <summary>
        /// Clears this undo task
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Undoes the operations performed by this undo task
        /// </summary>
        public abstract void Undo();

        /// <summary>
        /// Redoes the operations performed by this undo task
        /// </summary>
        public abstract void Redo();

        /// <summary>
        /// Gets the basic description for this undo task to display to the user
        /// </summary>
        /// <returns>The basic description for this undo task to display to the user</returns>
        public abstract string GetDescription();
    }
}
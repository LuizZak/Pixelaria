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
using Pixelaria.Algorithms.PaintOperations.Abstracts;
using Pixelaria.Utils;

namespace Pixelaria.Algorithms.PaintOperations.UndoTasks
{
    /// <summary>
    /// A per-pixel undo task
    /// </summary>
    public class PerPixelUndoTask : BasicPaintOperationUndoTask
    {
        /// <summary>
        /// The string that describes this PerPixelUndoTask
        /// </summary>
        private readonly string _description;

        /// <summary>
        /// The pixel history tracker containing the information for the undo/redo
        /// </summary>
        private readonly PixelHistoryTracker _pixelHistoryTracker;

        /// <summary>
        /// Gets the pixel history tracker for this PerPixelUndoTask class
        /// </summary>
        public PixelHistoryTracker PixelHistoryTracker
        {
            get { return _pixelHistoryTracker; }
        }

        /// <summary>
        /// Initializes a new instance of the PixelUndoTask
        /// </summary>
        /// <param name="bitmap">The target bitmap for hte undo operation</param>
        /// <param name="description">A description to use for this UndoTask</param>
        /// <param name="indexPixels">Whether to index the pixels being added so they appear sequentially on the pixel list</param>
        /// <param name="keepReplacedOriginals">Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.</param>
        public PerPixelUndoTask(Bitmap bitmap, string description, bool indexPixels = false, bool keepReplacedOriginals = false)
            : base(bitmap)
        {
            _description = description;

            _pixelHistoryTracker = new PixelHistoryTracker(indexPixels, keepReplacedOriginals, bitmap.Width);
        }

        /// <summary>
        /// Initializes a new instance of the PixelUndoTask
        /// </summary>
        /// <param name="bitmap">The target bitmap for hte undo operation</param>
        /// <param name="description">A description to use for this UndoTask</param>
        /// <param name="tracker">The pixel histroy tracker that contains the information to use on this PerPixelUndoTask</param>
        public PerPixelUndoTask(Bitmap bitmap, string description, PixelHistoryTracker tracker)
            : base(bitmap)
        {
            _description = description;

            _pixelHistoryTracker = tracker;
        }

        /// <summary>
        /// Clears this pencil undo task
        /// </summary>
        public override void Clear()
        {
            _pixelHistoryTracker.Clear();
        }

        /// <summary>
        /// Performs the undo operation on this per-pixel undo task
        /// </summary>
        public override void Undo()
        {
            using (FastBitmap bitmap = targetBitmap.FastLock())
            {
                foreach (var pixelUndo in _pixelHistoryTracker.StoredPixelsEnumerable)
                {
                    bitmap.SetPixel(pixelUndo.PixelX, pixelUndo.PixelY, pixelUndo.OldColor);
                }
                /*int c = _pixelHistoryTracker.PixelList.Count;
                for (int i = 0; i < c; i++)
                {
                    PixelHistoryTracker.PixelUndo pu = _pixelHistoryTracker.PixelList[i];
                    bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.OldColor);
                }*/
            }
        }

        /// <summary>
        /// Performs the redo operation on this per-pixel undo task
        /// </summary>
        public override void Redo()
        {
            using (FastBitmap bitmap = targetBitmap.FastLock())
            {
                foreach (var pixelUndo in _pixelHistoryTracker.StoredPixelsEnumerable)
                {
                    bitmap.SetPixel(pixelUndo.PixelX, pixelUndo.PixelY, pixelUndo.NewColor);
                }
                /*int c = _pixelHistoryTracker.PixelList.Count;
                for (int i = 0; i < c; i++)
                {
                    PixelHistoryTracker.PixelUndo pu = _pixelHistoryTracker.PixelList[i];
                    bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.NewColor);
                }*/
            }
        }

        /// <summary>
        /// Returns the string description of this undo task
        /// </summary>
        /// <returns>The string description of this undo task</returns>
        public override string GetDescription()
        {
            return _description;
        }
    }
}
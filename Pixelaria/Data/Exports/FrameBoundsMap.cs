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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Pixelaria.Data.Exports
{
    /// <summary>
    /// Provides a simple interface for managing sharing of frame bounds so that changes to one frame final sheet rectangle reflects on all other shared frames.
    /// </summary>
    public class FrameBoundsMap
    {
        /// <summary>
        /// Maps Frame.ID -> _sheetBounds Rectangle list index
        /// </summary>
        private readonly Dictionary<int, int> _frameSheetBoundsMap = new Dictionary<int, int>();

        /// <summary>
        /// Maps Frame.ID -> local frame image Rectangle
        /// </summary>
        private readonly Dictionary<int, Rectangle> _frameLocalBoundsMap = new Dictionary<int, Rectangle>();

        /// <summary>
        /// List of available frame bounds in global sheet coordinates
        /// </summary>
        private readonly List<Rectangle> _sheetBounds = new List<Rectangle>();

        /// <summary>
        /// Array of available frame bounds in global sheet coordinates
        /// </summary>
        public Rectangle[] SheetBounds => _sheetBounds.ToArray();

        /// <summary>
        /// Inits a new empty instance of the FrameBoundsMap class
        /// </summary>
        public FrameBoundsMap()
        {
            
        }

        private FrameBoundsMap(IDictionary<int, int> frameSheetBoundsMap, IDictionary<int, Rectangle> frameLocalBoundsMap, IEnumerable<Rectangle> sheetBounds)
        {
            _frameSheetBoundsMap = new Dictionary<int, int>(frameSheetBoundsMap);
            _frameLocalBoundsMap = new Dictionary<int, Rectangle>(frameLocalBoundsMap);
            _sheetBounds = new List<Rectangle>(sheetBounds);
        }

        /// <summary>
        /// Performs a deep copy of this frame bounds map object
        /// </summary>
        public FrameBoundsMap Copy()
        {
            return new FrameBoundsMap(_frameSheetBoundsMap, _frameLocalBoundsMap, _sheetBounds);
        }

        /// <summary>
        /// Register shared bounds for a given set of frames
        /// This method throws an exception if the frame passed in does not have an id set (-1).
        /// </summary>
        /// <param name="frames">Shared frames with the same bounds</param>
        /// <param name="localBounds">Local bounds for the frames</param>
        /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
        public void RegisterFrames(IEnumerable<IFrame> frames, Rectangle localBounds)
        {
            int sheetIndex = _sheetBounds.Count;

            var localZero = localBounds;
            localZero.X = 0;
            localZero.Y = 0;

            _sheetBounds.Add(localZero);

            // Map values
            foreach (var frame in frames)
            {
                // Check invalid id
                if (frame.ID == -1)
                    throw new ArgumentException(@"Frame appears to have no valid ID set (negative number).", nameof(frames));
                if (!frame.Initialized)
                    throw new ArgumentException(@"Frame is uninitialized.", nameof(frames));

                _frameSheetBoundsMap[frame.ID] = sheetIndex;
                _frameLocalBoundsMap[frame.ID] = localBounds;
            }
        }

        /// <summary>
        /// Replaces the current list of rectangles with a given rectangle collection.
        /// The rectangle collection must have the same count of rectangles as the currently registered SheetBounds array.
        /// An exception is thrown, if the count of new bounds do not match the current count of sheets
        /// </summary>
        /// <exception cref="ArgumentException">The newBounds enumerable has a count different than SheetBounds.Count</exception>
        public void ReplaceSheetBounds(IEnumerable<Rectangle> newBounds)
        {
            var newList = new List<Rectangle>(newBounds);
            if (newList.Count != _sheetBounds.Count)
                throw new ArgumentException($@"The count of items in the passed enumerable is mismatched: {newList.Count} new bounds vs {_sheetBounds.Count} local bounds", nameof(newBounds));

            // Straight replace the values
            _sheetBounds.Clear();
            _sheetBounds.AddRange(newList);
        }

        /// <summary>
        /// Gets the rectangle for a given frame.
        /// Returns null, if no rectangle was found
        /// </summary>
        public Rectangle? GetSheetBoundsForFrame(IFrame frame)
        {
            int index;
            if (_frameSheetBoundsMap.TryGetValue(frame.ID, out index))
            {
                return _sheetBounds[index];
            }

            return null;
        }

        /// <summary>
        /// Gets the local image bounds for a given frame
        /// </summary>
        public Rectangle? GetLocalBoundsForFrame(IFrame frame)
        {
            Rectangle bounds;
            if (_frameLocalBoundsMap.TryGetValue(frame.ID, out bounds))
            {
                return bounds;
            }

            return null;
        }

        /// <summary>
        /// Sets the sheet bounds for a specified frame, and all shared frames, in this frame bounds map.
        /// Does nothing if this frame is not registered on this frame bounds map.
        /// This method throws an exception if the frame passed in does not have an id set (-1).
        /// </summary>
        /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
        public void SetSheetBoundsForFrame(IFrame frame, Rectangle sheetBounds)
        {
            int index = SheetIndexForFrame(frame);
            _sheetBounds[index] = sheetBounds;
        }

        /// <summary>
        /// Sets the local bounds for a specified frame.
        /// Does nothing if this frame is not registered on this frame bounds map.
        /// This method throws an exception if the frame passed in does not have an id set (-1).
        /// </summary>
        /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
        public void SetLocalBoundsForFrame(IFrame frame, Rectangle localBounds)
        {
            // Check invalid id
            if (frame.ID == -1)
                throw new ArgumentException(@"Frame appears to have no valid ID set (negative number).", nameof(frame));
            if (!frame.Initialized)
                throw new ArgumentException(@"Frame is uninitialized.", nameof(frame));

            // Ignore frames that are not present in this map
            if (!ContainsFrame(frame))
                return;

            _frameLocalBoundsMap[frame.ID] = localBounds;
        }

        /// <summary>
        /// Marks two frames as sharing the same sheet bounds.
        /// Any changes to a sheet bound of one frame will reflect when fetching the sheet bounds of the other.
        /// Both frames must be registered on this frame bounds map previously, otherwise nothing is done and false is retured.
        /// In case the operation succeeded, true is returned.
        /// This effectively associates frame2 -> frame1, so any sheet bounds associated with frame2 are no longer associated to it.
        /// This method throws an exception if the frame passed in does not have an id set (-1).
        /// </summary>
        /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
        public bool ShareSheetBoundsForFrames(IFrame frame1, IFrame frame2)
        {
            // Find current indexes on map dictionary
            int frame1Index = SheetIndexForFrame(frame1);
            int frame2Index = SheetIndexForFrame(frame2);
            
            // Frames already share the same bounds
            if (frame1Index == -1 || frame2Index == -1 || frame1Index == frame2Index)
                return true;

            // Share from frame1 -> frame2
            _frameSheetBoundsMap[frame2.ID] = frame1Index;

            CompactMapArray();

            return true;
        }

        /// <summary>
        /// Splits the shared bounds of frames so that a frame that once shared bounds with other frames now contains its own unique reference
        /// to a same-area rectangle.
        /// The mehtod does nothing in case the frame did not share its bounds with any other frame.
        /// This method throws an exception if the frame passed in does not have an id set (-1).
        /// </summary>
        /// <returns>Whether the operation succeeded, that is, the frame had a shared bounds and it was split to be unique to this frame. Returns false, if frame was already unique.</returns>
        /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
        public bool SplitSharedSheetBoundsForFrame(IFrame frame)
        {
            // Checks uniqueness of frame
            var index = SheetIndexForFrame(frame);
            if (CountOfFramesAtSheetBoundsIndex(index) == 1)
                return false;

            // Extract frame to a separate index
            var local = GetLocalBoundsForFrame(frame);
            if (local == null)
                return false;

            RegisterFrames(new[] { frame }, local.Value);

            CompactMapArray();

            return true;
        }

        /// <summary>
        /// Returns whether this frame bounds map contains information pertaininig to a given frame
        /// </summary>
        public bool ContainsFrame(IFrame frame)
        {
            return _frameSheetBoundsMap.ContainsKey(frame.ID) && _frameLocalBoundsMap.ContainsKey(frame.ID);
        }

        /// <summary>
        /// Returns the number of frames that reference a given sheet bounds index
        /// </summary>
        /// <returns>The number of frames referencing a specific sheet index</returns>
        public int CountOfFramesAtSheetBoundsIndex(int index)
        {
            return _frameSheetBoundsMap.Keys.Count(key => _frameSheetBoundsMap[key] == index);
        }

        /// <summary>
        /// Gets the frame IDs that point to a given BoundsSheet array index
        /// </summary>
        public int[] FrameIdsAtSheetIndex(int index)
        {
            return _frameSheetBoundsMap.Keys.Where(key => _frameSheetBoundsMap[key] == index).ToArray();
        }

        /// <summary>
        /// Gets the index at the SheetBounds array that maps to a given frame, and any other frames shared, as well.
        /// This method throws an exception if the frame passed in does not have an id set (-1).
        /// </summary>
        /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
        public int SheetIndexForFrame(IFrame frame)
        {
            // Check invalid id
            if (frame.ID == -1)
                throw new ArgumentException(@"Frame appears to have no valid ID set (negative number).", nameof(frame));
            if (!frame.Initialized)
                throw new ArgumentException(@"Frame is uninitialized.", nameof(frame));

            return _frameSheetBoundsMap[frame.ID];
        }

        /// <summary>
        /// Moves all internal references from one frame sheet map array index to another
        /// </summary>
        private void MoveIndex(int oldIndex, int newIndex)
        {
            foreach (var key in _frameSheetBoundsMap.Keys.ToArray())
            {
                if (_frameSheetBoundsMap[key] == oldIndex)
                {
                    _frameSheetBoundsMap[key] = newIndex;
                }
            }
        }

        /// <summary>
        /// Compacts the data so the sheet array doesn't points to no-longer referenced indexes
        /// </summary>
        private void CompactMapArray()
        {
            // Ref-counting array
            var refs = _sheetBounds.Select(v => false).ToList();

            foreach (var value in _frameSheetBoundsMap.Values)
            {
                refs[value] = true;
            }

            // Find 'false' indexes and remove them, moving indexes back as we go
            for (int i = 0; i < refs.Count; i++)
            {
                if (refs[i])
                    continue;

                // Move all next indexes back
                for (int j = i; j < _sheetBounds.Count - 1; j++)
                {
                    MoveIndex(j + 1, j);
                }

                // Remove index, and continue searching
                _sheetBounds.RemoveAt(i);

                refs.RemoveAt(i);
                i--;
            }
        }
    }
}
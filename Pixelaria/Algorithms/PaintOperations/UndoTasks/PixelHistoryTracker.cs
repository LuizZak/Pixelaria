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

namespace Pixelaria.Algorithms.PaintOperations.UndoTasks
{
    /// <summary>
    /// Tracks changes of pixel colors by keeping a list of pixels and their previous and current colors
    /// </summary>
    public class PixelHistoryTracker
    {
        /// <summary>
        /// Dictionary of pixels stored on this per-pixel undo task
        /// </summary>
        private readonly Dictionary<int, PixelUndo> _pixelDictionary;

        /// <summary>
        /// Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.
        /// </summary>
        private readonly bool _keepOriginalUndos;

        /// <summary>
        /// The width of the bitmap being affected
        /// </summary>
        private readonly int _width;

        /// <summary>
        /// Gets an enumerable object for the pixels affected in this pixel history
        /// </summary>
        public IEnumerable<PixelUndo> StoredPixelsEnumerable => _pixelDictionary.Values;

        /// <summary>
        /// Gets the number of items stored in this PixelHistoryTracker
        /// </summary>
        public int PixelCount => _pixelDictionary.Count;

        /// <summary>
        /// Initializes a new instance of the pixel history tracker
        /// </summary>
        /// <param name="keepOriginalUndos">
        ///     Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified
        /// </param>
        /// <param name="width">The width of the bitmap being affected</param>
        public PixelHistoryTracker(bool keepOriginalUndos, int width)
        {
            _pixelDictionary = new Dictionary<int, PixelUndo>();

            _keepOriginalUndos = keepOriginalUndos;
            _width = width;
        }

        /// <summary>
        /// Clears this PixelHistoryTracker
        /// </summary>
        public void Clear()
        {
            _pixelDictionary.Clear();
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="ignoreIfDuplicated">
        /// Whether to check existing pixels before adding the new pixel and aborting if the pixel already exists.
        /// Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance
        /// </param>
        public void RegisterPixel(int x, int y, Color oldColor, Color newColor, bool ignoreIfDuplicated = true)
        {
            RegisterPixel(x, y, unchecked((uint)oldColor.ToArgb()), unchecked((uint)newColor.ToArgb()), ignoreIfDuplicated);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="ignoreIfDuplicated">
        /// Whether to check existing pixels before adding the new pixel and aborting if the pixel already exists.
        /// Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance
        /// </param>
        public void RegisterPixel(int x, int y, int oldColor, int newColor, bool ignoreIfDuplicated = true)
        {
            RegisterPixel(x, y, unchecked((uint)oldColor), unchecked((uint)newColor), ignoreIfDuplicated);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="ignoreIfDuplicated">
        /// Whether to check existing pixels before adding the new pixel and aborting if the pixel already exists.
        /// Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance
        /// </param>
        public void RegisterPixel(int x, int y, uint oldColor, uint newColor, bool ignoreIfDuplicated = true)
        {
            InternalRegisterPixel(x, y, oldColor, newColor, !ignoreIfDuplicated);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask without the existence of a similar prior pixel.
        /// If the pixel already exists, its values are replaced according to the keepOriginalUndos flag
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        public void RegisterUncheckedPixel(int x, int y, uint oldColor, uint newColor)
        {
            InternalRegisterPixel(x, y, oldColor, newColor, true);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask 
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="replaceExisting">Whether to allow replacing existing pixels on the list</param>
        private void InternalRegisterPixel(int x, int y, uint oldColor, uint newColor, bool replaceExisting)
        {
            int pixelIndex = x + y * _width;

            if (_pixelDictionary.ContainsKey(pixelIndex))
            {
                if (!replaceExisting)
                    return;

                var item = new PixelUndo(x, y, pixelIndex, oldColor, newColor);

                if (_keepOriginalUndos)
                {
                    item.OldColor = _pixelDictionary[pixelIndex].OldColor;
                }

                _pixelDictionary[pixelIndex] = item;
            }
            else
            {
                var item = new PixelUndo(x, y, pixelIndex, oldColor, newColor);

                _pixelDictionary[pixelIndex] = item;
            }
        }

        /// <summary>
        /// Returns whether this PerPixelUndoTask contains information about undoing the given pixel
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to search</param>
        /// <param name="y">The Y coordinate of the pixel to search</param>
        /// <returns>Whether this PerPixelUndoTask contains information about undoing the given pixel</returns>
        public bool ContainsPixel(int x, int y)
        {
            return _pixelDictionary.ContainsKey(y * _width + x);
        }

        /// <summary>
        /// Returns the pixel undo information for a pixel at a specified location
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to search</param>
        /// <param name="y">The Y coordinate of the pixel to search</param>
        /// <returns>A PixelUndo struct containing the information of the pixel, or null, if none was found</returns>
        public PixelUndo? PixelUndoForPixel(int x, int y)
        {

            if (_pixelDictionary.TryGetValue(y * _width + x, out var undo))
            {
                return undo;
            }

            return null;
        }

        /// <summary>
        /// Encapsulates an undo task on a single pixel
        /// </summary>
        public struct PixelUndo : IEquatable<PixelUndo>
        {
            /// <summary>
            /// Pre-computed hashcode for this PixelUndo
            /// </summary>
            private readonly int _hashCode;

            /// <summary>
            /// The X position of the pixel to draw
            /// </summary>
            public readonly int PixelX;

            /// <summary>
            /// The Y position of the pixel to draw
            /// </summary>
            public readonly int PixelY;

            /// <summary>
            /// The absolute index of the pixel
            /// </summary>
            public readonly int PixelIndex;

            /// <summary>
            /// The old color for this pixel
            /// </summary>
            public uint OldColor;

            /// <summary>
            /// The new (or current) color for this pixel
            /// </summary>
            public readonly uint NewColor;

            /// <summary>
            /// Initializes a new instance of the PixelUndo struct
            /// </summary>
            /// <param name="x">The X position of the pixel to draw</param>
            /// <param name="y">The Y position of the pixel to draw</param>
            /// <param name="pixelIndex">The absolute index of the pixel</param>
            /// <param name="oldColor">The color to apply on a undo operation</param>
            /// <param name="newColor">The color to apply on a redo operation</param>
            public PixelUndo(int x, int y, int pixelIndex, uint oldColor, uint newColor)
            {
                PixelX = x;
                PixelY = y;
                PixelIndex = pixelIndex;
                OldColor = oldColor;
                NewColor = newColor;

                unchecked
                {
                    int hashCode = PixelX;
                    hashCode = (hashCode * 397) ^ PixelY;
                    hashCode = (hashCode * 397) ^ PixelIndex;
                    _hashCode = hashCode;
                }
            }

            /// <summary>
            /// Returns whether the given PixelUndo object is equal to this object
            /// </summary>
            /// <param name="other">The object to test equality against this object</param>
            /// <returns>Whether the given PixelUndo object is equal to this object</returns>
            public bool Equals(PixelUndo other)
            {
                return PixelX == other.PixelX && PixelY == other.PixelY && PixelIndex == other.PixelIndex && NewColor == other.NewColor;
            }

            /// <summary>
            /// Returns whether the given object is equal to this object
            /// </summary>
            /// <param name="obj">The object to test equality against this object</param>
            /// <returns>Whether the given object is equal to this object</returns>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is PixelUndo other && Equals(other);
            }

            /// <summary>
            /// Gets the hashcode of this PixelUndo object
            /// </summary>
            /// <returns>The hash of this PixelUndo object</returns>
            public override int GetHashCode()
            {
                return _hashCode;
            }

            /// <summary>
            /// Returns whether two PixelUndo objects are equal, using the .Equals() method
            /// </summary>
            /// <param name="left">A PixelUndo object</param>
            /// <param name="right">Another PixelUndo object</param>
            /// <returns>Whether two PixelUndo objects are equal</returns>
            public static bool operator==(PixelUndo left, PixelUndo right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Returns whether two PixelUndo objects are not equal, using the .Equals() method
            /// </summary>
            /// <param name="left">A PixelUndo object</param>
            /// <param name="right">Another PixelUndo object</param>
            /// <returns>Whether two PixelUndo objects are not equal</returns>
            public static bool operator!=(PixelUndo left, PixelUndo right)
            {
                return !left.Equals(right);
            }
        }
    }
}
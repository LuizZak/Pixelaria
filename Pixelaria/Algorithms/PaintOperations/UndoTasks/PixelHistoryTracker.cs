using System.Collections.Generic;
using System.Drawing;

namespace Pixelaria.Algorithms.PaintOperations.UndoTasks
{
    /// <summary>
    /// Tracks changes of pixel colors
    /// </summary>
    public class PixelHistoryTracker
    {
        /// <summary>
        /// List of pixels stored on this per-pixel undo
        /// </summary>
        private readonly List<PixelUndo> _pixelList;

        /// <summary>
        /// Whether to index the pixels being added so they appear sequentially on the pixels list
        /// </summary>
        private readonly bool _indexPixels;

        /// <summary>
        /// Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.
        /// </summary>
        private readonly bool _keepReplacedOriginals;

        /// <summary>
        /// The width of the bitmap being affected
        /// </summary>
        private readonly int _width;

        /// <summary>
        /// Gets the list of pixels affected in this pixel history
        /// </summary>
        public List<PixelUndo> PixelList
        {
            get { return _pixelList; }
        }

        /// <summary>
        /// Initializes a new isntance of the pixel history tracker
        /// </summary>
        /// <param name="indexPixels">Whether to index the pixels being added so they appear sequentially on the pixels list</param>
        /// <param name="keepReplacedOriginals">
        /// Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified
        /// </param>
        /// <param name="width">The width of the bitmap being affected</param>
        public PixelHistoryTracker(bool indexPixels, bool keepReplacedOriginals, int width)
        {
            _pixelList = new List<PixelUndo>();
            _indexPixels = indexPixels;
            _keepReplacedOriginals = keepReplacedOriginals;
            _width = width;
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="checkExisting">Whether to check existing pixels before adding the new pixel. Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance</param>
        public void RegisterPixel(int x, int y, Color oldColor, Color newColor, bool checkExisting = true)
        {
            RegisterPixel(x, y, oldColor.ToArgb(), newColor.ToArgb(), checkExisting);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="checkExisting">Whether to check existing pixels before adding the new pixel. Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance</param>
        public void RegisterPixel(int x, int y, int oldColor, int newColor, bool checkExisting = true)
        {
            RegisterPixel(x, y, unchecked((uint)oldColor), unchecked((uint)newColor), !checkExisting);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="checkExisting">Whether to check existing pixels before adding the new pixel. Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance</param>
        public void RegisterPixel(int x, int y, uint oldColor, uint newColor, bool checkExisting = true)
        {
            // Early out: don't register duplicated pixels
            if (checkExisting && !_indexPixels)
            {
                foreach (PixelUndo pu in _pixelList)
                {
                    if (pu.PixelX == x && pu.PixelY == y)
                        return;
                }
            }

            InternalRegisterPixel(x, y, oldColor, newColor, !checkExisting);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask without the existance of a similar pixel priorly
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        public void RegisterUncheckedPixel(int x, int y, uint oldColor, uint newColor)
        {
            InternalRegisterPixel(x, y, oldColor, newColor, false);
        }

        /// <summary>
        /// Registers a pixel on this PixelUndoTask 
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to store</param>
        /// <param name="y">The Y coordinate of the pixel to store</param>
        /// <param name="oldColor">The old color of the pixel</param>
        /// <param name="newColor">The new color of the pixel</param>
        /// <param name="replaceExisting">Whether to allow relpacing existing pixels on the list</param>
        private void InternalRegisterPixel(int x, int y, uint oldColor, uint newColor, bool replaceExisting)
        {
            int pixelIndex = x + y * _width;

            PixelUndo item = new PixelUndo(x, y, pixelIndex, oldColor, newColor);

            if (!_indexPixels)
            {
                _pixelList.Add(item);
                return;
            }

            int l = _pixelList.Count;

            // Empty list: Add item directly
            if (l == 0)
            {
                _pixelList.Add(item);
                return;
            }

            int s = 0;
            int e = l - 1;
            while (true)
            {
                var idF = _pixelList[e].PixelIndex;

                // Pixel index of the item at the end of the interval is smaller than the current pixel index: Add
                // item after the interval
                if (idF < pixelIndex)
                {
                    _pixelList.Insert(e + 1, item);
                    return;
                }
                // Pixel index of the item at the end of the interval is equals to the item being added: Replace the pixel if replacing is allowed and quit
                if (idF == pixelIndex)
                {
                    if (replaceExisting)
                    {
                        if (_keepReplacedOriginals)
                        {
                            item.UndoColor = _pixelList[e].UndoColor;
                        }

                        _pixelList[e] = item;
                    }

                    return;
                }

                var idC = _pixelList[s].PixelIndex;

                // Pixel index of the item at the start of the interval is larger than the current pixel index: Add
                // item before the interval
                if (idC > pixelIndex)
                {
                    _pixelList.Insert(s, item);
                    return;
                }
                // Pixel index of the item at the start of the interval is equals to the item being added: Replace the pixel if replacing is allowed and quit
                if (idC == pixelIndex)
                {
                    if (replaceExisting)
                    {
                        if (_keepReplacedOriginals)
                        {
                            item.UndoColor = _pixelList[s].UndoColor;
                        }

                        _pixelList[s] = item;
                    }

                    return;
                }

                int mid = s + (e - s) / 2;
                var idM = _pixelList[mid].PixelIndex;

                if (idM > pixelIndex)
                {
                    s++;
                    e = mid - 1;
                }
                else if (idM < pixelIndex)
                {
                    s = mid + 1;
                    e--;
                }
                else if (idM == pixelIndex)
                {
                    if (replaceExisting)
                    {
                        if (_keepReplacedOriginals)
                        {
                            item.UndoColor = _pixelList[mid].UndoColor;
                        }

                        _pixelList[mid] = item;
                    }

                    return;
                }

                // End of search: Add item at the current index
                if (s > e)
                {
                    _pixelList.Insert(s, item);
                    return;
                }
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
            return IndexOfPixel(x, y) > -1;
        }

        /// <summary>
        /// Returns the index of a pixel in the pixel list. If no pixel is found, -1 is returned instead
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to search</param>
        /// <param name="y">The Y coordinate of the pixel to search</param>
        /// <returns>The index of a pixel in the pixel list</returns>
        private int IndexOfPixel(int x, int y)
        {
            if (_pixelList.Count == 0)
                return -1;

            int id = x + y * _width;

            int s = 0;
            int e = _pixelList.Count - 1;

            while (s <= e)
            {
                int mid = s + (e - s) / 2;
                int idMid = _pixelList[mid].PixelIndex;

                if (idMid == id)
                {
                    return mid;
                }

                if (idMid > id)
                {
                    e = mid - 1;
                }
                else if (idMid < id)
                {
                    s = mid + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Packs any underlying data so it occupies memory more efficietly
        /// </summary>
        public void PackData()
        {
            // Trim pixel list capacity
            _pixelList.Capacity = _pixelList.Count;
        }

        /// <summary>
        /// Encapsulates an undo task on a single pixel
        /// </summary>
        public struct PixelUndo
        {
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
            /// The color to apply on a undo operation
            /// </summary>
            public uint UndoColor;

            /// <summary>
            /// The color to apply on a redo operation
            /// </summary>
            public readonly uint RedoColor;

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
                UndoColor = oldColor;
                RedoColor = newColor;
            }
        }
    }
}
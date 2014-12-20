using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Pixelaria.Data.Undo;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools.Abstracts
{
    /// <summary>
    /// Implements basic functionality to paint operations
    /// </summary>
    public abstract class BasePaintTool : IPaintTool
    {
        /// <summary>
        /// The PictureBox owning this PaintOperation
        /// </summary>
        protected ImageEditPanel.InternalPictureBox pictureBox;

        /// <summary>
        /// The cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        protected Cursor toolCursor;

        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public Cursor ToolCursor
        {
            get { return toolCursor; }
            protected set { toolCursor = value; }
        }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public virtual bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public virtual void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            pictureBox = targetPictureBox;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public virtual void Destroy() { }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public virtual void ChangeBitmap(Bitmap newBitmap) { }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void Paint(PaintEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseDown(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseMove(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseUp(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseLeave(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseEnter(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void KeyDown(KeyEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void KeyUp(KeyEventArgs e) { }

        /// <summary>
        /// Returns whether the given Point is within the image bounds.
        /// The point must be in absolute image position
        /// </summary>
        /// <param name="point">The point to get whether or not it's within the image</param>
        /// <returns>Whether the given point is within the image bounds</returns>
        protected virtual bool WithinBounds(Point point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X < pictureBox.Image.Width && point.Y < pictureBox.Image.Height;
        }

        /// <summary>
        /// Invalidates a region of the control, with the given coordinates and size.
        /// The region must be in absolute pixels in relation to the image being edited
        /// </summary>
        /// <param name="point">The point to invalidate</param>
        /// <param name="width">The width of the area to invalidate</param>
        /// <param name="height">The height of the area to invalidate</param>
        protected virtual void InvalidateRect(PointF point, float width, float height)
        {
            point = GetRelativePoint(GetAbsolutePoint(point));

            point.X -= 1;
            point.Y -= 1;

            Rectangle rec = new Rectangle((int)point.X, (int)point.Y, (int)(width * pictureBox.Zoom.Y), (int)(height * pictureBox.Zoom.Y));

            pictureBox.Invalidate(rec);
        }

        /// <summary>
        /// Returns the absolute position of the given point on the canvas image
        /// </summary>
        /// <returns>The absolute position of the given point on the canvas image</returns>
        protected virtual Point GetAbsolutePoint(PointF point)
        {
            return Point.Truncate(pictureBox.GetAbsolutePoint(point));
        }

        /// <summary>
        /// Returns the relative position of the given point on the control bounds
        /// </summary>
        /// <returns>The relative position of the given point on the control bounds</returns>
        protected virtual PointF GetRelativePoint(Point point)
        {
            return pictureBox.GetRelativePoint(point);
        }

        /// <summary>
        /// A per-pixel undo task
        /// </summary>
        public class PerPixelUndoTask : IUndoTask
        {
            /// <summary>
            /// List of pixels stored on this per-pixel undo
            /// </summary>
            private List<PixelUndo> _pixelList;

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
            /// The target InternalPictureBox to perform the undo operation on
            /// </summary>
            private PictureBox _pictureBox;

            /// <summary>
            /// The string that describes this PerPixelUndoTask
            /// </summary>
            private readonly string _description;

            /// <summary>
            /// Initializes a new instance of the PixelUndoTask
            /// </summary>
            /// <param name="pictureBox">The target for the undo operation</param>
            /// <param name="description">A description to use for this UndoTask</param>
            /// <param name="indexPixels">Whether to index the pixels being added so they appear sequentially on the pixel list</param>
            /// <param name="keepReplacedOriginals">Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.</param>
            public PerPixelUndoTask(PictureBox pictureBox, string description, bool indexPixels = false, bool keepReplacedOriginals = false)
            {
                _pixelList = new List<PixelUndo>();
                _pictureBox = pictureBox;
                _description = description;
                _indexPixels = indexPixels;
                _keepReplacedOriginals = keepReplacedOriginals;

                _width = pictureBox.Image.Width;
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
            /// Clears this pencil undo task
            /// </summary>
            public void Clear()
            {
                _pixelList.Clear();
                _pixelList = null;
                _pictureBox = null;
            }

            /// <summary>
            /// Performs the undo operation on this per-pixel undo task
            /// </summary>
            public void Undo()
            {
                using (FastBitmap bitmap = (_pictureBox.Image as Bitmap).FastLock())
                {
                    int c = _pixelList.Count;
                    for (int i = 0; i < c; i++)
                    {
                        PixelUndo pu = _pixelList[i];
                        bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.UndoColor);
                    }
                }

                _pictureBox.Invalidate();
            }

            /// <summary>
            /// Performs the redo operation on this per-pixel undo task
            /// </summary>
            public void Redo()
            {
                using (FastBitmap bitmap = (_pictureBox.Image as Bitmap).FastLock())
                {
                    int c = _pixelList.Count;
                    for (int i = 0; i < c; i++)
                    {
                        PixelUndo pu = _pixelList[i];
                        bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.RedoColor);
                    }
                }

                _pictureBox.Invalidate();
            }

            /// <summary>
            /// Returns the string description of this undo task
            /// </summary>
            /// <returns>The string description of this undo task</returns>
            public virtual string GetDescription()
            {
                return _description;
            }

            /// <summary>
            /// Encapsulates an undo task on a single pixel
            /// </summary>
            private struct PixelUndo
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
}
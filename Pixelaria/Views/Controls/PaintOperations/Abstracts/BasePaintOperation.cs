using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Pixelaria.Data.Undo;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations.Abstracts
{
    /// <summary>
    /// Implements basic functionality to paint operations
    /// </summary>
    public abstract class BasePaintOperation : IPaintOperation
    {
        /// <summary>
        /// The PictureBox owning this PaintOperation
        /// </summary>
        protected ImageEditPanel.InternalPictureBox pictureBox;

        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public virtual Cursor OperationCursor { get { return Cursors.Default; } protected set { } }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public virtual bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public virtual void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
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
            private List<PixelUndo> pixelList;

            /// <summary>
            /// Whether to index the pixels being added so they appear sequentially on the pixels list
            /// </summary>
            private bool indexPixels;

            /// <summary>
            /// Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.
            /// </summary>
            private bool keepReplacedOriginals;

            /// <summary>
            /// The width of the bitmap being affected
            /// </summary>
            private int width;

            /// <summary>
            /// The height of the bitmap being affected
            /// </summary>
            private int height;

            /// <summary>
            /// The target InternalPictureBox to perform the undo operation on
            /// </summary>
            private ImageEditPanel.InternalPictureBox pictureBox;

            /// <summary>
            /// The string that describes this PerPixelUndoTask
            /// </summary>
            private string description;

            /// <summary>
            /// Initializes a new instance of the PixelUndoTask
            /// </summary>
            /// <param name="targetPictureBox">The target for the undo operation</param>
            /// <param name="description">A description to use for this UndoTask</param>
            /// <param name="indexPixels">Whether to index the pixels being added so they appear sequentially on the pixel list</param>
            /// <param name="keepReplacedOriginals">Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.</param>
            public PerPixelUndoTask(ImageEditPanel.InternalPictureBox targetPictureBox, string description, bool indexPixels = false, bool keepReplacedOriginals = false)
            {
                this.pixelList = new List<PixelUndo>();
                this.pictureBox = targetPictureBox;
                this.description = description;
                this.indexPixels = indexPixels;
                this.keepReplacedOriginals = keepReplacedOriginals;

                this.width = targetPictureBox.Bitmap.Width;
                this.height = targetPictureBox.Bitmap.Height;
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
                // Early out: don't register duplicated pixels
                if (checkExisting && !indexPixels)
                {
                    foreach (PixelUndo pu in pixelList)
                    {
                        if (pu.PixelX == x && pu.PixelY == y)
                            return;
                    }
                }

                InternalRegisterPixel(x, y, oldColor, newColor, !checkExisting);
            }

            /// <summary>
            /// Registers a pixel on this PixelUndoTask 
            /// </summary>
            /// <param name="x">The X coordinate of the pixel to store</param>
            /// <param name="y">The Y coordinate of the pixel to store</param>
            /// <param name="oldColor">The old color of the pixel</param>
            /// <param name="newColor">The new color of the pixel</param>
            /// <param name="replaceExisting">Whether to allow relpacing existing pixels on the list</param>
            private void InternalRegisterPixel(int x, int y, int oldColor, int newColor, bool replaceExisting)
            {
                int pixelIndex = x + y * width;

                PixelUndo item = new PixelUndo() { PixelX = x, PixelY = y, PixelIndex = pixelIndex, UndoColor = oldColor, RedoColor = newColor };

                if (!indexPixels)
                {
                    pixelList.Add(item);
                    return;
                }

                int l = pixelList.Count;

                // Empty list: Add item directly
                if (l == 0)
                {
                    pixelList.Add(item);
                    return;
                }

                int s = 0;
                int e = l - 1;
                while (true)
                {
                    int idC, idM, idF;

                    idF = pixelList[e].PixelIndex;

                    // Pixel index of the item at the end of the interval is smaller than the current pixel index: Add
                    // item after the interval
                    if (idF < pixelIndex)
                    {
                        pixelList.Insert(e + 1, item);
                        return;
                    }
                    // Pixel index of the item at the end of the interval is equals to the item being added: Replace the pixel if replacing is allowed and quit
                    else if (idF == pixelIndex)
                    {
                        if (replaceExisting)
                        {
                            if (keepReplacedOriginals)
                            {
                                item.UndoColor = pixelList[e].UndoColor;
                            }

                            pixelList[e] = item;
                        }

                        return;
                    }

                    idC = pixelList[s].PixelIndex;

                    // Pixel index of the item at the start of the interval is larger than the current pixel index: Add
                    // item before the interval
                    if (idC > pixelIndex)
                    {
                        pixelList.Insert(s, item);
                        return;
                    }
                    // Pixel index of the item at the start of the interval is equals to the item being added: Replace the pixel if replacing is allowed and quit
                    else if (idC == pixelIndex)
                    {
                        if (replaceExisting)
                        {
                            if (keepReplacedOriginals)
                            {
                                item.UndoColor = pixelList[s].UndoColor;
                            }

                            pixelList[s] = item;
                        }

                        return;
                    }

                    int mid = s + (e - s) / 2;
                    idM = pixelList[mid].PixelIndex;

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
                            if (keepReplacedOriginals)
                            {
                                item.UndoColor = pixelList[mid].UndoColor;
                            }

                            pixelList[mid] = item;
                        }

                        return;
                    }

                    // End of search: Add item at the current index
                    if (s > e)
                    {
                        pixelList.Insert(s, item);
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
            private bool ContainsPixel(int x, int y)
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
                if (pixelList.Count == 0)
                    return -1;

                int id = x + y * width;

                int s = 0;
                int e = pixelList.Count - 1;

                while (s <= e)
                {
                    int mid = s + (e - s) / 2;
                    int idMid = pixelList[mid].PixelIndex;

                    if (idMid == id)
                    {
                        return mid;
                    }
                    else if (idMid > id)
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
            /// Clears this pencil undo task
            /// </summary>
            public void Clear()
            {
                pixelList.Clear();
                pixelList = null;
                pictureBox = null;
            }

            /// <summary>
            /// Performs the undo operation on this per-pixel undo task
            /// </summary>
            public void Undo()
            {
                FastBitmap bitmap = new FastBitmap(pictureBox.Bitmap);

                bitmap.Lock();

                foreach (PixelUndo pu in pixelList)
                {
                    bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.UndoColor);
                }

                bitmap.Unlock();

                pictureBox.Invalidate();
            }

            /// <summary>
            /// Performs the redo operation on this per-pixel undo task
            /// </summary>
            public void Redo()
            {
                FastBitmap bitmap = new FastBitmap(pictureBox.Bitmap);

                bitmap.Lock();

                foreach (PixelUndo pu in pixelList)
                {
                    bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.RedoColor);
                }

                bitmap.Unlock();

                pictureBox.Invalidate();
            }

            /// <summary>
            /// Returns the string description of this undo task
            /// </summary>
            /// <returns>The string description of this undo task</returns>
            public virtual string GetDescription()
            {
                return description;
            }

            /// <summary>
            /// Encapsulates an undo task on a single pixel
            /// </summary>
            private struct PixelUndo
            {
                /// <summary>
                /// The X position of the pixel to draw
                /// </summary>
                public int PixelX;

                /// <summary>
                /// The Y position of the pixel to draw
                /// </summary>
                public int PixelY;

                /// <summary>
                /// The absolute index of the pixel
                /// </summary>
                public int PixelIndex;

                /// <summary>
                /// The color to apply on a undo operation
                /// </summary>
                public int UndoColor;

                /// <summary>
                /// The color to apply on a redo operation
                /// </summary>
                public int RedoColor;

                /// <summary>
                /// Initializes a new instance of the PixelUndo struct
                /// </summary>
                /// <param name="x">The X position of the pixel to draw</param>
                /// <param name="y">The Y position of the pixel to draw</param>
                /// <param name="pixelIndex">The absolute index of the pixel</param>
                /// <param name="oldColor">The color to apply on a undo operation</param>
                /// <param name="newColor">The color to apply on a redo operation</param>
                public PixelUndo(int x, int y, int pixelIndex, Color oldColor, Color newColor)
                {
                    this.PixelX = x;
                    this.PixelY = y;
                    this.PixelIndex = pixelIndex;
                    this.UndoColor = oldColor.ToArgb();
                    this.RedoColor = newColor.ToArgb();
                }

                /// <summary>
                /// Initializes a new instance of the PixelUndo struct
                /// </summary>
                /// <param name="x">The X position of the pixel to draw</param>
                /// <param name="y">The Y position of the pixel to draw</param>
                /// <param name="pixelIndex">The absolute index of the pixel</param>
                /// <param name="oldColor">The color to apply on a undo operation</param>
                /// <param name="newColor">The color to apply on a redo operation</param>
                public PixelUndo(int x, int y, int pixelIndex, int oldColor, int newColor)
                {
                    this.PixelX = x;
                    this.PixelY = y;
                    this.PixelIndex = pixelIndex;
                    this.UndoColor = oldColor;
                    this.RedoColor = newColor;
                }
            }
        }
    }
}
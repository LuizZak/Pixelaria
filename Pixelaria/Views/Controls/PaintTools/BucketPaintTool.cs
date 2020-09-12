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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using FastBitmapLib;
using JetBrains.Annotations;
using Pixelaria.Data.Undo;
using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;
using PixelariaLib.Utils;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Bucket paint tool
    /// </summary>
    internal class BucketPaintTool : AbstractPaintTool, IColoredPaintTool, ICompositingPaintTool
    {
        /// <summary>
        /// The compositing mode for this paint Tool
        /// </summary>
        protected CompositingMode compositingMode;

        /// <summary>
        /// The first color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color _firstColor;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color _secondColor;

        /// <summary>
        /// The point at which the mouse is currently over
        /// </summary>
        private Point _mousePosition;

        /// <summary>
        /// The last recorded mouse position
        /// </summary>
        private Point _lastMousePosition;

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color FirstColor { get => _firstColor;
            set => _firstColor = value;
        }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor { get => _secondColor;
            set => _secondColor = value;
        }

        /// <summary>
        /// Gets or sets the compositing mode for this paint tool
        /// </summary>
        public CompositingMode CompositingMode { get => compositingMode;
            set => compositingMode = value;
        }

        /// <summary>
        /// Initializes a new instance of the BucketPaintTool class, setting the two drawing colors
        /// for the paint tool
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public BucketPaintTool(Color firstColor, Color secondColor)
        {
            _firstColor = firstColor;
            _secondColor = secondColor;
        }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(PaintingOperationsPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            var cursorMemoryStream = new MemoryStream(Properties.Resources.bucket_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Tool
        /// </summary>
        public override void Destroy()
        {
            ToolCursor.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            var point = GetAbsolutePoint(e.Location);

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                var color = e.Button == MouseButtons.Left ? _firstColor : _secondColor;

                if (WithinBounds(point))
                {
                    PerformBucketOperation(color, point);
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                ColorPickAtPoint(point, ColorIndex.Indifferent);
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            _mousePosition = e.Location;

            if (e.Button == MouseButtons.Middle)
            {
                var mouse = GetAbsolutePoint(_mousePosition);
                var mouseLast = GetAbsolutePoint(_lastMousePosition);

                if (mouse != mouseLast && WithinBounds(mouse))
                {
                    ColorPickAtPoint(mouse, ColorIndex.Indifferent);
                }
            }

            _lastMousePosition = _mousePosition;
        }

        /// <summary>
        /// Performs the bucket fill operation
        /// </summary>
        /// <param name="color">The color of the fill operation</param>
        /// <param name="point">The point to start the fill operation at</param>
        public void PerformBucketOperation(Color color, Point point)
        {
            var internalPictureBox = pictureBox;

            if (internalPictureBox?.Bitmap == null)
                return;

            var undoTask = PerformBucketOperation(internalPictureBox.Bitmap, color, point, compositingMode, true);

            if (undoTask != null)
            {
                internalPictureBox.OwningPanel.UndoSystem.RegisterUndo(undoTask);

                // Finish the operation by updating the picture box
                internalPictureBox.Invalidate();
                internalPictureBox.MarkModified();
            }
        }

        /// <summary>
        /// Performs the bucket fill operation
        /// </summary>
        /// <param name="targetBitmap">The target bitmap for this bucket operation</param>
        /// <param name="color">The color of the fill operation</param>
        /// <param name="point">The point to start the fill operation at</param>
        /// <param name="compMode">The CompositingMode of the bucket fill operation</param>
        /// <param name="createUndo">Whether to create and return an undo task</param>
        /// <returns>The undo task associated with this operation, or null, if createUndo is false or if the operation failed</returns>
        [CanBeNull]
        public static unsafe IUndoTask PerformBucketOperation([NotNull] Bitmap targetBitmap, Color color, Point point, CompositingMode compMode, bool createUndo)
        {
            // Start the fill operation by getting the color under the user's mouse
            var pColor = targetBitmap.GetPixel(point.X, point.Y);
            // Do a pre-blend of the color, if the composition mode is SourceOver
            var newColor = compMode == CompositingMode.SourceOver ? color.Blend(pColor) : color;

            uint pColorI = unchecked((uint)pColor.ToArgb());
            uint newColorI = unchecked((uint)newColor.ToArgb());

            // Don't do anything if the fill operation doesn't ends up changing any pixel color
            if (pColorI == newColorI || pColor == color && (compMode == CompositingMode.SourceOver && pColor.A == 255 || compMode == CompositingMode.SourceCopy))
            {
                return null;
            }

            // Clone the bitmap to be used on undo/redo
            Bitmap originalBitmap = null;
            if (createUndo)
            {
                originalBitmap = new Bitmap(targetBitmap);
            }

            int minX = point.X;
            int minY = point.Y;
            int maxX = point.X;
            int maxY = point.Y;

            // Initialize the undo task
            BitmapUndoTask undoTask = null;

            if (createUndo)
                undoTask = new BitmapUndoTask(targetBitmap, "Flood fill");

            var stack = new Stack<int>();

            int width = targetBitmap.Width;
            int height = targetBitmap.Height;

            stack.Push(((point.X << 16) | point.Y));

            // Flood-fill the bitmap
            using (var fastBitmap = targetBitmap.FastLock())
            {
                var scan0 = (uint*)fastBitmap.Scan0;
                int strideWidth = fastBitmap.Stride;

                // Do a flood-fill using a vertical scan-line algorithm
                while (stack.Count > 0)
                {
                    int v = stack.Pop();
                    int x = v >> 16;
                    int y = v & 0xFFFF;
                    
                    // Expand horizontal area
                    minX = x < minX ? x : minX;
                    maxX = x > maxX ? x : maxX;

                    int y1 = y;

                    while (y1 >= 0 && *(scan0 + x + y1 * strideWidth) == pColorI) y1--;

                    y1++;
                    bool spanLeft = false, spanRight = false;

                    int row = y1 * strideWidth;

                    // Expand vertical area before traversal
                    minY = y1 < minY ? y1 : minY;
                    maxY = y1 > maxY ? y1 : maxY;

                    while (y1 < height && *(scan0 + x + row) == pColorI)
                    {
                        // Expand affected region boundaries
                        *(scan0 + x + row) = newColorI;

                        uint pixel;

                        if (x > 0)
                        {
                            pixel = *(scan0 + (x - 1) + row);

                            if (!spanLeft && pixel == pColorI)
                            {
                                stack.Push(((x - 1) << 16) | y1);

                                spanLeft = true;
                            }
                            else if (spanLeft && pixel != pColorI)
                            {
                                spanLeft = false;
                            }
                        }

                        if (x < width - 1)
                        {
                            pixel = *(scan0 + (x + 1) + row);

                            if (!spanRight && pixel == pColorI)
                            {
                                stack.Push(((x + 1) << 16) | y1);
                                spanRight = true;
                            }
                            else if (spanRight && pixel != pColorI)
                            {
                                spanRight = false;
                            }
                        }
                        y1++;
                        row = y1 * strideWidth;
                    }

                    // Expand vertical area after y traversal
                    minY = y1 < minY ? y1 : minY;
                    maxY = y1 > maxY ? y1 : maxY;
                }
            }

            if (!createUndo)
            {
                return null;
            }

            // Generate the undo now
            var affectedRectangle = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

            undoTask.DrawPoint = affectedRectangle.Location;

            // Slice and persist the undo/redo bitmap regions
            if (affectedRectangle != new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height))
            {
                undoTask.SetOldBitmap(FastBitmap.SliceBitmap(originalBitmap, affectedRectangle), false);
                originalBitmap.Dispose();
            }
            else
            {
                undoTask.SetOldBitmap(originalBitmap, false);
            }

            undoTask.SetNewBitmap(FastBitmap.SliceBitmap(targetBitmap, affectedRectangle), false);

            return undoTask;
        }
    }
}
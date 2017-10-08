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

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Algorithms.PaintOperations;
using Pixelaria.Algorithms.PaintOperations.UndoTasks;

using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Line paint operation
    /// </summary>
    internal class LinePaintTool : BaseDraggingPaintTool, IColoredPaintTool, ICompositingPaintTool, ISizedPaintTool
    {
        /// <summary>
        /// Graphics used to draw on the bitmap
        /// </summary>
        private Graphics _graphics;

        /// <summary>
        /// The first color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color _firstColor;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color _secondColor;

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color FirstColor
        {
            get => _firstColor;
            set => _firstColor = value;
        }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor
        {
            get => _secondColor;
            set => _secondColor = value;
        }

        /// <summary>
        /// Gets or sets the size of this LinePaintTool
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get; set; }

        /// <summary>
        /// Initialies a new instance of the LinePaintTool class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        /// <param name="size">The size for this line paint tool</param>
        public LinePaintTool(Color firstColor, Color secondColor, int size)
        {
            _firstColor = firstColor;
            _secondColor = secondColor;
            Size = size;
        }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize([NotNull] ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            var cursorMemoryStream = new MemoryStream(Properties.Resources.line_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
            
            mouseDown = false;
            
            _graphics = Graphics.FromImage(targetPictureBox.Image);

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Tool
        /// </summary>
        public override void Destroy()
        {
            pictureBox = null;

            ToolCursor.Dispose();

            _graphics.Flush();
            _graphics.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintTool that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            var internalPictureBox = pictureBox;
            if (internalPictureBox == null)
                return;

            if (mouseDown && (mouseButton == MouseButtons.Left || mouseButton == MouseButtons.Right))
            {
                PerformLineOperation(mouseButton == MouseButtons.Left ? _firstColor : _secondColor,
                    mouseDownAbsolutePoint, mouseAbsolutePoint, internalPictureBox.Buffer, CompositingMode, Size, false);
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            var internalPictureBox = pictureBox;
            if (internalPictureBox == null)
                return;

            if (e.Button == MouseButtons.Middle)
            {
                if (internalPictureBox.Bitmap != null)
                    _firstColor = internalPictureBox.Bitmap.GetPixel(mouseDownAbsolutePoint.X, mouseDownAbsolutePoint.Y);

                internalPictureBox.OwningPanel.FireColorChangeEvent(_firstColor);

                internalPictureBox.Invalidate();

                mouseDown = false;
            }
            else
            {
                var rec = GetCurrentRectangle(true);
                
                rec.X -= (int)(Size * internalPictureBox.Zoom.X) * 2;
                rec.Y -= (int)(Size * internalPictureBox.Zoom.Y) * 2;

                rec.Width += (int)(Size * internalPictureBox.Zoom.X) * 4;
                rec.Height += (int)(Size * internalPictureBox.Zoom.Y) * 4;

                internalPictureBox.Invalidate(rec);
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            var internalPictureBox = pictureBox;
            if (internalPictureBox == null)
                return;

            if (mouseDown)
            {
                var newArea = GetCurrentRectangle(true);

                internalPictureBox.Invalidate(newArea);

                // Draw the rectangle on the image now
                var rectArea = GetCurrentRectangle(false);

                if (rectArea.Width > 0 && rectArea.Height > 0)
                {
                    var color = (mouseButton == MouseButtons.Left ? _firstColor : _secondColor);

                    Debug.Assert(internalPictureBox.Bitmap != null, "pictureBox.Bitmap != null");
                    var task = PerformLineOperation(color, mouseDownAbsolutePoint, mouseAbsolutePoint, internalPictureBox.Bitmap, CompositingMode, Size, true);

                    if(task != null && task.PixelHistoryTracker.PixelCount > 0)
                    {
                        internalPictureBox.OwningPanel.UndoSystem.RegisterUndo(task);
                        internalPictureBox.MarkModified();
                    }
                }
            }

            mouseDown = false;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="relative">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            var rec1 = GetRelativeCircleBounds(mouseDownAbsolutePoint, Size + 1);
            var rec2 = GetRelativeCircleBounds(mouseAbsolutePoint, Size + 1);

            return Rectangle.Union(rec1, rec2);
        }

        /// <summary>
        /// Performs the Line paint operation with the given parameters
        /// </summary>
        /// <param name="color">The color to use when drawing the line</param>
        /// <param name="firstPoint">The first point of the line to draw</param>
        /// <param name="secondPoint">The second point of the line to draw</param>
        /// <param name="bitmap">The Bitmap to draw the line on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the line</param>
        /// <param name="size">The size of the line to draw</param>
        /// <param name="recordUndo">Whether to generate an undo operation for this line operation</param>
        /// <returns>An undo task for the line operation, or null, if recordUndo is false</returns>
        [CanBeNull]
        public static PerPixelUndoTask PerformLineOperation(Color color, Point firstPoint, Point secondPoint,
            [NotNull] Bitmap bitmap, CompositingMode compositingMode, int size, bool recordUndo)
        {
            PlottingPaintUndoGenerator generator = null;

            if (recordUndo)
            {
                generator = new PlottingPaintUndoGenerator(bitmap, "Line");
            }

            var operation = new PencilPaintOperation(bitmap, true, firstPoint)
            {
                Color = color,
                CompositingMode = compositingMode,
                Notifier = generator,
                Size = size
            };

            operation.StartOpertaion(false);
            operation.DrawTo(secondPoint.X, secondPoint.Y);
            operation.FinishOperation();

            return recordUndo ? generator.UndoTask : null;
        }
    }
}
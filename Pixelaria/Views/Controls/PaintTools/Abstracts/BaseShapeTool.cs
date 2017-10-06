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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Data.Undo;

namespace Pixelaria.Views.Controls.PaintTools.Abstracts
{
    /// <summary>
    /// Base class for shape dragging paint operations
    /// </summary>
    public abstract class BaseShapeTool : BaseDraggingPaintTool
    {
        /// <summary>
        /// The compositing mode for this paint operation
        /// </summary>
        protected CompositingMode compositingMode;

        /// <summary>
        /// The fill mode for this paint operation
        /// </summary>
        protected OperationFillMode fillMode;

        /// <summary>
        /// Graphics used to draw on the buffer bitmap
        /// </summary>
        protected Graphics graphics;

        /// <summary>
        /// The buffer bitmap for drawing the shape on
        /// </summary>
        protected Bitmap buffer;

        /// <summary>
        /// The first color currently being used to paint on the InternalPictureBox
        /// </summary>
        protected Color firstColor;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        protected Color secondColor;

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color FirstColor
        {
            get => firstColor;
            set => firstColor = value;
        }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor
        {
            get => secondColor;
            set => secondColor = value;
        }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get => compositingMode;
            set => compositingMode = value;
        }

        /// <summary>
        /// Gets or sets the fill mode for this paint operation
        /// </summary>
        public OperationFillMode FillMode { get => fillMode;
            set { fillMode = value; if (Loaded) { pictureBox.Invalidate(); } } }

        /// <summary>
        /// Initialies a new instance of the BaseShapeTool class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        protected BaseShapeTool(Color firstColor, Color secondColor)
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
        }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.rect_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            mouseDown = false;

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Tool
        /// </summary>
        public override void Destroy()
        {
            Loaded = false;

            FinishOperation();

            pictureBox = null;

            if (graphics != null)
            {
                graphics.Flush();
                graphics.Dispose();
            }

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            ToolCursor.Dispose();
        }

        /// <summary>
        /// Called to notify this PaintTool that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            if (mouseDown)
            {
                Rectangle rec = GetCurrentRectangle(false);

                Color fc = firstColor;
                Color sc = secondColor;

                if (mouseButton == MouseButtons.Right)
                {
                    sc = firstColor;
                    fc = secondColor;
                }

                graphics.Clear(Color.Transparent);

                PerformShapeOperation(fc, sc, rec, pictureBox.Buffer, compositingMode, fillMode, false);
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            buffer = new Bitmap(pictureBox.Width, pictureBox.Height);
            graphics = Graphics.FromImage(buffer);
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp([NotNull] MouseEventArgs e)
        {
            Rectangle oldArea = GetCurrentRectangle(true);

            mouseAbsolutePoint = GetAbsolutePoint(e.Location);

            Rectangle newArea = GetCurrentRectangle(true);

            pictureBox.Invalidate(oldArea);
            pictureBox.Invalidate(newArea);

            FinishOperation();

            base.MouseUp(e);
        }

        /// <summary>
        /// Finishes this BaseShapeTool's current drawing operation
        /// </summary>
        public virtual void FinishOperation()
        {
            if (!mouseDown)
                return;

            // Draw the rectangle on the image now
            Rectangle rectArea = GetCurrentRectangle(false);

            if (rectArea.Width > 0 && rectArea.Height > 0)
            {
                Color fc = firstColor;
                Color sc = secondColor;

                if (mouseButton == MouseButtons.Right)
                {
                    sc = firstColor;
                    fc = secondColor;
                }

                PerformShapeOperation(fc, sc, GetCurrentRectangle(false), pictureBox.Bitmap, compositingMode, fillMode, true);

                pictureBox.MarkModified();
            }

            buffer.Dispose();
            buffer = null;

            graphics.Dispose();
            graphics = null;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="relative">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            Point point = mouseAbsolutePoint;

            point.Offset(1, 1);

            Rectangle rec = GetRectangleArea(new [] { mouseDownAbsolutePoint, point }, relative);

            if (shiftDown)
            {
                rec.Width = Math.Max(rec.Width, rec.Height);
                rec.Height = Math.Max(rec.Width, rec.Height);
            }

            return rec;
        }

        /// <summary>
        /// Performs the shape paint operation with the given parameters
        /// </summary>
        /// <param name="color1">The first color to use when drawing the shape</param>
        /// <param name="color2">The second color to use when drawing the shape</param>
        /// <param name="area">The area of the shape to draw</param>
        /// <param name="bitmap">The Bitmap to draw the shape on</param>
        /// <param name="compMode">The CompositingMode to use when drawing the shape</param>
        /// <param name="opFillMode">The fill mode for this shape operation</param>
        /// <param name="registerUndo">Whether to register an undo task for this shape operation</param>
        public abstract ShapeUndoTask PerformShapeOperation(Color color1, Color color2, Rectangle area, Bitmap bitmap, CompositingMode compMode, OperationFillMode opFillMode, bool registerUndo);

        /// <summary>
        /// Performs the shape paint operation with the given parameters
        /// </summary>
        /// <param name="color1">The first color to use when drawing the shape</param>
        /// <param name="color2">The second color to use when drawing the shape</param>
        /// <param name="area">The area of the shape to draw</param>
        /// <param name="gph">The Graphics to draw the shape on</param>
        /// <param name="compMode">The CompositingMode to use when drawing the shape</param>
        /// <param name="opFillMode">The fill mode for this shape operation</param>
        /// <param name="registerUndo">Whether to register an undo task for this shape operation</param>
        public abstract void PerformShapeOperation(Color color1, Color color2, Rectangle area, Graphics gph, CompositingMode compMode, OperationFillMode opFillMode, bool registerUndo);

        /// <summary>
        /// Represents a shape drawing undo task
        /// </summary>
        public abstract class ShapeUndoTask  : IUndoTask
        {
            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public abstract void Clear();

            /// <summary>
            /// Undoes this shape task
            /// </summary>
            public abstract void Undo();

            /// <summary>
            /// Redoes this shape task
            /// </summary>
            public abstract void Redo();

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public abstract string GetDescription();
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Data.Undo;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Rectangle paint operation
    /// </summary>
    public class RectanglePaintOperation : BaseShapeOperation, IPaintOperation, IColoredPaintOperation, ICompositingPaintOperation, IFillModePaintOperation
    {
        /// <summary>
        /// Initialies a new instance of the RectanglePaintOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public RectanglePaintOperation(Color firstColor, Color secondColor)
            : base(firstColor, secondColor)
        {
            
        }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.rect_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            this.Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();

            this.OperationCursor.Dispose();
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            Rectangle rec = GetRectangleArea(new Point[] { mouseDownAbsolutePoint, mouseAbsolutePoint }, relative);

            if (relative)
            {
                rec.Width += (int)(pictureBox.Zoom.X);
                rec.Height += (int)(pictureBox.Zoom.Y);
            }
            else
            {
                rec.Width ++;
                rec.Height ++;
            }

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
        /// <param name="firstColor">The first color to use when drawing the shape</param>
        /// <param name="secondColor">The second color to use when drawing the shape</param>
        /// <param name="area">The area of the shape to draw</param>
        /// <param name="bitmap">The Bitmap to draw the shape on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the shape</param>
        /// <param name="fillMode">The fill mode for this shape operation</param>
        /// <param name="registerUndo">Whether to register an undo task for this shape operation</param>
        public override void PerformShapeOperation(Color firstColor, Color secondColor, Rectangle area, Bitmap bitmap, CompositingMode compositingMode, OperationFillMode fillMode, bool registerUndo)
        {
            if (registerUndo)
                pictureBox.OwningPanel.UndoSystem.RegisterUndo(new RectangleUndoTask(pictureBox, firstColor, secondColor, area, compositingMode, fillMode));

            PerformRectangleOperation(firstColor, secondColor, area, bitmap, compositingMode, fillMode);
        }

        /// <summary>
        /// Performs the shape paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the shape</param>
        /// <param name="secondColor">The second color to use when drawing the shape</param>
        /// <param name="area">The area of the shape to draw</param>
        /// <param name="graphics">The Graphics to draw the shape on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the shape</param>
        /// <param name="fillMode">The fill mode for this shape operation</param>
        /// <param name="registerUndo">Whether to register an undo task for this shape operation</param>
        public override void PerformShapeOperation(Color firstColor, Color secondColor, Rectangle area, Graphics graphics, CompositingMode compositingMode, OperationFillMode fillMode, bool registerUndo)
        {
            PerformRectangleOperation(firstColor, secondColor, area, graphics, compositingMode, fillMode);
        }

        /// <summary>
        /// Performs the Rectangle paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the rectangle</param>
        /// <param name="secondColor">The second color to use when drawing the rectangle</param>
        /// <param name="area">The area of the rectangle to draw</param>
        /// <param name="bitmap">The Bitmap to draw the rectangle on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
        /// <param name="fillMode">The fill mode for this rectangle operation</param>
        public static void PerformRectangleOperation(Color firstColor, Color secondColor, Rectangle area, Bitmap bitmap, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CompositingMode = compositingMode;

            PerformRectangleOperation(firstColor, secondColor, area, graphics, compositingMode, fillMode);

            graphics.Flush();
            graphics.Dispose();
        }

        /// <summary>
        /// Performs the Rectangle paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the rectangle</param>
        /// <param name="secondColor">The second color to use when drawing the rectangle</param>
        /// <param name="area">The area of the rectangle to draw</param>
        /// <param name="graphics">The Graphics to draw the rectangle on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
        /// <param name="fillMode">The fill mode for this rectangle operation</param>
        public static void PerformRectangleOperation(Color firstColor, Color secondColor, Rectangle area, Graphics graphics, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            Brush brush = new SolidBrush((fillMode == OperationFillMode.SolidFillFirstColor ? firstColor : secondColor));

            if (fillMode == OperationFillMode.SolidFillFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor || fillMode == OperationFillMode.SolidFillSecondColor)
            {
                Rectangle nArea = area;

                if (fillMode == OperationFillMode.OutlineFirstColorFillSecondColor)
                    nArea.Inflate(-1, -1);

                graphics.FillRectangle(brush, nArea);
            }

            if (fillMode == OperationFillMode.OutlineFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor)
            {
                Pen pen = new Pen(firstColor);

                area.Inflate(-1, -1);

                RectangleF nArea = area;

                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                nArea.X -= 0.5f;
                nArea.Y -= 0.5f;
                nArea.Width += 1f;
                nArea.Height += 1f;

                graphics.DrawRectangle(pen, nArea.X, nArea.Y, nArea.Width, nArea.Height);

                pen.Dispose();
            }

            brush.Dispose();
        }

        /// <summary>
        /// A rectangle undo task
        /// </summary>
        protected class RectangleUndoTask : IUndoTask
        {
            /// <summary>
            /// The target InternalPictureBox of this RectangleUndoTask
            /// </summary>
            ImageEditPanel.InternalPictureBox targetPictureBox;

            /// <summary>
            /// The area of the the image that was affected by the Rectangle operation
            /// </summary>
            Rectangle area;

            /// <summary>
            /// The first color used to draw the Rectangle
            /// </summary>
            Color firstColor;

            /// <summary>
            /// The second color used to draw the Rectangle
            /// </summary>
            Color secondColor;

            /// <summary>
            /// The original slice of bitmap that represents the image region before the rectangle
            /// was drawn
            /// </summary>
            Bitmap originalSlice;

            /// <summary>
            /// The bitmap where the Rectangle was drawn on
            /// </summary>
            Bitmap bitmap;

            /// <summary>
            /// The compositing mode of the paint operation
            /// </summary>
            CompositingMode compositingMode;

            /// <summary>
            /// The fill mode for the paint operation
            /// </summary>
            OperationFillMode fillMode;

            /// <summary>
            /// Initializes a new instance of the RectangleUndoTask class
            /// </summary>
            /// <param name="targetPictureBox">The target InternalPictureBox of this RectangleUndoTask</param>
            /// <param name="firstColor">The first color to use when drawing the rectangle</param>
            /// <param name="secondColor">The second color to use when drawing the rectangle</param>
            /// <param name="area">The area of the rectangle to draw</param>
            /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
            /// <param name="fillMode">The fill mode for this rectangle operation</param>
            public RectangleUndoTask(ImageEditPanel.InternalPictureBox targetPictureBox, Color firstColor, Color secondColor, Rectangle area, CompositingMode compositingMode, OperationFillMode fillMode)
            {
                this.targetPictureBox = targetPictureBox;
                this.firstColor = firstColor;
                this.secondColor = secondColor;
                this.area = area;
                this.bitmap = targetPictureBox.Bitmap;
                this.compositingMode = compositingMode;
                this.fillMode = fillMode;

                // Take the image slide now
                this.originalSlice = new Bitmap(area.Width, area.Height);
                
                Graphics g = Graphics.FromImage(originalSlice);
                g.DrawImage(bitmap, new Point(-area.X, -area.Y));
                g.Flush();
                g.Dispose();
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                originalSlice.Dispose();
                bitmap = null;
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                // Redraw the original slice back to the image
                Graphics g = Graphics.FromImage(bitmap);
                g.SetClip(area);
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;
                
                g.DrawImage(originalSlice, area);

                g.Flush();
                g.Dispose();

                // Invalidate the target box
                targetPictureBox.Invalidate();
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                // Draw the rectangle again
                RectanglePaintOperation.PerformRectangleOperation(firstColor, secondColor, area, bitmap, compositingMode, fillMode);

                // Invalidate the target box
                targetPictureBox.Invalidate();
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public string GetDescription()
            {
                return "Rectangle";
            }
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations.Abstracts
{
    /// <summary>
    /// Base class for shape dragging paint operations
    /// </summary>
    public abstract class BaseShapeOperation : BaseDraggingPaintOperation, IPaintOperation
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
        protected Color firstColor = Color.Black;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        protected Color secondColor = Color.Black;

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color FirstColor
        {
            get { return firstColor; }
            set
            {
                firstColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor
        {
            get { return secondColor; }
            set
            {
                secondColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get { return compositingMode; } set { compositingMode = value; } }

        /// <summary>
        /// Gets or sets the fill mode for this paint operation
        /// </summary>
        public OperationFillMode FillMode { get { return fillMode; } set { fillMode = value; if (Loaded) { pictureBox.Invalidate(); } } }

        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// Initialies a new instance of the BaseShapeOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public BaseShapeOperation(Color firstColor, Color secondColor)
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
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

            this.mouseDown = false;

            this.Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            Loaded = false;

            FinishOperation();

            this.pictureBox = null;

            if (this.graphics != null)
            {
                this.graphics.Flush();
                this.graphics.Dispose();
            }

            if (this.buffer != null)
            {
                buffer.Dispose();
                this.buffer = null;
            }

            this.OperationCursor.Dispose();
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
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
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            buffer = new Bitmap(pictureBox.Width, pictureBox.Height);
            graphics = Graphics.FromImage(buffer);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
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
        /// Finishes this BaseShapeOperation's current drawing operation
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
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            Point point = mouseAbsolutePoint;

            point.Offset(1, 1);

            Rectangle rec = GetRectangleArea(new Point[] { mouseDownAbsolutePoint, point }, relative);

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
        public abstract void PerformShapeOperation(Color firstColor, Color secondColor, Rectangle area, Bitmap bitmap, CompositingMode compositingMode, OperationFillMode fillMode, bool registerUndo);

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
        public abstract void PerformShapeOperation(Color firstColor, Color secondColor, Rectangle area, Graphics graphics, CompositingMode compositingMode, OperationFillMode fillMode, bool registerUndo);
    }
}
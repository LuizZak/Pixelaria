using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Data.Undo;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Line paint operation
    /// </summary>
    public class LinePaintOperation : BaseDraggingPaintOperation, IPaintOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// The compositing mode for this paint operation
        /// </summary>
        private CompositingMode compositingMode;

        /// <summary>
        /// Graphics used to draw on the bitmap
        /// </summary>
        private Graphics graphics;

        /// <summary>
        /// The first color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color firstColor = Color.Black;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color secondColor = Color.Black;

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
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// Initialies a new instance of the LinePaintOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public LinePaintOperation(Color firstColor, Color secondColor)
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
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.line_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
            
            this.mouseDown = false;

            this.graphics = Graphics.FromImage(pictureBox.Image);

            this.Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            this.pictureBox = null;

            this.OperationCursor.Dispose();

            this.graphics.Flush();
            this.graphics.Dispose();

            this.Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            if (mouseDown && (mouseButton == MouseButtons.Left || mouseButton == MouseButtons.Right))
            {
                PerformLineOperation((mouseButton == MouseButtons.Left ? firstColor : secondColor), mouseDownAbsolutePoint, mouseAbsolutePoint, pictureBox.Buffer, compositingMode);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            if (e.Button == MouseButtons.Middle)
            {
                firstColor = pictureBox.Bitmap.GetPixel(mouseDownAbsolutePoint.X, mouseDownAbsolutePoint.Y);

                pictureBox.OwningPanel.FireColorChangeEvent(firstColor);

                pictureBox.Invalidate();

                mouseDown = false;
            }
            else
            {
                Rectangle rec = GetCurrentRectangle(true);

                pictureBox.Invalidate(rec);
            }
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
            if (mouseDown)
            {
                Rectangle newArea = GetCurrentRectangle(true);

                pictureBox.Invalidate(newArea);

                // Draw the rectangle on the image now
                Rectangle rectArea = GetCurrentRectangle(false);

                if (rectArea.Width > 0 && rectArea.Height > 0)
                {
                    Color color = (mouseButton == MouseButtons.Left ? firstColor : secondColor);

                    pictureBox.OwningPanel.UndoSystem.RegisterUndo(new LineUndoTask(pictureBox, color, mouseDownAbsolutePoint, mouseAbsolutePoint, compositingMode));

                    PerformLineOperation(color, mouseDownAbsolutePoint, mouseAbsolutePoint, pictureBox.Bitmap, compositingMode);

                    pictureBox.MarkModified();
                }
            }

            mouseDown = false;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            Rectangle rec = GetRectangleArea(new Point[] { mouseDownAbsolutePoint, mouseAbsolutePoint }, relative);

            rec.Width += (int)(pictureBox.Zoom.X);
            rec.Height += (int)(pictureBox.Zoom.Y);

            return rec;
        }

        /// <summary>
        /// Performs the Line paint operation with the given parameters
        /// </summary>
        /// <param name="color">The color to use when drawing the line</param>
        /// <param name="firstPoint">The first point of the line to draw</param>
        /// <param name="secondPoint">The second point of the line to draw</param>
        /// <param name="bitmap">The Bitmap to draw the line on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the line</param>
        public static void PerformLineOperation(Color color, Point firstPoint, Point secondPoint, Bitmap bitmap, CompositingMode compositingMode)
        {
            // Implemented using the Bresenham's line algorithm
            int x0 = firstPoint.X;
            int y0 = firstPoint.Y;
            int x1 = secondPoint.X;
            int y1 = secondPoint.Y;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t = x0;
                x0 = y0;
                y0 = t;

                t = x1;
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t = x0;
                x0 = x1;
                x1 = t;

                t = y0;
                y0 = y1;
                y1 = t;
            }
            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep;
            int y = y0;

            if (y0 < y1)
                ystep = 1;
            else
                ystep = -1;

            using (FastBitmap fastBitmap = bitmap.FastLock())
            {
                Point p = new Point();
                for (int x = x0; x <= x1; x++)
                {
                    if (steep)
                    {
                        p.X = y;
                        p.Y = x;
                    }
                    else
                    {
                        p.X = x;
                        p.Y = y;
                    }

                    if (p.X < 0 || p.X >= fastBitmap.Width || p.Y < 0 || p.Y >= fastBitmap.Height)
                    {
                        continue;
                    }

                    if (compositingMode == CompositingMode.SourceOver)
                    {
                        Color newColor = color.Blend(fastBitmap.GetPixel(p.X, p.Y));

                        fastBitmap.SetPixel(p.X, p.Y, newColor);
                    }
                    else
                    {
                        fastBitmap.SetPixel(p.X, p.Y, color);
                    }

                    error = error - deltay;
                    if (error < 0)
                    {
                        y = y + ystep;
                        error = error + deltax;
                    }
                }
            }
        }

        /// <summary>
        /// Performs the Line paint operation with the given parameters
        /// </summary>
        /// <param name="color">The color to use when drawing the line</param>
        /// <param name="firstPoint">The first point of the line to draw</param>
        /// <param name="secondPoint">The second point of the line to draw</param>
        /// <param name="bitmap">The Graphics to draw the line on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the line</param>
        public static void PerformLineOperation(Color color, Point firstPoint, Point secondPoint, Graphics graphics, CompositingMode compositingMode)
        {
            // Implemented using the Bresenham's line algorithm
            int x0 = firstPoint.X;
            int y0 = firstPoint.Y;
            int x1 = secondPoint.X;
            int y1 = secondPoint.Y;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t = x0;
                x0 = y0;
                y0 = t;

                t = x1;
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t = x0;
                x0 = x1;
                x1 = t;

                t = y0;
                y0 = y1;
                y1 = t;
            }
            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep;
            int y = y0;

            if (y0 < y1)
                ystep = 1;
            else
                ystep = -1;

            Brush brush = new SolidBrush(color);

            Point p = new Point();
            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    p.X = y;
                    p.Y = x;
                }
                else
                {
                    p.X = x;
                    p.Y = y;
                }

                if (compositingMode == CompositingMode.SourceOver)
                {
                    Rectangle rec = new Rectangle(p.X, p.Y, 1, 1);

                    graphics.FillRectangle(brush, rec);
                }
                else
                {
                    Rectangle rec = new Rectangle(p.X, p.Y, 1, 1);

                    graphics.FillRectangle(brush, rec);
                }

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }

            graphics.Flush();

            brush.Dispose();
        }

        /// <summary>
        /// A line undo task
        /// </summary>
        protected class LineUndoTask : IUndoTask
        {
            /// <summary>
            /// The target InternalPictureBox of this RectangleUndoTask
            /// </summary>
            ImageEditPanel.InternalPictureBox targetPictureBox;

            /// <summary>
            /// The area of the the image that was affected by the line operation
            /// </summary>
            Rectangle area;

            /// <summary>
            /// The point of the start of the line
            /// </summary>
            Point lineStart;

            /// <summary>
            /// The point of the end of the line
            /// </summary>
            Point lineEnd;

            /// <summary>
            /// The color used to draw the line
            /// </summary>
            Color color;

            /// <summary>
            /// The original slice of bitmap that represents the image region before the rectangle
            /// was drawn
            /// </summary>
            Bitmap originalSlice;

            /// <summary>
            /// The bitmap where the line was drawn on
            /// </summary>
            Bitmap bitmap;

            /// <summary>
            /// The compositing mode of the paint operation
            /// </summary>
            CompositingMode compositingMode;

            /// <summary>
            /// Initializes a new instance of the RectangleUndoTask class
            /// </summary>
            /// <param name="targetPictureBox">The target InternalPictureBox of this RectangleUndoTask</param>
            /// <param name="color">The color to use when drawing the rectangle</param>
            /// <param name="area">The area of the rectangle to draw</param>
            /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
            public LineUndoTask(ImageEditPanel.InternalPictureBox targetPictureBox, Color color, Point lineStart, Point lineEnd, CompositingMode compositingMode)
            {
                this.targetPictureBox = targetPictureBox;
                this.color = color;
                this.lineStart = lineStart;
                this.lineEnd = lineEnd;
                this.bitmap = targetPictureBox.Bitmap;
                this.compositingMode = compositingMode;

                this.area = LinePaintOperation.GetRectangleAreaAbsolute(new Point[] { lineStart, lineEnd });

                area.Offset(-1, -1);
                area.Inflate(2, 2);

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
                LinePaintOperation.PerformLineOperation(color, lineStart, lineEnd, bitmap, compositingMode);

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
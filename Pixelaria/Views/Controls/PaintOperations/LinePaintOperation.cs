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
    public class LinePaintOperation : BaseDraggingPaintOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// The compositing mode for this paint operation
        /// </summary>
        private CompositingMode _compositingMode;

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
            get { return _firstColor; }
            set
            {
                _firstColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor
        {
            get { return _secondColor; }
            set
            {
                _secondColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get { return _compositingMode; } set { _compositingMode = value; } }

        /// <summary>
        /// Initialies a new instance of the LinePaintOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public LinePaintOperation(Color firstColor, Color secondColor)
        {
            _firstColor = firstColor;
            _secondColor = secondColor;
        }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.line_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
            
            mouseDown = false;

            _graphics = Graphics.FromImage(targetPictureBox.Image);

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            pictureBox = null;

            OperationCursor.Dispose();

            _graphics.Flush();
            _graphics.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            if (mouseDown && (mouseButton == MouseButtons.Left || mouseButton == MouseButtons.Right))
            {
                PerformLineOperation((mouseButton == MouseButtons.Left ? _firstColor : _secondColor), mouseDownAbsolutePoint, mouseAbsolutePoint, pictureBox.Buffer, _compositingMode);
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
                _firstColor = pictureBox.Bitmap.GetPixel(mouseDownAbsolutePoint.X, mouseDownAbsolutePoint.Y);

                pictureBox.OwningPanel.FireColorChangeEvent(_firstColor);

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
                    Color color = (mouseButton == MouseButtons.Left ? _firstColor : _secondColor);

                    pictureBox.OwningPanel.UndoSystem.RegisterUndo(new LineUndoTask(pictureBox, color, mouseDownAbsolutePoint, mouseAbsolutePoint, _compositingMode));

                    PerformLineOperation(color, mouseDownAbsolutePoint, mouseAbsolutePoint, pictureBox.Bitmap, _compositingMode);

                    pictureBox.MarkModified();
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
            Rectangle rec = GetRectangleArea(new [] { mouseDownAbsolutePoint, mouseAbsolutePoint }, relative);

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
        /// <param name="graphics">The Graphics to draw the line on</param>
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
            readonly ImageEditPanel.InternalPictureBox _targetPictureBox;

            /// <summary>
            /// The area of the the image that was affected by the line operation
            /// </summary>
            readonly Rectangle _area;

            /// <summary>
            /// The point of the start of the line
            /// </summary>
            readonly Point _lineStart;

            /// <summary>
            /// The point of the end of the line
            /// </summary>
            readonly Point _lineEnd;

            /// <summary>
            /// The color used to draw the line
            /// </summary>
            readonly Color _color;

            /// <summary>
            /// The original slice of bitmap that represents the image region before the rectangle
            /// was drawn
            /// </summary>
            readonly Bitmap _originalSlice;

            /// <summary>
            /// The bitmap where the line was drawn on
            /// </summary>
            readonly Bitmap _bitmap;

            /// <summary>
            /// The compositing mode of the paint operation
            /// </summary>
            readonly CompositingMode _compositingMode;

            /// <summary>
            /// Initializes a new instance of the LineUndoTask class
            /// </summary>
            /// <param name="targetPictureBox">The target InternalPictureBox of this LineUndoTask</param>
            /// <param name="color">The color to use when drawing the line</param>
            /// <param name="lineStart">The starting point of the line</param>
            /// <param name="lineEnd">The ending point of the line</param>
            /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
            public LineUndoTask(ImageEditPanel.InternalPictureBox targetPictureBox, Color color, Point lineStart, Point lineEnd, CompositingMode compositingMode)
            {
                _targetPictureBox = targetPictureBox;
                _color = color;
                _lineStart = lineStart;
                _lineEnd = lineEnd;
                _bitmap = targetPictureBox.Bitmap;
                _compositingMode = compositingMode;

                Rectangle rec = GetRectangleAreaAbsolute(new [] { lineStart, lineEnd });

                rec.Offset(-1, -1);
                rec.Inflate(2, 2);

                _area = rec;

                // Take the image slide now
                _originalSlice = new Bitmap(_area.Width, _area.Height);
                
                Graphics g = Graphics.FromImage(_originalSlice);
                g.DrawImage(_bitmap, new Point(-_area.X, -_area.Y));
                g.Flush();
                g.Dispose();
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                _originalSlice.Dispose();
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                // Redraw the original slice back to the image
                Graphics g = Graphics.FromImage(_bitmap);
                g.SetClip(_area);
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;
                
                g.DrawImage(_originalSlice, _area);

                g.Flush();
                g.Dispose();

                // Invalidate the target box
                _targetPictureBox.Invalidate();
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                // Draw the rectangle again
                PerformLineOperation(_color, _lineStart, _lineEnd, _bitmap, _compositingMode);

                // Invalidate the target box
                _targetPictureBox.Invalidate();
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
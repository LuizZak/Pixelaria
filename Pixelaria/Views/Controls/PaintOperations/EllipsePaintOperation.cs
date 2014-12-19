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
    /// Implements an Ellipse paint operation
    /// </summary>
    public class EllipsePaintOperation : BaseShapeOperation, IPaintOperation, IColoredPaintOperation, ICompositingPaintOperation, IFillModePaintOperation
    {
        /// <summary>
        /// Initialies a new instance of the RectanglePaintOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public EllipsePaintOperation(Color firstColor, Color secondColor)
            : base(firstColor, secondColor)
        {
            
        }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.circle_cursor);
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
                pictureBox.OwningPanel.UndoSystem.RegisterUndo(new EllipseUndoTask(pictureBox, firstColor, secondColor, area, compositingMode, fillMode));

            PerformEllipseOperation(firstColor, secondColor, area, bitmap, compositingMode, fillMode);
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
            PerformElipseOperation(firstColor, secondColor, area, graphics, compositingMode, fillMode);
        }

        /// <summary>
        /// Performs the Ellipse paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the ellipse</param>
        /// <param name="secondColor">The second color to use when drawing the ellipse</param>
        /// <param name="area">The area of the ellipse to draw</param>
        /// <param name="bitmap">The Bitmap to draw the ellipse on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the ellipse</param>
        /// <param name="fillMode">The fill mode for this ellipse operation</param>
        public static void PerformEllipseOperation(Color firstColor, Color secondColor, Rectangle area, Bitmap bitmap, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            //FastBitmap fb = new FastBitmap(bitmap);
            //fb.Lock();

            /*int originx = area.X + area.Width / 2;
            int originy = area.Y;
            int width = area.Width;
            int height = area.Height;
            int hh = height * height;
            int ww = width * width;
            int hhww = hh * ww;
            int x0 = width;
            int dx = 0;

            // do the horizontal diameter
            for (int x = -width; x <= width; x++)
            {
                //setpixel(origin.x + x, origin.y);
                if (originx + x > 0 && originx + x < fb.Width && originy > 0 && originy < fb.Height)
                    fb.SetPixel(originx + x, originy, firstColor);
            }

            // now do both halves at the same time, away from the diameter
            for (int y = 1; y <= height; y++)
            {
                int x1 = x0 - (dx - 1);  // try slopes of dx - 1 or more
                for (; x1 > 0; x1--)
                    if (x1 * x1 * hh + y * y * ww <= hhww)
                        break;
                dx = x0 - x1;  // current approximation of the slope
                x0 = x1;

                for (int x = -x0; x <= x0; x++)
                {
                    if (originx + x > 0 && originx + x < fb.Width && originy - y > 0 && originy - y < fb.Height)
                    fb.SetPixel(originx + x, originy - y, firstColor);
                    if (originx + x > 0 && originx + x < fb.Width && originy + y > 0 && originy + y < fb.Height)
                    fb.SetPixel(originx + x, originy + y, firstColor);
                    //setpixel(origin.x + x, origin.y - y);
                    //setpixel(origin.x + x, origin.y + y);
                }
            }*/

            //fb.Unlock();

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CompositingMode = compositingMode;

            PerformElipseOperation(firstColor, secondColor, area, graphics, compositingMode, fillMode);

            graphics.Flush();
            graphics.Dispose();
        }

        /// <summary>
        /// Performs the Ellipse paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the ellipse</param>
        /// <param name="secondColor">The second color to use when drawing the ellipse</param>
        /// <param name="area">The area of the ellipse to draw</param>
        /// <param name="graphics">The Graphics to draw the ellipse on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the ellipse</param>
        /// <param name="fillMode">The fill mode for this ellipse operation</param>
        public static void PerformElipseOperation(Color firstColor, Color secondColor, Rectangle area, Graphics graphics, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            Brush brush = new SolidBrush((fillMode == OperationFillMode.SolidFillFirstColor ? firstColor : secondColor));

            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

            if (fillMode == OperationFillMode.SolidFillFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor || fillMode == OperationFillMode.SolidFillSecondColor)
            {
                graphics.FillEllipse(brush, area);
            }

            if (fillMode == OperationFillMode.OutlineFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor)
            {
                Pen pen = new Pen(firstColor);

                RectangleF nArea = area;

                graphics.DrawEllipse(pen, area);

                pen.Dispose();
            }

            brush.Dispose();
        }

        /// <summary>
        /// An ellipse undo task
        /// </summary>
        protected class EllipseUndoTask : IUndoTask
        {
            /// <summary>
            /// The target InternalPictureBox of this EllipseUndoTask
            /// </summary>
            ImageEditPanel.InternalPictureBox targetPictureBox;

            /// <summary>
            /// The area of the the image that was affected by the ellipse operation
            /// </summary>
            Rectangle area;

            /// <summary>
            /// The first color used to draw the ellipse
            /// </summary>
            Color firstColor;

            /// <summary>
            /// The second color used to draw the ellipse
            /// </summary>
            Color secondColor;

            /// <summary>
            /// The original slice of bitmap that represents the image region before the ellipse
            /// was drawn
            /// </summary>
            Bitmap originalSlice;

            /// <summary>
            /// The bitmap where the ellipse was drawn on
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
            /// Initializes a new instance of the EllipseUndoTask class
            /// </summary>
            /// <param name="targetPictureBox">The target InternalPictureBox of this EllipseUndoTask</param>
            /// <param name="firstColor">The first color to use when drawing the ellipse</param>
            /// <param name="secondColor">The second color to use when drawing the ellipse</param>
            /// <param name="area">The area of the ellipse to draw</param>
            /// <param name="compositingMode">The CompositingMode to use when drawing the ellipse</param>
            /// <param name="fillMode">The fill mode for this ellipse operation</param>
            public EllipseUndoTask(ImageEditPanel.InternalPictureBox targetPictureBox, Color firstColor, Color secondColor, Rectangle area, CompositingMode compositingMode, OperationFillMode fillMode)
            {
                this.targetPictureBox = targetPictureBox;
                this.firstColor = firstColor;
                this.secondColor = secondColor;
                this.area = area;
                this.bitmap = targetPictureBox.Bitmap;
                this.compositingMode = compositingMode;
                this.fillMode = fillMode;

                // Take the image slide now
                this.originalSlice = new Bitmap(area.Width + 1, area.Height + 1);
                
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
                g.SetClip(new Rectangle(area.X, area.Y, originalSlice.Width, originalSlice.Height));
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;

                g.DrawImage(originalSlice, new Rectangle(area.X, area.Y, originalSlice.Width, originalSlice.Height));

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
                // Draw the ellipse again
                EllipsePaintOperation.PerformEllipseOperation(firstColor, secondColor, area, bitmap, compositingMode, fillMode);

                // Invalidate the target box
                targetPictureBox.Invalidate();
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public string GetDescription()
            {
                return "Elipse";
            }
        }
    }
}
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Bucket paint operation
    /// </summary>
    public class BucketPaintOperation : BasePaintOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// The compositing mode for this paint operation
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
        public virtual Color FirstColor { get { return _firstColor; } set { _firstColor = value; } }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor { get { return _secondColor; } set { _secondColor = value; } }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get { return compositingMode; } set { compositingMode = value; } }

        /// <summary>
        /// Initialies a new instance of the BucketPaintOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public BucketPaintOperation(Color firstColor, Color secondColor)
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
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.bucket_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            OperationCursor.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            Point point = GetAbsolutePoint(e.Location);

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                Color color = e.Button == MouseButtons.Left ? _firstColor : _secondColor;

                if (WithinBounds(point))
                {
                    PerformBucketOperaiton(color, point, compositingMode);
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                _firstColor = pictureBox.Bitmap.GetPixel(point.X, point.Y);

                pictureBox.OwningPanel.FireColorChangeEvent(_firstColor);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            _mousePosition = e.Location;

            if (e.Button == MouseButtons.Middle)
            {
                Point mouse = GetAbsolutePoint(_mousePosition);
                Point mouseLast = GetAbsolutePoint(_lastMousePosition);

                if (mouse != mouseLast && WithinBounds(mouse))
                {
                    _firstColor = pictureBox.Bitmap.GetPixel(mouse.X, mouse.Y);

                    pictureBox.OwningPanel.FireColorChangeEvent(_firstColor);
                }
            }

            _lastMousePosition = _mousePosition;
        }

        /// <summary>
        /// Performs the bucket fill operation
        /// </summary>
        /// <param name="color">The color of the fill operation</param>
        /// <param name="point">The point to start the fill operation at</param>
        /// <param name="compMode">The CompositingMode of the bucket fill operation</param>
        protected void PerformBucketOperaiton(Color color, Point point, CompositingMode compMode)
        {
            // Start the fill operation by getting the color under the user's mouse
            Color pColor = pictureBox.Bitmap.GetPixel(point.X, point.Y);
            // Do a pre-blend of the color, if the composition mode is SourceOver
            Color newColor = (compMode == CompositingMode.SourceOver ? color.Blend(pColor) : color);

            uint pColorI = unchecked((uint)pColor.ToArgb());
            uint newColorI = unchecked((uint)newColor.ToArgb());

            // Don't do anything if the fill operation doesn't ends up changing any pixel color
            if (pColorI == newColorI || pColor == color && (compMode == CompositingMode.SourceOver && pColor.A == 255 || compMode == CompositingMode.SourceCopy))
            {
                return;
            }

            // Clone the bitmap to be used on undo/redo
            Bitmap originalBitmap = new Bitmap(pictureBox.Bitmap);

            int minX = point.X;
            int minY = point.Y;
            int maxX = point.X;
            int maxY = point.Y;

            // Lock the bitmap
            FastBitmap fastBitmap = new FastBitmap(pictureBox.Bitmap);
            fastBitmap.Lock();

            // Initialize the undo task
            BitmapUndoTask undoTask = new BitmapUndoTask(pictureBox, pictureBox.Bitmap, "Flood fill");

            Stack<int> stack = new Stack<int>();

            int width = fastBitmap.Width;
            int height = fastBitmap.Height;

            stack.Push(((point.X << 16) | point.Y));

            // Do a floodfill using a vertical scanline algorithm
            while(stack.Count > 0)
            {
                int v = stack.Pop();
                int x = (v >> 16);
                int y = (v & 0xFFFF);

                var y1 = y;

                while (y1 >= 0 && fastBitmap.GetPixelUInt(x, y1) == pColorI) y1--;

                y1++;
                bool spanLeft = false, spanRight = false;

                while (y1 < height && fastBitmap.GetPixelUInt(x, y1) == pColorI)
                {
                    // Expand affected region boundaries
                    minX = x < minX ? x : minX;
                    maxX = x > maxX ? x : maxX;

                    minY = y1 < minY ? y1 : minY;
                    maxY = y1 > maxY ? y1 : maxY;

                    fastBitmap.SetPixel(x, y1, newColorI);

                    uint pixel;

                    if (x > 0)
                    {
                        pixel = fastBitmap.GetPixelUInt(x - 1, y1);

                        if (!spanLeft && pixel == pColorI)
                        {
                            stack.Push((((x - 1) << 16) | y1));

                            spanLeft = true;
                        }
                        else if (spanLeft && pixel != pColorI)
                        {
                            spanLeft = false;
                        }
                    }

                    if (x < width - 1)
                    {
                        pixel = fastBitmap.GetPixelUInt(x + 1, y1);

                        if (!spanRight && pixel == pColorI)
                        {
                            stack.Push((((x + 1) << 16) | y1));
                            spanRight = true;
                        }
                        else if (spanRight && pixel != pColorI)
                        {
                            spanRight = false;
                        }
                    }
                    y1++;
                }
            }

            fastBitmap.Unlock();

            // Generate the undo now
            Rectangle affectedRectangle = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

            undoTask.DrawPoint = affectedRectangle.Location;
            // Slice and persist the undo/redo bitmap regions
            undoTask.SetOldBitmap(FastBitmap.SliceBitmap(originalBitmap, affectedRectangle));
            undoTask.SetNewBitmap(FastBitmap.SliceBitmap(pictureBox.Bitmap, affectedRectangle));

            originalBitmap.Dispose();

            pictureBox.OwningPanel.UndoSystem.RegisterUndo(undoTask);

            // Finish the operation by updating the picture box
            pictureBox.Invalidate();
            pictureBox.MarkModified();
        }
    }
}
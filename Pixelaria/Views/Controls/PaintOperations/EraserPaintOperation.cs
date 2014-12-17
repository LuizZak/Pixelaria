using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements an Eraser paint operation
    /// </summary>
    public class EraserPaintOperation : BasePencilPaintOperation, IColoredPaintOperation
    {
        /// <summary>
        /// Initializes a new instance of the PencilPaintOperation class, initializing the object
        /// with the two pencil colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public EraserPaintOperation(Color firstColor, Color secondColor, int pencilSize)
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
            size = 1;
        }

        /// <summary>
        /// Initializes this EraserPaintOperation
        /// </summary>
        /// <param name="pictureBox">The target picture box</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            undoDecription = "Eraser";

            CompositingMode = CompositingMode.SourceCopy;

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.eraser_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            FirstColor = Color.FromArgb(0, 0, 0, 0);
            SecondColor = Color.FromArgb(0, 0, 0, 0);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="pe">The event args for this event</param>
        public override void Paint(PaintEventArgs pe)
        {
            if (!visible)
                return;

            // Prepare the drawing operation to be performed
            Point absolutePencil = Point.Round(GetAbsolutePoint(pencilPoint));
            Bitmap pen = (penId == 0 ? firstPenBitmap : secondPenBitmap);

            if (!WithinBounds(absolutePencil))
                return;

            if (size > 1)
            {
                absolutePencil.Offset(-size / 2, -size / 2);
            }

            pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            Color baseColor = (penId == 0 ? firstColor : secondColor);
            Color newColor = pictureBox.Bitmap.GetPixel(absolutePencil.X, absolutePencil.Y);

            if (firstColor.A == 0)
            {
                newColor = Color.FromArgb(0, 0, 0, 0);
            }
            else
            {
                float newAlpha = (((float)newColor.A / 255) * (1 - (float)baseColor.A / 255));
                newColor = Color.FromArgb((int)(newAlpha * 255), newColor.R, newColor.G, newColor.B);
            }

            currentTraceBitmap.SetPixel(0, 0, newColor);

            PointF p = GetRelativePoint(absolutePencil);

            RectangleF pointRect = new RectangleF(p.X, p.Y, 1 * pictureBox.Zoom.X, 1 * pictureBox.Zoom.Y);

            // Draw the pointer
            Matrix currentTransform = pe.Graphics.Transform;
            Region reg = pe.Graphics.Clip;

            pe.Graphics.ResetTransform();
            pe.Graphics.SetClip(pointRect);

            pictureBox.PaintBackground(pe);

            pe.Graphics.Transform = currentTransform;
            pe.Graphics.Clip = reg;

            pe.Graphics.DrawImage(currentTraceBitmap, new Rectangle(absolutePencil.X, absolutePencil.Y, 1, 1), new Rectangle(0, 0, 1, 1), GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            PencilPoint = e.Location;

            lastMousePosition = e.Location;

            Point absolutePencil = Point.Round(GetAbsolutePoint(pencilPoint));
            if (size > 1)
            {
                absolutePencil.Offset(-size / 2, -size / 2);
            }
            // Early out
            if (!WithinBounds(absolutePencil))
            {
                return;
            }

            if (!mouseDown)
            {
                currentUndoTask = new PerPixelUndoTask(pictureBox, undoDecription, true, true);

                // Mouse down
                if (e.Button == MouseButtons.Left)
                {
                    mouseDown = true;

                    penId = 0;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    mouseDown = true;

                    penId = 1;
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    firstColor = pictureBox.Bitmap.GetPixel(absolutePencil.X, absolutePencil.Y);
                    RegeneratePenBitmap();

                    pictureBox.OwningPanel.FireColorChangeEvent(firstColor);

                    pictureBox.Invalidate();
                }

                // Mouse handling
                Bitmap penBitmap = (penId == 0 ? firstPenBitmap : secondPenBitmap);

                // Start drawing the pixels
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    Color baseColor = (penId == 0 ? firstColor : secondColor);
                    Color newColor = pictureBox.Bitmap.GetPixel(absolutePencil.X, absolutePencil.Y);

                    if (baseColor.A == 0)
                    {
                        newColor = Color.FromArgb(0, 0, 0, 0);
                    }
                    else
                    {
                        float newAlpha = (((float)newColor.A / 255) * (1 - (float)baseColor.A / 255));
                        newColor = Color.FromArgb((int)(newAlpha * 255), newColor.R, newColor.G, newColor.B);
                    }

                    // Replace blend mode
                    if (CompositingMode == CompositingMode.SourceCopy)
                    {
                        Color oldColor = pictureBox.Bitmap.GetPixel(absolutePencil.X, absolutePencil.Y);

                        ((Bitmap)pictureBox.Image).SetPixel(absolutePencil.X, absolutePencil.Y, newColor);

                        pictureBox.MarkModified();

                        // Register pixel on undo operation
                        currentUndoTask.RegisterPixel(absolutePencil.X, absolutePencil.Y, oldColor, newColor);
                    }
                }
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            PencilPoint = e.Location;

            if (mouseDown)
            {
                Bitmap image = (CompositingMode == CompositingMode.SourceCopy ? pictureBox.Bitmap : currentTraceBitmap);
                Bitmap pen = (penId == 0 ? firstPenBitmap : secondPenBitmap);
                Color penColor = (penId == 0 ? firstColor : secondColor);

                Point pencil = GetAbsolutePoint(pencilPoint);
                Point pencilLast = GetAbsolutePoint(lastMousePosition);

                if (size > 1)
                {
                    pencil.Offset(-size / 2, -size / 2);
                    pencilLast.Offset(-size / 2, -size / 2);
                }

                if (pencil != pencilLast)
                {
                    int x0 = pencilLast.X;
                    int y0 = pencilLast.Y;
                    int x1 = pencil.X;
                    int y1 = pencil.Y;

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

                        if (p.X >= 0 && p.X < image.Width && p.Y >= 0 && p.Y < image.Height)
                        {
                            Color oldColor = pictureBox.Bitmap.GetPixel(p.X, p.Y);
                            Color newColor = oldColor;

                            if (penColor.A == 0)
                            {
                                newColor = Color.FromArgb(0, 0, 0, 0);
                            }
                            else
                            {
                                float newAlpha = (((float)newColor.A / 255) * (1 - (float)penColor.A / 255));
                                newColor = Color.FromArgb((int)(newAlpha * 255), newColor.R, newColor.G, newColor.B);
                            }

                            image.SetPixel(p.X, p.Y, newColor);

                            currentUndoTask.RegisterPixel(p.X, p.Y, oldColor, newColor, false);

                            InvalidateRect(GetRelativePoint(p), pen.Width, pen.Height);
                        }

                        error = error - deltay;
                        if (error < 0)
                        {
                            y = y + ystep;
                            error = error + deltax;
                        }
                    }

                    pictureBox.MarkModified();
                }
            }

            lastMousePosition = e.Location;
        }
    }
}
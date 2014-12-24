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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Pixelaria.Algorithms.PaintOperations;
using Pixelaria.Algorithms.PaintOperations.UndoTasks;

namespace Pixelaria.Views.Controls.PaintTools.Abstracts
{
    /// <summary>
    /// Base class for pencil-like paint tools
    /// </summary>
    public abstract class BasePencilPaintTool : BasePaintTool
    {
        /// <summary>
        /// Whether the pencil is visible
        /// </summary>
        protected bool visible;

        /// <summary>
        /// Whether the mouse is being held down on the form
        /// </summary>
        protected bool mouseDown;

        /// <summary>
        /// The pen which the user is currently drawing with
        /// </summary>
        protected int penId;

        /// <summary>
        /// The last position of the mouse in relatice control coordinates
        /// </summary>
        protected Point lastMousePosition;

        /// <summary>
        /// The current pencil point in relative control coordinates
        /// </summary>
        protected Point pencilPoint;

        /// <summary>
        /// The first color currently being used to paint on the InternalPictureBox
        /// </summary>
        protected Color firstColor = Color.Black;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        protected Color secondColor = Color.Black;

        /// <summary>
        /// Graphics used to draw on the bitmap
        /// </summary>
        protected Graphics graphics;

        /// <summary>
        /// The Bitmap to use as pen with the first color
        /// </summary>
        protected Bitmap firstPenBitmap;

        /// <summary>
        /// The Bitmap to use as pen with the second color
        /// </summary>
        protected Bitmap secondPenBitmap;

        /// <summary>
        /// The bitmap used to buffer the current pencil tool so the alpha channel is constant through the operation
        /// </summary>
        protected Bitmap currentTraceBitmap;

        /// <summary>
        /// The undo task for the current pencil operation being performed
        /// </summary>
        protected PerPixelUndoTask currentUndoTask;

        /// <summary>
        /// The radius of this pencil
        /// </summary>
        protected int size;

        /// <summary>
        /// The string to use as a description for the undo operation
        /// </summary>
        protected string undoDecription;

        /// <summary>
        /// The underlying paint operation that is affecting the bitmap
        /// </summary>
        protected PencilPaintOperation pencilOperation;

        /// <summary>
        /// The undo generator that is used during the plotting of the bitmap
        /// </summary>
        protected PlottingPaintUndoGenerator undoGenerator;

        /// <summary>
        /// The position to place the pencil point in absolute control coordinates
        /// </summary>
        public virtual Point PencilPoint
        {
            get { return pencilPoint; }
            set
            {
                InvalidatePen();
                pencilPoint = value;
                InvalidatePen();
            }
        }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color FirstColor
        {
            get { return firstColor; }
            set
            {
                firstColor = value;

                if (!Loaded)
                    return;

                RegeneratePenBitmap();

                if (visible)
                {
                    PointF p = GetRelativePoint(GetAbsolutePoint(pencilPoint));

                    Rectangle rec = new Rectangle((int)p.X, (int)p.Y, (int)(firstPenBitmap.Width * pictureBox.Zoom.Y), (int)(firstPenBitmap.Width * pictureBox.Zoom.Y));

                    pictureBox.Invalidate(rec);
                }
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

                if (!Loaded)
                    return;

                RegeneratePenBitmap();
            }
        }

        /// <summary>
        /// Gets or sets the pencil radius
        /// </summary>
        [DefaultValue(1)]
        [Browsable(false)]
        public virtual int Size { get { return size; } set { size = Math.Max(1, value); RegeneratePenBitmap(); } }

        /// <summary>
        /// Gets or sets the compositing mode for the pen
        /// </summary>
        public CompositingMode CompositingMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the BasePencilPaintTool class
        /// </summary>
        protected BasePencilPaintTool()
        {
            size = 1;
        }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint tool on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            pictureBox = targetPictureBox;
            lastMousePosition = new Point();

            RegeneratePenBitmap();

            ChangeBitmap(targetPictureBox.Bitmap);

            currentTraceBitmap = new Bitmap(targetPictureBox.Bitmap.Width, targetPictureBox.Bitmap.Height);

            CompositingMode = targetPictureBox.OwningPanel.DefaultCompositingMode;

            visible = true;

            pencilOperation = new PencilPaintOperation(targetPictureBox.Bitmap);

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Tool
        /// </summary>
        public override void Destroy()
        {
            if (!Loaded)
                return;

            FinishOperation();

            InvalidatePen();

            pictureBox = null;

            graphics.Dispose();
            firstPenBitmap.Dispose();
            secondPenBitmap.Dispose();

            ToolCursor.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public override void ChangeBitmap(Bitmap newBitmap)
        {
            if (mouseDown)
            {
                FinishOperation();
                mouseDown = false;
            }

            if (graphics != null)
                graphics.Dispose();

            graphics = newBitmap != null ? Graphics.FromImage(newBitmap) : null;
        }

        /// <summary>
        /// Called to notify this PaintTool that the control is being redrawn
        /// </summary>
        /// <param name="pe">The event args for this event</param>
        public override void Paint(PaintEventArgs pe)
        {
            if (!visible)
                return;

            // Draw the pencil position
            Point absolutePencil = Point.Round(GetAbsolutePoint(pencilPoint));
            Bitmap pen = (penId == 0 ? firstPenBitmap : secondPenBitmap);

            if (size > 1)
            {
                absolutePencil.Offset(-size / 2, -size / 2);
            }

            pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            if (CompositingMode == CompositingMode.SourceOver && !mouseDown)
            {
                // Create a color matrix object
                ColorMatrix matrix = new ColorMatrix
                {
                    Matrix33 = ((float)(penId == 0 ? firstColor : secondColor).A / 255)
                };

                // Create image attributes
                ImageAttributes attributes = new ImageAttributes();
                
                // Set the color(opacity) of the image
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                Graphics gfx = Graphics.FromImage(pictureBox.Buffer);

                gfx.DrawImage(pen, new Rectangle(absolutePencil, new Size(pen.Width, pen.Height)), 0, 0, pen.Width, pen.Height, GraphicsUnit.Pixel, attributes);

                gfx.Flush();
                gfx.Dispose();
            }

            // When rendering in Source Copy mode, draw the pencil color directly over the buffer
            if ((!mouseDown || CompositingMode != CompositingMode.SourceOver) && CompositingMode == CompositingMode.SourceCopy)
            {
                if (WithinBounds(absolutePencil))
                {
                    pictureBox.Buffer.SetPixel(absolutePencil.X, absolutePencil.Y, firstColor);
                }
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            PencilPoint = e.Location;

            lastMousePosition = e.Location;

            Point absolutePencil = Point.Round(GetAbsolutePoint(pencilPoint));

            // Mouse down
            if (e.Button == MouseButtons.Left)
            {
                penId = 0;

                StartOperation(absolutePencil);
            }
            else if (e.Button == MouseButtons.Right)
            {
                penId = 1;

                StartOperation(absolutePencil);
            }
            // Color pick
            else if (e.Button == MouseButtons.Middle)
            {
                mouseDown = true;

                // Set pen to first color because color picking always changes the first color
                penId = 0;

                firstColor = pictureBox.Bitmap.GetPixel(absolutePencil.X, absolutePencil.Y);
                RegeneratePenBitmap();

                pictureBox.OwningPanel.FireColorChangeEvent(firstColor);

                pictureBox.Invalidate();
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            PencilPoint = e.Location;

            if (mouseDown)
            {
                Point pencil = GetAbsolutePoint(pencilPoint);
                Point pencilLast = GetAbsolutePoint(lastMousePosition);

                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    DrawPencil(pencil, null);
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    if (pencil != pencilLast && WithinBounds(pencil))
                    {
                        firstColor = pictureBox.Bitmap.GetPixel(pencil.X, pencil.Y);
                        RegeneratePenBitmap();

                        pictureBox.OwningPanel.FireColorChangeEvent(firstColor);

                        pictureBox.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Middle)
            {
                FinishOperation();
            }

            mouseDown = false;
        }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseLeave(EventArgs e)
        {
            visible = false;
            InvalidatePen();
        }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseEnter(EventArgs e)
        {
            visible = true;
        }

        /// <summary>
        /// Starts this pencil operation on a specified point
        /// </summary>
        /// <param name="point">The point to start this operation on</param>
        private void StartOperation(Point point)
        {
            // Early out
            if (!WithinBounds(point))
            {
                return;
            }

            mouseDown = true;

            lastMousePosition = point;

            Color penColor = (penId == 0 ? firstColor : secondColor);
            pencilOperation.Color = penColor;
            pencilOperation.CompositingMode = CompositingMode;

            pencilOperation.StartOpertaion(false);

            undoGenerator = new PlottingPaintUndoGenerator(pictureBox.Bitmap, "Pencil");
            pencilOperation.Notifier = undoGenerator;

            pencilOperation.MoveTo(point.X, point.Y);
            pencilOperation.DrawTo(point.X, point.Y);
        }

        /// <summary>
        /// Finishes this BasePenTool's current drawing operation
        /// </summary>
        public virtual void FinishOperation()
        {
            if (!mouseDown)
                return;

            pencilOperation.FinishOperation();
            pictureBox.OwningPanel.UndoSystem.RegisterUndo(undoGenerator.UndoTask);

            pictureBox.MarkModified();
        }

        /// <summary>
        /// Draws the pencil with the current properties on the given bitmap object
        /// </summary>
        /// <param name="p">The point to draw the pencil to</param>
        /// <param name="bitmap">The bitmap to draw the pencil on</param>
        protected virtual void DrawPencil(Point p, Bitmap bitmap)
        {
            // Find the properties to draw the pen with
            Color penColor = (penId == 0 ? firstColor : secondColor);
            
            pencilOperation.Color = penColor;
            pencilOperation.DrawTo(p.X, p.Y);

            // Calculate the invalidation area
            Rectangle rectangle = new Rectangle(Math.Min(p.X, lastMousePosition.X), Math.Min(p.Y, lastMousePosition.Y), Math.Max(p.X, lastMousePosition.X), Math.Max(p.Y, lastMousePosition.Y));

            rectangle.Width -= rectangle.X - 2;
            rectangle.Height -= rectangle.Y - 2;
            rectangle.X -= 1;
            rectangle.Y -= 1;

            InvalidateRect(rectangle);

            lastMousePosition = p;
        }

        /// <summary>
        /// Generates the pen bitmap again
        /// </summary>
        protected virtual void RegeneratePenBitmap()
        {
            if (firstPenBitmap != null)
            {
                firstPenBitmap.Dispose();
            }
            if (secondPenBitmap != null)
            {
                secondPenBitmap.Dispose();
            }

            firstPenBitmap = new Bitmap(size + 1, size + 1, PixelFormat.Format32bppArgb);

            if (size == 1)
            {
                firstPenBitmap.SetPixel(0, 0, Color.FromArgb(255, firstColor.R, firstColor.G, firstColor.B));
            }
            else
            {
                Graphics g = Graphics.FromImage(firstPenBitmap);
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Brush b = new SolidBrush(Color.FromArgb(255, firstColor.R, firstColor.G, firstColor.B));
                g.FillEllipse(b, 0, 0, size, size);
            }

            secondPenBitmap = new Bitmap(size + 1, size + 1, PixelFormat.Format32bppArgb);

            if (size == 1)
            {
                secondPenBitmap.SetPixel(0, 0, Color.FromArgb(255, secondColor.R, secondColor.G, secondColor.B));
            }
            else
            {
                Graphics g = Graphics.FromImage(secondPenBitmap);
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Brush b = new SolidBrush(Color.FromArgb(255, secondColor.R, secondColor.G, secondColor.B));
                g.FillEllipse(b, 0, 0, size, size);
            }
        }

        /// <summary>
        /// Invalidates the region on the InternalPictureBox that represents the pen
        /// </summary>
        protected virtual void InvalidatePen()
        {
            if (firstPenBitmap != null)
            {
                Point invPoint = pencilPoint;

                if (size > 1)
                {
                    invPoint.Offset((int)(-size * pictureBox.Zoom.X / 2 - 1), (int)(-size * pictureBox.Zoom.Y / 2 - 1));
                }

                invPoint.X -= (int)(pictureBox.Zoom.X);
                invPoint.Y -= (int)(pictureBox.Zoom.Y);

                InvalidateRect(invPoint, firstPenBitmap.Width + 2, firstPenBitmap.Height + 2);
            }
        }
    }
}
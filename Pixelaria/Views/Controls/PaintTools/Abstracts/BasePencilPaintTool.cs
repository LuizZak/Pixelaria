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
        /// Whether the alpha should be accumulated in this operation
        /// </summary>
        protected bool accumulateAlpha;

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
        /// The compositing mode for the pen
        /// </summary>
        protected CompositingMode compositingMode;

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

                UpdatePen();

                if (visible && penId == 0)
                {
                    InvalidatePen();
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

                UpdatePen();

                if (visible && penId == 1)
                {
                    InvalidatePen();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pencil radius
        /// </summary>
        [DefaultValue(1)]
        [Browsable(false)]
        public virtual int Size { get { return size; } set { size = Math.Max(1, value); UpdatePen(); } }

        /// <summary>
        /// Gets or sets the compositing mode for the pen
        /// </summary>
        public CompositingMode CompositingMode
        {
            get { return compositingMode; }
            set
            {
                compositingMode = value;

                if (!Loaded)
                    return;

                pencilOperation.CompositingMode = value;
            }
        }

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

            pencilOperation = new PencilPaintOperation(targetPictureBox.Bitmap);

            UpdatePen();

            ChangeBitmap(targetPictureBox.Bitmap);

            currentTraceBitmap = new Bitmap(targetPictureBox.Bitmap.Width, targetPictureBox.Bitmap.Height);

            CompositingMode = targetPictureBox.OwningPanel.DefaultCompositingMode;

            visible = true;

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
            Point absolutePencil = GetAbsolutePoint(pencilPoint);

            // Draw the pencil on the spot under the user's mouse using the pencil paint operation
            DrawPencilPreview(pictureBox.Buffer, absolutePencil);
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
                UpdatePen();

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
                        UpdatePen();

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
            mouseDown = true;

            lastMousePosition = point;

            Color penColor = (penId == 0 ? firstColor : secondColor);
            pencilOperation.Color = penColor;
            pencilOperation.CompositingMode = CompositingMode;

            pencilOperation.StartOpertaion(accumulateAlpha);

            undoGenerator = new PlottingPaintUndoGenerator(pictureBox.Bitmap, undoDecription);
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
            pencilOperation.Notifier = null; // Nullify the notifier so subsequent operations don't interfere with previous undo operations
            
            // Verify that the generator has registered any modifications
            if(undoGenerator.UndoTask.PixelHistoryTracker.PixelList.Count > 0)
            {
                pictureBox.OwningPanel.UndoSystem.RegisterUndo(undoGenerator.UndoTask);
                pictureBox.MarkModified();
            }
        }

        /// <summary>
        /// Updates the pen configuration
        /// </summary>
        protected virtual void UpdatePen()
        {
            pencilOperation.Color = penId == 0 ? firstColor : secondColor;
            pencilOperation.CompositingMode = compositingMode;
            pencilOperation.Size = size;
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
            Rectangle lastRect = new Rectangle(Point.Subtract(lastMousePosition, new Size(size / 2, size / 2)), new Size(size * 2, size * 2));
            Rectangle curRect  = new Rectangle(Point.Subtract(p, new Size(size / 2, size / 2)), new Size(size * 2, size * 2));

            Rectangle invalidateRect = Rectangle.Union(lastRect, curRect);

            InvalidateRect(invalidateRect);

            lastMousePosition = p;
        }

        /// <summary>
        /// Draws the pencil preview on a specified bitmap at the specified point.
        /// If the current pencil operation is currently started, no preview is drawn
        /// </summary>
        /// <param name="bitmap">The bitmap to draw the pencil preview on</param>
        /// <param name="point">The point on the bitmap draw the pencil preview on</param>
        protected virtual void DrawPencilPreview(Bitmap bitmap, Point point)
        {
            if (!pencilOperation.OperationStarted)
            {
                pencilOperation.TargetBitmap = bitmap;

                pencilOperation.StartOpertaion(accumulateAlpha);

                pencilOperation.MoveTo(point.X, point.Y);
                pencilOperation.DrawTo(point.X, point.Y);

                pencilOperation.FinishOperation();

                pencilOperation.TargetBitmap = pictureBox.Bitmap;
            }
        }

        /// <summary>
        /// Invalidates the region on the InternalPictureBox that represents the pen
        /// </summary>
        protected virtual Rectangle InvalidatePen()
        {
            PointF invPoint = pencilPoint;

            invPoint.X -= (size * pictureBox.Zoom.X) / 2;
            invPoint.Y -= (size * pictureBox.Zoom.X) / 2;

            return InvalidateRect(invPoint, (size * pictureBox.Zoom.X) * 1.5f, (size * pictureBox.Zoom.X) * 1.5f);
        }
    }
}
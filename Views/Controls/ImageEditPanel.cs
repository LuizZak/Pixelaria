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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using Pixelaria.Data.Undo;
using Pixelaria.Utils;
using Pixelaria.Views.ModelViews;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Defines a panel that is used specifically for displaying a Bitmap object and allowing
    /// the user to make changes to it
    /// </summary>
    public class ImageEditPanel : Control, Modifiable
    {
        /// <summary>
        /// The PictureBox that displays the bitmap being edited
        /// </summary>
        private InternalPictureBox internalPictureBox;

        /// <summary>
        /// The Panel that will own the InternalPictureBox
        /// </summary>
        private Panel owningPanel;

        /// <summary>
        /// The undo system that handles undoing/redoing of tasks
        /// </summary>
        private UndoSystem undoSystem;

        /// <summary>
        /// The default compositing mode to use on paint operations that have a compositing operation
        /// </summary>
        private CompositingMode defaultCompositingMode;

        /// <summary>
        /// The default fill mode to use on paint operations that have a fill mode
        /// </summary>
        private OperationFillMode defaultFillMode;

        /// <summary>
        /// Gets or sets this panel's picture box's background image
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        public Image PictureBoxBackgroundImage { get { return internalPictureBox.BackgroundImage; } set { this.internalPictureBox.BackgroundImage = value; } }

        /// <summary>
        /// Delegate for a ColorSelect event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The arguments for the event</param>
        public delegate void ColorPickEventHandler(object sender, ColorPickEventArgs eventArgs);

        /// <summary>
        /// Occurs whenever the current paint operation selects a color
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the current paint operation selects a color.")]
        public event ColorPickEventHandler ColorSelect;

        /// <summary>
        /// Delegate for a ClipboardStateChanged event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The arguments for the event</param>
        public delegate void ClipboardStateEventHandler(object sender, ClipboardStateEventArgs eventArgs);

        /// <summary>
        /// Occurs whenever the state of the clipboard capabilities of this object changes
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the state of the clipboard capabilities of this object changes.")]
        public event ClipboardStateEventHandler ClipboardStateChanged;

        /// <summary>
        /// Delegate for a OperationStatusChanged event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The arguments for the event</param>
        public delegate void OperationStatusEventHandler(object sender, OperationStatusEventArgs eventArgs);

        /// <summary>
        /// Occurs whenever the current operation notified a status change
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the current operation notified a status change.")]
        public event OperationStatusEventHandler OperationStatusChanged;

        /// <summary>
        /// Gets the internal picture box currently loaded into this ImageEditPanel
        /// </summary>
        public InternalPictureBox PictureBox { get { return internalPictureBox; } }

        /// <summary>
        /// Gets or sets the modifiable to notify when changes are made to the bitmap
        /// </summary>
        public Modifiable NotifyTo { get; set; }

        /// <summary>
        /// Gets or sets the current paint operation to perform on this ImageEditPanel
        /// </summary>
        [Browsable(false)]
        public IPaintOperation CurrentPaintOperation
        {
            get { return internalPictureBox.CurrentPaintOperation; }
            set
            {
                if (IsDisposed) 
                    return;
                
                internalPictureBox.CurrentPaintOperation = value;

                if (value is IClipboardPaintOperation)
                {
                    FireClipboardStateEvent((value as IClipboardPaintOperation).CanCopy(), (value as IClipboardPaintOperation).CanCut(), (value as IClipboardPaintOperation).CanPaste());
                }
                else
                {
                    FireClipboardStateEvent(false, false, false);
                }
            }
        }

        /// <summary>
        /// Gets the undo system that handles the undo/redo tasks of this ImageEditPanel
        /// </summary>
        public UndoSystem UndoSystem { get { return undoSystem; } }

        /// <summary>
        /// Gets or sets the default compositing mode to use on paint operations that have a compositing component
        /// </summary>
        public CompositingMode DefaultCompositingMode
        {
            get { return defaultCompositingMode; }
            set
            {
                defaultCompositingMode = value;

                if (internalPictureBox.CurrentPaintOperation is ICompositingPaintOperation)
                {
                    (internalPictureBox.CurrentPaintOperation as ICompositingPaintOperation).CompositingMode = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default fill mode to use on paint operations that have a fill mode component
        /// </summary>
        public OperationFillMode DefaultFillMode
        {
            get { return defaultFillMode; }
            set
            {
                defaultFillMode = value;
                if (internalPictureBox.CurrentPaintOperation is IFillModePaintOperation)
                {
                    (internalPictureBox.CurrentPaintOperation as IFillModePaintOperation).FillMode = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ImageEditPanel
        /// </summary>
        public ImageEditPanel()
        {
            // Create the controls
            this.SuspendLayout();

            this.internalPictureBox = new InternalPictureBox(this);
            this.internalPictureBox.MinimumZoom = new PointF(0.25f, 0.25f);
            this.internalPictureBox.MaximumZoom = new PointF(160, 160);
            this.internalPictureBox.ShowImageArea = false;
            this.internalPictureBox.ClipBackgroundToImage = true;
            this.internalPictureBox.AllowDrag = false;
            this.internalPictureBox.notifyTo = this;
            this.internalPictureBox.Dock = DockStyle.Fill;

            this.owningPanel = new Panel();
            this.owningPanel.Dock = DockStyle.Fill;
            this.owningPanel.AutoScroll = true;
            this.owningPanel.BorderStyle = BorderStyle.FixedSingle;

            // Add controls
            this.owningPanel.Controls.Add(this.internalPictureBox);
            this.Controls.Add(this.owningPanel);

            this.ResumeLayout(true);

            this.undoSystem = new UndoSystem();

            this.defaultCompositingMode = CompositingMode.SourceOver;
            this.defaultFillMode = OperationFillMode.SolidFillFirstColor;
        }

        /// <summary>
        /// Initializes this ImageEditPanel instance
        /// </summary>
        public void Init()
        {
            internalPictureBox.HookToForm(this.FindForm());
        }

        /// <summary>
        /// Disposes of this ImageEditPanel and all resources used by it
        /// </summary>
        public new void Dispose()
        {
            this.internalPictureBox.Dispose();

            base.Dispose();
        }

        /// <summary>
        /// Loads the given Bitmap to this ImageEditPanel
        /// </summary>
        /// <param name="bitmap">The Bitmap to edit</param>
        public void LoadBitmap(Bitmap bitmap)
        {
            // Clear the undo system
            this.undoSystem.Clear();

            this.internalPictureBox.SetBitmap(bitmap);
            this.internalPictureBox.Width = owningPanel.Width;
            this.internalPictureBox.Height = owningPanel.Height;
        }

        /// <summary>
        /// Marks this ImageEditPanel as modified
        /// </summary>
        public void MarkModified()
        {
            if (NotifyTo != null)
            {
                NotifyTo.MarkModified();
            }
        }

        /// <summary>
        /// Fires the color change event with the given color
        /// </summary>
        /// <param name="color">The color to fire the event with</param>
        public void FireColorChangeEvent(Color color, ColorPickerColor colorIndex = ColorPickerColor.CurrentColor)
        {
            if(ColorSelect != null)
            {
                ColorSelect.Invoke(this, new ColorPickEventArgs(color, color, colorIndex));
            }
        }

        /// <summary>
        /// Fires the ClipboardStateChanged event with the given parameters
        /// </summary>
        /// <param name="canCopy">Whether there's content to be copied on the object</param>
        /// <param name="canCut">Whether there's content to be cut on the object</param>
        /// <param name="canPaste">Whether there's content to be pasted on the object</param>
        public void FireClipboardStateEvent(bool canCopy, bool canCut, bool canPaste)
        {
            if (ClipboardStateChanged != null)
            {
                ClipboardStateChanged.Invoke(this, new ClipboardStateEventArgs(canCopy, canCut, canPaste));
            }
        }

        /// <summary>
        /// Fires the OperationStatusChanged event with the given parameters
        /// </summary>
        /// <param name="operation">The operation that fired the event</param>
        /// <param name="status">The status for the event</param>
        public void FireOperationStatusEvent(IPaintOperation operation, string status)
        {
            if (OperationStatusChanged != null)
            {
                OperationStatusChanged.Invoke(this, new OperationStatusEventArgs(operation, status));
            }
        }

        /// <summary>
        /// Internal picture box that actually displays the bitmap to edit
        /// </summary>
        public class InternalPictureBox : ZoomablePictureBox, IDisposable
        {
            /// <summary>
            /// The ImageEditPanel that owns this InternalPictureBox
            /// </summary>
            private ImageEditPanel owningPanel;

            /// <summary>
            /// The current paint operation
            /// </summary>
            private IPaintOperation currentPaintOperation;

            /// <summary>
            /// The buffer bitmap that the paint operations will use in order to buffer screen previews
            /// </summary>
            private Bitmap buffer;

            /// <summary>
            /// The image to display under the current image
            /// </summary>
            private Image underImage;

            /// <summary>
            /// The image to display over the current image
            /// </summary>
            private Image overImage;

            /// <summary>
            /// Whether to display the current image
            /// </summary>
            private bool displayImage;

            /// <summary>
            /// The coordinate of the mouse, in absolute image pixels
            /// </summary>
            private Point mousePoint;

            /// <summary>
            /// Whether the mouse is currently over the image on the panel
            /// </summary>
            private bool mouseOverImage;

            /// <summary>
            /// Whether to display a grid over the image
            /// </summary>
            private bool displayGrid;

            /// <summary>
            /// Specifies the modifiable to notify when changes are made to the bitmap
            /// </summary>
            public Modifiable notifyTo;

            /// <summary>
            /// Gets or sets the current paint operation for this InternalPictureBox
            /// </summary>
            public IPaintOperation CurrentPaintOperation { get { return currentPaintOperation; } set { if (IsDisposed) return; SetPaintOperation(value); } }

            /// <summary>
            /// Gets the ImageEditPanel that owns this InternalPictureBox
            /// </summary>
            public ImageEditPanel OwningPanel { get { return owningPanel; } }

            /// <summary>
            /// Gets the Bitmap associated with this InternalPictureBox
            /// </summary>
            public Bitmap Bitmap { get { return (Image == null ? null : Image as Bitmap); } }

            /// <summary>
            /// Gets the buffer bitmap that the paint operations will use in order to buffer screen previews
            /// </summary>
            public Bitmap Buffer { get { return buffer; } }

            /// <summary>
            /// Gets or sets the image to display under the current image
            /// </summary>
            public Image UnderImage { get { return underImage; } set { underImage = value; Invalidate(); } }

            /// <summary>
            /// Gets or sets the image to display over the current image
            /// </summary>
            public Image OverImage { get { return overImage; } set { overImage = value; Invalidate(); } }

            /// <summary>
            /// Gets or sets whether to display the current image
            /// </summary>
            public bool DisplayImage { get { return displayImage; } set { if (displayImage != value) { displayImage = value; Invalidate(); } } }

            /// <summary>
            /// Gets the coordinate of the mouse, in absolute image pixels 
            /// </summary>
            public Point MousePoint { get { return mousePoint; } }

            /// <summary>
            /// Gets whether the mouse is currently over the image on the panel
            /// </summary>
            public bool MouseOverImage { get { return mouseOverImage; } }

            /// <summary>
            /// Gets or sets whether to display a grid over the image
            /// </summary>
            public bool DisplayGrid { get { return displayGrid; } set { this.displayGrid = value; Invalidate(); } }

            /// <summary>
            /// Initializes a new instance of the InternalPictureBox class
            /// </summary>
            /// <param name="owningPanel">The ImageEditPanel that will own this InternalPictureBox</param>
            public InternalPictureBox(ImageEditPanel owningPanel)
            {
                this.owningPanel = owningPanel;

                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                this.ZoomFactor = 2;
                this.displayImage = true;
                this.displayGrid = false;
                this.mousePoint = new Point();
                this.mouseOverImage = false;

                SetPaintOperation(new PencilPaintOperation());
            }

            /// <summary>
            /// Disposes of this InternalPictureBox and all used resources
            /// </summary>
            public new void Dispose()
            {
                if (currentPaintOperation != null)
                {
                    currentPaintOperation.Destroy();
                }

                buffer.Dispose();

                base.Dispose();
            }

            /// <summary>
            /// Sets the bitmap being edited
            /// </summary>
            /// <param name="bitmap">The bitmap to edit</param>
            public void SetBitmap(Bitmap bitmap)
            {
                this.Image = bitmap;

                if (buffer != null)
                    buffer.Dispose();                

                buffer = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

                if (currentPaintOperation != null)
                {
                    // Initialize the paint operation
                    if (!currentPaintOperation.Loaded)
                    {
                        currentPaintOperation.Initialize(this);

                        this.Cursor = currentPaintOperation.OperationCursor;
                    }

                    currentPaintOperation.ChangeBitmap(bitmap);
                }
            }

            /// <summary>
            /// Sets the current paint operation of this InternalPictureBox to be of the given type
            /// </summary>
            /// <param name="newPaintOperation"></param>
            public void SetPaintOperation(IPaintOperation newPaintOperation)
            {
                if (currentPaintOperation != null)
                {
                    owningPanel.FireOperationStatusEvent(currentPaintOperation, "");

                    currentPaintOperation.Destroy();
                }

                currentPaintOperation = newPaintOperation;

                if (Image != null)
                {
                    currentPaintOperation.Initialize(this);

                    if (!mouseOverImage)
                    {
                        currentPaintOperation.MouseLeave(new EventArgs());
                    }

                    this.Cursor = currentPaintOperation.OperationCursor;
                }

                if (currentPaintOperation is ICompositingPaintOperation)
                {
                    (currentPaintOperation as ICompositingPaintOperation).CompositingMode = owningPanel.defaultCompositingMode;
                }
                if (currentPaintOperation is IFillModePaintOperation)
                {
                    (currentPaintOperation as IFillModePaintOperation).FillMode = owningPanel.defaultFillMode;
                }
            }

            /// <summary>
            /// Marks this InternalPictureBox as modified
            /// </summary>
            public void MarkModified()
            {
                if (notifyTo != null)
                {
                    notifyTo.MarkModified();
                }
            }

            /// <summary>
            /// Paints the background of this PictureBox using the given PaintEventArgs
            /// </summary>
            /// <param name="pe">The PaintEventArgs to use on the paint background event</param>
            public void PaintBackground(PaintEventArgs pe)
            {
                OnPaintBackground(pe);
            }

            // 
            // OnPaint event handler
            // 
            protected override void OnPaint(PaintEventArgs pe)
            {
                // Draw the under image
                if (underImage != null)
                {
                    pe.Graphics.TranslateTransform(-offsetPoint.X, -offsetPoint.Y);
                    pe.Graphics.ScaleTransform(scale.X, scale.Y);

                    pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    pe.Graphics.InterpolationMode = ImageInterpolationMode;

                    pe.Graphics.DrawImage(underImage, new Point());

                    pe.Graphics.ResetTransform();
                }

                if (this.Image != null)
                {
                    UpdateGraphicsTransform(pe.Graphics);

                    // Clear the buffer
                    FastBitmap.CopyPixels(Bitmap, buffer);

                    // Clip to the image's boundaries
                    pe.Graphics.IntersectClip(new RectangleF(0, 0, Image.Width, Image.Height));

                    Region clip = pe.Graphics.Clip;

                    currentPaintOperation.Paint(pe);

                    if (displayImage)
                    {
                        // Draw the buffer now
                        pe.Graphics.DrawImage(buffer, 0, 0);
                    }

                    // Draw the over image
                    if (overImage != null)
                    {
                        RectangleF sourceRect = new RectangleF(0, 0, Image.Width, Image.Height);

                        sourceRect.X += offsetPoint.X;
                        sourceRect.Y += offsetPoint.Y;
                        sourceRect.Width /= scale.X;
                        sourceRect.Height /= scale.Y;

                        pe.Graphics.DrawImage(overImage, sourceRect, sourceRect, GraphicsUnit.Pixel);
                    }

                    // Reset the clipping and draw the grid
                    if (displayGrid && scale.X > 4 && scale.Y > 4)
                    {
                        pe.Graphics.Clip = clip;
                        pe.Graphics.ResetTransform();

                        Pen pen = Pens.Gray;

                        float xOff = (-offsetPoint.X) % scale.X;
                        float yOff = (-offsetPoint.Y) % scale.Y;

                        // Draw the horizontal lines
                        for (float y = yOff; y < Math.Min(this.Height, (this.Image.Height * scale.Y)); y += scale.Y)
                        {
                            pe.Graphics.DrawLine(pen, 0, y, (int)(this.Image.Width * scale.X), y);
                        }

                        // Draw the vertical lines
                        for (float x = xOff; x < Math.Min(this.Width, (this.Image.Width * scale.X)); x += scale.X)
                        {
                            pe.Graphics.DrawLine(pen, x, 0, x, (int)(this.Image.Height * scale.Y));
                        }
                    }
                }
                else
                {
                    // Draw the over image
                    if (overImage != null)
                    {
                        pe.Graphics.DrawImage(overImage, new Point());
                    }
                }
            }

            // 
            // Mouse Click event handler.
            // 
            protected override void OnMouseClick(MouseEventArgs e)
            {
                // Catch the mouse click so the middle mouse button doesn't reset the zoom
            }

            // 
            // Mouse Down event handler
            // 
            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                this.FindForm().ActiveControl = this;

                if (this.Image != null)
                    currentPaintOperation.MouseDown(e);
            }

            // 
            // Mouse Move event handler
            // 
            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (this.Image != null)
                {
                    currentPaintOperation.MouseMove(e);

                    mousePoint = GetAbsolutePoint(e.Location);

                    mouseOverImage = mousePoint.X >= 0 && mousePoint.Y >= 0 && mousePoint.X < Image.Width && mousePoint.Y < Image.Height;
                }
            }

            // 
            // Mouse Up event handler
            // 
            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (this.Image != null)
                    currentPaintOperation.MouseUp(e);
            }

            // 
            // Mouse Leave event handler
            // 
            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);

                mouseOverImage = false;

                if (this.Image != null)
                    currentPaintOperation.MouseLeave(e);
            }

            // 
            // Mouse Enter event handler
            // 
            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);

                if (this.Image != null)
                    currentPaintOperation.MouseEnter(e);
            }

            // 
            // Key Down event handler
            // 
            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (this.Image != null)
                    currentPaintOperation.KeyDown(e);
            }

            // 
            // Key Up event handler
            // 
            protected override void OnKeyUp(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (this.Image != null)
                    currentPaintOperation.KeyUp(e);
            }
        }
    }

    /// <summary>
    /// Arguments for the ClipboardState event
    /// </summary>
    public class ClipboardStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets whether there's content to be copied on the object
        /// </summary>
        public bool CanCopy { get; private set; }

        /// <summary>
        /// Gets whether there's content to be cut on the object
        /// </summary>
        public bool CanCut { get; private set; }

        /// <summary>
        /// Gets whether there's content to be pasted on the object
        /// </summary>
        public bool CanPaste { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ClipboardStateEventArgs
        /// </summary>
        /// <param name="canCopy">Whether there's content to be copied on the object</param>
        /// <param name="canCut">Whether there's content to be cut on the object</param>
        /// <param name="canPaste">Whether there's content to be pasted on the object</param>
        public ClipboardStateEventArgs(bool canCopy, bool canCut, bool canPaste)
        {
            this.CanCopy = canCopy;
            this.CanCut = canCut;
            this.CanPaste = canPaste;
        }
    }

    /// <summary>
    /// Arguments for a OperationStatusChange event
    /// </summary>
    public class OperationStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the operation status
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Gets the operation that fired the status change
        /// </summary>
        public IPaintOperation Operation { get; private set; }

        /// <summary>
        /// Creates a new instance of the OperationStatusChange event
        /// </summary>
        /// <param name="operation">The operation that fired the status change</param>
        /// <param name="status">The operation status</param>
        public OperationStatusEventArgs(IPaintOperation operation, string status)
        {
            this.Operation = operation;
            this.Status = status;
        }
    }

    /// <summary>
    /// Describes an undo task capable of undoing changes made to a bitmap
    /// </summary>
    public class BitmapUndoTask : IUndoTask
    {
        /// <summary>
        /// The target picture box that will be invalidated
        /// </summary>
        PictureBox targetPictureBox;

        /// <summary>
        /// The bitmap that will be the target for the changes
        /// </summary>
        Bitmap targetBitmap;

        /// <summary>
        /// The bitmap that contains the pixels for the undoing of the task
        /// </summary>
        Bitmap oldBitmap;

        /// <summary>
        /// The bitmap that contains the pixels for the redoing of the task
        /// </summary>
        Bitmap newBitmap;

        /// <summary>
        /// The string description for this BitmapUndoTask
        /// </summary>
        string description;

        /// <summary>
        /// Initializes a new instance of the BitmapUndoTask, with a 
        /// </summary>
        /// <param name="targetPictureBox">The target picture box that will be invalidated</param>
        /// <param name="targetBitmap">The target bitmap for this BitmapUndoTask</param>
        /// <param name="description">A short description for this BitmapUndoTask</param>
        public BitmapUndoTask(PictureBox targetPictureBox, Bitmap targetBitmap, string description)
        {
            this.targetPictureBox = targetPictureBox;
            this.targetBitmap = targetBitmap;
            this.description = description;

            oldBitmap = targetBitmap.Clone() as Bitmap;
        }

        /// <summary>
        /// Registers the pixels of the given bitmap as the redo bitmap
        /// </summary>
        /// <param name="newBitmap">The bitmap whose pixels will be used as the redo bitmap</param>
        public void RegisterNewBitmap(Bitmap newBitmap)
        {
            if (this.newBitmap != null)
            {
                this.newBitmap.Dispose();
            }

            this.newBitmap = new Bitmap(newBitmap.Width, newBitmap.Height, PixelFormat.Format32bppArgb);

            FastBitmap.CopyPixels(newBitmap, this.newBitmap);
        }

        /// <summary>
        /// Clears this UndoTask object
        /// </summary>
        public void Clear()
        {
            oldBitmap.Dispose();

            if (newBitmap != null)
            {
                newBitmap.Dispose();
            }
        }

        /// <summary>
        /// Undoes this task
        /// </summary>
        public void Undo()
        {
            FastBitmap.CopyPixels(oldBitmap, targetBitmap);
            targetPictureBox.Invalidate();
        }

        /// <summary>
        /// Redoes this task
        /// </summary>
        public void Redo()
        {
            FastBitmap.CopyPixels(newBitmap, targetBitmap);
            targetPictureBox.Invalidate();
        }

        /// <summary>
        /// Returns a short string description of this UndoTask
        /// </summary>
        /// <returns>A short string description of this UndoTask</returns>
        public string GetDescription()
        {
            return description;
        }
    }

    #region Interfaces

    /// <summary>
    /// Specifies a Paint Operation to be performed on the InternalPictureBox
    /// </summary>
    public interface IPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        Cursor OperationCursor { get; }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        bool Loaded { get; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        void Initialize(ImageEditPanel.InternalPictureBox pictureBox);

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        void Destroy();

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        void ChangeBitmap(Bitmap newBitmap);

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void Paint(PaintEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseDown(MouseEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseMove(MouseEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseUp(MouseEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseLeave(EventArgs e);

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseEnter(EventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyDown(KeyEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyUp(KeyEventArgs e);
    }

    /// <summary>
    /// Specifies a Paint Operation that has a color component
    /// </summary>
    public interface IColoredPaintOperation
    {
        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        Color FirstColor { get; set; }

        /// <summary>
        /// Gets or sets the second color being used to paint on the InternalPictureBox
        /// </summary>
        Color SecondColor { get; set; }
    }

    /// <summary>
    /// Specifies a Paint Operation that has a compositing mode
    /// </summary>
    public interface ICompositingPaintOperation
    {
        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        CompositingMode CompositingMode { get; set; }
    }

    /// <summary>
    /// Specifies a Paint Operation that has a size component
    /// </summary>
    public interface ISizedPaintOperation
    {
        /// <summary>
        /// Gets or sets the size of this SizedPaintOperation
        /// </summary>
        [DefaultValue(1)]
        int Size { get; set; }
    }

    /// <summary>
    /// Specifies a Paint Operation that has a fill mode
    /// </summary>
    public interface IFillModePaintOperation
    {
        /// <summary>
        /// Gets or sets the FillMode for this paint operation
        /// </summary>
        OperationFillMode FillMode { get; set; }
    }

    /// <summary>
    /// Specifies a Paint Operation that has clipboard access capabilities
    /// </summary>
    public interface IClipboardPaintOperation
    {
        /// <summary>
        /// Performs a Copy operation
        /// </summary>
        void Copy();

        /// <summary>
        /// Performs a Cut operation
        /// </summary>
        void Cut();

        /// <summary>
        /// Performs a Paste operation
        /// </summary>
        void Paste();

        /// <summary>
        /// Returns whether the paint operation can copy content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can copy content to the clipboard</returns>
        bool CanCopy();

        /// <summary>
        /// Returns whether the paint operation can cut content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can cut content to the clipboard</returns>
        bool CanCut();

        /// <summary>
        /// Returns whether the paint operation can paste content from the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can paste content from the clipboard</returns>
        bool CanPaste();
    }

    #endregion

    /// <summary>
    /// Specifies an empty paint operation
    /// </summary>
    public class NullPaintOperation : IPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public Cursor OperationCursor { get { return Cursors.Default; } }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public void Initialize(ImageEditPanel.InternalPictureBox pictureBox) { }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public void Destroy() { }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public void ChangeBitmap(Bitmap newBitmap) { }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void Paint(PaintEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseDown(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseMove(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseUp(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseLeave(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseEnter(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void KeyDown(KeyEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void KeyUp(KeyEventArgs e) { }
    }

    /// <summary>
    /// Implements basic functionality to paint operations
    /// </summary>
    public abstract class BasePaintOperation : IPaintOperation
    {
        /// <summary>
        /// The PictureBox owning this PaintOperation
        /// </summary>
        protected ImageEditPanel.InternalPictureBox pictureBox;

        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public virtual Cursor OperationCursor { get { return Cursors.Default; } protected set { } }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public virtual bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public virtual void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public virtual void Destroy() { }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public virtual void ChangeBitmap(Bitmap newBitmap) { }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void Paint(PaintEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseDown(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseMove(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseUp(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseLeave(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void MouseEnter(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void KeyDown(KeyEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public virtual void KeyUp(KeyEventArgs e) { }

        /// <summary>
        /// Returns whether the given Point is within the image bounds.
        /// The point must be in absolute image position
        /// </summary>
        /// <param name="point">The point to get whether or not it's within the image</param>
        /// <returns>Whether the given point is within the image bounds</returns>
        protected virtual bool WithinBounds(Point point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X < pictureBox.Image.Width && point.Y < pictureBox.Image.Height;
        }

        /// <summary>
        /// Invalidates a region of the control, with the given coordinates and size.
        /// The region must be in absolute pixels in relation to the image being edited
        /// </summary>
        /// <param name="point">The point to invalidate</param>
        /// <param name="width">The width of the area to invalidate</param>
        /// <param name="height">The height of the area to invalidate</param>
        protected virtual void InvalidateRect(PointF point, float width, float height)
        {
            point = GetRelativePoint(GetAbsolutePoint(point));

            point.X -= 1;
            point.Y -= 1;

            Rectangle rec = new Rectangle((int)point.X, (int)point.Y, (int)(width * pictureBox.Zoom.Y), (int)(height * pictureBox.Zoom.Y));

            pictureBox.Invalidate(rec);
        }

        /// <summary>
        /// Returns the absolute position of the given point on the canvas image
        /// </summary>
        /// <returns>The absolute position of the given point on the canvas image</returns>
        protected virtual Point GetAbsolutePoint(PointF point)
        {
            return Point.Truncate(pictureBox.GetAbsolutePoint(point));
        }

        /// <summary>
        /// Returns the relative position of the given point on the control bounds
        /// </summary>
        /// <returns>The relative position of the given point on the control bounds</returns>
        protected virtual PointF GetRelativePoint(Point point)
        {
            return pictureBox.GetRelativePoint(point);
        }

        /// <summary>
        /// A per-pixel undo task
        /// </summary>
        public class PerPixelUndoTask : IUndoTask
        {
            /// <summary>
            /// List of pixels stored on this per-pixel undo
            /// </summary>
            private List<PixelUndo> pixelList;

            /// <summary>
            /// Whether to index the pixels being added so they appear sequentially on the pixels list
            /// </summary>
            private bool indexPixels;

            /// <summary>
            /// The width of the bitmap being affected
            /// </summary>
            private int width;

            /// <summary>
            /// The height of the bitmap being affected
            /// </summary>
            private int height;

            /// <summary>
            /// The target InternalPictureBox to perform the undo operation on
            /// </summary>
            private ImageEditPanel.InternalPictureBox pictureBox;

            /// <summary>
            /// The string that describes this PerPixelUndoTask
            /// </summary>
            private string description;

            /// <summary>
            /// Initializes a new instance of the PixelUndoTask
            /// </summary>
            /// <param name="targetPictureBox">The target for the undo operation</param>
            /// <param name="description">A description to use for this UndoTask</param>
            /// <param name="indexPixels">Whether to index the pixels being added so they appear sequentially on the pixel list</param>
            public PerPixelUndoTask(ImageEditPanel.InternalPictureBox targetPictureBox, string description, bool indexPixels = false)
            {
                this.pixelList = new List<PixelUndo>();
                this.pictureBox = targetPictureBox;
                this.description = description;
                this.indexPixels = indexPixels;
                this.width = targetPictureBox.Bitmap.Width;
                this.height = targetPictureBox.Bitmap.Height;
            }

            /// <summary>
            /// Registers a pixel on this PixelUndoTask 
            /// </summary>
            /// <param name="x">The X coordinate of the pixel to store</param>
            /// <param name="y">The Y coordinate of the pixel to store</param>
            /// <param name="oldColor">The old color of the pixel</param>
            /// <param name="newColor">The new color of the pixel</param>
            /// <param name="checkExisting">Whether to check existing pixels before adding the new pixel. Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance</param>
            public void RegisterPixel(int x, int y, Color oldColor, Color newColor, bool checkExisting = true)
            {
                RegisterPixel(x, y, oldColor.ToArgb(), newColor.ToArgb(), checkExisting);
            }

            /// <summary>
            /// Registers a pixel on this PixelUndoTask 
            /// </summary>
            /// <param name="x">The X coordinate of the pixel to store</param>
            /// <param name="y">The Y coordinate of the pixel to store</param>
            /// <param name="oldColor">The old color of the pixel</param>
            /// <param name="newColor">The new color of the pixel</param>
            /// <param name="checkExisting">Whether to check existing pixels before adding the new pixel. Settings this value to false will allow duplicated pixels on this PerPixelUndoTask instance</param>
            public void RegisterPixel(int x, int y, int oldColor, int newColor, bool checkExisting = true)
            {
                // Early out: don't register duplicated pixels
                if (checkExisting && !indexPixels)
                {
                    foreach (PixelUndo pu in pixelList)
                    {
                        if (pu.PixelX == x && pu.PixelY == y)
                            return;
                    }
                }

                InternalRegisterPixel(x, y, oldColor, newColor, !checkExisting);
            }

            /// <summary>
            /// Registers a pixel on this PixelUndoTask 
            /// </summary>
            /// <param name="x">The X coordinate of the pixel to store</param>
            /// <param name="y">The Y coordinate of the pixel to store</param>
            /// <param name="oldColor">The old color of the pixel</param>
            /// <param name="newColor">The new color of the pixel</param>
            /// <param name="replaceExisting">Whether to allow relpacing existing pixels on the list</param>
            private void InternalRegisterPixel(int x, int y, int oldColor, int newColor, bool replaceExisting)
            {
                int pixelIndex = x + y * width;

                PixelUndo item = new PixelUndo() { PixelX = x, PixelY = y, PixelIndex = pixelIndex, UndoColor = oldColor, RedoColor = newColor };

                if (!indexPixels)
                {
                    pixelList.Add(item);
                    return;
                }

                int l = pixelList.Count;

                // Empty list: Add item directly
                if (l == 0)
                {
                    pixelList.Add(item);
                    return;
                }

                int s = 0;
                int e = l - 1;
                while (true)
                {
                    int idC, idM, idF;

                    idF = pixelList[e].PixelIndex;

                    // Pixel index of the item at the end of the interval is smaller than the current pixel index: Add
                    // item after the interval
                    if (idF < pixelIndex)
                    {
                        pixelList.Insert(e + 1, item);
                        return;
                    }
                    // Pixel index of the item at the end of the interval is equals to the item being added: Replace the pixel if replacing is allowed and quit
                    else if (idF == pixelIndex)
                    {
                        if (replaceExisting)
                            pixelList[e] = item;

                        return;
                    }

                    idC = pixelList[s].PixelIndex;

                    // Pixel index of the item at the start of the interval is larger than the current pixel index: Add
                    // item before the interval
                    if (idC > pixelIndex)
                    {
                        pixelList.Insert(s, item);
                        return;
                    }
                    // Pixel index of the item at the start of the interval is equals to the item being added: Replace the pixel if replacing is allowed and quit
                    else if (idC == pixelIndex)
                    {
                        if (replaceExisting)
                            pixelList[s] = item;

                        return;
                    }

                    int mid = s + (e - s) / 2;
                    idM = pixelList[mid].PixelIndex;

                    if (idM > pixelIndex)
                    {
                        s++;
                        e = mid - 1;
                    }
                    else if (idM < pixelIndex)
                    {
                        s = mid + 1;
                        e--;
                    }
                    else if (idM == pixelIndex)
                    {
                        if (replaceExisting)
                            pixelList[mid] = item;

                        return;
                    }

                    // End of search: Add item at the current index
                    if (s > e)
                    {
                        pixelList.Insert(s, item);
                        return;
                    }
                }
            }

            /// <summary>
            /// Returns whether this PerPixelUndoTask contains information about undoing the given pixel
            /// </summary>
            /// <param name="x">The X coordinate of the pixel to search</param>
            /// <param name="y">The Y coordinate of the pixel to search</param>
            /// <returns>Whether this PerPixelUndoTask contains information about undoing the given pixel</returns>
            private bool ContainsPixel(int x, int y)
            {
                return IndexOfPixel(x, y) > -1;
            }

            /// <summary>
            /// Returns the index of a pixel in the pixel list. If no pixel is found, -1 is returned instead
            /// </summary>
            /// <param name="x">The X coordinate of the pixel to search</param>
            /// <param name="y">The Y coordinate of the pixel to search</param>
            /// <returns>The index of a pixel in the pixel list</returns>
            private int IndexOfPixel(int x, int y)
            {
                if (pixelList.Count == 0)
                    return -1;

                int id = x + y * width;

                int s = 0;
                int e = pixelList.Count - 1;

                while (s <= e)
                {
                    int mid = s + (e - s) / 2;
                    int idMid = pixelList[mid].PixelIndex;

                    if (idMid == id)
                    {
                        return mid;
                    }
                    else if (idMid > id)
                    {
                        e = mid - 1;
                    }
                    else if (idMid < id)
                    {
                        s = mid + 1;
                    }
                }

                return -1;
            }

            /// <summary>
            /// Clears this pencil undo task
            /// </summary>
            public void Clear()
            {
                pixelList.Clear();
                pixelList = null;
                pictureBox = null;
            }

            /// <summary>
            /// Performs the undo operation on this per-pixel undo task
            /// </summary>
            public void Undo()
            {
                FastBitmap bitmap = new FastBitmap(pictureBox.Bitmap);

                bitmap.Lock();

                foreach (PixelUndo pu in pixelList)
                {
                    bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.UndoColor);
                }

                bitmap.Unlock();

                pictureBox.Invalidate();
            }

            /// <summary>
            /// Performs the redo operation on this per-pixel undo task
            /// </summary>
            public void Redo()
            {
                FastBitmap bitmap = new FastBitmap(pictureBox.Bitmap);

                bitmap.Lock();

                foreach (PixelUndo pu in pixelList)
                {
                    bitmap.SetPixel(pu.PixelX, pu.PixelY, pu.RedoColor);
                }

                bitmap.Unlock();

                pictureBox.Invalidate();
            }

            /// <summary>
            /// Returns the string description of this undo task
            /// </summary>
            /// <returns>The string description of this undo task</returns>
            public virtual string GetDescription()
            {
                return description;
            }

            /// <summary>
            /// Encapsulates an undo task on a single pixel
            /// </summary>
            private struct PixelUndo
            {
                /// <summary>
                /// The X position of the pixel to draw
                /// </summary>
                public int PixelX;

                /// <summary>
                /// The Y position of the pixel to draw
                /// </summary>
                public int PixelY;

                /// <summary>
                /// The absolute index of the pixel
                /// </summary>
                public int PixelIndex;

                /// <summary>
                /// The color to apply on a undo operation
                /// </summary>
                public int UndoColor;

                /// <summary>
                /// The color to apply on a redo operation
                /// </summary>
                public int RedoColor;

                /// <summary>
                /// Initializes a new instance of the PixelUndo struct
                /// </summary>
                /// <param name="x">The X position of the pixel to draw</param>
                /// <param name="y">The Y position of the pixel to draw</param>
                /// <param name="pixelIndex">The absolute index of the pixel</param>
                /// <param name="oldColor">The color to apply on a undo operation</param>
                /// <param name="newColor">The color to apply on a redo operation</param>
                public PixelUndo(int x, int y, int pixelIndex, Color oldColor, Color newColor)
                {
                    this.PixelX = x;
                    this.PixelY = y;
                    this.PixelIndex = pixelIndex;
                    this.UndoColor = oldColor.ToArgb();
                    this.RedoColor = newColor.ToArgb();
                }

                /// <summary>
                /// Initializes a new instance of the PixelUndo struct
                /// </summary>
                /// <param name="x">The X position of the pixel to draw</param>
                /// <param name="y">The Y position of the pixel to draw</param>
                /// <param name="pixelIndex">The absolute index of the pixel</param>
                /// <param name="oldColor">The color to apply on a undo operation</param>
                /// <param name="newColor">The color to apply on a redo operation</param>
                public PixelUndo(int x, int y, int pixelIndex, int oldColor, int newColor)
                {
                    this.PixelX = x;
                    this.PixelY = y;
                    this.PixelIndex = pixelIndex;
                    this.UndoColor = oldColor;
                    this.RedoColor = newColor;
                }
            }
        }
    }

    /// <summary>
    /// Implements a basic functionality for paint operations that require 'dragging to draw' features
    /// </summary>
    public abstract class BaseDraggingPaintOperation : BasePaintOperation
    {
        /// <summary>
        /// Whether the mouse is currently being held down
        /// </summary>
        protected bool mouseDown;

        /// <summary>
        /// The mouse button currently being held down
        /// </summary>
        protected MouseButtons mouseButton;

        /// <summary>
        /// The point at which the mouse was held down
        /// </summary>
        protected Point mouseDownAbsolutePoint;

        /// <summary>
        /// The current mouse point
        /// </summary>
        protected Point mouseAbsolutePoint;

        /// <summary>
        /// The last absolute mouse position
        /// </summary>
        protected Point lastMouseAbsolutePoint;

        /// <summary>
        /// The last rectangle drawn by the mouse
        /// </summary>
        protected Rectangle lastRect;

        /// <summary>
        /// Whether the SHIFT key is currently held down
        /// </summary>
        protected bool shiftDown;

        /// <summary>
        /// Whether the CONTROL key is currently held down
        /// </summary>
        protected bool ctrlDown;

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            shiftDown = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            Point imagePoint = GetAbsolutePoint(e.Location);

            if (WithinBounds(imagePoint))
            {
                mouseAbsolutePoint = imagePoint;
                mouseDownAbsolutePoint = imagePoint;
                lastMouseAbsolutePoint = imagePoint;

                mouseButton = e.Button;

                mouseDown = true;
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                Rectangle oldArea = lastRect;//GetCurrentRectangle(true);

                lastMouseAbsolutePoint = mouseAbsolutePoint;
                mouseAbsolutePoint = GetAbsolutePoint(e.Location);

                Rectangle newArea = GetCurrentRectangle(true);
                Rectangle newAreaAbs = GetRectangleArea(new Point[] { mouseDownAbsolutePoint, mouseAbsolutePoint }, false);

                if (shiftDown)
                {
                    newArea.Width = Math.Max(newArea.Width, newArea.Height);
                    newArea.Height = Math.Max(newArea.Width, newArea.Height);
                    newAreaAbs.Width = Math.Max(newAreaAbs.Width, newAreaAbs.Height);
                    newAreaAbs.Height = Math.Max(newAreaAbs.Width, newAreaAbs.Height);
                }

                newAreaAbs.Width++; newAreaAbs.Height++;
                pictureBox.OwningPanel.FireOperationStatusEvent(this, newAreaAbs.Width + " x " + newAreaAbs.Height);

                if (newArea != oldArea)
                {
                    oldArea.Inflate((int)(2 * pictureBox.Zoom.X), (int)(2 * pictureBox.Zoom.X));

                    newArea.Inflate((int)(2 * pictureBox.Zoom.X), (int)(2 * pictureBox.Zoom.X));

                    pictureBox.Invalidate(oldArea);
                    pictureBox.Invalidate(newArea);
                }

                lastRect = newArea;
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            mouseDown = false;

            pictureBox.OwningPanel.FireOperationStatusEvent(this, "");
        }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                shiftDown = true;
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                ctrlDown = true;
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void KeyUp(KeyEventArgs e)
        {
            if(e.KeyCode == Keys.ShiftKey)
            {
                shiftDown = false;
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                ctrlDown = false;
            }
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected virtual Rectangle GetCurrentRectangle(bool relative)
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
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected virtual Rectangle GetRectangleArea(Point[] pointList, bool relative)
        {
            Point p1 = pointList[0];

            if (relative)
            {
                p1 = Point.Truncate(GetRelativePoint(p1));
            }

            int minX = p1.X;
            int minY = p1.Y;

            int maxX = p1.X;
            int maxY = p1.Y;

            foreach (Point p in pointList)
            {
                p1 = p;

                if (relative)
                {
                    p1 = Point.Truncate(GetRelativePoint(p));
                }

                minX = Math.Min(p1.X, minX);
                minY = Math.Min(p1.Y, minY);

                maxX = Math.Max(p1.X, maxX);
                maxY = Math.Max(p1.Y, maxY);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        public static Rectangle GetRectangleAreaAbsolute(Point[] pointList)
        {
            int minX = pointList[0].X;
            int minY = pointList[0].Y;

            int maxX = pointList[0].X;
            int maxY = pointList[0].Y;

            foreach (Point p in pointList)
            {
                Point p1 = p;

                minX = Math.Min(p1.X, minX);
                minY = Math.Min(p1.Y, minY);

                maxX = Math.Max(p1.X, maxX);
                maxY = Math.Max(p1.Y, maxY);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }

    /// <summary>
    /// Base class for pencil-like paint operations
    /// </summary>
    public abstract class BasePencilPaintOperation : BasePaintOperation, IPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// Whether the pencil is visible
        /// </summary>
        protected bool visible;

        /// <summary>
        /// Whether the mouse is being held down on the form
        /// </summary>
        protected bool mouseDown = false;

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
        /// The bitmap used to buffer the current pencil operation so the alpha channel is 
        /// </summary>
        protected Bitmap currentTraceBitmap;

        /// <summary>
        /// The undo task for the current pencil operation being performed
        /// </summary>
        protected PerPixelUndoTask currentUndoTask;

        /// <summary>
        /// The compositing mode to use on this pencil
        /// </summary>
        protected CompositingMode compositingMode;

        /// <summary>
        /// The radius of this pencil
        /// </summary>
        protected int size;

        /// <summary>
        /// The string to use as a description for the undo operation
        /// </summary>
        protected string undoDecription;

        /// <summary>
        /// The position to place the pencil point in absolute control coordinates
        /// </summary>
        public virtual Point PencilPoint
        {
            get { return pencilPoint; }
            set
            {
                InvalidatePen();
                this.pencilPoint = value;
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
                    PointF p = pencilPoint;

                    p = GetRelativePoint(GetAbsolutePoint(pencilPoint));

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
        public CompositingMode CompositingMode { get { return compositingMode; } set { compositingMode = value; } }

        /// <summary>
        /// Initializes a new instance of the BasePencilPaintOperation class
        /// </summary>
        public BasePencilPaintOperation()
        {
            this.size = 1;
        }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            this.pictureBox = pictureBox;
            this.lastMousePosition = new Point();

            RegeneratePenBitmap();

            ChangeBitmap(pictureBox.Bitmap);

            this.currentTraceBitmap = new Bitmap(pictureBox.Bitmap.Width, pictureBox.Bitmap.Height);

            this.CompositingMode = pictureBox.OwningPanel.DefaultCompositingMode;

            this.visible = true;

            this.Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            if (!Loaded)
                return;

            FinishOperation();

            InvalidatePen();

            this.pictureBox = null;

            this.graphics.Dispose();
            this.firstPenBitmap.Dispose();
            this.secondPenBitmap.Dispose();

            this.OperationCursor.Dispose();

            this.Loaded = false;
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

            if (newBitmap != null)
            {
                graphics = Graphics.FromImage(newBitmap);
            }
            else
            {
                graphics = null;
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
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

            Graphics gfx = Graphics.FromImage(pictureBox.Buffer);

            // Create image attributes
            ImageAttributes attributes = new ImageAttributes();

            if (compositingMode == CompositingMode.SourceOver)
            {
                // Create a color matrix object
                ColorMatrix matrix = new ColorMatrix();

                // Set the opacity
                matrix.Matrix33 = ((float)(penId == 0 ? firstColor : secondColor).A / 255);

                // Set the color(opacity) of the image
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                if (!mouseDown)
                {
                    gfx.DrawImage(pen, new Rectangle(absolutePencil, new Size(pen.Width, pen.Height)), 0, 0, pen.Width, pen.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            if (mouseDown && CompositingMode == CompositingMode.SourceOver)
            {
                gfx.DrawImage(currentTraceBitmap, new Rectangle(0, 0, currentTraceBitmap.Width, currentTraceBitmap.Height), 0, 0, currentTraceBitmap.Width, currentTraceBitmap.Height, GraphicsUnit.Pixel, attributes);
            }
            else if (CompositingMode == CompositingMode.SourceCopy)
            {
                if (WithinBounds(absolutePencil))
                {
                    pictureBox.Buffer.SetPixel(absolutePencil.X, absolutePencil.Y, firstColor);
                }
            }

            gfx.Flush();
            gfx.Dispose();
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

            // Early out
            if (!WithinBounds(absolutePencil))
            {
                return;
            }

            if (!mouseDown)
            {
                currentUndoTask = new PerPixelUndoTask(pictureBox, undoDecription, true);

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
                // Color pick
                else if (e.Button == MouseButtons.Middle)
                {
                    mouseDown = true;

                    firstColor = pictureBox.Bitmap.GetPixel(absolutePencil.X, absolutePencil.Y);
                    RegeneratePenBitmap();

                    pictureBox.OwningPanel.FireColorChangeEvent(firstColor);

                    pictureBox.Invalidate();
                }

                // Draw a single pixel now
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    Color newColor = (penId == 0 ? firstColor : secondColor);
                    Bitmap targetBitmap = CompositingMode == CompositingMode.SourceOver ? currentTraceBitmap : pictureBox.Bitmap;

                    DrawPencil(absolutePencil, targetBitmap);


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
                Point pencil = GetAbsolutePoint(pencilPoint);
                Point pencilLast = GetAbsolutePoint(lastMousePosition);

                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    if (pencil != pencilLast)
                    {
                        Bitmap targetBitmap = (CompositingMode == CompositingMode.SourceCopy ? pictureBox.Bitmap : currentTraceBitmap);

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

                            if (WithinBounds(p) && p != pencilLast)
                            {
                                DrawPencil(p, targetBitmap);
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
                else if (e.Button == MouseButtons.Middle)
                {
                    if (pencil != pencilLast)
                    {
                        firstColor = pictureBox.Bitmap.GetPixel(pencil.X, pencil.Y);
                        RegeneratePenBitmap();

                        pictureBox.OwningPanel.FireColorChangeEvent(firstColor);

                        pictureBox.Invalidate();
                    }
                }
            }

            lastMousePosition = e.Location;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            FinishOperation();

            mouseDown = false;
        }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseLeave(EventArgs e)
        {
            this.visible = false;
            InvalidatePen();
        }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseEnter(EventArgs e)
        {
            this.visible = true;
        }

        /// <summary>
        /// Finishes this BasePenOperation's current drawing operation
        /// </summary>
        public virtual void FinishOperation()
        {
            if (!mouseDown)
                return;

            if (CompositingMode == CompositingMode.SourceOver)
            {
                // Draw the buffered trace bitmap now

                // Create a color matrix object  
                ColorMatrix matrix = new ColorMatrix();

                // Set the opacity  
                matrix.Matrix33 = ((float)(penId == 0 ? firstColor : secondColor).A / 255);

                // Create image attributes  
                ImageAttributes attributes = new ImageAttributes();

                // Set the color(opacity) of the image  
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                graphics.DrawImage(currentTraceBitmap, new Rectangle(0, 0, currentTraceBitmap.Width, currentTraceBitmap.Height), 0, 0, currentTraceBitmap.Width, currentTraceBitmap.Height, GraphicsUnit.Pixel, attributes);

                // Clear the trace bitmap
                Graphics gfx = Graphics.FromImage(currentTraceBitmap);

                gfx.Clear(Color.Transparent);

                gfx.Dispose();
            }

            pictureBox.MarkModified();

            pictureBox.OwningPanel.UndoSystem.RegisterUndo(currentUndoTask);
            currentUndoTask = null;
        }

        /// <summary>
        /// Draws the pencil with the current properties on the given bitmap object
        /// </summary>
        /// <param name="p">The point to draw the pencil to</param>
        /// <param name="bitmap">The bitmap to draw the pencil on</param>
        protected virtual void DrawPencil(Point p, Bitmap bitmap)
        {
            // Find the properties to draw the pen with
            Color oldColor = pictureBox.Bitmap.GetPixel(p.X, p.Y);
            Color newColor = Color.Black;
            Color penColor = (penId == 0 ? firstColor : secondColor);
            Bitmap pen = (penId == 0 ? firstPenBitmap : secondPenBitmap);

            if (CompositingMode == CompositingMode.SourceOver)
            {
                Color c = Color.FromArgb(penColor.ToArgb() | (0xFF << 24));
                bitmap.SetPixel(p.X, p.Y, c);

                Color backPixel = oldColor;
                newColor = penColor.Blend(backPixel);
            }
            else
            {
                bitmap.SetPixel(p.X, p.Y, penColor);

                newColor = penColor;
            }

            currentUndoTask.RegisterPixel(p.X, p.Y, oldColor, newColor);

            PointF pf = GetRelativePoint(p);
            InvalidateRect(pf, pen.Width, pen.Height);
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

            secondPenBitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            secondPenBitmap.SetPixel(0, 0, Color.FromArgb(255, secondColor.R, secondColor.G, secondColor.B));
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

    /// <summary>
    /// Implements a Pencil paint operation
    /// </summary>
    public class PencilPaintOperation : BasePencilPaintOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// Initializes a new instance of the PencilPaintOperation class
        /// </summary>
        public PencilPaintOperation()
            : base()
        {
            this.undoDecription = "Pencil";
        }

        /// <summary>
        /// Initializes a new instance of the PencilPaintOperation class, initializing the object
        /// with the two pencil colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public PencilPaintOperation(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.FirstColor = firstColor;
            this.SecondColor = secondColor;
            this.Size = 1;
        }

        /// <summary>
        /// Initializes this PencilPaintOperation
        /// </summary>
        /// <param name="pictureBox"></param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.pencil_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }
    }

    /// <summary>
    /// Implements an Eraser paint operation
    /// </summary>
    public class EraserPaintOperation : BasePencilPaintOperation, IColoredPaintOperation
    {
        /// <summary>
        /// Initializes this EraserPaintOperation
        /// </summary>
        /// <param name="pictureBox">The target picture box</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            this.undoDecription = "Eraser";

            this.CompositingMode = CompositingMode.SourceCopy;

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.eraser_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            this.FirstColor = Color.FromArgb(0, 0, 0, 0);
            this.SecondColor = Color.FromArgb(0, 0, 0, 0);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs pe)
        {
            if (!visible)
                return;

            // Prepare the drawing operation to be performed
            Point absolutePencil = Point.Round(GetAbsolutePoint(pencilPoint));
            Bitmap pen = (penId == 0 ? firstPenBitmap : secondPenBitmap);

            if (!this.WithinBounds(absolutePencil))
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
                currentUndoTask = new PerPixelUndoTask(pictureBox, undoDecription, true);

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
                    if (CompositingMode == System.Drawing.Drawing2D.CompositingMode.SourceCopy)
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
                    PointF pf = new PointF();
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

                            currentUndoTask.RegisterPixel(p.X, p.Y, oldColor, newColor);

                            pf = GetRelativePoint(p);
                            InvalidateRect(pf, pen.Width, pen.Height);
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

    /// <summary>
    /// Implements a Picker paint operation
    /// </summary>
    public class PickerPaintOperation : BasePaintOperation, IPaintOperation
    {
        /// <summary>
        /// The last absolute position of the mouse
        /// </summary>
        protected Point lastMousePointAbsolute;

        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public override bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            this.pictureBox = pictureBox;

            this.lastMousePointAbsolute = new Point(-1, -1);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.picker_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            pictureBox = null;

            this.OperationCursor.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            MouseMove(e);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                Point absolute = GetAbsolutePoint(e.Location);

                if (absolute != lastMousePointAbsolute)
                {
                    if (WithinBounds(absolute))
                    {
                        Color color = pictureBox.Bitmap.GetPixel(absolute.X, absolute.Y);

                        pictureBox.OwningPanel.FireColorChangeEvent(color, e.Button == MouseButtons.Left ? ColorPickerColor.FirstColor : ColorPickerColor.SecondColor);
                    }
                }

                lastMousePointAbsolute = absolute;
            }
        }
    }

    /// <summary>
    /// Implements a Spray paint operation
    /// </summary>
    public class SprayPaintOperation : BasePencilPaintOperation, IColoredPaintOperation, ISizedPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// Instance of a Random class used to randomize the spray of this SprayPaintOperation
        /// </summary>
        Random random;

        /// <summary>
        /// The spray's timer, used to make the operation paint with the mouse held down at a stationary point
        /// </summary>
        Timer sprayTimer;

        /// <summary>
        /// Initializes a new instance of the SprayPaintOperation class
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public SprayPaintOperation()
            : base()
        {
            random = new Random();

            sprayTimer = new Timer();
            sprayTimer.Interval = 10;
            sprayTimer.Tick += new EventHandler(sprayTimer_Tick);
        }

        /// <summary>
        /// Initializes a new instance of the SprayPaintOperation class, initializing the object
        /// with the two spray colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public SprayPaintOperation(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.FirstColor = firstColor;
            this.SecondColor = secondColor;
            this.Size = pencilSize;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            sprayTimer.Stop();
            sprayTimer.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// Initializes this PencilPaintOperation
        /// </summary>
        /// <param name="pictureBox"></param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.spray_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            this.undoDecription = "Spray";
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            if (mouseDown)
            {
                sprayTimer.Start();
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);

            if (!mouseDown)
            {
                sprayTimer.Stop();
            }
        }

        /// <summary>
        /// Draws the pencil with the current properties on the given bitmap object
        /// </summary>
        /// <param name="p">The point to draw the pencil to</param>
        /// <param name="bitmap">The bitmap to draw the pencil on</param>
        protected override void DrawPencil(Point p, Bitmap bitmap)
        {
            // Randomize the point around a circle based on the current radius
            double angle = random.NextDouble() * Math.PI * 2;
            float radius = (float)((float)(random.Next(0, size) / 2));

            p.X = p.X + (int)Math.Round(Math.Cos(angle) * radius);
            p.Y = p.Y + (int)Math.Round(Math.Sin(angle) * radius);

            if (WithinBounds(p))
            {
                base.DrawPencil(p, bitmap);
            }
        }

        // 
        // Spray Timer tick
        // 
        private void sprayTimer_Tick(object sender, EventArgs e)
        {
            DrawPencil(GetAbsolutePoint(pencilPoint), (compositingMode == CompositingMode.SourceOver ? currentTraceBitmap : pictureBox.Bitmap));
        }
    }

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
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

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
            FastBitmap fastBitmap = new FastBitmap(bitmap);
            fastBitmap.Lock();

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

                    //newColor = penColor;
                }

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }

            brush.Dispose();

            fastBitmap.Unlock();

            /*
            Graphics graphics = Graphics.FromImage(bitmap);

            PerformLineOperation(color, firstPoint, secondPoint, graphics, compositingMode);

            graphics.Flush();
            graphics.Dispose();*/
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
                    graphics.Flush();
                }
                else
                {
                    Rectangle rec = new Rectangle(p.X, p.Y, 1, 1);

                    graphics.FillRectangle(brush, rec);
                    graphics.Flush();
                    //image.SetPixel(p.X, p.Y, penColor);

                    //newColor = penColor;
                }

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }

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

    /// <summary>
    /// Implements a Bucket paint operation
    /// </summary>
    public class BucketPaintOperation : BasePaintOperation, IPaintOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// The compositing mode for this paint operation
        /// </summary>
        protected CompositingMode compositingMode;

        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// The first color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color firstColor = Color.Black;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color secondColor = Color.Black;

        /// <summary>
        /// The point at which the mouse is currently over
        /// </summary>
        private Point mousePosition;

        /// <summary>
        /// The last recorded mouse position
        /// </summary>
        private Point lastMousePosition;

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color FirstColor { get { return firstColor; } set { firstColor = value; } }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor { get { return secondColor; } set { secondColor = value; } }

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
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.bucket_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            this.Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            this.OperationCursor.Dispose();

            this.Loaded = false;
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
                Color color = e.Button == MouseButtons.Left ? firstColor : secondColor;

                if (WithinBounds(point))
                {
                    PerformBucketOperaiton(color, point, compositingMode);
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                firstColor = pictureBox.Bitmap.GetPixel(point.X, point.Y);

                pictureBox.OwningPanel.FireColorChangeEvent(firstColor);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            mousePosition = e.Location;

            if (e.Button == MouseButtons.Middle)
            {
                Point mouse = GetAbsolutePoint(mousePosition);
                Point mouseLast = GetAbsolutePoint(lastMousePosition);

                if (mouse != mouseLast)
                {
                    firstColor = pictureBox.Bitmap.GetPixel(mouse.X, mouse.Y);

                    pictureBox.OwningPanel.FireColorChangeEvent(firstColor);
                }
            }

            lastMousePosition = mousePosition;
        }

        /// <summary>
        /// Performs the bucket fill operation
        /// </summary>
        /// <param name="color">The color of the fill operation</param>
        /// <param name="point">The point to start the fill operation at</param>
        /// <param name="compositingMode">The CompositingMode of the bucket fill operation</param>
        protected unsafe void PerformBucketOperaiton(Color color, Point point, CompositingMode compositingMode)
        {
            // Start the fill operation by getting the color under the user's mouse
            Color pColor = pictureBox.Bitmap.GetPixel(point.X, point.Y);

            Color newColor = (compositingMode == CompositingMode.SourceCopy ? color : color.Blend(pColor));

            int pColorI = pColor.ToArgb();
            int newColorI = newColor.ToArgb();

            if (pColorI == newColorI || pColor == color && (compositingMode == CompositingMode.SourceOver && pColor.A == 255 || compositingMode == CompositingMode.SourceCopy))
            {
                return;
            }

            // Lock the bitmap
            FastBitmap fastBitmap = new FastBitmap(pictureBox.Bitmap);
            fastBitmap.Lock();

            // Initialize the undo task
            PerPixelUndoTask undoTask = new PerPixelUndoTask(pictureBox, "Flood fill");

            Stack<int> stack = new Stack<int>();

            int y1;
            bool spanLeft, spanRight;

            int width = fastBitmap.Width;
            int height = fastBitmap.Height;

            stack.Push((((int)point.X << 16) | (int)point.Y));

            // Do a floodfill using a vertical scanline algorithm
            while(stack.Count > 0)
            {
                int v = stack.Pop();
                int x = (int)(v >> 16);
                int y = (int)(v & 0xFFFF);

                y1 = y;

                while (y1 >= 0 && fastBitmap.GetPixelInt(x, y1) == pColorI) y1--;

                y1++;
                spanLeft = spanRight = false;

                while (y1 < height && fastBitmap.GetPixelInt(x, y1) == pColorI)
                {
                    fastBitmap.SetPixel(x, y1, newColorI);
                    undoTask.RegisterPixel(x, y1, pColorI, newColorI, false);

                    int pixel;

                    if (x > 0)
                    {
                        pixel = fastBitmap.GetPixelInt(x - 1, y1);

                        if (!spanLeft && pixel == pColorI)
                        {
                            stack.Push((((int)(x - 1) << 16) | (int)y1));

                            spanLeft = true;
                        }
                        else if (spanLeft && pixel != pColorI)
                        {
                            spanLeft = false;
                        }
                    }

                    if (x < width - 1)
                    {
                        pixel = fastBitmap.GetPixelInt(x + 1, y1);

                        if (!spanRight && pixel == pColorI)
                        {
                            stack.Push((((int)(x + 1) << 16) | (int)y1));
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

            pictureBox.Invalidate();
            pictureBox.MarkModified();

            pictureBox.OwningPanel.UndoSystem.RegisterUndo(undoTask);
        }
    }

    /// <summary>
    /// Implements a Selection paint operation
    /// </summary>
    public class SelectionPaintOperation : BaseDraggingPaintOperation, IPaintOperation, IClipboardPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// Timer used to animate the selection area
        /// </summary>
        private Timer animTimer;

        /// <summary>
        /// The dash offset to use when drawing the selection area
        /// </summary>
        private float dashOffset;

        /// <summary>
        /// The currently selected area
        /// </summary>
        protected Rectangle selectedArea;

        /// <summary>
        /// The currently selected area in control coordinates
        /// </summary>
        protected Rectangle relativeSelectedArea;

        /// <summary>
        /// The area the current selection was sliced from
        /// </summary>
        protected Rectangle selectedStartArea;

        /// <summary>
        /// Whether there's an area currently selected
        /// </summary>
        protected bool selected;

        /// <summary>
        /// Whether to display the selection
        /// </summary>
        protected bool displaySelection;

        /// <summary>
        /// Whether the mouse is currently drawing the selection area
        /// </summary>
        protected bool drawingSelection;

        /// <summary>
        /// Whether the mouse is currently moving the selection area
        /// </summary>
        protected bool movingSelection;

        /// <summary>
        /// The offset for the selection moving operation
        /// </summary>
        protected Point movingOffset;

        /// <summary>
        /// The compositing mode for this paint operation
        /// </summary>
        protected CompositingMode compositingMode;

        /// <summary>
        /// Graphics used to draw on the selection bitmap
        /// </summary>
        protected Graphics graphics;

        /// <summary>
        /// The buffer bitmap for drawing the clipboard on
        /// </summary>
        protected Bitmap selectionBitmap;

        /// <summary>
        /// The mode of the current selection operation
        /// </summary>
        private SelectionOperationType operationMode;

        /// <summary>
        /// Gets whether there's an area currently selected
        /// </summary>
        public bool Selected { get { return selected; } }

        /// <summary>
        /// Gets or sets the bitmap that represents the currently selected graphics
        /// </summary>
        public Bitmap SelectionBitmap { get { return selectionBitmap; } set { selectionBitmap = value; pictureBox.Invalidate(); } }

        /// <summary>
        /// Gets the area the selection is currently occupying in the canvas
        /// </summary>
        public Rectangle SelectionArea { get { return selectedArea; } set { selectedArea = value; pictureBox.Invalidate(GetSelectionArea(true)); } }

        /// <summary>
        /// Gets the area the selection was snipped from
        /// </summary>
        public Rectangle SelectionStartArea { get { return selectedStartArea; } }

        /// <summary>
        /// Gets the operation currently being performed by this SelectionPaintOperation
        /// </summary>
        public SelectionOperationType OperationType { get { return operationMode; } }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get { return compositingMode; } set { compositingMode = value; if (selected) { pictureBox.Invalidate(GetSelectionArea(true)); } } }

        /// <summary>
        /// Gets or sets whether to force the creation of an undo task after the operation is finished even if no significant changes have been made with this paint operation
        /// </summary>
        public bool ForceApplyChanges { get; set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.sel_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            this.animTimer = new Timer();
            this.animTimer.Interval = 200;
            this.animTimer.Tick += new EventHandler(animTimer_Tick);

            this.displaySelection = true;
            this.selected = false;

            base.Initialize(pictureBox);

            pictureBox.OwningPanel.UndoSystem.UndoPerformed += new UndoSystem.UndoEventHandler(UndoSystem_UndoPerformed);
            pictureBox.OwningPanel.UndoSystem.RedoPerformed += new UndoSystem.UndoEventHandler(UndoSystem_RedoPerformed);

            this.Loaded = true;
        }

        // 
        // Animation Timer tick
        // 
        private void animTimer_Tick(object sender, EventArgs e)
        {
            dashOffset += 1f;
            pictureBox.Invalidate(GetSelectionArea(true));
        }

        // 
        // Undo System Undo Performed event handler
        // 
        private void UndoSystem_UndoPerformed(object sender, UndoEventArgs e)
        {
            CancelOperation(false);
        }

        // 
        // Undo System Redo Performed event handler
        // 
        private void UndoSystem_RedoPerformed(object sender, UndoEventArgs e)
        {
            CancelOperation(false);
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            FinishOperation(true);

            this.pictureBox = null;

            if (graphics != null)
            {
                graphics.Dispose();
                graphics = null;
            }

            if (selectionBitmap != null)
            {
                selectionBitmap.Dispose();
                selectionBitmap = null;
            }

            OperationCursor.Dispose();

            animTimer.Stop();
            animTimer.Dispose();

            this.Loaded = false;
        }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public override void ChangeBitmap(Bitmap newBitmap)
        {
            CancelOperation(false);
        }

        /// <summary>
        /// Creates a selection that is the size of the current image
        /// </summary>
        public void SelectAll()
        {
            if (selected)
            {
                FinishOperation(true);
            }

            StartOperation(new Rectangle(0, 0, pictureBox.Image.Width, pictureBox.Image.Height), null);
        }

        /// <summary>
        /// Performs a Copy operation
        /// </summary>
        public void Copy()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                selectionBitmap.Save(stream, ImageFormat.Png);

                Clipboard.Clear();

                var data = new DataObject();
                data.SetData(DataFormats.Bitmap, true, selectionBitmap);
                data.SetData("PNG", true, stream);

                Clipboard.SetDataObject(data, true);
            }

            UpdateClipboardState();
        }

        /// <summary>
        /// Performs a Cut operation
        /// </summary>
        public void Cut()
        {
            operationMode = SelectionOperationType.Cut;

            Copy();

            FinishOperation(false);

            UpdateClipboardState();
        }

        /// <summary>
        /// Performs a Paste operation
        /// </summary>
        public void Paste()
        {
            Stream str = Clipboard.GetData("PNG") as Stream;
            Bitmap bit = null;

            if (str != null)
            {
                bit = Bitmap.FromStream(str) as Bitmap;

                Bitmap temp = new Bitmap(bit.Width, bit.Height, PixelFormat.Format32bppArgb);

                FastBitmap.CopyPixels(bit, temp);

                bit.Dispose();
                bit = temp;

                str.Dispose();
            }
            else
            {
                bit = Clipboard.GetImage() as Bitmap;
            }

            if (bit != null)
            {
                FinishOperation(true);

                operationMode = SelectionOperationType.Paste;

                // Get the top-left pixel to place the selection at
                Point loc = GetAbsolutePoint(new PointF(0, 0));
                
                StartOperation(new Rectangle(loc.X, loc.Y, bit.Width, bit.Height), bit);
            }

            pictureBox.MarkModified();
        }

        /// <summary>
        /// Returns whether the paint operation can copy content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can copy content to the clipboard</returns>
        public bool CanCopy()
        {
            return selected && !movingSelection;
        }

        /// <summary>
        /// Returns whether the paint operation can cut content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can cut content to the clipboard</returns>
        public bool CanCut()
        {
            return selected && !movingSelection;
        }

        /// <summary>
        /// Returns whether the paint operation can paste content from the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can paste content from the clipboard</returns>
        public bool CanPaste()
        {
            return Clipboard.ContainsData("PNG") || Clipboard.ContainsImage();
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            if (drawingSelection)
            {
                selectedArea = GetCurrentRectangle(false);
                relativeSelectedArea = GetCurrentRectangle(true);
            }

            if (selected || drawingSelection)
            {
                Graphics gfx = Graphics.FromImage(pictureBox.Buffer);

                // Draw the selection bitmap
                if (selectionBitmap != null)
                {
                    if (compositingMode == CompositingMode.SourceCopy)
                    {
                        Region reg = gfx.Clip;

                        gfx.SetClip(GetSelectionArea(false));

                        gfx.Clear(Color.Transparent);

                        gfx.Clip = reg;
                    }

                    gfx.DrawImage(selectionBitmap, selectedArea);
                }

                if (displaySelection)
                {
                    Rectangle rec = GetSelectionArea(false);

                    rec.Width--;
                    rec.Height--;

                    Pen p = new Pen(Color.Black);

                    p.DashStyle = DashStyle.Dash;
                    p.DashOffset = dashOffset;
                    p.DashPattern = new float[] { 2f, 2f };
                    p.Alignment = PenAlignment.Inset;
                    p.Width = 1;

                    gfx.DrawRectangle(p, rec);

                    p = new Pen(Color.White);

                    p.DashStyle = DashStyle.Dash;
                    p.DashOffset = dashOffset + 1.99f;
                    p.DashPattern = new float[] { 2f, 2f };
                    p.Alignment = PenAlignment.Inset;
                    p.Width = 1;

                    gfx.DrawRectangle(p, rec);

                    p.Dispose();
                }

                gfx.Flush();
                gfx.Dispose();
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            
            Point p = GetAbsolutePoint(e.Location);

            if (!selectedArea.Contains(p))
            {
                if (selected)
                {
                    FinishOperation(true);
                }
            }
            else if(selected && WithinBounds(p))
            {
                // If the control key is currently down, duplicate the image and start dragging the duplicated image instead
                if (ctrlDown)
                {
                    FinishOperation(true);
                    StartOperation(selectedArea, ExtractBitmap(selectedArea), SelectionOperationType.Paste);
                }

                movingSelection = true;
                movingOffset = new Point(p.X - selectedArea.X, p.Y - selectedArea.Y);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            Point p = GetAbsolutePoint(e.Location);

            if (selected)
            {
                if (movingSelection)
                {
                    if (lastMouseAbsolutePoint != mouseAbsolutePoint)
                    {
                        pictureBox.Invalidate(GetSelectionArea(true));

                        selectedArea.X = p.X - movingOffset.X;
                        selectedArea.Y = p.Y - movingOffset.Y;

                        pictureBox.Invalidate(GetSelectionArea(true));

                        displaySelection = false;

                        UpdateClipboardState();
                    }
                }

                if (selectedArea.Contains(p))
                {
                    pictureBox.Cursor = Cursors.SizeAll;
                }
                else
                {
                    pictureBox.Cursor = OperationCursor;
                }
            }
            else if (mouseDown)
            {
                if (lastMouseAbsolutePoint != mouseAbsolutePoint)
                {
                    drawingSelection = true;
                }
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            Point p = GetAbsolutePoint(e.Location);

            if (mouseDown)
            {
                if (movingSelection)
                {
                    movingSelection = false;
                    displaySelection = true;

                    pictureBox.MarkModified();

                    UpdateClipboardState();
                }
                else if(drawingSelection)
                {
                    selectedArea = GetCurrentRectangle(false);
                    relativeSelectedArea = GetCurrentRectangle(true);

                    if (selectedArea.Width > 0 && selectedArea.Height > 0)
                    {
                        selected = true;
                        animTimer.Start();

                        operationMode = SelectionOperationType.Moved;

                        selectedStartArea = selectedArea;

                        StartOperation(selectedArea, null);

                        drawingSelection = false;
                        mouseDown = false;
                    }
                }
            }
            else if (selected == false)
            {
                animTimer.Stop();
            }

            base.MouseUp(e);
        }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void KeyDown(KeyEventArgs e)
        {
            base.KeyDown(e);

            // Selection delete
            if (e.KeyCode == Keys.Delete)
            {
                if (selected)
                {
                    operationMode = SelectionOperationType.Cut;
                    FinishOperation(false);
                    pictureBox.MarkModified();
                }
            }
            // Selection moving
            else if (e.KeyCode == Keys.Left)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.X--;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
            else if (e.KeyCode == Keys.Right)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.X++;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
            else if (e.KeyCode == Keys.Up)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.Y--;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
            else if (e.KeyCode == Keys.Down)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.Y++;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
        }

        /// <summary>
        /// Extracts a portion of the edit image and returns a bitmap of it.
        /// The operation does not modifies the original image
        /// </summary>
        /// <param name="area">The area to remove from the image</param>
        /// <returns>A bitmap of the selected portion of the bitmap</returns>
        public Bitmap ExtractBitmap(Rectangle area)
        {
            Bitmap bitmap = new Bitmap(area.Width, area.Height);

            graphics = Graphics.FromImage(bitmap);

            graphics.DrawImage(pictureBox.Bitmap, new Rectangle(0, 0, area.Width, area.Height), area, GraphicsUnit.Pixel);

            graphics.Dispose();
            graphics = null;

            return bitmap;
        }

        /// <summary>
        /// Starts the selection operation with the given area
        /// </summary>
        /// <param name="area">The area to snip the bitmap from</param>
        public void StartOperation(Rectangle area)
        {
            StartOperation(area, null);
        }

        /// <summary>
        /// Starts the selection operation with the given area and operation mode
        /// </summary>
        /// <param name="area">The area to select</param>
        /// <param name="operation">The operation mode to apply</param>
        public void StartOperation(Rectangle area, SelectionOperationType operation)
        {
            StartOperation(area, null, operation);
        }

        /// <summary>
        /// Starts the selection operation with the given area and bitmap
        /// </summary>
        /// <param name="area">The area to place the bitmap at</param>
        /// <param name="pasteBitmap">The bitmap to paste</param>
        public void StartOperation(Rectangle area, Bitmap pasteBitmap)
        {
            StartOperation(area, pasteBitmap, operationMode);
        }

        /// <summary>
        /// Starts the selection operation witht he given area, bitmap and operation type
        /// </summary>
        /// <param name="area">The area to place the bitmap at</param>
        /// <param name="pasteBitmap">The bitmap to paste</param>
        /// <param name="operation">The operation type to mark this selection as</param>
        public void StartOperation(Rectangle area, Bitmap pasteBitmap, SelectionOperationType operation)
        {
            operationMode = operation;

            ForceApplyChanges = false;

            if (selectionBitmap != null)
            {
                selectionBitmap.Dispose();
            }

            this.selected = true;
            this.selectedArea = area;

            this.selectedStartArea = area;

            // Get the selected area
            if (pasteBitmap == null)
            {
                pasteBitmap = ExtractBitmap(area);

                // Clear the bitmap behind the image
                graphics = Graphics.FromImage(pictureBox.Image);

                graphics.SetClip(selectedArea);

                graphics.Clear(Color.Transparent);

                graphics.Dispose();
                graphics = null;

                UpdateClipboardState();
            }

            this.selectionBitmap = pasteBitmap;

            pictureBox.Invalidate(GetSelectionArea(true));

            animTimer.Start();
        }

        /// <summary>
        /// Cancels the current dragging operation
        /// </summary>
        public void CancelOperation(bool drawOnCanvas)
        {
            if (!selected)
                return;

            if (operationMode == SelectionOperationType.Moved && drawOnCanvas)
            {
                if (selectionBitmap != null)
                {
                    // Draw the original slice back
                    graphics = Graphics.FromImage(pictureBox.Image);

                    graphics.DrawImage(selectionBitmap, selectedStartArea);

                    graphics.Flush();
                    graphics.Dispose();
                    graphics = null;
                }
            }

            pictureBox.Invalidate(GetSelectionArea(true));

            selectedArea = selectedStartArea;

            pictureBox.Invalidate(GetSelectionArea(true));

            pictureBox.Cursor = OperationCursor;

            selectionBitmap.Dispose();
            selectionBitmap = null;

            movingSelection = false;
            displaySelection = true;

            selected = false;
            animTimer.Stop();

            UpdateClipboardState();

            // Default the operation mode to 'Moved'
            operationMode = SelectionOperationType.Moved;
        }

        /// <summary>
        /// Finishes the selection dragging operation
        /// </summary>
        /// <param name="drawToCanvas">Whether to draw the image to canvas before deleting it</param>
        public void FinishOperation(bool drawToCanvas)
        {
            if (selectionBitmap == null)
                return;

            Bitmap originalSlice = null;

            if (drawToCanvas)
            {
                originalSlice = new Bitmap(selectedArea.Width, selectedArea.Height);

                Graphics g = Graphics.FromImage(originalSlice);

                g.DrawImage(pictureBox.Image, new Rectangle(0, 0, selectedArea.Width, selectedArea.Height), selectedArea, GraphicsUnit.Pixel);

                g.Flush();
                g.Dispose();

                // Render the selected area
                graphics = Graphics.FromImage(pictureBox.Image);

                graphics.CompositingMode = compositingMode;

                graphics.DrawImage(selectionBitmap, selectedArea);

                graphics.Flush();
                graphics.Dispose();
                graphics = null;
            }

            if (selectedArea != selectedStartArea || operationMode != SelectionOperationType.Moved || ForceApplyChanges)
            {
                // Record the undo operation
                pictureBox.OwningPanel.UndoSystem.RegisterUndo(new SelectionUndoTask(this, pictureBox, selectionBitmap, originalSlice, selectedStartArea, selectedArea, operationMode, compositingMode));

                pictureBox.MarkModified();
            }

            Rectangle rec = GetSelectionArea(true);

            pictureBox.Invalidate(rec);

            pictureBox.Cursor = OperationCursor;

            selectionBitmap.Dispose();
            selectionBitmap = null;

            movingSelection = false;
            displaySelection = true;

            selected = false;
            animTimer.Stop();

            UpdateClipboardState();

            // Default the operation mode to 'Moved'
            operationMode = SelectionOperationType.Moved;
        }

        /// <summary>
        /// Updates the clipboard state of this paint operation
        /// </summary>
        private void UpdateClipboardState()
        {
            pictureBox.OwningPanel.FireClipboardStateEvent(CanCopy(), CanCut(), CanPaste());
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            Point p1 = mouseDownAbsolutePoint;
            Point p2 = mouseAbsolutePoint;

            // Clip the selected area to be within the image boundaries
            p1.X = Math.Max(0, Math.Min(pictureBox.Image.Width - 1, p1.X));
            p1.Y = Math.Max(0, Math.Min(pictureBox.Image.Height - 1, p1.Y));

            p2.X = Math.Max(0, Math.Min(pictureBox.Image.Width - 1, p2.X));
            p2.Y = Math.Max(0, Math.Min(pictureBox.Image.Height - 1, p2.Y));

            Rectangle rec = GetRectangleArea(new Point[] { p1, p2 }, relative);

            if (relative)
            {
                rec.Width += (int)(pictureBox.Zoom.X);
                rec.Height += (int)(pictureBox.Zoom.Y);
            }
            else
            {
                rec.Width++;
                rec.Height++;
            }

            if (shiftDown)
            {
                rec.Width = Math.Max(rec.Width, rec.Height);
                rec.Height = Math.Max(rec.Width, rec.Height);
            }

            return rec;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the currently selected area
        /// </summary>
        /// <param name="absolute">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the currently selected area</returns>
        protected Rectangle GetSelectionArea(bool relative)
        {
            if (!relative)
            {
                return selectedArea;
            }
            else
            {
                return relativeSelectedArea = new Rectangle((int)Math.Floor(-pictureBox.Offset.X + selectedArea.X * pictureBox.Zoom.X) - 1, (int)Math.Floor(-pictureBox.Offset.Y + selectedArea.Y * pictureBox.Zoom.Y) - 1, (int)Math.Ceiling(selectedArea.Width * pictureBox.Zoom.X) + 2, (int)Math.Ceiling(selectedArea.Height * pictureBox.Zoom.Y) + 2);
            }
        }

        /// <summary>
        /// A selection undo task
        /// </summary>
        protected class SelectionUndoTask : IUndoTask
        {
            /// <summary>
            /// The owning paint operation
            /// </summary>
            SelectionPaintOperation paintOperation;

            /// <summary>
            /// The target picture box for the undo task
            /// </summary>
            ImageEditPanel.InternalPictureBox pictureBox;

            /// <summary>
            /// The selection bitmap
            /// </summary>
            Bitmap selectionBitmap;

            /// <summary>
            /// The original image slice before a Move operation was made
            /// </summary>
            Bitmap originalSlice;

            /// <summary>
            /// The area that the selection started at
            /// </summary>
            Rectangle selectionStartArea;

            /// <summary>
            /// The area of the affected SelectionUndoTask
            /// </summary>
            Rectangle area;

            /// <summary>
            /// The operation type of this SelectionUndoTask
            /// </summary>
            SelectionOperationType operationType;

            /// <summary>
            /// The compositing mode for the operation
            /// </summary>
            CompositingMode compositingMode;

            /// <summary>
            /// Initializes a new instance of the SelectionUndoTask class
            /// </summary>
            /// <param name="paintOperation">The owning SelectionPaintOperation object</param>
            /// <param name="pictureBox">The target picture box for the undo task</param>
            /// <param name="selectionBitmap">The selection bitmap</param>
            /// <param name="originalSlice">The original image slice before a Move operation was made</param>
            /// <param name="selectionStartArea">The area that the selection started at</param>
            /// <param name="area">The area of the affected SelectionUndoTask</param>
            /// <param name="operationType">The operation type of this SelectionUndoTask</param>
            /// <param name="compositingMode">The compositing mode for the operation</param>
            public SelectionUndoTask(SelectionPaintOperation paintOperation, ImageEditPanel.InternalPictureBox pictureBox, Bitmap selectionBitmap, Bitmap originalSlice, Rectangle selectionStartArea, Rectangle area, SelectionOperationType operationType, CompositingMode compositingMode)
            {
                this.paintOperation = paintOperation;
                this.pictureBox = pictureBox;
                this.selectionBitmap = (Bitmap)selectionBitmap.Clone();
                this.originalSlice = (originalSlice == null ? null : (Bitmap)originalSlice.Clone());
                this.selectionStartArea = selectionStartArea;
                this.area = area;
                this.operationType = operationType;
                this.compositingMode = compositingMode;
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                pictureBox = null;

                if (originalSlice != null)
                {
                    originalSlice.Dispose();
                    originalSlice = null;
                }

                if (selectionBitmap != null)
                {
                    selectionBitmap.Dispose();
                    selectionBitmap = null;
                }
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                Graphics gfx = Graphics.FromImage(pictureBox.Bitmap);

                gfx.CompositingMode = CompositingMode.SourceCopy;

                switch (operationType)
                {
                    case SelectionOperationType.Moved:
                        // Draw the original slice back
                        gfx.DrawImage(originalSlice, area, new Rectangle(0, 0, originalSlice.Width, originalSlice.Height), GraphicsUnit.Pixel);
                        // Draw the selection back
                        gfx.DrawImage(selectionBitmap, selectionStartArea, new Rectangle(0, 0, selectionBitmap.Width, selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                    case SelectionOperationType.Cut:
                        // Draw the original slice back
                        gfx.DrawImage(selectionBitmap, selectionStartArea, new Rectangle(0, 0, selectionBitmap.Width, selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                    case SelectionOperationType.Paste:
                        // Draw the original slice back
                        gfx.DrawImage(originalSlice, area, new Rectangle(0, 0, originalSlice.Width, originalSlice.Height), GraphicsUnit.Pixel);
                        break;
                }

                gfx.Flush();
                gfx.Dispose();

                pictureBox.Invalidate();
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                Graphics gfx = Graphics.FromImage(pictureBox.Bitmap);
                Region reg;

                gfx.CompositingMode = compositingMode;

                switch (operationType)
                {
                    case SelectionOperationType.Moved:
                        // Clear the image background
                        reg = gfx.Clip;
                        gfx.SetClip(selectionStartArea);
                        gfx.Clear(Color.Transparent);
                        gfx.Clip = reg;
                        //gfx.DrawImage(selectionBitmap, area.X, area.Y);
                        gfx.DrawImage(selectionBitmap, area, new Rectangle(0, 0, selectionBitmap.Width, selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                    case SelectionOperationType.Cut:
                        reg = gfx.Clip;
                        gfx.SetClip(selectionStartArea);
                        gfx.Clear(Color.Transparent);
                        gfx.Clip = reg;
                        break;
                    case SelectionOperationType.Paste:
                        // Draw the original slice back
                        //gfx.DrawImage(selectionBitmap, area.X, area.Y);
                        gfx.DrawImage(selectionBitmap, area, new Rectangle(0, 0, selectionBitmap.Width, selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                }

                gfx.Flush();
                gfx.Dispose();

                pictureBox.Invalidate();
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public string GetDescription()
            {
                switch (operationType)
                {
                    case SelectionOperationType.Cut:
                        return "Cut";
                    case SelectionOperationType.Moved:
                        return "Move";
                    case SelectionOperationType.Paste:
                        return "Paste";
                    default:
                        return "Selection";
                }
            }
        }

        /// <summary>
        /// Describes the type of operation of a SelectionUndoTask
        /// </summary>
        public enum SelectionOperationType
        {
            /// <summary>
            /// The selection was cut from the canvas
            /// </summary>
            Cut,
            /// <summary>
            /// The selection was moved on the canvas
            /// </summary>
            Moved,
            /// <summary>
            /// The selection was pasted on the canvas
            /// </summary>
            Paste
        }
    }

    /// <summary>
    /// Implements a Zoom paint operation
    /// </summary>
    public class ZoomPaintOperation : BaseDraggingPaintOperation, IPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// The relative point where the mouse was held down, in control coordinates
        /// </summary>
        public Point mouseDownRelative;

        /// <summary>
        /// The relative point where the mouse is currently at, in control coordinates
        /// </summary>
        public Point mousePointRelative;

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.zoom_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
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
                // Draw the zoom area
                Rectangle rec = GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false);

                e.Graphics.ResetTransform();

                e.Graphics.DrawRectangle(Pens.Gray, rec);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            mousePointRelative = mouseDownRelative = e.Location;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            if (mouseDown)
            {
                pictureBox.Invalidate(GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false));

                mousePointRelative = e.Location;

                pictureBox.Invalidate(GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false));
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            FinishOperation();

            base.MouseUp(e);
        }

        /// <summary>
        /// Finishes this ZoomOperation's current operation
        /// </summary>
        public void FinishOperation()
        {
            if (!mouseDown)
                return;

            Rectangle zoomArea = GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false);

            pictureBox.Invalidate(zoomArea);

            float zoomX = pictureBox.Width / (float)zoomArea.Width;
            float zoomY = pictureBox.Height / (float)zoomArea.Height;

            if (zoomArea.Width < 2 && zoomArea.Height < 2)
            {
                zoomX = zoomY = 2f;

                zoomArea.X -= pictureBox.Width / 2;
                zoomArea.Y -= pictureBox.Height / 2;
            }

            zoomY = zoomX = Math.Min(zoomX, zoomY);

            pictureBox.Zoom = new PointF(pictureBox.Zoom.X * zoomX, pictureBox.Zoom.Y * zoomY);

            // Zoom in into the located region
            Point relative = pictureBox.Offset;

            relative.X += (int)(zoomArea.X * zoomX);
            relative.Y += (int)(zoomArea.Y * zoomX);

            pictureBox.Offset = relative;
        }
    }

    /// <summary>
    /// Describes the fill mode for a FillModePaintOperation
    /// </summary>
    public enum OperationFillMode
    {
        /// <summary>
        /// Uses the first color as outline for the rectangle
        /// </summary>
        OutlineFirstColor,
        /// <summary>
        /// Uses the first color as a solid color
        /// </summary>
        SolidFillFirstColor,
        /// <summary>
        /// Uses the first color as outline and the second color as fill
        /// </summary>
        OutlineFirstColorFillSecondColor,
        /// <summary>
        /// Uses the second color as solid color
        /// </summary>
        SolidFillSecondColor
    }
}
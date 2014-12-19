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
using Pixelaria.Views.Controls.ColorControls;
using Pixelaria.Views.Controls.PaintOperations;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;
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
            internalPictureBox.HookToControl(this.FindForm());
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
            /// The list of picture box decorators
            /// </summary>
            private List<PictureBoxDecorator> pictureBoxDecorators;

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
                this.pictureBoxDecorators = new List<PictureBoxDecorator>();

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

                foreach(PictureBoxDecorator decorator in pictureBoxDecorators)
                {
                    decorator.Destroy();
                }

                // Create the under and over images
                if (overImage != null)
                {
                    overImage.Dispose();
                    overImage = null;
                }
                if (underImage != null)
                {
                    underImage.Dispose();
                    underImage = null;
                }

                pictureBoxDecorators.Clear();

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

                // Create the under and over images
                if (overImage != null)
                {
                    overImage.Dispose();
                }
                if (underImage != null)
                {
                    underImage.Dispose();
                }

                overImage = new Bitmap(buffer.Width, buffer.Height, PixelFormat.Format32bppArgb);
                underImage = new Bitmap(buffer.Width, buffer.Height, PixelFormat.Format32bppArgb);

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

            /// <summary>
            /// Adds a decorator to this picture box
            /// </summary>
            /// <param name="decorator">The decorator to add to this picture box</param>
            public void AddDecorator(PictureBoxDecorator decorator)
            {
                this.pictureBoxDecorators.Add(decorator);
                decorator.AddedToPictureBox(this);
                this.Invalidate();
            }

            /// <summary>
            /// Removes a decorator from this picture box
            /// </summary>
            /// <param name="decorator">The decorator to remove from this picture box</param>
            public void RemoveDecorator(PictureBoxDecorator decorator)
            {
                this.pictureBoxDecorators.Remove(decorator);
                this.Invalidate();
            }

            /// <summary>
            /// Removes a decorator from this picture box
            /// </summary>
            /// <param name="decorator">The decorator to remove from this picture box</param>
            public void ClearDecorators()
            {
                this.pictureBoxDecorators.Clear();
                this.Invalidate();
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

                    // Apply the decorators
                    Image copy = underImage;

                    if (pictureBoxDecorators.Count > 0)
                    {
                        copy = ((Bitmap)underImage).Clone(new Rectangle(0, 0, underImage.Width, underImage.Height), underImage.PixelFormat);

                        foreach (PictureBoxDecorator decorator in pictureBoxDecorators)
                        {
                            decorator.DecorateUnderImage(copy);
                        }
                    }

                    pe.Graphics.DrawImage(copy, new Point());

                    pe.Graphics.ResetTransform();
                }

                if (this.Image != null)
                {
                    UpdateGraphicsTransform(pe.Graphics);

                    // Reset the buffer back to the original input bitmap state
                    FastBitmap.CopyPixels(Bitmap, buffer);

                    // Clip to the image's boundaries
                    pe.Graphics.IntersectClip(new RectangleF(0, 0, Image.Width, Image.Height));
                    Region clip = pe.Graphics.Clip;

                    // Start painting now
                    currentPaintOperation.Paint(pe);

                    if (displayImage)
                    {
                        foreach (PictureBoxDecorator decorator in pictureBoxDecorators)
                        {
                            decorator.DecorateMainImage(buffer);
                        }

                        // Draw the buffer now
                        pe.Graphics.DrawImage(buffer, 0, 0);
                    }

                    // Draw the over image
                    if (overImage != null)
                    {
                        // Apply the decorators
                        Image copy = overImage;

                        if (pictureBoxDecorators.Count > 0)
                        {
                            copy = ((Bitmap)overImage).Clone(new Rectangle(0, 0, overImage.Width, overImage.Height), overImage.PixelFormat);

                            foreach (PictureBoxDecorator decorator in pictureBoxDecorators)
                            {
                                decorator.DecorateFrontImage(copy);
                            }
                        }

                        pe.Graphics.DrawImage(copy, Point.Empty);
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
                        Image copy = overImage;

                        if (pictureBoxDecorators.Count > 0)
                        {
                            copy = ((Bitmap)overImage).Clone(new Rectangle(0, 0, overImage.Width, overImage.Height), overImage.PixelFormat);

                            foreach (PictureBoxDecorator decorator in pictureBoxDecorators)
                            {
                                decorator.DecorateUnderImage(copy);
                            }
                        }

                        pe.Graphics.DrawImage(copy, new Point());
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
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public override bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            this.pictureBox = targetPictureBox;

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

    /// <summary>
    /// Abstract class to be inherited by objects that decorate the picture box's visual display
    /// </summary>
    public abstract class PictureBoxDecorator
    {
        /// <summary>
        /// The reference to the picture box to decorate
        /// </summary>
        protected ImageEditPanel.InternalPictureBox pictureBox;

        /// <summary>
        /// Gets the reference to the picture box to decorate
        /// </summary>
        public ImageEditPanel.InternalPictureBox PictureBox
        {
            get { return pictureBox; }
        }

        /// <summary>
        /// Initializes a new instance of the PictureBoxDecorator class
        /// </summary>
        /// <param name="pictureBox">The picture box to decorate</param>
        public PictureBoxDecorator(ImageEditPanel.InternalPictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
        }

        /// <summary>
        /// Initializes this PictureBoxDecorator's instance
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called to notify this picture box decorator that it has been added to a picture box
        /// </summary>
        /// <param name="pictureBox">The picture box this decorator has been added to</param>
        public void AddedToPictureBox(ImageEditPanel.InternalPictureBox pictureBox)
        {
            Initialize();
        }

        /// <summary>
        /// Destroys this PictureBoxDecorator's instance
        /// </summary>
        public virtual void Destroy()
        {
            this.pictureBox = null;
        }

        /// <summary>
        /// Decorates the under image, using the given event arguments
        /// </summary>
        /// <param name="image">The under image to decorate</param>
        public virtual void DecorateUnderImage(Image image) { }

        /// <summary>
        /// Decorates the main image, using the given event arguments
        /// </summary>
        /// <param name="image">The main image to decorate</param>
        public virtual void DecorateMainImage(Image image) { }
        
        /// <summary>
        /// Decorates the front image, using the given event arguments
        /// </summary>
        /// <param name="image">The front image to decorate</param>
        public virtual void DecorateFrontImage(Image image) { }
    }
}
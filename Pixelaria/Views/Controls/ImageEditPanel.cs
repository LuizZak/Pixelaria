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
using System.Windows.Forms;
using Pixelaria.Algorithms.PaintOperations.Abstracts;
using Pixelaria.Data.Undo;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.ColorControls;
using Pixelaria.Views.Controls.PaintTools;
using Pixelaria.Views.Controls.PaintTools.Interfaces;
using Pixelaria.Views.ModelViews;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Defines a panel that is used specifically for displaying a Bitmap object and allowing the user to make changes to it
    /// </summary>
    public class ImageEditPanel : Control, IModifiable
    {
        /// <summary>
        /// The PictureBox that displays the bitmap being edited
        /// </summary>
        private readonly InternalPictureBox _internalPictureBox;

        /// <summary>
        /// The Panel that will own the InternalPictureBox
        /// </summary>
        private readonly Panel _owningPanel;

        /// <summary>
        /// The undo system that handles undoing/redoing of tasks
        /// </summary>
        private UndoSystem _undoSystem;

        /// <summary>
        /// The default compositing mode to use on paint operations that have a compositing operation
        /// </summary>
        private CompositingMode _defaultCompositingMode;

        /// <summary>
        /// The default fill mode to use on paint operations that have a fill mode
        /// </summary>
        private OperationFillMode _defaultFillMode;

        /// <summary>
        /// Whether editing the image is currently enabled on this image edit panel
        /// </summary>
        private bool _editingEnabled;

        /// <summary>
        /// Gets or sets this panel's picture box's background image
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        public Image PictureBoxBackgroundImage { get { return _internalPictureBox.BackgroundImage; } set { _internalPictureBox.BackgroundImage = value; } }

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
        public InternalPictureBox PictureBox => _internalPictureBox;

        /// <summary>
        /// Gets or sets the modifiable to notify when changes are made to the bitmap
        /// </summary>
        public IModifiable NotifyTo { get; set; }

        /// <summary>
        /// Gets or sets the current paint operation to perform on this ImageEditPanel
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public IPaintTool CurrentPaintTool
        {
            get { return _internalPictureBox.CurrentPaintTool; }
            set
            {
                if (IsDisposed) 
                    return;
                
                _internalPictureBox.CurrentPaintTool = value;

                var operation = value as IClipboardPaintTool;
                if (operation != null)
                {
                    FireClipboardStateEvent(operation.CanCopy(), operation.CanCut(), operation.CanPaste());
                }
                else
                {
                    FireClipboardStateEvent(false, false, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the undo system that handles the undo/redo tasks of this ImageEditPanel
        /// </summary>
        public UndoSystem UndoSystem { get { return _undoSystem; } set { _undoSystem = value; } }

        /// <summary>
        /// Gets or sets a value specifying whether editing the image is currently enabled on this image edit panel
        /// </summary>
        public bool EditingEnabled
        {
            get { return _editingEnabled; }
            set
            {
                _editingEnabled = value;

                if (!_editingEnabled)
                {
                    (CurrentPaintTool as IAreaOperation)?.CancelOperation(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the default compositing mode to use on paint operations that have a compositing component
        /// </summary>
        public CompositingMode DefaultCompositingMode
        {
            get { return _defaultCompositingMode; }
            set
            {
                _defaultCompositingMode = value;

                var operation = _internalPictureBox.CurrentPaintTool as ICompositingPaintTool;
                if (operation != null)
                {
                    operation.CompositingMode = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default fill mode to use on paint operations that have a fill mode component
        /// </summary>
        public OperationFillMode DefaultFillMode
        {
            get { return _defaultFillMode; }
            set
            {
                _defaultFillMode = value;
                var operation = _internalPictureBox.CurrentPaintTool as IFillModePaintTool;
                if (operation != null)
                {
                    operation.FillMode = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ImageEditPanel
        /// </summary>
        public ImageEditPanel()
        {
            EditingEnabled = true;

            // Create the controls
            SuspendLayout();

            _internalPictureBox = new InternalPictureBox(this)
            {
                MinimumZoom = new PointF(0.25f, 0.25f),
                MaximumZoom = new PointF(160, 160),
                ShowImageArea = false,
                ClipBackgroundToImage = true,
                AllowDrag = false,
                NotifyTo = this,
                Dock = DockStyle.Fill
            };

            _owningPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add controls
            _owningPanel.Controls.Add(_internalPictureBox);
            Controls.Add(_owningPanel);

            ResumeLayout(true);

            _undoSystem = new UndoSystem();

            _defaultCompositingMode = CompositingMode.SourceOver;
            _defaultFillMode = OperationFillMode.SolidFillFirstColor;
        }

        /// <summary>
        /// Initializes this ImageEditPanel instance
        /// </summary>
        public void Init()
        {
            _internalPictureBox.HookToControl(FindForm());
        }

        /// <summary>
        /// Disposes of this ImageEditPanel and all resources used by it
        /// </summary>
        public new void Dispose()
        {
            _internalPictureBox.Dispose();

            base.Dispose();
        }

        /// <summary>
        /// Loads the given Bitmap to this ImageEditPanel
        /// </summary>
        /// <param name="bitmap">The Bitmap to edit</param>
        /// <param name="clearUndoSystem">Whether to clear the undo system when changing images</param>
        public void LoadBitmap(Bitmap bitmap, bool clearUndoSystem = true)
        {
            if(clearUndoSystem)
            {
                // Clear the undo system
                _undoSystem.Clear();
            }

            _internalPictureBox.SetBitmap(bitmap);
            _internalPictureBox.Width = _owningPanel.Width;
            _internalPictureBox.Height = _owningPanel.Height;
        }

        /// <summary>
        /// Marks this ImageEditPanel as modified
        /// </summary>
        public void MarkModified()
        {
            NotifyTo?.MarkModified();
        }

        /// <summary>
        /// Fires the color change event with the given color
        /// </summary>
        /// <param name="color">The color to fire the event with</param>
        /// <param name="colorIndex">Which color index to fire the event fore. Defaults to the current color</param>
        public void FireColorChangeEvent(Color color, ColorPickerColor colorIndex = ColorPickerColor.CurrentColor)
        {
            ColorSelect?.Invoke(this, new ColorPickEventArgs(color, color, colorIndex));
        }

        /// <summary>
        /// Fires the ClipboardStateChanged event with the given parameters
        /// </summary>
        /// <param name="canCopy">Whether there's content to be copied on the object</param>
        /// <param name="canCut">Whether there's content to be cut on the object</param>
        /// <param name="canPaste">Whether there's content to be pasted on the object</param>
        public void FireClipboardStateEvent(bool canCopy, bool canCut, bool canPaste)
        {
            ClipboardStateChanged?.Invoke(this, new ClipboardStateEventArgs(canCopy, canCut, canPaste));
        }

        /// <summary>
        /// Fires the OperationStatusChanged event with the given parameters
        /// </summary>
        /// <param name="tool">The operation that fired the event</param>
        /// <param name="status">The status for the event</param>
        public void FireOperationStatusEvent(IPaintTool tool, string status)
        {
            OperationStatusChanged?.Invoke(this, new OperationStatusEventArgs(tool, status));
        }

        /// <summary>
        /// Forces the current paint tool to intercept the undo operation, returning whether the Paint Tool has intercepted the undo operation successfully.
        /// While intercepting an undo, a paint tool might perform actions of its own
        /// </summary>
        /// <returns>Whether the current paint tool intercepted the undo task. When the return is true, no undo operation might be performed</returns>
        public bool InterceptUndo()
        {
            var intercepter = CurrentPaintTool as IUndoIntercepterPaintTool;

            return intercepter != null && intercepter.InterceptUndo();
        }

        /// <summary>
        /// Forces the current paint tool to intercept the redo operation, returning whether the Paint Tool has intercepted the redo operation successfully.
        /// While intercepting a redo, a paint tool might perform actions of its own
        /// </summary>
        /// <returns>Whether the current paint tool intercepted the redo task. When the return is true, no redo operation might be performed</returns>
        public bool InterceptRedo()
        {
            var intercepter = CurrentPaintTool as IUndoIntercepterPaintTool;

            return intercepter != null && intercepter.InterceptRedo();
        }

        /// <summary>
        /// Internal picture box that actually displays the bitmap to edit
        /// </summary>
        public class InternalPictureBox : ZoomablePictureBox, IDisposable
        {
            /// <summary>
            /// The ImageEditPanel that owns this InternalPictureBox
            /// </summary>
            private readonly ImageEditPanel _owningPanel;

            /// <summary>
            /// The current paint operation
            /// </summary>
            private IPaintTool _currentPaintTool;

            /// <summary>
            /// The buffer bitmap that the paint operations will use in order to buffer screen previews
            /// </summary>
            private Bitmap _buffer;

            /// <summary>
            /// The image to display under the current image
            /// </summary>
            private Bitmap _underImage;

            /// <summary>
            /// The image to display over the current image
            /// </summary>
            private Bitmap _overImage;

            /// <summary>
            /// Whether the mouse is currently held down on this picture box
            /// </summary>
            private bool _mouseDown;

            /// <summary>
            /// Whether to display the current image
            /// </summary>
            private bool _displayImage;

            /// <summary>
            /// The coordinate of the mouse, in absolute image pixels
            /// </summary>
            private Point _mousePoint;

            /// <summary>
            /// Whether the mouse is currently over the image on the panel
            /// </summary>
            private bool _mouseOverImage;

            /// <summary>
            /// Whether to display a grid over the image
            /// </summary>
            private bool _displayGrid;

            /// <summary>
            /// Whether the space keyboard key is currently being held down
            /// </summary>
            private bool _spaceHeld;

            /// <summary>
            /// The list of picture box decorators
            /// </summary>
            private readonly List<PictureBoxDecorator> _pictureBoxDecorators;

            /// <summary>
            /// Specifies the modifiable to notify when changes are made to the bitmap
            /// </summary>
            public IModifiable NotifyTo;

            /// <summary>
            /// Event fired whenever the picture box has had its bitmap modified
            /// </summary>
            public event EventHandler Modified;

            /// <summary>
            /// Gets or sets the current paint operation for this InternalPictureBox
            /// </summary>
            public IPaintTool CurrentPaintTool { get { return _currentPaintTool; } set { if (IsDisposed) return; SetPaintOperation(value); } }

            /// <summary>
            /// Gets the ImageEditPanel that owns this InternalPictureBox
            /// </summary>
            public ImageEditPanel OwningPanel => _owningPanel;

            /// <summary>
            /// Gets the Bitmap associated with this InternalPictureBox
            /// </summary>
            public Bitmap Bitmap => Image as Bitmap;

            /// <summary>
            /// Gets the buffer bitmap that the paint operations will use in order to buffer screen previews
            /// </summary>
            public Bitmap Buffer => _buffer;

            /// <summary>
            /// Gets or sets the bitmap to display under the current image
            /// </summary>
            public Bitmap UnderImage { get { return _underImage; } set { _underImage = value; Invalidate(); } }

            /// <summary>
            /// Gets or sets the bitmap to display over the current image
            /// </summary>
            public Bitmap OverImage { get { return _overImage; } set { _overImage = value; Invalidate(); } }

            /// <summary>
            /// Gets or sets whether to display the current image
            /// </summary>
            public bool DisplayImage { get { return _displayImage; } set { if (_displayImage != value) { _displayImage = value; Invalidate(); } } }

            /// <summary>
            /// Gets the coordinate of the mouse, in absolute image pixels 
            /// </summary>
            public Point MousePoint => _mousePoint;

            /// <summary>
            /// Gets whether the mouse is currently over the image on the panel
            /// </summary>
            public bool MouseOverImage => _mouseOverImage;

            /// <summary>
            /// Gets or sets whether to display a grid over the image
            /// </summary>
            public bool DisplayGrid { get { return _displayGrid; } set { _displayGrid = value; Invalidate(); } }

            /// <summary>
            /// Gets a value specifying whether editing is currently enabled on this PictureBox
            /// </summary>
            public bool EditingEnabled => _owningPanel.EditingEnabled;

            /// <summary>
            /// Gets a value specifying whether the space keyboard key is currently being held down
            /// </summary>
            public bool SpaceHeld => _spaceHeld;

            /// <summary>
            /// Specifies the delegate signature for custom interceptable mouse events of this panel
            /// </summary>
            public delegate void InternalPictureBoxMouseEvent(object sender, InternalPictureBoxMouseEventArgs eventArgs);

            /// <summary>
            /// An event for mouse down that may be interceptable by a listener
            /// </summary>
            public event InternalPictureBoxMouseEvent InterceptableMouseDown;

            /// <summary>
            /// Initializes a new instance of the InternalPictureBox class
            /// </summary>
            /// <param name="owningPanel">The ImageEditPanel that will own this InternalPictureBox</param>
            public InternalPictureBox(ImageEditPanel owningPanel)
            {
                _owningPanel = owningPanel;

                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                ZoomFactor = 2;
                _displayImage = true;
                _displayGrid = false;
                _mousePoint = new Point();
                _mouseOverImage = false;
                _pictureBoxDecorators = new List<PictureBoxDecorator>();

                SetPaintOperation(new PencilPaintTool());
            }

            /// <summary>
            /// Disposes of this InternalPictureBox and all used resources
            /// </summary>
            public new void Dispose()
            {
                _currentPaintTool?.Destroy();

                foreach(PictureBoxDecorator decorator in _pictureBoxDecorators)
                {
                    decorator.Destroy();
                }

                // Create the under and over images
                if (_overImage != null)
                {
                    _overImage.Dispose();
                    _overImage = null;
                }
                if (_underImage != null)
                {
                    _underImage.Dispose();
                    _underImage = null;
                }

                _pictureBoxDecorators.Clear();

                _buffer.Dispose();

                base.Dispose();
            }

            /// <summary>
            /// Sets the bitmap being edited
            /// </summary>
            /// <param name="bitmap">The bitmap to edit</param>
            public void SetBitmap(Bitmap bitmap)
            {
                Image = bitmap;

                _buffer?.Dispose();

                _buffer = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

                // Create the under and over images
                _overImage?.Dispose();
                _underImage?.Dispose();

                _overImage = new Bitmap(_buffer.Width, _buffer.Height, PixelFormat.Format32bppArgb);
                _underImage = new Bitmap(_buffer.Width, _buffer.Height, PixelFormat.Format32bppArgb);

                if (_currentPaintTool != null)
                {
                    // Initialize the paint operation
                    if (!_currentPaintTool.Loaded)
                    {
                        _currentPaintTool.Initialize(this);

                        Cursor = _currentPaintTool.ToolCursor;
                    }

                    _currentPaintTool.ChangeBitmap(bitmap);
                }
            }

            /// <summary>
            /// Sets the current paint operation of this InternalPictureBox to be of the given type
            /// </summary>
            /// <param name="newPaintTool"></param>
            public void SetPaintOperation(IPaintTool newPaintTool)
            {
                if (_currentPaintTool != null)
                {
                    _owningPanel.FireOperationStatusEvent(_currentPaintTool, "");

                    _currentPaintTool.Destroy();
                }

                _currentPaintTool = newPaintTool;

                if (Image != null)
                {
                    _currentPaintTool.Initialize(this);

                    if (!_mouseOverImage)
                    {
                        _currentPaintTool.MouseLeave(new EventArgs());
                    }

                    Cursor = _currentPaintTool.ToolCursor;
                }

                var operation = _currentPaintTool as ICompositingPaintTool;
                if (operation != null)
                {
                    operation.CompositingMode = _owningPanel._defaultCompositingMode;
                }
                if (_currentPaintTool is IFillModePaintTool)
                {
                    (_currentPaintTool as IFillModePaintTool).FillMode = _owningPanel._defaultFillMode;
                }
            }

            /// <summary>
            /// Marks this InternalPictureBox as modified
            /// </summary>
            public void MarkModified()
            {
                NotifyTo?.MarkModified();

                NotifyBitmapModified();
            }

            /// <summary>
            /// Notifies the picture box that the underlying bitmap was modified
            /// </summary>
            public void NotifyBitmapModified()
            {
                Modified?.Invoke(this, new EventArgs());
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
                _pictureBoxDecorators.Add(decorator);
                decorator.AddedToPictureBox(this);
                Invalidate();
            }

            /// <summary>
            /// Removes a decorator from this picture box
            /// </summary>
            /// <param name="decorator">The decorator to remove from this picture box</param>
            public void RemoveDecorator(PictureBoxDecorator decorator)
            {
                _pictureBoxDecorators.Remove(decorator);
                Invalidate();
            }

            /// <summary>
            /// Removes a decorator from this picture box
            /// </summary>
            public void ClearDecorators()
            {
                _pictureBoxDecorators.Clear();
                Invalidate();
            }

            // 
            // OnPaint event handler
            // 
            protected override void OnPaint(PaintEventArgs pe)
            {
                // Draw the under image
                if (_underImage != null)
                {
                    pe.Graphics.TranslateTransform(-offsetPoint.X, -offsetPoint.Y);
                    pe.Graphics.ScaleTransform(scale.X, scale.Y);

                    pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    pe.Graphics.InterpolationMode = ImageInterpolationMode;

                    // Apply the decorators
                    Bitmap copy = _underImage;

                    if (_pictureBoxDecorators.Count > 0)
                    {
                        copy = _underImage.Clone(new Rectangle(0, 0, _underImage.Width, _underImage.Height), _underImage.PixelFormat);

                        foreach (PictureBoxDecorator decorator in _pictureBoxDecorators)
                        {
                            decorator.DecorateUnderBitmap(copy);
                        }
                    }

                    pe.Graphics.DrawImage(copy, 0, 0);

                    pe.Graphics.ResetTransform();
                }

                if (Image != null)
                {
                    UpdateGraphicsTransform(pe.Graphics);

                    // Reset the buffer back to the original input bitmap state
                    FastBitmap.CopyPixels(Bitmap, _buffer);

                    // Clip to the image's boundaries
                    pe.Graphics.IntersectClip(new RectangleF(0, 0, Image.Width, Image.Height));
                    Region clip = pe.Graphics.Clip;

                    // Render the current paint tool
                    if (EditingEnabled)
                    {
                        _currentPaintTool.Paint(pe);
                    }

                    // Draw the actual image
                    if (_displayImage)
                    {
                        foreach (PictureBoxDecorator decorator in _pictureBoxDecorators)
                        {
                            decorator.DecorateMainBitmap(_buffer);
                        }

                        // Draw the buffer now
                        pe.Graphics.DrawImage(_buffer, 0, 0);
                    }

                    // Draw the over image
                    if (_overImage != null)
                    {
                        // Apply the decorators
                        Bitmap copy = _overImage;

                        if (_pictureBoxDecorators.Count > 0)
                        {
                            copy = (_overImage.Clone(new Rectangle(0, 0, _overImage.Width, _overImage.Height), _overImage.PixelFormat));

                            foreach (PictureBoxDecorator decorator in _pictureBoxDecorators)
                            {
                                decorator.DecorateOverBitmap(copy);
                            }
                        }

                        pe.Graphics.DrawImage(copy, Point.Empty);
                    }

                    // Reset the clipping and draw the grid
                    if (_displayGrid && scale.X > 4 && scale.Y > 4)
                    {
                        pe.Graphics.Clip = clip;
                        pe.Graphics.ResetTransform();

                        Pen pen = Pens.Gray;

                        float xOff = (-offsetPoint.X) % scale.X;
                        float yOff = (-offsetPoint.Y) % scale.Y;

                        // Draw the horizontal lines
                        for (float y = yOff; y < Math.Min(Height, (Image.Height * scale.Y)); y += scale.Y)
                        {
                            pe.Graphics.DrawLine(pen, 0, y, (int)(Image.Width * scale.X), y);
                        }

                        // Draw the vertical lines
                        for (float x = xOff; x < Math.Min(Width, (Image.Width * scale.X)); x += scale.X)
                        {
                            pe.Graphics.DrawLine(pen, x, 0, x, (int)(Image.Height * scale.Y));
                        }
                    }
                }
                else
                {
                    // Draw the over image
                    if (_overImage == null)
                        return;

                    Bitmap copy = _overImage;

                    if (_pictureBoxDecorators.Count > 0)
                    {
                        copy = _overImage.Clone(new Rectangle(0, 0, _overImage.Width, _overImage.Height), _overImage.PixelFormat);

                        foreach (PictureBoxDecorator decorator in _pictureBoxDecorators)
                        {
                            decorator.DecorateUnderBitmap(copy);
                        }
                    }

                    pe.Graphics.DrawImage(copy, new Point());
                }

                // Paint the current paint tool's foreground
                if (EditingEnabled)
                {
                    pe.Graphics.ResetTransform();

                    UpdateGraphicsTransform(pe.Graphics);

                    _currentPaintTool.PaintForeground(pe);
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

                var location = GetAbsolutePoint(e.Location);

                var args = new InternalPictureBoxMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, location.X, location.Y, e.Delta);
                InterceptableMouseDown?.Invoke(this, args);

                // Event was handled
                if (args.Handled)
                    return;

                var findForm = FindForm();
                if (findForm != null)
                    findForm.ActiveControl = this;

                if (EditingEnabled && !AllowDrag)
                {
                    if (Image != null)
                        _currentPaintTool.MouseDown(e);
                }

                _mouseDown = true;
            }

            // 
            // Mouse Move event handler
            // 
            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (Image != null)
                {
                    if (EditingEnabled)
                        _currentPaintTool.MouseMove(e);

                    _mousePoint = GetAbsolutePoint(e.Location);

                    _mouseOverImage = _mousePoint.X >= 0 && _mousePoint.Y >= 0 && _mousePoint.X < Image.Width && _mousePoint.Y < Image.Height;
                }
            }

            // 
            // Mouse Up event handler
            // 
            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (_mouseDown && EditingEnabled && Image != null)
                    _currentPaintTool.MouseUp(e);

                _mouseDown = false;
            }

            // 
            // Mouse Leave event handler
            // 
            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);

                _mouseOverImage = false;

                if (Image != null)
                    _currentPaintTool.MouseLeave(e);
            }

            // 
            // Mouse Enter event handler
            // 
            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);

                if (Image != null)
                    _currentPaintTool.MouseEnter(e);
            }

            // 
            // Key Down event handler
            // 
            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (EditingEnabled && Image != null)
                    _currentPaintTool.KeyDown(e);

                if (e.KeyCode == Keys.Space && !_spaceHeld)
                {
                    AllowDrag = true;
                    _spaceHeld = true;

                    if (!mouseDown)
                    {
                        Invalidate();
                    }
                }
            }

            // 
            // Key Up event handler
            // 
            protected override void OnKeyUp(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (EditingEnabled && Image != null)
                    _currentPaintTool.KeyUp(e);

                if (e.KeyCode == Keys.Space)
                {
                    AllowDrag = false;
                    _spaceHeld = false;

                    if (!mouseDown)
                    {
                        Invalidate();
                    }
                }
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
            CanCopy = canCopy;
            CanCut = canCut;
            CanPaste = canPaste;
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
        public IPaintTool Tool { get; private set; }

        /// <summary>
        /// Creates a new instance of the OperationStatusChange event
        /// </summary>
        /// <param name="tool">The operation that fired the status change</param>
        /// <param name="status">The operation status</param>
        public OperationStatusEventArgs(IPaintTool tool, string status)
        {
            Tool = tool;
            Status = status;
        }
    }

    /// <summary>
    /// Describes an undo task capable of undoing changes made to a bitmap
    /// </summary>
    public class BitmapUndoTask : BasicPaintOperationUndoTask
    {
        /// <summary>
        /// The bitmap that will be the target for the changes
        /// </summary>
        readonly Bitmap _targetBitmap;

        /// <summary>
        /// The bitmap that contains the pixels for the undoing of the task
        /// </summary>
        Bitmap _oldBitmap;

        /// <summary>
        /// The bitmap that contains the pixels for the redoing of the task
        /// </summary>
        Bitmap _newBitmap;

        /// <summary>
        /// The point at which to draw the bitmaps when undoing/redoing the operation
        /// </summary>
        Point _drawPoint;

        /// <summary>
        /// The string description for this BitmapUndoTask
        /// </summary>
        readonly string _description;

        /// <summary>
        /// Gets or sets the point at which to draw the bitmaps when undoing/redoing the operation
        /// </summary>
        public Point DrawPoint
        {
            get { return _drawPoint; }
            set { _drawPoint = value; }
        }

        /// <summary>
        /// Initializes a new instance of the BitmapUndoTask, with a target picture box and bitmap
        /// </summary>
        /// <param name="targetBitmap">The target bitmap for this BitmapUndoTask</param>
        /// <param name="description">A short description for this BitmapUndoTask</param>
        /// <param name="drawPoint">The point at which to draw the bitmaps when undoing/redoing</param>
        public BitmapUndoTask(Bitmap targetBitmap, string description, Point drawPoint = new Point())
            : base(targetBitmap)
        {
            _targetBitmap = targetBitmap;
            _description = description;
            _drawPoint = drawPoint;
            _oldBitmap = targetBitmap.Clone() as Bitmap;
        }

        /// <summary>
        /// Registers the pixels of the given bitmap as the undo bitmap
        /// </summary>
        /// <param name="oldBitmap">The bitmap whose pixels will be used as the undo bitmap</param>
        /// <param name="cloneBitmap">Whether to clone the bitmap instead of only assigning it</param>
        public void SetOldBitmap(Bitmap oldBitmap, bool cloneBitmap = true)
        {
            if (_oldBitmap != null && cloneBitmap)
                _oldBitmap.Dispose();

            if(cloneBitmap)
            {
                _oldBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height, PixelFormat.Format32bppArgb);
                FastBitmap.CopyPixels(oldBitmap, _oldBitmap);
            }
            else
            {
                _oldBitmap = oldBitmap;
            }
        }

        /// <summary>
        /// Registers the pixels of the given bitmap as the redo bitmap
        /// </summary>
        /// <param name="newBitmap">The bitmap whose pixels will be used as the redo bitmap</param>
        /// <param name="cloneBitmap">Whether to clone the bitmap instead of only assigning it</param>
        public void SetNewBitmap(Bitmap newBitmap, bool cloneBitmap = true)
        {
            if (_newBitmap != null && cloneBitmap)
                _newBitmap.Dispose();

            if(cloneBitmap)
            {
                _newBitmap = new Bitmap(newBitmap.Width, newBitmap.Height, PixelFormat.Format32bppArgb);
                FastBitmap.CopyPixels(newBitmap, _newBitmap);
            }
            else
            {
                _newBitmap = newBitmap;
            }
        }

        /// <summary>
        /// Clears this UndoTask object
        /// </summary>
        public override void Clear()
        {
            _oldBitmap?.Dispose();

            _newBitmap?.Dispose();
        }

        /// <summary>
        /// Undoes this task
        /// </summary>
        public override void Undo()
        {
            FastBitmap.CopyRegion(_oldBitmap, _targetBitmap,
                new Rectangle(0, 0, _oldBitmap.Width, _oldBitmap.Height),
                new Rectangle(_drawPoint, new Size(_oldBitmap.Width, _oldBitmap.Height)));
        }

        /// <summary>
        /// Redoes this task
        /// </summary>
        public override void Redo()
        {
            FastBitmap.CopyRegion(_newBitmap, _targetBitmap,
                new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height),
                new Rectangle(_drawPoint, new Size(_targetBitmap.Width, _targetBitmap.Height)));
        }

        /// <summary>
        /// Returns a short string description of this UndoTask
        /// </summary>
        /// <returns>A short string description of this UndoTask</returns>
        public override string GetDescription()
        {
            return _description;
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
        public ImageEditPanel.InternalPictureBox PictureBox => pictureBox;

        /// <summary>
        /// Initializes a new instance of the PictureBoxDecorator class
        /// </summary>
        /// <param name="pictureBox">The picture box to decorate</param>
        protected PictureBoxDecorator(ImageEditPanel.InternalPictureBox pictureBox)
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
        /// <param name="pictBox">The picture box this decorator has been added to</param>
        public void AddedToPictureBox(ImageEditPanel.InternalPictureBox pictBox)
        {
            Initialize();
        }

        /// <summary>
        /// Destroys this PictureBoxDecorator's instance
        /// </summary>
        public virtual void Destroy()
        {
            pictureBox = null;
        }

        /// <summary>
        /// Decorates the under image, using the given event arguments
        /// </summary>
        public virtual void DecorateUnderBitmap(Bitmap bitmap) { }

        /// <summary>
        /// Decorates the main image, using the given event arguments
        /// </summary>
        public virtual void DecorateMainBitmap(Bitmap bitmap) { }

        /// <summary>
        /// Decorates the front image, using the given event arguments
        /// </summary>
        public virtual void DecorateOverBitmap(Bitmap bitmap) { }
    }

    /// <summary>
    /// A mouse event fired by the internal picture box of an image edit panel.
    /// This event allows listeners to intercept and handle mouse events of an internal picture box
    /// </summary>
    public class InternalPictureBoxMouseEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Whether this event was properly handled by the event listener
        /// </summary>
        public bool Handled;

        /// <summary>
        /// The x coordinate of this point, on image coordinates
        /// </summary>
        public int ImageX;

        /// <summary>
        /// The Y coordinate of this point, on image coordinates
        /// </summary>
        public int ImageY;

        /// <summary>
        /// Gets the point of this event, on absolute image coordinates
        /// </summary>
        public Point ImageLocation => new Point(ImageX, ImageY);

        public InternalPictureBoxMouseEventArgs(MouseButtons button, int clicks, int x, int y, int mouseX, int mouseY, int delta) : base(button, clicks, x, y, delta)
        {
            ImageX = mouseX;
            ImageY = mouseY;
        }
    }
}
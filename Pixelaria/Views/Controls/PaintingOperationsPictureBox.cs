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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using FastBitmapLib;
using JetBrains.Annotations;
using Pixelaria.Views.Controls.PaintTools;
using Pixelaria.Views.Controls.PaintTools.Interfaces;
using Pixelaria.Views.ModelViews;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// A picture box with extra support for delegating UI interaction events to paint tool operations.
    /// 
    /// Used primarily by <see cref="FrameView"/> form.
    /// </summary>
    public class PaintingOperationsPictureBox : ZoomablePictureBox
    {
        /// <summary>
        /// The current paint operation
        /// </summary>
        private IPaintingPictureBoxTool _currentPaintTool;

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
        /// Whether to display a grid over the image
        /// </summary>
        private bool _displayGrid;

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
        /// Gets or sets the pan mode for this picture box.
        /// 
        /// Defaults to <see cref="PictureBoxPanMode.SpaceKeyDrag"/>
        /// </summary>
        [Browsable(true)]
        [Description("Pan mode strategy for this picture box that specifies how the user may drag the view when zoomed in enough to display scroll bars.")]
        [DefaultValue(PictureBoxPanMode.SpaceKeyDrag)]
        public PictureBoxPanMode PanMode { get; set; } = PictureBoxPanMode.SpaceKeyDrag;

        /// <summary>
        /// Gets or sets the current paint operation for this <see cref="PaintingOperationsPictureBox"/>
        /// </summary>
        [Browsable(false)]
        internal IPaintingPictureBoxTool CurrentPaintTool { get => _currentPaintTool; set { if (IsDisposed) return; SetPaintTool(value); } }

        /// <summary>
        /// Gets the <see cref="ImageEditPanel"/> that owns this <see cref="PaintingOperationsPictureBox"/>
        /// </summary>
        [Browsable(false)]
        public ImageEditPanel OwningPanel { get; }
        
        /// <summary>
        /// Gets the Bitmap associated with this <see cref="PaintingOperationsPictureBox"/>
        /// </summary>
        [CanBeNull]
        public Bitmap Bitmap => Image as Bitmap;

        /// <summary>
        /// Gets the buffer bitmap that the paint operations will use in order to buffer screen previews
        /// </summary>
        [Browsable(false)]
        public Bitmap Buffer { get; private set; }

        /// <summary>
        /// Gets or sets the bitmap to display under the current image
        /// </summary>
        public Bitmap UnderImage { get => _underImage; set { _underImage = value; Invalidate(); } }

        /// <summary>
        /// Gets or sets the bitmap to display over the current image
        /// </summary>
        public Bitmap OverImage { get => _overImage; set { _overImage = value; Invalidate(); } }

        /// <summary>
        /// Gets or sets whether to display the current image
        /// </summary>
        public bool DisplayImage { get => _displayImage; set { if (_displayImage != value) { _displayImage = value; Invalidate(); } } }

        /// <summary>
        /// Gets the coordinate of the mouse, in absolute image pixels 
        /// </summary>
        public Point MousePoint { get; private set; }

        /// <summary>
        /// Gets whether the mouse is currently over the image on the panel
        /// </summary>
        public bool MouseOverImage { get; private set; }

        /// <summary>
        /// Gets or sets whether to display a grid over the image
        /// </summary>
        public bool DisplayGrid { get => _displayGrid; set { _displayGrid = value; Invalidate(); } }

        /// <summary>
        /// Gets a value specifying whether editing is currently enabled on this PictureBox
        /// </summary>
        [Browsable(false)]
        public bool EditingEnabled => OwningPanel.EditingEnabled;

        /// <summary>
        /// Gets a value specifying whether the space keyboard key is currently being held down
        /// </summary>
        [Browsable(false)]
        public bool SpaceHeld { get; private set; }

        /// <summary>
        /// Specifies the delegate signature for custom interceptable mouse events of this panel
        /// </summary>
        public delegate void PaintingOperationsPictureBoxMouseEvent(object sender, PaintingOperationsPictureBoxMouseEventArgs e);

        /// <summary>
        /// An event for mouse down that may be interceptable by a listener
        /// </summary>
        public event PaintingOperationsPictureBoxMouseEvent InterceptableMouseDown;

        /// <summary>
        /// Initializes a new instance of the InternalPictureBox class
        /// </summary>
        /// <param name="owningPanel">The ImageEditPanel that will own this InternalPictureBox</param>
        public PaintingOperationsPictureBox(ImageEditPanel owningPanel)
        {
            OwningPanel = owningPanel;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ZoomFactor = 2;
            _displayImage = true;
            _displayGrid = false;
            MousePoint = new Point();
            MouseOverImage = false;
            _pictureBoxDecorators = new List<PictureBoxDecorator>();

            SetPaintTool(new PencilPaintTool());
        }
            
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            _currentPaintTool?.Dispose();

            foreach (var decorator in _pictureBoxDecorators)
            {
                decorator.Dispose();
            }
            _pictureBoxDecorators.Clear();

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

            if (Buffer != null)
                Buffer.Dispose();
        }

        /// <summary>
        /// Sets the bitmap being edited
        /// </summary>
        /// <param name="bitmap">The bitmap to edit</param>
        public void SetBitmap([NotNull] Bitmap bitmap)
        {
            Image = bitmap;

            Buffer?.Dispose();

            Buffer = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

            // Create the under and over images
            _overImage?.Dispose();
            _underImage?.Dispose();

            _overImage = new Bitmap(Buffer.Width, Buffer.Height, PixelFormat.Format32bppArgb);
            _underImage = new Bitmap(Buffer.Width, Buffer.Height, PixelFormat.Format32bppArgb);

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
        /// Sets the current paint operation of this <see cref="PaintingOperationsPictureBox"/> to be of the given type
        /// </summary>
        internal void SetPaintTool(IPaintingPictureBoxTool newPaintTool)
        {
            if (_currentPaintTool != null)
            {
                OwningPanel.FireOperationStatusEvent(_currentPaintTool, "");

                _currentPaintTool.Dispose();
            }

            _currentPaintTool = newPaintTool;

            if (Image != null)
            {
                _currentPaintTool.Initialize(this);

                if (!MouseOverImage)
                {
                    _currentPaintTool.MouseLeave(EventArgs.Empty);
                }

                Cursor = _currentPaintTool.ToolCursor;
            }

            if (_currentPaintTool is ICompositingPaintTool operation)
            {
                operation.CompositingMode = OwningPanel.DefaultCompositingMode;
            }
            if (_currentPaintTool is IFillModePaintTool tool)
            {
                tool.FillMode = OwningPanel.DefaultFillMode;
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
            Modified?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Paints the background of this PictureBox using the given PaintEventArgs
        /// </summary>
        /// <param name="pe">The PaintEventArgs to use on the paint background event</param>
        public void PaintBackground([NotNull] PaintEventArgs pe)
        {
            OnPaintBackground(pe);
        }

        /// <summary>
        /// Adds a decorator to this picture box
        /// </summary>
        /// <param name="decorator">The decorator to add to this picture box</param>
        public void AddDecorator([NotNull] PictureBoxDecorator decorator)
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
                var copy = _underImage;

                if (_pictureBoxDecorators.Count > 0)
                {
                    copy = _underImage.Clone(new Rectangle(0, 0, _underImage.Width, _underImage.Height), _underImage.PixelFormat);

                    foreach (var decorator in _pictureBoxDecorators)
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
                FastBitmap.CopyPixels(Bitmap, Buffer);

                // Clip to the image's boundaries
                pe.Graphics.IntersectClip(new RectangleF(0, 0, Image.Width, Image.Height));
                var clip = pe.Graphics.Clip;

                // Render the current paint tool
                if (EditingEnabled)
                {
                    _currentPaintTool.Paint(pe);
                }

                // Draw the actual image
                if (_displayImage)
                {
                    foreach (var decorator in _pictureBoxDecorators)
                    {
                        decorator.DecorateMainBitmap(Buffer);
                    }

                    // Draw the buffer now
                    pe.Graphics.DrawImage(Buffer, 0, 0);
                }

                // Draw the over image
                if (_overImage != null)
                {
                    // Apply the decorators
                    var copy = _overImage;

                    if (_pictureBoxDecorators.Count > 0)
                    {
                        copy = _overImage.Clone(new Rectangle(0, 0, _overImage.Width, _overImage.Height), _overImage.PixelFormat);

                        foreach (var decorator in _pictureBoxDecorators)
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

                    var pen = Pens.Gray;

                    float xOff = -offsetPoint.X % scale.X;
                    float yOff = -offsetPoint.Y % scale.Y;

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

                var copy = _overImage;

                if (_pictureBoxDecorators.Count > 0)
                {
                    copy = _overImage.Clone(new Rectangle(0, 0, _overImage.Width, _overImage.Height), _overImage.PixelFormat);

                    foreach (var decorator in _pictureBoxDecorators)
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
            // Don't call base.OnMouseClick(e) to catch the mouse click so the middle mouse button doesn't reset the zoom

            if (Image != null && EditingEnabled)
                _currentPaintTool.MouseClick(e);
        }

        // 
        // Mouse Down event handler
        // 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (PanMode == PictureBoxPanMode.MiddleMouseDrag && e.Button == MouseButtons.Middle ||
                PanMode == PictureBoxPanMode.LeftMouseDrag && e.Button == MouseButtons.Left)
                AllowDrag = true;

            base.OnMouseDown(e);

            var location = GetAbsolutePoint(e.Location);

            var args = new PaintingOperationsPictureBoxMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, location.X, location.Y, e.Delta);
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

                MousePoint = GetAbsolutePoint(e.Location);

                MouseOverImage = MousePoint.X >= 0 && MousePoint.Y >= 0 && MousePoint.X < Image.Width && MousePoint.Y < Image.Height;
            }
        }

        // 
        // Mouse Up event handler
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (PanMode == PictureBoxPanMode.MiddleMouseDrag && e.Button == MouseButtons.Middle)
                AllowDrag = false;

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

            MouseOverImage = false;

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

            if (PanMode == PictureBoxPanMode.SpaceKeyDrag && (e.KeyCode == Keys.Space && !SpaceHeld))
            {
                AllowDrag = true;
                SpaceHeld = true;

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
            
            if (PanMode == PictureBoxPanMode.SpaceKeyDrag && e.KeyCode == Keys.Space)
            {
                AllowDrag = false;
                SpaceHeld = false;

                if (!mouseDown)
                {
                    Invalidate();
                }
            }
        }
    }

    /// <summary>
    /// A mouse event fired by the internal picture box of an image edit panel.
    /// This event allows listeners to intercept and handle mouse events of an internal picture box
    /// </summary>
    public class PaintingOperationsPictureBoxMouseEventArgs : MouseEventArgs
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

        public PaintingOperationsPictureBoxMouseEventArgs(MouseButtons button, int clicks, int x, int y, int mouseX, int mouseY, int delta) : base(button, clicks, x, y, delta)
        {
            ImageX = mouseX;
            ImageY = mouseY;
        }
    }

    /// <summary>
    /// Specifies the pan mode for a <see cref="PaintingOperationsPictureBox"/>.
    /// </summary>
    public enum PictureBoxPanMode
    {
        /// <summary>
        /// No panning action is provided.
        /// </summary>
        None,
        /// <summary>
        /// Specifies the panning happens when the left mouse is pressed down.
        /// 
        /// Disables paint tools mouse operations.
        /// </summary>
        LeftMouseDrag,
        /// <summary>
        /// Panning happens when the user drags the picture box while holding
        /// down the space key.
        /// </summary>
        SpaceKeyDrag,
        /// <summary>
        /// Panning happens when the user drags the picture box while holding
        /// down the middle mouse button.
        /// </summary>
        MiddleMouseDrag
    }
}
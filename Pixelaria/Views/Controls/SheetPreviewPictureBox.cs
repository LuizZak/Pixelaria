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
using FastBitmapLib;
using JetBrains.Annotations;
using Pixelaria.Controllers.Importers;
using Pixelaria.Data.Exports;
using Pixelaria.Views.Controls.PaintTools;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// ZoomablePictureBox implementation used to preview the sheet settings to the user
    /// </summary>
    public class SheetPreviewPictureBox : ZoomablePictureBox
    {
        /// <summary>
        /// Timer used to animate the selection area
        /// </summary>
        private readonly Timer _animTimer;

        /// <summary>
        /// The dash offset to use when drawing the selection area
        /// </summary>
        private float _dashOffset;

        /// <summary>
        /// The array of images used to represent the pixel digits
        /// </summary>
        private static Image[] _pixelDigitsImages;

        /// <summary>
        /// The bitmap sheet containing the boundaries for the frame rectangles
        /// </summary>
        private Bitmap _frameRectSheet;

        /// <summary>
        /// The current Sheet Settings this SheetPreviewPictureBox is displaying
        /// </summary>
        private SheetSettings _sheetSettings;

        /// <summary>
        /// The curre sheet export this SheetPreviewPictureBox is displaying
        /// </summary>
        private BundleSheetExport _sheetExport;

        /// <summary>
        /// The array of rectangle areas for each frame
        /// </summary>
        private Rectangle[] _frameRects;

        /// <summary>
        /// Whether to display the number of frames that have been reused when drawing the frame bounds
        /// </summary>
        private bool _displayReusedCount;

        /// <summary>
        /// Current mouse location on this preview picture box
        /// </summary>
        private Point _mouseLocation;

        /// <summary>
        /// Whether the mouse is currently over the control
        /// </summary>
        private bool _mouseOver;

        /// <summary>
        /// Whether the mouse is currently down on this control
        /// </summary>
        private bool _mouseDown;
        
        /// <summary>
        /// Whether this picture box should highlight frame rectangles under the mouse with a yellow outline
        /// </summary>
        private bool _allowMouseHover;
        
        /// <summary>
        /// Rectangle index the mouse is currently hovering over.
        /// Used to control redrawing of screen
        /// </summary>
        private int _mouseOverRectangleIndex;

        /// <summary>
        /// Delegate for the SheetPreviewPictureBoxClicked event
        /// </summary>
        public delegate void FrameBoundsBoxClicked(object sender, SheetPreviewFrameBoundsClickEventArgs e);

        /// <summary>
        /// Occurs whenever the user clicks with any mouse button over a frame rectangle
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user clicks with any mouse button over a frame rectangle")]
        public event FrameBoundsBoxClicked FrameBoundsMouseClicked;

        /// <summary>
        /// Gets or sets the IDefaultImporter to use when generating the sheet rectangles
        /// </summary>
        public IDefaultImporter Importer { get; set; }

        /// <summary>
        /// Gets or sets the current Sheet Settings this SheetPreviewPictureBox is displaying
        /// </summary>
        public SheetSettings SheetSettings
        {
            get => _sheetSettings;
            set
            {
                _sheetSettings = value;
                _frameRects = Importer.GenerateFrameBounds(Image, _sheetSettings);
                RefreshFrameBoundsPreview();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the current Sheet Export this SheetPreviewPictureBox is displaying
        /// </summary>
        public BundleSheetExport SheetExport
        {
            get => _sheetExport;
            set
            {
                _sheetExport = value;
                RefreshFrameBoundsPreview();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether to display the number of frames that have been reused when drawing the frame bounds
        /// </summary>
        [DefaultValue(false)]
        public bool DisplayReusedCount
        {
            get => _displayReusedCount;
            set
            {
                _displayReusedCount = value;
                RefreshFrameBoundsPreview();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether this picture box should highlight frame rectangles under the mouse with a yellow outline
        /// </summary>
        [Category("Appearance")]
        [Browsable(true)]
        [DefaultValue(false)]
        [Description("Sets whether the control highlights individual frame rectangles as the user hovers over them with the mouse")]
        public bool AllowMouseHover
        {
            get => _allowMouseHover;
            set
            {
                _mouseLocation = PointToClient(MousePosition);
                _allowMouseHover = value;
                Invalidate();
            }
        }
        
        /// <summary>
        /// Static constructor for the SheetPreviewPictureBox class
        /// </summary>
        static SheetPreviewPictureBox()
        {
            LoadPixelDigitImages();
        }

        /// <summary>
        /// Loads the pixel digit images
        /// </summary>
        private static void LoadPixelDigitImages()
        {
            _pixelDigitsImages = new Image[10];

            _pixelDigitsImages[0] = Properties.Resources.Numbers_0;
            _pixelDigitsImages[1] = Properties.Resources.Numbers_1;
            _pixelDigitsImages[2] = Properties.Resources.Numbers_2;
            _pixelDigitsImages[3] = Properties.Resources.Numbers_3;
            _pixelDigitsImages[4] = Properties.Resources.Numbers_4;
            _pixelDigitsImages[5] = Properties.Resources.Numbers_5;
            _pixelDigitsImages[6] = Properties.Resources.Numbers_6;
            _pixelDigitsImages[7] = Properties.Resources.Numbers_7;
            _pixelDigitsImages[8] = Properties.Resources.Numbers_8;
            _pixelDigitsImages[9] = Properties.Resources.Numbers_9;
        }

        /// <summary>
        /// Initializes a new instance of the SheetPreviewPictureBox
        /// </summary>
        public SheetPreviewPictureBox()
        {
            _animTimer = new Timer { Interval = 150 };
            _animTimer.Tick += animTimer_Tick;
            _animTimer.Start();
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            // ReSharper disable once UseNullPropagation
            if (_frameRectSheet != null)
                _frameRectSheet.Dispose();

            _animTimer.Stop();
            _animTimer.Dispose();
        }

        /// <summary>
        /// Loads an image import preview
        /// </summary>
        /// <param name="previewImage">The image to use as preview</param>
        /// <param name="sheetSettings">The sheet settings</param>
        public void LoadPreview(Image previewImage, SheetSettings sheetSettings)
        {
            UnloadExportSheet();

            // Reset the transformations
            scale = new PointF(1, 1);
            offsetPoint = Point.Empty;

            Image = previewImage;
            _sheetSettings = sheetSettings;

            _frameRects = Importer.GenerateFrameBounds(Image, sheetSettings);
            RefreshFrameBoundsPreview();

            Invalidate();

            UpdateScrollbars();
        }

        /// <summary>
        /// Loads a bundle sheet export preview
        /// </summary>
        /// <param name="bundleSheetExport">The bundle sheet export containing data about the exported image</param>
        public void LoadExportSheet([NotNull] BundleSheetExport bundleSheetExport)
        {
            UnloadExportSheet();

            // Reset the transformations
            Image = bundleSheetExport.Sheet;
            _sheetExport = bundleSheetExport;

            RefreshFrameBoundsPreview();

            Invalidate();

            UpdateScrollbars();
        }

        /// <summary>
        /// Unloads the currently displayed export sheet preview
        /// </summary>
        public void UnloadExportSheet()
        {
            _sheetExport = null;
            _frameRects = null;

            RefreshFrameBoundsPreview();

            Invalidate();
        }

        /// <summary>
        /// Refreshes the frame bounds preview image
        /// </summary>
        private void RefreshFrameBoundsPreview()
        {
            _frameRectSheet?.Dispose();

            if (Image == null)
                return;

            _frameRectSheet = new Bitmap(Image.Width, Image.Height);

            // Lay the frame rectangles on top of the image
            Rectangle[] rects = null;

            if (_sheetExport != null)
            {
                rects = _sheetExport.Atlas.UniqueBounds;
            }
            else if (Importer != null && _frameRects != null)
            {
                rects = _frameRects;
            }

            if (rects == null) return;
            
            using(var fast = _frameRectSheet.FastLock())
            {
                // Draw the frame bounds now
                uint color = unchecked((uint)Color.Red.ToArgb());
                foreach (var fRect in rects)
                {
                    // Draw the rectangle using a fast bitmap for quick pixel modification
                    int l = fRect.Left, r = fRect.Right, t = fRect.Top, b = fRect.Bottom;

                    // Top and bottom lines
                    for (int x = l; x < r; x++)
                    {
                        fast.SetPixel(x, t, color);
                        fast.SetPixel(x, b - 1, color);
                    }

                    // Left and right lines
                    for (int y = t; y < b; y++)
                    {
                        fast.SetPixel(l, y, color);
                        fast.SetPixel(r - 1, y, color);
                    }
                }
            }

            if (!_displayReusedCount)
                return;
            
            using (var g = Graphics.FromImage(_frameRectSheet))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighSpeed;

                // Draw the reuse count now
                for (int i = 0; i < rects.Length; i++)
                {
                    var fRect = rects[i];

                    fRect.X += 1;
                    fRect.Y += 1;
                    
                    if (!_displayReusedCount || _sheetExport == null)
                        continue;

                    // TODO: Store pixel digits created and avoid rendering multiple pixel digits on top of each other
                    Point pixelPoint = fRect.Location;

                    int digitsScale = 3;
                    int frameCount = _sheetExport.Atlas.GetFrameBoundsMap().CountOfFramesAtSheetBoundsIndex(i);

                    while ((fRect.Size.Width < SizeForImageNumber(frameCount, digitsScale).Width * 2 ||
                            fRect.Size.Height < SizeForImageNumber(frameCount, digitsScale).Height * 2)
                           && digitsScale > 1)
                        digitsScale--;

                    RenderPixelNumber(g, pixelPoint, frameCount, digitsScale);
                }
            }
        }
        
        // 
        // Animation Timer tick
        // 
        private void animTimer_Tick(object sender, EventArgs e)
        {
            _dashOffset -= 0.5f;
            if (AllowMouseHover && _mouseOver && ((_frameRects != null && _frameRects.Length > 0) || (_sheetExport != null && _sheetExport.FrameCount > 0)))
            {
                Invalidate();
            }
        }

        // 
        // OnPaint event handler. Draws the underlying sheet, and the frame rectangles on the sheet
        // 
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            // Frame rect sheet display
            if (_frameRectSheet == null)
                return;

            pe.Graphics.DrawImageUnscaled(_frameRectSheet, 0, 0);

            // Allow mouse hovering over individual frame bounds
            if (_mouseOver && AllowMouseHover && (_frameRects != null || _sheetExport != null))
            {
                var absolute = GetAbsolutePoint(_mouseLocation);
                
                pe.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                foreach (var rect in _frameRects ?? _sheetExport.Atlas.UniqueBounds)
                {
                    if (rect.Contains(absolute))
                    {
                        SelectionPaintTool.SelectionPaintToolPainter.PaintSelectionRectangle(pe.Graphics, rect, _dashOffset);
                    }
                }
            }
        }
        
        // 
        // OnMouseDown event handler. Used to pin down selected frame index
        // 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _mouseDown = true;
        }

        // 
        // OnMouseDown event handler. Used to pin down selected frame index
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseDown = false;

            if(!ClientRectangle.Contains(PointToClient(MousePosition)) && _mouseOver)
            {
                _mouseOver = false;
                _mouseOverRectangleIndex = -1;

                if (AllowMouseHover)
                {
                    Invalidate();
                }
            }
        }

        // 
        // OnClick event handler. Handles clicks over frame rectangle bounds
        // 
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (FrameBoundsMouseClicked == null)
                return;
            
            if (_sheetExport == null)
                return;
            
            var absolute = GetAbsolutePoint(_mouseLocation);

            for (int i = 0; i < _sheetExport.Atlas.UniqueBounds.Length; i++)
            {
                var rect = _sheetExport.Atlas.UniqueBounds[i];
                if (rect.Contains(absolute))
                {
                    FrameBoundsMouseClicked?.Invoke(this, new SheetPreviewFrameBoundsClickEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta, i));
                    break;
                }
            }
        }
        
        // 
        // OnMouseMove event handler. Used in conjunction with the AllowMouseHover flag to mark 
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _mouseLocation = e.Location;

            if (AllowMouseHover)
            {
                int nextIndex = -1;

                if (_mouseOver && AllowMouseHover && _sheetExport != null)
                {
                    var absolute = GetAbsolutePoint(_mouseLocation);

                    for (int i = 0; i < _sheetExport.Atlas.UniqueBounds.Length; i++)
                    {
                        var rect = _sheetExport.Atlas.UniqueBounds[i];
                        if (rect.Contains(absolute))
                        {
                            nextIndex = i;
                        }
                    }
                }

                if (_mouseOverRectangleIndex != nextIndex)
                {
                    Invalidate();
                }
            }
        }

        // 
        // OnMouseLeave event handler. Clears the current area under the mouse if the AllowMouseHover flag is true
        // 
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if(!_mouseDown)
            {
                _mouseOver = false;
                _mouseOverRectangleIndex = -1;

                if (AllowMouseHover)
                {
                    Invalidate();
                }
            }
        }

        // 
        // OnMouseEnter event handler. Clears the current area under the mouse if the AllowMouseHover flag is true
        // 
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            _mouseOver = true;

            if (AllowMouseHover)
            {
                Invalidate();
            }
        }

        /// <summary>
        /// Returns the size of a number to be rendered using pixel image digits
        /// </summary>
        /// <param name="number">The number to measure</param>
        /// <param name="digitScale">The scale of the number to produce</param>
        /// <returns>The size of the number, in pixels</returns>
        private static Size SizeForImageNumber(int number, int digitScale)
        {
            return new Size(((int)Math.Floor(Math.Log10(number)) + 1) * 5 * digitScale, 5 * digitScale);
        }

        /// <summary>
        /// Renders a pixel number on a given graphics object
        /// </summary>
        /// <param name="g">The graphics object to render the number to</param>
        /// <param name="point">The point to render the number at</param>
        /// <param name="number">The number to render</param>
        /// <param name="digitsScale">The scaling to use. Set to 1 to pass the default scale</param>
        private static void RenderPixelNumber(Graphics g, Point point, int number, int digitsScale = 1)
        {
            string numberString = Math.Abs(number).ToString();
            int x = 0;
            foreach (char digitChar in numberString)
            {
                int digitNum = int.Parse($"{digitChar}");

                var frameImage = ImageForDigit(digitNum);
                var rect = new Rectangle(point.X + x, point.Y, frameImage.Width * digitsScale, frameImage.Height * digitsScale);

                // Ignore if outside the visible area of the graphics object
                if (g.ClipBounds.IntersectsWith(rect))
                {
                    g.DrawImage(frameImage, rect);
                }

                x += frameImage.Width - 1; // Subtract 1 so the white margin is not noticeably too large
            }
        }

        /// <summary>
        /// Returns the image for a given pixel digit
        /// </summary>
        /// <param name="digit">A valid digit between 0-9</param>
        /// <returns>An image that represents the given digit</returns>
        private static Image ImageForDigit(int digit)
        {
            if (digit < 0 || digit >= _pixelDigitsImages.Length)
                return _pixelDigitsImages[0];

            return _pixelDigitsImages[digit];
        }
    }

    /// <summary>
    /// Arguments for event raised when clicking on sheets on a sheet preview picture box
    /// </summary>
    public class SheetPreviewFrameBoundsClickEventArgs : MouseEventArgs
    {
        /// <summary>
        /// The index of the sheet bounds on the texture atlas' Frame Bounds Map that represents the frame that was pressed.
        /// Is -1, if no sheet was under the mouse pointer.
        /// </summary>
        public int SheetBoundsIndex { get; }
        
        public SheetPreviewFrameBoundsClickEventArgs(MouseButtons button, int clicks, int x, int y, int delta, int sheetBoundsIndex) 
            : base(button, clicks, x, y, delta)
        {
            SheetBoundsIndex = sheetBoundsIndex;
        }
    }
}
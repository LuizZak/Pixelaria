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
using System.Linq;
using System.Windows.Forms;

using Pixelaria.Controllers.Importers;
using Pixelaria.Data.Exports;
using Pixelaria.Utils;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// ZoomablePictureBox implementation used to preview the sheet settings to the user
    /// </summary>
    public class SheetPreviewPictureBox : ZoomablePictureBox
    {
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

        private FrameBoundsMap _exportFrameBoundsMap;

        /// <summary>
        /// Gets or sets the IDefaultImporter to use when generating the sheet rectangles
        /// </summary>
        public IDefaultImporter Importer { get; set; }

        /// <summary>
        /// Gets or sets the current Sheet Settings this SheetPreviewPictureBox is displaying
        /// </summary>
        public SheetSettings SheetSettings
        {
            get { return _sheetSettings; }
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
            get { return _sheetExport; }
            set
            {
                _sheetExport = value;
                RefreshFrameBoundsPreview();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the current mappings of frame bounds, to go along a Sheet Export on this sheet preview picture box
        /// </summary>
        public FrameBoundsMap ExportFrameBoundsMap
        {
            get { return _exportFrameBoundsMap; }
            set {
                _exportFrameBoundsMap = value;
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
            get { return _displayReusedCount; }
            set
            {
                _displayReusedCount = value;
                RefreshFrameBoundsPreview();
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
        /// Descrutor for the SheetPreviewPictureBox class
        /// </summary>
        ~SheetPreviewPictureBox()
        {
            _frameRectSheet?.Dispose();
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
        /// <param name="map">The map of rectangles exported</param>
        public void LoadExportSheet(BundleSheetExport bundleSheetExport, FrameBoundsMap map)
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
            int[] reuseCount = null;

            if (_sheetExport != null)
            {
                rects = (_sheetExport.FrameRects.Select(f => f.SheetArea)).ToArray();
                reuseCount = _sheetExport.ReuseCounts;
            }
            else if (Importer != null && _frameRects != null)
            {
                rects = _frameRects;
                reuseCount = _frameRects.Select(f => 1).ToArray();
            }

            if (rects == null) return;

            var drawnRects = new HashSet<Rectangle>();

            using(var fast = _frameRectSheet.FastLock())
            {
                // Draw the frame bounds now
                uint color = unchecked((uint)Color.Red.ToArgb());
                foreach (Rectangle fRect in rects)
                {
                    // Avoid redrawing the same frame bound multiple times
                    if (drawnRects.Contains(fRect))
                        continue;
                    drawnRects.Add(fRect);

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

            drawnRects.Clear();

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

                    // Avoid redrawing the same frame bound multiple times
                    if (drawnRects.Contains(fRect))
                        continue;
                    drawnRects.Add(fRect);

                    // TODO: Store pixel digits created and avoid rendering multiple pixel digits on top of each other
                    if (_displayReusedCount)
                    {
                        Point pixelPoint = fRect.Location;

                        int digitsScale = 3;
                        int frameCount = reuseCount[i] + 1;

                        while ((fRect.Size.Width < SizeForImageNumber(frameCount, digitsScale).Width * 2 ||
                                fRect.Size.Height < SizeForImageNumber(frameCount, digitsScale).Height * 2)
                               && digitsScale > 1)
                            digitsScale--;

                        RenderPixelNumber(g, pixelPoint, frameCount, digitsScale);
                    }
                }
            }
        }

        // 
        // OnPaint event handler. Draws the underlying sheet, and the frame rectangles on the sheet
        // 
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if(_frameRectSheet != null)
            {
                pe.Graphics.DrawImageUnscaled(_frameRectSheet, 0, 0);
            }
        }

        /// <summary>
        /// Returns the size of a number to be rendered using pixel image digits
        /// </summary>
        /// <param name="number">The number to measure</param>
        /// <param name="digitScale">The scale of the number to produce</param>
        /// <returns>The size of the number, in pixels</returns>
        Size SizeForImageNumber(int number, int digitScale)
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
        static void RenderPixelNumber(Graphics g, Point point, int number, int digitsScale = 1)
        {
            string numberString = Math.Abs(number).ToString();
            int x = 0;
            foreach (char digitChar in numberString)
            {
                int digitNum = int.Parse(digitChar + "");

                Image frameImage = ImageForDigit(digitNum);
                Rectangle rect = new Rectangle(point.X + x, point.Y, frameImage.Width * digitsScale, frameImage.Height * digitsScale);

                // Ignore if outside the visible area of the graphics object
                RectangleF re = g.ClipBounds;
                if (re.IntersectsWith(rect))
                {
                    g.DrawImage(frameImage, rect);
                }

                x += frameImage.Width - 1; // Subtract 1 so the white margin is not noticeably too large

                // If the passed number is 0, quit now to avoid infinite loops
                if (number == 0)
                    return;
            }
        }

        /// <summary>
        /// Returns the image for a given pixel digit
        /// </summary>
        /// <param name="digit">A valid digit between 0-9</param>
        /// <returns>An image that represents the given digit</returns>
        static Image ImageForDigit(int digit)
        {
            if (digit < 0 || digit >= _pixelDigitsImages.Length)
                return _pixelDigitsImages[0];

            return _pixelDigitsImages[digit];
        }
    }
}
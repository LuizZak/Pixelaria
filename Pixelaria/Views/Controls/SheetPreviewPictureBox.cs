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
using System.Linq;
using System.Windows.Forms;

using Pixelaria.Data.Exports;
using Pixelaria.Importers;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// ZoomablePictureBox implementation used to preview the sheet settings to the user
    /// </summary>
    public class SheetPreviewPictureBox : ZoomablePictureBox
    {
        /// <summary>
        /// The current Sheet Settings this SheetPreviewPictureBox is displaying
        /// </summary>
        private SheetSettings sheetSettings;

        /// <summary>
        /// The curre sheet export this SheetPreviewPictureBox is displaying
        /// </summary>
        private BundleSheetExport sheetExport;

        /// <summary>
        /// The array of rectangle areas for each frame
        /// </summary>
        private Rectangle[] frameRects;

        /// <summary>
        /// Whether to display the number of frames that have been reused when drawing the frame bounds
        /// </summary>
        private bool displayReusedCount = false;

        /// <summary>
        /// The array of images used to represent the pixel digits
        /// </summary>
        private Image[] pixelDigitsImages;
        
        /// <summary>
        /// Gets or sets the IDefaultImporter to use when generating the sheet rectangles
        /// </summary>
        public IDefaultImporter Importer { get; set; }

        /// <summary>
        /// Gets or sets the current Sheet Settings this SheetPreviewPictureBox is displaying
        /// </summary>
        public SheetSettings SheetSettings { get { return sheetSettings; } set { sheetSettings = value; frameRects = Importer.GenerateFrameBounds(Image, sheetSettings); Invalidate(); } }

        /// <summary>
        /// Gets or sets the current Sheet Settings this SheetPreviewPictureBox is displaying
        /// </summary>
        public BundleSheetExport SheetExport { get { return sheetExport; } set { sheetExport = value; Invalidate(); } }

        /// <summary>
        /// Gets or sets whether to display the number of frames that have been reused when drawing the frame bounds
        /// </summary>
        [DefaultValue(false)]
        public bool DisplayReusedCount
        {
            get { return displayReusedCount; }
            set
            {
                if (pixelDigitsImages == null)
                {
                    LoadPixelDigitImages();
                }

                displayReusedCount = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Loads the pixel digit images
        /// </summary>
        private void LoadPixelDigitImages()
        {
            pixelDigitsImages = new Image[10];

            pixelDigitsImages[0] = Pixelaria.Properties.Resources.Numbers_0;
            pixelDigitsImages[1] = Pixelaria.Properties.Resources.Numbers_1;
            pixelDigitsImages[2] = Pixelaria.Properties.Resources.Numbers_2;
            pixelDigitsImages[3] = Pixelaria.Properties.Resources.Numbers_3;
            pixelDigitsImages[4] = Pixelaria.Properties.Resources.Numbers_4;
            pixelDigitsImages[5] = Pixelaria.Properties.Resources.Numbers_5;
            pixelDigitsImages[6] = Pixelaria.Properties.Resources.Numbers_6;
            pixelDigitsImages[7] = Pixelaria.Properties.Resources.Numbers_7;
            pixelDigitsImages[8] = Pixelaria.Properties.Resources.Numbers_8;
            pixelDigitsImages[9] = Pixelaria.Properties.Resources.Numbers_9;
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

            this.Image = previewImage;
            this.sheetSettings = sheetSettings;

            frameRects = Importer.GenerateFrameBounds(Image, sheetSettings);

            Invalidate();

            UpdateScrollbars();
        }

        /// <summary>
        /// Loads a bundle sheet export preview
        /// </summary>
        /// <param name="bundleSheetExport">The bundle sheet export containing data about the exported image</param>
        public void LoadExportSheet(BundleSheetExport bundleSheetExport)
        {
            UnloadExportSheet();

            // Reset the transformations
            this.Image = bundleSheetExport.Sheet;
            this.sheetExport = bundleSheetExport;

            Invalidate();

            UpdateScrollbars();
        }

        /// <summary>
        /// Unloads the currently displayed export sheet preview
        /// </summary>
        public void UnloadExportSheet()
        {
            sheetExport = null;
            frameRects = null;

            Invalidate();
        }

        // 
        // OnPaint event handler. Draws the underlying sheet, and the frame rectangles on the sheet
        // 
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (Image != null)
            {
                RectangleF[] rects = null;
                int[] reuseCont = null;

                if (sheetExport != null)
                {
                    rects = (from frameRect in sheetExport.FrameRects select (RectangleF)frameRect.SheetArea).ToArray();
                    reuseCont = sheetExport.ReuseCounts;
                }
                else if (Importer != null && frameRects != null)
                {
                    rects = (from frameRect in frameRects select (RectangleF)frameRect).ToArray();
                    reuseCont = (from i in frameRects select 1).ToArray();
                }

                pe.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                if (rects != null)
                {
                    List<RectangleF> drawnRects = new List<RectangleF>();

                    // Draw the frame bounds now
                    for(int i = 0; i < rects.Length; i++)
                    {
                        if (drawnRects.Contains(rects[i]))
                            continue;
                        drawnRects.Add(rects[i]);

                        RectangleF r = rects[i];
                        r.X += 0.5f;
                        r.Y += 0.5f;

                        if (!pe.Graphics.ClipBounds.IntersectsWith(r))
                            continue;

                        pe.Graphics.DrawRectangle(Pens.Red, r.X, r.Y, r.Width, r.Height);

                        // TODO: Store pixel digits created and avoid rendering multiple pixel digits on top of each other
                        Point pixelPoint = new Point((int)Math.Floor(r.X + 0.5f), (int)Math.Floor(r.Y + 0.5f));

                        if (this.displayReusedCount)
                        {
                            int scale = 3;
                            int frameCount = reuseCont[i] + 1;

                            while ((r.Size.Width < SizeForImageNumber(frameCount, scale).Width * 2 || r.Size.Height < SizeForImageNumber(frameCount, scale).Height * 2) && scale > 1)
                                scale--;

                            RenderPixelNumber(pe.Graphics, pixelPoint, frameCount, scale);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the size of a number to be rendered using pixel image digits
        /// </summary>
        /// <param name="number">The number to measure</param>
        /// <param name="scale">The scale of the number to produce</param>
        /// <returns>The size of the number, in pixels</returns>
        Size SizeForImageNumber(int number, int scale)
        {
            return new Size(((int)Math.Floor(Math.Log10(number)) + 1) * 5 * scale, 5 * scale);
        }

        /// <summary>
        /// Renders a pixel number on a given graphics object
        /// </summary>
        /// <param name="g">The graphics object to render the number to</param>
        /// <param name="point">The point to render the number at</param>
        /// <param name="number">The number to render</param>
        /// <param name="scale">The scaling to use. Set to 1 to pass the default scale</param>
        void RenderPixelNumber(Graphics g, Point point, int number, int scale = 1)
        {
            string numberString = Math.Abs(number).ToString();
            int x = 0;
            for (int i = 0; i < numberString.Length; i++)
            {
                int digit = int.Parse(numberString[i] + "");

                Image frameImage = ImageForDigit(digit);
                Rectangle rect = new Rectangle(point.X + x, point.Y, frameImage.Width * scale, frameImage.Height * scale);

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
        Image ImageForDigit(int digit)
        {
            if (digit < 0 || digit >= pixelDigitsImages.Length)
                return pixelDigitsImages[0];

            return pixelDigitsImages[digit];
        }
    }
}
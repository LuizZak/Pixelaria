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

using System.Drawing;
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
                if (sheetExport != null)
                {
                    // Draw the frame bounds now
                    foreach (BundleSheetExport.FrameRect rect in sheetExport.FrameRects)
                    {
                        RectangleF r = rect.SheetArea;
                        r.X += 0.5f;
                        r.Y += 0.5f;

                        pe.Graphics.DrawRectangle(Pens.Red, r.X, r.Y, r.Width, r.Height);
                    }
                }
                else if (Importer != null && frameRects != null)
                {
                    // Draw the frame bounds now
                    foreach (Rectangle rec in frameRects)
                    {
                        RectangleF r = rec;
                        r.X += 0.5f;
                        r.Y += 0.5f;

                        pe.Graphics.DrawRectangle(Pens.Red, r.X, r.Y, r.Width, r.Height);
                    }
                }
            }
        }
    }
}
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
using System.Drawing;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Utils;
using PixelariaTests.Generators;

namespace PixelariaTests.Utils
{
    /// <summary>
    /// Tests the functionalities of the ImageUtilities class and related components
    /// </summary>
    [TestClass]
    public class ImageUtilitiesTests
    {
        [TestMethod]
        public void TestImagesAreIdentical()
        {
            // Generate the bitmaps
            var bitmap1 = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);
            var bitmap2 = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            // Test the equality
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(bitmap1, bitmap2), "ImagesAreIdentical should return true for images that are equal down to each pixel");

            // Generate a different random bitmap
            bitmap2 = BitmapGenerator.GenerateRandomBitmap(64, 64, 11);

            Assert.IsFalse(ImageUtilities.ImagesAreIdentical(bitmap1, bitmap2), "ImagesAreIdentical should return false for images that are not equal down to each pixel");
        }

        [TestMethod]
        public void TestFindMinimumImageAreaFullBitmap()
        {
            // Test a fully filled bitmap

            // Create the bitmap
            var bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);
            
            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 0, 64, 64), Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 64, 64), bitmapArea, "The minimum image area for a fully filled bitmap must be its whole image area");
        }

        [TestMethod]
        public void TestFindMinimumImageAreaEmpty()
        {
            // Test an empty bitmap

            // Create the bitmap
            var bitmap = new Bitmap(5, 5); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 0, 0), bitmapArea, "The minimum image area for a fully filled bitmap must be its whole image area");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfLeft()
        {
            // Test a bitmap filled halfway horizontally from the left and completely vertically

            // Create the bitmap
            var bitmap = new Bitmap(6, 6); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 0, 3, 6), Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 3, 6), bitmapArea, "The minimum image area has to clip the minimum width that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfRight()
        {
            // Test a bitmap filled halfway horizontally from the right and completely vertically

            // Create the bitmap
            var bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(32, 0, 32, 64), Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(32, 0, 32, 64), bitmapArea, "The minimum image area has to clip the minimum X coordinate that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfTop()
        {
            // Test a bitmap filled completely horizontally and halfway vertically from the top

            // Create the bitmap
            var bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 0, 64, 32), Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 64, 32), bitmapArea, "The minimum image area has to clip the minimum height that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfBottom()
        {
            // Test a bitmap filled completely horizontally and halfway vertically from the bottom

            // Create the bitmap
            var bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 32, 64, 32), Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 32, 64, 32), bitmapArea, "The minimum image area has to clip the minimum Y coordinate that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaCenter()
        {
            // Test a bitmap filled with an arbitrary rectangle that is not touching any of the borders

            // Create the bitmap
            var bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            var areaFilled = new Rectangle(11, 1, 10, 10);

            // Fill the region
            FillBitmapRegion(bitmap, areaFilled, Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(areaFilled, bitmapArea, "The minimum image area has to clip around the opaque pixels");
        }

        [TestMethod]
        public void TestFindMinimumAreaSpotted()
        {
            // Test a bitmap with three plots that define a minimum rectangle:
            // ------------
            // ------------
            // ----[]--[]--
            // ------------
            // ----[]------
            // ------------

            // Create the bitmap
            var bitmap = new Bitmap(6, 6); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Add the three plots
            bitmap.SetPixel(2, 2, Color.Red);
            bitmap.SetPixel(4, 2, Color.Red);
            bitmap.SetPixel(2, 4, Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(2, 2, 3, 3), bitmapArea, "The minimum image area has to clip around the opaque pixels");
        }

        [TestMethod]
        public void TestFindMinimumAreaSpottedDiagonal()
        {
            // Test a bitmap with two plots that define a minimum rectangle:
            // ------------
            // ------------
            // --------[]--
            // ------------
            // ----[]------
            // ------------

            // Create the bitmap
            var bitmap = new Bitmap(6, 6); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Add the two plots
            bitmap.SetPixel(4, 2, Color.Red);
            bitmap.SetPixel(2, 4, Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(2, 2, 3, 3), bitmapArea, "The minimum image area has to clip around the opaque pixels");
        }

        [TestMethod]
        public void TestFindMinimumAreaSinglePixel()
        {
            // Test a bitmap with only a single pixel
            // ------------
            // ------------
            // ----[]------
            // ------------
            // ------------
            // ------------

            // Create the bitmap
            var bitmap = new Bitmap(6, 6); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Add the plot
            bitmap.SetPixel(2, 2, Color.Red);

            // Test the resulting area rectangle
            var bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(2, 2, 1, 1), bitmapArea, "The minimum image area has to clip around the opaque pixels");
        }

        /// <summary>
        /// Fills a rectangle region of bitmap with a specified color
        /// </summary>
        /// <param name="bitmap">The bitmap to operate on</param>
        /// <param name="region">The region to fill on the bitmap</param>
        /// <param name="color">The color to fill the bitmap with</param>
        public static void FillBitmapRegion([NotNull] Bitmap bitmap, Rectangle region, Color color)
        {
            for (int y = Math.Max(0, region.Top); y < Math.Min(bitmap.Height, region.Bottom); y++)
            {
                for (int x = Math.Max(0, region.Left); x < Math.Min(bitmap.Width, region.Right); x++)
                {
                    bitmap.SetPixel(x, y, color);
                }
            }
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Utils;

namespace PixelariaTests.Tests.Utils
{
    /// <summary>
    /// Tests the functionalities of the ImageUtilities class and related components
    /// </summary>
    [TestClass]
    public class ImageUtilitiesTests
    {
        [TestMethod]
        public void TestFindMinimumImageAreaFullBitmap()
        {
            // Test a fully filled bitmap

            // Create the bitmap
            Bitmap bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);
            
            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 0, 64, 64), Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 64, 64), bitmapArea, "The minimum image area for a fully filled bitmap must be its whole image area");
        }

        [TestMethod]
        public void TestFindMinimumImageAreaEmpty()
        {
            // Test an empty bitmap

            // Create the bitmap
            Bitmap bitmap = new Bitmap(5, 5); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 0, 0), bitmapArea, "The minimum image area for a fully filled bitmap must be its whole image area");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfLeft()
        {
            // Test a bitmap filled halfway horizontally from the left and completely vertically

            // Create the bitmap
            Bitmap bitmap = new Bitmap(6, 6); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 0, 3, 6), Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 3, 6), bitmapArea, "The minimum image area has to clip the minimum width that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfRight()
        {
            // Test a bitmap filled halfway horizontally from the right and completely vertically

            // Create the bitmap
            Bitmap bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(32, 0, 32, 64), Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(32, 0, 32, 64), bitmapArea, "The minimum image area has to clip the minimum X coordinate that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfTop()
        {
            // Test a bitmap filled completely horizontally and halfway vertically from the top

            // Create the bitmap
            Bitmap bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 0, 64, 32), Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 0, 64, 32), bitmapArea, "The minimum image area has to clip the minimum height that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaHalfBottom()
        {
            // Test a bitmap filled completely horizontally and halfway vertically from the bottom

            // Create the bitmap
            Bitmap bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Fill the region
            FillBitmapRegion(bitmap, new Rectangle(0, 32, 64, 32), Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(0, 32, 64, 32), bitmapArea, "The minimum image area has to clip the minimum Y coordinate that is not opaque");
        }

        [TestMethod]
        public void TestFindMinimumAreaCenter()
        {
            // Test a bitmap filled with an arbitrary rectangle that is not touching any of the borders

            // Create the bitmap
            Bitmap bitmap = new Bitmap(64, 64); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            Rectangle areaFilled = new Rectangle(11, 1, 10, 10);

            // Fill the region
            FillBitmapRegion(bitmap, areaFilled, Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(areaFilled, bitmapArea, "The minimum image area has to clip the minimum Y coordinate that is not opaque");
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
            Bitmap bitmap = new Bitmap(6, 6); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Add the three plots
            bitmap.SetPixel(2, 2, Color.Red);
            bitmap.SetPixel(4, 2, Color.Red);
            bitmap.SetPixel(2, 4, Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(2, 2, 3, 3), bitmapArea, "The minimum image area has to clip the minimum Y coordinate that is not opaque");
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
            Bitmap bitmap = new Bitmap(6, 6); FillBitmapRegion(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.Transparent);

            // Add the three plots
            bitmap.SetPixel(4, 2, Color.Red);
            bitmap.SetPixel(2, 4, Color.Red);

            // Test the resulting area rectangle
            Rectangle bitmapArea = ImageUtilities.FindMinimumImageArea(bitmap);
            Assert.AreEqual(new Rectangle(2, 2, 3, 3), bitmapArea, "The minimum image area has to clip the minimum Y coordinate that is not opaque");
        }

        /// <summary>
        /// Fills a rectangle region of bitmap with a specified color
        /// </summary>
        /// <param name="bitmap">The bitmap to operate on</param>
        /// <param name="region">The region to fill on the bitmap</param>
        /// <param name="color">The color to fill the bitmap with</param>
        public static void FillBitmapRegion(Bitmap bitmap, Rectangle region, Color color)
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
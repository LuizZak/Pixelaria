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
using System.IO;
using System.Security.Cryptography;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Utils;
using PixelariaTests.TestGenerators;

namespace PixelariaTests.Utils
{
    /// <summary>
    /// Tests the behavior of the Utils class and related components
    /// </summary>
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void TestImagesAreIdentical()
        {
            // Generate the bitmaps
            var bitmap1 = FrameGenerator.GenerateRandomBitmap(64, 64, 10);
            var bitmap2 = FrameGenerator.GenerateRandomBitmap(64, 64, 10);

            // Test the equality
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(bitmap1, bitmap2), "ImagesAreIdentical should return true for images that are equal down to each pixel");

            // Generate a different random bitmap
            bitmap2 = FrameGenerator.GenerateRandomBitmap(64, 64, 11);

            Assert.IsFalse(ImageUtilities.ImagesAreIdentical(bitmap1, bitmap2), "ImagesAreIdentical should return false for images that are not equal down to each pixel");
        }

        /// <summary>
        /// The hashing algorithm used for hashing the bitmaps
        /// </summary>
        private static readonly HashAlgorithm ShaM = new SHA256Managed();

        /// <summary>
        /// Returns a hash for the given Bitmap object
        /// </summary>
        /// <param name="bitmap">The bitmap to get the hash of</param>
        /// <returns>The hash of the given bitmap</returns>
        public static unsafe byte[] GetHashForBitmap([NotNull] Bitmap bitmap)
        {
            using (var fastBitmap = bitmap.FastLock())
            {
                var bytes = new byte[fastBitmap.Height * fastBitmap.Width * (Image.GetPixelFormatSize(bitmap.PixelFormat) / 8)];
                var scByte = (byte*)fastBitmap.Scan0;
                fixed (byte* pByte = bytes)
                {
                    FastBitmap.memcpy(pByte, scByte, (ulong)bytes.Length);
                }

                using (var stream = new MemoryStream(bytes, false))
                {
                    var hash = GetHashForStream(stream);
                    return hash;
                }
            }
        }

        /// <summary>
        /// Returns a hash for the given Stream object
        /// </summary>
        /// <param name="stream">The stream to get the hash of</param>
        /// <returns>The hash of the given stream</returns>
        public static byte[] GetHashForStream([NotNull] Stream stream)
        {
            // Compute a hash for the image
            return ShaM.ComputeHash(stream);
        }
    }
}
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Data.Persistence;
using PixelariaLib.Utils;
using PixelariaLibTests.TestGenerators;

namespace PixelariaLibTests.Data.Persistence
{
    /// <summary>
    /// Tests the functionality of the PersistenceHelper class and related components
    /// </summary>
    [TestClass]
    public class PersistenceHelperTests
    {
        /// <summary>
        /// Tests the SaveImageToStream and LoadImageFromStream methods
        /// </summary>
        [TestMethod]
        public void TestImagePersistence()
        {
            // Setup the test
            var bitmap1 = FrameGenerator.GenerateRandomBitmap(48, 37, 0);
            var bitmap2 = FrameGenerator.GenerateRandomBitmap(12, 45, 0);

            Stream stream = new MemoryStream();

            // Test the methods
            PersistenceHelper.SaveImageToStream(bitmap1, stream);
            PersistenceHelper.SaveImageToStream(bitmap2, stream);

            // Reset stream
            stream.Position = 0;

            // Load the bitmaps back up again one at a time
            var loadedBitmap1 = PersistenceHelper.LoadImageFromStream(stream);
            var loadedBitmap2 = PersistenceHelper.LoadImageFromStream(stream);

            // Compare images
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(bitmap1, loadedBitmap1), "An image loaded from a stream must match completely the image that was saved");
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(bitmap2, loadedBitmap2), "An image loaded from a stream must match completely the image that was saved");

            // Try to edit the loaded bitmaps to verify editability
            loadedBitmap1.SetPixel(0, 0, Color.Black);
            loadedBitmap2.SetPixel(0, 0, Color.Black);
        }
    }
}
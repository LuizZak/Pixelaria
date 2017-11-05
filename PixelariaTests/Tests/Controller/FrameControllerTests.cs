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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Utils;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Controller
{
    [TestClass]
    public class FrameControllerTests
    {
        [TestMethod]
        public void TestLayerCount()
        {
            // Tests LayerCount property

            var frame = FrameGenerator.GenerateRandomFrame(32, 32);

            var frameController = new FrameController(frame);
            
            Assert.AreEqual(frame.Layers.Count, frameController.LayerCount);
        }

        /// <summary>
        /// Tests frame layer indexing
        /// </summary>
        [TestMethod]
        public void TestLayerIndexing()
        {
            var frame = FrameGenerator.GenerateRandomFrame(64, 64, 5, 1);
            var controller = new FrameController(frame);

            controller.AddLayer(FrameGenerator.GenerateRandomBitmap(64, 64, 2));
            IFrameLayer layer = controller.CreateLayer();

            Assert.AreEqual(0, controller.GetLayerAt(0).Index, "Layers fetched with GetLayerAt() must have an index that match the parameter passed");
            Assert.AreEqual(1, controller.GetLayerAt(1).Index, "Layers fetched with GetLayerAt() must have an index that match the parameter passed");
            Assert.AreEqual(2, controller.GetLayerAt(2).Index, "Layers fetched with GetLayerAt() must have an index that match the parameter passed");

            Assert.AreEqual(2, layer.Index, "Layers returned by calls to AddLayer() and CreateLayer() must have an index that match the 'index' parameter passed");
        }

        /// <summary>
        /// Tests frame layer creation by creating a frame composed of multiple layers of a bitmap, and testing the resulting composed bitmap
        /// </summary>
        [TestMethod]
        public void TestFrameLayerCreation()
        {
            // TODO: Find out why this test fails under Release mode but succeeds under Debug mode
            // Issue seems to be related to ImageUtilities.FlattenBitmaps method, when rendering
            // with high-quality option set
            var frame = FrameGenerator.GenerateRandomFrame(64, 64, 1, 1);
            var controller = new FrameController(frame);

            controller.AddLayer(FrameGenerator.GenerateRandomBitmap(64, 64, 2));
            controller.AddLayer(FrameGenerator.GenerateRandomBitmap(64, 64, 3));

            var target = frame.GetComposedBitmap();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash = { 0xBF, 0x8F, 0x9B, 0x56, 0xBE, 0x8D, 0xF5, 0xD2, 0x99, 0x1C, 0x9B, 0xEC, 0x56, 0xB8, 0x6D, 0xA9, 0x41, 0xBB, 0x31, 0x66, 0xD, 0xB2, 0xDC, 0x66, 0xF3, 0x5C, 0x9A, 0xDE, 0x59, 0xC2, 0xC2, 0x52 };

            byte[] currentHash = GetHashForBitmap(target);

            RegisterResultBitmap(target, "FrameLayering");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the composed frame does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests layer insertion logic and asserts the layer insertions are behaving as expected
        /// </summary>
        [TestMethod]
        public void TestFrameLayerInsertion()
        {
            var frame = new Frame(null, 64, 64);
            var controller = new FrameController(frame);

            var layer1 = FrameGenerator.GenerateRandomBitmap(64, 64, 10);
            var layer2 = FrameGenerator.GenerateRandomBitmap(64, 64, 9);

            controller.AddLayer(layer1);
            controller.AddLayer(layer2, 1);

            Assert.AreEqual(frame, controller.GetLayerAt(0).Frame, "The Frame reference of the layer must match the frame that it was added to");
            Assert.AreEqual(3, controller.LayerCount, "The layer count must go up for each new layer added");
            Assert.AreEqual(1, controller.GetLayerAt(1).Index, "A layer's index must reflect its current position on the owning frame's layer list");
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(layer2, controller.GetLayerAt(1).LayerBitmap), "The layer bitmaps insertion must obey the index provided on AddLayer");
        }

        /// <summary>
        /// Tests removal of a single layer on a frame
        /// </summary>
        [TestMethod]
        public void TestOneLayerRemoval()
        {
            var frame = new Frame(null, 64, 64);
            var controller = new FrameController(frame);

            controller.RemoveLayerAt(0);

            Assert.AreEqual(1, controller.LayerCount, "The layer count for a frame can never go bellow 1");
        }

        /// <summary>
        /// Tests layer swapping logic and asserts the layer swappings are behaving as expected
        /// </summary>
        [TestMethod]
        public void TestLayerSwapping()
        {
            // Create a frame
            Bitmap layer0 = FrameGenerator.GenerateRandomBitmap(64, 64, 11);
            Frame frame = new Frame(null, 64, 64);
            frame.SetFrameBitmap(layer0);
            
            var controller = new FrameController(frame);

            Bitmap layer1 = FrameGenerator.GenerateRandomBitmap(64, 64, 10);
            Bitmap layer2 = FrameGenerator.GenerateRandomBitmap(64, 64, 9);

            controller.AddLayer(layer1);
            controller.AddLayer(layer2);

            // Swap the layers
            controller.SwapLayers(0, 2);

            // Test layer swapping by comparing the bitmaps
            Assert.AreEqual(0, controller.GetLayerAt(0).Index, "A layer's index must reflect its current position on the owning frame's layer list");
            Assert.AreEqual(2, controller.GetLayerAt(2).Index, "A layer's index must reflect its current position on the owning frame's layer list");

            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(layer2, controller.GetLayerAt(0).LayerBitmap), "The layers have not been swapped correctly");
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(layer0, controller.GetLayerAt(2).LayerBitmap), "The layers have not been swapped correctly");
        }

        /// <summary>
        /// Tests layer bitmap updating logic
        /// </summary>
        [TestMethod]
        public void TestLayerBitmapUpdating()
        {
            // Create a frame
            var frame = new Frame(null, 64, 64);
            var controller = new FrameController(frame);

            Bitmap layer1 = FrameGenerator.GenerateRandomBitmap(64, 64, 10);

            controller.CreateLayer();

            // Swap the layers
            controller.SetLayerBitmap(1, layer1);

            // Test layer swapping by comparing the bitmaps
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(layer1, controller.GetLayerAt(1).LayerBitmap), "The layer bitmap has not been updated correctly");
        }

        /// <summary>
        /// Tests layer removal logic
        /// </summary>
        [TestMethod]
        public void TestLayerRemoval()
        {
            // Create a frame
            var frame = new Frame(null, 64, 64);
            var controller = new FrameController(frame);

            var bitmap = FrameGenerator.GenerateRandomBitmap(64, 64, 10);

            controller.CreateLayer();

            // Swap the layers
            controller.SetLayerBitmap(1, bitmap);

            var layer = controller.GetLayerAt(0);
            controller.RemoveLayerAt(0);

            // Test layer swapping by comparing the bitmaps
            Assert.AreEqual(null, layer.Frame, "After removing a layer from a frame, its Frame reference must be null");
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(bitmap, controller.GetLayerAt(0).LayerBitmap), "The layer does not appear to have been correctly removed");
        }

        /// <summary>
        /// Tests layer remova/re-adition
        /// </summary>
        [TestMethod]
        public void TestLayerReinserting()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(64, 64, 10);
            var frame2 = frame1.Clone();
            
            var controller = new FrameController(frame2);

            var layer = controller.GetLayerAt(0);
            controller.RemoveLayerAt(0, false);
            controller.AddLayer(layer, 0);

            Assert.AreEqual(frame1, frame2, "After removing and readding a layer back to its original place, the frame structure must be considered unchanged");
        }
        
        /// <summary>
        /// Saves the specified bitmap on a desktop folder used to store resulting operations' bitmaps with the specified file name.
        /// The method saves both a .png format of the image, and a .txt file containing an array of bytes for the image's SHA256 hash
        /// </summary>
        /// <param name="bitmap">The bitmap to save</param>
        /// <param name="name">The file name to use on the bitmap</param>
        public void RegisterResultBitmap(Bitmap bitmap, string name)
        {
            var folder = "TestsResults" + Path.DirectorySeparatorChar + "FrameTests";
            var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + folder;
            var file = path + Path.DirectorySeparatorChar + name;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            bitmap.Save(file + ".png", ImageFormat.Png);

            // Also save a .txt file containing the hash
            var hashBytes = GetHashForBitmap(bitmap);
            var hashString = string.Join(",", hashBytes.Select(b => "0x" + b.ToString("X")));

            File.WriteAllText(file + ".txt", hashString);
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
        public static byte[] GetHashForBitmap(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);

                stream.Position = 0;

                // Compute a hash for the image
                byte[] hash = GetHashForStream(stream);

                return hash;
            }
        }

        /// <summary>
        /// Returns a hash for the given Stream object
        /// </summary>
        /// <param name="stream">The stream to get the hash of</param>
        /// <returns>The hash of the given stream</returns>
        public static byte[] GetHashForStream(Stream stream)
        {
            // Compute a hash for the image
            return ShaM.ComputeHash(stream);
        }
    }
}

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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Data.Persistence;
using Pixelaria.Utils;
using PixelariaTests.PixelariaTests.Generators;
using PixelariaTests.PixelariaTests.Tests.Utils;

// TODO: Derive unit tests for invalid files

namespace PixelariaTests.PixelariaTests.Tests.Data.Persistence
{
    /// <summary>
    /// Tests PixelariaFile, PixelariaFileLoader, and PixelariaFileSaver functionalities and related components
    /// </summary>
    [TestClass]
    public class PersistenceTests
    {
        /// <summary>
        /// Path of the file used during tests that involve disk access
        /// </summary>
        private string _testFilePath;

        /// <summary>
        /// Tests the PixelariaSaverLoader file saving stability
        /// </summary>
        [TestMethod]
        public void TestFileLoaderSaver()
        {
            var bundle = BundleGenerator.GenerateTestBundle(0);
            var stream = new MemoryStream();
            
            PixelariaSaverLoader.SaveBundleToStream(bundle, stream);

            // Test if the memory stream is now filled
            Assert.IsTrue(stream.Length > 0,
                "After a call to PixelariaFileSaver.Save(), the pixelaria file's stream should not be empty");

            // Bring the bundle back with a PixelariaFileLoader
            stream.Position = 0;
            var loadedBundle = PixelariaSaverLoader.LoadBundleFromStream(stream);

            Assert.IsNotNull(loadedBundle);

            UtilsTests.AssertBundlesEqual(bundle, loadedBundle,
                "After persisting a file to a stream and loading it back up again, the bundles must be equal");

            var preCopy = stream.GetBuffer().ToArray(); // Copy buffer for later comparing

            // Save the bundle a few more times to test resilience of the save/load process
            stream.Position = 0;
            loadedBundle = PixelariaSaverLoader.LoadBundleFromStream(stream);
            stream.Position = 0;
            PixelariaSaverLoader.SaveBundleToStream(loadedBundle, stream);
            
            Assert.IsNotNull(loadedBundle);

            UtilsTests.AssertBundlesEqual(bundle, loadedBundle,
                "After persisting a file to a stream and loading it back up again, the bundles must be equal");

            // Verify all frame images are not ReadOnly
            foreach (var frameLayer in bundle.Animations.SelectMany(anim => anim.Frames).Cast<Frame>().SelectMany(f => f.Layers))
            {
                Assert.IsTrue((frameLayer.LayerBitmap.Flags & (int)ImageFlags.ReadOnly) == 0, "Frame layer image incorrectly loaded as ReadOnly");
            }
            
            Assert.IsTrue(
                Utilities.ByteArrayCompare(preCopy, stream.GetBuffer()),
                "Two streams that represent the same Pixelaria File should be bytewise equal");
        }

        /// <summary>
        /// Tests the PixelariaSaverLoader helper class
        /// </summary>
        [TestMethod]
        public void TestPersistenceClass()
        {
            // Generate a dummy file name
            _testFilePath = Path.GetTempFileName();

            var originalBundle = BundleGenerator.GenerateTestBundle(0);
            var v8Bundle = originalBundle.Clone();

            // Flatten all layers from v8 bundle (v8 and prior do not feature frame layers)
            foreach (var frame in v8Bundle.Animations.SelectMany(anim => anim.Frames).Cast<Frame>())
            {
                var bitmap = frame.GetComposedBitmap();
                frame.Layers.RemoveAll(l => l.Index > 0); // All but first layer

                frame.SetFrameBitmap(bitmap);
            }

            // Test new file format saving and loading
            PixelariaSaverLoader.SaveBundleToDisk(originalBundle, _testFilePath);
            var newBundle = PixelariaSaverLoader.LoadBundleFromDisk(_testFilePath);

            Assert.IsNotNull(newBundle);

            UtilsTests.AssertBundlesEqual(originalBundle, newBundle,
                "After persisting a new (>= v8) file to disk and loading it back up again, the bundles must be equal");

            // Test old file format loading
            SaveBundle(originalBundle, _testFilePath);
            newBundle = PixelariaSaverLoader.LoadBundleFromDisk(_testFilePath);

            Assert.IsNotNull(newBundle);

            UtilsTests.AssertBundlesEqual(v8Bundle, newBundle,
                "After loading a legacy (< v8) file to disk and loading it back up again, the bundles must be equal");

            // Now load the bundle using the LoadFileFromDisk
            newBundle = PixelariaSaverLoader.LoadBundleFromDisk(_testFilePath);

            Assert.IsNotNull(newBundle);

            UtilsTests.AssertBundlesEqual(v8Bundle, newBundle,
                "After loading a legacy (< v8) file to disk and loading it back up again, the bundles must be equal");

            // Verify all frame images are not ReadOnly
            Assert.IsNotNull(newBundle);
            foreach (var frameLayer in newBundle.Animations.SelectMany(anim => anim.Frames).Cast<Frame>().SelectMany(f => f.Layers))
            {
                Assert.IsTrue((frameLayer.LayerBitmap.Flags & (int)ImageFlags.ReadOnly) == 0, "Frame layer image incorrectly loaded as ReadOnly");
            }
        }

        /// <summary>
        /// Common test cleanup code
        /// </summary>
        [TestCleanup]
        public void TestTeardown()
        {
            if (!string.IsNullOrEmpty(_testFilePath) && File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        #region Version 8 and prior saver

        /// <summary>
        /// Saves the given bundle to disk
        /// </summary>
        /// <param name="bundle">The bundle to save</param>
        /// <param name="path">The path to save the bundle to</param>
        public static void SaveBundle([NotNull] Bundle bundle, [NotNull] string path)
        {
            // Start writing to the file
            var stream = new FileStream(path, FileMode.Create);

            var writer = new BinaryWriter(stream);

            // Signature block
            writer.Write((byte)'P');
            writer.Write((byte)'X');
            writer.Write((byte)'L');

            // Bundle Header block
            writer.Write(8);
            writer.Write(bundle.Name);
            writer.Write(bundle.ExportPath);

            // Animation Block
            writer.Write(bundle.Animations.Count);

            foreach (var anim in bundle.Animations)
            {
                WriteAnimationToStream(anim, stream);
            }

            // Sheet block
            writer.Write(bundle.AnimationSheets.Count);

            foreach (var sheet in bundle.AnimationSheets)
            {
                WriteAnimationSheetToStream(sheet, stream);
            }

            stream.Close();
        }

        /// <summary>
        /// Writes the given Animation into a stream
        /// </summary>
        /// <param name="animation">The animation to write to the stream</param>
        /// <param name="stream">The stream to write the animation to</param>
        public static void WriteAnimationToStream([NotNull] Animation animation, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(animation.ID);
            writer.Write(animation.Name);
            writer.Write(animation.Width);
            writer.Write(animation.Height);
            writer.Write(animation.PlaybackSettings.FPS);
            writer.Write(animation.PlaybackSettings.FrameSkip);

            writer.Write(animation.FrameCount);

            for (int i = 0; i < animation.FrameCount; i++)
            {
                WriteFrameToStream(animation.GetFrameAtIndex(i), stream);
            }
        }

        /// <summary>
        /// Writes the given Frame into a stream
        /// </summary>
        /// <param name="frame">The frame to write to the stream</param>
        /// <param name="stream">The stream to write the frame to</param>
        public static void WriteFrameToStream([NotNull] IFrame frame, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);
            
            PersistenceHelper.SaveImageToStream(frame.GetComposedBitmap(), stream);

            // Re-calculate frame hash using legacy hashing mode
            frame.SetHash(LegacyGetHashForBitmap(frame.GetComposedBitmap()));

            // Write the frame ID
            writer.Write(frame.ID);

            // Write the hash now
            writer.Write(frame.Hash.Length);
            writer.Write(frame.Hash, 0, frame.Hash.Length);
        }

        /// <summary>
        /// Writes the given AnimationSheet into a stream
        /// </summary>
        /// <param name="sheet">The animation sheet to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        public static void WriteAnimationSheetToStream([NotNull] AnimationSheet sheet, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(sheet.ID);
            writer.Write(sheet.Name);
            WriteExportSettingsToStream(sheet.SheetExportSettings, stream);

            // Write the id of the animations of the sheet to the stream
            var anims = sheet.Animations;

            writer.Write(anims.Length);

            foreach (var anim in anims)
            {
                writer.Write(anim.ID);
            }
        }

        /// <summary>
        /// Writes the given AnimationExportSettings into a stream
        /// </summary>
        /// <param name="settings">The export settings to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        public static void WriteExportSettingsToStream(AnimationSheetExportSettings settings, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(settings.FavorRatioOverArea);
            writer.Write(settings.ForcePowerOfTwoDimensions);
            writer.Write(settings.ForceMinimumDimensions);
            writer.Write(settings.ReuseIdenticalFramesArea);
            writer.Write(settings.HighPrecisionAreaMatching);
            writer.Write(settings.AllowUnorderedFrames);
            writer.Write(settings.UseUniformGrid);
            writer.Write(settings.UsePaddingOnJson);
            writer.Write(settings.ExportJson);
            writer.Write(settings.XPadding);
            writer.Write(settings.YPadding);
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
        private static byte[] LegacyGetHashForBitmap([NotNull] Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
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
        private static byte[] GetHashForStream([NotNull] Stream stream)
        {
            // Compute a hash for the image
            return ShaM.ComputeHash(stream);
        }

        #endregion
    }
}
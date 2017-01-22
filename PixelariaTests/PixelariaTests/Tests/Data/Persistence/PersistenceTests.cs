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

using System.Drawing.Imaging;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Data.Persistence;

using Pixelaria.Utils;
using PixelariaTests.PixelariaTests.Generators;

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
        /// Tests the PixelariaFile, PixelariaFilesaver and PixelariaFileLoader basic functionality and stability
        /// </summary>
        [TestMethod]
        public void TestFileLoaderSaver()
        {
            Bundle bundle = BundleGenerator.GenerateTestBundle(0);
            Stream stream = new MemoryStream();

            PixelariaFile originalFile = new PixelariaFile(bundle, stream);

            PixelariaFileSaver.Save(originalFile);

            // Test if the memory stream is now filled
            Assert.IsTrue(stream.Length > 0, "After a call to PixelariaFileSaver.Save(), the pixelaria file's stream should not be empty");

            // Bring the bundle back with a PixelariaFileLoader
            PixelariaFile newFile = new PixelariaFile(new Bundle(""), stream);
            stream.Position = 0;
            PixelariaFileLoader.Load(newFile);

            Assert.AreEqual(originalFile.LoadedBundle, newFile.LoadedBundle, "After persisting a file to a stream and loading it back up again, the bundles must be equal");

            // Save the bundle a few more times to test resilience of the save/load process
            newFile.CurrentStream.Position = 0;
            PixelariaFileLoader.Load(newFile);
            newFile.CurrentStream.Position = 0;
            PixelariaFileSaver.Save(newFile);

            Assert.IsTrue(
                Utilities.ByteArrayCompare(((MemoryStream) newFile.CurrentStream).GetBuffer(),
                    ((MemoryStream) originalFile.CurrentStream).GetBuffer()), "Two streams that represent the same Pixelaria File should be bitwise equal");

            Assert.AreEqual(originalFile.LoadedBundle, newFile.LoadedBundle, "After persisting a file to a stream and loading it back up again, the bundles must be equal");
        }

        /// <summary>
        /// Tests the PixelariaSaverLoader helper class
        /// </summary>
        [TestMethod]
        public void TestPersistenceClass()
        {
            // Generate a dummy file name
            _testFilePath = Path.GetTempFileName();

            Bundle originalBundle = BundleGenerator.GenerateTestBundle(0);

            // Test new file format saving and loading
            PixelariaSaverLoader.SaveBundleToDisk(originalBundle, _testFilePath);
            Bundle newBundle = PixelariaSaverLoader.LoadBundleFromDisk(_testFilePath);

            Assert.AreEqual(originalBundle, newBundle, "After persisting a new (>= v8) file to disk and loading it back up again, the bundles must be equal");

            // Test old file format loading
            SaveBundle(originalBundle, _testFilePath);
            newBundle = PixelariaSaverLoader.LoadBundleFromDisk(_testFilePath);

            Assert.AreEqual(originalBundle, newBundle, "After loading a legacy (< v8) file to disk and loading it back up again, the bundles must be equal");

            // Now load the bundle using the LoadFileFromDisk
            newBundle = PixelariaSaverLoader.LoadFileFromDisk(_testFilePath).LoadedBundle;
            Assert.AreEqual(originalBundle, newBundle, "After loading a legacy (< v8) file to disk and loading it back up again, the bundles must be equal");
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
        public static void SaveBundle(Bundle bundle, string path)
        {
            // Start writing to the file
            Stream stream = new FileStream(path, FileMode.Create);

            BinaryWriter writer = new BinaryWriter(stream);

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

            foreach (Animation anim in bundle.Animations)
            {
                WriteAnimationToStream(anim, stream);
            }

            // Sheet block
            writer.Write(bundle.AnimationSheets.Count);

            foreach (AnimationSheet sheet in bundle.AnimationSheets)
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
        public static void WriteAnimationToStream(Animation animation, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

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
        public static void WriteFrameToStream(IFrame frame, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            long sizeOffset = stream.Position;

            writer.Write((long)0);

            frame.GetComposedBitmap().Save(stream, ImageFormat.Png);

            // Skip back to the offset and draw the size
            stream.Position = sizeOffset;

            // Write the size now
            writer.Write(stream.Length - sizeOffset - 8);

            // Skip to the end and keep saving
            stream.Position = stream.Length;

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
        public static void WriteAnimationSheetToStream(AnimationSheet sheet, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(sheet.ID);
            writer.Write(sheet.Name);
            WriteExportSettingsToStream(sheet.ExportSettings, stream);

            // Write the id of the animations of the sheet to the stream
            Animation[] anims = sheet.Animations;

            writer.Write(anims.Length);

            foreach (Animation anim in anims)
            {
                writer.Write(anim.ID);
            }
        }

        /// <summary>
        /// Writes the given AnimationExportSettings into a stream
        /// </summary>
        /// <param name="settings">The export settings to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        public static void WriteExportSettingsToStream(AnimationExportSettings settings, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

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

        #endregion
    }
}
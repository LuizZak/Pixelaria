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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Data;
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
        /// Asserts two bundles are equal, raising a descriptive exception if they are not.
        /// </summary>
        public static void AssertBundlesEqual([NotNull] Bundle lhs, [NotNull] Bundle rhs, string message = null)
        {
            if (lhs.Name != rhs.Name)
                throw new AssertFailedException($"Bundle names do not match: {lhs.Name} != {rhs.Name}");

            if (lhs.ExportPath != rhs.ExportPath)
                throw new AssertFailedException($"Bundle export paths do not match: {lhs.ExportPath} != {rhs.ExportPath}");

            if (lhs.SaveFile != rhs.SaveFile)
                throw new AssertFailedException($"Bundle save file names do not match: {lhs.SaveFile} != {rhs.SaveFile}");

            if (lhs.Animations.Count != rhs.Animations.Count)
                throw new AssertFailedException($"Bundle animation counts do not match: {lhs.Animations.Count} != {rhs.Animations.Count}");

            if (lhs.AnimationSheets.Count != rhs.AnimationSheets.Count)
                throw new AssertFailedException($"Bundle animation sheet counts do not match: {lhs.AnimationSheets.Count} != {rhs.AnimationSheets.Count}");

            // Compare animations
            for (int i = 0; i < lhs.Animations.Count; i++)
            {
                try
                {
                    var animation1 = lhs.Animations[i];
                    var animation2 = rhs.Animations[i];

                    AssertAnimationsEqual(animation1, animation2);
                }
                catch (Exception e)
                {
                    if(message != null)
                        Console.WriteLine(message);

                    Console.WriteLine($@"Animations are not equal: animations at index {i} do not match:");
                    Console.WriteLine(e);
                    throw;
                }
            }

            // Compare animation sheets
            for (int i = 0; i < lhs.AnimationSheets.Count; i++)
            {
                try
                {
                    var animSheet1 = lhs.AnimationSheets[i];
                    var animSheet2 = rhs.AnimationSheets[i];

                    AssertAnimationSheetsEqual(animSheet1, animSheet2);
                }
                catch (Exception e)
                {
                    if (message != null)
                        Console.WriteLine(message);

                    Console.WriteLine($@"Animation sheets are not equal: animation sheets at index {i} do not match:");
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Asserts two bundles are not equal, raising a descriptive exception if they are not.
        /// </summary>
        public static void AssertBundlesAreNotEqual(Bundle lhs, Bundle rhs, string message = null)
        {
            try
            {
                AssertBundlesEqual(lhs, rhs);
                throw new AssertFailedException("Bundles are equal.");
            }
            catch (Exception e)
            {
                 // Ok
                 Console.WriteLine($@"[Success] Received expected inequality exception: {e}");
            }
        }

        /// <summary>
        /// Asserts two animation sheets are equal, raising a descriptive exception if they are not.
        /// </summary>
        public static void AssertAnimationSheetsEqual([NotNull] AnimationSheet lhs, [NotNull] AnimationSheet rhs)
        {
            if (lhs.ID != rhs.ID)
                throw new AssertFailedException($"Animation sheet ID's do not match: {lhs.ID} != {rhs.ID}");

            if (lhs.Name != rhs.Name)
                throw new AssertFailedException($"Animation sheet names do not match: {lhs.Name} != {rhs.Name}");
            
            if (!AnimationExportSettings.AnimationExportSettingsComparer.Equals(lhs.ExportSettings, rhs.ExportSettings))
                throw new AssertFailedException($"Animation sheet export settings do not match: {lhs.ExportSettings} != {rhs.ExportSettings}");

            if (lhs.AnimationCount != rhs.AnimationCount)
                throw new AssertFailedException($"Animation sheet animation counts do not match: {lhs.Name} != {rhs.AnimationCount}");

            // Compare animations
            for (int i = 0; i < lhs.AnimationCount; i++)
            {
                try
                {
                    var animation1 = lhs.Animations[i];
                    var animation2 = rhs.Animations[i];

                    AssertAnimationsEqual(animation1, animation2);
                }
                catch (Exception e)
                {
                    Console.WriteLine($@"Animation sheets are not equal: animations at index {i} do not match:");
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Asserts two animations are equal, raising a descriptive exception if they are not.
        /// </summary>
        public static void AssertAnimationsEqual([NotNull] Animation lhs, [NotNull] Animation rhs)
        {
            if (lhs.ID != rhs.ID)
                throw new AssertFailedException($"Animation ID's do not match: {lhs.ID} != {rhs.ID}");

            if (lhs.Name != rhs.Name)
                throw new AssertFailedException($"Animation names do not match: {lhs.Name} != {rhs.Name}");

            if (lhs.Size != rhs.Size)
                throw new AssertFailedException($"Animation sizes do not match: {lhs.Size} != {rhs.Size}");

            if (!AnimationExportSettings.AnimationExportSettingsComparer.Equals(lhs.ExportSettings, rhs.ExportSettings))
                throw new AssertFailedException($"Animation export settings do not match: {lhs.ExportSettings} != {rhs.ExportSettings}");

            if (!AnimationPlaybackSettings.AnimationPlaybackSettingsComparer.Equals(lhs.PlaybackSettings, rhs.PlaybackSettings))
                throw new AssertFailedException($"Animation playback settings do not match: {lhs.PlaybackSettings} != {rhs.PlaybackSettings}");

            if (lhs.FrameCount != rhs.FrameCount)
                throw new AssertFailedException($"Animation frame counts do not match: {lhs.FrameCount} != {rhs.FrameCount}");
            
            // Compare frames
            for (int i = 0; i < lhs.FrameCount; i++)
            {
                try
                {
                    var frame1 = (Frame)lhs.Frames[i];
                    var frame2 = (Frame)rhs.Frames[i];

                    AssertFramesEqual(frame1, frame2);
                }
                catch (Exception e)
                {
                    Console.WriteLine($@"Animations are not equal: frames at index {i} do not match:");
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Asserts two frames are equal, raising a descriptive exception if they are not.
        /// </summary>
        public static void AssertFramesEqual([NotNull] Frame lhs, [NotNull] Frame rhs)
        {
            if(lhs.ID != rhs.ID)
                throw new AssertFailedException($"Frame ID's do not match: {lhs.ID} != {rhs.ID}");

            if(lhs.Size != rhs.Size)
                throw new AssertFailedException($"Frame sizes do not match: {lhs.Size} != {rhs.Size}");
            
            if(!lhs.Hash.SequenceEqual(rhs.Hash))
                throw new AssertFailedException($"Frame hashes do not match: {ByteString(lhs.Hash)} != {ByteString(rhs.Hash)}");

            if(lhs.LayerCount != rhs.LayerCount)
                throw new AssertFailedException($"Frame layer counts do not match: {lhs.LayerCount} != {rhs.LayerCount}");

            // Compare layers
            for (int i = 0; i < lhs.LayerCount; i++)
            {
                try
                {
                    var layer1 = lhs.Layers[i];
                    var layer2 = rhs.Layers[i];

                    AssertLayersAreEqual(layer1, layer2);
                }
                catch (Exception e)
                {
                    Console.WriteLine($@"Frames are not equal: layers at index {i} do not match:");
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Asserts two layers are equal, raising a descriptive exception if they are not.
        /// </summary>
        public static void AssertLayersAreEqual([NotNull] IFrameLayer layer1, [NotNull] IFrameLayer layer2)
        {
            if (layer1.Index != layer2.Index)
                throw new AssertFailedException($"Layer indices do not match: {layer1.Index} != {layer2.Index}");

            if (layer1.Size != layer2.Size)
                throw new AssertFailedException($"Layer sizes do not match: {layer1.Size} != {layer2.Size}");

            if (layer1.Name != layer2.Name)
                throw new AssertFailedException($"Layer names do not match: {layer1.Name} != {layer2.Name}");

            // Images are equal- layers are similar.
            if (ImageUtilities.ImagesAreIdentical(layer1.LayerBitmap, layer2.LayerBitmap))
                return;

            // Verify bitmap colors one by one
            using (FastBitmap fast1 = layer1.LayerBitmap.FastLock(), fast2 = layer2.LayerBitmap.FastLock())
            {
                for (int y = 0; y < fast1.Height; y++)
                {
                    for (int x = 0; x < fast1.Width; x++)
                    {
                        if(fast1.GetPixelInt(x, y) != fast2.GetPixelInt(x, y))
                            throw new AssertFailedException($"Layer bitmaps mismatch at pixel (x: {x}, y: {y}): {fast1.GetPixelInt(x, y)} != {fast2.GetPixelInt(x, y)}");
                    }
                }
            }

            var layer1Hash = GetHashForBitmap(layer1.LayerBitmap);
            var layer2Hash = GetHashForBitmap(layer2.LayerBitmap);

            if (!layer1Hash.SequenceEqual(layer2Hash))
                throw new AssertFailedException($"Layer bitmaps do not match: {ByteString(layer1Hash)} != {ByteString(layer2Hash)}");
        }

        private static string ByteString([NotNull] IEnumerable<byte> bytes)
        {
            return "0x" + string.Join("", bytes.Select(b => b.ToString("X2")));
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
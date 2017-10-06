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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Data
{
    /// <summary>
    /// Test suite for the Frame classes and related components
    /// </summary>
    [TestClass]
    public class FrameTests
    {
        [TestMethod]
        public void TestFrameClone()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(64, 63, 2);
            var frame2 = frame1.Clone();

            Assert.AreEqual(frame1, frame2, "Frames cloned using .Clone() should be exactly equivalent");
        }

        [TestMethod]
        public void TestFrameMemoryUsage()
        {
            var frame = new Frame(null, 64, 64, false);

            var memory = frame.CalculateMemoryUsageInBytes(true);

            Assert.AreEqual(64 * 64 * 32 / 8, memory, "The memory usage for a 64 x 64 frame with 32bpp should be equal to 16.384 bytes");

            // Test with a different resolution + bit depth
            frame.SetFrameBitmap(new Bitmap(128, 32, PixelFormat.Format24bppRgb));

            memory = frame.CalculateMemoryUsageInBytes(true);

            Assert.AreEqual(128 * 32 * 24 / 8, memory, "The memory usage for a 128 x 32 frame with 32bpp should be equal to 12.288 bytes");
        }

        [TestMethod]
        public void TestFrameResizing()
        {
            var frame1 = new Frame(null, 64, 64);
            var frame2 = new Frame(null, 12, 16);

            frame1.Resize(12, 16, PerFrameScalingMethod.PlaceAtTopLeft, InterpolationMode.NearestNeighbor);

            Assert.AreEqual(frame1, frame2, "After an empty frame is resized to the size of another empty frame, they are to be considered equal");
        }

        [TestMethod]
        public void TestFrameCopyFrom()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(16, 16);
            var frame2 = FrameGenerator.GenerateRandomFrame(16, 16);
            
            frame1.CopyFrom(frame2);

            Assert.AreEqual(frame1, frame2, "After a successful call to CopyFrom(), the frames must return true to .Equals()");
        }

        [TestMethod]
        public void TestCanCopyFrom()
        {
            var frame = new Frame(null, 64, 64);

            Assert.IsTrue(frame.CanCopyFromType<Frame>(), "A call to CanCopyFromType with the same type as the frame object should return true");
            Assert.IsFalse(frame.CanCopyFromType<DifferentFrame>(), "A call to CanCopyFromType with a type that is not assignable to the frame's type should return false");
            Assert.IsFalse(frame.CanCopyFromType<DerivedFrame>(), "A call to CanCopyFromType with a type that is derived from the frame's type should return false");

            var derivedFrame = new DerivedFrame(null, 64, 64);

            Assert.IsTrue(derivedFrame.CanCopyFromType<Frame>(), "A call to CanCopyFromType with a type that is a super type of the DerivedFrame's type should return true");
            Assert.IsTrue(derivedFrame.CanCopyFromType<DerivedFrame>(), "A call to CanCopyFromType with a type of DerivedFrame should return true");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Trying to copy a frame with different dimensions while inside an animation should raise an exception")]
        public void TestFrameInvalidCopyFrom()
        {
            var anim = new Animation("TestAnimation1", 21, 21);
            var animController = new AnimationController(null, anim);
            animController.CreateFrame();

            var frame1 = anim[0];
            var frame2 = new Frame(null, 20, 20);

            frame1.CopyFrom(frame2);
        }

        /// <summary>
        /// Tests the Frame.Index property
        /// </summary>
        [TestMethod]
        public void TestGetFrameIndex()
        {
            // Create an animation and an empty dummy frame
            var anim = new Animation("TestAnimation1", 64, 64);

            var controller = new AnimationController(null, anim);

            var frame1 = new Frame(null, 64, 64);
            var frame2 = new Frame(null, 64, 64);
            var frame3 = new Frame(null, 64, 64);

            controller.AddFrame(frame1);
            controller.AddFrame(frame2);
            controller.AddFrame(frame3);

            Assert.AreEqual(0, frame1.Index, "The Inedx property of a frame must reflect the frame's own position in the Animation it is on");
            Assert.AreEqual(1, frame2.Index, "The Inedx property of a frame must reflect the frame's own position in the Animation it is on");

            Assert.AreEqual(frame1.Index, anim.GetFrameIndex(frame1), "A frame's Index property should be equivalent to a call to Animation.GetFrameIndex(frame)");
            Assert.AreEqual(frame2.Index, anim.GetFrameIndex(frame2), "A frame's Index property should be equivalent to a call to Animation.GetFrameIndex(frame)");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Tring to perform any action on an uninitialized frame other than a call to Initialize should raise an InvalidOperationException")]
        public void TestFrameUninitializedException()
        {
            var frame = new Frame();
            frame.GetComposedBitmap();
        }

        [TestMethod]
        public void TestFrameInitialize()
        {
            var frame = new Frame();
            frame.Initialize(null, 64, 64);
            frame.GetComposedBitmap();
        }

        /// <summary>
        /// Tests frame compositing by creating and saving a composed frame to a file and comparing its result
        /// </summary>
        [TestMethod]
        public void TestFrameCompositing()
        {
            var frame = FrameGenerator.GenerateRandomFrame(64, 64, 5, 1);
            var target = frame.GetComposedBitmap();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash = { 0xB2, 0x91, 0xF2, 0xB1, 0x17, 0xF7, 0x17, 0x46, 0xA0, 0x1C, 0xA4, 0xCB, 0x45, 0x82, 0x17, 0xA4, 0x42, 0x60, 0x2F, 0xEE, 0x7E, 0x1A, 0xDC, 0xE3, 0x2F, 0xB, 0x89, 0xEC, 0x76, 0x6, 0x2C, 0xA1 };

            var currentHash = GetHashForBitmap(target);

            RegisterResultBitmap(target, "FrameCompositing");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the composed frame does not match the good hash stored. Verify the output image for an analysis of what went wrong");
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
            var hashString = "";
            hashBytes.ToList().ForEach(b => hashString += (hashString.Length == 0 ? "" : ",") + "0x" + b.ToString("X"));
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
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);

                stream.Position = 0;

                // Compute a hash for the image
                var hash = GetHashForStream(stream);

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

    /// <summary>
    /// Specifies an IFrame class that is different from a Frame class
    /// </summary>
    public class DifferentFrame : IFrame
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public int ID
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Equals(IFrame other)
        {
            throw new NotImplementedException();
        }

        public int Width
        {
            get { throw new NotImplementedException(); }
        }

        public int Height
        {
            get { throw new NotImplementedException(); }
        }

        public Size Size
        {
            get { throw new NotImplementedException(); }
        }

        public int Index
        {
            get { throw new NotImplementedException(); }
        }

        public Animation Animation
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }

        public bool Initialized
        {
            get { throw new NotImplementedException(); }
        }

        public void Initialize(Animation animation, int width, int height, bool initHash = true)
        {
            throw new NotImplementedException();
        }

        public void Removed()
        {
            throw new NotImplementedException();
        }

        public void Added(Animation newAnimation)
        {
            throw new NotImplementedException();
        }

        public Frame Clone()
        {
            throw new NotImplementedException();
        }

        public void CopyFrom<TFrame>(TFrame frame) where TFrame : IFrame
        {
            throw new NotImplementedException();
        }

        public bool CanCopyFromType<TFrame>() where TFrame : IFrame
        {
            throw new NotImplementedException();
        }

        public bool Equals(Frame frame)
        {
            throw new NotImplementedException();
        }

        public long CalculateMemoryUsageInBytes(bool composed)
        {
            throw new NotImplementedException();
        }

        public Bitmap GetComposedBitmap()
        {
            throw new NotImplementedException();
        }

        public Image GenerateThumbnail(int width, int height, bool resizeOnSmaller, bool centered, Color backColor)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth, int newHeight, PerFrameScalingMethod scalingMethod, InterpolationMode interpolationMode)
        {
            throw new NotImplementedException();
        }

        public void UpdateHash()
        {
            throw new NotImplementedException();
        }

        public void SetHash(byte[] newHash)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Specifies a Frame class that is derived from the original Frame class
    /// </summary>
    public class DerivedFrame : Frame
    {
        /// <summary>
        /// Creates a new derived animation frame
        /// </summary>
        /// <param name="parentAnimation">The parent animation</param>
        /// <param name="width">The width of this frame</param>
        /// <param name="height">The height of this frame</param>
        /// <param name="initHash">Whether to initialize the frame's hash now</param>
        public DerivedFrame(Animation parentAnimation, int width, int height, bool initHash = true)
            : base(parentAnimation, width, height, initHash)
        {

        }

        /// <summary>
        /// Returns whether the current frame can copy the conents of the specified frame type
        /// </summary>
        /// <typeparam name="TFrame">The type of frame to copy from</typeparam>
        public override bool CanCopyFromType<TFrame>()
        {
            return typeof(TFrame).IsAssignableFrom(typeof(DerivedFrame));
        }
    }
}
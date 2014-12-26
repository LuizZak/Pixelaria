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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Frame frame1 = FrameGenerator.GenerateRandomFrame(64, 64, 0);
            Frame frame2 = frame1.Clone();

            Assert.AreEqual(frame1, frame2, "Frames cloned using .Clone() should be exactly equivalent");
        }

        [TestMethod]
        public void TestFrameMemoryUsage()
        {
            Frame frame = new Frame(null, 64, 64, false);

            long memory = frame.CalculateMemoryUsageInBytes();

            Assert.AreEqual(64 * 64 * 32 / 8, memory, "The memory usage for a 64 x 64 frame with 32bpp should be equal to 16.384 bytes");

            // Test with a different resolution + bit depth
            frame.SetFrameBitmap(new Bitmap(128, 32, PixelFormat.Format24bppRgb));

            memory = frame.CalculateMemoryUsageInBytes();

            Assert.AreEqual(128 * 32 * 24 / 8, memory, "The memory usage for a 128 x 32 frame with 32bpp should be equal to 12.288 bytes");
        }

        [TestMethod]
        public void TestFrameResizing()
        {
            Frame frame1 = new Frame(null, 64, 64);
            Frame frame2 = new Frame(null, 12, 16);

            frame1.Resize(12, 16, PerFrameScalingMethod.PlaceAtTopLeft, InterpolationMode.NearestNeighbor);

            Assert.AreEqual(frame1, frame2, "After an empty frame is resized to the size of another empty frame, they are to be considered equal");
        }

        [TestMethod]
        public void TestFrameCopyFrom()
        {
            Animation anim = new Animation("TestAnimation", 16, 16);

            Frame frame1 = anim.CreateFrame();
            Frame frame2 = FrameGenerator.GenerateRandomFrame(16, 16);
            
            frame1.CopyFrom(frame2);

            Assert.AreEqual(frame1, frame2, "After a successful call to CopyFrom(), the frames must return true to .Equals()");
        }

        [TestMethod]
        public void TestCanCopyFrom()
        {
            Frame frame = new Frame(null, 64, 64);

            Assert.IsTrue(frame.CanCopyFromType<Frame>(), "A call to CanCopyFromType with the same type as the frame object should return true");
            Assert.IsFalse(frame.CanCopyFromType<DifferentFrame>(), "A call to CanCopyFromType with a type that is not assignable to the frame's type should return false");
            Assert.IsFalse(frame.CanCopyFromType<DerivedFrame>(), "A call to CanCopyFromType with a type that is derived from the frame's type should return false");

            DerivedFrame derivedFrame = new DerivedFrame(null, 64, 64);

            Assert.IsTrue(derivedFrame.CanCopyFromType<Frame>(), "A call to CanCopyFromType with a type that is a super type of the DerivedFrame's type should return true");
            Assert.IsTrue(derivedFrame.CanCopyFromType<DerivedFrame>(), "A call to CanCopyFromType with a type of DerivedFrame should return true");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Trying to copy a frame with different dimensions while inside an animation should raise an exception")]
        public void TestFrameInvalidCopyFrom()
        {
            Animation anim = new Animation("TestAnimation", 16, 16);

            Frame frame1 = anim.CreateFrame();
            Frame frame2 = new Frame(null, 20, 20);

            frame1.CopyFrom(frame2);
        }

        /// <summary>
        /// Tests the Frame.Index property
        /// </summary>
        [TestMethod]
        public void TestGetFrameIndex()
        {
            // Create an animation and an empty dummy frame
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            Frame frame1 = new Frame(null, 64, 64);
            Frame frame2 = new Frame(null, 64, 64);
            Frame frame3 = new Frame(null, 64, 64);

            anim1.AddFrame(frame1);
            anim1.AddFrame(frame2);
            anim1.AddFrame(frame3);

            Assert.AreEqual(0, frame1.Index, "The Inedx property of a frame must reflect the frame's own position in the Animation it is on");
            Assert.AreEqual(1, frame2.Index, "The Inedx property of a frame must reflect the frame's own position in the Animation it is on");

            Assert.AreEqual(frame1.Index, anim1.GetFrameIndex(frame1), "A frame's Index property should be equivalent to a call to Animation.GetFrameIndex(frame)");
            Assert.AreEqual(frame2.Index, anim1.GetFrameIndex(frame2), "A frame's Index property should be equivalent to a call to Animation.GetFrameIndex(frame)");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Tring to perform any action on an uninitialized frame other than a call to Initialize should raise an InvalidOperationException")]
        public void TestFrameUninitializedException()
        {
            Frame frame = new Frame();
            frame.GetComposedBitmap();
        }

        [TestMethod]
        public void TestFrameInitialize()
        {
            Frame frame = new Frame();
            frame.Initialize(null, 64, 64);
            frame.GetComposedBitmap();
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

        public long CalculateMemoryUsageInBytes()
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
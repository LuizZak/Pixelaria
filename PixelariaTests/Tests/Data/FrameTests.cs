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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using PixelariaTests.Generators;

namespace PixelariaTests.Tests.Data
{
    /// <summary>
    /// Test suite for the Frame classes and related components
    /// </summary>
    [TestClass]
    public class FrameTests
    {
        [TestMethod]
        public void TestframeClone()
        {
            Frame frame1 = FrameGenerator.GenerateRandomFrame(64, 64, 0);
            Frame frame2 = frame1.Clone();
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
    }
}
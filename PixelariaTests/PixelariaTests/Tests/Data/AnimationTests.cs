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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Utils;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Data
{
    /// <summary>
    /// Test suite for the Animation and Frame classes and related components
    /// </summary>
    [TestClass]
    public class AnimationTests
    {
        [TestMethod]
        public void TestFrameEquals()
        {
            Frame fr1 = new Frame(null, 64, 64);
            Frame fr2 = fr1.Clone();

            Assert.AreEqual(fr1, fr2, "The frames after a Clone() operation must be equal");

            // Fill both frames with randomly generated images
            Bitmap bit = FrameGenerator.GenerateRandomBitmap(64, 64, 0);
            fr1.SetFrameBitmap(bit);
            fr2.SetFrameBitmap(bit);
            
            Assert.AreEqual(fr1, fr2, "Frames with equal images must return true to .Equals()");

            fr2.SetFrameBitmap(FrameGenerator.GenerateRandomBitmap(64, 64, 1));

            Assert.AreNotEqual(fr1, fr2, "Frames with different images must return false to .Equals()");
        }

        [TestMethod]
        public void TestAnimationFrameCreation()
        {
            Animation anim = new Animation("TestAnimation", 64, 64);
            Frame frame = anim.CreateFrame();

            Assert.AreEqual(anim.FrameCount, 1, "Frame count must be 1 after call to CreateFrame()");
            Assert.AreEqual(anim.Size, frame.Size, "Frame's dimensions must be similar to the parent animation");
        }

        [TestMethod]
        public void TestAnimationDuplicate()
        {
            Animation anim1 = AnimationGenerator.GenerateAnimation("TestAnimation1", 64, 64, 10);
            // Create an empty frame to test frame duplication
            anim1.CreateFrame();

            Animation anim2 = anim1.Clone();

            Assert.AreEqual(anim1, anim2,  "The animations after a Clone() operation must be equal");

            // Change the animations by adding frames
            anim2.CreateFrame();

            Assert.AreNotEqual(anim1, anim2, "Animations with different frame counts cannot be equal");

            // Remove the newly created frame from the second animation
            anim2.RemoveFrameIndex(anim2.FrameCount - 1);

            Assert.AreEqual(anim1, anim2, "After a RemoveFrameIndex() call, the frame must be removed from the animation");

            // Test different values for the struct properties
            anim2.ExportSettings = new AnimationExportSettings { AllowUnorderedFrames = true, UseUniformGrid = true };

            Assert.AreNotEqual(anim1, anim2, "Animations with different values for the ExportSettings must not be equal");

            anim2.ExportSettings = anim1.ExportSettings;
            anim2.PlaybackSettings.FPS = 0;

            Assert.AreNotEqual(anim1, anim2, "Animations with different values for the ExportSettings must not be equal");
        }

        [TestMethod]
        public void TestAnimationCopying()
        {
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            // Create an empty frame to test frame duplication
            anim1.CreateFrame();

            Animation anim2 = new Animation("TestAnimation1", 64, 64);
            anim2.CopyFrom(anim1, false);

            Assert.AreEqual(anim1, anim2, "An animation with all parameters except frames similar after a .CopyFrom() operation must be equal to the original animation");
        }

        /// <summary>
        /// Tests the frame inserting mechanics of the Animation object by inserting multiple repeated and similar frames and checking the end result
        /// </summary>
        [TestMethod]
        public void TestDuplicatedFrameInserting()
        {
            // Create an animation and an empty dummy frame
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            Frame frame1 = new Frame(null, 64, 64);

            anim1.AddFrame(frame1);

            Assert.AreEqual(1, anim1.FrameCount, "After adding a unique frame to an animation, it frame count should be bumped up");

            // Add the same frame
            anim1.AddFrame(frame1);

            Assert.AreEqual(1, anim1.FrameCount, "An animation should not allow adding the same frame object reference twice");

            // Add a clone of the frame
            anim1.AddFrame(frame1.Clone());

            Assert.AreEqual(2, anim1.FrameCount, "Animations should allow inserting equal frames as long as they are not the same object reference");
        }

        /// <summary>
        /// Tests removal of frames from an animation
        /// </summary>
        [TestMethod]
        public void TestFrameRemoval()
        {
            // Create an animation and an empty dummy frame
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            Frame frame1 = new Frame(null, 64, 64);
            Frame frame2 = new Frame(null, 64, 64);
            Frame frame3 = new Frame(null, 64, 64);

            anim1.AddFrame(frame1);
            anim1.AddFrame(frame2);
            anim1.AddFrame(frame3);

            anim1.RemoveFrame(frame3);

            Assert.IsTrue(!anim1.Frames.ToList().ContainsReference(frame3), "Removing a frame from an animation should have removed its reference");
            Assert.IsTrue(anim1.Frames.ToList().ContainsReference(frame1), "Removing a frame from an animation should not remove any other frame by value");
        }

        /// <summary>
        /// Tests removal of frames from an animation
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to remove a frame from an animation it does not belong to should raise an ArgumentException")]
        public void TestFrameRemovalException()
        {
            // Create an animation and an empty dummy frame
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            Frame frame1 = new Frame(null, 64, 64);

            anim1.RemoveFrame(frame1);
        }

        /// <summary>
        /// Tests frame index fetching
        /// </summary>
        [TestMethod]
        public void TestFrameGetIndex()
        {
            // Create an animation and an empty dummy frame
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            Frame frame1 = new Frame(null, 64, 64);
            Frame frame2 = new Frame(null, 64, 64);
            Frame frame3 = new Frame(null, 64, 64);
            Frame frame4 = new Frame(null, 64, 64);

            anim1.AddFrame(frame1);
            anim1.AddFrame(frame2);
            anim1.AddFrame(frame3);

            Assert.AreEqual(2, anim1.GetFrameIndex(frame3), "Fetching a frame index should return the index of the frame by reference, not by value");
            Assert.AreEqual(-1, anim1.GetFrameIndex(frame4), "Fetching a frame index for a frame that does not belongs to an animation should return -1");
        }

        [TestMethod]
        public void TestContainsFrame()
        {
            // Create an animation and an empty dummy frame
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            Frame frame1 = new Frame(null, 64, 64);
            Frame frame4 = new Frame(null, 64, 64);

            anim1.AddFrame(frame1);

            Assert.IsTrue(anim1.ContainsFrame(frame1), "Calling ContainsFrame() with a frame in the animation must return true");
            Assert.IsFalse(anim1.ContainsFrame(frame4), "Calling ContainsFrame() with a frame that is not in the animation must return false");
        }

        /// <summary>
        /// Tests the GetFrameAtIndex() method
        /// </summary>
        [TestMethod]
        public void TestGetFrameAtIndex()
        {
            // Create an animation and an empty dummy frame
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            Frame frame1 = new Frame(null, 64, 64);
            Frame frame2 = new Frame(null, 64, 64);
            Frame frame3 = new Frame(null, 64, 64);

            anim1.AddFrame(frame1);
            anim1.AddFrame(frame2);
            anim1.AddFrame(frame3);

            Assert.AreEqual(frame3, anim1.GetFrameAtIndex(2), "Fetching a frame at an index should return the proper frame in the order it is listed inside the Animation object");

            // Removing the second frame should lower the third frame's index by 1
            anim1.RemoveFrame(frame2);

            Assert.AreEqual(frame3, anim1.GetFrameAtIndex(1), "After removing a frame, all frames after it must have their indexes lowered by 1");
        }

        /// <summary>
        /// Tests multiple frame insertion and the behavior of frame rescaling with the 'UseNewSize' setting
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "When trying to add a series of frames through 'AddFrames' with different dimensions, an ArgumentException needs to be thrown")]
        public void TestMultiSizedFrameInsertingException()
        {
            Animation anim = new Animation("TestAnim", 16, 16);

            Frame firstFrame = new Frame(null, 16, 16);
            Frame secondFrame = new Frame(null, 20, 16);
            Frame thirdFrame = new Frame(null, 27, 21);

            List<Frame> frames = new List<Frame> { firstFrame, secondFrame, thirdFrame };

            anim.AddFrames(frames);
        }

        /// <summary>
        /// Tests multiple frame insertion and the behavior of frame rescaling with the 'UseLargestSize' setting
        /// </summary>
        [TestMethod]
        public void TestMultiSizedFrameInsertingLargestSize()
        {
            Animation anim = new Animation("TestAnim", 16, 16);

            Frame firstFrame = new Frame(null, 16, 16);

            List<Frame> frames = new List<Frame> {firstFrame, new Frame(null, 20, 16)};

            anim.AddFrames(frames,
                new FrameSizeMatchingSettings()
                {
                    AnimationDimensionMatchMethod = AnimationDimensionMatchMethod.UseLargestSize
                });

            Assert.AreEqual(new Size(20, 16), firstFrame.Size,
                "After inserting frames into an animation with the 'UseLargestSize' flag on the frame size matching settings, the frames must take the largest dimensions");
        }

        /// <summary>
        /// Tests multiple frame insertion and the behavior of frame rescaling with the 'KeepOriginal' setting
        /// </summary>
        [TestMethod]
        public void TestMultiSizedFrameInsertingKeepOriginal()
        {
            Animation anim = new Animation("TestAnim", 16, 16);

            Frame firstFrame = new Frame(null, 16, 16);
            Frame secondFrame = new Frame(null, 20, 16);

            List<Frame> frames = new List<Frame> { firstFrame, secondFrame };

            anim.AddFrames(frames,
                new FrameSizeMatchingSettings()
                {
                    AnimationDimensionMatchMethod = AnimationDimensionMatchMethod.KeepOriginal
                });

            Assert.AreEqual(new Size(16, 16), firstFrame.Size,
                "After inserting frames into an animation with the 'KeepOriginal' flag on the frame size matching settings, the frames must remain constant");
            Assert.AreEqual(new Size(16, 16), secondFrame.Size,
                "After inserting frames into an animation with the 'KeepOriginal' flag on the frame size matching settings, frames that have different dimensions should resize");
        }

        /// <summary>
        /// Tests multiple frame insertion and the behavior of frame rescaling with the 'UseNewSize' setting
        /// </summary>
        [TestMethod]
        public void TestMultiSizedFrameInsertingUseNewSize()
        {
            Animation anim = new Animation("TestAnim", 16, 16);

            Frame firstFrame = new Frame(null, 16, 16);
            Frame secondFrame = new Frame(null, 20, 16);
            Frame thirdFrame = new Frame(null, 27, 21);

            List<Frame> frames = new List<Frame> { firstFrame, secondFrame, thirdFrame };

            anim.AddFrames(frames,
                new FrameSizeMatchingSettings()
                {
                    AnimationDimensionMatchMethod = AnimationDimensionMatchMethod.UseNewSize
                });

            Assert.AreEqual(new Size(27, 21), firstFrame.Size,
                "After inserting frames into an animation with the 'UseNewSize' flag on the frame size matching settings, the frames must match the largest size from the new frame set");

            // Test now with a set of frames that are smaller than the animation's original size
            anim = new Animation("TestAnim", 32, 32);

            firstFrame = new Frame(null, 16, 16);
            secondFrame = new Frame(null, 20, 16);
            thirdFrame = new Frame(null, 27, 21);

            frames = new List<Frame> { firstFrame, secondFrame, thirdFrame };

            anim.AddFrames(frames,
                new FrameSizeMatchingSettings()
                {
                    AnimationDimensionMatchMethod = AnimationDimensionMatchMethod.UseNewSize
                });

            Assert.AreEqual(new Size(27, 21), firstFrame.Size,
                "After inserting frames into an animation with the 'UseNewSize' flag on the frame size matching settings, the frames must match the largest size from the new frame set, even if the animation is larger");
        }

        /// <summary>
        /// Tests animation frame swapping
        /// </summary>
        [TestMethod]
        public void TestFrameSwapping()
        {
            Animation anim = new Animation("TestAnim", 16, 16);

            Frame frame1 = anim.CreateFrame();
            Frame frame2 = anim.CreateFrame();

            anim.SwapFrameIndices(0, 1);

            Assert.AreEqual(1, anim.GetFrameIndex(frame1), "Swapping frame indices must be reflected in GetframeIndex()");
            Assert.AreEqual(0, anim.GetFrameIndex(frame2), "Swapping frame indices must be reflected in GetframeIndex()");
        }

        [TestMethod]
        public void TestAnimationClearing()
        {
            Animation anim = new Animation("TestAnim", 16, 16);

            Frame frame1 = anim.CreateFrame();

            anim.Clear();

            Assert.AreEqual(0, anim.FrameCount, "After a call to Clear(), the animation's frame count should be 0");
            Assert.IsFalse(anim.ContainsFrame(frame1), "Immediately after a call to Clear(), no frame passed to ContainsFrame() should return true");
        }

        /// <summary>
        /// Tests memory usage information accumulated by frames that is returned by an animation
        /// </summary>
        [TestMethod]
        public void TestMemoryUsage()
        {
            Animation anim1 = AnimationGenerator.GenerateAnimation("TestAnim1", 16, 16, 10);
            Animation anim2 = AnimationGenerator.GenerateAnimation("TestAnim1", 32, 32, 16);

            Assert.AreEqual(16 * 16 * 10 * 4, anim1.CalculateMemoryUsageInBytes(true), "The memory usage returned for a 16 x 16 animation that is 10 frames long should be 10.240 bytes");
            Assert.AreEqual(32 * 32 * 16 * 4, anim2.CalculateMemoryUsageInBytes(true), "The memory usage returned for a 16 x 16 animation that is 10 frames long should be 65.536 bytes");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Tring to add an unitialized frame to an animation should raise an exception")]
        public void TestAddFrameUninitializedException()
        {
            Animation anim = new Animation("TestAnim", 64, 64);
            Frame frame = new Frame();
            
            anim.AddFrame(frame);
        }
    }
}
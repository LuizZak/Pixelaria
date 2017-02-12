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

using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Controller
{

    [TestClass]
    public class AnimationControllerTests
    {
        [TestMethod]
        public void TestFrameCount()
        {
            // Tests the FrameCount property

            var animation = AnimationGenerator.GenerateAnimation("Test0", 32, 32, 2);

            var controller = new AnimationController(animation);

            Assert.AreEqual(animation.Frames.Count, controller.FrameCount);
        }

        [TestMethod]
        public void TestGetFrames()
        {
            // Tests the GetFrames method

            var animation = AnimationGenerator.GenerateAnimation("Test0", 32, 32, 2);

            var controller = new AnimationController(animation);

            // Tests frame ids match the ones from the controller, in order
            Assert.IsTrue(controller.GetFrames().Select(id => id.Id).SequenceEqual(animation.Frames.Select(f => f.ID).ToArray()));
        }
        
        /// <summary>
        /// Tests removal of frames from an animation
        /// </summary>
        [TestMethod]
        public void TestFrameRemoval()
        {
            // Create an animation and an empty dummy frame
            var anim1 = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(anim1)
            {
                FrameIdGenerator = new FrameIdGenerator()
            };

            var frame1 = controller.CreateFrame();
            controller.CreateFrame();
            var frame3 = controller.CreateFrame();

            controller.RemoveFrame(frame3);

            Assert.IsFalse(anim1.Frames.Any(f => f.ID == frame3.Id), "Removing a frame from an animation should have removed its reference");
            Assert.IsTrue(anim1.Frames.Any(f => f.ID == frame1.Id), "Removing a frame from an animation should not remove any other frame by value");
        }

        /// <summary>
        /// Tests removal of frames from an animation
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Trying to remove a frame from an animation it does not belong to should raise an ArgumentException")]
        public void TestFrameRemovalException()
        {
            // Create an animation and an empty dummy frame
            var anim1 = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(anim1);

            var frame = controller.CreateFrame();

            // Remove frame to invalidate
            anim1.Frames.Clear();

            controller.RemoveFrame(frame);
        }

        /// <summary>
        /// Tests frame index fetching
        /// </summary>
        [TestMethod]
        public void TestFrameGetIndex()
        {
            // Create an animation and an empty dummy frame
            var anim1 = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(anim1)
            {
                FrameIdGenerator = new FrameIdGenerator()
            };

            controller.CreateFrame();
            var frame2 = controller.CreateFrame();
            var frame3 = controller.CreateFrame();
            controller.CreateFrame();

            // remove last frame to invalidate
            anim1.Frames.RemoveAt(1);

            Assert.AreEqual(1, controller.GetFrameIndex(frame3), "Fetching a frame index should return the index of the frame by reference, not by value");
            Assert.AreEqual(-1, controller.GetFrameIndex(frame2), "Fetching a frame index for a frame that does not belongs to an animation should return -1");
        }

        [TestMethod]
        public void TestContainsFrame()
        {
            // Create an animation and an empty dummy frame
            var anim1 = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(anim1)
            {
                FrameIdGenerator = new FrameIdGenerator()
            };

            var frame1 = controller.CreateFrame();
            var frame2 = controller.CreateFrame();
            
            // Remove last frame
            anim1.Frames.RemoveAt(1);

            Assert.IsTrue(controller.ContainsFrame(frame1), "Calling ContainsFrame() with a frame in the animation must return true");
            Assert.IsFalse(controller.ContainsFrame(frame2), "Calling ContainsFrame() with a frame that is not in the animation must return false");
        }

        /// <summary>
        /// Tests the GetFrameAtIndex() method
        /// </summary>
        [TestMethod]
        public void TestGetFrameAtIndex()
        {
            // Create an animation and an empty dummy frame
            var anim1 = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(anim1)
            {
                FrameIdGenerator = new FrameIdGenerator()
            };

            controller.CreateFrame();
            var frame2 = controller.CreateFrame();
            var frame3 = controller.CreateFrame();
            
            Assert.AreEqual(frame3, controller.GetFrameAtIndex(2), "Fetching a frame at an index should return the proper frame in the order it is listed inside the Animation object");

            // Removing the second frame should lower the third frame's index by 1
            controller.RemoveFrame(frame2);

            Assert.AreEqual(frame3, controller.GetFrameAtIndex(1), "After removing a frame, all frames after it must have their indexes lowered by 1");
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

            List<Frame> frames = new List<Frame> { firstFrame, new Frame(null, 20, 16) };

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
            var anim = new Animation("TestAnimation1", 16, 16);
            var controller = new AnimationController(anim)
            {
                FrameIdGenerator = new FrameIdGenerator()
            };

            var frame1 = controller.CreateFrame();
            var frame2 = controller.CreateFrame();

            anim.SwapFrameIndices(0, 1);

            Assert.AreEqual(1, controller.GetFrameIndex(frame1), "Swapping frame indices must be reflected in GetframeIndex()");
            Assert.AreEqual(0, controller.GetFrameIndex(frame2), "Swapping frame indices must be reflected in GetframeIndex()");
        }
    }
}
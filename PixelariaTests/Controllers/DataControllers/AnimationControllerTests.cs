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
using PixLib.Controllers.DataControllers;
using PixLib.Data;
using Pixelaria.Utils;
using PixelariaTests.Generators;

namespace PixelariaTests.Controllers.DataControllers
{
    /// <summary>
    /// Test suite for the Animation and Frame classes and related components
    /// </summary>
    [TestClass]
    public class AnimationControllerTests
    {
        [TestMethod]
        public void TestFrameEquals()
        {
            var fr1 = new Frame(null, 64, 64);
            var fr2 = fr1.Clone();

            Assert.AreEqual(fr1, fr2, "The frames after a Clone() operation must be equal");

            // Fill both frames with randomly generated images
            var bit = BitmapGenerator.GenerateRandomBitmap(64, 64, 0);
            fr1.SetFrameBitmap(bit);
            fr2.SetFrameBitmap(bit);
            
            Assert.AreEqual(fr1, fr2, "Frames with equal images must return true to .Equals()");

            fr2.SetFrameBitmap(BitmapGenerator.GenerateRandomBitmap(64, 64, 1));

            Assert.AreNotEqual(fr1, fr2, "Frames with different images must return false to .Equals()");
        }

        [TestMethod]
        public void TestAnimationFrameCreation()
        {
            var anim = new Animation("TestAnimation", 64, 64);
            var controller = new AnimationController(null, anim);
            var frame = controller.CreateFrame();

            var frameController = controller.GetFrameController(frame);

            Assert.AreEqual(anim.FrameCount, 1, "Frame count must be 1 after call to CreateFrame()");
            Assert.AreEqual(anim.Size, frameController.Size, "Frame's dimensions must be similar to the parent animation");
        }

        [TestMethod]
        public void TestAnimationDuplicate()
        {
            var anim1 = AnimationGenerator.GenerateAnimation("TestAnimation1", 64, 64, 10);
            var controller1 = new AnimationController(null, anim1);

            // Create an empty frame to test frame duplication
            controller1.CreateFrame();
            var clone = controller1.CloneAnimation();

            Assert.AreEqual(anim1, clone, "The animations after a CloneAnimation() operation must be equal");
            Assert.AreNotSame(anim1, clone);

            var controller2 = controller1.MakeCopyForEditing(true);

            // Change the animations by adding frames
            controller2.CreateFrame();

            Assert.AreNotEqual(anim1, controller2.CloneAnimation(), "Animations with different frame counts cannot be equal");

            // Remove the newly created frame from the second animation
            controller2.RemoveFrameIndex(controller2.FrameCount - 1);

            Assert.AreEqual(anim1, controller2.CloneAnimation(), "After a RemoveFrameIndex() call, the frame must be removed from the animation");

            // Test different values for the struct properties
            controller2.SheetExportSettings = new AnimationSheetExportSettings { AllowUnorderedFrames = true, UseUniformGrid = true };

            Assert.AreNotEqual(anim1, controller2.CloneAnimation(), "Animations with different values for the ExportSettings must not be equal");

            controller2.SheetExportSettings = anim1.SheetExportSettings;

            var playbackSettings = controller2.PlaybackSettings;
            playbackSettings.FPS = 0;
            controller2.PlaybackSettings = playbackSettings;

            Assert.AreNotEqual(anim1, controller2.CloneAnimation(), "Animations with different values for the ExportSettings must not be equal");
        }

        [TestMethod]
        public void TestAnimationCopying()
        {
            var anim1 = new Animation("TestAnimation1", 64, 64);
            var controller1 = new AnimationController(null, anim1);
            // Create an empty frame to test frame duplication
            controller1.CreateFrame();

            var anim2 = new Animation("TestAnimation1", 64, 64);
            var controller2 = new AnimationController(null, anim2);
            controller2.CopyFrom(controller1, false);

            Assert.AreEqual(anim1, anim2, "An animation with all parameters except frames similar after a .CopyFrom() operation must be equal to the original animation");
        }

        /// <summary>
        /// Tests the frame inserting mechanics of the Animation object by inserting multiple repeated and similar frames and checking the end result
        /// </summary>
        [TestMethod]
        public void TestDuplicatedFrameInserting()
        {
            // Create an animation and an empty dummy frame
            var anim = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(null, anim);
            var frame1 = new Frame(null, 64, 64);

            controller.AddFrame(frame1);

            Assert.AreEqual(1, anim.FrameCount, "After adding a unique frame to an animation, it frame count should be bumped up");

            // Add the same frame
            controller.AddFrame(frame1);

            Assert.AreEqual(1, anim.FrameCount, "An animation should not allow adding the same frame object reference twice");

            // Add a clone of the frame
            controller.AddFrame(frame1.Clone());

            Assert.AreEqual(2, anim.FrameCount, "Animations should allow inserting equal frames as long as they are not the same object reference");
        }

        /// <summary>
        /// Tests removal of frames from an animation
        /// </summary>
        [TestMethod]
        public void TestFrameRemoval()
        {
            // Create an animation and an empty dummy frame
            var anim = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(null, anim);

            var frame1 = new Frame(null, 64, 64);
            var frame2 = new Frame(null, 64, 64);
            var frame3 = new Frame(null, 64, 64);

            controller.AddFrame(frame1);
            controller.AddFrame(frame2);
            var frameId3 = controller.AddFrame(frame3);

            controller.RemoveFrame(frameId3);

            Assert.IsTrue(!anim.Frames.ToList().ContainsReference(frame3), "Removing a frame from an animation should have removed its reference");
            Assert.IsTrue(anim.Frames.ToList().ContainsReference(frame1), "Removing a frame from an animation should not remove any other frame by value");
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
            var anim2 = new Animation("TestAnimation2", 64, 64);
            var controller1 = new AnimationController(null, anim1);
            var controller2 = new AnimationController(null, anim2);

            var frame = controller1.CreateFrame();

            controller2.RemoveFrame(frame);
        }

        /// <summary>
        /// Tests frame index fetching
        /// </summary>
        [TestMethod]
        public void TestFrameGetIndex()
        {
            // Create an animation and an empty dummy frame
            var anim = new Animation("TestAnimation1", 64, 64);
            var controller = new AnimationController(null, anim);

            var frame1 = new Frame(null, 64, 64);
            var frame2 = new Frame(null, 64, 64);
            var frame3 = new Frame(null, 64, 64);

            controller.AddFrame(frame1);
            controller.AddFrame(frame2);
            var id3 = controller.AddFrame(frame3);

            Assert.AreEqual(2, controller.GetFrameIndex(id3), "Fetching a frame index should return the index of the frame by reference, not by value");

            var anim2 = new Animation("TestAnimation2", 64, 64);
            var controller2 = new AnimationController(null, anim2);

            Assert.AreEqual(-1, controller2.GetFrameIndex(id3), "Fetching a frame index for a frame that does not belongs to an animation should return -1");
        }

        [TestMethod]
        public void TestContainsFrame()
        {
            // Create an animation and an empty dummy frame
            var anim1 = new Animation("TestAnimation1", 64, 64);
            var controller1 = new AnimationController(null, anim1);

            var frame1 = new Frame(null, 64, 64);
            var frame4 = new Frame(null, 64, 64);

            controller1.AddFrame(frame1);

            Assert.IsTrue(anim1.ContainsFrame(frame1), "Calling ContainsFrame() with a frame in the animation must return true");
            Assert.IsFalse(anim1.ContainsFrame(frame4), "Calling ContainsFrame() with a frame that is not in the animation must return false");

            // Test controller
            Assert.IsTrue(controller1.ContainsFrame(controller1.GetFrameAtIndex(0)), "Calling ContainsFrame() with a frame in the animation must return true");

            var anim2 = new Animation("TestAnimation2", 64, 64);
            var controller2 = new AnimationController(null, anim2);

            Assert.IsFalse(controller2.ContainsFrame(controller1.GetFrameAtIndex(0)), "Calling ContainsFrame() with a frame that is not in the animation must return false");
        }

        /// <summary>
        /// Tests the GetFrameAtIndex() method
        /// </summary>
        [TestMethod]
        public void TestGetFrameAtIndex()
        {
            // Create an animation and an empty dummy frame
            var anim = new Animation("TestAnimation1", 64, 64);

            var controller = new AnimationController(null, anim);

            var frame1 = new Frame(null, 64, 64);
            var frame2 = new Frame(null, 64, 64);
            var frame3 = new Frame(null, 64, 64);

            controller.AddFrame(frame1);
            var f2Id = controller.AddFrame(frame2);
            controller.AddFrame(frame3);

            Assert.AreEqual(frame3, anim.GetFrameAtIndex(2), "Fetching a frame at an index should return the proper frame in the order it is listed inside the Animation object");

            // Removing the second frame should lower the third frame's index by 1
            controller.RemoveFrame(f2Id);

            Assert.AreEqual(frame3, anim.GetFrameAtIndex(1), "After removing a frame, all frames after it must have their indexes lowered by 1");
        }

        /// <summary>
        /// Tests multiple frame insertion and the behavior of frame rescaling with the 'UseNewSize' setting
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "When trying to add a series of frames through 'AddFrames' with different dimensions, an ArgumentException needs to be thrown")]
        public void TestMultiSizedFrameInsertingException()
        {
            var anim = new Animation("TestAnim", 16, 16);

            var controller = new AnimationController(null, anim);

            var firstFrame = new Frame(null, 16, 16);
            var secondFrame = new Frame(null, 20, 16);
            var thirdFrame = new Frame(null, 27, 21);

            var frames = new List<Frame> { firstFrame, secondFrame, thirdFrame };

            controller.AddFrames(frames);
        }

        /// <summary>
        /// Tests multiple frame insertion and the behavior of frame rescaling with the 'UseLargestSize' setting
        /// </summary>
        [TestMethod]
        public void TestMultiSizedFrameInsertingLargestSize()
        {
            var anim = new Animation("TestAnim", 16, 16);

            var controller = new AnimationController(null, anim);

            var firstFrame = new Frame(null, 16, 16);

            var frames = new List<Frame> {firstFrame, new Frame(null, 20, 16)};

            controller.AddFrames(frames,
                new FrameSizeMatchingSettings
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
            var anim = new Animation("TestAnim", 16, 16);

            var controller = new AnimationController(null, anim);

            var firstFrame = new Frame(null, 16, 16);
            var secondFrame = new Frame(null, 20, 16);

            var frames = new List<Frame> { firstFrame, secondFrame };

            controller.AddFrames(frames,
                new FrameSizeMatchingSettings
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
            var anim = new Animation("TestAnim", 16, 16);

            var controller = new AnimationController(null, anim);

            var firstFrame = new Frame(null, 16, 16);
            var secondFrame = new Frame(null, 20, 16);
            var thirdFrame = new Frame(null, 27, 21);

            var frames = new List<Frame> { firstFrame, secondFrame, thirdFrame };

            controller.AddFrames(frames,
                new FrameSizeMatchingSettings
                {
                    AnimationDimensionMatchMethod = AnimationDimensionMatchMethod.UseNewSize
                });

            Assert.AreEqual(new Size(27, 21), firstFrame.Size,
                "After inserting frames into an animation with the 'UseNewSize' flag on the frame size matching settings, the frames must match the largest size from the new frame set");

            // Test now with a set of frames that are smaller than the animation's original size
            anim = new Animation("TestAnim", 32, 32);
            controller = new AnimationController(null, anim);

            firstFrame = new Frame(null, 16, 16);
            secondFrame = new Frame(null, 20, 16);
            thirdFrame = new Frame(null, 27, 21);

            frames = new List<Frame> { firstFrame, secondFrame, thirdFrame };

            controller.AddFrames(frames,
                new FrameSizeMatchingSettings
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
            var anim = new Animation("TestAnim", 16, 16);
            var controller = new AnimationController(null, anim);

            var frame1 = controller.CreateFrame();
            var frame2 = controller.CreateFrame();

            controller.SwapFrameIndices(0, 1);

            Assert.AreEqual(1, controller.GetFrameIndex(frame1), "Swapping frame indices must be reflected in GetframeIndex()");
            Assert.AreEqual(0, controller.GetFrameIndex(frame2), "Swapping frame indices must be reflected in GetframeIndex()");
        }

        [TestMethod]
        public void TestAnimationClearing()
        {
            var anim = new Animation("TestAnim", 16, 16);

            var controller = new AnimationController(null, anim);

            var f1Id = controller.CreateFrame();
            var frame1 = anim.Frames[0];

            anim.Clear();

            Assert.AreEqual(0, anim.FrameCount, "After a call to Clear(), the animation's frame count should be 0");
            Assert.IsFalse(anim.Frames.ContainsReference(frame1), "Immediately after a call to Clear(), no frame passed to ContainsFrame() should return true");
            Assert.IsFalse(controller.ContainsFrame(f1Id), "Immediately after a call to Clear(), no frame passed to ContainsFrame() should return true");
        }

        /// <summary>
        /// Tests memory usage information accumulated by frames that is returned by an animation
        /// </summary>
        [TestMethod]
        public void TestMemoryUsage()
        {
            var anim1 = AnimationGenerator.GenerateAnimation("TestAnim1", 16, 16, 10);
            var anim2 = AnimationGenerator.GenerateAnimation("TestAnim1", 32, 32, 16);

            var controller1 = new AnimationController(null, anim1);
            var controller2 = new AnimationController(null, anim2);

            Assert.AreEqual(16 * 16 * 10 * 4, controller1.CalculateMemoryUsageInBytes(true), "The memory usage returned for a 16 x 16 animation that is 10 frames long should be 10.240 bytes");
            Assert.AreEqual(32 * 32 * 16 * 4, controller2.CalculateMemoryUsageInBytes(true), "The memory usage returned for a 16 x 16 animation that is 10 frames long should be 65.536 bytes");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Tring to add an unitialized frame to an animation should raise an exception")]
        public void TestAddFrameUninitializedException()
        {
            var anim = new Animation("TestAnim", 64, 64);

            var controller = new AnimationController(null, anim);

            var frame = new Frame();

            controller.AddFrame(frame);
        }
    }
}
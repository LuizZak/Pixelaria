using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Utils;
using PixelariaTests.Generators;

namespace PixelariaTests.Tests.Data
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
            Bitmap bit = FrameGenerator.GenerateFrameImage(64, 64, 0);
            fr1.SetFrameBitmap(bit);
            fr2.SetFrameBitmap(bit);
            
            Assert.AreEqual(fr1, fr2, "Frames with equal images must return true to .Equals()");

            fr2.SetFrameBitmap(FrameGenerator.GenerateFrameImage(64, 64, 1));

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

            anim1.AddFrame(frame1);
            anim1.AddFrame(frame2);
            anim1.AddFrame(frame3);

            Assert.AreEqual(2, anim1.GetFrameIndex(frame3), "Fetching a frame index should return the index of the frame by reference, not by value");
        }
    }
}
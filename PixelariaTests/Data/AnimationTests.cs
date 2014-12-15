using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using PixelariaTests.Generators;

namespace PixelariaTests.Data
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
            Animation anim1 = new Animation("TestAnimation1", 64, 64);
            // Create an empty frame to test frame duplication
            anim1.CreateFrame();

            Animation anim2 = anim1.Clone();

            Assert.AreEqual(anim1, anim2,  "The animations after a Clone() operation must be equal");

            // Change the animations by adding frames
            anim2.CreateFrame();

            Assert.AreNotEqual(anim1, anim2, "Animations with different frame counts cannot be equal");

            // Remove the newly created frame from the second animation
            anim2.RemoveFrameIndex(1);

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
    }
}
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
            Frame frame1 = FrameGenerator.GenerateFrame(64, 64, 0);
            Frame frame2 = frame1.Clone();
        }

        [TestMethod]
        public void TestFrameMemoryUsage()
        {
            Frame frame = new Frame(null, 64, 64, false);

            long memory = frame.CalculateMemoryUsageInBytes();

            Assert.AreEqual(64 * 64 * 32 / 8, memory, "The memory usage for a 64 x 64 frame with 32bpp should be equal to 16,384 bytes");

            // Test with a different resolution + bit depth
            frame.SetFrameBitmap(new Bitmap(128, 32, PixelFormat.Format24bppRgb));

            memory = frame.CalculateMemoryUsageInBytes();

            Assert.AreEqual(128 * 32 * 24 / 8, memory, "The memory usage for a 128 x 32 frame with 32bpp should be equal to 12,288 bytes");
        }

        [TestMethod]
        public void TestFrameResizing()
        {
            Frame frame1 = new Frame(null, 64, 64);
            Frame frame2 = new Frame(null, 12, 16);

            frame1.Resize(12, 16, PerFrameScalingMethod.PlaceAtTopLeft, InterpolationMode.NearestNeighbor);

            Assert.AreEqual(frame1, frame2, "After an empty frame is resized to the size of another empty frame, they are to be considered equal");
        }
    }
}
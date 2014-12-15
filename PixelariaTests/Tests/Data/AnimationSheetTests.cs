using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using PixelariaTests.Generators;

namespace PixelariaTests.Tests.Data
{
    /// <summary>
    /// Tests AnimationSheet class functionality and related components
    /// </summary>
    [TestClass]
    public class AnimationSheetTests
    {
        [TestMethod]
        public void TestAnimationSheetClone()
        {
            AnimationSheet sheet1 = AnimationSheetGenerator.GenerateAnimationSheet("TestSheet", 2, 64, 64, 10, 0);
            AnimationSheet sheet2 = sheet1.Clone();

            Assert.AreEqual(sheet1, sheet2, "Animation sheets copied with the Clone() method must be equal");

            // Modify one of the sheet's properties
            sheet2.Animations[0].Frames[0].SetFrameBitmap(FrameGenerator.GenerateDifferentFrom(sheet1.Animations[0].Frames[0].GetComposedBitmap()));

            Assert.AreNotEqual(sheet1, sheet2, "After modification of a cloned animation sheet's animation's frame, it must no longer be considered equal to the original");

            // Modify the export params struct
            sheet2.Animations[0].Frames[0].SetFrameBitmap(sheet1.Animations[0].Frames[0].GetComposedBitmap());
            sheet2.ExportSettings = new AnimationExportSettings {ExportXml = !sheet1.ExportSettings.ExportXml};

            Assert.AreNotEqual(sheet1, sheet2, "After modification of a cloned animation sheet's export settings, it must no longer be considered equal to the original");
        }

        [TestMethod]
        public void TestAnimationSheetFrameCount()
        {
            Random r = new Random();

            for (int i = 0; i < 10; i++)
            {
                int animCount = r.Next(1, 10);
                int frameCount = r.Next(1, 10);
                AnimationSheet sheet1 = AnimationSheetGenerator.GenerateAnimationSheet("TestSheet", animCount, 64, 64, frameCount, 0);

                Assert.AreEqual(sheet1.GetFrameCount(), animCount * frameCount, "GetFrameCount() must return the frame count of all the animations in a sprite sheet");
            }
        }
    }
}
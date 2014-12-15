using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using PixelariaTests.Generators;

namespace PixelariaTests.Tests.Data.Exports
{
    /// <summary>
    /// Tests the TextureAtlas class' functionality and related components
    /// </summary>
    [TestClass]
    public class TextureAtlasTests
    {
        [TestMethod]
        public void TestTextureAtlasFrameInsert()
        {
            TextureAtlas atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings(), "TestAtlas");

            Animation anim = AnimationGenerator.GenerateAnimation("TestAnim1", 16, 16, 10);

            foreach (var frame in anim.Frames)
            {
                atlas.InsertFrame(frame);
            }
            
            Assert.AreEqual(anim, atlas.GetAnimationsOnAtlas()[0], "After adding frames of an animation to an atlas, the animation should be listed on the atlas' GetAnimationsOnAtlas()");
            Assert.AreEqual(anim.FrameCount, atlas.FrameCount, "When adding frames to an atlas, all valid frames passed to the InsertFrame() should be counted in it");
            Assert.IsTrue(!anim.Frames.Where((t, i) => !ReferenceEquals(t, atlas.FrameList[i])).Any(),
                "When adding frames to an atlas, all valid frames passed to the InsertFrame() should be counted in it");
        }
    }
}
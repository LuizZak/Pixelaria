using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Algorithms.Packers;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Algorithms.Packers
{
    /// <summary>
    /// Tests the functionality of the DefaultTexturePacker class and related components
    /// </summary>
    [TestClass]
    public class DefaultTesturePackerTests
    {
        [TestMethod]
        public void TestPackEmptyAtlas()
        {
            TextureAtlas atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings());

            DefaultTexturePacker packer = new DefaultTexturePacker();

            packer.Pack(atlas);

            Assert.AreEqual(new Rectangle(0, 0, 1, 1), atlas.AtlasRectangle, "Packing an empty texture atlas should result in an atlas with an empty area");
        }

        [TestMethod]
        public void TestPackEmptyFrames()
        {
            Animation anim = new Animation("TestAnim", 64, 64);

            // Fill the animation with a few empty frames
            anim.CreateFrame();
            anim.CreateFrame();

            TextureAtlas atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings());

            atlas.InsertFramesFromAnimation(anim);

            DefaultTexturePacker packer = new DefaultTexturePacker();

            packer.Pack(atlas);

            Assert.AreEqual(new Rectangle(0, 0, 1, 1), atlas.AtlasRectangle, "Packing a texture atlas that contains empty frames should result in an atlas with a 1x1 area");
        }
    }
}
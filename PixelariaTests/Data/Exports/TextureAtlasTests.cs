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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data.Exports;
using PixelariaTests.Generators;

namespace PixelariaTests.Data.Exports
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
            var atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings(), "TestAtlas");
            var anim = AnimationGenerator.GenerateAnimation("TestAnim1", 16, 16, 10);

            foreach (var frame in anim.Frames)
            {
                atlas.InsertFrame(frame);
            }
            
            Assert.AreEqual(anim, atlas.GetAnimationsOnAtlas()[0], "After adding frames of an animation to an atlas, the animation should be listed on the atlas' GetAnimationsOnAtlas()");
            Assert.AreEqual(anim.FrameCount, atlas.FrameCount, "When adding frames to an atlas, all valid frames passed to the InsertFrame() should be counted in it");
            Assert.IsTrue(!anim.Frames.Where((t, i) => !ReferenceEquals(t, atlas.FrameList[i])).Any(),
                "When adding frames to an atlas, all valid frames passed to the InsertFrame() should be counted in it");
        }

        [TestMethod]
        public void TestInsertDuplicatedFrames()
        {
            var atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings(), "TestAtlas");
            var anim = AnimationGenerator.GenerateAnimation("TestAnim1", 16, 16, 5);

            foreach (var frame in anim.Frames)
            {
                atlas.InsertFrame(frame);
                atlas.InsertFrame(frame);
            }

            Assert.AreEqual(anim, atlas.GetAnimationsOnAtlas()[0], "Adding the same frames to an atlas twice should have no effect on its animation count");
            Assert.AreEqual(anim.FrameCount, atlas.FrameCount, "Adding the same frames to an atlas twice should have no effect on its frame count");
            Assert.IsTrue(!anim.Frames.Where((t, i) => !ReferenceEquals(t, atlas.FrameList[i])).Any(),
                "Adding the same frames to an atlas twice should have no effect on its frame set");
        }

        [TestMethod]
        public void TestInsertAnimation()
        {
            var atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings(), "TestAtlas");
            var anim = AnimationGenerator.GenerateAnimation("TestAnim1", 16, 16, 10);

            atlas.InsertFramesFromAnimation(anim);

            Assert.AreEqual(anim, atlas.GetAnimationsOnAtlas()[0], "After adding an animation to an atlas, the animation should be listed on the atlas' GetAnimationsOnAtlas()");
            Assert.AreEqual(anim.FrameCount, atlas.FrameCount, "When adding animations to an atlas, all valid frames on the animation should be counted in it");
            Assert.IsTrue(!anim.Frames.Where((t, i) => !ReferenceEquals(t, atlas.FrameList[i])).Any(),
                "When adding animations to an atlas, all valid frames on the animation should be counted in it");
        }

        [TestMethod]
        public void TestInsertDuplicatedAnimation()
        {
            var atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings(), "TestAtlas");
            var anim = AnimationGenerator.GenerateAnimation("TestAnim1", 16, 16, 10);

            atlas.InsertFramesFromAnimation(anim);
            atlas.InsertFramesFromAnimation(anim);

            Assert.AreEqual(anim, atlas.GetAnimationsOnAtlas()[0], "Adding the same animation to an atlas twice should have no effect on its animation count");
            Assert.AreEqual(anim.FrameCount, atlas.FrameCount, "Adding the same animation to an atlas twice should have no effect on its frame count");
            Assert.IsTrue(!anim.Frames.Where((t, i) => !ReferenceEquals(t, atlas.FrameList[i])).Any(),
                "Adding the same animation to an atlas twice should have no effect in its frame set");
        }
    }
}
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

using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Algorithms.Packers;
using PixelariaLib.Controllers.DataControllers;
using PixelariaLib.Data;
using PixelariaLib.Data.Exports;
using PixelariaLibTests.TestGenerators;

namespace PixelariaLibTests.Algorithms.Packers
{
    /// <summary>
    /// Tests the functionality of the DefaultTexturePacker class and related components
    /// </summary>
    [TestClass]
    public class DefaultTexturePackerTests
    {
        [TestMethod]
        public void TestPackEmptyAtlas()
        {
            var atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings());

            var packer = new DefaultTexturePacker();

            packer.Pack(atlas);

            Assert.AreEqual(new Rectangle(0, 0, 1, 1), atlas.AtlasRectangle, "Packing an empty texture atlas should result in an atlas with an empty area");
        }

        [TestMethod]
        public void TestPackEmptyFrames()
        {
            var anim = new Animation("TestAnim", 64, 64);

            var controller = new AnimationController(null, anim) {FrameIdGenerator = new FrameIdGenerator()};

            // Fill the animation with a few empty frames
            controller.CreateFrame();
            controller.CreateFrame();

            var atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings());

            atlas.InsertFramesFromAnimation(anim);

            var packer = new DefaultTexturePacker();

            packer.Pack(atlas);

            Assert.AreEqual(new Rectangle(0, 0, 1, 1), atlas.AtlasRectangle, "Packing a texture atlas that contains empty frames should result in an atlas with a 1x1 area");
        }
    }
}
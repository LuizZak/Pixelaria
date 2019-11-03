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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Controllers.Exporters.Pixelaria;
using Pixelaria.Controllers.Exporters.Unity;
using Pixelaria.Data;
using Pixelaria.Data.Persistence;
using Pixelaria.Data.Persistence.PixelariaFileBlocks;
using PixelariaTests.TestGenerators;

namespace PixelariaTests.Data.Persistence
{
    [TestClass]
    public class PixelariaFileTests
    {
        [TestMethod]
        public void TestDefaultFileBlocks()
        {
            var memoryStream = new MemoryStream();
            var bundle = new Bundle("bundle");
            var sut = new PixelariaFile(bundle, memoryStream);

            sut.AddDefaultBlocks();

            Assert.AreEqual(1, sut.GetBlocksByType(typeof(AnimationSheetBlock)).Length);
            Assert.AreEqual(1, sut.GetBlocksByType(typeof(ProjectTreeBlock)).Length);
            Assert.AreEqual(1, sut.GetBlocksByType(typeof(ExporterNameBlock)).Length);

            sut.Dispose();
        }

        [TestMethod]
        public void TestAddsAnimationHeaderBlocks()
        {
            var memoryStream = new MemoryStream();
            var bundle = BundleGenerator.GenerateTestBundle(0, 2, 2);
            var sut = new PixelariaFile(bundle, memoryStream);

            sut.AddDefaultBlocks();

            Assert.AreEqual(4, sut.GetBlocksByType(typeof(AnimationHeaderBlock)).Length);
        }

        [TestMethod]
        public void TestAddsExporterSettingsBlocks()
        {
            var memoryStream = new MemoryStream();
            var bundle = BundleGenerator.GenerateTestBundle(0);
            bundle.ExporterSettingsMap["abc"] = new UnityExporter.Settings();
            bundle.ExporterSettingsMap["def"] = new PixelariaExporter.Settings();
            var sut = new PixelariaFile(bundle, memoryStream);

            sut.AddDefaultBlocks();

            Assert.AreEqual(2, sut.GetBlocksByType(typeof(ExporterSettingsBlock)).Length);
        }

        [TestMethod]
        public void TestAddsKeyframeBlocks()
        {
            var stream = new MemoryStream();
            var bundle = BundleGenerator.GenerateTestBundle(0);
            bundle.Animations[0].Frames[0].KeyframeMetadata["point"] = new Point(0, 1);
            var sut = new PixelariaFile(bundle, stream);

            sut.PrepareBlocksWithBundle();

            Assert.AreEqual(1, sut.GetBlocksByType(typeof(KeyframeBlock)).Length);
        }

        [TestMethod]
        public void TestConstructBundleAssignsKeyframes()
        {
            var stream = new MemoryStream();
            var bundle = BundleGenerator.GenerateTestBundle(0, 1, 1, 1);
            bundle.Animations[0].Frames[0].KeyframeMetadata["point"] = new Point(1, 2);
            var sut = new PixelariaFile(bundle, stream);
            sut.AddBlock(new AnimationHeaderBlock(bundle.Animations[0]));
            sut.AddBlock(new FrameBlock(bundle.Animations[0].Frames[0]));
            sut.Save();

            var result = sut.ConstructBundle();

            Assert.AreEqual(1, result.Animations.Count);
            Assert.AreEqual(1, result.Animations[0].FrameCount);
            Assert.AreEqual(new Point(1, 2), result.Animations[0].Frames[0].KeyframeMetadata["point"]);

        }
    }
}

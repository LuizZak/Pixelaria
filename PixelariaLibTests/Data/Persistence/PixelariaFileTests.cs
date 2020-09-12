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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Controllers.Exporters.Pixelaria;
using PixelariaLib.Controllers.Exporters.Unity;
using PixelariaLib.Data;
using PixelariaLib.Data.Persistence;
using PixelariaLib.Data.Persistence.PixelariaFileBlocks;
using PixelariaLibTests.TestGenerators;

namespace PixelariaLibTests.Data.Persistence
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
    }
}

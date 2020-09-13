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
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Controllers.Exporters;
using PixelariaLib.Controllers.Exporters.Unity;
using PixelariaLib.Data;
using PixelariaLib.Data.Persistence.PixelariaFileBlocks;
using PixelariaTests.TestUtils;

namespace PixelariaLibTests.Data.Persistence.PixelariaFileBlocks
{
    [TestClass]
    public class ExporterSettingsBlockTests
    {
        [TestMethod]
        public void TestRemoveOnPrepare()
        {
            var sut = new ExporterSettingsBlock();

            Assert.IsTrue(sut.RemoveOnPrepare);
        }

        [TestMethod]
        public void TestPrepareFromBundle()
        {
            var bundle = new Bundle("bundle");
            bundle.ExporterSettingsMap[UnityExporter.SerializedName] = new UnityExporter.Settings {GenerateAnimationControllers = false};
            var sut = new ExporterSettingsBlock(UnityExporter.SerializedName);

            sut.PrepareFromBundle(bundle);

            Assert.AreSame(bundle.ExporterSettingsMap[UnityExporter.SerializedName], sut.Settings);
        }

        [TestMethod]
        public void TestSaveToStream()
        {
            var settings = new UnityExporter.Settings {GenerateAnimationControllers = false};
            var expectedStream = GenerateTestStream(settings);
            var stream = new MemoryStream();
            var bundle = new Bundle("bundle");
            bundle.ExporterSettingsMap[UnityExporter.SerializedName] = settings;
            var sut = new ExporterSettingsBlock(UnityExporter.SerializedName);
            sut.PrepareFromBundle(bundle);

            sut.SaveToStream(stream);

            Assert.That.MemoryStreamMatches(stream, expectedStream);
        }

        [TestMethod]
        public void TestLoadContentFromStream()
        {
            var stream = new MemoryStream(GenerateTestStream(new UnityExporter.Settings{GenerateAnimationControllers = true}));
            var sut = new ExporterSettingsBlock();

            sut.LoadFromStream(stream);

            Assert.AreEqual(sut.SerializedExporterName, new UnityExporter.Settings().ExporterSerializedName);
            Assert.IsTrue(((UnityExporter.Settings) sut.Settings).GenerateAnimationControllers);
        }

        private static byte[] GenerateTestStream([NotNull] IBundleExporterSettings settings)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            // Header (ID, length, version)
            writer.Write(FileBlock.BLOCKID_EXPORTER_SETTINGS);
            writer.Write((long) 11);
            writer.Write((short) 0);
            // settings block
            writer.Write(settings.ExporterSerializedName);
            settings.Save(stream);

            return stream.GetBuffer().Take((int) stream.Length).ToArray();
        }
    }
}

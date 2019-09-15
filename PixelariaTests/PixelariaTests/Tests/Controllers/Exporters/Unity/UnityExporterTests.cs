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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Controllers.Exporters;
using Pixelaria.Controllers.Exporters.Unity;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Controllers.Exporters.Unity
{
    [TestClass]
    public class UnityExporterTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Directory.CreateDirectory(GetTemporaryDirectoryPath());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Delete(GetTemporaryDirectoryPath(), true);
        }

        [TestMethod]
        public void TestExportBundle()
        {
            var expectedFiles = new[]
            {
                "Sheet0.png", 
                "Sheet0.png.meta", 
                "Sheet0Animation0.anim", 
                "Sheet0Animation0.anim.meta",
                "Sheet0Animation0.controller", 
                "Sheet0Animation0.controller.meta"
            };
            var sut = CreateUnityExporter();
            var bundle = BundleGenerator.GenerateTestBundle(0, 1, 1);
            bundle.ExportPath = GetTemporaryDirectoryPath();

            sut.ExportBundleConcurrent(bundle).Wait();

            var files = Directory.GetFiles(bundle.ExportPath).Select(Path.GetFileName).ToArray();
            
            Assert.IsTrue(new HashSet<string>(files).SetEquals(expectedFiles), 
                $"Expected list of files [{string.Join(", ",expectedFiles)}], but received [{string.Join(", ", files)}]");
        }

        [TestMethod]
        public void TestExportBundleSkipsAnimationFilesForAnimationsWithZeroFps()
        {
            var expectedFiles = new[]
            {
                "Sheet0.png",
                "Sheet0.png.meta"
            };
            var sut = CreateUnityExporter();
            var bundle = BundleGenerator.GenerateTestBundle(0, 1, 1);
            var playbackSettings = bundle.Animations[0].PlaybackSettings;
            playbackSettings.FPS = 0;
            bundle.Animations[0].PlaybackSettings = playbackSettings;
            bundle.ExportPath = GetTemporaryDirectoryPath();

            sut.ExportBundleConcurrent(bundle).Wait();

            var files = Directory.GetFiles(bundle.ExportPath).Select(Path.GetFileName).ToArray();

            Assert.IsTrue(new HashSet<string>(files).SetEquals(expectedFiles),
                $"Expected list of files [{string.Join(", ", expectedFiles)}], but received [{string.Join(", ", files)}]");
        }

        private static string GetTemporaryDirectoryPath()
        {
            string path = Path.Combine(Path.GetTempPath(), "_tempPixelaria");
            return path;
        }

        private static UnityExporter CreateUnityExporter(ISheetExporter sheetExporter = null)
        {
            return new UnityExporter(sheetExporter ?? new DefaultSheetExporter());
        }
    }
}

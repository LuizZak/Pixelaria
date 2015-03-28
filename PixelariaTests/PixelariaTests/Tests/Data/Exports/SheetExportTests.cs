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

using System;
using System.Drawing;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Data.Exporters;
using Pixelaria.Utils;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Data.Exports
{
    /// <summary>
    /// Tests the DefaultPngExporter, BundleSheetExport, and TextureAtlas functionalities and related components
    /// </summary>
    [TestClass]
    public class SheetExportTests
    {
        /// <summary>
        /// Path of the temporary folder used to store export sheets
        /// </summary>
        private string _tempExportPath;

        /// <summary>
        /// The animation sheet that was originally exported
        /// </summary>
        public AnimationSheet OriginalSheet;
        /// <summary>
        /// The animation sheet that was imported from disk
        /// </summary>
        public AnimationSheet SheetFromDisk;

        /// <summary>
        /// Tests sheet generation by creating an animation sheet, exporting it, importing it back up and comparing the similarities to the pixel level.
        /// </summary>
        [TestMethod]
        public void TestSheetExportConsistency()
        {
            AnimationExportSettings[] permSettings = AnimationSheetGenerator.GetExportSettingsPermutations();

            foreach (var settings in permSettings)
            {
                TestSheetExportWithSettings(settings);
                TestTeardown();
            }
        }

        /// <summary>
        /// Tests a sheet export procedure with a given export settings struct
        /// </summary>
        /// <param name="settings">The export settings to use in this test</param>
        /// <param name="failMessage">The message to print if the test fails</param>
        private void TestSheetExportWithSettings(AnimationExportSettings settings, string failMessage = "Exported animation sheets should be equivalent to their original sheets")
        {
            // In theory, if you export a sheet and import it back just the way it was described on the generated XML file, it will equal the original sheet completely
            OriginalSheet = new AnimationSheet("Sheet1");
            OriginalSheet.ExportSettings = settings;

            for (int i = 0; i < 10; i++)
            {
                int animationWidth = 12 + i / 2;
                int animationHeight = 12 + i / 2;

                OriginalSheet.AddAnimation(AnimationGenerator.GenerateAnimation("Anim" + OriginalSheet.AnimationCount,
                    animationWidth, animationHeight, 10, OriginalSheet.AnimationCount * 2));
            }

            // Generate export path
            _tempExportPath = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
            Directory.CreateDirectory(_tempExportPath);

            string exportPath = _tempExportPath + Path.DirectorySeparatorChar + OriginalSheet.Name;
            string xmlPath = exportPath + ".xml";

            // Export and save to disk
            IBundleExporter exporter = new DefaultPngExporter();
            exporter.ExportBundleSheet(OriginalSheet.ExportSettings, OriginalSheet.Animations)
                .SaveToDisk(_tempExportPath + Path.DirectorySeparatorChar + OriginalSheet.Name);

            // Import it back up
            SheetFromDisk = (AnimationSheet)ImportBundle(xmlPath);
            SheetFromDisk.ExportSettings = OriginalSheet.ExportSettings;

            Assert.AreEqual(OriginalSheet, SheetFromDisk, failMessage);
        }

        [TestCleanup]
        public void TestTeardown()
        {
            try
            {
                Directory.Delete(_tempExportPath, true);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Import a bundle from .png and .xml pair files
        /// </summary>
        /// <param name="bundlePath">The common path name of the .png and .xml bundle, with a .xml extension</param>
        public static object ImportBundle(string bundlePath)
        {
            var xml = new XmlDocument();
            xml.Load(bundlePath);

            // Read the sheet file path
            foreach (XmlNode childNode in xml.ChildNodes)
            {
                if (childNode.Name == "sheet")
                {
                    string path = Path.GetDirectoryName(bundlePath) + "\\" + Path.GetFileName(childNode.Attributes["file"].InnerText);

                    byte[] bytes = File.ReadAllBytes(path);

                    Bitmap texture = null;

                    using(MemoryStream stream = new MemoryStream(bytes))
                    {
                        Image original = Image.FromStream(stream);

                        texture = (Bitmap)original.Clone();

                        original.Dispose();
                    }

                    return ImportAnimationSheet(texture, xml);
                }
            }

            return null;
        }

        /// <summary>
        /// Imports a bundle composed from the given Texture2D and XmlDocument file
        /// </summary>
        /// <param name="texture">The Texture2D sheet</param>
        /// <param name="document">The .xml sheet description</param>
        public static AnimationSheet ImportAnimationSheet(Bitmap texture, XmlDocument document)
        {
            XmlNode sheetNode = document.ChildNodes[1];

            // Load the animations from the .xml
            AnimationSheet sheet = new AnimationSheet(Path.GetFileNameWithoutExtension(sheetNode.Attributes["file"].Value));

            foreach (XmlNode animNode in sheetNode.ChildNodes)
            {
                // Load the animation properties
                string animName = animNode.Attributes["name"].InnerText;
                int animWidth = int.Parse(animNode.Attributes["width"].InnerText);
                int animHeight = int.Parse(animNode.Attributes["height"].InnerText);
                int fps = int.Parse(animNode.Attributes["fps"].InnerText);
                bool frameskip = animNode.Attributes["frameskip"].InnerText == "true";

                Animation anim = new Animation(animName, animWidth, animHeight);

                var playbackSettings = anim.PlaybackSettings;
                playbackSettings.FPS = fps;
                playbackSettings.FrameSkip = frameskip;
                anim.PlaybackSettings = playbackSettings;

                foreach (XmlNode frameNode in animNode.ChildNodes)
                {
                    int index = int.Parse(frameNode.Attributes["index"].InnerText);
                    int sheetX = int.Parse(frameNode.Attributes["sheetX"].InnerText);
                    int sheetY = int.Parse(frameNode.Attributes["sheetY"].InnerText);
                    int sheetW = int.Parse(frameNode.Attributes["sheetW"].InnerText);
                    int sheetH = int.Parse(frameNode.Attributes["sheetH"].InnerText);
                    int frameX = int.Parse(frameNode.Attributes["frameX"].InnerText);
                    int frameY = int.Parse(frameNode.Attributes["frameY"].InnerText);
                    int frameW = int.Parse(frameNode.Attributes["frameW"].InnerText);
                    int frameH = int.Parse(frameNode.Attributes["frameH"].InnerText);

                    Rectangle bounds = new Rectangle(sheetX, sheetY, sheetW, sheetH);
                    Rectangle origins = new Rectangle(frameX, frameY, frameW, frameH);

                    Bitmap frame = SliceImage(texture, new Size(animWidth, animHeight), bounds, origins);

                    anim.CreateFrame().SetFrameBitmap(frame);
                }

                sheet.AddAnimation(anim);
            }

            return sheet;
        }

        /// <summary>
        /// Returns an image that represents the slice of another image
        /// </summary>
        /// <param name="original">The original image to get the slice of</param>
        /// <param name="sliceSize">The size of the sliced image</param>
        /// <param name="bounds">The bounds of the original image to slice</param>
        /// <param name="origin">A rectangle that represents the rectangle the image will take in the sliced image</param>
        /// <returns>A slice of the original image that fits within the specified bounds</returns>
        public static Bitmap SliceImage(Bitmap original, Size sliceSize, Rectangle bounds, Rectangle origin = new Rectangle())
        {
            Bitmap ret = new Bitmap(sliceSize.Width, sliceSize.Height, original.PixelFormat);

            FastBitmap.CopyRegion(original, ret, bounds, origin);

            return ret;
        }
    }
}
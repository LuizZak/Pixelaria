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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Controllers.Exporters;
using Pixelaria.Controllers.Exporters.Pixelaria;
using Pixelaria.Data;
using PixelariaTests.TestGenerators;

namespace PixelariaTests.Data.Exports
{
    /// <summary>
    /// Tests the <see cref="DefaultSheetExporter"/>, <see cref="PixelariaExporter"/>, <see cref="Pixelaria.Data.Exports.BundleSheetExport"/>, and <see cref="Pixelaria.Data.Exports.TextureAtlas"/> functionalities and related components
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
            var permSettings = AnimationSheetGenerator.GetExportSettingsPermutations();

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
            // In theory, if you export a sheet and import it back just the way it was described on the generated json file, it will equal the original sheet completely
            OriginalSheet = new AnimationSheet("Sheet1")
            {
                ExportSettings = settings
            };

            for (int i = 0; i < 10; i++)
            {
                int animationWidth = 12 + i / 2;
                int animationHeight = 12 + i / 2;

                OriginalSheet.AddAnimation(AnimationGenerator.GenerateAnimation("Anim" + OriginalSheet.AnimationCount,
                    animationWidth, animationHeight, 10, OriginalSheet.AnimationCount * 2));
            }

            // Generate export path
            _tempExportPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempExportPath);

            string exportPath = _tempExportPath + Path.DirectorySeparatorChar + OriginalSheet.Name;
            string jsonPath = exportPath + ".json";

            // Export and save to disk
            var exporter = new DefaultSheetExporter();

            exporter.ExportBundleSheet(OriginalSheet).Result
                .SaveToDisk(_tempExportPath + Path.DirectorySeparatorChar + OriginalSheet.Name);

            // Export sheet temporarily
            for (int i = 0; i < OriginalSheet.Animations.Length; i++)
            {
                var animation = OriginalSheet.Animations[i];
                var image = exporter.GenerateSpriteStrip(new AnimationController(null, animation));

                var path = _tempExportPath + Path.DirectorySeparatorChar + OriginalSheet.Name + "_" + i + ".png";

                image.Save(path, ImageFormat.Png);
            }

            // Import it back up
            SheetFromDisk = (AnimationSheet)ImportSheetFile(jsonPath);
            SheetFromDisk.ExportSettings = OriginalSheet.ExportSettings;

            Assert.AreEqual(OriginalSheet, SheetFromDisk, failMessage);
        }

        [TestCleanup]
        public void TestTeardown()
        {
            try
            {
                //Directory.Delete(_tempExportPath, true);
            }
            catch (Exception)
            {
                // unused
            }
        }

        /// <summary>
        /// Import a bundle from .png and .json pair files
        /// </summary>
        /// <param name="sheetPath">The common path name of the .png and .json bundle, with a .json extension</param>
        public static object ImportSheetFile(string sheetPath)
        {
            string jsonPath = Path.ChangeExtension(sheetPath, "json");

            Debug.Assert(jsonPath != null, "jsonPath != null");
            string json = File.ReadAllText(jsonPath);

            var sheet = (JObject)JsonConvert.DeserializeObject(json);
            
            var file = (string)sheet.SelectToken("sprite_image");
            if (file == null)
                return null;

            string path = Path.GetDirectoryName(sheetPath) + "\\" + Path.GetFileName(file);

            byte[] bytes = File.ReadAllBytes(path);
            Bitmap texture;
            using (var stream = new MemoryStream(bytes))
            {
                var original = Image.FromStream(stream);

                texture = (Bitmap)original.Clone();

                original.Dispose();
            }

            return ImportAnimationSheet(texture, sheet);
        }

        /// <summary>
        /// Imports a bundle composed from the given Texture2D and JObject (json) file
        /// </summary>
        /// <param name="texture">The Texture2D sheet</param>
        /// <param name="json">The .json sheet description</param>
        public static AnimationSheet ImportAnimationSheet(Bitmap texture, [NotNull] JObject json)
        {
            // Imports a JSON formatted as follows:
            /*
            {
                "sprite_image": "<name>.png",
                "animations": [
                    {
                        "name": "<name>",
                        "width": 24,
                        "height": 23,
                        "fps": 14,
                        "frameskip": false,
                        "frames": [
                            {
                                "sheet": {
                                    "x": 58,
                                    "y": 47,
                                    "width": 15,
                                    "height": 21
                                },
                                "frame": {
                                    "x": 3,
                                    "y": 2,
                                    "width": 15,
                                    "height": 21
                                }
                            }
                        ]
                    }
                ]
            }
            */

            var sheet = new AnimationSheet(Path.GetFileNameWithoutExtension((string)json.SelectToken("sprite_image")));

            foreach (var child in json.SelectToken("animations").Children())
            {
                // Load the animation properties
                string animName = (string)child.SelectToken("name");
                int animWidth = (int)child.SelectToken("width");
                int animHeight = (int)child.SelectToken("height");
                int fps = (int)child.SelectToken("fps");
                bool frameskip = (bool)child.SelectToken("frameskip");

                var anim = new Animation(animName, animWidth, animHeight);
                
                var controller = new AnimationController(null, anim);

                var playbackSettings = anim.PlaybackSettings;
                playbackSettings.FPS = fps;
                playbackSettings.FrameSkip = frameskip;
                anim.PlaybackSettings = playbackSettings;
                
                foreach (var frameNode in child.SelectToken("frames").Children())
                {
                    var frameSheet = frameNode.SelectToken("sheet");
                    var frameLocal = frameNode.SelectToken("frame");

                    int sheetX = (int)frameSheet.SelectToken("x");
                    int sheetY = (int)frameSheet.SelectToken("y");
                    int sheetW = (int)frameSheet.SelectToken("width");
                    int sheetH = (int)frameSheet.SelectToken("height");
                    int frameX = (int)frameLocal.SelectToken("x");
                    int frameY = (int)frameLocal.SelectToken("y");
                    int frameW = (int)frameLocal.SelectToken("width");
                    int frameH = (int)frameLocal.SelectToken("height");

                    var bounds = new Rectangle(sheetX, sheetY, sheetW, sheetH);
                    var origins = new Rectangle(frameX, frameY, frameW, frameH);

                    var frame = SliceImage(texture, new Size(animWidth, animHeight), bounds, origins);

                    var frameId = controller.CreateFrame();
                    controller.GetFrameController(frameId).SetFrameBitmap(frame);
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
        public static Bitmap SliceImage([NotNull] Bitmap original, Size sliceSize, Rectangle bounds, Rectangle origin = new Rectangle())
        {
            var ret = new Bitmap(sliceSize.Width, sliceSize.Height, original.PixelFormat);

            FastBitmap.CopyRegion(original, ret, bounds, origin);

            return ret;
        }
    }
}
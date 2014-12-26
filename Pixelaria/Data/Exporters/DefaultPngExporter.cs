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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using Pixelaria.Algorithms;
using Pixelaria.Algorithms.Packers;
using Pixelaria.Data.Exports;
using Pixelaria.Utils;

namespace Pixelaria.Data.Exporters
{
    /// <summary>
    /// Default exporter that uses PNG as the texture format
    /// </summary>
    public class DefaultPngExporter : IBundleExporter
    {
        /// <summary>
        /// Exports the given Bundle
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        public void ExportBundle(Bundle bundle, BundleExportProgressEventHandler progressHandler = null)
        {
            // The total final stage count is two times the sheet array size (one stage for atlasses, and another stage for saving to disk, for each sheet)
            int totalStages = bundle.AnimationSheets.Count(sheet => sheet.Animations.Length > 0);

            // Create a proxy handler to handle total progress
            float totalProgress;
            float stages = totalStages * 2 + 1;
            float currentStage = 0;

            BundleExportProgressEventHandler proxyHandler = null;

            // Create the lists needed for the export
            List<string> xmls = new List<string>();
            List<BundleSheetExport> bundleSheetList = new List<BundleSheetExport>();

            if (progressHandler != null)
            {
                var stage = currentStage;
                proxyHandler = args =>
                {
                    totalProgress = ((stage + (float)args.StageProgress / 100) / stages);

                    // Calculate total progress
                    progressHandler.Invoke(new BundleExportProgressEventArgs(args.ExportStage, args.StageProgress, (int)Math.Floor(totalProgress * 100), args.StageDescription));
                };
            }

            // Export all the animation sheets now

            // 
            // 1. Export Animation Sheets
            // 
            foreach (AnimationSheet sheet in bundle.AnimationSheets)
            {
                if (sheet.Animations.Length > 0)
                {
                    BundleSheetExport exp = ExportBundleSheet(sheet, proxyHandler);

                    xmls.Add(Path.GetFullPath(bundle.ExportPath) + "\\" + sheet.Name);

                    bundleSheetList.Add(exp);
                }

                currentStage++;
            }

            //
            // 2. Save the sheets to disk
            //
            for(int i = 0; i < bundleSheetList.Count; i++)
            {
                BundleSheetExport exp = bundleSheetList[i];

                if (proxyHandler != null)
                {
                    int progress = (int)((float)i / bundleSheetList.Count * 100);
                    proxyHandler.Invoke(new BundleExportProgressEventArgs(BundleExportStage.SavingToDisk, progress, progress));
                }

                exp.SaveToDisk(xmls[i]);

                currentStage++;
            }

            //
            // 3. Compose the main bundle .xml
            //
            XmlDocument xml = new XmlDocument();

            xml.AppendChild(xml.CreateNode(XmlNodeType.XmlDeclaration, "sheetList", ""));

            XmlNode rootNode = xml.CreateNode(XmlNodeType.Element, "sheetList", "");

            // Count number of exported sheets
            int expCount = 0;
            // Append the animation sheets now
            for(int i = 0; i < xmls.Count; i++)
            {
                if (!bundleSheetList[i].ExportSettings.ExportXml)
                    continue;

                expCount++;

                string sheetXml = xmls[i];

                XmlNode sheetNode = xml.CreateNode(XmlNodeType.Element, "sheet", "");

                if (sheetNode.Attributes != null)
                    sheetNode.Attributes.Append(xml.CreateAttribute("path")).InnerText = Utilities.GetRelativePath(sheetXml + ".xml", bundle.ExportPath);

                rootNode.AppendChild(sheetNode);
            }

            if (expCount > 0)
            {
                xml.AppendChild(rootNode);
                xml.Save(Path.GetFullPath(bundle.ExportPath) + "\\" + bundle.Name + ".xml");
            }

            if (proxyHandler != null)
            {
                proxyHandler.Invoke(new BundleExportProgressEventArgs(BundleExportStage.Ended, 100, 100));
            }
        }

        /// <summary>
        /// Exports the given animations into an image sheet and returns the created sheet
        /// </summary>
        /// <param name="exportSettings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>An image sheet representing the animations passed</returns>
        public Image ExportAnimationSheet(AnimationExportSettings exportSettings, Animation[] anims, BundleExportProgressEventHandler progressHandler = null)
        {
            TextureAtlas atlas = GenerateAtlasFromAnimations(exportSettings, anims, "", progressHandler);

            return atlas.GenerateSheet();
        }

        /// <summary>
        /// Exports the given animation sheet into an image sheet and returns the created sheet
        /// </summary>
        /// <param name="sheet">The sheet to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>An image sheet representing the animation sheet passed</returns>
        public Image ExportAnimationSheet(AnimationSheet sheet, BundleExportProgressEventHandler progressHandler = null)
        {
            Image image = GenerateAtlasFromAnimationSheet(sheet).GenerateSheet();

            return image;
        }

        /// <summary>
        /// Exports the given animations into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="sheet">The sheet to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animation sheet passed ready to be saved to disk</returns>
        public BundleSheetExport ExportBundleSheet(AnimationSheet sheet, BundleExportProgressEventHandler progressHandler = null)
        {
            //
            // 1. Generate texture atlas
            //
            using (TextureAtlas atlas = GenerateAtlasFromAnimationSheet(sheet, progressHandler))
            {
                //
                // 2. Generate an export sheet from the texture atlas
                //
                return BundleSheetExport.FromAtlas(atlas);
            }
        }

        /// <summary>
        /// Exports the given animations into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="settings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animations passed ready to be saved to disk</returns>
        public BundleSheetExport ExportBundleSheet(AnimationExportSettings settings, Animation[] anims, BundleExportProgressEventHandler progressHandler = null)
        {
            using (TextureAtlas atlas = GenerateAtlasFromAnimations(settings, anims, "", progressHandler))
            {
                return BundleSheetExport.FromAtlas(atlas);
            }
        }

        /// <summary>
        /// Generates a TextureAtlas from the given AnimationSheet object
        /// </summary>
        /// <param name="sheet">The AnimationSheet to generate the TextureAtlas of</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A TextureAtlas generated from the given AnimationSheet</returns>
        public TextureAtlas GenerateAtlasFromAnimationSheet(AnimationSheet sheet, BundleExportProgressEventHandler progressHandler = null)
        {
            return GenerateAtlasFromAnimations(sheet.ExportSettings, sheet.Animations, sheet.Name, progressHandler);
        }

        /// <summary>
        /// Exports the given animations into an image sheet and returns the created sheet
        /// </summary>
        /// <param name="exportSettings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <param name="name">The name for the generated texture atlas. Used for progress reports</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>An image sheet representing the animations passed</returns>
        public TextureAtlas GenerateAtlasFromAnimations(AnimationExportSettings exportSettings, Animation[] anims, string name = "", BundleExportProgressEventHandler progressHandler = null)
        {
            TextureAtlas atlas = new TextureAtlas(exportSettings, name);

            //
            // 1. Add the frames to the texture atlas
            //
            foreach (Animation anim in anims)
            {
                for (int i = 0; i < anim.FrameCount; i++)
                {
                    atlas.InsertFrame(anim.GetFrameAtIndex(i));
                }
            }

            //
            // 2. Pack the frames into the atlas
            //
            ITexturePacker packer = new DefaultTexturePacker();
            packer.Pack(atlas, progressHandler);

            return atlas;
        }
    }
}
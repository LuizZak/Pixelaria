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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Pixelaria.Data;
using Pixelaria.Data.Exports;

using Pixelaria.Utils;

namespace Pixelaria.Exporters
{
    /// <summary>
    /// Default exporter that uses PNG as the texture format
    /// </summary>
    public class DefaultPngExporter : IDefaultExporter
    {
        /// <summary>
        /// Exports the given Bundle
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        public void ExportBundle(Bundle bundle, BundleExportProgressEventHandler progressHandler = null)
        {
            // Create a proxy handler to handle total progress
            float totalProgress = 0;
            float stages = 0;
            float currentStage = 0;

            BundleExportProgressEventHandler proxyHandler = null;

            if(progressHandler != null)
            {
                proxyHandler = new BundleExportProgressEventHandler(
                    (BundleExportProgressEventArgs args) => {
                        totalProgress = ((currentStage + (float)args.StageProgress / 100) / stages);

                        // Calculate total progress
                        progressHandler.Invoke(new BundleExportProgressEventArgs(args.ExportStage, args.StageProgress, (int)Math.Floor(totalProgress * 100), args.StageDescription));
                    }
                );
            }

            // Create the lists needed for the export
            List<string> xmls = new List<string>();
            List<BundleSheetExport> bundleSheetList = new List<BundleSheetExport>();

            // The total final stage count is two times the sheet array size (one stage for atlasses, and another stage for saving to disk, for each sheet)
            int totalStages = 0;
            foreach (AnimationSheet sheet in bundle.AnimationSheets)
            {
                if (sheet.Animations.Length > 0)
                {
                    totalStages++;
                }
            }
            stages = totalStages * 2 + 1;

            // Export all the animation sheets now
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

            // Save the sheets to disk
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

            // Compose the main bundle .xml
            XmlDocument xml = new XmlDocument();

            xml.AppendChild(xml.CreateNode(XmlNodeType.XmlDeclaration, "sheetList", ""));

            XmlNode rootNode = xml.CreateNode(XmlNodeType.Element, "sheetList", "");

            // Append the animation sheets now
            foreach (string sheetXml in xmls)
            {
                XmlNode sheetNode = xml.CreateNode(XmlNodeType.Element, "sheet", "");

                sheetNode.Attributes.Append(xml.CreateAttribute("path")).InnerText = Utilities.GetRelativePath(sheetXml + ".xml", bundle.ExportPath);

                rootNode.AppendChild(sheetNode);
            }

            xml.AppendChild(rootNode);
            xml.Save(Path.GetFullPath(bundle.ExportPath) + "\\" + bundle.Name + ".xml");

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
            TextureAtlas atlas = new TextureAtlas(exportSettings);

            // Pack the frames into the atlas
            foreach (Animation anim in anims)
            {
                for (int i = 0; i < anim.FrameCount; i++)
                {
                    atlas.InsertFrame(anim.GetFrameAtIndex(i));
                }
            }

            atlas.Pack();

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
            using (TextureAtlas atlas = GenerateAtlasFromAnimationSheet(sheet, progressHandler))
            {
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
            using (TextureAtlas atlas = new TextureAtlas(settings))
            {
                // Pack the frames into the atlas
                foreach (Animation anim in anims)
                {
                    for (int i = 0; i < anim.FrameCount; i++)
                    {
                        atlas.InsertFrame(anim.GetFrameAtIndex(i));
                    }
                }

                atlas.Pack(progressHandler);

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
            TextureAtlas atlas = new TextureAtlas(sheet.ExportSettings, sheet.Name);

            // Pack the frames into the atlas
            foreach (Animation anim in sheet.Animations)
            {
                for (int i = 0; i < anim.FrameCount; i++)
                {
                    atlas.InsertFrame(anim.GetFrameAtIndex(i));
                }
            }

            atlas.Pack(progressHandler);

            return atlas;
        }
    }
}
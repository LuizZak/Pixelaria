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
using System.Xml;

using Pixelaria.Utils;

namespace Pixelaria.Data.Exports
{
    /// <summary>
    /// Describes an exported Bundle Sheet containing a sequence of animations, and data pertaining to the animations
    /// </summary>
    public class BundleSheetExport : IDisposable
    {
        /// <summary>
        /// The list of FrameRect objects inside this BundleSheetExport
        /// </summary>
        FrameRect[] frameRects;

        /// <summary>
        /// The list of animations in this BundleSheet
        /// </summary>
        Animation[] animations;

        /// <summary>
        /// Export settings to be used when exporting the Bundle Sheet
        /// </summary>
        AnimationExportSettings exportSettings;

        /// <summary>
        /// The frame sheet itself
        /// </summary>
        Image sheet;

        /// <summary>
        /// The number of repeated frames
        /// </summary>
        int reusedFrameCount;

        /// <summary>
        /// The frame sheet
        /// </summary>
        public Image Sheet { get { return sheet; } }

        /// <summary>
        /// Gets the number of frames on this BundleSheetExport
        /// </summary>
        public int FrameCount { get { return frameRects.Length; } }

        /// <summary>
        /// Gets the number of reused frames on this BundleSheetExport
        /// </summary>
        public int ReusedFrameCount { get { return reusedFrameCount; } }

        /// <summary>
        /// Gets the FrameRect for the frame at the given index on this BundleSheetExport
        /// </summary>
        /// <param name="i">An index</param>
        /// <returns>The FrameRect stored at that index</returns>
        public FrameRect this[int i] { get { return frameRects[i]; } }

        /// <summary>
        /// Gets the array of FrameRect objects inside this BundleSheetExport
        /// </summary>
        public FrameRect[] FrameRects { get { return frameRects; } }

        /// <summary>
        /// Gets or sets the export settings to be used when exporting the Bundle Sheet
        /// </summary>
        public AnimationExportSettings ExportSettings { get { return exportSettings; } }

        /// <summary>
        /// Default constructor for the BundleSheetExport class
        /// </summary>
        private BundleSheetExport()
        {

        }

        /// <summary>
        /// Disposes of this bundle sheet and all resources allocated by it
        /// </summary>
        public void Dispose()
        {
            this.sheet.Dispose();
        }

        /// <summary>
        /// Saves the contents of this BundleSheetExport to disk, using the given path
        /// as a base path, and savind the .png and .xml as that base path
        /// </summary>
        /// <param name="basePath">The base path to export the animations as</param>
        public void SaveToDisk(string basePath)
        {
            SaveToDisk(Path.GetFullPath(basePath) + ".png", Path.GetFullPath(basePath) + ".xml");
        }

        /// <summary>
        /// Saves the contents of this BundleSheetExport to the disk
        /// </summary>
        /// <param name="sheetPath">The path to the sprite sheet to save</param>
        /// <param name="xmlPath">The path to the XML file containing the data for the animations</param>
        public void SaveToDisk(string sheetPath, string xmlPath)
        {
            // Save the sprite sheet first
            sheet.Save(sheetPath, ImageFormat.Png);

            // Early quit - The xml generation is disabled
            if (!exportSettings.ExportXml)
                return;

            // Compose the XML file now
            XmlDocument xml = new XmlDocument();

            xml.AppendChild(xml.CreateNode(XmlNodeType.XmlDeclaration, "sheet", ""));

            XmlNode rootNode = xml.CreateNode(XmlNodeType.Element, "sheet", "");

            rootNode.Attributes.Append(xml.CreateAttribute("file")).InnerText = Utilities.GetRelativePath(sheetPath, Path.GetDirectoryName(xmlPath));

            // Append the animation sheets now
            foreach (Animation anim in animations)
            {
                XmlNode animationNode = xml.CreateNode(XmlNodeType.Element, "anim", "");

                animationNode.Attributes.Append(xml.CreateAttribute("name")).InnerText = anim.Name;
                animationNode.Attributes.Append(xml.CreateAttribute("width")).InnerText = anim.Width + "";
                animationNode.Attributes.Append(xml.CreateAttribute("height")).InnerText = anim.Height + "";
                animationNode.Attributes.Append(xml.CreateAttribute("fps")).InnerText = anim.PlaybackSettings.FPS + "";
                animationNode.Attributes.Append(xml.CreateAttribute("frameskip")).InnerText = anim.PlaybackSettings.FrameSkip.ToString().ToLower();

                // Write down the frame bounds now
                for (int i = 0; i < anim.FrameCount; i++)
                {
                    Frame frame = anim.GetFrameAtIndex(i);

                    if (!ContainsFrame(frame))
                        continue;

                    FrameRect rect = GetFrameRectForFrame(frame);

                    XmlNode frameNode = xml.CreateNode(XmlNodeType.Element, "frame", "");

                    frameNode.Attributes.Append(xml.CreateAttribute("index")).InnerText = frame.Index + "";
                    frameNode.Attributes.Append(xml.CreateAttribute("sheetX")).InnerText = rect.SheetArea.X - (exportSettings.UsePaddingOnXml ? exportSettings.XPadding / 2 : 0) + "";
                    frameNode.Attributes.Append(xml.CreateAttribute("sheetY")).InnerText = rect.SheetArea.Y - (exportSettings.UsePaddingOnXml ? exportSettings.YPadding / 2 : 0) + "";
                    frameNode.Attributes.Append(xml.CreateAttribute("sheetW")).InnerText = rect.SheetArea.Width + (exportSettings.UsePaddingOnXml ? exportSettings.XPadding : 0) + "";
                    frameNode.Attributes.Append(xml.CreateAttribute("sheetH")).InnerText = rect.SheetArea.Height + (exportSettings.UsePaddingOnXml ? exportSettings.YPadding : 0) + "";

                    frameNode.Attributes.Append(xml.CreateAttribute("frameX")).InnerText = rect.FrameArea.X - (exportSettings.UsePaddingOnXml ? exportSettings.XPadding / 2 : 0) + "";
                    frameNode.Attributes.Append(xml.CreateAttribute("frameY")).InnerText = rect.FrameArea.Y - (exportSettings.UsePaddingOnXml ? exportSettings.YPadding / 2 : 0) + "";
                    frameNode.Attributes.Append(xml.CreateAttribute("frameW")).InnerText = rect.FrameArea.Width + "";
                    frameNode.Attributes.Append(xml.CreateAttribute("frameH")).InnerText = rect.FrameArea.Height + "";

                    animationNode.AppendChild(frameNode);
                }

                rootNode.AppendChild(animationNode);
            }

            xml.AppendChild(rootNode);

            xml.Save(xmlPath);
        }

        /// <summary>
        /// Returns whether or not the given frame is inside this BundleSheetExport
        /// </summary>
        /// <param name="frame">The frame to search for</param>
        /// <returns>True whether the given frame is inside this BundleSheetExport, false otherwise</returns>
        public bool ContainsFrame(Frame frame)
        {
            foreach (FrameRect frameRect in frameRects)
            {
                if (frameRect.Frame == frame)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the FrameRect object that represents the given Frame. If no FrameRect represents the given frame,
        /// null is returned.
        /// </summary>
        /// <param name="frame">The Frame to get the corresponding FrameRect</param>
        /// <returns>The FrameRect object that represents the given Frame. If no FrameRect represents the given frame, null is returned.</returns>
        public FrameRect GetFrameRectForFrame(Frame frame)
        {
            foreach (FrameRect frameRect in frameRects)
            {
                if (frameRect.Frame == frame)
                    return frameRect;
            }

            return null;
        }

        /// <summary>
        /// Creates a new BundleSheetExport from a TextureAtlas
        /// </summary>
        /// <param name="atlas">The TextureAtlas to import</param>
        /// <returns>A new BundleSheetExport created from the given TextureAtlas</returns>
        public static BundleSheetExport FromAtlas(TextureAtlas atlas)
        {
            //
            // 1. Generate final export image
            //
            Image image = atlas.GenerateSheet();

            // Import the frame rects to a bundle sheet now
            BundleSheetExport export = new BundleSheetExport();

            export.sheet = image;
            export.exportSettings = atlas.ExportSettings;
            export.animations = atlas.GetAnimationsOnAtlas();
            export.reusedFrameCount = atlas.Information.ReusedFrameOriginsCount;

            List<FrameRect> frameRectList = new List<FrameRect>();

            //
            // 2. Copy the frame bounds from the atlas to the bundle sheet
            //
            for (int i = 0; i < atlas.FrameCount; i++)
            {
                frameRectList.Add(new FrameRect(atlas.GetFrame(i), atlas.GetFrameBoundsRectangle(i), atlas.GetFrameOriginsRectangle(i)));
            }

            export.frameRects = frameRectList.ToArray();

            return export;
        }

        /// <summary>
        /// Describes a frame on this BundleSheetExport
        /// </summary>
        public class FrameRect
        {
            /// <summary>
            /// The Frame represented by this FrameRect
            /// </summary>
            private Frame frame;

            /// <summary>
            /// Represents the area the frame occupies inside the sheet
            /// </summary>
            private Rectangle sheetArea;

            /// <summary>
            /// Represents the area of the frame that is used on the sheet
            /// </summary>
            private Rectangle frameArea;

            /// <summary>
            /// Gets the Frame represented by this FrameRect
            /// </summary>
            public Frame Frame { get { return frame; } }

            /// <summary>
            /// Gets the area the frame occupies inside the sheet
            /// </summary>
            public Rectangle SheetArea { get { return sheetArea; } }

            /// <summary>
            /// Gets the area of the frame that is used on the sheet
            /// </summary>
            public Rectangle FrameArea { get { return frameArea; } }

            /// <summary>
            /// Creates a new FrameRect using the given parameters
            /// </summary>
            /// <param name="frame">The frame to represent on this FrameRect</param>
            /// <param name="sheetArea">The area the frame occupies inside the sheet</param>
            /// <param name="frameArea">The area of the frame that is used on the sheet</param>
            public FrameRect(Frame frame, Rectangle sheetArea, Rectangle frameArea)
            {
                this.frame = frame;
                this.sheetArea = sheetArea;
                this.frameArea = frameArea;

                // Clip the area to the frame size
                if (this.sheetArea.Width > this.frame.Width)
                    this.sheetArea.Width = this.frame.Width;
                if (this.sheetArea.Height > this.frame.Height)
                    this.sheetArea.Height = this.frame.Height;

                if (this.frameArea.Width > this.frame.Width)
                    this.frameArea.Width = this.frame.Width;
                if (this.frameArea.Height > this.frame.Height)
                    this.frameArea.Height = this.frame.Height;
            }
        }
    }

    /// <summary>
    /// The delegate used to report progress of bundle export
    /// </summary>
    /// <param name="progressArgs">The current export progress summary</param>
    public delegate void BundleExportProgressEventHandler(BundleExportProgressEventArgs progressArgs);

    /// <summary>
    /// Contains information about the progress of a bundle export
    /// </summary>
    public class BundleExportProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current export stage for the bundle
        /// </summary>
        public BundleExportStage ExportStage { get; private set; }

        /// <summary>
        /// Gets the current stage export progress.
        /// The progress ranges from 0 - 100
        /// </summary>
        public int StageProgress { get; private set; }

        /// <summary>
        /// Gets the total export progress.
        /// The progress ranges from 0 - 100
        /// </summary>
        public int TotalProgress { get; private set; }

        /// <summary>
        /// Gets the description of the current stage
        /// </summary>
        public string StageDescription { get; private set; }

        /// <summary>
        /// Initializes a new instance of the BundleExportProgressEventArgs
        /// </summary>
        /// <param name="exportStage">The current export stage for the bundle</param>
        /// <param name="stageProgress">The progress of the current export stage</param>
        /// <param name="totalProgress">The total progress of the export</param>
        /// <param name="stageDescription">An optional description for the current stage</param>
        public BundleExportProgressEventArgs(BundleExportStage exportStage, int stageProgress, int totalProgress, string stageDescription = "")
        {
            this.ExportStage = exportStage;
            this.StageProgress = stageProgress;
            this.TotalProgress = totalProgress;
            this.StageDescription = stageDescription;
        }
    }

    /// <summary>
    /// Describes the stage a bundle export is currently at
    /// </summary>
    public enum BundleExportStage
    {
        /// <summary>
        /// Specifies that the progress is currently at the texture atlas generation
        /// </summary>
        TextureAtlasGeneration,
        /// <summary>
        /// Specifies that the progress is currently at the saving to disk stage
        /// </summary>
        SavingToDisk,
        /// <summary>
        /// Specifies that the export process is over
        /// </summary>
        Ended
    }
}
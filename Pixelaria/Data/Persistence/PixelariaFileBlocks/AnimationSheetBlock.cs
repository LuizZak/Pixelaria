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
using System.IO;
using System.Linq;
using System.Text;

namespace Pixelaria.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Represents an Animation Sheet block on a pixelaria file
    /// </summary>
    public class AnimationSheetBlock : FileBlock
    {
        /// <summary>
        /// Initializes a new instance of the AnimationSheetBlock class
        /// </summary>
        public AnimationSheetBlock()
        {
            this.blockID = BLOCKID_ANIMATIONSHEET;
        }

        /// <summary>
        /// Loads the content portion of this block from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the content portion from</param>
        protected override void LoadContentFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            int animationSheetCount = reader.ReadInt32();

            // Load the animations now
            for (int i = 0; i < animationSheetCount; i++)
            {
                owningFile.LoadedBundle.AddAnimationSheet(LoadAnimationSheetFromStream(stream));
            }
        }

        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected override void SaveContentToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            AnimationSheet[] animationSheets = readyBundle.AnimationSheets;

            writer.Write(animationSheets.Length);

            foreach (var animationSheet in animationSheets)
            {
                SaveAnimationSheetToStream(animationSheet, stream);
            }
        }

        /// <summary>
        /// Loads an AnimationSheet from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the animation sheet from</param>
        /// <param name="parentBundle">The bundle that will contain this AnimationSheet</param>
        /// <returns>The Animation object loaded</returns>
        protected AnimationSheet LoadAnimationSheetFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            // Load the animation sheet data
            int id = reader.ReadInt32();
            string name = reader.ReadString();
            AnimationExportSettings settings = LoadExportSettingsFromStream(stream);

            // Create the animation sheet
            AnimationSheet sheet = new AnimationSheet(name);

            sheet.ID = id;
            sheet.ExportSettings = settings;

            // Load the animation indices
            int animCount = reader.ReadInt32();

            for (int i = 0; i < animCount; i++)
            {
                Animation anim = owningFile.LoadedBundle.GetAnimationByID(reader.ReadInt32());

                if (anim != null)
                {
                    sheet.AddAnimation(anim);
                }
            }

            return sheet;
        }

        /// <summary>
        /// Loads an AnimationExportSettings from the given stream, using the specified
        /// version number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the export settings from</param>
        /// <returns>The AnimationExportSettings object loaded</returns>
        protected AnimationExportSettings LoadExportSettingsFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            AnimationExportSettings settings = new AnimationExportSettings();

            settings.FavorRatioOverArea = reader.ReadBoolean();
            settings.ForcePowerOfTwoDimensions = reader.ReadBoolean();
            settings.ForceMinimumDimensions = reader.ReadBoolean();
            settings.ReuseIdenticalFramesArea = reader.ReadBoolean();
            settings.HighPrecisionAreaMatching = reader.ReadBoolean();
            settings.AllowUnorderedFrames = reader.ReadBoolean();
            settings.UseUniformGrid = reader.ReadBoolean();
            settings.UsePaddingOnXml = reader.ReadBoolean();
            settings.ExportXml = reader.ReadBoolean();
            settings.XPadding = reader.ReadInt32();
            settings.YPadding = reader.ReadInt32();

            return settings;
        }

        /// <summary>
        /// Saves the given AnimationSheet into a stream
        /// </summary>
        /// <param name="sheet">The animation sheet to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        protected void SaveAnimationSheetToStream(AnimationSheet sheet, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(sheet.ID);
            writer.Write(sheet.Name);
            SaveExportSettingsToStream(sheet.ExportSettings, stream);

            // Write the id of the animations of the sheet to the stream
            Animation[] anims = sheet.Animations;

            writer.Write(anims.Length);

            foreach (Animation anim in anims)
            {
                writer.Write(anim.ID);
            }
        }

        /// <summary>
        /// Saves the given AnimationExportSettings into a stream
        /// </summary>
        /// <param name="settings">The export settings to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        protected void SaveExportSettingsToStream(AnimationExportSettings settings, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(settings.FavorRatioOverArea);
            writer.Write(settings.ForcePowerOfTwoDimensions);
            writer.Write(settings.ForceMinimumDimensions);
            writer.Write(settings.ReuseIdenticalFramesArea);
            writer.Write(settings.HighPrecisionAreaMatching);
            writer.Write(settings.AllowUnorderedFrames);
            writer.Write(settings.UseUniformGrid);
            writer.Write(settings.UsePaddingOnXml);
            writer.Write(settings.ExportXml);
            writer.Write(settings.XPadding);
            writer.Write(settings.YPadding);
        }
    }
}
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
using JetBrains.Annotations;

namespace PixelariaLib.Data.Persistence.PixelariaFileBlocks
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
            blockID = BLOCKID_ANIMATIONSHEET;
        }
        
        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected override void SaveContentToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);

            var animationSheets = readyBundle.AnimationSheets;

            writer.Write(animationSheets.Count);

            foreach (var animationSheet in animationSheets)
            {
                SaveAnimationSheetToStream(animationSheet, stream);
            }
        }

        /// <summary>
        /// Loads an AnimationSheet from the current bytes buffer.
        /// </summary>
        /// <returns>The Animation sheet entries loaded</returns>
        public AnimationSheetEntry[] LoadAnimationSheetsFromBuffer()
        {
            using var stream = new MemoryStream(GetBlockBuffer(), false);
            var reader = new BinaryReader(stream);

            var sheetCount = reader.ReadInt32();
            var sheets = new AnimationSheetEntry[sheetCount];

            for (int i = 0; i < sheetCount; i++)
            {
                // Load the animation sheet data
                int id = reader.ReadInt32();
                string name = reader.ReadString();
                var settings = LoadExportSettingsFromStream(stream);

                // Create the animation sheet
                var sheet = new AnimationSheet(name) { ID = id, ExportSettings = settings };

                // Load the animation indices
                int animCount = reader.ReadInt32();

                int[] animationIds = new int[animCount];

                for (int j = 0; j < animCount; j++)
                {
                    animationIds[j] = reader.ReadInt32();
                }

                sheets[i] = new AnimationSheetEntry(sheet, animationIds);
            }

            return sheets;
        }

        /// <summary>
        /// Loads an AnimationExportSettings from the given stream, using the specified
        /// version number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the export settings from</param>
        /// <returns>The AnimationExportSettings object loaded</returns>
        protected AnimationExportSettings LoadExportSettingsFromStream([NotNull] Stream stream)
        {
            var reader = new BinaryReader(stream);

            var settings = new AnimationExportSettings
            {
                FavorRatioOverArea = reader.ReadBoolean(),
                ForcePowerOfTwoDimensions = reader.ReadBoolean(),
                ForceMinimumDimensions = reader.ReadBoolean(),
                ReuseIdenticalFramesArea = reader.ReadBoolean(),
                HighPrecisionAreaMatching = reader.ReadBoolean(),
                AllowUnorderedFrames = reader.ReadBoolean(),
                UseUniformGrid = reader.ReadBoolean(),
                UsePaddingOnJson = reader.ReadBoolean(),
                ExportJson = reader.ReadBoolean(),
                XPadding = reader.ReadInt32(),
                YPadding = reader.ReadInt32()
            };

            return settings;
        }

        /// <summary>
        /// Saves the given AnimationSheet into a stream
        /// </summary>
        /// <param name="sheet">The animation sheet to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        protected void SaveAnimationSheetToStream([NotNull] AnimationSheet sheet, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(sheet.ID);
            writer.Write(sheet.Name);
            SaveExportSettingsToStream(sheet.ExportSettings, stream);

            // Write the id of the animations of the sheet to the stream
            var anims = sheet.Animations;

            writer.Write(anims.Length);

            foreach (var anim in anims)
            {
                writer.Write(anim.ID);
            }
        }

        /// <summary>
        /// Saves the given AnimationExportSettings into a stream
        /// </summary>
        /// <param name="settings">The export settings to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        protected void SaveExportSettingsToStream(AnimationExportSettings settings, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(settings.FavorRatioOverArea);
            writer.Write(settings.ForcePowerOfTwoDimensions);
            writer.Write(settings.ForceMinimumDimensions);
            writer.Write(settings.ReuseIdenticalFramesArea);
            writer.Write(settings.HighPrecisionAreaMatching);
            writer.Write(settings.AllowUnorderedFrames);
            writer.Write(settings.UseUniformGrid);
            writer.Write(settings.UsePaddingOnJson);
            writer.Write(settings.ExportJson);
            writer.Write(settings.XPadding);
            writer.Write(settings.YPadding);
        }

        /// <summary>
        /// Encapsulates an animation sheet and associated animation IDs
        /// </summary>
        public readonly struct AnimationSheetEntry
        {
            public AnimationSheet Sheet { get; }
            public int[] AnimationIds { get; }

            public AnimationSheetEntry(AnimationSheet sheet, int[] animationIds) : this()
            {
                Sheet = sheet;
                AnimationIds = animationIds;
            }
        }
    }
}
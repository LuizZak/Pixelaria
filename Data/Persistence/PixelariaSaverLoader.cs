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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Pixelaria.Utils;

namespace Pixelaria.Data.Persistence
{
    /// <summary>
    /// Main persistence handler for the Pixelaria save format
    /// </summary>
    public class PixelariaSaverLoader
    {
        /// <summary>
        /// The version of this Pixelaria persistence handler
        /// </summary>
        private static int version = 6;

        /// <summary>
        /// Gets the version of this Pixelaria persistence handler
        /// </summary>
        public static int Version { get { return version; } }

        #region Loading

        /// <summary>
        /// Loads a bundle from disk
        /// </summary>
        /// <param name="path">The path to load the bundle from</param>
        /// <returns>The bundle that was loaded from the given path</returns>
        public static Bundle LoadBundleFromDisk(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);

            // Signature Block
            if (reader.ReadByte() != 'P' || reader.ReadByte() != 'X' || reader.ReadByte() != 'L')
            {
                return null;
            }

            // Bundle Header block
            int bundleVersion = reader.ReadInt32();
            string bundleName = reader.ReadString();
            string bundlePath = "";

            if (bundleVersion >= 3)
            {
                bundlePath = reader.ReadString();
            }

            Bundle bundle = new Bundle(bundleName);

            bundle.ExportPath = bundlePath;

            // Animation block
            int animCount = reader.ReadInt32();

            for (int i = 0; i < animCount; i++)
            {
                bundle.AddAnimation(LoadAnimationFromStream(stream, bundleVersion));
            }

            // The next block (Animation Sheets) are only available on version 2 and higher
            if (bundleVersion == 1)
                return bundle;

            // AnimationSheet block
            int sheetCount = reader.ReadInt32();

            for (int i = 0; i < sheetCount; i++)
            {
                bundle.AddAnimationSheet(LoadAnimationSheetFromStream(stream, bundle, bundleVersion));
            }

            stream.Close();

            return bundle;
        }

        /// <summary>
        /// Loads an Animation from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the animation from</param>
        /// <param name="version">The version that the stream was written on</param>
        /// <returns>The Animation object loaded</returns>
        public static Animation LoadAnimationFromStream(Stream stream, int version)
        {
            BinaryReader reader = new BinaryReader(stream);

            int id = -1;
            if (version >= 2)
            {
                id = reader.ReadInt32();
            }
            string name = reader.ReadString();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int fps = reader.ReadInt32();
            bool frameskip = reader.ReadBoolean();

            Animation anim = new Animation(name, width, height);

            anim.ID = id;
            anim.PlaybackSettings.FPS = fps;
            anim.PlaybackSettings.FrameSkip = frameskip;

            int frameCount = reader.ReadInt32();

            for (int i = 0; i < frameCount; i++)
            {
                anim.AddFrame(LoadFrameFromStream(stream, anim, version));
            }

            return anim;
        }

        /// <summary>
        /// Loads a Frame from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the frame from</param>
        /// <param name="owningAnimation">The Animation object that will be used to create the Frame with</param>
        /// <param name="version">The version that the stream was written on</param>
        /// <returns>The Frame object loaded</returns>
        public static Frame LoadFrameFromStream(Stream stream, Animation owningAnimation, int version)
        {
            BinaryReader reader = new BinaryReader(stream);

            // Read the size of the frame texture
            long textSize = reader.ReadInt64();

            Frame frame = new Frame(owningAnimation, owningAnimation.Width, owningAnimation.Height, false);

            MemoryStream memStream = new MemoryStream();

            long pos = stream.Position;

            byte[] buff = new byte[textSize];
            stream.Read(buff, 0, buff.Length);
            stream.Position = pos + textSize;

            memStream.Write(buff, 0, buff.Length);

            Bitmap bitmap = Bitmap.FromStream(memStream) as Bitmap;

            // Get the hash now
            byte[] hash = null;

            if (version >= 6)
            {
                int length = reader.ReadInt32();
                hash = new byte[length];
                stream.Read(hash, 0, length);
            }
            else
            {
                memStream.Position = 0;
                hash = Utilities.GetHashForStream(memStream);
            }

            memStream.Dispose();

            frame.SetFrameBitmap(bitmap, false);
            frame.SetHash(hash);

            return frame;
        }

        /// <summary>
        /// Loads an AnimationSheet from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the animation sheet from</param>
        /// <param name="parentBundle">The bundle that will contain this AnimationSheet</param>
        /// <param name="version">The version that the stream was written on</param>
        /// <returns>The Animation object loaded</returns>
        public static AnimationSheet LoadAnimationSheetFromStream(Stream stream, Bundle parentBundle, int version)
        {
            BinaryReader reader = new BinaryReader(stream);

            // Load the animation sheet data
            int id = reader.ReadInt32();
            string name = reader.ReadString();
            AnimationExportSettings settings = LoadExportSettingsFromStream(stream, version);

            // Create the animation sheet
            AnimationSheet sheet = new AnimationSheet(name);

            sheet.ID = id;
            sheet.ExportSettings = settings;

            // Load the animation indices
            int animCount = reader.ReadInt32();

            for (int i = 0; i < animCount; i++)
            {
                Animation anim = parentBundle.GetAnimationByID(reader.ReadInt32());

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
        /// <param name="version">The version that the stream was writter on</param>
        /// <returns>The AnimationExportSettings object loaded</returns>
        public static AnimationExportSettings LoadExportSettingsFromStream(Stream stream, int version)
        {
            BinaryReader reader = new BinaryReader(stream);

            AnimationExportSettings settings = new AnimationExportSettings();

            settings.FavorRatioOverArea = reader.ReadBoolean();
            settings.ForcePowerOfTwoDimensions = reader.ReadBoolean();
            settings.ForceMinimumDimensions = reader.ReadBoolean();
            settings.ReuseIdenticalFramesArea = reader.ReadBoolean();
            if (version >= 4)
            {
                settings.HighPrecisionAreaMatching = reader.ReadBoolean();
            }
            settings.AllowUnorderedFrames = reader.ReadBoolean();
            settings.UsePaddingOnXml = reader.ReadBoolean();
            if (version >= 5)
            {
                settings.ExportXml = reader.ReadBoolean();
            }
            else
            {
                settings.ExportXml = true;
            }
            settings.XPadding = reader.ReadInt32();
            settings.YPadding = reader.ReadInt32();

            return settings;
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the given bundle to disk
        /// </summary>
        /// <param name="bundle">The bundle to save</param>
        /// <param name="path">The path to save the bundle to</param>
        public static void SaveBundleToDisk(Bundle bundle, string path)
        {
            // Start writing to the file
            Stream stream = null;

            stream = new FileStream(path, FileMode.Create);

            BinaryWriter writer = new BinaryWriter(stream);

            // Signature block
            writer.Write((byte)'P');
            writer.Write((byte)'X');
            writer.Write((byte)'L');

            // Bundle Header block
            writer.Write(version);
            writer.Write(bundle.Name);
            writer.Write(bundle.ExportPath);

            // Animation Block
            writer.Write(bundle.Animations.Length);

            foreach(Animation anim in bundle.Animations)
            {
                WriteAnimationToStream(anim, stream);
            }

            // Sheet block
            writer.Write(bundle.AnimationSheets.Length);

            foreach (AnimationSheet sheet in bundle.AnimationSheets)
            {
                WriteAnimationSheetToStream(sheet, stream);
            }

            stream.Close();
        }

        /// <summary>
        /// Writes the given Animation into a stream
        /// </summary>
        /// <param name="animation">The animation to write to the stream</param>
        /// <param name="stream">The stream to write the animation to</param>
        public static void WriteAnimationToStream(Animation animation, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(animation.ID);
            writer.Write(animation.Name);
            writer.Write(animation.Width);
            writer.Write(animation.Height);
            writer.Write(animation.PlaybackSettings.FPS);
            writer.Write(animation.PlaybackSettings.FrameSkip);

            writer.Write(animation.FrameCount);

            for (int i = 0; i < animation.FrameCount; i++)
            {
                WriteFrameToStream(animation.GetFrameAtIndex(i), stream);
            }
        }

        /// <summary>
        /// Writes the given Frame into a stream
        /// </summary>
        /// <param name="frame">The frame to write to the stream</param>
        /// <param name="stream">The stream to write the frame to</param>
        public static void WriteFrameToStream(Frame frame, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            long sizeOffset = stream.Position;

            writer.Write((long)0);

            frame.GetComposedBitmap().Save(stream, ImageFormat.Png);

            // Skip back to the offset and draw the size
            stream.Position = sizeOffset;

            // Write the size now
            writer.Write(stream.Length - sizeOffset - 8);

            // Skip to the end and keep saving
            stream.Position = stream.Length;

            // Write the hash now
            writer.Write(frame.Hash.Length);
            writer.Write(frame.Hash, 0, frame.Hash.Length);
        }

        /// <summary>
        /// Writes the given AnimationSheet into a stream
        /// </summary>
        /// <param name="sheet">The animation sheet to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        public static void WriteAnimationSheetToStream(AnimationSheet sheet, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(sheet.ID);
            writer.Write(sheet.Name);
            WriteExportSettingsToStream(sheet.ExportSettings, stream);

            // Write the id of the animations of the sheet to the stream
            Animation[] anims = sheet.Animations;

            writer.Write(anims.Length);

            foreach (Animation anim in anims)
            {
                writer.Write(anim.ID);
            }
        }

        /// <summary>
        /// Writes the given AnimationExportSettings into a stream
        /// </summary>
        /// <param name="settings">The export settings to write to the stream</param>
        /// <param name="stream">The stream to write the animation sheet to</param>
        public static void WriteExportSettingsToStream(AnimationExportSettings settings, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(settings.FavorRatioOverArea);
            writer.Write(settings.ForcePowerOfTwoDimensions);
            writer.Write(settings.ForceMinimumDimensions);
            writer.Write(settings.ReuseIdenticalFramesArea);
            writer.Write(settings.HighPrecisionAreaMatching);
            writer.Write(settings.AllowUnorderedFrames);
            writer.Write(settings.UsePaddingOnXml);
            writer.Write(settings.ExportXml);
            writer.Write(settings.XPadding);
            writer.Write(settings.YPadding);
        }

        #endregion
    }
}
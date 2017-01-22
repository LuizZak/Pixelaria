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
        public const int Version = 9;

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

            // New pixelaria file format
            if (bundleVersion >= 9)
            {
                stream.Close();

                return LoadFileFromDisk(path).LoadedBundle;
            }

            ////////
            //// Legacy file formats
            ////////

            if (bundleVersion >= 3)
            {
                bundlePath = reader.ReadString();
            }

            Bundle bundle = new Bundle(bundleName) { ExportPath = bundlePath };

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
        /// Loads a Pixelaria (.plx) file from disk
        /// </summary>
        /// <param name="path">The path of the file to load</param>
        /// <returns>A new Pixelaria file</returns>
        public static PixelariaFile LoadFileFromDisk(string path)
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
            stream.Close();

            PixelariaFile file;

            ////////
            //// Version 9 and later
            ////////
            if (bundleVersion >= 9)
            {
                file = new PixelariaFile(new Bundle("Name"), path);

                PixelariaFileLoader.Load(file);

                return file;
            }

            Bundle bundle = LoadBundleFromDisk(path);

            file = new PixelariaFile(bundle, path);
            file.PrepareBlocksWithBundle();

            return file;
        }

        #endregion

        #region Saving

        /// <summary>
        /// Saves the given bundle to disk
        /// </summary>
        /// <param name="bundle">The bundle to save</param>
        /// <param name="path">The path to save the bundle to</param>
        public static void SaveBundleToDisk(Bundle bundle, string path)
        {
            PixelariaFile file = new PixelariaFile(bundle, path);

            SaveFileToDisk(file);
        }

        /// <summary>
        /// Saves the given PixelariaFile object to disk
        /// </summary>
        /// <param name="file">The file to save to disk</param>
        public static void SaveFileToDisk(PixelariaFile file)
        {
            PixelariaFileSaver.Save(file);
        }

        #endregion

        #region Version 8 and prior loader

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

            Animation anim = new Animation(name, width, height)
            {
                ID = id,
                PlaybackSettings = new AnimationPlaybackSettings { FPS = fps, FrameSkip = frameskip }
            };

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

            Image img = Image.FromStream(memStream);

            // The Bitmap constructor is used here because images loaded from streams are read-only and cannot be directly edited
            Bitmap bitmap = new Bitmap(img);

            img.Dispose();

            if (version >= 8)
            {
                frame.ID = reader.ReadInt32();
            }

            // Get the hash now
            byte[] hash;

            if (version >= 6)
            {
                int length = reader.ReadInt32();
                hash = new byte[length];
                stream.Read(hash, 0, length);
            }
            else
            {
                memStream.Position = 0;
                hash = ImageUtilities.GetHashForStream(memStream);
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
            AnimationSheet sheet = new AnimationSheet(name) { ID = id, ExportSettings = settings };

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

            AnimationExportSettings settings = new AnimationExportSettings
            {
                FavorRatioOverArea = reader.ReadBoolean(),
                ForcePowerOfTwoDimensions = reader.ReadBoolean(),
                ForceMinimumDimensions = reader.ReadBoolean(),
                ReuseIdenticalFramesArea = reader.ReadBoolean(),
                // >= Version 4
                HighPrecisionAreaMatching = version >= 4 && reader.ReadBoolean(),
                AllowUnorderedFrames = reader.ReadBoolean(),
                // >= Version 7
                UseUniformGrid = version >= 7 && reader.ReadBoolean(),
                UsePaddingOnJson = reader.ReadBoolean(),
                // >= Version 5
                ExportJson = version < 5 || reader.ReadBoolean(),
                XPadding = reader.ReadInt32(),
                YPadding = reader.ReadInt32()
            };

            return settings;
        }

        #endregion
    }

    /// <summary>
    /// Encapsulates a Version 9 and later block-composed file loader
    /// </summary>
    public class PixelariaFileLoader
    {
        /// <summary>
        /// The file to load
        /// </summary>
        private readonly PixelariaFile _file;

        /// <summary>
        /// Whether to reset the bundle before loading contents from the stream
        /// </summary>
        private readonly bool _resetBundle;

        /// <summary>
        /// Initializes a new instance of the PixelariaFileLoader class
        /// </summary>
        /// <param name="file">The file to load from the stream</param>
        /// <param name="resetBundle">Whether to reset the bundle to a clear state before loading the new file</param>
        public PixelariaFileLoader(PixelariaFile file, bool resetBundle)
        {
            _file = file;
            _resetBundle = resetBundle;
        }

        /// <summary>
        /// Loads the contents of a PixelariaFile
        /// </summary>
        public void Load()
        {
            _file.ResetBundleOnLoad = _resetBundle;
            _file.Load();
        }

        /// <summary>
        /// Loads a pixelaria file's contents from its stream or file path
        /// </summary>
        /// <param name="file">A valid PixelariaFile with a stream of valid file path set</param>
        /// <param name="resetBundle">Whether to reset the bundle to a clear state before loading the new file</param>
        public static void Load(PixelariaFile file, bool resetBundle = true)
        {
            // TODO: Verify correctess of clearing the pixelaria file's internal blocks list before loading the file from the stream again
            PixelariaFileLoader loader = new PixelariaFileLoader(file, resetBundle);
            loader.Load();
        }
    }

    /// <summary>
    /// Encapsulates a Version 9 and later block-composed file saver
    /// </summary>
    public class PixelariaFileSaver
    {
        /// <summary>
        /// The file to save
        /// </summary>
        private readonly PixelariaFile _file;

        /// <summary>
        /// Initializes a new instance of the PixelariaFileLoader class
        /// </summary>
        /// <param name="file">The file to save to the stream</param>
        public PixelariaFileSaver(PixelariaFile file)
        {
            _file = file;
        }

        /// <summary>
        /// Saves the contents of a PixelariaFile
        /// </summary>
        public void Save()
        {
            _file.Save();
        }

        /// <summary>
        /// Saves a pixelaria file's contents to its stream or file path
        /// </summary>
        /// <param name="file">A valid PixelariaFile with a stream of valid file path set</param>
        public static void Save(PixelariaFile file)
        {
            PixelariaFileSaver saver = new PixelariaFileSaver(file);
            saver.Save();
        }
    }
}
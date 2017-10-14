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
using Pixelaria.Controllers.DataControllers;

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
        [CanBeNull]
        public static Bundle LoadBundleFromDisk([NotNull] string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return LoadBundleFromStream(stream);
            }
        }

        /// <summary>
        /// Loads a bundle from a given stream
        /// </summary>
        /// <param name="stream">The stream to load the bundle from</param>
        /// <returns>The bundle that was loaded from the given stream</returns>
        [CanBeNull]
        public static Bundle LoadBundleFromStream([NotNull] Stream stream)
        {
            var reader = new BinaryReader(stream);

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
                var loader = new PixelariaFile(null, stream);
                loader.Load();

                return loader.ConstructBundle();
            }

            ////////
            //// Legacy file formats
            ////////

            if (bundleVersion >= 3)
            {
                bundlePath = reader.ReadString();
            }

            var bundle = new Bundle(bundleName) { ExportPath = bundlePath };

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

            return bundle;
        }

        #endregion

        #region Saving

        /// <summary>
        /// Saves the given bundle to disk
        /// </summary>
        /// <param name="bundle">The bundle to save</param>
        /// <param name="path">The path to save the bundle to</param>
        public static void SaveBundleToDisk(Bundle bundle, [NotNull] string path)
        {
            var file = new PixelariaFile(bundle, path);
            file.Save();
        }

        /// <summary>
        /// Saves the given bundle to a stream
        /// </summary>
        /// <param name="bundle">The bundle to save</param>
        /// <param name="stream">Stream to save bundle to</param>
        public static void SaveBundleToStream(Bundle bundle, [NotNull] Stream stream)
        {
            var file = new PixelariaFile(bundle, stream);
            file.Save();
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
        private static Animation LoadAnimationFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

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

            var anim = new Animation(name, width, height)
            {
                ID = id,
                PlaybackSettings = new AnimationPlaybackSettings { FPS = fps, FrameSkip = frameskip }
            };

            int frameCount = reader.ReadInt32();

            for (int i = 0; i < frameCount; i++)
            {
                // TODO: Don't add frames to animations directly on this block- let an external object handle piecing
                // frames to animations externally.
                var controller = new AnimationController(null, anim);

                controller.AddFrame(LoadFrameFromStream(stream, anim, version));
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
        private static Frame LoadFrameFromStream([NotNull] Stream stream, [NotNull] Animation owningAnimation, int version)
        {
            var reader = new BinaryReader(stream);

            // Read frame texture
            var bitmap = PersistenceHelper.LoadImageFromStream(stream);

            var frame = new Frame(owningAnimation, owningAnimation.Width, owningAnimation.Height, false);
            
            if (version >= 8)
            {
                frame.ID = reader.ReadInt32();
            }

            // Skip the hash now (newer versions from >8 have new unstable hashing algorithms)
            if (version >= 6)
            {
                int length = reader.ReadInt32();
                reader.ReadBytes(length);
            }
            
            frame.SetFrameBitmap(bitmap);
            
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
        private static AnimationSheet LoadAnimationSheetFromStream([NotNull] Stream stream, Bundle parentBundle, int version)
        {
            var reader = new BinaryReader(stream);

            // Load the animation sheet data
            int id = reader.ReadInt32();
            string name = reader.ReadString();
            var settings = LoadExportSettingsFromStream(stream, version);

            // Create the animation sheet
            var sheet = new AnimationSheet(name) { ID = id, SheetExportSettings = settings };

            // Load the animation indices
            int animCount = reader.ReadInt32();

            for (int i = 0; i < animCount; i++)
            {
                var anim = parentBundle.GetAnimationByID(reader.ReadInt32());

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
        private static AnimationSheetExportSettings LoadExportSettingsFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

            var settings = new AnimationSheetExportSettings
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
}
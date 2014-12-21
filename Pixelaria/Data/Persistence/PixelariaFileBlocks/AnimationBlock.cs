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

namespace Pixelaria.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Represents an animation block to save/load from a file
    /// </summary>
    public class AnimationBlock : FileBlock
    {
        /// <summary>
        /// Initializes a new instance of the AnimationBlock class
        /// </summary>
        public AnimationBlock()
        {
            blockID = BLOCKID_ANIMATION;
        }

        /// <summary>
        /// Loads the content portion of this block from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the content portion from</param>
        protected override void LoadContentFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            int animationCount = reader.ReadInt32();

            // Load the animations now
            for (int i = 0; i < animationCount; i++)
            {
                owningFile.LoadedBundle.AddAnimation(LoadAnimationFromStream(stream));
            }
        }

        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected override void SaveContentToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            // Get the list of animations to save
            Animation[] animations = readyBundle.Animations;

            // Save the number of animations inside the file
            writer.Write(animations.Length);

            // Save the animations now
            foreach (var animation in animations)
            {
                SaveAnimationToStream(animation, stream);
            }
        }

        /// <summary>
        /// Loads an Animation from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the animation from</param>
        /// <returns>The Animation object loaded</returns>
        protected Animation LoadAnimationFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            int id = reader.ReadInt32();
            string name = reader.ReadString();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int fps = reader.ReadInt32();
            bool frameskip = reader.ReadBoolean();

            Animation anim = new Animation(name, width, height)
            {
                ID = id,
                PlaybackSettings = { FPS = fps, FrameSkip = frameskip }
            };

            int frameCount = reader.ReadInt32();

            for (int i = 0; i < frameCount; i++)
            {
                anim.AddFrame(LoadFrameFromStream(stream, anim));
            }

            return anim;
        }

        /// <summary>
        /// Loads a Frame from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the frame from</param>
        /// <param name="owningAnimation">The Animation object that will be used to create the Frame with</param>
        /// <returns>The Frame object loaded</returns>
        protected Frame LoadFrameFromStream(Stream stream, Animation owningAnimation)
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

            frame.ID = reader.ReadInt32();

            // Get the hash now
            int length = reader.ReadInt32();
            var hash = new byte[length];
            stream.Read(hash, 0, length);

            memStream.Dispose();

            frame.SetFrameBitmap(bitmap, false);
            frame.SetHash(hash);

            return frame;
        }

        /// <summary>
        /// Saves the given Animation object to the given stream
        /// </summary>
        /// <param name="animation">The animation to save</param>
        /// <param name="stream">The stream to save the animation to</param>
        protected void SaveAnimationToStream(Animation animation, Stream stream)
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
                SaveFrameToStream(animation.GetFrameAtIndex(i), stream);
            }
        }

        /// <summary>
        /// Saves the given Frame into a stream
        /// </summary>
        /// <param name="frame">The frame to write to the stream</param>
        /// <param name="stream">The stream to write the frame to</param>
        protected void SaveFrameToStream(IFrame frame, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            // Save the space for the image size on the stream
            long sizeOffset = stream.Position;
            writer.Write((long)0);

            // Save the frame image
            frame.GetComposedBitmap().Save(stream, ImageFormat.Png);

            // Skip back to the image size offset and save the size
            long streamEnd = stream.Position;
            stream.Position = sizeOffset;
            writer.Write(streamEnd - sizeOffset - 8);

            // Skip back to the end to keep saving
            stream.Position = streamEnd;

            // Write the frame ID
            writer.Write(frame.ID);

            // Write the hash now
            writer.Write(frame.Hash.Length);
            writer.Write(frame.Hash, 0, frame.Hash.Length);
        }
    }
}
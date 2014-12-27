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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Pixelaria.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Represents a frame block saved to a file
    /// </summary>
    public class FrameBlock : FileBlock
    {
        /// <summary>
        /// The frame bieng manipulated by this FrameBlock
        /// </summary>
        private IFrame _frame;

        /// <summary>
        /// The frame bieng manipulated by this FrameBlock
        /// </summary>
        public IFrame Frame
        {
            get { return _frame; }
        }

        /// <summary>
        /// Initializes a new instance of the FrameBlock class
        /// </summary>
        public FrameBlock()
        {
            blockID = BLOCKID_FRAME;
            removeOnPrepare = true;
        }

        /// <summary>
        /// Initializes a new instance of the FrameBlock class
        /// </summary>
        public FrameBlock(IFrame frame)
            : this()
        {
            _frame = frame;
        }

        /// <summary>
        /// Loads the content portion of this block from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the content portion from</param>
        protected override void LoadContentFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            int animationId = reader.ReadInt32();

            Animation animation = owningFile.LoadedBundle.GetAnimationByID(animationId);

            if (animation == null)
            {
                throw new Exception(@"The frame's animation ID target is invalid");
            }
            
            _frame = LoadFrameFromStream(stream, animation);
        }

        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected override void SaveContentToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(_frame.Animation.ID);

            SaveFrameToStream(_frame, stream);
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

            // TODO: Deal with GetComposedBitmap()'s result, also deal with layering
            // Save the frame image
            using (Bitmap bitmap = frame.GetComposedBitmap())
            {
                bitmap.Save(stream, ImageFormat.Png);
            }

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

            owningAnimation.AddFrame(frame);

            return frame;
        }
    }
}
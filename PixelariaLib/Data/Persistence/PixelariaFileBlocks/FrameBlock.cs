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
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace PixelariaLib.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Represents a frame block saved to a file
    /// </summary>
    public class FrameBlock : FileBlock
    {
        private readonly IFrame _frame;

        /// <summary>
        /// The current block version for this frame block
        /// </summary>
        private const int CurrentVersion = 3;

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
            blockVersion = CurrentVersion;
        }

        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected override void SaveContentToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(_frame.Animation.ID);

            SaveFrameToStream(_frame, stream);
        }

        /// <summary>
        /// Saves the given Frame into a stream
        /// </summary>
        /// <param name="frame">The frame to write to the stream</param>
        /// <param name="stream">The stream to write the frame to</param>
        protected void SaveFrameToStream([NotNull] IFrame frame, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            if (frame is Frame castFrame)
            {
                SaveLayersToStream(castFrame, stream);
            }
            else
            {
                using (var bitmap = frame.GetComposedBitmap())
                {
                    PersistenceHelper.SaveImageToStream(bitmap, stream);
                }
            }

            // Write the frame ID
            writer.Write(frame.ID);

            // Write the hash now
            writer.Write(frame.Hash.Length);
            writer.Write(frame.Hash, 0, frame.Hash.Length);
        }

        /// <summary>
        /// Saves the layers of the given frame to a stream
        /// </summary>
        /// <param name="frame">The frame to save the layers to the strean</param>
        /// <param name="stream">The stream to save the layers to</param>
        protected void SaveLayersToStream([NotNull] Frame frame, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            // Save the number of layers stored on the frame object
            writer.Write(frame.LayerCount);

            for (int i = 0; i < frame.LayerCount; i++)
            {
                SaveLayerToStream(frame.Layers[i], stream);
            }
        }

        /// <summary>
        /// Saves the contents of a layer to a stream
        /// </summary>
        /// <param name="layer">The layer to save</param>
        /// <param name="stream">The stream to save the layer to</param>
        private static void SaveLayerToStream([NotNull] IFrameLayer layer, [NotNull] Stream stream)
        {
            // Save the layer's name
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(layer.Name);

            PersistenceHelper.SaveImageToStream(layer.LayerBitmap, stream);
        }
        
        /// <summary>
        /// Loads a Frame from the current bytes buffer.
        /// </summary>
        /// <returns>The Frame object loaded</returns>
        public FrameInfo LoadFrameFromBuffer(int width, int height)
        {
            using (var stream = new MemoryStream(GetBlockBuffer(), false))
            {
                var reader = new BinaryReader(stream);
                
                var animationId = reader.ReadInt32();

                var frame = new Frame(null, width, height, false);
                frame.Layers.Clear();

                FrameLayer[] layers;

                if (blockVersion == 0)
                {
                    var bitmap = PersistenceHelper.LoadImageFromStream(stream);
                    frame.SetFrameBitmap(bitmap, false);

                    layers = new FrameLayer[0];
                }
                else if (blockVersion >= 1 && blockVersion <= CurrentVersion)
                {
                    layers = LoadLayersFromStream(stream);
                }
                else
                {
                    throw new Exception("Unknown frame block version " + blockVersion);
                }

                frame.ID = reader.ReadInt32();

                // Get the hash now
                int length = reader.ReadInt32();
                var hash = reader.ReadBytes(length);
                
                return new FrameInfo(animationId, hash, layers, frame);
            }
        }

        /// <summary>
        /// Reads the animation ID from the underyling bytes buffer
        /// </summary>
        public int ReadAnimationId()
        {
            var stream = new MemoryStream(GetBlockBuffer(), false);
            var reader = new BinaryReader(stream);

            return reader.ReadInt32();
        }

        /// <summary>
        /// Loads layers stored on the given stream on the given frame
        /// </summary>
        /// <param name="stream">The stream to load the layers from</param>
        protected FrameLayer[] LoadLayersFromStream([NotNull] Stream stream)
        {
            var reader = new BinaryReader(stream);

            int layerCount = reader.ReadInt32();
            var layers = new FrameLayer[layerCount];

            for (int i = 0; i < layerCount; i++)
            {
                layers[i] = LoadLayerFromStream(stream);
            }
            
            return layers;
        }

        /// <summary>
        /// Loads a single layer from a specified stream
        /// </summary>
        /// <param name="stream">The stream to load the layer from</param>
        private FrameLayer LoadLayerFromStream([NotNull] Stream stream)
        {
            // Load the layer's name
            string name = "";

            var reader = new BinaryReader(stream, Encoding.UTF8);

            if (blockVersion >= 2)
            {
                name = reader.ReadString();
            }

            var length = reader.ReadInt64();
            var bytes = reader.ReadBytes((int)length);

            return new FrameLayer(name, bytes);
        }

        /// <summary>
        /// Metadata for a loaded frame, including the frame itself and other relevant information
        /// </summary>
        public struct FrameInfo
        {
            /// <summary>
            /// When read from a stream, contains the ID of the animation the frame from
            /// this block belongs to
            /// </summary>
            public int AnimationId { get; }

            /// <summary>
            /// The frame bieng manipulated by this FrameBlock
            /// </summary>
            public Frame Frame { get; }

            /// <summary>
            /// The frame's hash as bytes
            /// </summary>
            public byte[] HashBytes { get; }

            /// <summary>
            /// When read from a stream, contains all the layers for the associated frame, in order
            /// that they where read from the stream.
            /// 
            /// This value may be null, if data was not read from a stream containing a frame layer yet.
            /// </summary>
            [CanBeNull]
            public FrameLayer[] Layers { get; }

            public FrameInfo(int animationId, byte[] hashBytes, [CanBeNull] FrameLayer[] layers, Frame frame)
            {
                AnimationId = animationId;
                Layers = layers;
                Frame = frame;
                HashBytes = hashBytes;
            }
        }

        /// <summary>
        /// Basic read-time structure that encapsulates layers for a frame read from stream
        /// </summary>
        public struct FrameLayer
        {
            public string Name { get; }
            public byte[] ImageData { get; }

            public FrameLayer(string name, byte[] imageData)
            {
                Name = name;
                ImageData = imageData;
            }
        }
    }
}
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
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Pixelaria.Controllers.DataControllers;

namespace Pixelaria.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Represents a frame block saved to a file
    /// </summary>
    public class FrameBlock : FileBlock
    {
        /// <summary>
        /// The current block version for this frame block
        /// </summary>
        private const int CurrentVersion = 2;

        /// <summary>
        /// The frame bieng manipulated by this FrameBlock
        /// </summary>
        public IFrame Frame { get; private set; }

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
            Frame = frame;
            blockVersion = CurrentVersion;
        }

        /// <summary>
        /// Loads the content portion of this block from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the content portion from</param>
        protected override void LoadContentFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);

            int animationId = reader.ReadInt32();

            var animation = owningFile.LoadedBundle.GetAnimationByID(animationId);

            if (animation == null)
            {
                throw new Exception(@"The frame's animation ID target is invalid");
            }
            
            Frame = LoadFrameFromStream(stream, animation);
        }

        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected override void SaveContentToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(Frame.Animation.ID);

            SaveFrameToStream(Frame, stream);
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
                using (Bitmap bitmap = frame.GetComposedBitmap())
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
            BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(layer.Name);

            PersistenceHelper.SaveImageToStream(layer.LayerBitmap, stream);
        }
        
        /// <summary>
        /// Loads a Frame from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the frame from</param>
        /// <param name="owningAnimation">The Animation object that will be used to create the Frame with</param>
        /// <returns>The Frame object loaded</returns>
        protected Frame LoadFrameFromStream([NotNull] Stream stream, [NotNull] Animation owningAnimation)
        {
            var reader = new BinaryReader(stream);

            var frame = new Frame(owningAnimation, owningAnimation.Width, owningAnimation.Height, false);

            if(blockVersion == 0)
            {
                var bitmap = PersistenceHelper.LoadImageFromStream(stream);
                frame.SetFrameBitmap(bitmap, false);
            }
            else if (blockVersion >= 1 && blockVersion <= CurrentVersion)
            {
                LoadLayersFromStream(stream, frame);
            }
            else
            {
                throw new Exception("Unknown frame block version " + blockVersion);
            }

            frame.ID = reader.ReadInt32();

            // Get the hash now
            int length = reader.ReadInt32();
            var hash = new byte[length];
            stream.Read(hash, 0, length);

            frame.SetHash(hash);

            // If the block version is prior to 1, update the frame's hash value due to the new way the hash is calculated
            if (blockVersion < 1)
            {
                frame.UpdateHash();
            }
            
            // TODO: Don't add frames to animations directly on this block- let an external object handle piecing
            // frames to animations externally.

            var controller = new AnimationController(owningFile.LoadedBundle, owningAnimation);

            controller.AddFrame(frame);

            return frame;
        }

        /// <summary>
        /// Loads layers stored on the given stream on the given frame
        /// </summary>
        /// <param name="stream">The stream to load the layers from</param>
        /// <param name="frame">The frame to load the layers into</param>
        protected void LoadLayersFromStream([NotNull] Stream stream, [NotNull] Frame frame)
        {
            // Remove the first default layer of the frame
            frame.Layers.RemoveAt(0);

            var reader = new BinaryReader(stream);

            int layerCount = reader.ReadInt32();

            for (int i = 0; i < layerCount; i++)
            {
                LoadLayerFromStream(frame, stream);
            }
        }

        /// <summary>
        /// Loads a single layer from a specified stream
        /// </summary>
        /// <param name="frame">The frame to load the layer into</param>
        /// <param name="stream">The stream to load the layer from</param>
        private void LoadLayerFromStream(Frame frame, [NotNull] Stream stream)
        {
            var controller = new FrameController(frame);

            // Load the layer's name
            string name = null;

            if(blockVersion >= 2)
            {
                var reader = new BinaryReader(stream, Encoding.UTF8);
                name = reader.ReadString();
            }

            var layerBitmap = PersistenceHelper.LoadImageFromStream(stream);
            var layer = controller.AddLayer(layerBitmap);

            // Add the attributes that were loaded earlier
            if (name != null)
            {
                controller.SetLayerName(layer.Index, name);
            }
        }
    }
}
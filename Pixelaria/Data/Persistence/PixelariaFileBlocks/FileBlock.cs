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

namespace Pixelaria.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Base class for Pixelaria .plx file format blocks
    /// </summary>
    public class FileBlock : BaseBlock
    {
        /// <summary>
        /// Gets or sets the file this block is currently present in
        /// </summary>
        public PixelariaFile OwningFile
        {
            get { return owningFile; }
            set { owningFile = value; }
        }

        /// <summary>
        /// Gets whether this file block should be removed when the pixelaria file is being prepared to be saved/loaded
        /// </summary>
        public bool RemoveOnPrepare
        {
            get { return removeOnPrepare; }
        }

        /// <summary>
        /// Prepares the contents of this block to be saved based on the contents of the given Bundle
        /// </summary>
        /// <param name="bundle">The bundle to prepare this block from</param>
        public virtual void PrepareFromBundle(Bundle bundle)
        {
            readyBundle = bundle;
        }

        /// <summary>
        /// The file this block is currently present in
        /// </summary>
        protected PixelariaFile owningFile;

        /// <summary>
        /// The bundle that this block was prepared to handle
        /// </summary>
        protected Bundle readyBundle;

        /// <summary>
        /// Whether this file block should be removed when the pixelaria file is being prepared to be saved/loaded
        /// </summary>
        protected bool removeOnPrepare;

        /// <summary>
        /// Reads a block from the given stream object
        /// </summary>
        /// <param name="stream">The stream to read the block from</param>
        /// <param name="file">The PixelariaFile to use when loading the block</param>
        /// <returns>A block read from the given stream</returns>
        public static FileBlock FromStream(Stream stream, PixelariaFile file)
        {
            // Save the current stream position
            long offset = stream.Position;

            // Reader to read the ID from the stream
            BinaryReader reader = new BinaryReader(stream);
            FileBlock block = CreateBlockById(reader.ReadInt16());

            // Rewind the stream and read the block now
            stream.Position = offset;
            block.owningFile = file;
            block.LoadFromStream(stream);

            return block;
        }

        /// <summary>
        /// Creates and returns a block that matches the given ID
        /// </summary>
        /// <param name="blockId">The ID of the block to get</param>
        /// <returns>The Block, ready to be used</returns>
        public static FileBlock CreateBlockById(int blockId)
        {
            switch (blockId)
            {
                // Animation block
                case BLOCKID_ANIMATION:
                    return new AnimationBlock();
                // Animation Sheet block
                case BLOCKID_ANIMATIONSHEET:
                    return new AnimationSheetBlock();
                // Project Tree block
                case BLOCKID_PROJECTTREE:
                    return new ProjectTreeBlock();
                // Frame block
                case BLOCKID_FRAME:
                    return new FrameBlock();
                // Animation Header block
                case BLOCKID_ANIMATION_HEADER:
                    return new AnimationHeaderBlock();
                default:
                    return new FileBlock();
            }
        }

        /// <summary>Represents a Null block</summary>
        public const short BLOCKID_NULL = 0x0000;
        /// <summary>Represents an Animation block</summary>
        public const short BLOCKID_ANIMATION = 0x0001;
        /// <summary>Represents an Animation Sheet block</summary>
        public const short BLOCKID_ANIMATIONSHEET = 0x0002;
        /// <summary>Represents a Project Tree block</summary>
        public const short BLOCKID_PROJECTTREE = 0x0003;
        /// <summary>Represents a Frame block</summary>
        public const short BLOCKID_FRAME = 0x0004;
        /// <summary>Represents an Animation Header block</summary>
        public const short BLOCKID_ANIMATION_HEADER = 0x0005;
    }
}
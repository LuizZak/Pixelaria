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

namespace Pixelaria.Data.Persistence.Blocks
{
    /// <summary>
    /// Base class for Pixelaria .plx file format blocks
    /// </summary>
    public class Block
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
        /// Gets the ID of this block
        /// </summary>
        public short BlockID
        {
            get { return this.blockID; }
        }

        /// <summary>
        /// <para>Gets the version of the contents of this block.</para>
        /// <para>The version is unrelated to the File version and is used to verify what is inside the content portion</para>
        /// </summary>
        public short BlockVersion
        {
            get { return this.blockVersion; }
        }

        /// <summary>
        /// <para>Gets the length of this block data on the stream.</para>
        /// <para>This includes the block ID, block size and block data</para>
        /// </summary>
        public long BlockLength
        {
            get { return blockLength; }
        }

        /// <summary>
        /// <para>Gets the starting position of the block on the stream.</para>
        /// <para>The position is relative to the first byte of the Block ID</para>
        /// </summary>
        public long BlockOffset
        {
            get { return blockOffset; }
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
        /// Saves this block to the given Stream
        /// </summary>
        /// <param name="stream">The stream to save this block to</param>
        public virtual void SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            blockOffset = stream.Position;

            // Write the block ID
            writer.Write(blockID);
            // Write the block length
            writer.Write(blockLength);
            // Write the block version
            writer.Write(blockVersion);

            long contentOffset = stream.Position;

            // Save the content now
            SaveContentToStream(stream);

            // Write the content length now
            long length = stream.Position - contentOffset;

            stream.Position = blockOffset + sizeof(short);
            writer.Write(length);

            stream.Position = contentOffset + length;
        }

        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected virtual void SaveContentToStream(Stream stream)
        {
            if (blockContent == null || blockContent.Length == 0)
                return;

            stream.Write(blockContent, 0, blockContent.Length);
        }

        /// <summary>
        /// Loads this block from the given Stream
        /// </summary>
        /// <param name="stream">The stream to load this block from</param>
        public virtual void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            // Save the stream offset
            blockOffset = stream.Position;

            // Read the block ID
            blockID = reader.ReadInt16();
            // Read the block length
            blockLength = reader.ReadInt64();
            // Read the block version
            blockVersion = reader.ReadInt16();
            // Read the content now
            LoadContentFromStream(stream);
        }

        /// <summary>
        /// Loads the content portion of the block from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the content from</param>
        protected virtual void LoadContentFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            blockContent = reader.ReadBytes((int)blockLength);
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
        /// The ID of this block
        /// </summary>
        protected short blockID;

        /// <summary>
        /// The version of the contents of this block
        /// </summary>
        protected short blockVersion;

        /// <summary>
        /// <para>The length of this block data on the stream.</para>
        /// <para>This includes the block ID, block size and block data</para>
        /// </summary>
        protected long blockLength;

        /// <summary>
        /// <para>The starting position of the block on the stream.</para>
        /// <para>The position is relative to the first byte of the Block ID</para>
        /// </summary>
        protected long blockOffset;

        /// <summary>
        /// A temporary, private buffer for saving bytes on memory. Used when the block is of an unknown type and must be kept on memory.
        /// </summary>
        private byte[] blockContent;

        /// <summary>
        /// Reads a block from the given stream object
        /// </summary>
        /// <param name="stream">The stream to read the block from</param>
        /// <param name="file">The PixelariaFile to use when loading the block</param>
        /// <returns>A block read from the given stream</returns>
        public static Block FromStream(Stream stream, PixelariaFile file)
        {
            // Save the current stream position
            long offset = stream.Position;

            // Reader to read the ID from the stream
            BinaryReader reader = new BinaryReader(stream);
            Block block = CreateBlockByID(reader.ReadInt16());

            // Rewind the stream and read the block now
            stream.Position = offset;
            block.owningFile = file;
            block.LoadFromStream(stream);

            return block;
        }

        /// <summary>
        /// Creates and returns a block that matches the given ID
        /// </summary>
        /// <param name="blockID">The ID of the block to get</param>
        /// <returns>The Block, ready to be used</returns>
        public static Block CreateBlockByID(int blockID)
        {
            switch (blockID)
            {
                // Null block
                case BLOCKID_NULL:
                    return new Block();
                // Animation block
                case BLOCKID_ANIMATION:
                    return new AnimationBlock();
                // Animation Sheet block
                case BLOCKID_ANIMATIONSHEET:
                    return new AnimationSheetBlock();
                // Project Tree block
                case BLOCKID_PROJECTTREE:
                    return new ProjectTreeBlock();
            }

            return new Block();
        }

        /// <summary>Represents a Null block</summary>
        public const short BLOCKID_NULL = 0x0000;
        /// <summary>Represents an Animation block</summary>
        public const short BLOCKID_ANIMATION = 0x0001;
        /// <summary>Represents an Animation Sheet block</summary>
        public const short BLOCKID_ANIMATIONSHEET = 0x0002;
        /// <summary>Represents a Project Tree block</summary>
        public const short BLOCKID_PROJECTTREE = 0x0003;
    }
}
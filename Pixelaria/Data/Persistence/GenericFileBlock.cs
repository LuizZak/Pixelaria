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

namespace Pixelaria.Data.Persistence
{
    /// <summary>
    /// Represents a base class for block data objects to be used by the persistence manager to encode/decode data sequentially from streams
    /// </summary>
    public abstract class GenericFileBlock : IDisposable
    {
        /// <summary>
        /// Gets the ID of this block
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public short BlockID
        {
            get { return blockID; }
        }

        /// <summary>
        /// <para>Gets the version of the contents of this block.</para>
        /// <para>The version is unrelated to the File version and is used to verify what is inside the content portion</para>
        /// </summary>
        public short BlockVersion
        {
            get { return blockVersion; }
        }

        /// <summary>
        /// <para>Gets the length of this block data on the stream.</para>
        /// <para>This does not include the block ID, block size and block data</para>
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
        /// Disposes of this BaseBlock and all related resources
        /// </summary>
        public virtual void Dispose()
        {
            _blockContent = null;
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
            if (_blockContent == null || _blockContent.Length == 0)
                return;

            stream.Write(_blockContent, 0, _blockContent.Length);
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

            if (stream.Length - stream.Position < blockLength)
            {
                throw new ArgumentException(@"The stream provided does not have the required " + blockLength + @" bytes needed to load this file block.", "stream");
            }

            _blockContent = reader.ReadBytes((int)blockLength);
        }

        /// <summary>
        /// Gets the byte array that represents the buffer for the block's contents that were read off the stream
        /// </summary>
        /// <returns>The byte array that represents the buffer for the block's contents that were read off the stream</returns>
        public byte[] GetBlockBuffer()
        {
            return _blockContent;
        }

        /// <summary>
        /// The ID of this block
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected short blockID;

        /// <summary>
        /// The version of the contents of this block
        /// </summary>
        protected short blockVersion;

        /// <summary>
        /// <para>The length of this block data on the stream.</para>
        /// <para>This does not include the block ID, block size and block data</para>
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
        private byte[] _blockContent;
    }
}
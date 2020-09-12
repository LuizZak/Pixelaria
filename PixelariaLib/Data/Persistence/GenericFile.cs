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
using JetBrains.Annotations;

namespace PixelariaLib.Data.Persistence
{
    /// <summary>
    /// Represents a generic persistence file that contains a series of blocks
    /// </summary>
    public class GenericFile<T> : IDisposable where T : GenericFileBlock, new()
    {
        /// <summary>
        /// The stream containing this file
        /// </summary>
        protected Stream stream;

        /// <summary>
        /// The file header for this generic file
        /// </summary>
        protected FileHeader fileHeader = new FileHeader();

        /// <summary>
        /// The path to the .plx file to manipulate
        /// </summary>
        protected string filePath;

        /// <summary>
        /// The list of blocks currently on the file
        /// </summary>
        protected List<T> blockList;

        /// <summary>
        /// Gets the current stream containing the file
        /// </summary>
        public Stream CurrentStream
        {
            get => stream;
            set => stream = value;
        }

        /// <summary>
        /// The path to the .plx file to manipulate
        /// </summary>
        public string FilePath => filePath;

        /// <summary>
        /// Gets the list of blocks currently in this PixelariaFile
        /// </summary>
        public T[] Blocks => blockList.ToArray();

        /// <summary>
        /// Gets the number of blocks inside this PixelariaFile
        /// </summary>
        public int BlockCount => blockList.Count;

        ~GenericFile()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of this GenericFile and all used resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            foreach (var block in blockList)
            {
                block.Dispose();
            }

            blockList.Clear();
            blockList = null;
        }

        /// <summary>
        /// Adds a block to this file's composition
        /// </summary>
        /// <param name="block">The block to add to this PixelariaFile</param>
        public virtual void AddBlock(T block)
        {
            blockList.Add(block);
        }

        /// <summary>
        /// Removes a block from this file's composition
        /// </summary>
        /// <param name="block">The block to remove</param>
        /// <param name="dispose">Whether to dispose of the block after its removal</param>
        public virtual void RemoveBlock(T block, bool dispose = true)
        {
            blockList.Remove(block);
            if (dispose)
            {
                block.Dispose();
            }
        }

        /// <summary>
        /// Removes all file blocks in this file's composition
        /// </summary>
        public void ClearBlockList()
        {
            foreach (var block in blockList)
            {
                block.Dispose();
            }

            blockList.Clear();
        }

        /// <summary>
        /// Returns a list of blocks that match a given type
        /// </summary>
        /// <param name="blockType">The block type to match</param>
        /// <returns>A list of all the blocks that match the given type</returns>
        public T[] GetBlocksByType(Type blockType)
        {
            return blockList.Where(block => block.GetType() == blockType).ToArray();
        }

        /// <summary>
        /// Gets all the blocks inside this PixelariaFile that match the given blockID
        /// </summary>
        /// <param name="blockId">The blockID to match</param>
        /// <returns>All the blocks that match the given ID inside this PixelariaFile</returns>
        // ReSharper disable once InconsistentNaming
        public T[] GetBlocksByID(short blockId)
        {
            return blockList.Where(block => block.BlockID == blockId).ToArray();
        }

        /// <summary>
        /// Saves the contents of a GenericFile into the file system
        /// </summary>
        public void Save()
        {
            // Get the stream to save the file to
            bool closeStream = false;
            if (stream == null)
            {
                stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                closeStream = true;
            }

            stream.SetLength(0);

            SaveHeader();
            SaveBlocks();

            // Truncate the stream so any unwanted extra data is not left pending, that can lead to potential crashes when reading the file back again
            stream.SetLength(stream.Position);

            if (closeStream)
            {
                stream.Close();
                stream = null;
            }
        }

        /// <summary>
        /// Saves the header into the currently opened stream
        /// </summary>
        protected virtual void SaveHeader()
        {
            fileHeader.SaveToSteam(stream);
        }

        /// <summary>
        /// Saves the file blocks into the currently opened stream
        /// </summary>
        protected virtual void SaveBlocks()
        {
            // Save the blocks
            foreach (var block in blockList)
            {
                SaveBlock(block, stream);
            }
        }

        /// <summary>
        /// Loads the contents of this GenericFile from the file system
        /// </summary>
        public virtual void Load()
        {
            // Get the stream to load the file from
            bool closeStream = false;
            if (stream == null)
            {
                stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                closeStream = true;
            }

            stream.Seek(0, SeekOrigin.Begin);

            LoadHeader();

            // If the file header is not valid, return
            if (!fileHeader.IsValid)
            {
                if (closeStream)
                {
                    stream.Close();
                    stream = null;
                }

                return;
            }

            LoadBlocksFromStream();
            
            if (closeStream)
            {
                stream.Close();
                stream = null;
            }
        }

        /// <summary>
        /// Loads the header from the currently opened stream
        /// </summary>
        protected virtual void LoadHeader()
        {
            fileHeader.LoadFromSteam(stream);
        }

        /// <summary>
        /// Loads the blocks from the underlying stream
        /// </summary>
        protected virtual void LoadBlocksFromStream()
        {
            // Clear the blocks
            ClearBlockList();

            // Load the blocks
            while (stream.Position < stream.Length)
            {
                AddBlockFromStream();
            }
        }

        /// <summary>
        /// Adds a block from the currently opened stream
        /// </summary>
        protected virtual void AddBlockFromStream()
        {
            var block = new T();
            block.LoadFromStream(stream);
            AddBlock(block);
        }

        /// <summary>
        /// Saves the specified block into a target stream
        /// </summary>
        /// <param name="block">The block to save</param>
        /// <param name="targetStream">The stream to save the block to</param>
        protected virtual void SaveBlock([NotNull] T block, [NotNull] Stream targetStream)
        {
            block.SaveToStream(targetStream);
        }

        /// <summary>
        /// Represents a file header block that can is used to identify a file format
        /// </summary>
        public class FileHeader
        {
            /// <summary>
            /// The allowed number of bytes in the current magic number byte array
            /// </summary>
            public readonly int MagicNumbersLength = 3;

            /// <summary>
            /// The magic numbers for this file header
            /// </summary>
            protected byte[] magicNumberBytes;

            /// <summary>
            /// The expected magic numbers for this file header
            /// </summary>
            protected byte[] expectedMagicNumberBytes;

            /// <summary>
            /// Gets the byte magic numbers as a string of characters encoded from ASCII format
            /// </summary>
            public string MagicNumbersString
            {
                get
                {
                    StringBuilder builder = new StringBuilder();

                    Array.ForEach(magicNumberBytes, b => builder.Append((char)b));

                    return builder.ToString();
                }
            }

            /// <summary>
            /// Gets or sets the magic numbers for this file header.
            /// The magic numbers must be defined as an array of bytes representing ASCII characters.
            /// If the length of the array passed does not match the underlying magic numbers length, an exception is thrown
            /// </summary>
            public byte[] MagicNumberBytes
            {
                get => magicNumberBytes;
                set
                {
                    if(value.Length != MagicNumbersLength)
                        throw new ArgumentException(@"The magic numbers must have exactly " + MagicNumbersLength + @" bytes as defined by the type.", nameof(value));

                    magicNumberBytes = value;
                }
            }

            /// <summary>
            /// Gets or sets the expected magic numbers for this file header.
            /// The magic numbers must be defined as an array of bytes representing ASCII characters
            /// </summary>
            public byte[] ExpectedMagicNumberBytes
            {
                get => expectedMagicNumberBytes;
                set
                {
                    if (value.Length != MagicNumbersLength)
                        throw new ArgumentException(@"The expected magic numbers must have exactly " + MagicNumbersLength + @" bytes as defined by the type.", nameof(value));

                    expectedMagicNumberBytes = value;
                }
            }

            /// <summary>
            /// Gets a value specifying whether this file header is valid.
            /// The validity is checked by comparing the magic number bytes with the expected magic number bytes
            /// </summary>
            public bool IsValid => ExpectedMagicNumberBytes.SequenceEqual(MagicNumberBytes);

            /// <summary>
            /// Gets or sets the version for this file header
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// Saves this file header information to a given stream
            /// </summary>
            /// <param name="stream">The stream to save the file header to</param>
            public virtual void SaveToSteam([NotNull] Stream stream)
            {
                // Save the header
                var writer = new BinaryWriter(stream);

                // Signature Block
                Array.ForEach(MagicNumberBytes, writer.Write);
                // File version
                writer.Write(Version);
            }

            /// <summary>
            /// Loads the file header information from a given stream
            /// </summary>
            /// <param name="stream">The stream to load the header from</param>
            public virtual void LoadFromSteam([NotNull] Stream stream)
            {
                var reader = new BinaryReader(stream);

                stream.Read(magicNumberBytes, 0, MagicNumbersLength);
                Version = reader.ReadInt32();
            }
        }
    }
}
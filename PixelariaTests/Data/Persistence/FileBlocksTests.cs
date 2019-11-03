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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data.Persistence.PixelariaFileBlocks;

namespace PixelariaTests.Data.Persistence
{
    /// <summary>
    /// Tests the behavior of the FileBlock class and related components
    /// </summary>
    [TestClass]
    public class FileBlocksTests
    {
        /// <summary>
        /// Tests the behavior of each individual file block for the Pixelaria format
        /// </summary>
        [TestMethod]
        public void TestFileBlockIds()
        {
            using var fileBlock = new FileBlock();
            using var animBlock = new AnimationBlock();
            using var animHeaderBlock = new AnimationHeaderBlock();
            using var sheetBlock = new AnimationSheetBlock();
            using var frameBlock = new FrameBlock();
            using var treeBlock = new ProjectTreeBlock();
            using var exporterNameBlock = new ExporterNameBlock();
            using var exporterSettingsBlock = new ExporterSettingsBlock();

            // Assert block IDs
            Assert.AreEqual(FileBlock.BLOCKID_NULL, fileBlock.BlockID);
            Assert.AreEqual(FileBlock.BLOCKID_ANIMATION, animBlock.BlockID);
            Assert.AreEqual(FileBlock.BLOCKID_ANIMATION_HEADER, animHeaderBlock.BlockID);
            Assert.AreEqual(FileBlock.BLOCKID_ANIMATIONSHEET, sheetBlock.BlockID);
            Assert.AreEqual(FileBlock.BLOCKID_FRAME, frameBlock.BlockID);
            Assert.AreEqual(FileBlock.BLOCKID_PROJECTTREE, treeBlock.BlockID);
            Assert.AreEqual(FileBlock.BLOCKID_EXPORTER_NAME, exporterNameBlock.BlockID);
            Assert.AreEqual(FileBlock.BLOCKID_EXPORTER_SETTINGS, exporterSettingsBlock.BlockID);
        }

        /// <summary>
        /// Tests the behavior of each individual file block for the Pixelaria format
        /// </summary>
        [TestMethod]
        public void TestCreateFileBlockById()
        {
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_NULL), typeof(FileBlock));
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_ANIMATION), typeof(AnimationBlock));
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_ANIMATION_HEADER), typeof(AnimationHeaderBlock));
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_ANIMATIONSHEET), typeof(AnimationSheetBlock));
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_FRAME), typeof(FrameBlock));
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_PROJECTTREE), typeof(ProjectTreeBlock));
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_EXPORTER_NAME), typeof(ExporterNameBlock));
            Assert.IsInstanceOfType(FileBlock.CreateBlockById(FileBlock.BLOCKID_EXPORTER_SETTINGS), typeof(ExporterSettingsBlock));
        }

        /// <summary>
        /// Tests the reading behavior of the FileBlock class
        /// </summary>
        [TestMethod]
        public void TestBlockRead()
        {
            // Create a memory stream to generate the data to work with
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            byte[] contents = { 0xFF, 0xFE, 0xFD, 0xFC };
            byte[] offsetBytes = { 0, 0, 0, 0 };

            // Write some data off so the memory stream starts with an offset
            writer.Write(offsetBytes, 0, 4);
            
            // Block ID
            writer.Write(FileBlock.BLOCKID_NULL);
            // Block length
            writer.Write(contents.LongLength);
            // Block version
            writer.Write((short)1);

            // Block contents
            memStream.Write(contents, 0, 4);

            // Reset memory stream position and read a block
            // We reset to just past the offset so the block offset property can be tested as well
            memStream.Position = offsetBytes.Length;

            var block = FileBlock.FromStream(memStream, null);

            Assert.AreEqual(FileBlock.BLOCKID_NULL, block.BlockID, "The block's ID must match the data that was read off the stream");
            Assert.AreEqual(offsetBytes.Length, block.BlockOffset, "The block's length must match the block content's size in bytes");
            Assert.AreEqual(contents.Length, block.BlockLength, "The block's length must match the block content's size in bytes");
            Assert.AreEqual(1, block.BlockVersion, "The block's ID must match the data that was read off the stream");
            Assert.IsTrue(contents.SequenceEqual(block.GetBlockBuffer()), "The buffered data on the block must match the data that was read off the stream");
        }

        /// <summary>
        /// Tests the writing behavior of the FileBlock class
        /// </summary>
        [TestMethod]
        public void TestBlockWrite()
        {
            // Create a memory stream to generate the data to work with
            var memStream = new MemoryStream();
            var writeMemStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            byte[] contents = { 0xFF, 0xFE, 0xFD, 0xFC };
            byte[] offsetBytes = { 0, 0, 0, 0 };

            // Write some data off so the memory stream starts with an offset
            writer.Write(offsetBytes, 0, 4);
            writeMemStream.Write(offsetBytes, 0, 4);

            // Block ID
            writer.Write(FileBlock.BLOCKID_NULL);
            // Block length
            writer.Write(contents.LongLength);
            // Block version
            writer.Write((short)1);

            // Block contents
            memStream.Write(contents, 0, 4);

            // Reset memory stream position and read a block
            // We reset to just past the offset so the block offset property can be tested as well
            memStream.Position = offsetBytes.Length;

            var block = FileBlock.FromStream(memStream, null);

            block.SaveToStream(writeMemStream);

            Assert.IsTrue(memStream.GetBuffer().SequenceEqual(writeMemStream.GetBuffer()),
                "After writing the contents of a block back to a stream, its contents must exactly match the original contents that were read");
        }

        /// <summary>
        /// Tests the exception raised when the block contents
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Reading a block with a block length that goes beyond the stream's end must raise an ArgumentException")]
        public void TestBlockReadError()
        {
            // Create a memory stream to generate the data to work with
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            byte[] contents = { 0xFF, 0xFE, 0xFD, 0xFC };
            byte[] offsetBytes = { 0, 0, 0, 0 };

            // Write some data off so the memory stream starts with an offset
            writer.Write(offsetBytes, 0, 4);
            
            // Block ID
            writer.Write(FileBlock.BLOCKID_NULL);
            // Block length
            writer.Write(contents.LongLength + 1); // Write a block length that is larger than the real block
            // Block version
            writer.Write((short)1);

            // Block contents
            memStream.Write(contents, 0, 4);

            // Reset memory stream position and read a block
            // We reset to just past the offset so the block offset property can be tested as well
            memStream.Position = offsetBytes.Length;

            FileBlock.FromStream(memStream, null);
        }
    }
}
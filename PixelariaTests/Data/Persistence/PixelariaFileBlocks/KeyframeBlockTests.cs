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
using Pixelaria.Data;
using Pixelaria.Data.Persistence.PixelariaFileBlocks;
using PixelariaTests.TestUtils;
using Rhino.Mocks;

namespace PixelariaTests.Data.Persistence.PixelariaFileBlocks
{
    [TestClass]
    public class KeyframeBlockTests
    {
        [TestMethod]
        public void TestBlockId()
        {
            var sut = new KeyframeBlock();

            Assert.AreEqual(FileBlock.BLOCKID_KEYFRAME, sut.BlockID);
        }

        [TestMethod]
        public void TestRemoveOnPrepare()
        {
            var sut = new KeyframeBlock();

            Assert.AreEqual(true, sut.RemoveOnPrepare);
        }

        [TestMethod]
        public void TestCtor()
        {
            var sut = new KeyframeBlock(new TestKeyframeSerializer(), "value", 1, 1);

            Assert.AreEqual("value", sut.KeyframeName);
            Assert.AreEqual("TestSerializer", sut.SerializerName);
            Assert.IsTrue(new byte[] { 0x01, 0x00, 0x00, 0x00 }.SequenceEqual(sut.Contents));
            Assert.AreEqual(1, sut.FrameId);
        }
        
        [TestMethod]
        public void TestSaveToStream()
        {
            var sut = new KeyframeBlock("SerializerName", "value", new byte[] {0, 1, 2}, 1);
            var stream = new MemoryStream();

            sut.SaveToStream(stream);

            Assert.That.MemoryStreamMatches(stream, new byte[]
            {
                // Block ID
                0x08, 0x00,
                // Block length
                0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                // Block version
                0x01, 0x00,
                // Frame ID
                0x01, 0x00, 0x00, 0x00,
                // Serializer name (UTF-8)
                0x0E, 0x53, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x69, 0x7A, 0x65, 0x72, 0x4E, 0x61, 0x6D, 0x65,
                // Keyframe name (UTF-8)
                0x05, 0x76, 0x61, 0x6C, 0x75, 0x65,
                // Length of serialized contents
                0x03, 0x00, 0x00, 0x00,
                // Contents
                0x00, 0x01, 0x02
            });
        }

        [TestMethod]
        public void TestLoadFromStream()
        {
            var buffer = new byte[]
            {
                // Block ID
                0x08, 0x00,
                // Block length
                0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                // Block version
                0x01, 0x00,
                // Frame ID
                0x01, 0x00, 0x00, 0x00,
                // Serializer name (UTF-8)
                0x0E, 0x53, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x69, 0x7A, 0x65, 0x72, 0x4E, 0x61, 0x6D, 0x65,
                // Keyframe name (UTF-8)
                0x05, 0x76, 0x61, 0x6C, 0x75, 0x65,
                // Length of serialized contents
                0x03, 0x00, 0x00, 0x00,
                // Contents
                0x00, 0x01, 0x02
            };
            var stream = new MemoryStream(buffer);
            stream.Position = 0;
            var sut = new KeyframeBlock();

            sut.LoadFromStream(stream);

            Assert.AreEqual(1, sut.FrameId);
            Assert.IsTrue(new byte[] {0x00, 0x01, 0x02}.SequenceEqual(sut.Contents));
            Assert.AreEqual("SerializerName", sut.SerializerName);
            Assert.AreEqual("value", sut.KeyframeName);
        }

        [TestMethod]
        public void TestDeserializeValue()
        {
            const float deserializeResult = 1.0f;
            var mock = MockRepository.GenerateStub<IKeyframeValueSerializer>();
            mock.Stub(s => s.Deserialize(Arg<Stream>.Matches(p => p.ReadByte() == 0 && p.ReadByte() == 1 && p.ReadByte() == 2)))
                .Return(deserializeResult);
            var sut = new KeyframeBlock("SerializerName", "value", new byte[] { 0, 1, 2 }, 1);

            var result = sut.DeserializerValue(mock);

            Assert.AreEqual("value", result.Key);
            Assert.AreEqual(deserializeResult, result.Value);
        }

        private class TestKeyframeSerializer : IKeyframeValueSerializer
        {
            public Type SerializedType => typeof(int);
            public string SerializedName => "TestSerializer";

            public void Serialize(object value, Stream stream)
            {
                var writer = new BinaryWriter(stream);
                writer.Write((int) value);
            }

            public object Deserialize(Stream stream)
            {
                var reader = new BinaryReader(stream);
                return reader.ReadInt32();
            }
        }
    }
}

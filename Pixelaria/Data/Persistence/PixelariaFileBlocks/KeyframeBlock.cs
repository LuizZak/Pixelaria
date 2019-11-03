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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Pixelaria.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Represents a block for a frame's keyframe property
    /// </summary>
    public class KeyframeBlock : FileBlock
    {
        public string SerializerName { get; private set; }

        public string KeyframeName { get; private set; }

        public int FrameId { get; private set; }

        public byte[] Contents { get; private set; }

        /// <summary>
        /// The current block version for this keyframe block
        /// </summary>
        private const int CurrentVersion = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyframeBlock"/> class
        /// </summary>
        public KeyframeBlock()
        {
            blockID = BLOCKID_KEYFRAME;
            removeOnPrepare = true;
        }

        public KeyframeBlock([NotNull] IKeyframeValueSerializer serializer, string name, [NotNull] object value, int frameId) : this()
        {
            blockVersion = CurrentVersion;
            
            var memoryStream = new MemoryStream();
            serializer.Serialize(value, memoryStream);

            SerializerName = serializer.SerializedName;
            KeyframeName = name;
            FrameId = frameId;
            Contents = memoryStream.GetBuffer().Take((int) memoryStream.Length).ToArray();
        }

        public KeyframeBlock(string name, string keyframeName, byte[] contents, int frameId) : this()
        {
            blockVersion = CurrentVersion;

            SerializerName = name;
            KeyframeName = keyframeName;
            Contents = contents;
            FrameId = frameId;
        }

        protected override void SaveContentToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(FrameId);
            writer.Write(SerializerName);
            writer.Write(KeyframeName);
            writer.Write(Contents.Length);
            writer.Write(Contents);
        }

        protected override void LoadContentFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8);
            FrameId = reader.ReadInt32();
            SerializerName = reader.ReadString();
            KeyframeName = reader.ReadString();
            int length = reader.ReadInt32();
            Contents = reader.ReadBytes(length);
        }

        public int GetFrameId()
        {
            return FrameId;
        }

        public KeyValuePair<string, object> DeserializerValue(IKeyframeValueSerializer serializer)
        {
            return new KeyValuePair<string, object>(KeyframeName, serializer?.Deserialize(new MemoryStream(Contents)));
        }
    }
}

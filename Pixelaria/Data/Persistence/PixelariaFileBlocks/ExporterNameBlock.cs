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
    /// Contains information about the selected exporter from a Pixelaria file
    /// </summary>
    public class ExporterNameBlock : FileBlock
    {
        /// <summary>
        /// Gets or sets the serialized exporter name
        /// </summary>
        public string ExporterSerializedName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterNameBlock"/> class
        /// </summary>
        public ExporterNameBlock()
        {
            blockID = BLOCKID_EXPORTER_NAME;
        }

        public override void PrepareFromBundle(Bundle bundle)
        {
            base.PrepareFromBundle(bundle);

            ExporterSerializedName = bundle.ExporterSerializedName;
        }

        protected override void SaveContentToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(ExporterSerializedName);
        }

        protected override void LoadContentFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            ExporterSerializedName = reader.ReadString();
        }
    }
}
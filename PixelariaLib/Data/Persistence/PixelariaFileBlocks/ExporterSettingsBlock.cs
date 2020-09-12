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
using System.Text;
using PixelariaLib.Controllers.Exporters;

namespace PixelariaLib.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Contains information about the settings for a particular exporter
    /// </summary>
    public class ExporterSettingsBlock : FileBlock
    {
        /// <summary>
        /// Gets or sets the serialized exporter name which this configuration belongs to
        /// </summary>
        public string SerializedExporterName { get; set; }

        /// <summary>
        /// Gets or sets the associated settings for this settings block
        /// </summary>
        public IBundleExporterSettings Settings { get; set; }

        /// <summary>
        /// An exporter controller that will be used when loading settings from a stream.
        ///
        /// Defaults to <see cref="ExporterController.Instance"/>.
        /// </summary>
        public IExporterController ExporterControllerInstance { get; set; } = ExporterController.Instance;

        public ExporterSettingsBlock()
        {
            blockID = BLOCKID_EXPORTER_SETTINGS;
            removeOnPrepare = true;
        }

        public ExporterSettingsBlock(string serializedExporterName) : this()
        {
            SerializedExporterName = serializedExporterName;
        }

        public override void PrepareFromBundle(Bundle bundle)
        {
            base.PrepareFromBundle(bundle);

            if (bundle.ExporterSettingsMap.TryGetValue(SerializedExporterName, out var settings))
            {
                Settings = settings;
            }
        }

        protected override void SaveContentToStream(Stream stream)
        {
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(SerializedExporterName);
            Settings.Save(stream);
        }

        protected override void LoadContentFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8);
            SerializedExporterName = reader.ReadString();

            var settings = ExporterControllerInstance.CreateExporterSettingsForSerializedName(SerializedExporterName);
            if (settings == null)
                return;

            settings.Load(stream);
            Settings = settings;
        }
    }
}

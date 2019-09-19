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
using System.Linq;
using JetBrains.Annotations;
using Pixelaria.Controllers.Exporters.Pixelaria;
using Pixelaria.Controllers.Exporters.Unity;

namespace Pixelaria.Controllers.Exporters
{
    /// <summary>
    /// Controller that deals with management and creation of known <see cref="IBundleExporter"/> kinds.
    /// </summary>
    public class ExporterController : IExporterController
    {
        /// <summary>
        /// Gets the singleton instance for this <see cref="ExporterController"/> class.
        /// </summary>
        public static ExporterController Instance = new ExporterController();

        private readonly KnownExporterEntry _defaultExporter = new KnownExporterEntry("Pixelaria", PixelariaExporter.SerializedName, false, () => new PixelariaExporter(new DefaultSheetExporter()));
        private readonly List<KnownExporterEntry> _exporterList = new List<KnownExporterEntry>();

        public IKnownExporterEntry DefaultExporter => _defaultExporter;
        public IReadOnlyList<IKnownExporterEntry> Exporters => _exporterList;

        private ExporterController()
        {
            PopulateKnownExporters();
        }

        private void PopulateKnownExporters()
        {
            _exporterList.Add(_defaultExporter);
            _exporterList.Add(new KnownExporterEntry("Unity", UnityExporter.SerializedName, true, () => new UnityExporter(new DefaultSheetExporter())));
        }

        /// <summary>
        /// Returns whether an exporter exists with a given serialized name.
        /// </summary>
        public bool HasExporter(string serializedName)
        {
            return _exporterList.Any(e => e.SerializationName == serializedName);
        }

        /// <summary>
        /// Returns a newly created exporter for a given serialized name.
        ///
        /// If no known serialized was found, the default 'Pixelaria' exporter is returned, instead.
        /// </summary>
        public IBundleExporter CreateExporterForSerializedName(string serializedName)
        {
            var exporterEntry = ExporterEntryForSerializedName(serializedName) ?? _defaultExporter;

            return exporterEntry.Generator();
        }

        /// <summary>
        /// Returns the recorded display name for an exporter with a given serialized name.
        ///
        /// Returns null, if no exporter with the given serialized name is known.
        /// </summary>
        public string DisplayNameForExporter(string serializedName)
        {
            return Exporters.FirstOrDefault(e => e.SerializationName == serializedName)?.DisplayName;
        }

        /// <summary>
        /// Returns a newly-created, default exporter settings for a given serialized name.
        ///
        /// Returns null, if no exporter with the given serialized name is known.
        /// </summary>
        public IBundleExporterSettings CreateExporterSettingsForSerializedName(string serializedName)
        {
            if (!HasExporter(serializedName))
                return null;

            return CreateExporterForSerializedName(serializedName).GenerateDefaultSettings();
        }

        [CanBeNull]
        private KnownExporterEntry ExporterEntryForSerializedName(string serializedName)
        {
            return _exporterList.FirstOrDefault(exporter => exporter.SerializationName == serializedName);
        }

        private class KnownExporterEntry : IKnownExporterEntry
        {
            public string DisplayName { get; }
            public string SerializationName { get; }
            public bool HasSettings { get; }

            /// <summary>
            /// Gets the generator which is used to create new instances of this exporter kind
            /// </summary>
            public Func<IBundleExporter> Generator { get; }

            public KnownExporterEntry(string displayName, string serializationName, bool hasSettings, Func<IBundleExporter> generator)
            {
                DisplayName = displayName;
                SerializationName = serializationName;
                HasSettings = hasSettings;
                Generator = generator;
            }
        }
    }

    /// <summary>
    /// Interface for a known exporter entry
    /// </summary>
    public interface IKnownExporterEntry
    {
        /// <summary>
        /// Gets the display name for this exporter
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the serialization name for this exporter
        /// </summary>
        string SerializationName { get; }

        /// <summary>
        /// Gets whether this exporter has customizable settings
        /// </summary>
        bool HasSettings { get; }
    }
}

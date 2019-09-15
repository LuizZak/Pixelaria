﻿/*
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
using Pixelaria.Controllers.Exporters.Unity;

namespace Pixelaria.Controllers.Exporters
{
    /// <summary>
    /// Controller that deals with management and creation of known <see cref="IBundleExporter"/> kinds.
    /// </summary>
    public class ExporterController
    {
        /// <summary>
        /// Gets the singleton instance for this <see cref="ExporterController"/> class.
        /// </summary>
        public static ExporterController Instance = new ExporterController();

        private readonly KnownExporterEntry _defaultExporter = new KnownExporterEntry("Pixelaria", "pixelaria", () => new DefaultPngExporter(new DefaultSheetExporter()));
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
            _exporterList.Add(new KnownExporterEntry("Unity", "unityv1", () => new UnityExporter(new DefaultSheetExporter())));
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

        [CanBeNull]
        private KnownExporterEntry ExporterEntryForSerializedName(string serializedName)
        {
            return _exporterList.FirstOrDefault(exporter => exporter.SerializationName == serializedName);
        }

        private class KnownExporterEntry : IKnownExporterEntry
        {
            public string DisplayName { get; }
            public string SerializationName { get; }

            /// <summary>
            /// Gets the generator which is used to create new instances of this exporter kind
            /// </summary>
            public Func<IBundleExporter> Generator { get; }

            public KnownExporterEntry(string displayName, string serializationName, Func<IBundleExporter> generator)
            {
                DisplayName = displayName;
                SerializationName = serializationName;
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
    }
}

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

using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pixelaria.Data;
using Pixelaria.Data.Exports;

namespace Pixelaria.Controllers.Exporters
{
    /// <summary>
    /// Defines the behavior that must be implemented by bundle exporters in the program
    /// </summary>
    public interface IBundleExporter
    {
        /// <summary>
        /// Exports a given bundle in concurrent fashion, performing multiple bundle sheet encodings at once
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="cancellationToken">A cancellation token that is passed to the exporters and can be used to cancel the export process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        Task ExportBundleConcurrent([NotNull] Bundle bundle, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null);
        
        /// <summary>
        /// Returns a number from 0-1 describing the export progress for a given animation sheet.
        /// 0 means unstarted, 1 means the animation sheet was generated.
        /// </summary>
        /// <param name="sheet">The animation sheet to get the progress of</param>
        float ProgressForAnimationSheet([NotNull] AnimationSheet sheet);

        /// <summary>
        /// Sets the settings of this exporter.
        ///
        /// It must be an instance 
        /// </summary>
        void SetSettings(IBundleExporterSettings settings);

        /// <summary>
        /// Generates a default settings object for this bundle exporter.
        /// </summary>
        [NotNull]
        IBundleExporterSettings GenerateDefaultSettings();
    }

    /// <summary>
    /// Interface for configuring settings of an <see cref="IBundleExporter"/>.
    /// </summary>
    public interface IBundleExporterSettings
    {
        /// <summary>
        /// Gets the serialized name of the exporter that this settings object belongs to.
        /// </summary>
        [Browsable(false)]
        [NotNull]
        string ExporterSerializedName { get; }

        /// <summary>
        /// Creates an exact copy of this bundle export settings object.
        /// </summary>
        [NotNull]
        IBundleExporterSettings Clone();

        /// <summary>
        /// Saves the configurations of this exporter settings to a given stream
        /// </summary>
        void Save([NotNull] Stream stream);

        /// <summary>
        /// Loads the configurations of this exporter from a given stream
        /// </summary>
        void Load([NotNull] Stream stream);
    }
}
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

using JetBrains.Annotations;

namespace PixelariaLib.Controllers.Exporters
{
    /// <summary>
    /// Interface for an exporter controller
    /// </summary>
    public interface IExporterController
    {
        /// <summary>
        /// Returns whether an exporter exists with a given serialized name.
        /// </summary>
        bool HasExporter(string serializedName);

        /// <summary>
        /// Returns a newly created exporter for a given serialized name.
        ///
        /// If no known serialized was found, the default 'Pixelaria' exporter is returned, instead.
        /// </summary>
        [NotNull]
        IBundleExporter CreateExporterForSerializedName(string serializedName);

        /// <summary>
        /// Returns the recorded display name for an exporter with a given serialized name.
        ///
        /// Returns null, if no exporter with the given serialized name is known.
        /// </summary>
        [CanBeNull]
        string DisplayNameForExporter(string serializedName);

        /// <summary>
        /// Returns a newly-created, default exporter settings for a given serialized name.
        ///
        /// Returns null, if no exporter with the given serialized name is known.
        /// </summary>
        [CanBeNull]
        IBundleExporterSettings CreateExporterSettingsForSerializedName(string serializedName);
    }
}
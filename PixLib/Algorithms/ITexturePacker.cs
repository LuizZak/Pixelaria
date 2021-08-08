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

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PixLib.Data.Exports;

namespace PixLib.Algorithms
{
    /// <summary>
    /// Interface to be implemented by texture packers
    /// </summary>
    public interface ITexturePacker
    {
        /// <summary>
        /// Packs a given atlas with a specified progress event handler
        /// </summary>
        /// <param name="atlas">The texture atlas to pack</param>
        /// <param name="handler">The event handler for the packing process</param>
        void Pack([NotNull] TextureAtlas atlas, BundleExportProgressEventHandler handler = null);

        /// <summary>
        /// Packs a given atlas with a specified progress event handler
        /// </summary>
        /// <param name="atlas">The texture atlas to pack</param>
        /// <param name="cancellationToken">A token that can be used to cancel the export process</param>
        /// <param name="handler">The event handler for the packing process</param>
        Task Pack([NotNull] TextureAtlas atlas, CancellationToken cancellationToken, BundleExportProgressEventHandler handler = null);
    }
}
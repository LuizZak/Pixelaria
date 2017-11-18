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
using JetBrains.Annotations;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// A container for pipeline nodes and connections.
    /// 
    /// Exposes the nodes and connections between these nodes on this container.
    /// </summary>
    internal interface IPipelineContainer
    {
        /// <summary>
        /// Gets a list of all nodes currently on this container
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyList<IPipelineNode> Nodes { get; }

        /// <summary>
        /// Gets a list of all node connections present
        /// </summary>
        [NotNull, ItemNotNull]
        IReadOnlyList<IPipelineLinkConnection> Connections { get; }
        
        /// <summary>
        /// Returns true if the given input and output links have a connection present
        /// in <see cref="Connections"/>.
        /// </summary>
        bool AreConnected([NotNull] IPipelineInput input, [NotNull] IPipelineOutput output);

        /// <summary>
        /// Returns a list of all connections a link participates in
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable<IPipelineLinkConnection> ConnectionsFor([NotNull] IPipelineNodeLink link);
    }

    /// <summary>
    /// Simple interface that is used on objects that validate whether connections
    /// between nodes can be made.
    /// </summary>
    internal interface IPipelineConnectionDelegate
    {
        /// <summary>
        /// Returns whether a connection between the given input and output nodes can be made.
        /// </summary>
        bool CanConnect([NotNull] IPipelineInput input, [NotNull] IPipelineOutput output, [NotNull] IPipelineContainer container);
    }
}

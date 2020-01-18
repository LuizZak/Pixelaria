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

namespace PixPipelineGraph
{
    /// <summary>
    /// Provides a wrapper that pipes information about a particular pipeline node from
    /// a graph to the consumer.
    /// </summary>
    public interface IPipelineNodeView
    {
        /// <summary>
        /// Gets the node ID for the underlying pipeline node
        /// </summary>
        PipelineNodeId NodeId { get; }

        /// <summary>
        /// Gets the title for the underlying pipeline node
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the pipeline node kind for the underlying pipeline node
        /// </summary>
        PipelineNodeKind NodeKind { get; }

        /// <summary>
        /// Gets the pipeline body ID for the underlying pipeline node
        /// </summary>
        PipelineBodyId BodyId { get; }

        /// <summary>
        /// Gets the pipeline metadata for the underlying pipeline node
        /// </summary>
        IPipelineMetadata PipelineMetadata { get; }

        /// <summary>
        /// Gets a list of all inputs for the underlying node
        /// </summary>
        IReadOnlyList<IPipelineInput> Inputs { get; }

        /// <summary>
        /// Gets a list of all outputs for the underlying node
        /// </summary>
        IReadOnlyList<IPipelineOutput> Outputs { get; }
    }
}

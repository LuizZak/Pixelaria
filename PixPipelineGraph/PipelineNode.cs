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
    /// Represents a pipeline node
    /// </summary>
    internal class PipelineNode : IPipelineNodeView
    {
        public PipelineNodeKind NodeKind { get; set; }
        public string Title { get; set; }
        public PipelineNodeId NodeId { get; set; }

        public IReadOnlyList<IPipelineInput> Inputs => InternalInputs;
        public IReadOnlyList<IPipelineOutput> Outputs => InternalOutputs;

        public PipelineBodyId BodyId => Body.Id;

        internal List<InternalPipelineInput> InternalInputs = new List<InternalPipelineInput>();
        internal List<InternalPipelineOutput> InternalOutputs = new List<InternalPipelineOutput>();

        internal PipelineBody Body { get; set; }

        public IPipelineMetadata PipelineMetadata { get; set; } = new PipelineMetadata();

        internal PipelineNode(PipelineNodeId nodeId)
        {
            NodeId = nodeId;
        }

        internal PipelineInput NextAvailableInputId()
        {
            return new PipelineInput(NodeId, InternalInputs.Count);
        }

        internal PipelineOutput NextAvailableOutputId()
        {
            return new PipelineOutput(NodeId, InternalOutputs.Count);
        }
    }
}
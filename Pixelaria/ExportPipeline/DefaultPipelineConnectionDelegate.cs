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
using JetBrains.Annotations;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Default implementation of <see cref="IPipelineConnectionDelegate"/>.
    /// </summary>
    internal class DefaultPipelineConnectionDelegate : IPipelineConnectionDelegate
    {
        public bool CanConnect(IPipelineInput input, IPipelineOutput output, IPipelineGraph graph)
        {
            var inputNode = input.NodeId;
            var outputNode = output.NodeId;

            // Verify connection is not currently present already
            // Avoid links that belong to the same pipeline step
            if (input.NodeId == output.NodeId)
                return false;

            // Avoid links that are already connected
            if (graph.AreConnected(input.NodeId, output.NodeId))
                return false;

            // Verify connections won't result in a cycle in the node graphs
            if (IsIndirectlyConnected(inputNode, outputNode, graph))
                return false;
            
            return
                input.NodeId != output.NodeId &&
                input.DataType.IsAssignableFrom(output.DataType);
        }

        /// <summary>
        /// From a given node, traverses all parent nodes (connected via outputs) in breadth-first order until
        /// either <see cref="closure"/> returns false or all nodes have been traversed.
        /// </summary>
        /// <param name="node">Node to start traversing from.</param>
        /// <param name="container">Container to check connection with</param>
        /// <param name="closure">
        /// A visitor closure that will be called for <see cref="node"/> and all parent nodes.
        /// If this closure returns false, traversal stops earlier.
        /// </param>
        public void TraverseInputs(PipelineNodeId node, [NotNull] IPipelineGraph container, Func<PipelineNodeId, bool> closure)
        {
            var visited = new HashSet<PipelineNodeId>();
            var queue = new Queue<PipelineNodeId>();

            queue.Enqueue(node);

            // Do a breadth-first search
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();

                if (!closure(cur))
                    return;

                visited.Add(cur);

                var connections = container.ConnectionsFromNode(cur);

                foreach (var connection in connections)
                {
                    if (!visited.Contains(connection.End.NodeId))
                        queue.Enqueue(connection.End.NodeId);
                }
            }
        }

        /// <summary>
        /// Returns if a node is indirectly connected to another node either via inputs or outputs.
        /// 
        /// Used to detect cycles before they can be made.
        /// </summary>
        public bool IsIndirectlyConnected(PipelineNodeId node, PipelineNodeId target, [NotNull] IPipelineGraph container)
        {
            bool connected = false;

            // Try target -> node cycle first
            TraverseInputs(node, container, n =>
            {
                if (n == target)
                    connected = true;
                return !connected;
            });

            if (connected)
                return true;

            // Now try again just to see if we're not connected from node -> target instead
            TraverseInputs(target, container, n =>
            {
                if (n == node)
                    connected = true;
                return !connected;
            });

            return connected;
        }
    }
}
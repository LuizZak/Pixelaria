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

namespace PixPipelineGraph
{
    /// <summary>
    /// Represents a read-only pipeline graph object
    /// </summary>
    public interface IPipelineGraph
    {
        /// <summary>
        /// Gets a list of all pipeline nodes on this graph
        /// </summary>
        IReadOnlyList<PipelineNodeId> PipelineNodes { get; }

        /// <summary>
        /// Gets a list of all pipeline connections on this graph
        /// </summary>
        IReadOnlyList<IPipelineConnection> PipelineConnections { get; }

        /// <summary>
        /// Returns true if a given node is part of this graph.
        /// </summary>
        bool ContainsNode(PipelineNodeId nodeId);

        /// <summary>
        /// Lists all inputs for a pipeline node.
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="nodeId"/> consists of a reference an nonexistent node ID</exception>
        IReadOnlyList<PipelineInput> InputsForNode(PipelineNodeId nodeId);

        /// <summary>
        /// Lists all outputs for a pipeline node.
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="nodeId"/> consists of a reference an nonexistent node ID</exception>
        IReadOnlyList<PipelineOutput> OutputsForNode(PipelineNodeId nodeId);

        /// <summary>
        /// Returns true if two nodes are directly connected to one another.
        ///
        /// If both node ids represent the same node value, false is returned.
        /// </summary>
        bool AreConnected(PipelineNodeId node1, PipelineNodeId node2);

        /// <summary>
        /// Returns true if a given set of input/output are connected to one another.
        /// </summary>
        bool AreConnected(PipelineOutput output, PipelineInput input);

        /// <summary>
        /// Returns a list of all ingoing and outgoing connections for a given pipeline node on this graph
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="node"/> is a reference an nonexistent node ID</exception>
        IReadOnlyList<IPipelineConnection> AllConnectionsForNode(PipelineNodeId node);

        /// <summary>
        /// Returns a list of all connections that start from an output of a given node.
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="node"/> is a reference an nonexistent node ID</exception>
        IReadOnlyList<IPipelineConnection> ConnectionsFromNode(PipelineNodeId node);

        /// <summary>
        /// Returns a list of all connections that end on an input of a given node.
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="node"/> is a reference an nonexistent node ID</exception>
        IReadOnlyList<IPipelineConnection> ConnectionsTowardsNode(PipelineNodeId node);

        /// <summary>
        /// Returns a list of all connections that start from a given node output.
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="output"/> consists of a reference an nonexistent node ID</exception>
        IReadOnlyList<IPipelineConnection> ConnectionsFromOutput(PipelineOutput output);

        /// <summary>
        /// Returns a list of all connections that end on a given input.
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="input"/> consists of a reference an nonexistent node ID</exception>
        IReadOnlyList<IPipelineConnection> ConnectionsTowardsInput(PipelineInput input);

        /// <summary>
        /// Returns the pipeline metadata for a given node ID.
        ///
        /// May return null, in case the node is not present in this graph.
        /// </summary>
        [CanBeNull]
        IPipelineMetadata MetadataForNode(PipelineNodeId nodeId);
    }

    public static class PipelineGraphExtensions
    {
        /// <summary>
        /// From a given node, traverses all parent nodes (connected via outputs) in breadth-first order until
        /// either <see cref="!:closure" /> returns false or all nodes have been traversed.
        /// </summary>
        /// <param name="graph">The graph to perform the operations on</param>
        /// <param name="node">Node to start traversing from.</param>
        /// <param name="closure">
        /// A visitor closure that will be called for <see cref="!:node" /> and all parent nodes.
        /// If this closure returns false, traversal stops earlier.
        /// </param>
        public static void TraverseInputs([NotNull] this IPipelineGraph graph, PipelineNodeId node, Func<PipelineNodeId, bool> closure)
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

                var inputs = graph.InputsForNode(cur);

                foreach (var input in inputs)
                {
                    foreach (var connection in graph.ConnectionsTowardsInput(input))
                    {
                        if (!visited.Contains(connection.Start.PipelineNodeId))
                            queue.Enqueue(connection.Start.PipelineNodeId);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if a node is directly or indirectly connected to another node either via inputs or outputs.
        /// 
        /// Can be used to detect cycles before they can be made.
        /// </summary>
        public static bool AreIndirectlyConnected([NotNull] this IPipelineGraph graph, PipelineNodeId node, PipelineNodeId target)
        {
            bool connected = false;

            // Try target -> node cycle first
            graph.TraverseInputs(node, n =>
            {
                if (n == target)
                    connected = true;
                return !connected;
            });

            if (connected)
                return true;

            // Now try again just to see if we're not connected from node -> target instead
            graph.TraverseInputs(target, n =>
            {
                if (n == node)
                    connected = true;
                return !connected;
            });

            return connected;
        }
    }
}
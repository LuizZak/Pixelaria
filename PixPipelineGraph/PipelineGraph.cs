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

namespace PixPipelineGraph
{
    /// <summary>
    /// Describes a pipeline graph, with nodes for each pipeline step and vertices representing connections between node outputs
    /// </summary>
    public partial class PipelineGraph : IPipelineGraph
    {
        private readonly List<PipelineNode> _nodes = new List<PipelineNode>();
        private readonly List<PipelineConnection> _connections = new List<PipelineConnection>();

        /// <inheritdoc />
        public IReadOnlyList<PipelineNodeId> PipelineNodes => _nodes.Select(n => n.Id).ToArray();

        /// <inheritdoc />
        public IReadOnlyList<IPipelineConnection> PipelineConnections => _connections;

        /// <summary>
        /// Creates and returns a new empty node
        /// </summary>
        public PipelineNodeId CreateNode()
        {
            return CreateNode(_ => { });
        }

        /// <summary>
        /// Creates and returns a new empty node
        /// </summary>
        public PipelineNodeId CreateNode([NotNull, InstantHandle] Action<PipelineNodeBuilder> constructor)
        {
            var id = GenerateUniquePipelineNodeId();

            var builder = new PipelineNodeBuilder();

            constructor(builder);

            var node = builder.Build(id);

            _nodes.Add(node);

            return id;
        }

        /// <inheritdoc />
        public bool ContainsNode(PipelineNodeId nodeId)
        {
            return _nodes.Any(node => node.Id == nodeId);
        }

        /// <summary>
        /// Removes a given node from this graph.
        /// </summary>
        /// <remarks>
        /// Also removes any connections that are associated with the node on this graph.
        /// </remarks>
        /// <returns><i>true</i> if the node has been found and removed from this graph, <i>false</i> otherwise.</returns>
        public bool RemoveNode(PipelineNodeId nodeId)
        {
            var node = NodeWithId(nodeId);

            var outConnections = ConnectionsFromNode(nodeId);
            var inConnections = ConnectionsTowardsNode(nodeId);

            foreach (var connection in inConnections.Concat(outConnections))
            {
                Disconnect(connection);
            }

            return _nodes.Remove(node);
        }

        /// <summary>
        /// Creates a connection between an input and an output.
        ///
        /// If <see cref="input"/> and <see cref="output"/> are already connected, nothing is done.
        /// </summary>
        /// <returns>The newly created connection, or previous connection, in case it already existed. Returns null, if node could not be connected.</returns>
        /// <exception cref="ArgumentException">If either <see cref="input"/> or <see cref="output"/> consist of a reference an nonexistent node ID</exception>
        [CanBeNull]
        public IPipelineConnection Connect(PipelineOutput output, PipelineInput input)
        {
            if (input.PipelineNodeId == output.PipelineNodeId)
                return null;

            var existing = GetConnection(input, output);
            if (existing != null)
                return existing;

            var start = OutputWithIdOrException(output);
            var end = InputWithIdOrException(input);

            var connection = new PipelineConnection(start, end);

            _connections.Add(connection);

            return connection;
        }

        /// <summary>
        /// Tears down the connection between a given input and output.
        ///
        /// Does nothing, in case no connection currently exists between the two elements.
        /// </summary>
        public void Disconnect(PipelineInput input, PipelineOutput output)
        {
            var connection = GetConnection(input, output);
            if (connection == null)
                return;

            RemoveConnection(connection);
        }

        /// <summary>
        /// Tears down a given connection.
        /// </summary>
        public void Disconnect([NotNull] IPipelineConnection connection)
        {
            var conn = GetConnection(connection.End, connection.Start);
            if (conn == null)
                return;

            RemoveConnection(conn);
        }

        /// <inheritdoc />
        public IReadOnlyList<PipelineInput> InputsForNode(PipelineNodeId nodeId)
        {
            var node = NodeWithIdOrException(nodeId);

            return node.Inputs.Select(i => i.Id).ToList();
        }

        /// <inheritdoc />
        public IReadOnlyList<PipelineOutput> OutputsForNode(PipelineNodeId nodeId)
        {
            var node = NodeWithIdOrException(nodeId);

            return node.Outputs.Select(o => o.Id).ToList();
        }

        /// <inheritdoc />
        public bool AreConnected(PipelineNodeId node1, PipelineNodeId node2)
        {
            if (node1 == node2)
                return false;

            return _connections.Any(conn => conn.Input.Node.Id == node1 && conn.Output.Node.Id == node2 ||
                                            conn.Input.Node.Id == node2 && conn.Output.Node.Id == node1);
        }

        /// <inheritdoc />
        public bool AreConnected(PipelineOutput output, PipelineInput input)
        {
            return _connections.Any(conn => conn.Input.Id == input && conn.Output.Id == output);
        }

        /// <inheritdoc />
        public IReadOnlyList<IPipelineConnection> AllConnectionsForNode(PipelineNodeId node)
        {
            return _connections.Where(c => c.Input.Node.Id == node || c.Output.Node.Id == node).ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyList<IPipelineConnection> ConnectionsFromNode(PipelineNodeId node)
        {
            return _connections.Where(c => c.Output.Node.Id == node).ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyList<IPipelineConnection> ConnectionsTowardsNode(PipelineNodeId node)
        {
            return _connections.Where(c => c.Input.Node.Id == node).ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyList<IPipelineConnection> ConnectionsFromOutput(PipelineOutput output)
        {
            return _connections.Where(c => c.Output.Id == output).ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyList<IPipelineConnection> ConnectionsTowardsInput(PipelineInput input)
        {
            return _connections.Where(c => c.Input.Id == input).ToArray();
        }

        /// <inheritdoc />
        public IPipelineMetadata MetadataForNode(PipelineNodeId nodeId)
        {
            return _nodes.FirstOrDefault(n => n.Id == nodeId)?.PipelineMetadata;
        }
    }

    #region Subgraph Operations

    public partial class PipelineGraph
    {
        /// <summary>
        /// Adds a given graph's contents to this graph, copying over nodes and any available connections, as well.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="other"/> is the same instance as <c>this</c></exception>
        public void AddFromGraph([NotNull] PipelineGraph other)
        {
            if(ReferenceEquals(this, other))
                throw new InvalidOperationException("Cannot copy a graph into itself");

            var nodesMap = new Dictionary<PipelineNodeId, PipelineNodeId>();

            foreach (var node in other._nodes)
            {
                nodesMap[node.Id] = CreateNode(n =>
                {
                    foreach (var input in node.Inputs)
                    {
                        n.CreateInput(input.Name, i =>
                        {
                            foreach (var type in input.DataTypes)
                            {
                                i.AddInputType(type);
                            }
                        });
                    }

                    foreach (var output in node.Outputs)
                    {
                        n.CreateOutput(output.Name, o =>
                        {
                            o.SetOutputType(output.DataType);
                        });
                    }
                });
            }

            foreach (var connection in other._connections)
            {
                var start = nodesMap[connection.Start.PipelineNodeId];
                var end = nodesMap[connection.End.PipelineNodeId];

                int startIndex = connection.Start.Index;
                int endIndex = connection.End.Index;

                var output = OutputsForNode(start)[startIndex];
                var input = InputsForNode(end)[endIndex];

                Connect(output, input);
            }
        }
    }

    #endregion

    #region Private Members

    public partial class PipelineGraph
    {
        private void RemoveConnection(PipelineConnection connection)
        {
            _connections.Remove(connection);
        }

        [CanBeNull]
        private InternalPipelineInput InputWithId(PipelineInput input)
        {
            return NodeWithId(input.PipelineNodeId)?.Inputs[input.Index];
        }

        [CanBeNull]
        private InternalPipelineOutput OutputWithId(PipelineOutput input)
        {
            return NodeWithId(input.PipelineNodeId)?.Outputs[input.Index];
        }

        [NotNull]
        private InternalPipelineInput InputWithIdOrException(PipelineInput input)
        {
            return NodeWithIdOrException(input.PipelineNodeId).Inputs[input.Index];
        }

        [NotNull]
        private InternalPipelineOutput OutputWithIdOrException(PipelineOutput input)
        {
            return NodeWithIdOrException(input.PipelineNodeId).Outputs[input.Index];
        }

        [NotNull]
        private PipelineNode NodeWithIdOrException(PipelineNodeId nodeId)
        {
            return NodeWithId(nodeId) ?? throw new ArgumentException($"Cannot find corresponding node for node id ${nodeId}");
        }

        [CanBeNull]
        private PipelineNode NodeWithId(PipelineNodeId nodeId)
        {
            return _nodes.FirstOrDefault(node => node.Id == nodeId);
        }

        [CanBeNull]
        private PipelineConnection GetConnection(PipelineInput input, PipelineOutput output)
        {
            return _connections.FirstOrDefault(conn => conn.Input.Id == input && conn.Output.Id == output);
        }

        private static PipelineNodeId GenerateUniquePipelineNodeId() => new PipelineNodeId(Guid.NewGuid());
    }

    #endregion
}

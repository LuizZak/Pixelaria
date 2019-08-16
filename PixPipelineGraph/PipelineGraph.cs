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
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
        /// Gets or sets the pipeline graph body provider.
        /// </summary>
        [NotNull]
        public IPipelineGraphNodeProvider NodeProvider { get; set; }

        /// <summary>
        /// A delegate that is invoked before making connections on this graph.
        /// </summary>
        [CanBeNull]
        public IPipelineConnectionDelegate ConnectionDelegate { get; set; }

        #region Events

        /// <summary>
        /// An event fired when a connection between an input and an output is made.
        /// </summary>
        public event ConnectionEventHandler ConnectionWasAdded;

        /// <summary>
        /// An event fired when a connection between an input and an output is about to be undone.
        /// </summary>
        public event ConnectionEventHandler ConnectionWillBeRemoved;

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="PipelineGraph"/> with a given body provider.
        /// </summary>
        public PipelineGraph([NotNull] IPipelineGraphNodeProvider nodeProvider)
        {
            NodeProvider = nodeProvider;
        }

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

            var builder = new PipelineNodeBuilder(NodeProvider);

            constructor(builder);

            var node = builder.Build(id);

            _nodes.Add(node);

            return id;
        }

        /// <summary>
        /// Requests that a node with a specified kind be created on this graph.
        ///
        /// The node kind is created from the currently configured <see cref="NodeProvider"/>.
        /// In case the provider cannot create a node kind (see <see cref="IPipelineGraphNodeProvider.CanCreateNode"/>),
        /// no node is created and <c>null</c> is returned instead.
        /// </summary>
        [CanBeNull]
        public PipelineNodeId? CreateNode(PipelineNodeKind kind)
        {
            if (!NodeProvider.CanCreateNode(kind))
                return null;

            return CreateNode(builder =>
            {
                NodeProvider.CreateNode(kind, builder);
            });
        }

        /// <summary>
        /// Helper method for creating one-parameter-one-output pipeline nodes.
        ///
        /// All type handling is done automatically while creating the pipeline node.
        /// </summary>
        public PipelineNodeId CreateFromLambda<T1, T2>(string title, [NotNull] Func<T1, T2> lambda)
        {
            return CreateNode(builder =>
            {
                builder.SetTitle(title);
                builder.CreateInput("v1", inputBuilder => inputBuilder.SetInputType(typeof(T1)));
                builder.CreateOutput("o", outputBuilder => outputBuilder.SetOutputType(typeof(T2)));
                builder.SetBody(new PipelineBody(new PipelineBodyId(Guid.NewGuid().ToString()), new []{typeof(T1)}, new []{ typeof(T2) }, 
                    context =>
                    {
                        if (context.TryGetIndexedInputs(out IObservable<T1> t1))
                        {
                            return AnyObservable.FromObservable(t1.Select(lambda));
                        }

                        return PipelineBodyInvocationResponse.MismatchedInputType<T2>(typeof(T1));
                    }));
            });
        }

        /// <summary>
        /// Helper method for creating two-parameter-one-output pipeline nodes.
        ///
        /// All type handling is done automatically while creating the pipeline node.
        ///
        /// The source observables are combined in a cartesian product, taking each available input combination and
        /// producing an output that is mapped with the provided <see cref="lambda"/> function.
        /// </summary>
        public PipelineNodeId CreateFromLambda<T1, T2, T3>(string title, [NotNull] Func<T1, T2, T3> lambda)
        {
            return CreateNode(builder =>
            {
                builder.SetTitle(title);
                builder.CreateInput("v1", inputBuilder => inputBuilder.SetInputType(typeof(T1)));
                builder.CreateInput("v2", inputBuilder => inputBuilder.SetInputType(typeof(T2)));
                builder.CreateOutput("o", outputBuilder => outputBuilder.SetOutputType(typeof(T3)));
                builder.SetBody(new PipelineBody(new PipelineBodyId(Guid.NewGuid().ToString()), new[] { typeof(T1), typeof(T2) }, new[] {typeof(T3)},
                    context =>
                    {
                        try
                        {
                            context.GetIndexedInputs(out IObservable<T1> t1, out IObservable<T2> t2);

                            var cartesian = t1.SelectMany((arg1, _) => t2.Select(arg2 => (arg1, arg2)))
                                .Select(tuple => lambda(tuple.arg1, tuple.arg2));

                            return AnyObservable.FromObservable(cartesian);
                        }
                        catch (Exception e)
                        {
                            return PipelineBodyInvocationResponse.Exception<T3>(e);
                        }
                    }));
            });
        }

        /// <summary>
        /// Helper method for creating zero-parameter-one-output pipeline nodes.
        ///
        /// All type handling is done automatically while creating the pipeline node.
        /// </summary>
        public PipelineNodeId CreateFromGenerator<T>(string title, [NotNull] Func<T> generator)
        {
            return CreateNode(builder =>
            {
                builder.SetTitle(title);
                builder.CreateOutput("o", outputBuilder => outputBuilder.SetOutputType(typeof(T)));
                builder.SetBody(new PipelineBody(new PipelineBodyId(Guid.NewGuid().ToString()), Type.EmptyTypes, new[] {typeof(T)},
                    context =>
                    {
                        try
                        {
                            var observable = new AnonymousObservable<T>(observer =>
                            {
                                observer.OnNext(generator());
                                observer.OnCompleted();

                                return Disposable.Empty;
                            });

                            return AnyObservable.FromObservable(observable);
                        }
                        catch (Exception e)
                        {
                            return PipelineBodyInvocationResponse.Exception<T>(e);
                        }
                    }));
            });
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
            if (input.NodeId == output.NodeId)
                return null;

            var existing = GetConnection(input, output);
            if (existing != null)
                return existing;

            var start = OutputWithIdOrException(output);
            var end = InputWithIdOrException(input);

            var connection = new PipelineConnection(start, end);

            _connections.Add(connection);

            ConnectionWasAdded?.Invoke(this, new ConnectionEventArgs(connection));

            return connection;
        }

        /// <summary>
        /// Creates a connection between two nodes with the first match of input/output found between the two.
        ///
        /// If no connection could be made, <c>null</c> is returned, instead.
        /// </summary>
        [CanBeNull]
        public IPipelineConnection Connect(PipelineNodeId start, PipelineNodeId end)
        {
            // Detect cycles
            if (AreDirectlyConnected(start, end))
                return null;

            // Find first matching output from this that matches an input from other
            foreach (var output in OutputsForNode(start))
            {
                foreach (var input in InputsForNode(end))
                {
                    var connections = ConnectionsTowardsInput(input);

                    if (connections.Any(c => c.Start == output))
                        continue;

                    if (!CanConnect(input, output)) continue;

                    return Connect(output, input);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a connection between a node and another node's input with the first match of input/output found between the two.
        ///
        /// If no connection could be made, <c>null</c> is returned, instead.
        /// </summary>
        [CanBeNull]
        public IPipelineConnection Connect(PipelineNodeId start, PipelineInput input)
        {
            // Detect cycles
            if (AreDirectlyConnected(input.NodeId, start))
                return null;

            // Find first matching output from this that matches an input from other
            foreach (var output in OutputsForNode(start))
            {
                var connections = ConnectionsTowardsInput(input);

                if (connections.Any(c => c.Start == output))
                    continue;

                if (!CanConnect(input, output)) continue;

                return Connect(output, input);
            }

            return null;
        }

        /// <summary>
        /// Returns whether a pipeline input can be connected to an output.
        /// 
        /// The method looks through the proper data types accepted by the input and the data type
        /// of the output to make the decision.
        /// </summary>
        public bool CanConnect(PipelineInput input, PipelineOutput output)
        {
            if (input.NodeId == output.NodeId)
                return false;

            var inputData = GetInput(input);
            var outputData = GetOutput(output);

            if (inputData == null || outputData == null)
                return false;

            if (ConnectionDelegate != null && !ConnectionDelegate.CanConnect(inputData, outputData, this))
                return false;

            return inputData.DataType.IsAssignableFrom(outputData.DataType);
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

        [CanBeNull]
        public IPipelineInput GetInput(PipelineInput input)
        {
            return InputWithId(input);
        }

        [CanBeNull]
        public IPipelineOutput GetOutput(PipelineOutput output)
        {
            return OutputWithId(output);
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
        public bool AreDirectlyConnected(PipelineNodeId node, PipelineNodeId target)
        {
            bool connected = false;
            
            // Try target -> node cycle first
            this.TraverseInputs(node, n =>
            {
                if (n == target)
                    connected = true;
                return !connected;
            });

            if (connected)
                return true;

            // Now try again just to see if we're not connected from node -> target instead
            this.TraverseInputs(target, n =>
            {
                if (n == node)
                    connected = true;
                return !connected;
            });

            return connected;
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

        /// <summary>
        /// Returns the pipeline body for the given node.
        ///
        /// May be <c>null</c>, in case no node was found with a matching id.
        /// </summary>
        [CanBeNull]
        public PipelineBody BodyForNode(PipelineNodeId nodeId)
        {
            return _nodes.FirstOrDefault(n => n.Id == nodeId)?.Body;
        }

        /// <summary>
        /// Returns the title of a node with a given ID.
        ///
        /// May be <c>null</c>, in case no node was found with a matching id.
        /// </summary>
        [CanBeNull]
        public string TitleForNode(PipelineNodeId nodeId)
        {
            return _nodes.FirstOrDefault(n => n.Id == nodeId)?.Title;
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
                    n.SetBody(node.Body);

                    foreach (var input in node.Inputs)
                    {
                        n.CreateInput(input.Name, i =>
                        {
                            i.SetInputType(input.DataType);
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
                var start = nodesMap[connection.Start.NodeId];
                var end = nodesMap[connection.End.NodeId];

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
            ConnectionWillBeRemoved?.Invoke(this, new ConnectionEventArgs(connection));

            _connections.Remove(connection);
        }

        [CanBeNull]
        private InternalPipelineInput InputWithId(PipelineInput input)
        {
            return NodeWithId(input.NodeId)?.Inputs[input.Index];
        }

        [CanBeNull]
        private InternalPipelineOutput OutputWithId(PipelineOutput input)
        {
            return NodeWithId(input.NodeId)?.Outputs[input.Index];
        }

        [NotNull]
        private InternalPipelineInput InputWithIdOrException(PipelineInput input)
        {
            return NodeWithIdOrException(input.NodeId).Inputs[input.Index];
        }

        [NotNull]
        private InternalPipelineOutput OutputWithIdOrException(PipelineOutput input)
        {
            return NodeWithIdOrException(input.NodeId).Outputs[input.Index];
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

    /// <summary>
    /// Delegate for connection events in a <see cref="PipelineGraph"/>.
    /// </summary>
    public delegate void ConnectionEventHandler([NotNull] object sender, [NotNull] ConnectionEventArgs args);

    /// <summary>
    /// Event args for a <see cref="ConnectionEventHandler"/> event.
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        public IPipelineConnection Connection { get; }
        
        public ConnectionEventArgs(IPipelineConnection connection)
        {
            Connection = connection;
        }
    }
}

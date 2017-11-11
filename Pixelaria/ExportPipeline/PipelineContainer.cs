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

namespace Pixelaria.ExportPipeline
{
    internal class PipelineContainer: IPipelineContainer
    {
        private readonly List<IPipelineNode> _nodes = new List<IPipelineNode>();
        private readonly List<IPipelineLinkConnection> _connections = new List<IPipelineLinkConnection>();
        
        public IReadOnlyList<IPipelineNode> Nodes => _nodes;
        public IReadOnlyList<IPipelineLinkConnection> Connections => _connections;

        /// <summary>
        /// Connection delegate when making connections
        /// </summary>
        [NotNull]
        public IPipelineConnectionDelegate ConnectionDelegate { get; set; } = new DefaultPipelineConnectionDelegate();

        /// <summary>
        /// Adds a new empty instance of a given pipeline node type.
        /// </summary>
        [NotNull]
        public T AddNode<T>() where T : IPipelineNode, new()
        {
            var node = new T();

            ValidateNoIdCollision(node);
            
            if(node.Id == Guid.Empty)
                throw new InvalidOperationException($"Type {typeof(T).Name} returned from constructor with Id == Guid.Empty. This value must be set to a non-empty Guid on construction by all pipeline nodes.");

            _nodes.Add(node);

            return node;
        }

        /// <summary>
        /// Adds an existing pipeline node to this container.
        /// 
        /// If an existing node with the same matching Id already exists,
        /// an exception is raised.
        /// </summary>
        public void AddNode([NotNull] IPipelineNode node)
        {
            ValidateNoIdCollision(node);

            if (node.Id == Guid.Empty)
                throw new InvalidOperationException($"Node {node} has Id == Guid.Empty. This value must be set to a non-empty Guid before being added to a pipeline container.");

            _nodes.Add(node);
        }

        /// <summary>
        /// Removes a given node from this pipeline container
        /// </summary>
        public void RemoveNode([NotNull] IPipelineNode node)
        {
            _nodes.Remove(node);
        }

        /// <summary>
        /// Adds a new connection between two pipeline node links.
        /// 
        /// If the types for the nodes do not match, no connection is made and null is returned.
        /// </summary>
        [CanBeNull]
        public IPipelineLinkConnection AddConnection([NotNull] IPipelineInput input, [NotNull] IPipelineOutput output)
        {
            if (!ConnectionDelegate.CanConnect(input, output, this))
                return null;

            var connection = new PipelineLinkConnection(input, output, conn =>
            {
                input.Disconnect(output);
                _connections.Remove(conn);
            });

            _connections.Add(connection);

            return connection;
        }

        public IEnumerable<IPipelineLinkConnection> ConnectionsFor(IPipelineNodeLink link)
        {
            return _connections.Where(con => con.Input == link || con.Output == link);
        }

        public bool AreConnected(IPipelineInput start, IPipelineOutput end)
        {
            return _connections.Any(view => ReferenceEquals(view.Input, start) && ReferenceEquals(view.Output, end));
        }

        private void ValidateNoIdCollision([NotNull] IPipelineNode node)
        {
            if (node.Id == Guid.Empty)
                return;
            
            if(_nodes.Any(n => n.Id == node.Id && !ReferenceEquals(n, node)))
                throw new InvalidOperationException($"A node with a matching id {node.Id} already exists within this container.");
        }
    }
}
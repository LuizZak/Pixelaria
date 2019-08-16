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
    /// Class used to help track down changes that will be performed on a <see cref="PipelineGraph"/>
    /// using a comprehensive changes stack.
    /// </summary>
    public class PipelineGraphChanges
    {
        private Stack<StackItem> _stack = new Stack<StackItem>(new []{new StackItem()});
        private StackItem TopmostItem => _stack.Peek();

        public int StackDepth => _stack.Count;

        public IReadOnlyList<PipelineNodeId> NodesCreated => TopmostItem.NodesCreated;
        public IReadOnlyList<PipelineNodeId> NodesRemoved => TopmostItem.NodesRemoved;
        public IReadOnlyList<IPipelineConnection> ConnectionsCreated => TopmostItem.ConnectionsCreated;
        public IReadOnlyList<IPipelineConnection> ConnectionsRemoved => TopmostItem.ConnectionsRemoved;

        internal PipelineGraphChanges()
        {

        }

        private PipelineGraphChanges([NotNull] StackItem stackItem)
        {
            TopmostItem.NodesCreated.AddRange(stackItem.NodesCreated);
            TopmostItem.NodesRemoved.AddRange(stackItem.NodesRemoved);
            TopmostItem.ConnectionsCreated.AddRange(stackItem.ConnectionsCreated);
            TopmostItem.ConnectionsRemoved.AddRange(stackItem.ConnectionsRemoved);

            FlattenEvents();
        }

        internal void RecordNodeCreated(PipelineNodeId nodeId)
        {
            TopmostItem.NodesCreated.Add(nodeId);
        }

        internal void RecordNodeRemoved(PipelineNodeId nodeId)
        {
            TopmostItem.NodesRemoved.Add(nodeId);
        }

        internal void RecordConnectionCreated(IPipelineConnection connection)
        {
            TopmostItem.ConnectionsCreated.Add(connection);
        }

        internal void RecordConnectionRemoved(IPipelineConnection connection)
        {
            TopmostItem.ConnectionsRemoved.Add(connection);
        }

        internal void PushStack()
        {
            _stack.Push(new StackItem());
        }

        internal PipelineGraphChanges PopStack()
        {
            if(_stack.Count <= 1)
                throw new InvalidOperationException("Unbalanced stack push/pop calls");

            var topmost = _stack.Pop();

            TopmostItem.NodesCreated.AddRange(topmost.NodesCreated);
            TopmostItem.NodesRemoved.AddRange(topmost.NodesRemoved);
            TopmostItem.ConnectionsCreated.AddRange(topmost.ConnectionsCreated);
            TopmostItem.ConnectionsRemoved.AddRange(topmost.ConnectionsRemoved);

            return new PipelineGraphChanges(topmost);
        }

        internal void FlattenEvents()
        {
            // Remove event intersections (i.e. node/connection was added then removed during the same event)
            var intersectNodes = TopmostItem.NodesCreated.Intersect(TopmostItem.NodesRemoved).ToArray();
            var intersectConnections = TopmostItem.ConnectionsCreated.Intersect(TopmostItem.ConnectionsRemoved).ToArray();

            foreach (var id in intersectNodes)
            {
                TopmostItem.NodesCreated.Remove(id);
                TopmostItem.NodesRemoved.Remove(id);
            }

            foreach (var id in intersectConnections)
            {
                TopmostItem.ConnectionsCreated.Remove(id);
                TopmostItem.ConnectionsRemoved.Remove(id);
            }
        }
        
        private class StackItem
        {
            public readonly List<PipelineNodeId> NodesCreated = new List<PipelineNodeId>();
            public readonly List<PipelineNodeId> NodesRemoved = new List<PipelineNodeId>();
            public readonly List<IPipelineConnection> ConnectionsCreated = new List<IPipelineConnection>();
            public readonly List<IPipelineConnection> ConnectionsRemoved = new List<IPipelineConnection>();
        }
    }
}

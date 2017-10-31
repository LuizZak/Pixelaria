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
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    /// <summary>
    /// Used to auto-layout graph nodes for the export pipeline view
    /// </summary>
    internal class GraphLayout
    {
        public static void LayoutNodes(LayeredDirectedAcyclicNode node)
        {
            
        }
    }
    
    /// <summary>
    /// Opaque interface for a node on a graph
    /// </summary>
    public interface IGraphNode<out T> where T: IGraphNode<T>
    {
        /// <summary>
        /// Gets outgoing connections from this node
        /// </summary>
        IReadOnlyList<T> Next { get; }

        /// <summary>
        /// Gets ingoing connections from this node
        /// </summary>
        IReadOnlyList<T> Previous { get; }
    }
    
    public sealed class DirectedAcyclicNode<T> : IGraphNode<DirectedAcyclicNode<T>> where T: IEquatable<T>
    {
        /// <summary>
        /// Used during longest path finding
        /// </summary>
        private int _distance;

        private readonly List<DirectedAcyclicNode<T>> _previous = new List<DirectedAcyclicNode<T>>();
        private readonly List<DirectedAcyclicNode<T>> _next = new List<DirectedAcyclicNode<T>>();

        public IReadOnlyList<DirectedAcyclicNode<T>> Previous => _previous;
        public IReadOnlyList<DirectedAcyclicNode<T>> Next => _next;

        [CanBeNull]
        public readonly T Value;

        public DirectedAcyclicNode(T value)
        {
            Value = value;
        }

        public DirectedAcyclicNode<T> CreateChild(T tag, Action<DirectedAcyclicNode<T>> initFunc = null)
        {
            var newNode = new DirectedAcyclicNode<T>(tag);

            initFunc?.Invoke(newNode);

            AddChild(newNode);

            return newNode;
        }

        public void AddChild([NotNull] DirectedAcyclicNode<T> node)
        {
            // Detect cycles
            if (RecurseDetectCycle(node))
                throw new ArgumentException(@"Detected cycle in graph", nameof(node));

            if (!node._previous.Contains(this))
                node._previous.Add(this);

            if (!_next.Contains(node))
                _next.Add(node);
        }

        [CanBeNull]
        public DirectedAcyclicNode<T> FindChild(T value)
        {
            if (Equals(Value, value))
                return this;

            return _next.Select(node => node.FindChild(value)).FirstOrDefault(found => found != null);
        }

        /// <summary>
        /// Returns true if the given node is a parent of this node.
        /// </summary>
        public bool IsDescendentOf(DirectedAcyclicNode<T> node)
        {
            if (node == this)
                return false;

            var queue = new Queue<DirectedAcyclicNode<T>>();
            var visited = new HashSet<DirectedAcyclicNode<T>>();

            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var next = queue.Dequeue();
                if (visited.Contains(next))
                    continue;

                if (next == node)
                    return true;

                visited.Add(next);

                foreach (var parent in next.Previous)
                {
                    queue.Enqueue(parent);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the topological sorting of this node's sub graph.
        /// </summary>
        public DirectedAcyclicNode<T>[] TopologicalSorted()
        {
            var stack = new Stack<DirectedAcyclicNode<T>>();
            var visited = new HashSet<DirectedAcyclicNode<T>>();

            RecursiveTopologicalSorted(stack, visited);

            return stack.ToArray();
        }

        /// <summary>
        /// Returns the longest path on this graph.
        /// </summary>
        public DirectedAcyclicNode<T>[] LongestPath()
        {
            // First, reset all distances of the graph
            BreadthFirstTraverseAll(node =>
            {
                node._distance = -1;
            });

            var sorted = TopologicalSorted();

            foreach (var node in sorted)
            {
                if (node.Previous.Count == 0)
                {
                    node._distance = 0;
                    continue;
                }

                node._distance = node.Previous.Max(n => n._distance);
                if (node._distance == -1)
                    node._distance = 0;
                else
                    node._distance += 1;
            }
            
            // Find largest distance child
            int largestD = -1;
            DirectedAcyclicNode<T> largestDNode = null;

            BreadthFirstTraverseChildren(node =>
            {
                if (node._distance <= largestD)
                    return;

                largestD = node._distance;
                largestDNode = node;
            });

            Debug.Assert(largestDNode != null, "largestDNode != null");

            var sortedStack = new Stack<DirectedAcyclicNode<T>>();

            largestDNode.RecursiveFindLargestDistance(this, sortedStack);

            return sortedStack.ToArray();
        }

        private void RecursiveFindLargestDistance([NotNull] DirectedAcyclicNode<T> stopAtNode, [NotNull] Stack<DirectedAcyclicNode<T>> stack)
        {
            stack.Push(this);

            if (stopAtNode == this)
                return;
            
            var largest = Previous.OrderByDescending(n => n._distance).FirstOrDefault();

            if (largest != null && largest._distance > -1)
            {
                largest.RecursiveFindLargestDistance(stopAtNode, stack);
            }
        }

        private void RecursiveTopologicalSorted([NotNull] Stack<DirectedAcyclicNode<T>> stack, [NotNull] ISet<DirectedAcyclicNode<T>> visited)
        {
            visited.Add(this);

            foreach (var node in Next)
            {
                if(!visited.Contains(node))
                    node.RecursiveTopologicalSorted(stack, visited);
            }
            
            stack.Push(this);
        }

        private bool RecurseDetectCycle([NotNull] DirectedAcyclicNode<T> node)
        {
            return node == this || _previous.Any(previous => previous.RecurseDetectCycle(node));
        }

        private void BreadthFirstTraverseChildren([InstantHandle] Action<DirectedAcyclicNode<T>> visit)
        {
            var queue = new Queue<DirectedAcyclicNode<T>>();
            var visited = new HashSet<DirectedAcyclicNode<T>>();

            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var next = queue.Dequeue();
                if (visited.Contains(next))
                    continue;

                visited.Add(next);

                visit(next);

                foreach (var node in next.Next)
                {
                    queue.Enqueue(node);
                }
            }
        }

        private void BreadthFirstTraverseAll([InstantHandle] Action<DirectedAcyclicNode<T>> visit)
        {
            var queue = new Queue<DirectedAcyclicNode<T>>();
            var visited = new HashSet<DirectedAcyclicNode<T>>();

            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var next = queue.Dequeue();
                if (visited.Contains(next))
                    continue;

                visited.Add(next);

                visit(next);

                foreach (var node in next.Next)
                {
                    queue.Enqueue(node);
                }
                foreach (var node in next.Previous)
                {
                    queue.Enqueue(node);
                }
            }
        }
    }

    public sealed class LayeredDirectedAcyclicNode : IGraphNode<LayeredDirectedAcyclicNode>
    {
        private readonly List<LayeredDirectedAcyclicNode> _previous = new List<LayeredDirectedAcyclicNode>();
        private readonly List<LayeredDirectedAcyclicNode> _next = new List<LayeredDirectedAcyclicNode>();
        
        public IReadOnlyList<LayeredDirectedAcyclicNode> Previous => _previous;
        public IReadOnlyList<LayeredDirectedAcyclicNode> Next => _next;

        public object Tag { get; }

        public int Layer { get; private set; }
        public bool IsPlaceholder => true;
        
        private LayeredDirectedAcyclicNode(object tag, int layer)
        {
            Tag = tag;
            Layer = layer;
        }

        public void SwapNodesTo(int child1, int child2)
        {
            var n1 = _next[child1];

            _next[child1] = _next[child2];
            _next[child2] = n1;
        }

        private void RecurseAdjustLayer()
        {
            // Root node
            if (_previous.Count == 0)
                Layer = 0;

            foreach (var to in _next)
            {
                to.Layer = Math.Max(to.Layer, Layer + 1);

                to.RecurseAdjustLayer();
            }
        }

        /// <summary>
        /// Recursively creates a PlaceholderNode tree from a given Node object.
        /// 
        /// Layers are automatically deduced along the way.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static LayeredDirectedAcyclicNode FromNode<T>([NotNull] DirectedAcyclicNode<T> node) where T: IEquatable<T>
        {
            var nodeMap = new Dictionary<object, LayeredDirectedAcyclicNode>();

            // Do a breadth-first pass to adjust any multiple node-from references along the way
            var root = new LayeredDirectedAcyclicNode(node.Value, 0);
            var end = new LayeredDirectedAcyclicNode(null, 0);

            var queue = new Queue<DirectedAcyclicNode<T>>();
            var visited = new HashSet<DirectedAcyclicNode<T>>();

            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var next = queue.Dequeue();

                if (visited.Contains(next))
                    continue;

                visited.Add(next);

                if (next != node && next.Value != null)
                {
                    nodeMap[next.Value] = new LayeredDirectedAcyclicNode(next.Value, 0);
                }

                foreach (var node1 in next.Next)
                {
                    queue.Enqueue(node1);
                }
            }

            queue.Clear();
            visited.Clear();

            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var next = queue.Dequeue();

                if (visited.Contains(next))
                    continue;

                visited.Add(next);

                foreach (var node1 in next.Next)
                {
                    queue.Enqueue(node1);
                }

                if (next == node || next.Value == null) // Root node
                {
                    continue;
                }

                var target = nodeMap[next.Value];

                var nodesFrom = next.Previous.Select(n => n == node || n.Value == null ? root : nodeMap[n.Value]).ToArray();
                var nodesTo = next.Next.Select(n => n == node || n.Value == null ? root : nodeMap[n.Value]).ToArray();

                target._previous.AddRange(nodesFrom);
                target._next.AddRange(nodesTo);

                // This is a leaf node- we just tie it to the end node then.
                if (nodesTo.Length == 0)
                {
                    target._next.Add(end);
                    end._previous.Add(target);
                }

                foreach (var n in nodesFrom)
                {
                    if (!n._next.Contains(n))
                        n._next.Add(target);
                }
            }

            root.RecurseAdjustLayer();

            return root;
        }
    }
}

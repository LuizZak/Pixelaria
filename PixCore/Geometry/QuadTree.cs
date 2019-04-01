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

/*
    The following code is based off of Velcro Physics/Farseer Physics Engine,
    the license of which is written bellow:

    MIT License

    Copyright (c) 2017 Ian Qvist

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PixCore.Geometry
{
    public class QuadTree<T>
    {
        public int MaxBucket;
        public int MaxDepth;
        public List<QuadTreeElement<T>> Nodes;
        public AABB Span;
        public QuadTree<T>[] SubTrees;

        public QuadTree(AABB span, int maxBucket, int maxDepth)
        {
            Span = span;
            Nodes = new List<QuadTreeElement<T>>();

            MaxBucket = maxBucket;
            MaxDepth = maxDepth;
        }

        public bool IsPartitioned => SubTrees != null;

        /// <summary>
        /// returns the quadrant of span that entirely contains test. if none, return 0.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private static int Partition(in AABB span, in AABB test)
        {
            if (Q1(in span).Contains(in test)) return 1;
            if (Q2(in span).Contains(in test)) return 2;
            if (Q3(in span).Contains(in test)) return 3;
            if (Q4(in span).Contains(in test)) return 4;

            return 0;
        }

        public void AddNode(QuadTreeElement<T> node)
        {
            if (!IsPartitioned)
            {
                if (Nodes.Count >= MaxBucket && MaxDepth > 0) //bin is full and can still subdivide
                {
                    //
                    //partition into quadrants and sort existing nodes amonst quads.
                    //
                    Nodes.Add(node); //treat new node just like other nodes for partitioning

                    SubTrees = new QuadTree<T>[4];
                    SubTrees[0] = new QuadTree<T>(Q1(Span), MaxBucket, MaxDepth - 1);
                    SubTrees[1] = new QuadTree<T>(Q2(Span), MaxBucket, MaxDepth - 1);
                    SubTrees[2] = new QuadTree<T>(Q3(Span), MaxBucket, MaxDepth - 1);
                    SubTrees[3] = new QuadTree<T>(Q4(Span), MaxBucket, MaxDepth - 1);

                    var remNodes = new List<QuadTreeElement<T>>();
                    //nodes that are not fully contained by any quadrant

                    foreach (var n in Nodes)
                    {
                        switch (Partition(Span, n.Span))
                        {
                            case 1: //quadrant 1
                                SubTrees[0].AddNode(n);
                                break;
                            case 2:
                                SubTrees[1].AddNode(n);
                                break;
                            case 3:
                                SubTrees[2].AddNode(n);
                                break;
                            case 4:
                                SubTrees[3].AddNode(n);
                                break;
                            default:
                                n.Parent = this;
                                remNodes.Add(n);
                                break;
                        }
                    }

                    Nodes = remNodes;
                }
                else
                {
                    node.Parent = this;
                    Nodes.Add(node);
                    //if bin is not yet full or max depth has been reached, just add the node without subdividing
                }
            }
            else //we already have children nodes
            {
                //
                //add node to specific sub-tree
                //
                switch (Partition(Span, node.Span))
                {
                    case 1: //quadrant 1
                        SubTrees[0].AddNode(node);
                        break;
                    case 2:
                        SubTrees[1].AddNode(node);
                        break;
                    case 3:
                        SubTrees[2].AddNode(node);
                        break;
                    case 4:
                        SubTrees[3].AddNode(node);
                        break;
                    default:
                        node.Parent = this;
                        Nodes.Add(node);
                        break;
                }
            }
        }
        
        public bool OverlapsElement(in AABB searchR)
        {
            var stack = new Stack<QuadTree<T>>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var qt = stack.Pop();

                if (!TestOverlap(in searchR, in qt.Span))
                    continue;

                foreach (var n in qt.Nodes)
                {
                    if (TestOverlap(in searchR, in n.Span))
                        return true;
                }
                
                if (!qt.IsPartitioned)
                    continue;

                foreach (var st in qt.SubTrees)
                {
                    stack.Push(st);
                }
            }

            return false;
        }

        public void QueryAabb([NotNull, InstantHandle] Func<QuadTreeElement<T>, bool> callback, in AABB searchR)
        {
            var stack = new Stack<QuadTree<T>>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var qt = stack.Pop();

                if (!TestOverlap(in searchR, in qt.Span))
                    continue;

                foreach (var n in qt.Nodes)
                {
                    if (!TestOverlap(in searchR, in n.Span)) 
                        continue;

                    if (!callback(n))
                        return;
                }

                if (!qt.IsPartitioned) 
                    continue;

                foreach (var st in qt.SubTrees)
                {
                    stack.Push(st);
                }
            }
        }

        public bool QueryAabbAny([NotNull, InstantHandle] Func<QuadTreeElement<T>, bool> callback, in AABB searchR)
        {
            var stack = new Stack<QuadTree<T>>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var qt = stack.Pop();

                if (!TestOverlap(in searchR, in qt.Span))
                    continue;

                foreach (var n in qt.Nodes)
                {
                    if (!TestOverlap(in searchR, in n.Span)) 
                        continue;

                    if (callback(n))
                        return true;
                }

                if (!qt.IsPartitioned) 
                    continue;

                foreach (var st in qt.SubTrees)
                {
                    stack.Push(st);
                }
            }

            return false;
        }

        public void GetAllNodesR(ref List<QuadTreeElement<T>> nodes)
        {
            nodes.AddRange(Nodes);

            if (!IsPartitioned) return;

            foreach (var st in SubTrees)
            {
                st.GetAllNodesR(ref nodes);
            }
        }

        public void RemoveNode([NotNull] QuadTreeElement<T> node)
        {
            node.Parent.Nodes.Remove(node);
        }

        public void Reconstruct()
        {
            var allNodes = new List<QuadTreeElement<T>>();
            GetAllNodesR(ref allNodes);

            Clear();

            allNodes.ForEach(AddNode);
        }

        public void Clear()
        {
            Nodes.Clear();
            SubTrees = null;
        }

        private static bool TestOverlap(in AABB a, in AABB b)
        {
            float d1X = b.Minimum.X - a.Maximum.X;
            float d1Y = b.Minimum.Y - a.Maximum.Y;

            float d2X = a.Minimum.X - b.Maximum.X;
            float d2Y = a.Minimum.Y - b.Maximum.Y;
            
            if (d1X > 0.0f || d1Y > 0.0f)
                return false;

            if (d2X > 0.0f || d2Y > 0.0f)
                return false;

            return true;
        }

        private static AABB Q1(in AABB aabb)
        {
            return new AABB(aabb.Center, aabb.Maximum);
        }

        private static AABB Q2(in AABB aabb)
        {
            return new AABB(new Vector(aabb.Minimum.X, aabb.Center.Y), new Vector(aabb.Center.X, aabb.Maximum.Y));
        }

        private static AABB Q3(in AABB aabb)
        {
            return new AABB(aabb.Minimum, aabb.Center);
        }

        private static AABB Q4(in AABB aabb)
        {
            return new AABB(new Vector(aabb.Center.X, aabb.Minimum.Y), new Vector(aabb.Maximum.X, aabb.Center.Y));
        }
    }

    public class QuadTreeElement<T>
    {
        public QuadTree<T> Parent;
        public AABB Span;
        public T Value;

        public QuadTreeElement(T value, AABB span)
        {
            Span = span;
            Value = value;
            Parent = null;
        }
    }
}

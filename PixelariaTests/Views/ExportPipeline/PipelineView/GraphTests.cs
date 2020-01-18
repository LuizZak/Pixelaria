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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Views.ExportPipeline.PipelineView;

namespace PixelariaTests.Views.ExportPipeline.PipelineView
{
    [TestClass]
    public class GraphTests
    {
        [TestMethod]
        public void TestCreateSimpleGraph()
        {
            // Creates a graph that looks kinda line this:
            //
            // [start] ---- (1) --- (3) ----+---> [end] (not a real node yet)
            //      \                      /
            //       \----- (2) ----------/
            //

            var graph = new DirectedAcyclicNode<string>(null);
            
            graph.CreateChild("1").CreateChild("3");
            graph.CreateChild("2");
            
            // Now test node precedence

            Assert.AreEqual(2, graph.Next.Count);
            Assert.AreEqual("1", graph.Next[0].Value);
            Assert.AreEqual("2", graph.Next[1].Value);

            Assert.AreEqual("3", graph.Next[0].Next[0].Value);

            var child = graph.FindChild("3");
            Assert.IsTrue(child != null && child.IsDescendentOf(graph.FindChild("1")));
            Assert.IsTrue(child != null && child.IsDescendentOf(graph));
            Assert.IsTrue(child != null && !child.IsDescendentOf(graph.FindChild("2")));
        }

        [TestMethod]
        public void TestCalculateLayers()
        {
            // Creates a graph that looks kinda line this:
            //
            // [start] ---- (1) --- (3) ----+---> [end] (materialized by method)
            //      \                      /
            //       \----- (2) ----------/
            //

            var graph = new DirectedAcyclicNode<string>(null);

            graph.CreateChild("1").CreateChild("3");
            graph.CreateChild("2");

            var root = LayeredDirectedAcyclicNode.FromNode(graph);

            // Now test node precedence

            Assert.AreEqual(2, root.Next.Count);
            Assert.AreEqual("1", root.Next[0].Tag);
            Assert.AreEqual("2", root.Next[1].Tag);

            Assert.AreEqual("3", root.Next[0].Next[0].Tag);

            // [start]
            Assert.AreEqual(0, root.Layer);

            // (1)
            Assert.AreEqual(1, root.Next[0].Layer);
            // (2)
            Assert.AreEqual(1, root.Next[1].Layer);

            // (3)
            Assert.AreEqual(2, root.Next[0].Next[0].Layer);

            // [end]
            Assert.AreEqual(3, root.Next[0].Next[0].Next[0].Layer);
            Assert.AreEqual(3, root.Next[1].Next[0].Layer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDetectCycle()
        {
            var graph = new DirectedAcyclicNode<string>(null);

            graph.CreateChild("1").CreateChild("3");
            graph.FindChild("3")?.AddChild(graph);
        }

        [TestMethod]
        public void TestTopologicalSorting()
        {
            // Create a graph that looks like this:
            //
            //   A   B
            //    \ / \
            //     C   \
            //    /     \
            //   D       E
            //     \   /
            //       F
            //       |
            //       G
            //

            var root = new DirectedAcyclicNode<string>(null);
            
            var a = new DirectedAcyclicNode<string>("A");
            var b = new DirectedAcyclicNode<string>("B");
            var c = new DirectedAcyclicNode<string>("C");
            var d = new DirectedAcyclicNode<string>("D");
            var e = new DirectedAcyclicNode<string>("E");
            var f = new DirectedAcyclicNode<string>("F");
            var g = new DirectedAcyclicNode<string>("G");

            a.AddChild(c);
            b.AddChild(e);
            b.AddChild(c);
            c.AddChild(d);
            e.AddChild(f);
            d.AddChild(f);
            f.AddChild(g);

            root.AddChild(a);
            root.AddChild(b);

            var sorted = root.TopologicalSorted();

            Console.WriteLine(string.Join(", ", sorted.Select(n => n.Value?.ToString() ?? "<null>")));

            // Now we check that the topological sorting follow the requirement:
            //
            //  - Given vertices u and v, where u -> v, when sorted u must always show before v.
            for (int i = 0; i < sorted.Length; i++)
            {
                for (int j = i + 1; j < sorted.Length; j++)
                {
                    var first = sorted[i];
                    var second = sorted[j];

                    Assert.IsFalse(first.IsDescendentOf(second));
                }
            }
        }

        [TestMethod]
        public void TestLongestSubpath()
        {
            // Create a graph that looks like this:
            //
            //   A   B
            //    \ / \
            //     C   \
            //    /     \
            //   D       E
            //     \   /
            //       F
            //       |
            //       G
            //
            // The longest path should be: 
            // root - A - C - D - F - G
            // Or, equally:
            // root - B - C - D - F - G

            var root = new DirectedAcyclicNode<string>(null);

            var a = new DirectedAcyclicNode<string>("A");
            var b = new DirectedAcyclicNode<string>("B");
            var c = new DirectedAcyclicNode<string>("C");
            var d = new DirectedAcyclicNode<string>("D");
            var e = new DirectedAcyclicNode<string>("E");
            var f = new DirectedAcyclicNode<string>("F");
            var g = new DirectedAcyclicNode<string>("G");

            a.AddChild(c);
            b.AddChild(e);
            b.AddChild(c);
            c.AddChild(d);
            e.AddChild(f);
            d.AddChild(f);
            f.AddChild(g);

            root.AddChild(a);
            root.AddChild(b);

            var path = root.LongestPath();

            Console.WriteLine(string.Join(", ", path.Select(n => n.Value?.ToString() ?? "<null>")));

            Assert.AreEqual(path[0], root);
            Assert.IsTrue(path[1] == a || path[1] == b);
            Assert.AreEqual(path[2], c);
            Assert.AreEqual(path[3], d);
            Assert.AreEqual(path[4], f);
            Assert.AreEqual(path[5], g);
        }

        /*
        [TestMethod]
        public void TestCreateEntangledGraph()
        {
            // Creates a graph that looks kinda line this:
            //
            // [start] ---- (1) -\ /- (3) ----+---> [end] (materialized by method)
            //      \             X          /
            //       \----- (2) -/ \- (4) --/
            //

            var graph = new DirectedAcyclicNode(null);

            graph.CreateNode("1");
            graph.CreateNode("2");
            graph.CreateNode("3", "2");
            graph.CreateNode("4", "1");

            var root = graph.CreateLayeredTree();

            // Now test node precedence

            Assert.AreEqual(2, root.EdgesTo.Count);
            Assert.AreEqual("1", root.EdgesTo[0].Tag);
            Assert.AreEqual("2", root.EdgesTo[1].Tag);

            Assert.AreEqual("4", root.EdgesTo[0].EdgesTo[0].Tag);
            Assert.AreEqual("3", root.EdgesTo[1].EdgesTo[0].Tag);

            // [start]
            Assert.AreEqual(0, root.Layer);

            // (1)
            Assert.AreEqual(1, root.EdgesTo[0].Layer);
            // (2)
            Assert.AreEqual(1, root.EdgesTo[1].Layer);

            // (3)
            Assert.AreEqual(2, root.EdgesTo[0].EdgesTo[0].Layer);
            // (4)
            Assert.AreEqual(2, root.EdgesTo[1].EdgesTo[0].Layer);

            // [end]
            Assert.AreEqual(3, root.EdgesTo[0].EdgesTo[0].EdgesTo[0].Layer);
            Assert.AreEqual(3, root.EdgesTo[1].EdgesTo[0].EdgesTo[0].Layer);
        }

        [TestMethod]
        public void TestGraphLayout()
        {
            // Creates a graph that looks kinda line this:
            //
            // [start] ---- (1) -\ /- (3) ----+---> [end] (materialized by method)
            //      \             X          /
            //       \----- (2) -/ \- (4) --/
            //
            // After sorting the graph layout, it should look like this:
            // 
            // [start] ---- (1) ----- (4) ----+---> [end] (materialized by method)
            //      \                        /
            //       \----- (2) ----- (3) --/
            //

            var graph = new Graph();

            graph.CreateNode("1");
            graph.CreateNode("2");
            graph.CreateNode("3", "2");
            graph.CreateNode("4", "1");

            var root = graph.CreateLayeredTree();
            GraphLayout.LayoutNodes(root);

            // Now test node precedence

            Assert.AreEqual(2, root.EdgesTo.Count);
            Assert.AreEqual("1", root.EdgesTo[0].Tag);
            Assert.AreEqual("2", root.EdgesTo[1].Tag);

            Assert.AreEqual("4", root.EdgesTo[0].EdgesTo[0].Tag);
            Assert.AreEqual("3", root.EdgesTo[1].EdgesTo[0].Tag);

            // [start]
            Assert.AreEqual(0, root.Layer);

            // (1)
            Assert.AreEqual(1, root.EdgesTo[0].Layer);
            // (2)
            Assert.AreEqual(1, root.EdgesTo[1].Layer);

            // (3)
            Assert.AreEqual(2, root.EdgesTo[0].EdgesTo[0].Layer);
            // (4)
            Assert.AreEqual(2, root.EdgesTo[1].EdgesTo[0].Layer);

            // [end]
            Assert.AreEqual(3, root.EdgesTo[0].EdgesTo[0].EdgesTo[0].Layer);
            Assert.AreEqual(3, root.EdgesTo[1].EdgesTo[0].EdgesTo[0].Layer);
        }
        */
    }
}

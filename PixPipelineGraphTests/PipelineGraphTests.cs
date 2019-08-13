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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixPipelineGraph;

namespace PixPipelineGraphTests
{
    [TestClass]
    public class PipelineGraphTests
    {
        [TestMethod]
        public void TestEphemeral()
        {
            var sut = CreatePipelineGraph();

            Assert.AreEqual(sut.PipelineNodes.Count, 0);
        }

        [TestMethod]
        public void TestCreateNode()
        {
            var sut = CreatePipelineGraph();

            sut.CreateNode();

            Assert.AreEqual(sut.PipelineNodes.Count, 1);
        }

        [TestMethod]
        public void TestConnect()
        {
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateNode(builder =>
            {
                builder.CreateOutput("output");
            });
            var node2 = sut.CreateNode(builder =>
            {
                builder.CreateInput("input");
            });

            sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node2)[0]);

            Assert.IsTrue(sut.AreConnected(node1, node2));
            Assert.AreEqual(sut.PipelineConnections.Count, 1);
        }

        [TestMethod]
        public void TestAreConnected()
        {
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateNode(builder => { builder.CreateOutput("output"); });
            var node2 = sut.CreateNode(builder => { builder.CreateInput("input"); });
            sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node2)[0]);

            Assert.IsTrue(sut.AreConnected(node1, node2));
        }

        [TestMethod]
        public void TestAreConnectedByLinks()
        {
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateNode(builder => { builder.CreateOutput("output"); });
            var node2 = sut.CreateNode(builder => { builder.CreateInput("input"); });
            sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node2)[0]);

            Assert.IsTrue(sut.AreConnected(sut.OutputsForNode(node1)[0], sut.InputsForNode(node2)[0]));
        }

        [TestMethod]
        public void TestAreConnectedFalse()
        {
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateNode();
            var node2 = sut.CreateNode();

            Assert.IsFalse(sut.AreConnected(node1, node2));
        }

        [TestMethod]
        public void TestContainsNode()
        {
            var sut = CreatePipelineGraph();
            var node = sut.CreateNode();

            Assert.IsTrue(sut.ContainsNode(node));
        }

        [TestMethod]
        public void TestContainsNodeFalse()
        {
            var sut = CreatePipelineGraph();
            var nonexistentNode = new PipelineNodeId(Guid.Empty);

            Assert.IsFalse(sut.ContainsNode(nonexistentNode));
        }

        [TestMethod]
        public void TestRemoveNode()
        {
            var sut = CreatePipelineGraph();
            var node = sut.CreateNode();

            sut.RemoveNode(node);

            Assert.AreEqual(sut.PipelineNodes.Count, 0);
        }

        [TestMethod]
        public void TestRemoveNodeRemovesRelatedConnections()
        {
            var sut = CreatePipelineGraph();
            var node = sut.CreateNode(n =>
            {
                n.CreateOutput("output");
                n.CreateInput("input");
            });
            var inNode = sut.CreateNode(n => n.CreateOutput("output"));
            var outNode = sut.CreateNode(n => n.CreateInput("input"));
            sut.Connect(sut.OutputsForNode(inNode)[0], sut.InputsForNode(node)[0]);
            sut.Connect(sut.OutputsForNode(node)[0], sut.InputsForNode(outNode)[0]);

            sut.RemoveNode(node);

            Assert.AreEqual(sut.PipelineConnections.Count, 0);
        }

        [TestMethod]
        public void TestAddFromGraph()
        {
            var graph1 = CreatePipelineGraph();
            graph1.CreateNode(n => n.CreateOutput("output"));
            graph1.CreateNode(n => n.CreateInput("input"));
            graph1.Connect(graph1.OutputsForNode(graph1.PipelineNodes[0])[0], graph1.InputsForNode(graph1.PipelineNodes[1])[0]);
            var sut = CreatePipelineGraph();
            sut.CreateNode(n => n.CreateOutput("output"));
            sut.CreateNode(n => n.CreateInput("input"));
            sut.Connect(sut.OutputsForNode(sut.PipelineNodes[0])[0], sut.InputsForNode(sut.PipelineNodes[1])[0]);

            sut.AddFromGraph(graph1);

            Assert.AreEqual(sut.PipelineNodes.Count, 4);
            Assert.AreEqual(sut.PipelineConnections.Count, 2);
        }

        [TestMethod]
        public void TestAddFromGraphCreatesNewNodeIds()
        {
            var graph1 = CreatePipelineGraph();
            graph1.CreateNode(n => n.CreateOutput("output"));
            graph1.CreateNode(n => n.CreateInput("input"));
            graph1.Connect(graph1.OutputsForNode(graph1.PipelineNodes[0])[0], graph1.InputsForNode(graph1.PipelineNodes[1])[0]);
            var sut = CreatePipelineGraph();

            sut.AddFromGraph(graph1);

            Assert.IsFalse(graph1.ContainsNode(sut.PipelineNodes[0]));
            Assert.IsFalse(graph1.ContainsNode(sut.PipelineNodes[1]));
            Assert.IsFalse(sut.ContainsNode(graph1.PipelineNodes[0]));
            Assert.IsFalse(sut.ContainsNode(graph1.PipelineNodes[1]));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestOutputsForNodeRaisesExceptionOnInvalidNodeId()
        {
            var graph1 = CreatePipelineGraph();
            graph1.CreateNode(n => n.CreateOutput("output"));
            var graph2 = CreatePipelineGraph();

            graph2.OutputsForNode(graph1.PipelineNodes[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInputsForNodeRaisesExceptionOnInvalidNodeId()
        {
            var graph1 = CreatePipelineGraph();
            graph1.CreateNode(n => n.CreateInput("input"));
            var graph2 = CreatePipelineGraph();

            graph2.InputsForNode(graph1.PipelineNodes[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConnectRaisesExceptionOnInvalidNodeId()
        {
            var graph1 = CreatePipelineGraph();
            graph1.CreateNode(n => n.CreateOutput("output"));
            graph1.CreateNode(n => n.CreateInput("input"));
            var graph2 = CreatePipelineGraph();
            graph2.CreateNode(n => n.CreateOutput("output"));
            graph2.CreateNode(n => n.CreateInput("input"));

            graph2.Connect(graph1.OutputsForNode(graph1.PipelineNodes[0])[0], graph1.InputsForNode(graph1.PipelineNodes[1])[0]);
        }

        #region Instantiation

        private static PipelineGraph CreatePipelineGraph()
        {
            return new PipelineGraph(new MockPipelineBodyProvider());
        }

        #endregion
    }
}

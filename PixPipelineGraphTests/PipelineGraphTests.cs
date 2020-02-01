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
            Assert.AreEqual(sut.PipelineConnections.Count, 0);
        }

        [TestMethod]
        public void TestCreateNode()
        {
            var sut = CreatePipelineGraph();

            sut.CreateNode();

            Assert.AreEqual(sut.PipelineNodes.Count, 1);
        }

        [TestMethod]
        public void TestCreateNodeFromKind()
        {
            var mockProvider = new MockPipelineNodeProvider();
            var sut = CreatePipelineGraph(mockProvider);
            var nodeKind1 = new PipelineNodeKind("node1");
            var nodeKind2 = new PipelineNodeKind("node2");
            mockProvider.NodeCreators[nodeKind1] = builder =>
            {
                builder.SetTitle("node1");
            };
            mockProvider.NodeCreators[nodeKind2] = builder =>
            {
                builder.SetTitle("node2");
            };

            var node1 = sut.CreateNode(nodeKind1);
            var node2 = sut.CreateNode(nodeKind2);

            Assert.AreEqual(sut.TitleForNode(node1.Value), "node1");
            Assert.AreEqual(sut.TitleForNode(node2.Value), "node2");
        }

        [TestMethod]
        public void TestCreateNodeFromKindReturnsNullOnUnknownNodeKind()
        {
            var sut = CreatePipelineGraph();
            var nodeKind = new PipelineNodeKind("node");

            var node = sut.CreateNode(nodeKind);

            Assert.IsNull(node);
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
        public void TestConnectNodeToInput()
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

            sut.Connect(node1, sut.InputsForNode(node2)[0]);

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
        public void TestAreConnectedDetectsIndirectConnection()
        {
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateNode(builder => { builder.CreateOutput("output"); });
            var node2 = sut.CreateNode(builder => { builder.CreateInput("input"); builder.CreateOutput("output"); });
            var node3 = sut.CreateNode(builder => { builder.CreateInput("input"); });
            sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node2)[0]);
            sut.Connect(sut.OutputsForNode(node2)[0], sut.InputsForNode(node3)[0]);

            Assert.IsTrue(sut.AreConnected(node1, node3));
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
        public void TestAreDirectlyConnected()
        {
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateNode(builder => { builder.CreateOutput("output"); });
            var node2 = sut.CreateNode(builder => { builder.CreateInput("input"); builder.CreateOutput("output"); });
            var node3 = sut.CreateNode(builder => { builder.CreateInput("input"); });
            sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node2)[0]);
            sut.Connect(sut.OutputsForNode(node2)[0], sut.InputsForNode(node3)[0]);

            Assert.IsFalse(sut.AreDirectlyConnected(node1, node3));
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

            bool didRemove = sut.RemoveNode(node);

            Assert.IsTrue(didRemove);
            Assert.AreEqual(sut.PipelineNodes.Count, 0);
        }

        [TestMethod]
        public void TestRemoveNodeReturnsFalseOnUnknownNodeId()
        {
            var sut = CreatePipelineGraph();

            Assert.IsFalse(sut.RemoveNode(new PipelineNodeId(Guid.Empty)));
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
        public void TestTitleForNode()
        {
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateNode(builder =>
            {
                builder.SetTitle("Title 1");
            });
            var node2 = sut.CreateNode(builder =>
            {
                builder.SetTitle("Title 2");
            });

            Assert.AreEqual("Title 1", sut.TitleForNode(node1));
            Assert.AreEqual("Title 2", sut.TitleForNode(node2));
            Assert.AreEqual(null, sut.TitleForNode(new PipelineNodeId(Guid.NewGuid())));
        }

        [TestMethod]
        public void TestRecordingChanges()
        {
            var sut = CreatePipelineGraph();
            PipelineNodeId node1;
            var node2 = new PipelineNodeId();
            PipelineNodeId node3;
            IPipelineConnection connection1 = null;
            node1 = sut.CreateNode(builder =>
            {
                builder.CreateOutput("output");
            });
            node3 = sut.CreateNode(builder =>
            {
                builder.CreateInput("input");
            });
            var connection2 = sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node3)[0]);

            var result = sut.RecordingChanges(() =>
            {
                node2 = sut.CreateNode(builder =>
                {
                    builder.CreateInput("input");
                }); 

                connection1 = sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node2)[0]);
                sut.RemoveNode(node3);
            });

            Assert.IsTrue(result.NodesCreated.Contains(node2));
            Assert.IsTrue(result.NodesRemoved.Contains(node3));
            Assert.IsTrue(result.ConnectionsCreated.Contains(connection1));
            Assert.IsTrue(result.ConnectionsRemoved.Contains(connection2));
        }

        [TestMethod]
        public void TestRecordingChangesFlattensRedundantEvents()
        {
            // We should not see the same node/connection being featured in both
            // Created/Removed events at the same time.
            var sut = CreatePipelineGraph();

            var node3 = new PipelineNodeId();
            IPipelineConnection connection2 = null;

            var result = sut.RecordingChanges(() =>
            {
                var node1 = sut.CreateNode(builder =>
                {
                    builder.CreateOutput("output");
                });
                node3 = sut.CreateNode(builder =>
                {
                    builder.CreateInput("input");
                });

                connection2 = sut.Connect(sut.OutputsForNode(node1)[0], sut.InputsForNode(node3)[0]);
                sut.RemoveNode(node3);
            });

            Assert.IsFalse(result.NodesCreated.Contains(node3));
            Assert.IsFalse(result.NodesRemoved.Contains(node3));
            Assert.IsFalse(result.ConnectionsCreated.Contains(connection2));
            Assert.IsFalse(result.ConnectionsRemoved.Contains(connection2));
        }

        #region Events

        [TestMethod]
        public void TestNodesWhereAdded()
        {
            PipelineNodeEventArgs nodeArgs = null;
            var sut = CreatePipelineGraph();
            sut.NodesWhereAdded += (sender, args) => nodeArgs = args;
            
            var nodeId = sut.CreateNode();

            Assert.IsTrue(nodeArgs.NodeIds.Contains(nodeId));
        }

        [TestMethod]
        public void TestNodesWillBeRemoved()
        {
            PipelineNodeEventArgs nodeArgs = null;
            var sut = CreatePipelineGraph();
            var nodeId = sut.CreateNode();
            sut.NodesWillBeRemoved += (sender, args) => nodeArgs = args;

            sut.RemoveNode(nodeId);

            Assert.IsTrue(nodeArgs.NodeIds.Contains(nodeId));
        }

        [TestMethod]
        public void TestNodesWillBeRemovedIsInvokedBeforeConnectionWillBeRemoved()
        {
            var args = new List<EventArgs>();
            var sut = CreatePipelineGraph();
            var node1 = sut.CreateFromGenerator("signal", () => 1);
            var node2 = sut.CreateFromLambda("forwarder", (int input) => input);
            sut.Connect(node1, node2);
            sut.NodesWillBeRemoved += (_, eventArgs) => args.Add(eventArgs);
            sut.ConnectionWillBeRemoved += (_, eventArgs) => args.Add(eventArgs);

            sut.RemoveNode(node1);

            Assert.IsTrue(args.FindIndex(eventArgs => eventArgs is PipelineNodeEventArgs) < args.FindIndex(eventArgs => eventArgs is ConnectionEventArgs));
        }

        #endregion

        #region Exception Tests

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

        #endregion

        #region Instantiation

        private static PipelineGraph CreatePipelineGraph(IPipelineGraphNodeProvider nodeProvider = null)
        {
            return new PipelineGraph(nodeProvider ?? new MockPipelineNodeProvider());
        }

        #endregion
    }
}

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
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Inputs.Abstract;
using Pixelaria.ExportPipeline.Outputs.Abstract;

namespace PixelariaTests.Tests.ExportPipeline
{
    [TestClass]
    public class PipelineContainerTests
    {
        [TestMethod]
        public void TestAddNodeGeneric()
        {
            var sut = new PipelineContainer();

            sut.AddNode<TestNode>();

            Assert.AreEqual(sut.Nodes.Count, 1);
            Assert.IsTrue(sut.Nodes[0] is TestNode);
        }

        [TestMethod]
        public void TestAddNode()
        {
            var sut = new PipelineContainer();
            var node = new TestNode();

            sut.AddNode(node);

            Assert.AreEqual(sut.Nodes.Count, 1);
            Assert.AreEqual(sut.Nodes[0], node);
        }

        [TestMethod]
        public void TestRemoveNode()
        {
            var sut = new PipelineContainer();
            var node = new TestNode();

            sut.AddNode(node);
            sut.RemoveNode(node);

            Assert.AreEqual(sut.Nodes.Count, 0);
        }

        [TestMethod]
        public void TestAddConnection()
        {
            var sut = new PipelineContainer();

            var node1 = new TestNode();
            var node2 = new TestNode();

            sut.AddNode(node1);
            sut.AddNode(node2);

            Assert.IsFalse(sut.AreConnected(node2.Input[0], node1.Output[0]));

            sut.AddConnection(node2.Input[0], node1.Output[0]);

            Assert.IsTrue(sut.AreConnected(node2.Input[0], node1.Output[0]));
        }

        [TestMethod]
        public void TestAddConnectionDisconnect()
        {
            var sut = new PipelineContainer();

            var node1 = new TestNode();
            var node2 = new TestNode();

            sut.AddNode(node1);
            sut.AddNode(node2);

            Assert.IsFalse(sut.AreConnected(node2.Input[0], node1.Output[0]));

            var con = sut.AddConnection(node2.Input[0], node1.Output[0]);

            Assert.IsNotNull(con);
            Assert.AreEqual(con.Input, node2.Input[0]);
            Assert.AreEqual(con.Output, node1.Output[0]);
            Assert.IsTrue(con.Connected);
            
            con.Disconnect();

            Assert.IsFalse(con.Connected);
            Assert.IsFalse(sut.AreConnected(node2.Input[0], node1.Output[0]));

            // Check sequential calls don't trigger unwanted disconnections
            sut.AddConnection(node2.Input[0], node1.Output[0]);

            con.Disconnect();

            Assert.IsTrue(sut.AreConnected(node2.Input[0], node1.Output[0]));
        }

        [TestMethod]
        public void TestAddConnectionCallsConnectionDelegate()
        {
            var sut = new PipelineContainer();
            var del = new ConnectionDelegate();

            sut.ConnectionDelegate = del;

            var node1 = new TestNode();
            var node2 = new TestNode();

            sut.AddNode(node1);
            sut.AddNode(node2);

            sut.AddConnection(node2.Input[0], node1.Output[0]);
            
            Assert.IsTrue(del.Called);
            Assert.AreEqual(del.Input, node2.Input[0]);
            Assert.AreEqual(del.Output, node1.Output[0]);
            Assert.AreEqual(del.Container, sut);
        }

        private class ConnectionDelegate : IPipelineConnectionDelegate
        {
            public bool Called;
            public IPipelineInput Input;
            public IPipelineOutput Output;
            public IPipelineContainer Container;

            public bool CanConnect(IPipelineInput input, IPipelineOutput output, IPipelineContainer container)
            {
                Input = input;
                Output = output;
                Container = container;

                Called = true;
                return true;
            }
        }

        private class TestNode : IPipelineStep
        {
            public Guid Id { get; } = Guid.NewGuid();
            public string Name { get; } = "";
            
            public IReadOnlyList<IPipelineInput> Input { get; }
            public IReadOnlyList<IPipelineOutput> Output { get; }

            public TestNode()
            {
                Input = new IPipelineInput[] { new GenericPipelineInput<int>(this, "") };
                Output = new IPipelineOutput[] { new GenericPipelineOutput<int>(this, Observable.Empty<int>(), "") };
            }

            public IPipelineMetadata GetMetadata()
            {
                throw new NotImplementedException();
            }
        }
    }
}

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
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Inputs.Abstract;
using Pixelaria.ExportPipeline.Outputs.Abstract;

namespace PixelariaTests.Tests.ExportPipeline
{
    [TestClass]
    public class DefaultPipelineConnectionDelegateTests
    {
        [TestMethod]
        public void TestCanConnect()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var container = new TestContainer();

            var node1 = new TestNode();
            var node2 = new TestNode();

            var input = new GenericPipelineInput<int>(node1, "");
            var output = new GenericPipelineOutput<int>(node2, Observable.Empty<int>(), "");

            Assert.IsTrue(sut.CanConnect(input, output, container));
        }

        [TestMethod]
        public void TestCanConnectReturnsFalseForLinksOfSameNode()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var container = new TestContainer();

            var node1 = new TestNode();

            var input = new GenericPipelineInput<int>(node1, "");
            var output = new GenericPipelineOutput<int>(node1, Observable.Empty<int>(), "");

            Assert.IsFalse(sut.CanConnect(input, output, container));
        }

        [TestMethod]
        public void TestCanConnectReturnsFalseForNodesAlreadyConnected()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var container = new TestContainer();

            var node1 = new TestNode();
            var node2 = new TestNode();

            var input = new GenericPipelineInput<int>(node1, "");
            var output = new GenericPipelineOutput<int>(node2, Observable.Empty<int>(), "");

            container.ConnectionsList.Add(new TestConnection(input, output, true));

            Assert.IsFalse(sut.CanConnect(input, output, container));
        }

        [TestMethod]
        public void TestCanConnectReturnsFalseForNodesIndirectlyConnectedInACycle()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var container = new TestContainer();

            var node1 = new TestNode();
            var node2 = new TestNode();
            var node3 = new TestNode();

            node1.AddInput(new GenericPipelineInput<int>(node1, ""));
            node1.AddOutput(new GenericPipelineOutput<int>(node1, Observable.Empty<int>(), ""));

            node2.AddInput(new GenericPipelineInput<int>(node2, ""));
            node2.AddOutput(new GenericPipelineOutput<int>(node2, Observable.Empty<int>(), ""));

            node3.AddInput(new GenericPipelineInput<int>(node3, ""));

            // Make dummy connections
            container.ConnectionsList.Add(new TestConnection(node1.Input[0], node2.Output[0], true));
            container.ConnectionsList.Add(new TestConnection(node2.Input[0], node3.Output[0], true));
            
            Assert.IsFalse(sut.CanConnect(node3.Input[0], node1.Output[0], container));
        }

        private class TestContainer : IPipelineContainer
        {
            public readonly List<IPipelineNode> NodesList = new List<IPipelineNode>();
            public readonly List<IPipelineLinkConnection> ConnectionsList = new List<IPipelineLinkConnection>();

            public IReadOnlyList<IPipelineNode> Nodes => NodesList;
            public IReadOnlyList<IPipelineLinkConnection> Connections => ConnectionsList;

            public bool AreConnected(IPipelineInput input, IPipelineOutput output)
            {
                return ConnectionsList.Any(c => c.Input == input && c.Output == output);
            }

            public IEnumerable<IPipelineLinkConnection> ConnectionsFor(IPipelineNodeLink link)
            {
                return ConnectionsList.Where(con => con.Input == link || con.Output == link);
            }
        }

        private class TestNode : IPipelineStep
        {
            public Guid Id { get; } = Guid.NewGuid();
            public string Name { get; } = "";

            private readonly List<IPipelineInput> _inputList;
            private readonly List<IPipelineOutput> _outputList;

            public IReadOnlyList<IPipelineInput> Input => _inputList;
            public IReadOnlyList<IPipelineOutput> Output => _outputList;

            public TestNode()
            {
                _inputList = new List<IPipelineInput>
                {
                    new GenericPipelineInput<int>(this, "")
                };
                _outputList = new List<IPipelineOutput>
                {
                    new GenericPipelineOutput<int>(this, Observable.Empty<int>(), "")
                };
            }

            public void AddInput(IPipelineInput input)
            {
                _inputList.Add(input);
            }

            public void AddOutput(IPipelineOutput output)
            {
                _outputList.Add(output);
            }

            public IPipelineMetadata GetMetadata()
            {
                throw new NotImplementedException();
            }
        }

        private class TestConnection : IPipelineLinkConnection
        {
            public IPipelineInput Input { get; }
            public IPipelineOutput Output { get; }
            public bool Connected { get; }

            public TestConnection(IPipelineInput input, IPipelineOutput output, bool connected)
            {
                Input = input;
                Output = output;
                Connected = connected;
            }

            public void Disconnect()
            {
                throw new NotImplementedException();
            }

            public IPipelineMetadata GetMetadata()
            {
                throw new NotImplementedException();
            }
        }
    }
}

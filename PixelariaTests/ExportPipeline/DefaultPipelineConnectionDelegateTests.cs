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

using System.Reactive.Subjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.ExportPipeline;
using PixPipelineGraph;

namespace PixelariaTests.ExportPipeline
{
    [TestClass]
    public class DefaultPipelineConnectionDelegateTests
    {
        [TestMethod]
        public void TestCanConnect()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var graph = new PipelineGraph(new MockPipelineNodeProvider());

            IPipelineLazyValue<PipelineInput> lazyInput = null;
            IPipelineLazyValue<PipelineOutput> lazyOutput = null;
            var node1 = graph.CreateNode(n =>
            {
                lazyInput = n.CreateInput("", builder => builder.SetInputType(typeof(int)));
            });
            var node2 = graph.CreateNode(n =>
            {
                lazyOutput = n.CreateOutput("", builder => builder.SetOutputType(typeof(int)));
            });

            var input = graph.GetInput(lazyInput.LazyValue);
            var output = graph.GetOutput(lazyOutput.LazyValue);

            Assert.IsTrue(sut.CanConnect(input, output, graph));
        }

        [TestMethod]
        public void TestCanConnectReturnsFalseForLinksOfSameNode()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var graph = new PipelineGraph(new MockPipelineNodeProvider());

            IPipelineLazyValue<PipelineInput> lazyInput = null;
            IPipelineLazyValue<PipelineOutput> lazyOutput = null;
            var node1 = graph.CreateNode(n =>
            {
                lazyInput = n.CreateInput("", builder => builder.SetInputType(typeof(int)));
                lazyOutput = n.CreateOutput("", builder => builder.SetOutputType(typeof(int)));
            });

            var input = graph.GetInput(lazyInput.LazyValue);
            var output = graph.GetOutput(lazyOutput.LazyValue);

            Assert.IsFalse(sut.CanConnect(input, output, graph));
        }

        [TestMethod]
        public void TestCanConnectReturnsFalseForNodesAlreadyConnected()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var graph = new PipelineGraph(new MockPipelineNodeProvider());

            IPipelineLazyValue<PipelineInput> lazyInput = null;
            IPipelineLazyValue<PipelineOutput> lazyOutput = null;
            var node1 = graph.CreateNode(n =>
            {
                lazyOutput = n.CreateOutput("", builder => builder.SetOutputType(typeof(int)));
            });
            var node2 = graph.CreateNode(n =>
            {
                lazyInput = n.CreateInput("", builder => builder.SetInputType(typeof(int)));
            });

            graph.Connect(node1, node2);

            var input = graph.GetInput(lazyInput.LazyValue);
            var output = graph.GetOutput(lazyOutput.LazyValue);

            Assert.IsFalse(sut.CanConnect(input, output, graph));
        }

        [TestMethod]
        public void TestCanConnectReturnsFalseForNodesIndirectlyConnectedInACycle()
        {
            var sut = new DefaultPipelineConnectionDelegate();
            var graph = new PipelineGraph(new MockPipelineNodeProvider());

            IPipelineLazyValue<PipelineInput> lazyInput = null;
            IPipelineLazyValue<PipelineOutput> lazyOutput = null;
            var node1 = graph.CreateNode(n =>
            {
                lazyOutput = n.CreateOutput("", builder => builder.SetOutputType(typeof(int)));
            });
            var node2 = graph.CreateNode(n =>
            {
                n.CreateInput("", builder => builder.SetInputType(typeof(int)));
                n.CreateOutput("", builder => builder.SetOutputType(typeof(int)));
            });
            var node3 = graph.CreateNode(n =>
            {
                lazyInput = n.CreateInput("", builder => builder.SetInputType(typeof(int)));
            });

            graph.Connect(node1, node2);
            graph.Connect(node2, node3);

            var input = graph.GetInput(lazyInput.LazyValue);
            var output = graph.GetOutput(lazyOutput.LazyValue);

            Assert.IsFalse(sut.CanConnect(input, output, graph));
        }

        private class MockPipelineNodeProvider : IPipelineGraphNodeProvider
        {
            public PipelineBody GetBody(PipelineBodyId id)
            {
                return new PipelineBody(id, new []{ typeof(int) }, new[] {typeof(int)}, o => new []{AnyObservable.FromObservable(new Subject<object>())});
            }

            public bool CanCreateNode(PipelineNodeKind kind)
            {
                return false;
            }

            public void CreateNode(PipelineNodeKind nodeKind, PipelineNodeBuilder builder)
            {

            }
        }
    }
}

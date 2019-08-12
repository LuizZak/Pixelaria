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
using System.Windows.Threading;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixDirectX.Rendering.DirectX;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixPipelineGraph;
using PixRendering;
using PixUI.Controls;

namespace PixelariaTests.Views.ExportPipeline.PipelineView
{
    [TestClass]
    public class DefaultPipelineNodeViewSizerTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            // PipelineViewSnapshot.RecordMode = true;

            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
            ControlView.TextLayoutRenderer = new TestDirect2DRenderManager();
        }

        [TestMethod]
        public void TestAutoSizeEmptyNode()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeEmptyNodeStretchesToFitTitle()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Long Pipeline Step name to test view stretching");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);

            RunTest(nodeView, sut);
        }
        
        [TestMethod]
        public void TestAutoSizeEmptyNodeWithImage()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithInput()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input 1");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithInputStretchesToFitLabel()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input with large name to test view stretching");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithOutput()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddOutput("Output 1");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithOutputStretchesToFitLabel()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddOutput("Output 1 with large name to test view stretching");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }
        
        [TestMethod]
        public void TestAutoSizeNodeWithInputAndOutput()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input 1");
            gen.AddOutput("Output 1");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithInputAndOutputStretchesToFitLabels()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input with very long name");
            gen.AddOutput("Output with very long name");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithDescription()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.SetBodyText("A description that is placed within the node's body");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithDescriptionAndInput()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input");
            gen.SetBodyText("A description that is placed within the node's body");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithDescriptionAndInputAndOutput()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input");
            gen.AddOutput("Output");
            gen.SetBodyText("A description that is placed within the node's body");
            var node = gen.GetDescriptor();

            var nodeView = PipelineNodeView.Create(node, null);

            RunTest(nodeView, sut);
        }

        private void RunTest([NotNull] PipelineNodeView view, IPipelineNodeViewSizer sut, bool? recordMode = null)
        {
            TestWithRenderingState(provider =>
            {
                var sizeProvider = new D2DTextSizeProvider(provider);

                sut.AutoSize(view, sizeProvider);
            });

            PipelineViewSnapshot.Snapshot(view, TestContext, recordMode);
        }

        private static void TestWithRenderingState(Action<IDirect2DRenderingStateProvider> testAction)
        {
            using (var control = new ExportPipelineControl())
            using (var factory = new SharpDX.Direct2D1.Factory())
            using (var renderManager = new Direct2DRenderLoopManager(control, factory))
            {
                renderManager.Initialize();

                renderManager.RenderSingleFrame(state =>
                {
                    ((IDirect2DRenderingState) state).D2DRenderTarget.Clear(null);

                    var renderer = new TestDirect2DRenderManager();
                    renderer.Initialize(state);

                    PipelineControlConfigurator.RegisterIcons(renderer.ImageResources, state);

                    control.InitializeRenderer(renderer);

                    testAction(new StaticDirect2DRenderingStateProvider((IDirect2DRenderingState)state));
                });
            }
        }
    }

    internal class PipelineStepGenerator
    {
        private readonly MockPipelineStep _node;

        internal PipelineStepGenerator([NotNull] string name)
        {
            _node = new MockPipelineStep(name);
        }

        internal void SetBodyText([CanBeNull] string bodyText)
        {
            _node.BodyText = bodyText;
        }

        internal void AddInput([NotNull] string name)
        {
            var input = new MockPipelineInput(_node.Id, name);
            _node.InputLinks.Add(input);
        }

        internal void AddOutput([NotNull] string name)
        {
            var output = new MockPipelineOutput(_node.Id, name);
            _node.OutputLinks.Add(output);
        }

        internal IPipelineStep GetMock()
        {
            return _node;
        }

        internal PipelineNodeDescriptor GetDescriptor()
        {
            var descriptor = new PipelineNodeDescriptor
            {
                Title = _node.Name,
                BodyText = _node.BodyText
            };
            foreach (var input in _node.Input)
            {
                descriptor.Inputs.Add(new PipelineInputDescriptor(input.Name));
            }
            foreach (var output in _node.Output)
            {
                descriptor.Outputs.Add(new PipelineOutputDescriptor(output.Name));
            }

            return descriptor;
        }

        private class MockPipelineStep: IPipelineStep
        {
            public PipelineNodeId Id { get; } = new PipelineNodeId(Guid.NewGuid());
            public string Name { get; }
            public string BodyText { get; set; }

            public IReadOnlyList<IPipelineInput> Input => InputLinks;
            public IReadOnlyList<IPipelineOutput> Output => OutputLinks;

            public readonly List<MockPipelineInput> InputLinks = new List<MockPipelineInput>();
            public readonly List<MockPipelineOutput> OutputLinks = new List<MockPipelineOutput>();

            public MockPipelineStep([NotNull] string name)
            {
                Name = name;
            }

            public IPipelineMetadata GetMetadata()
            {
                if (BodyText == null) 
                    return PipelineMetadata.Empty;

                var objects = new Dictionary<string, object> {
                {
                    PipelineMetadataKeys.PipelineStepBodyText, BodyText
                }};

                return new PipelineMetadata(objects);

            }
        }

        private class MockPipelineInput : IPipelineInput
        {
            public PipelineNodeId NodeId { get; }
            public string Name { get; }

            public PipelineInput Id { get; }
            public IReadOnlyList<Type> DataTypes => new[] {typeof(object)};

            public MockPipelineInput(PipelineNodeId nodeId, [NotNull] string name)
            {
                NodeId = nodeId;
                Name = name;
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }
        }

        private class MockPipelineOutput : IPipelineOutput
        {
            public PipelineNodeId NodeId { get; }
            public string Name { get; }

            public PipelineOutput Id { get; }
            public Type DataType => typeof(object);

            public MockPipelineOutput(PipelineNodeId nodeId, [NotNull] string name)
            {
                NodeId = nodeId;
                Name = name;
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }
        }
    }
}

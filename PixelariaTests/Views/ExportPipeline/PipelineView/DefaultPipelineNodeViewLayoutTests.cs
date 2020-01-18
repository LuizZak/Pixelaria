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
using Pixelaria.Views.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixPipelineGraph;
using PixRendering;
using PixUI.Controls;

namespace PixelariaTests.Views.ExportPipeline.PipelineView
{
    [TestClass]
    public class DefaultPipelineNodeViewLayoutTests
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
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeEmptyNodeStretchesToFitTitle()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Long Pipeline Step name to test view stretching");
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);

            RunTest(nodeView, sut);
        }
        
        [TestMethod]
        public void TestAutoSizeEmptyNodeWithImage()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeEmptyNodeWithManagedImage()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            TestDirect2DRenderManager.CreateTemporary(manager => nodeView.ManagedIcon = manager.ImageResources.CreateManagedImageResource(Pixelaria.Properties.Resources.anim_icon));

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithInput()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input 1", typeof(object));
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithInputStretchesToFitLabel()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input with large name to test view stretching", typeof(object));
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithOutput()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddOutput("Output 1", typeof(object));
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithOutputStretchesToFitLabel()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddOutput("Output 1 with large name to test view stretching", typeof(object));
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }
        
        [TestMethod]
        public void TestAutoSizeNodeWithInputAndOutput()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input 1", typeof(object));
            gen.AddOutput("Output 1", typeof(object));
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithInputAndOutputStretchesToFitLabels()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input with very long name", typeof(object));
            gen.AddOutput("Output with very long name", typeof(object));
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.Icon = new ImageResource("anim_icon", 16, 16);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithDescription()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.SetBodyText("A description that is placed within the node's body");
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithDescriptionAndInput()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input", typeof(object));
            gen.SetBodyText("A description that is placed within the node's body");
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithDescriptionAndInputAndOutput()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input", typeof(object));
            gen.AddOutput("Output", typeof(object));
            gen.SetBodyText("A description that is placed within the node's body");
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);

            RunTest(nodeView, sut);
        }

        [TestMethod]
        public void TestAutoSizeNodeWithEditableInput()
        {
            var sut = new DefaultPipelineNodeViewLayout();

            var gen = new PipelineStepGenerator("Pipeline Step");
            gen.AddInput("Input", typeof(object));
            var node = gen.GetNodeView();

            var nodeView = PipelineNodeView.Create(node);
            nodeView.InputViews[0].IsInputEditable = true;

            RunTest(nodeView, sut);
        }

        private void RunTest([NotNull] PipelineNodeView view, [NotNull] IPipelineNodeViewLayout sut, bool? recordMode = null)
        {
            var sizeProvider = new D2DTextSizeProvider();

            sut.Layout(view, sizeProvider);

            PipelineViewSnapshot.Snapshot(view, TestContext, recordMode);
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

        internal void AddInput([NotNull] string name, [NotNull] Type type)
        {
            var input = new MockPipelineInput(_node.Id, name, type);
            _node.InputLinks.Add(input);
        }

        internal void AddOutput([NotNull] string name, [NotNull] Type type)
        {
            var output = new MockPipelineOutput(_node.Id, name, type);
            _node.OutputLinks.Add(output);
        }

        internal IPipelineNodeView GetNodeView()
        {
            return GetDescriptor().CreateView();
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
                descriptor.Inputs.Add(new PipelineInputDescriptor(input.Name, input.DataType));
            }
            foreach (var output in _node.Output)
            {
                descriptor.Outputs.Add(new PipelineOutputDescriptor(output.Name, output.DataType));
            }

            return descriptor;
        }

        private class MockPipelineStep
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
        }

        private class MockPipelineInput : IPipelineInput
        {
            public PipelineNodeId NodeId { get; }
            public string Name { get; }

            public PipelineInput Id { get; }
            public Type DataType { get; }

            public MockPipelineInput(PipelineNodeId nodeId, [NotNull] string name, [NotNull] Type dataType)
            {
                NodeId = nodeId;
                Name = name;
                DataType = dataType;
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
            public Type DataType { get; }

            public MockPipelineOutput(PipelineNodeId nodeId, [NotNull] string name, [NotNull] Type dataType)
            {
                NodeId = nodeId;
                Name = name;
                DataType = dataType;
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }
        }
    }
}

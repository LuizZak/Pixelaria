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
using PixDirectX.Rendering;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI.Controls;
using PixUI.Rendering;
using PixUITests.TestUtils;
using SharpDX.DirectWrite;

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
            ControlView.DirectWriteFactory = new Factory();
        }

        [TestMethod]
        public void TestAutoSizeEmptyNode()
        {
            var sut = new DefaultPipelineNodeViewSizer();

            var gen = new PipelineStepGenerator("Pipeline Step");
            var node = gen.GetMock();

            var nodeView = PipelineNodeView.Create(node);

            TestWithRenderingState(provider =>
            {
                var sizeProvider = new DefaultLabelViewSizeProvider(provider);

                sut.AutoSize(nodeView, sizeProvider);
            });

            BaseViewSnapshot.ImagesConfig = PipelineControlConfigurator.RegisterIcons;
            PipelineViewSnapshot.Snapshot(nodeView, TestContext);
        }

        private void TestWithRenderingState(Action<IDirect2DRenderingStateProvider> testAction)
        {
            using (var control = new ExportPipelineControl())
            using (var renderManager = new Direct2DRenderLoopManager(control))
            {
                renderManager.InitializeDirect2D();

                renderManager.RenderSingleFrame(state =>
                {
                    control.InitializeDirect2DRenderer(state);

                    var renderer = new TestDirect2DRenderer();
                    renderer.Initialize(state);
                    
                    PipelineControlConfigurator.RegisterIcons(renderer.ImageResources, state);

                    testAction(new StaticDirect2DRenderingStateProvider(state));
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

        internal void AddInput([NotNull] string name)
        {
            var input = new MockPipelineInput(_node, name);
            _node.InputLinks.Add(input);
        }

        internal void AddOutput([NotNull] string name)
        {
            var output = new MockPipelineOutput(_node, name);
            _node.OutputLinks.Add(output);
        }

        internal IPipelineStep GetMock()
        {
            return _node;
        }

        private class MockPipelineStep: IPipelineStep
        {
            public Guid Id { get; } = Guid.NewGuid();
            public string Name { get; }

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
                return PipelineMetadata.Empty;
            }
        }

        private class MockPipelineInput : IPipelineInput
        {
            public IPipelineNode Node { get; }

            public string Name { get; }

            public Type[] DataTypes => new[] {typeof(object)};

            public IPipelineOutput[] Connections => new IPipelineOutput[0];

            public MockPipelineInput(IPipelineNode node, [NotNull] string name)
            {
                Node = node;
                Name = name;
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }

            public IPipelineLinkConnection Connect(IPipelineOutput output)
            {
                return new PipelineLinkConnection(this, output, connection => { });
            }

            public void Disconnect(IPipelineOutput output)
            {
                
            }
        }

        private class MockPipelineOutput : IPipelineOutput
        {
            public IPipelineNode Node { get; }
            public string Name { get; }

            public Type DataType => typeof(object);

            public MockPipelineOutput(IPipelineNode node, [NotNull] string name)
            {
                Node = node;
                Name = name;
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }

            public IObservable<object> GetObservable()
            {
                throw new NotImplementedException();
            }
        }
    }
}

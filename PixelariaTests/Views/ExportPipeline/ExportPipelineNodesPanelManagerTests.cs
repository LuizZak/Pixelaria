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

using System.Drawing;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixDirectX.Rendering;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using PixUI.Controls;

namespace PixelariaTests.Views.ExportPipeline
{
    [TestClass]
    public class ExportPipelineNodesPanelManagerTests
    {
        private ExportPipelineControl _control;
        private TestContainer _container;
        private Direct2DRenderLoopManager _renderer;
        private ExportPipelineNodesPanelManager _sut;

        [TestInitialize]
        public void TestInitialize()
        {
            //BaseViewSnapshot.RecordMode = true;
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            _control = new ExportPipelineControl
            {
                Size = new Size(640, 480)
            };
            _container = new TestContainer
            {
                Container = {Size = _control.Size}
            };
            _renderer = new Direct2DRenderLoopManager(_control);
            _renderer.InitializeDirect2D();

            PipelineControlConfigurator.Configure(_control, _renderer.RenderingState);

            _sut = new ExportPipelineNodesPanelManager(_container, _control.D2DRenderer, new PipelineNodeBitmapGenerator(_control));
            _sut.RegisterResizeEvent(_control);

            var provider = new DefaultPipelineNodeSpecsProvider();
            _sut.LoadCreatablePipelineNodes(provider.GetNodeSpecs());

            BaseViewSnapshot.ImagesConfig = PipelineControlConfigurator.RegisterIcons;
        }

        [TestMethod]
        public void TestRendering()
        {
            BaseViewSnapshot.Snapshot(_container.Container, TestContext);
        }

        public TestContext TestContext { get; set; }

        internal class TestContainer : IControlContainer
        {
            public ControlView Container = new ControlView
            {
                BackColor = Color.Transparent
            };

            public void AddControl(ControlView view)
            {
                Container.AddChild(view);
            }

            public void RemoveControl(ControlView view)
            {
                Container.RemoveChild(view);
            }
        }
    }
}

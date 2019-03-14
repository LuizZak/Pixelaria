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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixDirectX.Rendering;

namespace PixDirectXTests.Rendering
{
    [TestClass]
    public class BaseDirect2DRendererTests
    {
        [TestMethod]
        public void TestRenderListenerOrdering()
        {
            var sut = new MockBaseDirect2DRenderer();
            var lastListener = new MockRenderListener(0);
            var firstListener1 = new MockRenderListener(1);
            var firstListener2 = new MockRenderListener(1);

            sut.AddRenderListener(lastListener);
            sut.AddRenderListener(firstListener1);
            sut.AddRenderListener(firstListener2);

            Assert.AreEqual(sut.RenderListenersSpy.IndexOf(lastListener), 0);
            Assert.AreEqual(sut.RenderListenersSpy.IndexOf(firstListener1), 1);
            Assert.AreEqual(sut.RenderListenersSpy.IndexOf(firstListener2), 2);
        }
    }

    class MockRenderListener : IRenderListener
    {
        public int RenderOrder { get; set; }

        public MockRenderListener(int renderOrder)
        {
            RenderOrder = renderOrder;
        }

        public void RecreateState(IDirect2DRenderingState state)
        {

        }

        public void Render(IRenderListenerParameters parameters)
        {
            
        }
    }

    class MockBaseDirect2DRenderer : BaseDirect2DRenderer
    {
        public IList<IRenderListener> RenderListenersSpy => RenderListeners;
    }
}

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

using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI.Controls;
using PixUITests.TestUtils;

namespace PixUITests.Controls
{
    [TestClass]
    public class ScrollBarControlTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            //BaseViewSnapshot.RecordMode = true;
        }
        
        [TestMethod]
        public void TestRenderingHorizontalDarkStyle()
        {
            var sut = new ScrollBarControl
            {
                Orientation = ScrollBarControl.ScrollBarOrientation.Horizontal,
                Size = new Vector(300, 20),
                ContentSize = 100,
                VisibleSize = 33,
                Scroll = 20,
                ScrollBarStyle = new DarkScrollBarControlStyle()
            };

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingHorizontalLightStyle()
        {
            var sut = new ScrollBarControl
            {
                Orientation = ScrollBarControl.ScrollBarOrientation.Horizontal,
                Size = new Vector(300, 20),
                ContentSize = 100,
                VisibleSize = 33,
                Scroll = 20,
                ScrollBarStyle = new LightScrollBarControlStyle()
            };

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        public TestContext TestContext { get; set; }
    }
}

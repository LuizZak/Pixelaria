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
using PixUI;
using PixUI.Controls;
using PixUI.Controls.ToolStrip;
using PixUITests.TestUtils;

namespace PixUITests.Controls.ToolStrip
{
    [TestClass]
    public class ToolStripMenuTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            // BaseViewSnapshot.RecordMode = true;
        }

        #region Layout Tests

        [TestMethod]
        public void TestAnchorIntoView_Top()
        {
            var sut = CreateSut();
            var view = new BaseView();
            
            sut.AnchorIntoView(view, ToolStripAnchorPosition.Top);
          
            Assert.AreEqual(ToolStripOrientation.Horizontal, sut.Orientation);
            Assert.That.Layout(sut).Constrains(sut.Anchors.Height, constant: ToolStripMenu.BarSize);
            Assert.That.Layout(view).Constrains(sut.Anchors.Top, view.Anchors.Top);
            Assert.That.Layout(view).Constrains(sut.Anchors.Left, view.Anchors.Left);
            Assert.That.Layout(view).Constrains(sut.Anchors.Right, view.Anchors.Right);
        }

        [TestMethod]
        public void TestAnchorIntoView_Left()
        {
            var sut = CreateSut();
            var view = new BaseView();

            sut.AnchorIntoView(view, ToolStripAnchorPosition.Left);

            Assert.AreEqual(ToolStripOrientation.Vertical, sut.Orientation);
            Assert.That.Layout(sut).Constrains(sut.Anchors.Width, constant: ToolStripMenu.BarSize);
            Assert.That.Layout(view).Constrains(sut.Anchors.Left, view.Anchors.Left);
            Assert.That.Layout(view).Constrains(sut.Anchors.Top, view.Anchors.Top);
            Assert.That.Layout(view).Constrains(sut.Anchors.Bottom, view.Anchors.Bottom);
        }

        [TestMethod]
        public void TestAnchorIntoView_Right()
        {
            var sut = CreateSut();
            var view = new BaseView();

            sut.AnchorIntoView(view, ToolStripAnchorPosition.Right);

            Assert.AreEqual(ToolStripOrientation.Vertical, sut.Orientation);
            Assert.That.Layout(sut).Constrains(sut.Anchors.Width, constant: ToolStripMenu.BarSize);
            Assert.That.Layout(view).Constrains(sut.Anchors.Right, view.Anchors.Right);
            Assert.That.Layout(view).Constrains(sut.Anchors.Top, view.Anchors.Top);
            Assert.That.Layout(view).Constrains(sut.Anchors.Bottom, view.Anchors.Bottom);
        }

        [TestMethod]
        public void TestAnchorIntoView_Bottom()
        {
            var sut = CreateSut();
            var view = new BaseView();

            sut.AnchorIntoView(view, ToolStripAnchorPosition.Bottom);

            Assert.AreEqual(ToolStripOrientation.Horizontal, sut.Orientation);
            Assert.That.Layout(sut).Constrains(sut.Anchors.Height, constant: ToolStripMenu.BarSize);
            Assert.That.Layout(view).Constrains(sut.Anchors.Bottom, view.Anchors.Bottom);
            Assert.That.Layout(view).Constrains(sut.Anchors.Left, view.Anchors.Left);
            Assert.That.Layout(view).Constrains(sut.Anchors.Right, view.Anchors.Right);
        }

        #endregion

        #region Rendering
        
        [TestMethod]
        public void TestEmptyRendering()
        {
            var sut = CreateSut();
            sut.Size = new Vector(100, ToolStripMenu.BarSize);

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        #endregion
        
        private static ToolStripMenu CreateSut()
        {
            return ToolStripMenu.Create();
        }
    }
}

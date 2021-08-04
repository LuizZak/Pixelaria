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
using PixUI.Controls.ToolStrip;

namespace PixUITests.Controls.ToolStrip
{
    [TestClass]
    public class ToolStripMenuViewManagerTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            // BaseViewSnapshot.RecordMode = true;
        }

        [TestMethod]
        public void TestRectForItem_Button_Horizontal()
        {
            var menu = ToolStripMenu.Create();
            menu.Orientation = ToolStripOrientation.Horizontal;
            var item = new ToolStripButton();
            var sut = CreateSut();

            Assert.AreEqual(AABB.FromRectangle(0, 0, 22, 22), sut.RectForItem(item, menu));
        }

        [TestMethod]
        public void TestRectForItem_Button_Vertical()
        {
            var menu = ToolStripMenu.Create();
            menu.Orientation = ToolStripOrientation.Vertical;
            var item = new ToolStripButton();
            var sut = CreateSut();

            Assert.AreEqual(AABB.FromRectangle(0, 0, 22, 22), sut.RectForItem(item, menu));
        }

        [TestMethod]
        public void TestRectForItem_Separator_Horizontal()
        {
            var menu = ToolStripMenu.Create();
            menu.Orientation = ToolStripOrientation.Horizontal;
            var item = new ToolStripSeparatorItem();
            var sut = CreateSut();

            Assert.AreEqual(AABB.FromRectangle(0, 0, 25, 6), sut.RectForItem(item, menu));
        }

        [TestMethod]
        public void TestRectForItem_Separator_Vertical()
        {
            var menu = ToolStripMenu.Create();
            menu.Orientation = ToolStripOrientation.Vertical;
            var item = new ToolStripSeparatorItem();
            var sut = CreateSut();

            Assert.AreEqual(AABB.FromRectangle(0, 0, 6, 25), sut.RectForItem(item, menu));
        }

        static ToolStripMenuViewManager CreateSut()
        {
            return new ToolStripMenuViewManager();
        }
    }
}

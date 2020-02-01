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
using FastBitmapLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixUI.Controls;
using PixUI.Controls.ContextMenu;
using PixUITests.TestUtils;

namespace PixUITests.Controls.ContextMenu
{
    [TestClass]
    public class ContextMenuControlTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            //BaseViewSnapshot.RecordMode = true;
        }

        [TestMethod]
        public void TestRender()
        {
            var item = new ContextMenuDropDownItem("Title");
            TestDirect2DRenderManager.CreateTemporary(manager =>
            {
                var bitmap = new Bitmap(16, 16);
                FastBitmap.ClearBitmap(bitmap, Color.Red);

                item.DropDownItems.Add("Item 1");
                item.DropDownItems.Add("Item 2 with long name", manager.ImageResources.CreateManagedImageResource(bitmap));

                var subItem = new ContextMenuDropDownItem("Item 3 with sub items");
                subItem.DropDownItems.Add("Sub Item");

                item.DropDownItems.Add(subItem);
                item.DropDownItems.Add("-");
                item.DropDownItems.Add("Item 4");
            });

            var sut = ContextMenuControl.Create(item);
            sut.Layout();
            sut.AutoSize();

            BaseViewSnapshot.Snapshot(sut, TestContext, recordMode: true);
        }

        [TestMethod]
        public void TestRenderSelectedItem()
        {
            var item = new ContextMenuDropDownItem("Title");
            TestDirect2DRenderManager.CreateTemporary(manager =>
            {
                var bitmap = new Bitmap(16, 16);
                FastBitmap.ClearBitmap(bitmap, Color.Red);

                item.DropDownItems.Add("Item 1");
                item.DropDownItems.Add("Item 2 with long name", manager.ImageResources.CreateManagedImageResource(bitmap));
                item.DropDownItems.Add("Item 3");
                item.DropDownItems.Add("-");
                item.DropDownItems.Add("Item 4");
            });

            var sut = ContextMenuControl.Create(item);
            sut.Layout();
            sut.AutoSize();
            var bounds = sut.BoundsForItem(item.DropDownItems[1]);
            sut.HitTestControl(bounds.Center).Highlighted = true;

            BaseViewSnapshot.Snapshot(sut, TestContext, recordMode: true);
        }

        public TestContext TestContext { get; set; }
    }
}

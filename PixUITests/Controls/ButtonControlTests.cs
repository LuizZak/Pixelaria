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
using PixCore.Geometry;
using PixRendering;
using PixUI.Controls;
using PixUITests.TestUtils;

namespace PixUITests.Controls
{
    [TestClass]
    public class ButtonControlTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            //BaseViewSnapshot.RecordMode = true;
        }

        [TestMethod]
        public void TestRendering()
        {
            var button = ButtonControl.Create();
            button.Text = "Button 1";
            button.TextInset = new InsetBounds(0, 0, 0, 0);
            button.Size = new Vector(60, 30);
            button.HorizontalTextAlignment = HorizontalTextAlignment.Center;
            button.VerticalTextAlignment = VerticalTextAlignment.Center;

            BaseViewSnapshot.Snapshot(button, TestContext);
        }

        [TestMethod]
        public void TestTextInsetsRendering()
        {
            var button = ButtonControl.Create();
            button.Text = "Button 1";
            button.TextInset = new InsetBounds(2, 2, 2, 2);
            button.Size = new Vector(60, 30);
            button.HorizontalTextAlignment = HorizontalTextAlignment.Center;
            button.VerticalTextAlignment = VerticalTextAlignment.Center;

            BaseViewSnapshot.Snapshot(button, TestContext);
        }

        [TestMethod]
        public void TestTightTextInsetsRendering()
        {
            var button = ButtonControl.Create();
            button.Text = "Button 1";
            button.TextInset = new InsetBounds(15, 4, 4, 15);
            button.Size = new Vector(60, 30);
            button.HorizontalTextAlignment = HorizontalTextAlignment.Center;
            button.VerticalTextAlignment = VerticalTextAlignment.Center;

            BaseViewSnapshot.Snapshot(button, TestContext);
        }

        [TestMethod]
        public void TestButtonImageResource()
        {
            var bitmap = new Bitmap(16, 16);
            FastBitmap.ClearBitmap(bitmap, Color.Red);
            var resources = new BaseViewSnapshotResources();
            var resource = resources.CreateImageResource("image", bitmap);
            var button = ButtonControl.Create();
            button.Text = "Button 1";
            button.TextInset = new InsetBounds(15, 4, 4, 15);
            button.ImageInset = new InsetBounds(6, 0, 0, 0);
            button.Size = new Vector(90, 30);
            button.HorizontalTextAlignment = HorizontalTextAlignment.Center;
            button.VerticalTextAlignment = VerticalTextAlignment.Center;
            button.Image = resource;

            BaseViewSnapshot.Snapshot(button, TestContext, resources: resources);
        }

        [TestMethod]
        public void TestButtonManagedImage()
        {
            var bitmap = new Bitmap(16, 16);
            FastBitmap.ClearBitmap(bitmap, Color.Red);
            var button = ButtonControl.Create();
            button.Text = "Button 1";
            button.TextInset = new InsetBounds(15, 4, 4, 15);
            button.ImageInset = new InsetBounds(6, 0, 0, 0);
            button.Size = new Vector(90, 30);
            button.HorizontalTextAlignment = HorizontalTextAlignment.Center;
            button.VerticalTextAlignment = VerticalTextAlignment.Center;

            BaseViewSnapshot.SnapshotTest(button, TestContext, imageResources =>
            {
                button.ManagedImage = imageResources.CreateManagedImageResource(bitmap);
            });
        }

        public TestContext TestContext { get; set; }
    }
}

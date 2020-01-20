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
using PixCore.Geometry;
using PixUI.Controls;
using PixUITests.Properties;
using PixUITests.TestUtils;

namespace PixUITests.Controls
{
    [TestClass]
    public class ImageViewControlTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
        }

        [TestMethod]
        public void TestImageFitModeNone()
        {
            RunFitModeTest(ImageFitMode.None);
        }

        [TestMethod]
        public void TestImageFitModeCenter()
        {
            RunFitModeTest(ImageFitMode.Center);
        }

        [TestMethod]
        public void TestImageFitModeStretch()
        {
            RunFitModeTest(ImageFitMode.Stretch);
        }

        [TestMethod]
        public void TestImageFitModeZoom()
        {
            RunFitModeTest(ImageFitMode.Zoom);
        }

        [TestMethod]
        public void TestImageFitModeTile()
        {
            RunFitModeTest(ImageFitMode.Tile, new Vector(512, 512));
        }

        [TestMethod]
        public void TestImageTint()
        {
            // Arrange
            var resources = new BaseViewSnapshotResources();
            var image = resources.CreateImageResource("image", Resources.pxl_icon_256x256);

            var sut = ImageViewControl.Create(image);
            sut.AutoSize = true;
            sut.Layout();

            // Act
            sut.ImageTintColor = Color.Red;

            // Assert
            BaseViewSnapshot.Snapshot(sut, TestContext, resources: resources);
        }

        private void RunFitModeTest(ImageFitMode fitMode, Vector? size = null)
        {
            // Arrange
            var resources = new BaseViewSnapshotResources();
            var image = resources.CreateImageResource("image", Resources.pxl_icon_256x256);

            var sut = ImageViewControl.Create(image);
            sut.AutoSize = false;
            sut.Size = size ?? new Vector(200, 128);

            // Act
            sut.ImageFitMode = fitMode;

            // Assert
            BaseViewSnapshot.Snapshot(sut, TestContext, resources: resources);
        }
    }
}

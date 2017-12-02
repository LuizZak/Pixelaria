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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Controls.ColorControls;

namespace PixCoreTests.Controls.ColorControls
{
    [TestClass]
    public class ColorPickerTests
    {
        [TestMethod]
        public void TestInitialState()
        {
            var sut = new ColorPicker();

            Assert.AreEqual(Color.FromArgb(255, 0, 0, 0), sut.FirstColor);
            Assert.AreEqual(Color.FromArgb(255, 255, 255, 255), sut.SecondColor);
        }

        [TestMethod]
        public void TestSetCurrentColorWithFirstColor()
        {
            var sut = new ColorPicker {SelectedColor = ColorPickerColor.FirstColor};

            sut.SetCurrentColor(Color.FromArgb(255, 255, 0, 0));
            
            Assert.AreEqual(Color.FromArgb(255, 255, 0, 0), sut.FirstColor);
        }

        [TestMethod]
        public void TestSetCurrentColorWithSecondColor()
        {
            var sut = new ColorPicker { SelectedColor = ColorPickerColor.SecondColor };

            sut.SetCurrentColor(Color.FromArgb(255, 255, 0, 0));

            Assert.AreEqual(Color.FromArgb(255, 255, 0, 0), sut.SecondColor);
        }

        [TestMethod]
        public void TestGetCurrentColorWithFirstColor()
        {
            var sut = new ColorPicker { SelectedColor = ColorPickerColor.FirstColor };
            sut.SetCurrentColor(Color.FromArgb(255, 255, 0, 0));
            sut.SelectedColor = ColorPickerColor.SecondColor;
            sut.SetCurrentColor(Color.FromArgb(255, 0, 0, 255));
            sut.SelectedColor = ColorPickerColor.FirstColor;

            var color = sut.GetCurrentColor();

            Assert.AreEqual(Color.FromArgb(255, 255, 0, 0), color);
        }

        [TestMethod]
        public void TestGetCurrentColorWithSecondColor()
        {
            var sut = new ColorPicker { SelectedColor = ColorPickerColor.FirstColor };
            sut.SetCurrentColor(Color.FromArgb(255, 255, 0, 0));
            sut.SelectedColor = ColorPickerColor.SecondColor;
            sut.SetCurrentColor(Color.FromArgb(255, 0, 0, 255));
            sut.SelectedColor = ColorPickerColor.SecondColor;

            var color = sut.GetCurrentColor();

            Assert.AreEqual(Color.FromArgb(255, 0, 0, 255), color);
        }
    }
}

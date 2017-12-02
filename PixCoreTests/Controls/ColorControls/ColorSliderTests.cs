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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Colors;
using PixCore.Controls.ColorControls;

namespace PixCoreTests.Controls.ColorControls
{
    [TestClass]
    public class ColorSliderTests
    {
        [TestMethod]
        public void TestCurrentValueAlphaColorComponent()
        {
            var sut = new ColorSlider
            {
                ActiveColor = new AhslColor(0.5f, 1, 1, 1),
                ColorComponent = ColorSliderComponent.Alpha
            };
            
            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestCurrentValueRedColorComponent()
        {
            var sut = new ColorSlider
            {
                ActiveColor = AhslColor.FromArgb(1f, 0.5f, 1f, 1f),
                ColorComponent = ColorSliderComponent.Red
            };

            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestCurrentValueGreenColorComponent()
        {
            var sut = new ColorSlider
            {
                ActiveColor = AhslColor.FromArgb(1f, 1f, 0.5f, 1f),
                ColorComponent = ColorSliderComponent.Green
            };

            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestCurrentValueBlueColorComponent()
        {
            var sut = new ColorSlider
            {
                ActiveColor = AhslColor.FromArgb(1f, 1f, 1f, 0.5f),
                ColorComponent = ColorSliderComponent.Blue
            };

            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestCurrentValueHueColorComponent()
        {
            var sut = new ColorSlider
            {
                ActiveColor = new AhslColor(1f, 0.5f, 1f, 1f),
                ColorComponent = ColorSliderComponent.Hue
            };

            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestCurrentValueSaturationColorComponent()
        {
            var sut = new ColorSlider
            {
                ActiveColor = new AhslColor(1f, 1f, 0.5f, 1f),
                ColorComponent = ColorSliderComponent.Saturation
            };

            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestCurrentValueLightnessColorComponent()
        {
            var sut = new ColorSlider
            {
                ActiveColor = new AhslColor(1f, 1f, 1f, 0.5f),
                ColorComponent = ColorSliderComponent.Lightness
            };

            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestCurrentValueCustomColorComponent()
        {
            var sut = new ColorSlider
            {
                CustomStartColor = AhslColor.FromArgb(1f, 1f, 1f, 1f),
                CustomEndColor = AhslColor.FromArgb(1f, 0f, 0f, 0f),
                CurrentValue = 0.5f,
                ColorComponent = ColorSliderComponent.Custom
            };

            Assert.AreEqual(0.5f, sut.CurrentValue);
        }

        [TestMethod]
        public void TestActiveColorCustomColorComponent()
        {
            var sut = new ColorSlider
            {
                CustomStartColor = AhslColor.FromArgb(1f, 1f, 1f, 1f),
                CustomEndColor = AhslColor.FromArgb(1f, 0f, 0f, 0f),
                CurrentValue = 0.5f,
                ColorComponent = ColorSliderComponent.Custom
            };

            Assert.AreEqual(AhslColor.FromArgb(1f, 0.5f, 0.5f, 0.5f), sut.ActiveColor);
        }
    }
}

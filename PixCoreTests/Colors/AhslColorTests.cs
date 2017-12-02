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

namespace PixCoreTests.Colors
{
    [TestClass]
    public class AhslColorTests
    {
        [TestMethod]
        public void TestCreateEmpty()
        {
            var sut = new AhslColor();

            Assert.AreEqual(0, sut.Alpha);
            Assert.AreEqual(0, sut.Hue);
            Assert.AreEqual(0, sut.Saturation);
            Assert.AreEqual(0, sut.Lightness);
            Assert.AreEqual(0, sut.Green);
            Assert.AreEqual(0, sut.Red);
            Assert.AreEqual(0, sut.Blue);
            Assert.AreEqual(0, sut.FloatAlpha);
            Assert.AreEqual(0, sut.FloatHue);
            Assert.AreEqual(0, sut.FloatSaturation);
            Assert.AreEqual(0, sut.FloatLightness);
            Assert.AreEqual(0, sut.FloatRed);
            Assert.AreEqual(0, sut.FloatGreen);
            Assert.AreEqual(0, sut.FloatBlue);
        }

        [TestMethod]
        public void TestCreateHslFloat()
        {
            var sut = new AhslColor(1.0f, 1, 1, 1);

            Assert.AreEqual(255, sut.Alpha);
            Assert.AreEqual(360, sut.Hue);
            Assert.AreEqual(100, sut.Saturation);
            Assert.AreEqual(100, sut.Lightness);
            Assert.AreEqual(255, sut.Green);
            Assert.AreEqual(255, sut.Red);
            Assert.AreEqual(255, sut.Blue);
            Assert.AreEqual(1, sut.FloatAlpha);
            Assert.AreEqual(1, sut.FloatHue);
            Assert.AreEqual(1, sut.FloatSaturation);
            Assert.AreEqual(1, sut.FloatLightness);
            Assert.AreEqual(1, sut.FloatRed);
            Assert.AreEqual(1, sut.FloatGreen);
            Assert.AreEqual(1, sut.FloatBlue);
        }

        [TestMethod]
        public void TestCreateHslInt()
        {
            var sut = new AhslColor(255, 360, 100, 100);

            Assert.AreEqual(255, sut.Alpha);
            Assert.AreEqual(360, sut.Hue);
            Assert.AreEqual(100, sut.Saturation);
            Assert.AreEqual(100, sut.Lightness);
            Assert.AreEqual(255, sut.Green);
            Assert.AreEqual(255, sut.Red);
            Assert.AreEqual(255, sut.Blue);
            Assert.AreEqual(1, sut.FloatAlpha);
            Assert.AreEqual(1, sut.FloatHue);
            Assert.AreEqual(1, sut.FloatSaturation);
            Assert.AreEqual(1, sut.FloatLightness);
            Assert.AreEqual(1, sut.FloatRed);
            Assert.AreEqual(1, sut.FloatGreen);
            Assert.AreEqual(1, sut.FloatBlue);
        }

        [TestMethod]
        public void TestWithTransparency()
        {
            var sut = new AhslColor(255, 360, 100, 100);

            var transparent = sut.WithTransparency(0.5f);

            Assert.AreEqual(127, transparent.Alpha);
        }

        [TestMethod]
        public void TestEquality()
        {
            Assert.AreEqual(new AhslColor(10, 11, 12, 13), new AhslColor(10, 11, 12, 13));
            Assert.AreNotEqual(new AhslColor(100, 110, 120, 130), new AhslColor(10, 11, 12, 13));
        }

        [TestMethod]
        public void TestFromArgb()
        {
            var color = AhslColor.FromArgb(255, 127, 128, 129);

            Assert.AreEqual(255, color.Alpha);
            Assert.AreEqual(127, color.Red);
            Assert.AreEqual(128, color.Green);
            Assert.AreEqual(129, color.Blue);
        }

        [TestMethod]
        public void TestFromArgbInt()
        {
            var color = AhslColor.FromArgb(unchecked((int)0xFFEEDDCC));

            Assert.AreEqual(0xFF, color.Alpha);
            Assert.AreEqual(0xEE, color.Red);
            Assert.AreEqual(0xDD, color.Green);
            Assert.AreEqual(0xCC, color.Blue);
        }

        [TestMethod]
        public void TestFromArgbfloat()
        {
            var color = AhslColor.FromArgb(1f, 0.5f, 0.25f, 0.125f);

            Assert.AreEqual(255, color.Alpha);
            Assert.AreEqual(127, color.Red);
            Assert.AreEqual(63, color.Green);
            Assert.AreEqual(31, color.Blue);
        }
    }
}

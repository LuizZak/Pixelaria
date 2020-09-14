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
using Pixelaria.Timeline;

namespace PixelariaTests.Timeline
{
    [TestClass]
    public class TimelineTests
    {
        [TestMethod]
        public void TestKeyframeRangeRatio()
        {
            var range = new KeyframeRange(1, 3);

            Assert.AreEqual(0, range.Ratio(0));
            Assert.AreEqual(0, range.Ratio(1));
            Assert.AreEqual(1.0f / 3.0f, range.Ratio(2));
            Assert.AreEqual(2.0f / 3.0f, range.Ratio(3));
            Assert.AreEqual(1.0f, range.Ratio(4));
            Assert.AreEqual(1.0f, range.Ratio(5));
        }

        [TestMethod]
        public void TestKeyframeRangeRatioOneSpan()
        {
            var range = new KeyframeRange(1, 1);

            Assert.AreEqual(0, range.Ratio(1));
            Assert.AreEqual(1f, range.Ratio(2));
        }
    }
}

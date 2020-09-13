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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Data.KeyframeMetadataSerializers;
using PixelariaLibTests.TestUtils;

namespace PixelariaLibTests.Data.KeyframeMetadataSerializers
{
    [TestClass]
    public class PointKeyframeSerializerTests
    {
        [TestMethod]
        public void TestSerializedType()
        {
            var sut = new PointKeyframeSerializer();

            Assert.AreEqual(typeof(Point), sut.SerializedType);
        }

        [TestMethod]
        public void TestSerializedName()
        {
            var sut = new PointKeyframeSerializer();

            Assert.AreEqual("System.Drawing.Point", sut.SerializedName);
        }

        [TestMethod]
        public void TestSerialize()
        {
            var sut = new PointKeyframeSerializer();
            var value = new Point(5, 6);
            var stream = new MemoryStream();

            sut.Serialize(value, stream);

            Assert.That.MemoryStreamMatches(stream, new byte[] {5, 0, 0, 0, 6, 0, 0, 0});
        }

        [TestMethod]
        public void TestDeserialize()
        {
            var pointKeyframeSerializer = new PointKeyframeSerializer();
            var stream = new MemoryStream(new byte[] {5, 0, 0, 0, 6, 0, 0, 0});

            var result = (Point) pointKeyframeSerializer.Deserialize(stream);

            Assert.AreEqual(new Point(5, 6), result);
        }
    }
}

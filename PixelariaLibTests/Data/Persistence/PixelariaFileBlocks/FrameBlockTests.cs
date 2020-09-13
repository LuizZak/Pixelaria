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
using PixelariaLib.Data.Persistence;
using PixelariaLib.Data.Persistence.PixelariaFileBlocks;
using PixelariaLibTests.TestGenerators;

namespace PixelariaLibTests.Data.Persistence.PixelariaFileBlocks
{
    [TestClass]
    public class FrameBlockTests
    {
        [TestMethod]
        public void TestPrepareFromBundleAddsKeyframeBlocks()
        {
            var stream = new MemoryStream();
            var bundle = BundleGenerator.GenerateTestBundle(0);
            var file = new PixelariaFile(bundle, stream);
            var frame = bundle.Animations[0].Frames[0];
            frame.KeyframeMetadata["point"] = new Point(0, 1);
            var frameBlock = new FrameBlock(frame)
            {
                OwningFile = file
            };

            frameBlock.PrepareFromBundle(bundle);

            Assert.AreEqual(1, file.GetBlocksByType(typeof(KeyframeBlock)).Length);
        }
    }
}

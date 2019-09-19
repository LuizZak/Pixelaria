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
using Pixelaria.Data.Persistence.PixelariaFileBlocks;
using Pixelaria.Data.Persistence;
using PixelariaTests.TestGenerators;

namespace PixelariaTests.Data.Persistence.PixelariaFileBlocks
{
    [TestClass]
    public class AnimationHeaderBlockTests
    {
        [TestMethod]
        public void TestPrepareFromBundle()
        {
            var bundle = BundleGenerator.GenerateTestBundle(0);
            var animation = AnimationGenerator.GenerateAnimation("name", 16, 16, 4);
            var file = new PixelariaFile(bundle, "");
            var sut = new AnimationHeaderBlock(animation) {OwningFile = file};

            sut.PrepareFromBundle(bundle);

            Assert.AreEqual(4, file.GetBlocksByType(typeof(FrameBlock)).Length);
        }
    }
}

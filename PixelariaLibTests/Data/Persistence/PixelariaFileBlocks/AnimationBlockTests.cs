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
using PixelariaLib.Data.Persistence;
using PixelariaLib.Data.Persistence.PixelariaFileBlocks;
using PixelariaLibTests.TestGenerators;

namespace PixelariaLibTests.Data.Persistence.PixelariaFileBlocks
{
    [TestClass]
    public class AnimationBlockTests
    {
        [TestMethod]
        public void TestPrepareFromBundle()
        {
            var bundle = BundleGenerator.GenerateTestBundle(0, 2, 2, 2);
            var file = new PixelariaFile(bundle, "");
            var sut = new AnimationBlock { OwningFile = file };

            sut.PrepareFromBundle(bundle);

            Assert.AreEqual(8, file.GetBlocksByType(typeof(FrameBlock)).Length);
        }
    }
}
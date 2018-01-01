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
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.ExportPipeline.Steps;
using Rhino.Mocks;

namespace PixelariaTests.ExportPipeline.Steps
{
    [TestClass]
    public class BitmapImportPipelineStepTests
    {
        [TestMethod]
        [Timeout(500)]
        public async Task TestStep()
        {
            var bitmap = Generators.BitmapGenerator.GenerateRandomBitmap(16, 16, 0);
            var source = MockRepository.GenerateStub<IBitmapImportSource>();
            source.Stub(s => s.LoadBitmap()).Return(bitmap);
            var sut = new BitmapImportPipelineStep(source);

            object bit = await sut.Output[0].GetObservable();

            Assert.AreEqual(bitmap, bit);
        }

        [TestMethod]
        [Timeout(500)]
        public async Task TestStepMakesOnlyOneLoadRequestAndSharesResult()
        {
            var bitmap = Generators.BitmapGenerator.GenerateRandomBitmap(16, 16, 0);
            var source = MockRepository.GenerateStrictMock<IBitmapImportSource>();
            source.Stub(s => s.LoadBitmap()).Return(bitmap).Repeat.Once();
            var sut = new BitmapImportPipelineStep(source);

            object bit1 = await sut.Output[0].GetObservable();
            object bit2 = await sut.Output[0].GetObservable();

            Assert.AreEqual(bit1, bit2);
        }
    }
}

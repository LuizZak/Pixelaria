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
using Pixelaria.Algorithms.PaintOperations;
using PixelariaTests.Generators;

namespace PixelariaTests.Algorithms.PaintOperations
{
    /// <summary>
    /// Tests the PlottingPaintUndoGenerator class and related components
    /// </summary>
    [TestClass]
    public class PlottingPaintUndoGeneratorTests
    {
        /// <summary>
        /// Tests the registering of the plotted pixels on the underlying undo task
        /// </summary>
        [TestMethod]
        public void TestPlottingRegistering()
        {
            var bitmap = BitmapGenerator.GenerateRandomBitmap(16, 16, 1);
            var sut = new PlottingPaintUndoGenerator(bitmap, "");

            sut.OperationStarted(true);
            sut.PlottedPixel(new Point(5, 5), 0xFFFF22, 0xFF0011);
            sut.OperationFinished(null);

            Assert.AreEqual(1, sut.UndoTask.PixelHistoryTracker.PixelCount);
            Assert.IsTrue(sut.UndoTask.PixelHistoryTracker.ContainsPixel(5, 5));
        }
    }
}
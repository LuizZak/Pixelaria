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
using Pixelaria.Algorithms.PaintOperations.UndoTasks;

namespace PixelariaTests.Tests.PaintOperations
{
    /// <summary>
    /// Tests the PixelHistoryTracker class and related components
    /// </summary>
    [TestClass]
    public class PixelHistoryTrackerTests
    {
        /// <summary>
        /// Tests that the RegisterPixel method is behaving correctly
        /// </summary>
        [TestMethod]
        public void TestPixelRegistering()
        {
            // Create the tracker
            PixelHistoryTracker tracker = new PixelHistoryTracker(true, 12);

            // Add the pixels
            tracker.RegisterPixel(5, 5, 0xFF, 0x1F);

            var undo = tracker.PixelUndoForPixel(5, 5);
            Assert.IsTrue(undo != null && undo.Value.OldColor == 0xFF, "The returned PixelUndo does not contains the undo color that was expected");
        }

        /// <summary>
        /// Tests duplicated pixels feeded to the RegisterPixel with the KeepReplacedOriginal intact
        /// </summary>
        [TestMethod]
        public void TestDuplicatedPixelRegisteringKeepReplacedOriginal()
        {
            // Create the tracker
            PixelHistoryTracker tracker = new PixelHistoryTracker(true, 12);

            // Add the pixels
            tracker.RegisterPixel(5, 5, 0xFF, 0x1F);
            tracker.RegisterPixel(5, 5, 0xEF, 0x2F);
            tracker.RegisterPixel(5, 5, 0xCF, 0x3F);

            var undo = tracker.PixelUndoForPixel(5, 5);
            Assert.IsTrue(undo != null && undo.Value.OldColor == 0xFF,
                "The returned PixelUndo does not contains the undo color that was expected. The pixel color must match the color of the first pixel registered RegisterPixel");
        }

        /// <summary>
        /// Tests duplicated pixels feeded to the RegisterPixel
        /// </summary>
        [TestMethod]
        public void TestDuplicatedPixelRegisteringReplaceOriginal()
        {
            // Create the tracker
            PixelHistoryTracker tracker = new PixelHistoryTracker(false, 12);

            // Add the pixels
            tracker.RegisterPixel(5, 5, 0xFF, 0x1F, false);
            tracker.RegisterPixel(5, 5, 0xEF, 0x2F, false);
            tracker.RegisterPixel(5, 5, 0xCF, 0x3F, false);

            var undo = tracker.PixelUndoForPixel(5, 5);
            Assert.IsTrue(undo != null && undo.Value.OldColor == 0xCF,
                "The returned PixelUndo does not contains the undo color that was expected. The pixel color must match the color of the last pixel registered RegisterPixel");
        }

        /// <summary>
        /// Tests RegisterUncheckedPixel with duplicated pixels and keep original undos off
        /// </summary>
        [TestMethod]
        public void TestDuplicatedPixelUncheckedRegisteringReplaceOriginal()
        {
            // Create the tracker
            PixelHistoryTracker tracker = new PixelHistoryTracker(false, 12);

            // Add the pixels
            tracker.RegisterUncheckedPixel(5, 5, 0xFF, 0x1F);
            tracker.RegisterUncheckedPixel(5, 5, 0xEF, 0x2F);
            tracker.RegisterUncheckedPixel(5, 5, 0xCF, 0x3F);

            var undo = tracker.PixelUndoForPixel(5, 5);
            Assert.IsTrue(undo != null && undo.Value.OldColor == 0xCF,
                "The returned PixelUndo does not contains the undo color that was expected. The pixel color must match the color of the last pixel registered RegisterPixel");
        }

        /// <summary>
        /// Tests Clear method
        /// </summary>
        [TestMethod]
        public void TestClearing()
        {
            // Create the tracker
            PixelHistoryTracker tracker = new PixelHistoryTracker(false, 12);

            // Add the pixels
            tracker.RegisterPixel(5, 5, 0xFF, 0x1F);
            tracker.RegisterPixel(6, 5, 0xEF, 0x2F);
            tracker.RegisterPixel(7, 5, 0xCF, 0x3F);

            tracker.Clear();

            var undo = tracker.PixelUndoForPixel(5, 5);

            Assert.IsNull(undo, "After a call to .Clear(), all pixels that were previously stored must be cleared off the PixelHistoryTracker");
        }

        /// <summary>
        /// Tests the PixelCount property and its intended value
        /// </summary>
        [TestMethod]
        public void TestPixelCount()
        {
            // Create the tracker
            PixelHistoryTracker tracker = new PixelHistoryTracker(false, 12);

            // Add the pixels
            tracker.RegisterPixel(5, 5, 0xFF, 0x1F);
            tracker.RegisterPixel(6, 5, 0xEF, 0x2F);
            tracker.RegisterPixel(7, 5, 0xCF, 0x3F);

            Assert.AreEqual(3, tracker.PixelCount, "After consecutive calls to RegisterPixel, the pixel count must match the number of unique pixels provided");
        }

        /// <summary>
        /// Tests equality accross PixelUndo values
        /// </summary>
        [TestMethod]
        public void TestPixelUndoEquality()
        {
            PixelHistoryTracker.PixelUndo undo1 = new PixelHistoryTracker.PixelUndo(0, 0, 0, 0xFF, 0xFE);
            PixelHistoryTracker.PixelUndo undo2 = new PixelHistoryTracker.PixelUndo(0, 0, 0, 0x00, 0xFE);

            Assert.AreEqual(undo1, undo2, "Equal PixelUndos of different old colors are to be considered equal");
        }

        /// <summary>
        /// Tests equality accross PixelUndo values
        /// </summary>
        [TestMethod]
        public void TestPixelUndoInequality()
        {
            PixelHistoryTracker.PixelUndo undo1 = new PixelHistoryTracker.PixelUndo(0, 0, 0, 0xFF, 0xFE);
            PixelHistoryTracker.PixelUndo undo2 = new PixelHistoryTracker.PixelUndo(1, 0, 0, 0xFF, 0xFE);

            Assert.AreNotEqual(undo1, undo2, "PixelUndos of different pixel coordinates are not to be considered equal");
        }

        /// <summary>
        /// Tests equality accross PixelUndo values
        /// </summary>
        [TestMethod]
        public void TestPixelUndoSameHashCode()
        {
            PixelHistoryTracker.PixelUndo undo1 = new PixelHistoryTracker.PixelUndo(0, 0, 0, 0xFF, 0xFE);
            PixelHistoryTracker.PixelUndo undo2 = new PixelHistoryTracker.PixelUndo(0, 0, 0, 0x00, 0xFE);

            Assert.AreEqual(undo1.GetHashCode(), undo2.GetHashCode(), "Equal PixelUndos of different old colors must have the same hash code");
        }

        /// <summary>
        /// Tests equality accross PixelUndo values
        /// </summary>
        [TestMethod]
        public void TestPixelUndoDifferentHashCode()
        {
            PixelHistoryTracker.PixelUndo undo1 = new PixelHistoryTracker.PixelUndo(0, 0, 0, 0xFF, 0xFE);
            PixelHistoryTracker.PixelUndo undo2 = new PixelHistoryTracker.PixelUndo(0, 1, 0, 0xFF, 0xFE);

            Assert.AreNotEqual(undo1.GetHashCode(), undo2.GetHashCode(), "PixelUndos of different coordinates must no have the same hash code");
        }
    }
}
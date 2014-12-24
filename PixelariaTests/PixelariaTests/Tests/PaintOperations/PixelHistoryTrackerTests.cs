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

namespace PixelariaTests.PixelariaTests.Tests.PaintOperations
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
            PixelHistoryTracker tracker = new PixelHistoryTracker(true, true, 12);

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
            PixelHistoryTracker tracker = new PixelHistoryTracker(true, true, 12);

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
            PixelHistoryTracker tracker = new PixelHistoryTracker(true, false, 12);

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
            PixelHistoryTracker tracker = new PixelHistoryTracker(true, false, 12);

            // Add the pixels
            tracker.RegisterUncheckedPixel(5, 5, 0xFF, 0x1F);
            tracker.RegisterUncheckedPixel(5, 5, 0xEF, 0x2F);
            tracker.RegisterUncheckedPixel(5, 5, 0xCF, 0x3F);

            var undo = tracker.PixelUndoForPixel(5, 5);
            Assert.IsTrue(undo != null && undo.Value.OldColor == 0xCF,
                "The returned PixelUndo does not contains the undo color that was expected. The pixel color must match the color of the last pixel registered RegisterPixel");
        }

        /// <summary>
        /// Tests duplicated pixels feeded to the RegisterPixel with 'ignoreIfDuplicated' set to false and pixel indexing disabled 
        /// </summary>
        [TestMethod]
        public void TestNoDuplicateAllowDuplicated()
        {
            // Create the tracker
            PixelHistoryTracker tracker = new PixelHistoryTracker(false, false, 12);

            // Add the pixels
            tracker.RegisterPixel(5, 5, 0xFF, 0x1F, false);
            tracker.RegisterPixel(5, 5, 0xEF, 0x2F, false);
            tracker.RegisterPixel(5, 5, 0xCF, 0x3F, false);

            Assert.AreEqual(3, tracker.PixelList.Count,
                "The number of pixels stored does not match the expected number of pixels stored after calling RegisterPixel with 'checkExisting' false and pixel indexing disabled");
        }
    }
}
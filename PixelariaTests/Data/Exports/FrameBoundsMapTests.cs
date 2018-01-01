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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using PixelariaTests.Generators;

namespace PixelariaTests.Data.Exports
{
    /// <summary>
    /// Tests FrameBoundsMap class and related components
    /// </summary>
    [TestClass]
    public class FrameBoundsMapTests
    {
        /// <summary>
        /// Tests basic frame registering on a frame bounds map
        /// </summary>
        [TestMethod]
        public void TestFrameRegistering()
        {
            var frame = FrameGenerator.GenerateRandomFrame(32, 32);
            var bounds = new Rectangle(0, 10, 32, 32);

            var boundsZero = new Rectangle(0, 0, 32, 32);

            var map = new FrameBoundsMap();

            Assert.IsNull(map.GetLocalBoundsForFrame(frame));
            Assert.IsFalse(map.ContainsFrame(frame));

            map.RegisterFrames(new[] {frame}, bounds);

            Assert.IsNotNull(map.GetLocalBoundsForFrame(frame));
            Assert.IsTrue(map.ContainsFrame(frame));

            Assert.AreEqual(map.GetLocalBoundsForFrame(frame), bounds);
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame), boundsZero, "When adding a frame sheet bounds via RegisterFrames(), the X and Y axis of the bounds rectangle must be ignored and set to 0.");
        }

        /// <summary>
        /// Tests sharing of frame bounds, where modifications to a frame's bound reflect on another
        /// </summary>
        [TestMethod]
        public void TestFrameBoundsSharing()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame3 = FrameGenerator.GenerateRandomFrame(32, 32);
            var bounds1 = new Rectangle(0, 0, 32, 32);
            var bounds2 = new Rectangle(0, 0, 40, 40);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, bounds1);
            map.RegisterFrames(new[] { frame2 }, bounds2);
            map.RegisterFrames(new[] { frame3 }, bounds2);

            Assert.AreEqual(map.GetSheetBoundsForFrame(frame1), bounds1);
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame2), bounds2);

            map.ShareSheetBoundsForFrames(frame1, frame2);

            // frame2 now should point to the same bounds as frame1
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame2), bounds1);
            
            // Test calling share again with already-shared frames is a noop
            map.ShareSheetBoundsForFrames(frame1, frame2);
            
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame2), bounds1);
        }

        /// <summary>
        /// Tests replacing of entire sheet bounds arrays in a sheet frame bounds map
        /// </summary>
        [TestMethod]
        public void TestFrameBoundsReplacing()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var bounds1 = new Rectangle(0, 0, 32, 32);
            var bounds2 = new Rectangle(32, 32, 40, 40);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, bounds1);
            map.RegisterFrames(new[] { frame2 }, bounds2);

            var replace1 = new Rectangle(10, 20, 30, 40);
            var replace2 = new Rectangle(100, 200, 300, 400);

            map.ReplaceSheetBounds(new [] { replace1, replace2 });

            Assert.AreEqual(map.GetSheetBoundsForFrame(frame1), replace1);
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame2), replace2);
        }

        /// <summary>
        /// Tests replacing of entire sheet bounds arrays in a sheet frame bounds map, while bounds are shared between frames
        /// </summary>
        [TestMethod]
        public void TestFrameBoundsReplacingShared()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var bounds1 = new Rectangle(0, 0, 32, 32);
            var bounds2 = new Rectangle(0, 0, 40, 40);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, bounds1);
            map.RegisterFrames(new[] { frame2 }, bounds2);

            map.ShareSheetBoundsForFrames(frame1, frame2);

            var replace1 = new Rectangle(10, 20, 30, 40);

            map.ReplaceSheetBounds(new[] { replace1 });

            Assert.AreEqual(map.GetSheetBoundsForFrame(frame1), replace1);
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame2), replace1);
        }

        /// <summary>
        /// Tests direct replacement of frame bounds in a frame bounds map
        /// </summary>
        [TestMethod]
        public void TestFrameBoundsSetForFrame()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var rectangle = new Rectangle(0, 0, 32, 32);

            var newBounds = new Rectangle(32, 0, 32, 32);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, rectangle);
            map.RegisterFrames(new[] { frame2 }, rectangle);

            // Direct set
            map.SetSheetBoundsForFrame(frame1, newBounds);

            Assert.AreEqual(map.GetSheetBoundsForFrame(frame1), newBounds);
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame2), rectangle);

            // Now share and set the bounds again
            map.ShareSheetBoundsForFrames(frame1, frame2);
            map.SetSheetBoundsForFrame(frame1, newBounds);

            Assert.AreEqual(map.GetSheetBoundsForFrame(frame2), newBounds);
        }

        /// <summary>
        /// Tests setting of local frame bounds for frames via <see cref="FrameBoundsMap.SetLocalBoundsForFrame"/> method
        /// </summary>
        [TestMethod]
        public void TestSetLocalBoundsForFrame()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var originalRect = new Rectangle(5, 5, 15, 15);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, originalRect);
            map.RegisterFrames(new[] { frame2 }, originalRect);

            var newRect1 = new Rectangle(5, 5, 20, 20);
            
            // Set local bounds now
            map.SetLocalBoundsForFrame(frame1, newRect1);

            Assert.AreEqual(map.GetLocalBoundsForFrame(frame1), newRect1);
            Assert.AreEqual(map.GetLocalBoundsForFrame(frame2), originalRect); // Check we didn't affect another unrelated frame
        }

        /// <summary>
        /// Tests setting of local frame bounds for frames via <see cref="FrameBoundsMap.SetLocalBoundsForFrame"/> method, where frames
        /// are currently sharing sheet bounds - in this case, settings a local bounds should not affect other shared frames' local bounds
        /// </summary>
        [TestMethod]
        public void TestSetLocalBoundsForFrameShared()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var originalRect = new Rectangle(5, 5, 15, 15);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1, frame2 }, originalRect);

            var newRect1 = new Rectangle(5, 5, 20, 20);

            // Set local bounds now
            map.SetLocalBoundsForFrame(frame1, newRect1);

            Assert.AreEqual(map.GetLocalBoundsForFrame(frame1), newRect1);
            Assert.AreEqual(map.GetLocalBoundsForFrame(frame2), originalRect); // Check we didn't affect another unrelated frame
        }

        /// <summary>
        /// Tests counting of frames referencing a specific sheet index via <see cref="FrameBoundsMap.CountOfFramesAtSheetBoundsIndex"/> method
        /// </summary>
        [TestMethod]
        public void TestCountFramesOnBoundsIndex()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new []{ frame1 }, new Rectangle());
            map.RegisterFrames(new []{ frame2 }, new Rectangle());

            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(0), 1);
            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(1), 1);

            // Share frames and check results now
            map.ShareSheetBoundsForFrames(frame1, frame2);

            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(0), 2);
        }

        /// <summary>
        /// Tests sharing of bounds on sequential list of frames
        /// </summary>
        [TestMethod]
        public void TestSequentialFrameListBoundsSharing()
        {
            var frames = new List<Frame>();

            for (int i = 0; i < 100; i++)
            {
                var frame = FrameGenerator.GenerateRandomFrame(32, 32);
                frames.Add(frame);
            }

            var map = new FrameBoundsMap();

            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                map.RegisterFrames(new[] {frame}, new Rectangle(0, 0, 10, 10 + i));
            }
            
            // Go around now sharing frames
            for (int i = 0; i < frames.Count - 1; i++)
            {
                map.ShareSheetBoundsForFrames(frames[i], frames[i + 1]);
            }

            // Test now frames are all the same
            var targetRect = new Rectangle(0, 0, 10, 10);

            foreach (var frame in frames)
            {
                Assert.AreEqual(map.GetSheetBoundsForFrame(frame), targetRect);
            }
        }

        /// <summary>
        /// Tests sharing of bounds on long lists of frames
        /// </summary>
        [TestMethod]
        public void TestLongFrameListBoundsSharing()
        {
            var frames = new List<Frame>();

            for (int i = 0; i < 100; i++)
            {
                var frame = FrameGenerator.GenerateRandomFrame(32, 32);
                frames.Add(frame);
            }

            var map = new FrameBoundsMap();

            foreach (var frame in frames)
            {
                map.RegisterFrames(new[] { frame }, new Rectangle(0, 0, 10, 10));
            }

            var rand = new Random();

            // Go around now sharing frames
            for (int i = 0; i < frames.Count; i++)
            {
                int lower = rand.Next(i, frames.Count);
                int upper = rand.Next(i, frames.Count);

                map.ShareSheetBoundsForFrames(frames[lower], frames[upper]);
            }

            // Go around now sharing frames
            map.ShareSheetBoundsForFrames(frames[0], frames[1]);
            map.ShareSheetBoundsForFrames(frames[10], frames[20]);
            map.ShareSheetBoundsForFrames(frames[30], frames[5]);
        }

        /// <summary>
        /// Tests splitting a once shared bounds index with another frame via <see cref="FrameBoundsMap.SplitSharedSheetBoundsForFrame"/> method
        /// </summary>
        [TestMethod]
        public void TestSplitSharedFrameBounds()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, new Rectangle());
            map.RegisterFrames(new[] { frame2 }, new Rectangle());

            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(0), 1);
            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(1), 1);

            Assert.AreEqual(map.SheetBounds.Length, 2);

            // Share frames and check results now
            map.ShareSheetBoundsForFrames(frame1, frame2);

            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(0), 2);

            Assert.AreEqual(map.SheetBounds.Length, 1);

            // Now split frame2 out of the list to a unique index
            Assert.IsTrue(map.SplitSharedSheetBoundsForFrame(frame2));

            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(0), 1);
            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(1), 1);

            Assert.AreEqual(map.SheetBounds.Length, 2);

            // Change rectangle of frame2
            Assert.AreEqual(map.GetSheetBoundsForFrame(frame1), map.GetSheetBoundsForFrame(frame2)); // Currently the same

            map.SetSheetBoundsForFrame(frame2, new Rectangle(0, 0, 20, 20));

            Assert.AreNotEqual(map.GetSheetBoundsForFrame(frame1), map.GetSheetBoundsForFrame(frame2));

            // Tests trying to split a unique frame again results in false being returned, and no changes being recorded
            Assert.IsFalse(map.SplitSharedSheetBoundsForFrame(frame2));

            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(0), 1);
            Assert.AreEqual(map.CountOfFramesAtSheetBoundsIndex(1), 1);

            Assert.AreEqual(map.SheetBounds.Length, 2);
        }

        /// <summary>
        /// Tests fetching index at SheetBounds array that maps to a specific frame, shared or not, via <see cref="FrameBoundsMap.SheetIndexForFrame"/> method
        /// </summary>
        [TestMethod]
        public void TestSheetIndexForFrame()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame3 = FrameGenerator.GenerateRandomFrame(32, 32);
            var rect1 = new Rectangle(0, 0, 20, 20);
            var rect2 = new Rectangle(0, 0, 32, 32);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, rect1);
            map.RegisterFrames(new[] { frame2, frame3 }, rect2);

            Assert.AreEqual(map.SheetBounds[map.SheetIndexForFrame(frame1)], rect1);
            Assert.AreEqual(map.SheetBounds[map.SheetIndexForFrame(frame2)], rect2);
            Assert.AreEqual(map.SheetBounds[map.SheetIndexForFrame(frame3)], rect2);

            // Try splitting shared index now and making sure it works
            map.SplitSharedSheetBoundsForFrame(frame3);

            map.SetSheetBoundsForFrame(frame2, new Rectangle());

            Assert.AreEqual(map.SheetBounds[map.SheetIndexForFrame(frame3)], rect2);
        }

        /// <summary>
        /// Tests fetching frame IDs at a BoundsSheet index using the <see cref="FrameBoundsMap.FrameIdsAtSheetIndex"/> method
        /// </summary>
        [TestMethod]
        public void TestFramesAtBoundsSheetIndex()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32);
            var frame3 = FrameGenerator.GenerateRandomFrame(32, 32);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame1 }, new Rectangle());
            map.RegisterFrames(new[] { frame2, frame3 }, new Rectangle());

            Assert.IsTrue(map.FrameIdsAtSheetIndex(map.SheetIndexForFrame(frame1)).Contains(frame1.ID));
            Assert.IsTrue(map.FrameIdsAtSheetIndex(map.SheetIndexForFrame(frame2)).Contains(frame2.ID));
            Assert.IsTrue(map.FrameIdsAtSheetIndex(map.SheetIndexForFrame(frame3)).Contains(frame3.ID));
        }

        /// <summary>
        /// Tests that an exception is raised when trying to replace the rectangles of a frame bounds map with a list
        /// of rectangles that has a different count
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFrameBoundReplaceCountMismatch()
        {
            var map = new FrameBoundsMap();
            
            var replace1 = new Rectangle(10, 20, 30, 40);

            map.ReplaceSheetBounds(new[] { replace1 });
        }

        /// <summary>
        /// Tests that an exception is thrown when trying to insert a non-initialized frame
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRegisterFramesNonInitializedFrameException()
        {
            var frame = new Frame();

            var map = new FrameBoundsMap();
            
            map.RegisterFrames(new [] { frame }, new Rectangle());
        }

        /// <summary>
        /// Tests that an exception is thrown when trying to share bounds with a non-initialized frame
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestShareFrameBoundsNonInitializedFrameException()
        {
            var frame1 = new Frame();
            var frame2 = new Frame();

            var map = new FrameBoundsMap();

            map.ShareSheetBoundsForFrames(frame1, frame2);
        }

        /// <summary>
        /// Tests that an exception is thrown when trying to set the bounds of a non-initialized frame
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSetSheetBoundsForFrameNonInitializedFrameException()
        {
            var frame = new Frame();

            var map = new FrameBoundsMap();

            map.SetSheetBoundsForFrame(frame, new Rectangle());
        }

        /// <summary>
        /// Tests that an exception is thrown when trying to insert a frame with an invalid id of -1
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRegisterFramesInvalidFrameIdException()
        {
            var frame = new Frame(null, 32, 32, false);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new[] { frame }, new Rectangle());
        }

        /// <summary>
        /// Tests that an exception is thrown when trying to share bounds with a frame with an invalid id of -1
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestShareFrameBoundsInvalidFrameIdException()
        {
            var frame1 = new Frame(null, 32, 32, false);
            var frame2 = new Frame(null, 32, 32, false);

            var map = new FrameBoundsMap();

            map.ShareSheetBoundsForFrames(frame1, frame2);
        }

        /// <summary>
        /// Tests that an exception is thrown when trying to set the bounds of a frame with an invalid id of -1
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSetSheetBoundsForFrameInvalidFrameIdException()
        {
            var frame = new Frame(null, 32, 32, false);

            var map = new FrameBoundsMap();

            map.SetSheetBoundsForFrame(frame, new Rectangle());
        }

        /// <summary>
        /// Tests a bug that resulted in sheet bounds being mangled between frames when sharing frames in a certain order
        /// </summary>
        [TestMethod]
        public void TestFrameSharing_LocalBoundsBug()
        {
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32); frame1.ID = 1;
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32); frame2.ID = 2;
            var frame3 = FrameGenerator.GenerateRandomFrame(32, 32); frame3.ID = 3;
            var bounds1 = new Rectangle(0, 0, 32, 32);
            var bounds2 = new Rectangle(0, 0, 20, 20);
            var bounds3 = new Rectangle(0, 0, 10, 10);

            var map = new FrameBoundsMap();

            map.RegisterFrames(new [] { frame1 }, bounds1);
            map.RegisterFrames(new[] { frame2 }, bounds2);
            map.RegisterFrames(new[] { frame3 }, bounds3);

            map.ShareSheetBoundsForFrames(frame1, frame2);

            Assert.AreEqual(map.GetSheetBoundsForFrame(frame3), bounds3);
        }
    }
}

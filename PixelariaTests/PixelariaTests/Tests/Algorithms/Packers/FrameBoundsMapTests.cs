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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Pixelaria.Algorithms.Packers;
using Pixelaria.Data;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Algorithms.Packers
{
    /// <summary>
    /// Tests DefaultTexturePacker.FrameBoundsMap class and related components
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
            var frame = FrameGenerator.GenerateRandomFrame(32, 32); frame.ID = 1;
            var bounds = new Rectangle(0, 10, 32, 32);

            var boundsZero = new Rectangle(0, 0, 32, 32);

            var map = new DefaultTexturePacker.FrameBoundsMap();

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
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32); frame1.ID = 1;
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32); frame2.ID = 2;
            var frame3 = FrameGenerator.GenerateRandomFrame(32, 32); frame3.ID = 3;
            var bounds1 = new Rectangle(0, 0, 32, 32);
            var bounds2 = new Rectangle(0, 0, 40, 40);

            var map = new DefaultTexturePacker.FrameBoundsMap();

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
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32); frame1.ID = 1;
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32); frame2.ID = 2;
            var bounds1 = new Rectangle(0, 0, 32, 32);
            var bounds2 = new Rectangle(32, 32, 40, 40);

            var map = new DefaultTexturePacker.FrameBoundsMap();

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
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32); frame1.ID = 1;
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32); frame2.ID = 2;
            var bounds1 = new Rectangle(0, 0, 32, 32);
            var bounds2 = new Rectangle(0, 0, 40, 40);

            var map = new DefaultTexturePacker.FrameBoundsMap();

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
            var frame1 = FrameGenerator.GenerateRandomFrame(32, 32); frame1.ID = 1;
            var frame2 = FrameGenerator.GenerateRandomFrame(32, 32); frame2.ID = 2;
            var rectangle = new Rectangle(0, 0, 32, 32);

            var newBounds = new Rectangle(32, 0, 32, 32);

            var map = new DefaultTexturePacker.FrameBoundsMap();

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
        /// Tests sharing of bounds on sequential list of frames
        /// </summary>
        [TestMethod]
        public void TestSequentialFrameListBoundsSharing()
        {
            var frames = new List<Frame>();

            for (int i = 0; i < 100; i++)
            {
                var frame = FrameGenerator.GenerateRandomFrame(32, 32);
                frame.ID = i;
                frames.Add(frame);
            }

            var map = new DefaultTexturePacker.FrameBoundsMap();

            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                map.RegisterFrames(new[] {frame}, new Rectangle(0, 0, 10, 10 + i));
            }

            var rand = new Random();

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
                frame.ID = i;
                frames.Add(frame);
            }

            var map = new DefaultTexturePacker.FrameBoundsMap();

            foreach (var frame in frames)
            {
                map.RegisterFrames(new[] { frame }, new Rectangle(0, 0, 10, 10));
            }

            var rand = new Random();

            // Go around now sharing frames
            for (int i = 0; i < frames.Count; i++)
            {
                int lower = rand.Next(0, frames.Count);
                int upper = rand.Next(0, frames.Count);

                map.ShareSheetBoundsForFrames(frames[lower], frames[upper]);
            }

            // Go around now sharing frames
            map.ShareSheetBoundsForFrames(frames[0], frames[1]);
            map.ShareSheetBoundsForFrames(frames[10], frames[20]);
            map.ShareSheetBoundsForFrames(frames[30], frames[5]);
        }

        /// <summary>
        /// Tests that an exception is raised when trying to replace the rectangles of a frame bounds map with a list
        /// of rectangles that has a different count
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFrameBoundReplaceCountMismatch()
        {
            var map = new DefaultTexturePacker.FrameBoundsMap();
            
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

            var map = new DefaultTexturePacker.FrameBoundsMap();
            
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

            var map = new DefaultTexturePacker.FrameBoundsMap();

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

            var map = new DefaultTexturePacker.FrameBoundsMap();

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

            var map = new DefaultTexturePacker.FrameBoundsMap();

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

            var map = new DefaultTexturePacker.FrameBoundsMap();

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

            var map = new DefaultTexturePacker.FrameBoundsMap();

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

            var map = new DefaultTexturePacker.FrameBoundsMap();

            map.RegisterFrames(new [] { frame1 }, bounds1);
            map.RegisterFrames(new[] { frame2 }, bounds2);
            map.RegisterFrames(new[] { frame3 }, bounds3);

            map.ShareSheetBoundsForFrames(frame1, frame2);

            Assert.AreEqual(map.GetSheetBoundsForFrame(frame3), bounds3);
        }
    }
}

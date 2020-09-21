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
using PixelariaLib.Timeline;

namespace PixelariaLibTests.Timeline
{
    [TestClass]
    public class KeyframeCollectionSourceTests
    {
        [TestMethod]
        public void AddKeyframeRemovesKeyframesOverlappingIncomingKeyframe()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(1, 1, 0));
            sut.AddKeyframe(new Keyframe(2, 1, 0));
            sut.AddKeyframe(new Keyframe(3, 2, 0));

            sut.AddKeyframe(new Keyframe(0, 4, 0));

            Assert.AreEqual(1, sut.Keyframes.Count);
            Assert.AreEqual(new KeyframeRange(0, 4), sut.Keyframes[0].KeyframeRange);
        }

        [TestMethod]
        public void TestAddKeyframeLimitsExistingKeyframeRanges()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(0, 5, 0));

            sut.AddKeyframe(new Keyframe(2, 1, 0));

            Assert.AreEqual(2, sut.Keyframes.Count);
            Assert.AreEqual(new KeyframeRange(0, 2), sut.Keyframes[0].KeyframeRange);
            Assert.AreEqual(new KeyframeRange(2, 1), sut.Keyframes[1].KeyframeRange);
        }

        [TestMethod]
        public void TestInsertKeyframe()
        {
            var sut = CreateSut();

            sut.AddKeyframe(new Keyframe(0, 5, 10));

            Assert.AreEqual(5, sut.FrameCount);
            Assert.AreEqual(1, sut.Keyframes.Count);
        }

        [TestMethod]
        public void TestInsertKeyframeAtFrameIndex()
        {
            var sut = CreateSut();

            sut.InsertKeyframe(0, 10);

            Assert.AreEqual(1, sut.FrameCount);
            Assert.AreEqual(1, sut.Keyframes.Count);
        }

        [TestMethod]
        public void TestInsertKeyframeAtFrameIndexSplicesKeyframeRanges()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(0, 10, 10));

            sut.InsertKeyframe(5, 5);

            Assert.AreEqual(10, sut.FrameCount);
            Assert.AreEqual(2, sut.Keyframes.Count);
            Assert.AreEqual(new KeyframeRange(0, 5), sut.Keyframes[0].KeyframeRange);
            Assert.AreEqual(new KeyframeRange(5, 5), sut.Keyframes[1].KeyframeRange);
        }

        [TestMethod]
        public void TestInsertKeyframeAtFrameIndexOnLastFrameOfExistingKeyframeRange()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(29, 2, 10));

            sut.InsertKeyframe(30, 5);

            Assert.AreEqual(31, sut.FrameCount);
            Assert.AreEqual(2, sut.Keyframes.Count);
            Assert.AreEqual(new KeyframeRange(29, 1), sut.Keyframes[0].KeyframeRange);
            Assert.AreEqual(new KeyframeRange(30, 1), sut.Keyframes[1].KeyframeRange);
        }

        [TestMethod]
        public void TestInsertKeyframeAtFrameIndexReplacesKeyframeIfOverlappingExactlyOnKeyframe()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(0, 10, 10));

            sut.InsertKeyframe(0, 5);

            Assert.AreEqual(10, sut.FrameCount);
            Assert.AreEqual(1, sut.Keyframes.Count);
            Assert.AreEqual(new KeyframeRange(0, 10), sut.Keyframes[0].KeyframeRange);
        }

        [TestMethod]
        public void TestChangeKeyframeLength()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(0, 5, 0));

            sut.ChangeKeyframeLength(0, 10);

            Assert.AreEqual(new KeyframeRange(0, 10), sut.Keyframes[0].KeyframeRange);
        }

        [TestMethod]
        public void TestChangeKeyframeLengthSetsMinimumLengthOfOne()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(0, 5, 0));

            sut.ChangeKeyframeLength(0, -1);

            Assert.AreEqual(new KeyframeRange(0, 1), sut.Keyframes[0].KeyframeRange);
        }

        [TestMethod]
        public void TestChangeKeyframeLengthCapsMaximumLengthToOneFrameBeforeNextKeyframe()
        {
            var sut = CreateSut();
            sut.AddKeyframe(new Keyframe(0, 1, 0));
            sut.AddKeyframe(new Keyframe(5, 1, 0));

            sut.ChangeKeyframeLength(0, 10);

            Assert.AreEqual(new KeyframeRange(0, 5), sut.Keyframes[0].KeyframeRange);
            Assert.AreEqual(new KeyframeRange(5, 1), sut.Keyframes[1].KeyframeRange);
        }

        private static KeyframeCollectionSource CreateSut()
        {
            return new KeyframeCollectionSource();
        }
    }
}

﻿/*
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
using Pixelaria.Timeline;
using Pixelaria.Views.ModelViews;

namespace PixelariaTests.Timeline
{
    [TestClass]
    public class TimelineTests
    {
        [TestMethod]
        public void TestRelationship()
        {
            var sut = CreateTimeline();
            sut.FrameCount = 6;
            sut.AddKeyframe(0);
            sut.AddKeyframe(1);
            sut.AddKeyframe(3);

            Assert.AreEqual(KeyframePosition.Full, sut.RelationshipToFrame(0));
            Assert.AreEqual(KeyframePosition.First, sut.RelationshipToFrame(1));
            Assert.AreEqual(KeyframePosition.Last, sut.RelationshipToFrame(2));
            Assert.AreEqual(KeyframePosition.First, sut.RelationshipToFrame(3));
            Assert.AreEqual(KeyframePosition.Center, sut.RelationshipToFrame(4));
            Assert.AreEqual(KeyframePosition.Last, sut.RelationshipToFrame(5));
        }

        [TestMethod]
        public void TestSetFrameCount()
        {
            var sut = CreateTimeline();

            sut.FrameCount = 0;

            Assert.AreEqual(sut.FrameCount, 1);
        }

        [TestMethod]
        public void TestKeyframeForFrame()
        {
            var sut = CreateTimeline();
            sut.FrameCount = 6;
            sut.AddKeyframe(0);
            sut.AddKeyframe(1);
            sut.AddKeyframe(3);

            Assert.AreEqual(0, sut.KeyframeForFrame(0).Value.Frame);
            Assert.AreEqual(1, sut.KeyframeForFrame(1).Value.Frame);
            Assert.AreEqual(1, sut.KeyframeForFrame(2).Value.Frame);
            Assert.AreEqual(3, sut.KeyframeForFrame(3).Value.Frame);
        }

        [TestMethod]
        public void TestKeyframeRangeForFrame()
        {
            var sut = CreateTimeline();
            sut.FrameCount = 6;
            sut.AddKeyframe(0);
            sut.AddKeyframe(1);
            sut.AddKeyframe(3);

            Assert.AreEqual(new Pixelaria.Timeline.Timeline.KeyframeRange(0, 1), sut.KeyframeRangeForFrame(0));
            Assert.AreEqual(new Pixelaria.Timeline.Timeline.KeyframeRange(1, 2), sut.KeyframeRangeForFrame(1));
            Assert.AreEqual(new Pixelaria.Timeline.Timeline.KeyframeRange(1, 2), sut.KeyframeRangeForFrame(2));
            Assert.AreEqual(new Pixelaria.Timeline.Timeline.KeyframeRange(3, 3), sut.KeyframeRangeForFrame(3));
            Assert.AreEqual(new Pixelaria.Timeline.Timeline.KeyframeRange(3, 3), sut.KeyframeRangeForFrame(4));
            Assert.AreEqual(new Pixelaria.Timeline.Timeline.KeyframeRange(3, 3), sut.KeyframeRangeForFrame(5));
        }

        [TestMethod]
        public void TestKeyframeExactlyOnFrame()
        {
            var sut = CreateTimeline();
            sut.FrameCount = 6;
            sut.AddKeyframe(0);
            sut.AddKeyframe(1);
            sut.AddKeyframe(3);

            Assert.IsNotNull(sut.KeyframeExactlyOnFrame(0));
            Assert.IsNotNull(sut.KeyframeExactlyOnFrame(1));
            Assert.IsNull(sut.KeyframeExactlyOnFrame(2));
            Assert.IsNotNull(sut.KeyframeExactlyOnFrame(3));
            Assert.IsNull(sut.KeyframeExactlyOnFrame(4));
            Assert.IsNull(sut.KeyframeExactlyOnFrame(5));
        }

        [TestMethod]
        public void TestKeyframeRangeRatio()
        {
            var range = new Pixelaria.Timeline.Timeline.KeyframeRange(1, 3);

            Assert.AreEqual(0, range.Ratio(0));
            Assert.AreEqual(0, range.Ratio(1));
            Assert.AreEqual(1.0f / 3.0f, range.Ratio(2));
            Assert.AreEqual(2.0f / 3.0f, range.Ratio(3));
            Assert.AreEqual(1.0f, range.Ratio(4));
            Assert.AreEqual(1.0f, range.Ratio(5));
        }

        [TestMethod]
        public void TestKeyframeRangeRatioOneSpan()
        {
            var range = new Pixelaria.Timeline.Timeline.KeyframeRange(1, 1);

            Assert.AreEqual(0, range.Ratio(1));
            Assert.AreEqual(1f, range.Ratio(2));
        }

        [TestMethod]
        public void TestKeyframeValuesBetween()
        {
            var sut = CreateTimeline(new NumericTimelineLayerController());
            sut.FrameCount = 6;
            sut.AddKeyframe(0, 0.0f);
            sut.AddKeyframe(1, 1.0f);
            sut.AddKeyframe(3, 2.0f);

            Assert.AreEqual((0.0f, 1.0f), sut.KeyframeValuesBetween(0));
            Assert.AreEqual((1.0f, 2.0f), sut.KeyframeValuesBetween(1));
            Assert.AreEqual((1.0f, 2.0f), sut.KeyframeValuesBetween(2));
            Assert.AreEqual((2.0f, 2.0f), sut.KeyframeValuesBetween(3));
            Assert.AreEqual((2.0f, 2.0f), sut.KeyframeValuesBetween(4));
            Assert.AreEqual((2.0f, 2.0f), sut.KeyframeValuesBetween(5));
        }

        [TestMethod]
        public void TestInterpolateKeyframes()
        {
            var sut = CreateTimeline(new NumericTimelineLayerController());
            sut.FrameCount = 11;
            sut.AddKeyframe(0, 0.0f);
            sut.AddKeyframe(10, 1.0f);
            sut.AddKeyframe(4);

            var keyframe = sut.KeyframeExactlyOnFrame(4);

            Assert.AreEqual(0.4f, keyframe.Value.Value);
        }

        private static Pixelaria.Timeline.Timeline CreateTimeline(ITimelineLayerController controller = null)
        {
            return new Pixelaria.Timeline.Timeline(new KeyframeCollectionSource(), controller ?? new FrameOriginTimelineController());
        }

        private class NumericTimelineLayerController : ITimelineLayerController
        {
            public object DefaultKeyframeValue()
            {
                return 0.0f;
            }

            public object DuplicateKeyframeValue(object value)
            {
                return value;
            }

            public object InterpolatedValue(object start, object end, float ratio)
            {
                if (start is float f1 && end is float f2)
                {
                    return (f2 - f1) * ratio + f1;
                }

                return start;
            }
        }
    }
}

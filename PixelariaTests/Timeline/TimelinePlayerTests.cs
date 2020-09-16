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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Timeline;

namespace PixelariaTests.Timeline
{
    [TestClass]
    public class TimelinePlayerTests
    {
        [TestMethod]
        public void TestValueForFrame()
        {
            var timeline = new Pixelaria.Timeline.Timeline();
            timeline.AddLayer(new IntegerTimelineLayerController());
            timeline.AddKeyframe(0, 0, 0);
            timeline.AddKeyframe(5, 0, 5);
            timeline.AddKeyframe(10, 0, 0);
            var timelinePlayer = new TimelinePlayer(timeline);

            Assert.AreEqual(0, timelinePlayer.ValueForFrame(0, 0));
            Assert.AreEqual(1, timelinePlayer.ValueForFrame(1, 0));
            Assert.AreEqual(2, timelinePlayer.ValueForFrame(2, 0));
            Assert.AreEqual(3, timelinePlayer.ValueForFrame(3, 0));
            Assert.AreEqual(4, timelinePlayer.ValueForFrame(4, 0));
            Assert.AreEqual(5, timelinePlayer.ValueForFrame(5, 0));
            Assert.AreEqual(4, timelinePlayer.ValueForFrame(6, 0));
            Assert.AreEqual(3, timelinePlayer.ValueForFrame(7, 0));
            Assert.AreEqual(2, timelinePlayer.ValueForFrame(8, 0));
            Assert.AreEqual(1, timelinePlayer.ValueForFrame(9, 0));
            Assert.AreEqual(0, timelinePlayer.ValueForFrame(10, 0));
        }

        private class IntegerTimelineLayerController : ITimelineLayerController
        {
            public object DefaultKeyframeValue()
            {
                return 0;
            }

            public object DuplicateKeyframeValue(object value)
            {
                return value;
            }

            public object InterpolatedValue(object start, object end, float ratio)
            {
                if (start is int f1 && end is int f2)
                {
                    return (int) Math.Round((f2 - f1) * ratio + f1);
                }
                
                return start;
            }
        }
    }
}

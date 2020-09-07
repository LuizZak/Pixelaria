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
using System.ComponentModel;

namespace Pixelaria.Timeline
{
    public interface ITimeline
    {
        /// <summary>
        /// Event handler for keyframe-related events
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        delegate void KeyframeEventHandler(object sender, TimelineKeyframeEventArgs e);
        /// <summary>
        /// Event fired when a new keyframe is added
        /// </summary> 
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a new keyframe is added")]
        event KeyframeEventHandler KeyframeAdded;
        /// <summary>
        /// Event fired when a keyframe is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a keyframe iss removed")]
        event KeyframeEventHandler KeyframeRemoved;

        /// <summary>
        /// Event handler for events related to keyframe value changes
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        delegate void KeyframeValueChangedEventHandler(object sender, TimelineKeyframeValueChangeEventArgs e);
        /// <summary>
        /// Event fired when a keyframe's value has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the the value of a keyframe changes")]
        event KeyframeValueChangedEventHandler KeyframeValueChanged;

        int FrameCount { get; }
        ITimelineLayerController LayerController { get; }
        KeyframePosition RelationshipToFrame(int frame);
        void SetKeyframeValue(int frame, object value);
        void AddKeyframe(int frame, object value = null);
        void RemoveKeyframe(int frame);
        Keyframe? KeyframeExactlyOnFrame(int frame);
        Timeline.KeyframeRange? KeyframeRangeForFrame(int frame);

        /// <summary>
        /// Searches for the two keyframes immediately before and after a given frame, and
        /// returns their keyframe values.
        ///
        /// In case the frame lands exactly on a frame, the method returns that keyframe's
        /// value as the first element of the tuple, and the value for the next keyframe
        /// after that keyframe as the second element of the tuple.
        ///
        /// In case the frame lands after the last keyframe, both values represent the last
        /// keyframe's value.
        ///
        /// If there are no keyframes on this timeline, a (null, null) tuple is returned,
        /// instead.
        /// </summary>
        (object, object) KeyframeValuesBetween(int frame);
    }

    public class TimelineKeyframeEventArgs : EventArgs
    {
        public int FrameIndex { get; }

        public TimelineKeyframeEventArgs(int frameIndex)
        {
            FrameIndex = frameIndex;
        }
    }

    public class TimelineKeyframeValueChangeEventArgs : EventArgs
    {
        public int FrameIndex { get; }
        public object Value { get; }

        public TimelineKeyframeValueChangeEventArgs(int frameIndex, object value)
        {
            FrameIndex = frameIndex;
            Value = value;
        }
    }
}
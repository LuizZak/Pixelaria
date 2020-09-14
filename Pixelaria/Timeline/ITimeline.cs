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

        /// <summary>
        /// Gets the number of available layers on this timeline.
        /// </summary>
        int LayerCount { get; }

        /// <summary>
        /// Gets the total number of frames on this timeline.
        ///
        /// This value is the maximal frame count across all available timeline layers.
        /// </summary>
        int FrameCount { get; }

        /// <summary>
        /// Returns a layer object at a given index.
        ///
        /// The index must be between 0 and <see cref="LayerCount"/> - 1.
        /// </summary>
        ITimelineLayer LayerAtIndex(int index);
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
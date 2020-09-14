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

using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Pixelaria.Timeline
{
    public interface IKeyframeSource
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
        /// Gets a list of the keyframe indexes.
        /// </summary>
        IReadOnlyList<int> KeyframeIndexes { get; }

        /// <summary>
        /// Gets the total frame count for this keyframe source.
        /// </summary>
        int FrameCount { get; }

        /// <summary>
        /// Sets the value for a keyframe at a specific frame. If the frame is not
        /// a keyframe, a new keyframe is created and its value set as <see cref="value"/>
        /// </summary>
        void SetKeyframeValue(int frame, object value);

        /// <summary>
        /// Adds a new keyframe at a specified frame.
        /// </summary>
        void AddKeyframe(int frame, object value = null);

        /// <summary>
        /// Removes a keyframe at a specified frame. If no keyframes exists at <see cref="frame"/>,
        /// nothing is changed.
        /// </summary>
        void RemoveKeyframe(int frame);

        /// <summary>
        /// Gets the value for a keyframe on a given index.
        ///
        /// Returns null, in case the frame at the given index is not a keyframe or has no value associated.
        /// </summary>
        [CanBeNull]
        object ValueForKeyframe(int frameIndex);
    }
}
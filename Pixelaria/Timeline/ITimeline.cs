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

namespace Pixelaria.Timeline
{
    public interface ITimeline
    {
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
}
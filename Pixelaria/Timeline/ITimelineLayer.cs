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
    /// <summary>
    /// Interface for an abstract timeline layer composed of keyframes interspersed across discrete time entries
    /// </summary>
    public interface ITimelineLayer
    {
        /// <summary>
        /// Gets the total frame count for this keyframe source.
        /// </summary>
        int FrameCount { get; }

        /// <summary>
        /// The layer controller for this layer
        /// </summary>
        ITimelineLayerController LayerController { get; }

        /// <summary>
        /// Returns the keyframe that controls a given frame index.
        /// </summary>
        Keyframe? KeyframeForFrame(int frame);

        /// <summary>
        /// Returns the keyframe relationship of a specific frame in this layer
        /// </summary>
        KeyframePosition RelationshipToFrame(int frame);

        /// <summary>
        /// If the current frame at <see cref="frame"/> is a keyframe, returns that keyframe information,
        /// otherwise returns null.
        /// </summary>
        Keyframe? KeyframeExactlyOnFrame(int frame);

        /// <summary>
        /// Returns the range of a keyframe for a frame located at <see cref="frame"/>.
        ///
        /// The range matches the keyframe range that <see cref="frame"/> is contained at,
        /// or null, in case no keyframe is set before or after <see cref="frame"/>
        /// </summary>
        KeyframeRange? KeyframeRangeForFrame(int frame);

        /// <summary>
        /// Searches for the two keyframes before and after a given frame, and returns
        /// their keyframe values.
        ///
        /// In case the frame lands exactly on a key frame, the method returns that keyframe's
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

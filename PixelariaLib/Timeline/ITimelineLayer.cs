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

namespace PixelariaLib.Timeline
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
        /// The display name for this layer
        /// </summary>
        string LayerName { get; }

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
    }
}

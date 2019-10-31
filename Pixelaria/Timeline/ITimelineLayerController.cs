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
    /// Controls the values for a specific timeline layer's keyframes
    /// </summary>
    public interface ITimelineLayerController
    {
        /// <summary>
        /// Requests the default value for a newly created keyframe.
        /// </summary>
        object DefaultKeyframeValue();

        /// <summary>
        /// Requests a copy of the given keyframe value.
        /// </summary>
        object DuplicateKeyframeValue(object value);

        /// <summary>
        /// Requests the interpolated value between two keyframe values, with a given ratio.
        /// </summary>
        object InterpolatedValue(object start, object end, float ratio);
    }
}
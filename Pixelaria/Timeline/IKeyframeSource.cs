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
using JetBrains.Annotations;

namespace Pixelaria.Timeline
{
    public interface IKeyframeSource
    {
        /// <summary>
        /// Gets a list of the keyframe indexes.
        /// </summary>
        IReadOnlyList<int> KeyframeIndexes { get; }

        /// <summary>
        /// Adds a new keyframe on a given frame with a given value
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <param name="value"></param>
        void AddKeyframe(int frameIndex, object value);

        /// <summary>
        /// Sets the value for a given keyframe
        /// </summary>
        void SetKeyframeValue(int frameIndex, object value);

        /// <summary>
        /// Gets the value for a keyframe on a given index.
        ///
        /// Returns null, in case the frame at the given index is not a keyframe or has no value associated.
        /// </summary>
        [CanBeNull]
        object ValueForKeyframe(int frameIndex);

        /// <summary>
        /// Removes a keyframe at a given frame
        /// </summary>
        void RemoveKeyframe(int frameIndex);
    }
}
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
    /// Interface for a timeline object
    /// </summary>
    public interface ITimeline
    {
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
    }
}
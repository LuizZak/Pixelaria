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
using System.Drawing;

namespace Pixelaria.Data
{
    /// <summary>
    /// Common interface for animation objects
    /// </summary>
    public interface IAnimation : IDisposable
    {
        /// <summary>
        /// Gets or sets the name of this animation
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the width of this animation's frames
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of this animation's frames
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the size of this animation's frames
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Gets the number of frames of this Animaion
        /// </summary>
        int FrameCount { get; }

        /// <summary>
        /// The playbar settings for this Animation
        /// </summary>
        AnimationPlaybackSettings PlaybackSettings { get; set; }

        /// <summary>
        /// The export settings of this animation
        /// </summary>
        AnimationExportSettings ExportSettings { get; set; }

        /// <summary>
        /// Gets the composed bitmap for a specified frame index
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the composed bitmap of</param>
        /// <returns>The composed bitmap for the frame at the specified index on this animation</returns>
        Bitmap GetComposedBitmapForFrame(int frameIndex);
    }
}
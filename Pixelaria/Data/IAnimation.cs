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

using System.Drawing;

namespace Pixelaria.Data
{
    /// <summary>
    /// Common interface for animation objects
    /// </summary>
    public interface IAnimation : IBitmapFrameSequence
    {
        /// <summary>
        /// Gets or sets the name of this animation
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The playbar settings for this Animation
        /// </summary>
        AnimationPlaybackSettings PlaybackSettings { get; }

        /// <summary>
        /// The export settings of this animation
        /// </summary>
        AnimationSheetExportSettings SheetExportSettings { get; }
        
        /// <summary>
        /// Returns the matching frame at a givne index
        /// </summary>
        IFrame GetFrameAtIndex(int i);
    }
    
    /// <summary>
    /// A basic interface for objects that can provide sequences of individual bitmaps representing frames
    /// for an animation.
    /// </summary>
    public interface IBitmapFrameSequence
    {
        /// <summary>
        /// Gets the width of this frame sequence's frames
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of this frame sequence's frames
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the size of this frame sequence's frames
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Gets the number of frames of this frame sequence
        /// </summary>
        int FrameCount { get; }
        
        /// <summary>
        /// Gets the composed bitmap for a specified frame index
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the composed bitmap of</param>
        /// <returns>The composed bitmap for the frame at the specified index on this frame sequence</returns>
        Bitmap GetComposedBitmapForFrame(int frameIndex);
    }
}
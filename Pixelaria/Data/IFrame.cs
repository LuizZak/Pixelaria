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
using System.Drawing.Drawing2D;

namespace Pixelaria.Data
{
    /// <summary>
    /// Interface to be implemented by any object that can be used as an animation frame
    /// </summary>
    public interface IFrame : IDObject, IEquatable<IFrame>
    {
        /// <summary>
        /// Gets the width of this frame
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of this frame
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the size of this frame
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Gets the index of this frame on the parent animation
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the animation this frame belongs to
        /// </summary>
        Animation Animation { get; set; }

        /// <summary>
        /// Gets the hash of this Frame texture
        /// </summary>
        byte[] Hash { get; }

        /// <summary>
        /// Gets whether this frame has been initialized
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Gets the keyframe metadata for this frame.
        /// </summary>
        KeyframeMetadata KeyframeMetadata { get; }

        /// <summary>
        /// Initializes this frame with the given set of properties
        /// </summary>
        /// <param name="animation">The Animation that will own this frame</param>
        /// <param name="width">The width of the frame</param>
        /// <param name="height">The height of the frame</param>
        /// <param name="initHash">Whether to initialize the frame's hash now</param>
        void Initialize(Animation animation, int width, int height, bool initHash = true);

        /// <summary>
        /// Called when this Frame object is to be removed from an Animation.
        /// This method does not actually remove the frame from the animation, only
        /// removes the reference to the 
        /// </summary>
        void Removed();

        /// <summary>
        /// Called when this Frame object is added to an Animation.
        /// If this frame currently has an animation set, an exception
        /// is thrown
        /// </summary>
        /// <param name="newAnimation">The new animation</param>
        void Added(Animation newAnimation);

        /// <summary>
        /// Clones this Frame and the underlying texture.
        /// Cloning sets the frame lose of the Animation currently owning
        /// this frame, and may not be used before being added to an animation
        /// </summary>
        /// <returns>A clone of this Frame, with a new underlying texture</returns>
        Frame Clone();

        /// <summary>
        /// Copies the frame information from the given Frame object.
        /// This method clones the underlying texture.
        /// If the given frame's dimensions are different from this frame's, while
        /// this frame is placed inside an Animation, an exception is thrown
        /// </summary>
        /// <param name="frame">The frame to copy</param>
        void CopyFrom<TFrame>(TFrame frame) where TFrame : IFrame;

        /// <summary>
        /// Returns whether the current frame can copy the conents of the specified frame type
        /// </summary>
        /// <typeparam name="TFrame">The type of frame to copy from</typeparam>
        bool CanCopyFromType<TFrame>() where TFrame : IFrame;

        /// <summary>
        /// Returns whether this Frame's contents match another frame's
        /// </summary>
        /// <param name="frame">The second frame to test</param>
        /// <returns>Whether this frame's contents match another frame's</returns>
        bool Equals(Frame frame);

        /// <summary>
        /// Returns the memory usage of this frame, in bytes
        /// </summary>
        /// <param name="composed">Whether to calculate the memory usage after the bitmap has been composed into a single image</param>
        /// <returns>Total memory usage, in bytes</returns>
        long CalculateMemoryUsageInBytes(bool composed);

        /// <summary>
        /// Returns the composed Bitmap for this frame
        /// </summary>
        /// <returns>The composed bitmap for this frame</returns>
        Bitmap GetComposedBitmap();
        
        /// <summary>
        /// Resizes this Frame so it matches the given dimensions, scaling with the given scaling method, and interpolating
        /// with the given interpolation mode.
        /// Note that trying to resize a frame while it's inside an animation, and that animation's dimensions don't match
        /// the new size, an exception is thrown.
        /// This method disposes of the current frame texture
        /// </summary>
        /// <param name="newWidth">The new width of this animation</param>
        /// <param name="newHeight">The new height of this animation</param>
        /// <param name="scalingMethod">The scaling method to use to match this frame to the new size</param>
        /// <param name="interpolationMode">The interpolation mode to use when drawing the new frame</param>
        void Resize(int newWidth, int newHeight, PerFrameScalingMethod scalingMethod, InterpolationMode interpolationMode);

        /// <summary>
        /// Updates this Frame's texture's hash
        /// </summary>
        void UpdateHash();

        /// <summary>
        /// Manually set this frame's hash
        /// </summary>
        /// <param name="newHash">The new hash for the frame</param>
        void SetHash(byte[] newHash);
    }
}
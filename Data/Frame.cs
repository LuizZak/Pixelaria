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
using System.Drawing.Imaging;

using Pixelaria.Utils;

namespace Pixelaria.Data
{
    /// <summary>
    /// Describes an animation frame
    /// </summary>
    public class Frame : IDisposable
    {
        /// <summary>
        /// The width of this frame
        /// </summary>
        private int width;

        /// <summary>
        /// The height of this frame
        /// </summary>
        private int height;

        /// <summary>
        /// The animation this frames belongs to
        /// </summary>
        private Animation animation;

        /// <summary>
        /// The texture of this frame
        /// </summary>
        private Bitmap frameTexture;

        /// <summary>
        /// This Frame's texture's hash
        /// </summary>
        private byte[] hash;

        /// <summary>
        /// Gets the width of this frame
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// Gets the height of this frame
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// Gets the index of this frame on the parent animation
        /// </summary>
        public int Index { get { return animation.GetFrameIndex(this); } }

        /// <summary>
        /// Gets the animation this frame belongs to
        /// </summary>
        public Animation Animation { get { return animation; } }

        /// <summary>
        /// Gets the hash of this Frame texture
        /// </summary>
        public byte[] Hash { get { return hash; } }

        /// <summary>
        /// Creates a new animation frame
        /// </summary>
        /// <param name="parentAnimation">The parent animation</param>
        /// <param name="width">The width of this frame</param>
        /// <param name="height">The height of this frame</param>
        /// <param name="initHash">Whether to initialize the frame's hash now</param>
        public Frame(Animation parentAnimation, int width, int height, bool initHash = true)
        {
            this.width = width;
            this.height = height;
            this.animation = parentAnimation;

            frameTexture = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            if (initHash)
            {
                // Update the hash now
                UpdateHash();
            }

            Added(parentAnimation);
        }

        /// <summary>
        /// Disposes of this Frame
        /// </summary>
        public void Dispose()
        {
            animation = null;
            if (frameTexture != null)
                frameTexture.Dispose();
            frameTexture = null;
            hash = null;
        }

        /// <summary>
        /// Called when this Frame object is to be removed from an Animation.
        /// This method does not actually remove the frame from the animation, only
        /// removes the reference to the 
        /// </summary>
        public void Removed()
        {
            animation = null;
        }

        /// <summary>
        /// Called when this Frame object is added to an Animation.
        /// If this frame currently has an animation set, an exception
        /// is thrown
        /// </summary>
        /// <param name="newAnimation">The new animation</param>
        public void Added(Animation newAnimation)
        {
            if (this.animation != null && this.animation != newAnimation)
            {
                throw new Exception("The frame may not be added to another animation before being removed to one prior");
            }

            this.animation = newAnimation;
        }

        /// <summary>
        /// Clones this Frame and the underlying texture.
        /// Cloning sets the frame lose of the Animation currently owning
        /// this frame, and may not be used before being added to an animation
        /// </summary>
        /// <returns>A clone of this Frame, with a new underlying texture</returns>
        public Frame Clone()
        {
            Frame ret = new Frame(null, Width, Height, false);

            ret.frameTexture = frameTexture.Clone(new Rectangle(0, 0, frameTexture.Width, frameTexture.Height), frameTexture.PixelFormat);
            ret.hash = hash;

            return ret;
        }

        /// <summary>
        /// Copies the frame information from the given Frame object.
        /// This method clones the underlying texture.
        /// If the given frame's dimensions are different from this frame's, while
        /// this frame is placed inside an Animation, an exception is thrown
        /// </summary>
        /// <param name="frame">The frame to copy</param>
        public void CopyFrom(Frame frame)
        {
            if (this == frame)
                return;

            if (animation != null && frame.width != this.width && frame.width != animation.Width && frame.height != this.height && frame.height != animation.Height)
            {
                throw new Exception("The dimensions of the frames don't match, the 'copy from' operation cannot be performed.");
            }

            this.width = frame.width;
            this.height = frame.height;
            this.frameTexture = frame.frameTexture.Clone(new Rectangle(0, 0, frame.frameTexture.Width, frame.frameTexture.Height), frame.frameTexture.PixelFormat);

            this.hash = frame.hash;
        }

        /// <summary>
        /// Returns whether this Frame's contents match another frame's
        /// </summary>
        /// <param name="frame">The second frame to test</param>
        /// <returns>Whether this frame's contents match another frame's</returns>
        public bool Equals(Frame frame)
        {
            if (this.width != frame.width || this.height != frame.height)
                return false;

            if (this.hash == null || frame.hash == null)
                return false;

            int l = this.hash.Length;
            for (int i = 0; i < l; i++)
            {
                if (this.hash[i] != frame.hash[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the memory usage of this frame, in bytes
        /// </summary>
        /// <returns>Total memory usage, in bytes</returns>
        public long CalculateMemoryUsageInBytes()
        {
            return Utilities.MemoryUsageOfImage(frameTexture);
        }

        /// <summary>
        /// Swaps the current frame bitmap with the given one. If the new bitmap's dimensions
        /// don't match the Frame's dimensions, an ArgumentException is thrown.
        /// If there's already a Bitmap loaded, the current Bitmap is disposed to save memory.
        /// </summary>
        /// <param name="bitmap">The new frame bitmap</param>
        /// <param name="updateHash">Whether to update the hash after settings the bitmap</param>
        public void SetFrameBitmap(Bitmap bitmap, bool updateHash = true)
        {
            if (frameTexture != null)
                frameTexture.Dispose();

            frameTexture = bitmap;

            if (updateHash)
                UpdateHash();
        }

        /// <summary>
        /// Returns the composed Bitmap for this frame
        /// </summary>
        /// <returns>The composed bitmap for this frame</returns>
        public Bitmap GetComposedBitmap()
        {
            return frameTexture;
        }

        /// <summary>
        /// Generates a Image that represents the thumbnail for this frame using the given size
        /// </summary>
        /// <param name="width">The width of the thumbnail</param>
        /// <param name="height">The height of the thumbnail</param>
        /// <param name="resizeOnSmaller">Whether to resize the thumbnail up if it it's smaller than the thumbnail size</param>
        /// <param name="centered">Whether to center the image on the center of the thumbnail</param>
        /// <param name="backColor">The color to use as a background color</param>
        /// <returns>The thumbnail image</returns>
        public Image GenerateThumbnail(int width, int height, bool resizeOnSmaller, bool centered, Color backColor)
        {
            Image output = new Bitmap(width, height);
            Bitmap composed = GetComposedBitmap();

            Graphics graphics = Graphics.FromImage(output);

			float tx = 0, ty = 0;
            float scaleX = 1, scaleY = 1;

            if (composed.Width >= composed.Height)
            {
                if (width < composed.Width || resizeOnSmaller)
                {
                    scaleX = (float)width / composed.Width;
                    scaleY = scaleX;
                }
                else
                {
                    tx = (float)height / 2 - ((float)composed.Width * scaleX / 2);
                }

                ty = (float)width / 2 - ((float)composed.Height * scaleY / 2);
            }
            else
            {
                if (height < composed.Height || resizeOnSmaller)
                {
                    scaleY = (float)height / composed.Height;
                    scaleX = scaleY;
                }
                else
                {
                    ty = (float)width / 2 - ((float)composed.Height * scaleY / 2);
                }

                tx = (float)height / 2 - ((float)composed.Width * scaleX / 2);
            }

            if (!centered)
            {
                tx = ty = 0;
            }

            RectangleF area = new RectangleF((float)Math.Round(tx), (float)Math.Round(ty), (float)Math.Round(composed.Width * scaleX), (float)Math.Round(composed.Height * scaleY));

            graphics.Clear(backColor);

            graphics.DrawImage(composed, area);

            graphics.Flush();
            graphics.Dispose();

            return output;
        }

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
        public void Resize(int newWidth, int newHeight, PerFrameScalingMethod scalingMethod, InterpolationMode interpolationMode)
        {
            if (animation != null && (animation.Width != newWidth || animation.Height != newHeight))
            {
                throw new Exception("The dimensions of the Animation that owns this frame don't match the given new dimensions.");
            }

            if(this.width == newWidth && this.height == newHeight)
                return;

            Rectangle currentBounds = new Rectangle(0, 0, width, height);
            Rectangle newBounds = new Rectangle(0, 0, newWidth, newHeight);
            
            // New bounds calculation
            if (scalingMethod == PerFrameScalingMethod.PlaceAtTopLeft)
            {
                newBounds = currentBounds;
            }
            else if (scalingMethod == PerFrameScalingMethod.PlaceAtCenter)
            {
                // Center the sprite
                currentBounds.X = newBounds.Width / 2 - currentBounds.Width / 2;
                currentBounds.Y = newBounds.Height / 2 - currentBounds.Height / 2;

                newBounds = currentBounds;
            }

            // New texture creation
            Bitmap newTexture = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            
            Graphics graphics = Graphics.FromImage(newTexture);

            graphics.InterpolationMode = interpolationMode;
            graphics.DrawImage(frameTexture, newBounds);

            graphics.Flush();
            graphics.Dispose();

            // Texture replacement
            frameTexture.Dispose();
            frameTexture = newTexture;

            this.width = newWidth;
            this.height = newHeight;

            // Update hash
            UpdateHash();
        }

        /// <summary>
        /// Updates this Frame's texture's hash
        /// </summary>
        public void UpdateHash()
        {
            hash = Utilities.GetHashForBitmap(frameTexture);
        }

        /// <summary>
        /// Manually set this frame's hash
        /// </summary>
        /// <param name="newHash">The new hash for the frame</param>
        public void SetHash(byte[] newHash)
        {
            this.hash = newHash;
        }
    }
}
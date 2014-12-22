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
    public class Frame : IFrame
    {
        /// <summary>
        /// Whether this frame has been initialized
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// The width of this frame
        /// </summary>
        private int _width;

        /// <summary>
        /// The height of this frame
        /// </summary>
        private int _height;

        /// <summary>
        /// The animation this frames belongs to
        /// </summary>
        private Animation _animation;

        /// <summary>
        /// The texture of this frame
        /// </summary>
        private Bitmap _frameTexture;

        /// <summary>
        /// This Frame's texture's hash
        /// </summary>
        private byte[] _hash;

        /// <summary>
        /// The unique identifier for this frame in the whole bundle
        /// </summary>
        private int _id;

        /// <summary>
        /// Gets the width of this frame
        /// </summary>
        public int Width { get { return _width; } }

        /// <summary>
        /// Gets the height of this frame
        /// </summary>
        public int Height { get { return _height; } }

        /// <summary>
        /// Gets the size of this animation's frames
        /// </summary>
        public Size Size { get { return new Size(_width, _height); } }

        /// <summary>
        /// Gets the index of this frame on the parent animation
        /// </summary>
        public int Index { get { return _animation.GetFrameIndex(this); } }

        /// <summary>
        /// Gets the animation this frame belongs to
        /// </summary>
        public Animation Animation { get { return _animation; } }

        /// <summary>
        /// Gets the hash of this Frame texture
        /// </summary>
        public byte[] Hash { get { return _hash; } }

        /// <summary>
        /// Gets or sets the ID of this frame
        /// </summary>
        public int ID { get { return _id; } set { _id = value; } }

        /// <summary>
        /// Gets whether this frame has been initialized
        /// </summary>
        public bool Initialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// Creates a new instance of the Frame class
        /// </summary>
        public Frame()
        {
            
        }

        /// <summary>
        /// Creates a new animation frame
        /// </summary>
        /// <param name="parentAnimation">The parent animation</param>
        /// <param name="width">The width of this frame</param>
        /// <param name="height">The height of this frame</param>
        /// <param name="initHash">Whether to initialize the frame's hash now</param>
        public Frame(Animation parentAnimation, int width, int height, bool initHash = true)
        {
            Initialize(parentAnimation, width, height, initHash);
        }

        /// <summary>
        /// Creates a new animation frame
        /// </summary>
        /// <param name="animation">The parent animation</param>
        /// <param name="width">The width of this frame</param>
        /// <param name="height">The height of this frame</param>
        /// <param name="initHash">Whether to initialize the frame's hash now</param>
        /// <exception cref="InvalidOperationException">The Initialize funcion was already called</exception>
        public void Initialize(Animation animation, int width, int height, bool initHash = true)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Calling Initialize() on an already initialized frame");
            }

            _initialized = true;

            _id = -1;
            _width = width;
            _height = height;
            _animation = animation;

            _frameTexture = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            if (initHash)
            {
                // Update the hash now
                UpdateHash();
            }

            Added(animation);
        }

        /// <summary>
        /// Disposes of this Frame
        /// </summary>
        public void Dispose()
        {
            _animation = null;
            if (_frameTexture != null)
                _frameTexture.Dispose();
            _frameTexture = null;
            _hash = null;
        }

        /// <summary>
        /// Called when this Frame object is to be removed from an Animation.
        /// This method does not actually remove the frame from the animation, only
        /// removes the reference to the 
        /// </summary>
        public void Removed()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            _animation = null;
        }

        /// <summary>
        /// Called when this Frame object is added to an Animation.
        /// If this frame currently has an animation set, an exception
        /// is thrown
        /// </summary>
        /// <param name="newAnimation">The new animation</param>
        public void Added(Animation newAnimation)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (_animation != null && !ReferenceEquals(_animation, newAnimation))
            {
                throw new InvalidOperationException("The frame may not be added to another animation before being removed from the current one before");
            }

            _animation = newAnimation;
        }

        /// <summary>
        /// Clones this Frame and the underlying texture.
        /// Cloning sets the frame lose of the Animation currently owning
        /// this frame, and may not be used before being added to an animation
        /// </summary>
        /// <returns>A clone of this Frame, with a new underlying texture</returns>
        public Frame Clone()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            Frame ret = new Frame(null, Width, Height, false);
            
            ret.CopyFrom(this);

            return ret;
        }

        /// <summary>
        /// Copies the frame information from the given Frame object.
        /// This method clones the underlying texture.
        /// If the given frame's dimensions are different from this frame's, while
        /// this frame is placed inside an Animation, an exception is thrown
        /// </summary>
        /// <param name="frame">The frame to copy</param>
        public void CopyFrom<TFrame>(TFrame frame) where TFrame : IFrame
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (ReferenceEquals(this, frame))
                return;

            if (_animation != null && frame.Width != _animation.Width && frame.Height != _animation.Height)
            {
                throw new InvalidOperationException("The dimensions of the frames don't match, the 'copy from' operation cannot be performed.");
            }

            Bitmap frameTexture = frame.GetComposedBitmap();

            _width = frame.Width;
            _height = frame.Height;
            _frameTexture = frameTexture.Clone(new Rectangle(0, 0, frameTexture.Width, frameTexture.Height), frameTexture.PixelFormat);

            _hash = frame.Hash;
        }

        /// <summary>
        /// Returns whether this Frame's contents match another frame's
        /// </summary>
        /// <param name="frame">The second frame to test</param>
        /// <returns>Whether this frame's contents match another frame's</returns>
        public bool Equals(Frame frame)
        {
            if (_width != frame._width || _height != frame._height)
                return false;

            if (_hash == null || frame._hash == null)
                return false;

            int l = _hash.Length;
            for (int i = 0; i < l; i++)
            {
                if (_hash[i] != frame._hash[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether this Frame's contents match another frame's
        /// </summary>
        /// <param name="frame">The second frame to test</param>
        /// <returns>Whether this frame's contents match another frame's</returns>
        public bool Equals(IFrame frame)
        {
            var frameCasted = frame as Frame;
            if (frameCasted != null)
                return Equals(frameCasted);

            return false;
        }

        /// <summary>
        /// Returns the memory usage of this frame, in bytes
        /// </summary>
        /// <returns>Total memory usage, in bytes</returns>
        public long CalculateMemoryUsageInBytes()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            return Utilities.MemoryUsageOfImage(_frameTexture);
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
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (bitmap != _frameTexture)
            {
                if (_frameTexture != null)
                    _frameTexture.Dispose();

                _frameTexture = bitmap;
            }

            if (updateHash)
                UpdateHash();
        }

        /// <summary>
        /// Returns the composed Bitmap for this frame
        /// </summary>
        /// <returns>The composed bitmap for this frame</returns>
        public Bitmap GetComposedBitmap()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            return _frameTexture;
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
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

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
                    tx = (float)height / 2 - (composed.Width * scaleX / 2);
                }

                ty = (float)width / 2 - (composed.Height * scaleY / 2);
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
                    ty = (float)width / 2 - (composed.Height * scaleY / 2);
                }

                tx = (float)height / 2 - (composed.Width * scaleX / 2);
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
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (_animation != null && (_animation.Width != newWidth || _animation.Height != newHeight))
            {
                throw new Exception("The dimensions of the Animation that owns this frame don't match the given new dimensions.");
            }

            if(_width == newWidth && _height == newHeight)
                return;

            Bitmap newTexture = (Bitmap)ImageUtilities.Resize(_frameTexture, newWidth, newHeight, scalingMethod, interpolationMode);

            // Texture replacement
            _frameTexture.Dispose();
            _frameTexture = newTexture;

            _width = newWidth;
            _height = newHeight;

            // Update hash
            UpdateHash();
        }

        /// <summary>
        /// Updates this Frame's texture's hash
        /// </summary>
        public void UpdateHash()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            _hash = Utilities.GetHashForBitmap(_frameTexture);
        }

        /// <summary>
        /// Manually set this frame's hash
        /// </summary>
        /// <param name="newHash">The new hash for the frame</param>
        public void SetHash(byte[] newHash)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            _hash = newHash;
        }

        // Override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
                return true;

            Frame other = (Frame) obj;

            return _hash != null && other._hash != null && Utilities.ByteArrayCompare(_hash, other._hash) && _width == other._width &&
                   _height == other._height && _frameTexture != null && other._frameTexture != null;
        }

        // Override object.GetHashCode
        public override int GetHashCode()
        {
            return _width ^ _height ^ _id;
        }
    }
}
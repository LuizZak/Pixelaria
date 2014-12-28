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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
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
        /// The list of layers laid on this frame
        /// </summary>
        private readonly List<FrameLayer> _layers;

        /// <summary>
        /// The animation this frames belongs to
        /// </summary>
        private Animation _animation;

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
        /// Gets the total number of layers stored on this Frame object
        /// </summary>
        public int LayerCount
        {
            get { return _layers.Count; }
        }

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
            _layers = new List<FrameLayer>();
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
            _layers = new List<FrameLayer>();
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

            CreateLayer(0);

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

            ClearLayers();

            _hash = null;
        }

        /// <summary>
        /// Clears all the layers stored on this Frame object
        /// </summary>
        private void ClearLayers()
        {
            foreach (var layer in _layers)
            {
                layer.Dispose();
            }
            _layers.Clear();
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
        /// <exception cref="InvalidOperationException">The frame is not initialized</exception>
        /// <exception cref="InvalidOperationException">The frame is hosted inside an animation and the dimensions of the frames don't match</exception>
        /// <exception cref="InvalidOperationException">The frame's type is not copyable to this type. Use CanCopyFromType&lt;&gt;() to verify type compatibility</exception>
        public void CopyFrom<TFrame>(TFrame frame) where TFrame : IFrame
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if(!CanCopyFromType<TFrame>())
                throw new InvalidOperationException("The provided frame's type is not copyable to this frame's type. Use CanCopyFromType<>() to verify compatibility fist");

            if (ReferenceEquals(this, frame))
                return;

            if (_animation != null && frame.Width != _animation.Width && frame.Height != _animation.Height)
            {
                throw new InvalidOperationException("The dimensions of the frames don't match, the 'copy from' operation cannot be performed.");
            }

            Frame castFrame = frame as Frame;

            if (castFrame == null)
                return;

            // TODO: Deal with layering in the copy operation
            _width = frame.Width;
            _height = frame.Height;

            // Clear the current layers and clone from the passed frame
            ClearLayers();

            _layers.AddRange(castFrame._layers.Select(t => t.Clone()));

            // Update the indices of the layers
            UpdateLayerIndices();

            _hash = frame.Hash;
        }

        /// <summary>
        /// Returns whether the current frame can copy the conents of the specified frame type
        /// </summary>
        /// <typeparam name="TFrame">The type of frame to copy from</typeparam>
        public virtual bool CanCopyFromType<TFrame>() where TFrame : IFrame
        {
            return typeof(TFrame).IsAssignableFrom(typeof(Frame));
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
        /// Creates a new empty layer on this Frame
        /// </summary>
        /// <param name="layerIndex">The index to add the layer at. Leave -1 to add to the end of the layer list</param>
        public IFrameLayer CreateLayer(int layerIndex = -1)
        {
            FrameLayer layer = new FrameLayer(new Bitmap(_width, _height, PixelFormat.Format32bppArgb))
            {
                Index = layerIndex == -1 ? _layers.Count : layerIndex
            };

            if (layerIndex == -1)
                _layers.Add(layer);
            else
                _layers.Insert(layerIndex, layer);

            UpdateLayerIndices();

            return layer;
        }

        /// <summary>
        /// Adds a layer on this Frame object based on the specified bitmap.
        /// If the bitmap does not match this frame's dimensions or its pixel format is not 32bpp, an exception is raised
        /// </summary>
        /// <param name="bitmap">The bitmap to use as a layer image</param>
        /// <param name="layerIndex">The index to add the layer at. Leave -1 to add to the end of the layer list</param>
        /// <returns>The layer that was just created</returns>
        /// <exception cref="ArgumentException">The provided bitmap's dimensions does not match this Frame's dimensions, or its pixel format isn't 32bpp</exception>
        public IFrameLayer AddLayer(Bitmap bitmap, int layerIndex = -1)
        {
            if (bitmap.Width != _width || bitmap.Height != _height || Image.GetPixelFormatSize(bitmap.PixelFormat) != 32)
            {
                throw new ArgumentException(@"The provided bitmap's dimensions must match the size of this frame and its pixel format must be a 32bpp variant", "bitmap");
            }

            FrameLayer layer = (FrameLayer)CreateLayer(layerIndex);

            layer.CopyFromBitmap(bitmap);

            return layer;
        }

        /// <summary>
        /// Removes a layer that is stored on the specified index on this Frame
        /// </summary>
        /// <param name="layerIndex">The layer index to remove</param>
        public void RemoveLayer(int layerIndex)
        {
            _layers[layerIndex].Dispose();
            _layers.RemoveAt(layerIndex);

            if (_layers.Count == 0)
                CreateLayer();

            UpdateLayerIndices();
        }

        /// <summary>
        /// Gets a layer at the specified index on this Frame object
        /// </summary>
        /// <param name="index">The index to get the layer at</param>
        /// <returns>A layer at the specified index on this Frame object</returns>
        public IFrameLayer GetLayerAt(int index)
        {
            return _layers[index];
        }

        /// <summary>
        /// Swaps layers at the two specified indices
        /// </summary>
        /// <param name="firstIndex">The first layer index to swap</param>
        /// <param name="secondIndex">The second layer index to swap</param>
        /// <param name="updateHash">Whether to update the frame's hash after the operation</param>
        public void SwapLayers(int firstIndex, int secondIndex, bool updateHash = true)
        {
            FrameLayer secondLayer = _layers[secondIndex];
            _layers[secondIndex] = _layers[firstIndex];
            _layers[firstIndex] = secondLayer;

            UpdateLayerIndices();

            if (updateHash)
            {
                UpdateHash();
            }
        }

        /// <summary>
        /// Updates the bitmap of a given layer. If the dimensions of the bitmap don't match 32bpp, an exception is raised
        /// </summary>
        /// <param name="layerIndex">The index of the layer to update</param>
        /// <param name="layerBitmap">The new layer bitmap</param>
        /// <param name="updateHash">Whether to update the frame's hash after the operation</param>
        /// <exception cref="ArgumentException">The dimensions of the bitmap don't match this frame's size, or its pixel format isn't 32bpp</exception>
        public void SetLayerBitmap(int layerIndex, Bitmap layerBitmap, bool updateHash = true)
        {
            if (layerBitmap.Width != _width || layerBitmap.Height != _height || Image.GetPixelFormatSize(layerBitmap.PixelFormat) != 32)
            {
                throw new ArgumentException(@"The provided bitmap's dimensions must match the size of this frame and its pixel format must be a 32bpp variant", "bitmap");
            }

            _layers[layerIndex].LayerBitmap = layerBitmap;

            if (updateHash)
            {
                UpdateHash();
            }
        }

        /// <summary>
        /// Updates the indices for all the layers stored on this frame
        /// </summary>
        private void UpdateLayerIndices()
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                _layers[i].Index = i;
            }
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

            // Copy to the first layer
            _layers[0].LayerBitmap.Dispose();
            _layers[0].LayerBitmap = bitmap;

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
                throw new InvalidOperationException(@"The frame was not initialized prior to this action");
            }

            Bitmap composedBitmap = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
            FastBitmap.CopyPixels(_layers[0].LayerBitmap, composedBitmap);

            FastBitmap fastBitmap = composedBitmap.FastLock();

            // Compose the layers by blending all the pixels from each layer into the final image
            for(int i = 1; i < _layers.Count; i++)
            {
                IFrameLayer layer = _layers[i];
                using (FastBitmap fastLayer = layer.LayerBitmap.FastLock())
                {
                    for (int y = 0; y < _height; y++)
                    {
                        for (int x = 0; x < _width; x++)
                        {
                            Color blendedColor = Utilities.FlattenColor(fastBitmap.GetPixel(x, y), fastLayer.GetPixel(x, y));

                            fastBitmap.SetPixel(x, y, blendedColor);
                        }
                    }
                }
            }

            fastBitmap.Unlock();

            /*using(Graphics gfx = Graphics.FromImage(composedBitmap))
            {
                gfx.Clear(Color.FromArgb(0, 0, 0, 0));

                gfx.CompositingMode = CompositingMode.SourceOver;
                gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
                gfx.CompositingQuality = CompositingQuality.HighQuality;

                foreach (var layer in _layers)
                {
                    gfx.DrawImageUnscaled(layer.LayerBitmap, 0, 0);
                }

                gfx.Flush();
            }*/

            return composedBitmap;
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

            composed.Dispose();

            return output;
        }

        /// <summary>
        /// Resizes this Frame so it matches the given dimensions, scaling with the given scaling method, and interpolating with the given interpolation mode.
        /// Note that trying to resize a frame while it's inside an animation, and that animation's dimensions don't match the new size, an exception is thrown.
        /// This method disposes of the current frame texture
        /// </summary>
        /// <param name="newWidth">The new width for this frame</param>
        /// <param name="newHeight">The new height for this frame </param>
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

            //Bitmap newTexture = (Bitmap)ImageUtilities.Resize(_frameTexture, newWidth, newHeight, scalingMethod, interpolationMode);

            // Texture replacement
            //_frameTexture.Dispose();
            //_frameTexture = newTexture;

            _width = newWidth;
            _height = newHeight;

            foreach (var layer in _layers)
            {
                layer.Resize(newWidth, newHeight, scalingMethod, interpolationMode);
            }

            // Update hash
            UpdateHash();
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

            // Calculate the usage of each layer individually
            return _layers.Sum(layer => Utilities.MemoryUsageOfImage(layer.LayerBitmap));
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

            using (var bitmap = GetComposedBitmap())
            {
                _hash = Utilities.GetHashForBitmap(bitmap);
            }
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

            if (_layers.Count != other._layers.Count || _hash == null || other._hash == null || !Utilities.ByteArrayCompare(_hash, other._hash) ||
                _width != other._width || _height != other._height)
            {
                return false;
            }

            // Compare each layer individually
            for (int i = 0; i < _layers.Count; i++)
            {
                if (!_layers[i].Equals(other._layers[i]))
                    return false;
            }

            return true;
        }

        // Override object.GetHashCode
        public override int GetHashCode()
        {
            return _width ^ _height ^ _id;
        }

        /// <summary>
        /// Represents the layer for a frame
        /// </summary>
        protected class FrameLayer : IFrameLayer, IDisposable, IEquatable<FrameLayer>
        {
            /// <summary>
            /// The bitmap for this layer
            /// </summary>
            private Bitmap _layerBitmap;

            /// <summary>
            /// Gets this layer's width
            /// </summary>
            public int Width
            {
                get { return _layerBitmap.Width; }
            }

            /// <summary>
            /// Gets this layer's height
            /// </summary>
            public int Height
            {
                get { return _layerBitmap.Height; }
            }

            /// <summary>
            /// Gets the index of this layer on the origin frame
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Gets this layer's bitmap content
            /// </summary>
            public Bitmap LayerBitmap
            {
                get { return _layerBitmap; }
                set { _layerBitmap = value; }
            }

            /// <summary>
            /// Initializes a new instance of the FrameLayer class, with a bitmap to bind to this layer
            /// </summary>
            /// <param name="layerBitmap">The bitmap to bind to this layer</param>
            public FrameLayer(Bitmap layerBitmap)
            {
                _layerBitmap = layerBitmap;
            }

            /// <summary>
            /// Clones this frame layer object
            /// </summary>
            /// <returns>A clone of this frame layer's object</returns>
            public FrameLayer Clone()
            {
                FrameLayer layer = new FrameLayer(new Bitmap(Width, Height, _layerBitmap.PixelFormat));

                layer.CopyFromBitmap(_layerBitmap);

                return layer;
            }
            
            /// <summary>
            /// Destructor the FrameLayer class
            /// </summary>
            ~FrameLayer()
            {
                Dispose(false);
            }

            #region IDisposable Members

            /// <summary>
            /// Internal variable which checks if Dispose has already been called
            /// </summary>
            private Boolean _disposed;

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            private void Dispose(Boolean disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if(_layerBitmap != null)
                    _layerBitmap.Dispose();

                _disposed = true;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                // Call the private Dispose(bool) helper and indicate 
                // that we are explicitly disposing
                Dispose(true);

                // Tell the garbage collector that the object doesn't require any
                // cleanup when collected since Dispose was called explicitly.
                GC.SuppressFinalize(this);
            }

            #endregion

            /// <summary>
            /// Resizes this Layer so it matches the given dimensions, scaling with the given scaling method, and interpolating with the given interpolation mode.
            /// This method disposes of the current layer texture
            /// </summary>
            /// <param name="newWidth">The new width for this layer</param>
            /// <param name="newHeight">The new height for this layer</param>
            /// <param name="scalingMethod">The scaling method to use to match this layer to the new size</param>
            /// <param name="interpolationMode">The interpolation mode to use when drawing the new layer</param>
            public void Resize(int newWidth, int newHeight, PerFrameScalingMethod scalingMethod, InterpolationMode interpolationMode)
            {
                if (Width == newWidth && Height == newHeight)
                    return;

                Bitmap newTexture = (Bitmap)ImageUtilities.Resize(_layerBitmap, newWidth, newHeight, scalingMethod, interpolationMode);

                // Texture replacement
                _layerBitmap.Dispose();
                _layerBitmap = newTexture;
            }

            /// <summary>
            /// Copies this layer's contents from the given bitmap.
            /// If the layer's dimensions don't match the passed bitmap's dimensions, an ArgumentException is raised
            /// </summary>
            /// <param name="bitmap">The bitmap to copy to this layer</param>
            /// <exception cref="ArgumentException">The bitmap's dimensions don't match this layer's dimensions</exception>
            public void CopyFromBitmap(Bitmap bitmap)
            {
                if (bitmap.Width != _layerBitmap.Width || bitmap.Height != _layerBitmap.Height)
                {
                    throw new ArgumentException(@"The provided bitmap's dimensions don't match this bitmap's dimensions", "bitmap");
                }

                // Copy the pixels
                FastBitmap.CopyPixels(bitmap, _layerBitmap);
            }

            #region Equality members

            /// <summary>
            /// Returns whether this FrameLayer is equal to another FrameLayer
            /// </summary>
            /// <param name="other">The other FrameLayer to test</param>
            /// <returns>Whether this FrameLayer is equal to another FrameLayer</returns>
            public bool Equals(FrameLayer other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Utilities.ImagesAreIdentical(_layerBitmap, other._layerBitmap) && Index == other.Index;
            }

            /// <summary>
            /// Returns whether this FrameLayer equals to the provided object
            /// </summary>
            /// <param name="obj">The object to compare to this FrameLayer</param>
            /// <returns>Whether this FrameLayer equals to the provided object</returns>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((FrameLayer)obj);
            }

            /// <summary>
            /// Gets the hash code for this FrameLayer.
            /// The hash code is computed from the underlying layer bitmap
            /// </summary>
            /// <returns>The hash code for this FrameLayer</returns>
            public override int GetHashCode()
            {
                return (_layerBitmap != null ? _layerBitmap.GetHashCode() : 0) ^ (Index * 367);
            }

            /// <summary>
            /// Equality operator for the FrameLayer class
            /// </summary>
            /// <param name="left">The first layer to compare</param>
            /// <param name="right">The second layer to compare</param>
            /// <returns>Whether the layers are equal</returns>
            public static bool operator==(FrameLayer left, FrameLayer right)
            {
                return Equals(left, right);
            }

            /// <summary>
            /// Inequality operator for the FrameLayer class
            /// </summary>
            /// <param name="left">The first layer to compare</param>
            /// <param name="right">The second layer to compare</param>
            /// <returns>Whether the layers are unequal</returns>
            public static bool operator!=(FrameLayer left, FrameLayer right)
            {
                return !Equals(left, right);
            }

            #endregion
        }
    }

    /// <summary>
    /// Interface to be implemented by frame layers 
    /// </summary>
    public interface IFrameLayer
    {
        /// <summary>
        /// Gets this layer's width
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets this layer's height
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the index of this layer on the origin frame
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets this layer's bitmap content
        /// </summary>
        Bitmap LayerBitmap { get; }
    }
}
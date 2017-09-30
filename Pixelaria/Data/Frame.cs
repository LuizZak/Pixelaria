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
using FastBitmapLib;
using Pixelaria.Utils;

namespace Pixelaria.Data
{
    /// <summary>
    /// Describes an animation frame
    /// </summary>
    public class Frame : IFrame
    {
        /// <summary>
        /// The list of layers laid on this frame
        /// </summary>
        private readonly List<FrameLayer> _layers;

        /// <summary>
        /// Gets the width of this frame
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of this frame
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the size of this animation's frames
        /// </summary>
        public Size Size => new Size(Width, Height);

        /// <summary>
        /// Gets the total number of layers stored on this Frame object
        /// </summary>
        public int LayerCount => _layers.Count;

        /// <summary>
        /// Gets the index of this frame on the parent animation
        /// </summary>
        public int Index => Animation.GetFrameIndex(this);

        /// <summary>
        /// Gets the animation this frame belongs to
        /// </summary>
        public Animation Animation { get; private set; }

        /// <summary>
        /// Gets the hash of this Frame texture
        /// </summary>
        public byte[] Hash { get; private set; }

        /// <summary>
        /// A short hash of the hash value above - used for faster drops of unequal frames
        /// </summary>
        private long _shortHash;

        /// <summary>
        /// Gets or sets the ID of this frame
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets whether this frame has been initialized
        /// </summary>
        public bool Initialized { get; private set; }

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
            if (Initialized)
            {
                throw new InvalidOperationException("Calling Initialize() on an already initialized frame");
            }

            Initialized = true;

            ID = -1;
            Width = width;
            Height = height;
            Animation = animation;

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
            Animation = null;

            ClearLayers();

            Hash = null;
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
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            Animation = null;
        }

        /// <summary>
        /// Called when this Frame object is added to an Animation.
        /// If this frame currently has an animation set, an exception
        /// is thrown
        /// </summary>
        /// <param name="newAnimation">The new animation</param>
        public void Added(Animation newAnimation)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (Animation != null && !ReferenceEquals(Animation, newAnimation))
            {
                throw new InvalidOperationException("The frame may not be added to another animation before being removed from the current one before");
            }

            Animation = newAnimation;
        }

        /// <summary>
        /// Clones this Frame and the underlying texture.
        /// Cloning sets the frame lose of the Animation currently owning
        /// this frame, and may not be used before being added to an animation
        /// </summary>
        /// <returns>A clone of this Frame, with a new underlying texture</returns>
        public Frame Clone()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            var ret = new Frame(null, Width, Height, false);
            
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
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if(!CanCopyFromType<TFrame>())
                throw new InvalidOperationException("The provided frame's type is not copyable to this frame's type. Use CanCopyFromType<>() to verify compatibility fist");

            if (ReferenceEquals(this, frame))
                return;

            if (Animation != null && frame.Width != Animation.Width && frame.Height != Animation.Height)
            {
                throw new InvalidOperationException("The dimensions of the frames don't match, the 'copy from' operation cannot be performed.");
            }

            var castFrame = frame as Frame;

            if (castFrame == null)
                return;

            Width = frame.Width;
            Height = frame.Height;

            // Clear the current layers and clone from the passed frame
            ClearLayers();
            _layers.AddRange(castFrame._layers.Select(t => t.Clone() as FrameLayer));

            // Update the indices of the layers
            UpdateLayerIndices();

            Hash = frame.Hash;
            _shortHash = castFrame._shortHash;
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
            if (Width != frame.Width || Height != frame.Height)
                return false;

            if (Hash == null || frame.Hash == null)
                return false;
            
            if (_shortHash != frame._shortHash) // Check short hash beforehands
                return false;
            
            return Utilities.memcmp(Hash, frame.Hash, Hash.Length) == 0;
        }

        /// <summary>
        /// Returns whether this Frame's contents match another frame's
        /// </summary>
        /// <param name="frame">The second frame to test</param>
        /// <returns>Whether this frame's contents match another frame's</returns>
        public bool Equals(IFrame frame)
        {
            if (frame is Frame frameCasted)
                return Equals(frameCasted);

            return false;
        }

        /// <summary>
        /// Creates a new empty layer on this Frame
        /// </summary>
        /// <param name="layerIndex">The index to add the layer at. Leave -1 to add to the end of the layer list</param>
        public IFrameLayer CreateLayer(int layerIndex = -1)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            FrameLayer layer = new FrameLayer(new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                Index = layerIndex == -1 ? _layers.Count : layerIndex
            };

            AddLayer(layer, layerIndex);

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
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (bitmap.Width != Width || bitmap.Height != Height || Image.GetPixelFormatSize(bitmap.PixelFormat) != 32)
            {
                throw new ArgumentException(@"The provided bitmap's dimensions must match the size of this frame and its pixel format must be a 32bpp variant", nameof(bitmap));
            }

            var layer = (FrameLayer)CreateLayer(layerIndex);

            layer.CopyFromBitmap(bitmap);

            return layer;
        }

        /// <summary>
        /// Adds the specified layer to this Frame object.
        /// If the layer's size does not match this frame's dimensions, an exception is raised.
        /// If the layer's type does not match the internal layer type (or, the layer does not originates from a CreateLayer/AddLayer/GetLayerAt from this object), an exception is raised
        /// </summary>
        /// <param name="layer">The layer to add to this frame</param>
        /// <param name="layerIndex">The index at which to add the layer</param>
        /// <exception cref="ArgumentException">
        /// The provided layers's dimensions does not match this Frame's dimensions, or the provided layers's type
        /// is not compatible with this Frame object, or the provided layer is already stored in a Frame object
        /// </exception>
        public void AddLayer(IFrameLayer layer, int layerIndex = -1)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (layer.Width != Width || layer.Height != Height)
            {
                throw new ArgumentException(@"The provided layer's dimensions must match the size of this frame", nameof(layer));
            }
            if (layer.Frame != null)
            {
                throw new ArgumentException("The specified layer is already stored in a Frame object");
            }

            var frameLayer = layer as FrameLayer;
            if (frameLayer == null)
            {
                throw new ArgumentException("The provided layers's type is not compatible with this Frame object");
            }

            if (layerIndex == -1 || layerIndex == _layers.Count)
                _layers.Add(frameLayer);
            else
                _layers.Insert(layerIndex, frameLayer);

            frameLayer.Frame = this;

            UpdateLayerIndices();
        }

        /// <summary>
        /// Removes a layer that is stored on the specified index on this Frame
        /// </summary>
        /// <param name="layerIndex">The layer index to remove</param>
        /// <param name="dispose">Whether to dispose of the layer after removing it</param>
        public void RemoveLayerAt(int layerIndex, bool dispose = true)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if(dispose)
            {
                _layers[layerIndex].Dispose();
            }

            _layers[layerIndex].Frame = null;

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
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

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
            var secondLayer = _layers[secondIndex];
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
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (layerBitmap.Width != Width || layerBitmap.Height != Height || Image.GetPixelFormatSize(layerBitmap.PixelFormat) != 32)
            {
                throw new ArgumentException(@"The provided bitmap's dimensions must match the size of this frame and its pixel format must be a 32bpp variant", nameof(layerBitmap));
            }

            //_layers[layerIndex].LayerBitmap = layerBitmap;
            _layers[layerIndex].CopyFromBitmap(layerBitmap);

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
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            // Copy to the first layer
            //_layers[0].CopyFromBitmap(bitmap);
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
            if (!Initialized)
            {
                throw new InvalidOperationException(@"The frame was not initialized prior to this action");
            }

            Bitmap composedBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            FastBitmap.CopyPixels(_layers[0].LayerBitmap, composedBitmap);

            // Compose the layers by blending all the pixels from each layer into the final image
            for (int i = 1; i < _layers.Count; i++)
            {
                ImageUtilities.FlattenBitmaps(composedBitmap, _layers[i].LayerBitmap, true);
            }
            
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
            if (!Initialized)
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
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (Animation != null && (Animation.Width != newWidth || Animation.Height != newHeight))
            {
                throw new Exception("The dimensions of the Animation that owns this frame don't match the given new dimensions.");
            }

            if(Width == newWidth && Height == newHeight)
                return;

            Width = newWidth;
            Height = newHeight;

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
        /// <param name="composed">Whether to calculte the memory usage after the frame has been composed into a single image</param>
        /// <returns>Total memory usage, in bytes</returns>
        public long CalculateMemoryUsageInBytes(bool composed)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            // For composed mode, use the memory usage of the first layer
            if (composed)
                return ImageUtilities.MemoryUsageOfImage(_layers[0].LayerBitmap);

            // Calculate the usage of each layer individually
            return _layers.Sum(layer => ImageUtilities.MemoryUsageOfImage(layer.LayerBitmap));
        }

        /// <summary>
        /// Updates this Frame's texture's hash
        /// </summary>
        public void UpdateHash()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            using (var bitmap = GetComposedBitmap())
            {
                SetHash(ImageUtilities.GetHashForBitmap(bitmap));
            }
        }

        /// <summary>
        /// Manually set this frame's hash
        /// </summary>
        /// <param name="newHash">The new hash for the frame</param>
        public void SetHash(byte[] newHash)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            Hash = newHash;

            // Calculate short hash
            unchecked
            {
                var hashCode = Hash[0].GetHashCode();

                for (int i = 1; i < Hash.Length; i++)
                {
                    hashCode = (hashCode * 397) ^ Hash[i].GetHashCode();
                }

                _shortHash = hashCode;
            }
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

            if (_layers.Count != other._layers.Count || Hash == null || other.Hash == null || !Utilities.ByteArrayCompare(Hash, other.Hash) ||
                Width != other.Width || Height != other.Height)
            {
                return false;
            }

            // Compare each layer individually
            // Disable LINQ suggestion because it'd actually be considerably slower
            // ReSharper disable once LoopCanBeConvertedToQuery
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
            return Width ^ Height ^ ID;
        }

        /// <summary>
        /// Represents the layer for a frame
        /// </summary>
        protected class FrameLayer : IFrameLayer, IEquatable<FrameLayer>
        {
            /// <summary>
            /// Gets this layer's width
            /// </summary>
            public int Width => LayerBitmap.Width;

            /// <summary>
            /// Gets this layer's height
            /// </summary>
            public int Height => LayerBitmap.Height;

            /// <summary>
            /// Gets the size of this layer
            /// </summary>
            public Size Size => new Size(Width, Height);

            /// <summary>
            /// Gets the index of this layer on the origin frame
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the name for this layer
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets this layer's bitmap content
            /// </summary>
            public Bitmap LayerBitmap { get; set; }

            /// <summary>
            /// Gets the frame that owns this IFrameLayer object
            /// </summary>
            public Frame Frame { get; set; }

            /// <summary>
            /// Initializes a new instance of the FrameLayer class, with a bitmap to bind to this layer
            /// </summary>
            /// <param name="layerBitmap">The bitmap to bind to this layer</param>
            public FrameLayer(Bitmap layerBitmap)
            {
                Name = string.Empty;
                LayerBitmap = layerBitmap;
            }

            /// <summary>
            /// Clones this frame layer object
            /// </summary>
            /// <returns>A clone of this frame layer's object</returns>
            public IFrameLayer Clone()
            {
                FrameLayer layer = new FrameLayer(new Bitmap(Width, Height, LayerBitmap.PixelFormat)) { Name = Name };

                layer.CopyFromBitmap(LayerBitmap);

                return layer;
            }
            
            /// <summary>
            /// Destructor the FrameLayer class
            /// </summary>
            ~FrameLayer()
            {
                Dispose();
            }

            #region IDisposable Members

            /// <summary>
            /// Internal variable which checks if Dispose has already been called
            /// </summary>
            private Boolean _disposed;
            
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                LayerBitmap?.Dispose();

                _disposed = true;

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

                Bitmap newTexture = (Bitmap)ImageUtilities.Resize(LayerBitmap, newWidth, newHeight, scalingMethod, interpolationMode);

                // Texture replacement
                LayerBitmap.Dispose();
                LayerBitmap = newTexture;
            }

            /// <summary>
            /// Copies this layer's contents from the given bitmap.
            /// If the layer's dimensions don't match the passed bitmap's dimensions, an ArgumentException is raised
            /// </summary>
            /// <param name="bitmap">The bitmap to copy to this layer</param>
            /// <exception cref="ArgumentException">The bitmap's dimensions don't match this layer's dimensions</exception>
            public void CopyFromBitmap(Bitmap bitmap)
            {
                if (bitmap.Width != LayerBitmap.Width || bitmap.Height != LayerBitmap.Height)
                {
                    throw new ArgumentException(@"The provided bitmap's dimensions don't match this bitmap's dimensions", nameof(bitmap));
                }

                // Copy the pixels
                FastBitmap.CopyPixels(bitmap, LayerBitmap);
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
                return ImageUtilities.ImagesAreIdentical(LayerBitmap, other.LayerBitmap) && Index == other.Index;
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
                return (LayerBitmap?.GetHashCode() ?? 0) ^ (Index * 367);
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
    public interface IFrameLayer : IDisposable
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
        /// Gets or sets the name for this layer object
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the frame that owns this IFrameLayer object
        /// </summary>
        Frame Frame { get; }

        /// <summary>
        /// Gets this layer's bitmap content
        /// </summary>
        Bitmap LayerBitmap { get; }

        /// <summary>
        /// Gets the size of this layer
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Clones this Frame Layer object
        /// </summary>
        /// <returns>A copy of this Frame Layer object</returns>
        IFrameLayer Clone();
    }
}
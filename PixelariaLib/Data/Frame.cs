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
using JetBrains.Annotations;
using PixelariaLib.Utils;

namespace PixelariaLib.Data
{
    /// <summary>
    /// Describes an animation frame
    /// </summary>
    public class Frame : IFrame
    {
        /// <summary>
        /// The list of layers laid on this frame
        /// </summary>
        public readonly List<FrameLayer> Layers;

        public KeyframeMetadata KeyframeMetadata { get; } = new KeyframeMetadata();

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
        public int LayerCount => Layers.Count;

        // TODO: Find a way to fetch this Index from somewhere else other than
        // the base animation object directly.
        /// <summary>
        /// Gets the index of this frame on the parent animation
        /// </summary>
        public int Index => Animation.GetFrameIndex(this);

        /// <summary>
        /// Gets the animation this frame belongs to
        /// </summary>
        public Animation Animation { get; set; }

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
            Layers = new List<FrameLayer>();
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
            Layers = new List<FrameLayer>();
            Initialize(parentAnimation, width, height, initHash);
        }

        /// <summary>
        /// Creates a new animation frame
        /// </summary>
        /// <param name="animation">The parent animation</param>
        /// <param name="width">The width of this frame</param>
        /// <param name="height">The height of this frame</param>
        /// <param name="initHash">Whether to initialize the frame's hash now</param>
        /// <exception cref="InvalidOperationException">The Initialize function was already called</exception>
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

            var frameLayer = new FrameLayer(new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                Index = 0
            };
            Layers.Add(frameLayer);
            frameLayer.Frame = this;

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
            foreach (var layer in Layers)
            {
                layer.Dispose();
            }
            Layers.Clear();
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
            Layers.AddRange(castFrame.Layers.Select(t => t.Clone() as FrameLayer));

            // Update the indices of the layers
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Index = i;
            }

            Hash = frame.Hash;
            _shortHash = castFrame._shortHash;

            // Copy metadata
            KeyframeMetadata.CopyFrom(frame.KeyframeMetadata);
        }

        /// <summary>
        /// Returns whether the current frame can copy the contents of the specified frame type
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
        public bool Equals([NotNull] Frame frame)
        {
            if (Width != frame.Width || Height != frame.Height)
                return false;

            if (Hash == null || frame.Hash == null)
                return false;
            
            if (_shortHash != frame._shortHash) // Check short hash before hands
                return false;
            
            return UnsafeNativeMethods.memcmp(Hash, frame.Hash, Hash.Length) == 0;
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
        /// Swaps the current frame bitmap with the given one. If the new bitmap's dimensions
        /// don't match the Frame's dimensions, an ArgumentException is thrown.
        /// If there's already a Bitmap loaded, the current Bitmap is disposed to save memory.
        /// </summary>
        /// <param name="bitmap">The new frame bitmap</param>
        /// <param name="updateHash">Whether to update the hash after settings the bitmap</param>
        public void SetFrameBitmap([NotNull] Bitmap bitmap, bool updateHash = true)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            // Copy to the first layer
            Layers[0].CopyFromBitmap(bitmap);

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

            var composedBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            FastBitmap.CopyPixels(Layers[0].LayerBitmap, composedBitmap);

            // Compose the layers by blending all the pixels from each layer into the final image
            for (int i = 1; i < Layers.Count; i++)
            {
                ImageUtilities.FlattenBitmaps(composedBitmap, Layers[i].LayerBitmap, true);
            }
            
            return composedBitmap;
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

            foreach (var layer in Layers)
            {
                layer.Resize(newWidth, newHeight, scalingMethod, interpolationMode);
            }

            // Update hash
            UpdateHash();
        }

        /// <summary>
        /// Returns the memory usage of this frame, in bytes
        /// </summary>
        /// <param name="composed">Whether to calculate the memory usage after the frame has been composed into a single image</param>
        /// <returns>Total memory usage, in bytes</returns>
        public long CalculateMemoryUsageInBytes(bool composed)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            // For composed mode, use the memory usage of the first layer
            if (composed)
                return ImageUtilities.MemoryUsageOfImage(Layers[0].LayerBitmap);

            // Calculate the usage of each layer individually
            return Layers.Sum(layer => ImageUtilities.MemoryUsageOfImage(layer.LayerBitmap));
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

            using var bitmap = GetComposedBitmap();
            SetHash(ImageUtilities.GetHashForBitmap(bitmap));
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

            var other = (Frame) obj;

            if (Layers.Count != other.Layers.Count || Hash == null || other.Hash == null || !Utilities.ByteArrayCompare(Hash, other.Hash) ||
                Width != other.Width || Height != other.Height)
            {
                return false;
            }

            // Compare each layer individually
            // Disable LINQ suggestion because it'd actually be considerably slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < Layers.Count; i++)
            {
                if (!Layers[i].Equals(other.Layers[i]))
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
        public class FrameLayer : IFrameLayer, IEquatable<FrameLayer>
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
            /// <param name="name">A display name for the layer</param>
            public FrameLayer(Bitmap layerBitmap, string name = "")
            {
                Name = name;
                LayerBitmap = layerBitmap;
            }

            /// <summary>
            /// Clones this frame layer object
            /// </summary>
            /// <returns>A clone of this frame layer's object</returns>
            public IFrameLayer Clone()
            {
                var layer = new FrameLayer(new Bitmap(Width, Height, LayerBitmap.PixelFormat)) { Name = Name };

                layer.CopyFromBitmap(LayerBitmap);

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
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            
            protected virtual void Dispose(bool disposing)
            {
                if (!disposing)
                    return;

                LayerBitmap?.Dispose();
                
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

                var newTexture = (Bitmap)ImageUtilities.Resize(LayerBitmap, newWidth, newHeight, scalingMethod, interpolationMode);

                // Texture replacement
                LayerBitmap.Dispose();
                LayerBitmap = newTexture;
            }

            /// <summary>
            /// Resizes a copy of this Layer that matches the given dimensions, scaling with the given scaling method, and interpolating with the given interpolation mode.
            /// </summary>
            /// <param name="newWidth">The new width for the copy layer</param>
            /// <param name="newHeight">The new height for the copy layer</param>
            /// <param name="scalingMethod">The scaling method to use to match the copy layer's size to the new size</param>
            /// <param name="interpolationMode">The interpolation mode to use when drawing the new layer</param>
            public IFrameLayer Resized(int newWidth, int newHeight, PerFrameScalingMethod scalingMethod, InterpolationMode interpolationMode)
            {
                var layerCopy = (FrameLayer)Clone();
                layerCopy.Resize(newWidth, newHeight, scalingMethod, interpolationMode);
                return layerCopy;
            }

            /// <summary>
            /// Copies this layer's contents from the given bitmap.
            /// If the layer's dimensions don't match the passed bitmap's dimensions, an ArgumentException is raised
            /// </summary>
            /// <param name="bitmap">The bitmap to copy to this layer</param>
            /// <exception cref="ArgumentException">The bitmap's dimensions don't match this layer's dimensions</exception>
            public void CopyFromBitmap([NotNull] Bitmap bitmap)
            {
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
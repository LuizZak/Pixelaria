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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FastBitmapLib;
using JetBrains.Annotations;
using Pixelaria.Data;
using Pixelaria.Utils;

namespace Pixelaria.Controllers.DataControllers
{
    public class FrameController: IDisposable
    {
        private bool _disposed;
        private readonly Frame _frame;
        private FrameController _original;

        public int Height => _frame.Height;

        public int Width => _frame.Width;

        public Size Size => _frame.Size;

        /// <summary>
        /// Index on the current animation associated with this frame
        /// </summary>
        public int Index => _frame.Index;

        /// <summary>
        /// Gets the number of layers on this frame controller
        /// </summary>
        public int LayerCount => _frame.LayerCount;

        public FrameController(Frame frame)
        {
            _frame = frame;
        }

        /// <summary>
        /// Disposes the underlying frame from memory.
        /// 
        /// Warning: Do not use on frames that are referenced by an animation or anything else!
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Debug.Assert(_original != null, "_original != null", "Trying to discard original frame controller that points to on-disk/storage frame.");
            _frame.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Returns a frame controller that is a surrogate for this frame controller, which can later
        /// either discard changes or pass them back to this controller to persist on the original bundle.
        /// </summary>
        public FrameController MakeCopyForEditing()
        {
            var newFrame = _frame.Clone();

            return new FrameController(newFrame) {_original = _original ?? this};
        }

        /// <summary>
        /// Pushes changes to original animation controller, and consequently to the base bundle.
        /// </summary>
        public void ApplyChanges()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            _original?.InternalApplyChanges(_frame);
        }

        private void InternalApplyChanges(Frame frame)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            _frame.CopyFrom(frame);
        }

        /// <summary>
        /// Returns an independent copy of the frame managed by this frame controller.
        /// 
        /// TODO: This is hackish to overcome encapsulation of AnimationController/FrameControllers. Find a better way to do this later.
        /// </summary>
        public IFrame GetStandaloneCopy()
        {
            return _frame.Clone();
        }

        /// <summary>
        /// Gets the bitmap for a layer on a given index in this frame controller
        /// </summary>
        public Bitmap GetLayerBitmap(int index)
        {
            return GetLayerAt(0).LayerBitmap;
        }
        
        /// <summary>
        /// Adds a layer on this Frame object. Optionally allow specifying a bitmap as an initial image.
        /// If the bitmap does not match the frame's dimensions or its pixel format is not 32bpp, an exception is raised.
        /// 
        /// If no bitmap is provided, an empty transparent bitmap is used instead.
        /// </summary>
        /// <param name="name">A display name for the layer. Defaults to an empty string.</param>
        /// <param name="bitmap">The bitmap to use as a layer image</param>
        /// <param name="layerIndex">The index to add the layer at. Leave -1 to add to the end of the layer list</param>
        /// <returns>The layer that was just created</returns>
        /// <exception cref="ArgumentException">The provided bitmap's dimensions does not match the Frame's dimensions, or its pixel format isn't 32bpp</exception>
        public IFrameLayer CreateLayer(Bitmap bitmap = null, [NotNull] string name = "", int layerIndex = -1)
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }
            if (bitmap != null)
            {
                if (bitmap.Width != Width || bitmap.Height != Height || Image.GetPixelFormatSize(bitmap.PixelFormat) != 32)
                    throw new ArgumentException(
                        @"The provided bitmap's dimensions must match the size of this frame and its pixel format must be a 32bpp variant",
                        nameof(bitmap));
            }

            var layer = new Frame.FrameLayer(bitmap?.DeepClone() ?? new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                Index = layerIndex == -1 ? LayerCount : layerIndex,
                Name = name
            };

            AddLayer(layer, layerIndex);
            
            return layer;
        }

        /// <summary>
        /// Adds the specified layer to the Frame object.
        /// If the layer's size does not match this frame's dimensions, an exception is raised.
        /// If the layer's type does not match the internal layer type (or, the layer does not originates from a CreateLayer/AddLayer/GetLayerAt from this object), an exception is raised
        /// </summary>
        /// <param name="layer">The layer to add to this frame</param>
        /// <param name="layerIndex">The index at which to add the layer</param>
        /// <exception cref="ArgumentException">
        /// The provided layers's dimensions does not match this Frame's dimensions, or the provided layers's type
        /// is not compatible with the Frame object, or the provided layer is already stored in a Frame object
        /// </exception>
        public void AddLayer([NotNull] IFrameLayer layer, int layerIndex = -1)
        {
            if (!_frame.Initialized)
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

            var frameLayer = layer as Frame.FrameLayer;
            if (frameLayer == null)
            {
                throw new ArgumentException("The provided layers's type is not compatible with this Frame object");
            }

            if (layerIndex == -1 || layerIndex == _frame.Layers.Count)
                _frame.Layers.Add(frameLayer);
            else
                _frame.Layers.Insert(layerIndex, frameLayer);

            frameLayer.Frame = _frame;

            UpdateLayerIndices();
        }

        /// <summary>
        /// Removes a layer that is stored on the specified index on the Frame
        /// </summary>
        /// <param name="layerIndex">The layer index to remove</param>
        /// <param name="dispose">Whether to dispose of the layer after removing it</param>
        public void RemoveLayerAt(int layerIndex, bool dispose = true)
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (dispose)
            {
                _frame.Layers[layerIndex].Dispose();
            }

            _frame.Layers[layerIndex].Frame = null;

            _frame.Layers.RemoveAt(layerIndex);

            if (_frame.Layers.Count == 0)
                CreateLayer();

            UpdateLayerIndices();
        }

        /// <summary>
        /// Gets a layer at the specified index on the Frame object
        /// </summary>
        /// <param name="index">The index to get the layer at</param>
        /// <returns>A layer at the specified index on the Frame object</returns>
        public IFrameLayer GetLayerAt(int index)
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            return _frame.Layers[index];
        }

        /// <summary>
        /// Gets a layer metadata at the specified index on the Frame object
        /// </summary>
        /// <param name="index">The index to get the layer at</param>
        /// <returns>A layer metadata at the specified index on the Frame object</returns>
        public LayerMetadata GetLayerMetadata(int index)
        {
            var layer = _frame.Layers[index];
            return new LayerMetadata(layer.Size, layer.Index, layer.Name);
        }

        /// <summary>
        /// Swaps layers at the two specified indices
        /// </summary>
        /// <param name="firstIndex">The first layer index to swap</param>
        /// <param name="secondIndex">The second layer index to swap</param>
        /// <param name="updateHash">Whether to update the frame's hash after the operation</param>
        public void SwapLayers(int firstIndex, int secondIndex, bool updateHash = true)
        {
            var secondLayer = _frame.Layers[secondIndex];
            _frame.Layers[secondIndex] = _frame.Layers[firstIndex];
            _frame.Layers[firstIndex] = secondLayer;

            UpdateLayerIndices();

            if (updateHash)
            {
                _frame.UpdateHash();
            }
        }

        /// <summary>
        /// Updates the bitmap of a given layer. If the dimensions of the bitmap don't match 32bpp, an exception is raised
        /// </summary>
        /// <param name="layerIndex">The index of the layer to update</param>
        /// <param name="layerBitmap">The new layer bitmap</param>
        /// <param name="updateHash">Whether to update the frame's hash after the operation</param>
        /// <exception cref="ArgumentException">The dimensions of the bitmap don't match this frame's size, or its pixel format isn't 32bpp</exception>
        public void SetLayerBitmap(int layerIndex, [NotNull] Bitmap layerBitmap, bool updateHash = true)
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (layerBitmap.Width != Width || layerBitmap.Height != Height || Image.GetPixelFormatSize(layerBitmap.PixelFormat) != 32)
            {
                throw new ArgumentException(@"The provided bitmap's dimensions must match the size of this frame and its pixel format must be a 32bpp variant", nameof(layerBitmap));
            }

            //_layers[layerIndex].LayerBitmap = layerBitmap;
            _frame.Layers[layerIndex].CopyFromBitmap(layerBitmap);

            if (updateHash)
            {
                _frame.UpdateHash();
            }
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
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            // Copy to the first layer
            _frame.Layers[0].LayerBitmap.Dispose();
            _frame.Layers[0].LayerBitmap = bitmap.DeepClone();

            if (updateHash)
                _frame.UpdateHash();
        }

        /// <summary>
        /// Returns the composed Bitmap for this frame
        /// </summary>
        /// <returns>The composed bitmap for this frame</returns>
        public Bitmap GetComposedBitmap()
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException(@"The frame was not initialized prior to this action");
            }

            var composedBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            FastBitmap.CopyPixels(GetLayerBitmap(0), composedBitmap);

            // Compose the layers by blending all the pixels from each layer into the final image
            for (int i = 1; i < LayerCount; i++)
            {
                ImageUtilities.FlattenBitmaps(composedBitmap, GetLayerBitmap(i), true);
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
            _frame.Resize(newWidth, newHeight, scalingMethod, interpolationMode);
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
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            var output = new Bitmap(width, height);

            using (var composed = GetComposedBitmap())
            using (var graphics = Graphics.FromImage(output))
            {
                float tx = 0, ty = 0;
                float scaleX = 1, scaleY = 1;

                if (composed.Width >= composed.Height)
                {
                    if (width < composed.Width || resizeOnSmaller)
                    {
                        scaleX = (float) width / composed.Width;
                        scaleY = scaleX;
                    }
                    else
                    {
                        tx = (float) height / 2 - (composed.Width * scaleX / 2);
                    }

                    ty = (float) width / 2 - (composed.Height * scaleY / 2);
                }
                else
                {
                    if (height < composed.Height || resizeOnSmaller)
                    {
                        scaleY = (float) height / composed.Height;
                        scaleX = scaleY;
                    }
                    else
                    {
                        ty = (float) width / 2 - (composed.Height * scaleY / 2);
                    }

                    tx = (float) height / 2 - (composed.Width * scaleX / 2);
                }

                if (!centered)
                {
                    tx = ty = 0;
                }

                var area = new RectangleF((float) Math.Round(tx), (float) Math.Round(ty),
                    (float) Math.Round(composed.Width * scaleX), (float) Math.Round(composed.Height * scaleY));

                graphics.Clear(backColor);
                
                graphics.DrawImage(composed, area);

                graphics.Flush();
            }

            return output;
        }

        /// <summary>
        /// Updates the indices for all the layers stored on the frame
        /// </summary>
        public void UpdateLayerIndices()
        {
            for (int i = 0; i < _frame.Layers.Count; i++)
            {
                _frame.Layers[i].Index = i;
            }
        }

        /// <summary>
        /// Updates the name of a layer at a given index on the frame
        /// </summary>
        public void SetLayerName(int index, string name)
        {
            _frame.Layers[index].Name = name;
        }
    }
}
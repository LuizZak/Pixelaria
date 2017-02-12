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
using System.Drawing.Imaging;

using FastBitmapLib;

using Pixelaria.Data;
using Pixelaria.Utils;

namespace Pixelaria.Controllers.DataControllers
{
    public class FrameController
    {
        private readonly Frame _frame;

        public int Height => _frame.Height;

        public int Width => _frame.Width;

        /// <summary>
        /// Gets the number of layers on this frame controller
        /// </summary>
        public int LayerCount => _frame.LayerCount;

        public FrameController(Frame frame)
        {
            _frame = frame;
        }

        /// <summary>
        /// Gets the bitmap for a layer on a given index in this frame controller
        /// </summary>
        public Bitmap GetLayerBitmap(int index)
        {
            return GetLayerAt(0).LayerBitmap;
        }

        /// <summary>
        /// Creates a new empty layer on this Frame
        /// </summary>
        /// <param name="layerIndex">The index to add the layer at. Leave -1 to add to the end of the layer list</param>
        public IFrameLayer CreateLayer(int layerIndex = -1)
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            var layer = new Frame.FrameLayer(new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                Index = layerIndex == -1 ? LayerCount : layerIndex
            };

            AddLayer(layer, layerIndex);

            return layer;
        }

        /// <summary>
        /// Adds a layer on this Frame object based on the specified bitmap.
        /// If the bitmap does not match the frame's dimensions or its pixel format is not 32bpp, an exception is raised
        /// </summary>
        /// <param name="bitmap">The bitmap to use as a layer image</param>
        /// <param name="layerIndex">The index to add the layer at. Leave -1 to add to the end of the layer list</param>
        /// <returns>The layer that was just created</returns>
        /// <exception cref="ArgumentException">The provided bitmap's dimensions does not match the Frame's dimensions, or its pixel format isn't 32bpp</exception>
        public IFrameLayer AddLayer(Bitmap bitmap, int layerIndex = -1)
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException("The frame was not initialized prior to this action");
            }

            if (bitmap.Width != Width || bitmap.Height != Height || Image.GetPixelFormatSize(bitmap.PixelFormat) != 32)
            {
                throw new ArgumentException(@"The provided bitmap's dimensions must match the size of this frame and its pixel format must be a 32bpp variant", nameof(bitmap));
            }

            var layer = (Frame.FrameLayer)CreateLayer(layerIndex);

            layer.CopyFromBitmap(bitmap);

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
        public void AddLayer(IFrameLayer layer, int layerIndex = -1)
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
        public void SetLayerBitmap(int layerIndex, Bitmap layerBitmap, bool updateHash = true)
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
        /// Returns the composed Bitmap for this frame
        /// </summary>
        /// <returns>The composed bitmap for this frame</returns>
        public Bitmap GetComposedBitmap()
        {
            if (!_frame.Initialized)
            {
                throw new InvalidOperationException(@"The frame was not initialized prior to this action");
            }

            Bitmap composedBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            FastBitmap.CopyPixels(GetLayerBitmap(0), composedBitmap);

            // Compose the layers by blending all the pixels from each layer into the final image
            for (int i = 1; i < LayerCount; i++)
            {
                ImageUtilities.FlattenBitmaps(composedBitmap, GetLayerBitmap(i), true);
            }

            return composedBitmap;
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
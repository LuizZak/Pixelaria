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
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Utils;

namespace Pixelaria.Controllers.LayerControlling
{
    /// <summary>
    /// Class that is used as an interface for layer management
    /// </summary>
    public class LayerController
    {
        /// <summary>
        /// The frame from which the layers this layer controller is manipulating come from
        /// </summary>
        private Frame _frame;

        /// <summary>
        /// Gets the frame controller for this layer controller
        /// </summary>
        private FrameController _frameController;

        /// <summary>
        /// The currently active layer index
        /// </summary>
        private int _activeLayerIndex;

        /// <summary>
        /// Delegate for the LayersSwapped event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayersSwappedEventHandler(object sender, LayerControllerLayerMovedEventArgs args);
        /// <summary>
        /// Event fired before two layers are swapped  with the layer controller.
        /// This event is called before any modification is made to the underlying frame
        /// </summary>
        public event LayersSwappedEventHandler BeforeLayerMoved;
        /// <summary>
        /// Event fired whenever a call to SwapLayers is made
        /// </summary>
        public event LayersSwappedEventHandler LayerMoved;

        /// <summary>
        /// Delegate for the LayerCreated event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayerCreatedEventHandler(object sender, LayerControllerLayerCreatedEventArgs args);
        /// <summary>
        /// Event fired before a layer is created with the layer controller.
        /// This event is called before any modification is made to the underlying frame
        /// </summary>
        public event EventHandler BeforeLayerCreated;
        /// <summary>
        /// Event fired whenever a call to CreateLayer or AddLayer is made
        /// </summary>
        public event LayerCreatedEventHandler LayerCreated;

        /// <summary>
        /// Delegate for the LayerDuplicated event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayerDuplicatedEventHandler(object sender, LayerControllerLayerDuplicatedEventArgs args);
        /// <summary>
        /// Event fired before a layer is duplicated with the layer controller.
        /// This event is called before any modification is made to the underlying frame
        /// </summary>
        public event LayerDuplicatedEventHandler BeforeLayerDuplicated;
        /// <summary>
        /// Event fired whenever a call to DuplicateLayer is made
        /// </summary>
        public event LayerDuplicatedEventHandler LayerDuplicated;

        /// <summary>
        /// Delegate for the LayerRemoved event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayerRemovedEventHandler(object sender, LayerControllerLayerRemovedEventArgs args);
        /// <summary>
        /// Event fired before a layer is removed with the layer controller.
        /// This event is called before any modification is made to the underlying frame
        /// </summary>
        public event LayerRemovedEventHandler BeforeLayerRemoved;
        /// <summary>
        /// Event fired whenever a call to RemoveLayer is made
        /// </summary>
        public event LayerRemovedEventHandler LayerRemoved;

        /// <summary>
        /// Delegate for the LayerImageUpdated event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayerImageUpdatedEventHandler(object sender, LayerControllerLayerImageUpdatedEventArgs args);
        /// <summary>
        /// Event fired whenever a call to UpdateLayerBitmap is made
        /// </summary>
        public event LayerImageUpdatedEventHandler LayerImageUpdated;

        /// <summary>
        /// Delegate for the LayerNameUpdated event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayerNameUpdatedEventHandler(object sender, LayerControllerLayerNameUpdatedEventArgs args);
        /// <summary>
        /// Event fired whenever a call to SetLayerName is made
        /// </summary>
        public event LayerNameUpdatedEventHandler LayerNameUpdated;

        /// <summary>
        /// Delegate for the LayersCombined event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayersCombineEventHandler(object sender, LayerControllerLayersCombinedEventArgs args);
        /// <summary>
        /// Event fired before a set of layers are combined with the layer controller.
        /// This event is called before any modification is made to the underlying frame
        /// </summary>
        public event LayersCombineEventHandler BeforeLayersCombined;
        /// <summary>
        /// Event fired whenever a call to CombineLayers is made
        /// </summary>
        public event LayersCombineEventHandler LayersCombined;

        /// <summary>
        /// Delegate for the FrameChanged event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void FrameChangedEventHandler(object sender, LayerControllerFrameChangedEventArgs args);
        /// <summary>
        /// Event fired whenever the current frame being controlled is changed
        /// </summary>
        public event FrameChangedEventHandler FrameChanged;

        /// <summary>
        /// Delegate for the ActiveLayerIndexChanged event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void ActiveLayerIndexChangedEventHandler(object sender, ActiveLayerIndexChangedEventArgs args);
        /// <summary>
        /// Event fired before the current active layer index is changed.
        /// </summary>
        public event ActiveLayerIndexChangedEventHandler BeforeActiveLayerIndexChanged;
        /// <summary>
        /// Event fired whenever the current active layer index is changed
        /// </summary>
        public event ActiveLayerIndexChangedEventHandler ActiveLayerIndexChanged;

        /// <summary>
        /// Gets or sets the current frame being controlled
        /// </summary>
        public Frame Frame
        {
            get => _frame;
            set
            {
                if (ReferenceEquals(_frame, value))
                    return;

                _frame = value;
                _frameController = new FrameController(_frame);

                FrameChanged?.Invoke(this, new LayerControllerFrameChangedEventArgs(value));
            }
        }

        /// <summary>
        /// Gets or sets the currently active layer index.
        /// When settings the value, it must be >= 0 and smaller than layer count
        /// </summary>
        public int ActiveLayerIndex
        {
            get => Math.Max(0, Math.Min(_frame.LayerCount - 1, _activeLayerIndex));
            set
            {
                if (_activeLayerIndex == value)
                    return;

                if(value < 0 || value >= _frame.LayerCount)
                    throw new ArgumentOutOfRangeException(nameof(value), @"The value specified must be >= 0 and smaller than the layer count");

                BeforeActiveLayerIndexChanged?.Invoke(this, new ActiveLayerIndexChangedEventArgs(value));

                _activeLayerIndex = value;

                ActiveLayerIndexChanged?.Invoke(this, new ActiveLayerIndexChangedEventArgs(value));
            }
        }

        /// <summary>
        /// Gets the currently active layer
        /// </summary>
        public IFrameLayer ActiveLayer => _frameController.GetLayerAt(ActiveLayerIndex);

        /// <summary>
        /// Gets an array of all layers for the frame being controller
        /// </summary>
        public IFrameLayer[] FrameLayers
        {
            get
            {
                IFrameLayer[] layers = new IFrameLayer[_frame.LayerCount];

                for (int i = 0; i < _frame.LayerCount; i++)
                {
                    layers[i] = _frameController.GetLayerAt(i);
                }

                return layers;
            }
        }

        /// <summary>
        /// Gets the number of layers on the current active frame
        /// </summary>
        public int LayerCount => _frame.LayerCount;

        /// <summary>
        /// Initializes a new instance of the LayerController class with a specified frame to control
        /// </summary>
        /// <param name="frame">The frame to control on this Layer Controller</param>
        public LayerController(Frame frame)
        {
            _frame = frame;
            if (frame != null)
            {
                _frameController = new FrameController(frame);
            }
        }

        /// <summary>
        /// Creates a layer at the specified index
        /// </summary>
        /// <param name="layerIndex">The index at which to create the layer</param>
        public IFrameLayer CreateLayer(int layerIndex = -1)
        {
            BeforeLayerCreated?.Invoke(this, new EventArgs());

            IFrameLayer layer = _frameController.CreateLayer(layerIndex);

            LayerCreated?.Invoke(this, new LayerControllerLayerCreatedEventArgs(layer));

            return layer;
        }

        /// <summary>
        /// Adds a layer with a specified bitmap at the specified index
        /// </summary>
        /// <param name="bitmap">The bitmap to use as a layer</param>
        /// <param name="index">The index to add the layer at</param>
        /// <returns>The layer that was created</returns>
        public IFrameLayer AddLayer(Bitmap bitmap, int index = -1)
        {
            BeforeLayerCreated?.Invoke(this, new EventArgs());

            IFrameLayer layer = _frameController.AddLayer(bitmap, index);

            LayerCreated?.Invoke(this, new LayerControllerLayerCreatedEventArgs(layer));

            return layer;
        }

        /// <summary>
        /// Adds the specified layer at a specified index
        /// </summary>
        /// <param name="layer">The layer to add</param>
        /// <param name="index">The index to add the layer at</param>
        public void AddLayer(IFrameLayer layer, int index = -1)
        {
            BeforeLayerCreated?.Invoke(this, new EventArgs());

            _frameController.AddLayer(layer, index);

            LayerCreated?.Invoke(this, new LayerControllerLayerCreatedEventArgs(layer));
        }

        /// <summary>
        /// Moves a layer from one point to another in the frame.
        /// If the layer index and the new index are the same, nothing is done
        /// </summary>
        /// <param name="layerIndex">The index of the layer to swap</param>
        /// <param name="newIndex">The index of the second layer to swap</param>
        public void MoveLayer(int layerIndex, int newIndex)
        {
            if (layerIndex == newIndex)
                return;

            // Notify before event
            BeforeLayerMoved?.Invoke(this, new LayerControllerLayerMovedEventArgs(layerIndex, newIndex));

            // Move layer
            if(layerIndex < newIndex)
            {
                // Move layer by swapping it until it is in the new desired place
                for (int i = layerIndex; i < newIndex; i++)
                {
                    _frameController.SwapLayers(i, i + 1);
                }
            }
            else if (layerIndex > newIndex)
            {
                // Move layer by swapping it until it is in the new desired place
                for (int i = layerIndex; i > newIndex; i--)
                {
                    _frameController.SwapLayers(i, i - 1);
                }
            }

            // Notify after event
            LayerMoved?.Invoke(this, new LayerControllerLayerMovedEventArgs(layerIndex, newIndex));

            // Update active layer index
            if (ActiveLayerIndex == layerIndex)
            {
                ActiveLayerIndex = newIndex;
            }
            else if (ActiveLayerIndex == newIndex)
            {
                ActiveLayerIndex = layerIndex;
            }
        }

        /// <summary>
        /// Removes a layer that is at the specified index
        /// </summary>
        /// <param name="layerIndex">The index of the layer to remove</param>
        /// <param name="dispose">Whether to dispose of the layer that was removed</param>
        public void RemoveLayer(int layerIndex, bool dispose = true)
        {
            IFrameLayer layer = _frameController.GetLayerAt(layerIndex);

            BeforeLayerRemoved?.Invoke(this, new LayerControllerLayerRemovedEventArgs(layer));

            _frameController.RemoveLayerAt(layerIndex, dispose);

            // Normalize active layer
            _activeLayerIndex = ActiveLayerIndex;

            LayerRemoved?.Invoke(this, new LayerControllerLayerRemovedEventArgs(layer));
        }

        /// <summary>
        /// Copies a specified bitmap to a layer on a given index
        /// </summary>
        /// <param name="layerIndex">The index of the layer to update the bitmap of</param>
        /// <param name="bitmap">The new bitmap for the layer</param>
        public void UpdateLayerBitmap(int layerIndex, Bitmap bitmap)
        {
            Bitmap oldBitmap = null;

            // Make a copy of the bitmap before modifying it for the event
            var frameLayer = _frameController.GetLayerAt(layerIndex);

            if (LayerImageUpdated != null)
            {
                oldBitmap = frameLayer.LayerBitmap.Clone(new Rectangle(Point.Empty, frameLayer.Size), frameLayer.LayerBitmap.PixelFormat);
            }

            _frameController.SetLayerBitmap(layerIndex, bitmap);

            LayerImageUpdated?.Invoke(this, new LayerControllerLayerImageUpdatedEventArgs(frameLayer, oldBitmap));
        }

        /// <summary>
        /// Renames a specific layer
        /// </summary>
        /// <param name="layerIndex">The index of the layer to rename</param>
        /// <param name="newName">The new name for the layer</param>
        public void SetLayerName(int layerIndex, string newName)
        {
            var layer = _frameController.GetLayerAt(layerIndex);
            string oldName = layer.Name;

            layer.Name = newName;

            LayerNameUpdated?.Invoke(this, new LayerControllerLayerNameUpdatedEventArgs(layer, oldName));
        }

        /// <summary>
        /// Duplicates the specified layer index
        /// </summary>
        /// <param name="layerIndex">The layer to duplicate</param>
        /// <returns>An IFrameLayer for the newly duplicated layer</returns>
        public IFrameLayer DuplicateLayer(int layerIndex)
        {
            BeforeLayerDuplicated?.Invoke(this, new LayerControllerLayerDuplicatedEventArgs(layerIndex));

            // Duplicate the layer up
            IFrameLayer layer = _frameController.GetLayerAt(layerIndex).Clone();

            // Use the class' AddLayer method to take advantage of the event firing
            if (layerIndex == _frame.LayerCount - 1)
                AddLayer(layer);
            else
                AddLayer(layer, layerIndex + 1);

            LayerDuplicated?.Invoke(this, new LayerControllerLayerDuplicatedEventArgs(layerIndex));

            return layer;
        }

        /// <summary>
        /// Combines all the provided layers. Combining layers results in the removal of all layers but the bottom-most one, where
        /// the image of the combined layers will be set
        /// </summary>
        /// <param name="layers">The layers to combine</param>
        public void CombineLayers(IFrameLayer[] layers)
        {
            BeforeLayersCombined?.Invoke(this, new LayerControllerLayersCombinedEventArgs(layers));

            // Combine the layers by first removing all the layers but the bottom-most one
            Bitmap combinedBitmap = new Bitmap(_frame.Width, _frame.Height);

            for (int i = 0; i < layers.Length; i++)
            {
                // Combine the bitmap
                ImageUtilities.FlattenBitmaps(combinedBitmap, layers[i].LayerBitmap, true);

                if (i != 0)
                {
                    RemoveLayer(layers[i].Index, false);
                }
            }

            // Switch image of the last bitmap
            UpdateLayerBitmap(layers[0].Index, combinedBitmap);

            combinedBitmap.Dispose();

            LayersCombined?.Invoke(this, new LayerControllerLayersCombinedEventArgs(layers));
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayersMoved event
    /// </summary>
    public class LayerControllerLayerMovedEventArgs : EventArgs
    {
        /// <summary>
        /// The index of the layer that was moved
        /// </summary>
        public int LayerIndex { get; }

        /// <summary>
        /// The new index of the layer that was moved
        /// </summary>
        public int NewIndex { get; }

        /// <summary>
        /// Creates a new instance of the LayerControllerLayerMovedEventArgs class
        /// </summary>
        /// <param name="layerIndex">The index of the layer that was moved</param>
        /// <param name="newIndex">The new index of the layer that was moved</param>
        public LayerControllerLayerMovedEventArgs(int layerIndex, int newIndex)
        {
            LayerIndex = layerIndex;
            NewIndex = newIndex;
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayerCreated event
    /// </summary>
    public class LayerControllerLayerCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the layer that was created
        /// </summary>
        public IFrameLayer FrameLayer { get; }

        /// <summary>
        /// Initializes a new instance of the LayerControllerLayerCreatedEventArgs class
        /// </summary>
        /// <param name="frameLayer">The frame layer that was created</param>
        public LayerControllerLayerCreatedEventArgs(IFrameLayer frameLayer)
        {
            FrameLayer = frameLayer;
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayerRemoved event
    /// </summary>
    public class LayerControllerLayerRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the layer that was removed
        /// </summary>
        public IFrameLayer FrameLayer { get; }

        /// <summary>
        /// Initializes a new instance of the LayerControllerLayerRemovedEventArgs class
        /// </summary>
        /// <param name="frameLayer">The layer that was removed</param>
        public LayerControllerLayerRemovedEventArgs(IFrameLayer frameLayer)
        {
            FrameLayer = frameLayer;
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayerImageUpdated event
    /// </summary>
    public class LayerControllerLayerImageUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the layer that was updated
        /// </summary>
        public IFrameLayer FrameLayer { get; }

        /// <summary>
        /// Gets the old bitmap before the layer image was updated. This is an independent copy of the layer's bitmap object
        /// </summary>
        public Bitmap OldLayerBitmap { get; }

        /// <summary>
        /// Initializes a new instance of the LayerControllerLayerImageUpdatedEventArgs class
        /// </summary>
        /// <param name="frameLayer">The layer that was updated</param>
        /// <param name="oldLayerBitmap">The old layer bitmap before the layer image was updated</param>
        public LayerControllerLayerImageUpdatedEventArgs(IFrameLayer frameLayer, Bitmap oldLayerBitmap)
        {
            FrameLayer = frameLayer;
            OldLayerBitmap = oldLayerBitmap;
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayerNameUpdated event
    /// </summary>
    public class LayerControllerLayerNameUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the layer that was updated
        /// </summary>
        public IFrameLayer FrameLayer { get; }

        /// <summary>
        /// Gets the old name before the layer name was updated
        /// </summary>
        public string OldLayerName { get; }

        /// <summary>
        /// Initializes a new instance of the LayerControllerLayerNameUpdatedEventArgs class
        /// </summary>
        /// <param name="frameLayer">The layer that was updated</param>
        /// <param name="oldLayerName">Gets the old name before the layer name was updated</param>
        public LayerControllerLayerNameUpdatedEventArgs(IFrameLayer frameLayer, string oldLayerName)
        {
            FrameLayer = frameLayer;
            OldLayerName = oldLayerName;
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayerDuplicated event
    /// </summary>
    public class LayerControllerLayerDuplicatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the index of the layer that was duplicated
        /// </summary>
        public int LayerIndex { get; }

        /// <summary>
        /// Creates a new instance of the LayerControllerLayerDuplicatedEventArgs class
        /// </summary>
        /// <param name="layerIndex">The index of the layer that was duplicated</param>
        public LayerControllerLayerDuplicatedEventArgs(int layerIndex)
        {
            LayerIndex = layerIndex;
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayersCombined event
    /// </summary>
    public class LayerControllerLayersCombinedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the layers that were combined
        /// </summary>
        public IFrameLayer[] LayersCombined { get; }

        /// <summary>
        /// Initializes a new instance of the LayerControllerLayersCombinedEventArgs class
        /// </summary>
        /// <param name="layersCombined">The layers that were combined</param>
        public LayerControllerLayersCombinedEventArgs(IFrameLayer[] layersCombined)
        {
            LayersCombined = layersCombined;
        }
    }

    /// <summary>
    /// Specifies the event arguments for a FrameChanged event
    /// </summary>
    public class LayerControllerFrameChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the frame that was changed to
        /// </summary>
        public Frame NewFrame { get; }

        /// <summary>
        /// Initializes a new instance of the FrameChangedEventArgs
        /// </summary>
        /// <param name="newFrame">The new frame that was changed to</param>
        public LayerControllerFrameChangedEventArgs(Frame newFrame)
        {
            NewFrame = newFrame;
        }
    }

    /// <summary>
    /// Specifies the event arguments for an ActiveLayerIndexChanged event
    /// </summary>
    public class ActiveLayerIndexChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new current active layer index
        /// </summary>
        public int ActiveLayerIndex { get; }

        /// <summary>
        /// Creates a new instance of the ActiveLayerIndexChangedEventArgs class
        /// </summary>
        /// <param name="activeLayerIndex">The new current active layer index</param>
        public ActiveLayerIndexChangedEventArgs(int activeLayerIndex)
        {
            ActiveLayerIndex = activeLayerIndex;
        }
    }
}
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
using Pixelaria.Data;

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
        /// The currently active layer index
        /// </summary>
        private int _activeLayerIndex;

        /// <summary>
        /// Delegate for the LayersSwapped event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayersSwappedEventHandler(object sender, LayerControllerLayersSwappedEventArgs args);
        /// <summary>
        /// Event fired whenever a call to SwapLayers is made
        /// </summary>
        public event LayersSwappedEventHandler LayersSwapped;

        /// <summary>
        /// Delegate for the LayerCreated event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayerCreatedEventHandler(object sender, LayerControllerLayerCreatedEventArgs args);
        /// <summary>
        /// Event fired whenever a call to CreateLayer or AddLayer is made
        /// </summary>
        public event LayerCreatedEventHandler LayerCreated;

        /// <summary>
        /// Delegate for the LayerRemoved event
        /// </summary>
        /// <param name="sender">The sender for the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void LayerRemovedEventHandler(object sender, LayerControllerLayerRemovedEventArgs args);
        /// <summary>
        /// Event fired whenever a call to RemoveLayer is made
        /// </summary>
        public event LayerRemovedEventHandler LayerRemoved;

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
        /// Event fired whenever the current active layer index is changed
        /// </summary>
        public event ActiveLayerIndexChangedEventHandler ActiveLayerIndexChanged;

        /// <summary>
        /// Gets or sets the current frame being controlled
        /// </summary>
        public Frame Frame
        {
            get { return _frame; }
            set
            {
                if (ReferenceEquals(_frame, value))
                    return;

                _frame = value;

                if (FrameChanged != null)
                {
                    FrameChanged(this, new LayerControllerFrameChangedEventArgs(value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently active layer index.
        /// When settings the value, it must be >= 0 and smaller than layer count
        /// </summary>
        /// 
        public int ActiveLayerIndex
        {
            get { return Math.Max(0, Math.Min(_frame.LayerCount - 1, _activeLayerIndex)); }
            set
            {
                if (_activeLayerIndex == value)
                    return;

                if(value < 0 || value >= _frame.LayerCount)
                    throw new ArgumentOutOfRangeException("value", @"The value specified must be >= 0 and smaller than the layer count");

                _activeLayerIndex = value;

                if (ActiveLayerIndexChanged != null)
                {
                    ActiveLayerIndexChanged(this, new ActiveLayerIndexChangedEventArgs(value));
                }
            }
        }

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
                    layers[i] = _frame.GetLayerAt(i);
                }

                return layers;
            }
        }

        /// <summary>
        /// Initializes a new instance of the LayerController class with a specified frame to control
        /// </summary>
        /// <param name="frame">The frame to control on this Layer Controller</param>
        public LayerController(Frame frame)
        {
            _frame = frame;
        }

        /// <summary>
        /// Creates a layer at the specified index
        /// </summary>
        /// <param name="layerIndex">The index at which to create the layer</param>
        public IFrameLayer CreateLayer(int layerIndex = -1)
        {
            IFrameLayer layer = _frame.CreateLayer(layerIndex);

            if (LayerCreated != null)
            {
                LayerCreated(this, new LayerControllerLayerCreatedEventArgs(layer));
            }

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
            IFrameLayer layer = _frame.AddLayer(bitmap, index);

            if (LayerCreated != null)
            {
                LayerCreated(this, new LayerControllerLayerCreatedEventArgs(layer));
            }

            return layer;
        }

        /// <summary>
        /// Swaps two layers from the frame
        /// </summary>
        /// <param name="layer1">The index of the first layer to swap</param>
        /// <param name="layer2">The index of the second layer to swap</param>
        public void SwapLayers(int layer1, int layer2)
        {
            _frame.SwapLayers(layer1, layer2);

            if (LayersSwapped != null)
            {
                LayersSwapped(this, new LayerControllerLayersSwappedEventArgs(layer1, layer2));
            }
        }

        /// <summary>
        /// Removes a layer that is at the specified index
        /// </summary>
        /// <param name="layerIndex">The index of the layer to remove</param>
        /// <param name="dispose">Whether to dispose of the layer that was removed</param>
        public void RemoveLayer(int layerIndex, bool dispose = true)
        {
            IFrameLayer layer = _frame.GetLayerAt(layerIndex);

            _frame.RemoveLayerAt(layerIndex, dispose);

            if (LayerRemoved != null)
            {
                LayerRemoved(this, new LayerControllerLayerRemovedEventArgs(layer));
            }
        }
    }

    /// <summary>
    /// Specifies the event arguments for a LayersSwapped event
    /// </summary>
    public class LayerControllerLayersSwappedEventArgs : EventArgs
    {
        /// <summary>
        /// The first layer that was swapped
        /// </summary>
        public int FirstLayer { get; private set; }

        /// <summary>
        /// The second layer that was swapped
        /// </summary>
        public int SecondLayer { get; private set; }

        /// <summary>
        /// Creates a new instance of the LayersSwappedEventArgs class
        /// </summary>
        /// <param name="firstLayer">The first layer that was swapped</param>
        /// <param name="secondLayer">The second layer that was swapped</param>
        public LayerControllerLayersSwappedEventArgs(int firstLayer, int secondLayer)
        {
            FirstLayer = firstLayer;
            SecondLayer = secondLayer;
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
        public IFrameLayer FrameLayer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the LayerCreatedEventArgs class
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
        public IFrameLayer FrameLayer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the LayerRemovedEventArgs class
        /// </summary>
        /// <param name="frameLayer">The layer that was removed</param>
        public LayerControllerLayerRemovedEventArgs(IFrameLayer frameLayer)
        {
            FrameLayer = frameLayer;
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
        public Frame NewFrame { get; private set; }

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
        public int ActiveLayerIndex { get; private set; }

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
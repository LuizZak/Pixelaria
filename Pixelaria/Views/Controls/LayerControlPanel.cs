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
using Pixelaria.Controllers.LayerControlling;
using Pixelaria.Data;
using Pixelaria.Views.Controls.LayerControls;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Control that is used to display an interface for the user to manage a frame's layers
    /// </summary>
    public class LayerControlPanel : LabeledPanel
    {
        /// <summary>
        /// The controller that this layer control panel uses to interact with the layers
        /// </summary>
        private LayerController _controller;

        /// <summary>
        /// The list of all currently registered layer controls
        /// </summary>
        private readonly List<LayerControl> _layerControls;

        /// <summary>
        /// Initializes a new instance of the LayerControlPanel class
        /// </summary>
        public LayerControlPanel()
            : this(null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the LayerControlPanel class with a layer controller
        /// </summary>
        /// <param name="controller">The layer controller to bind to this layer control panel</param>
        public LayerControlPanel(LayerController controller)
        {
            panelTitle = "Layers";

            _layerControls = new List<LayerControl>();

            ClearAllControls();

            if(_controller != null && _controller.Frame != null)
            {
                SetController(controller);
            }
        }

        /// <summary>
        /// Sets the layer controller for this LayerControlPanel instance
        /// </summary>
        /// <param name="controller">The controller for this LayerControlPanel</param>
        public void SetController(LayerController controller)
        {
            _controller = controller;

            _controller.LayerCreated += OnLayerCreated;
            _controller.LayerRemoved += OnLayerRemoved;
            _controller.LayersSwapped += OnLayersSwapped;
            _controller.FrameChanged += OnFrameChanged;

            if (_controller.Frame != null)
                LoadLayers();
        }

        // 
        // Frame Changed event handler
        // 
        private void OnFrameChanged(object sender, LayerControllerFrameChangedEventArgs args)
        {
            LoadLayers();
        }

        // 
        // Layers Swapped event handler
        // 
        private void OnLayersSwapped(object sender, LayerControllerLayersSwappedEventArgs args)
        {
            throw new NotImplementedException();
        }

        // 
        // Layer Removed event handler
        // 
        private void OnLayerRemoved(object sender, LayerControllerLayerRemovedEventArgs args)
        {
            throw new NotImplementedException();
        }

        // 
        // Layer Created event handler
        // 
        private void OnLayerCreated(object sender, LayerControllerLayerCreatedEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the layers from the currently bound controller
        /// </summary>
        private void LoadLayers()
        {
            ClearAllControls();

            if (_controller.Frame == null)
                return;

            IFrameLayer[] layers = _controller.FrameLayers;

            foreach (var layer in layers)
            {
                AddLayerControl(layer, false);
            }

            ArrangeControls();
        }

        /// <summary>
        /// Clears all the layer controls currently registered
        /// </summary>
        private void ClearAllControls()
        {
            foreach (var control in _layerControls)
            {
                control.Dispose();
            }

            _layerControls.Clear();
        }

        /// <summary>
        /// Adds a new layer control for a given frame layer
        /// </summary>
        /// <param name="layer">The layer to create the layer control out of</param>
        /// <param name="arrangeAfter">Whether to call the ArrangeControls method after adding the control</param>
        private void AddLayerControl(IFrameLayer layer, bool arrangeAfter = true)
        {
            LayerControl control = new LayerControl(layer);

            _layerControls.Add(control);

            Controls.Add(control);

            if (arrangeAfter)
            {
                ArrangeControls();
            }
        }

        /// <summary>
        /// Rearranges all the currently registered layer controls
        /// </summary>
        private void ArrangeControls()
        {
            const int x = 0;
            int y = 19;

            foreach (var control in _layerControls)
            {
                control.Location = new Point(x, y);

                y += control.Height + 2;
            }
        }
    }
}
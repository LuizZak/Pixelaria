﻿/*
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
using System.Linq;
using System.Windows.Forms;
using Pixelaria.Controllers.LayerControlling;
using Pixelaria.Data;

namespace Pixelaria.Views.Controls.LayerControls
{
    /// <summary>
    /// Control that is used to display an interface for the user to manage a frame's layers
    /// </summary>
    public partial class LayerControlPanel : UserControl
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
        /// Whether the user is currently swapping controls
        /// </summary>
        private bool _swappingControls;

        /// <summary>
        /// Gets the array of layer status for each layer
        /// </summary>
        public LayerStatus[] LayerStatuses
        {
            get { return _layerControls.Select(layer => layer.LayerStatus).ToArray(); }
        }

        /// <summary>
        /// Occurs whenever the status of any of the layer controls is changed
        /// </summary>
        public event EventHandler LayerStatusesUpdated;

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
            _layerControls = new List<LayerControl>();

            ClearAllControls();

            if(_controller != null && _controller.Frame != null)
            {
                SetController(controller);
            }

            InitializeComponent();
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
            _controller.ActiveLayerIndexChanged += OnActiveLayerIndexChanged;

            if (_controller.Frame != null)
                ReloadLayers();
        }

        /// <summary>
        /// Updates the display of the layers on this layer control panel
        /// </summary>
        public void UpdateLayersDisplay()
        {
            foreach (var control in _layerControls)
            {
                control.UpdateBitmapDisplay();
            }
        }

        // 
        // Frame Changed event handler
        // 
        private void OnFrameChanged(object sender, LayerControllerFrameChangedEventArgs args)
        {
            ReloadLayers();
        }

        // 
        // Layers Swapped event handler
        // 
        private void OnLayersSwapped(object sender, LayerControllerLayersSwappedEventArgs args)
        {
            LayerControl secondLayer = _layerControls[args.SecondLayerIndex];
            _layerControls[args.SecondLayerIndex] = _layerControls[args.FirstLayerIndex];
            _layerControls[args.FirstLayerIndex] = secondLayer;

            ArrangeControls();
        }

        // 
        // Layer Removed event handler
        // 
        private void OnLayerRemoved(object sender, LayerControllerLayerRemovedEventArgs args)
        {
            RemoveLayerControl(GetLayerControlForLayer(args.FrameLayer));
        }

        // 
        // Layer Created event handler
        // 
        private void OnLayerCreated(object sender, LayerControllerLayerCreatedEventArgs args)
        {
            AddLayerControl(args.FrameLayer);
        }

        // 
        // Active Layer Index Changed event handler
        // 
        private void OnActiveLayerIndexChanged(object sender, ActiveLayerIndexChangedEventArgs args)
        {
            UpdateActiveLayerDisplay();
        }

        /// <summary>
        /// Loads/reloads the layers from the currently bound controller
        /// </summary>
        public void ReloadLayers()
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

            UpdateActiveLayerDisplay();
        }

        /// <summary>
        /// Clears all the layer controls currently registered
        /// </summary>
        private void ClearAllControls()
        {
            SuspendLayout();

            while (_layerControls.Count > 0)
            {
                RemoveLayerControl(_layerControls[0], false);
            }

            ResumeLayout();

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

            control.LayerSelected += OnLayerControlSelected;
            control.LayerStatusChanged += OnLayerStatusChanged;
            control.DuplicateLayerSelected += OnDuplicateLayerSelected;
            control.RemoveLayerSelected += OnRemoveLayerSelected;
            control.LayerControlDragged += OnLayerControlDragged;

            control.LayerImagePressed += OnLayerImagePressed;
            control.LayerImageReleased += OnLayerImageReleased;

            _layerControls.Insert(layer.Index, control);

            pnl_container.Controls.Add(control);

            if (arrangeAfter)
            {
                ArrangeControls();
            }
        }

        /// <summary>
        /// Removes the specified layer control from this layer contro panel
        /// </summary>
        /// <param name="control">The layer control to remove</param>
        /// <param name="arrangeAfter">Whether to call the ArrangeControls method after removing the control</param>
        private void RemoveLayerControl(LayerControl control, bool arrangeAfter = true)
        {
            control.LayerStatusChanged -= OnLayerStatusChanged;
            control.LayerSelected -= OnLayerControlSelected;
            control.DuplicateLayerSelected -= OnDuplicateLayerSelected;
            control.RemoveLayerSelected -= OnRemoveLayerSelected;
            control.LayerControlDragged -= OnLayerControlDragged;

            control.LayerImagePressed -= OnLayerImagePressed;
            control.LayerImageReleased -= OnLayerImageReleased;

            control.Dispose();

            Controls.Remove(control);

            _layerControls.Remove(control);

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
            int y = 0;

            for (int i = _layerControls.Count - 1; i >= 0; i--)
            {
                var control = _layerControls[i];
                
                control.Location = new Point(x, y);
                control.UpdateDisplay();

                y += control.Height + 2;
            }
        }

        /// <summary>
        /// Updates the visual feedback for the currently active layer
        /// </summary>
        private void UpdateActiveLayerDisplay()
        {
            foreach (var control in _layerControls)
            {
                control.BackColor = (control.Layer.Index == _controller.ActiveLayerIndex ? Color.FromArgb(255, 200, 200, 200) : Color.FromKnownColor(KnownColor.Control));
            }
        }

        /// <summary>
        /// Gets the layer control for the specified layer, or null, if none was found
        /// </summary>
        /// <param name="layer">A valid IFrameLayer that is currently registered on this layer control panel</param>
        /// <returns>The layer control for the specified layer, or null, if none was found</returns>
        private LayerControl GetLayerControlForLayer(IFrameLayer layer)
        {
            return _layerControls.FirstOrDefault(control => ReferenceEquals(control.Layer, layer));
        }

        // 
        // Create New Layer button click
        // 
        private void btn_createNewLayer_Click(object sender, EventArgs e)
        {
            if (_controller.ActiveLayerIndex == _controller.LayerCount - 1)
            {
                _controller.ActiveLayerIndex = _controller.CreateLayer().Index;
            }
            else
            {
                _controller.ActiveLayerIndex = _controller.CreateLayer(_controller.ActiveLayerIndex + 1).Index;
            }
        }

        #region Layer Control event handlers

        // 
        // Layer Selected event handler
        // 
        private void OnLayerControlSelected(object sender, LayerControl control)
        {
            _controller.ActiveLayerIndex = control.Layer.Index;
        }
        // 
        // Layer Status Changed event handler
        // 
        private void OnLayerStatusChanged(object sender, LayerControlStatusChangedEventArgs args)
        {
            if (LayerStatusesUpdated != null)
            {
                LayerStatusesUpdated(this, new EventArgs());
            }
        }
        // 
        // Duplicate Layer layer control button click
        // 
        private void OnDuplicateLayerSelected(object sender, EventArgs eventArgs)
        {
            // Convert the sender
            LayerControl control = sender as LayerControl;
            if (control == null)
                return;

            _controller.DuplicateLayer(control.Layer.Index);
        }
        // 
        // Remove Layer layer control button click
        // 
        private void OnRemoveLayerSelected(object sender, EventArgs eventArgs)
        {
            // Convert the sender
            LayerControl control = sender as LayerControl;
            if (control == null)
                return;

            // Do not allow removing the only layer on the frame
            if (_controller.FrameLayers.Length == 1)
                return;

            _controller.RemoveLayer(control.Layer.Index, false);
        }
        // 
        // Layer Drag event handler
        // 
        private void OnLayerControlDragged(object sender, LayerControlDragEventArgs args)
        {
            // Get the index of the control being dragged
            LayerControl control = sender as LayerControl;
            if (control == null)
                return;

            _swappingControls = true;

            // Swap controls via the index of the controls
            int index = _layerControls.IndexOf(control);
            int newIndex = index + (args.DragDirection == LayerDragDirection.Down ? -1 : 1);

            var layerControl = _layerControls[index];
            _layerControls[index] = _layerControls[newIndex];
            _layerControls[newIndex] = layerControl;

            ArrangeControls();
        }

        // 
        // Layer Image Pressed mouse event handler
        // 
        private void OnLayerImagePressed(object sender, MouseEventArgs mouseEventArgs)
        {
            
        }
        // 
        // Layer Image Released mouse event handler
        // 
        private void OnLayerImageReleased(object sender, MouseEventArgs mouseEventArgs)
        {
            // Get the index of the control being dragged
            LayerControl control = sender as LayerControl;
            if (control == null || !_swappingControls)
                return;

            _swappingControls = false;

            int index = control.Layer.Index;
            int newIndex = _layerControls.IndexOf(control);

            if (index == newIndex)
                return;

            // Reset the layer control index
            var layerControl = _layerControls[index];
            _layerControls[index] = _layerControls[newIndex];
            _layerControls[newIndex] = layerControl;

            if (newIndex >= 0 && newIndex < _controller.LayerCount)
            {
                _controller.SwapLayers(index, newIndex);
            }
        }

        #endregion
    }
}
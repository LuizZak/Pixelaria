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
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixelariaLib.Controllers.LayerControlling;
using PixelariaLib.Data;

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
        private bool _movingControls;

        /// <summary>
        /// Whether to ignore layer status change events coming from the LayerControls
        /// </summary>
        private bool _ignoreLayerStatusEvents;

        /// <summary>
        /// Gets the LayerControl for the currently active layer
        /// </summary>
        public LayerControl ActiveLayerControl => _layerControls[_controller.ActiveLayerIndex];

        /// <summary>
        /// Gets the array of layer status for each layer
        /// </summary>
        public LayerStatus[] LayerStatuses
        {
            get { return _layerControls.Select(layer => layer.LayerStatus).ToArray(); }
        }

        /// <summary>
        /// Gets an array of all the layer controls that are currently selected
        /// </summary>
        public LayerControl[] SelectedControls
        {
            get { return _layerControls.Where(control => control.Selected).ToArray(); }
        }

        /// <summary>
        /// Gets an array of all the layers that are currently selected on the layer controls
        /// </summary>
        public IFrameLayer[] SelectedLayers
        {
            get { return _layerControls.Where(control => control.Selected).Select(control => control.Layer).ToArray(); }
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

            if(_controller?.Frame != null)
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
            _controller.LayerMoved += OnLayerMoved;
            _controller.FrameChanged += OnFrameChanged;
            _controller.ActiveLayerIndexChanged += OnActiveLayerIndexChanged;
            _controller.LayerNameUpdated += OnLayerNameUpdated;

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
        // Layer Moved event handler
        // 
        private void OnLayerMoved(object sender, [NotNull] LayerControllerLayerMovedEventArgs args)
        {
            SwapLayerControls(args.NewIndex, args.LayerIndex);
            ClearSelection();
        }

        // 
        // Layer Removed event handler
        // 
        private void OnLayerRemoved(object sender, [NotNull] LayerControllerLayerRemovedEventArgs args)
        {
            var layerControl = GetLayerControlForLayer(args.FrameLayer);
            if (layerControl != null)
                RemoveLayerControl(layerControl);

            // Update selected layer
            UpdateActiveLayerDisplay();
        }

        // 
        // Layer Created event handler
        // 
        private void OnLayerCreated(object sender, [NotNull] LayerControllerLayerCreatedEventArgs args)
        {
            AddLayerControl(args.FrameLayer);

            // Update selected layer
            UpdateActiveLayerDisplay();
        }

        // 
        // Layer Name Updated event handler
        // 
        private void OnLayerNameUpdated(object sender, [NotNull] LayerControllerLayerNameUpdatedEventArgs args)
        {
            // Update the display of the layer control associated with the layer
            GetLayerControlForLayer(args.FrameLayer)?.UpdateDisplay();
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
        /// Toggles the visibility of the non-active layers
        /// </summary>
        public void ToggleNonActiveLayersVisibility()
        {
            // The event can only happen when there is more than 1 layer registered
            if (_controller.LayerCount == 1)
                return;

            _ignoreLayerStatusEvents = true;

            // Find the index of the first non-active layer
            int index = _controller.ActiveLayerIndex == _controller.LayerCount - 1 ? _controller.LayerCount - 2 : _controller.LayerCount - 1;

            bool newValue = !_layerControls[index].LayerVisible;

            // Switch the layer visibility
            foreach (var control in _layerControls)
            {
                if(control.Layer.Index != _controller.ActiveLayerIndex)
                    control.LayerVisible = newValue;
            }

            _ignoreLayerStatusEvents = false;

            // Call the notification event
            LayerStatusesUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resets the transparencies of all the layers to be fully opaque again
        /// </summary>
        public void ResetTransparencies()
        {
            _ignoreLayerStatusEvents = true;

            // Switch the layer visibility
            foreach (var control in _layerControls)
            {
                control.Transparency = 1;
            }

            _ignoreLayerStatusEvents = false;

            // Call the notification event
            LayerStatusesUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Expands all layers in this layer control panel
        /// </summary>
        public void ExpandAll()
        {
            pnl_container.SuspendLayout();

            foreach (var control in _layerControls)
            {
                control.Collapsed = false;
            }

            pnl_container.ResumeLayout(true);

            ArrangeControls();
        }

        /// <summary>
        /// Collapses all layers in this layer control panel
        /// </summary>
        public void CollapseAll()
        {
            pnl_container.SuspendLayout();

            foreach (var control in _layerControls)
            {
                control.Collapsed = true;
            }

            pnl_container.ResumeLayout(true);

            // We perform two calls here so one re-orders the controls, while the second
            // shrinks the container panel and re-orders it again in the correct placements
            ArrangeControls();
            ArrangeControls();
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
        private void AddLayerControl([NotNull] IFrameLayer layer, bool arrangeAfter = true)
        {
            var control = new LayerControl(layer);

            control.LayerClicked += OnLayerControlClicked;
            control.LayerStatusChanged += OnLayerStatusChanged;
            control.DuplicateLayerSelected += OnDuplicateLayerSelected;
            control.RemoveLayerSelected += OnRemoveLayerSelected;
            control.LayerControlDragged += OnLayerControlDragged;
            control.LayerNameEdited += OnLayerNameEdited;
            control.LayerCollapseChanged += OnLayerCollapseChanged;

            control.LayerImagePressed += OnLayerImagePressed;
            control.LayerImageReleased += OnLayerImageReleased;

            _layerControls.Insert(layer.Index, control);

            pnl_container.Controls.Add(control);

            if (arrangeAfter)
            {
                ArrangeControls();
            }

            ClearSelection();
        }

        /// <summary>
        /// Removes the specified layer control from this layer control panel
        /// </summary>
        /// <param name="control">The layer control to remove</param>
        /// <param name="arrangeAfter">Whether to call the ArrangeControls method after removing the control</param>
        private void RemoveLayerControl([NotNull] LayerControl control, bool arrangeAfter = true)
        {
            control.Dispose();

            Controls.Remove(control);

            _layerControls.Remove(control);

            if (arrangeAfter)
            {
                ArrangeControls();
            }

            ClearSelection();
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
        /// Clears the selection of all the currently selected layers
        /// </summary>
        private void ClearSelection()
        {
            foreach (var control in _layerControls)
            {
                control.Selected = false;
            }
        }

        /// <summary>
        /// Combines all currently selected layers
        /// </summary>
        private void CombineLayers()
        {
            _controller.CombineLayers(SelectedLayers);
        }

        /// <summary>
        /// Shows the context menu to be displayed when the user right clicks on the layers
        /// </summary>
        private void ShowLayersContextMenu()
        {
            // Update usability of the buttons
            cmb_combineLayers.Enabled = SelectedControls.Length > 1;

            cms_layersRightClick.Show(MousePosition);
        }

        /// <summary>
        /// Gets the layer control for the specified layer, or null, if none was found
        /// </summary>
        /// <param name="layer">A valid IFrameLayer that is currently registered on this layer control panel</param>
        /// <returns>The layer control for the specified layer, or null, if none was found</returns>
        [CanBeNull]
        private LayerControl GetLayerControlForLayer(IFrameLayer layer)
        {
            return _layerControls.FirstOrDefault(control => ReferenceEquals(control.Layer, layer));
        }

        /// <summary>
        /// Swaps two layer controls over, using their indexes on the layer control array
        /// </summary>
        /// <param name="layerIndex1">The first layer control to swap</param>
        /// <param name="layerIndex2">The second layer control to swap</param>
        private void SwapLayerControls(int layerIndex1, int layerIndex2)
        {
            pnl_container.SuspendLayout();

            var layerControl1 = _layerControls[layerIndex1];
            var layerControl2 = _layerControls[layerIndex2];

            // Swap positions
            var loc1 = layerControl1.Location;
            layerControl1.Location = layerControl2.Location;
            layerControl2.Location = loc1;

            // Swap indices
            _layerControls[layerIndex1] = layerControl2;
            _layerControls[layerIndex2] = layerControl1;

            pnl_container.ResumeLayout(true);
        }

        // 
        // Create New Layer button click
        // 
        private void btn_createNewLayer_Click(object sender, EventArgs e)
        {
            _controller.ActiveLayerIndex = _controller.CreateLayer(_controller.ActiveLayerIndex + 1).Index;
        }
        
        // 
        // Expand All button click
        // 
        private void btn_expand_Click(object sender, EventArgs e)
        {
            ExpandAll();
        }

        // 
        // Collapse All button click
        // 
        private void btn_collapse_Click(object sender, EventArgs e)
        {
            CollapseAll();
        }

        #region Layer Control event handlers

        // 
        // Layer Selected event handler
        // 
        private void OnLayerControlClicked(object sender, EventArgs e)
        {
            if (!ModifierKeys.HasFlag(Keys.Shift) && !ModifierKeys.HasFlag(Keys.Control))
            {
                ClearSelection();

                _controller.ActiveLayerIndex = ((LayerControl)sender).Layer.Index;
            }
        }
        // 
        // Layer Status Changed event handler
        // 
        private void OnLayerStatusChanged(object sender, LayerControlStatusChangedEventArgs args)
        {
            if (_ignoreLayerStatusEvents)
                return;

            LayerStatusesUpdated?.Invoke(this, EventArgs.Empty);
        }
        // 
        // Duplicate Layer layer control button click
        // 
        private void OnDuplicateLayerSelected(object sender, EventArgs eventArgs)
        {
            // Convert the sender
            var control = sender as LayerControl;
            if (control == null)
                return;

            // Duplicate and select the layer
            _controller.ActiveLayerIndex = _controller.DuplicateLayer(control.Layer.Index).Index;
        }
        // 
        // Remove Layer layer control button click
        // 
        private void OnRemoveLayerSelected(object sender, EventArgs eventArgs)
        {
            // Convert the sender
            var control = sender as LayerControl;
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
            var control = sender as LayerControl;
            if (control == null)
                return;

            _movingControls = true;

            // Swap controls via the index of the controls
            int index = _layerControls.IndexOf(control);
            int newIndex = index + (args.DragDirection == LayerDragDirection.Down ? -1 : 1);

            if (newIndex < 0 || newIndex >= _controller.LayerCount)
                return;

            pnl_container.SuspendLayout();

            SwapLayerControls(index, newIndex);
            ArrangeControls();

            pnl_container.ResumeLayout();

            // Keep the swapped layer in focus
            pnl_container.ScrollControlIntoView(control);
        }

        // 
        // Layer Collapse changed
        // 
        private void OnLayerCollapseChanged(object sender, EventArgs e)
        {
            pnl_container.SuspendLayout();

            ArrangeControls();

            pnl_container.ResumeLayout();
        }

        // 
        // Layer Name Edited event handler
        // 
        private void OnLayerNameEdited(object sender, LayerRenameEventArgs e)
        {
            var control = sender as LayerControl;
            if (control == null)
                return;

            _controller.SetLayerName(control.Layer.Index, e.NewName);
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
            var control = sender as LayerControl;
            if (control == null)
                return;

            // Open context menu
            if (mouseEventArgs.Button == MouseButtons.Right)
            {
                // If no controls are selected, select the control that was pressed
                if (SelectedControls.Length == 0)
                {
                    control.Selected = true;
                }

                ShowLayersContextMenu();

                return;
            }

            // Select layers
            if (!_movingControls)
            {
                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    ClearSelection();

                    // Select all layers from the current active layer to the selected control layer
                    for (int i = Math.Min(ActiveLayerControl.Layer.Index, control.Layer.Index); i <= Math.Max(ActiveLayerControl.Layer.Index, control.Layer.Index); i++)
                    {
                        _layerControls[i].Selected = true;
                    }

                    return;
                }
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    // Select the control
                    control.Selected = !control.Selected;

                    return;
                }
            }

            ClearSelection();

            // Move layers
            if (!_movingControls)
                return;

            _movingControls = false;

            int index = control.Layer.Index;
            int newIndex = _layerControls.IndexOf(control);

            if (index == newIndex)
                return;

            // Reset the layer control index
            SwapLayerControls(index, newIndex);

            if (newIndex >= 0 && newIndex < _controller.LayerCount)
            {
                _controller.MoveLayer(index, newIndex);
            }
        }

        #endregion

        #region Layers Context Menu

        // 
        // Combine Layers context menu
        // 
        private void cmb_combineLayers_Click(object sender, EventArgs e)
        {
            CombineLayers();
        }

        #endregion

        // 
        // Container Panel mouse click
        // 
        private void pnl_container_Click(object sender, EventArgs e)
        {
            ClearSelection();
        }
    }
}
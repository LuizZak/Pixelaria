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
using System.ComponentModel;
using System.Windows.Forms;
using Pixelaria.Data;

namespace Pixelaria.Views.Controls.LayerControls
{
    /// <summary>
    /// Specifies a visual representation of a layer in which the user can interact with in order to manage a frame's layers
    /// </summary>
    [DefaultEvent("LayerStatusChanged")]
    public partial class LayerControl : UserControl
    {
        /// <summary>
        /// The layer this layer control is binded to
        /// </summary>
        private readonly IFrameLayer _layer;

        /// <summary>
        /// Whether the layer being displayed is currently visible
        /// </summary>
        private bool _layerVisible;

        /// <summary>
        /// Whether the layer being displayed is currently locked
        /// </summary>
        private bool _layerLocked;

        /// <summary>
        /// Gets or sets a value specifying whether the layer is visible
        /// </summary>
        public bool LayerVisible
        {
            get { return _layerVisible; }
            set
            {
                if (_layerVisible == value)
                    return;

                _layerVisible = value;

                UpdateDisplay();

                if (LayerStatusChanged != null)
                {
                    LayerStatusChanged(this, new LayerControlStatusChangedEventArgs(LayerStatus));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value specifying whether the layer is visible
        /// </summary>
        public bool LayerLocked
        {
            get { return _layerLocked; }
            set
            {
                if (_layerLocked == value)
                    return;

                _layerLocked = value;

                UpdateDisplay();

                if (LayerStatusChanged != null)
                {
                    LayerStatusChanged(this, new LayerControlStatusChangedEventArgs(LayerStatus));
                }
            }
        }

        /// <summary>
        /// Gets the display status for this layer control
        /// </summary>
        public LayerStatus LayerStatus
        {
            get
            {
                return new LayerStatus(_layerVisible, _layerLocked);
            }
        }

        /// <summary>
        /// Gets the layer this layer control is binded to
        /// </summary>
        public IFrameLayer Layer
        {
            get { return _layer; }
        }

        /// <summary>
        /// The delegate for the LayerStatusChanged event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The event args for the event</param>
        public delegate void LayerStatusChangedEventHandler(object sender, LayerControlStatusChangedEventArgs args);

        /// <summary>
        /// The event fired whenever the status of the layer currently displayed is changed by the user
        /// </summary>
        [Browsable(true)]
        [Description("The event fired whenever the status of the layer currently displayed is changed by the user")]
        public event LayerStatusChangedEventHandler LayerStatusChanged;

        /// <summary>
        /// Occurs whenever the user clicks the Duplicate Layer button
        /// </summary>
        public event EventHandler DuplicateLayerSelected;

        /// <summary>
        /// Occurs whenever the user clicks the Remove Layer button
        /// </summary>
        public event EventHandler RemoveLayerSelected;

        /// <summary>
        /// Delegate for the LayerSelected event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="layer">The layer that was selected</param>
        public delegate void LayerSelectedEventHandler(object sender, LayerControl layer);

        /// <summary>
        /// Event called whenever the user selects the layer
        /// </summary>
        public event LayerSelectedEventHandler LayerSelected;

        /// <summary>
        /// Initializes a new instance of the LayerControl class
        /// </summary>
        /// <param name="layer">The layer this control will bind to</param>
        public LayerControl(IFrameLayer layer)
        {
            InitializeComponent();
            _layer = layer;

            // Update startup values
            _layerVisible = true;
            _layerLocked = false;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the display of the current layer
        /// </summary>
        public void UpdateDisplay()
        {
            UpdateBitmapDisplay();

            lbl_layerName.Text = @"Layer " + (_layer.Index + 1);

            btn_visible.Image = _layerVisible ? Properties.Resources.filter_enable_icon : Properties.Resources.filter_disable_icon;
            btn_locked.Image = _layerLocked ? Properties.Resources.padlock_closed : Properties.Resources.padlock_open;
        }

        /// <summary>
        /// Updates the bitmap display for the layers
        /// </summary>
        public void UpdateBitmapDisplay()
        {
            pb_layerImage.Image = _layer.LayerBitmap;
            pb_layerImage.Invalidate();
        }

        // 
        // Layer Visible button
        // 
        private void btn_visible_Click(object sender, EventArgs e)
        {
            LayerVisible = !LayerVisible;
        }

        // 
        // Layer Locked button
        // 
        private void btn_locked_Click(object sender, EventArgs e)
        {
            LayerLocked = !LayerLocked;
        }

        // 
        // Duplicate Layer button click
        // 
        private void btn_duplicate_Click(object sender, EventArgs e)
        {
            if (DuplicateLayerSelected != null)
            {
                DuplicateLayerSelected(this, new EventArgs());
            }
        }

        // 
        // Remove Layer button click
        // 
        private void btn_remove_Click(object sender, EventArgs e)
        {
            if (RemoveLayerSelected != null)
            {
                RemoveLayerSelected(this, new EventArgs());
            }
        }

        // 
        // Layer Image picture box click
        // 
        private void pb_layerImage_Click(object sender, EventArgs e)
        {
            if (LayerSelected != null)
                LayerSelected(this, this);
        }
    }

    /// <summary>
    /// Represents the event arguments for the LayerControlStatusChanged event
    /// </summary>
    public class LayerControlStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Status for the layer control
        /// </summary>
        public LayerStatus Status { get; private set; }

        /// <summary>
        /// Initializes a new LayerControlStatusChangedEventArgs class
        /// </summary>
        /// <param name="status">The status for this event args object</param>
        public LayerControlStatusChangedEventArgs(LayerStatus status)
        {
            Status = status;
        }
    }

    /// <summary>
    /// Represents the display status of a layer on a layer control
    /// </summary>
    public struct LayerStatus
    {
        /// <summary>
        /// Whether the layer is visible
        /// </summary>
        public readonly bool Visible;

        /// <summary>
        /// Whetehr the layer is locked
        /// </summary>
        public readonly bool Locked;

        /// <summary>
        /// Creates a new LayerStatus struct
        /// </summary>
        /// <param name="visible">Whether the layer is currently visible</param>
        /// <param name="locked">Whether the layer is currently locked</param>
        public LayerStatus(bool visible, bool locked)
        {
            Visible = visible;
            Locked = locked;
        }
    }
}
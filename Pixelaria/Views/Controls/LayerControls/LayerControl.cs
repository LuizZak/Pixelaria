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

                btn_visible.Image = _layerVisible
                                        ? Properties.Resources.filter_enable_icon
                                        : Properties.Resources.filter_disable_icon;

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
                return new LayerStatus(_layerVisible);
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
        /// Initializes a new instance of the LayerControl class
        /// </summary>
        /// <param name="layer">The layer this control will bind to</param>
        public LayerControl(IFrameLayer layer)
        {
            InitializeComponent();
            _layer = layer;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the display of the current layer
        /// </summary>
        public void UpdateDisplay()
        {
            pb_layerImage.Image = _layer.LayerBitmap;
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
        /// Creates a new LayerStatus struct
        /// </summary>
        /// <param name="visible">Whether the layer is currently visible</param>
        public LayerStatus(bool visible)
        {
            Visible = visible;
        }
    }
}
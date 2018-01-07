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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixCore.Controls.ColorControls;
using PixCore.Geometry;
using Pixelaria.Data;
using Pixelaria.Properties;

using Utilities = Pixelaria.Utils.Utilities;

namespace Pixelaria.Views.Controls.LayerControls
{
    /// <summary>
    /// Specifies a visual representation of a layer in which the user can interact with in order to manage a frame's layers
    /// </summary>
    [DefaultEvent("LayerStatusChanged")]
    public partial class LayerControl : UserControl
    {
        /// <summary>
        /// Cached version of the layer hidden image
        /// </summary>
        private readonly Image _layerHiddenImage = Resources.filter_disable_icon;
        /// <summary>
        /// Cached version of the layer visible image
        /// </summary>
        private readonly Image _layerVisibleImage = Resources.filter_enable_icon;

        /// <summary>
        /// Cached version of the layer locked image
        /// </summary>
        private readonly Image _layerLockedImage = Resources.padlock_closed;
        /// <summary>
        /// Cached version of the layer unlocked image
        /// </summary>
        private readonly Image _layerUnlockedImage = Resources.padlock_open;

        /// <summary>
        /// Whether the user is currently dragging the layer around
        /// </summary>
        private bool _draggingLayer;

        /// <summary>
        /// Whether the user is currently pressing down on the layer bitmap
        /// </summary>
        private bool _pressingLayer;

        /// <summary>
        /// Whether the user is currently pressing down on the container (this) control
        /// </summary>
        private bool _pressingContainer;

        /// <summary>
        /// Whether to not update the transparency slider during Transparency property changes
        /// </summary>
        private bool _ignoreTransparencySliderUpdates;

        /// <summary>
        /// Specifies the point where the player pressed down on the layer's image
        /// </summary>
        private Point _layerPressPoint;

        /// <summary>
        /// Whether the layer control is currently collapsed
        /// </summary>
        private bool _collapsed;

        /// <summary>
        /// Whether the layer being displayed is currently visible
        /// </summary>
        private bool _layerVisible;

        /// <summary>
        /// Whether the layer being displayed is currently locked
        /// </summary>
        private bool _layerLocked;

        /// <summary>
        /// The transparency for the layer
        /// </summary>
        private float _transparency;

        /// <summary>
        /// Whether this layer control is currently selected
        /// </summary>
        private bool _selected;

        /// <summary>
        /// Whether the user is currently editing the layer's name
        /// </summary>
        private bool _editingName;

        /// <summary>
        /// The last active control before the layer edit operation started
        /// </summary>
        private Control _lastActiveControl;

        /// <summary>
        /// Gets or sets a value specifying whether the layer control is currently collapsed or expanded
        /// </summary>
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                if (_collapsed == value)
                    return;

                _collapsed = value;

                btn_collapse.Image = _collapsed ? Resources.action_add_grey : Resources.action_remove_gray;

                UpdateDisplay();
            }
        }

        /// <summary>
        /// Gets or sets a value specifying whether the layer is visible
        /// </summary>
        public bool LayerVisible
        {
            get => _layerVisible;
            set
            {
                if (_layerVisible == value)
                    return;

                _layerVisible = value;

                UpdateDisplay();

                LayerStatusChanged?.Invoke(this, new LayerControlStatusChangedEventArgs(LayerStatus));
            }
        }

        /// <summary>
        /// Gets or sets a value specifying whether the layer is visible
        /// </summary>
        public bool LayerLocked
        {
            get => _layerLocked;
            set
            {
                if (_layerLocked == value)
                    return;

                _layerLocked = value;

                UpdateDisplay();

                LayerStatusChanged?.Invoke(this, new LayerControlStatusChangedEventArgs(LayerStatus));
            }
        }

        /// <summary>
        /// Gets or sets the display transparency for the layer
        /// </summary>
        public float Transparency
        {
            get => _transparency;
            set
            {
                float clamped = Math.Min(1.0f, Math.Max(0.0f, value));

                if (Math.Abs(clamped - _transparency) < float.Epsilon)
                    return;

                _transparency = clamped;

                if (!_ignoreTransparencySliderUpdates)
                    tcs_transparency.CurrentValue = value;

                UpdateDisplay(false);

                LayerStatusChanged?.Invoke(this, new LayerControlStatusChangedEventArgs(LayerStatus));
            }
        }

        /// <summary>
        /// Gets the display status for this layer control
        /// </summary>
        public LayerStatus LayerStatus => new LayerStatus(_layerVisible, _layerLocked, _transparency);

        /// <summary>
        /// Gets the layer this layer control is binded to
        /// </summary>
        public IFrameLayer Layer { get; }

        /// <summary>
        /// Gets or sets a value specifying whether this layer control is currently selected
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value)
                    return;

                _selected = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The delegate for the LayerStatusChanged event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event args for the event</param>
        public delegate void LayerStatusChangedEventHandler(object sender, LayerControlStatusChangedEventArgs e);

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
        /// Delegate for the LayerClicked event
        /// </summary>
        public delegate void LayerClickedEventHandler(object sender, EventArgs e);
        /// <summary>
        /// Event called whenever the user clicks the layer
        /// </summary>
        public event LayerClickedEventHandler LayerClicked;

        /// <summary>
        /// Delegate for the LayerControlDragged event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void LayerControlDraggedEventHandler(object sender, LayerControlDragEventArgs e);
        /// <summary>
        /// Occurs whenever the user drags the layer in order to swap it with another layer up or down
        /// </summary>
        public event LayerControlDraggedEventHandler LayerControlDragged;

        /// <summary>
        /// Delegate for the LayerNameEdited event
        /// </summary>
        public delegate void LayerControlNameEditedEventHandler(object sender, LayerRenameEventargs e);
        /// <summary>
        /// Occurs whenever the user finishes editing the display name for a layer
        /// </summary>
        public event LayerControlNameEditedEventHandler LayerNameEdited;

        /// <summary>
        /// Occurs whenever the user presses on the layer image area
        /// </summary>
        public event MouseEventHandler LayerImagePressed;

        /// <summary>
        /// Occurs whenever the user releases the layer image area
        /// </summary>
        public event MouseEventHandler LayerImageReleased;

        /// <summary>
        /// Occurs whenever the user collapses/expands the layer through the button on the layer's interface
        /// </summary>
        public event EventHandler LayerCollapeChanged;

        /// <summary>
        /// Initializes a new instance of the LayerControl class
        /// </summary>
        /// <param name="layer">The layer this control will bind to</param>
        public LayerControl(IFrameLayer layer)
        {
            InitializeComponent();
            Layer = layer;

            // Update startup values
            _layerVisible = true;
            _layerLocked = false;
            _transparency = 1.0f;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the display of the current layer
        /// </summary>
        /// <param name="refreshBitmap">Whether to refresh the layer preview bitmap</param>
        public void UpdateDisplay(bool refreshBitmap = true)
        {
            LayoutItems();

            if (refreshBitmap)
            {
                UpdateBitmapDisplay();
            }

            if (string.IsNullOrEmpty(Layer.Name))
            {
                lbl_layerName.ForeColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
                lbl_layerName.Text = @"Layer " + (Layer.Index + 1);
            }
            else
            {
                lbl_layerName.ForeColor = Color.Black;
                lbl_layerName.Text = Layer.Name;
            }

            btn_visible.Image = _layerVisible ? _layerVisibleImage : _layerHiddenImage;
            btn_locked.Image = _layerLocked ? _layerLockedImage : _layerUnlockedImage;
        }

        /// <summary>
        /// Lays out the contents of this layer control, taking collapsing in consideration
        /// </summary>
        private void LayoutItems()
        {
            if (Collapsed)
            {
                btn_locked.Location = new Point(24, 19);
                btn_duplicate.Location = new Point(48, 19);
                btn_remove.Location = new Point(72, 19);

                Size = new Size(125, 43);

                pb_layerImage.Hide();
                tcs_transparency.Hide();
            }
            else
            {
                btn_locked.Location = new Point(3, 40);
                btn_duplicate.Location = new Point(3, 61);
                btn_remove.Location = new Point(3, 82);

                Size = new Size(125, 105);

                pb_layerImage.Show();
                tcs_transparency.Show();
            }
        }

        /// <summary>
        /// Updates the bitmap display for the layers
        /// </summary>
        public void UpdateBitmapDisplay()
        {
            pb_layerImage.Image = Layer.LayerBitmap;
            pb_layerImage.Invalidate();
        }

        /// <summary>
        /// Begins the edit layer name operation
        /// </summary>
        private void BeginEditLayerName()
        {
            _lastActiveControl = Utilities.FindFocusedControl(FindForm());

            _editingName = true;

            txt_layerNameEditBox.Text = lbl_layerName.Text;

            txt_layerNameEditBox.Visible = true;
            txt_layerNameEditBox.Focus();
            txt_layerNameEditBox.SelectAll();
        }

        /// <summary>
        /// Ends the layer name editing, optionally commiting the edit
        /// </summary>
        /// <param name="commit">Whether to commit the edit and edit the underlying layer's name</param>
        private void EndEditLayerName(bool commit)
        {
            if (!_editingName)
                return;

            // Do not fire any change notification if the label has not changed
            if (commit && txt_layerNameEditBox.Text != lbl_layerName.Text)
            {
                LayerNameEdited?.Invoke(this, new LayerRenameEventargs(txt_layerNameEditBox.Text));
            }

            _editingName = false;
            txt_layerNameEditBox.Visible = false;

            var form = FindForm();
            if (form != null)
                form.ActiveControl = _lastActiveControl;

            _lastActiveControl = null;
        }

        // 
        // OnPaintBackground event handler
        // 
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            
            // During a drag operation, draw a marquee around the control
            if(_draggingLayer || Selected)
            {
                Pen p = new Pen(Color.Black)
                {
                    DashStyle = DashStyle.Dash,
                    DashPattern = new[] { 2f, 2f },
                    Alignment = PenAlignment.Inset,
                    Width = 1
                };

                Rectangle rec = new Rectangle(Point.Empty, new Size(Width - 1, Height - 1));
                e.Graphics.DrawRectangle(p, rec);
            }
        }

        #region Event Handlers

        // 
        // Collapse/Expand button
        // 
        private void btn_collapse_Click(object sender, EventArgs e)
        {
            Collapsed = !Collapsed;

            LayerCollapeChanged?.Invoke(this, EventArgs.Empty);
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
            DuplicateLayerSelected?.Invoke(this, EventArgs.Empty);
        }

        // 
        // Remove Layer button click
        // 
        private void btn_remove_Click(object sender, EventArgs e)
        {
            RemoveLayerSelected?.Invoke(this, EventArgs.Empty);
        }

        // 
        // Layer Image picture box mouse down
        // 
        private void pb_layerImage_MouseDown(object sender, [NotNull] MouseEventArgs e)
        {
            _layerPressPoint = e.Location;
            _pressingLayer = true;

            LayerImagePressed?.Invoke(this, e);
        }

        // 
        // Layer Image picture box mouse move
        // 
        private void pb_layerImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_pressingLayer) return;

            if (_layerPressPoint.Distance(e.Location) > 20)
            {
                _draggingLayer = true;
                Invalidate();
            }

            if (_draggingLayer)
            {
                if (e.Location.Y < -pb_layerImage.Location.Y - 5)
                {
                    LayerControlDragged?.Invoke(this, new LayerControlDragEventArgs(LayerDragDirection.Up));
                }
                else if (e.Location.Y - pb_layerImage.Location.Y > Height + 5)
                {
                    LayerControlDragged?.Invoke(this, new LayerControlDragEventArgs(LayerDragDirection.Down));
                }
            }
        }

        // 
        // Layer Image picture box mouse up
        // 
        private void pb_layerImage_MouseUp(object sender, MouseEventArgs e)
        {
            _draggingLayer = false;
            _pressingLayer = false;

            if (LayerImageReleased != null)
            {
                LayerImageReleased(this, e);
                Invalidate();
            }

            if (!_draggingLayer && LayerClicked != null && e.Button == MouseButtons.Left)
                LayerClicked(this, EventArgs.Empty);
        }

        // 
        // Self mouse down
        // 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _pressingContainer = true;
        }

        // 
        // Self mouse move
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if(_pressingContainer)
            {
                if (_layerPressPoint.Distance(e.Location) > 20)
                {
                    _draggingLayer = true;
                    Invalidate();
                }
            }

            if (_draggingLayer)
            {
                if (e.Location.Y < -pb_layerImage.Location.Y - 5)
                {
                    LayerControlDragged?.Invoke(this, new LayerControlDragEventArgs(LayerDragDirection.Up));
                }
                else if (e.Location.Y - pb_layerImage.Location.Y > Height + 5)
                {
                    LayerControlDragged?.Invoke(this, new LayerControlDragEventArgs(LayerDragDirection.Down));
                }
            }
        }

        // 
        // Self mouse up
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _draggingLayer = false;
            _pressingContainer = false;

            if (LayerImageReleased != null)
            {
                LayerImageReleased(this, e);
                Invalidate();
            }

            if (!_draggingLayer && LayerClicked != null && e.Button == MouseButtons.Left)
                LayerClicked(this, EventArgs.Empty);
        }

        // 
        // Tiny Color Slider color changed event
        // 
        private void tcs_transparency_ColorChanged(object sender, [NotNull] ColorChangedEventArgs eventArgs)
        {
            _ignoreTransparencySliderUpdates = true;

            Transparency = eventArgs.NewColor.FloatAlpha;

            _ignoreTransparencySliderUpdates = false;
        }

        // 
        // Layer Name label double click
        // 
        private void lbl_layerName_DoubleClick(object sender, EventArgs e)
        {
            BeginEditLayerName();
        }

        // 
        // Layer Name text box key down
        // 
        private void txt_layerNameEditBox_KeyDown(object sender, [NotNull] KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                EndEditLayerName(true);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                EndEditLayerName(false);
            }
        }

        // 
        // Layer Name text box focus leave
        // 
        private void txt_layerNameEditBox_Leave(object sender, EventArgs e)
        {
            EndEditLayerName(true);
        }

        #endregion
    }

    /// <summary>
    /// Represents the event arguments for the LayerStatusChanged event
    /// </summary>
    public class LayerControlStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Status for the layer control
        /// </summary>
        public LayerStatus Status { get; }

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
    /// Represents the event arguments for the LayerControlDragged event
    /// </summary>
    public class LayerControlDragEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the direction of the drag
        /// </summary>
        public LayerDragDirection DragDirection { get; }

        /// <summary>
        /// Initializes a new instance of the LayerControlDragEventArgs class
        /// </summary>
        /// <param name="dragDirection">The direction of the drag</param>
        public LayerControlDragEventArgs(LayerDragDirection dragDirection)
        {
            DragDirection = dragDirection;
        }
    }

    /// <summary>
    /// Represents the event arguments for the LayercontrolNameEditedEvent event
    /// </summary>
    public class LayerRenameEventargs : EventArgs
    {
        /// <summary>
        /// Gets the new name of the layer
        /// </summary>
        public string NewName { get; }

        public LayerRenameEventargs(string newName)
        {
            NewName = newName;
        }
    }

    /// <summary>
    /// Represents the display status of a layer on a layer control
    /// </summary>
    public readonly struct LayerStatus
    {
        /// <summary>
        /// Whether the layer is visible
        /// </summary>
        public readonly bool Visible;

        /// <summary>
        /// Whether the layer is locked
        /// </summary>
        public readonly bool Locked;

        /// <summary>
        /// The display transparency for the layer, ranging from 0 - 1
        /// </summary>
        public readonly float Transparency;

        /// <summary>
        /// Creates a new LayerStatus struct
        /// </summary>
        /// <param name="visible">Whether the layer is currently visible</param>
        /// <param name="locked">Whether the layer is currently locked</param>
        /// <param name="transparency">The display transparency for the layer, ranging from 0 - 1</param>
        public LayerStatus(bool visible, bool locked, float transparency)
        {
            Visible = visible;
            Locked = locked;
            Transparency = transparency;
        }
    }

    /// <summary>
    /// Specifies the direction of the drag for a layer
    /// </summary>
    public enum LayerDragDirection
    {
        /// <summary>
        /// Specifies that the direction dragged was upwards
        /// </summary>
        Up,
        /// <summary>
        /// Specifies that the direction dragged was downards
        /// </summary>
        Down
    }
}
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

using JetBrains.Annotations;
using PixCore.Text;
using PixRendering;
using System;

namespace PixUI.Controls.ContextMenu
{
    /// <summary>
    /// An item for a <see cref="ContextMenuControl"/> that displays a label, with an optional image attached.
    /// </summary>
    public class ContextMenuItem: ContextMenuItemBase
    {
        private bool _selected = false;
        private AttributedText _attributedName = new AttributedText();

        /// <summary>
        /// Gets or sets the display name for this context menu item.
        /// 
        /// Setting this value resets <see cref="AttributedName"/>.
        /// </summary>
        [NotNull]
        public string Name
        {
            get { return AttributedName.String; }
            set { AttributedName = new AttributedText(value); }
        }

        /// <summary>
        /// Gets or sets the attributed name for this context menu item.
        /// </summary>
        [NotNull]
        public AttributedText AttributedName
        {
            get { return _attributedName; }
            set
            {
                _attributedName = value;
                AttributedNameChanged?.Invoke(this, _attributedName);
            }
        }

        /// <summary>
        /// Changes the selected status of this context menu item.
        /// </summary>
        public bool Selected
        {
            get { return _selected; }
            set {
                if (_selected == value)
                    return;

                _selected = value;
                SelectChange?.Invoke(this, _selected);
            }
        }

        #region Events

        public delegate void SelectChangeEventHandler(object sender, bool selected);
        public delegate void AttributedNameChangedEventHandler(object sender, AttributedText attributedName);

        /// <summary>
        /// Event raised when <see cref="AttributedName"/> or <see cref="Name"/> changes.
        /// </summary>
        public event AttributedNameChangedEventHandler AttributedNameChanged;

        /// <summary>
        /// Event raised when <see cref="Selected"/> changes.
        /// </summary>
        public event SelectChangeEventHandler SelectChange;

        /// <summary>
        /// Event raised when the mouse has entered this context menu item.
        /// </summary>
        public event EventHandler MouseEnter;

        /// <summary>
        /// Event raised when the moust has left this context menu item.
        /// </summary>
        public event EventHandler MouseLeave;

        /// <summary>
        /// Event raised when the user has selected this context menu item with the mouse.
        /// </summary>
        public event EventHandler Click;

        #endregion

        public ContextMenuItem([NotNull] string value) : this(value, null)
        {

        }

        public ContextMenuItem([NotNull] string value, ImageResource image) : this(value, null)
        {
            Image = image;
        }

        public ContextMenuItem([NotNull] string value, IManagedImageResource managedImage)
        {
            Name = value;
            ManagedImage = managedImage;
        }

        /// <summary>
        /// Changes the selection state of this context menu item to be selected.
        /// </summary>
        public void Select()
        {
            Selected = true;
        }

        /// <summary>
        /// Invokes <see cref="Click"/> event handler for this item.
        /// </summary>
        public void PerformClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="MouseEnter"/> event.
        /// </summary>
        internal void OnMouseEnter(object sender, EventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MouseLeave"/> event.
        /// </summary>
        internal void OnMouseLeave(object sender, EventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="Click"/> event.
        /// </summary>
        internal void OnClick(object sender, EventArgs e)
        {
            Click?.Invoke(this, e);
        }

        void AttributedName_modified(object sender, EventArgs args)
        {
            AttributedNameChanged?.Invoke(this, AttributedName);
        }

        internal override ContextMenuControl.ContextMenuItemViewBase CreateContextMenuItemView()
        {
            return ContextMenuControl.ContextMenuItemView.Create(this);
        }
    }
}
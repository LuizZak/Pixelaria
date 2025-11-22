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
using PixRendering;

namespace PixUI.Controls.ContextMenu
{
    public abstract class ContextMenuItemBase
    {
        private bool visible = true;

        /// <summary>
        /// Gets or sets a flag determining if this context menu item is visible.
        /// </summary>
        public bool Visible
        {
            get => visible;
            set
            {
                if (value == visible) return;

                visible = value;
                VisibleChanged?.Invoke(this, visible);
            }
        }

        /// <summary>
        /// The image displayed alongside this drop down item
        /// </summary>
        public ImageResource? Image { get; set; }

        /// <summary>
        /// The managed image to render alongside this drop down item.
        ///
        /// Overrides the value configured in <see cref="Image"/>.
        /// </summary>
        [CanBeNull]
        public IManagedImageResource ManagedImage { get; set; }

        /// <summary>
        /// The drop down item that contains this menu item.
        ///
        /// May be null, in case this context menu item has no parent.
        /// </summary>
        [CanBeNull]
        public ContextMenuDropDownItem DropDownItem { get; internal set; }

        /// <summary>
        /// Gets the index of this context menu item on its parent drop down item.
        /// 
        /// If this item is not added to a parent, -1 is returned.
        /// </summary>
        public int Index => DropDownItem?.DropDownItems.IndexOf(this) ?? -1;

        #region Events

        /// <summary>
        /// Event delegate for the <see cref="VisibleChanged"/> event.
        /// </summary>
        public delegate void VisibleChangedEventHandler(object sender, bool value);

        public event VisibleChangedEventHandler VisibleChanged;

        #endregion

        internal abstract ContextMenuControl.ContextMenuItemViewBase CreateContextMenuItemView();
    }
}

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

namespace PixUI.Controls.ContextMenu
{
    /// <summary>
    /// A context menu item that hosts another control within its bounds.
    /// </summary>
    public class ContextMenuControlHostItem: ContextMenuItemBase
    {
        internal ControlView control;

        /// <summary>
        /// Whether to create layout constraints to constrain the control within the context menu view item.
        /// 
        /// If <c>false</c>, constraints the control by positional location, and auto-sizing forces the context menu
        /// to be at least the size of the control.
        /// </summary>
        public bool CreateConstraints { get; set; } = true;

        /// <summary>
        /// The minimum width to contain the control in.
        /// </summary>
        public int MinimumWidth { get; set; } = 8;

        public ContextMenuControlHostItem(ControlView control) : base()
        {
            this.control = control;
        }

        internal override ContextMenuControl.ContextMenuItemViewBase CreateContextMenuItemView()
        {
            return ContextMenuControl.ContextMenuControlHostItemView.Create(this);
        }
    }
}

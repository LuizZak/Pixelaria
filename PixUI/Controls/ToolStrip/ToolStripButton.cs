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

using PixRendering;

namespace PixUI.Controls.ToolStrip
{
    /// <summary>
    /// A menu item containing a button with an image and an optional label
    /// </summary>
    public class ToolStripButton : ToolStripMenuItem
    {
        private ImageResource _image;

        /// <summary>
        /// Gets or sets the display image for this tool strip menu item
        /// </summary>
        public ImageResource Image
        {
            get => _image;
            set
            {
                menu?.InvalidateItem(this);
                _image = value;
                menu?.InvalidateItem(this);
            }
        }

        internal override ToolStripMenuItemKind Kind => ToolStripMenuItemKind.Button;
    }
}

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
using JetBrains.Annotations;
using PixCore.Geometry;

namespace PixUI.Controls.ToolStrip
{
    /// <summary>
    /// Class that internally manages the views for menu items within a <see cref="ToolStripMenu"/>
    /// </summary>
    internal class ToolStripMenuViewManager
    {
        /// <summary>
        /// Returns the relative rect for a given <see cref="ToolStripMenuItem"/>.
        /// </summary>
        public AABB RectForItem([NotNull] ToolStripMenuItem item, [NotNull] ToolStripMenu menu)
        {
            switch (item.Kind)
            {
                case ToolStripMenuItemKind.Button:
                    return AABB.FromRectangle(0, 0, 22, 22);
                    
                case ToolStripMenuItemKind.Separator:

                    switch (menu.Orientation)
                    {
                        case ToolStripOrientation.Horizontal:
                            return new AABB(0, 0, 6, menu.barSize);
                            
                        case ToolStripOrientation.Vertical:
                            return new AABB(0, 0, menu.barSize, 6);

                        default:
                            throw new ArgumentOutOfRangeException(nameof(menu.Orientation), menu.Orientation, null);
                    }
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

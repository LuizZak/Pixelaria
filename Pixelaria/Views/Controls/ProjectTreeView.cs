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

using System.Windows.Forms;

using Pixelaria.Data;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Represents a TreeView that can be used to display a ProjectTree object
    /// </summary>
    public class ProjectTreeView : RearrangeableTreeView
    {
        /// <summary>
        /// The project tree this project tree view is currently representing
        /// </summary>
        ProjectTree _loadedProjectTree;

        /// <summary>
        /// Gets or sets the project tree this project tree view is currently representing
        /// </summary>
        public ProjectTree LoadedProjectTree
        {
            get { return _loadedProjectTree; }
            set { _loadedProjectTree = value; }
        }
        /*
        /// <summary>
        /// Loads the given ProjectTree object into this ProjectTreeView
        /// </summary>
        /// <param name="tree">The project tree to load on this ProjectTreeView</param>
        private void LoadProjectTree(ProjectTree tree)
        {
            _loadedProjectTree = tree;
        }

        // 
        // OnItemDrag event handler. Starts dragging a node
        // 
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            TreeNode node = e.Item as TreeNode;

            base.OnItemDrag(e);
        }*/
    }
}
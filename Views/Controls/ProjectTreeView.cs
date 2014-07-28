using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        ProjectTree loadedProjectTree;

        /// <summary>
        /// Gets or sets the project tree this project tree view is currently representing
        /// </summary>
        public ProjectTree LoadedProjectTree
        {
            get { return loadedProjectTree; }
            set { loadedProjectTree = value; }
        }

        /// <summary>
        /// Loads the given ProjectTree object into this ProjectTreeView
        /// </summary>
        /// <param name="tree">The project tree to load on this ProjectTreeView</param>
        private void LoadProjectTree(ProjectTree tree)
        {
            this.loadedProjectTree = tree;
        }

        // 
        // OnItemDrag event handler. Starts dragging a node
        // 
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            TreeNode node = e.Item as TreeNode;

            base.OnItemDrag(e);
        }
    }
}
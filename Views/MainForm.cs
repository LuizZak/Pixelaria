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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Pixelaria.Controllers;

using Pixelaria.Data;

using Pixelaria.Views.ModelViews;
using Pixelaria.Views.SettingsViews;
using Pixelaria.Views.Controls;

namespace Pixelaria.Views
{
    /// <summary>
    /// The main form of the application
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// The Controller instance that owns this MainForm
        /// </summary>
        public Controller controller;

        /// <summary>
        /// Event handler for the recent file menu item list click
        /// </summary>
        private EventHandler recentFileClick;

        /// <summary>
        /// Creates a new instance of the MainForm class
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public MainForm(string[] args)
        {
            InitializeComponent();

            // Enable double buffering on the MDI client to avoid flickering while redrawing
            foreach (Control control in this.Controls)
            {
                if (control is MdiClient)
                {
                    MethodInfo method = ((MdiClient)control).GetType().GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
                    method.Invoke((MdiClient)control, new Object[] { ControlStyles.OptimizedDoubleBuffer, true });
                }
            }

            this.Menu = this.mm_menu;

            this.il_treeView.Images.SetKeyName(2, "EMPTY");

            this.recentFileClick = new EventHandler(mi_fileItem_Click);

            // Hook up the TreeView event handlers
            this.tv_bundleAnimations.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(AnimationNodeDoubleClickHandler);
            this.tv_bundleAnimations.NodeMouseClick += new TreeNodeMouseClickEventHandler(TreeViewNodeClickHandler);
            this.tv_bundleAnimations.DragOperation += new RearrangeableTreeView.DragOperationHandler(TreeViewDragOperationHandler);
            this.tv_bundleAnimations.MouseDown += new MouseEventHandler(TreeViewMouseDown);

            this.controller = new Controller(this);

            if (args.Length > 0 && File.Exists(args[0]))
            {
                this.controller.LoadBundleFromFile(args[0]);
            }
        }

        /// <summary>
        /// Loads the given bundle into this interface
        /// </summary>
        /// <param name="bundle">The bundle to load</param>
        public void LoadBundle(Bundle bundle)
        {
            CloseAllWindows();
            UpdateTitleBar(bundle);
            UpdateAnimationsTreeView(bundle);

            // Open the root node
            tv_bundleAnimations.Nodes[0].Expand();
        }

        /// <summary>
        /// Closes all MDI windows currently opened
        /// </summary>
        public void CloseAllWindows()
        {
            foreach (Form form in MdiChildren)
            {
                form.Close();
                form.Dispose();
            }
        }

        /// <summary>
        /// Updates the title bar with the information of the given bundle
        /// </summary>
        /// <param name="bundle">The bundle to fill the title bar with information of</param>
        public void UpdateTitleBar(Bundle bundle)
        {
            this.Text = "Pixelaria v1.85 [" + bundle.Name + "]" + (controller.UnsavedChanges ? "*" : "");
        }

        /// <summary>
        /// Fills in the animations tree view with the animation information of the given bundle
        /// </summary>
        /// <param name="bundle">The bundle to load the animations from</param>
        public void UpdateAnimationsTreeView(Bundle bundle)
        {
            // Clear the nodes
            tv_bundleAnimations.Nodes.Clear();

            // Clear the thumbnails
            while(il_treeView.Images.Count > 3)
            {
                il_treeView.Images[3].Dispose();
                il_treeView.Images.RemoveAt(3);
            }

            // Start filling the tree view again
            TreeNode bundleNode = tv_bundleAnimations.Nodes.Add("Bundle");

            bundleNode.Tag = bundle;

            // Add the animation sheet nodes
            foreach (AnimationSheet sheet in bundle.AnimationSheets)
            {
                AddAnimationSheet(sheet);
            }

            // Add the animation nodes
            foreach (Animation anim in bundle.Animations)
            {
                AddAnimation(anim);
            }

            // Move nodes from the root to their respective AnimationSheets
            foreach (AnimationSheet sheet in bundle.AnimationSheets)
            {
                TreeNode sheetNode = GetTreeNodeFor(sheet);
                foreach (Animation anim in sheet.Animations)
                {
                    // Get the animation node
                    TreeNode animNode = GetTreeNodeFor(anim);

                    animNode.Remove();

                    sheetNode.Nodes.Add(animNode);
                }
            }
        }

        /// <summary>
        /// Updates the icons of all the nodes on the tree view
        /// </summary>
        public void UpdateTreeViewIcons()
        {
            Queue<TreeNode> nodeQueue = new Queue<TreeNode>();

            nodeQueue.Enqueue(tv_bundleAnimations.Nodes[0]);

            while(nodeQueue.Count != 0)
            {
                TreeNode node = nodeQueue.Dequeue();

                if(node.Tag is Animation)
                {
                    Animation tag = node.Tag as Animation;

                    node.ImageKey = node.SelectedImageKey = (tag.Name + tag.ID);
                }

                // Enqueue the children nodes
                foreach (TreeNode childNode in node.Nodes)
                {
                    nodeQueue.Enqueue(childNode);
                }
            }
        }

        /// <summary>
        /// Updates the recent files list
        /// </summary>
        public void UpdateRecentFilesList()
        {
            // Remove the event listeners first
            foreach (MenuItem item in mi_recentFiles.MenuItems)
            {
                item.Click -= recentFileClick;
            }

            mi_recentFiles.MenuItems.Clear();
            
            // Start adding the files now
            for (int i = 0; i < controller.CurrentRecentFileList.FileCount; i++)
            {
                string path = controller.CurrentRecentFileList[i];

                if (path == "")
                    continue;

                MenuItem item = new MenuItem((i + 1) + " - " + (path == "" ? "--" : Path.GetFileName(path)));

                item.Tag = i;
                item.Click += recentFileClick;

                mi_recentFiles.MenuItems.Add(item);
            }

            if (mi_recentFiles.MenuItems.Count == 0)
            {
                MenuItem item = new MenuItem("No recent files");
                item.Enabled = false;

                mi_recentFiles.MenuItems.Add(item);
            }
        }

        /// <summary>
        /// Updates the interface to reflect the values of the Unsaved Changes flag.
        /// </summary>
        /// <param name="isUnsaved">The  current Unsaved Changes flag</param>
        public void UnsavedChangesUpdated(bool isUnsaved)
        {
            UpdateTitleBar(controller.CurrentBundle);
        }

        /// <summary>
        /// Adds the given Animation to this form
        /// </summary>
        /// <param name="animation">The animation to add</param>
        /// <param name="selectOnAdd">Whether to select the sheet's node after it's added to the interface</param>
        public void AddAnimation(Animation animation, bool selectOnAdd = false)
        {
            TreeNode parentNode = tv_bundleAnimations.Nodes[0];

            // If the animation is owned by a sheet, set the sheet's tree node as the parent for the animation node
            AnimationSheet sheet = controller.GetOwningAnimationSheet(animation);
            if (sheet != null)
            {
                parentNode = GetTreeNodeFor(sheet);
            }

            // Create the tree node now
            il_treeView.Images.Add(animation.Name + animation.ID, animation.GetFrameAtIndex(0).GenerateThumbnail(16, 16, true, true, Color.White));

            int addIndex = controller.GetAnimationIndex(animation);

            // If the target node is the bundle root, add the index of the animation sheets to the target add index
            if (parentNode.Tag is Bundle)
            {
                foreach (TreeNode node in tv_bundleAnimations.Nodes[0].Nodes)
                {
                    if (node.Tag is AnimationSheet)
                    {
                        addIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            TreeNode animNode = parentNode.Nodes.Insert(addIndex, animation.Name);

            animNode.Tag = animation;

            animNode.ImageKey = animNode.SelectedImageKey = animation.Name + animation.ID;

            if (selectOnAdd)
            {
                // Select the new animation node
                tv_bundleAnimations.SelectedNode = animNode;
                tv_bundleAnimations.SelectedNode.EnsureVisible();
            }
        }

        /// <summary>
        /// Removes the given Animation's representation from this form
        /// </summary>
        /// <param name="anim">The animation to remove</param>
        public void RemoveAnimation(Animation anim)
        {
            foreach (Form curView in this.MdiChildren)
            {
                if (curView is AnimationView && ((AnimationView)curView).CurrentAnimation == anim)
                {
                    curView.Close();
                    break;
                }
            }

            // Remove the animation's treenode
            TreeNode animNode = GetTreeNodeFor(anim);

            il_treeView.Images.RemoveByKey(animNode.ImageKey);

            animNode.Remove();

            UpdateTreeViewIcons();
        }

        /// <summary>
        /// Updates this form's reprensentation of the given Animation
        /// </summary>
        /// <param name="animation">The Animation to update the representation of</param>
        public void UpdateAnimation(Animation animation)
        {
            // Seek the TreeView and update the treenode
            TreeNode animNode = GetTreeNodeFor(animation);

            animNode.Text = animation.Name;

            // Update the node's thumb
            if (il_treeView.Images.ContainsKey(animation.Name + animation.ID) && animation.FrameCount == 0)
            {
                il_treeView.Images.RemoveByKey(animation.Name + animation.ID);
            }

            if (animation.FrameCount > 0)
            {
                if (!il_treeView.Images.ContainsKey(animation.Name + animation.ID))
                {
                    il_treeView.Images.Add(animation.Name + animation.ID, animation.GetFrameAtIndex(0).GenerateThumbnail(16, 16, true, true, Color.White));
                    animNode.ImageKey = animNode.SelectedImageKey = animation.Name + animation.ID;
                }
                else
                {
                    il_treeView.Images[(animNode.ImageIndex == -1 ? il_treeView.Images.IndexOfKey(animNode.ImageKey) : animNode.ImageIndex)] = animation.GetFrameAtIndex(0).GenerateThumbnail(16, 16, true, true, Color.White);
                }
            }
            else
            {
                animNode.ImageKey = animNode.SelectedImageKey = "EMPTY";
            }

            UpdateTreeViewIcons();
        }

        /// <summary>
        /// Opens a view for the given animation (or brings a view already binded to this animation to the front)
        /// </summary>
        /// <param name="animation">The Animation to open the AnimationView for</param>
        /// <returns>The created AnimationView</returns>
        public AnimationView OpenViewForAnimation(Animation animation)
        {
            foreach (Form curView in this.MdiChildren)
            {
                if (curView is AnimationView && ((AnimationView)curView).CurrentAnimation == animation)
                {
                    curView.BringToFront();
                    curView.Focus();
                    return ((AnimationView)curView);
                }
            }

            AnimationView view = new AnimationView(controller, animation);

            view.MdiParent = this;
            view.Show();
            view.BringToFront();

            return view;
        }

        /// <summary>
        /// Adds the given Animation Sheet to this form
        /// </summary>
        /// <param name="sheet">The animation to add</param>
        /// <param name="selectOnAdd">Whether to select the sheet's node after it's added to the interface</param>
        public void AddAnimationSheet(AnimationSheet sheet, bool selectOnAdd = false)
        {
            TreeNode bundleNode = tv_bundleAnimations.Nodes[0];

            // Find a new valid node position for the animation sheet
            int nodePos = 0;

            for (nodePos = 0; nodePos < bundleNode.Nodes.Count; nodePos++)
            {
                if (bundleNode.Nodes[nodePos].Tag is Animation)
                {
                    break;
                }
            }

            // Create the tree node now
            TreeNode sheetNode = bundleNode.Nodes.Insert(nodePos, sheet.Name);

            sheetNode.Tag = sheet;

            sheetNode.ImageIndex = sheetNode.SelectedImageIndex = 1;

            if (selectOnAdd)
            {
                // Select the new animation sheet node
                tv_bundleAnimations.SelectedNode = sheetNode;
                tv_bundleAnimations.SelectedNode.EnsureVisible();
            }
        }

        /// <summary>
        /// Removes the given AnimationSheet's representation from this form
        /// </summary>
        /// <param name="sheet">The sheet to remove</param>
        public void RemoveAnimationSheet(AnimationSheet sheet)
        {
            foreach (Form curView in this.MdiChildren)
            {
                if (curView is AnimationSheetView && ((AnimationSheetView)curView).CurrentSheet == sheet)
                {
                    curView.Close();
                    break;
                }
            }

            // Remove the sheet's treenode
            TreeNode sheetNode = GetTreeNodeFor(sheet);

            sheetNode.Remove();
        }

        /// <summary>
        /// Updates this form's reprensentation of the given AnimationSheet
        /// </summary>
        /// <param name="sheet">The AnimationSheet to update the representation of</param>
        public void UpdateAnimationSheet(AnimationSheet sheet)
        {
            // Seek the TreeView and update the treenode
            TreeNode node = GetTreeNodeFor(sheet);

            node.Text = sheet.Name;
        }

        /// <summary>
        /// Opens a view for the given animation (or brings a view already binded to this animation to the front)
        /// </summary>
        /// <param name="sheet">The Animation to open the AnimationView for</param>
        /// <returns>The created AnimationView</returns>
        public AnimationSheetView OpenViewForAnimationSheet(AnimationSheet sheet)
        {
            foreach (Form curView in this.MdiChildren)
            {
                if (curView is AnimationSheetView && ((AnimationSheetView)curView).CurrentSheet == sheet)
                {
                    curView.BringToFront();
                    curView.Focus();
                    return ((AnimationSheetView)curView);
                }
            }

            AnimationSheetView view = new AnimationSheetView(controller, sheet);

            view.MdiParent = this;
            view.Show();
            view.BringToFront();

            return view;
        }

        /// <summary>
        /// Opens the bundle settings dialog for the given bundle
        /// </summary>
        public void OpenBundleSettings(Bundle bundle)
        {
            BundleSettingsView bsv = new BundleSettingsView(controller, bundle);

            if (bsv.ShowDialog(this) == DialogResult.OK)
            {
                UpdateTitleBar(bundle);
            }
        }

        /// <summary>
        /// Displays an interface for creating a new bundle
        /// </summary>
        private void NewBundle()
        {
            controller.ShowNewBundle();
        }

        /// <summary>
        /// Displays an interface to load a bundle from disk
        /// </summary>
        private void LoadBundle()
        {
            controller.ShowLoadBundle();
        }

        /// <summary>
        /// Displays an interface to save the currently loaded bundle to disk
        /// </summary>
        /// <param name="forceNew">Whether to force the display of a file dialog even if the bundle is already saved on disk</param>
        private void SaveBundle(bool forceNew = false)
        {
            // Forces the currently opened windows to save their contents
            foreach (ModifiableContentView view in this.MdiChildren)
            {
                view.ApplyChanges();
            }

            controller.ShowSaveBundle(forceNew);
        }

        /// <summary>
        /// Displays an interface for exporting the bundle
        /// </summary>
        private void ExportBundle()
        {
            controller.ShowExportBundle();
        }

        /// <summary>
        /// Opens the currently selected node's data for editing
        /// </summary>
        private void OpenSelectedNode()
        {
            if (tv_bundleAnimations.SelectedNode == null)
                return;

            if (tv_bundleAnimations.SelectedNode.Tag is Animation)
            {
                OpenViewForAnimation(tv_bundleAnimations.SelectedNode.Tag as Animation);
            }
            else if (tv_bundleAnimations.SelectedNode.Tag is AnimationSheet)
            {
                OpenViewForAnimationSheet(tv_bundleAnimations.SelectedNode.Tag as AnimationSheet);
            }
        }

        #region Interface Related Methods

        /// <summary>
        /// Displays a confirmation to the user to delete the given Animation
        /// </summary>
        /// <param name="anim">The animation to show the confirmation to delete</param>
        public void ConfirmDeleteAnimation(Animation anim)
        {
            if (MessageBox.Show("Delete the selected animation?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                controller.RemoveAnimation(anim);
            }
        }

        /// <summary>
        /// Displays a confirmation to the user to delete the given AnimationSheet
        /// </summary>
        /// <param name="sheet">The sheet to show the confirmation to delete</param>
        public void ConfirmDeleteAnimationSheet(AnimationSheet sheet)
        {
            if (MessageBox.Show("Delete the selected animation sheet?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Whether the user has chosen to remove the animations as well
                bool removeAnims = false;
                
                // Confirm nested animations removal
                if (sheet.Animations.Length > 0)
                {
                    removeAnims = MessageBox.Show("Delete the sheet's animations as well?\nChoosing 'No' will move the sheet's animations to the bundle's root.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

                    if (!removeAnims)
                    {
                        // Move animations tree nodes to the bundle root
                        foreach (Animation anim in sheet.Animations)
                        {
                            TreeNode node = GetTreeNodeFor(anim);

                            node.Remove();

                            tv_bundleAnimations.Nodes[0].Nodes.Add(node);
                        }
                    }
                }

                controller.RemoveAnimationSheet(sheet, removeAnims);

                // Update the animations currently at the bundle root
                if (!removeAnims)
                {
                    int index = 0;

                    foreach (TreeNode node in tv_bundleAnimations.Nodes[0].Nodes)
                    {
                        if (node.Tag is Animation)
                        {
                            Animation anim = node.Tag as Animation;

                            controller.RearrangeAnimationsPosition(anim, index++);
                        }
                    }
                }
            }
        }

        #endregion

        #region TreeView Related Methods

        /// <summary>
        /// Gets the TreeNode currently representing the given object
        /// </summary>
        /// <param name="tag">The object to get the tree node representation of</param>
        /// <returns>The TreeNode that represents the given object</returns>
        public TreeNode GetTreeNodeFor(Object tag)
        {
            // Do a breadth-first search on the tree-view
            List<TreeNode> traversalList = new List<TreeNode>();
            TreeNode currentNode = tv_bundleAnimations.Nodes[0];
            int i = 0;

            foreach (TreeNode node in tv_bundleAnimations.Nodes)
            {
                traversalList.Add(node);
            }

            while (true)
            {
                currentNode = traversalList[i];

                if (currentNode.Tag == tag)
                    return currentNode;

                // Add the node's children nodes as well
                foreach (TreeNode node in currentNode.Nodes)
                {
                    traversalList.Add(node);
                }

                i++;

                if (i >= traversalList.Count)
                    return null;
            }
        }

        #endregion

        #region Interface Event Handlers

        // 
        // Form Closing event handler
        // 
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (controller.UnsavedChanges)
            {
                if (controller.ShowConfirmSaveChanges() == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        // 
        // OnPaintBackground event handler
        // 
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        //
        // TreeView Node Double Click event handler
        // 
        private void AnimationNodeDoubleClickHandler(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenSelectedNode();
        }

        // 
        // TreeView Key Down event handler
        // 
        private void tv_bundleAnimations_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OpenSelectedNode();
                tv_bundleAnimations.Focus();
            }
        }

        // 
        // TreeView Node Click ecent handler
        // 
        private void TreeViewNodeClickHandler(object sender, TreeNodeMouseClickEventArgs e)
        {
            tv_bundleAnimations.SelectedNode = e.Node;

            if (e.Button == MouseButtons.Right)
            {
                if (e.Node.Tag is Bundle)
                {
                    cms_bundleNodeRightClick.Show(MousePosition);
                }
                else if (e.Node.Tag is AnimationSheet)
                {
                    cms_sheetNodeRightClick.Show(MousePosition);
                }
                else if (e.Node.Tag is Animation)
                {
                    cms_animationNodeRightClick.Show(MousePosition);
                }
            }
        }

        // 
        // TreeView Drag Operation event handler
        // 
        private void TreeViewDragOperationHandler(TreeViewNodeDragEventArgs eventArgs)
        {
            // Handle drag start events
            if (eventArgs.EventType == TreeViewNodeDragEventType.DragStart)
            {
                if (eventArgs.DraggedNode.Tag is Bundle)
                {
                    eventArgs.Cancel = true;
                }
            }

            // Handle drag over events
            if (eventArgs.EventType == TreeViewNodeDragEventType.DragOver)
            {
                
                // Target and dragged nodes are AnimationSheet nodes:
                // Show a disabled cursor because the operation is not valid
                if (eventArgs.TargetNode.Tag is Animation && eventArgs.DraggedNode.Tag is AnimationSheet)
                {
                    eventArgs.Allow = false;
                    eventArgs.Cancel = true;
                }
            }

            // Handle drag end events
            if (eventArgs.EventType == TreeViewNodeDragEventType.DragEnd)
            {
                // Target is an Animation, or both target and dropped nodes are both AnimationSheet nodes:
                // Re-order the dragged node to before/after the target node
                if (eventArgs.TargetNode.Tag is Animation ||
                    eventArgs.TargetNode.Tag is AnimationSheet && eventArgs.DraggedNode.Tag is AnimationSheet)
                {
                    eventArgs.EventBehavior = TreeViewNodeDragEventBehavior.PlaceBeforeOrAfterAuto;
                }

                // Target is an AnimationSheet and dragged node is an Animation:
                // Add the dragged Animation into the target AnimationSheet
                if (eventArgs.TargetNode.Tag is AnimationSheet && eventArgs.DraggedNode.Tag is Animation)
                {
                    AnimationSheet sheet = (AnimationSheet)eventArgs.TargetNode.Tag;
                    Animation anim = (Animation)eventArgs.DraggedNode.Tag;

                    controller.AddAnimationToAnimationSheet(anim, sheet);
                }

                // Target is Bundle root and dragged node is an Animation:
                // Remove the animation from the current bundle, if it's in one
                if (eventArgs.TargetNode.Tag is Bundle && eventArgs.DraggedNode.Tag is Animation)
                {
                    Animation anim = (Animation)eventArgs.DraggedNode.Tag;

                    controller.AddAnimationToAnimationSheet(anim, null);
                }
            }

            // Handle side effects after drag events
            if (eventArgs.EventType == TreeViewNodeDragEventType.AfterDragEnd)
            {
                // Target and dragged node are Animation nodes, rearrange them in the model level
                if (eventArgs.TargetNode.Tag is Animation && eventArgs.DraggedNode.Tag is Animation)
                {
                    Animation targetAnim = (Animation)eventArgs.TargetNode.Tag;
                    Animation droppedAnim = (Animation)eventArgs.DraggedNode.Tag;

                    AnimationSheet sheet = controller.GetOwningAnimationSheet(targetAnim);

                    controller.AddAnimationToAnimationSheet(droppedAnim, sheet);
                        
                    // Swap the position of the animation on the container
                    TreeNode node = GetTreeNodeFor(droppedAnim);

                    // Count the nodes up to the animation node, ignoring all the animation sheets on the way
                    int actualIndex = 0;

                    for (int i = 0; i < node.Index; i++)
                    {
                        if (!(node.Parent.Nodes[i].Tag is AnimationSheet))
                        {
                            actualIndex++;
                        }
                    }

                    controller.RearrangeAnimationsPosition(droppedAnim, actualIndex);
                }
                // Target and dragged node are AnimationSheet nodes, rearrange them in the model level
                if (eventArgs.TargetNode.Tag is AnimationSheet && eventArgs.DraggedNode.Tag is AnimationSheet)
                {
                    AnimationSheet targetSheet = (AnimationSheet)eventArgs.TargetNode.Tag;
                    AnimationSheet droppedSheet = (AnimationSheet)eventArgs.DraggedNode.Tag;

                    // Swap the position of the animation on the container
                    TreeNode node = GetTreeNodeFor(droppedSheet);

                    controller.RearrangeAnimationSheetsPosition(droppedSheet, node.Index);
                }
            }
        }

        /// <summary>
        /// Specifies that the given TreeNode should not be expanded on the next call of the Before Expand/Collapse event handlers.
        /// The value is nullified once a matching Before Expand/Collapse event is fired
        /// </summary>
        private TreeNode cancelExpandCollapseForNode = null;
        private Point lastMousePoint = new Point();
        // 
        // TreeView Mouse Down event handler
        // 
        private void TreeViewMouseDown(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Clicks == 2 && tv_bundleAnimations.SelectedNode != null && tv_bundleAnimations.SelectedNode.Tag is AnimationSheet && tv_bundleAnimations.Bounds.Contains(e.Location) && Math.Sqrt((lastMousePoint.X - e.Location.X) * (lastMousePoint.X - e.Location.X) + (lastMousePoint.Y - e.Location.Y) * (lastMousePoint.Y - e.Location.Y)) < 5)
            {
                cancelExpandCollapseForNode = tv_bundleAnimations.SelectedNode;
            }
            lastMousePoint = e.Location;
        }

        // 
        // TreeView Before Node Collapse event handler
        // 
        private void tv_bundleAnimations_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == cancelExpandCollapseForNode)
            {
                e.Cancel = true;
                cancelExpandCollapseForNode = null;
            }
        }
        // 
        // TreeView Before Node Expand event handler
        // 
        private void tv_bundleAnimations_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == cancelExpandCollapseForNode)
            {
                e.Cancel = true;
                cancelExpandCollapseForNode = null;
            }
        }

        // 
        // New click
        // 
        private void mi_new_Click(object sender, EventArgs e)
        {
            NewBundle();
        }

        // 
        // New menu bar button click
        // 
        private void tsb_new_Click(object sender, EventArgs e)
        {
            NewBundle();
        }

        // 
        // Open click
        // 
        private void mi_open_Click(object sender, EventArgs e)
        {
            LoadBundle();
        }

        // 
        // Open menu bar button click
        // 
        private void tsb_open_Click(object sender, EventArgs e)
        {
            LoadBundle();
        }

        // 
        // Save click
        // 
        private void mi_save_Click(object sender, EventArgs e)
        {
            SaveBundle();
        }

        // 
        // Save menu bar button click
        // 
        private void tsb_save_Click(object sender, EventArgs e)
        {
            SaveBundle();
        }

        // 
        // Save As click
        // 
        private void mi_saveAs_Click(object sender, EventArgs e)
        {
            SaveBundle(true);
        }

        // 
        // Quit click
        // 
        private void mi_quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // 
        // Recent File item click
        // 
        private void mi_fileItem_Click(object sender, EventArgs e)
        {
            controller.LoadBundleFromRecentFileList((int)((sender as MenuItem).Tag));
        }

        // 
        // Cascade click
        // 
        private void mi_cascade_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        // 
        // Tile Horizontally click
        // 
        private void mi_tileHorizontally_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        // 
        // Arrange Icons click
        // 
        private void mi_arrangeIcons_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.ArrangeIcons);
        }

        // 
        // Bundle Settings menu button click
        // 
        private void tsb_bundleSettings_Click(object sender, EventArgs e)
        {
            OpenBundleSettings(controller.CurrentBundle);
        }

        // 
        // Bundle Settings context menu button click
        // 
        private void cmb_bundleSettingsClick(object sender, EventArgs e)
        {
            OpenBundleSettings(controller.CurrentBundle);
        }

        // 
        // Create Animation main menu button click
        // 
        private void mi_addAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (tv_bundleAnimations.SelectedNode == null || !(tv_bundleAnimations.SelectedNode.Tag is AnimationSheet) ? null : (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag);

            if (sheet == null && tv_bundleAnimations.SelectedNode != null && tv_bundleAnimations.SelectedNode.Tag is Animation)
            {
                sheet = controller.GetOwningAnimationSheet(tv_bundleAnimations.SelectedNode.Tag as Animation);
            }

            controller.ShowCreateAnimation(sheet);
        }

        // 
        // Create Animation menu bar button click
        // 
        private void tsb_createAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (tv_bundleAnimations.SelectedNode == null || !(tv_bundleAnimations.SelectedNode.Tag is AnimationSheet) ? null : (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag);

            if (sheet == null && tv_bundleAnimations.SelectedNode != null && tv_bundleAnimations.SelectedNode.Tag is Animation)
            {
                sheet = controller.GetOwningAnimationSheet(tv_bundleAnimations.SelectedNode.Tag as Animation);
            }

            controller.ShowCreateAnimation(sheet);
        }

        // 
        // Create Animation context menu button click
        // 
        private void cmb_createNewAnimationClick(object sender, EventArgs e)
        {
            controller.ShowCreateAnimation();
        }

        // 
        // Import Animation menu bar button click
        // 
        private void tsb_importAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (tv_bundleAnimations.SelectedNode == null || !(tv_bundleAnimations.SelectedNode.Tag is AnimationSheet) ? null : (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag);

            if (sheet == null && tv_bundleAnimations.SelectedNode != null && tv_bundleAnimations.SelectedNode.Tag is Animation)
            {
                sheet = controller.GetOwningAnimationSheet(tv_bundleAnimations.SelectedNode.Tag as Animation);
            }

            controller.ShowImportAnimation(sheet);
        }

        // 
        // Import Animation context menu button click
        // 
        private void cmb_importAnimationClick(object sender, EventArgs e)
        {
            controller.ShowImportAnimation();
        }

        // 
        // Create New Animation Sheet context menu button click
        // 
        private void createNewBundleSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.ShowCreateAnimationSheet();
        }

        // 
        // Create New Animation Sheet menu bar button click
        // 
        private void tsb_createAnimationSheet_Click(object sender, EventArgs e)
        {
            controller.ShowCreateAnimationSheet();
        }

        // 
        // Delete Animation Sheet context menu button click
        // 
        private void cmb_deleteSheet_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = tv_bundleAnimations.SelectedNode.Tag as AnimationSheet;

            if (sheet != null)
            {
                ConfirmDeleteAnimationSheet(sheet);
            }
        }

        // 
        // Create New Animation on Sheet context menu click
        // 
        private void tsm_sheetCreateAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag;

            if (sheet != null)
            {
                controller.ShowCreateAnimation(sheet);
            }
        }

        // 
        // Import Animation on Sheet context menu click
        // 
        private void tsm_sheetImportAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag;

            if (sheet != null)
            {
                controller.ShowImportAnimation(sheet);
            }
        }

        // 
        // Duplicate Animation Sheet context menu click
        // 
        private void tsm_duplicateSheet_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag;

            if (sheet != null)
            {
                controller.ShowDuplicateAnimationSheet(sheet);
            }
        }

        // 
        // Export Animation Sheet context menu click
        // 
        private void exportSheetImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag;

            if (sheet != null)
            {
                controller.ShowExportAnimationSheetImage(sheet);
            }
        }

        // 
        // Edit Sheet Properties context menu button click
        // 
        private void tsm_editSheetPropertiesClick(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = (AnimationSheet)tv_bundleAnimations.SelectedNode.Tag;

            if (sheet != null)
            {
                OpenViewForAnimationSheet(sheet);
            }
        }

        // 
        // Delete Animation context menu button click
        // 
        private void cmb_deleteAnim_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            Animation anim = tv_bundleAnimations.SelectedNode.Tag as Animation;

            if(anim != null)
            {
                ConfirmDeleteAnimation(anim);
            }
        }

        // 
        // Duplicat Animation context menu button click
        // 
        private void duplicateAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            Animation anim = tv_bundleAnimations.SelectedNode.Tag as Animation;

            if (anim != null)
            {
                controller.ShowDuplicateAnimation(anim);
            }
        }

        // 
        // Edit Animation Properties context menu button clikc
        // 
        private void cmb_editAnimProperties_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            Animation anim = tv_bundleAnimations.SelectedNode.Tag as Animation;

            if(anim != null)
            {
                OpenViewForAnimation(anim);
            }
        }

        // 
        // Export Bundle menu button click
        // 
        private void tsb_exportButton_Click(object sender, EventArgs e)
        {
            ExportBundle();
        }

        // 
        // About menu item click
        // 
        private void mi_about_Click(object sender, EventArgs e)
        {
            MiscViews.AboutBox aboutBox = new MiscViews.AboutBox();

            aboutBox.ShowDialog(this);
        }

        #endregion
    }
}
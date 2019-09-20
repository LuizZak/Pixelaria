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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Controllers;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Controllers.Exporters;
using Pixelaria.Data;
using Pixelaria.Properties;
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
        private readonly Reactive _reactive = new Reactive();
        private readonly ExporterSelectionController _exporterSelectionController;

        /// <summary>
        /// Gets the public-facing reactive bindings object
        /// </summary>
        public IReactive Rx => _reactive;

        /// <summary>
        /// Event handler for the recent file menu item list click
        /// </summary>
        private readonly EventHandler _recentFileClick;

        /// <summary>
        /// Delegate for a ViewOpenedClosed event
        /// </summary>
        public delegate void ViewOpenedClosedEventDelegate(object sender, ViewOpenCloseEventArgs eventArgs);

        /// <summary>
        /// Event raised when an MDI child was opened or closed on this form
        /// This is a forwarded event and the 'sender' argument points to the child view
        /// </summary>
        public event ViewOpenedClosedEventDelegate ViewOpenedClosed;

        /// <summary>
        /// Event raised when an MDI child suffered a change in its 'Modified' state.
        /// This is a forwarded event and the 'sender' argument points to the view which had its Modified state changed.
        /// </summary>
        public event EventHandler ChildViewModifiedChanged;

        /// <summary>
        /// The root tree node for the tree view
        /// </summary>
        private TreeNode _rootNode;

        /// <summary>
        /// Gets or sets the Controller instance that owns this MainForm
        /// </summary>
        public Controller Controller { get; set; }

        /// <summary>
        /// Creates a new instance of the MainForm class
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public MainForm([NotNull] IReadOnlyList<string> args)
        {
            InitializeComponent();

            // Enable double buffering on the MDI client to avoid flickering while redrawing
            foreach (var control in Controls.OfType<MdiClient>())
            {
                var method = control.GetType().GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
                method?.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, true });
            }

            Menu = mm_menu;

            il_treeView.Images.SetKeyName(2, "EMPTY");

            _rootNode = tv_bundleAnimations.Nodes[0];

            _recentFileClick = mi_fileItem_Click;

            // Hook up the TreeView event handlers
            tv_bundleAnimations.NodeMouseDoubleClick += AnimationNodeDoubleClickHandler;
            tv_bundleAnimations.NodeMouseClick += TreeViewNodeClickHandler;
            tv_bundleAnimations.DragOperation += TreeViewDragOperationHandler;
            tv_bundleAnimations.MouseDown += TreeViewMouseDown;

            Controller = new Controller(this);

            if (args.Count > 0 && File.Exists(args[0]))
            {
                Controller.LoadBundleFromFile(args[0]);
            }

            _exporterSelectionController = new ExporterSelectionController(tscb_exporter, tsb_exporterSettings, Controller);
        }

        /// <summary>
        /// Loads the given bundle into this interface
        /// </summary>
        /// <param name="bundle">The bundle to load</param>
        public void LoadBundle([NotNull] Bundle bundle)
        {
            CloseAllWindows();
            UpdateTitleBar(bundle);
            UpdateAnimationsTreeView(bundle);

            // Open the root node
            _rootNode.Expand();
        }

        /// <summary>
        /// Closes all MDI windows currently opened
        /// </summary>
        public void CloseAllWindows()
        {
            foreach (var form in MdiChildren)
            {
                form.Close();
            }
        }

        /// <summary>
        /// Updates the title bar with the information of the given bundle
        /// </summary>
        /// <param name="bundle">The bundle to fill the title bar with information of</param>
        public void UpdateTitleBar([NotNull] Bundle bundle)
        {
            const string version = "Pixelaria v1.17.7b";
            var saveState = Controller.UnsavedChanges ? "*" : "";

            if (bundle.SaveFile != "")
            {
                Text = $@"{version} [{bundle.Name} - {bundle.SaveFile}]{saveState}";
            }
            else
            {
                Text = $@"{version} [{bundle.Name}]{saveState}";
            }
        }

        /// <summary>
        /// Fills in the animations tree view with the animation information of the given bundle
        /// </summary>
        /// <param name="bundle">The bundle to load the animations from</param>
        public void UpdateAnimationsTreeView([NotNull] Bundle bundle)
        {
            // Clear the nodes
            tv_bundleAnimations.Nodes.Clear();

            _rootNode = null;

            // Clear the thumbnails
            while(il_treeView.Images.Count > 4)
            {
                il_treeView.Images[4].Dispose();
                il_treeView.Images.RemoveAt(4);
            }

            // Start filling the tree view again
            var bundleNode = _rootNode = new TreeNode("Bundle");

            bundleNode.Tag = bundle;

            // Add the animation sheet nodes
            foreach (var sheet in bundle.AnimationSheets)
            {
                AddAnimationSheet(sheet);
            }

            // Add the animation nodes
            foreach (var anim in bundle.Animations)
            {
                AddAnimation(anim);
            }

            // Move nodes from the root to their respective AnimationSheets
            foreach (var sheet in bundle.AnimationSheets)
            {
                var sheetNode = GetTreeNodeFor(sheet);
                foreach (var anim in sheet.Animations)
                {
                    // Get the animation node
                    var animNode = GetTreeNodeFor(anim);

                    if (animNode == null) continue;

                    animNode.Remove();
                    sheetNode?.Nodes.Add(animNode);
                }
            }

            tv_bundleAnimations.Nodes.Add(_rootNode);
        }

        /// <summary>
        /// Updates the icons of all the nodes on the tree view
        /// </summary>
        public void UpdateTreeViewIcons()
        {
            var nodeQueue = new Queue<TreeNode>();

            nodeQueue.Enqueue(_rootNode);

            while(nodeQueue.Count != 0)
            {
                var node = nodeQueue.Dequeue();

                if (node.Tag is Animation animation)
                {
                    node.ImageKey = node.SelectedImageKey = (animation.Name + animation.ID);
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
                item.Click -= _recentFileClick;
            }

            mi_recentFiles.MenuItems.Clear();
            
            // Start adding the files now
            for (int i = 0; i < Controller.CurrentRecentFileList.FileCount; i++)
            {
                string path = Controller.CurrentRecentFileList[i];

                if (path == "")
                    continue;

                var item = new MenuItem((i + 1) + " - " + (path == "" ? "--" : Path.GetFileName(path)))
                {
                    Tag = i
                };

                item.Click += _recentFileClick;

                mi_recentFiles.MenuItems.Add(item);
            }

            if (mi_recentFiles.MenuItems.Count == 0)
            {
                var item = new MenuItem("No recent files")
                {
                    Enabled = false
                };

                mi_recentFiles.MenuItems.Add(item);
            }
        }

        /// <summary>
        /// Updates the interface to reflect the values of the Unsaved Changes flag.
        /// </summary>
        /// <param name="isUnsaved">The current Unsaved Changes flag</param>
        public void UnsavedChangesUpdated(bool isUnsaved)
        {
            UpdateTitleBar(Controller.CurrentBundle);
        }

        /// <summary>
        /// Adds the given Animation to this form
        /// </summary>
        /// <param name="animation">The animation to add</param>
        /// <param name="selectOnAdd">Whether to select the sheet's node after it's added to the interface</param>
        public void AddAnimation([NotNull] Animation animation, bool selectOnAdd = false)
        {
            var parentNode = _rootNode;

            // If the animation is owned by a sheet, set the sheet's tree node as the parent for the animation node
            var sheet = Controller.GetOwningAnimationSheet(animation);
            if (sheet != null)
            {
                parentNode = GetTreeNodeFor(sheet);
            }

            Debug.Assert(parentNode != null, "parentNode != null");

            // Create the tree node now
            if (animation.FrameCount > 0)
            {
                var animController = new AnimationController(Controller.CurrentBundle, animation);
                var frameController = animController.GetFrameController(animController.GetFrameAtIndex(0));

                il_treeView.Images.Add(animation.Name + animation.ID, frameController.GenerateThumbnail(16, 16, true, true, Color.White));                
            }

            int addIndex = Controller.GetAnimationIndex(animation);

            // If the target node is the bundle root, add the index of the animation sheets to the target add index
            if (parentNode.Tag is Bundle)
            {
                foreach (TreeNode node in _rootNode.Nodes)
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

            var animNode = parentNode.Nodes.Insert(addIndex, animation.Name);

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
            foreach (var curView in MdiChildren)
            {
                if (curView is AnimationView view && ReferenceEquals(view.CurrentAnimation, anim))
                {
                    view.Close();
                    break;
                }
            }

            // Remove the animation's treenode
            var animNode = GetTreeNodeFor(anim);

            if (animNode == null)
                return;

            il_treeView.Images.RemoveByKey(animNode.ImageKey);

            animNode.Remove();

            UpdateTreeViewIcons();
        }

        /// <summary>
        /// Updates this form's reprensentation of the given Animation
        /// </summary>
        /// <param name="animation">The Animation to update the representation of</param>
        public void UpdateAnimation([NotNull] Animation animation)
        {
            // Seek the TreeView and update the treenode
            var animNode = GetTreeNodeFor(animation);
            
            Debug.Assert(animNode != null, "animNode != null");

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
                    var animController = new AnimationController(Controller.CurrentBundle, animation);
                    var frameController = animController.GetFrameController(animController.GetFrameAtIndex(0));

                    il_treeView.Images.Add(animation.Name + animation.ID, frameController.GenerateThumbnail(16, 16, true, true, Color.White));
                    animNode.ImageKey = animNode.SelectedImageKey = animation.Name + animation.ID;
                }
                else
                {
                    var animController = new AnimationController(Controller.CurrentBundle, animation);
                    var frameController = animController.GetFrameController(animController.GetFrameAtIndex(0));

                    il_treeView.Images[animNode.ImageIndex == -1 ? il_treeView.Images.IndexOfKey(animNode.ImageKey) : animNode.ImageIndex] = frameController.GenerateThumbnail(16, 16, true, true, Color.White);
                }
            }
            else
            {
                animNode.ImageKey = animNode.SelectedImageKey = animNode.StateImageKey = @"EMPTY";
            }

            UpdateTreeViewIcons();
        }

        /// <summary>
        /// Opens a view for the given animation (or brings a view already binded to this animation to the front)
        /// </summary>
        /// <param name="animation">The Animation to open the AnimationView for</param>
        /// <param name="selectedFrameIndex">An optional value to specify the view which frame to select once it opens</param>
        /// <returns>The created AnimationView</returns>
        public AnimationView OpenViewForAnimation(Animation animation, int selectedFrameIndex = -1)
        {
            var currentForm = GetOpenedViewForAnimation(animation);
            if (currentForm != null)
            {
                if (selectedFrameIndex != -1)
                {
                    currentForm.SelectFrameIndex(selectedFrameIndex);
                }

                currentForm.BringToFront();
                currentForm.Focus();
                return currentForm;
            }
            
            var view = new AnimationView(Controller, animation) { MdiParent = this };

            if(selectedFrameIndex != -1)
            {
                view.SelectFrameIndex(selectedFrameIndex);
            }

            view.Show();
            view.BringToFront();

            _reactive.OnMdiChildrenChanged.OnNext(MdiChildren);

            view.ModifiedChanged += (sender, args) => { ChildViewModifiedChanged?.Invoke(sender, args); };
            
            // Fire Opened event
            ViewOpenedClosed?.Invoke(view, new ViewOpenCloseEventArgs(view, ViewOpenCloseEventArgs.OpenCloseEventType.Opened));

            view.FormClosed += (sender, args) =>
            {
                ViewOpenedClosed?.Invoke(view, new ViewOpenCloseEventArgs(view, ViewOpenCloseEventArgs.OpenCloseEventType.Closed));
                _reactive.OnMdiChildrenChanged.OnNext(MdiChildren);
            };

            _reactive.OnOpenedAnimationView.OnNext(view);

            return view;
        }

        /// <summary>
        /// Gets the currently opened view, if any, that is associated with a given animation.
        /// Returns null, if no view is currently opened for the given animation in this form
        /// </summary>
        /// <param name="animation">The animation that might be opened on this form</param>
        /// <returns>The view for the animation; or null, if none could be found</returns>
        [CanBeNull]
        public AnimationView GetOpenedViewForAnimation(Animation animation)
        {
            return MdiChildren.OfType<AnimationView>().FirstOrDefault(view => view.CurrentAnimation.ID == animation.ID);
        }

        /// <summary>
        /// Adds the given Animation Sheet to this form
        /// </summary>
        /// <param name="sheet">The animation to add</param>
        /// <param name="selectOnAdd">Whether to select the sheet's node after it's added to the interface</param>
        public void AddAnimationSheet([NotNull] AnimationSheet sheet, bool selectOnAdd = false)
        {
            var bundleNode = _rootNode;

            // Find a new valid node position for the animation sheet
            int nodePos;

            for (nodePos = 0; nodePos < bundleNode.Nodes.Count; nodePos++)
            {
                if (bundleNode.Nodes[nodePos].Tag is Animation)
                {
                    break;
                }
            }

            // Create the tree node now
            var sheetNode = bundleNode.Nodes.Insert(nodePos, sheet.Name);

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
            foreach (var curView in MdiChildren)
            {
                if (curView is AnimationSheetView view && ReferenceEquals(view.CurrentSheet, sheet))
                {
                    view.Close();
                    break;
                }
            }

            // Remove the sheet's treenode
            var sheetNode = GetTreeNodeFor(sheet);

            sheetNode?.Remove();
        }

        /// <summary>
        /// Updates this form's reprensentation of the given AnimationSheet
        /// </summary>
        /// <param name="sheet">The AnimationSheet to update the representation of</param>
        public void UpdateAnimationSheet([NotNull] AnimationSheet sheet)
        {
            // Seek the TreeView and update the treenode
            var node = GetTreeNodeFor(sheet);
            if (node != null)
                node.Text = sheet.Name;
        }

        /// <summary>
        /// Opens a view for the given animation (or brings a view already binded to this animation to the front)
        /// </summary>
        /// <param name="sheet">The Animation to open the AnimationView for</param>
        /// <returns>The created AnimationView</returns>
        public AnimationSheetView OpenViewForAnimationSheet(AnimationSheet sheet)
        {
            var sheetView = GetOpenedViewForAnimationSheet(sheet);
            if (sheetView != null)
            {
                sheetView.BringToFront();
                sheetView.Focus();
                return sheetView;
            }

            var view = new AnimationSheetView(Controller, sheet) { MdiParent = this };

            view.Show();
            view.BringToFront();

            _reactive.OnMdiChildrenChanged.OnNext(MdiChildren);

            view.ModifiedChanged += (sender, args) =>
            {
                ChildViewModifiedChanged?.Invoke(sender, args);
            };

            // Fire Opened event
            ViewOpenedClosed?.Invoke(view, new ViewOpenCloseEventArgs(view, ViewOpenCloseEventArgs.OpenCloseEventType.Opened));

            view.FormClosed += (sender, args) =>
            {
                ViewOpenedClosed?.Invoke(view, new ViewOpenCloseEventArgs(view, ViewOpenCloseEventArgs.OpenCloseEventType.Closed));

                _reactive.OnMdiChildrenChanged.OnNext(MdiChildren);
            };

            _reactive.OnOpenedAnimationSheetView.OnNext(view);

            return view;
        }
        
        /// <summary>
        /// Gets the currently opened view, if any, that is associated with a given animation sheet.
        /// Returns null, if no view is currently opened for the given animation sheet in this form
        /// </summary>
        /// <param name="sheet">The animation sheet that might be opened on this form</param>
        /// <returns>The view for the animation sheet; or null, if none could be found</returns>
        [CanBeNull]
        public AnimationSheetView GetOpenedViewForAnimationSheet(AnimationSheet sheet)
        {
            return MdiChildren.OfType<AnimationSheetView>().FirstOrDefault(view => view.CurrentSheet.ID == sheet.ID);
        }

        /// <summary>
        /// Opens the bundle settings dialog for the given bundle
        /// </summary>
        public void OpenBundleSettings([NotNull] Bundle bundle)
        {
            var bsv = new BundleSettingsView(Controller, bundle);

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
            Controller.ShowNewBundle();
        }

        /// <summary>
        /// Displays an interface to load a bundle from disk
        /// </summary>
        private void LoadBundle()
        {
            Controller.ShowLoadBundle();
        }

        /// <summary>
        /// Displays an interface to save the currently loaded bundle to disk. If the bundle is already saved in disk, it doesn't display the interface.
        /// Changes made in any opened window are saved as if the user clicked 'Accept' on each one prior to saving
        /// </summary>
        /// <param name="forceNew">Whether to force the display of a file dialog even if the bundle is already saved on disk</param>
        private void SaveBundle(bool forceNew = false)
        {
            // Forces the currently opened windows to save their contents
            foreach (var form in MdiChildren)
            {
                var view = (ModifiableContentView)form;
                view?.ApplyChanges();
            }

            Controller.ShowSaveBundle(forceNew);
        }

        /// <summary>
        /// Displays an interface for exporting the bundle
        /// </summary>
        private void ExportBundle()
        {
            Controller.ShowExportBundle();
        }

        /// <summary>
        /// Opens the currently selected node's data for editing
        /// </summary>
        private void OpenSelectedNode()
        {
            if (tv_bundleAnimations.SelectedNode == null)
                return;

            if (tv_bundleAnimations.SelectedNode.Tag is Animation animation)
            {
                OpenViewForAnimation(animation);
            }
            else
            {
                if (tv_bundleAnimations.SelectedNode.Tag is AnimationSheet sheet)
                {
                    OpenViewForAnimationSheet(sheet);
                }
            }
        }

        #region Interface Related Methods

        /// <summary>
        /// Displays a confirmation to the user to delete the given Animation
        /// </summary>
        /// <param name="anim">The animation to show the confirmation to delete</param>
        public void ConfirmDeleteAnimation(Animation anim)
        {
            if (MessageBox.Show(Resources.MainForm_ConfirmDeleteAnimation, Resources.Confirmation_Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Controller.RemoveAnimation(anim);
            }
        }

        /// <summary>
        /// Displays a confirmation to the user to delete the given AnimationSheet
        /// </summary>
        /// <param name="sheet">The sheet to show the confirmation to delete</param>
        public void ConfirmDeleteAnimationSheet(AnimationSheet sheet)
        {
            if (MessageBox.Show(Resources.MainForm_ConfirmDeleteAnimationShee, Resources.Confirmation_Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Whether the user has chosen to remove the animations as well
                bool removeAnims = false;
                
                // Confirm nested animations removal
                if (sheet.Animations.Length > 0)
                {
                    removeAnims = MessageBox.Show(Resources.MainForm_ConfirmDeleteAnimationSheet_ChildAnims, Resources.Confirmation_Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

                    if (!removeAnims)
                    {
                        // Move animations tree nodes to the bundle root
                        foreach (var anim in sheet.Animations)
                        {
                            var node = GetTreeNodeFor(anim);
                            if (node == null) continue;

                            node.Remove();

                            _rootNode.Nodes.Add(node);
                        }
                    }
                }

                Controller.RemoveAnimationSheet(sheet, removeAnims);

                // Update the animations currently at the bundle root
                if (!removeAnims)
                {
                    int index = 0;

                    foreach (TreeNode node in _rootNode.Nodes)
                    {
                        if (node.Tag is Animation animation)
                        {
                            var anim = animation;
                            Controller.RearrangeAnimationsPosition(anim, index++);
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
        [CanBeNull]
        public TreeNode GetTreeNodeFor(object tag)
        {
            // Do a breadth-first search on the tree-view
            var currentNode = _rootNode;
            int i = 0;

            var traversalList = currentNode.Nodes.Cast<TreeNode>().ToList();

            while (true)
            {
                currentNode = traversalList[i];

                if (currentNode.Tag == tag)
                    return currentNode;

                // Add the node's children nodes as well
                traversalList.AddRange(currentNode.Nodes.Cast<TreeNode>());

                i++;

                if (i >= traversalList.Count)
                    return null;
            }
        }

        /// <summary>
        /// Gets the type for the currently selected tree view node.
        /// Returns null, if no node is currently selected
        /// </summary>
        /// <returns>The type for the currently selected node, or null, if none was available</returns>
        private TreeViewNodeType? GetTypeForSelectedNode()
        {
            if (tv_bundleAnimations.SelectedNode == null)
                return null;

            return GetTypeForNode(tv_bundleAnimations.SelectedNode);
        }

        /// <summary>
        /// Gets the type for the given tree view node
        /// </summary>
        /// <param name="node">The node to get the type of</param>
        /// <returns>The type for the given node</returns>
        private TreeViewNodeType GetTypeForNode([NotNull] TreeNode node)
        {
            if (node.Tag is Bundle)
            {
                return TreeViewNodeType.Bundle;
            }

            if (node.Tag is Animation)
            {
                return TreeViewNodeType.Animation;
            }
            
            if (node.Tag is AnimationSheet)
            {
                return TreeViewNodeType.AnimationSheet;
            }

            return TreeViewNodeType.Unknown;
        }

        #endregion

        #region Interface Event Handlers

        // 
        // Form Closing event handler
        // 
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Controller.UnsavedChanges)
            {
                if (Controller.ShowConfirmSaveChanges() == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        #region Keyboard

        // 
        // TreeView Key Down event handler
        // 
        private void tv_bundleAnimations_KeyDown(object sender, [NotNull] KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OpenSelectedNode();
                tv_bundleAnimations.Focus();
            }

            if (e.KeyCode == Keys.Delete)
            {
                var nodeType = GetTypeForSelectedNode();

                if (nodeType == null)
                    return;

                switch (nodeType.Value)
                {
                    case TreeViewNodeType.Animation:
                        // Get the currently selected Animation node
                        Animation anim = tv_bundleAnimations.SelectedNode.Tag as Animation;
                        ConfirmDeleteAnimation(anim);
                        break;
                    case TreeViewNodeType.AnimationSheet:
                        // Get the currently selected AnimationSheet node
                        AnimationSheet sheet = tv_bundleAnimations.SelectedNode.Tag as AnimationSheet;
                        ConfirmDeleteAnimationSheet(sheet);
                        break;
                }
            }
        }

        #endregion

        #region Mouse

        //
        // TreeView Node Double Click event handler
        // 
        private void AnimationNodeDoubleClickHandler(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenSelectedNode();
        }

        // 
        // TreeView Node Click ecent handler
        // 
        private void TreeViewNodeClickHandler(object sender, [NotNull] TreeNodeMouseClickEventArgs e)
        {
            tv_bundleAnimations.SelectedNode = e.Node;

            if (e.Button != MouseButtons.Right)
                return;

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

        // 
        // TreeView Drag Operation event handler
        // 
        private void TreeViewDragOperationHandler(object sender, [NotNull] TreeViewNodeDragEventArgs eventArgs)
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
                if (eventArgs.TargetNode.Tag is AnimationSheet tag && eventArgs.DraggedNode.Tag is Animation)
                {
                    var sheet = tag;
                    var anim = (Animation)eventArgs.DraggedNode.Tag;

                    Controller.AddAnimationToAnimationSheet(anim, sheet);
                }

                // Target is Bundle root and dragged node is an Animation:
                // Remove the animation from the current bundle, if it's in one
                if (eventArgs.TargetNode.Tag is Bundle && eventArgs.DraggedNode.Tag is Animation)
                {
                    var anim = (Animation)eventArgs.DraggedNode.Tag;

                    Controller.AddAnimationToAnimationSheet(anim, null);
                }
            }

            // Handle side effects after drag events
            if (eventArgs.EventType == TreeViewNodeDragEventType.AfterDragEnd)
            {
                // Target and dragged node are Animation nodes, rearrange them in the model level
                if (eventArgs.TargetNode.Tag is Animation tag && eventArgs.DraggedNode.Tag is Animation)
                {
                    var targetAnim = tag;
                    var droppedAnim = (Animation)eventArgs.DraggedNode.Tag;

                    var sheet = Controller.GetOwningAnimationSheet(targetAnim);

                    Controller.AddAnimationToAnimationSheet(droppedAnim, sheet);

                    // Swap the position of the animation on the container
                    var node = GetTreeNodeFor(droppedAnim);
                    Debug.Assert(node != null, "node != null");

                    // Count the nodes up to the animation node, ignoring all the animation sheets on the way
                    int actualIndex = 0;

                    for (int i = 0; i < node.Index; i++)
                    {
                        if (!(node.Parent.Nodes[i].Tag is AnimationSheet))
                        {
                            actualIndex++;
                        }
                    }

                    Controller.RearrangeAnimationsPosition(droppedAnim, actualIndex);
                }
                // Target and dragged node are AnimationSheet nodes, rearrange them in the model level
                if (eventArgs.TargetNode.Tag is AnimationSheet && eventArgs.DraggedNode.Tag is AnimationSheet)
                {
                    var droppedSheet = (AnimationSheet)eventArgs.DraggedNode.Tag;

                    // Swap the position of the animation on the container
                    var node = GetTreeNodeFor(droppedSheet);

                    Debug.Assert(node != null, "node != null");
                    Controller.RearrangeAnimationSheetsPosition(droppedSheet, node.Index);
                }
            }
        }

        /// <summary>
        /// Specifies that the given TreeNode should not be expanded on the next call of the Before Expand/Collapse event handlers.
        /// The value is nullified once a matching Before Expand/Collapse event is fired
        /// </summary>
        private TreeNode _cancelExpandCollapseForNode;
        private Point _lastMousePoint;
        // 
        // TreeView Mouse Down event handler
        // 
        private void TreeViewMouseDown(object sender, [NotNull] MouseEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Clicks == 2 && tv_bundleAnimations.SelectedNode?.Tag is AnimationSheet && tv_bundleAnimations.Bounds.Contains(e.Location) && Math.Sqrt((_lastMousePoint.X - e.Location.X) * (_lastMousePoint.X - e.Location.X) + (_lastMousePoint.Y - e.Location.Y) * (_lastMousePoint.Y - e.Location.Y)) < 5)
            {
                _cancelExpandCollapseForNode = tv_bundleAnimations.SelectedNode;
            }
            _lastMousePoint = e.Location;
        }

        #endregion

        // 
        // TreeView Before Node Collapse event handler
        // 
        private void tv_bundleAnimations_BeforeCollapse(object sender, [NotNull] TreeViewCancelEventArgs e)
        {
            if (e.Node == _cancelExpandCollapseForNode)
            {
                e.Cancel = true;
                _cancelExpandCollapseForNode = null;
            }
        }
        // 
        // TreeView Before Node Expand event handler
        // 
        private void tv_bundleAnimations_BeforeExpand(object sender, [NotNull] TreeViewCancelEventArgs e)
        {
            if (e.Node == _cancelExpandCollapseForNode)
            {
                e.Cancel = true;
                _cancelExpandCollapseForNode = null;
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

            if (sender is MenuItem menuItem)
                Controller.LoadBundleFromRecentFileList((int)(menuItem.Tag));
        }

        // 
        // Cascade click
        // 
        private void mi_cascade_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        // 
        // Tile Horizontally click
        // 
        private void mi_tileHorizontally_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        // 
        // Arrange Icons click
        // 
        private void mi_arrangeIcons_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        // 
        // Bundle Settings menu button click
        // 
        private void tsb_bundleSettings_Click(object sender, EventArgs e)
        {
            OpenBundleSettings(Controller.CurrentBundle);
        }

        // 
        // Bundle Settings context menu button click
        // 
        private void cmb_bundleSettingsClick(object sender, EventArgs e)
        {
            OpenBundleSettings(Controller.CurrentBundle);
        }

        // 
        // Create Animation main menu button click
        // 
        private void mi_addAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = tv_bundleAnimations.SelectedNode?.Tag as AnimationSheet;

            if (sheet == null && tv_bundleAnimations.SelectedNode?.Tag is Animation)
            {
                sheet = Controller.GetOwningAnimationSheet((Animation)tv_bundleAnimations.SelectedNode.Tag);
            }

            Controller.ShowCreateAnimation(sheet);
        }

        // 
        // Create Animation menu bar button click
        // 
        private void tsb_createAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = tv_bundleAnimations.SelectedNode?.Tag as AnimationSheet;

            if (sheet == null && tv_bundleAnimations.SelectedNode?.Tag is Animation)
            {
                sheet = Controller.GetOwningAnimationSheet((Animation)tv_bundleAnimations.SelectedNode.Tag);
            }

            Controller.ShowCreateAnimation(sheet);
        }

        // 
        // Create Animation context menu button click
        // 
        private void cmb_createNewAnimationClick(object sender, EventArgs e)
        {
            Controller.ShowCreateAnimation();
        }

        // 
        // Import Animation menu bar button click
        // 
        private void tsb_importAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            AnimationSheet sheet = tv_bundleAnimations.SelectedNode?.Tag as AnimationSheet;

            if (sheet == null && tv_bundleAnimations.SelectedNode?.Tag is Animation)
            {
                sheet = Controller.GetOwningAnimationSheet((Animation)tv_bundleAnimations.SelectedNode.Tag);
            }

            Controller.ShowImportAnimation(sheet);
        }

        // 
        // Import Animation context menu button click
        // 
        private void cmb_importAnimationClick(object sender, EventArgs e)
        {
            Controller.ShowImportAnimation();
        }

        // 
        // Create New Animation Sheet context menu button click
        // 
        private void createNewBundleSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Controller.ShowCreateAnimationSheet();
        }

        // 
        // Create New Animation Sheet menu bar button click
        // 
        private void tsb_createAnimationSheet_Click(object sender, EventArgs e)
        {
            Controller.ShowCreateAnimationSheet();
        }

        // 
        // Delete Animation Sheet context menu button click
        // 
        private void cmb_deleteSheet_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node

            if (tv_bundleAnimations.SelectedNode.Tag is AnimationSheet sheet)
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
            var sheet = tv_bundleAnimations.SelectedNode.Tag as AnimationSheet;

            if (sheet != null)
            {
                Controller.ShowCreateAnimation(sheet);
            }
        }

        // 
        // Import Animation on Sheet context menu click
        // 
        private void tsm_sheetImportAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            var sheet = tv_bundleAnimations.SelectedNode.Tag as AnimationSheet;

            if (sheet != null)
            {
                Controller.ShowImportAnimation(sheet);
            }
        }

        // 
        // Duplicate Animation Sheet context menu click
        // 
        private void tsm_duplicateSheet_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            var sheet = tv_bundleAnimations.SelectedNode.Tag as AnimationSheet;

            if (sheet != null)
            {
                Controller.ShowDuplicateAnimationSheet(sheet);
            }
        }

        // 
        // Export Animation Sheet context menu click
        // 
        private void tsm_exportSheetImage_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            var sheet = tv_bundleAnimations.SelectedNode.Tag as AnimationSheet;

            if (sheet != null)
            {
                Controller.ShowExportAnimationSheetImage(sheet);
            }
        }

        // 
        // Edit Sheet Properties context menu button click
        // 
        private void tsm_editSheetPropertiesClick(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node
            var sheet = tv_bundleAnimations.SelectedNode.Tag as AnimationSheet;

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

            if (tv_bundleAnimations.SelectedNode.Tag is Animation anim)
            {
                ConfirmDeleteAnimation(anim);
            }
        }

        // 
        // Duplicat Animation context menu button click
        // 
        private void cmb_duplicateAnimation_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node

            if (tv_bundleAnimations.SelectedNode.Tag is Animation anim)
            {
                Controller.ShowDuplicateAnimation(anim);
            }
        }

        // 
        // Save Animation Strip context menu button click
        // 
        private void cmb_saveAnimationStrip_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node

            if (tv_bundleAnimations.SelectedNode.Tag is Animation anim)
            {
                Controller.ShowSaveAnimationStrip(new AnimationController(Controller.CurrentBundle, anim));
            }
        }

        // 
        // Edit Animation Properties context menu button clikc
        // 
        private void cmb_editAnimProperties_Click(object sender, EventArgs e)
        {
            // Get the currently selected AnimationSheet node

            if (tv_bundleAnimations.SelectedNode.Tag is Animation anim)
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
        // Support menu item click
        //
        private void mi_fileBug_Click(object sender, EventArgs e)
        {
            Process.Start("https://sourceforge.net/p/pixelaria/tickets/?source=navbar");
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

        /// <summary>
        /// Specifies the type of a tree view node
        /// </summary>
        private enum TreeViewNodeType
        {
            /// <summary>
            /// Specifies a bundle node
            /// </summary>
            Bundle,
            /// <summary>
            /// Specifies an animation node
            /// </summary>
            Animation,
            /// <summary>
            /// Specifies an animation sheet node
            /// </summary>
            AnimationSheet,
            /// <summary>
            /// Specifies an unknown node type
            /// </summary>
            Unknown
        }

        private class Reactive : IReactive
        {
            public readonly Subject<AnimationSheetView> OnOpenedAnimationSheetView = new Subject<AnimationSheetView>();
            public readonly Subject<AnimationView> OnOpenedAnimationView = new Subject<AnimationView>();
            public readonly Subject<Form[]> OnMdiChildrenChanged = new Subject<Form[]>();

            public IObservable<Form[]> MdiChildrenChanged => OnMdiChildrenChanged;
            public IObservable<AnimationSheetView> OpenedAnimationSheetView => OnOpenedAnimationSheetView;
            public IObservable<AnimationView> OpenedAnimationView => OnOpenedAnimationView;
        }

        /// <summary>
        /// Public-facing Reactive bindings
        /// </summary>
        public interface IReactive
        {
            /// <summary>
            /// Called whenever a new animation sheet view opens.
            /// 
            /// Not called when an already opened view is brought to the foreground by an attempt to
            /// open it.
            /// </summary>
            IObservable<AnimationSheetView> OpenedAnimationSheetView { get; }

            /// <summary>
            /// Called whenever a new animation view opens
            /// 
            /// Not called when an already opened view is brought to the foreground by an attempt to
            /// open it.
            /// </summary>
            IObservable<AnimationView> OpenedAnimationView { get; }

            /// <summary>
            /// Called whenever a new Form has been shown/removed on the main form
            /// </summary>
            IObservable<Form[]> MdiChildrenChanged { get; }
        }

        /// <summary>
        /// Controls the exporter combo box/settings controls from the main form
        /// </summary>
        private class ExporterSelectionController
        {
            private ToolStripComboBox ComboBox { get; }
            private ToolStripButton SettingsButton { get; }
            private Controller Controller { get; }

            public ExporterSelectionController(ToolStripComboBox comboBox, ToolStripButton settingsButton, Controller controller)
            {
                ComboBox = comboBox;
                SettingsButton = settingsButton;
                Controller = controller;

                PopulateExportMethods();
                ConfigureEvents();

                var exporter = ExporterController.Instance.Exporters.FirstOrDefault(e => e.SerializationName == Controller.CurrentBundle.ExporterSerializedName) ?? ExporterController.Instance.DefaultExporter;
                OnExporterChanged(exporter);
            }

            private void PopulateExportMethods()
            {
                ComboBox.BeginUpdate();
                ComboBox.Items.Clear();
                foreach (var exporter in ExporterController.Instance.Exporters)
                {
                    ComboBox.Items.Add(exporter.DisplayName);
                }
                ComboBox.EndUpdate();
            }

            private void ConfigureEvents()
            {
                Controller.Rx.ExporterChanged.Subscribe(OnExporterChanged);
                ComboBox.SelectedIndexChanged += ComboBoxOnSelectedIndexChanged;
                SettingsButton.Click += SettingsButtonOnClick;
            }

            private void SettingsButtonOnClick(object sender, EventArgs e)
            {
                Controller.ShowExporterSettings(Controller.CurrentBundle.ExporterSerializedName);
            }

            private void ComboBoxOnSelectedIndexChanged(object sender, EventArgs e)
            {
                Controller.SetExporter(ExporterController.Instance.Exporters[ComboBox.SelectedIndex]);
            }

            private void OnExporterChanged([NotNull] IKnownExporterEntry obj)
            {
                ComboBox.SelectedIndex = ComboBox.FindString(obj.DisplayName);
                SettingsButton.Enabled = obj.HasSettings;
            }
        }
    }

    /// <summary>
    /// Specifies events related to opening and closing of views on a main form
    /// </summary>
    public class ViewOpenCloseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the type of event this event represents
        /// </summary>
        public OpenCloseEventType EventType { get; }

        /// <summary>
        /// Gets the view that was either opened or closed
        /// </summary>
        public Form View { get; }

        public ViewOpenCloseEventArgs(Form view, OpenCloseEventType eventType)
        {
            View = view;
            EventType = eventType;
        }

        public enum OpenCloseEventType
        {
            Opened,
            Closed
        }
    }

    /// <summary>
    /// Class that controls the presentation of a project tree view
    /// </summary>
    public class ProjectTreeViewController
    {
        
    }
}
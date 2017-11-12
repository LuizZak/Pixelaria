/*
Pixelaria
Copyright (C) 2013 Luiz Fernando Silva

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

The full license may be found on the License.txt file attached to the
base directory of this project.
*/

namespace Pixelaria.Views
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Bundle");
            this.ts_mainStrip = new System.Windows.Forms.ToolStrip();
            this.tsb_new = new System.Windows.Forms.ToolStripButton();
            this.tsb_open = new System.Windows.Forms.ToolStripButton();
            this.tsb_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_createAnimation = new System.Windows.Forms.ToolStripButton();
            this.tsb_importAnimation = new System.Windows.Forms.ToolStripButton();
            this.tsb_createAnimationSheet = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_exportButton = new System.Windows.Forms.ToolStripButton();
            this.tsb_bundleSettings = new System.Windows.Forms.ToolStripButton();
            this.cms_bundleNodeRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmb_createNewAnimation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_importAnimation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_createNewAnimationSheet = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmb_bundleSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mm_menu = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mi_new = new System.Windows.Forms.MenuItem();
            this.mi_open = new System.Windows.Forms.MenuItem();
            this.mi_recentFiles = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.mi_save = new System.Windows.Forms.MenuItem();
            this.mi_saveAs = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.mi_quit = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.mi_addAnimation = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.mi_cascade = new System.Windows.Forms.MenuItem();
            this.mi_tileHorizontally = new System.Windows.Forms.MenuItem();
            this.mi_arrangeIcons = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.mi_fileBug = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.mi_about = new System.Windows.Forms.MenuItem();
            this.cms_sheetNodeRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmb_deleteSheet = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_sheetCreateAnimation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_sheetImportAnimation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_duplicateSheet = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_exportSheetImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmb_editSheetProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.cms_animationNodeRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmb_deleteAnim = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_duplicateAnimation = new System.Windows.Forms.ToolStripMenuItem();
            this.cmb_saveAnimationStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.cmb_editAnimProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.il_treeView = new System.Windows.Forms.ImageList(this.components);
            this.tv_bundleAnimations = new Pixelaria.Views.Controls.ProjectTreeView();
            this.ts_mainStrip.SuspendLayout();
            this.cms_bundleNodeRightClick.SuspendLayout();
            this.cms_sheetNodeRightClick.SuspendLayout();
            this.cms_animationNodeRightClick.SuspendLayout();
            this.SuspendLayout();
            // 
            // ts_mainStrip
            // 
            this.ts_mainStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ts_mainStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsb_new,
            this.tsb_open,
            this.tsb_save,
            this.toolStripSeparator1,
            this.tsb_createAnimation,
            this.tsb_importAnimation,
            this.tsb_createAnimationSheet,
            this.toolStripSeparator2,
            this.tsb_exportButton,
            this.tsb_bundleSettings});
            this.ts_mainStrip.Location = new System.Drawing.Point(0, 0);
            this.ts_mainStrip.Name = "ts_mainStrip";
            this.ts_mainStrip.Size = new System.Drawing.Size(1125, 25);
            this.ts_mainStrip.TabIndex = 4;
            this.ts_mainStrip.Text = "toolStrip1";
            // 
            // tsb_new
            // 
            this.tsb_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_new.Image = global::Pixelaria.Properties.Resources.document_new;
            this.tsb_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_new.Name = "tsb_new";
            this.tsb_new.Size = new System.Drawing.Size(23, 22);
            this.tsb_new.Text = "New";
            this.tsb_new.Click += new System.EventHandler(this.tsb_new_Click);
            // 
            // tsb_open
            // 
            this.tsb_open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_open.Image = global::Pixelaria.Properties.Resources.document_open;
            this.tsb_open.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_open.Name = "tsb_open";
            this.tsb_open.Size = new System.Drawing.Size(23, 22);
            this.tsb_open.Text = "Open...";
            this.tsb_open.Click += new System.EventHandler(this.tsb_open_Click);
            // 
            // tsb_save
            // 
            this.tsb_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_save.Image = global::Pixelaria.Properties.Resources.document_save;
            this.tsb_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_save.Name = "tsb_save";
            this.tsb_save.Size = new System.Drawing.Size(23, 22);
            this.tsb_save.Text = "Save...";
            this.tsb_save.Click += new System.EventHandler(this.tsb_save_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_createAnimation
            // 
            this.tsb_createAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_createAnimation.Image = global::Pixelaria.Properties.Resources.anim_new_icon;
            this.tsb_createAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_createAnimation.Name = "tsb_createAnimation";
            this.tsb_createAnimation.Size = new System.Drawing.Size(23, 22);
            this.tsb_createAnimation.Text = "Create New Animation...";
            this.tsb_createAnimation.Click += new System.EventHandler(this.tsb_createAnimation_Click);
            // 
            // tsb_importAnimation
            // 
            this.tsb_importAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_importAnimation.Image = global::Pixelaria.Properties.Resources.edit_undo;
            this.tsb_importAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_importAnimation.Name = "tsb_importAnimation";
            this.tsb_importAnimation.Size = new System.Drawing.Size(23, 22);
            this.tsb_importAnimation.Text = "Import Animation...";
            this.tsb_importAnimation.Click += new System.EventHandler(this.tsb_importAnimation_Click);
            // 
            // tsb_createAnimationSheet
            // 
            this.tsb_createAnimationSheet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_createAnimationSheet.Image = global::Pixelaria.Properties.Resources.sheet_new;
            this.tsb_createAnimationSheet.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_createAnimationSheet.Name = "tsb_createAnimationSheet";
            this.tsb_createAnimationSheet.Size = new System.Drawing.Size(23, 22);
            this.tsb_createAnimationSheet.Text = "Create New Animation Sheet...";
            this.tsb_createAnimationSheet.Click += new System.EventHandler(this.tsb_createAnimationSheet_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_exportButton
            // 
            this.tsb_exportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_exportButton.Image = global::Pixelaria.Properties.Resources.emblem_symbolic_link;
            this.tsb_exportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_exportButton.Name = "tsb_exportButton";
            this.tsb_exportButton.Size = new System.Drawing.Size(23, 22);
            this.tsb_exportButton.Text = "Export Bundle";
            this.tsb_exportButton.Click += new System.EventHandler(this.tsb_exportButton_Click);
            // 
            // tsb_bundleSettings
            // 
            this.tsb_bundleSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_bundleSettings.Image = global::Pixelaria.Properties.Resources.document_properties;
            this.tsb_bundleSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_bundleSettings.Name = "tsb_bundleSettings";
            this.tsb_bundleSettings.Size = new System.Drawing.Size(23, 22);
            this.tsb_bundleSettings.Text = "Bundle Settings...";
            this.tsb_bundleSettings.Click += new System.EventHandler(this.tsb_bundleSettings_Click);
            // 
            // cms_bundleNodeRightClick
            // 
            this.cms_bundleNodeRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmb_createNewAnimation,
            this.cmb_importAnimation,
            this.cmb_createNewAnimationSheet,
            this.toolStripMenuItem1,
            this.cmb_bundleSettings});
            this.cms_bundleNodeRightClick.Name = "cms_nodeRightClick";
            this.cms_bundleNodeRightClick.Size = new System.Drawing.Size(236, 98);
            // 
            // cmb_createNewAnimation
            // 
            this.cmb_createNewAnimation.Image = global::Pixelaria.Properties.Resources.document_new;
            this.cmb_createNewAnimation.Name = "cmb_createNewAnimation";
            this.cmb_createNewAnimation.Size = new System.Drawing.Size(235, 22);
            this.cmb_createNewAnimation.Text = "Create New Animation...";
            this.cmb_createNewAnimation.Click += new System.EventHandler(this.cmb_createNewAnimationClick);
            // 
            // cmb_importAnimation
            // 
            this.cmb_importAnimation.Image = global::Pixelaria.Properties.Resources.edit_undo;
            this.cmb_importAnimation.Name = "cmb_importAnimation";
            this.cmb_importAnimation.Size = new System.Drawing.Size(235, 22);
            this.cmb_importAnimation.Text = "Import Animation...";
            this.cmb_importAnimation.Click += new System.EventHandler(this.cmb_importAnimationClick);
            // 
            // cmb_createNewAnimationSheet
            // 
            this.cmb_createNewAnimationSheet.Image = global::Pixelaria.Properties.Resources.sheet_new;
            this.cmb_createNewAnimationSheet.Name = "cmb_createNewAnimationSheet";
            this.cmb_createNewAnimationSheet.Size = new System.Drawing.Size(235, 22);
            this.cmb_createNewAnimationSheet.Text = "Create New Animation Sheet...";
            this.cmb_createNewAnimationSheet.Click += new System.EventHandler(this.createNewBundleSheetToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(232, 6);
            // 
            // cmb_bundleSettings
            // 
            this.cmb_bundleSettings.Image = global::Pixelaria.Properties.Resources.document_properties;
            this.cmb_bundleSettings.Name = "cmb_bundleSettings";
            this.cmb_bundleSettings.Size = new System.Drawing.Size(235, 22);
            this.cmb_bundleSettings.Text = "Bundle Settings...";
            this.cmb_bundleSettings.Click += new System.EventHandler(this.cmb_bundleSettingsClick);
            // 
            // mm_menu
            // 
            this.mm_menu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem7,
            this.menuItem9,
            this.menuItem2});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mi_new,
            this.mi_open,
            this.mi_recentFiles,
            this.menuItem6,
            this.mi_save,
            this.mi_saveAs,
            this.menuItem5,
            this.mi_quit});
            this.menuItem1.Text = "File";
            // 
            // mi_new
            // 
            this.mi_new.Index = 0;
            this.mi_new.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.mi_new.Text = "&New";
            this.mi_new.Click += new System.EventHandler(this.mi_new_Click);
            // 
            // mi_open
            // 
            this.mi_open.Index = 1;
            this.mi_open.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.mi_open.Text = "&Open...";
            this.mi_open.Click += new System.EventHandler(this.mi_open_Click);
            // 
            // mi_recentFiles
            // 
            this.mi_recentFiles.Index = 2;
            this.mi_recentFiles.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem8});
            this.mi_recentFiles.Text = "Recent Files";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 0;
            this.menuItem8.Text = "File 0";
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 3;
            this.menuItem6.Text = "-";
            // 
            // mi_save
            // 
            this.mi_save.Index = 4;
            this.mi_save.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.mi_save.Text = "&Save";
            this.mi_save.Click += new System.EventHandler(this.mi_save_Click);
            // 
            // mi_saveAs
            // 
            this.mi_saveAs.Index = 5;
            this.mi_saveAs.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftS;
            this.mi_saveAs.Text = "Save As...";
            this.mi_saveAs.Click += new System.EventHandler(this.mi_saveAs_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 6;
            this.menuItem5.Text = "-";
            // 
            // mi_quit
            // 
            this.mi_quit.Index = 7;
            this.mi_quit.Shortcut = System.Windows.Forms.Shortcut.CtrlQ;
            this.mi_quit.Text = "&Quit";
            this.mi_quit.Click += new System.EventHandler(this.mi_quit_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 1;
            this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mi_addAnimation});
            this.menuItem7.Text = "Bundle";
            // 
            // mi_addAnimation
            // 
            this.mi_addAnimation.Index = 0;
            this.mi_addAnimation.Text = "Create &Animation";
            this.mi_addAnimation.Click += new System.EventHandler(this.mi_addAnimation_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 2;
            this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mi_cascade,
            this.mi_tileHorizontally,
            this.mi_arrangeIcons});
            this.menuItem9.Text = "Windows";
            // 
            // mi_cascade
            // 
            this.mi_cascade.Index = 0;
            this.mi_cascade.Text = "Cascade";
            this.mi_cascade.Click += new System.EventHandler(this.mi_cascade_Click);
            // 
            // mi_tileHorizontally
            // 
            this.mi_tileHorizontally.Index = 1;
            this.mi_tileHorizontally.Text = "Tile Horizontally";
            this.mi_tileHorizontally.Click += new System.EventHandler(this.mi_tileHorizontally_Click);
            // 
            // mi_arrangeIcons
            // 
            this.mi_arrangeIcons.Index = 2;
            this.mi_arrangeIcons.Text = "Arrange Icons";
            this.mi_arrangeIcons.Click += new System.EventHandler(this.mi_arrangeIcons_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 3;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mi_fileBug,
            this.menuItem3,
            this.mi_about});
            this.menuItem2.Text = "Help";
            // 
            // mi_fileBug
            // 
            this.mi_fileBug.Index = 0;
            this.mi_fileBug.Text = "File a Bug...";
            this.mi_fileBug.Click += new System.EventHandler(this.mi_fileBug_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.Text = "-";
            // 
            // mi_about
            // 
            this.mi_about.Index = 2;
            this.mi_about.Text = "About";
            this.mi_about.Click += new System.EventHandler(this.mi_about_Click);
            // 
            // cms_sheetNodeRightClick
            // 
            this.cms_sheetNodeRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmb_deleteSheet,
            this.cmb_sheetCreateAnimation,
            this.cmb_sheetImportAnimation,
            this.cmb_duplicateSheet,
            this.cmb_exportSheetImage,
            this.toolStripMenuItem2,
            this.cmb_editSheetProperties});
            this.cms_sheetNodeRightClick.Name = "cms_sheetNodeRightClick";
            this.cms_sheetNodeRightClick.Size = new System.Drawing.Size(204, 142);
            // 
            // cmb_deleteSheet
            // 
            this.cmb_deleteSheet.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.cmb_deleteSheet.Name = "cmb_deleteSheet";
            this.cmb_deleteSheet.Size = new System.Drawing.Size(203, 22);
            this.cmb_deleteSheet.Text = "Delete Sheet";
            this.cmb_deleteSheet.Click += new System.EventHandler(this.cmb_deleteSheet_Click);
            // 
            // cmb_sheetCreateAnimation
            // 
            this.cmb_sheetCreateAnimation.Image = global::Pixelaria.Properties.Resources.anim_new_icon;
            this.cmb_sheetCreateAnimation.Name = "cmb_sheetCreateAnimation";
            this.cmb_sheetCreateAnimation.Size = new System.Drawing.Size(203, 22);
            this.cmb_sheetCreateAnimation.Text = "Create New Animation...";
            this.cmb_sheetCreateAnimation.Click += new System.EventHandler(this.tsm_sheetCreateAnimation_Click);
            // 
            // cmb_sheetImportAnimation
            // 
            this.cmb_sheetImportAnimation.Image = global::Pixelaria.Properties.Resources.edit_undo;
            this.cmb_sheetImportAnimation.Name = "cmb_sheetImportAnimation";
            this.cmb_sheetImportAnimation.Size = new System.Drawing.Size(203, 22);
            this.cmb_sheetImportAnimation.Text = "Import Animation...";
            this.cmb_sheetImportAnimation.Click += new System.EventHandler(this.tsm_sheetImportAnimation_Click);
            // 
            // cmb_duplicateSheet
            // 
            this.cmb_duplicateSheet.Image = global::Pixelaria.Properties.Resources.sheet_duplicate_icon;
            this.cmb_duplicateSheet.Name = "cmb_duplicateSheet";
            this.cmb_duplicateSheet.Size = new System.Drawing.Size(203, 22);
            this.cmb_duplicateSheet.Text = "Duplicate Sheet";
            this.cmb_duplicateSheet.Click += new System.EventHandler(this.tsm_duplicateSheet_Click);
            // 
            // cmb_exportSheetImage
            // 
            this.cmb_exportSheetImage.Image = global::Pixelaria.Properties.Resources.sheet_save_icon;
            this.cmb_exportSheetImage.Name = "cmb_exportSheetImage";
            this.cmb_exportSheetImage.Size = new System.Drawing.Size(203, 22);
            this.cmb_exportSheetImage.Text = "Export Sheet Image...";
            this.cmb_exportSheetImage.Click += new System.EventHandler(this.tsm_exportSheetImage_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(200, 6);
            // 
            // cmb_editSheetProperties
            // 
            this.cmb_editSheetProperties.Image = global::Pixelaria.Properties.Resources.edit_properties;
            this.cmb_editSheetProperties.Name = "cmb_editSheetProperties";
            this.cmb_editSheetProperties.Size = new System.Drawing.Size(203, 22);
            this.cmb_editSheetProperties.Text = "Edit Properties";
            this.cmb_editSheetProperties.Click += new System.EventHandler(this.tsm_editSheetPropertiesClick);
            // 
            // cms_animationNodeRightClick
            // 
            this.cms_animationNodeRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmb_deleteAnim,
            this.cmb_duplicateAnimation,
            this.cmb_saveAnimationStrip,
            this.toolStripMenuItem3,
            this.cmb_editAnimProperties});
            this.cms_animationNodeRightClick.Name = "contextMenuStrip1";
            this.cms_animationNodeRightClick.Size = new System.Drawing.Size(194, 98);
            // 
            // cmb_deleteAnim
            // 
            this.cmb_deleteAnim.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.cmb_deleteAnim.Name = "cmb_deleteAnim";
            this.cmb_deleteAnim.Size = new System.Drawing.Size(193, 22);
            this.cmb_deleteAnim.Text = "Delete Animation";
            this.cmb_deleteAnim.Click += new System.EventHandler(this.cmb_deleteAnim_Click);
            // 
            // cmb_duplicateAnimation
            // 
            this.cmb_duplicateAnimation.Image = global::Pixelaria.Properties.Resources.edit_copy;
            this.cmb_duplicateAnimation.Name = "cmb_duplicateAnimation";
            this.cmb_duplicateAnimation.Size = new System.Drawing.Size(193, 22);
            this.cmb_duplicateAnimation.Text = "Duplicate Animation";
            this.cmb_duplicateAnimation.Click += new System.EventHandler(this.cmb_duplicateAnimation_Click);
            // 
            // cmb_saveAnimationStrip
            // 
            this.cmb_saveAnimationStrip.Image = global::Pixelaria.Properties.Resources.frame_save_icon;
            this.cmb_saveAnimationStrip.Name = "cmb_saveAnimationStrip";
            this.cmb_saveAnimationStrip.Size = new System.Drawing.Size(193, 22);
            this.cmb_saveAnimationStrip.Text = "Save Animation Strip...";
            this.cmb_saveAnimationStrip.Click += new System.EventHandler(this.cmb_saveAnimationStrip_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(190, 6);
            // 
            // cmb_editAnimProperties
            // 
            this.cmb_editAnimProperties.Image = global::Pixelaria.Properties.Resources.edit_properties;
            this.cmb_editAnimProperties.Name = "cmb_editAnimProperties";
            this.cmb_editAnimProperties.Size = new System.Drawing.Size(193, 22);
            this.cmb_editAnimProperties.Text = "Edit Properties";
            this.cmb_editAnimProperties.Click += new System.EventHandler(this.cmb_editAnimProperties_Click);
            // 
            // splitter1
            // 
            this.splitter1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.splitter1.Location = new System.Drawing.Point(275, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 692);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            // 
            // il_treeView
            // 
            this.il_treeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("il_treeView.ImageStream")));
            this.il_treeView.TransparentColor = System.Drawing.Color.Transparent;
            this.il_treeView.Images.SetKeyName(0, "package-x-generic.png");
            this.il_treeView.Images.SetKeyName(1, "sheet_icon.png");
            this.il_treeView.Images.SetKeyName(2, "empty_icon.png");
            this.il_treeView.Images.SetKeyName(3, "anim_icon.png");
            // 
            // tv_bundleAnimations
            // 
            this.tv_bundleAnimations.AllowDrop = true;
            this.tv_bundleAnimations.Dock = System.Windows.Forms.DockStyle.Left;
            this.tv_bundleAnimations.FullRowSelect = true;
            this.tv_bundleAnimations.HideSelection = false;
            this.tv_bundleAnimations.ImageIndex = 0;
            this.tv_bundleAnimations.ImageList = this.il_treeView;
            this.tv_bundleAnimations.LoadedProjectTree = null;
            this.tv_bundleAnimations.Location = new System.Drawing.Point(0, 25);
            this.tv_bundleAnimations.Name = "tv_bundleAnimations";
            treeNode2.Name = "Node0";
            treeNode2.Text = "Bundle";
            this.tv_bundleAnimations.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2});
            this.tv_bundleAnimations.SelectedImageIndex = 0;
            this.tv_bundleAnimations.Size = new System.Drawing.Size(275, 692);
            this.tv_bundleAnimations.TabIndex = 2;
            this.tv_bundleAnimations.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.tv_bundleAnimations_BeforeCollapse);
            this.tv_bundleAnimations.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tv_bundleAnimations_BeforeExpand);
            this.tv_bundleAnimations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tv_bundleAnimations_KeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::Pixelaria.Properties.Resources.pxl_icon_tile_392x424;
            this.ClientSize = new System.Drawing.Size(1125, 717);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.tv_bundleAnimations);
            this.Controls.Add(this.ts_mainStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pixelaria";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ts_mainStrip.ResumeLayout(false);
            this.ts_mainStrip.PerformLayout();
            this.cms_bundleNodeRightClick.ResumeLayout(false);
            this.cms_sheetNodeRightClick.ResumeLayout(false);
            this.cms_animationNodeRightClick.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Pixelaria.Views.Controls.ProjectTreeView tv_bundleAnimations;
        private System.Windows.Forms.ToolStrip ts_mainStrip;
        private System.Windows.Forms.ToolStripButton tsb_bundleSettings;
        private System.Windows.Forms.ContextMenuStrip cms_bundleNodeRightClick;
        private System.Windows.Forms.ToolStripMenuItem cmb_bundleSettings;
        private System.Windows.Forms.ToolStripMenuItem cmb_createNewAnimation;
        private System.Windows.Forms.ToolStripMenuItem cmb_importAnimation;
        private System.Windows.Forms.ToolStripButton tsb_createAnimation;
        private System.Windows.Forms.ToolStripButton tsb_importAnimation;
        private System.Windows.Forms.MainMenu mm_menu;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem mi_new;
        private System.Windows.Forms.MenuItem mi_open;
        private System.Windows.Forms.MenuItem mi_save;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem mi_quit;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem mi_addAnimation;
        private System.Windows.Forms.MenuItem menuItem9;
        private System.Windows.Forms.MenuItem mi_cascade;
        private System.Windows.Forms.MenuItem mi_tileHorizontally;
        private System.Windows.Forms.MenuItem mi_arrangeIcons;
        private System.Windows.Forms.ToolStripMenuItem cmb_createNewAnimationSheet;
        private System.Windows.Forms.ToolStripButton tsb_createAnimationSheet;
        private System.Windows.Forms.ContextMenuStrip cms_sheetNodeRightClick;
        private System.Windows.Forms.ToolStripMenuItem cmb_editSheetProperties;
        private System.Windows.Forms.ToolStripButton tsb_new;
        private System.Windows.Forms.ToolStripButton tsb_open;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsb_save;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem mi_about;
        private System.Windows.Forms.MenuItem mi_recentFiles;
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem mi_saveAs;
        private System.Windows.Forms.ToolStripMenuItem cmb_sheetCreateAnimation;
        private System.Windows.Forms.ToolStripMenuItem cmb_sheetImportAnimation;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ContextMenuStrip cms_animationNodeRightClick;
        private System.Windows.Forms.ToolStripMenuItem cmb_deleteAnim;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem cmb_editAnimProperties;
        private System.Windows.Forms.ToolStripMenuItem cmb_deleteSheet;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsb_exportButton;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ToolStripMenuItem cmb_duplicateAnimation;
        private System.Windows.Forms.ToolStripMenuItem cmb_duplicateSheet;
        private System.Windows.Forms.ToolStripMenuItem cmb_exportSheetImage;
        private System.Windows.Forms.MenuItem mi_fileBug;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.ImageList il_treeView;
        private System.Windows.Forms.ToolStripMenuItem cmb_saveAnimationStrip;
    }
}
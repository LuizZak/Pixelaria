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

namespace Pixelaria.Views.ModelViews
{
    partial class AnimationView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationView));
            this.il_framesThumbs = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cb_enablePreview = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.animationPreviewPanel = new Pixelaria.Views.ModelViews.AnimationPreviewPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lv_frames = new Pixelaria.Views.Controls.RearrangeableListView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_frameskip = new System.Windows.Forms.CheckBox();
            this.nud_fps = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsb_applyChangesAndClose = new System.Windows.Forms.ToolStripButton();
            this.tsb_applyChanges = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_editFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_insertFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_addNewFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_resizeAnim = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_copyFrames = new System.Windows.Forms.ToolStripButton();
            this.tsb_cutFrames = new System.Windows.Forms.ToolStripButton();
            this.tsb_pasteFrames = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_undo = new System.Windows.Forms.ToolStripButton();
            this.tsb_redo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.txt_animName = new System.Windows.Forms.ToolStripTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_addFrameFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_undo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_redo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_copy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_cut = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_paste = new System.Windows.Forms.ToolStripMenuItem();
            this.framesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_insertFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_addNewFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsl_error = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_dimensions = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_frameCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_memory = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_fps)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // il_framesThumbs
            // 
            this.il_framesThumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.il_framesThumbs.ImageSize = new System.Drawing.Size(48, 48);
            this.il_framesThumbs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 49);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.cb_enablePreview);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Panel2.Controls.Add(this.panel3);
            this.splitContainer1.Size = new System.Drawing.Size(792, 411);
            this.splitContainer1.SplitterDistance = 210;
            this.splitContainer1.TabIndex = 2;
            // 
            // cb_enablePreview
            // 
            this.cb_enablePreview.AutoSize = true;
            this.cb_enablePreview.Checked = true;
            this.cb_enablePreview.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_enablePreview.Location = new System.Drawing.Point(3, 19);
            this.cb_enablePreview.Name = "cb_enablePreview";
            this.cb_enablePreview.Size = new System.Drawing.Size(100, 17);
            this.cb_enablePreview.TabIndex = 0;
            this.cb_enablePreview.Text = "Enable Preview";
            this.cb_enablePreview.UseVisualStyleBackColor = true;
            this.cb_enablePreview.CheckedChanged += new System.EventHandler(this.cb_enablePreview_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.animationPreviewPanel);
            this.panel1.Location = new System.Drawing.Point(3, 38);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(205, 658);
            this.panel1.TabIndex = 1;
            // 
            // animationPreviewPanel
            // 
            this.animationPreviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationPreviewPanel.Location = new System.Drawing.Point(0, 0);
            this.animationPreviewPanel.Name = "animationPreviewPanel";
            this.animationPreviewPanel.Size = new System.Drawing.Size(201, 654);
            this.animationPreviewPanel.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Animation Preview";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.lv_frames);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(578, 355);
            this.panel2.TabIndex = 2;
            // 
            // lv_frames
            // 
            this.lv_frames.AllowDrop = true;
            this.lv_frames.BackColor = System.Drawing.SystemColors.Control;
            this.lv_frames.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lv_frames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_frames.HideSelection = false;
            this.lv_frames.LargeImageList = this.il_framesThumbs;
            this.lv_frames.Location = new System.Drawing.Point(0, 0);
            this.lv_frames.Name = "lv_frames";
            this.lv_frames.ShowGroups = false;
            this.lv_frames.Size = new System.Drawing.Size(574, 351);
            this.lv_frames.TabIndex = 1;
            this.lv_frames.UseCompatibleStateImageBehavior = false;
            this.lv_frames.DragOperation += new Pixelaria.Views.Controls.RearrangeableListView.DragOperationHandler(this.lv_frames_DragOperation);
            this.lv_frames.SelectedIndexChanged += new System.EventHandler(this.lv_frames_SelectedIndexChanged);
            this.lv_frames.DoubleClick += new System.EventHandler(this.lv_frames_DoubleClick);
            this.lv_frames.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lv_frames_KeyDown);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.groupBox1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 355);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(578, 56);
            this.panel3.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_frameskip);
            this.groupBox1.Controls.Add(this.nud_fps);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 50);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Playback Settings";
            // 
            // cb_frameskip
            // 
            this.cb_frameskip.AutoSize = true;
            this.cb_frameskip.Location = new System.Drawing.Point(151, 22);
            this.cb_frameskip.Name = "cb_frameskip";
            this.cb_frameskip.Size = new System.Drawing.Size(74, 17);
            this.cb_frameskip.TabIndex = 2;
            this.cb_frameskip.Text = "Frameskip";
            this.cb_frameskip.UseVisualStyleBackColor = true;
            this.cb_frameskip.CheckedChanged += new System.EventHandler(this.cb_frameskip_CheckedChanged);
            // 
            // nud_fps
            // 
            this.nud_fps.Location = new System.Drawing.Point(42, 19);
            this.nud_fps.Maximum = new decimal(new int[] {
            420,
            0,
            0,
            0});
            this.nud_fps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nud_fps.Name = "nud_fps";
            this.nud_fps.Size = new System.Drawing.Size(103, 20);
            this.nud_fps.TabIndex = 1;
            this.nud_fps.ValueChanged += new System.EventHandler(this.nud_fps_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "FPS:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.CanOverflow = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsb_applyChangesAndClose,
            this.tsb_applyChanges,
            this.toolStripSeparator3,
            this.tsb_editFrame,
            this.tsb_insertFrame,
            this.tsb_addNewFrame,
            this.tsb_resizeAnim,
            this.toolStripSeparator1,
            this.tsb_copyFrames,
            this.tsb_cutFrames,
            this.tsb_pasteFrames,
            this.toolStripSeparator4,
            this.tsb_undo,
            this.tsb_redo,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.txt_animName});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(792, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsb_applyChangesAndClose
            // 
            this.tsb_applyChangesAndClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_applyChangesAndClose.Image = global::Pixelaria.Properties.Resources.action_check1;
            this.tsb_applyChangesAndClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_applyChangesAndClose.Name = "tsb_applyChangesAndClose";
            this.tsb_applyChangesAndClose.Size = new System.Drawing.Size(23, 22);
            this.tsb_applyChangesAndClose.Text = "Apply changes and close";
            this.tsb_applyChangesAndClose.Click += new System.EventHandler(this.tsb_applyChangesAndClose_Click);
            // 
            // tsb_applyChanges
            // 
            this.tsb_applyChanges.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_applyChanges.Image = global::Pixelaria.Properties.Resources.download;
            this.tsb_applyChanges.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_applyChanges.Name = "tsb_applyChanges";
            this.tsb_applyChanges.Size = new System.Drawing.Size(23, 22);
            this.tsb_applyChanges.Text = "Apply changes";
            this.tsb_applyChanges.Click += new System.EventHandler(this.tsb_applyChanges_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_editFrame
            // 
            this.tsb_editFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_editFrame.Image = global::Pixelaria.Properties.Resources.frame_edit_icon;
            this.tsb_editFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_editFrame.Name = "tsb_editFrame";
            this.tsb_editFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_editFrame.Text = "Edit currently selected frame";
            this.tsb_editFrame.Click += new System.EventHandler(this.tsb_editFrame_Click);
            // 
            // tsb_insertFrame
            // 
            this.tsb_insertFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_insertFrame.Image = global::Pixelaria.Properties.Resources.frame_insert_new_icon;
            this.tsb_insertFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_insertFrame.Name = "tsb_insertFrame";
            this.tsb_insertFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_insertFrame.Text = "Insert frame before currently selected range";
            this.tsb_insertFrame.Click += new System.EventHandler(this.tsb_insertFrame_Click);
            // 
            // tsb_addNewFrame
            // 
            this.tsb_addNewFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_addNewFrame.Image = global::Pixelaria.Properties.Resources.frame_add_new_icon;
            this.tsb_addNewFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_addNewFrame.Name = "tsb_addNewFrame";
            this.tsb_addNewFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_addNewFrame.Text = "Add new frame at the end of the animation";
            this.tsb_addNewFrame.Click += new System.EventHandler(this.tsb_addNewFrame_Click);
            // 
            // tsb_resizeAnim
            // 
            this.tsb_resizeAnim.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_resizeAnim.Image = global::Pixelaria.Properties.Resources.anim_resize_icon;
            this.tsb_resizeAnim.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_resizeAnim.Name = "tsb_resizeAnim";
            this.tsb_resizeAnim.Size = new System.Drawing.Size(23, 22);
            this.tsb_resizeAnim.Text = "Resize Animation";
            this.tsb_resizeAnim.Click += new System.EventHandler(this.tsb_resizeAnim_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_copyFrames
            // 
            this.tsb_copyFrames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_copyFrames.Image = global::Pixelaria.Properties.Resources.edit_copy;
            this.tsb_copyFrames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_copyFrames.Name = "tsb_copyFrames";
            this.tsb_copyFrames.Size = new System.Drawing.Size(23, 22);
            this.tsb_copyFrames.Text = "Copy selected frames";
            this.tsb_copyFrames.Click += new System.EventHandler(this.tsb_copyFrames_Click);
            // 
            // tsb_cutFrames
            // 
            this.tsb_cutFrames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_cutFrames.Image = global::Pixelaria.Properties.Resources.edit_cut;
            this.tsb_cutFrames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_cutFrames.Name = "tsb_cutFrames";
            this.tsb_cutFrames.Size = new System.Drawing.Size(23, 22);
            this.tsb_cutFrames.Text = "Cut selected frames";
            this.tsb_cutFrames.Click += new System.EventHandler(this.tsb_cutFrames_Click);
            // 
            // tsb_pasteFrames
            // 
            this.tsb_pasteFrames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_pasteFrames.Image = global::Pixelaria.Properties.Resources.edit_paste;
            this.tsb_pasteFrames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_pasteFrames.Name = "tsb_pasteFrames";
            this.tsb_pasteFrames.Size = new System.Drawing.Size(23, 22);
            this.tsb_pasteFrames.Text = "Paste frames";
            this.tsb_pasteFrames.Click += new System.EventHandler(this.tsb_pasteFrames_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_undo
            // 
            this.tsb_undo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_undo.Image = global::Pixelaria.Properties.Resources.edit_undo;
            this.tsb_undo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_undo.Name = "tsb_undo";
            this.tsb_undo.Size = new System.Drawing.Size(23, 22);
            this.tsb_undo.Text = "Undo";
            this.tsb_undo.Click += new System.EventHandler(this.tsb_undo_Click);
            // 
            // tsb_redo
            // 
            this.tsb_redo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_redo.Image = global::Pixelaria.Properties.Resources.edit_redo;
            this.tsb_redo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_redo.Name = "tsb_redo";
            this.tsb_redo.Size = new System.Drawing.Size(23, 22);
            this.tsb_redo.Text = "Redo";
            this.tsb_redo.Click += new System.EventHandler(this.tsb_redo_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(42, 22);
            this.toolStripLabel1.Text = "Name:";
            // 
            // txt_animName
            // 
            this.txt_animName.Name = "txt_animName";
            this.txt_animName.Size = new System.Drawing.Size(150, 25);
            this.txt_animName.TextChanged += new System.EventHandler(this.txt_animName_TextChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.AllowMerge = false;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.framesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(792, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_addFrameFromFile});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // tsm_addFrameFromFile
            // 
            this.tsm_addFrameFromFile.Image = global::Pixelaria.Properties.Resources.frame_open_icon;
            this.tsm_addFrameFromFile.Name = "tsm_addFrameFromFile";
            this.tsm_addFrameFromFile.Size = new System.Drawing.Size(187, 22);
            this.tsm_addFrameFromFile.Text = "Add frame form file...";
            this.tsm_addFrameFromFile.Click += new System.EventHandler(this.tsm_addFrameFromFile_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_undo,
            this.tsm_redo,
            this.toolStripMenuItem1,
            this.tsm_copy,
            this.tsm_cut,
            this.tsm_paste});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // tsm_undo
            // 
            this.tsm_undo.Image = global::Pixelaria.Properties.Resources.edit_undo;
            this.tsm_undo.Name = "tsm_undo";
            this.tsm_undo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.tsm_undo.Size = new System.Drawing.Size(144, 22);
            this.tsm_undo.Text = "Undo";
            this.tsm_undo.Click += new System.EventHandler(this.tsm_undo_Click);
            // 
            // tsm_redo
            // 
            this.tsm_redo.Image = global::Pixelaria.Properties.Resources.edit_redo;
            this.tsm_redo.Name = "tsm_redo";
            this.tsm_redo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.tsm_redo.Size = new System.Drawing.Size(144, 22);
            this.tsm_redo.Text = "Redo";
            this.tsm_redo.Click += new System.EventHandler(this.tsm_redo_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(141, 6);
            // 
            // tsm_copy
            // 
            this.tsm_copy.Image = global::Pixelaria.Properties.Resources.edit_copy;
            this.tsm_copy.Name = "tsm_copy";
            this.tsm_copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.tsm_copy.Size = new System.Drawing.Size(144, 22);
            this.tsm_copy.Text = "Copy";
            // 
            // tsm_cut
            // 
            this.tsm_cut.Image = global::Pixelaria.Properties.Resources.edit_cut;
            this.tsm_cut.Name = "tsm_cut";
            this.tsm_cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.tsm_cut.Size = new System.Drawing.Size(144, 22);
            this.tsm_cut.Text = "Cut";
            // 
            // tsm_paste
            // 
            this.tsm_paste.Image = global::Pixelaria.Properties.Resources.edit_paste;
            this.tsm_paste.Name = "tsm_paste";
            this.tsm_paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.tsm_paste.Size = new System.Drawing.Size(144, 22);
            this.tsm_paste.Text = "Paste";
            // 
            // framesToolStripMenuItem
            // 
            this.framesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_insertFrame,
            this.tsm_addNewFrame});
            this.framesToolStripMenuItem.Name = "framesToolStripMenuItem";
            this.framesToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.framesToolStripMenuItem.Text = "Frames";
            // 
            // tsm_insertFrame
            // 
            this.tsm_insertFrame.Image = global::Pixelaria.Properties.Resources.frame_insert_new_icon;
            this.tsm_insertFrame.Name = "tsm_insertFrame";
            this.tsm_insertFrame.Size = new System.Drawing.Size(159, 22);
            this.tsm_insertFrame.Text = "Insert Frame";
            this.tsm_insertFrame.Click += new System.EventHandler(this.tsm_insertFrame_Click);
            // 
            // tsm_addNewFrame
            // 
            this.tsm_addNewFrame.Image = global::Pixelaria.Properties.Resources.frame_add_new_icon;
            this.tsm_addNewFrame.Name = "tsm_addNewFrame";
            this.tsm_addNewFrame.Size = new System.Drawing.Size(159, 22);
            this.tsm_addNewFrame.Text = "Add New Frame";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsl_error,
            this.toolStripStatusLabel1,
            this.tssl_dimensions,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel2,
            this.tssl_frameCount,
            this.toolStripStatusLabel5,
            this.toolStripStatusLabel4,
            this.tssl_memory,
            this.toolStripStatusLabel6});
            this.statusStrip1.Location = new System.Drawing.Point(0, 460);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(792, 25);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsl_error
            // 
            this.tsl_error.Image = global::Pixelaria.Properties.Resources.process_stop;
            this.tsl_error.Name = "tsl_error";
            this.tsl_error.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.tsl_error.Size = new System.Drawing.Size(79, 20);
            this.tsl_error.Text = "Error Label";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(72, 20);
            this.toolStripStatusLabel1.Text = "Dimensions:";
            // 
            // tssl_dimensions
            // 
            this.tssl_dimensions.Name = "tssl_dimensions";
            this.tssl_dimensions.Size = new System.Drawing.Size(12, 20);
            this.tssl_dimensions.Text = "-";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.AutoSize = false;
            this.toolStripStatusLabel3.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel3.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(10, 20);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(79, 20);
            this.toolStripStatusLabel2.Text = "Frame Count:";
            // 
            // tssl_frameCount
            // 
            this.tssl_frameCount.Name = "tssl_frameCount";
            this.tssl_frameCount.Size = new System.Drawing.Size(12, 20);
            this.tssl_frameCount.Text = "-";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.AutoSize = false;
            this.toolStripStatusLabel5.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel5.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(10, 20);
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(55, 20);
            this.toolStripStatusLabel4.Text = "Memory:";
            // 
            // tssl_memory
            // 
            this.tssl_memory.Name = "tssl_memory";
            this.tssl_memory.Size = new System.Drawing.Size(12, 20);
            this.tssl_memory.Text = "-";
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.AutoSize = false;
            this.toolStripStatusLabel6.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel6.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(10, 20);
            // 
            // AnimationView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 485);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(808, 523);
            this.Name = "AnimationView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Animation [ ]";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AnimationView_FormClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AnimationView_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_fps)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem framesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsm_addNewFrame;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ImageList il_framesThumbs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cb_enablePreview;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cb_frameskip;
        private System.Windows.Forms.NumericUpDown nud_fps;
        private System.Windows.Forms.Label label3;
        private AnimationPreviewPanel animationPreviewPanel;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsb_addNewFrame;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tssl_dimensions;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel tssl_frameCount;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel tssl_memory;
        private System.Windows.Forms.ToolStripButton tsb_applyChangesAndClose;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.ToolStripStatusLabel tsl_error;
        private System.Windows.Forms.ToolStripButton tsb_copyFrames;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsb_pasteFrames;
        private System.Windows.Forms.ToolStripButton tsb_cutFrames;
        private Pixelaria.Views.Controls.RearrangeableListView lv_frames;
        private System.Windows.Forms.ToolStripTextBox txt_animName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsb_insertFrame;
        private System.Windows.Forms.ToolStripMenuItem tsm_insertFrame;
        private System.Windows.Forms.ToolStripButton tsb_editFrame;
        private System.Windows.Forms.ToolStripButton tsb_applyChanges;
        private System.Windows.Forms.ToolStripMenuItem tsm_addFrameFromFile;
        private System.Windows.Forms.ToolStripButton tsb_resizeAnim;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsm_copy;
        private System.Windows.Forms.ToolStripMenuItem tsm_cut;
        private System.Windows.Forms.ToolStripMenuItem tsm_paste;
        private System.Windows.Forms.ToolStripButton tsb_undo;
        private System.Windows.Forms.ToolStripButton tsb_redo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tsm_undo;
        private System.Windows.Forms.ToolStripMenuItem tsm_redo;
    }
}
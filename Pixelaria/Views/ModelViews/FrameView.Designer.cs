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

using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.ColorControls;
using Pixelaria.Views.Controls.LayerControls;
using Pixelaria.Views.Controls.PaintTools;

namespace Pixelaria.Views.ModelViews
{
    partial class FrameView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrameView));
            Pixelaria.Views.Controls.PaintTools.NullPaintTool nullPaintTool1 = new Pixelaria.Views.Controls.PaintTools.NullPaintTool();
            Pixelaria.Data.Undo.UndoSystem undoSystem1 = new Pixelaria.Data.Undo.UndoSystem();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.tt_mainTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.rb_pencil = new System.Windows.Forms.RadioButton();
            this.rb_eraser = new System.Windows.Forms.RadioButton();
            this.rb_picker = new System.Windows.Forms.RadioButton();
            this.rb_sprayPaint = new System.Windows.Forms.RadioButton();
            this.rb_line = new System.Windows.Forms.RadioButton();
            this.rb_rectangle = new System.Windows.Forms.RadioButton();
            this.rb_circle = new System.Windows.Forms.RadioButton();
            this.rb_bucket = new System.Windows.Forms.RadioButton();
            this.rb_selection = new System.Windows.Forms.RadioButton();
            this.rb_zoom = new System.Windows.Forms.RadioButton();
            this.pnl_framePreview = new Pixelaria.Views.Controls.LabeledPanel();
            this.zpb_framePreview = new Pixelaria.Views.Controls.ZoomablePictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.tc_currentFrame = new Pixelaria.Views.Controls.TimelineControl();
            this.pb_zoomIcon = new System.Windows.Forms.PictureBox();
            this.anud_zoom = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.cs_colorSwatch = new Pixelaria.Views.Controls.ColorControls.ColorSwatchControl();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.cp_mainColorPicker = new Pixelaria.Views.Controls.ColorControls.ColorPicker();
            this.panel1 = new Pixelaria.Views.Controls.LabeledPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_blendingBlend = new System.Windows.Forms.RadioButton();
            this.rb_blendingReplace = new System.Windows.Forms.RadioButton();
            this.gb_sizeGroup = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.btn_brushSize_1 = new System.Windows.Forms.Button();
            this.btn_brushSize_2 = new System.Windows.Forms.Button();
            this.btn_brushSize_3 = new System.Windows.Forms.Button();
            this.btn_brushSize_4 = new System.Windows.Forms.Button();
            this.btn_brushSize_5 = new System.Windows.Forms.Button();
            this.btn_brushSize_6 = new System.Windows.Forms.Button();
            this.anud_brushSize = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.gb_fillMode = new System.Windows.Forms.GroupBox();
            this.rb_fillMode_3 = new System.Windows.Forms.RadioButton();
            this.rb_fillMode_2 = new System.Windows.Forms.RadioButton();
            this.rb_fillMode_1 = new System.Windows.Forms.RadioButton();
            this.gb_otherGroup = new System.Windows.Forms.GroupBox();
            this.cb_airbrushMode = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsb_applyChangesAndClose = new System.Windows.Forms.ToolStripButton();
            this.tsb_applyChanges = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_prevFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_nextFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_insertNewframe = new System.Windows.Forms.ToolStripButton();
            this.tsb_addFrameAtEnd = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_clearFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_copy = new System.Windows.Forms.ToolStripButton();
            this.tsb_cut = new System.Windows.Forms.ToolStripButton();
            this.tsb_paste = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_undo = new System.Windows.Forms.ToolStripButton();
            this.tsb_redo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_grid = new System.Windows.Forms.ToolStripButton();
            this.tsb_previewFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_previewAnimation = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_onionSkin = new System.Windows.Forms.ToolStripButton();
            this.tsb_osPrevFrames = new System.Windows.Forms.ToolStripButton();
            this.tsb_osShowCurrentFrame = new System.Windows.Forms.ToolStripButton();
            this.tsb_osNextFrames = new System.Windows.Forms.ToolStripButton();
            this.tsb_osDisplayOnFront = new System.Windows.Forms.ToolStripButton();
            this.tsl_onionSkinDepth = new System.Windows.Forms.ToolStripLabel();
            this.tscb_osFrameCount = new System.Windows.Forms.ToolStripComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_exportFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_importFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_undo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_redo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_copy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_cut = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_paste = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_selectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_prevFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_nextFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_switchBlendingMode = new System.Windows.Forms.ToolStripMenuItem();
            this.layersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_toggleVisibleLayers = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_resetLayerTransparencies = new System.Windows.Forms.ToolStripMenuItem();
            this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_expandAllLayers = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_collapseAllLayers = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_filters = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_emptyFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_filterPresets = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_lastUsedFilterPresets = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsl_coordinates = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsl_operationLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.iepb_frame = new Pixelaria.Views.Controls.ImageEditPanel();
            this.lcp_layers = new Pixelaria.Views.Controls.LayerControls.LayerControlPanel();
            this.pnl_framePreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_framePreview)).BeginInit();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_zoomIcon)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gb_sizeGroup.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.gb_fillMode.SuspendLayout();
            this.gb_otherGroup.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // rb_pencil
            // 
            this.rb_pencil.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_pencil.AutoSize = true;
            this.rb_pencil.Checked = true;
            this.rb_pencil.Image = global::Pixelaria.Properties.Resources.pencil_icon;
            this.rb_pencil.Location = new System.Drawing.Point(3, 3);
            this.rb_pencil.Name = "rb_pencil";
            this.rb_pencil.Size = new System.Drawing.Size(22, 22);
            this.rb_pencil.TabIndex = 9;
            this.rb_pencil.TabStop = true;
            this.tt_mainTooltip.SetToolTip(this.rb_pencil, "\"Pencil (D)\"");
            this.rb_pencil.UseVisualStyleBackColor = true;
            this.rb_pencil.CheckedChanged += new System.EventHandler(this.rb_pencil_CheckedChanged);
            // 
            // rb_eraser
            // 
            this.rb_eraser.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_eraser.AutoSize = true;
            this.rb_eraser.Image = global::Pixelaria.Properties.Resources.eraser_icon;
            this.rb_eraser.Location = new System.Drawing.Point(31, 3);
            this.rb_eraser.Name = "rb_eraser";
            this.rb_eraser.Size = new System.Drawing.Size(22, 22);
            this.rb_eraser.TabIndex = 10;
            this.tt_mainTooltip.SetToolTip(this.rb_eraser, "Eraser (E)");
            this.rb_eraser.UseVisualStyleBackColor = true;
            this.rb_eraser.CheckedChanged += new System.EventHandler(this.rb_eraser_CheckedChanged);
            // 
            // rb_picker
            // 
            this.rb_picker.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_picker.AutoSize = true;
            this.rb_picker.Image = global::Pixelaria.Properties.Resources.picker_icon;
            this.rb_picker.Location = new System.Drawing.Point(59, 3);
            this.rb_picker.Name = "rb_picker";
            this.rb_picker.Size = new System.Drawing.Size(22, 22);
            this.rb_picker.TabIndex = 11;
            this.tt_mainTooltip.SetToolTip(this.rb_picker, "Color Picker (C)");
            this.rb_picker.UseVisualStyleBackColor = true;
            this.rb_picker.CheckedChanged += new System.EventHandler(this.rb_picker_CheckedChanged);
            // 
            // rb_sprayPaint
            // 
            this.rb_sprayPaint.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_sprayPaint.AutoSize = true;
            this.rb_sprayPaint.Image = global::Pixelaria.Properties.Resources.spray_icon;
            this.rb_sprayPaint.Location = new System.Drawing.Point(3, 31);
            this.rb_sprayPaint.Name = "rb_sprayPaint";
            this.rb_sprayPaint.Size = new System.Drawing.Size(22, 22);
            this.rb_sprayPaint.TabIndex = 18;
            this.tt_mainTooltip.SetToolTip(this.rb_sprayPaint, "Spray Paint (V)");
            this.rb_sprayPaint.UseVisualStyleBackColor = true;
            this.rb_sprayPaint.CheckedChanged += new System.EventHandler(this.rb_sprayPaint_CheckedChanged);
            // 
            // rb_line
            // 
            this.rb_line.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_line.AutoSize = true;
            this.rb_line.Image = global::Pixelaria.Properties.Resources.line_icon;
            this.rb_line.Location = new System.Drawing.Point(31, 31);
            this.rb_line.Name = "rb_line";
            this.rb_line.Size = new System.Drawing.Size(22, 22);
            this.rb_line.TabIndex = 17;
            this.rb_line.TabStop = true;
            this.tt_mainTooltip.SetToolTip(this.rb_line, "Line (V)");
            this.rb_line.UseVisualStyleBackColor = true;
            this.rb_line.CheckedChanged += new System.EventHandler(this.rb_line_CheckedChanged);
            // 
            // rb_rectangle
            // 
            this.rb_rectangle.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_rectangle.AutoSize = true;
            this.rb_rectangle.Image = global::Pixelaria.Properties.Resources.rectangle_icon;
            this.rb_rectangle.Location = new System.Drawing.Point(59, 31);
            this.rb_rectangle.Name = "rb_rectangle";
            this.rb_rectangle.Size = new System.Drawing.Size(22, 22);
            this.rb_rectangle.TabIndex = 12;
            this.tt_mainTooltip.SetToolTip(this.rb_rectangle, "Rectangle (R)");
            this.rb_rectangle.UseVisualStyleBackColor = true;
            this.rb_rectangle.CheckedChanged += new System.EventHandler(this.rb_rectangle_CheckedChanged);
            // 
            // rb_circle
            // 
            this.rb_circle.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_circle.AutoSize = true;
            this.rb_circle.Image = global::Pixelaria.Properties.Resources.circle_icon;
            this.rb_circle.Location = new System.Drawing.Point(3, 59);
            this.rb_circle.Name = "rb_circle";
            this.rb_circle.Size = new System.Drawing.Size(22, 22);
            this.rb_circle.TabIndex = 13;
            this.tt_mainTooltip.SetToolTip(this.rb_circle, "Ellipse (Q)");
            this.rb_circle.UseVisualStyleBackColor = true;
            this.rb_circle.CheckedChanged += new System.EventHandler(this.rb_circle_CheckedChanged);
            // 
            // rb_bucket
            // 
            this.rb_bucket.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_bucket.AutoSize = true;
            this.rb_bucket.Image = global::Pixelaria.Properties.Resources.bucket_icon;
            this.rb_bucket.Location = new System.Drawing.Point(31, 59);
            this.rb_bucket.Name = "rb_bucket";
            this.rb_bucket.Size = new System.Drawing.Size(22, 22);
            this.rb_bucket.TabIndex = 14;
            this.tt_mainTooltip.SetToolTip(this.rb_bucket, "Flood Fill (F)");
            this.rb_bucket.UseVisualStyleBackColor = true;
            this.rb_bucket.CheckedChanged += new System.EventHandler(this.rb_bucket_CheckedChanged);
            // 
            // rb_selection
            // 
            this.rb_selection.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_selection.AutoSize = true;
            this.rb_selection.Image = global::Pixelaria.Properties.Resources.selection_icon;
            this.rb_selection.Location = new System.Drawing.Point(59, 59);
            this.rb_selection.Name = "rb_selection";
            this.rb_selection.Size = new System.Drawing.Size(22, 22);
            this.rb_selection.TabIndex = 15;
            this.tt_mainTooltip.SetToolTip(this.rb_selection, "Selection (S)");
            this.rb_selection.UseVisualStyleBackColor = true;
            this.rb_selection.CheckedChanged += new System.EventHandler(this.rb_selection_CheckedChanged);
            // 
            // rb_zoom
            // 
            this.rb_zoom.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_zoom.AutoSize = true;
            this.rb_zoom.Enabled = false;
            this.rb_zoom.Image = global::Pixelaria.Properties.Resources.zoom_icon;
            this.rb_zoom.Location = new System.Drawing.Point(3, 87);
            this.rb_zoom.Name = "rb_zoom";
            this.rb_zoom.Size = new System.Drawing.Size(22, 22);
            this.rb_zoom.TabIndex = 16;
            this.tt_mainTooltip.SetToolTip(this.rb_zoom, "Zoom (Z)");
            this.rb_zoom.UseVisualStyleBackColor = true;
            this.rb_zoom.Visible = false;
            this.rb_zoom.CheckedChanged += new System.EventHandler(this.rb_zoom_CheckedChanged);
            // 
            // pnl_framePreview
            // 
            this.pnl_framePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_framePreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnl_framePreview.Controls.Add(this.zpb_framePreview);
            this.pnl_framePreview.Location = new System.Drawing.Point(397, 49);
            this.pnl_framePreview.Name = "pnl_framePreview";
            this.pnl_framePreview.PanelTitle = "Frame Preview";
            this.pnl_framePreview.Size = new System.Drawing.Size(249, 318);
            this.pnl_framePreview.TabIndex = 7;
            // 
            // zpb_framePreview
            // 
            this.zpb_framePreview.AllowScrollbars = false;
            this.zpb_framePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zpb_framePreview.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_framePreview.ClipBackgroundToImage = true;
            this.zpb_framePreview.HorizontalScrollValue = 0;
            this.zpb_framePreview.Location = new System.Drawing.Point(0, 18);
            this.zpb_framePreview.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_framePreview.MaximumZoom")));
            this.zpb_framePreview.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_framePreview.MinimumZoom")));
            this.zpb_framePreview.Name = "zpb_framePreview";
            this.zpb_framePreview.Size = new System.Drawing.Size(245, 296);
            this.zpb_framePreview.TabIndex = 0;
            this.zpb_framePreview.TabStop = false;
            this.zpb_framePreview.VerticalScrollValue = 0;
            this.zpb_framePreview.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_framePreview.Zoom")));
            this.zpb_framePreview.ZoomFactor = 1.414214F;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel4.Controls.Add(this.tc_currentFrame);
            this.panel4.Controls.Add(this.pb_zoomIcon);
            this.panel4.Controls.Add(this.anud_zoom);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(90, 586);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(556, 92);
            this.panel4.TabIndex = 5;
            // 
            // tc_currentFrame
            // 
            this.tc_currentFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tc_currentFrame.BehaviorType = Pixelaria.Views.Controls.TimelineBehaviorType.Timeline;
            this.tc_currentFrame.CurrentFrame = 2;
            this.tc_currentFrame.FrameDisplayType = Pixelaria.Views.Controls.TimelineFrameDisplayType.FrameNumber;
            this.tc_currentFrame.Location = new System.Drawing.Point(4, 3);
            this.tc_currentFrame.Maximum = 10;
            this.tc_currentFrame.Minimum = 0;
            this.tc_currentFrame.Name = "tc_currentFrame";
            this.tc_currentFrame.Range = new System.Drawing.Point(0, 10);
            this.tc_currentFrame.ScrollScaleWidth = 1D;
            this.tc_currentFrame.ScrollX = 0D;
            this.tc_currentFrame.Size = new System.Drawing.Size(545, 41);
            this.tc_currentFrame.TabIndex = 2;
            this.tc_currentFrame.FrameChanged += new Pixelaria.Views.Controls.TimelineControl.FrameChangedEventHandler(this.tc_currentFrame_FrameChanged);
            // 
            // pb_zoomIcon
            // 
            this.pb_zoomIcon.Image = global::Pixelaria.Properties.Resources.zoom_icon;
            this.pb_zoomIcon.Location = new System.Drawing.Point(4, 50);
            this.pb_zoomIcon.Name = "pb_zoomIcon";
            this.pb_zoomIcon.Size = new System.Drawing.Size(16, 16);
            this.pb_zoomIcon.TabIndex = 1;
            this.pb_zoomIcon.TabStop = false;
            // 
            // anud_zoom
            // 
            this.anud_zoom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_zoom.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_zoom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_zoom.DecimalPlaces = 2;
            this.anud_zoom.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.anud_zoom.Location = new System.Drawing.Point(26, 50);
            this.anud_zoom.Maximum = new decimal(new int[] {
            160,
            0,
            0,
            0});
            this.anud_zoom.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.anud_zoom.Name = "anud_zoom";
            this.anud_zoom.Size = new System.Drawing.Size(523, 35);
            this.anud_zoom.TabIndex = 0;
            this.anud_zoom.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_zoom.ValueChanged += new System.EventHandler(this.anud_zoom_ValueChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.splitter1);
            this.panel2.Controls.Add(this.cp_mainColorPicker);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(646, 49);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(172, 629);
            this.panel2.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.cs_colorSwatch);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 553);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(172, 284);
            this.panel3.TabIndex = 5;
            // 
            // cs_colorSwatch
            // 
            this.cs_colorSwatch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_colorSwatch.Location = new System.Drawing.Point(0, 0);
            this.cs_colorSwatch.Name = "cs_colorSwatch";
            this.cs_colorSwatch.Size = new System.Drawing.Size(171, 283);
            this.cs_colorSwatch.TabIndex = 4;
            this.cs_colorSwatch.ColorSelect += new Pixelaria.Views.Controls.ColorControls.ColorSwatchControl.ColorSelectEventHandler(this.cs_colorSwatch_ColorSelect);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 550);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(172, 3);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            // 
            // cp_mainColorPicker
            // 
            this.cp_mainColorPicker.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.cp_mainColorPicker.Dock = System.Windows.Forms.DockStyle.Top;
            this.cp_mainColorPicker.FirstColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cp_mainColorPicker.Location = new System.Drawing.Point(0, 0);
            this.cp_mainColorPicker.Name = "cp_mainColorPicker";
            this.cp_mainColorPicker.SecondColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cp_mainColorPicker.SelectedColor = Pixelaria.Views.Controls.ColorControls.ColorPickerColor.FirstColor;
            this.cp_mainColorPicker.Size = new System.Drawing.Size(172, 550);
            this.cp_mainColorPicker.TabIndex = 3;
            this.cp_mainColorPicker.ColorPick += new Pixelaria.Views.Controls.ColorControls.ColorPicker.ColorPickEventHandler(this.cp_mainColorPicker_ColorPick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.flowLayoutPanel3);
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 49);
            this.panel1.Name = "panel1";
            this.panel1.PanelTitle = "Toolbox";
            this.panel1.Size = new System.Drawing.Size(90, 629);
            this.panel1.TabIndex = 1;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.groupBox1);
            this.flowLayoutPanel3.Controls.Add(this.gb_sizeGroup);
            this.flowLayoutPanel3.Controls.Add(this.gb_fillMode);
            this.flowLayoutPanel3.Controls.Add(this.gb_otherGroup);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(0, 190);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(90, 582);
            this.flowLayoutPanel3.TabIndex = 7;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rb_blendingBlend);
            this.groupBox1.Controls.Add(this.rb_blendingReplace);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(84, 73);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Blending";
            // 
            // rb_blendingBlend
            // 
            this.rb_blendingBlend.AutoSize = true;
            this.rb_blendingBlend.Checked = true;
            this.rb_blendingBlend.Location = new System.Drawing.Point(3, 20);
            this.rb_blendingBlend.Name = "rb_blendingBlend";
            this.rb_blendingBlend.Size = new System.Drawing.Size(52, 17);
            this.rb_blendingBlend.TabIndex = 2;
            this.rb_blendingBlend.TabStop = true;
            this.rb_blendingBlend.Text = "Blend";
            this.rb_blendingBlend.UseVisualStyleBackColor = true;
            this.rb_blendingBlend.CheckedChanged += new System.EventHandler(this.rb_blendingBlend_CheckedChanged);
            // 
            // rb_blendingReplace
            // 
            this.rb_blendingReplace.AutoSize = true;
            this.rb_blendingReplace.Location = new System.Drawing.Point(3, 43);
            this.rb_blendingReplace.Name = "rb_blendingReplace";
            this.rb_blendingReplace.Size = new System.Drawing.Size(65, 17);
            this.rb_blendingReplace.TabIndex = 0;
            this.rb_blendingReplace.Text = "Replace";
            this.rb_blendingReplace.UseVisualStyleBackColor = true;
            this.rb_blendingReplace.CheckedChanged += new System.EventHandler(this.rb_blendingReplace_CheckedChanged);
            // 
            // gb_sizeGroup
            // 
            this.gb_sizeGroup.Controls.Add(this.flowLayoutPanel2);
            this.gb_sizeGroup.Controls.Add(this.anud_brushSize);
            this.gb_sizeGroup.Location = new System.Drawing.Point(3, 82);
            this.gb_sizeGroup.Name = "gb_sizeGroup";
            this.gb_sizeGroup.Size = new System.Drawing.Size(84, 112);
            this.gb_sizeGroup.TabIndex = 3;
            this.gb_sizeGroup.TabStop = false;
            this.gb_sizeGroup.Text = "Size";
            this.gb_sizeGroup.Visible = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.Controls.Add(this.btn_brushSize_1);
            this.flowLayoutPanel2.Controls.Add(this.btn_brushSize_2);
            this.flowLayoutPanel2.Controls.Add(this.btn_brushSize_3);
            this.flowLayoutPanel2.Controls.Add(this.btn_brushSize_4);
            this.flowLayoutPanel2.Controls.Add(this.btn_brushSize_5);
            this.flowLayoutPanel2.Controls.Add(this.btn_brushSize_6);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(2, 57);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(80, 52);
            this.flowLayoutPanel2.TabIndex = 7;
            // 
            // btn_brushSize_1
            // 
            this.btn_brushSize_1.Image = global::Pixelaria.Properties.Resources.brush_size_1;
            this.btn_brushSize_1.Location = new System.Drawing.Point(2, 2);
            this.btn_brushSize_1.Margin = new System.Windows.Forms.Padding(2);
            this.btn_brushSize_1.Name = "btn_brushSize_1";
            this.btn_brushSize_1.Size = new System.Drawing.Size(22, 22);
            this.btn_brushSize_1.TabIndex = 0;
            this.btn_brushSize_1.UseVisualStyleBackColor = true;
            this.btn_brushSize_1.Click += new System.EventHandler(this.btn_brushSize_1_Click);
            // 
            // btn_brushSize_2
            // 
            this.btn_brushSize_2.Image = global::Pixelaria.Properties.Resources.brush_size_2;
            this.btn_brushSize_2.Location = new System.Drawing.Point(28, 2);
            this.btn_brushSize_2.Margin = new System.Windows.Forms.Padding(2);
            this.btn_brushSize_2.Name = "btn_brushSize_2";
            this.btn_brushSize_2.Size = new System.Drawing.Size(22, 22);
            this.btn_brushSize_2.TabIndex = 1;
            this.btn_brushSize_2.UseVisualStyleBackColor = true;
            this.btn_brushSize_2.Click += new System.EventHandler(this.btn_brushSize_2_Click);
            // 
            // btn_brushSize_3
            // 
            this.btn_brushSize_3.Image = global::Pixelaria.Properties.Resources.brush_size_3;
            this.btn_brushSize_3.Location = new System.Drawing.Point(54, 2);
            this.btn_brushSize_3.Margin = new System.Windows.Forms.Padding(2);
            this.btn_brushSize_3.Name = "btn_brushSize_3";
            this.btn_brushSize_3.Size = new System.Drawing.Size(22, 22);
            this.btn_brushSize_3.TabIndex = 2;
            this.btn_brushSize_3.UseVisualStyleBackColor = true;
            this.btn_brushSize_3.Click += new System.EventHandler(this.btn_brushSize_3_Click);
            // 
            // btn_brushSize_4
            // 
            this.btn_brushSize_4.Image = global::Pixelaria.Properties.Resources.brush_size_4;
            this.btn_brushSize_4.Location = new System.Drawing.Point(2, 28);
            this.btn_brushSize_4.Margin = new System.Windows.Forms.Padding(2);
            this.btn_brushSize_4.Name = "btn_brushSize_4";
            this.btn_brushSize_4.Size = new System.Drawing.Size(22, 22);
            this.btn_brushSize_4.TabIndex = 3;
            this.btn_brushSize_4.UseVisualStyleBackColor = true;
            this.btn_brushSize_4.Click += new System.EventHandler(this.btn_brushSize_4_Click);
            // 
            // btn_brushSize_5
            // 
            this.btn_brushSize_5.Image = global::Pixelaria.Properties.Resources.brush_size_5;
            this.btn_brushSize_5.Location = new System.Drawing.Point(28, 28);
            this.btn_brushSize_5.Margin = new System.Windows.Forms.Padding(2);
            this.btn_brushSize_5.Name = "btn_brushSize_5";
            this.btn_brushSize_5.Size = new System.Drawing.Size(22, 22);
            this.btn_brushSize_5.TabIndex = 4;
            this.btn_brushSize_5.UseVisualStyleBackColor = true;
            this.btn_brushSize_5.Click += new System.EventHandler(this.btn_brushSize_5_Click);
            // 
            // btn_brushSize_6
            // 
            this.btn_brushSize_6.Image = global::Pixelaria.Properties.Resources.brush_size_6;
            this.btn_brushSize_6.Location = new System.Drawing.Point(54, 28);
            this.btn_brushSize_6.Margin = new System.Windows.Forms.Padding(2);
            this.btn_brushSize_6.Name = "btn_brushSize_6";
            this.btn_brushSize_6.Size = new System.Drawing.Size(22, 22);
            this.btn_brushSize_6.TabIndex = 5;
            this.btn_brushSize_6.UseVisualStyleBackColor = true;
            this.btn_brushSize_6.Click += new System.EventHandler(this.btn_brushSize_6_Click);
            // 
            // anud_brushSize
            // 
            this.anud_brushSize.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_brushSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_brushSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_brushSize.Location = new System.Drawing.Point(6, 19);
            this.anud_brushSize.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.anud_brushSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_brushSize.Name = "anud_brushSize";
            this.anud_brushSize.Size = new System.Drawing.Size(72, 32);
            this.anud_brushSize.TabIndex = 0;
            this.anud_brushSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_brushSize.ValueChanged += new System.EventHandler(this.anud_brushSize_ValueChanged);
            // 
            // gb_fillMode
            // 
            this.gb_fillMode.Controls.Add(this.rb_fillMode_3);
            this.gb_fillMode.Controls.Add(this.rb_fillMode_2);
            this.gb_fillMode.Controls.Add(this.rb_fillMode_1);
            this.gb_fillMode.Location = new System.Drawing.Point(3, 200);
            this.gb_fillMode.Name = "gb_fillMode";
            this.gb_fillMode.Size = new System.Drawing.Size(84, 107);
            this.gb_fillMode.TabIndex = 8;
            this.gb_fillMode.TabStop = false;
            this.gb_fillMode.Text = "Fill Mode";
            this.gb_fillMode.Visible = false;
            // 
            // rb_fillMode_3
            // 
            this.rb_fillMode_3.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_fillMode_3.Image = global::Pixelaria.Properties.Resources.fill_mode_3;
            this.rb_fillMode_3.Location = new System.Drawing.Point(4, 77);
            this.rb_fillMode_3.Name = "rb_fillMode_3";
            this.rb_fillMode_3.Size = new System.Drawing.Size(76, 23);
            this.rb_fillMode_3.TabIndex = 2;
            this.rb_fillMode_3.TabStop = true;
            this.rb_fillMode_3.UseVisualStyleBackColor = true;
            this.rb_fillMode_3.CheckedChanged += new System.EventHandler(this.rb_fillMode_3_CheckedChanged);
            // 
            // rb_fillMode_2
            // 
            this.rb_fillMode_2.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_fillMode_2.Image = global::Pixelaria.Properties.Resources.fill_mode_2;
            this.rb_fillMode_2.Location = new System.Drawing.Point(4, 48);
            this.rb_fillMode_2.Name = "rb_fillMode_2";
            this.rb_fillMode_2.Size = new System.Drawing.Size(76, 23);
            this.rb_fillMode_2.TabIndex = 1;
            this.rb_fillMode_2.TabStop = true;
            this.rb_fillMode_2.UseVisualStyleBackColor = true;
            this.rb_fillMode_2.CheckedChanged += new System.EventHandler(this.rb_fillMode_2_CheckedChanged);
            // 
            // rb_fillMode_1
            // 
            this.rb_fillMode_1.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_fillMode_1.Image = global::Pixelaria.Properties.Resources.fill_mode_1;
            this.rb_fillMode_1.Location = new System.Drawing.Point(4, 19);
            this.rb_fillMode_1.Name = "rb_fillMode_1";
            this.rb_fillMode_1.Size = new System.Drawing.Size(76, 23);
            this.rb_fillMode_1.TabIndex = 0;
            this.rb_fillMode_1.TabStop = true;
            this.rb_fillMode_1.UseVisualStyleBackColor = true;
            this.rb_fillMode_1.CheckedChanged += new System.EventHandler(this.rb_fillMode_1_CheckedChanged);
            // 
            // gb_otherGroup
            // 
            this.gb_otherGroup.Controls.Add(this.cb_airbrushMode);
            this.gb_otherGroup.Location = new System.Drawing.Point(3, 313);
            this.gb_otherGroup.Name = "gb_otherGroup";
            this.gb_otherGroup.Size = new System.Drawing.Size(84, 73);
            this.gb_otherGroup.TabIndex = 9;
            this.gb_otherGroup.TabStop = false;
            this.gb_otherGroup.Text = "Other";
            this.gb_otherGroup.Visible = false;
            // 
            // cb_airbrushMode
            // 
            this.cb_airbrushMode.AutoSize = true;
            this.cb_airbrushMode.Location = new System.Drawing.Point(6, 19);
            this.cb_airbrushMode.Name = "cb_airbrushMode";
            this.cb_airbrushMode.Size = new System.Drawing.Size(64, 17);
            this.cb_airbrushMode.TabIndex = 0;
            this.cb_airbrushMode.Text = "Airbrush";
            this.cb_airbrushMode.UseVisualStyleBackColor = true;
            this.cb_airbrushMode.CheckedChanged += new System.EventHandler(this.cb_enablePencilFlow_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flowLayoutPanel1.Controls.Add(this.rb_pencil);
            this.flowLayoutPanel1.Controls.Add(this.rb_eraser);
            this.flowLayoutPanel1.Controls.Add(this.rb_picker);
            this.flowLayoutPanel1.Controls.Add(this.rb_sprayPaint);
            this.flowLayoutPanel1.Controls.Add(this.rb_line);
            this.flowLayoutPanel1.Controls.Add(this.rb_rectangle);
            this.flowLayoutPanel1.Controls.Add(this.rb_circle);
            this.flowLayoutPanel1.Controls.Add(this.rb_bucket);
            this.flowLayoutPanel1.Controls.Add(this.rb_selection);
            this.flowLayoutPanel1.Controls.Add(this.rb_zoom);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 18);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(90, 166);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsb_applyChangesAndClose,
            this.tsb_applyChanges,
            this.toolStripSeparator1,
            this.tsb_prevFrame,
            this.tsb_nextFrame,
            this.tsb_insertNewframe,
            this.tsb_addFrameAtEnd,
            this.toolStripSeparator2,
            this.tsb_clearFrame,
            this.tsb_copy,
            this.tsb_cut,
            this.tsb_paste,
            this.toolStripSeparator4,
            this.tsb_undo,
            this.tsb_redo,
            this.toolStripSeparator3,
            this.tsb_grid,
            this.tsb_previewFrame,
            this.tsb_previewAnimation,
            this.toolStripSeparator5,
            this.tsb_onionSkin,
            this.tsb_osPrevFrames,
            this.tsb_osShowCurrentFrame,
            this.tsb_osNextFrames,
            this.tsb_osDisplayOnFront,
            this.tsl_onionSkinDepth,
            this.tscb_osFrameCount});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(818, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsb_applyChangesAndClose
            // 
            this.tsb_applyChangesAndClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_applyChangesAndClose.Image = global::Pixelaria.Properties.Resources.action_check;
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
            this.tsb_applyChanges.Text = "Apply Changes";
            this.tsb_applyChanges.Click += new System.EventHandler(this.tsb_applyChanges_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_prevFrame
            // 
            this.tsb_prevFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_prevFrame.Image = global::Pixelaria.Properties.Resources.frame_previous;
            this.tsb_prevFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_prevFrame.Name = "tsb_prevFrame";
            this.tsb_prevFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_prevFrame.Text = "Switch to previous frame";
            this.tsb_prevFrame.Click += new System.EventHandler(this.tsb_prevFrame_Click);
            // 
            // tsb_nextFrame
            // 
            this.tsb_nextFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_nextFrame.Image = global::Pixelaria.Properties.Resources.frame_next;
            this.tsb_nextFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_nextFrame.Name = "tsb_nextFrame";
            this.tsb_nextFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_nextFrame.Text = "Switch to next frame";
            this.tsb_nextFrame.Click += new System.EventHandler(this.tsb_nextFrame_Click);
            // 
            // tsb_insertNewframe
            // 
            this.tsb_insertNewframe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_insertNewframe.Image = global::Pixelaria.Properties.Resources.frame_insert_new_icon;
            this.tsb_insertNewframe.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_insertNewframe.Name = "tsb_insertNewframe";
            this.tsb_insertNewframe.Size = new System.Drawing.Size(23, 22);
            this.tsb_insertNewframe.Text = "Insert new frame after this one";
            this.tsb_insertNewframe.Click += new System.EventHandler(this.tsb_insertNewframe_Click);
            // 
            // tsb_addFrameAtEnd
            // 
            this.tsb_addFrameAtEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_addFrameAtEnd.Image = global::Pixelaria.Properties.Resources.frame_add_new_icon;
            this.tsb_addFrameAtEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_addFrameAtEnd.Name = "tsb_addFrameAtEnd";
            this.tsb_addFrameAtEnd.Size = new System.Drawing.Size(23, 22);
            this.tsb_addFrameAtEnd.Text = "Add new frame at the end of the animation and open for edit";
            this.tsb_addFrameAtEnd.Click += new System.EventHandler(this.tsb_addFrameAtEnd_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_clearFrame
            // 
            this.tsb_clearFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_clearFrame.Image = global::Pixelaria.Properties.Resources.document_new;
            this.tsb_clearFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_clearFrame.Name = "tsb_clearFrame";
            this.tsb_clearFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_clearFrame.Text = "Clear Frame";
            this.tsb_clearFrame.Click += new System.EventHandler(this.tsb_clearFrame_Click);
            // 
            // tsb_copy
            // 
            this.tsb_copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_copy.Image = global::Pixelaria.Properties.Resources.edit_copy;
            this.tsb_copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_copy.Name = "tsb_copy";
            this.tsb_copy.Size = new System.Drawing.Size(23, 22);
            this.tsb_copy.Text = "Copy";
            this.tsb_copy.Click += new System.EventHandler(this.tsb_copy_Click);
            // 
            // tsb_cut
            // 
            this.tsb_cut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_cut.Image = global::Pixelaria.Properties.Resources.edit_cut;
            this.tsb_cut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_cut.Name = "tsb_cut";
            this.tsb_cut.Size = new System.Drawing.Size(23, 22);
            this.tsb_cut.Text = "Cut";
            this.tsb_cut.Click += new System.EventHandler(this.tsb_cut_Click);
            // 
            // tsb_paste
            // 
            this.tsb_paste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_paste.Image = global::Pixelaria.Properties.Resources.edit_paste;
            this.tsb_paste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_paste.Name = "tsb_paste";
            this.tsb_paste.Size = new System.Drawing.Size(23, 22);
            this.tsb_paste.Text = "Paste";
            this.tsb_paste.Click += new System.EventHandler(this.tsb_paste_Click);
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
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_grid
            // 
            this.tsb_grid.CheckOnClick = true;
            this.tsb_grid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_grid.Image = global::Pixelaria.Properties.Resources.grid_icon;
            this.tsb_grid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_grid.Name = "tsb_grid";
            this.tsb_grid.Size = new System.Drawing.Size(23, 22);
            this.tsb_grid.Text = "Enable/disable grid";
            this.tsb_grid.Click += new System.EventHandler(this.tsb_grid_Click);
            // 
            // tsb_previewFrame
            // 
            this.tsb_previewFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_previewFrame.Image = global::Pixelaria.Properties.Resources.frame_preview_icon;
            this.tsb_previewFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_previewFrame.Name = "tsb_previewFrame";
            this.tsb_previewFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_previewFrame.Text = "Preview Frame";
            this.tsb_previewFrame.Click += new System.EventHandler(this.tsb_previewFrame_Click);
            // 
            // tsb_previewAnimation
            // 
            this.tsb_previewAnimation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_previewAnimation.Image = global::Pixelaria.Properties.Resources.anim_preview_icon;
            this.tsb_previewAnimation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_previewAnimation.Name = "tsb_previewAnimation";
            this.tsb_previewAnimation.Size = new System.Drawing.Size(23, 22);
            this.tsb_previewAnimation.Text = "Preview Animation";
            this.tsb_previewAnimation.Visible = false;
            this.tsb_previewAnimation.Click += new System.EventHandler(this.tsb_previewAnimation_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // tsb_onionSkin
            // 
            this.tsb_onionSkin.CheckOnClick = true;
            this.tsb_onionSkin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_onionSkin.Image = global::Pixelaria.Properties.Resources.frame_onionskin_icon;
            this.tsb_onionSkin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_onionSkin.Name = "tsb_onionSkin";
            this.tsb_onionSkin.Size = new System.Drawing.Size(23, 22);
            this.tsb_onionSkin.Text = "Enable/disable onion skin";
            this.tsb_onionSkin.Click += new System.EventHandler(this.tsb_onionSkin_Click);
            // 
            // tsb_osPrevFrames
            // 
            this.tsb_osPrevFrames.Checked = true;
            this.tsb_osPrevFrames.CheckOnClick = true;
            this.tsb_osPrevFrames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsb_osPrevFrames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_osPrevFrames.Image = global::Pixelaria.Properties.Resources.frame_os_prev;
            this.tsb_osPrevFrames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_osPrevFrames.Name = "tsb_osPrevFrames";
            this.tsb_osPrevFrames.Size = new System.Drawing.Size(23, 22);
            this.tsb_osPrevFrames.Text = "Show previous frames on onion skin";
            this.tsb_osPrevFrames.Visible = false;
            this.tsb_osPrevFrames.Click += new System.EventHandler(this.tsb_osPrevFrames_Click);
            // 
            // tsb_osShowCurrentFrame
            // 
            this.tsb_osShowCurrentFrame.Checked = true;
            this.tsb_osShowCurrentFrame.CheckOnClick = true;
            this.tsb_osShowCurrentFrame.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsb_osShowCurrentFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_osShowCurrentFrame.Image = global::Pixelaria.Properties.Resources.frame_os_current;
            this.tsb_osShowCurrentFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_osShowCurrentFrame.Name = "tsb_osShowCurrentFrame";
            this.tsb_osShowCurrentFrame.Size = new System.Drawing.Size(23, 22);
            this.tsb_osShowCurrentFrame.Text = "Show current frame";
            this.tsb_osShowCurrentFrame.Visible = false;
            this.tsb_osShowCurrentFrame.Click += new System.EventHandler(this.tsb_hideCurrentFrame_Click);
            // 
            // tsb_osNextFrames
            // 
            this.tsb_osNextFrames.Checked = true;
            this.tsb_osNextFrames.CheckOnClick = true;
            this.tsb_osNextFrames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsb_osNextFrames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_osNextFrames.Image = global::Pixelaria.Properties.Resources.frame_os_next;
            this.tsb_osNextFrames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_osNextFrames.Name = "tsb_osNextFrames";
            this.tsb_osNextFrames.Size = new System.Drawing.Size(23, 22);
            this.tsb_osNextFrames.Text = "Show next frames on onion skin";
            this.tsb_osNextFrames.Visible = false;
            this.tsb_osNextFrames.Click += new System.EventHandler(this.tsb_osNextFrames_Click);
            // 
            // tsb_osDisplayOnFront
            // 
            this.tsb_osDisplayOnFront.CheckOnClick = true;
            this.tsb_osDisplayOnFront.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_osDisplayOnFront.Image = global::Pixelaria.Properties.Resources.frame_os_order;
            this.tsb_osDisplayOnFront.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_osDisplayOnFront.Name = "tsb_osDisplayOnFront";
            this.tsb_osDisplayOnFront.Size = new System.Drawing.Size(23, 22);
            this.tsb_osDisplayOnFront.Text = "Display onion skin abore frame";
            this.tsb_osDisplayOnFront.Visible = false;
            this.tsb_osDisplayOnFront.Click += new System.EventHandler(this.tsb_osDisplayOnFront_Click);
            // 
            // tsl_onionSkinDepth
            // 
            this.tsl_onionSkinDepth.Name = "tsl_onionSkinDepth";
            this.tsl_onionSkinDepth.Size = new System.Drawing.Size(101, 22);
            this.tsl_onionSkinDepth.Text = "Onion skin depth:";
            this.tsl_onionSkinDepth.Visible = false;
            // 
            // tscb_osFrameCount
            // 
            this.tscb_osFrameCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tscb_osFrameCount.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.tscb_osFrameCount.Name = "tscb_osFrameCount";
            this.tscb_osFrameCount.Size = new System.Drawing.Size(121, 25);
            this.tscb_osFrameCount.Visible = false;
            this.tscb_osFrameCount.SelectedIndexChanged += new System.EventHandler(this.tscb_osFrameCount_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.layersToolStripMenuItem,
            this.tsm_filters});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(818, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_exportFrame,
            this.tsm_importFrame});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // tsm_exportFrame
            // 
            this.tsm_exportFrame.Image = global::Pixelaria.Properties.Resources.frame_save_icon;
            this.tsm_exportFrame.Name = "tsm_exportFrame";
            this.tsm_exportFrame.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.tsm_exportFrame.Size = new System.Drawing.Size(198, 22);
            this.tsm_exportFrame.Text = "Export Frame...";
            this.tsm_exportFrame.Click += new System.EventHandler(this.tsm_exportFrame_Click);
            // 
            // tsm_importFrame
            // 
            this.tsm_importFrame.Image = global::Pixelaria.Properties.Resources.frame_open_icon;
            this.tsm_importFrame.Name = "tsm_importFrame";
            this.tsm_importFrame.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.tsm_importFrame.Size = new System.Drawing.Size(198, 22);
            this.tsm_importFrame.Text = "Import Frame...";
            this.tsm_importFrame.Click += new System.EventHandler(this.tsm_importFrame_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_undo,
            this.tsm_redo,
            this.toolStripMenuItem2,
            this.tsm_copy,
            this.tsm_cut,
            this.tsm_paste,
            this.tsm_selectAll,
            this.toolStripMenuItem3,
            this.tsm_prevFrame,
            this.tsm_nextFrame,
            this.toolStripMenuItem1,
            this.tsm_switchBlendingMode});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // tsm_undo
            // 
            this.tsm_undo.Image = global::Pixelaria.Properties.Resources.edit_undo;
            this.tsm_undo.Name = "tsm_undo";
            this.tsm_undo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.tsm_undo.Size = new System.Drawing.Size(234, 22);
            this.tsm_undo.Text = "Undo";
            this.tsm_undo.Click += new System.EventHandler(this.tsm_undo_Click);
            // 
            // tsm_redo
            // 
            this.tsm_redo.Image = global::Pixelaria.Properties.Resources.edit_redo;
            this.tsm_redo.Name = "tsm_redo";
            this.tsm_redo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.tsm_redo.Size = new System.Drawing.Size(234, 22);
            this.tsm_redo.Text = "Redo";
            this.tsm_redo.Click += new System.EventHandler(this.tsm_redo_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(231, 6);
            // 
            // tsm_copy
            // 
            this.tsm_copy.Image = global::Pixelaria.Properties.Resources.edit_copy;
            this.tsm_copy.Name = "tsm_copy";
            this.tsm_copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.tsm_copy.Size = new System.Drawing.Size(234, 22);
            this.tsm_copy.Text = "Copy";
            this.tsm_copy.Click += new System.EventHandler(this.tsm_copy_Click);
            // 
            // tsm_cut
            // 
            this.tsm_cut.Image = global::Pixelaria.Properties.Resources.edit_cut;
            this.tsm_cut.Name = "tsm_cut";
            this.tsm_cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.tsm_cut.Size = new System.Drawing.Size(234, 22);
            this.tsm_cut.Text = "Cut";
            this.tsm_cut.Click += new System.EventHandler(this.tsm_cut_Click);
            // 
            // tsm_paste
            // 
            this.tsm_paste.Image = global::Pixelaria.Properties.Resources.edit_paste;
            this.tsm_paste.Name = "tsm_paste";
            this.tsm_paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.tsm_paste.Size = new System.Drawing.Size(234, 22);
            this.tsm_paste.Text = "Paste";
            this.tsm_paste.Click += new System.EventHandler(this.tsm_paste_Click);
            // 
            // tsm_selectAll
            // 
            this.tsm_selectAll.Image = global::Pixelaria.Properties.Resources.selection_icon;
            this.tsm_selectAll.Name = "tsm_selectAll";
            this.tsm_selectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.tsm_selectAll.Size = new System.Drawing.Size(234, 22);
            this.tsm_selectAll.Text = "Select All";
            this.tsm_selectAll.Click += new System.EventHandler(this.tsm_selectAll_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(231, 6);
            // 
            // tsm_prevFrame
            // 
            this.tsm_prevFrame.Image = global::Pixelaria.Properties.Resources.frame_previous;
            this.tsm_prevFrame.Name = "tsm_prevFrame";
            this.tsm_prevFrame.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.tsm_prevFrame.Size = new System.Drawing.Size(234, 22);
            this.tsm_prevFrame.Text = "Previous Frame";
            this.tsm_prevFrame.Click += new System.EventHandler(this.tsm_prevFrame_Click);
            // 
            // tsm_nextFrame
            // 
            this.tsm_nextFrame.Image = global::Pixelaria.Properties.Resources.frame_next;
            this.tsm_nextFrame.Name = "tsm_nextFrame";
            this.tsm_nextFrame.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.tsm_nextFrame.Size = new System.Drawing.Size(234, 22);
            this.tsm_nextFrame.Text = "Next Frame";
            this.tsm_nextFrame.Click += new System.EventHandler(this.tsm_nextFrame_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(231, 6);
            // 
            // tsm_switchBlendingMode
            // 
            this.tsm_switchBlendingMode.Name = "tsm_switchBlendingMode";
            this.tsm_switchBlendingMode.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.tsm_switchBlendingMode.Size = new System.Drawing.Size(234, 22);
            this.tsm_switchBlendingMode.Text = "Switch Blending Mode";
            this.tsm_switchBlendingMode.Click += new System.EventHandler(this.tsm_switchBlendingMode_Click);
            // 
            // layersToolStripMenuItem
            // 
            this.layersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_toggleVisibleLayers,
            this.tsm_resetLayerTransparencies,
            this.controlToolStripMenuItem});
            this.layersToolStripMenuItem.Name = "layersToolStripMenuItem";
            this.layersToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.layersToolStripMenuItem.Text = "Layers";
            // 
            // tsm_toggleVisibleLayers
            // 
            this.tsm_toggleVisibleLayers.Image = global::Pixelaria.Properties.Resources.filter_enable_icon;
            this.tsm_toggleVisibleLayers.Name = "tsm_toggleVisibleLayers";
            this.tsm_toggleVisibleLayers.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.tsm_toggleVisibleLayers.Size = new System.Drawing.Size(250, 22);
            this.tsm_toggleVisibleLayers.Text = "Hide/show other layers";
            this.tsm_toggleVisibleLayers.Click += new System.EventHandler(this.tsm_toggleVisibleLayers_Click);
            // 
            // tsm_resetLayerTransparencies
            // 
            this.tsm_resetLayerTransparencies.Image = global::Pixelaria.Properties.Resources.filter_transparency_icon;
            this.tsm_resetLayerTransparencies.Name = "tsm_resetLayerTransparencies";
            this.tsm_resetLayerTransparencies.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.tsm_resetLayerTransparencies.Size = new System.Drawing.Size(250, 22);
            this.tsm_resetLayerTransparencies.Text = "Reset layer transparencies";
            this.tsm_resetLayerTransparencies.Click += new System.EventHandler(this.tsm_resetLayerTransparencies_Click);
            // 
            // controlToolStripMenuItem
            // 
            this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_expandAllLayers,
            this.tsm_collapseAllLayers});
            this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            this.controlToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.controlToolStripMenuItem.Text = "Control";
            // 
            // tsm_expandAllLayers
            // 
            this.tsm_expandAllLayers.Name = "tsm_expandAllLayers";
            this.tsm_expandAllLayers.Size = new System.Drawing.Size(136, 22);
            this.tsm_expandAllLayers.Text = "Expand All";
            this.tsm_expandAllLayers.Click += new System.EventHandler(this.tsm_expandAllLayers_Click);
            // 
            // tsm_collapseAllLayers
            // 
            this.tsm_collapseAllLayers.Name = "tsm_collapseAllLayers";
            this.tsm_collapseAllLayers.Size = new System.Drawing.Size(136, 22);
            this.tsm_collapseAllLayers.Text = "Collapse All";
            this.tsm_collapseAllLayers.Click += new System.EventHandler(this.tsm_collapseAllLayers_Click);
            // 
            // tsm_filters
            // 
            this.tsm_filters.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_emptyFilter,
            this.tsm_filterPresets,
            this.tsm_lastUsedFilterPresets,
            this.toolStripMenuItem4});
            this.tsm_filters.Name = "tsm_filters";
            this.tsm_filters.Size = new System.Drawing.Size(50, 20);
            this.tsm_filters.Text = "Fi&lters";
            // 
            // tsm_emptyFilter
            // 
            this.tsm_emptyFilter.Image = global::Pixelaria.Properties.Resources.document_new;
            this.tsm_emptyFilter.Name = "tsm_emptyFilter";
            this.tsm_emptyFilter.Size = new System.Drawing.Size(124, 22);
            this.tsm_emptyFilter.Text = "Empty";
            this.tsm_emptyFilter.Click += new System.EventHandler(this.tsm_emptyFilter_Click);
            // 
            // tsm_filterPresets
            // 
            this.tsm_filterPresets.Image = global::Pixelaria.Properties.Resources.preset_icon;
            this.tsm_filterPresets.Name = "tsm_filterPresets";
            this.tsm_filterPresets.Size = new System.Drawing.Size(124, 22);
            this.tsm_filterPresets.Text = "Presets";
            // 
            // tsm_lastUsedFilterPresets
            // 
            this.tsm_lastUsedFilterPresets.Image = global::Pixelaria.Properties.Resources.preset_icon;
            this.tsm_lastUsedFilterPresets.Name = "tsm_lastUsedFilterPresets";
            this.tsm_lastUsedFilterPresets.Size = new System.Drawing.Size(124, 22);
            this.tsm_lastUsedFilterPresets.Text = "Last Used";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(121, 6);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsl_coordinates,
            this.tsl_operationLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 678);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(818, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsl_coordinates
            // 
            this.tsl_coordinates.Name = "tsl_coordinates";
            this.tsl_coordinates.Size = new System.Drawing.Size(30, 17);
            this.tsl_coordinates.Text = "3 x 4";
            // 
            // tsl_operationLabel
            // 
            this.tsl_operationLabel.Name = "tsl_operationLabel";
            this.tsl_operationLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // iepb_frame
            // 
            this.iepb_frame.CurrentPaintTool = nullPaintTool1;
            this.iepb_frame.DefaultCompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            this.iepb_frame.DefaultFillMode = Pixelaria.Views.Controls.OperationFillMode.SolidFillFirstColor;
            this.iepb_frame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iepb_frame.EditingEnabled = true;
            this.iepb_frame.Location = new System.Drawing.Point(239, 49);
            this.iepb_frame.Name = "iepb_frame";
            this.iepb_frame.NotifyTo = null;
            this.iepb_frame.PictureBoxBackgroundImage = ((System.Drawing.Image)(resources.GetObject("iepb_frame.PictureBoxBackgroundImage")));
            this.iepb_frame.Size = new System.Drawing.Size(407, 537);
            this.iepb_frame.TabIndex = 0;
            this.iepb_frame.Text = "imageEditPictureBox1";
            undoSystem1.MaximumTaskCount = 15;
            this.iepb_frame.UndoSystem = undoSystem1;
            this.iepb_frame.ColorSelect += new Pixelaria.Views.Controls.ImageEditPanel.ColorPickEventHandler(this.iepb_frame_ColorSelect);
            this.iepb_frame.ClipboardStateChanged += new Pixelaria.Views.Controls.ImageEditPanel.ClipboardStateEventHandler(this.iepb_frame_ClipboardStateChanged);
            this.iepb_frame.ClipboardSetContents += new System.EventHandler(this.iepb_frame_ClipboardSetContents);
            this.iepb_frame.OperationStatusChanged += new Pixelaria.Views.Controls.ImageEditPanel.OperationStatusEventHandler(this.iepb_frame_OperationStatusChanged);
            this.iepb_frame.MouseEnter += new System.EventHandler(this.iepb_frame_MouseEnter);
            this.iepb_frame.MouseLeave += new System.EventHandler(this.iepb_frame_MouseLeave);
            this.iepb_frame.MouseMove += new System.Windows.Forms.MouseEventHandler(this.iepb_frame_MouseMove);
            // 
            // lcp_layers
            // 
            this.lcp_layers.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lcp_layers.Dock = System.Windows.Forms.DockStyle.Left;
            this.lcp_layers.Location = new System.Drawing.Point(90, 49);
            this.lcp_layers.Name = "lcp_layers";
            this.lcp_layers.Size = new System.Drawing.Size(149, 537);
            this.lcp_layers.TabIndex = 9;
            // 
            // FrameView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 700);
            this.Controls.Add(this.pnl_framePreview);
            this.Controls.Add(this.iepb_frame);
            this.Controls.Add(this.lcp_layers);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(834, 738);
            this.Name = "FrameView";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Frame Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrameView_FormClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrameView_KeyDown);
            this.pnl_framePreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.zpb_framePreview)).EndInit();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb_zoomIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gb_sizeGroup.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.gb_fillMode.ResumeLayout(false);
            this.gb_otherGroup.ResumeLayout(false);
            this.gb_otherGroup.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
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

        private Pixelaria.Views.Controls.ImageEditPanel iepb_frame;
        private LabeledPanel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsb_applyChangesAndClose;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsb_prevFrame;
        private System.Windows.Forms.ToolStripButton tsb_nextFrame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.RadioButton rb_pencil;
        private System.Windows.Forms.RadioButton rb_eraser;
        private System.Windows.Forms.RadioButton rb_picker;
        private System.Windows.Forms.RadioButton rb_rectangle;
        private System.Windows.Forms.RadioButton rb_circle;
        private System.Windows.Forms.RadioButton rb_bucket;
        private System.Windows.Forms.RadioButton rb_selection;
        private System.Windows.Forms.ToolStripButton tsb_undo;
        private System.Windows.Forms.ToolStripButton tsb_redo;
        private System.Windows.Forms.RadioButton rb_zoom;
        private ColorPicker cp_mainColorPicker;
        private System.Windows.Forms.Panel panel2;
        private ColorSwatchControl cs_colorSwatch;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel4;
        private Controls.AssistedNumericUpDown anud_zoom;
        private System.Windows.Forms.PictureBox pb_zoomIcon;
        private System.Windows.Forms.ToolStripButton tsb_addFrameAtEnd;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsm_undo;
        private System.Windows.Forms.ToolStripMenuItem tsm_redo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb_blendingBlend;
        private System.Windows.Forms.RadioButton rb_blendingReplace;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tsm_switchBlendingMode;
        private System.Windows.Forms.GroupBox gb_sizeGroup;
        private Controls.AssistedNumericUpDown anud_brushSize;
        private System.Windows.Forms.ToolStripMenuItem tsm_exportFrame;
        private System.Windows.Forms.ToolStripMenuItem tsm_importFrame;
        private System.Windows.Forms.ToolStripButton tsb_applyChanges;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem tsm_prevFrame;
        private System.Windows.Forms.ToolStripMenuItem tsm_nextFrame;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btn_brushSize_1;
        private System.Windows.Forms.Button btn_brushSize_2;
        private System.Windows.Forms.Button btn_brushSize_3;
        private System.Windows.Forms.Button btn_brushSize_4;
        private System.Windows.Forms.Button btn_brushSize_5;
        private System.Windows.Forms.Button btn_brushSize_6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsb_grid;
        private System.Windows.Forms.RadioButton rb_line;
        private System.Windows.Forms.GroupBox gb_fillMode;
        private System.Windows.Forms.RadioButton rb_fillMode_1;
        private System.Windows.Forms.RadioButton rb_fillMode_2;
        private System.Windows.Forms.RadioButton rb_fillMode_3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.ToolStripButton tsb_copy;
        private System.Windows.Forms.ToolStripButton tsb_cut;
        private System.Windows.Forms.ToolStripButton tsb_paste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem tsm_copy;
        private System.Windows.Forms.ToolStripMenuItem tsm_cut;
        private System.Windows.Forms.ToolStripMenuItem tsm_paste;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolTip tt_mainTooltip;
        private LabeledPanel pnl_framePreview;
        private System.Windows.Forms.ToolStripButton tsb_previewFrame;
        private System.Windows.Forms.ToolStripButton tsb_previewAnimation;
        private Controls.ZoomablePictureBox zpb_framePreview;
        private System.Windows.Forms.ToolStripButton tsb_insertNewframe;
        private System.Windows.Forms.ToolStripButton tsb_clearFrame;
        private System.Windows.Forms.ToolStripMenuItem tsm_filters;
        private System.Windows.Forms.ToolStripMenuItem tsm_selectAll;
        private System.Windows.Forms.ToolStripButton tsb_onionSkin;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton tsb_osPrevFrames;
        private System.Windows.Forms.ToolStripButton tsb_osNextFrames;
        private System.Windows.Forms.ToolStripComboBox tscb_osFrameCount;
        private System.Windows.Forms.ToolStripLabel tsl_onionSkinDepth;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsl_operationLabel;
        private System.Windows.Forms.ToolStripStatusLabel tsl_coordinates;
        private System.Windows.Forms.ToolStripMenuItem tsm_emptyFilter;
        private System.Windows.Forms.ToolStripMenuItem tsm_filterPresets;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripButton tsb_osShowCurrentFrame;
        private System.Windows.Forms.RadioButton rb_sprayPaint;
        private Controls.TimelineControl tc_currentFrame;
        private System.Windows.Forms.ToolStripButton tsb_osDisplayOnFront;
        private System.Windows.Forms.GroupBox gb_otherGroup;
        private System.Windows.Forms.CheckBox cb_airbrushMode;
        private LayerControlPanel lcp_layers;
        private System.Windows.Forms.ToolStripMenuItem layersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsm_toggleVisibleLayers;
        private System.Windows.Forms.ToolStripMenuItem tsm_resetLayerTransparencies;
        private System.Windows.Forms.ToolStripMenuItem tsm_lastUsedFilterPresets;
        private System.Windows.Forms.ToolStripMenuItem controlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsm_expandAllLayers;
        private System.Windows.Forms.ToolStripMenuItem tsm_collapseAllLayers;
    }
}
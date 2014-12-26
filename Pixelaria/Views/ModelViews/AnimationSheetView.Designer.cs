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
    partial class AnimationSheetView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationSheetView));
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pb_exportProgress = new System.Windows.Forms.ProgressBar();
            this.nud_yPadding = new System.Windows.Forms.NumericUpDown();
            this.nud_xPadding = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cb_favorRatioOverArea = new System.Windows.Forms.CheckBox();
            this.cb_forcePowerOfTwoDimensions = new System.Windows.Forms.CheckBox();
            this.cb_forceMinimumDimensions = new System.Windows.Forms.CheckBox();
            this.cb_reuseIdenticalFrames = new System.Windows.Forms.CheckBox();
            this.cb_highPrecision = new System.Windows.Forms.CheckBox();
            this.cb_allowUordering = new System.Windows.Forms.CheckBox();
            this.cb_useUniformGrid = new System.Windows.Forms.CheckBox();
            this.cb_exportXml = new System.Windows.Forms.CheckBox();
            this.cb_padFramesOnXml = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_generatePreview = new System.Windows.Forms.Button();
            this.pnl_alertPanel = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lbl_alertLabel = new System.Windows.Forms.Label();
            this.pnl_errorPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_error = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txt_sheetName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_sheetPreview = new System.Windows.Forms.Label();
            this.zpb_sheetPreview = new Pixelaria.Views.Controls.SheetPreviewPictureBox();
            this.gb_sheetInfo = new System.Windows.Forms.GroupBox();
            this.lbl_frameCount = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbl_animCount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.gb_exportSummary = new System.Windows.Forms.GroupBox();
            this.lbl_memoryUsage = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lbl_reusedFrames = new System.Windows.Forms.Label();
            this.lbl_framesOnSheet = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbl_pixelCount = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbl_dimensions = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbl_zoomLevel = new System.Windows.Forms.Label();
            this.cb_showFrameBounds = new System.Windows.Forms.CheckBox();
            this.btn_apply = new System.Windows.Forms.Button();
            this.cb_showReuseCount = new System.Windows.Forms.CheckBox();
            this.tt_mainTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_yPadding)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_xPadding)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnl_alertPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.pnl_errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_sheetPreview)).BeginInit();
            this.gb_sheetInfo.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.gb_exportSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_ok
            // 
            resources.ApplyResources(this.btn_ok, "btn_ok");
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Name = "btn_ok";
            this.helpProvider1.SetShowHelp(this.btn_ok, ((bool)(resources.GetObject("btn_ok.ShowHelp"))));
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            resources.ApplyResources(this.btn_cancel, "btn_cancel");
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Name = "btn_cancel";
            this.helpProvider1.SetShowHelp(this.btn_cancel, ((bool)(resources.GetObject("btn_cancel.ShowHelp"))));
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.pb_exportProgress);
            this.groupBox2.Controls.Add(this.nud_yPadding);
            this.groupBox2.Controls.Add(this.nud_xPadding);
            this.groupBox2.Controls.Add(this.flowLayoutPanel1);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btn_generatePreview);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.helpProvider1.SetShowHelp(this.groupBox2, ((bool)(resources.GetObject("groupBox2.ShowHelp"))));
            this.groupBox2.TabStop = false;
            // 
            // pb_exportProgress
            // 
            resources.ApplyResources(this.pb_exportProgress, "pb_exportProgress");
            this.pb_exportProgress.Name = "pb_exportProgress";
            this.helpProvider1.SetShowHelp(this.pb_exportProgress, ((bool)(resources.GetObject("pb_exportProgress.ShowHelp"))));
            this.pb_exportProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // nud_yPadding
            // 
            resources.ApplyResources(this.nud_yPadding, "nud_yPadding");
            this.nud_yPadding.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nud_yPadding.Name = "nud_yPadding";
            this.helpProvider1.SetShowHelp(this.nud_yPadding, ((bool)(resources.GetObject("nud_yPadding.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.nud_yPadding, resources.GetString("nud_yPadding.ToolTip"));
            this.nud_yPadding.ValueChanged += new System.EventHandler(this.nuds_Common);
            // 
            // nud_xPadding
            // 
            resources.ApplyResources(this.nud_xPadding, "nud_xPadding");
            this.nud_xPadding.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nud_xPadding.Name = "nud_xPadding";
            this.helpProvider1.SetShowHelp(this.nud_xPadding, ((bool)(resources.GetObject("nud_xPadding.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.nud_xPadding, resources.GetString("nud_xPadding.ToolTip"));
            this.nud_xPadding.ValueChanged += new System.EventHandler(this.nuds_Common);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cb_favorRatioOverArea);
            this.flowLayoutPanel1.Controls.Add(this.cb_forcePowerOfTwoDimensions);
            this.flowLayoutPanel1.Controls.Add(this.cb_forceMinimumDimensions);
            this.flowLayoutPanel1.Controls.Add(this.cb_reuseIdenticalFrames);
            this.flowLayoutPanel1.Controls.Add(this.cb_highPrecision);
            this.flowLayoutPanel1.Controls.Add(this.cb_allowUordering);
            this.flowLayoutPanel1.Controls.Add(this.cb_useUniformGrid);
            this.flowLayoutPanel1.Controls.Add(this.cb_exportXml);
            this.flowLayoutPanel1.Controls.Add(this.cb_padFramesOnXml);
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.helpProvider1.SetShowHelp(this.flowLayoutPanel1, ((bool)(resources.GetObject("flowLayoutPanel1.ShowHelp"))));
            // 
            // cb_favorRatioOverArea
            // 
            resources.ApplyResources(this.cb_favorRatioOverArea, "cb_favorRatioOverArea");
            this.cb_favorRatioOverArea.Name = "cb_favorRatioOverArea";
            this.helpProvider1.SetShowHelp(this.cb_favorRatioOverArea, ((bool)(resources.GetObject("cb_favorRatioOverArea.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_favorRatioOverArea, resources.GetString("cb_favorRatioOverArea.ToolTip"));
            // 
            // cb_forcePowerOfTwoDimensions
            // 
            resources.ApplyResources(this.cb_forcePowerOfTwoDimensions, "cb_forcePowerOfTwoDimensions");
            this.flowLayoutPanel1.SetFlowBreak(this.cb_forcePowerOfTwoDimensions, true);
            this.cb_forcePowerOfTwoDimensions.Name = "cb_forcePowerOfTwoDimensions";
            this.helpProvider1.SetShowHelp(this.cb_forcePowerOfTwoDimensions, ((bool)(resources.GetObject("cb_forcePowerOfTwoDimensions.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_forcePowerOfTwoDimensions, resources.GetString("cb_forcePowerOfTwoDimensions.ToolTip"));
            this.cb_forcePowerOfTwoDimensions.UseVisualStyleBackColor = true;
            this.cb_forcePowerOfTwoDimensions.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // cb_forceMinimumDimensions
            // 
            resources.ApplyResources(this.cb_forceMinimumDimensions, "cb_forceMinimumDimensions");
            this.cb_forceMinimumDimensions.Checked = true;
            this.cb_forceMinimumDimensions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_forceMinimumDimensions, true);
            this.cb_forceMinimumDimensions.Name = "cb_forceMinimumDimensions";
            this.helpProvider1.SetShowHelp(this.cb_forceMinimumDimensions, ((bool)(resources.GetObject("cb_forceMinimumDimensions.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_forceMinimumDimensions, resources.GetString("cb_forceMinimumDimensions.ToolTip"));
            this.cb_forceMinimumDimensions.UseVisualStyleBackColor = true;
            this.cb_forceMinimumDimensions.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // cb_reuseIdenticalFrames
            // 
            resources.ApplyResources(this.cb_reuseIdenticalFrames, "cb_reuseIdenticalFrames");
            this.cb_reuseIdenticalFrames.Checked = true;
            this.cb_reuseIdenticalFrames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_reuseIdenticalFrames, true);
            this.cb_reuseIdenticalFrames.Name = "cb_reuseIdenticalFrames";
            this.helpProvider1.SetShowHelp(this.cb_reuseIdenticalFrames, ((bool)(resources.GetObject("cb_reuseIdenticalFrames.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_reuseIdenticalFrames, resources.GetString("cb_reuseIdenticalFrames.ToolTip"));
            this.cb_reuseIdenticalFrames.UseVisualStyleBackColor = true;
            this.cb_reuseIdenticalFrames.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // cb_highPrecision
            // 
            resources.ApplyResources(this.cb_highPrecision, "cb_highPrecision");
            this.cb_highPrecision.Name = "cb_highPrecision";
            this.helpProvider1.SetShowHelp(this.cb_highPrecision, ((bool)(resources.GetObject("cb_highPrecision.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_highPrecision, resources.GetString("cb_highPrecision.ToolTip"));
            this.cb_highPrecision.UseVisualStyleBackColor = true;
            this.cb_highPrecision.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // cb_allowUordering
            // 
            resources.ApplyResources(this.cb_allowUordering, "cb_allowUordering");
            this.cb_allowUordering.Checked = true;
            this.cb_allowUordering.CheckState = System.Windows.Forms.CheckState.Checked;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_allowUordering, true);
            this.cb_allowUordering.Name = "cb_allowUordering";
            this.helpProvider1.SetShowHelp(this.cb_allowUordering, ((bool)(resources.GetObject("cb_allowUordering.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_allowUordering, resources.GetString("cb_allowUordering.ToolTip"));
            this.cb_allowUordering.UseVisualStyleBackColor = true;
            this.cb_allowUordering.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // cb_useUniformGrid
            // 
            resources.ApplyResources(this.cb_useUniformGrid, "cb_useUniformGrid");
            this.flowLayoutPanel1.SetFlowBreak(this.cb_useUniformGrid, true);
            this.cb_useUniformGrid.Name = "cb_useUniformGrid";
            this.helpProvider1.SetShowHelp(this.cb_useUniformGrid, ((bool)(resources.GetObject("cb_useUniformGrid.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_useUniformGrid, resources.GetString("cb_useUniformGrid.ToolTip"));
            this.cb_useUniformGrid.UseVisualStyleBackColor = true;
            this.cb_useUniformGrid.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // cb_exportXml
            // 
            resources.ApplyResources(this.cb_exportXml, "cb_exportXml");
            this.cb_exportXml.Checked = true;
            this.cb_exportXml.CheckState = System.Windows.Forms.CheckState.Checked;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_exportXml, true);
            this.cb_exportXml.Name = "cb_exportXml";
            this.helpProvider1.SetShowHelp(this.cb_exportXml, ((bool)(resources.GetObject("cb_exportXml.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_exportXml, resources.GetString("cb_exportXml.ToolTip"));
            this.cb_exportXml.UseVisualStyleBackColor = true;
            this.cb_exportXml.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // cb_padFramesOnXml
            // 
            resources.ApplyResources(this.cb_padFramesOnXml, "cb_padFramesOnXml");
            this.flowLayoutPanel1.SetFlowBreak(this.cb_padFramesOnXml, true);
            this.cb_padFramesOnXml.Name = "cb_padFramesOnXml";
            this.helpProvider1.SetShowHelp(this.cb_padFramesOnXml, ((bool)(resources.GetObject("cb_padFramesOnXml.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.cb_padFramesOnXml, resources.GetString("cb_padFramesOnXml.ToolTip"));
            this.cb_padFramesOnXml.UseVisualStyleBackColor = true;
            this.cb_padFramesOnXml.CheckedChanged += new System.EventHandler(this.checkboxes_Change);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            this.helpProvider1.SetShowHelp(this.label4, ((bool)(resources.GetObject("label4.ShowHelp"))));
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.helpProvider1.SetShowHelp(this.label3, ((bool)(resources.GetObject("label3.ShowHelp"))));
            // 
            // btn_generatePreview
            // 
            this.btn_generatePreview.Image = global::Pixelaria.Properties.Resources.go_next;
            resources.ApplyResources(this.btn_generatePreview, "btn_generatePreview");
            this.btn_generatePreview.Name = "btn_generatePreview";
            this.helpProvider1.SetShowHelp(this.btn_generatePreview, ((bool)(resources.GetObject("btn_generatePreview.ShowHelp"))));
            this.btn_generatePreview.UseVisualStyleBackColor = true;
            this.btn_generatePreview.Click += new System.EventHandler(this.btn_generatePreview_Click);
            // 
            // pnl_alertPanel
            // 
            resources.ApplyResources(this.pnl_alertPanel, "pnl_alertPanel");
            this.pnl_alertPanel.Controls.Add(this.pictureBox2);
            this.pnl_alertPanel.Controls.Add(this.lbl_alertLabel);
            this.pnl_alertPanel.Name = "pnl_alertPanel";
            this.helpProvider1.SetShowHelp(this.pnl_alertPanel, ((bool)(resources.GetObject("pnl_alertPanel.ShowHelp"))));
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.helpProvider1.SetShowHelp(this.pictureBox2, ((bool)(resources.GetObject("pictureBox2.ShowHelp"))));
            this.pictureBox2.TabStop = false;
            // 
            // lbl_alertLabel
            // 
            resources.ApplyResources(this.lbl_alertLabel, "lbl_alertLabel");
            this.lbl_alertLabel.Name = "lbl_alertLabel";
            this.helpProvider1.SetShowHelp(this.lbl_alertLabel, ((bool)(resources.GetObject("lbl_alertLabel.ShowHelp"))));
            // 
            // pnl_errorPanel
            // 
            resources.ApplyResources(this.pnl_errorPanel, "pnl_errorPanel");
            this.pnl_errorPanel.Controls.Add(this.pictureBox1);
            this.pnl_errorPanel.Controls.Add(this.lbl_error);
            this.pnl_errorPanel.Name = "pnl_errorPanel";
            this.helpProvider1.SetShowHelp(this.pnl_errorPanel, ((bool)(resources.GetObject("pnl_errorPanel.ShowHelp"))));
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.helpProvider1.SetShowHelp(this.pictureBox1, ((bool)(resources.GetObject("pictureBox1.ShowHelp"))));
            this.pictureBox1.TabStop = false;
            // 
            // lbl_error
            // 
            resources.ApplyResources(this.lbl_error, "lbl_error");
            this.lbl_error.Name = "lbl_error";
            this.helpProvider1.SetShowHelp(this.lbl_error, ((bool)(resources.GetObject("lbl_error.ShowHelp"))));
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.txt_sheetName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Name = "groupBox1";
            this.helpProvider1.SetShowHelp(this.groupBox1, ((bool)(resources.GetObject("groupBox1.ShowHelp"))));
            this.groupBox1.TabStop = false;
            // 
            // txt_sheetName
            // 
            resources.ApplyResources(this.txt_sheetName, "txt_sheetName");
            this.txt_sheetName.Name = "txt_sheetName";
            this.helpProvider1.SetShowHelp(this.txt_sheetName, ((bool)(resources.GetObject("txt_sheetName.ShowHelp"))));
            this.txt_sheetName.TextChanged += new System.EventHandler(this.txt_sheetName_TextChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.helpProvider1.SetShowHelp(this.label1, ((bool)(resources.GetObject("label1.ShowHelp"))));
            // 
            // lbl_sheetPreview
            // 
            resources.ApplyResources(this.lbl_sheetPreview, "lbl_sheetPreview");
            this.lbl_sheetPreview.Name = "lbl_sheetPreview";
            this.helpProvider1.SetShowHelp(this.lbl_sheetPreview, ((bool)(resources.GetObject("lbl_sheetPreview.ShowHelp"))));
            // 
            // zpb_sheetPreview
            // 
            this.zpb_sheetPreview.AllowScrollbars = false;
            resources.ApplyResources(this.zpb_sheetPreview, "zpb_sheetPreview");
            this.zpb_sheetPreview.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_sheetPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.zpb_sheetPreview.ClipBackgroundToImage = true;
            this.zpb_sheetPreview.Importer = null;
            this.zpb_sheetPreview.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_sheetPreview.MaximumZoom")));
            this.zpb_sheetPreview.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_sheetPreview.MinimumZoom")));
            this.zpb_sheetPreview.Name = "zpb_sheetPreview";
            this.zpb_sheetPreview.SheetExport = null;
            this.helpProvider1.SetShowHelp(this.zpb_sheetPreview, ((bool)(resources.GetObject("zpb_sheetPreview.ShowHelp"))));
            this.zpb_sheetPreview.ShowImageArea = true;
            this.zpb_sheetPreview.TabStop = false;
            this.zpb_sheetPreview.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_sheetPreview.Zoom")));
            this.zpb_sheetPreview.ZoomFactor = 1.414214F;
            this.zpb_sheetPreview.ZoomChanged += new Pixelaria.Views.Controls.ZoomablePictureBox.ZoomChangedEventHandler(this.zpb_sheetPreview_ZoomChanged);
            // 
            // gb_sheetInfo
            // 
            this.gb_sheetInfo.Controls.Add(this.lbl_frameCount);
            this.gb_sheetInfo.Controls.Add(this.label5);
            this.gb_sheetInfo.Controls.Add(this.lbl_animCount);
            this.gb_sheetInfo.Controls.Add(this.label2);
            resources.ApplyResources(this.gb_sheetInfo, "gb_sheetInfo");
            this.gb_sheetInfo.Name = "gb_sheetInfo";
            this.helpProvider1.SetShowHelp(this.gb_sheetInfo, ((bool)(resources.GetObject("gb_sheetInfo.ShowHelp"))));
            this.gb_sheetInfo.TabStop = false;
            // 
            // lbl_frameCount
            // 
            resources.ApplyResources(this.lbl_frameCount, "lbl_frameCount");
            this.lbl_frameCount.Name = "lbl_frameCount";
            this.helpProvider1.SetShowHelp(this.lbl_frameCount, ((bool)(resources.GetObject("lbl_frameCount.ShowHelp"))));
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            this.helpProvider1.SetShowHelp(this.label5, ((bool)(resources.GetObject("label5.ShowHelp"))));
            // 
            // lbl_animCount
            // 
            resources.ApplyResources(this.lbl_animCount, "lbl_animCount");
            this.lbl_animCount.Name = "lbl_animCount";
            this.helpProvider1.SetShowHelp(this.lbl_animCount, ((bool)(resources.GetObject("lbl_animCount.ShowHelp"))));
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.helpProvider1.SetShowHelp(this.label2, ((bool)(resources.GetObject("label2.ShowHelp"))));
            // 
            // flowLayoutPanel2
            // 
            resources.ApplyResources(this.flowLayoutPanel2, "flowLayoutPanel2");
            this.flowLayoutPanel2.Controls.Add(this.gb_sheetInfo);
            this.flowLayoutPanel2.Controls.Add(this.groupBox2);
            this.flowLayoutPanel2.Controls.Add(this.gb_exportSummary);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.helpProvider1.SetShowHelp(this.flowLayoutPanel2, ((bool)(resources.GetObject("flowLayoutPanel2.ShowHelp"))));
            // 
            // gb_exportSummary
            // 
            this.gb_exportSummary.Controls.Add(this.lbl_memoryUsage);
            this.gb_exportSummary.Controls.Add(this.label11);
            this.gb_exportSummary.Controls.Add(this.lbl_reusedFrames);
            this.gb_exportSummary.Controls.Add(this.lbl_framesOnSheet);
            this.gb_exportSummary.Controls.Add(this.label9);
            this.gb_exportSummary.Controls.Add(this.label8);
            this.gb_exportSummary.Controls.Add(this.lbl_pixelCount);
            this.gb_exportSummary.Controls.Add(this.label7);
            this.gb_exportSummary.Controls.Add(this.lbl_dimensions);
            this.gb_exportSummary.Controls.Add(this.label6);
            resources.ApplyResources(this.gb_exportSummary, "gb_exportSummary");
            this.gb_exportSummary.Name = "gb_exportSummary";
            this.helpProvider1.SetShowHelp(this.gb_exportSummary, ((bool)(resources.GetObject("gb_exportSummary.ShowHelp"))));
            this.gb_exportSummary.TabStop = false;
            // 
            // lbl_memoryUsage
            // 
            resources.ApplyResources(this.lbl_memoryUsage, "lbl_memoryUsage");
            this.lbl_memoryUsage.Name = "lbl_memoryUsage";
            this.helpProvider1.SetShowHelp(this.lbl_memoryUsage, ((bool)(resources.GetObject("lbl_memoryUsage.ShowHelp"))));
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            this.helpProvider1.SetShowHelp(this.label11, ((bool)(resources.GetObject("label11.ShowHelp"))));
            // 
            // lbl_reusedFrames
            // 
            resources.ApplyResources(this.lbl_reusedFrames, "lbl_reusedFrames");
            this.lbl_reusedFrames.Name = "lbl_reusedFrames";
            this.helpProvider1.SetShowHelp(this.lbl_reusedFrames, ((bool)(resources.GetObject("lbl_reusedFrames.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.lbl_reusedFrames, resources.GetString("lbl_reusedFrames.ToolTip"));
            // 
            // lbl_framesOnSheet
            // 
            resources.ApplyResources(this.lbl_framesOnSheet, "lbl_framesOnSheet");
            this.lbl_framesOnSheet.Name = "lbl_framesOnSheet";
            this.helpProvider1.SetShowHelp(this.lbl_framesOnSheet, ((bool)(resources.GetObject("lbl_framesOnSheet.ShowHelp"))));
            this.tt_mainTooltip.SetToolTip(this.lbl_framesOnSheet, resources.GetString("lbl_framesOnSheet.ToolTip"));
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            this.helpProvider1.SetShowHelp(this.label9, ((bool)(resources.GetObject("label9.ShowHelp"))));
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            this.helpProvider1.SetShowHelp(this.label8, ((bool)(resources.GetObject("label8.ShowHelp"))));
            // 
            // lbl_pixelCount
            // 
            resources.ApplyResources(this.lbl_pixelCount, "lbl_pixelCount");
            this.lbl_pixelCount.Name = "lbl_pixelCount";
            this.helpProvider1.SetShowHelp(this.lbl_pixelCount, ((bool)(resources.GetObject("lbl_pixelCount.ShowHelp"))));
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            this.helpProvider1.SetShowHelp(this.label7, ((bool)(resources.GetObject("label7.ShowHelp"))));
            // 
            // lbl_dimensions
            // 
            resources.ApplyResources(this.lbl_dimensions, "lbl_dimensions");
            this.lbl_dimensions.Name = "lbl_dimensions";
            this.helpProvider1.SetShowHelp(this.lbl_dimensions, ((bool)(resources.GetObject("lbl_dimensions.ShowHelp"))));
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            this.helpProvider1.SetShowHelp(this.label6, ((bool)(resources.GetObject("label6.ShowHelp"))));
            // 
            // lbl_zoomLevel
            // 
            resources.ApplyResources(this.lbl_zoomLevel, "lbl_zoomLevel");
            this.lbl_zoomLevel.Name = "lbl_zoomLevel";
            this.helpProvider1.SetShowHelp(this.lbl_zoomLevel, ((bool)(resources.GetObject("lbl_zoomLevel.ShowHelp"))));
            // 
            // cb_showFrameBounds
            // 
            resources.ApplyResources(this.cb_showFrameBounds, "cb_showFrameBounds");
            this.cb_showFrameBounds.Name = "cb_showFrameBounds";
            this.helpProvider1.SetShowHelp(this.cb_showFrameBounds, ((bool)(resources.GetObject("cb_showFrameBounds.ShowHelp"))));
            this.cb_showFrameBounds.UseVisualStyleBackColor = true;
            this.cb_showFrameBounds.CheckedChanged += new System.EventHandler(this.cb_showFrameBounds_CheckedChanged);
            // 
            // btn_apply
            // 
            resources.ApplyResources(this.btn_apply, "btn_apply");
            this.btn_apply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_apply.Image = global::Pixelaria.Properties.Resources.download;
            this.btn_apply.Name = "btn_apply";
            this.helpProvider1.SetShowHelp(this.btn_apply, ((bool)(resources.GetObject("btn_apply.ShowHelp"))));
            this.btn_apply.UseVisualStyleBackColor = true;
            this.btn_apply.Click += new System.EventHandler(this.btn_apply_Click);
            // 
            // cb_showReuseCount
            // 
            resources.ApplyResources(this.cb_showReuseCount, "cb_showReuseCount");
            this.cb_showReuseCount.Name = "cb_showReuseCount";
            this.helpProvider1.SetShowHelp(this.cb_showReuseCount, ((bool)(resources.GetObject("cb_showReuseCount.ShowHelp"))));
            this.cb_showReuseCount.UseVisualStyleBackColor = true;
            this.cb_showReuseCount.CheckedChanged += new System.EventHandler(this.cb_showReuseCount_CheckedChanged);
            // 
            // tt_mainTooltip
            // 
            this.tt_mainTooltip.AutoPopDelay = 30000;
            this.tt_mainTooltip.InitialDelay = 500;
            this.tt_mainTooltip.ReshowDelay = 100;
            // 
            // AnimationSheetView
            // 
            this.AcceptButton = this.btn_ok;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.Controls.Add(this.cb_showReuseCount);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.cb_showFrameBounds);
            this.Controls.Add(this.lbl_zoomLevel);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.pnl_alertPanel);
            this.Controls.Add(this.pnl_errorPanel);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lbl_sheetPreview);
            this.Controls.Add(this.zpb_sheetPreview);
            this.HelpButton = true;
            this.Name = "AnimationSheetView";
            this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AnimationSheetView_FormClosed);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_yPadding)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_xPadding)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.pnl_alertPanel.ResumeLayout(false);
            this.pnl_alertPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.pnl_errorPanel.ResumeLayout(false);
            this.pnl_errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_sheetPreview)).EndInit();
            this.gb_sheetInfo.ResumeLayout(false);
            this.gb_sheetInfo.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.gb_exportSummary.ResumeLayout(false);
            this.gb_exportSummary.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel pnl_alertPanel;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lbl_alertLabel;
        private System.Windows.Forms.Panel pnl_errorPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbl_error;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.TextBox txt_sheetName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private Pixelaria.Views.Controls.SheetPreviewPictureBox zpb_sheetPreview;
        private System.Windows.Forms.Label lbl_sheetPreview;
        private System.Windows.Forms.Button btn_generatePreview;
        private System.Windows.Forms.CheckBox cb_forcePowerOfTwoDimensions;
        private System.Windows.Forms.CheckBox cb_favorRatioOverArea;
        private System.Windows.Forms.CheckBox cb_allowUordering;
        private System.Windows.Forms.CheckBox cb_reuseIdenticalFrames;
        private System.Windows.Forms.CheckBox cb_forceMinimumDimensions;
        private System.Windows.Forms.CheckBox cb_padFramesOnXml;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.NumericUpDown nud_yPadding;
        private System.Windows.Forms.NumericUpDown nud_xPadding;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.CheckBox cb_highPrecision;
        private System.Windows.Forms.CheckBox cb_exportXml;
        private System.Windows.Forms.GroupBox gb_sheetInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_animCount;
        private System.Windows.Forms.Label lbl_frameCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.GroupBox gb_exportSummary;
        private System.Windows.Forms.Label lbl_dimensions;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbl_pixelCount;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lbl_framesOnSheet;
        private System.Windows.Forms.Label lbl_reusedFrames;
        private System.Windows.Forms.ProgressBar pb_exportProgress;
        private System.Windows.Forms.Label lbl_zoomLevel;
        private System.Windows.Forms.ToolTip tt_mainTooltip;
        private System.Windows.Forms.CheckBox cb_useUniformGrid;
        private System.Windows.Forms.Label lbl_memoryUsage;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox cb_showFrameBounds;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.CheckBox cb_showReuseCount;
    }
}
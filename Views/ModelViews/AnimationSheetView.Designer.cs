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
            this.cb_favorRatioOverarea = new System.Windows.Forms.CheckBox();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pb_exportProgress = new System.Windows.Forms.ProgressBar();
            this.nud_yPadding = new System.Windows.Forms.NumericUpDown();
            this.nud_xPadding = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cb_forcePowerOfTwoDimensions = new System.Windows.Forms.CheckBox();
            this.cb_forceMinimumDimensions = new System.Windows.Forms.CheckBox();
            this.cb_reuseIdenticalFrames = new System.Windows.Forms.CheckBox();
            this.cb_highPrecision = new System.Windows.Forms.CheckBox();
            this.cb_allowUordering = new System.Windows.Forms.CheckBox();
            this.cb_padFramesOnXml = new System.Windows.Forms.CheckBox();
            this.cb_exportXml = new System.Windows.Forms.CheckBox();
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
            this.zpb_sheetPreview = new Pixelaria.Views.Controls.ZoomablePictureBox();
            this.gb_sheetInfo = new System.Windows.Forms.GroupBox();
            this.lbl_frameCount = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbl_animCount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.gb_exportSummary = new System.Windows.Forms.GroupBox();
            this.lbl_reusedFrames = new System.Windows.Forms.Label();
            this.lbl_framesOnSheet = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbl_pixelCount = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbl_dimensions = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbl_zoomLevel = new System.Windows.Forms.Label();
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
            // cb_favorRatioOverarea
            // 
            this.cb_favorRatioOverarea.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_favorRatioOverarea, true);
            this.cb_favorRatioOverarea.Location = new System.Drawing.Point(3, 3);
            this.cb_favorRatioOverarea.Name = "cb_favorRatioOverarea";
            this.helpProvider1.SetShowHelp(this.cb_favorRatioOverarea, true);
            this.cb_favorRatioOverarea.Size = new System.Drawing.Size(124, 17);
            this.cb_favorRatioOverarea.TabIndex = 3;
            this.cb_favorRatioOverarea.Text = "Favor ratio over area";
            this.tt_mainTooltip.SetToolTip(this.cb_favorRatioOverarea, "Whether to favor ratio over minimum area.\r\nChecking this will produce a more squa" +
        "re-ish image.\r\nThe output is also produced faster, but may be\r\nslightly bigger i" +
        "n pixel count.");
            this.cb_favorRatioOverarea.UseVisualStyleBackColor = true;
            this.cb_favorRatioOverarea.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Enabled = false;
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(446, 493);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 29);
            this.btn_ok.TabIndex = 24;
            this.btn_ok.Text = "&OK";
            this.btn_ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Location = new System.Drawing.Point(527, 493);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 29);
            this.btn_cancel.TabIndex = 23;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
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
            this.groupBox2.Location = new System.Drawing.Point(3, 60);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(293, 276);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Export Settings";
            // 
            // pb_exportProgress
            // 
            this.pb_exportProgress.Location = new System.Drawing.Point(210, 132);
            this.pb_exportProgress.Name = "pb_exportProgress";
            this.pb_exportProgress.Size = new System.Drawing.Size(79, 17);
            this.pb_exportProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pb_exportProgress.TabIndex = 14;
            this.pb_exportProgress.Visible = false;
            // 
            // nud_yPadding
            // 
            this.nud_yPadding.Location = new System.Drawing.Point(74, 241);
            this.nud_yPadding.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nud_yPadding.Name = "nud_yPadding";
            this.nud_yPadding.Size = new System.Drawing.Size(87, 20);
            this.nud_yPadding.TabIndex = 13;
            this.tt_mainTooltip.SetToolTip(this.nud_yPadding, "The horizontal spacing to apply between each frame on\r\nthe sheet texture. The spa" +
        "cing will also be applied around\r\nthe texture edges.");
            this.nud_yPadding.ValueChanged += new System.EventHandler(this.nudsCommon);
            // 
            // nud_xPadding
            // 
            this.nud_xPadding.Location = new System.Drawing.Point(74, 215);
            this.nud_xPadding.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nud_xPadding.Name = "nud_xPadding";
            this.nud_xPadding.Size = new System.Drawing.Size(87, 20);
            this.nud_xPadding.TabIndex = 12;
            this.tt_mainTooltip.SetToolTip(this.nud_xPadding, "The horizontal spacing to apply between each frame on\r\nthe sheet texture. The spa" +
        "cing will also be applied around\r\nthe texture edges.");
            this.nud_xPadding.ValueChanged += new System.EventHandler(this.nudsCommon);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cb_favorRatioOverarea);
            this.flowLayoutPanel1.Controls.Add(this.cb_forcePowerOfTwoDimensions);
            this.flowLayoutPanel1.Controls.Add(this.cb_forceMinimumDimensions);
            this.flowLayoutPanel1.Controls.Add(this.cb_reuseIdenticalFrames);
            this.flowLayoutPanel1.Controls.Add(this.cb_highPrecision);
            this.flowLayoutPanel1.Controls.Add(this.cb_allowUordering);
            this.flowLayoutPanel1.Controls.Add(this.cb_padFramesOnXml);
            this.flowLayoutPanel1.Controls.Add(this.cb_exportXml);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(195, 188);
            this.flowLayoutPanel1.TabIndex = 11;
            // 
            // cb_forcePowerOfTwoDimensions
            // 
            this.cb_forcePowerOfTwoDimensions.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_forcePowerOfTwoDimensions, true);
            this.cb_forcePowerOfTwoDimensions.Location = new System.Drawing.Point(3, 26);
            this.cb_forcePowerOfTwoDimensions.Name = "cb_forcePowerOfTwoDimensions";
            this.cb_forcePowerOfTwoDimensions.Size = new System.Drawing.Size(172, 17);
            this.cb_forcePowerOfTwoDimensions.TabIndex = 4;
            this.cb_forcePowerOfTwoDimensions.Text = "Force power of two dimensions";
            this.tt_mainTooltip.SetToolTip(this.cb_forcePowerOfTwoDimensions, resources.GetString("cb_forcePowerOfTwoDimensions.ToolTip"));
            this.cb_forcePowerOfTwoDimensions.UseVisualStyleBackColor = true;
            this.cb_forcePowerOfTwoDimensions.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // cb_forceMinimumDimensions
            // 
            this.cb_forceMinimumDimensions.AutoSize = true;
            this.cb_forceMinimumDimensions.Checked = true;
            this.cb_forceMinimumDimensions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_forceMinimumDimensions, true);
            this.cb_forceMinimumDimensions.Location = new System.Drawing.Point(3, 49);
            this.cb_forceMinimumDimensions.Name = "cb_forceMinimumDimensions";
            this.cb_forceMinimumDimensions.Size = new System.Drawing.Size(151, 17);
            this.cb_forceMinimumDimensions.TabIndex = 5;
            this.cb_forceMinimumDimensions.Text = "Force minimum dimensions";
            this.tt_mainTooltip.SetToolTip(this.cb_forceMinimumDimensions, "Checking this option will pack textures tightier by removing\r\nthe transparent edg" +
        "es around the frames.\r\n");
            this.cb_forceMinimumDimensions.UseVisualStyleBackColor = true;
            this.cb_forceMinimumDimensions.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // cb_reuseIdenticalFrames
            // 
            this.cb_reuseIdenticalFrames.AutoSize = true;
            this.cb_reuseIdenticalFrames.Checked = true;
            this.cb_reuseIdenticalFrames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_reuseIdenticalFrames, true);
            this.cb_reuseIdenticalFrames.Location = new System.Drawing.Point(3, 72);
            this.cb_reuseIdenticalFrames.Name = "cb_reuseIdenticalFrames";
            this.cb_reuseIdenticalFrames.Size = new System.Drawing.Size(169, 17);
            this.cb_reuseIdenticalFrames.TabIndex = 6;
            this.cb_reuseIdenticalFrames.Text = "Reuse area of identical frames";
            this.tt_mainTooltip.SetToolTip(this.cb_reuseIdenticalFrames, resources.GetString("cb_reuseIdenticalFrames.ToolTip"));
            this.cb_reuseIdenticalFrames.UseVisualStyleBackColor = true;
            this.cb_reuseIdenticalFrames.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // cb_highPrecision
            // 
            this.cb_highPrecision.AutoSize = true;
            this.cb_highPrecision.Location = new System.Drawing.Point(3, 95);
            this.cb_highPrecision.Name = "cb_highPrecision";
            this.cb_highPrecision.Size = new System.Drawing.Size(183, 17);
            this.cb_highPrecision.TabIndex = 9;
            this.cb_highPrecision.Text = "Use high precision area matching";
            this.tt_mainTooltip.SetToolTip(this.cb_highPrecision, resources.GetString("cb_highPrecision.ToolTip"));
            this.cb_highPrecision.UseVisualStyleBackColor = true;
            this.cb_highPrecision.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // cb_allowUordering
            // 
            this.cb_allowUordering.AutoSize = true;
            this.cb_allowUordering.Checked = true;
            this.cb_allowUordering.CheckState = System.Windows.Forms.CheckState.Checked;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_allowUordering, true);
            this.cb_allowUordering.Location = new System.Drawing.Point(3, 118);
            this.cb_allowUordering.Name = "cb_allowUordering";
            this.cb_allowUordering.Size = new System.Drawing.Size(114, 17);
            this.cb_allowUordering.TabIndex = 7;
            this.cb_allowUordering.Text = "Sort frames by size";
            this.tt_mainTooltip.SetToolTip(this.cb_allowUordering, "Whether to sort frames by size on the export sheet.\r\nThe frames will be laid from" +
        " larger dimensions to smaller.\r\nChecking this option will in almost all cases im" +
        "prove packing\r\nefficiency.");
            this.cb_allowUordering.UseVisualStyleBackColor = true;
            this.cb_allowUordering.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // cb_padFramesOnXml
            // 
            this.cb_padFramesOnXml.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.cb_padFramesOnXml, true);
            this.cb_padFramesOnXml.Location = new System.Drawing.Point(3, 141);
            this.cb_padFramesOnXml.Name = "cb_padFramesOnXml";
            this.cb_padFramesOnXml.Size = new System.Drawing.Size(152, 17);
            this.cb_padFramesOnXml.TabIndex = 8;
            this.cb_padFramesOnXml.Text = "Pad frame bounds on XML";
            this.tt_mainTooltip.SetToolTip(this.cb_padFramesOnXml, resources.GetString("cb_padFramesOnXml.ToolTip"));
            this.cb_padFramesOnXml.UseVisualStyleBackColor = true;
            this.cb_padFramesOnXml.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // cb_exportXml
            // 
            this.cb_exportXml.AutoSize = true;
            this.cb_exportXml.Checked = true;
            this.cb_exportXml.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_exportXml.Location = new System.Drawing.Point(3, 164);
            this.cb_exportXml.Name = "cb_exportXml";
            this.cb_exportXml.Size = new System.Drawing.Size(76, 17);
            this.cb_exportXml.TabIndex = 10;
            this.cb_exportXml.Text = "Export Xml";
            this.tt_mainTooltip.SetToolTip(this.cb_exportXml, resources.GetString("cb_exportXml.ToolTip"));
            this.cb_exportXml.UseVisualStyleBackColor = true;
            this.cb_exportXml.CheckedChanged += new System.EventHandler(this.checkboxesChange);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 241);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Y padding:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 217);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "X padding:";
            // 
            // btn_generatePreview
            // 
            this.btn_generatePreview.Image = global::Pixelaria.Properties.Resources.go_next;
            this.btn_generatePreview.Location = new System.Drawing.Point(209, 85);
            this.btn_generatePreview.Name = "btn_generatePreview";
            this.btn_generatePreview.Size = new System.Drawing.Size(80, 46);
            this.btn_generatePreview.TabIndex = 2;
            this.btn_generatePreview.Text = "Generate Preview";
            this.btn_generatePreview.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btn_generatePreview.UseVisualStyleBackColor = true;
            this.btn_generatePreview.Click += new System.EventHandler(this.btn_generatePreview_Click);
            // 
            // pnl_alertPanel
            // 
            this.pnl_alertPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_alertPanel.Controls.Add(this.pictureBox2);
            this.pnl_alertPanel.Controls.Add(this.lbl_alertLabel);
            this.pnl_alertPanel.Location = new System.Drawing.Point(12, 493);
            this.pnl_alertPanel.Name = "pnl_alertPanel";
            this.pnl_alertPanel.Size = new System.Drawing.Size(428, 29);
            this.pnl_alertPanel.TabIndex = 26;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(5, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(22, 22);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // lbl_alertLabel
            // 
            this.lbl_alertLabel.AutoSize = true;
            this.lbl_alertLabel.Location = new System.Drawing.Point(29, 8);
            this.lbl_alertLabel.Name = "lbl_alertLabel";
            this.lbl_alertLabel.Size = new System.Drawing.Size(242, 13);
            this.lbl_alertLabel.TabIndex = 9;
            this.lbl_alertLabel.Text = "The project folder path is invalid or does not exists";
            // 
            // pnl_errorPanel
            // 
            this.pnl_errorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_errorPanel.Controls.Add(this.pictureBox1);
            this.pnl_errorPanel.Controls.Add(this.lbl_error);
            this.pnl_errorPanel.Location = new System.Drawing.Point(12, 493);
            this.pnl_errorPanel.Name = "pnl_errorPanel";
            this.pnl_errorPanel.Size = new System.Drawing.Size(428, 29);
            this.pnl_errorPanel.TabIndex = 25;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(5, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(22, 22);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // lbl_error
            // 
            this.lbl_error.AutoSize = true;
            this.lbl_error.Location = new System.Drawing.Point(29, 8);
            this.lbl_error.Name = "lbl_error";
            this.lbl_error.Size = new System.Drawing.Size(242, 13);
            this.lbl_error.TabIndex = 9;
            this.lbl_error.Text = "The project folder path is invalid or does not exists";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.txt_sheetName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(590, 53);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animation Sheet Information";
            // 
            // txt_sheetName
            // 
            this.txt_sheetName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_sheetName.Location = new System.Drawing.Point(50, 23);
            this.txt_sheetName.Name = "txt_sheetName";
            this.txt_sheetName.Size = new System.Drawing.Size(534, 20);
            this.txt_sheetName.TabIndex = 1;
            this.txt_sheetName.TextChanged += new System.EventHandler(this.txt_sheetName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // lbl_sheetPreview
            // 
            this.lbl_sheetPreview.AutoSize = true;
            this.lbl_sheetPreview.Location = new System.Drawing.Point(314, 74);
            this.lbl_sheetPreview.Name = "lbl_sheetPreview";
            this.lbl_sheetPreview.Size = new System.Drawing.Size(79, 13);
            this.lbl_sheetPreview.TabIndex = 1;
            this.lbl_sheetPreview.Text = "Sheet Preview:";
            // 
            // zpb_sheetPreview
            // 
            this.zpb_sheetPreview.AllowScrollbars = false;
            this.zpb_sheetPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zpb_sheetPreview.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_sheetPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.zpb_sheetPreview.ClipBackgroundToImage = true;
            this.zpb_sheetPreview.Location = new System.Drawing.Point(317, 90);
            this.zpb_sheetPreview.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_sheetPreview.MaximumZoom")));
            this.zpb_sheetPreview.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_sheetPreview.MinimumZoom")));
            this.zpb_sheetPreview.Name = "zpb_sheetPreview";
            this.zpb_sheetPreview.ShowImageArea = true;
            this.zpb_sheetPreview.Size = new System.Drawing.Size(285, 381);
            this.zpb_sheetPreview.TabIndex = 0;
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
            this.gb_sheetInfo.Location = new System.Drawing.Point(3, 3);
            this.gb_sheetInfo.Name = "gb_sheetInfo";
            this.gb_sheetInfo.Size = new System.Drawing.Size(293, 51);
            this.gb_sheetInfo.TabIndex = 28;
            this.gb_sheetInfo.TabStop = false;
            this.gb_sheetInfo.Text = "Sheet Info";
            // 
            // lbl_frameCount
            // 
            this.lbl_frameCount.AutoSize = true;
            this.lbl_frameCount.Location = new System.Drawing.Point(207, 23);
            this.lbl_frameCount.Name = "lbl_frameCount";
            this.lbl_frameCount.Size = new System.Drawing.Size(13, 13);
            this.lbl_frameCount.TabIndex = 3;
            this.lbl_frameCount.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(131, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Frame Count:";
            // 
            // lbl_animCount
            // 
            this.lbl_animCount.AutoSize = true;
            this.lbl_animCount.Location = new System.Drawing.Point(77, 23);
            this.lbl_animCount.Name = "lbl_animCount";
            this.lbl_animCount.Size = new System.Drawing.Size(13, 13);
            this.lbl_animCount.TabIndex = 1;
            this.lbl_animCount.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Animations:";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel2.Controls.Add(this.gb_sheetInfo);
            this.flowLayoutPanel2.Controls.Add(this.groupBox2);
            this.flowLayoutPanel2.Controls.Add(this.gb_exportSummary);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(12, 74);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(299, 413);
            this.flowLayoutPanel2.TabIndex = 29;
            // 
            // gb_exportSummary
            // 
            this.gb_exportSummary.Controls.Add(this.lbl_reusedFrames);
            this.gb_exportSummary.Controls.Add(this.lbl_framesOnSheet);
            this.gb_exportSummary.Controls.Add(this.label9);
            this.gb_exportSummary.Controls.Add(this.label8);
            this.gb_exportSummary.Controls.Add(this.lbl_pixelCount);
            this.gb_exportSummary.Controls.Add(this.label7);
            this.gb_exportSummary.Controls.Add(this.lbl_dimensions);
            this.gb_exportSummary.Controls.Add(this.label6);
            this.gb_exportSummary.Location = new System.Drawing.Point(3, 342);
            this.gb_exportSummary.Name = "gb_exportSummary";
            this.gb_exportSummary.Size = new System.Drawing.Size(293, 70);
            this.gb_exportSummary.TabIndex = 29;
            this.gb_exportSummary.TabStop = false;
            this.gb_exportSummary.Text = "Export Summary";
            // 
            // lbl_reusedFrames
            // 
            this.lbl_reusedFrames.AutoSize = true;
            this.lbl_reusedFrames.Location = new System.Drawing.Point(227, 48);
            this.lbl_reusedFrames.Name = "lbl_reusedFrames";
            this.lbl_reusedFrames.Size = new System.Drawing.Size(10, 13);
            this.lbl_reusedFrames.TabIndex = 7;
            this.lbl_reusedFrames.Text = "-";
            this.tt_mainTooltip.SetToolTip(this.lbl_reusedFrames, "The number of frames that got their areas reused on the\r\nsheet.");
            // 
            // lbl_framesOnSheet
            // 
            this.lbl_framesOnSheet.AutoSize = true;
            this.lbl_framesOnSheet.Location = new System.Drawing.Point(186, 26);
            this.lbl_framesOnSheet.Name = "lbl_framesOnSheet";
            this.lbl_framesOnSheet.Size = new System.Drawing.Size(10, 13);
            this.lbl_framesOnSheet.TabIndex = 6;
            this.lbl_framesOnSheet.Text = "-";
            this.tt_mainTooltip.SetToolTip(this.lbl_framesOnSheet, "The total ammount of frames visible on the sheet,\r\nnot including reused frames.");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(136, 48);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 13);
            this.label9.TabIndex = 5;
            this.label9.Text = "Reused frames:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(136, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Frames:";
            // 
            // lbl_pixelCount
            // 
            this.lbl_pixelCount.AutoSize = true;
            this.lbl_pixelCount.Location = new System.Drawing.Point(81, 48);
            this.lbl_pixelCount.Name = "lbl_pixelCount";
            this.lbl_pixelCount.Size = new System.Drawing.Size(10, 13);
            this.lbl_pixelCount.TabIndex = 3;
            this.lbl_pixelCount.Text = "-";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 48);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Pixel count:";
            // 
            // lbl_dimensions
            // 
            this.lbl_dimensions.AutoSize = true;
            this.lbl_dimensions.Location = new System.Drawing.Point(80, 26);
            this.lbl_dimensions.Name = "lbl_dimensions";
            this.lbl_dimensions.Size = new System.Drawing.Size(10, 13);
            this.lbl_dimensions.TabIndex = 1;
            this.lbl_dimensions.Text = "-";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Dimensions:";
            // 
            // lbl_zoomLevel
            // 
            this.lbl_zoomLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl_zoomLevel.AutoSize = true;
            this.lbl_zoomLevel.Location = new System.Drawing.Point(317, 473);
            this.lbl_zoomLevel.Name = "lbl_zoomLevel";
            this.lbl_zoomLevel.Size = new System.Drawing.Size(51, 13);
            this.lbl_zoomLevel.TabIndex = 30;
            this.lbl_zoomLevel.Text = "Zoom: 1x";
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(614, 534);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(630, 572);
            this.Name = "AnimationSheetView";
            this.Text = "Animation Sheet Editor";
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
        private Pixelaria.Views.Controls.ZoomablePictureBox zpb_sheetPreview;
        private System.Windows.Forms.Label lbl_sheetPreview;
        private System.Windows.Forms.Button btn_generatePreview;
        private System.Windows.Forms.CheckBox cb_forcePowerOfTwoDimensions;
        private System.Windows.Forms.CheckBox cb_favorRatioOverarea;
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
    }
}
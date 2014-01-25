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

namespace Pixelaria.Views.MiscViews
{
    partial class AnimationResizeView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationResizeView));
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rb_drawingMode_highQuality = new System.Windows.Forms.RadioButton();
            this.rb_drawingMode_lowQuality = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_stretch = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_placeAtCenter = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_placeAtTopLeft = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nud_width = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nud_height = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_keepAspectRatio = new System.Windows.Forms.CheckBox();
            this.nud_scaleY = new System.Windows.Forms.NumericUpDown();
            this.nud_scaleX = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.zpb_preview = new Pixelaria.Views.Controls.ZoomablePictureBox();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_width)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_height)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_scaleY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_scaleX)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_preview)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rb_drawingMode_highQuality);
            this.groupBox3.Controls.Add(this.rb_drawingMode_lowQuality);
            this.groupBox3.Location = new System.Drawing.Point(12, 227);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(272, 72);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Drawing Mode";
            // 
            // rb_drawingMode_highQuality
            // 
            this.rb_drawingMode_highQuality.AutoSize = true;
            this.rb_drawingMode_highQuality.Location = new System.Drawing.Point(6, 42);
            this.rb_drawingMode_highQuality.Name = "rb_drawingMode_highQuality";
            this.rb_drawingMode_highQuality.Size = new System.Drawing.Size(85, 17);
            this.rb_drawingMode_highQuality.TabIndex = 1;
            this.rb_drawingMode_highQuality.Text = "Photography";
            this.rb_drawingMode_highQuality.UseVisualStyleBackColor = true;
            this.rb_drawingMode_highQuality.CheckedChanged += new System.EventHandler(this.radioButtonsChange);
            // 
            // rb_drawingMode_lowQuality
            // 
            this.rb_drawingMode_lowQuality.AutoSize = true;
            this.rb_drawingMode_lowQuality.Checked = true;
            this.rb_drawingMode_lowQuality.Location = new System.Drawing.Point(6, 19);
            this.rb_drawingMode_lowQuality.Name = "rb_drawingMode_lowQuality";
            this.rb_drawingMode_lowQuality.Size = new System.Drawing.Size(62, 17);
            this.rb_drawingMode_lowQuality.TabIndex = 0;
            this.rb_drawingMode_lowQuality.TabStop = true;
            this.rb_drawingMode_lowQuality.Text = "Pixel art";
            this.rb_drawingMode_lowQuality.UseVisualStyleBackColor = true;
            this.rb_drawingMode_lowQuality.CheckedChanged += new System.EventHandler(this.radioButtonsChange);
            // 
            // rb_frameScaling_stretch
            // 
            this.rb_frameScaling_stretch.AutoSize = true;
            this.rb_frameScaling_stretch.Location = new System.Drawing.Point(6, 65);
            this.rb_frameScaling_stretch.Name = "rb_frameScaling_stretch";
            this.rb_frameScaling_stretch.Size = new System.Drawing.Size(59, 17);
            this.rb_frameScaling_stretch.TabIndex = 2;
            this.rb_frameScaling_stretch.Text = "Stretch";
            this.rb_frameScaling_stretch.UseVisualStyleBackColor = true;
            this.rb_frameScaling_stretch.CheckedChanged += new System.EventHandler(this.radioButtonsChange);
            // 
            // rb_frameScaling_placeAtCenter
            // 
            this.rb_frameScaling_placeAtCenter.AutoSize = true;
            this.rb_frameScaling_placeAtCenter.Location = new System.Drawing.Point(6, 42);
            this.rb_frameScaling_placeAtCenter.Name = "rb_frameScaling_placeAtCenter";
            this.rb_frameScaling_placeAtCenter.Size = new System.Drawing.Size(98, 17);
            this.rb_frameScaling_placeAtCenter.TabIndex = 1;
            this.rb_frameScaling_placeAtCenter.Text = "Place at Center";
            this.rb_frameScaling_placeAtCenter.UseVisualStyleBackColor = true;
            this.rb_frameScaling_placeAtCenter.CheckedChanged += new System.EventHandler(this.radioButtonsChange);
            // 
            // rb_frameScaling_placeAtTopLeft
            // 
            this.rb_frameScaling_placeAtTopLeft.AutoSize = true;
            this.rb_frameScaling_placeAtTopLeft.Checked = true;
            this.rb_frameScaling_placeAtTopLeft.Location = new System.Drawing.Point(6, 19);
            this.rb_frameScaling_placeAtTopLeft.Name = "rb_frameScaling_placeAtTopLeft";
            this.rb_frameScaling_placeAtTopLeft.Size = new System.Drawing.Size(107, 17);
            this.rb_frameScaling_placeAtTopLeft.TabIndex = 0;
            this.rb_frameScaling_placeAtTopLeft.TabStop = true;
            this.rb_frameScaling_placeAtTopLeft.Text = "Place at Top Left";
            this.rb_frameScaling_placeAtTopLeft.UseVisualStyleBackColor = true;
            this.rb_frameScaling_placeAtTopLeft.CheckedChanged += new System.EventHandler(this.radioButtonsChange);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rb_frameScaling_stretch);
            this.groupBox2.Controls.Add(this.rb_frameScaling_placeAtCenter);
            this.groupBox2.Controls.Add(this.rb_frameScaling_placeAtTopLeft);
            this.groupBox2.Location = new System.Drawing.Point(12, 121);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(272, 100);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Frame Scaling Method";
            // 
            // nud_width
            // 
            this.nud_width.Location = new System.Drawing.Point(50, 25);
            this.nud_width.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.nud_width.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_width.Name = "nud_width";
            this.nud_width.Size = new System.Drawing.Size(54, 20);
            this.nud_width.TabIndex = 12;
            this.nud_width.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nud_width.ValueChanged += new System.EventHandler(this.nud_width_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Width:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Height:";
            // 
            // nud_height
            // 
            this.nud_height.Location = new System.Drawing.Point(50, 51);
            this.nud_height.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.nud_height.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_height.Name = "nud_height";
            this.nud_height.Size = new System.Drawing.Size(54, 20);
            this.nud_height.TabIndex = 15;
            this.nud_height.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nud_height.ValueChanged += new System.EventHandler(this.nud_height_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cb_keepAspectRatio);
            this.groupBox1.Controls.Add(this.nud_scaleY);
            this.groupBox1.Controls.Add(this.nud_scaleX);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.nud_height);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.nud_width);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 103);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Size";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(110, 41);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(18, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "px";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(251, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "%";
            // 
            // cb_keepAspectRatio
            // 
            this.cb_keepAspectRatio.AutoSize = true;
            this.cb_keepAspectRatio.Location = new System.Drawing.Point(76, 77);
            this.cb_keepAspectRatio.Name = "cb_keepAspectRatio";
            this.cb_keepAspectRatio.Size = new System.Drawing.Size(115, 17);
            this.cb_keepAspectRatio.TabIndex = 20;
            this.cb_keepAspectRatio.Text = "Keep Aspect Ratio";
            this.cb_keepAspectRatio.UseVisualStyleBackColor = true;
            this.cb_keepAspectRatio.CheckedChanged += new System.EventHandler(this.cb_keepAspectRatio_CheckedChanged);
            // 
            // nud_scaleY
            // 
            this.nud_scaleY.DecimalPlaces = 2;
            this.nud_scaleY.Location = new System.Drawing.Point(178, 51);
            this.nud_scaleY.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_scaleY.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_scaleY.Name = "nud_scaleY";
            this.nud_scaleY.Size = new System.Drawing.Size(67, 20);
            this.nud_scaleY.TabIndex = 19;
            this.nud_scaleY.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud_scaleY.ValueChanged += new System.EventHandler(this.nud_scaleY_ValueChanged);
            // 
            // nud_scaleX
            // 
            this.nud_scaleX.DecimalPlaces = 2;
            this.nud_scaleX.Location = new System.Drawing.Point(178, 25);
            this.nud_scaleX.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_scaleX.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_scaleX.Name = "nud_scaleX";
            this.nud_scaleX.Size = new System.Drawing.Size(67, 20);
            this.nud_scaleX.TabIndex = 18;
            this.nud_scaleX.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud_scaleX.ValueChanged += new System.EventHandler(this.nud_scaleX_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(131, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Scale Y:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(131, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Scale X:";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.zpb_preview);
            this.groupBox4.Location = new System.Drawing.Point(290, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(279, 287);
            this.groupBox4.TabIndex = 17;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Preview";
            // 
            // zpb_preview
            // 
            this.zpb_preview.AllowScrollbars = false;
            this.zpb_preview.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_preview.Location = new System.Drawing.Point(6, 16);
            this.zpb_preview.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.MaximumZoom")));
            this.zpb_preview.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.MinimumZoom")));
            this.zpb_preview.Name = "zpb_preview";
            this.zpb_preview.ShowImageArea = true;
            this.zpb_preview.Size = new System.Drawing.Size(267, 265);
            this.zpb_preview.TabIndex = 0;
            this.zpb_preview.TabStop = false;
            this.zpb_preview.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.Zoom")));
            this.zpb_preview.ZoomFactor = 1.414214F;
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(427, 305);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(61, 23);
            this.btn_ok.TabIndex = 10;
            this.btn_ok.Text = "&Ok";
            this.btn_ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Location = new System.Drawing.Point(494, 305);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 11;
            this.btn_cancel.Text = "&Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // AnimationResizeView
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(581, 339);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AnimationResizeView";
            this.Text = "Animation Resize";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AnimationResizeView_FormClosed);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_width)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_height)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_scaleY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_scaleX)).EndInit();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.zpb_preview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rb_drawingMode_highQuality;
        private System.Windows.Forms.RadioButton rb_drawingMode_lowQuality;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.RadioButton rb_frameScaling_stretch;
        private System.Windows.Forms.RadioButton rb_frameScaling_placeAtCenter;
        private System.Windows.Forms.RadioButton rb_frameScaling_placeAtTopLeft;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown nud_width;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nud_height;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_scaleY;
        private System.Windows.Forms.NumericUpDown nud_scaleX;
        private System.Windows.Forms.CheckBox cb_keepAspectRatio;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox4;
        private Controls.ZoomablePictureBox zpb_preview;
    }
}
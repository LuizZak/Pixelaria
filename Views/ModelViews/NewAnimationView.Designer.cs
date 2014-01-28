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
    partial class NewAnimationView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewAnimationView));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cb_frameskip = new System.Windows.Forms.CheckBox();
            this.nud_fps = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nud_height = new System.Windows.Forms.NumericUpDown();
            this.nud_width = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_animationName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pnl_alertPanel = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lbl_alertLabel = new System.Windows.Forms.Label();
            this.pnl_errorPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_error = new System.Windows.Forms.Label();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_fps)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_width)).BeginInit();
            this.pnl_alertPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.pnl_errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.txt_animationName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(520, 135);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animation Properties";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cb_frameskip);
            this.groupBox3.Controls.Add(this.nud_fps);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(149, 45);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(128, 84);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Playback Settings";
            // 
            // cb_frameskip
            // 
            this.cb_frameskip.AutoSize = true;
            this.cb_frameskip.Location = new System.Drawing.Point(10, 51);
            this.cb_frameskip.Name = "cb_frameskip";
            this.cb_frameskip.Size = new System.Drawing.Size(110, 17);
            this.cb_frameskip.TabIndex = 5;
            this.cb_frameskip.Text = "Enable Frameskip";
            this.cb_frameskip.UseVisualStyleBackColor = true;
            // 
            // nud_fps
            // 
            this.nud_fps.Location = new System.Drawing.Point(42, 18);
            this.nud_fps.Maximum = new decimal(new int[] {
            480,
            0,
            0,
            0});
            this.nud_fps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nud_fps.Name = "nud_fps";
            this.nud_fps.Size = new System.Drawing.Size(74, 20);
            this.nud_fps.TabIndex = 4;
            this.nud_fps.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "FPS:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nud_height);
            this.groupBox2.Controls.Add(this.nud_width);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(9, 45);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(134, 84);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Dimensions";
            // 
            // nud_height
            // 
            this.nud_height.Location = new System.Drawing.Point(50, 56);
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
            this.nud_height.Size = new System.Drawing.Size(74, 20);
            this.nud_height.TabIndex = 3;
            this.nud_height.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // nud_width
            // 
            this.nud_width.Location = new System.Drawing.Point(50, 19);
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
            this.nud_width.Size = new System.Drawing.Size(74, 20);
            this.nud_width.TabIndex = 2;
            this.nud_width.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(82, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "x";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Height:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Width:";
            // 
            // txt_animationName
            // 
            this.txt_animationName.Location = new System.Drawing.Point(50, 19);
            this.txt_animationName.Name = "txt_animationName";
            this.txt_animationName.Size = new System.Drawing.Size(464, 20);
            this.txt_animationName.TabIndex = 0;
            this.txt_animationName.TextChanged += new System.EventHandler(this.txt_animationName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // pnl_alertPanel
            // 
            this.pnl_alertPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_alertPanel.Controls.Add(this.pictureBox2);
            this.pnl_alertPanel.Controls.Add(this.lbl_alertLabel);
            this.pnl_alertPanel.Location = new System.Drawing.Point(12, 153);
            this.pnl_alertPanel.Name = "pnl_alertPanel";
            this.pnl_alertPanel.Size = new System.Drawing.Size(358, 29);
            this.pnl_alertPanel.TabIndex = 22;
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
            this.pnl_errorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_errorPanel.Controls.Add(this.pictureBox1);
            this.pnl_errorPanel.Controls.Add(this.lbl_error);
            this.pnl_errorPanel.Location = new System.Drawing.Point(12, 153);
            this.pnl_errorPanel.Name = "pnl_errorPanel";
            this.pnl_errorPanel.Size = new System.Drawing.Size(358, 29);
            this.pnl_errorPanel.TabIndex = 21;
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
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Enabled = false;
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(376, 153);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 29);
            this.btn_ok.TabIndex = 6;
            this.btn_ok.Text = "&OK";
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
            this.btn_cancel.Location = new System.Drawing.Point(457, 153);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 29);
            this.btn_cancel.TabIndex = 7;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // NewAnimationView
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(544, 194);
            this.Controls.Add(this.pnl_alertPanel);
            this.Controls.Add(this.pnl_errorPanel);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(560, 232);
            this.MinimumSize = new System.Drawing.Size(560, 232);
            this.Name = "NewAnimationView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create New Animation";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_fps)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_width)).EndInit();
            this.pnl_alertPanel.ResumeLayout(false);
            this.pnl_alertPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.pnl_errorPanel.ResumeLayout(false);
            this.pnl_errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_animationName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nud_fps;
        private System.Windows.Forms.CheckBox cb_frameskip;
        private System.Windows.Forms.NumericUpDown nud_height;
        private System.Windows.Forms.NumericUpDown nud_width;
    }
}
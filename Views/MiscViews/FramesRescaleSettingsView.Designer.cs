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
    partial class FramesRescaleSettingsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FramesRescaleSettingsView));
            this.lbl_message = new System.Windows.Forms.Label();
            this.gb_animationSize = new System.Windows.Forms.GroupBox();
            this.rb_animSize_useNewSize = new System.Windows.Forms.RadioButton();
            this.rb_animSize_useMaximumSize = new System.Windows.Forms.RadioButton();
            this.rb_animSize_keepOriginal = new System.Windows.Forms.RadioButton();
            this.gb_frameScaling = new System.Windows.Forms.GroupBox();
            this.rb_frameScaling_stretch = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_placeAtCenter = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_placeAtTopLeft = new System.Windows.Forms.RadioButton();
            this.gb_drawingMode = new System.Windows.Forms.GroupBox();
            this.rb_drawingMode_highQuality = new System.Windows.Forms.RadioButton();
            this.rb_drawingMode_lowQuality = new System.Windows.Forms.RadioButton();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.rb_frameScaling_zoom = new System.Windows.Forms.RadioButton();
            this.gb_animationSize.SuspendLayout();
            this.gb_frameScaling.SuspendLayout();
            this.gb_drawingMode.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_message
            // 
            this.lbl_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_message.Location = new System.Drawing.Point(12, 9);
            this.lbl_message.Name = "lbl_message";
            this.lbl_message.Size = new System.Drawing.Size(232, 44);
            this.lbl_message.TabIndex = 0;
            this.lbl_message.Text = "Some of the frames being pasted don\'t have a resolution that matchis this animati" +
    "on\'s. Please select the scaling options for these frames:";
            // 
            // gb_animationSize
            // 
            this.gb_animationSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_animationSize.Controls.Add(this.rb_animSize_useNewSize);
            this.gb_animationSize.Controls.Add(this.rb_animSize_useMaximumSize);
            this.gb_animationSize.Controls.Add(this.rb_animSize_keepOriginal);
            this.gb_animationSize.Location = new System.Drawing.Point(3, 3);
            this.gb_animationSize.Name = "gb_animationSize";
            this.gb_animationSize.Size = new System.Drawing.Size(226, 95);
            this.gb_animationSize.TabIndex = 1;
            this.gb_animationSize.TabStop = false;
            this.gb_animationSize.Text = "Animation Size";
            // 
            // rb_animSize_useNewSize
            // 
            this.rb_animSize_useNewSize.AutoSize = true;
            this.rb_animSize_useNewSize.Location = new System.Drawing.Point(6, 65);
            this.rb_animSize_useNewSize.Name = "rb_animSize_useNewSize";
            this.rb_animSize_useNewSize.Size = new System.Drawing.Size(92, 17);
            this.rb_animSize_useNewSize.TabIndex = 2;
            this.rb_animSize_useNewSize.TabStop = true;
            this.rb_animSize_useNewSize.Text = "Use New Size";
            this.rb_animSize_useNewSize.UseVisualStyleBackColor = true;
            // 
            // rb_animSize_useMaximumSize
            // 
            this.rb_animSize_useMaximumSize.AutoSize = true;
            this.rb_animSize_useMaximumSize.Location = new System.Drawing.Point(6, 42);
            this.rb_animSize_useMaximumSize.Name = "rb_animSize_useMaximumSize";
            this.rb_animSize_useMaximumSize.Size = new System.Drawing.Size(114, 17);
            this.rb_animSize_useMaximumSize.TabIndex = 1;
            this.rb_animSize_useMaximumSize.Text = "Use Maximum Size";
            this.rb_animSize_useMaximumSize.UseVisualStyleBackColor = true;
            // 
            // rb_animSize_keepOriginal
            // 
            this.rb_animSize_keepOriginal.AutoSize = true;
            this.rb_animSize_keepOriginal.Checked = true;
            this.rb_animSize_keepOriginal.Location = new System.Drawing.Point(6, 19);
            this.rb_animSize_keepOriginal.Name = "rb_animSize_keepOriginal";
            this.rb_animSize_keepOriginal.Size = new System.Drawing.Size(88, 17);
            this.rb_animSize_keepOriginal.TabIndex = 0;
            this.rb_animSize_keepOriginal.TabStop = true;
            this.rb_animSize_keepOriginal.Text = "Keep Original";
            this.rb_animSize_keepOriginal.UseVisualStyleBackColor = true;
            // 
            // gb_frameScaling
            // 
            this.gb_frameScaling.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_zoom);
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_stretch);
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_placeAtCenter);
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_placeAtTopLeft);
            this.gb_frameScaling.Location = new System.Drawing.Point(3, 104);
            this.gb_frameScaling.Name = "gb_frameScaling";
            this.gb_frameScaling.Size = new System.Drawing.Size(226, 113);
            this.gb_frameScaling.TabIndex = 2;
            this.gb_frameScaling.TabStop = false;
            this.gb_frameScaling.Text = "Frame Scaling Method";
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
            // 
            // gb_drawingMode
            // 
            this.gb_drawingMode.Controls.Add(this.rb_drawingMode_highQuality);
            this.gb_drawingMode.Controls.Add(this.rb_drawingMode_lowQuality);
            this.gb_drawingMode.Location = new System.Drawing.Point(3, 223);
            this.gb_drawingMode.Name = "gb_drawingMode";
            this.gb_drawingMode.Size = new System.Drawing.Size(226, 72);
            this.gb_drawingMode.TabIndex = 3;
            this.gb_drawingMode.TabStop = false;
            this.gb_drawingMode.Text = "Drawing Mode";
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
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(102, 365);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(61, 23);
            this.btn_ok.TabIndex = 4;
            this.btn_ok.Text = "&Ok";
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
            this.btn_cancel.Location = new System.Drawing.Point(169, 365);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = "&Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.gb_animationSize);
            this.flowLayoutPanel1.Controls.Add(this.gb_frameScaling);
            this.flowLayoutPanel1.Controls.Add(this.gb_drawingMode);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 56);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(232, 301);
            this.flowLayoutPanel1.TabIndex = 6;
            // 
            // rb_frameScaling_zoom
            // 
            this.rb_frameScaling_zoom.AutoSize = true;
            this.rb_frameScaling_zoom.Location = new System.Drawing.Point(6, 88);
            this.rb_frameScaling_zoom.Name = "rb_frameScaling_zoom";
            this.rb_frameScaling_zoom.Size = new System.Drawing.Size(52, 17);
            this.rb_frameScaling_zoom.TabIndex = 3;
            this.rb_frameScaling_zoom.Text = "Zoom";
            this.rb_frameScaling_zoom.UseVisualStyleBackColor = true;
            // 
            // FramesRescaleSettingsView
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(256, 400);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.lbl_message);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FramesRescaleSettingsView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Frames Rescale Settings";
            this.gb_animationSize.ResumeLayout(false);
            this.gb_animationSize.PerformLayout();
            this.gb_frameScaling.ResumeLayout(false);
            this.gb_frameScaling.PerformLayout();
            this.gb_drawingMode.ResumeLayout(false);
            this.gb_drawingMode.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.GroupBox gb_animationSize;
        private System.Windows.Forms.RadioButton rb_animSize_useMaximumSize;
        private System.Windows.Forms.RadioButton rb_animSize_keepOriginal;
        private System.Windows.Forms.GroupBox gb_frameScaling;
        private System.Windows.Forms.RadioButton rb_frameScaling_placeAtTopLeft;
        private System.Windows.Forms.RadioButton rb_frameScaling_placeAtCenter;
        private System.Windows.Forms.RadioButton rb_frameScaling_stretch;
        private System.Windows.Forms.GroupBox gb_drawingMode;
        private System.Windows.Forms.RadioButton rb_drawingMode_highQuality;
        private System.Windows.Forms.RadioButton rb_drawingMode_lowQuality;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.RadioButton rb_animSize_useNewSize;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.RadioButton rb_frameScaling_zoom;
    }
}
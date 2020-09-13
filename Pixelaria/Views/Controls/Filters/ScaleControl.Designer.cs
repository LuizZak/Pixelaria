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

namespace Pixelaria.Views.Controls.Filters
{
    partial class ScaleControl
    {
        #region Component Designer generated code

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        protected System.ComponentModel.IContainer components = null;

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

            if (preview != null)
            {
                preview.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        protected void InitializeComponent()
        {
            this.cb_centered = new System.Windows.Forms.CheckBox();
            this.cb_keepAspect = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.anud_scaleY = new PixelariaLib.Views.Controls.AssistedNumericUpDown();
            this.anud_scaleX = new PixelariaLib.Views.Controls.AssistedNumericUpDown();
            this.cb_pixelQuality = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cb_centered
            // 
            this.cb_centered.AutoSize = true;
            this.cb_centered.Location = new System.Drawing.Point(60, 76);
            this.cb_centered.Name = "cb_centered";
            this.cb_centered.Size = new System.Drawing.Size(69, 17);
            this.cb_centered.TabIndex = 7;
            this.cb_centered.Text = "Centered";
            this.cb_centered.UseVisualStyleBackColor = true;
            this.cb_centered.CheckedChanged += new System.EventHandler(this.cb_centered_CheckedChanged);
            // 
            // cb_keepAspect
            // 
            this.cb_keepAspect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_keepAspect.AutoSize = true;
            this.cb_keepAspect.Location = new System.Drawing.Point(480, 76);
            this.cb_keepAspect.Name = "cb_keepAspect";
            this.cb_keepAspect.Size = new System.Drawing.Size(115, 17);
            this.cb_keepAspect.TabIndex = 6;
            this.cb_keepAspect.Text = "Keep Aspect Ratio";
            this.cb_keepAspect.UseVisualStyleBackColor = true;
            this.cb_keepAspect.CheckedChanged += new System.EventHandler(this.cb_keepAspect_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Vertical:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Horizontal:";
            // 
            // anud_scaleY
            // 
            this.anud_scaleY.AllowDecimalOnMouse = true;
            this.anud_scaleY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_scaleY.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_scaleY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_scaleY.DecimalPlaces = 3;
            this.anud_scaleY.Increment = new decimal(new int[] {
            125,
            0,
            0,
            196608});
            this.anud_scaleY.Location = new System.Drawing.Point(60, 38);
            this.anud_scaleY.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.anud_scaleY.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.anud_scaleY.Name = "anud_scaleY";
            this.anud_scaleY.Size = new System.Drawing.Size(532, 32);
            this.anud_scaleY.TabIndex = 5;
            this.anud_scaleY.Value = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            this.anud_scaleY.ValueChanged += new System.EventHandler(this.anud_scaleY_ValueChanged);
            // 
            // anud_scaleX
            // 
            this.anud_scaleX.AllowDecimalOnMouse = true;
            this.anud_scaleX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_scaleX.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_scaleX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_scaleX.DecimalPlaces = 3;
            this.anud_scaleX.Increment = new decimal(new int[] {
            125,
            0,
            0,
            196608});
            this.anud_scaleX.Location = new System.Drawing.Point(60, 0);
            this.anud_scaleX.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.anud_scaleX.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.anud_scaleX.Name = "anud_scaleX";
            this.anud_scaleX.Size = new System.Drawing.Size(532, 32);
            this.anud_scaleX.TabIndex = 3;
            this.anud_scaleX.Value = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            this.anud_scaleX.ValueChanged += new System.EventHandler(this.anud_scaleX_ValueChanged);
            // 
            // cb_pixelQuality
            // 
            this.cb_pixelQuality.AutoSize = true;
            this.cb_pixelQuality.Location = new System.Drawing.Point(135, 76);
            this.cb_pixelQuality.Name = "cb_pixelQuality";
            this.cb_pixelQuality.Size = new System.Drawing.Size(83, 17);
            this.cb_pixelQuality.TabIndex = 8;
            this.cb_pixelQuality.Text = "Pixel Quality";
            this.cb_pixelQuality.UseVisualStyleBackColor = true;
            this.cb_pixelQuality.CheckedChanged += new System.EventHandler(this.cb_pixelQuality_CheckedChanged);
            // 
            // ScaleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cb_pixelQuality);
            this.Controls.Add(this.cb_centered);
            this.Controls.Add(this.cb_keepAspect);
            this.Controls.Add(this.anud_scaleY);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.anud_scaleX);
            this.Controls.Add(this.label1);
            this.Name = "ScaleControl";
            this.Size = new System.Drawing.Size(595, 95);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PixelariaLib.Views.Controls.AssistedNumericUpDown anud_scaleX;
        private System.Windows.Forms.Label label1;
        private PixelariaLib.Views.Controls.AssistedNumericUpDown anud_scaleY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_keepAspect;
        private System.Windows.Forms.CheckBox cb_centered;
        private System.Windows.Forms.CheckBox cb_pixelQuality;
    }
}

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
    partial class OffsetControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.anud_offsetY = new PixelariaLib.Views.Controls.AssistedNumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.anud_offsetX = new PixelariaLib.Views.Controls.AssistedNumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_wrapHorizontal = new System.Windows.Forms.CheckBox();
            this.cb_wrapVertical = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // anud_offsetY
            // 
            this.anud_offsetY.AllowDecimalOnMouse = true;
            this.anud_offsetY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_offsetY.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_offsetY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_offsetY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_offsetY.Location = new System.Drawing.Point(60, 38);
            this.anud_offsetY.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.anud_offsetY.Minimum = new decimal(new int[] {
            4096,
            0,
            0,
            -2147483648});
            this.anud_offsetY.Name = "anud_offsetY";
            this.anud_offsetY.Size = new System.Drawing.Size(478, 32);
            this.anud_offsetY.TabIndex = 9;
            this.anud_offsetY.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_offsetY.ValueChanged += new System.EventHandler(this.anud_offsetY_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Vertical:";
            // 
            // anud_offsetX
            // 
            this.anud_offsetX.AllowDecimalOnMouse = true;
            this.anud_offsetX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_offsetX.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_offsetX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_offsetX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_offsetX.Location = new System.Drawing.Point(60, 0);
            this.anud_offsetX.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.anud_offsetX.Minimum = new decimal(new int[] {
            4096,
            0,
            0,
            -2147483648});
            this.anud_offsetX.Name = "anud_offsetX";
            this.anud_offsetX.Size = new System.Drawing.Size(478, 32);
            this.anud_offsetX.TabIndex = 7;
            this.anud_offsetX.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_offsetX.ValueChanged += new System.EventHandler(this.anud_offsetX_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Horizontal:";
            // 
            // cb_wrapHorizontal
            // 
            this.cb_wrapHorizontal.AutoSize = true;
            this.cb_wrapHorizontal.Location = new System.Drawing.Point(60, 76);
            this.cb_wrapHorizontal.Name = "cb_wrapHorizontal";
            this.cb_wrapHorizontal.Size = new System.Drawing.Size(109, 17);
            this.cb_wrapHorizontal.TabIndex = 10;
            this.cb_wrapHorizontal.Text = "Wrap Horizontally";
            this.cb_wrapHorizontal.UseVisualStyleBackColor = true;
            this.cb_wrapHorizontal.CheckedChanged += new System.EventHandler(this.cb_wrapHorizontal_CheckedChanged);
            // 
            // cb_wrapVertical
            // 
            this.cb_wrapVertical.AutoSize = true;
            this.cb_wrapVertical.Location = new System.Drawing.Point(175, 76);
            this.cb_wrapVertical.Name = "cb_wrapVertical";
            this.cb_wrapVertical.Size = new System.Drawing.Size(97, 17);
            this.cb_wrapVertical.TabIndex = 11;
            this.cb_wrapVertical.Text = "Wrap Vertically";
            this.cb_wrapVertical.UseVisualStyleBackColor = true;
            this.cb_wrapVertical.CheckedChanged += new System.EventHandler(this.cb_wrapVertical_CheckedChanged);
            // 
            // OffsetControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cb_wrapVertical);
            this.Controls.Add(this.cb_wrapHorizontal);
            this.Controls.Add(this.anud_offsetY);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.anud_offsetX);
            this.Controls.Add(this.label1);
            this.Name = "OffsetControl";
            this.Size = new System.Drawing.Size(541, 98);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PixelariaLib.Views.Controls.AssistedNumericUpDown anud_offsetY;
        private System.Windows.Forms.Label label2;
        private PixelariaLib.Views.Controls.AssistedNumericUpDown anud_offsetX;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cb_wrapHorizontal;
        private System.Windows.Forms.CheckBox cb_wrapVertical;
    }
}

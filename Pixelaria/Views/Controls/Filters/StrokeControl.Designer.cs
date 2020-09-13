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

using PixelariaLib.Views.Controls.ColorControls;

namespace Pixelaria.Views.Controls.Filters
{
    partial class StrokeControl
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
            this.anud_strokeSize = new PixelariaLib.Views.Controls.AssistedNumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_knockout = new System.Windows.Forms.CheckBox();
            this.cb_smooth = new System.Windows.Forms.CheckBox();
            this.cp_color = new PixelariaLib.Views.Controls.ColorControls.ColorPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // anud_strokeSize
            // 
            this.anud_strokeSize.AllowDecimalOnMouse = true;
            this.anud_strokeSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_strokeSize.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_strokeSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_strokeSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_strokeSize.Location = new System.Drawing.Point(50, 38);
            this.anud_strokeSize.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_strokeSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_strokeSize.Name = "anud_strokeSize";
            this.anud_strokeSize.Size = new System.Drawing.Size(386, 32);
            this.anud_strokeSize.TabIndex = 7;
            this.anud_strokeSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_strokeSize.ValueChanged += new System.EventHandler(this.anud_strokeSize_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Stroke:";
            // 
            // cb_knockout
            // 
            this.cb_knockout.AutoSize = true;
            this.cb_knockout.Location = new System.Drawing.Point(50, 76);
            this.cb_knockout.Name = "cb_knockout";
            this.cb_knockout.Size = new System.Drawing.Size(104, 17);
            this.cb_knockout.TabIndex = 10;
            this.cb_knockout.Text = "Knockout Image";
            this.cb_knockout.UseVisualStyleBackColor = true;
            this.cb_knockout.CheckedChanged += new System.EventHandler(this.cb_knockout_CheckedChanged);
            // 
            // cb_smooth
            // 
            this.cb_smooth.AutoSize = true;
            this.cb_smooth.Location = new System.Drawing.Point(160, 76);
            this.cb_smooth.Name = "cb_smooth";
            this.cb_smooth.Size = new System.Drawing.Size(62, 17);
            this.cb_smooth.TabIndex = 11;
            this.cb_smooth.Text = "Smooth";
            this.cb_smooth.UseVisualStyleBackColor = true;
            this.cb_smooth.CheckedChanged += new System.EventHandler(this.cb_smooth_CheckedChanged);
            // 
            // cp_color
            // 
            this.cp_color.BackColor = System.Drawing.Color.White;
            this.cp_color.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.cp_color.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cp_color.Location = new System.Drawing.Point(50, 3);
            this.cp_color.Name = "cp_color";
            this.cp_color.Size = new System.Drawing.Size(146, 29);
            this.cp_color.TabIndex = 13;
            this.cp_color.Click += new System.EventHandler(this.cp_color_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Color:";
            // 
            // StrokeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cp_color);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cb_smooth);
            this.Controls.Add(this.cb_knockout);
            this.Controls.Add(this.anud_strokeSize);
            this.Controls.Add(this.label1);
            this.Name = "StrokeControl";
            this.Size = new System.Drawing.Size(439, 100);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PixelariaLib.Views.Controls.AssistedNumericUpDown anud_strokeSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cb_knockout;
        private System.Windows.Forms.CheckBox cb_smooth;
        private ColorPanel cp_color;
        private System.Windows.Forms.Label label2;
    }
}

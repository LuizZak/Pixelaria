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

using Pixelaria.Views.Controls.ColorControls;

namespace Pixelaria.Views.Controls.Filters
{
    partial class FadeControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.cp_color = new ColorPanel();
            this.anud_factor = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Color:";
            // 
            // cp_color
            // 
            this.cp_color.BackColor = System.Drawing.Color.White;
            this.cp_color.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.cp_color.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cp_color.Location = new System.Drawing.Point(49, 3);
            this.cp_color.Name = "cp_color";
            this.cp_color.Size = new System.Drawing.Size(146, 29);
            this.cp_color.TabIndex = 1;
            this.cp_color.Click += new System.EventHandler(this.cp_color_Click);
            // 
            // anud_factor
            // 
            this.anud_factor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_factor.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_factor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_factor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_factor.Location = new System.Drawing.Point(49, 38);
            this.anud_factor.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_factor.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_factor.Name = "anud_factor";
            this.anud_factor.Size = new System.Drawing.Size(388, 32);
            this.anud_factor.TabIndex = 2;
            this.anud_factor.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.anud_factor.ValueChanged += new System.EventHandler(this.anud_factor_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Factor:";
            // 
            // FadeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.anud_factor);
            this.Controls.Add(this.cp_color);
            this.Controls.Add(this.label1);
            this.Name = "FadeControl";
            this.Size = new System.Drawing.Size(440, 74);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private ColorPanel cp_color;
        private AssistedNumericUpDown anud_factor;
        private System.Windows.Forms.Label label2;
    }
}

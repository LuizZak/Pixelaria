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

namespace Pixelaria.Views.Controls
{
    partial class ColorPicker
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
            this.label2 = new System.Windows.Forms.Label();
            this.pb_palette = new System.Windows.Forms.PictureBox();
            this.pnl_firstColor = new Pixelaria.Views.Controls.ColorPanel();
            this.pnl_secondColor = new Pixelaria.Views.Controls.ColorPanel();
            this.anud_transparency = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.anud_redComonent = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.anud_greenComponent = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.anud_blueComponent = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.anud_l = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.anud_s = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.anud_h = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pb_palette)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Color Pìcker";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 280);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Alpha:";
            // 
            // pb_palette
            // 
            this.pb_palette.Image = global::Pixelaria.Properties.Resources.color_picker;
            this.pb_palette.Location = new System.Drawing.Point(3, 68);
            this.pb_palette.Name = "pb_palette";
            this.pb_palette.Size = new System.Drawing.Size(216, 206);
            this.pb_palette.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_palette.TabIndex = 0;
            this.pb_palette.TabStop = false;
            this.pb_palette.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pb_palette_MouseDown);
            this.pb_palette.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pb_palette_MouseMove);
            this.pb_palette.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pb_palette_MouseUp);
            // 
            // pnl_firstColor
            // 
            this.pnl_firstColor.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.pnl_firstColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnl_firstColor.Location = new System.Drawing.Point(4, 3);
            this.pnl_firstColor.Name = "pnl_firstColor";
            this.pnl_firstColor.Size = new System.Drawing.Size(104, 46);
            this.pnl_firstColor.TabIndex = 4;
            this.pnl_firstColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl_firstColor_MouseDown);
            // 
            // pnl_secondColor
            // 
            this.pnl_secondColor.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.pnl_secondColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_secondColor.Location = new System.Drawing.Point(115, 3);
            this.pnl_secondColor.Name = "pnl_secondColor";
            this.pnl_secondColor.Size = new System.Drawing.Size(104, 46);
            this.pnl_secondColor.TabIndex = 5;
            this.pnl_secondColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl_secondColor_MouseDown);
            // 
            // anud_transparency
            // 
            this.anud_transparency.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_transparency.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_transparency.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_transparency.Location = new System.Drawing.Point(42, 280);
            this.anud_transparency.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.anud_transparency.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.anud_transparency.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_transparency.Name = "anud_transparency";
            this.anud_transparency.Size = new System.Drawing.Size(177, 33);
            this.anud_transparency.TabIndex = 2;
            this.anud_transparency.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.anud_transparency.ValueChanged += new System.EventHandler(this.anud_transparency_ValueChanged);
            // 
            // anud_redComonent
            // 
            this.anud_redComonent.AssistBarColor = System.Drawing.Color.Red;
            this.anud_redComonent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_redComonent.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_redComonent.Location = new System.Drawing.Point(42, 324);
            this.anud_redComonent.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.anud_redComonent.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.anud_redComonent.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_redComonent.Name = "anud_redComonent";
            this.anud_redComonent.Size = new System.Drawing.Size(177, 32);
            this.anud_redComonent.TabIndex = 6;
            this.anud_redComonent.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_redComonent.ValueChanged += new System.EventHandler(this.anud_redComonent_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 324);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Red:";
            // 
            // anud_greenComponent
            // 
            this.anud_greenComponent.AssistBarColor = System.Drawing.Color.Lime;
            this.anud_greenComponent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_greenComponent.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_greenComponent.Location = new System.Drawing.Point(42, 356);
            this.anud_greenComponent.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.anud_greenComponent.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.anud_greenComponent.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_greenComponent.Name = "anud_greenComponent";
            this.anud_greenComponent.Size = new System.Drawing.Size(177, 32);
            this.anud_greenComponent.TabIndex = 8;
            this.anud_greenComponent.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_greenComponent.ValueChanged += new System.EventHandler(this.anud_greenComponent_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1, 356);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Green:";
            // 
            // anud_blueComponent
            // 
            this.anud_blueComponent.AssistBarColor = System.Drawing.Color.Blue;
            this.anud_blueComponent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_blueComponent.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_blueComponent.Location = new System.Drawing.Point(42, 388);
            this.anud_blueComponent.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.anud_blueComponent.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.anud_blueComponent.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_blueComponent.Name = "anud_blueComponent";
            this.anud_blueComponent.Size = new System.Drawing.Size(177, 32);
            this.anud_blueComponent.TabIndex = 10;
            this.anud_blueComponent.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_blueComponent.ValueChanged += new System.EventHandler(this.anud_blueComponent_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 388);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Blue:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 433);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(18, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "H:";
            // 
            // anud_l
            // 
            this.anud_l.AssistBarColor = System.Drawing.Color.Blue;
            this.anud_l.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_l.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_l.Location = new System.Drawing.Point(42, 497);
            this.anud_l.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.anud_l.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_l.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_l.Name = "anud_l";
            this.anud_l.Size = new System.Drawing.Size(177, 32);
            this.anud_l.TabIndex = 15;
            this.anud_l.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_l.ValueChanged += new System.EventHandler(this.anud_l_ValueChanged);
            // 
            // anud_s
            // 
            this.anud_s.AssistBarColor = System.Drawing.Color.Lime;
            this.anud_s.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_s.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_s.Location = new System.Drawing.Point(42, 465);
            this.anud_s.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.anud_s.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_s.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_s.Name = "anud_s";
            this.anud_s.Size = new System.Drawing.Size(177, 32);
            this.anud_s.TabIndex = 14;
            this.anud_s.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_s.ValueChanged += new System.EventHandler(this.anud_s_ValueChanged);
            // 
            // anud_h
            // 
            this.anud_h.AssistBarColor = System.Drawing.Color.Red;
            this.anud_h.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_h.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_h.Location = new System.Drawing.Point(42, 433);
            this.anud_h.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.anud_h.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.anud_h.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_h.Name = "anud_h";
            this.anud_h.Size = new System.Drawing.Size(177, 32);
            this.anud_h.TabIndex = 13;
            this.anud_h.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_h.ValueChanged += new System.EventHandler(this.anud_h_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 465);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "S:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 497);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(16, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "L:";
            // 
            // ColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.anud_l);
            this.Controls.Add(this.anud_s);
            this.Controls.Add(this.anud_h);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.anud_blueComponent);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.anud_greenComponent);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.anud_redComonent);
            this.Controls.Add(this.pnl_secondColor);
            this.Controls.Add(this.pnl_firstColor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.anud_transparency);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pb_palette);
            this.Name = "ColorPicker";
            this.Size = new System.Drawing.Size(225, 624);
            ((System.ComponentModel.ISupportInitialize)(this.pb_palette)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_palette;
        private System.Windows.Forms.Label label1;
        private AssistedNumericUpDown anud_transparency;
        private System.Windows.Forms.Label label2;
        private ColorPanel pnl_firstColor;
        private ColorPanel pnl_secondColor;
        private AssistedNumericUpDown anud_redComonent;
        private System.Windows.Forms.Label label3;
        private AssistedNumericUpDown anud_greenComponent;
        private System.Windows.Forms.Label label4;
        private AssistedNumericUpDown anud_blueComponent;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private AssistedNumericUpDown anud_l;
        private AssistedNumericUpDown anud_s;
        private AssistedNumericUpDown anud_h;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}

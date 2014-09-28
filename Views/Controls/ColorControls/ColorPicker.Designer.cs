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
            this.pb_palette = new System.Windows.Forms.PictureBox();
            this.cs_lightness = new Pixelaria.Views.Controls.ColorControls.ColorSlider();
            this.cs_saturation = new Pixelaria.Views.Controls.ColorControls.ColorSlider();
            this.cs_hue = new Pixelaria.Views.Controls.ColorControls.ColorSlider();
            this.cs_blue = new Pixelaria.Views.Controls.ColorControls.ColorSlider();
            this.cs_green = new Pixelaria.Views.Controls.ColorControls.ColorSlider();
            this.cs_red = new Pixelaria.Views.Controls.ColorControls.ColorSlider();
            this.cs_alpha = new Pixelaria.Views.Controls.ColorControls.ColorSlider();
            this.pnl_secondColor = new Pixelaria.Views.Controls.ColorPanel();
            this.pnl_firstColor = new Pixelaria.Views.Controls.ColorPanel();
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
            // pb_palette
            // 
            this.pb_palette.Image = global::Pixelaria.Properties.Resources.color_picker;
            this.pb_palette.Location = new System.Drawing.Point(3, 68);
            this.pb_palette.Name = "pb_palette";
            this.pb_palette.Size = new System.Drawing.Size(163, 151);
            this.pb_palette.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_palette.TabIndex = 0;
            this.pb_palette.TabStop = false;
            this.pb_palette.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pb_palette_MouseDown);
            this.pb_palette.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pb_palette_MouseMove);
            this.pb_palette.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pb_palette_MouseUp);
            // 
            // cs_lightness
            // 
            this.cs_lightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_lightness.ColorComponent = Pixelaria.Views.Controls.ColorControls.ColorSliderComponent.Lightness;
            this.cs_lightness.Location = new System.Drawing.Point(4, 502);
            this.cs_lightness.Name = "cs_lightness";
            this.cs_lightness.Size = new System.Drawing.Size(161, 38);
            this.cs_lightness.TabIndex = 24;
            // 
            // cs_saturation
            // 
            this.cs_saturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_saturation.ColorComponent = Pixelaria.Views.Controls.ColorControls.ColorSliderComponent.Saturation;
            this.cs_saturation.Location = new System.Drawing.Point(4, 458);
            this.cs_saturation.Name = "cs_saturation";
            this.cs_saturation.Size = new System.Drawing.Size(161, 38);
            this.cs_saturation.TabIndex = 23;
            // 
            // cs_hue
            // 
            this.cs_hue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_hue.ColorComponent = Pixelaria.Views.Controls.ColorControls.ColorSliderComponent.Hue;
            this.cs_hue.Location = new System.Drawing.Point(4, 414);
            this.cs_hue.Name = "cs_hue";
            this.cs_hue.Size = new System.Drawing.Size(161, 38);
            this.cs_hue.TabIndex = 22;
            // 
            // cs_blue
            // 
            this.cs_blue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_blue.ColorComponent = Pixelaria.Views.Controls.ColorControls.ColorSliderComponent.Blue;
            this.cs_blue.Location = new System.Drawing.Point(4, 357);
            this.cs_blue.Name = "cs_blue";
            this.cs_blue.Size = new System.Drawing.Size(161, 38);
            this.cs_blue.TabIndex = 21;
            // 
            // cs_green
            // 
            this.cs_green.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_green.ColorComponent = Pixelaria.Views.Controls.ColorControls.ColorSliderComponent.Green;
            this.cs_green.Location = new System.Drawing.Point(4, 313);
            this.cs_green.Name = "cs_green";
            this.cs_green.Size = new System.Drawing.Size(161, 38);
            this.cs_green.TabIndex = 20;
            // 
            // cs_red
            // 
            this.cs_red.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_red.ColorComponent = Pixelaria.Views.Controls.ColorControls.ColorSliderComponent.Red;
            this.cs_red.Location = new System.Drawing.Point(5, 269);
            this.cs_red.Name = "cs_red";
            this.cs_red.Size = new System.Drawing.Size(161, 38);
            this.cs_red.TabIndex = 19;
            // 
            // cs_alpha
            // 
            this.cs_alpha.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_alpha.Location = new System.Drawing.Point(4, 225);
            this.cs_alpha.Name = "cs_alpha";
            this.cs_alpha.Size = new System.Drawing.Size(161, 38);
            this.cs_alpha.TabIndex = 18;
            // 
            // pnl_secondColor
            // 
            this.pnl_secondColor.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.pnl_secondColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_secondColor.Location = new System.Drawing.Point(85, 3);
            this.pnl_secondColor.Name = "pnl_secondColor";
            this.pnl_secondColor.Size = new System.Drawing.Size(81, 46);
            this.pnl_secondColor.TabIndex = 5;
            this.pnl_secondColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl_secondColor_MouseDown);
            // 
            // pnl_firstColor
            // 
            this.pnl_firstColor.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.pnl_firstColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnl_firstColor.Location = new System.Drawing.Point(4, 3);
            this.pnl_firstColor.Name = "pnl_firstColor";
            this.pnl_firstColor.Size = new System.Drawing.Size(75, 46);
            this.pnl_firstColor.TabIndex = 4;
            this.pnl_firstColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl_firstColor_MouseDown);
            // 
            // ColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cs_lightness);
            this.Controls.Add(this.cs_saturation);
            this.Controls.Add(this.cs_hue);
            this.Controls.Add(this.cs_blue);
            this.Controls.Add(this.cs_green);
            this.Controls.Add(this.cs_red);
            this.Controls.Add(this.cs_alpha);
            this.Controls.Add(this.pnl_secondColor);
            this.Controls.Add(this.pnl_firstColor);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pb_palette);
            this.Name = "ColorPicker";
            this.Size = new System.Drawing.Size(170, 624);
            ((System.ComponentModel.ISupportInitialize)(this.pb_palette)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_palette;
        private System.Windows.Forms.Label label1;
        private ColorPanel pnl_firstColor;
        private ColorPanel pnl_secondColor;
        private ColorControls.ColorSlider cs_alpha;
        private ColorControls.ColorSlider cs_red;
        private ColorControls.ColorSlider cs_green;
        private ColorControls.ColorSlider cs_blue;
        private ColorControls.ColorSlider cs_hue;
        private ColorControls.ColorSlider cs_saturation;
        private ColorControls.ColorSlider cs_lightness;
    }
}

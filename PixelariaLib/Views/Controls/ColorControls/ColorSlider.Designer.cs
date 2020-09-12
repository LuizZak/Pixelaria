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

namespace PixelariaLib.Views.Controls.ColorControls
{
    partial class ColorSlider
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
            this.txt_value = new System.Windows.Forms.TextBox();
            this.pnl_textHolder = new System.Windows.Forms.Panel();
            this.pnl_textHolder.SuspendLayout();
            this.SuspendLayout();
            // 
            // txt_value
            // 
            this.txt_value.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_value.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_value.Location = new System.Drawing.Point(0, 0);
            this.txt_value.MaxLength = 4;
            this.txt_value.Name = "txt_value";
            this.txt_value.Size = new System.Drawing.Size(29, 13);
            this.txt_value.TabIndex = 0;
            this.txt_value.TabStop = false;
            this.txt_value.Text = "100";
            this.txt_value.TextChanged += new System.EventHandler(this.rtb_value_TextChanged);
            this.txt_value.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtb_value_KeyDown);
            // 
            // pnl_textHolder
            // 
            this.pnl_textHolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_textHolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_textHolder.Controls.Add(this.txt_value);
            this.pnl_textHolder.Location = new System.Drawing.Point(208, 0);
            this.pnl_textHolder.Name = "pnl_textHolder";
            this.pnl_textHolder.Size = new System.Drawing.Size(31, 17);
            this.pnl_textHolder.TabIndex = 2;
            // 
            // ColorSlider
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnl_textHolder);
            this.Name = "ColorSlider";
            this.Size = new System.Drawing.Size(240, 38);
            this.pnl_textHolder.ResumeLayout(false);
            this.pnl_textHolder.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.TextBox txt_value;
        protected System.Windows.Forms.Panel pnl_textHolder;
    }
}

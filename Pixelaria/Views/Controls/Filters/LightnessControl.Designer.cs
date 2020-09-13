namespace Pixelaria.Views.Controls.Filters
{
    partial class LightnessControl
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
            this.cb_relative = new System.Windows.Forms.CheckBox();
            this.cb_multiply = new System.Windows.Forms.CheckBox();
            this.cs_lightness = new PixelariaLib.Views.Controls.ColorControls.ColorSlider();
            this.SuspendLayout();
            // 
            // cb_relative
            // 
            this.cb_relative.AutoSize = true;
            this.cb_relative.Location = new System.Drawing.Point(5, 39);
            this.cb_relative.Name = "cb_relative";
            this.cb_relative.Size = new System.Drawing.Size(65, 17);
            this.cb_relative.TabIndex = 8;
            this.cb_relative.Text = "Relative";
            this.cb_relative.UseVisualStyleBackColor = true;
            this.cb_relative.CheckedChanged += new System.EventHandler(this.cb_relative_CheckedChanged);
            // 
            // cb_multiply
            // 
            this.cb_multiply.AutoSize = true;
            this.cb_multiply.Location = new System.Drawing.Point(76, 39);
            this.cb_multiply.Name = "cb_multiply";
            this.cb_multiply.Size = new System.Drawing.Size(61, 17);
            this.cb_multiply.TabIndex = 9;
            this.cb_multiply.Text = "Multiply";
            this.cb_multiply.UseVisualStyleBackColor = true;
            this.cb_multiply.CheckedChanged += new System.EventHandler(this.cb_multiply_CheckedChanged);
            // 
            // cs_lightness
            // 
            this.cs_lightness.ActiveColor = new PixelariaLib.Utils.AhslColor(1F, 0F, 0F, 1F);
            this.cs_lightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_lightness.ColorComponent = PixelariaLib.Views.Controls.ColorControls.ColorSliderComponent.Lightness;
            this.cs_lightness.CurrentValue = 1F;
            this.cs_lightness.CustomColorTitle = "Custom";
            this.cs_lightness.CustomEndColor = new PixelariaLib.Utils.AhslColor(1F, 0F, 0F, 1F);
            this.cs_lightness.CustomStartColor = new PixelariaLib.Utils.AhslColor(1F, 0F, 0F, 0F);
            this.cs_lightness.Location = new System.Drawing.Point(1, 1);
            this.cs_lightness.Name = "cs_lightness";
            this.cs_lightness.Size = new System.Drawing.Size(535, 38);
            this.cs_lightness.TabIndex = 10;
            this.cs_lightness.ColorChanged += new PixelariaLib.Views.Controls.ColorControls.ColorSlider.ColorChangedEventHandler(this.cs_lightness_ColorChanged);
            // 
            // LightnessControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cs_lightness);
            this.Controls.Add(this.cb_multiply);
            this.Controls.Add(this.cb_relative);
            this.Name = "LightnessControl";
            this.Size = new System.Drawing.Size(539, 58);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb_relative;
        private System.Windows.Forms.CheckBox cb_multiply;
        private PixelariaLib.Views.Controls.ColorControls.ColorSlider cs_lightness;
    }
}

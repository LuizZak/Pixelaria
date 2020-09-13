namespace Pixelaria.Views.Controls.Filters
{
    partial class HueControl
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
            this.cs_hue = new PixelariaLib.Views.Controls.ColorControls.ColorSlider();
            this.SuspendLayout();
            // 
            // cb_relative
            // 
            this.cb_relative.AutoSize = true;
            this.cb_relative.Location = new System.Drawing.Point(5, 39);
            this.cb_relative.Name = "cb_relative";
            this.cb_relative.Size = new System.Drawing.Size(65, 17);
            this.cb_relative.TabIndex = 2;
            this.cb_relative.Text = "Relative";
            this.cb_relative.UseVisualStyleBackColor = true;
            this.cb_relative.CheckedChanged += new System.EventHandler(this.cb_relative_CheckedChanged);
            // 
            // cs_hue
            // 
            this.cs_hue.ActiveColor = new PixelariaLib.Utils.AhslColor(1F, 0F, 1F, 0.5F);
            this.cs_hue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_hue.ColorComponent = PixelariaLib.Views.Controls.ColorControls.ColorSliderComponent.Hue;
            this.cs_hue.CurrentValue = 0F;
            this.cs_hue.CustomColorTitle = "Custom";
            this.cs_hue.CustomEndColor = new PixelariaLib.Utils.AhslColor(1F, 0F, 0F, 1F);
            this.cs_hue.CustomStartColor = new PixelariaLib.Utils.AhslColor(1F, 0F, 0F, 0F);
            this.cs_hue.Location = new System.Drawing.Point(1, 1);
            this.cs_hue.Name = "cs_hue";
            this.cs_hue.Size = new System.Drawing.Size(532, 38);
            this.cs_hue.TabIndex = 3;
            this.cs_hue.ColorChanged += new PixelariaLib.Views.Controls.ColorControls.ColorSlider.ColorChangedEventHandler(this.cs_hue_ColorChanged);
            // 
            // HueControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cs_hue);
            this.Controls.Add(this.cb_relative);
            this.Name = "HueControl";
            this.Size = new System.Drawing.Size(534, 58);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox cb_relative;
        private PixelariaLib.Views.Controls.ColorControls.ColorSlider cs_hue;
    }
}

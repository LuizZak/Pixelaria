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
            this.label1 = new System.Windows.Forms.Label();
            this.anud_lightness = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.SuspendLayout();
            // 
            // cb_relative
            // 
            this.cb_relative.AutoSize = true;
            this.cb_relative.Location = new System.Drawing.Point(61, 41);
            this.cb_relative.Name = "cb_relative";
            this.cb_relative.Size = new System.Drawing.Size(65, 17);
            this.cb_relative.TabIndex = 8;
            this.cb_relative.Text = "Relative";
            this.cb_relative.UseVisualStyleBackColor = true;
            this.cb_relative.CheckedChanged += new System.EventHandler(this.cb_relative_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Lightness:";
            // 
            // anud_lightness
            // 
            this.anud_lightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_lightness.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_lightness.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_lightness.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_lightness.Location = new System.Drawing.Point(61, 3);
            this.anud_lightness.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_lightness.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.anud_lightness.Name = "anud_lightness";
            this.anud_lightness.Size = new System.Drawing.Size(475, 32);
            this.anud_lightness.TabIndex = 6;
            this.anud_lightness.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_lightness.ValueChanged += new System.EventHandler(this.anud_lightness_ValueChanged);
            // 
            // LightnessControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cb_relative);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.anud_lightness);
            this.Name = "LightnessControl";
            this.Size = new System.Drawing.Size(539, 58);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb_relative;
        private System.Windows.Forms.Label label1;
        private AssistedNumericUpDown anud_lightness;
    }
}

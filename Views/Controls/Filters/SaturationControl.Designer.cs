namespace Pixelaria.Views.Controls.Filters
{
    partial class SaturationControl
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
            this.anud_saturation = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.cb_keepGrays = new System.Windows.Forms.CheckBox();
            this.cb_multiply = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cb_relative
            // 
            this.cb_relative.AutoSize = true;
            this.cb_relative.Location = new System.Drawing.Point(61, 41);
            this.cb_relative.Name = "cb_relative";
            this.cb_relative.Size = new System.Drawing.Size(65, 17);
            this.cb_relative.TabIndex = 5;
            this.cb_relative.Text = "Relative";
            this.cb_relative.UseVisualStyleBackColor = true;
            this.cb_relative.CheckedChanged += new System.EventHandler(this.cb_relative_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Saturation:";
            // 
            // anud_saturation
            // 
            this.anud_saturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_saturation.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_saturation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_saturation.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_saturation.Location = new System.Drawing.Point(61, 3);
            this.anud_saturation.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_saturation.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.anud_saturation.Name = "anud_saturation";
            this.anud_saturation.Size = new System.Drawing.Size(472, 32);
            this.anud_saturation.TabIndex = 3;
            this.anud_saturation.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.anud_saturation.ValueChanged += new System.EventHandler(this.anud_saturation_ValueChanged);
            // 
            // cb_keepGrays
            // 
            this.cb_keepGrays.AutoSize = true;
            this.cb_keepGrays.Checked = true;
            this.cb_keepGrays.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_keepGrays.Location = new System.Drawing.Point(132, 41);
            this.cb_keepGrays.Name = "cb_keepGrays";
            this.cb_keepGrays.Size = new System.Drawing.Size(81, 17);
            this.cb_keepGrays.TabIndex = 6;
            this.cb_keepGrays.Text = "Keep Grays";
            this.cb_keepGrays.UseVisualStyleBackColor = true;
            this.cb_keepGrays.CheckedChanged += new System.EventHandler(this.cb_keepGrays_CheckedChanged);
            // 
            // cb_multiply
            // 
            this.cb_multiply.AutoSize = true;
            this.cb_multiply.Location = new System.Drawing.Point(219, 41);
            this.cb_multiply.Name = "cb_multiply";
            this.cb_multiply.Size = new System.Drawing.Size(61, 17);
            this.cb_multiply.TabIndex = 7;
            this.cb_multiply.Text = "Multiply";
            this.cb_multiply.UseVisualStyleBackColor = true;
            this.cb_multiply.CheckedChanged += new System.EventHandler(this.cb_multiply_CheckedChanged);
            // 
            // SaturationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cb_multiply);
            this.Controls.Add(this.cb_keepGrays);
            this.Controls.Add(this.cb_relative);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.anud_saturation);
            this.Name = "SaturationControl";
            this.Size = new System.Drawing.Size(536, 58);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb_relative;
        private System.Windows.Forms.Label label1;
        private AssistedNumericUpDown anud_saturation;
        private System.Windows.Forms.CheckBox cb_keepGrays;
        private System.Windows.Forms.CheckBox cb_multiply;
    }
}

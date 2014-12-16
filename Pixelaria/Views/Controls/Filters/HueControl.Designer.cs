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
            this.anud_hue = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_relative = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // anud_hue
            // 
            this.anud_hue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_hue.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_hue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_hue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_hue.Location = new System.Drawing.Point(61, 3);
            this.anud_hue.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.anud_hue.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_hue.Name = "anud_hue";
            this.anud_hue.Size = new System.Drawing.Size(470, 32);
            this.anud_hue.TabIndex = 0;
            this.anud_hue.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_hue.ValueChanged += new System.EventHandler(this.anud_hue_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Hue:";
            // 
            // cb_relative
            // 
            this.cb_relative.AutoSize = true;
            this.cb_relative.Location = new System.Drawing.Point(61, 41);
            this.cb_relative.Name = "cb_relative";
            this.cb_relative.Size = new System.Drawing.Size(65, 17);
            this.cb_relative.TabIndex = 2;
            this.cb_relative.Text = "Relative";
            this.cb_relative.UseVisualStyleBackColor = true;
            this.cb_relative.CheckedChanged += new System.EventHandler(this.cb_relative_CheckedChanged);
            // 
            // HueControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cb_relative);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.anud_hue);
            this.Name = "HueControl";
            this.Size = new System.Drawing.Size(534, 58);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AssistedNumericUpDown anud_hue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cb_relative;
    }
}

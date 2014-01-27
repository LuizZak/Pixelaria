namespace Pixelaria.Views.Controls.Filters
{
    partial class RotationControl
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
            this.anud_angle = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.cb_rotateAroundCenter = new System.Windows.Forms.CheckBox();
            this.cb_pixelQuality = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Angle (degrees):";
            // 
            // anud_angle
            // 
            this.anud_angle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_angle.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_angle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_angle.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_angle.Location = new System.Drawing.Point(87, 3);
            this.anud_angle.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.anud_angle.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.anud_angle.Name = "anud_angle";
            this.anud_angle.Size = new System.Drawing.Size(429, 32);
            this.anud_angle.TabIndex = 1;
            this.anud_angle.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_angle.ValueChanged += new System.EventHandler(this.anud_angle_ValueChanged);
            // 
            // cb_rotateAroundCenter
            // 
            this.cb_rotateAroundCenter.AutoSize = true;
            this.cb_rotateAroundCenter.Location = new System.Drawing.Point(87, 41);
            this.cb_rotateAroundCenter.Name = "cb_rotateAroundCenter";
            this.cb_rotateAroundCenter.Size = new System.Drawing.Size(129, 17);
            this.cb_rotateAroundCenter.TabIndex = 2;
            this.cb_rotateAroundCenter.Text = "Rotate Around Center";
            this.cb_rotateAroundCenter.UseVisualStyleBackColor = true;
            this.cb_rotateAroundCenter.CheckedChanged += new System.EventHandler(this.cb_rotateAroundCenter_CheckedChanged);
            // 
            // cb_pixelQuality
            // 
            this.cb_pixelQuality.AutoSize = true;
            this.cb_pixelQuality.Location = new System.Drawing.Point(220, 41);
            this.cb_pixelQuality.Name = "cb_pixelQuality";
            this.cb_pixelQuality.Size = new System.Drawing.Size(83, 17);
            this.cb_pixelQuality.TabIndex = 3;
            this.cb_pixelQuality.Text = "Pixel Quality";
            this.cb_pixelQuality.UseVisualStyleBackColor = true;
            this.cb_pixelQuality.CheckedChanged += new System.EventHandler(this.cb_pixelQuality_CheckedChanged);
            // 
            // RotationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cb_pixelQuality);
            this.Controls.Add(this.cb_rotateAroundCenter);
            this.Controls.Add(this.anud_angle);
            this.Controls.Add(this.label1);
            this.Name = "RotationControl";
            this.Size = new System.Drawing.Size(519, 63);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private AssistedNumericUpDown anud_angle;
        private System.Windows.Forms.CheckBox cb_rotateAroundCenter;
        private System.Windows.Forms.CheckBox cb_pixelQuality;
    }
}

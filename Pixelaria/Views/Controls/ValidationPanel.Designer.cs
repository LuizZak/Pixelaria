namespace Pixelaria.Views.Controls
{
    partial class ValidationPanel
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
            this.pb_state = new System.Windows.Forms.PictureBox();
            this.lbl_alertLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pb_state)).BeginInit();
            this.SuspendLayout();
            // 
            // pb_state
            // 
            this.pb_state.Image = global::Pixelaria.Properties.Resources.important_22;
            this.pb_state.Location = new System.Drawing.Point(3, 3);
            this.pb_state.Name = "pb_state";
            this.pb_state.Size = new System.Drawing.Size(22, 22);
            this.pb_state.TabIndex = 10;
            this.pb_state.TabStop = false;
            // 
            // lbl_alertLabel
            // 
            this.lbl_alertLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_alertLabel.AutoSize = true;
            this.lbl_alertLabel.Location = new System.Drawing.Point(31, 8);
            this.lbl_alertLabel.Name = "lbl_alertLabel";
            this.lbl_alertLabel.Size = new System.Drawing.Size(93, 13);
            this.lbl_alertLabel.TabIndex = 9;
            this.lbl_alertLabel.Text = "Warning: Warning";
            // 
            // ValidationPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_alertLabel);
            this.Controls.Add(this.pb_state);
            this.Name = "ValidationPanel";
            this.Size = new System.Drawing.Size(421, 28);
            ((System.ComponentModel.ISupportInitialize)(this.pb_state)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_state;
        private System.Windows.Forms.Label lbl_alertLabel;
    }
}

namespace Pixelaria.Views.Controls
{
    partial class AssistedNumericUpDown
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
            this.nud_controlNud = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nud_controlNud)).BeginInit();
            this.SuspendLayout();
            // 
            // nud_controlNud
            // 
            this.nud_controlNud.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nud_controlNud.Location = new System.Drawing.Point(-1, -1);
            this.nud_controlNud.Name = "nud_controlNud";
            this.nud_controlNud.Size = new System.Drawing.Size(78, 20);
            this.nud_controlNud.TabIndex = 0;
            this.nud_controlNud.ValueChanged += new System.EventHandler(this.nud_controlNud_ValueChanged);
            // 
            // AssistedNumericUpDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.nud_controlNud);
            this.Name = "AssistedNumericUpDown";
            this.Size = new System.Drawing.Size(76, 32);
            ((System.ComponentModel.ISupportInitialize)(this.nud_controlNud)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nud_controlNud;
    }
}
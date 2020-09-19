namespace Pixelaria.Views.Controls
{
    partial class TimelineControl
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
            this.sc_container = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.sc_container)).BeginInit();
            this.sc_container.SuspendLayout();
            this.SuspendLayout();
            // 
            // sc_container
            // 
            this.sc_container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sc_container.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.sc_container.Location = new System.Drawing.Point(0, 0);
            this.sc_container.Name = "sc_container";
            this.sc_container.Panel1MinSize = 50;
            this.sc_container.Size = new System.Drawing.Size(150, 100);
            this.sc_container.TabIndex = 0;
            ((System.ComponentModel.ISupportInitialize)(this.sc_container)).EndInit();
            this.sc_container.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer sc_container;
    }
}

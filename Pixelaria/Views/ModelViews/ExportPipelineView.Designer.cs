namespace Pixelaria.Views.ModelViews
{
    partial class ExportPipelineView
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.exportPipelineRenderer1 = new Pixelaria.Views.ModelViews.ExportPipelineRenderer();
            this.SuspendLayout();
            // 
            // exportPipelineRenderer1
            // 
            this.exportPipelineRenderer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exportPipelineRenderer1.Location = new System.Drawing.Point(12, 12);
            this.exportPipelineRenderer1.Name = "exportPipelineRenderer1";
            this.exportPipelineRenderer1.Size = new System.Drawing.Size(1157, 687);
            this.exportPipelineRenderer1.TabIndex = 0;
            this.exportPipelineRenderer1.Text = "exportPipelineRenderer1";
            // 
            // ExportPipelineView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1181, 711);
            this.Controls.Add(this.exportPipelineRenderer1);
            this.DoubleBuffered = true;
            this.Name = "ExportPipelineView";
            this.Text = "ExportPipelineView";
            this.ResumeLayout(false);

        }

        #endregion

        private ExportPipelineRenderer exportPipelineRenderer1;
    }
}
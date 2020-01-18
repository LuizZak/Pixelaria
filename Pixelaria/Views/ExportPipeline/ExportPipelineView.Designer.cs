namespace Pixelaria.Views.ExportPipeline
{
    partial class ExportPipelineView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Pixelaria.Views.ExportPipeline.PipelineView.DefaultPipelineNodeViewLayout defaultPipelineNodeViewSizer1 = new Pixelaria.Views.ExportPipeline.PipelineView.DefaultPipelineNodeViewLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportPipelineView));
            this.exportPipelineControl = new Pixelaria.Views.ExportPipeline.ExportPipelineControl();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tab_open = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.tsb_sortSelected = new System.Windows.Forms.ToolStripButton();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // exportPipelineControl
            // 
            this.exportPipelineControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportPipelineControl.Location = new System.Drawing.Point(0, 25);
            this.exportPipelineControl.Name = "exportPipelineControl";
            this.exportPipelineControl.PipelineNodeViewLayout = defaultPipelineNodeViewSizer1;
            this.exportPipelineControl.Size = new System.Drawing.Size(1181, 686);
            this.exportPipelineControl.TabIndex = 0;
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tab_open,
            this.toolStripButton1,
            this.tsb_sortSelected});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1181, 25);
            this.toolStrip.TabIndex = 1;
            // 
            // tab_open
            // 
            this.tab_open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tab_open.Image = global::Pixelaria.Properties.Resources.document_open;
            this.tab_open.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tab_open.Name = "tab_open";
            this.tab_open.Size = new System.Drawing.Size(23, 22);
            this.tab_open.Text = "toolStripButton2";
            this.tab_open.Click += new System.EventHandler(this.tab_open_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::Pixelaria.Properties.Resources.anim_new_icon;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Add Animation";
            // 
            // tsb_sortSelected
            // 
            this.tsb_sortSelected.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsb_sortSelected.Image = ((System.Drawing.Image)(resources.GetObject("tsb_sortSelected.Image")));
            this.tsb_sortSelected.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_sortSelected.Name = "tsb_sortSelected";
            this.tsb_sortSelected.Size = new System.Drawing.Size(79, 22);
            this.tsb_sortSelected.Text = "&Sort Selected";
            this.tsb_sortSelected.Click += new System.EventHandler(this.tsb_sortSelected_Click);
            // 
            // ExportPipelineView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1181, 711);
            this.Controls.Add(this.exportPipelineControl);
            this.Controls.Add(this.toolStrip);
            this.DoubleBuffered = true;
            this.Name = "ExportPipelineView";
            this.Text = "ExportPipelineView";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ExportPipelineControl exportPipelineControl;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton tsb_sortSelected;
        private System.Windows.Forms.ToolStripButton tab_open;
    }
}
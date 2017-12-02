namespace Pixelaria.Views.MiscViews
{
    partial class BundleExportProgressView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BundleExportProgressView));
            this.pb_progress = new System.Windows.Forms.ProgressBar();
            this.lbl_progress = new System.Windows.Forms.Label();
            this.btn_ok = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tv_sheets = new Pixelaria.Views.MiscViews.BundleExportProgressView.BufferedTreeView();
            this.il_treeView = new System.Windows.Forms.ImageList(this.components);
            this.pb_stageProgress = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_elapsed = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pb_progress
            // 
            this.pb_progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pb_progress.Location = new System.Drawing.Point(9, 32);
            this.pb_progress.Name = "pb_progress";
            this.pb_progress.Size = new System.Drawing.Size(396, 15);
            this.pb_progress.TabIndex = 0;
            // 
            // lbl_progress
            // 
            this.lbl_progress.AutoSize = true;
            this.lbl_progress.Location = new System.Drawing.Point(6, 579);
            this.lbl_progress.Name = "lbl_progress";
            this.lbl_progress.Size = new System.Drawing.Size(87, 13);
            this.lbl_progress.TabIndex = 2;
            this.lbl_progress.Text = "Task Description";
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.Location = new System.Drawing.Point(349, 613);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 23);
            this.btn_ok.TabIndex = 3;
            this.btn_ok.Text = "Ok";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lbl_elapsed);
            this.groupBox1.Controls.Add(this.tv_sheets);
            this.groupBox1.Controls.Add(this.pb_stageProgress);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lbl_progress);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.pb_progress);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(412, 595);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Export Progress";
            // 
            // tv_sheets
            // 
            this.tv_sheets.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.tv_sheets.ImageIndex = 0;
            this.tv_sheets.ImageList = this.il_treeView;
            this.tv_sheets.Location = new System.Drawing.Point(9, 53);
            this.tv_sheets.Name = "tv_sheets";
            this.tv_sheets.SelectedImageIndex = 0;
            this.tv_sheets.Size = new System.Drawing.Size(396, 489);
            this.tv_sheets.TabIndex = 5;
            this.tv_sheets.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.tv_sheets_DrawNode);
            // 
            // il_treeView
            // 
            this.il_treeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("il_treeView.ImageStream")));
            this.il_treeView.TransparentColor = System.Drawing.Color.Transparent;
            this.il_treeView.Images.SetKeyName(0, "sheet_icon.png");
            // 
            // pb_stageProgress
            // 
            this.pb_stageProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pb_stageProgress.Location = new System.Drawing.Point(9, 561);
            this.pb_stageProgress.Name = "pb_stageProgress";
            this.pb_stageProgress.Size = new System.Drawing.Size(396, 15);
            this.pb_stageProgress.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 545);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current Task:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Total:";
            // 
            // lbl_elapsed
            // 
            this.lbl_elapsed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_elapsed.AutoSize = true;
            this.lbl_elapsed.Location = new System.Drawing.Point(370, 16);
            this.lbl_elapsed.Name = "lbl_elapsed";
            this.lbl_elapsed.Size = new System.Drawing.Size(34, 13);
            this.lbl_elapsed.TabIndex = 6;
            this.lbl_elapsed.Text = "00:00";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(294, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Elapsed time:";
            // 
            // BundleExportProgressView
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 648);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_ok);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BundleExportProgressView";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Progress";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar pb_progress;
        private System.Windows.Forms.Label lbl_progress;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ProgressBar pb_stageProgress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private BufferedTreeView tv_sheets;
        private System.Windows.Forms.ImageList il_treeView;
        private System.Windows.Forms.Label lbl_elapsed;
        private System.Windows.Forms.Label label4;
    }
}
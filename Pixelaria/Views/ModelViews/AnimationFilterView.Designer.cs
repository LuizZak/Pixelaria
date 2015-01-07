namespace Pixelaria.Views.ModelViews
{
    partial class AnimationFilterView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationFilterView));
            this.pnl_bottom = new System.Windows.Forms.Panel();
            this.pnl_errorPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_error = new System.Windows.Forms.Label();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tc_timeline = new Pixelaria.Views.Controls.TimelineControl();
            this.fs_filters = new Pixelaria.Views.Controls.FilterSelector();
            this.pnl_bottom.SuspendLayout();
            this.pnl_errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pnl_bottom
            // 
            this.pnl_bottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_bottom.Controls.Add(this.pnl_errorPanel);
            this.pnl_bottom.Controls.Add(this.btn_cancel);
            this.pnl_bottom.Controls.Add(this.btn_ok);
            this.pnl_bottom.Location = new System.Drawing.Point(12, 621);
            this.pnl_bottom.Name = "pnl_bottom";
            this.pnl_bottom.Size = new System.Drawing.Size(624, 35);
            this.pnl_bottom.TabIndex = 31;
            // 
            // pnl_errorPanel
            // 
            this.pnl_errorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_errorPanel.Controls.Add(this.pictureBox1);
            this.pnl_errorPanel.Controls.Add(this.lbl_error);
            this.pnl_errorPanel.Location = new System.Drawing.Point(3, 3);
            this.pnl_errorPanel.Name = "pnl_errorPanel";
            this.pnl_errorPanel.Size = new System.Drawing.Size(457, 29);
            this.pnl_errorPanel.TabIndex = 29;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(5, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(22, 22);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // lbl_error
            // 
            this.lbl_error.AutoSize = true;
            this.lbl_error.Location = new System.Drawing.Point(29, 8);
            this.lbl_error.Name = "lbl_error";
            this.lbl_error.Size = new System.Drawing.Size(242, 13);
            this.lbl_error.TabIndex = 9;
            this.lbl_error.Text = "The project folder path is invalid or does not exists";
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Location = new System.Drawing.Point(546, 3);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 29);
            this.btn_cancel.TabIndex = 27;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Enabled = false;
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(466, 3);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 29);
            this.btn_ok.TabIndex = 28;
            this.btn_ok.Text = "&OK";
            this.btn_ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 559);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(248, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "Preview frame and select range to modify with filter:";
            // 
            // tc_timeline
            // 
            this.tc_timeline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tc_timeline.BehaviorType = Pixelaria.Views.Controls.TimelineBehaviorType.TimelineWithRange;
            this.tc_timeline.CurrentFrame = 1;
            this.tc_timeline.DisableFrameSelectionOutOfRange = false;
            this.tc_timeline.FrameDisplayType = Pixelaria.Views.Controls.TimelineFrameDisplayType.FrameNumber;
            this.tc_timeline.Location = new System.Drawing.Point(12, 575);
            this.tc_timeline.Maximum = 1;
            this.tc_timeline.Minimum = 0;
            this.tc_timeline.Name = "tc_timeline";
            this.tc_timeline.Range = new System.Drawing.Point(0, 1);
            this.tc_timeline.ScrollScaleWidth = 1D;
            this.tc_timeline.ScrollX = 0D;
            this.tc_timeline.Size = new System.Drawing.Size(624, 40);
            this.tc_timeline.TabIndex = 32;
            this.tc_timeline.Text = "timelineControl1";
            this.tc_timeline.FrameChanged += new Pixelaria.Views.Controls.TimelineControl.FrameChangedEventHandler(this.tc_timeline_FrameChanged);
            // 
            // fs_filters
            // 
            this.fs_filters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fs_filters.Location = new System.Drawing.Point(12, 12);
            this.fs_filters.Name = "fs_filters";
            this.fs_filters.Size = new System.Drawing.Size(624, 537);
            this.fs_filters.TabIndex = 0;
            // 
            // AnimationFilterView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 668);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tc_timeline);
            this.Controls.Add(this.pnl_bottom);
            this.Controls.Add(this.fs_filters);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(663, 3000);
            this.MinimumSize = new System.Drawing.Size(663, 706);
            this.Name = "AnimationFilterView";
            this.Text = "Filter";
            this.pnl_bottom.ResumeLayout(false);
            this.pnl_errorPanel.ResumeLayout(false);
            this.pnl_errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.FilterSelector fs_filters;
        private System.Windows.Forms.Panel pnl_bottom;
        private System.Windows.Forms.Panel pnl_errorPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbl_error;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_ok;
        private Controls.TimelineControl tc_timeline;
        private System.Windows.Forms.Label label1;


    }
}
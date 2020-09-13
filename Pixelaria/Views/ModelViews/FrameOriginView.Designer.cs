namespace Pixelaria.Views.ModelViews
{
    partial class FrameOriginView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrameOriginView));
            this.btn_ok = new System.Windows.Forms.Button();
            this.timelineControl = new Pixelaria.Views.Controls.TimelineControl();
            this.zpb_framePreview = new Pixelaria.Views.ModelViews.FrameOriginView.FrameOriginEditImageBox();
            this.btn_cancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_framePreview)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(856, 566);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 29);
            this.btn_ok.TabIndex = 28;
            this.btn_ok.Text = "&OK";
            this.btn_ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // timelineControl
            // 
            this.timelineControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timelineControl.Location = new System.Drawing.Point(12, 522);
            this.timelineControl.Name = "timelineControl";
            this.timelineControl.Size = new System.Drawing.Size(1000, 38);
            this.timelineControl.TabIndex = 31;
            this.timelineControl.Text = "timelineControl";
            // 
            // zpb_framePreview
            // 
            this.zpb_framePreview.AllowScrollbars = false;
            this.zpb_framePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zpb_framePreview.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_framePreview.ClipBackgroundToImage = true;
            this.zpb_framePreview.HorizontalScrollValue = 0;
            this.zpb_framePreview.Location = new System.Drawing.Point(12, 12);
            this.zpb_framePreview.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_framePreview.MaximumZoom")));
            this.zpb_framePreview.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_framePreview.MinimumZoom")));
            this.zpb_framePreview.Name = "zpb_framePreview";
            this.zpb_framePreview.Size = new System.Drawing.Size(1000, 504);
            this.zpb_framePreview.TabIndex = 0;
            this.zpb_framePreview.TabStop = false;
            this.zpb_framePreview.VerticalScrollValue = 0;
            this.zpb_framePreview.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_framePreview.Zoom")));
            this.zpb_framePreview.ZoomFactor = 1.414214F;
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Location = new System.Drawing.Point(937, 566);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 29);
            this.btn_cancel.TabIndex = 32;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // FrameOriginView
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(1024, 607);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.timelineControl);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.zpb_framePreview);
            this.Name = "FrameOriginView";
            this.Text = "Frame Origin";
            ((System.ComponentModel.ISupportInitialize)(this.zpb_framePreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FrameOriginEditImageBox zpb_framePreview;
        private System.Windows.Forms.Button btn_ok;
        private Controls.TimelineControl timelineControl;
        private System.Windows.Forms.Button btn_cancel;
    }
}
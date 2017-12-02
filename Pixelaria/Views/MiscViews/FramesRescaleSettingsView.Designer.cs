namespace Pixelaria.Views.MiscViews
{
    partial class FramesRescaleSettingsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FramesRescaleSettingsView));
            this.lbl_message = new System.Windows.Forms.Label();
            this.gb_animationSize = new System.Windows.Forms.GroupBox();
            this.rb_animSize_useNewSize = new System.Windows.Forms.RadioButton();
            this.rb_animSize_useMaximumSize = new System.Windows.Forms.RadioButton();
            this.rb_animSize_keepOriginal = new System.Windows.Forms.RadioButton();
            this.gb_frameScaling = new System.Windows.Forms.GroupBox();
            this.rb_frameScaling_zoom = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_stretch = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_placeAtCenter = new System.Windows.Forms.RadioButton();
            this.rb_frameScaling_placeAtTopLeft = new System.Windows.Forms.RadioButton();
            this.gb_drawingMode = new System.Windows.Forms.GroupBox();
            this.rb_drawingMode_highQuality = new System.Windows.Forms.RadioButton();
            this.rb_drawingMode_lowQuality = new System.Windows.Forms.RadioButton();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.gb_animationSize.SuspendLayout();
            this.gb_frameScaling.SuspendLayout();
            this.gb_drawingMode.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_message
            // 
            resources.ApplyResources(this.lbl_message, "lbl_message");
            this.lbl_message.Name = "lbl_message";
            // 
            // gb_animationSize
            // 
            resources.ApplyResources(this.gb_animationSize, "gb_animationSize");
            this.gb_animationSize.Controls.Add(this.rb_animSize_useNewSize);
            this.gb_animationSize.Controls.Add(this.rb_animSize_useMaximumSize);
            this.gb_animationSize.Controls.Add(this.rb_animSize_keepOriginal);
            this.gb_animationSize.Name = "gb_animationSize";
            this.gb_animationSize.TabStop = false;
            // 
            // rb_animSize_useNewSize
            // 
            resources.ApplyResources(this.rb_animSize_useNewSize, "rb_animSize_useNewSize");
            this.rb_animSize_useNewSize.Name = "rb_animSize_useNewSize";
            this.rb_animSize_useNewSize.TabStop = true;
            this.rb_animSize_useNewSize.UseVisualStyleBackColor = true;
            // 
            // rb_animSize_useMaximumSize
            // 
            resources.ApplyResources(this.rb_animSize_useMaximumSize, "rb_animSize_useMaximumSize");
            this.rb_animSize_useMaximumSize.Name = "rb_animSize_useMaximumSize";
            this.rb_animSize_useMaximumSize.UseVisualStyleBackColor = true;
            // 
            // rb_animSize_keepOriginal
            // 
            resources.ApplyResources(this.rb_animSize_keepOriginal, "rb_animSize_keepOriginal");
            this.rb_animSize_keepOriginal.Checked = true;
            this.rb_animSize_keepOriginal.Name = "rb_animSize_keepOriginal";
            this.rb_animSize_keepOriginal.TabStop = true;
            this.rb_animSize_keepOriginal.UseVisualStyleBackColor = true;
            // 
            // gb_frameScaling
            // 
            resources.ApplyResources(this.gb_frameScaling, "gb_frameScaling");
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_zoom);
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_stretch);
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_placeAtCenter);
            this.gb_frameScaling.Controls.Add(this.rb_frameScaling_placeAtTopLeft);
            this.gb_frameScaling.Name = "gb_frameScaling";
            this.gb_frameScaling.TabStop = false;
            // 
            // rb_frameScaling_zoom
            // 
            resources.ApplyResources(this.rb_frameScaling_zoom, "rb_frameScaling_zoom");
            this.rb_frameScaling_zoom.Name = "rb_frameScaling_zoom";
            this.rb_frameScaling_zoom.UseVisualStyleBackColor = true;
            // 
            // rb_frameScaling_stretch
            // 
            resources.ApplyResources(this.rb_frameScaling_stretch, "rb_frameScaling_stretch");
            this.rb_frameScaling_stretch.Name = "rb_frameScaling_stretch";
            this.rb_frameScaling_stretch.UseVisualStyleBackColor = true;
            // 
            // rb_frameScaling_placeAtCenter
            // 
            resources.ApplyResources(this.rb_frameScaling_placeAtCenter, "rb_frameScaling_placeAtCenter");
            this.rb_frameScaling_placeAtCenter.Name = "rb_frameScaling_placeAtCenter";
            this.rb_frameScaling_placeAtCenter.UseVisualStyleBackColor = true;
            // 
            // rb_frameScaling_placeAtTopLeft
            // 
            resources.ApplyResources(this.rb_frameScaling_placeAtTopLeft, "rb_frameScaling_placeAtTopLeft");
            this.rb_frameScaling_placeAtTopLeft.Checked = true;
            this.rb_frameScaling_placeAtTopLeft.Name = "rb_frameScaling_placeAtTopLeft";
            this.rb_frameScaling_placeAtTopLeft.TabStop = true;
            this.rb_frameScaling_placeAtTopLeft.UseVisualStyleBackColor = true;
            // 
            // gb_drawingMode
            // 
            this.gb_drawingMode.Controls.Add(this.rb_drawingMode_highQuality);
            this.gb_drawingMode.Controls.Add(this.rb_drawingMode_lowQuality);
            resources.ApplyResources(this.gb_drawingMode, "gb_drawingMode");
            this.gb_drawingMode.Name = "gb_drawingMode";
            this.gb_drawingMode.TabStop = false;
            // 
            // rb_drawingMode_highQuality
            // 
            resources.ApplyResources(this.rb_drawingMode_highQuality, "rb_drawingMode_highQuality");
            this.rb_drawingMode_highQuality.Name = "rb_drawingMode_highQuality";
            this.rb_drawingMode_highQuality.UseVisualStyleBackColor = true;
            // 
            // rb_drawingMode_lowQuality
            // 
            resources.ApplyResources(this.rb_drawingMode_lowQuality, "rb_drawingMode_lowQuality");
            this.rb_drawingMode_lowQuality.Checked = true;
            this.rb_drawingMode_lowQuality.Name = "rb_drawingMode_lowQuality";
            this.rb_drawingMode_lowQuality.TabStop = true;
            this.rb_drawingMode_lowQuality.UseVisualStyleBackColor = true;
            // 
            // btn_ok
            // 
            resources.ApplyResources(this.btn_ok, "btn_ok");
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            resources.ApplyResources(this.btn_cancel, "btn_cancel");
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.gb_animationSize);
            this.flowLayoutPanel1.Controls.Add(this.gb_frameScaling);
            this.flowLayoutPanel1.Controls.Add(this.gb_drawingMode);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // FramesRescaleSettingsView
            // 
            this.AcceptButton = this.btn_ok;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.lbl_message);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FramesRescaleSettingsView";
            this.gb_animationSize.ResumeLayout(false);
            this.gb_animationSize.PerformLayout();
            this.gb_frameScaling.ResumeLayout(false);
            this.gb_frameScaling.PerformLayout();
            this.gb_drawingMode.ResumeLayout(false);
            this.gb_drawingMode.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.GroupBox gb_animationSize;
        private System.Windows.Forms.RadioButton rb_animSize_useMaximumSize;
        private System.Windows.Forms.RadioButton rb_animSize_keepOriginal;
        private System.Windows.Forms.GroupBox gb_frameScaling;
        private System.Windows.Forms.RadioButton rb_frameScaling_placeAtTopLeft;
        private System.Windows.Forms.RadioButton rb_frameScaling_placeAtCenter;
        private System.Windows.Forms.RadioButton rb_frameScaling_stretch;
        private System.Windows.Forms.GroupBox gb_drawingMode;
        private System.Windows.Forms.RadioButton rb_drawingMode_highQuality;
        private System.Windows.Forms.RadioButton rb_drawingMode_lowQuality;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.RadioButton rb_animSize_useNewSize;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.RadioButton rb_frameScaling_zoom;
    }
}
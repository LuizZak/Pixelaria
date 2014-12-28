namespace Pixelaria.Views.Controls.LayerControls
{
    partial class LayerControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerControl));
            this.btn_visible = new System.Windows.Forms.Button();
            this.btn_remove = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.lbl_layerName = new System.Windows.Forms.Label();
            this.pb_layerImage = new Pixelaria.Views.Controls.ZoomablePictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pb_layerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_visible
            // 
            this.btn_visible.FlatAppearance.BorderSize = 0;
            this.btn_visible.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.btn_visible.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_visible.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_visible.Image = global::Pixelaria.Properties.Resources.filter_enable_icon;
            this.btn_visible.Location = new System.Drawing.Point(3, 19);
            this.btn_visible.Name = "btn_visible";
            this.btn_visible.Size = new System.Drawing.Size(18, 18);
            this.btn_visible.TabIndex = 4;
            this.btn_visible.UseVisualStyleBackColor = true;
            // 
            // btn_remove
            // 
            this.btn_remove.FlatAppearance.BorderSize = 0;
            this.btn_remove.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.btn_remove.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_remove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_remove.Image = global::Pixelaria.Properties.Resources.edit_copy;
            this.btn_remove.Location = new System.Drawing.Point(3, 61);
            this.btn_remove.Name = "btn_remove";
            this.btn_remove.Size = new System.Drawing.Size(18, 18);
            this.btn_remove.TabIndex = 3;
            this.btn_remove.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Image = global::Pixelaria.Properties.Resources.padlock_open;
            this.button1.Location = new System.Drawing.Point(3, 40);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(18, 18);
            this.button1.TabIndex = 5;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.button2.Location = new System.Drawing.Point(3, 82);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(18, 18);
            this.button2.TabIndex = 6;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // lbl_layerName
            // 
            this.lbl_layerName.Location = new System.Drawing.Point(24, 3);
            this.lbl_layerName.Name = "lbl_layerName";
            this.lbl_layerName.Size = new System.Drawing.Size(96, 13);
            this.lbl_layerName.TabIndex = 8;
            this.lbl_layerName.Text = "Layer 1";
            this.lbl_layerName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pb_layerImage
            // 
            this.pb_layerImage.AllowScrollbars = false;
            this.pb_layerImage.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.pb_layerImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pb_layerImage.ClipBackgroundToImage = true;
            this.pb_layerImage.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pb_layerImage.Location = new System.Drawing.Point(24, 19);
            this.pb_layerImage.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("pb_layerImage.MaximumZoom")));
            this.pb_layerImage.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("pb_layerImage.MinimumZoom")));
            this.pb_layerImage.Name = "pb_layerImage";
            this.pb_layerImage.Size = new System.Drawing.Size(96, 81);
            this.pb_layerImage.TabIndex = 7;
            this.pb_layerImage.TabStop = false;
            this.pb_layerImage.Zoom = ((System.Drawing.PointF)(resources.GetObject("pb_layerImage.Zoom")));
            this.pb_layerImage.ZoomFactor = 1.414214F;
            // 
            // LayerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_layerName);
            this.Controls.Add(this.pb_layerImage);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_visible);
            this.Controls.Add(this.btn_remove);
            this.Name = "LayerControl";
            this.Size = new System.Drawing.Size(125, 100);
            ((System.ComponentModel.ISupportInitialize)(this.pb_layerImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_visible;
        private System.Windows.Forms.Button btn_remove;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private ZoomablePictureBox pb_layerImage;
        private System.Windows.Forms.Label lbl_layerName;
    }
}

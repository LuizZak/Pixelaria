namespace Pixelaria.Views.Controls.LayerControl
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
            this.btn_enable = new System.Windows.Forms.Button();
            this.btn_remove = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pb_layerImage = new System.Windows.Forms.PictureBox();
            this.lbl_layerName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pb_layerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_enable
            // 
            this.btn_enable.FlatAppearance.BorderSize = 0;
            this.btn_enable.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.btn_enable.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_enable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_enable.Image = global::Pixelaria.Properties.Resources.filter_enable_icon;
            this.btn_enable.Location = new System.Drawing.Point(3, 19);
            this.btn_enable.Name = "btn_enable";
            this.btn_enable.Size = new System.Drawing.Size(18, 18);
            this.btn_enable.TabIndex = 4;
            this.btn_enable.UseVisualStyleBackColor = true;
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
            // pb_layerImage
            // 
            this.pb_layerImage.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.pb_layerImage.Location = new System.Drawing.Point(24, 19);
            this.pb_layerImage.Name = "pb_layerImage";
            this.pb_layerImage.Size = new System.Drawing.Size(96, 81);
            this.pb_layerImage.TabIndex = 7;
            this.pb_layerImage.TabStop = false;
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
            // LayerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_layerName);
            this.Controls.Add(this.pb_layerImage);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_enable);
            this.Controls.Add(this.btn_remove);
            this.Name = "LayerControl";
            this.Size = new System.Drawing.Size(120, 100);
            ((System.ComponentModel.ISupportInitialize)(this.pb_layerImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_enable;
        private System.Windows.Forms.Button btn_remove;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pb_layerImage;
        private System.Windows.Forms.Label lbl_layerName;
    }
}

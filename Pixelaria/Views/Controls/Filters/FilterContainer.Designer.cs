namespace Pixelaria.Views.Controls.Filters
{
    partial class FilterContainer
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
            this.pnl_container = new System.Windows.Forms.Panel();
            this.btn_remove = new System.Windows.Forms.Button();
            this.lbl_filterName = new System.Windows.Forms.Label();
            this.btn_enable = new System.Windows.Forms.Button();
            this.btn_collapse = new System.Windows.Forms.Button();
            this.pb_filterIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pb_filterIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pnl_container
            // 
            this.pnl_container.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_container.Location = new System.Drawing.Point(20, 20);
            this.pnl_container.Name = "pnl_container";
            this.pnl_container.Size = new System.Drawing.Size(776, 143);
            this.pnl_container.TabIndex = 0;
            // 
            // btn_remove
            // 
            this.btn_remove.FlatAppearance.BorderSize = 0;
            this.btn_remove.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.btn_remove.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_remove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_remove.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_remove.Location = new System.Drawing.Point(2, 20);
            this.btn_remove.Name = "btn_remove";
            this.btn_remove.Size = new System.Drawing.Size(15, 15);
            this.btn_remove.TabIndex = 0;
            this.btn_remove.UseVisualStyleBackColor = true;
            this.btn_remove.Click += new System.EventHandler(this.btn_remove_Click);
            // 
            // lbl_filterName
            // 
            this.lbl_filterName.AutoSize = true;
            this.lbl_filterName.Location = new System.Drawing.Point(60, 5);
            this.lbl_filterName.Name = "lbl_filterName";
            this.lbl_filterName.Size = new System.Drawing.Size(60, 13);
            this.lbl_filterName.TabIndex = 1;
            this.lbl_filterName.Text = "Filter Name";
            // 
            // btn_enable
            // 
            this.btn_enable.FlatAppearance.BorderSize = 0;
            this.btn_enable.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.btn_enable.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_enable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_enable.Image = global::Pixelaria.Properties.Resources.filter_enable_icon;
            this.btn_enable.Location = new System.Drawing.Point(2, 41);
            this.btn_enable.Name = "btn_enable";
            this.btn_enable.Size = new System.Drawing.Size(15, 15);
            this.btn_enable.TabIndex = 2;
            this.btn_enable.UseVisualStyleBackColor = true;
            this.btn_enable.Click += new System.EventHandler(this.btn_enable_Click);
            // 
            // btn_collapse
            // 
            this.btn_collapse.FlatAppearance.BorderSize = 0;
            this.btn_collapse.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ScrollBar;
            this.btn_collapse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_collapse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_collapse.Image = global::Pixelaria.Properties.Resources.minus_icon;
            this.btn_collapse.Location = new System.Drawing.Point(18, 3);
            this.btn_collapse.Name = "btn_collapse";
            this.btn_collapse.Size = new System.Drawing.Size(15, 15);
            this.btn_collapse.TabIndex = 3;
            this.btn_collapse.UseVisualStyleBackColor = true;
            this.btn_collapse.Click += new System.EventHandler(this.btn_collapse_Click);
            // 
            // pb_filterIcon
            // 
            this.pb_filterIcon.Location = new System.Drawing.Point(39, 3);
            this.pb_filterIcon.Name = "pb_filterIcon";
            this.pb_filterIcon.Size = new System.Drawing.Size(15, 15);
            this.pb_filterIcon.TabIndex = 4;
            this.pb_filterIcon.TabStop = false;
            // 
            // FilterContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pb_filterIcon);
            this.Controls.Add(this.btn_collapse);
            this.Controls.Add(this.btn_enable);
            this.Controls.Add(this.lbl_filterName);
            this.Controls.Add(this.btn_remove);
            this.Controls.Add(this.pnl_container);
            this.Name = "FilterContainer";
            this.Size = new System.Drawing.Size(796, 161);
            ((System.ComponentModel.ISupportInitialize)(this.pb_filterIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnl_container;
        private System.Windows.Forms.Button btn_remove;
        private System.Windows.Forms.Label lbl_filterName;
        private System.Windows.Forms.Button btn_enable;
        private System.Windows.Forms.Button btn_collapse;
        private System.Windows.Forms.PictureBox pb_filterIcon;
    }
}

namespace Pixelaria.Views.Controls
{
    partial class FilterSelector
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterSelector));
            this.btn_loadPreset = new System.Windows.Forms.Button();
            this.btn_deletePreset = new System.Windows.Forms.Button();
            this.btn_savePreset = new System.Windows.Forms.Button();
            this.cb_filterPresets = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gb_filterControlContainer = new System.Windows.Forms.GroupBox();
            this.btn_addFilter = new System.Windows.Forms.Button();
            this.pnl_container = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cms_filters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.zpb_preview = new Pixelaria.Views.Controls.ZoomablePictureBox();
            this.zpb_original = new Pixelaria.Views.Controls.ZoomablePictureBox();
            this.gb_filterControlContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_preview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_original)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_loadPreset
            // 
            this.btn_loadPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_loadPreset.Image = global::Pixelaria.Properties.Resources.folder_open;
            this.btn_loadPreset.Location = new System.Drawing.Point(384, 288);
            this.btn_loadPreset.Name = "btn_loadPreset";
            this.btn_loadPreset.Size = new System.Drawing.Size(75, 21);
            this.btn_loadPreset.TabIndex = 44;
            this.btn_loadPreset.Text = "Load";
            this.btn_loadPreset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_loadPreset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_loadPreset.UseVisualStyleBackColor = true;
            // 
            // btn_deletePreset
            // 
            this.btn_deletePreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_deletePreset.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_deletePreset.Location = new System.Drawing.Point(546, 289);
            this.btn_deletePreset.Name = "btn_deletePreset";
            this.btn_deletePreset.Size = new System.Drawing.Size(77, 21);
            this.btn_deletePreset.TabIndex = 43;
            this.btn_deletePreset.Text = "Delete";
            this.btn_deletePreset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_deletePreset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_deletePreset.UseVisualStyleBackColor = true;
            // 
            // btn_savePreset
            // 
            this.btn_savePreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_savePreset.Image = global::Pixelaria.Properties.Resources.save_icon;
            this.btn_savePreset.Location = new System.Drawing.Point(465, 288);
            this.btn_savePreset.Name = "btn_savePreset";
            this.btn_savePreset.Size = new System.Drawing.Size(75, 21);
            this.btn_savePreset.TabIndex = 42;
            this.btn_savePreset.Text = "Save";
            this.btn_savePreset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_savePreset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_savePreset.UseVisualStyleBackColor = true;
            // 
            // cb_filterPresets
            // 
            this.cb_filterPresets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_filterPresets.FormattingEnabled = true;
            this.cb_filterPresets.Location = new System.Drawing.Point(43, 289);
            this.cb_filterPresets.Name = "cb_filterPresets";
            this.cb_filterPresets.Size = new System.Drawing.Size(335, 21);
            this.cb_filterPresets.TabIndex = 41;
            this.cb_filterPresets.Text = "New Preset";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-3, 292);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Preset:";
            // 
            // gb_filterControlContainer
            // 
            this.gb_filterControlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_filterControlContainer.Controls.Add(this.btn_addFilter);
            this.gb_filterControlContainer.Controls.Add(this.pnl_container);
            this.gb_filterControlContainer.Location = new System.Drawing.Point(0, 316);
            this.gb_filterControlContainer.Name = "gb_filterControlContainer";
            this.gb_filterControlContainer.Size = new System.Drawing.Size(623, 260);
            this.gb_filterControlContainer.TabIndex = 39;
            this.gb_filterControlContainer.TabStop = false;
            this.gb_filterControlContainer.Text = "Filter Options";
            // 
            // btn_addFilter
            // 
            this.btn_addFilter.FlatAppearance.BorderSize = 0;
            this.btn_addFilter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_addFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_addFilter.Image = global::Pixelaria.Properties.Resources.action_add;
            this.btn_addFilter.Location = new System.Drawing.Point(6, 19);
            this.btn_addFilter.Name = "btn_addFilter";
            this.btn_addFilter.Size = new System.Drawing.Size(22, 22);
            this.btn_addFilter.TabIndex = 1;
            this.btn_addFilter.UseVisualStyleBackColor = true;
            // 
            // pnl_container
            // 
            this.pnl_container.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_container.AutoScroll = true;
            this.pnl_container.Location = new System.Drawing.Point(3, 47);
            this.pnl_container.Name = "pnl_container";
            this.pnl_container.Size = new System.Drawing.Size(617, 207);
            this.pnl_container.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Image = global::Pixelaria.Properties.Resources.go_next;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label1.Location = new System.Drawing.Point(289, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 36);
            this.label1.TabIndex = 37;
            this.label1.Text = "Preview";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // cms_filters
            // 
            this.cms_filters.Name = "cms_filters";
            this.cms_filters.Size = new System.Drawing.Size(61, 4);
            // 
            // zpb_preview
            // 
            this.zpb_preview.AllowScrollbars = false;
            this.zpb_preview.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_preview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.zpb_preview.ClipBackgroundToImage = true;
            this.zpb_preview.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.zpb_preview.Location = new System.Drawing.Point(340, 0);
            this.zpb_preview.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.MaximumZoom")));
            this.zpb_preview.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.MinimumZoom")));
            this.zpb_preview.Name = "zpb_preview";
            this.zpb_preview.Size = new System.Drawing.Size(283, 283);
            this.zpb_preview.TabIndex = 38;
            this.zpb_preview.TabStop = false;
            this.zpb_preview.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.Zoom")));
            this.zpb_preview.ZoomFactor = 1.414214F;
            // 
            // zpb_original
            // 
            this.zpb_original.AllowScrollbars = false;
            this.zpb_original.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_original.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.zpb_original.ClipBackgroundToImage = true;
            this.zpb_original.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.zpb_original.Location = new System.Drawing.Point(0, 0);
            this.zpb_original.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_original.MaximumZoom")));
            this.zpb_original.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_original.MinimumZoom")));
            this.zpb_original.Name = "zpb_original";
            this.zpb_original.Size = new System.Drawing.Size(283, 283);
            this.zpb_original.TabIndex = 36;
            this.zpb_original.TabStop = false;
            this.zpb_original.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_original.Zoom")));
            this.zpb_original.ZoomFactor = 1.414214F;
            // 
            // FilterSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btn_loadPreset);
            this.Controls.Add(this.btn_deletePreset);
            this.Controls.Add(this.btn_savePreset);
            this.Controls.Add(this.cb_filterPresets);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.gb_filterControlContainer);
            this.Controls.Add(this.zpb_preview);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.zpb_original);
            this.Name = "FilterSelector";
            this.Size = new System.Drawing.Size(624, 582);
            this.gb_filterControlContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.zpb_preview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_original)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_loadPreset;
        private System.Windows.Forms.Button btn_deletePreset;
        private System.Windows.Forms.Button btn_savePreset;
        private System.Windows.Forms.ComboBox cb_filterPresets;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox gb_filterControlContainer;
        private System.Windows.Forms.Button btn_addFilter;
        private System.Windows.Forms.FlowLayoutPanel pnl_container;
        private ZoomablePictureBox zpb_preview;
        private System.Windows.Forms.Label label1;
        private ZoomablePictureBox zpb_original;
        private System.Windows.Forms.ContextMenuStrip cms_filters;


    }
}

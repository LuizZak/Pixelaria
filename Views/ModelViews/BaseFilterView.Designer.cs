/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

namespace Pixelaria.Views.ModelViews
{
    partial class BaseFilterView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseFilterView));
            this.gb_filterControlContainer = new System.Windows.Forms.GroupBox();
            this.btn_addFilter = new System.Windows.Forms.Button();
            this.pnl_container = new System.Windows.Forms.FlowLayoutPanel();
            this.pnl_errorPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_error = new System.Windows.Forms.Label();
            this.pnl_bottom = new System.Windows.Forms.Panel();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_ok = new System.Windows.Forms.Button();
            this.cms_filters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.zpb_preview = new Pixelaria.Views.Controls.ZoomablePictureBox();
            this.zpb_original = new Pixelaria.Views.Controls.ZoomablePictureBox();
            this.gb_filterControlContainer.SuspendLayout();
            this.pnl_errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnl_bottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_preview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_original)).BeginInit();
            this.SuspendLayout();
            // 
            // gb_filterControlContainer
            // 
            this.gb_filterControlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_filterControlContainer.Controls.Add(this.btn_addFilter);
            this.gb_filterControlContainer.Controls.Add(this.pnl_container);
            this.gb_filterControlContainer.Location = new System.Drawing.Point(12, 301);
            this.gb_filterControlContainer.Name = "gb_filterControlContainer";
            this.gb_filterControlContainer.Size = new System.Drawing.Size(623, 160);
            this.gb_filterControlContainer.TabIndex = 26;
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
            this.btn_addFilter.Click += new System.EventHandler(this.btn_addFilter_Click);
            // 
            // pnl_container
            // 
            this.pnl_container.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_container.AutoScroll = true;
            this.pnl_container.Location = new System.Drawing.Point(3, 47);
            this.pnl_container.Name = "pnl_container";
            this.pnl_container.Size = new System.Drawing.Size(617, 107);
            this.pnl_container.TabIndex = 0;
            // 
            // pnl_errorPanel
            // 
            this.pnl_errorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_errorPanel.Controls.Add(this.pictureBox1);
            this.pnl_errorPanel.Controls.Add(this.lbl_error);
            this.pnl_errorPanel.Location = new System.Drawing.Point(3, 3);
            this.pnl_errorPanel.Name = "pnl_errorPanel";
            this.pnl_errorPanel.Size = new System.Drawing.Size(456, 29);
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
            // pnl_bottom
            // 
            this.pnl_bottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_bottom.Controls.Add(this.pnl_errorPanel);
            this.pnl_bottom.Controls.Add(this.btn_cancel);
            this.pnl_bottom.Controls.Add(this.btn_ok);
            this.pnl_bottom.Location = new System.Drawing.Point(12, 467);
            this.pnl_bottom.Name = "pnl_bottom";
            this.pnl_bottom.Size = new System.Drawing.Size(623, 35);
            this.pnl_bottom.TabIndex = 30;
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Location = new System.Drawing.Point(545, 3);
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
            this.btn_ok.Location = new System.Drawing.Point(465, 3);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 29);
            this.btn_ok.TabIndex = 28;
            this.btn_ok.Text = "&OK";
            this.btn_ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // cms_filters
            // 
            this.cms_filters.Name = "cms_filters";
            this.cms_filters.Size = new System.Drawing.Size(61, 4);
            // 
            // label1
            // 
            this.label1.Image = global::Pixelaria.Properties.Resources.go_next;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label1.Location = new System.Drawing.Point(301, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 36);
            this.label1.TabIndex = 1;
            this.label1.Text = "Preview";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // zpb_preview
            // 
            this.zpb_preview.AllowScrollbars = false;
            this.zpb_preview.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_preview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.zpb_preview.ClipBackgroundToImage = true;
            this.zpb_preview.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.zpb_preview.Location = new System.Drawing.Point(352, 12);
            this.zpb_preview.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.MaximumZoom")));
            this.zpb_preview.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.MinimumZoom")));
            this.zpb_preview.Name = "zpb_preview";
            this.zpb_preview.Size = new System.Drawing.Size(283, 283);
            this.zpb_preview.TabIndex = 2;
            this.zpb_preview.TabStop = false;
            this.zpb_preview.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_preview.Zoom")));
            this.zpb_preview.ZoomFactor = 1.414214F;
            this.zpb_preview.ZoomChanged += new Pixelaria.Views.Controls.ZoomablePictureBox.ZoomChangedEventHandler(this.zpb_preview_ZoomChanged);
            // 
            // zpb_original
            // 
            this.zpb_original.AllowScrollbars = false;
            this.zpb_original.BackgroundImage = global::Pixelaria.Properties.Resources.checkers_pattern;
            this.zpb_original.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.zpb_original.ClipBackgroundToImage = true;
            this.zpb_original.ImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.zpb_original.Location = new System.Drawing.Point(12, 12);
            this.zpb_original.MaximumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_original.MaximumZoom")));
            this.zpb_original.MinimumZoom = ((System.Drawing.PointF)(resources.GetObject("zpb_original.MinimumZoom")));
            this.zpb_original.Name = "zpb_original";
            this.zpb_original.Size = new System.Drawing.Size(283, 283);
            this.zpb_original.TabIndex = 0;
            this.zpb_original.TabStop = false;
            this.zpb_original.Zoom = ((System.Drawing.PointF)(resources.GetObject("zpb_original.Zoom")));
            this.zpb_original.ZoomFactor = 1.414214F;
            this.zpb_original.ZoomChanged += new Pixelaria.Views.Controls.ZoomablePictureBox.ZoomChangedEventHandler(this.zpb_original_ZoomChanged);
            // 
            // BaseFilterView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 514);
            this.Controls.Add(this.pnl_bottom);
            this.Controls.Add(this.gb_filterControlContainer);
            this.Controls.Add(this.zpb_preview);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.zpb_original);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BaseFilterView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter";
            this.gb_filterControlContainer.ResumeLayout(false);
            this.pnl_errorPanel.ResumeLayout(false);
            this.pnl_errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnl_bottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.zpb_preview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zpb_original)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ZoomablePictureBox zpb_original;
        private System.Windows.Forms.Label label1;
        private Controls.ZoomablePictureBox zpb_preview;
        private System.Windows.Forms.GroupBox gb_filterControlContainer;
        private System.Windows.Forms.Panel pnl_errorPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbl_error;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Panel pnl_bottom;
        private System.Windows.Forms.FlowLayoutPanel pnl_container;
        private System.Windows.Forms.Button btn_addFilter;
        private System.Windows.Forms.ContextMenuStrip cms_filters;
    }
}
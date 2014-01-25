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
    partial class AnimationPreviewPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationPreviewPanel));
            this.cb_playPreview = new System.Windows.Forms.CheckBox();
            this.tb_timeline = new System.Windows.Forms.TrackBar();
            this.nud_previewZoom = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.pnl_preview = new Pixelaria.Views.Controls.CPictureBox();
            this.tb_zoomTrack = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_currentFrame = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_frameCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tb_timeline)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_previewZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnl_preview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_zoomTrack)).BeginInit();
            this.SuspendLayout();
            // 
            // cb_playPreview
            // 
            this.cb_playPreview.AutoSize = true;
            this.cb_playPreview.Location = new System.Drawing.Point(6, 26);
            this.cb_playPreview.Name = "cb_playPreview";
            this.cb_playPreview.Size = new System.Drawing.Size(46, 17);
            this.cb_playPreview.TabIndex = 3;
            this.cb_playPreview.Text = "Play";
            this.cb_playPreview.UseVisualStyleBackColor = true;
            this.cb_playPreview.CheckedChanged += new System.EventHandler(this.cb_playPreview_CheckedChanged);
            // 
            // tb_timeline
            // 
            this.tb_timeline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_timeline.Location = new System.Drawing.Point(3, 62);
            this.tb_timeline.Maximum = 0;
            this.tb_timeline.Name = "tb_timeline";
            this.tb_timeline.Size = new System.Drawing.Size(197, 45);
            this.tb_timeline.TabIndex = 4;
            this.tb_timeline.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.tb_timeline.Scroll += new System.EventHandler(this.tb_timeline_Scroll);
            this.tb_timeline.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tb_timeline_MouseDown);
            this.tb_timeline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tb_timeline_MouseUp);
            // 
            // nud_previewZoom
            // 
            this.nud_previewZoom.DecimalPlaces = 2;
            this.nud_previewZoom.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.nud_previewZoom.Location = new System.Drawing.Point(46, 4);
            this.nud_previewZoom.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nud_previewZoom.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.nud_previewZoom.Name = "nud_previewZoom";
            this.nud_previewZoom.Size = new System.Drawing.Size(56, 20);
            this.nud_previewZoom.TabIndex = 1;
            this.nud_previewZoom.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_previewZoom.ValueChanged += new System.EventHandler(this.nud_previewZoom_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Zoom:";
            // 
            // pnl_preview
            // 
            this.pnl_preview.BackColor = System.Drawing.Color.White;
            this.pnl_preview.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnl_preview.BackgroundImage")));
            this.pnl_preview.ImageInterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            this.pnl_preview.ImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pnl_preview.Location = new System.Drawing.Point(3, 113);
            this.pnl_preview.Name = "pnl_preview";
            this.pnl_preview.Size = new System.Drawing.Size(197, 162);
            this.pnl_preview.TabIndex = 6;
            this.pnl_preview.TabStop = false;
            this.pnl_preview.DoubleClick += new System.EventHandler(this.pnl_preview_DoubleClick);
            // 
            // tb_zoomTrack
            // 
            this.tb_zoomTrack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_zoomTrack.Location = new System.Drawing.Point(108, 3);
            this.tb_zoomTrack.Name = "tb_zoomTrack";
            this.tb_zoomTrack.Size = new System.Drawing.Size(92, 45);
            this.tb_zoomTrack.TabIndex = 2;
            this.tb_zoomTrack.Scroll += new System.EventHandler(this.tb_zoomTrack_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Frame:";
            // 
            // lbl_currentFrame
            // 
            this.lbl_currentFrame.AutoSize = true;
            this.lbl_currentFrame.Location = new System.Drawing.Point(43, 46);
            this.lbl_currentFrame.Name = "lbl_currentFrame";
            this.lbl_currentFrame.Size = new System.Drawing.Size(13, 13);
            this.lbl_currentFrame.TabIndex = 13;
            this.lbl_currentFrame.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(83, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "/";
            // 
            // lbl_frameCount
            // 
            this.lbl_frameCount.AutoSize = true;
            this.lbl_frameCount.Location = new System.Drawing.Point(125, 46);
            this.lbl_frameCount.Name = "lbl_frameCount";
            this.lbl_frameCount.Size = new System.Drawing.Size(13, 13);
            this.lbl_frameCount.TabIndex = 15;
            this.lbl_frameCount.Text = "0";
            // 
            // AnimationPreviewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_frameCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbl_currentFrame);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_zoomTrack);
            this.Controls.Add(this.cb_playPreview);
            this.Controls.Add(this.tb_timeline);
            this.Controls.Add(this.nud_previewZoom);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pnl_preview);
            this.Name = "AnimationPreviewPanel";
            this.Size = new System.Drawing.Size(203, 584);
            ((System.ComponentModel.ISupportInitialize)(this.tb_timeline)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_previewZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnl_preview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_zoomTrack)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb_playPreview;
        private System.Windows.Forms.TrackBar tb_timeline;
        private System.Windows.Forms.NumericUpDown nud_previewZoom;
        private System.Windows.Forms.Label label2;
        private Pixelaria.Views.Controls.CPictureBox pnl_preview;
        private System.Windows.Forms.TrackBar tb_zoomTrack;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_currentFrame;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_frameCount;
    }
}

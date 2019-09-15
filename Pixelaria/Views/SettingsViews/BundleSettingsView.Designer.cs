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

namespace Pixelaria.Views.SettingsViews
{
    partial class BundleSettingsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BundleSettingsView));
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cb_exportMethod = new System.Windows.Forms.ComboBox();
            this.lbl_name = new System.Windows.Forms.Label();
            this.txt_bundleName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_browse = new System.Windows.Forms.Button();
            this.txt_exportPath = new System.Windows.Forms.TextBox();
            this.lbl_exportPath = new System.Windows.Forms.Label();
            this.lbl_exportMethod = new System.Windows.Forms.Label();
            this.pnl_alertPanel = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lbl_alertLabel = new System.Windows.Forms.Label();
            this.pnl_errorPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_error = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnl_alertPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.pnl_errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Enabled = false;
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(947, 193);
            this.btn_ok.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(112, 45);
            this.btn_ok.TabIndex = 10;
            this.btn_ok.Text = "&OK";
            this.btn_ok.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_ok.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Image = global::Pixelaria.Properties.Resources.action_delete;
            this.btn_cancel.Location = new System.Drawing.Point(1068, 193);
            this.btn_cancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(112, 45);
            this.btn_cancel.TabIndex = 9;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(18, 18);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(1163, 166);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bundle Information";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 122F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl_name, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txt_bundleName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl_exportPath, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl_exportMethod, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1155, 137);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cb_exportMethod);
            this.panel2.Location = new System.Drawing.Point(125, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1027, 44);
            this.panel2.TabIndex = 19;
            // 
            // cb_exportMethod
            // 
            this.cb_exportMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_exportMethod.Location = new System.Drawing.Point(3, 9);
            this.cb_exportMethod.Name = "cb_exportMethod";
            this.cb_exportMethod.Size = new System.Drawing.Size(1021, 28);
            this.cb_exportMethod.TabIndex = 0;
            // 
            // lbl_name
            // 
            this.lbl_name.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_name.AutoSize = true;
            this.lbl_name.Location = new System.Drawing.Point(4, 50);
            this.lbl_name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_name.Name = "lbl_name";
            this.lbl_name.Size = new System.Drawing.Size(114, 36);
            this.lbl_name.TabIndex = 0;
            this.lbl_name.Text = "Name:";
            this.lbl_name.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_bundleName
            // 
            this.txt_bundleName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_bundleName.Location = new System.Drawing.Point(126, 55);
            this.txt_bundleName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_bundleName.Name = "txt_bundleName";
            this.txt_bundleName.Size = new System.Drawing.Size(1025, 26);
            this.txt_bundleName.TabIndex = 12;
            this.txt_bundleName.TextChanged += new System.EventHandler(this.txt_bundleName_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_browse);
            this.panel1.Controls.Add(this.txt_exportPath);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(125, 89);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1027, 45);
            this.panel1.TabIndex = 16;
            // 
            // btn_browse
            // 
            this.btn_browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_browse.Image = global::Pixelaria.Properties.Resources.folder_open;
            this.btn_browse.Location = new System.Drawing.Point(909, 5);
            this.btn_browse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(117, 35);
            this.btn_browse.TabIndex = 15;
            this.btn_browse.Text = "Browse";
            this.btn_browse.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_browse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // txt_exportPath
            // 
            this.txt_exportPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_exportPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txt_exportPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txt_exportPath.Location = new System.Drawing.Point(4, 9);
            this.txt_exportPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_exportPath.Name = "txt_exportPath";
            this.txt_exportPath.Size = new System.Drawing.Size(897, 26);
            this.txt_exportPath.TabIndex = 14;
            this.txt_exportPath.TextChanged += new System.EventHandler(this.txt_exportPath_TextChanged);
            // 
            // lbl_exportPath
            // 
            this.lbl_exportPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_exportPath.AutoSize = true;
            this.lbl_exportPath.Location = new System.Drawing.Point(4, 86);
            this.lbl_exportPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_exportPath.Name = "lbl_exportPath";
            this.lbl_exportPath.Size = new System.Drawing.Size(114, 51);
            this.lbl_exportPath.TabIndex = 13;
            this.lbl_exportPath.Text = "Export Path:";
            this.lbl_exportPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_exportMethod
            // 
            this.lbl_exportMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_exportMethod.AutoSize = true;
            this.lbl_exportMethod.Location = new System.Drawing.Point(3, 0);
            this.lbl_exportMethod.Name = "lbl_exportMethod";
            this.lbl_exportMethod.Size = new System.Drawing.Size(116, 50);
            this.lbl_exportMethod.TabIndex = 17;
            this.lbl_exportMethod.Text = "Export Method";
            this.lbl_exportMethod.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnl_alertPanel
            // 
            this.pnl_alertPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_alertPanel.Controls.Add(this.pictureBox2);
            this.pnl_alertPanel.Controls.Add(this.lbl_alertLabel);
            this.pnl_alertPanel.Location = new System.Drawing.Point(18, 193);
            this.pnl_alertPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnl_alertPanel.Name = "pnl_alertPanel";
            this.pnl_alertPanel.Size = new System.Drawing.Size(920, 45);
            this.pnl_alertPanel.TabIndex = 18;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(8, 6);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(33, 34);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // lbl_alertLabel
            // 
            this.lbl_alertLabel.AutoSize = true;
            this.lbl_alertLabel.Location = new System.Drawing.Point(44, 12);
            this.lbl_alertLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_alertLabel.Name = "lbl_alertLabel";
            this.lbl_alertLabel.Size = new System.Drawing.Size(358, 20);
            this.lbl_alertLabel.TabIndex = 9;
            this.lbl_alertLabel.Text = "The project folder path is invalid or does not exists";
            // 
            // pnl_errorPanel
            // 
            this.pnl_errorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_errorPanel.Controls.Add(this.pictureBox1);
            this.pnl_errorPanel.Controls.Add(this.lbl_error);
            this.pnl_errorPanel.Location = new System.Drawing.Point(18, 193);
            this.pnl_errorPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnl_errorPanel.Name = "pnl_errorPanel";
            this.pnl_errorPanel.Size = new System.Drawing.Size(920, 45);
            this.pnl_errorPanel.TabIndex = 17;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 6);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(33, 34);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // lbl_error
            // 
            this.lbl_error.AutoSize = true;
            this.lbl_error.Location = new System.Drawing.Point(44, 12);
            this.lbl_error.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_error.Name = "lbl_error";
            this.lbl_error.Size = new System.Drawing.Size(358, 20);
            this.lbl_error.TabIndex = 9;
            this.lbl_error.Text = "The project folder path is invalid or does not exists";
            // 
            // BundleSettingsView
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(1199, 253);
            this.Controls.Add(this.pnl_alertPanel);
            this.Controls.Add(this.pnl_errorPanel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_cancel);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximumSize = new System.Drawing.Size(2989, 309);
            this.MinimumSize = new System.Drawing.Size(856, 309);
            this.Name = "BundleSettingsView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bundle Settings";
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnl_alertPanel.ResumeLayout(false);
            this.pnl_alertPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.pnl_errorPanel.ResumeLayout(false);
            this.pnl_errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lbl_name;
        private System.Windows.Forms.TextBox txt_bundleName;
        private System.Windows.Forms.Panel pnl_alertPanel;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lbl_alertLabel;
        private System.Windows.Forms.Panel pnl_errorPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbl_error;
        private System.Windows.Forms.TextBox txt_exportPath;
        private System.Windows.Forms.Label lbl_exportPath;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox cb_exportMethod;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbl_exportMethod;
    }
}
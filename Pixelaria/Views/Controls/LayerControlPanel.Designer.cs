using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls
{
    public partial class LayerControlPanel
    {
        private LabeledPanel labeledPanel1;
        private Panel panel1;
        private Panel pnl_container;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_createNewLayer = new System.Windows.Forms.Button();
            this.pnl_container = new System.Windows.Forms.Panel();
            this.labeledPanel1 = new Pixelaria.Views.Controls.LabeledPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.btn_createNewLayer);
            this.panel1.Location = new System.Drawing.Point(0, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(125, 26);
            this.panel1.TabIndex = 1;
            // 
            // btn_createNewLayer
            // 
            this.btn_createNewLayer.FlatAppearance.BorderSize = 0;
            this.btn_createNewLayer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_createNewLayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_createNewLayer.Image = global::Pixelaria.Properties.Resources.action_add;
            this.btn_createNewLayer.Location = new System.Drawing.Point(3, 3);
            this.btn_createNewLayer.Name = "btn_createNewLayer";
            this.btn_createNewLayer.Size = new System.Drawing.Size(20, 20);
            this.btn_createNewLayer.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btn_createNewLayer, "Add New Layer");
            this.btn_createNewLayer.UseVisualStyleBackColor = true;
            this.btn_createNewLayer.Click += new System.EventHandler(this.btn_createNewLayer_Click);
            // 
            // pnl_container
            // 
            this.pnl_container.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_container.Location = new System.Drawing.Point(0, 49);
            this.pnl_container.Name = "pnl_container";
            this.pnl_container.Size = new System.Drawing.Size(125, 283);
            this.pnl_container.TabIndex = 3;
            // 
            // labeledPanel1
            // 
            this.labeledPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.labeledPanel1.Location = new System.Drawing.Point(0, 0);
            this.labeledPanel1.Name = "labeledPanel1";
            this.labeledPanel1.PanelTitle = "Layers";
            this.labeledPanel1.Size = new System.Drawing.Size(125, 18);
            this.labeledPanel1.TabIndex = 0;
            // 
            // LayerControlPanel
            // 
            this.Controls.Add(this.pnl_container);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labeledPanel1);
            this.Name = "LayerControlPanel";
            this.Size = new System.Drawing.Size(125, 332);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Button btn_createNewLayer;
        private ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
    }
}
using System.Windows.Forms;

namespace Pixelaria.Views.Controls.LayerControls
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
            this.cms_layersRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmb_combineLayers = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.cms_layersRightClick.SuspendLayout();
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
            this.pnl_container.Click += new System.EventHandler(this.pnl_container_Click);
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
            // contextMenuStrip1
            // 
            this.cms_layersRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmb_combineLayers});
            this.cms_layersRightClick.Name = "cms_layersRightClick";
            this.cms_layersRightClick.Size = new System.Drawing.Size(160, 48);
            // 
            // cmb_combineLayers
            // 
            this.cmb_combineLayers.Image = global::Pixelaria.Properties.Resources.layer_flatten_layers;
            this.cmb_combineLayers.Name = "cmb_combineLayers";
            this.cmb_combineLayers.Size = new System.Drawing.Size(159, 22);
            this.cmb_combineLayers.Text = "Combine Layers";
            this.cmb_combineLayers.Click += new System.EventHandler(this.cmb_combineLayers_Click);
            // 
            // LayerControlPanel
            // 
            this.Controls.Add(this.pnl_container);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labeledPanel1);
            this.DoubleBuffered = true;
            this.Name = "LayerControlPanel";
            this.Size = new System.Drawing.Size(125, 332);
            this.panel1.ResumeLayout(false);
            this.cms_layersRightClick.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Button btn_createNewLayer;
        private ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private ContextMenuStrip cms_layersRightClick;
        private ToolStripMenuItem cmb_combineLayers;
    }
}
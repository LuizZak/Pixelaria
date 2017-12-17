using PixCore.Undo;

namespace Pixelaria.Views.ModelViews
{
    partial class ImportAnimationView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportAnimationView));
            UndoSystem undoSystem1 = new UndoSystem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_dimensions = new System.Windows.Forms.ToolStripStatusLabel();
            this.cpb_sheetPreview = new Pixelaria.Views.Controls.ImageEditPanel();
            this.btn_browse = new System.Windows.Forms.Button();
            this.txt_fileName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cb_frameskip = new System.Windows.Forms.CheckBox();
            this.nud_fps = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.nud_skipCount = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.btn_fitHeightRight = new System.Windows.Forms.Button();
            this.btn_fitWidthRight = new System.Windows.Forms.Button();
            this.btn_fitHeightLeft = new System.Windows.Forms.Button();
            this.btn_fitWidthLeft = new System.Windows.Forms.Button();
            this.cb_reverseFrameOrder = new System.Windows.Forms.CheckBox();
            this.nud_frameCount = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.nud_startY = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.nud_startX = new System.Windows.Forms.NumericUpDown();
            this.nud_height = new System.Windows.Forms.NumericUpDown();
            this.nud_width = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_animationName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pnl_alertPanel = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lbl_alertLabel = new System.Windows.Forms.Label();
            this.pnl_errorPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_error = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ap_animationPreview = new Pixelaria.Views.ModelViews.AnimationPreviewPanel();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_fps)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_skipCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_frameCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_startY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_startX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_width)).BeginInit();
            this.pnl_alertPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.pnl_errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Controls.Add(this.btn_browse);
            this.groupBox1.Controls.Add(this.txt_fileName);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.txt_animationName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(533, 436);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animation Properties";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.statusStrip1);
            this.groupBox5.Controls.Add(this.cpb_sheetPreview);
            this.groupBox5.Location = new System.Drawing.Point(162, 90);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(365, 336);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Sheet Preview";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tssl_dimensions});
            this.statusStrip1.Location = new System.Drawing.Point(3, 311);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(359, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(72, 17);
            this.toolStripStatusLabel1.Text = "Dimensions:";
            // 
            // tssl_dimensions
            // 
            this.tssl_dimensions.Name = "tssl_dimensions";
            this.tssl_dimensions.Size = new System.Drawing.Size(12, 17);
            this.tssl_dimensions.Text = "-";
            // 
            // cpb_sheetPreview
            // 
            this.cpb_sheetPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cpb_sheetPreview.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cpb_sheetPreview.BackgroundImage")));
            this.cpb_sheetPreview.DefaultCompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            this.cpb_sheetPreview.DefaultFillMode = Pixelaria.Views.Controls.OperationFillMode.SolidFillFirstColor;
            this.cpb_sheetPreview.EditingEnabled = true;
            this.cpb_sheetPreview.Location = new System.Drawing.Point(3, 19);
            this.cpb_sheetPreview.Name = "cpb_sheetPreview";
            this.cpb_sheetPreview.NotifyTo = null;
            this.cpb_sheetPreview.PictureBoxBackgroundImage = global::PixCore.Properties.Resources.checkers_pattern;
            this.cpb_sheetPreview.Size = new System.Drawing.Size(359, 289);
            this.cpb_sheetPreview.TabIndex = 8;
            this.cpb_sheetPreview.TabStop = false;
            undoSystem1.MaximumTaskCount = 15;
            this.cpb_sheetPreview.UndoSystem = undoSystem1;
            // 
            // btn_browse
            // 
            this.btn_browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_browse.Image = global::Pixelaria.Properties.Resources.folder_open;
            this.btn_browse.Location = new System.Drawing.Point(452, 21);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(75, 26);
            this.btn_browse.TabIndex = 7;
            this.btn_browse.Text = "Browse...";
            this.btn_browse.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_browse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // txt_fileName
            // 
            this.txt_fileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_fileName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txt_fileName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.txt_fileName.Location = new System.Drawing.Point(50, 25);
            this.txt_fileName.Name = "txt_fileName";
            this.txt_fileName.Size = new System.Drawing.Size(396, 20);
            this.txt_fileName.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(26, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "File:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cb_frameskip);
            this.groupBox3.Controls.Add(this.nud_fps);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(6, 339);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(150, 84);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Playback Settings";
            // 
            // cb_frameskip
            // 
            this.cb_frameskip.AutoSize = true;
            this.cb_frameskip.Location = new System.Drawing.Point(21, 57);
            this.cb_frameskip.Name = "cb_frameskip";
            this.cb_frameskip.Size = new System.Drawing.Size(110, 17);
            this.cb_frameskip.TabIndex = 2;
            this.cb_frameskip.Text = "Enable Frameskip";
            this.cb_frameskip.UseVisualStyleBackColor = true;
            this.cb_frameskip.CheckedChanged += new System.EventHandler(this.cb_frameskip_CheckedChanged);
            // 
            // nud_fps
            // 
            this.nud_fps.Location = new System.Drawing.Point(44, 26);
            this.nud_fps.Maximum = new decimal(new int[] {
            480,
            0,
            0,
            0});
            this.nud_fps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nud_fps.Name = "nud_fps";
            this.nud_fps.Size = new System.Drawing.Size(100, 20);
            this.nud_fps.TabIndex = 7;
            this.nud_fps.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nud_fps.ValueChanged += new System.EventHandler(this.nud_fps_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "FPS:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nud_skipCount);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.btn_fitHeightRight);
            this.groupBox2.Controls.Add(this.btn_fitWidthRight);
            this.groupBox2.Controls.Add(this.btn_fitHeightLeft);
            this.groupBox2.Controls.Add(this.btn_fitWidthLeft);
            this.groupBox2.Controls.Add(this.cb_reverseFrameOrder);
            this.groupBox2.Controls.Add(this.nud_frameCount);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.nud_startY);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.nud_startX);
            this.groupBox2.Controls.Add(this.nud_height);
            this.groupBox2.Controls.Add(this.nud_width);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(6, 90);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(150, 243);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Frame Settings";
            // 
            // nud_skipCount
            // 
            this.nud_skipCount.Location = new System.Drawing.Point(81, 182);
            this.nud_skipCount.Name = "nud_skipCount";
            this.nud_skipCount.Size = new System.Drawing.Size(63, 20);
            this.nud_skipCount.TabIndex = 29;
            this.nud_skipCount.ValueChanged += new System.EventHandler(this.nud_skipCount_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 184);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 13);
            this.label10.TabIndex = 28;
            this.label10.Text = "Skip Count:";
            // 
            // btn_fitHeightRight
            // 
            this.btn_fitHeightRight.Enabled = false;
            this.btn_fitHeightRight.Location = new System.Drawing.Point(124, 119);
            this.btn_fitHeightRight.Name = "btn_fitHeightRight";
            this.btn_fitHeightRight.Size = new System.Drawing.Size(20, 20);
            this.btn_fitHeightRight.TabIndex = 27;
            this.btn_fitHeightRight.Text = ">";
            this.btn_fitHeightRight.UseVisualStyleBackColor = true;
            this.btn_fitHeightRight.Click += new System.EventHandler(this.btn_fitHeightRight_Click);
            // 
            // btn_fitWidthRight
            // 
            this.btn_fitWidthRight.Enabled = false;
            this.btn_fitWidthRight.Location = new System.Drawing.Point(124, 80);
            this.btn_fitWidthRight.Name = "btn_fitWidthRight";
            this.btn_fitWidthRight.Size = new System.Drawing.Size(20, 20);
            this.btn_fitWidthRight.TabIndex = 26;
            this.btn_fitWidthRight.Text = ">";
            this.btn_fitWidthRight.UseVisualStyleBackColor = true;
            this.btn_fitWidthRight.Click += new System.EventHandler(this.btn_fitWidthRight_Click);
            // 
            // btn_fitHeightLeft
            // 
            this.btn_fitHeightLeft.Enabled = false;
            this.btn_fitHeightLeft.Location = new System.Drawing.Point(44, 119);
            this.btn_fitHeightLeft.Name = "btn_fitHeightLeft";
            this.btn_fitHeightLeft.Size = new System.Drawing.Size(20, 20);
            this.btn_fitHeightLeft.TabIndex = 25;
            this.btn_fitHeightLeft.Text = "<";
            this.btn_fitHeightLeft.UseVisualStyleBackColor = true;
            this.btn_fitHeightLeft.Click += new System.EventHandler(this.btn_fitHeightLeft_Click);
            // 
            // btn_fitWidthLeft
            // 
            this.btn_fitWidthLeft.Enabled = false;
            this.btn_fitWidthLeft.Location = new System.Drawing.Point(44, 80);
            this.btn_fitWidthLeft.Name = "btn_fitWidthLeft";
            this.btn_fitWidthLeft.Size = new System.Drawing.Size(20, 20);
            this.btn_fitWidthLeft.TabIndex = 24;
            this.btn_fitWidthLeft.Text = "<";
            this.btn_fitWidthLeft.UseVisualStyleBackColor = true;
            this.btn_fitWidthLeft.Click += new System.EventHandler(this.btn_fitWidthLeft_Click);
            // 
            // cb_reverseFrameOrder
            // 
            this.cb_reverseFrameOrder.AutoSize = true;
            this.cb_reverseFrameOrder.Location = new System.Drawing.Point(11, 220);
            this.cb_reverseFrameOrder.Name = "cb_reverseFrameOrder";
            this.cb_reverseFrameOrder.Size = new System.Drawing.Size(127, 17);
            this.cb_reverseFrameOrder.TabIndex = 12;
            this.cb_reverseFrameOrder.Text = "Reverse Frame Order";
            this.cb_reverseFrameOrder.UseVisualStyleBackColor = true;
            this.cb_reverseFrameOrder.CheckedChanged += new System.EventHandler(this.cb_reverseFrameOrder_CheckedChanged);
            // 
            // nud_frameCount
            // 
            this.nud_frameCount.Location = new System.Drawing.Point(81, 149);
            this.nud_frameCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nud_frameCount.Name = "nud_frameCount";
            this.nud_frameCount.Size = new System.Drawing.Size(63, 20);
            this.nud_frameCount.TabIndex = 11;
            this.nud_frameCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nud_frameCount.ValueChanged += new System.EventHandler(this.nud_frameCount_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 151);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Frame Count:";
            // 
            // nud_startY
            // 
            this.nud_startY.Location = new System.Drawing.Point(66, 45);
            this.nud_startY.Name = "nud_startY";
            this.nud_startY.Size = new System.Drawing.Size(78, 20);
            this.nud_startY.TabIndex = 4;
            this.nud_startY.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 47);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Start Y:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Start X:";
            // 
            // nud_startX
            // 
            this.nud_startX.Location = new System.Drawing.Point(66, 19);
            this.nud_startX.Name = "nud_startX";
            this.nud_startX.Size = new System.Drawing.Size(78, 20);
            this.nud_startX.TabIndex = 3;
            this.nud_startX.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // nud_height
            // 
            this.nud_height.Location = new System.Drawing.Point(68, 119);
            this.nud_height.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.nud_height.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_height.Name = "nud_height";
            this.nud_height.Size = new System.Drawing.Size(54, 20);
            this.nud_height.TabIndex = 6;
            this.nud_height.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nud_height.ValueChanged += new System.EventHandler(this.nud_height_ValueChanged);
            // 
            // nud_width
            // 
            this.nud_width.Location = new System.Drawing.Point(68, 80);
            this.nud_width.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.nud_width.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_width.Name = "nud_width";
            this.nud_width.Size = new System.Drawing.Size(54, 20);
            this.nud_width.TabIndex = 5;
            this.nud_width.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nud_width.ValueChanged += new System.EventHandler(this.nud_width_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(90, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "x";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Height:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Width:";
            // 
            // txt_animationName
            // 
            this.txt_animationName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_animationName.Location = new System.Drawing.Point(50, 58);
            this.txt_animationName.Name = "txt_animationName";
            this.txt_animationName.Size = new System.Drawing.Size(477, 20);
            this.txt_animationName.TabIndex = 2;
            this.txt_animationName.TextChanged += new System.EventHandler(this.txt_animationName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // pnl_alertPanel
            // 
            this.pnl_alertPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_alertPanel.Controls.Add(this.pictureBox2);
            this.pnl_alertPanel.Controls.Add(this.lbl_alertLabel);
            this.pnl_alertPanel.Location = new System.Drawing.Point(12, 457);
            this.pnl_alertPanel.Name = "pnl_alertPanel";
            this.pnl_alertPanel.Size = new System.Drawing.Size(598, 29);
            this.pnl_alertPanel.TabIndex = 22;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(5, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(22, 22);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // lbl_alertLabel
            // 
            this.lbl_alertLabel.AutoSize = true;
            this.lbl_alertLabel.Location = new System.Drawing.Point(29, 8);
            this.lbl_alertLabel.Name = "lbl_alertLabel";
            this.lbl_alertLabel.Size = new System.Drawing.Size(242, 13);
            this.lbl_alertLabel.TabIndex = 9;
            this.lbl_alertLabel.Text = "The project folder path is invalid or does not exists";
            // 
            // pnl_errorPanel
            // 
            this.pnl_errorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_errorPanel.Controls.Add(this.pictureBox1);
            this.pnl_errorPanel.Controls.Add(this.lbl_error);
            this.pnl_errorPanel.Location = new System.Drawing.Point(12, 457);
            this.pnl_errorPanel.Name = "pnl_errorPanel";
            this.pnl_errorPanel.Size = new System.Drawing.Size(598, 29);
            this.pnl_errorPanel.TabIndex = 21;
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
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.ap_animationPreview);
            this.groupBox4.Location = new System.Drawing.Point(551, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(227, 439);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Animation Preview";
            // 
            // ap_animationPreview
            // 
            this.ap_animationPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ap_animationPreview.Location = new System.Drawing.Point(3, 16);
            this.ap_animationPreview.Name = "ap_animationPreview";
            this.ap_animationPreview.Size = new System.Drawing.Size(221, 420);
            this.ap_animationPreview.TabIndex = 23;
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Enabled = false;
            this.btn_ok.Image = global::Pixelaria.Properties.Resources.action_check;
            this.btn_ok.Location = new System.Drawing.Point(616, 457);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 29);
            this.btn_ok.TabIndex = 20;
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
            this.btn_cancel.Location = new System.Drawing.Point(697, 457);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 29);
            this.btn_cancel.TabIndex = 19;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_cancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // ImportAnimationView
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(784, 498);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.pnl_alertPanel);
            this.Controls.Add(this.pnl_errorPanel);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 494);
            this.Name = "ImportAnimationView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Animation";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ImportAnimationView_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_fps)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_skipCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_frameCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_startY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_startX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_width)).EndInit();
            this.pnl_alertPanel.ResumeLayout(false);
            this.pnl_alertPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.pnl_errorPanel.ResumeLayout(false);
            this.pnl_errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel pnl_alertPanel;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lbl_alertLabel;
        private System.Windows.Forms.Panel pnl_errorPanel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbl_error;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_animationName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nud_fps;
        private System.Windows.Forms.CheckBox cb_frameskip;
        private System.Windows.Forms.NumericUpDown nud_height;
        private System.Windows.Forms.NumericUpDown nud_width;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.TextBox txt_fileName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown nud_startY;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nud_startX;
        private Pixelaria.Views.Controls.ImageEditPanel cpb_sheetPreview;
        private AnimationPreviewPanel ap_animationPreview;
        private System.Windows.Forms.NumericUpDown nud_frameCount;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox cb_reverseFrameOrder;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tssl_dimensions;
        private System.Windows.Forms.Button btn_fitHeightLeft;
        private System.Windows.Forms.Button btn_fitWidthLeft;
        private System.Windows.Forms.Button btn_fitHeightRight;
        private System.Windows.Forms.Button btn_fitWidthRight;
        private System.Windows.Forms.NumericUpDown nud_skipCount;
        private System.Windows.Forms.Label label10;
    }
}
namespace Framer
{
    partial class Framer
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Framer));
            this.label9 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.trackBarFrame = new System.Windows.Forms.TrackBar();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxScale = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButtonZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxSystem = new System.Windows.Forms.ToolStripComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxFrames = new System.Windows.Forms.GroupBox();
            this.btnAddFrame = new System.Windows.Forms.Button();
            this.dataGridSprites = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAddSprite = new System.Windows.Forms.Button();
            this.dataGridFrames = new System.Windows.Forms.DataGridView();
            this.colPosX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPosY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSprts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTiles = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cbBoxSpriteList = new System.Windows.Forms.ComboBox();
            this.trackBarMask = new System.Windows.Forms.TrackBar();
            this.panelMaskColor = new System.Windows.Forms.Panel();
            this.lblMask = new System.Windows.Forms.Label();
            this.panelFrameColor = new System.Windows.Forms.Panel();
            this.lblFrame = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.scrollV = new System.Windows.Forms.VScrollBar();
            this.scrollH = new System.Windows.Forms.HScrollBar();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.chkBoxGridMode = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFrame)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxFrames.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridSprites)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFrames)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMask)).BeginInit();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(672, 512);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(35, 13);
            this.label9.TabIndex = 44;
            this.label9.Text = "label9";
            this.label9.Visible = false;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(52, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 13);
            this.label7.TabIndex = 43;
            this.label7.Text = "label7";
            this.label7.Visible = false;
            // 
            // trackBarFrame
            // 
            this.trackBarFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarFrame.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarFrame.LargeChange = 16;
            this.trackBarFrame.Location = new System.Drawing.Point(794, 480);
            this.trackBarFrame.Maximum = 255;
            this.trackBarFrame.Name = "trackBarFrame";
            this.trackBarFrame.Size = new System.Drawing.Size(194, 45);
            this.trackBarFrame.SmallChange = 16;
            this.trackBarFrame.TabIndex = 50;
            this.trackBarFrame.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarFrame.Value = 128;
            this.trackBarFrame.ValueChanged += new System.EventHandler(this.trackBarFrame_ValueChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOpen,
            this.toolStripButtonSave,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripComboBoxScale,
            this.toolStripButtonZoomOut,
            this.toolStripButtonZoomIn,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.toolStripComboBoxSystem});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1012, 25);
            this.toolStrip1.TabIndex = 37;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonOpen
            // 
            this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonOpen.Image")));
            this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonOpen.Text = "Open stage";
            this.toolStripButtonOpen.ToolTipText = "Open";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.toolStripButtonOpen_Click);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSave.Image = global::Framer.Properties.Resources.disk;
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSave.Text = "Save";
            this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSave_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(37, 22);
            this.toolStripLabel1.Text = "Scale:";
            // 
            // toolStripComboBoxScale
            // 
            this.toolStripComboBoxScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxScale.Items.AddRange(new object[] {
            "100%",
            "200%",
            "400%",
            "800%"});
            this.toolStripComboBoxScale.Name = "toolStripComboBoxScale";
            this.toolStripComboBoxScale.Size = new System.Drawing.Size(75, 25);
            this.toolStripComboBoxScale.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxScale_SelectedIndexChanged);
            // 
            // toolStripButtonZoomOut
            // 
            this.toolStripButtonZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonZoomOut.Image")));
            this.toolStripButtonZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonZoomOut.Name = "toolStripButtonZoomOut";
            this.toolStripButtonZoomOut.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonZoomOut.Text = "Scale down";
            this.toolStripButtonZoomOut.Click += new System.EventHandler(this.toolStripButtonZoomOut_Click);
            // 
            // toolStripButtonZoomIn
            // 
            this.toolStripButtonZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonZoomIn.Image")));
            this.toolStripButtonZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonZoomIn.Name = "toolStripButtonZoomIn";
            this.toolStripButtonZoomIn.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonZoomIn.Text = "Scale up";
            this.toolStripButtonZoomIn.Click += new System.EventHandler(this.toolStripButtonZoomIn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(48, 22);
            this.toolStripLabel2.Text = "System:";
            // 
            // toolStripComboBoxSystem
            // 
            this.toolStripComboBoxSystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxSystem.Items.AddRange(new object[] {
            "NeoGeo",
            "Megadrive",
            "Super Famicom",
            "PC-Engine",
            "CPS1/CPS2"});
            this.toolStripComboBoxSystem.Name = "toolStripComboBoxSystem";
            this.toolStripComboBoxSystem.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBoxSystem.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxSystem_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBoxFrames);
            this.groupBox1.Controls.Add(this.cbBoxSpriteList);
            this.groupBox1.Location = new System.Drawing.Point(663, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(337, 416);
            this.groupBox1.TabIndex = 45;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sprites";
            // 
            // groupBoxFrames
            // 
            this.groupBoxFrames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.groupBoxFrames.Controls.Add(this.btnAddFrame);
            this.groupBoxFrames.Controls.Add(this.dataGridSprites);
            this.groupBoxFrames.Controls.Add(this.btnAddSprite);
            this.groupBoxFrames.Controls.Add(this.dataGridFrames);
            this.groupBoxFrames.Controls.Add(this.label7);
            this.groupBoxFrames.Location = new System.Drawing.Point(6, 46);
            this.groupBoxFrames.Name = "groupBoxFrames";
            this.groupBoxFrames.Size = new System.Drawing.Size(325, 364);
            this.groupBoxFrames.TabIndex = 52;
            this.groupBoxFrames.TabStop = false;
            this.groupBoxFrames.Text = "Frames";
            // 
            // btnAddFrame
            // 
            this.btnAddFrame.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAddFrame.Location = new System.Drawing.Point(6, 331);
            this.btnAddFrame.Name = "btnAddFrame";
            this.btnAddFrame.Size = new System.Drawing.Size(153, 27);
            this.btnAddFrame.TabIndex = 45;
            this.btnAddFrame.Text = "Add frame";
            this.btnAddFrame.UseVisualStyleBackColor = true;
            this.btnAddFrame.Click += new System.EventHandler(this.btnAddFrame_Click);
            // 
            // dataGridSprites
            // 
            this.dataGridSprites.AllowUserToAddRows = false;
            this.dataGridSprites.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.dataGridSprites.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridSprites.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.dataGridSprites.Location = new System.Drawing.Point(6, 172);
            this.dataGridSprites.Name = "dataGridSprites";
            this.dataGridSprites.ReadOnly = true;
            this.dataGridSprites.Size = new System.Drawing.Size(313, 153);
            this.dataGridSprites.TabIndex = 44;
            this.dataGridSprites.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dataGridSprites_RowsRemoved);
            this.dataGridSprites.SelectionChanged += new System.EventHandler(this.dataGridSprites_SelectionChanged);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "pos X";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Width = 60;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "pos Y";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn2.Width = 60;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "width";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn3.Width = 60;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "height";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn4.Width = 60;
            // 
            // btnAddSprite
            // 
            this.btnAddSprite.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAddSprite.Location = new System.Drawing.Point(165, 331);
            this.btnAddSprite.Name = "btnAddSprite";
            this.btnAddSprite.Size = new System.Drawing.Size(154, 27);
            this.btnAddSprite.TabIndex = 2;
            this.btnAddSprite.Text = "Add selected sprite";
            this.btnAddSprite.UseVisualStyleBackColor = true;
            this.btnAddSprite.Click += new System.EventHandler(this.btnAddSprite_Click);
            // 
            // dataGridFrames
            // 
            this.dataGridFrames.AllowUserToAddRows = false;
            this.dataGridFrames.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridFrames.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colPosX,
            this.colPosY,
            this.colWidth,
            this.colHeight,
            this.colSprts,
            this.colTiles});
            this.dataGridFrames.Location = new System.Drawing.Point(6, 19);
            this.dataGridFrames.Name = "dataGridFrames";
            this.dataGridFrames.ReadOnly = true;
            this.dataGridFrames.Size = new System.Drawing.Size(313, 147);
            this.dataGridFrames.TabIndex = 1;
            this.dataGridFrames.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridFrames_CellValidated);
            this.dataGridFrames.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridFrames_CellValidating);
            this.dataGridFrames.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dataGridFrames_RowsRemoved);
            this.dataGridFrames.SelectionChanged += new System.EventHandler(this.dataGridFrames_SelectionChanged);
            // 
            // colPosX
            // 
            this.colPosX.HeaderText = "pos X";
            this.colPosX.Name = "colPosX";
            this.colPosX.ReadOnly = true;
            this.colPosX.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colPosX.Width = 42;
            // 
            // colPosY
            // 
            this.colPosY.HeaderText = "pos Y";
            this.colPosY.Name = "colPosY";
            this.colPosY.ReadOnly = true;
            this.colPosY.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colPosY.Width = 42;
            // 
            // colWidth
            // 
            this.colWidth.HeaderText = "width";
            this.colWidth.Name = "colWidth";
            this.colWidth.ReadOnly = true;
            this.colWidth.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colWidth.Width = 42;
            // 
            // colHeight
            // 
            this.colHeight.HeaderText = "height";
            this.colHeight.Name = "colHeight";
            this.colHeight.ReadOnly = true;
            this.colHeight.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colHeight.Width = 42;
            // 
            // colSprts
            // 
            this.colSprts.HeaderText = "Sprites";
            this.colSprts.Name = "colSprts";
            this.colSprts.ReadOnly = true;
            this.colSprts.Width = 42;
            // 
            // colTiles
            // 
            this.colTiles.HeaderText = "Tiles";
            this.colTiles.Name = "colTiles";
            this.colTiles.ReadOnly = true;
            this.colTiles.Width = 42;
            // 
            // cbBoxSpriteList
            // 
            this.cbBoxSpriteList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBoxSpriteList.FormattingEnabled = true;
            this.cbBoxSpriteList.Location = new System.Drawing.Point(6, 19);
            this.cbBoxSpriteList.Name = "cbBoxSpriteList";
            this.cbBoxSpriteList.Size = new System.Drawing.Size(325, 21);
            this.cbBoxSpriteList.TabIndex = 0;
            this.cbBoxSpriteList.SelectedIndexChanged += new System.EventHandler(this.cbBoxSpriteList_SelectedIndexChanged);
            // 
            // trackBarMask
            // 
            this.trackBarMask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarMask.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarMask.LargeChange = 16;
            this.trackBarMask.Location = new System.Drawing.Point(794, 447);
            this.trackBarMask.Maximum = 255;
            this.trackBarMask.Name = "trackBarMask";
            this.trackBarMask.Size = new System.Drawing.Size(194, 45);
            this.trackBarMask.SmallChange = 16;
            this.trackBarMask.TabIndex = 25;
            this.trackBarMask.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarMask.Value = 128;
            this.trackBarMask.ValueChanged += new System.EventHandler(this.trackBarMask_ValueChanged);
            // 
            // panelMaskColor
            // 
            this.panelMaskColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMaskColor.BackColor = System.Drawing.Color.OliveDrab;
            this.panelMaskColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelMaskColor.Cursor = System.Windows.Forms.Cursors.Default;
            this.panelMaskColor.Location = new System.Drawing.Point(734, 447);
            this.panelMaskColor.Name = "panelMaskColor";
            this.panelMaskColor.Size = new System.Drawing.Size(54, 27);
            this.panelMaskColor.TabIndex = 51;
            this.panelMaskColor.Click += new System.EventHandler(this.panelMaskColor_Click);
            // 
            // lblMask
            // 
            this.lblMask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMask.AutoSize = true;
            this.lblMask.Location = new System.Drawing.Point(666, 452);
            this.lblMask.Name = "lblMask";
            this.lblMask.Size = new System.Drawing.Size(59, 13);
            this.lblMask.TabIndex = 49;
            this.lblMask.Text = "Mask color";
            // 
            // panelFrameColor
            // 
            this.panelFrameColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFrameColor.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.panelFrameColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelFrameColor.Cursor = System.Windows.Forms.Cursors.Default;
            this.panelFrameColor.Location = new System.Drawing.Point(734, 480);
            this.panelFrameColor.Name = "panelFrameColor";
            this.panelFrameColor.Size = new System.Drawing.Size(54, 27);
            this.panelFrameColor.TabIndex = 49;
            this.panelFrameColor.Click += new System.EventHandler(this.panelFrameColor_Click);
            // 
            // lblFrame
            // 
            this.lblFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFrame.AutoSize = true;
            this.lblFrame.Location = new System.Drawing.Point(666, 485);
            this.lblFrame.Name = "lblFrame";
            this.lblFrame.Size = new System.Drawing.Size(66, 13);
            this.lblFrame.TabIndex = 48;
            this.lblFrame.Text = "Select. color";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.LargeChange = 480;
            this.scrollV.Location = new System.Drawing.Point(640, 25);
            this.scrollV.Maximum = 480;
            this.scrollV.Name = "scrollV";
            this.scrollV.Size = new System.Drawing.Size(20, 480);
            this.scrollV.SmallChange = 16;
            this.scrollV.TabIndex = 47;
            this.scrollV.ValueChanged += new System.EventHandler(this.scrollV_ValueChanged);
            // 
            // scrollH
            // 
            this.scrollH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollH.LargeChange = 640;
            this.scrollH.Location = new System.Drawing.Point(0, 505);
            this.scrollH.Maximum = 640;
            this.scrollH.Name = "scrollH";
            this.scrollH.Size = new System.Drawing.Size(640, 20);
            this.scrollH.SmallChange = 16;
            this.scrollH.TabIndex = 46;
            this.scrollH.ValueChanged += new System.EventHandler(this.scrollH_ValueChanged);
            // 
            // colorDialog1
            // 
            this.colorDialog1.Color = System.Drawing.Color.DeepSkyBlue;
            // 
            // chkBoxGridMode
            // 
            this.chkBoxGridMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBoxGridMode.AutoSize = true;
            this.chkBoxGridMode.Location = new System.Drawing.Point(734, 511);
            this.chkBoxGridMode.Name = "chkBoxGridMode";
            this.chkBoxGridMode.Size = new System.Drawing.Size(134, 17);
            this.chkBoxGridMode.TabIndex = 52;
            this.chkBoxGridMode.Text = "Lock cursor to 8x8 grid";
            this.chkBoxGridMode.UseVisualStyleBackColor = true;
            this.chkBoxGridMode.CheckedChanged += new System.EventHandler(this.chkBoxGridMode_CheckedChanged);
            // 
            // Framer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 534);
            this.Controls.Add(this.chkBoxGridMode);
            this.Controls.Add(this.scrollV);
            this.Controls.Add(this.trackBarFrame);
            this.Controls.Add(this.scrollH);
            this.Controls.Add(this.trackBarMask);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panelMaskColor);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lblMask);
            this.Controls.Add(this.panelFrameColor);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lblFrame);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1028, 572);
            this.Name = "Framer";
            this.Text = "Framer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFrame)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBoxFrames.ResumeLayout(false);
            this.groupBoxFrames.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridSprites)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFrames)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMask)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar trackBarFrame;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomIn;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxScale;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomOut;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbBoxSpriteList;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.DataGridView dataGridFrames;
        private System.Windows.Forms.VScrollBar scrollV;
        private System.Windows.Forms.HScrollBar scrollH;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Panel panelFrameColor;
        private System.Windows.Forms.TrackBar trackBarMask;
        private System.Windows.Forms.Panel panelMaskColor;
        private System.Windows.Forms.Label lblMask;
        private System.Windows.Forms.Label lblFrame;
        private System.Windows.Forms.GroupBox groupBoxFrames;
        private System.Windows.Forms.Button btnAddSprite;
        private System.Windows.Forms.DataGridView dataGridSprites;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.CheckBox chkBoxGridMode;
        private System.Windows.Forms.Button btnAddFrame;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPosX;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPosY;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWidth;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHeight;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSprts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTiles;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSystem;
    }
}


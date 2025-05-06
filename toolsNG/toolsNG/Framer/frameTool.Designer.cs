namespace Framer
{
    partial class frameTool
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
            this.pictBoxPanel = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.stripScaleComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.stripBtnScaleDown = new System.Windows.Forms.ToolStripButton();
            this.stripBtnScaleUp = new System.Windows.Forms.ToolStripButton();
            this.dataGridChunks = new System.Windows.Forms.DataGridView();
            this.colPvwStmp = new System.Windows.Forms.DataGridViewImageColumn();
            this.colFlipX = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colFlipY = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.lblDebug = new System.Windows.Forms.Label();
            this.chkBoxOutlines = new System.Windows.Forms.CheckBox();
            this.btnResetStamps = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pictBox = new customControls.PictureBoxWithInterpolationMode();
            this.pictBoxPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridChunks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictBoxPanel
            // 
            this.pictBoxPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictBoxPanel.AutoScroll = true;
            this.pictBoxPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictBoxPanel.Controls.Add(this.pictBox);
            this.pictBoxPanel.Location = new System.Drawing.Point(0, 28);
            this.pictBoxPanel.Name = "pictBoxPanel";
            this.pictBoxPanel.Size = new System.Drawing.Size(675, 579);
            this.pictBoxPanel.TabIndex = 1;
            this.pictBoxPanel.SizeChanged += new System.EventHandler(this.pictBoxPanel_SizeChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.stripScaleComboBox,
            this.stripBtnScaleDown,
            this.stripBtnScaleUp});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(982, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(37, 22);
            this.toolStripLabel1.Text = "Scale:";
            // 
            // stripScaleComboBox
            // 
            this.stripScaleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stripScaleComboBox.DropDownWidth = 75;
            this.stripScaleComboBox.Items.AddRange(new object[] {
            "100%",
            "200%",
            "400%",
            "800%"});
            this.stripScaleComboBox.Name = "stripScaleComboBox";
            this.stripScaleComboBox.Size = new System.Drawing.Size(75, 25);
            this.stripScaleComboBox.SelectedIndexChanged += new System.EventHandler(this.stripScaleComboBox_SelectedIndexChanged);
            // 
            // stripBtnScaleDown
            // 
            this.stripBtnScaleDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stripBtnScaleDown.Image = global::Framer.Properties.Resources.magifier_zoom_out;
            this.stripBtnScaleDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stripBtnScaleDown.Name = "stripBtnScaleDown";
            this.stripBtnScaleDown.Size = new System.Drawing.Size(23, 22);
            this.stripBtnScaleDown.Text = "stripBtnScaleDown";
            this.stripBtnScaleDown.ToolTipText = "Scale down";
            this.stripBtnScaleDown.Click += new System.EventHandler(this.stripBtnScaleDown_Click);
            // 
            // stripBtnScaleUp
            // 
            this.stripBtnScaleUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stripBtnScaleUp.Image = global::Framer.Properties.Resources.magnifier_zoom_in;
            this.stripBtnScaleUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stripBtnScaleUp.Name = "stripBtnScaleUp";
            this.stripBtnScaleUp.Size = new System.Drawing.Size(23, 22);
            this.stripBtnScaleUp.Text = "Scale up";
            this.stripBtnScaleUp.Click += new System.EventHandler(this.stripBtnScaleUp_Click);
            // 
            // dataGridChunks
            // 
            this.dataGridChunks.AllowUserToAddRows = false;
            this.dataGridChunks.AllowUserToDeleteRows = false;
            this.dataGridChunks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridChunks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridChunks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colPvwStmp,
            this.colFlipX,
            this.colFlipY});
            this.dataGridChunks.Location = new System.Drawing.Point(681, 28);
            this.dataGridChunks.Name = "dataGridChunks";
            this.dataGridChunks.RowHeadersWidth = 30;
            this.dataGridChunks.Size = new System.Drawing.Size(289, 401);
            this.dataGridChunks.TabIndex = 4;
            this.dataGridChunks.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridChunks_CellValidated);
            this.dataGridChunks.SelectionChanged += new System.EventHandler(this.dataGridChunks_SelectionChanged);
            // 
            // colPvwStmp
            // 
            this.colPvwStmp.HeaderText = "Stamp preview";
            this.colPvwStmp.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.colPvwStmp.MinimumWidth = 180;
            this.colPvwStmp.Name = "colPvwStmp";
            this.colPvwStmp.Width = 180;
            // 
            // colFlipX
            // 
            this.colFlipX.HeaderText = "X";
            this.colFlipX.MinimumWidth = 20;
            this.colFlipX.Name = "colFlipX";
            this.colFlipX.Width = 20;
            // 
            // colFlipY
            // 
            this.colFlipY.HeaderText = "Y";
            this.colFlipY.MinimumWidth = 20;
            this.colFlipY.Name = "colFlipY";
            this.colFlipY.Width = 20;
            // 
            // lblDebug
            // 
            this.lblDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDebug.AutoSize = true;
            this.lblDebug.Location = new System.Drawing.Point(744, 508);
            this.lblDebug.Name = "lblDebug";
            this.lblDebug.Size = new System.Drawing.Size(35, 13);
            this.lblDebug.TabIndex = 5;
            this.lblDebug.Text = "label1";
            this.lblDebug.Visible = false;
            // 
            // chkBoxOutlines
            // 
            this.chkBoxOutlines.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBoxOutlines.AutoSize = true;
            this.chkBoxOutlines.Checked = true;
            this.chkBoxOutlines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxOutlines.Location = new System.Drawing.Point(878, 441);
            this.chkBoxOutlines.Name = "chkBoxOutlines";
            this.chkBoxOutlines.Size = new System.Drawing.Size(92, 17);
            this.chkBoxOutlines.TabIndex = 6;
            this.chkBoxOutlines.Text = "Show outlines";
            this.chkBoxOutlines.UseVisualStyleBackColor = true;
            this.chkBoxOutlines.CheckedChanged += new System.EventHandler(this.chkBoxOutlines_CheckedChanged);
            // 
            // btnResetStamps
            // 
            this.btnResetStamps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetStamps.Location = new System.Drawing.Point(681, 435);
            this.btnResetStamps.Name = "btnResetStamps";
            this.btnResetStamps.Size = new System.Drawing.Size(98, 26);
            this.btnResetStamps.TabIndex = 7;
            this.btnResetStamps.Text = "Reset layout";
            this.btnResetStamps.UseVisualStyleBackColor = true;
            this.btnResetStamps.Click += new System.EventHandler(this.btnResetStamps_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(865, 561);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(105, 33);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(754, 561);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(105, 33);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // pictBox
            // 
            this.pictBox.BackColor = System.Drawing.SystemColors.Info;
            this.pictBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictBox.InitialImage = null;
            this.pictBox.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.pictBox.Location = new System.Drawing.Point(23, 27);
            this.pictBox.Name = "pictBox";
            this.pictBox.Size = new System.Drawing.Size(512, 512);
            this.pictBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictBox.TabIndex = 0;
            this.pictBox.TabStop = false;
            this.pictBox.Resize += new System.EventHandler(this.pictBox_Resize);
            // 
            // frameTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 606);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnResetStamps);
            this.Controls.Add(this.chkBoxOutlines);
            this.Controls.Add(this.lblDebug);
            this.Controls.Add(this.dataGridChunks);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.pictBoxPanel);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.Name = "frameTool";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frameTool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frameTool_FormClosing);
            this.Load += new System.EventHandler(this.frameTool_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frameTool_KeyDown);
            this.pictBoxPanel.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridChunks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel pictBoxPanel;
        //private System.Windows.Forms.PictureBox pictBox;
        private customControls.PictureBoxWithInterpolationMode pictBox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox stripScaleComboBox;
        private System.Windows.Forms.ToolStripButton stripBtnScaleDown;
        private System.Windows.Forms.ToolStripButton stripBtnScaleUp;
        private System.Windows.Forms.DataGridView dataGridChunks;
        private System.Windows.Forms.DataGridViewImageColumn colPvwStmp;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colFlipX;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colFlipY;
        private System.Windows.Forms.Label lblDebug;
        private System.Windows.Forms.CheckBox chkBoxOutlines;
        private System.Windows.Forms.Button btnResetStamps;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
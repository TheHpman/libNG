using System.Windows.Forms;

namespace Animator
{
    partial class Animator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Animator));
            this.scrollV = new System.Windows.Forms.VScrollBar();
            this.scrollH = new System.Windows.Forms.HScrollBar();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.listBoxFrames = new System.Windows.Forms.ListBox();
            this.grpSprite = new System.Windows.Forms.GroupBox();
            this.grpFrameDetails = new System.Windows.Forms.GroupBox();
            this.lblFrameHeight = new System.Windows.Forms.Label();
            this.lblFrameWidth = new System.Windows.Forms.Label();
            this.grpBoxAnimDetails = new System.Windows.Forms.GroupBox();
            this.lblCName = new System.Windows.Forms.Label();
            this.lblRepeatInfo = new System.Windows.Forms.Label();
            this.txtRepeats = new System.Windows.Forms.TextBox();
            this.lblRepeat = new System.Windows.Forms.Label();
            this.comboLink = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSteps = new System.Windows.Forms.Label();
            this.lblTotalTiming = new System.Windows.Forms.Label();
            this.chkPlayRepeats = new System.Windows.Forms.CheckBox();
            this.chkLoop = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnAddAnimation = new System.Windows.Forms.Button();
            this.txtBoxAnimName = new System.Windows.Forms.TextBox();
            this.listBoxAnimations = new System.Windows.Forms.ListBox();
            this.btnStepDelete = new System.Windows.Forms.Button();
            this.grpBoxAlign = new System.Windows.Forms.GroupBox();
            this.chkShowPrev = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTiming = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.chkAlignAll = new System.Windows.Forms.CheckBox();
            this.txtAlignY = new System.Windows.Forms.TextBox();
            this.btnAlignDown = new System.Windows.Forms.Button();
            this.txtAlignX = new System.Windows.Forms.TextBox();
            this.btnAlignRight = new System.Windows.Forms.Button();
            this.btnAlignLeft = new System.Windows.Forms.Button();
            this.btnAlignUp = new System.Windows.Forms.Button();
            this.listBoxSteps = new System.Windows.Forms.ListBox();
            this.btnStepUp = new System.Windows.Forms.Button();
            this.btnStepDown = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonExport = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxScale = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButtonZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cboBoxGroup = new System.Windows.Forms.ToolStripComboBox();
            this.btnBoxColor = new System.Windows.Forms.ToolStripButton();
            this.btnAddBoxGroup = new System.Windows.Forms.ToolStripButton();
            this.btnBoxShow = new System.Windows.Forms.ToolStripButton();
            this.tabAnimations = new System.Windows.Forms.TabControl();
            this.tabPageAnimation = new System.Windows.Forms.TabPage();
            this.grpBoxPlayback = new System.Windows.Forms.GroupBox();
            this.tabPageSteps = new System.Windows.Forms.TabPage();
            this.grpBoxBoxes = new System.Windows.Forms.GroupBox();
            this.button17 = new System.Windows.Forms.Button();
            this.btnAddBox = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.boxGrid = new System.Windows.Forms.DataGridView();
            this.boxX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.boxY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.boxW = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.boxH = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblDebug = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.grpSprite.SuspendLayout();
            this.grpFrameDetails.SuspendLayout();
            this.grpBoxAnimDetails.SuspendLayout();
            this.grpBoxAlign.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabAnimations.SuspendLayout();
            this.tabPageAnimation.SuspendLayout();
            this.grpBoxPlayback.SuspendLayout();
            this.tabPageSteps.SuspendLayout();
            this.grpBoxBoxes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.boxGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // scrollV
            // 
            this.scrollV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollV.Enabled = false;
            this.scrollV.LargeChange = 64;
            this.scrollV.Location = new System.Drawing.Point(640, 25);
            this.scrollV.Name = "scrollV";
            this.scrollV.Size = new System.Drawing.Size(17, 480);
            this.scrollV.SmallChange = 16;
            this.scrollV.TabIndex = 0;
            this.scrollV.ValueChanged += new System.EventHandler(this.scrollV_ValueChanged);
            // 
            // scrollH
            // 
            this.scrollH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollH.Enabled = false;
            this.scrollH.LargeChange = 64;
            this.scrollH.Location = new System.Drawing.Point(0, 505);
            this.scrollH.Name = "scrollH";
            this.scrollH.Size = new System.Drawing.Size(640, 17);
            this.scrollH.SmallChange = 16;
            this.scrollH.TabIndex = 1;
            this.scrollH.ValueChanged += new System.EventHandler(this.scrollH_ValueChanged);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select spr folder";
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // listBoxFrames
            // 
            this.listBoxFrames.FormattingEnabled = true;
            this.listBoxFrames.Location = new System.Drawing.Point(6, 19);
            this.listBoxFrames.Name = "listBoxFrames";
            this.listBoxFrames.Size = new System.Drawing.Size(159, 147);
            this.listBoxFrames.TabIndex = 3;
            this.listBoxFrames.SelectedIndexChanged += new System.EventHandler(this.listBoxFrames_SelectedIndexChanged);
            this.listBoxFrames.DoubleClick += new System.EventHandler(this.listBoxFrames_DoubleClick);
            // 
            // grpSprite
            // 
            this.grpSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSprite.Controls.Add(this.grpFrameDetails);
            this.grpSprite.Controls.Add(this.listBoxFrames);
            this.grpSprite.Location = new System.Drawing.Point(666, 28);
            this.grpSprite.Name = "grpSprite";
            this.grpSprite.Size = new System.Drawing.Size(462, 172);
            this.grpSprite.TabIndex = 2;
            this.grpSprite.TabStop = false;
            this.grpSprite.Text = "Sprite data";
            // 
            // grpFrameDetails
            // 
            this.grpFrameDetails.Controls.Add(this.lblFrameHeight);
            this.grpFrameDetails.Controls.Add(this.lblFrameWidth);
            this.grpFrameDetails.Location = new System.Drawing.Point(171, 19);
            this.grpFrameDetails.Name = "grpFrameDetails";
            this.grpFrameDetails.Size = new System.Drawing.Size(285, 147);
            this.grpFrameDetails.TabIndex = 0;
            this.grpFrameDetails.TabStop = false;
            this.grpFrameDetails.Text = "Frame details";
            this.grpFrameDetails.Paint += new System.Windows.Forms.PaintEventHandler(this.grpFrameDetails_Paint);
            // 
            // lblFrameHeight
            // 
            this.lblFrameHeight.AutoSize = true;
            this.lblFrameHeight.Location = new System.Drawing.Point(155, 42);
            this.lblFrameHeight.Name = "lblFrameHeight";
            this.lblFrameHeight.Size = new System.Drawing.Size(41, 13);
            this.lblFrameHeight.TabIndex = 1;
            this.lblFrameHeight.Text = "Height:";
            // 
            // lblFrameWidth
            // 
            this.lblFrameWidth.AutoSize = true;
            this.lblFrameWidth.Location = new System.Drawing.Point(155, 29);
            this.lblFrameWidth.Name = "lblFrameWidth";
            this.lblFrameWidth.Size = new System.Drawing.Size(38, 13);
            this.lblFrameWidth.TabIndex = 0;
            this.lblFrameWidth.Text = "Width:";
            // 
            // grpBoxAnimDetails
            // 
            this.grpBoxAnimDetails.Controls.Add(this.lblCName);
            this.grpBoxAnimDetails.Controls.Add(this.lblRepeatInfo);
            this.grpBoxAnimDetails.Controls.Add(this.txtRepeats);
            this.grpBoxAnimDetails.Controls.Add(this.lblRepeat);
            this.grpBoxAnimDetails.Controls.Add(this.comboLink);
            this.grpBoxAnimDetails.Controls.Add(this.label4);
            this.grpBoxAnimDetails.Controls.Add(this.lblSteps);
            this.grpBoxAnimDetails.Controls.Add(this.lblTotalTiming);
            this.grpBoxAnimDetails.Location = new System.Drawing.Point(164, 6);
            this.grpBoxAnimDetails.Name = "grpBoxAnimDetails";
            this.grpBoxAnimDetails.Size = new System.Drawing.Size(284, 112);
            this.grpBoxAnimDetails.TabIndex = 3;
            this.grpBoxAnimDetails.TabStop = false;
            this.grpBoxAnimDetails.Text = "Animation details";
            // 
            // lblCName
            // 
            this.lblCName.AutoSize = true;
            this.lblCName.Location = new System.Drawing.Point(6, 16);
            this.lblCName.Name = "lblCName";
            this.lblCName.Size = new System.Drawing.Size(21, 13);
            this.lblCName.TabIndex = 4;
            this.lblCName.Text = "ID:";
            // 
            // lblRepeatInfo
            // 
            this.lblRepeatInfo.AutoSize = true;
            this.lblRepeatInfo.Location = new System.Drawing.Point(115, 62);
            this.lblRepeatInfo.Name = "lblRepeatInfo";
            this.lblRepeatInfo.Size = new System.Drawing.Size(136, 13);
            this.lblRepeatInfo.TabIndex = 10;
            this.lblRepeatInfo.Text = "(times played = 1 + repeats)";
            // 
            // txtRepeats
            // 
            this.txtRepeats.Location = new System.Drawing.Point(62, 59);
            this.txtRepeats.Name = "txtRepeats";
            this.txtRepeats.Size = new System.Drawing.Size(47, 20);
            this.txtRepeats.TabIndex = 3;
            this.txtRepeats.TextChanged += new System.EventHandler(this.txtRepeats_TextChanged);
            // 
            // lblRepeat
            // 
            this.lblRepeat.AutoSize = true;
            this.lblRepeat.Location = new System.Drawing.Point(6, 62);
            this.lblRepeat.Name = "lblRepeat";
            this.lblRepeat.Size = new System.Drawing.Size(50, 13);
            this.lblRepeat.TabIndex = 2;
            this.lblRepeat.Text = "Repeats:";
            // 
            // comboLink
            // 
            this.comboLink.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLink.FormattingEnabled = true;
            this.comboLink.Items.AddRange(new object[] {
            "(none)"});
            this.comboLink.Location = new System.Drawing.Point(62, 85);
            this.comboLink.Name = "comboLink";
            this.comboLink.Size = new System.Drawing.Size(151, 21);
            this.comboLink.TabIndex = 5;
            this.comboLink.SelectedIndexChanged += new System.EventHandler(this.comboLink_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Links to:";
            // 
            // lblSteps
            // 
            this.lblSteps.AutoSize = true;
            this.lblSteps.Location = new System.Drawing.Point(6, 29);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Size = new System.Drawing.Size(37, 13);
            this.lblSteps.TabIndex = 0;
            this.lblSteps.Text = "Steps:";
            // 
            // lblTotalTiming
            // 
            this.lblTotalTiming.AutoSize = true;
            this.lblTotalTiming.Location = new System.Drawing.Point(108, 29);
            this.lblTotalTiming.Name = "lblTotalTiming";
            this.lblTotalTiming.Size = new System.Drawing.Size(64, 13);
            this.lblTotalTiming.TabIndex = 1;
            this.lblTotalTiming.Text = "Total timing:";
            // 
            // chkPlayRepeats
            // 
            this.chkPlayRepeats.AutoSize = true;
            this.chkPlayRepeats.Checked = true;
            this.chkPlayRepeats.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPlayRepeats.Location = new System.Drawing.Point(106, 21);
            this.chkPlayRepeats.Name = "chkPlayRepeats";
            this.chkPlayRepeats.Size = new System.Drawing.Size(116, 17);
            this.chkPlayRepeats.TabIndex = 8;
            this.chkPlayRepeats.Text = "Play repeats / links";
            this.chkPlayRepeats.UseVisualStyleBackColor = true;
            // 
            // chkLoop
            // 
            this.chkLoop.AutoSize = true;
            this.chkLoop.Checked = true;
            this.chkLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLoop.Location = new System.Drawing.Point(106, 44);
            this.chkLoop.Name = "chkLoop";
            this.chkLoop.Size = new System.Drawing.Size(93, 17);
            this.chkLoop.TabIndex = 9;
            this.chkLoop.Text = "Playback loop";
            this.chkLoop.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Arial Black", 13.75F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(56, 19);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(44, 42);
            this.btnStop.TabIndex = 7;
            this.btnStop.Text = "■";
            this.btnStop.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Font = new System.Drawing.Font("Arial Black", 11.75F, System.Drawing.FontStyle.Bold);
            this.btnPlay.Location = new System.Drawing.Point(6, 19);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(44, 42);
            this.btnPlay.TabIndex = 6;
            this.btnPlay.Text = "►";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnAddAnimation
            // 
            this.btnAddAnimation.Location = new System.Drawing.Point(114, 6);
            this.btnAddAnimation.Name = "btnAddAnimation";
            this.btnAddAnimation.Size = new System.Drawing.Size(43, 20);
            this.btnAddAnimation.TabIndex = 1;
            this.btnAddAnimation.Text = "Add";
            this.btnAddAnimation.UseVisualStyleBackColor = true;
            this.btnAddAnimation.Click += new System.EventHandler(this.btnAddAnimation_Click);
            // 
            // txtBoxAnimName
            // 
            this.txtBoxAnimName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtBoxAnimName.Location = new System.Drawing.Point(6, 6);
            this.txtBoxAnimName.Name = "txtBoxAnimName";
            this.txtBoxAnimName.Size = new System.Drawing.Size(102, 20);
            this.txtBoxAnimName.TabIndex = 0;
            this.txtBoxAnimName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBoxAnimName_KeyDown);
            // 
            // listBoxAnimations
            // 
            this.listBoxAnimations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxAnimations.FormattingEnabled = true;
            this.listBoxAnimations.Location = new System.Drawing.Point(6, 32);
            this.listBoxAnimations.Name = "listBoxAnimations";
            this.listBoxAnimations.Size = new System.Drawing.Size(152, 251);
            this.listBoxAnimations.TabIndex = 2;
            this.listBoxAnimations.SelectedIndexChanged += new System.EventHandler(this.listBoxAnimations_SelectedIndexChanged);
            this.listBoxAnimations.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxAnimations_MouseDoubleClick);
            // 
            // btnStepDelete
            // 
            this.btnStepDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStepDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStepDelete.Location = new System.Drawing.Point(112, 261);
            this.btnStepDelete.Name = "btnStepDelete";
            this.btnStepDelete.Size = new System.Drawing.Size(46, 23);
            this.btnStepDelete.TabIndex = 20;
            this.btnStepDelete.Text = "X";
            this.btnStepDelete.UseVisualStyleBackColor = true;
            this.btnStepDelete.Click += new System.EventHandler(this.btnStepDelete_Click);
            // 
            // grpBoxAlign
            // 
            this.grpBoxAlign.Controls.Add(this.chkShowPrev);
            this.grpBoxAlign.Controls.Add(this.label3);
            this.grpBoxAlign.Controls.Add(this.txtTiming);
            this.grpBoxAlign.Controls.Add(this.label2);
            this.grpBoxAlign.Controls.Add(this.label1);
            this.grpBoxAlign.Controls.Add(this.chkAlignAll);
            this.grpBoxAlign.Controls.Add(this.txtAlignY);
            this.grpBoxAlign.Controls.Add(this.btnAlignDown);
            this.grpBoxAlign.Controls.Add(this.txtAlignX);
            this.grpBoxAlign.Controls.Add(this.btnAlignRight);
            this.grpBoxAlign.Controls.Add(this.btnAlignLeft);
            this.grpBoxAlign.Controls.Add(this.btnAlignUp);
            this.grpBoxAlign.Enabled = false;
            this.grpBoxAlign.Location = new System.Drawing.Point(164, 6);
            this.grpBoxAlign.Name = "grpBoxAlign";
            this.grpBoxAlign.Size = new System.Drawing.Size(284, 95);
            this.grpBoxAlign.TabIndex = 1;
            this.grpBoxAlign.TabStop = false;
            this.grpBoxAlign.Text = "Align && timing";
            // 
            // chkShowPrev
            // 
            this.chkShowPrev.AutoSize = true;
            this.chkShowPrev.Location = new System.Drawing.Point(136, 72);
            this.chkShowPrev.Name = "chkShowPrev";
            this.chkShowPrev.Size = new System.Drawing.Size(96, 17);
            this.chkShowPrev.TabIndex = 11;
            this.chkShowPrev.Text = "Show previous";
            this.chkShowPrev.UseVisualStyleBackColor = true;
            this.chkShowPrev.CheckedChanged += new System.EventHandler(this.chkShowPrev_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(228, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "T";
            // 
            // txtTiming
            // 
            this.txtTiming.Location = new System.Drawing.Point(248, 19);
            this.txtTiming.Name = "txtTiming";
            this.txtTiming.Size = new System.Drawing.Size(30, 20);
            this.txtTiming.TabIndex = 10;
            this.txtTiming.TextChanged += new System.EventHandler(this.txtTiming_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(172, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Y";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(116, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "X";
            // 
            // chkAlignAll
            // 
            this.chkAlignAll.AutoSize = true;
            this.chkAlignAll.Location = new System.Drawing.Point(136, 49);
            this.chkAlignAll.Name = "chkAlignAll";
            this.chkAlignAll.Size = new System.Drawing.Size(88, 17);
            this.chkAlignAll.TabIndex = 4;
            this.chkAlignAll.Text = "Mod all steps";
            this.chkAlignAll.UseVisualStyleBackColor = true;
            // 
            // txtAlignY
            // 
            this.txtAlignY.Location = new System.Drawing.Point(192, 19);
            this.txtAlignY.Name = "txtAlignY";
            this.txtAlignY.Size = new System.Drawing.Size(30, 20);
            this.txtAlignY.TabIndex = 8;
            this.txtAlignY.TextChanged += new System.EventHandler(this.txtAlignY_TextChanged);
            // 
            // btnAlignDown
            // 
            this.btnAlignDown.Font = new System.Drawing.Font("Arial Black", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignDown.Location = new System.Drawing.Point(44, 57);
            this.btnAlignDown.Name = "btnAlignDown";
            this.btnAlignDown.Size = new System.Drawing.Size(32, 32);
            this.btnAlignDown.TabIndex = 1;
            this.btnAlignDown.Text = "▼";
            this.btnAlignDown.UseVisualStyleBackColor = true;
            this.btnAlignDown.Click += new System.EventHandler(this.btnAlignDown_Click);
            // 
            // txtAlignX
            // 
            this.txtAlignX.Location = new System.Drawing.Point(136, 19);
            this.txtAlignX.Name = "txtAlignX";
            this.txtAlignX.Size = new System.Drawing.Size(30, 20);
            this.txtAlignX.TabIndex = 6;
            this.txtAlignX.TextChanged += new System.EventHandler(this.txtAlignX_TextChanged);
            // 
            // btnAlignRight
            // 
            this.btnAlignRight.Font = new System.Drawing.Font("Arial Black", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignRight.Location = new System.Drawing.Point(82, 57);
            this.btnAlignRight.Name = "btnAlignRight";
            this.btnAlignRight.Size = new System.Drawing.Size(32, 32);
            this.btnAlignRight.TabIndex = 3;
            this.btnAlignRight.Text = "►";
            this.btnAlignRight.UseVisualStyleBackColor = true;
            this.btnAlignRight.Click += new System.EventHandler(this.btnAlignRight_Click);
            // 
            // btnAlignLeft
            // 
            this.btnAlignLeft.Font = new System.Drawing.Font("Arial Black", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignLeft.Location = new System.Drawing.Point(6, 57);
            this.btnAlignLeft.Name = "btnAlignLeft";
            this.btnAlignLeft.Size = new System.Drawing.Size(32, 32);
            this.btnAlignLeft.TabIndex = 2;
            this.btnAlignLeft.Text = "◄";
            this.btnAlignLeft.UseVisualStyleBackColor = true;
            this.btnAlignLeft.Click += new System.EventHandler(this.btnAlignLeft_Click);
            // 
            // btnAlignUp
            // 
            this.btnAlignUp.Font = new System.Drawing.Font("Arial Black", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignUp.Location = new System.Drawing.Point(44, 19);
            this.btnAlignUp.Name = "btnAlignUp";
            this.btnAlignUp.Size = new System.Drawing.Size(32, 32);
            this.btnAlignUp.TabIndex = 0;
            this.btnAlignUp.Text = "▲";
            this.btnAlignUp.UseVisualStyleBackColor = true;
            this.btnAlignUp.Click += new System.EventHandler(this.btnAlignUp_Click);
            // 
            // listBoxSteps
            // 
            this.listBoxSteps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxSteps.FormattingEnabled = true;
            this.listBoxSteps.Location = new System.Drawing.Point(6, 6);
            this.listBoxSteps.Name = "listBoxSteps";
            this.listBoxSteps.Size = new System.Drawing.Size(152, 251);
            this.listBoxSteps.TabIndex = 0;
            this.listBoxSteps.SelectedIndexChanged += new System.EventHandler(this.listBoxSteps_SelectedIndexChanged);
            // 
            // btnStepUp
            // 
            this.btnStepUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStepUp.Location = new System.Drawing.Point(6, 261);
            this.btnStepUp.Name = "btnStepUp";
            this.btnStepUp.Size = new System.Drawing.Size(47, 23);
            this.btnStepUp.TabIndex = 18;
            this.btnStepUp.Text = "▲";
            this.btnStepUp.UseVisualStyleBackColor = true;
            this.btnStepUp.Click += new System.EventHandler(this.btnStepUp_Click);
            // 
            // btnStepDown
            // 
            this.btnStepDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStepDown.Location = new System.Drawing.Point(59, 261);
            this.btnStepDown.Name = "btnStepDown";
            this.btnStepDown.Size = new System.Drawing.Size(47, 23);
            this.btnStepDown.TabIndex = 19;
            this.btnStepDown.Text = "▼";
            this.btnStepDown.UseVisualStyleBackColor = true;
            this.btnStepDown.Click += new System.EventHandler(this.btnStepDown_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOpen,
            this.toolStripSeparator2,
            this.toolStripButtonSave,
            this.toolStripButtonExport,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripComboBoxScale,
            this.toolStripButtonZoomOut,
            this.toolStripButtonZoomIn,
            this.toolStripSeparator3,
            this.toolStripLabel2,
            this.cboBoxGroup,
            this.btnBoxColor,
            this.btnAddBoxGroup,
            this.btnBoxShow});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1140, 25);
            this.toolStrip1.TabIndex = 38;
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
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSave.Enabled = false;
            this.toolStripButtonSave.Image = global::Animator.Properties.Resources.disk;
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSave.Text = "Save";
            this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSave_Click);
            // 
            // toolStripButtonExport
            // 
            this.toolStripButtonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonExport.Enabled = false;
            this.toolStripButtonExport.Image = global::Animator.Properties.Resources.brick_go;
            this.toolStripButtonExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonExport.Name = "toolStripButtonExport";
            this.toolStripButtonExport.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonExport.Text = "Export";
            this.toolStripButtonExport.Click += new System.EventHandler(this.toolStripButtonExport_Click);
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
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(70, 22);
            this.toolStripLabel2.Text = "Box groups:";
            // 
            // cboBoxGroup
            // 
            this.cboBoxGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBoxGroup.Name = "cboBoxGroup";
            this.cboBoxGroup.Size = new System.Drawing.Size(121, 25);
            this.cboBoxGroup.SelectedIndexChanged += new System.EventHandler(this.cboBoxGroup_SelectedIndexChanged);
            // 
            // btnBoxColor
            // 
            this.btnBoxColor.AutoSize = false;
            this.btnBoxColor.BackColor = System.Drawing.Color.Silver;
            this.btnBoxColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.btnBoxColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBoxColor.Name = "btnBoxColor";
            this.btnBoxColor.Size = new System.Drawing.Size(30, 20);
            this.btnBoxColor.ToolTipText = "Group color";
            this.btnBoxColor.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // btnAddBoxGroup
            // 
            this.btnAddBoxGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddBoxGroup.Image = global::Animator.Properties.Resources.shape_square_add;
            this.btnAddBoxGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddBoxGroup.Name = "btnAddBoxGroup";
            this.btnAddBoxGroup.Size = new System.Drawing.Size(23, 22);
            this.btnAddBoxGroup.Click += new System.EventHandler(this.btnAddBoxGroup_Click_1);
            // 
            // btnBoxShow
            // 
            this.btnBoxShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnBoxShow.Image = ((System.Drawing.Image)(resources.GetObject("btnBoxShow.Image")));
            this.btnBoxShow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBoxShow.Name = "btnBoxShow";
            this.btnBoxShow.Size = new System.Drawing.Size(75, 22);
            this.btnBoxShow.Text = "Show group";
            this.btnBoxShow.Click += new System.EventHandler(this.btnBoxShow_Click);
            // 
            // tabAnimations
            // 
            this.tabAnimations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabAnimations.Controls.Add(this.tabPageAnimation);
            this.tabAnimations.Controls.Add(this.tabPageSteps);
            this.tabAnimations.Enabled = false;
            this.tabAnimations.Location = new System.Drawing.Point(666, 206);
            this.tabAnimations.Name = "tabAnimations";
            this.tabAnimations.SelectedIndex = 0;
            this.tabAnimations.Size = new System.Drawing.Size(462, 310);
            this.tabAnimations.TabIndex = 39;
            // 
            // tabPageAnimation
            // 
            this.tabPageAnimation.Controls.Add(this.grpBoxPlayback);
            this.tabPageAnimation.Controls.Add(this.grpBoxAnimDetails);
            this.tabPageAnimation.Controls.Add(this.txtBoxAnimName);
            this.tabPageAnimation.Controls.Add(this.btnAddAnimation);
            this.tabPageAnimation.Controls.Add(this.listBoxAnimations);
            this.tabPageAnimation.Location = new System.Drawing.Point(4, 22);
            this.tabPageAnimation.Name = "tabPageAnimation";
            this.tabPageAnimation.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAnimation.Size = new System.Drawing.Size(454, 284);
            this.tabPageAnimation.TabIndex = 0;
            this.tabPageAnimation.Text = "Animations";
            this.tabPageAnimation.UseVisualStyleBackColor = true;
            // 
            // grpBoxPlayback
            // 
            this.grpBoxPlayback.Controls.Add(this.btnPlay);
            this.grpBoxPlayback.Controls.Add(this.btnStop);
            this.grpBoxPlayback.Controls.Add(this.chkPlayRepeats);
            this.grpBoxPlayback.Controls.Add(this.chkLoop);
            this.grpBoxPlayback.Location = new System.Drawing.Point(164, 124);
            this.grpBoxPlayback.Name = "grpBoxPlayback";
            this.grpBoxPlayback.Size = new System.Drawing.Size(284, 154);
            this.grpBoxPlayback.TabIndex = 4;
            this.grpBoxPlayback.TabStop = false;
            this.grpBoxPlayback.Text = "Animation preview";
            // 
            // tabPageSteps
            // 
            this.tabPageSteps.Controls.Add(this.grpBoxBoxes);
            this.tabPageSteps.Controls.Add(this.btnStepDelete);
            this.tabPageSteps.Controls.Add(this.listBoxSteps);
            this.tabPageSteps.Controls.Add(this.grpBoxAlign);
            this.tabPageSteps.Controls.Add(this.btnStepDown);
            this.tabPageSteps.Controls.Add(this.btnStepUp);
            this.tabPageSteps.Location = new System.Drawing.Point(4, 22);
            this.tabPageSteps.Name = "tabPageSteps";
            this.tabPageSteps.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSteps.Size = new System.Drawing.Size(454, 284);
            this.tabPageSteps.TabIndex = 1;
            this.tabPageSteps.Text = "Steps";
            this.tabPageSteps.UseVisualStyleBackColor = true;
            // 
            // grpBoxBoxes
            // 
            this.grpBoxBoxes.Controls.Add(this.button17);
            this.grpBoxBoxes.Controls.Add(this.btnAddBox);
            this.grpBoxBoxes.Controls.Add(this.button16);
            this.grpBoxBoxes.Controls.Add(this.button15);
            this.grpBoxBoxes.Controls.Add(this.button14);
            this.grpBoxBoxes.Controls.Add(this.boxGrid);
            this.grpBoxBoxes.Location = new System.Drawing.Point(164, 107);
            this.grpBoxBoxes.Name = "grpBoxBoxes";
            this.grpBoxBoxes.Size = new System.Drawing.Size(284, 171);
            this.grpBoxBoxes.TabIndex = 21;
            this.grpBoxBoxes.TabStop = false;
            this.grpBoxBoxes.Text = "Boxes";
            // 
            // button17
            // 
            this.button17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button17.Location = new System.Drawing.Point(214, 142);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(46, 23);
            this.button17.TabIndex = 25;
            this.button17.Text = "Fill";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // btnAddBox
            // 
            this.btnAddBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddBox.Location = new System.Drawing.Point(6, 142);
            this.btnAddBox.Name = "btnAddBox";
            this.btnAddBox.Size = new System.Drawing.Size(46, 23);
            this.btnAddBox.TabIndex = 24;
            this.btnAddBox.Text = "Add";
            this.btnAddBox.UseVisualStyleBackColor = true;
            this.btnAddBox.Click += new System.EventHandler(this.btnAddBox_Click_1);
            // 
            // button16
            // 
            this.button16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button16.Location = new System.Drawing.Point(58, 142);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(46, 23);
            this.button16.TabIndex = 23;
            this.button16.Text = "X";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            this.button15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button15.Location = new System.Drawing.Point(162, 142);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(46, 23);
            this.button15.TabIndex = 22;
            this.button15.Text = "Paste";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // button14
            // 
            this.button14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button14.Location = new System.Drawing.Point(110, 142);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(46, 23);
            this.button14.TabIndex = 21;
            this.button14.Text = "Copy";
            this.button14.UseVisualStyleBackColor = true;
            // 
            // boxGrid
            // 
            this.boxGrid.AllowUserToAddRows = false;
            this.boxGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.boxGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.boxX,
            this.boxY,
            this.boxW,
            this.boxH});
            this.boxGrid.Location = new System.Drawing.Point(6, 19);
            this.boxGrid.Name = "boxGrid";
            this.boxGrid.Size = new System.Drawing.Size(272, 117);
            this.boxGrid.TabIndex = 8;
            this.boxGrid.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.boxGrid_CellValidated);
            this.boxGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.boxGrid_CellValidating);
            this.boxGrid.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.boxGrid_RowsRemoved);
            // 
            // boxX
            // 
            this.boxX.HeaderText = "boxX";
            this.boxX.Name = "boxX";
            this.boxX.Width = 50;
            // 
            // boxY
            // 
            this.boxY.HeaderText = "boxY";
            this.boxY.Name = "boxY";
            this.boxY.Width = 50;
            // 
            // boxW
            // 
            this.boxW.HeaderText = "boxW";
            this.boxW.Name = "boxW";
            this.boxW.Width = 50;
            // 
            // boxH
            // 
            this.boxH.HeaderText = "boxH";
            this.boxH.Name = "boxH";
            this.boxH.Width = 50;
            // 
            // lblDebug
            // 
            this.lblDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDebug.AutoSize = true;
            this.lblDebug.Location = new System.Drawing.Point(669, 12);
            this.lblDebug.Name = "lblDebug";
            this.lblDebug.Size = new System.Drawing.Size(35, 13);
            this.lblDebug.TabIndex = 46;
            this.lblDebug.Text = "label5";
            // 
            // Animator
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 524);
            this.Controls.Add(this.lblDebug);
            this.Controls.Add(this.tabAnimations);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.grpSprite);
            this.Controls.Add(this.scrollH);
            this.Controls.Add(this.scrollV);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1156, 563);
            this.Name = "Animator";
            this.Text = "Animator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Animator_FormClosing);
            this.Load += new System.EventHandler(this.Animator_Load);
            this.Shown += new System.EventHandler(this.Animator_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Animator_DragDrop);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.Animator_DragOver);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Animator_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Animator_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Animator_MouseDown);
            this.MouseLeave += new System.EventHandler(this.Animator_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Animator_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Animator_MouseUp);
            this.Resize += new System.EventHandler(this.Animator_Resize);
            this.grpSprite.ResumeLayout(false);
            this.grpFrameDetails.ResumeLayout(false);
            this.grpFrameDetails.PerformLayout();
            this.grpBoxAnimDetails.ResumeLayout(false);
            this.grpBoxAnimDetails.PerformLayout();
            this.grpBoxAlign.ResumeLayout(false);
            this.grpBoxAlign.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabAnimations.ResumeLayout(false);
            this.tabPageAnimation.ResumeLayout(false);
            this.tabPageAnimation.PerformLayout();
            this.grpBoxPlayback.ResumeLayout(false);
            this.grpBoxPlayback.PerformLayout();
            this.tabPageSteps.ResumeLayout(false);
            this.grpBoxBoxes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.boxGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.VScrollBar scrollV;
        private System.Windows.Forms.HScrollBar scrollH;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ListBox listBoxFrames;
        private System.Windows.Forms.GroupBox grpSprite;
        private System.Windows.Forms.GroupBox grpFrameDetails;
        private System.Windows.Forms.Label lblFrameHeight;
        private System.Windows.Forms.Label lblFrameWidth;
        private System.Windows.Forms.Button btnAddAnimation;
        private System.Windows.Forms.TextBox txtBoxAnimName;
        private System.Windows.Forms.ListBox listBoxAnimations;
        private System.Windows.Forms.TextBox txtAlignX;
        private System.Windows.Forms.ListBox listBoxSteps;
        private System.Windows.Forms.GroupBox grpBoxAlign;
        private System.Windows.Forms.TextBox txtAlignY;
        private System.Windows.Forms.Button btnAlignDown;
        private System.Windows.Forms.Button btnAlignRight;
        private System.Windows.Forms.Button btnAlignLeft;
        private System.Windows.Forms.Button btnAlignUp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkAlignAll;
        private System.Windows.Forms.GroupBox grpBoxAnimDetails;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTiming;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblTotalTiming;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.ComboBox comboLink;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkShowPrev;
        private System.Windows.Forms.TextBox txtRepeats;
        private System.Windows.Forms.Label lblRepeat;
        private System.Windows.Forms.CheckBox chkPlayRepeats;
        private System.Windows.Forms.CheckBox chkLoop;
        private System.Windows.Forms.Label lblRepeatInfo;
        private System.Windows.Forms.Button btnStepUp;
        private System.Windows.Forms.Button btnStepDown;
        private System.Windows.Forms.Button btnStepDelete;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxScale;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomOut;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomIn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonExport;
        private System.Windows.Forms.TabControl tabAnimations;
        private System.Windows.Forms.TabPage tabPageAnimation;
        private System.Windows.Forms.TabPage tabPageSteps;
        private System.Windows.Forms.Label lblCName;
        private System.Windows.Forms.GroupBox grpBoxBoxes;
        private System.Windows.Forms.Label lblDebug;
        private System.Windows.Forms.DataGridView boxGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn boxX;
        private System.Windows.Forms.DataGridViewTextBoxColumn boxY;
        private System.Windows.Forms.DataGridViewTextBoxColumn boxW;
        private System.Windows.Forms.DataGridViewTextBoxColumn boxH;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ToolStripComboBox cboBoxGroup;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton btnAddBoxGroup;
        private System.Windows.Forms.ToolStripButton btnBoxColor;
        private System.Windows.Forms.GroupBox grpBoxPlayback;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.ToolStripButton btnBoxShow;
        private System.Windows.Forms.Button btnAddBox;
        private System.Windows.Forms.Button button17;
    }
}


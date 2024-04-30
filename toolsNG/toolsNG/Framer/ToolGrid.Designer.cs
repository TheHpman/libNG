namespace Framer
{
    partial class ToolGrid
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.comboOrientation = new System.Windows.Forms.ComboBox();
            this.txtBoxWidth = new System.Windows.Forms.TextBox();
            this.txtBoxHeight = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBoxCenterY = new System.Windows.Forms.TextBox();
            this.txtBoxCenterX = new System.Windows.Forms.TextBox();
            this.chkBoxCenter = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(93, 131);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(84, 21);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(183, 131);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 21);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // comboOrientation
            // 
            this.comboOrientation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOrientation.FormattingEnabled = true;
            this.comboOrientation.Items.AddRange(new object[] {
            "Left-right,  top down",
            "Top-down, left-right"});
            this.comboOrientation.Location = new System.Drawing.Point(83, 38);
            this.comboOrientation.Name = "comboOrientation";
            this.comboOrientation.Size = new System.Drawing.Size(184, 21);
            this.comboOrientation.TabIndex = 2;
            // 
            // txtBoxWidth
            // 
            this.txtBoxWidth.Location = new System.Drawing.Point(73, 12);
            this.txtBoxWidth.Name = "txtBoxWidth";
            this.txtBoxWidth.Size = new System.Drawing.Size(66, 20);
            this.txtBoxWidth.TabIndex = 3;
            this.txtBoxWidth.Text = "8";
            // 
            // txtBoxHeight
            // 
            this.txtBoxHeight.Location = new System.Drawing.Point(201, 12);
            this.txtBoxHeight.Name = "txtBoxHeight";
            this.txtBoxHeight.Size = new System.Drawing.Size(66, 20);
            this.txtBoxHeight.TabIndex = 4;
            this.txtBoxHeight.Text = "8";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Tile width:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(145, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Tile height:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Fill direction:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(144, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Center Y:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Center X:";
            // 
            // txtBoxCenterY
            // 
            this.txtBoxCenterY.Location = new System.Drawing.Point(201, 88);
            this.txtBoxCenterY.Name = "txtBoxCenterY";
            this.txtBoxCenterY.Size = new System.Drawing.Size(66, 20);
            this.txtBoxCenterY.TabIndex = 9;
            this.txtBoxCenterY.Text = "8";
            // 
            // txtBoxCenterX
            // 
            this.txtBoxCenterX.Location = new System.Drawing.Point(69, 88);
            this.txtBoxCenterX.Name = "txtBoxCenterX";
            this.txtBoxCenterX.Size = new System.Drawing.Size(66, 20);
            this.txtBoxCenterX.TabIndex = 8;
            this.txtBoxCenterX.Text = "8";
            // 
            // chkBoxCenter
            // 
            this.chkBoxCenter.AutoSize = true;
            this.chkBoxCenter.Location = new System.Drawing.Point(12, 65);
            this.chkBoxCenter.Name = "chkBoxCenter";
            this.chkBoxCenter.Size = new System.Drawing.Size(101, 17);
            this.chkBoxCenter.TabIndex = 12;
            this.chkBoxCenter.Text = "generate center";
            this.chkBoxCenter.UseVisualStyleBackColor = true;
            // 
            // ToolGrid
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(279, 164);
            this.ControlBox = false;
            this.Controls.Add(this.chkBoxCenter);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtBoxCenterY);
            this.Controls.Add(this.txtBoxCenterX);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBoxHeight);
            this.Controls.Add(this.txtBoxWidth);
            this.Controls.Add(this.comboOrientation);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "ToolGrid";
            this.Text = "ToolGrid";
            this.Load += new System.EventHandler(this.ToolGrid_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox comboOrientation;
        private System.Windows.Forms.TextBox txtBoxWidth;
        private System.Windows.Forms.TextBox txtBoxHeight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBoxCenterY;
        private System.Windows.Forms.TextBox txtBoxCenterX;
        private System.Windows.Forms.CheckBox chkBoxCenter;
    }
}
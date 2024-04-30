using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Framer
{
    public partial class ToolGrid : Form
    {
        public DialogResult result;
        public int cellW, cellH, cellDir, centerX, centerY;
        public bool generateCenter = false;


        public ToolGrid()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if ((generateCenter = chkBoxCenter.Checked) &&
                !(int.TryParse(txtBoxCenterX.Text, out centerX)
                && int.TryParse(txtBoxCenterY.Text, out centerY)))
            {
                MessageBox.Show("Nope");
                return;
            }

            if (int.TryParse(txtBoxWidth.Text, out cellW)
                && int.TryParse(txtBoxHeight.Text, out cellH))
            {
                if (MessageBox.Show("This will replace all current sprites, continue?", "Generate cells", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                    return;
                cellDir = comboOrientation.SelectedIndex;
                result = DialogResult.OK;
                this.Close();
            }
            else MessageBox.Show("Nope");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            result = DialogResult.Cancel;
        }

        private void ToolGrid_Load(object sender, EventArgs e)
        {
            txtBoxHeight.Text = cellH.ToString();
            txtBoxWidth.Text = cellW.ToString();
            comboOrientation.SelectedIndex = cellDir;
            chkBoxCenter.Checked = generateCenter;
            txtBoxCenterX.Text = centerX.ToString();
            txtBoxCenterY.Text = centerY.ToString();
            if (comboOrientation.SelectedIndex == -1)
                comboOrientation.SelectedIndex = 0;
        }
    }
}

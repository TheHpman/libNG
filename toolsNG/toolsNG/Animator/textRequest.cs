using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Animator
{
    public partial class textRequest : Form
    {
        public string text;
        //private string textBckp;

        public textRequest()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            text = txtBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textRequest_Shown(object sender, EventArgs e)
        {
            txtBox.Text = text;
            txtBox.SelectAll();
            //textBckp = text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //text = textBckp;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Framer
{
    public partial class frameTool : Form
    {
        public Framer parent = null;
        frame sourceFrame = null;
        Bitmap sourceBmp = null;
        frame workFrame = new frame();
        List<chunk> chunks = new List<chunk>(0);
        List<int> selectedChunks = new List<int>(0);
        bool closeWarning = false;
        Rectangle newOutline;

        public frameTool(Bitmap _bmp, frame _frame = null)
        {
            InitializeComponent();
            if (((sourceFrame = _frame) != null) && ((sourceBmp = _bmp) != null))
            {
                foreach (sprite spr in sourceFrame.sprites)
                {
                    workFrame.sprites.Add(new sprite(spr.posX, spr.posY, spr.width, spr.height));

                    chunk chk = new chunk(256 - (spr.width / 2), 256 - (spr.width / 2), spr.width, spr.height);

                    Bitmap bmp = new Bitmap(spr.width, spr.height, PixelFormat.Format32bppArgb);
                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(sourceBmp, new Rectangle(0, 0, spr.width, spr.height), spr.posX, spr.posY, spr.width, spr.height, GraphicsUnit.Pixel);
                    g.Dispose();
                    bmp.MakeTransparent(Color.Fuchsia);

                    Bitmap bmp2 = new Bitmap(bmp);
                    bmp2.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    Bitmap bmp3 = new Bitmap(bmp);
                    bmp3.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    Bitmap bmp4 = new Bitmap(bmp);
                    bmp4.RotateFlip(RotateFlipType.RotateNoneFlipXY);

                    chk.stamps.Add(bmp);
                    chk.stamps.Add(bmp2);
                    chk.stamps.Add(bmp3);
                    chk.stamps.Add(bmp4);

                    if (_frame.customLayout)
                    {
                        chk.planeX = 256 - (_frame.customOutline.Width / 2) + spr.relocateX;
                        chk.planeY = 256 - (_frame.customOutline.Height / 2) + spr.relocateY;
                    }
                    chunks.Add(chk);
                }
            }
            rebuildPreview();
            dataGridChunks.Rows.Clear();
            foreach (chunk chk in chunks)
                dataGridChunks.Rows.Add(chk.stamps[0], chk.flipX, chk.flipY);
            foreach (DataGridViewRow row in dataGridChunks.Rows)
                row.Height = 128;
        }

        private void frameTool_Load(object sender, EventArgs e)
        {
            stripScaleComboBox.SelectedIndex = 0;
        }

        private void stripBtnScaleUp_Click(object sender, EventArgs e)
        {
            if (stripScaleComboBox.SelectedIndex < 3)
                stripScaleComboBox.SelectedIndex++;
        }

        private void stripBtnScaleDown_Click(object sender, EventArgs e)
        {
            if (stripScaleComboBox.SelectedIndex > 0)
                stripScaleComboBox.SelectedIndex--;
        }

        private void stripScaleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            rescale(1 << (stripScaleComboBox.SelectedIndex & 3));
        }

        private void rescale(int newScale)
        {
            pictBox.Size = new Size(512 * newScale, 512 * newScale);
        }

        private void pictBox_Resize(object sender, EventArgs e)
        {
            adjustPictBox();
        }
        private void btnResetStamps_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Reset layout?\nThis operation cannot be undone.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                foreach (chunk chk in chunks)
                {
                    chk.planeX = 256 - (chk.width / 2);
                    chk.planeY = 256 - (chk.height / 2);
                    chk.flipX = chk.flipY = false;
                }
                rebuildPreview();
            }
        }

        private void chkBoxOutlines_CheckedChanged(object sender, EventArgs e)
        {
            rebuildPreview();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void frameTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(closeWarning)
                if (MessageBox.Show("Discard changes?", "Abort mission", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                    e.Cancel = true;
        }

        private void adjustPictBox()
        {
            int posX = 0, posY = 0;
            if (pictBox.Size.Width < pictBoxPanel.Width)
                posX = (pictBoxPanel.Width - pictBox.Size.Width) / 2;
            if (pictBox.Size.Height < pictBoxPanel.Height)
                posY = (pictBoxPanel.Height - pictBox.Size.Height) / 2;

            pictBox.Location = new Point(posX, posY);
        }

        private void pictBoxPanel_SizeChanged(object sender, EventArgs e)
        {
            adjustPictBox();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // apply change to source frame
            sourceFrame.customLayout = true;
            sourceFrame.customOutline = new Rectangle(0, 0, newOutline.Width, newOutline.Height);

            for (int x = 0; x < chunks.Count; x++)
            {
                sourceFrame.sprites[x].relocateX = chunks[x].planeX - newOutline.X;
                sourceFrame.sprites[x].relocateY = chunks[x].planeY - newOutline.Y;
            }

            // close form
            closeWarning = false;
            this.Close();
        }

        private void rebuildPreview()
        {
            Bitmap bmp = new Bitmap(512, 512, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);

            g.FillRectangle(Brushes.Fuchsia, 0, 0, bmp.Width, bmp.Height);

            int minX = 1000, maxX = -1000, minY = 1000, maxY = -1000;
            foreach (chunk chk in chunks)
            {
                minX = Math.Min(minX, chk.planeX);
                maxX = Math.Max(maxX, chk.planeX + chk.width);
                minY = Math.Min(minY, chk.planeY);
                maxY = Math.Max(maxY, chk.planeY + chk.height);
            }
            newOutline = new Rectangle(minX, minY, maxX - minX, maxY - minY);

            if (chkBoxOutlines.Checked)
                g.DrawRectangle(Pens.Cyan, minX, minY, maxX - minX - 1, maxY - minY - 1);

            for (int x = 0; x < chunks.Count; x++)
            {
                chunk chk = chunks[x];

                g.DrawImage(chk.stamps[(chk.flipX ? 1 : 0) + (chk.flipY ? 2 : 0)], new Rectangle(chk.planeX, chk.planeY, chk.width, chk.height), 0, 0, chk.width, chk.height, GraphicsUnit.Pixel);
                if (chkBoxOutlines.Checked && selectedChunks.Contains(x))
                    g.DrawRectangle(Pens.Yellow, new Rectangle(chk.planeX, chk.planeY, chk.width - 1, chk.height - 1));
            }

            g.Dispose();
            pictBox.Image = bmp;
        }

        private void dataGridChunks_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            int x = e.RowIndex;
            chunks[x].flipX = Convert.ToBoolean(dataGridChunks.Rows[x].Cells[1].Value);
            chunks[x].flipY = Convert.ToBoolean(dataGridChunks.Rows[x].Cells[2].Value);
            closeWarning = true;
            rebuildPreview();
        }

        private void dataGridChunks_SelectionChanged(object sender, EventArgs e)
        {
            selectedChunks = new List<int>(0);

            foreach (DataGridViewRow row in dataGridChunks.SelectedRows)
                selectedChunks.Add(row.Index);
            rebuildPreview();
        }

        private void shiftChunks(int shiftX, int shiftY)
        {
            for (int x = 0; x < chunks.Count; x++)
                if (selectedChunks.Contains(x))
                {
                    chunks[x].planeX += shiftX;
                    chunks[x].planeY += shiftY;
                }
            closeWarning = true;
            rebuildPreview();
        }

        private void frameTool_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if ((e.Modifiers == Keys.Shift) || (e.Modifiers == (Keys.Shift | Keys.Control)))
                    {
                        shiftChunks(0, e.Modifiers == Keys.Shift ? -1 : -8);
                        e.Handled = true;
                    }
                    break;
                case Keys.Down:
                    if ((e.Modifiers == Keys.Shift) || (e.Modifiers == (Keys.Shift | Keys.Control)))
                    {
                        shiftChunks(0, e.Modifiers == Keys.Shift ? 1 : 8);
                        e.Handled = true;
                    }
                    break;
                case Keys.Left:
                    if ((e.Modifiers == Keys.Shift) || (e.Modifiers == (Keys.Shift | Keys.Control)))
                    {
                        shiftChunks(e.Modifiers == Keys.Shift ? -1 : -8, 0);
                        e.Handled = true;
                    }
                    break;
                case Keys.Right:
                    if ((e.Modifiers == Keys.Shift) || (e.Modifiers == (Keys.Shift | Keys.Control)))
                    {
                        shiftChunks(e.Modifiers == Keys.Shift ? 1 : 8, 0);
                        e.Handled = true;
                    }
                    break;
                case Keys.Add:
                    stripBtnScaleUp.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Subtract:
                    stripBtnScaleDown.PerformClick();
                    e.Handled = true;
                    break;
            }
        }
    }

    public class chunk
    {
        public int planeX = 0, planeY = 0;
        public int width, height;
        public bool flipX = false, flipY = false;
        public List<Bitmap> stamps = new List<Bitmap>(0);
        public chunk(int x, int y, int w, int h) { planeX = x; planeY = y; width = w; height = h; }
    }
}

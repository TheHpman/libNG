using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Xml;
using System.Threading;
using NeoTools;
using System.Runtime.InteropServices;

namespace Animator
{
    public partial class Animator : Form
    {
        private List<frame> frameList = null;
        private List<animation> animationList = null;
        private string appName = "libNG's Animator";
        private int scale = 1;
        private static int areaWidth = 640;
        private static int areaHeigth = 480;
        private static int previewWidth = 640;
        private static int previewHeigth = 480;
        private int originX = areaWidth / 2;
        private int originY = areaHeigth / 2;
        private int currAnimation = -1;
        private int currStep = -1;
        private int timerCounter, playedAnim, playedStep;
        private bool offsetXAutoFill = false;
        private bool offsetYAutoFill = false;
        private bool timingAutoFill = false;
        private ImageAttributes att = null;
        private int repeats = 0;
        private int frameCounter = 0;
        private delegate void redrawDelegate();
        private Thread animatorThread = null;
        private bool animating = false;
        private Rectangle drawArea;
        private SolidBrush originBrush = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        private SolidBrush originLineBrush = new SolidBrush(Color.FromArgb(32, 255, 0, 0));
        private Region clipping;

        //box stuff
        private int cursorX, cursorY;
        private bool cursorIntersect = false;
        private Rectangle previewArea;
        private int selectionBoxX, selectionBoxY, selectionBoxW, selectionBoxH;
        private bool mouseDown = false;
        private bool boxRefill = false;
        private List<boxGroup> boxGroupList = new List<boxGroup>(0);
        private bool showAllBoxes = false;

        //mouse
        private int clickX, clickY;
        private int mouseFrameX, mouseFrameY;
        mouseModes mouseMode = mouseModes.MOUSE_BOX;

        //WIP
        private bool showBoxes = false;
        public Animator()
        {
            InitializeComponent();
        }

        /*****************
         * CONTROL EVENTS
         * ***************/
        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                openFolder(folderBrowserDialog1.SelectedPath);
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveData();
        }

        private void toolStripButtonExport_Click(object sender, EventArgs e)
        {
            exportData();
        }

        private void toolStripComboBoxScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBoxScale.SelectedIndex)
            {
                case 0: scale = 1;
                    break;
                case 1: scale = 2;
                    break;
                case 2: scale = 4;
                    break;
                case 3: scale = 8;
                    break;
            }
            rescale();
        }

        private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxScale.SelectedIndex > 0)
                toolStripComboBoxScale.SelectedIndex--;
        }

        private void toolStripButtonZoomIn_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxScale.SelectedIndex < 3)
                toolStripComboBoxScale.SelectedIndex++;
        }

        private void scrollH_ValueChanged(object sender, EventArgs e)
        {
            redrawPreview();
        }

        private void scrollV_ValueChanged(object sender, EventArgs e)
        {
            redrawPreview();
        }

        private void listBoxFrames_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxAnimations.SelectedIndex != -1) //animation selected
            {
                animationList[listBoxAnimations.SelectedIndex].addStep(new animStep(frameList[listBoxFrames.SelectedIndex].ID, listBoxFrames.SelectedIndex, boxGroupList.Count));
                refillStepsList();
                showAnimDetails();
            }
        }

        private void listBoxFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            int h, w;

            h = frameList[listBoxFrames.SelectedIndex].bmp.Height;
            w = frameList[listBoxFrames.SelectedIndex].bmp.Width;

            lblFrameWidth.Text = string.Format("Width: {0} px ({1} tile{2})", w, w / 16, (w / 16) > 1 ? "s" : "");
            lblFrameHeight.Text = string.Format("Height: {0} px ({1} tile{2})", h, h / 16, (h / 16) > 1 ? "s" : "");
            grpFrameDetails.Invalidate();
        }

        private void txtBoxAnimName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                addAnimation();
                e.Handled = true;
            }
        }

        private void btnAddAnimation_Click(object sender, EventArgs e)
        {
            addAnimation();
        }

        private void listBoxAnimations_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            tabAnimations.SelectedIndex = 1;
        }

        private void listBoxAnimations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAnimations.SelectedIndex >= 0)
            {
                //enable & fill steps group
                currAnimation = listBoxAnimations.SelectedIndex;
                currStep = -1;
                tabAnimations.TabPages[1].Text = "Steps - " + animationList[currAnimation].ID;
                refillStepsList();
                showAnimDetails();
                grpBoxAlign.Enabled = false;
            }
            else
            {
                tabAnimations.TabPages[1].Text = "Steps";

                currAnimation = -1;
                currStep = -1;
            }
        }

        private void txtRepeats_TextChanged(object sender, EventArgs e)
        {
            int x;

            if (int.TryParse(txtRepeats.Text, out x))
            {
                if (x >= 0)
                {
                    animationList[currAnimation].repeats = x;
                }
            }
        }

        private void comboLink_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboLink.SelectedIndex == 0) //none
            {
                if (currAnimation != -1)
                    animationList[currAnimation].link = "";
            }
            else
            {
                animationList[currAnimation].link = comboLink.SelectedItem.ToString();
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            startPlayback();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            stopPlayback();
        }

        private void listBoxSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            currStep = listBoxSteps.SelectedIndex;
            showCurrStep();
        }

        private void btnStepUp_Click(object sender, EventArgs e)
        {
            int x;
            List<animStep> list = null;
            animStep step = null;

            x = listBoxSteps.SelectedIndex;
            if (x > 0)
            {
                list = animationList[currAnimation].getSteps();
                step = list[x];
                list.RemoveAt(x);
                list.Insert(x - 1, step);
                refillStepsList();
                listBoxSteps.SelectedIndex = x - 1;
            }
        }

        private void btnStepDown_Click(object sender, EventArgs e)
        {
            int x;
            List<animStep> list = null;
            animStep step = null;

            x = listBoxSteps.SelectedIndex;
            if (x < listBoxSteps.Items.Count - 1)
            {
                list = animationList[currAnimation].getSteps();
                step = list[x];
                list.RemoveAt(x);
                list.Insert(x + 1, step);
                refillStepsList();
                listBoxSteps.SelectedIndex = x + 1;
            }
        }

        private void btnStepDelete_Click(object sender, EventArgs e)
        {
            int x = listBoxSteps.SelectedIndex;

            if (x >= 0)
            {
                animationList[currAnimation].getSteps().RemoveAt(x);
                refillStepsList();
                listBoxSteps.SelectedIndex = x < listBoxSteps.Items.Count ? x : (listBoxSteps.Items.Count - 1);
            }
        }

        private void btnAlignUp_Click(object sender, EventArgs e)
        {
            int x;

            if (chkAlignAll.Checked)
            {
                for (x = 0; x < animationList[currAnimation].steps.Count; x++)
                {
                    animationList[currAnimation].steps[x].offsetY--;
                }
            }
            else
            {
                animationList[currAnimation].steps[currStep].offsetY--;
            }
            showCurrStep();
        }

        private void btnAlignLeft_Click(object sender, EventArgs e)
        {
            int x;

            if (chkAlignAll.Checked)
            {
                for (x = 0; x < animationList[currAnimation].steps.Count; x++)
                {
                    animationList[currAnimation].steps[x].offsetX--;
                }
            }
            else
            {
                animationList[currAnimation].steps[currStep].offsetX--;
            }
            showCurrStep();
        }

        private void btnAlignRight_Click(object sender, EventArgs e)
        {
            int x;

            if (chkAlignAll.Checked)
            {
                for (x = 0; x < animationList[currAnimation].steps.Count; x++)
                {
                    animationList[currAnimation].steps[x].offsetX++;
                }
            }
            else
            {
                animationList[currAnimation].steps[currStep].offsetX++;
            }
            showCurrStep();
        }

        private void btnAlignDown_Click(object sender, EventArgs e)
        {
            int x;

            if (chkAlignAll.Checked)
            {
                for (x = 0; x < animationList[currAnimation].steps.Count; x++)
                {
                    animationList[currAnimation].steps[x].offsetY++;
                }
            }
            else
            {
                animationList[currAnimation].steps[currStep].offsetY++;
            }
            showCurrStep();
        }

        private void txtAlignX_TextChanged(object sender, EventArgs e)
        {
            int x, i;

            if (offsetXAutoFill) { offsetXAutoFill = false; return; }

            if (int.TryParse(txtAlignX.Text, out x))
            {
                if (chkAlignAll.Checked)
                {
                    for (i = 0; i < animationList[currAnimation].steps.Count; i++)
                    {
                        animationList[currAnimation].steps[i].offsetX = x;
                    }
                }
                else
                {
                    animationList[currAnimation].steps[currStep].offsetX = x;
                }
            }
            redrawPreview();
        }

        private void txtAlignY_TextChanged(object sender, EventArgs e)
        {
            int x, i;

            if (offsetYAutoFill) { offsetYAutoFill = false; return; }

            if (int.TryParse(txtAlignY.Text, out x))
            {
                if (chkAlignAll.Checked)
                {
                    for (i = 0; i < animationList[currAnimation].steps.Count; i++)
                    {
                        animationList[currAnimation].steps[i].offsetY = x;
                    }
                }
                else
                {
                    animationList[currAnimation].steps[currStep].offsetY = x;
                }
            }
            redrawPreview();
        }

        private void txtTiming_TextChanged(object sender, EventArgs e)
        {
            int x, i;

            if (timingAutoFill) { timingAutoFill = false; return; }

            if (int.TryParse(txtTiming.Text, out x))
            {
                if (x > 0)
                {
                    if (chkAlignAll.Checked)
                    {
                        for (i = 0; i < animationList[currAnimation].steps.Count; i++)
                        {
                            animationList[currAnimation].steps[i].duration = x;
                        }
                    }
                    else
                    {
                        animationList[currAnimation].steps[currStep].duration = x;
                    }
                }
            }
            redrawPreview();
        }

        private void chkShowPrev_CheckedChanged(object sender, EventArgs e)
        {
            redrawPreview();
        }



        /*****************
         * OTHER EVENTS
         * ***************/
        private void Animator_Load(object sender, EventArgs e)
        {
            float[][] matrixItems = { new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1, 0, 0, 0 }, new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, 0, 0, 0.5f, 0 }, new float[] { 0, 0, 0, 0, 1 } };
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
            att = new ImageAttributes();
            att.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            //this.Size = new Size(1140 + 10, 550 + 11);
            this.Text = appName;
            drawArea = new Rectangle(scrollH.Location.X, scrollV.Location.Y, areaWidth, areaHeigth);
            clipping = new Region(drawArea);
            toolStripComboBoxScale.SelectedIndex = 0;

            if(!showBoxes)
            {
                toolStripLabel2.Visible = false;
                cboBoxGroup.Visible = false;
                btnBoxColor.Visible = false;
                btnAddBoxGroup.Visible = false;
                btnBoxShow.Visible = false;
                grpBoxBoxes.Visible = false;
            }
#if !DEBUG
            lblDebug.Visible = false;
#endif
        }

        private void Animator_Shown(object sender, EventArgs e)
        {
            comboLink.SelectedIndex = 0;
            folderBrowserDialog1.SelectedPath = Directory.GetCurrentDirectory();
            previewArea = new Rectangle(scrollH.Location.X, scrollV.Location.Y, scrollH.Width, scrollV.Height);
        }

        private void Animator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (animatorThread != null) animatorThread.Abort();
        }

        private void Animator_AltCtrlKeyDown(ref object sender, ref KeyEventArgs e)
        {
            int shiftX = 0;
            int shiftY = 0;
            if (boxGrid.SelectedRows.Count == 0) return;
            switch (e.KeyCode)
            {
                case Keys.Up: shiftY--; e.Handled = true; break;
                case Keys.Down: shiftY++; e.Handled = true; break;
                case Keys.Left: shiftX--; e.Handled = true; break;
                case Keys.Right: shiftX++; e.Handled = true; break;
            }

            boxRefill = true;
            for (int i = 0; i < boxGrid.SelectedRows.Count; i++)
            {
                int x = boxGrid.SelectedRows[i].Index;
                animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].x += shiftX;
                animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].y += shiftY;

                boxGrid.Rows[x].Cells["boxX"].Value = int.Parse(boxGrid.Rows[x].Cells["boxX"].Value.ToString()) + shiftX;
                boxGrid.Rows[x].Cells["boxY"].Value = int.Parse(boxGrid.Rows[x].Cells["boxY"].Value.ToString()) + shiftY;
            }
            boxRefill = false;
            redraw();
        }

        private void Animator_AltKeyDown(ref object sender, ref KeyEventArgs e)
        {
            int shiftX = 0, shiftY = 0;
            if (boxGrid.SelectedRows.Count == 0) return;
            switch (e.KeyCode)
            {
                case Keys.Up: shiftY--; e.Handled = true; break;
                case Keys.Down: shiftY++; e.Handled = true; break;
                case Keys.Left: shiftX--; e.Handled = true; break;
                case Keys.Right: shiftX++; e.Handled = true; break;
            }

            boxRefill = true;
            for (int i = 0; i < boxGrid.SelectedRows.Count; i++)
            {
                int x = boxGrid.SelectedRows[i].Index;
                animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].width += shiftX;
                if (animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].width < 1) animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].width = 1;
                animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].height += shiftY;
                if (animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].height < 1) animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].height = 1;

                boxGrid.Rows[x].Cells["boxW"].Value = animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].width;
                boxGrid.Rows[x].Cells["boxH"].Value = animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][x].height;
            }
            boxRefill = false;
            redraw();
        }

        private void Animator_KeyDown(object sender, KeyEventArgs e)
        {
            int modX = 0, modY = 0;

            if (e.Modifiers == (Keys.Alt|Keys.Control)) { Animator_AltCtrlKeyDown(ref sender, ref e); return; }
            if (e.Modifiers == Keys.Alt) { Animator_AltKeyDown(ref sender, ref e); return; }

            switch (e.KeyCode)
            {
                case Keys.Down: if (e.Modifiers == Keys.Shift) modY = 1; else return;
                    break;
                case Keys.Up: if (e.Modifiers == Keys.Shift) modY = -1; else return;
                    break;
                case Keys.Left: if (e.Modifiers == Keys.Shift) modX = -1; else return;
                    break;
                case Keys.Right: if (e.Modifiers == Keys.Shift) modX = 1; else return;
                    break;
                case Keys.Space: if (animatorThread != null) stopPlayback(); else startPlayback(); e.Handled = true;
                    break;
                default: return;
            }

            if ((tabAnimations.SelectedIndex == 1) && (listBoxSteps.SelectedIndex != -1))
            {
                if (chkAlignAll.Checked)
                {
                    for (int x = 0; x < animationList[currAnimation].steps.Count; x++)
                    {
                        animationList[currAnimation].steps[x].offsetX += modX;
                        animationList[currAnimation].steps[x].offsetY += modY;
                    }
                }
                else
                {
                    animationList[currAnimation].steps[currStep].offsetX += modX;
                    animationList[currAnimation].steps[currStep].offsetY += modY;
                }
                e.Handled = true;
                showCurrStep();
            }
        }

        private void Animator_Paint(object sender, PaintEventArgs e)
        {
            int frameNum;
            Graphics gfx = e.Graphics;
            Rectangle dest, source;
            Bitmap bmp = null;
            int x, y;
            animStep step = null;

            //gfx.Clip = new Region(new Rectangle(scrollH.Location.X, scrollV.Location.Y, areaWidth, areaHeight));
            gfx.PixelOffsetMode = PixelOffsetMode.Half;
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.Clip = clipping;
            //compute origin position
            if (scrollH.Width > areaWidth * scale)
                x = scrollH.Location.X + (scrollH.Width / 2);
            else x = (originX * scale) - (scrollH.Value * scale) + scrollH.Location.X;
            if (scrollV.Height > areaHeigth * scale)
                y = scrollV.Location.Y + (scrollV.Height / 2);
            else y = (originY * scale) - (scrollV.Value * scale) + scrollV.Location.Y;

            //draw origin
            gfx.FillRectangle(originLineBrush, new Rectangle(x, 0 - (scrollV.Value * scale) + scrollV.Location.Y, scale, previewHeigth * scale)); //|
            gfx.FillRectangle(originLineBrush, new Rectangle(scrollH.Location.X - (scrollH.Value * scale), y, previewWidth * scale, scale)); //-
            gfx.FillRectangle(originBrush, new Rectangle(x, y, scale, scale)); //.

            if (animating == true) //running on timer
            {
                step = animationList[playedAnim].steps[playedStep];
                frameNum = step.frameNum;
                bmp = frameList[frameNum].bmp;
                source = new Rectangle(0, 0, bmp.Width, bmp.Height);
                dest = new Rectangle(x + (step.offsetX * scale), y + (step.offsetY * scale), bmp.Width * scale, bmp.Height * scale);
                gfx.DrawImage(frameList[frameNum].bmp, dest, source, GraphicsUnit.Pixel);
                gfx.DrawString(string.Format("Playback - frame {0}", frameCounter), new Font("Terminal", 10), Brushes.Black, new Point(scrollH.Location.X, scrollV.Location.Y));

            }
            else //display work one
            {
                if (currAnimation > -1 && currStep > -1) //has to draw frame
                {
                    if (currStep > 0 && chkShowPrev.Checked)
                    {
                        step = animationList[currAnimation].steps[currStep - 1];
                        frameNum = step.frameNum;
                        bmp = frameList[frameNum].bmp;
                        source = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        dest = new Rectangle(x + (step.offsetX * scale), y + (step.offsetY * scale), bmp.Width * scale, bmp.Height * scale);
                        //gfx.DrawImage(frameList[frameNum].getBmp(), dest, source, GraphicsUnit.Pixel,);
                        gfx.DrawImage(bmp, dest, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, att);
                    }

                    step = animationList[currAnimation].steps[currStep];
                    frameNum = step.frameNum;
                    bmp = frameList[frameNum].bmp;
                    source = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    dest = new Rectangle(x + (step.offsetX * scale), y + (step.offsetY * scale), bmp.Width * scale, bmp.Height * scale);
                    gfx.DrawImage(frameList[frameNum].bmp, dest, source, GraphicsUnit.Pixel);


                    //Paint boxes
                    if (cboBoxGroup.SelectedIndex >= 0)
                    {
                        Color clr = boxGroupList[cboBoxGroup.SelectedIndex].color;
                        Pen boxPen = new Pen(clr);
                        //boxPen = new Pen(clr, scale);
                        Brush boxBrush = new SolidBrush(Color.FromArgb(64, clr.R, clr.G, clr.B));

                        gfx.PixelOffsetMode = PixelOffsetMode.None;
                        foreach (box b in animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex])
                        {
                            gfx.FillRectangle(boxBrush, x + (b.x * scale), y + (b.y * scale), (b.width * scale) - 1, (b.height * scale) - 1);
                            gfx.DrawRectangle(boxPen, x + (b.x * scale), y + (b.y * scale), (b.width * scale) - 1, (b.height * scale) - 1);
                            //Pen p=new Pen()
                        }
                    }

                }
                else
                {   //TODO:display nothing

                }
            }

            //paint boxes

            if ((selectionBoxH != 0) && (selectionBoxW != 0))
            {
                gfx.PixelOffsetMode = PixelOffsetMode.None;
                gfx.FillRectangle(new SolidBrush(Color.FromArgb(64, 255, 0, 255)), x + (selectionBoxX * scale), y + (selectionBoxY * scale), (selectionBoxW * scale) - 1, (selectionBoxH * scale) - 1);
                gfx.DrawRectangle(Pens.Fuchsia, x + (selectionBoxX * scale), y + (selectionBoxY * scale), (selectionBoxW * scale) - 1, (selectionBoxH * scale) - 1);
            }
        }


        /*****************
         * OTHER CODE
         * ***************/
        private void redrawPreview()
        {
            this.Invalidate(drawArea);
        }
        
        private int getFrameNum(string id)
        {
            int x;

            for (x = 0; x < frameList.Count; x++)
            {
                if (frameList[x].ID == id) return (x);
            }
            return (-1);
        }

        private int getAnimNum(string ID)
        {
            int x;

            for (x = 0; x < animationList.Count; x++)
            {
                if (animationList[x].ID == ID) return (x);
            }
            return (-1);
        }

        private void refillFramesList(List<frame> list)
        {
            int x;

            listBoxFrames.Items.Clear();
            for (x = 0; x < list.Count; x++)
            {
                listBoxFrames.Items.Add(list[x].ID);
            }
            if (list.Count > 0) tabAnimations.Enabled = true;
            else tabAnimations.Enabled = false;
        }

        private void refillAnimationsList()
        {
            int i;

            listBoxAnimations.Items.Clear();
            for (i = 0; i < animationList.Count; i++)
            {
                listBoxAnimations.Items.Add(animationList[i].ID);
            }
        }

        private void refillStepsList()
        {
            List<animStep> steps = null;
            int i;

            listBoxSteps.Items.Clear();
            if (listBoxAnimations.SelectedIndex < 0) return;

            steps = animationList[listBoxAnimations.SelectedIndex].getSteps();
            for (i = 0; i < steps.Count; i++)
            {
                listBoxSteps.Items.Add(steps[i].frameID);
            }

        }

        private void grpFrameDetails_Paint(object sender, PaintEventArgs e)
        {
            Graphics gfx = e.Graphics;
            Bitmap bmp = null;

            gfx.PixelOffsetMode = PixelOffsetMode.Half;
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;

            if (listBoxFrames.SelectedItem != null)
            {
                int x;

                bmp = frameList[listBoxFrames.SelectedIndex].bmp;
                //gfx.DrawImage(frameList[listBoxFrames.SelectedIndex].getBmp(), new Rectangle(5, 14, 128, 128), new Rectangle(0, 0, frameList[listBoxFrames.SelectedIndex].getBmp().Width, frameList[listBoxFrames.SelectedIndex].getBmp().Height), GraphicsUnit.Pixel);
                //gfx.FillRectangle(Brushes.OliveDrab, new Rectangle(5, 14, 128, 128));
                if (bmp.Width < bmp.Height)
                {
                    x = (int)(((float)bmp.Width / (float)bmp.Height) * 128);
                    //gfx.FillRectangle(Brushes.LightSkyBlue, new Rectangle(5 + ((128 - x) / 2), 14, x, 128));
                    gfx.DrawImage(bmp, new Rectangle(5 + ((128 - x) / 2), 14, x, 128), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                }
                else
                {
                    x = (int)(((float)bmp.Height / (float)bmp.Width) * 128);
                    //gfx.FillRectangle(Brushes.LightSkyBlue, new Rectangle(5, 14 + ((128 - x) / 2), 128, x));
                    gfx.DrawImage(bmp, new Rectangle(5, 14 + ((128 - x) / 2), 128, x), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                }
            }
        }

        private void addAnimation()
        {
            //if (!Regex.IsMatch(txtBoxAnimName.Text, @"[A-Za-z0-9_]$"))
            if (txtBoxAnimName.Text == "")
            {
                MessageBox.Show("Invalid name.", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int i = 0; i < animationList.Count; i++)
            {
                if (txtBoxAnimName.Text.ToUpper() == animationList[i].ID)
                {
                    MessageBox.Show("Duplicate name.", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            animationList.Add(new animation(txtBoxAnimName.Text.ToUpper()));
            comboLink.Items.Add(txtBoxAnimName.Text.ToUpper());
            txtBoxAnimName.Clear();
            refillAnimationsList();
        }

        private void showAnimDetails()
        {
            string baseName = frameList[0].ID.Substring(0, frameList[0].ID.LastIndexOf("_")).ToUpper();

            lblTotalTiming.Text = string.Format("Total timing: {0} frames", animationList[currAnimation].totalDuration());
            lblSteps.Text = string.Format("Steps: {0}", animationList[currAnimation].steps.Count);
            txtRepeats.Text = animationList[currAnimation].repeats.ToString();
            if (animationList[currAnimation].link == "") //no link
                comboLink.SelectedIndex = 0;
            else comboLink.SelectedItem = animationList[currAnimation].link;

            lblCName.Text = "ID: " + baseName + "_ANIM_" + animationList[listBoxAnimations.SelectedIndex].ID;
        }

        private void showCurrStep()
        {
            if (currStep > -1)
            {
                if (txtAlignX.Text != animationList[currAnimation].steps[currStep].offsetX.ToString())
                {
                    offsetXAutoFill = true;
                    txtAlignX.Text = animationList[currAnimation].steps[currStep].offsetX.ToString();
                }
                if (txtAlignY.Text != animationList[currAnimation].steps[currStep].offsetY.ToString())
                {
                    offsetYAutoFill = true;
                    txtAlignY.Text = animationList[currAnimation].steps[currStep].offsetY.ToString();
                }
                if (txtTiming.Text != animationList[currAnimation].steps[currStep].duration.ToString())
                {
                    timingAutoFill = true;
                    txtTiming.Text = animationList[currAnimation].steps[currStep].duration.ToString();
                }
                grpBoxAlign.Enabled = true;
                showStepBoxes();
            }
            else
            {
                grpBoxAlign.Enabled = false;
            }
            this.Invalidate();
        }

        private void showStepBoxes()
        {
            int x = cboBoxGroup.SelectedIndex;

            if (x < 0) return;
            if (currAnimation == -1) return;
            if (currStep == -1) return;

            //fill box grid
            boxRefill = true;

            //clear table
            while (boxGrid.Rows.Count > (boxGrid.AllowUserToAddRows == true ? 1 : 0))
                boxGrid.Rows.Remove(boxGrid.Rows[0]);

            if (animationList[currAnimation].steps[currStep].boxes.Count > x)
                foreach (box bx in animationList[currAnimation].steps[currStep].boxes[x])
                {
                    boxGrid.Rows.Add(bx.x, bx.y, bx.width, bx.height);
                }

            boxRefill = false;
        }

        private void rescale()
        {
            if (areaWidth * scale > scrollH.Width)
            {
                scrollH.Maximum = ((areaWidth * scale) - scrollH.Width) / scale;
                scrollH.Maximum = scrollH.Maximum - 1 + scrollH.LargeChange;
                // scrollH.Value = scale == 2 ? 160 :
                //     scale == 4 ? 240 : 280;
                scrollH.Value = ((areaWidth * scale) - previewWidth) / scale / 2;
                //640 640*2= (1280-640)/2/2
                //2: 160
                //4: 240
                //8: 280
                scrollH.Enabled = true;
            }
            else
            {
                scrollH.Enabled = false;
                scrollH.Value = 0;
            }
            if (areaHeigth * scale > scrollV.Height)
            {
                scrollV.Maximum = ((areaHeigth * scale) - scrollV.Height) / scale;
                scrollV.Maximum = scrollV.Maximum - 1 + scrollV.LargeChange;
                //    scrollV.Value = scale == 2 ? 120 :
                //        scale == 4 ? 180 : 210;
                scrollV.Value = ((areaHeigth * scale) - previewHeigth) / scale / 2;
                scrollV.Enabled = true;
            }
            else
            {
                scrollV.Enabled = false;
                scrollV.Value = 0;
            }
            redrawPreview();
        }

        void resetState()
        {
            stopPlayback();
            currAnimation = -1;
            currStep = -1;
            listBoxSteps.Items.Clear();
            tabAnimations.SelectedIndex = 0;
            tabAnimations.TabPages[1].Text = "Steps";
            grpBoxAlign.Enabled = false;
            //refillAnimationsList();
            //refillStepsList();
            listBoxAnimations.Items.Clear();
            cboBoxGroup.Items.Clear();
            boxGrid.Rows.Clear();
        }

        private void openFolder(string path, bool folderDrop = false)
        {
            int i, j;
            string[] files;
            string tmpstr;
            XmlDocument doc = new XmlDocument();
            bool foundfile = true;
            List<animStep> steps = null;
            int frameNum = 0;

            if (folderDrop)
                folderBrowserDialog1.SelectedPath = path;

            //if (/*folderBrowserDialog1.ShowDialog() == DialogResult.OK*/)
            if(Directory.Exists(path))
            {
                //stop current animation
                stopPlayback();

                //this.Text = folderBrowserDialog1.SelectedPath + " - " + appName;
                //files = Directory.GetFiles(folderBrowserDialog1.SelectedPath, "(????) *_????.png");
                this.Text = path + " - " + appName;
                files = Directory.GetFiles(path, "(????) *_????.png");
                Array.Sort(files, StringComparer.InvariantCulture);

                frameList = new List<frame>(0);
                animationList = new List<animation>(0);

                for (i = 0; i < files.Length; i++)
                {
                    tmpstr = files[i].Substring(files[i].LastIndexOf(") ") + 2, files[i].LastIndexOf(".png") - files[i].LastIndexOf(") ") - 2);
                    frameList.Add(new frame(files[i], tmpstr));
                }

                try
                {
                    //doc.Load(folderBrowserDialog1.SelectedPath + Path.DirectorySeparatorChar + "animdata.xml");
                    doc.Load(path + Path.DirectorySeparatorChar + "animdata.xml");
                }
                catch
                {
                    foundfile = false;
                }

                resetState();

                if (foundfile) //found animdata xml file
                {
                    XmlNodeList animNodes = null;
                    XmlNodeList boxGroups = null;
                    XmlNode docNode = doc.DocumentElement;


                    XmlNode animsNode = docNode.SelectSingleNode("animations");
                    if (animsNode != null)
                    {
                        //new ver.
                        animNodes = animsNode.SelectNodes("animation");
                        boxGroups = docNode.SelectSingleNode("boxGroups").SelectNodes("group");
                    }
                    else
                    {
                        //older ver. no box info
                        animNodes = docNode.SelectNodes("animation");
                    }

                    //resetState();

                    if (boxGroups != null)
                        foreach (XmlNode node in boxGroups)
                            addBoxGroup(node.Attributes["name"].Value, node.Attributes["color"].Value);

                    if (cboBoxGroup.SelectedIndex >= 0) btnBoxColor.BackColor = boxGroupList[cboBoxGroup.SelectedIndex].color;

                    for (i = 0; i < animNodes.Count; i++)
                    {
                        XmlNodeList stepNodes = animNodes[i].SelectNodes("step");
                        steps = new List<animStep>(0);
                        for (j = 0; j < stepNodes.Count; j++)
                        {
                            frameNum = getFrameNum(stepNodes[j].Attributes["frame"].Value);
                            if (frameNum == -1)
                            {
                                MessageBox.Show("Error loading xml file: unknown frame " + stepNodes[j].Attributes["frame"].Value + ".", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                animationList = new List<animation>(0);
                                refillAnimationsList();
                                refillFramesList(frameList);
                                return;
                            }
                            steps.Add(new animStep(stepNodes[j].Attributes["frame"].Value, frameNum, boxGroupList.Count, int.Parse(stepNodes[j].Attributes["x"].Value), int.Parse(stepNodes[j].Attributes["y"].Value), int.Parse(stepNodes[j].Attributes["t"].Value)));

                            XmlNodeList boxNodes = stepNodes[j].SelectNodes("box");
                            if (boxNodes != null)
                                foreach (XmlNode box in boxNodes)
                                    steps[steps.Count - 1].boxes[int.Parse(box.Attributes["group"].Value)].Add(new box(int.Parse(box.Attributes["x"].Value), int.Parse(box.Attributes["y"].Value), int.Parse(box.Attributes["w"].Value), int.Parse(box.Attributes["h"].Value)));
                        }
                        animationList.Add(new animation(animNodes[i].Attributes["name"].Value, animNodes[i].Attributes["link"].Value, steps, int.Parse(animNodes[i].Attributes["repeats"].Value)));
                        comboLink.Items.Add(animNodes[i].Attributes["name"].Value);
                    }
                    refillAnimationsList();
                }
                refillFramesList(frameList);

                toolStripButtonSave.Enabled = true;
                toolStripButtonExport.Enabled = true;
            }
            else
            {
                this.Text = appName;
            }
        }

        private void saveData()
        {
            int i, j;
            List<animStep> steps = null;
            XmlDocument doc = new XmlDocument();
            XmlNode decNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(decNode);

            XmlNode root = doc.CreateElement("data");
            doc.AppendChild(root);

            XmlNode boxGrpNode = doc.CreateElement("boxGroups");
            root.AppendChild(boxGrpNode);

            foreach (boxGroup bg in boxGroupList)
            {
                XmlNode boxNode = doc.CreateElement("group");
                XmlAttribute attr = doc.CreateAttribute("name");
                attr.Value = bg.name;
                boxNode.Attributes.Append(attr);

                attr = doc.CreateAttribute("color");
                //attr.Value = bg.color.Name;
                attr.Value = string.Format("{0:x8}", bg.color.ToArgb());
                boxNode.Attributes.Append(attr);

                //doc.AppendChild(boxNode);
                boxGrpNode.AppendChild(boxNode);
            }

            XmlNode framesNode = doc.CreateElement("animations");
            root.AppendChild(framesNode);

            for (i = 0; i < animationList.Count; i++)
            {
                XmlNode animNode = doc.CreateElement("animation");
                XmlAttribute attr = doc.CreateAttribute("name");
                attr.Value = animationList[i].ID;
                animNode.Attributes.Append(attr);

                attr = doc.CreateAttribute("repeats");
                attr.Value = animationList[i].repeats.ToString();
                animNode.Attributes.Append(attr);

                attr = doc.CreateAttribute("link");
                attr.Value = animationList[i].link;
                animNode.Attributes.Append(attr);

                steps = animationList[i].getSteps();

                for (j = 0; j < steps.Count; j++)
                {
                    XmlNode stepNode = doc.CreateElement("step");
                    XmlAttribute stepAttr = doc.CreateAttribute("frame");
                    stepAttr.Value = steps[j].frameID;
                    stepNode.Attributes.Append(stepAttr);

                    stepAttr = doc.CreateAttribute("x");
                    stepAttr.Value = steps[j].offsetX.ToString();
                    stepNode.Attributes.Append(stepAttr);

                    stepAttr = doc.CreateAttribute("y");
                    stepAttr.Value = steps[j].offsetY.ToString();
                    stepNode.Attributes.Append(stepAttr);

                    stepAttr = doc.CreateAttribute("t");
                    stepAttr.Value = steps[j].duration.ToString();
                    stepNode.Attributes.Append(stepAttr);

                    for (int g = 0; g < steps[j].boxes.Count; g++)
                    {
                        for (int b = 0; b < steps[j].boxes[g].Count; b++)
                        {
                            XmlNode boxNode = doc.CreateElement("box");
                            XmlAttribute boxAttr = doc.CreateAttribute("group");
                            boxAttr.Value = g.ToString();
                            boxNode.Attributes.Append(boxAttr);

                            boxAttr = doc.CreateAttribute("x");
                            boxAttr.Value = steps[j].boxes[g][b].x.ToString();
                            boxNode.Attributes.Append(boxAttr);

                            boxAttr = doc.CreateAttribute("y");
                            boxAttr.Value = steps[j].boxes[g][b].y.ToString();
                            boxNode.Attributes.Append(boxAttr);

                            boxAttr = doc.CreateAttribute("w");
                            boxAttr.Value = steps[j].boxes[g][b].width.ToString();
                            boxNode.Attributes.Append(boxAttr);

                            boxAttr = doc.CreateAttribute("h");
                            boxAttr.Value = steps[j].boxes[g][b].height.ToString();
                            boxNode.Attributes.Append(boxAttr);

                            stepNode.AppendChild(boxNode);
                        }
                    }


                    animNode.AppendChild(stepNode);
                }
                framesNode.AppendChild(animNode);
            }


            doc.Save(folderBrowserDialog1.SelectedPath + Path.DirectorySeparatorChar + "animdata.xml");
            MessageBox.Show("Saved.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exportData()
        {
            int i, j;
            // (moved macro defines to buildchar)
            //string asmOut = "\t.macro\t_ANM_END\n\t.word\t0xc000\n\t.endm\n\n" +
            //                "\t.macro\t_ANM_REPEAT _count\n\t.word\t0x8000, \\_count\n\t.endm\n\n"+
            //                "\t.macro\t_ANM_LINK _id _ptr\n\t.word\t0xa000, \\_id\n\t.long\t\\_ptr\n\t.endm\n\n"
            //    ;
            string asmOut = "";
            string asmOut2 = "";
            string cOut = "";
            string baseName;
            List<animStep> steps = null;

            baseName = frameList[0].ID.Substring(0, frameList[0].ID.LastIndexOf("_"));

            asmOut += string.Format("{0}_animations:" + Environment.NewLine, baseName);
            for (i = 0; i < animationList.Count; i++)
            {
                steps = animationList[i].getSteps();
                cOut += string.Format("#define {0}_ANIM_{1} {2}" + Environment.NewLine, baseName.ToUpper(), animationList[i].ID, i);
                //asmOut += string.Format("{0}_anim_{1}:\n\t.word\t0x{2:x4}, 0x{6:x4}\t;* {2} steps, {6} repeats\n\t.long\t{3}_anim_{4}_steps, {5}\t;* steplist, link\n",
                //    baseName, animationList[i].ID,      //label
                //    animationList[i].steps.Count,       //stepcount
                //    baseName, animationList[i].ID,      //steplist
                //    animationList[i].link == "" ? "0x00000000" : string.Format("{0}_anim_{1}", baseName, animationList[i].link),
                //    animationList[i].repeats);          //link
                asmOut += string.Format("\t.long\t{0}_anim_{1}_steps\t;* steplist" + Environment.NewLine,
                    baseName, animationList[i].ID      //steplist
                    );
                asmOut2 += string.Format("{0}_anim_{1}_steps:" + Environment.NewLine, baseName, animationList[i].ID);
                for (j = 0; j < steps.Count; j++)
                {
                    asmOut2 += string.Format("\t.long\t{0}\t;* frame ptr" + Environment.NewLine + "\t.word\t0x{1:x4}, 0x{2:x4}, 0x{3:x4}, 0x{4:x4}, 0x{5:x4}\t;* flipShiftX, shiftX, flipShiftY, shiftY, duration" + Environment.NewLine,
                        steps[j].frameID,
                        (short)-(frameList[steps[j].frameNum].bmp.Width + steps[j].offsetX - 1),
                        (short)steps[j].offsetX,
                        (short)-(frameList[steps[j].frameNum].bmp.Height + steps[j].offsetY - 1),
                        (short)steps[j].offsetY,
                        (short)steps[j].duration
                        );
                }
                // repeat/link info
            //    asmOut2 += string.Format("\t.word\t0x{0:x4}{1}{2}",
            //        (animationList[i].repeats == 0 ? 0 : 0x0100) + (animationList[i].link == "" ? 0 : 0x0200) + ((animationList[i].repeats == 0 && animationList[i].link == "") ? 0x8000 : 0), //flags
            //        animationList[i].repeats == 0 ? Environment.NewLine : string.Format(", 0x{0:x4}" + Environment.NewLine, animationList[i].repeats), //rpt count
            //        animationList[i].link == "" ? "" : string.Format("\t.word\t0x{2:x4}" + Environment.NewLine + "\t.long\t{0}_anim_{1}_steps" + Environment.NewLine, baseName, animationList[i].link, getAnimID(animationList[i].link))   //link
            //        );
                if (animationList[i].repeats > 0)
                    asmOut2 += string.Format("\t_ANM_REPEAT 0x{0:x4}\n", animationList[i].repeats);  //repeats
                if (animationList[i].link == "")
                    asmOut2 += string.Format("\t_ANM_END\n\n");   //no link
                else
                    asmOut2 += string.Format("\t_ANM_LINK 0x{2:x4} {0}_anim_{1}_steps\n\n", baseName, animationList[i].link, getAnimID(animationList[i].link));   //link
            }

            FileOps.strToFile(folderBrowserDialog1.SelectedPath + Path.DirectorySeparatorChar + baseName + ".h", cOut);
            FileOps.strToFile(folderBrowserDialog1.SelectedPath + Path.DirectorySeparatorChar + baseName + "_anims.s", asmOut.Replace("\n", Environment.NewLine) + Environment.NewLine + asmOut2.Replace("\n", Environment.NewLine));
            MessageBox.Show("Exported.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int getAnimID(string anim)
        {
            for(int x=0;x<animationList.Count;x++)
            {
                if (animationList[x].ID == anim) return x;
            }
            return -1;
        }

        private void startPlayback()
        {
            if (animating)
            {
                if (animatorThread != null) animatorThread.Abort();
                animating = false;
                redrawPreview();
            }

            if (!animating)
            {
                if (currAnimation == -1) return;
                if (animationList[currAnimation].steps.Count < 1) return;
                frameCounter = 0;
                timerCounter = 0;
                playedAnim = currAnimation;
                playedStep = 0;
                repeats = 0;
                redrawPreview();
                //timerAnim.Enabled = true;
                animating = true;
                animatorThread = new Thread(new ThreadStart(animThread));
                animatorThread.Start();
            }
        }

        private void refreshBoxGroups()
        {
            cboBoxGroup.Items.Clear();
            foreach(boxGroup bg in boxGroupList)
            {
                cboBoxGroup.Items.Add(bg.name);
            }
        }

        private void addBoxGroup(string grpName, string color="")
        {
            boxGroup bg = new boxGroup();
            bg.name = grpName;
            if (color == "")
            {
                bg.color = Color.Silver;
            }
            else
            {
                uint c;
                c = Convert.ToUInt32(color, 16);
                bg.color = Color.FromArgb(255, (int)((c >> 16) & 0xff), (int)((c >> 8) & 0xff), (int)(c & 0xff));
                //bg.color = Color.FromName(color);
            }

            boxGroupList.Add(bg);

            refreshBoxGroups();

            cboBoxGroup.SelectedIndex = cboBoxGroup.Items.Count - 1;

            if (animationList == null) return;
            foreach (animation a in animationList)
            {
                foreach(animStep s in a.steps)
                {
                    while (s.boxes.Count< cboBoxGroup.Items.Count)
                    {
                        s.boxes.Add(new List<box>(0));
                    }
                }
            }
        }

        private void btnAddBoxGroup_Click_1(object sender, EventArgs e)
        {
            textRequest dlg = new textRequest();
            dlg.Text = "Group name:";
            dlg.text = "Group" + boxGroupList.Count();
            dlg.ShowDialog();

            if (dlg.DialogResult == DialogResult.OK)
            {
                //add the new group
                string name = dlg.text;
                foreach (boxGroup bgi in boxGroupList)
                {
                    if (name == bgi.name)
                    {
                        MessageBox.Show("Group name already used.", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                addBoxGroup(name);
            }
        }

        private void redraw()
        {
            this.Invalidate(drawArea);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                if (cboBoxGroup.SelectedIndex != -1)
                {
                    boxGroupList[cboBoxGroup.SelectedIndex].color = colorDialog1.Color;
                    redraw();
                }
                btnBoxColor.BackColor = colorDialog1.Color;
            }
        }

        private void cboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnBoxColor.BackColor = boxGroupList[cboBoxGroup.SelectedIndex].color;
            showStepBoxes();
            redraw();
        }

        private void addBox(int BX, int BY, int BW, int BH, int group = 0, int anim = -1, int step = -1)
        {
            box bx = new box(BX, BY, BW, BH);

            if (anim == -1)
            {
                if ((currAnimation == -1) || (currStep == -1)) return;
                animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex].Add(bx);
                boxGrid.Rows.Add(BX, BY, BW, BH);
            }
            else
            {
                animationList[anim].steps[step].boxes[group].Add(bx);
            }
        }

        private void btnAddBox_Click(object sender, EventArgs e)
        {
            if ((selectionBoxW > 0) && (selectionBoxH > 0))
                addBox(selectionBoxX, selectionBoxY, selectionBoxW, selectionBoxH);
        }

        private void btnBoxShow_Click(object sender, EventArgs e)
        {
            btnBoxShow.Text = showAllBoxes ? "Show group" : "Show all";
            showAllBoxes = !showAllBoxes;
            redraw();
        }

        private void btnAddBox_Click_1(object sender, EventArgs e)
        {
            if ((selectionBoxW > 0) && (selectionBoxH > 0))
                addBox(selectionBoxX, selectionBoxY, selectionBoxW, selectionBoxH);
        }

        private void boxGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            short x;
            if (!short.TryParse(e.FormattedValue.ToString(), out x))
            {
                e.Cancel = true;
                return;
            }
        }

        private void boxGrid_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (boxRefill) return;
            if ((currAnimation == -1) || (currStep == -1)) return;
            animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][e.RowIndex].x = int.Parse(boxGrid.Rows[e.RowIndex].Cells["boxX"].Value.ToString());
            animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][e.RowIndex].y = int.Parse(boxGrid.Rows[e.RowIndex].Cells["boxY"].Value.ToString());
            animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][e.RowIndex].width = int.Parse(boxGrid.Rows[e.RowIndex].Cells["boxW"].Value.ToString());
            animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][e.RowIndex].height = int.Parse(boxGrid.Rows[e.RowIndex].Cells["boxH"].Value.ToString());
            redraw();
        }

        private void boxGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (boxRefill) return;
            //if ((currAnimation == -1) || (currStep == -1)) return;
            animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex].Remove(animationList[currAnimation].steps[currStep].boxes[cboBoxGroup.SelectedIndex][e.RowIndex]);
            redraw();
        }

        private void Animator_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (Directory.Exists(files[0]))
                openFolder(files[0], true);
        }

        private void Animator_DragOver(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            e.Effect = files.Count() == 1 ? DragDropEffects.All : DragDropEffects.None;
        }

        private void Animator_Resize(object sender, EventArgs e)
        {
            previewWidth = scrollH.Width;
            previewHeigth = scrollV.Height;
            previewArea = new Rectangle(scrollH.Location.X, scrollV.Location.Y, scrollH.Width, scrollV.Height);
            drawArea = new Rectangle(scrollH.Location.X, scrollV.Location.Y, previewWidth, previewHeigth);
            clipping = new Region(drawArea);
            rescale();
        }

        private void Animator_MouseLeave(object sender, EventArgs e)
        {
            cursorIntersect = false;
            lblDebug.Text = "-";
        }

        private void Animator_MouseDown(object sender, MouseEventArgs e)
        {
            if (cursorIntersect)
            {
                if (mouseDown)
                {
                    mouseDown = false;
                    mouseMode = mouseModes.MOUSE_DEFAULT;
                    return;
                }
                mouseDown = true;
                if (Control.ModifierKeys == Keys.Shift)
                {
                    if ((tabAnimations.SelectedIndex == 1) && (listBoxSteps.SelectedIndex != -1))
                    {
                        mouseMode = mouseModes.MOUSE_MOVEFRAME;
                        clickX = e.X;
                        clickY = e.Y;
                        mouseFrameX = animationList[currAnimation].steps[currStep].offsetX;
                        mouseFrameY = animationList[currAnimation].steps[currStep].offsetY;
                    }
                }
                else
                {
                    mouseMode = mouseModes.MOUSE_BOX;
                    selectionBoxX = cursorX;
                    selectionBoxY = cursorY;
                    selectionBoxW = selectionBoxH = 0;
                }
            }
        }

        private void Animator_MouseUp(object sender, MouseEventArgs e)
        {
            if (cursorIntersect)
            {
                mouseDown = false;
            }
        }

        private void Animator_MouseMove(object sender, MouseEventArgs e)
        {
            if (cursorIntersect = previewArea.IntersectsWith(new Rectangle(e.X, e.Y, 1, 1)))
            {
                int centerX, centerY;
                if (scrollH.Width > areaWidth * scale)
                    centerX = scrollH.Location.X + (scrollH.Width / 2);
                else centerX = (originX * scale) - (scrollH.Value * scale) + scrollH.Location.X;
                if (scrollV.Height > areaHeigth * scale)
                    centerY = scrollV.Location.Y + (scrollV.Height / 2);
                else centerY = (originY * scale) - (scrollV.Value * scale) + scrollV.Location.Y;
                cursorX = (e.X - centerX) / scale;
                cursorY = (e.Y - centerY) / scale;
                lblDebug.Text = cursorX + "/" + cursorY;

                if (mouseDown)
                {
                    switch(mouseMode)
                    {
                        case mouseModes.MOUSE_MOVEFRAME:
                            int offsetX = (clickX - e.X) / scale;
                            int offsetY = (clickY - e.Y) / scale;
                            if (chkAlignAll.Checked)
                                foreach (animStep step in animationList[currAnimation].steps)
                                {
                                    step.offsetX = mouseFrameX - offsetX;
                                    step.offsetY = mouseFrameY - offsetY;
                                }
                            else
                            {
                                animationList[currAnimation].steps[currStep].offsetX = mouseFrameX - offsetX;
                                animationList[currAnimation].steps[currStep].offsetY = mouseFrameY - offsetY;
                            }
                            redrawPreview();
                            break;
                        case mouseModes.MOUSE_BOX:
                            selectionBoxW = cursorX - selectionBoxX;
                            selectionBoxH = cursorY - selectionBoxY;
                            lblDebug.Text += "   " + selectionBoxW + "/" + selectionBoxH;
                            redrawPreview();
                            break;
                    }
                }
            }
            else lblDebug.Text = "-";
        }

        private void stopPlayback()
        {
            if (animatorThread != null) animatorThread.Abort();
            animating = false;
            animatorThread = null;
            redrawPreview();
        }

        private void animThread()
        {
            DateTime updateTime;
            int frmDelta;

            while (true)
            {
                frmDelta = animationList[playedAnim].steps[playedStep].duration;
                updateTime = DateTime.Now.AddMilliseconds(frmDelta * 17); //~17ms per frame
                
                while (DateTime.Now < updateTime) ; //wait it out
                frameCounter += frmDelta;
                if (++playedStep == animationList[playedAnim].getSteps().Count) //was last step, loop/repeat/link?
                {
                    if (!chkPlayRepeats.Checked) //don't play repeats/links
                    {
                        if (chkLoop.Checked) //loop playback
                        {
                            repeats = 0;
                            playedStep = 0;
                        }
                        else playedStep--;
                        this.Invoke(new redrawDelegate(this.redrawPreview));
                        continue;
                    }

                    if (animationList[playedAnim].repeats > repeats) //repeats?
                    {
                        repeats++;
                        playedStep = 0;
                    }
                    else //not a repeat, link?
                    {
                        if (animationList[playedAnim].link != "") //take link
                        {
                            playedAnim = getAnimNum(animationList[playedAnim].link);
                            repeats = 0;
                            playedStep = 0;
                        }
                        else //no link, loop?
                        {
                            if (chkLoop.Checked)
                            {
                                playedAnim = currAnimation;
                                playedStep = 0;
                                repeats = 0;
                            }
                            else playedStep--;
                        }
                    }
                }
                this.Invoke(new redrawDelegate(this.redrawPreview));
            }
        }

    }



    /****************
     * OTHER CLASSES
     * **************/
    public class frame
    {
        public string ID;
        public Bitmap bmp;

        public frame(string fileName, string id)
        {
            var buffer = File.ReadAllBytes(fileName);
            var ms = new MemoryStream(buffer);
            bmp = (Bitmap)Bitmap.FromStream(ms);
            //bmp = (Bitmap)new System.Drawing.Bitmap(fileName);
            ID = id;
        }
    }

    public class animation
    {
        public string ID;
        public List<animStep> steps = null;
        public string link;
        public int repeats;

        public animation(string id)
        {
            ID = id;
            steps = new List<animStep>(0);
            link = "";
            repeats = 0;
        }

        public animation(string id, string Link, int rpt = 0)
        {
            ID = id;
            link = Link;
            steps = new List<animStep>(0);
            repeats = rpt;
        }

        public animation(string id, string Link, List<animStep> stepList, int rpt = 0)
        {
            ID = id;
            link = Link;
            steps = stepList;
            repeats = rpt;
        }

        public List<animStep> getSteps() { return (steps); }
        //public animStep getStep(int x) { return (steps[x]); }
        //public int stepsCount() { return (steps.Count); }

        public void addStep(animStep step)
        {
            steps.Add(step);
        }

        public int totalDuration()
        {
            int total = 0;

            for (int x = 0; x < steps.Count; x++)
                total += steps[x].duration;
            return (total);
        }
    }

    public class animStep
    {
        public int frameNum;
        public string frameID;
        public int duration;
        public int offsetX;
        public int offsetY;
        public List<List<box>> boxes;

        public animStep(string strID, int num, int boxLists, int x = 0, int y = 0, int dur = 5)
        {
            frameID = strID;
            frameNum = num;
            offsetX = x;
            offsetY = y;
            duration = dur;

            boxes = new List<List<box>>(0);

            while(boxes.Count<boxLists)
            {
                boxes.Add(new List<box>(0));
            }
        }
    }

    public class box
    {
        public int x, y;
        public int width, height;

        public box(int boxX, int boxY, int boxW, int boxH)
        {
            x = boxX;
            y = boxY;
            width = boxW;
            height = boxH;
        }
    }

    public class boxGroup
    {
        public string name;
        public Color color;
    }

    enum mouseModes
    {
        MOUSE_DEFAULT,
        MOUSE_BOX,
        MOUSE_MOVEFRAME
    }
}
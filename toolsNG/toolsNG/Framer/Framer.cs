using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Drawing.Drawing2D;

namespace Framer
{
    enum systems
    {
        NeoGeo,
        Megadrive,
        SuperFamicom,
        PCEngine,
        CPS
    }

    public partial class Framer : Form
    {
        private XmlDocument doc = null;
        List<sprData> sheets = null;
        Rectangle drawLocation = new Rectangle(0, 0, 0, 0);
        int currentSheet = -1;
        int currentFrame = -1;
        int scale = 1;
        string fName = null;
        Rectangle mouseArea = new Rectangle(0, 0, 0, 0);
        int posX = 0, posY = 0;
        int hoveredTile = -1;
        int frameStartTile = -1;
        int frameEndTile = -1;
        bool mouseDown = false;
        SolidBrush frameBrush = new SolidBrush(Color.FromArgb(128, Color.DeepSkyBlue));
        SolidBrush maskBrush = new SolidBrush(Color.FromArgb(128, Color.OliveDrab));
        Pen maskPen = new Pen(Color.FromArgb(255, Color.OliveDrab));
        bool refilling = false;
        bool spriteRefilling = false;
        string appName = "libNG's Framer";
        bool gridMode = false;
        int hoverX = -1;
        int hoverY = -1;
        int selectSizeX = 0;
        int selectSizeY = 0;
        int selectOriginX = -1;
        int selectOriginY = -1;
        int sys = (int)systems.NeoGeo;
        public static int[] unitSize = { 16, 8, 8, 16, 16 };
        public static int[] unitMask = { 0x7ffffff0, 0x7ffffff8, 0x7ffffff8, 0x7ffffff0, 0x7ffffff0 };
        public static int[] unitShift = { 4, 3, 3, 4, 4 };
        ToolGrid tg = new ToolGrid();

        public Framer()
        {
            InitializeComponent();
            this.Text = appName;
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < sheets.Count; i++)
            {
                sheets[i].rebuildXml(doc);
            }
            doc.Save(fName);
            MessageBox.Show("Saved.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cbBoxSpriteList_SelectedIndexChanged(object sender, EventArgs e)
        {
            frameStartTile = -1;
            frameEndTile = -1;
            currentSheet = cbBoxSpriteList.SelectedIndex;
            selectSheet(currentSheet);
        }

        private void toolStripComboBoxScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBoxScale.SelectedIndex)
            {
                case 0:
                    rescale(1);
                    break;
                case 1:
                    rescale(2);
                    break;
                case 2:
                    rescale(4);
                    break;
                case 3:
                    rescale(8);
                    break;
            }
            this.Invalidate(drawLocation);
        }

        private void scrollH_ValueChanged(object sender, EventArgs e)
        {
            this.Invalidate(drawLocation);
            //label7.Text = scrollH.Value.ToString() + " - " + scrollV.Value.ToString();
        }

        private void scrollV_ValueChanged(object sender, EventArgs e)
        {
            this.Invalidate(drawLocation);
            //label7.Text = scrollH.Value.ToString() + " - " + scrollV.Value.ToString();
        }

        private void trackBarFrame_ValueChanged(object sender, EventArgs e)
        {
            frameBrush = new SolidBrush(Color.FromArgb(trackBarFrame.Value, panelFrameColor.BackColor/*colorDialog1.Color*/));
            this.Invalidate(drawLocation);
        }

        private void trackBarMask_ValueChanged(object sender, EventArgs e)
        {
            maskBrush = new SolidBrush(Color.FromArgb(trackBarMask.Value, panelMaskColor.BackColor));
            this.Invalidate(drawLocation);
        }

        private void panelFrameColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelFrameColor.BackColor;
            colorDialog1.ShowDialog();
            panelFrameColor.BackColor = colorDialog1.Color;
            frameBrush = new SolidBrush(Color.FromArgb(trackBarFrame.Value, colorDialog1.Color));
            this.Invalidate(drawLocation);
        }

        private void panelMaskColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelMaskColor.BackColor;
            colorDialog1.ShowDialog();
            panelMaskColor.BackColor = colorDialog1.Color;
            maskBrush = new SolidBrush(Color.FromArgb(trackBarMask.Value, colorDialog1.Color));
            maskPen = new Pen(Color.FromArgb(255, colorDialog1.Color));
            this.Invalidate(drawLocation);
        }

        private void _addSprite()
        {
            if (selectSizeX != 0)
                addSelectedSprite();
            else MessageBox.Show("Empty selection.", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        private void btnAddSprite_Click(object sender, EventArgs e)
        {
            _addSprite();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripComboBoxScale.SelectedIndex = 0;
            toolStripComboBoxSystem.SelectedIndex = 0;
            mouseArea.X = scrollH.Location.X;
            mouseArea.Y = scrollV.Location.Y;
            mouseArea.Width = scrollH.Width;
            mouseArea.Height = scrollV.Height;
        }

        private void selectFrame(int frm)
        {
            int x;
            //clear grid
            spriteRefilling = true;
            while (dataGridSprites.Rows.Count > (dataGridSprites.AllowUserToAddRows == true ? 1 : 0))
            {
                dataGridSprites.Rows.Remove(dataGridSprites.Rows[0]);
            }
            spriteRefilling = false;

            if (frm == -1) return;
            for (x = 0; x < sheets[currentSheet].frames[frm].sprites.Count; x++)
                dataGridSprites.Rows.Add(sheets[currentSheet].frames[frm].sprites[x].posX, sheets[currentSheet].frames[frm].sprites[x].posY, sheets[currentSheet].frames[frm].sprites[x].width, sheets[currentSheet].frames[frm].sprites[x].height);
        }

        private void dataGridFrames_SelectionChanged(object sender, EventArgs e)
        {
            int oldframe = currentFrame;
            currentFrame = -1;

            if (dataGridFrames.SelectedRows.Count > 0)
                currentFrame = dataGridFrames.SelectedRows[0].Index;

            if (currentFrame != oldframe)
                selectFrame(currentFrame);

            label9.Text = currentFrame.ToString();

            this.Invalidate(drawLocation);
        }

        private void dataGridFrames_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            ushort x;

            if (!ushort.TryParse(e.FormattedValue.ToString(), out x))
            {
                e.Cancel = true;
                return;
            }
            if (x < 0)
            {
                e.Cancel = true;
            }
        }

        private void dataGridFrames_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (currentSheet == -1) return;
            if (sheets[currentSheet].frames.Count - 1 < e.RowIndex) return;
            sheets[currentSheet].frames[e.RowIndex].posX = ushort.Parse(dataGridFrames.Rows[e.RowIndex].Cells[0].Value.ToString());
            sheets[currentSheet].frames[e.RowIndex].posY = ushort.Parse(dataGridFrames.Rows[e.RowIndex].Cells[1].Value.ToString());
            sheets[currentSheet].frames[e.RowIndex].width = ushort.Parse(dataGridFrames.Rows[e.RowIndex].Cells[2].Value.ToString());
            sheets[currentSheet].frames[e.RowIndex].height = ushort.Parse(dataGridFrames.Rows[e.RowIndex].Cells[3].Value.ToString());
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            int x = 0, y = 0;
            int hover = -1;

            //something loaded & mouse in display zone
            if ((currentSheet != -1) && (mouseArea.IntersectsWith(new Rectangle(e.Location.X, e.Location.Y, 1, 1))))
            {
                x = posX = e.Location.X - scrollH.Location.X + (scrollH.Value * scale);
                y = posY = e.Location.Y - scrollV.Location.Y + (scrollV.Value * scale);

                switch (scale)
                {
                    case 2:
                        { x >>= 1; y >>= 1; }
                        break;
                    case 4:
                        { x >>= 2; y >>= 2; }
                        break;
                    case 8:
                        { x >>= 3; y >>= 3; }
                        break;
                }

                hover = ((y >> unitShift[sys]) * sheets[currentSheet].tileWidth[sys]) + (x >> unitShift[sys]);
                hoverX = x;
                hoverY = y;
                if (x > sheets[currentSheet].bmp.Width - 1) hoverX = hoverY = hover = -1;
                if (y > sheets[currentSheet].bmp.Height - 1) hoverX = hoverY = hover = -1;
            }
            label9.Text = x.ToString() + " " + y.ToString() + " " + hover.ToString();


            //cursor
            if (gridMode)
            {
                //changed tile
                if (hover != hoveredTile)
                {
                    hoveredTile = hover;
                    if (mouseDown)
                    {
                        frameEndTile = hover;
                        if (hoverX >= selectOriginX) //expanding right side
                            selectSizeX = (hoverX & unitMask[sys]) - selectOriginX + unitSize[sys];
                        else selectSizeX = (hoverX & unitMask[sys]) - selectOriginX - unitSize[sys];

                        if (hoverY >= selectOriginY) //expanding down side
                            selectSizeY = (hoverY & unitMask[sys]) - selectOriginY + unitSize[sys];
                        else selectSizeY = (hoverY & unitMask[sys]) - selectOriginY - unitSize[sys];
                        //label7.Text = string.Format("{0}/{1}>{2}/{3}:{4}/{5}", selectOriginX, selectOriginY, x, y, selectSizeX, selectSizeY);
                    }
                }
            }
            else
            {
                //free mode
                if (mouseDown)
                {
                    if (hoverX >= selectOriginX)
                        selectSizeX = (hoverX - selectOriginX + unitSize[sys]) & unitMask[sys];
                    else selectSizeX = unchecked((hoverX - selectOriginX - unitSize[sys]) & (int)(unitMask[sys] | 0x80000000));

                    if (hoverY >= selectOriginY)
                        selectSizeY = (hoverY - selectOriginY + unitSize[sys]) & unitMask[sys];
                    else selectSizeY = unchecked((hoverY - selectOriginY - unitSize[sys]) & (int)(unitMask[sys] | 0x80000000));
                }
            }

            //clip selection
            switch (sys)
            {
                case (int)systems.NeoGeo:
                    break;
                case (int)systems.Megadrive:
                    if (selectSizeX > 32) selectSizeX = 32;
                    else if (selectSizeX < -32) selectSizeX = -32;
                    if (selectSizeY > 32) selectSizeY = 32;
                    else if (selectSizeY < -32) selectSizeY = -32;
                    break;
                case (int)systems.SuperFamicom:
                    int ssx = Math.Abs(selectSizeX);
                    if (ssx >= 64) ssx = 64;
                    else if (ssx >= 32) ssx = 32;
                    else if (ssx >= 16) ssx = 16;
                    else ssx = 8;
                    int ssy = Math.Abs(selectSizeY);
                    if (ssy >= 64) ssy = 64;
                    else if (ssy >= 32) ssy = 32;
                    else if (ssy >= 16) ssy = 16;
                    else ssy = 8;
                    if (ssx != ssy)
                    {
                        if (ssx > ssy) ssy = ssx;
                        else ssx = ssy;
                    }
                    selectSizeX = selectSizeX < 0 ? -ssx : ssx;
                    selectSizeY = selectSizeY < 0 ? -ssy : ssy;
                    break;
                case (int)systems.PCEngine:
                    ssx = Math.Abs(selectSizeX);
                    if (ssx >= 32) ssx = 32;
                    else ssx = 16;
                    ssy = Math.Abs(selectSizeY);
                    if (ssy >= 64) ssy = 64;
                    else if (ssy >= 32) ssy = 32;
                    else ssy = 16;
                    selectSizeX = selectSizeX < 0 ? -ssx : ssx;
                    selectSizeY = selectSizeY < 0 ? -ssy : ssy;
                    break;
                case (int)systems.CPS:
                    if (selectSizeX > 256) selectSizeX = 256;
                    else if (selectSizeX < -256) selectSizeX = -256;
                    if (selectSizeY > 256) selectSizeY = 256;
                    else if (selectSizeY < -256) selectSizeY = -256;
                    break;
            }
            this.Invalidate(drawLocation);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int y;

            mouseArea.X = scrollH.Location.X;
            mouseArea.Y = scrollV.Location.Y;
            mouseArea.Width = scrollH.Width;
            mouseArea.Height = scrollV.Height;
            rescale(scale);

            y = (groupBoxFrames.Height - 58 - 6) / 2;
            dataGridFrames.Height = y;
            dataGridSprites.Location = new Point(dataGridSprites.Location.X, dataGridFrames.Location.Y + dataGridFrames.Size.Height + 6);
            dataGridSprites.Height = y;
        }

        private Rectangle selectedRectangle()
        {
            Rectangle r = new Rectangle();

            int ox, oy, w, h;
            if (selectSizeX > 0)
            {
                ox = selectOriginX;
                w = selectSizeX;
            }
            else
            {
                ox = selectOriginX + selectSizeX + unitSize[sys];
                w = -selectSizeX;
            }
            if (selectSizeY > 0)
            {
                oy = selectOriginY;
                h = selectSizeY;
            }
            else
            {
                oy = selectOriginY + selectSizeY + unitSize[sys];
                h = -selectSizeY;
            }

            r.X = ox;
            r.Y = oy;
            r.Width = w;
            r.Height = h;

            return r;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics gfx = e.Graphics;
            int shiftX, shiftY, i;

            if (currentSheet == -1) return;

            gfx.PixelOffsetMode = PixelOffsetMode.Half;
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;

            drawLocationUpdate();

            shiftX = drawLocation.X - (scrollH.Value * scale);
            shiftY = drawLocation.Y - (scrollV.Value * scale);

            gfx.Clip = new System.Drawing.Region(drawLocation);
            gfx.DrawImage(sheets[currentSheet].bmp, new Rectangle(shiftX, shiftY, sheets[currentSheet].bmp.Width * scale, sheets[currentSheet].bmp.Height * scale));

            gfx.PixelOffsetMode = PixelOffsetMode.None;

            //usage mask
            for (i = 0; i < dataGridFrames.SelectedRows.Count; i++)
            {
                int x, y, w, h;
                int spx, spy, spw, sph;

                x = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].posX;
                y = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].posY;
                w = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].width;
                h = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].height;

                if (w == 0) continue;

                //draw heach HW spr
                for (int j = 0; j < sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].sprites.Count; j++)
                {
                    spx = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].sprites[j].posX;
                    spy = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].sprites[j].posY;
                    spw = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].sprites[j].width;
                    sph = sheets[currentSheet].frames[dataGridFrames.SelectedRows[i].Index].sprites[j].height;

                    gfx.FillRectangle(maskBrush, shiftX + (spx * scale), shiftY + (spy * scale), (spw * scale), (sph * scale));
                    gfx.DrawRectangle(Pens.Yellow, shiftX + (spx * scale), shiftY + (spy * scale), (spw * scale) - 1, (sph * scale) - 1);
                }
                gfx.DrawRectangle(maskPen, shiftX + (x * scale), shiftY + (y * scale), w * scale - 1, h * scale - 1);
            }

            //frame highlight
            if (selectSizeX != 0)
            {
                int ox, oy, w, h;
                if (selectSizeX > 0)
                {
                    ox = selectOriginX;
                    w = selectSizeX;
                }
                else
                {
                    ox = selectOriginX + selectSizeX + unitSize[sys];
                    w = -selectSizeX;
                }
                if (selectSizeY > 0)
                {
                    oy = selectOriginY;
                    h = selectSizeY;
                }
                else
                {
                    oy = selectOriginY + selectSizeY + unitSize[sys];
                    h = -selectSizeY;
                }
                gfx.FillRectangle(frameBrush, shiftX + (ox * scale), shiftY + (oy * scale), (w * scale), (h * scale));
            }

            //selected sprite highlight
            for (i = 0; i < dataGridSprites.SelectedRows.Count; i++)
            {
                int x, y, w, h;

                x = sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[i].Index].posX;
                y = sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[i].Index].posY;
                w = sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[i].Index].width;
                h = sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[i].Index].height;

                gfx.DrawLine(Pens.Cyan, shiftX + x * scale, shiftY + y * scale, shiftX + (x + w) * scale - 1, shiftY + (y + h) * scale - 1);
                gfx.DrawLine(Pens.Cyan, shiftX + x * scale, shiftY + (y + h) * scale - 1, shiftX + (x + w) * scale - 1, shiftY + y * scale);
            }

            //cursor
            if (gridMode)
            {
                if (hoveredTile > -1)
                {
                    int v, w;
                    v = hoveredTile % sheets[currentSheet].tileWidth[sys];
                    w = hoveredTile / sheets[currentSheet].tileWidth[sys];
                    gfx.DrawRectangle(Pens.Black, shiftX + ((v << unitShift[sys]) * scale), shiftY + ((w << unitShift[sys]) * scale), (unitSize[sys] * scale) - 1, (unitSize[sys] * scale) - 1);
                }
            }
            else
            {
                gfx.DrawRectangle(Pens.Black, shiftX + (posX - (posX % scale)), shiftY + (posY - (posY % scale)), (unitSize[sys] * scale) - 1, (unitSize[sys] * scale) - 1);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                frameStartTile = hoveredTile;
                frameEndTile = hoveredTile;

                selectOriginX = hoverX & (gridMode ? unitMask[sys] : 0x7fffffff);
                selectOriginY = hoverY & (gridMode ? unitMask[sys] : 0x7fffffff);

                selectSizeX = unitSize[sys];
                selectSizeY = unitSize[sys];


                mouseDown = true;
                this.Invalidate(drawLocation);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                frameEndTile = hoveredTile;
                mouseDown = false;
                this.Invalidate(drawLocation);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    _addSprite();
                    e.Handled = true;
                    break;
                case Keys.Left:
                    if (e.Modifiers == Keys.Control)
                        resizeSpriteX(-unitSize[sys]);
                    else shiftSpritesX((e.Modifiers & Keys.Shift) == Keys.Shift ? -unitSize[sys] : -1);
                    e.Handled = true;
                    break;
                case Keys.Right:
                    if (e.Modifiers == Keys.Control)
                        resizeSpriteX(unitSize[sys]);
                    else shiftSpritesX((e.Modifiers & Keys.Shift) == Keys.Shift ? unitSize[sys] : 1);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    if (e.Modifiers == Keys.Control)
                        resizeSpriteY(-unitSize[sys]);
                    else shiftSpritesY((e.Modifiers & Keys.Shift) == Keys.Shift ? -unitSize[sys] : -1);
                    e.Handled = true;
                    break;
                case Keys.Down:
                    if (e.Modifiers == Keys.Control)
                        resizeSpriteY(unitSize[sys]);
                    else shiftSpritesY((e.Modifiers & Keys.Shift) == Keys.Shift ? unitSize[sys] : 1);
                    e.Handled = true;
                    break;
                case Keys.N:
                    if (e.Modifiers == Keys.None)
                    {
                        _addFrame();
                        e.Handled = true;
                    }
                    break;
                case Keys.G:
                    //undocumented
                    if (e.Modifiers == (Keys.Control | Keys.Shift))
                    {
                        tg.cellH = sheets[currentSheet].cellH;
                        tg.cellW = sheets[currentSheet].cellW;
                        tg.cellDir = sheets[currentSheet].cellDir;
                        tg.ShowDialog();
                        if(tg.result==DialogResult.OK)
                        {
                            sprData sheet = sheets[currentSheet];

                            sheet.cellH = tg.cellH;
                            sheet.cellW = tg.cellW;
                            sheet.cellDir = tg.cellDir;
                            sheet.hasGrid = true;

                            //clear all frames
                            sheet.frames.Clear();

                            //generate cells & center point
                            for (int y = 0; y < sheet.bmp.Height; y += sheet.cellH)
                                for (int x = 0; x < sheet.bmp.Width; x += sheet.cellW)
                                {
                                    sheet.addFrame();
                                    sheet.frames[sheet.frames.Count - 1].addSprite(x, y, sheet.cellW, sheet.cellH, unitShift[sys]); //cell
                                    sheet.frames[sheet.frames.Count - 1].cell = new Rectangle(x, y, sheet.cellW, sheet.cellH);
                                    if (tg.generateCenter)
                                        sheet.frames[sheet.frames.Count - 1].addSprite(x + tg.centerX, y + tg.centerY, 1, 1, unitShift[sys]); //center point
                                }

                            //refresh display
                            selectSheet(currentSheet);
                        }
                    }
                    e.Handled = true;
                    break;
                case Keys.F:
                    if (e.Modifiers == Keys.None)
                        _addFrame();
                    else if (e.Modifiers == (Keys.Control | Keys.Shift))
                    {
                        //undocumented, sp flips process for unnamed project
                        //row1 => row7, row2 => row6, row3 => row5
                        sprData sheet = sheets[currentSheet];
                        if (sheet.bmp.Height / sheet.cellH != 8)
                            break;
                        int rowSize = sheet.bmp.Width / sheet.cellW;

                        //mirrorFrame(sheet.frames[4], sheet.frames[28]);
                        for (int x = 0; x < rowSize; x++)
                        {
                            mirrorFrame(sheet.frames[1 * rowSize + x], sheet.frames[7 * rowSize + x]);
                            mirrorFrame(sheet.frames[2 * rowSize + x], sheet.frames[6 * rowSize + x]);
                            mirrorFrame(sheet.frames[3 * rowSize + x], sheet.frames[5 * rowSize + x]);
                        }
                        MessageBox.Show("");
                    }
                    e.Handled = true;
                    break;
            }
        }

        void mirrorFrame(frame from, frame to)
        {
            //remove all but center point
            for (int x = 0; x < to.sprites.Count; x++)
            {
                if (!(to.sprites[x].width == 1 && to.sprites[x].height == 1))
                    to.sprites.Remove(to.sprites[x--]);
            }

            foreach (sprite s in from.sprites)
            {
                if (s.width == 1 && s.height == 1) continue;
                int x = from.cell.Width - s.posX - s.width + from.cell.X;
                to.sprites.Add(new sprite(x + to.cell.X, (s.posY - from.cell.Y) + to.cell.Y, s.width, s.height));
            }
        }

        void updateSpritesData()
        {
            for (int i = 0; i < sheets[currentSheet].frames[currentFrame].sprites.Count; i++)
                dataGridSprites.Rows[i].SetValues(sheets[currentSheet].frames[currentFrame].sprites[i].posX, sheets[currentSheet].frames[currentFrame].sprites[i].posY, sheets[currentSheet].frames[currentFrame].sprites[i].width, sheets[currentSheet].frames[currentFrame].sprites[i].height);
            sheets[currentSheet].frames[currentFrame].updateOutline(unitShift[sys]);
            dataGridFrames.Rows[currentFrame].SetValues(sheets[currentSheet].frames[currentFrame].posX, sheets[currentSheet].frames[currentFrame].posY, sheets[currentSheet].frames[currentFrame].width, sheets[currentSheet].frames[currentFrame].height, sheets[currentSheet].frames[currentFrame].sprites.Count, sheets[currentSheet].frames[currentFrame].tileCount);
        }

        private void resizeSpriteX(int mod)
        {
            int w;
            if (dataGridSprites.SelectedRows.Count != 1) return;

            w = sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[0].Index].width;
            w += mod;
            if (w < 8) w = 8;
            if (w > 32) w = 32;
            sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[0].Index].width = w;

            updateSpritesData();
            Invalidate(drawLocation);
        }

        private void resizeSpriteY(int mod)
        {
            int h;
            if (dataGridSprites.SelectedRows.Count != 1) return;

            h = sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[0].Index].height;
            h += mod;
            if (h < 8) h = 8;
            if (h > 32) h = 32;
            sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[0].Index].height = h;

            updateSpritesData();
            Invalidate(drawLocation);
        }

        private void shiftSpritesX(int shift)
        {
            for (int i = 0; i < dataGridSprites.SelectedRows.Count; i++)
                sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[i].Index].posX += shift;

            updateSpritesData();
            Invalidate(drawLocation);
        }

        private void shiftSpritesY(int shift)
        {
            for (int i = 0; i < dataGridSprites.SelectedRows.Count; i++)
                sheets[currentSheet].frames[currentFrame].sprites[dataGridSprites.SelectedRows[i].Index].posY += shift;

            updateSpritesData();
            Invalidate(drawLocation);
        }

        private void addSelectedSprite()
        {
            int i;
            Rectangle r = selectedRectangle();

            if (r.Width == 0) return; //nothing selected

            if (currentFrame < 0)
            {
                MessageBox.Show("No selected frame", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!sheets[currentSheet].frames[currentFrame].addSprite(r.X, r.Y, r.Width, r.Height, unitShift[sys]))
                return;
            dataGridSprites.Rows.Add(r.X, r.Y, r.Width, r.Height);

            //update frame limits
            dataGridFrames.Rows[currentFrame].SetValues(sheets[currentSheet].frames[currentFrame].posX, sheets[currentSheet].frames[currentFrame].posY, sheets[currentSheet].frames[currentFrame].width, sheets[currentSheet].frames[currentFrame].height, sheets[currentSheet].frames[currentFrame].sprites.Count, sheets[currentSheet].frames[currentFrame].tileCount);

            for (i = 0; i < dataGridSprites.Rows.Count - 1; i++)
                dataGridSprites.Rows[i].Selected = false;
            dataGridSprites.Rows[i].Selected = true;

            frameStartTile = -1;
            frameEndTile = -1;
            this.Invalidate(drawLocation);
        }

        private void drawLocationUpdate()
        {
            int w = scrollH.Width;
            int h = scrollV.Height;

            if (currentSheet == -1) return;

            if (scale > 1)
            {
                if ((w % scale) != 0)
                {
                    w += (int)(scale - (w % scale));
                }
                if ((h % scale) != 0)
                {
                    h += (int)(scale - (h % scale));
                }
            }
            drawLocation.X = scrollH.Location.X;
            drawLocation.Y = scrollV.Location.Y;
            if (scrollH.Width > sheets[currentSheet].bmp.Width * scale)
                drawLocation.Width = sheets[currentSheet].bmp.Width * scale;
            else drawLocation.Width = w;
            if (scrollV.Height > sheets[currentSheet].bmp.Height * scale)
                drawLocation.Height = sheets[currentSheet].bmp.Height * scale;
            else drawLocation.Height = h;
        }

        private void rescale(int newscale)
        {
            scale = newscale;
            adjustScrollBars();
            this.Invalidate();
        }

        private void adjustScrollBars()
        {
            scrollV.LargeChange = 480 / scale;
            scrollH.LargeChange = 640 / scale;

            if (currentSheet >= 0)
            {
                //vertical
                if (sheets[currentSheet].bmp.Height * scale > scrollV.Height)
                {
                    scrollV.Enabled = true;
                    scrollV.Maximum = (scrollV.LargeChange - 1) + (sheets[currentSheet].bmp.Height - (scrollV.Height / scale));
                }
                else { scrollV.Enabled = false; }

                //horizontal
                if (sheets[currentSheet].bmp.Width * scale > scrollH.Width)
                {
                    scrollH.Enabled = true;
                    scrollH.Maximum = (scrollH.LargeChange - 1) + (sheets[currentSheet].bmp.Width - (scrollH.Width / scale));
                }
                else { scrollH.Enabled = false; }
            }
            else
            {
                scrollV.Enabled = false;
                scrollH.Enabled = false;
            }
        }

        private void selectSheet(int sht)
        {
            int x;
            //clear grid
            refilling = true;
            dataGridFrames.Rows.Clear();
            refilling = false;

            //dataGridFrames.Hide();
            for (x = 0; x < sheets[sht].frames.Count; x++)
            {
                dataGridFrames.Rows.Add(sheets[sht].frames[x].posX, sheets[sht].frames[x].posY, sheets[sht].frames[x].width, sheets[sht].frames[x].height, sheets[sht].frames[x].sprites.Count, sheets[sht].frames[x].tileCount);
            }
            //dataGridFrames.Show();

            groupBoxFrames.Text = "Frames - " + cbBoxSpriteList.Items[sht];

            scrollH.Value = 0;
            scrollV.Value = 0;
            rescale(scale);
        }

        private void clearAll()
        {
            sheets = null;
            currentSheet = -1;
            cbBoxSpriteList.Items.Clear();
            refilling = true;
            while (dataGridFrames.Rows.Count > (dataGridFrames.AllowUserToAddRows == true ? 1 : 0))
                dataGridFrames.Rows.Remove(dataGridFrames.Rows[0]);
            refilling = false;

            spriteRefilling = true;
            while (dataGridSprites.Rows.Count > (dataGridSprites.AllowUserToAddRows == true ? 1 : 0))
                dataGridSprites.Rows.Remove(dataGridSprites.Rows[0]);
            spriteRefilling = false;
        }

        private void loadXML()
        {
            // openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            XmlNodeList subNodes = null;
            string baseDir = null;
            int gridW, gridH;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                clearAll();
                try
                {
                    doc = new XmlDocument();
                    fName = openFileDialog.FileName;
                    doc.Load(fName);
                    baseDir = openFileDialog.FileName.Substring(0, fName.LastIndexOf("\\")) + "\\";

                    XmlNode docNode = doc.DocumentElement;
                    XmlNodeList sprtNodes = docNode.SelectNodes("sprt");

                    sheets = new List<sprData>(0);

                    for (int i = 0; i < sprtNodes.Count; i++)
                    {
                        cbBoxSpriteList.Items.Add(sprtNodes[i].Attributes["id"].Value);
                        sheets.Add(new sprData(baseDir + sprtNodes[i].SelectSingleNode("file").InnerText, sprtNodes[i]));

                        subNodes = sprtNodes[i].SelectNodes("frame");
                        for (int j = 0; j < subNodes.Count; j++)
                        {
                            sheets[i].parseFrame(subNodes[j], unitShift[sys]);
                        }

                        gridW = gridH = 0;
                        if (sprtNodes[i].Attributes["grid"] != null)
                        {
                            gridW = int.Parse(sprtNodes[i].Attributes["grid"].Value.Split(',')[0]);
                            gridH = int.Parse(sprtNodes[i].Attributes["grid"].Value.Split(',')[1]);
                        }
                        if (gridW != 0 && gridH != 0)
                        {
                            int f = 0;
                            sheets[i].cellW = gridW;
                            sheets[i].cellH = gridH;
                            //presuming frame count=cell count
                            for (int y = 0; y < sheets[i].bmp.Height; y += gridH)
                                for (int x = 0; x < sheets[i].bmp.Width; x += gridW)
                                {
                                    sheets[i].frames[f++].cell = new Rectangle(x, y, gridW, gridH);
                                }
                        }
                    }
                    if (cbBoxSpriteList.Items.Count == 0) MessageBox.Show("No sprites found in selected file.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else cbBoxSpriteList.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    this.Close();
                }
                this.Text = fName + " - " + appName;
            }
        }


        #region Interface controls...

        private void dataGridFrames_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (!refilling)
                sheets[currentSheet].frames.Remove(sheets[currentSheet].frames[e.RowIndex]);
        }

        private void dataGridSprites_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (!spriteRefilling)
            {
                sheets[currentSheet].frames[currentFrame].removeSprite(e.RowIndex, unitShift[sys]);
                dataGridFrames.Rows[currentFrame].SetValues(sheets[currentSheet].frames[currentFrame].posX, sheets[currentSheet].frames[currentFrame].posY, sheets[currentSheet].frames[currentFrame].width, sheets[currentSheet].frames[currentFrame].height, sheets[currentSheet].frames[currentFrame].sprites.Count, sheets[currentSheet].frames[currentFrame].tileCount);
            }
            Invalidate(drawLocation);
        }

        #region Scale controls
        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            loadXML();
        }

        private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxScale.SelectedIndex > 0)
                toolStripComboBoxScale.SelectedIndex--;
        }

        private void toolStripButtonZoomIn_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxScale.SelectedIndex < toolStripComboBoxScale.Items.Count - 1)
                toolStripComboBoxScale.SelectedIndex++;
        }
        #endregion

        private void dataGridSprites_SelectionChanged(object sender, EventArgs e)
        {
            Invalidate(drawLocation);
        }

        private void _addFrame()
        {
            int i;
            if (currentSheet < 0) return;
            sheets[currentSheet].addFrame();
            dataGridFrames.Rows.Add(0, 0, 0, 0);
            for (i = 0; i < dataGridFrames.Rows.Count - 1; i++)
                dataGridFrames.Rows[i].Selected = false;
            dataGridFrames.Rows[i].Selected = true;
        }

        private void btnAddFrame_Click(object sender, EventArgs e)
        {
            _addFrame();
        }

        private void toolStripComboBoxSystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            sys = (int)(systems)toolStripComboBoxSystem.SelectedIndex;
            chkBoxGridMode.Text = string.Format("Lock cursor to {0}x{0} grid", unitSize[sys]);
        }

        private void chkBoxGridMode_CheckedChanged(object sender, EventArgs e)
        {
            gridMode = chkBoxGridMode.Checked;
        }

        #endregion

    }
}

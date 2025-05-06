using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace Framer
{
    public class sprData
    {
        public Bitmap bmp = null;
        public List<frame> frames = null;
        public XmlNode node = null;
        public int[] tileWidth, tileHeight;
        public bool hasGrid = false;
        public int cellW = 0, cellH = 0, cellDir = 0;

        public sprData(string fileName, XmlNode n)
        {
            var buffer = File.ReadAllBytes(fileName);
            var ms = new MemoryStream(buffer);
            bmp = (Bitmap)Bitmap.FromStream(ms);
            //bmp = (Bitmap)Bitmap.FromFile(fileName);
            int sysCount = Framer.unitSize.Length;
            tileWidth = new int[sysCount];
            tileHeight = new int[sysCount];
            for(int i = 0; i < sysCount; i++)
            {
                tileWidth[i] = bmp.Width >> Framer.unitShift[i];
                tileHeight[i] = bmp.Height >> Framer.unitShift[i];
            }
            frames = new List<frame>(0);
            node = n;
        }

        public void addFrame()
        {
            frames.Add(new frame());
        }

        public void parseFrame(XmlNode frameNode, int unitShift)
        {
            XmlNodeList sprNodes = null;
            frame frame = null;
            int x, y, w, h;

            frame = new frame();
            sprNodes = frameNode.SelectNodes("hwspr");

            if (frameNode.Attributes["customOutline"] != null)
            {
                string str = frameNode.Attributes["customOutline"].Value;
                frame.customLayout = true;

                x = int.Parse(str.Split(':')[0].Split(',')[0].Trim());
                y = int.Parse(str.Split(':')[0].Split(',')[1].Trim());
                w = int.Parse(str.Split(':')[1].Split(',')[0].Trim());
                h = int.Parse(str.Split(':')[1].Split(',')[1].Trim());
                frame.customOutline = new Rectangle(x, y, w, h);
            }

            if ((frameNode.Attributes["outline"] == null) && (sprNodes.Count == 0))
            {   //old NG only framer format
                x = 16 * int.Parse(frameNode.InnerText.Split(':')[0].Split(',')[0].Trim());
                y = 16 * int.Parse(frameNode.InnerText.Split(':')[0].Split(',')[1].Trim());
                w = 16 * int.Parse(frameNode.InnerText.Split(':')[1].Split(',')[0].Trim());
                h = 16 * int.Parse(frameNode.InnerText.Split(':')[1].Split(',')[1].Trim());
                frame.addSprite(x, y, w, h, unitShift);
            }
            else
            {   //normal format
                foreach (XmlNode node in sprNodes)
                {
                    x = int.Parse(node.InnerText.Split(':')[0].Split(',')[0].Trim());
                    y = int.Parse(node.InnerText.Split(':')[0].Split(',')[1].Trim());
                    w = int.Parse(node.InnerText.Split(':')[1].Split(',')[0].Trim());
                    h = int.Parse(node.InnerText.Split(':')[1].Split(',')[1].Trim());
                    frame.addSprite(x, y, w, h, unitShift);

                    if (node.Attributes["relocate"] != null)
                    {
                        string str = node.Attributes["relocate"].Value;
                        frame.sprites[frame.sprites.Count - 1].relocateX = int.Parse(str.Split(',')[0].Trim());
                        frame.sprites[frame.sprites.Count - 1].relocateY = int.Parse(str.Split(',')[1].Trim());
                    }
                }
            }

            if (frameNode.Attributes["flips"] != null)
            {
                frame.doFlipX = frameNode.Attributes["flips"].InnerText.ToUpper().Contains("X") ? true : false;
                frame.doFlipY = frameNode.Attributes["flips"].InnerText.ToUpper().Contains("Y") ? true : false;
                frame.doFlipXY = frameNode.Attributes["flips"].InnerText.ToUpper().Contains("Z") ? true : false;
            }

            //frame.updateOutline(4);
            frame.posX = 8;
            frame.posY = 8;
            frame.width = 16;
            frame.height = 16;
            frames.Add(frame);
        }

        public void rebuildXml(XmlDocument doc)
        {
            XmlNode dataNode = null;
            XmlNode hwSprNode = null;
            XmlAttribute attr = null;

            if (hasGrid)
            {
                attr = doc.CreateAttribute("grid");
                attr.Value = string.Format("{0},{1}", cellW, cellH);
                node.Attributes.Append(attr);
            }

            foreach (XmlNode n in node.SelectNodes("frame"))
                node.RemoveChild(n);

            foreach (frame frm in frames)
            {
                dataNode = doc.CreateElement("frame");
                attr = doc.CreateAttribute("outline");
                attr.Value = string.Format("{0},{1}:{2},{3}", frm.posX, frm.posY, frm.width, frm.height);
                dataNode.Attributes.SetNamedItem(attr);

                if (frm.doFlipX || frm.doFlipY || frm.doFlipXY)
                {
                    attr = doc.CreateAttribute("flips");
                    attr.Value = (frm.doFlipX ? "x" : "") + (frm.doFlipY ? "y" : "") + (frm.doFlipXY ? "z" : "");
                    dataNode.Attributes.SetNamedItem(attr);
                }

                if (frm.customLayout)
                {
                    attr = doc.CreateAttribute("customOutline");
                    attr.Value = string.Format("{0},{1}:{2},{3}", frm.customOutline.X, frm.customOutline.Y, frm.customOutline.Width, frm.customOutline.Height);
                    dataNode.Attributes.SetNamedItem(attr);
                }

                foreach (sprite spr in frm.sprites)
                {
                    hwSprNode = doc.CreateElement("hwspr");
                    hwSprNode.InnerText = string.Format("{0},{1}:{2},{3}", spr.posX, spr.posY, spr.width, spr.height);
                    
                    if(frm.customLayout)
                    {
                        attr = doc.CreateAttribute("relocate");
                        attr.Value = string.Format("{0},{1}", spr.relocateX, spr.relocateY);
                        hwSprNode.Attributes.SetNamedItem(attr);
                    }
                    dataNode.AppendChild(hwSprNode);
                }
                node.AppendChild(dataNode);
            }
        }
    }

    public class frame
    {
        public List<sprite> sprites = null;

        public int posX, posY, width, height, tileCount;
        public bool doFlipX, doFlipY, doFlipXY;
        public Rectangle cell = new Rectangle(0, 0, 0, 0);
        public bool customLayout = false;
        public Rectangle customOutline = new Rectangle(0, 0, 0, 0);

        public frame()
        {
            sprites = new List<sprite>(0);
            posX = posY = width = height = tileCount = 0;
            doFlipX = doFlipY = doFlipXY = false;
        }

        public void updateOutline(int tileShift)
        {
            int minX = 0x7ffffff, minY = 0x7ffffff, maxX = 0, maxY = 0;
            tileCount = 0;

            if (sprites.Count==0)
            {
                posX = posY = width = height = 0;
                return;
            }

            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i].posX < minX) minX = sprites[i].posX;
                if (sprites[i].posY < minY) minY = sprites[i].posY;
                if (sprites[i].posX + sprites[i].width > maxX) maxX = sprites[i].posX + sprites[i].width;
                if (sprites[i].posY + sprites[i].height > maxY) maxY = sprites[i].posY + sprites[i].height;
                tileCount += (sprites[i].width >> tileShift) * (sprites[i].height >> tileShift);
            }
            posX = minX;
            posY = minY;
            width = maxX - minX;
            height = maxY - minY;
        }

        public bool addSprite(int x, int y, int w, int h, int unitShift)
        {
            foreach (sprite s in sprites)
            {
                if (s.posX == x && s.posY == y && s.width == w && s.height == h)
                    if (MessageBox.Show("This sprite is already defined, add a duplicate?", "Duplicate sprite?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        break;
                    else return false;
            }
            sprites.Add(new sprite(x, y, w, h));
            updateOutline(unitShift);
            return true;
        }

        public void removeSprite(int index, int unitShift)
        {
            sprites.Remove(sprites[index]);
            updateOutline(unitShift);
        }
    }

    //HW sprite definition
    public class sprite
    {
        public int posX, posY, width, height;
        public int relocateX = 0, relocateY = 0;

        public sprite(int x, int y, int w, int h)
        {
            posX = x;
            posY = y;
            width = w;
            height = h;
        }
    }
}

using NeoTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildchar
{
    public class NGspr
    {
        public Rectangle outline;
        public int relocateX, relocateY;
        public bool relocated = false;
        public uint[] mapData;
        public NGspr(int x, int y, int w, int h) { outline = new Rectangle(x, y, w, h); }
        public NGspr(int x, int y, int w, int h, int rx, int ry)
        {
            outline = new Rectangle(x, y, w, h);
            relocated = true;
            relocateX = rx;
            relocateY = ry;
        }

        public void pickTiles(ref Bitmap bmp, ref List<NGpal> pals, ref paletteRange palRange, ref tileBank bank)
        {
            int idx = 0;
            mapData = new uint[(outline.Height / 16) * (outline.Width / 16)];
            int palIndex = 0;
            for (int x = 0; x < outline.Width; x += 16)
                for (int y = 0; y < outline.Height; y += 16)
                {
                    bool fit = false;
                    NGTile t = new NGTile();
                    //for (int p = 0; p < pals.Count; p++)
                    int endIndex = palRange.start + palRange.size > pals.Count ? pals.Count : palRange.start + palRange.size;
                    for (int p = palRange.start; p < endIndex; p++)
                    {
                        if (fit = t.build(ref bmp, outline.X + x, outline.Y + y, ref pals[p].colors))
                        {
                            palIndex = p - palRange.start;
                            break;
                        }
                    }
                    if (!fit)
                    {
                        Console.WriteLine("Error Processing frame: could not match tile/pals");
#if DEBUG
                        Console.Read();
#endif
                        Environment.Exit(1);
                    }
                    uint m = bank.addTile(t);
                    mapData[idx++] = (uint)(m + (palIndex << 8));
                }
        }
    }

    public class NGFrame
    {
        public List<NGspr> sprites;
        public Rectangle outline;
        public int offsetX = 0, offsetY = 0;
        public NGFrame(int x, int y, int w, int h)
        {
            sprites = new List<NGspr>(0);
            outline = new Rectangle(x, y, w, h);
        }
        public void addSpr(int x, int y, int w, int h)
        {
            sprites.Add(new NGspr(x, y, w, h));
        }
        public void pickTiles(ref Bitmap bmp, ref List<NGpal> pals, ref paletteRange palRange, ref tileBank bank)
        {
            foreach (NGspr spr in sprites)
                spr.pickTiles(ref bmp, ref pals, ref palRange, ref bank);
        }
        public void toAsm(ref string info, ref string maps, string ID, int frameNum, bool flipX, bool flipY, bool flipXY, bool noFlips)
        {
            info += string.Format("{0}_{1:x4}:" + Environment.NewLine, ID, frameNum);
            if (sprites.Count == 1)
            {
                //as basic plain frame data
                info += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;*{0} cols of {1} tiles, {2} bytes per strip" + Environment.NewLine, outline.Width / 16, outline.Height / 16, outline.Height / 16 * 4);
                if (noFlips)
                    info += string.Format("\t.long\t{0}_{1:x4}_Map" + Environment.NewLine, ID, frameNum);
                else info += string.Format("\t.long\t{0}_{1:x4}_Map, {0}_{1:x4}_Map{2}, {0}_{1:x4}_Map{3}, {0}_{1:x4}_Map{4}" + Environment.NewLine, ID, frameNum, flipX ? "_FlipX" : "", flipY ? "_FlipY" : "", flipXY ? "_FlipXY" : "");

                maps += string.Format("{0}_{1:x4}_Map:" + Environment.NewLine, ID, frameNum);
                for (int w = 0; w < outline.Width; w += 16)
                {
                    maps += "\t.long\t";
                    for (int h = 0; h < outline.Height; h += 16)
                    {
                        uint v = sprites[0].mapData[((w / 16) * (outline.Height / 16)) + (h / 16)];
                        maps += string.Format("0x{0:x8}{1}", v, h != (outline.Height - 16) ? ", " : Environment.NewLine);
                    }
                }
                if (flipX)
                {
                    maps += string.Format("{0}_{1:x4}_Map_FlipX:" + Environment.NewLine, ID, frameNum);
                    for (int w = outline.Width - 16; w >= 0; w -= 16)
                    {
                        maps += "\t.long\t";
                        for (int h = 0; h < outline.Height; h += 16)
                        {
                            uint v = sprites[0].mapData[((w / 16) * (outline.Height / 16)) + (h / 16)] ^ 0x1;
                            maps += string.Format("0x{0:x8}{1}", v, h != (outline.Height - 16) ? ", " : Environment.NewLine);
                        }
                    }
                }
                if (flipY)
                {
                    maps += string.Format("{0}_{1:x4}_Map_FlipY:" + Environment.NewLine, ID, frameNum);
                    for (int w = 0; w < outline.Width; w += 16)
                    {
                        maps += "\t.long\t";
                        for (int h = outline.Height - 16; h >= 0; h -= 16)
                        {
                            uint v = sprites[0].mapData[((w / 16) * (outline.Height / 16)) + (h / 16)] ^ 0x2;
                            maps += string.Format("0x{0:x8}{1}", v, h != 0 ? ", " : Environment.NewLine);
                        }
                    }
                }
                if (flipXY)
                {
                    maps += string.Format("{0}_{1:x4}_Map_FlipXY:" + Environment.NewLine, ID, frameNum);
                    for (int w = outline.Width - 16; w >= 0; w -= 16)
                    {
                        maps += "\t.long\t";
                        for (int h = outline.Height - 16; h >= 0; h -= 16)
                        {
                            uint v = sprites[0].mapData[((w / 16) * (outline.Height / 16)) + (h / 16)] ^ 0x3;
                            maps += string.Format("0x{0:x8}{1}", v, h != 0 ? ", " : Environment.NewLine);
                        }
                    }
                }
            }
            else
            {
                //as multiple individual sprites
                string dataFlipNone = string.Format("{0}_{1:x4}_Data:" + Environment.NewLine, ID, frameNum);
                string dataFlipX = string.Format("{0}_{1:x4}_Data_FlipX:" + Environment.NewLine, ID, frameNum);
                string dataFlipY = string.Format("{0}_{1:x4}_Data_FlipY:" + Environment.NewLine, ID, frameNum);
                string dataFlipXY = string.Format("{0}_{1:x4}_Data_FlipXY:" + Environment.NewLine, ID, frameNum);

                info += string.Format("\t.word\t0x0000, 0x{0:x4}\t;* alt format, {0} sprites" + Environment.NewLine, sprites.Count);
                if (noFlips)
                    info += string.Format("\t.long\t{0}_{1:x4}_Data" + Environment.NewLine, ID, frameNum);
                else info += string.Format("\t.long\t{0}_{1:x4}_Data, {0}_{1:x4}_Data{2}, {0}_{1:x4}_Data{3}, {0}_{1:x4}_Data{4}" + Environment.NewLine, ID, frameNum, flipX ? "_FlipX" : "", flipY ? "_FlipY" : "", flipXY ? "_FlipXY" : "");

                foreach (NGspr spr in sprites)
                {
                    int sprX = spr.relocated ? spr.relocateX : spr.outline.X;
                    int sprY = spr.relocated ? spr.relocateY : spr.outline.Y;
                    dataFlipNone += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;* posX, posY, tileSize" + Environment.NewLine, sprX - outline.X, sprY - outline.Y, spr.outline.Height / 16);
                    for (int y = 0; y < spr.outline.Height / 16; y++)
                    {
                        dataFlipNone += string.Format("{0}0x{1:x8}", y == 0 ? "\t.long\t" : ", ", spr.mapData[y]);
                    }
                    dataFlipNone += Environment.NewLine;

                    if (flipX)
                    {
                        int posX = outline.Width - 16 - (sprX - outline.X);
                        dataFlipX += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;* posX, posY, tileSize" + Environment.NewLine, posX, sprY - outline.Y, spr.outline.Height / 16);
                        for (int y = 0; y < spr.mapData.Length; y++)
                        {
                            dataFlipX += string.Format("{0}0x{1:x8}", y == 0 ? "\t.long\t" : ", ", spr.mapData[y] ^ 0x1);
                        }
                        dataFlipX += Environment.NewLine;
                    }
                    if (flipY)
                    {
                        int posY = outline.Height - spr.outline.Height - (sprY - outline.Y);
                        dataFlipY += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;* posX, posY, tileSize" + Environment.NewLine, sprX - outline.X, posY, spr.outline.Height / 16);
                        for (int y = spr.mapData.Length - 1; y >= 0; y--)
                        {
                            dataFlipY += string.Format("{0}0x{1:x8}", y == spr.mapData.Length - 1 ? "\t.long\t" : ", ", spr.mapData[y] ^ 0x2);
                        }
                        dataFlipY += Environment.NewLine;
                    }
                    if (flipXY)
                    {
                        int posX = outline.Width - 16 - (sprX - outline.X);
                        int posY = outline.Height - spr.outline.Height - (sprY - outline.Y);
                        dataFlipXY += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;* posX, posY, tileSize" + Environment.NewLine, posX, posY, spr.outline.Height / 16);
                        for (int y = spr.mapData.Length - 1; y >= 0; y--)
                        {
                            dataFlipXY += string.Format("{0}0x{1:x8}", y == spr.mapData.Length - 1 ? "\t.long\t" : ", ", spr.mapData[y] ^ 0x3);
                        }
                        dataFlipXY += Environment.NewLine;
                    }
                }
                maps += dataFlipNone + (flipX ? dataFlipX : "") + (flipY ? dataFlipY : "") + (flipXY ? dataFlipXY : "");
            }
        }
    }
}

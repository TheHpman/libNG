using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using NeoTools;

//still is midway through switching to color index instead of pixels but will work for now

namespace Buildchar
{
    public class NGspr
    {
        public Rectangle outline;
        public NGspr(int x, int y, int w, int h) { outline = new Rectangle(x, y, w, h); }
        public uint[] mapData;
        public void pickTiles(ref Bitmap bmp, ref List<NGpal> pals, ref tileBank bank)
        {
            int idx = 0;
            mapData = new uint[(outline.Height / 16) * (outline.Width / 16)];
            int palIndex = 0;
            for (int x = 0; x < outline.Width; x += 16)
                for (int y = 0; y < outline.Height; y += 16)
                {
                    bool fit = false;
                    NGTile t = new NGTile();
                    for (int p = 0; p < pals.Count; p++)
                    {
                        if (fit = t.build(ref bmp, outline.X + x, outline.Y + y, ref pals[p].colors))
                        {
                            palIndex = p;
                            break;
                        }
                    }
                    if (!fit)
                    {
                        Console.WriteLine("Error Processing frame: could not match tile/pals");
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
        public void pickTiles(ref Bitmap bmp, ref List<NGpal> pals, ref tileBank bank)
        {
            foreach (NGspr spr in sprites)
                spr.pickTiles(ref bmp, ref pals, ref bank);
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
                    dataFlipNone += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;* posX, posY, tileSize" + Environment.NewLine, spr.outline.X - outline.X, spr.outline.Y - outline.Y, spr.outline.Height / 16);
                    for (int y = 0; y < spr.outline.Height / 16; y++)
                    {
                        dataFlipNone += string.Format("{0}0x{1:x8}", y == 0 ? "\t.long\t" : ", ", spr.mapData[y]);
                    }
                    dataFlipNone += Environment.NewLine;

                    if (flipX)
                    {
                        int posX = outline.Width - 16 - (spr.outline.X - outline.X);
                        dataFlipX += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;* posX, posY, tileSize" + Environment.NewLine, posX, spr.outline.Y - outline.Y, spr.outline.Height / 16);
                        for (int y = 0; y < spr.mapData.Length; y++)
                        {
                            dataFlipX += string.Format("{0}0x{1:x8}", y == 0 ? "\t.long\t" : ", ", spr.mapData[y] ^ 0x1);
                        }
                        dataFlipX += Environment.NewLine;
                    }
                    if (flipY)
                    {
                        int posY = outline.Height - spr.outline.Height - (spr.outline.Y - outline.Y);
                        dataFlipY += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;* posX, posY, tileSize" + Environment.NewLine, spr.outline.X - outline.X, posY, spr.outline.Height / 16);
                        for (int y = spr.mapData.Length - 1; y >= 0; y--)
                        {
                            dataFlipY += string.Format("{0}0x{1:x8}", y == spr.mapData.Length - 1 ? "\t.long\t" : ", ", spr.mapData[y] ^ 0x2);
                        }
                        dataFlipY += Environment.NewLine;
                    }
                    if (flipXY)
                    {
                        int posX = outline.Width - 16 - (spr.outline.X - outline.X);
                        int posY = outline.Height - spr.outline.Height - (spr.outline.Y - outline.Y);
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

    public class NGpal
    {
        public List<Color> colors;
        public NGpal() { colors = new List<Color>(0); }

        public bool addColor(Color c, bool forceAdd = false)
        {
            if (forceAdd)
            {
                colors.Add(c);
                return true;
            }
            if (c.A != 255) return true;
            if (colors.Contains(c)) return true;
            if (colors.Count >= 16) return false;
            colors.Add(c);
            return true;
        }
        public string toAsm()
        {
            string data = "\t.word\t";

            for (int i = 0; i < colors.Count; i++)
                data += string.Format("0x{0:x4}{1}", ColorTool.NeoColor(colors[i].R, colors[i].G, colors[i].B), i != 15 ? ", " : "");
            for (int i = colors.Count; i < 16; i++)
                data += i == 0 ? "0x8000" : ", 0x8000";
            return data;
        }
    }

    public class palette
    {
        public List<ushort> colors;
        public int location;

        public palette() { colors = new List<ushort>(0); }

        public bool addRGBColor(byte r, byte g, byte b)
        {
            ushort color = ColorTool.NeoColor(r, g, b);

            if (colors.Contains(color)) return true;
            if (colors.Count >= 15) return false;
            colors.Add(color);
            return true;
        }

        public bool contains(palette p)
        {
            if (p.colors.Count > colors.Count)
                return false;

            foreach (ushort c in p.colors)
                if (!colors.Contains(c))
                    return false;

            return true;
        }

        public int checkMerge(palette p)
        {
            int x = colors.Count;

            foreach (ushort c in p.colors)
                if (!colors.Contains(c))
                    x++;

            return x;
        }

        public void merge(palette p)
        {
            foreach (ushort c in p.colors)
                if (!colors.Contains(c))
                    colors.Add(c);
        }

        public string toAsm()
        {
            string data = "\t.word\t0x8000";

            for (int i = 0; i < colors.Count; i++)
                data += string.Format(", 0x{0:x4}", colors[i]);

            for (int i = colors.Count; i < 15; i++)
                data += ", 0x8000";

            return data;
        }
    }

    public class tile
    {
        public static byte[] auto4hash = { 46, 56, 132, 98, 9, 247, 191, 112, 54, 98, 161, 213, 69, 241, 152, 122 };
        public Bitmap tile0 = null, tile1 = null, tile2 = null, tile3 = null, tile4 = null, tile5 = null, tile6 = null, tile7 = null;
        public Bitmap tileStrip = null;
        public Bitmap texture = null;
        public byte[] hash, hashFlipX, hashFlipY, hashFlipXY;
        public int autoAnim, location;

        public int duplicateOf = -1, duplicateFlip = 0;
        public int usingPalette = -1, csPalette = -1;
        public bool remappedPalette = false;

        public byte[] tileData = null;

        public palette pal = null;
        private static Rectangle tileRect = new Rectangle(0, 0, 16, 16);

        public void doTile(ref Bitmap b, int x, int y, bool process = true)
        {
            tile0 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);

            Graphics gfx = Graphics.FromImage(tile0);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b, new Rectangle(0, 0, 16, 16), x, y, 16, 16, GraphicsUnit.Pixel);
            gfx.Dispose();

            if (process)
                parseSingle();
        }

        public void doTile(ref Bitmap b, ref Bitmap b1, ref Bitmap b2, ref Bitmap b3, int x, int y)
        {
            byte[] data = new byte[16 * 16 * 4];
            BitmapData bdata;
            byte[] hash1, hash2, hash3;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            Graphics gfx;

            tile0 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile1 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile2 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile3 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);

            gfx = Graphics.FromImage(tile0);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile0.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile0.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile1);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b1, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile1.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile1.UnlockBits(bdata);
            hash1 = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile2);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b2, new Rectangle(0, 0, 16, 16), x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile2.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile2.UnlockBits(bdata);
            hash2 = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile3);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b3, new Rectangle(0, 0, 16, 16), x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile3.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile3.UnlockBits(bdata);
            hash3 = md5.ComputeHash(data);

            gfx.Dispose();

            if (hash.SequenceEqual(hash1) && hash.SequenceEqual(hash2) && hash.SequenceEqual(hash3))
                parseSingle();
            else parseQuad();

            data = null;
            hash1 = hash2 = hash3 = null;
        }

        public tile(ref Bitmap b, ref Bitmap b1, ref Bitmap b2, ref Bitmap b3, ref Bitmap b4, ref Bitmap b5, ref Bitmap b6, ref Bitmap b7, int x, int y)
        {
            if (b3 == null) { doTile(ref b, x, y); return; }
            if (b7 == null) { doTile(ref b, ref b1, ref b2, ref b3, x, y); return; }

            byte[] data = new byte[16 * 16 * 4];
            BitmapData bdata;
            byte[] hash1, hash2, hash3, hash4, hash5, hash6, hash7;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            Graphics gfx;

            tile0 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile1 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile2 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile3 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile4 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile5 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile6 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            tile7 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);

            gfx = Graphics.FromImage(tile0);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile0.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile0.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile1);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b1, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile1.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile1.UnlockBits(bdata);
            hash1 = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile2);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b2, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile2.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile2.UnlockBits(bdata);
            hash2 = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile3);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b3, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile3.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile3.UnlockBits(bdata);
            hash3 = md5.ComputeHash(data);

            /**/
            gfx = Graphics.FromImage(tile4);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b4, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile4.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile4.UnlockBits(bdata);
            hash4 = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile5);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b5, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile5.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile5.UnlockBits(bdata);
            hash5 = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile6);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b6, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile6.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile6.UnlockBits(bdata);
            hash6 = md5.ComputeHash(data);

            gfx = Graphics.FromImage(tile7);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(b7, tileRect/*new Rectangle(0, 0, 16, 16)*/, x, y, 16, 16, GraphicsUnit.Pixel);
            bdata = tile7.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile7.UnlockBits(bdata);
            hash7 = md5.ComputeHash(data);

            gfx.Dispose();

            if (hash.SequenceEqual(hash1) && hash.SequenceEqual(hash2) && hash.SequenceEqual(hash3) && hash.SequenceEqual(hash4) && hash.SequenceEqual(hash5) && hash.SequenceEqual(hash6) && hash.SequenceEqual(hash7))
                parseSingle();
            else if (hash4.SequenceEqual(auto4hash))
                parseQuad();
            else parseOcto();

            data = null;
            hash1 = hash2 = hash3 = hash4 = hash5 = hash6 = hash7 = null;
        }

        public void parseSingle()
        {
            byte[] data = new byte[16 * 16 * 4];
            BitmapData bdata;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            autoAnim = 1;

            //no flip
            bdata = tile0.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile0.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            //X flip
            tile0.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bdata = tile0.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile0.UnlockBits(bdata);
            hashFlipX = md5.ComputeHash(data);

            //XY flip
            tile0.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bdata = tile0.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile0.UnlockBits(bdata);
            hashFlipXY = md5.ComputeHash(data);

            //Y flip
            tile0.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bdata = tile0.LockBits(tileRect/*new Rectangle(0, 0, 16, 16)*/, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            tile0.UnlockBits(bdata);
            hashFlipY = md5.ComputeHash(data);

            //restore to std orientation
            tile0.RotateFlip(RotateFlipType.RotateNoneFlipY);

            data = null;
            md5.Dispose();
        }

        private void parseQuad()
        {
            //parse tile0..3
            BitmapData bdata;
            byte[] data = new byte[16 * 64 * 4];
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            autoAnim = 4;

            tileStrip = new Bitmap(16, 64, PixelFormat.Format32bppArgb);

            Graphics gfx = Graphics.FromImage(tileStrip);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(tile0, 0, 0);
            gfx.DrawImage(tile1, 0, 16);
            gfx.DrawImage(tile2, 0, 32);
            gfx.DrawImage(tile3, 0, 48);

            //no flip
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 64), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 64 * 4);
            tileStrip.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            //X flip
            tileStrip.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 64), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 64 * 4);
            tileStrip.UnlockBits(bdata);
            hashFlipX = md5.ComputeHash(data);

            //Y flip
            tile0.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile1.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile2.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile3.RotateFlip(RotateFlipType.RotateNoneFlipY);
            gfx.Clear(Color.Transparent);
            gfx.DrawImage(tile0, 0, 0);
            gfx.DrawImage(tile1, 0, 16);
            gfx.DrawImage(tile2, 0, 32);
            gfx.DrawImage(tile3, 0, 48);
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 64), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 64 * 4);
            tileStrip.UnlockBits(bdata);
            hashFlipY = md5.ComputeHash(data);

            //XY flip
            tileStrip.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 64), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 64 * 4);
            tileStrip.UnlockBits(bdata);
            hashFlipXY = md5.ComputeHash(data);

            data = null;
            gfx.Dispose();
            md5.Dispose();
            tile0.Dispose();
            tile1.Dispose();
            tile2.Dispose();
            tile3.Dispose();
            tile0 = tile1 = tile2 = tile3 = null;
        }

        private void parseOcto()
        {
            //parse tile0..7
            BitmapData bdata;
            byte[] data = new byte[16 * 128 * 4];
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            autoAnim = 8;

            tileStrip = new Bitmap(16, 128, PixelFormat.Format32bppArgb);

            Graphics gfx = Graphics.FromImage(tileStrip);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(tile0, 0, 0);
            gfx.DrawImage(tile1, 0, 16);
            gfx.DrawImage(tile2, 0, 32);
            gfx.DrawImage(tile3, 0, 48);
            gfx.DrawImage(tile4, 0, 64);
            gfx.DrawImage(tile5, 0, 80);
            gfx.DrawImage(tile6, 0, 96);
            gfx.DrawImage(tile7, 0, 112);

            //no flip
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 128), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 128 * 4);
            tileStrip.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            //X flip
            tileStrip.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 128), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 128 * 4);
            tileStrip.UnlockBits(bdata);
            hashFlipX = md5.ComputeHash(data);

            //Y flip
            tile0.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile1.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile2.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile3.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile4.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile5.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile6.RotateFlip(RotateFlipType.RotateNoneFlipY);
            tile7.RotateFlip(RotateFlipType.RotateNoneFlipY);
            gfx.Clear(Color.Transparent);
            gfx.DrawImage(tile0, 0, 0);
            gfx.DrawImage(tile1, 0, 16);
            gfx.DrawImage(tile2, 0, 32);
            gfx.DrawImage(tile3, 0, 48);
            gfx.DrawImage(tile4, 0, 64);
            gfx.DrawImage(tile5, 0, 80);
            gfx.DrawImage(tile6, 0, 96);
            gfx.DrawImage(tile7, 0, 112);
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 128), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 128 * 4);
            tileStrip.UnlockBits(bdata);
            hashFlipY = md5.ComputeHash(data);

            //XY flip
            tileStrip.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bdata = tileStrip.LockBits(new Rectangle(0, 0, 16, 128), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 128 * 4);
            tileStrip.UnlockBits(bdata);
            hashFlipXY = md5.ComputeHash(data);

            data = null;
            gfx.Dispose();
            md5.Dispose();
            tile0.Dispose();
            tile1.Dispose();
            tile2.Dispose();
            tile3.Dispose();
            tile4.Dispose();
            tile5.Dispose();
            tile6.Dispose();
            tile7.Dispose();

            tile0 = tile1 = tile2 = tile3 = tile4 = tile5 = tile6 = tile7 = null;
        }

        public bool makePalette()
        {
            BitmapData bmpData;
            byte r, g, b;

            pal = new palette();

            bmpData = (autoAnim > 1 ? tileStrip : tile0).LockBits(new Rectangle(0, 0, 16, 16 * autoAnim), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr;
                ptr = (byte*)bmpData.Scan0;
                for (int x = 0; x < 256 * autoAnim; x++)
                {
                    b = *ptr++;
                    g = *ptr++;
                    r = *ptr++;
                    if (*ptr++ != 0) //alpha not 0, pixel is visible
                        if (!pal.addRGBColor(r, g, b))
                        {
                            (autoAnim > 1 ? tileStrip : tile0).UnlockBits(bmpData);
                            return false;
                        }
                }
            }

            (autoAnim > 1 ? tileStrip : tile0).UnlockBits(bmpData);
            return true;
        }

        public void rasterize(palette pal, bool makeTexture = false)
        {
            byte b1, b2, b3, b4, r, g, b;
            byte[] data;
            int dataIdx = 0;
            int idxInc = 1;

            tileData = new byte[128 * autoAnim];
            data = new byte[256 * autoAnim];
            if (autoAnim > 1)
            {
                //we left autoanim tile strip in XY flip state
                dataIdx = data.Length - 1;
                idxInc = -1;
            }

            //build px index table
            BitmapData bmpData = (autoAnim > 1 ? tileStrip : tile0).LockBits(new Rectangle(0, 0, 16, 16 * autoAnim), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* ptr;
                ptr = (byte*)bmpData.Scan0;

                for (int x = 0; x < 256 * autoAnim; x++)
                {
                    b = *ptr++;
                    g = *ptr++;
                    r = *ptr++;

                    data[dataIdx] = (byte)((*ptr++ != 0) ? (pal.colors.IndexOf(ColorTool.NeoColor(r, g, b)) + 1) : 0);
                    dataIdx += idxInc;
                }
            }
            (autoAnim > 1 ? tileStrip : tile0).UnlockBits(bmpData);

            dataIdx = 0;
            for (int t = 0; t < autoAnim; t++)
            {
                for (int p = 0 + (t * 256); p < 256 + (t * 256); p += 16)
                {
                    b4 = (byte)((data[p + 15] << 4) & 0x80);
                    b2 = (byte)((data[p + 15] << 5) & 0x80);
                    b3 = (byte)((data[p + 15] << 6) & 0x80);
                    b1 = (byte)((data[p + 15] << 7) & 0x80);
                    b4 |= (byte)((data[p + 14] << 3) & 0x40);
                    b2 |= (byte)((data[p + 14] << 4) & 0x40);
                    b3 |= (byte)((data[p + 14] << 5) & 0x40);
                    b1 |= (byte)((data[p + 14] << 6) & 0x40);
                    b4 |= (byte)((data[p + 13] << 2) & 0x20);
                    b2 |= (byte)((data[p + 13] << 3) & 0x20);
                    b3 |= (byte)((data[p + 13] << 4) & 0x20);
                    b1 |= (byte)((data[p + 13] << 5) & 0x20);
                    b4 |= (byte)((data[p + 12] << 1) & 0x10);
                    b2 |= (byte)((data[p + 12] << 2) & 0x10);
                    b3 |= (byte)((data[p + 12] << 3) & 0x10);
                    b1 |= (byte)((data[p + 12] << 4) & 0x10);
                    b4 |= (byte)((data[p + 11] << 0) & 0x08);
                    b2 |= (byte)((data[p + 11] << 1) & 0x08);
                    b3 |= (byte)((data[p + 11] << 2) & 0x08);
                    b1 |= (byte)((data[p + 11] << 3) & 0x08);
                    b4 |= (byte)((data[p + 10] >> 1) & 0x04);
                    b2 |= (byte)((data[p + 10] << 0) & 0x04);
                    b3 |= (byte)((data[p + 10] << 1) & 0x04);
                    b1 |= (byte)((data[p + 10] << 2) & 0x04);
                    b4 |= (byte)((data[p + 9] >> 2) & 0x02);
                    b2 |= (byte)((data[p + 9] >> 1) & 0x02);
                    b3 |= (byte)((data[p + 9] << 0) & 0x02);
                    b1 |= (byte)((data[p + 9] << 1) & 0x02);
                    b4 |= (byte)((data[p + 8] >> 3) & 0x01);
                    b2 |= (byte)((data[p + 8] >> 2) & 0x01);
                    b3 |= (byte)((data[p + 8] >> 1) & 0x01);
                    b1 |= (byte)((data[p + 8] << 0) & 0x01);
                    tileData[dataIdx++] = b1;
                    tileData[dataIdx++] = b2;
                    tileData[dataIdx++] = b3;
                    tileData[dataIdx++] = b4;
                }
                for (int p = 0 + (t * 256); p < 256 + (t * 256); p += 16)
                {
                    b4 = (byte)((data[p + 7] << 4) & 0x80);
                    b2 = (byte)((data[p + 7] << 5) & 0x80);
                    b3 = (byte)((data[p + 7] << 6) & 0x80);
                    b1 = (byte)((data[p + 7] << 7) & 0x80);
                    b4 |= (byte)((data[p + 6] << 3) & 0x40);
                    b2 |= (byte)((data[p + 6] << 4) & 0x40);
                    b3 |= (byte)((data[p + 6] << 5) & 0x40);
                    b1 |= (byte)((data[p + 6] << 6) & 0x40);
                    b4 |= (byte)((data[p + 5] << 2) & 0x20);
                    b2 |= (byte)((data[p + 5] << 3) & 0x20);
                    b3 |= (byte)((data[p + 5] << 4) & 0x20);
                    b1 |= (byte)((data[p + 5] << 5) & 0x20);
                    b4 |= (byte)((data[p + 4] << 1) & 0x10);
                    b2 |= (byte)((data[p + 4] << 2) & 0x10);
                    b3 |= (byte)((data[p + 4] << 3) & 0x10);
                    b1 |= (byte)((data[p + 4] << 4) & 0x10);
                    b4 |= (byte)((data[p + 3] << 0) & 0x08);
                    b2 |= (byte)((data[p + 3] << 1) & 0x08);
                    b3 |= (byte)((data[p + 3] << 2) & 0x08);
                    b1 |= (byte)((data[p + 3] << 3) & 0x08);
                    b4 |= (byte)((data[p + 2] >> 1) & 0x04);
                    b2 |= (byte)((data[p + 2] << 0) & 0x04);
                    b3 |= (byte)((data[p + 2] << 1) & 0x04);
                    b1 |= (byte)((data[p + 2] << 2) & 0x04);
                    b4 |= (byte)((data[p + 1] >> 2) & 0x02);
                    b2 |= (byte)((data[p + 1] >> 1) & 0x02);
                    b3 |= (byte)((data[p + 1] << 0) & 0x02);
                    b1 |= (byte)((data[p + 1] << 1) & 0x02);
                    b4 |= (byte)((data[p + 0] >> 3) & 0x01);
                    b2 |= (byte)((data[p + 0] >> 2) & 0x01);
                    b3 |= (byte)((data[p + 0] >> 1) & 0x01);
                    b1 |= (byte)((data[p + 0] << 0) & 0x01);
                    tileData[dataIdx++] = b1;
                    tileData[dataIdx++] = b2;
                    tileData[dataIdx++] = b3;
                    tileData[dataIdx++] = b4;
                }
            }

            if (makeTexture)
            {
                texture = new Bitmap(20 * autoAnim, 20, PixelFormat.Format32bppArgb);
                Graphics gfx = Graphics.FromImage(texture);
                gfx.Clear(Color.Black);
                gfx.Dispose();

                for (int t = 0; t < autoAnim; t++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        int c = data[y * 16 + 0] << 4;
                        texture.SetPixel(t * 20 + 1, y + 2, Color.FromArgb(c, c, c));
                        texture.SetPixel(t * 20 + 2, y + 2, Color.FromArgb(c, c, c));
                        for (int x = 1; x < 15; x++)
                        {
                            c = data[y * 16 + x] << 4;
                            texture.SetPixel(t * 20 + 2 + x, y + 2, Color.FromArgb(c, c, c));
                        }
                        c = data[y * 16 + 15] << 4;
                        texture.SetPixel(t * 20 + 17, y + 2, Color.FromArgb(c, c, c));
                        texture.SetPixel(t * 20 + 18, y + 2, Color.FromArgb(c, c, c));
                    }
                    for (int x = 1; x < 19; x++)
                    {
                        texture.SetPixel(t * 20 + x, 1, texture.GetPixel(t * 20 + x, 2));
                        texture.SetPixel(t * 20 + x, 18, texture.GetPixel(t * 20 + x, 17));
                    }
                }
            }
        }
    }

    public enum patnStatus { OK, colorOverload, fixedPaletteMismatch };

    public class pattern
    {
        public string objID;
        public bool isColorStream = false;
        public List<tile> tiles;
        int tileWidth, tileHeight;
        public List<palette> fixedPalettes = null;
        public patnStatus status = patnStatus.OK;

        public pattern(string id, ref Bitmap sourceBmp, ref Bitmap sourceBmp1, ref Bitmap sourceBmp2, ref Bitmap sourceBmp3, ref Bitmap sourceBmp4, ref Bitmap sourceBmp5, ref Bitmap sourceBmp6, ref Bitmap sourceBmp7, ref List<palette> fixedPals)
        {
            objID = id;
            fixedPalettes = fixedPals;
            tileWidth = sourceBmp.Width >> 4;
            tileHeight = sourceBmp.Height >> 4;

            tiles = new List<tile>(0);
            Stopwatch sw = new Stopwatch();

            sw.Start();

            for (int w = 0; w < sourceBmp.Width; w += 16)
                for (int h = 0; h < sourceBmp.Height; h += 16)
                    tiles.Add(new tile(ref sourceBmp, ref sourceBmp1, ref sourceBmp2, ref sourceBmp3, ref sourceBmp4, ref sourceBmp5, ref sourceBmp6, ref sourceBmp7, w, h));

            //search for duplicate tiles
            sw.Restart();
            for (int i = 0; i < tiles.Count - 1; i++)
            {
                if (tiles[i].duplicateOf >= 0) continue;

                Parallel.ForEach(tiles.GetRange(i + 1, tiles.Count - (i + 1)), (t) =>
                {
                    if (t.duplicateOf < 0)
                    {
                        int cmp = compareTiles(tiles[i], t);
                        if (cmp != 0)
                        {
                            //duplicate tile
                            t.duplicateOf = i;
                            t.duplicateFlip = cmp;
                        }
                    }
                });
            }
            sw.Stop();
            //            Console.WriteLine(sw.Elapsed);

            //each tile starts with its own palette
            for (int i = 0; i < tiles.Count; i++)
                tiles[i].usingPalette = i;

            //build palette for each tile
            sw.Restart();
            Parallel.ForEach(tiles, (t) =>
            {
                //t.autoAnim = 0;
                if (t.duplicateOf == -1)
                    t.makePalette();
                else t.remappedPalette = true;
            });
            sw.Stop();
            //            Console.WriteLine("pals picking (//): " + sw.Elapsed);

            //check color overload
            foreach (tile t in tiles)
                if (t.duplicateOf < 0)
                    if (t.pal.colors.Count > 15)
                    {
                        status = patnStatus.colorOverload;
                        break;
                    }

            if (status == patnStatus.OK)
            {
                if (fixedPalettes.Count == 0)
                {
                    //optimize palettes
                    sw.Restart();
                    for (int i = 0; i < tiles.Count - 1; i++)
                    {
                        if (tiles[i].remappedPalette) continue;

                        for (int j = i + 1; j < tiles.Count; j++)
                        {
                            if (!tiles[j].remappedPalette)
                            {
                                //both palettes used
                                if (tiles[j].pal.colors.Count <= tiles[i].pal.colors.Count)
                                {
                                    if (tiles[i].pal.contains(tiles[j].pal))
                                    {
                                        tiles[j].usingPalette = i;
                                        tiles[j].remappedPalette = true;
                                    }
                                }
                                else
                                {
                                    if (tiles[j].pal.contains(tiles[i].pal))
                                    {
                                        tiles[i].usingPalette = j;
                                        tiles[i].remappedPalette = true;
                                        Parallel.ForEach(tiles, (t) =>
                                        {
                                            if (t.remappedPalette && t.usingPalette == i)
                                                t.usingPalette = j;
                                        });
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //merge pass
                    int mergeTarget = 16;
                    while (--mergeTarget > 1)
                    {
                        bool merged = false;
                        for (int i = 0; i < tiles.Count - 1; i++)
                        {
                            if (tiles[i].remappedPalette) continue;

                            for (int j = i + 1; j < tiles.Count; j++)
                            {
                                if (tiles[j].remappedPalette) continue;

                                if (tiles[i].pal.checkMerge(tiles[j].pal) == mergeTarget)
                                {
                                    //merge pals
                                    tiles[i].pal.merge(tiles[j].pal);
                                    merged = true;
                                    //replace indexes
                                    Parallel.ForEach(tiles, (t) =>
                                    {
                                        if (/*!t.remappedPalette &&*/ t.usingPalette == j)
                                        {
                                            t.remappedPalette = true;
                                            t.usingPalette = i;
                                        }
                                    });

                                    //check if we can merge pals in newly created one
                                    for (int a = 0; a < tiles.Count; a++)
                                    {
                                        if ((a == i) || tiles[a].remappedPalette) continue;

                                        if (tiles[i].pal.contains(tiles[a].pal))
                                        {
                                            tiles[a].remappedPalette = true;
                                            Parallel.ForEach(tiles, (t) =>
                                            {
                                                if (t.usingPalette == a)
                                                    t.usingPalette = i;
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        if (merged) mergeTarget = 16;
                    }
                    sw.Stop();
                    //Console.WriteLine("pals pass 2: " + sw.Elapsed);
                }
                else
                {
                    //using provided palettes

                    Parallel.ForEach(tiles, (t) =>
                    {
                        if (t.duplicateOf < 0)
                            for (int i = 0; i < fixedPalettes.Count; i++)
                                if (fixedPalettes[i].contains(t.pal))
                                {
                                    t.remappedPalette = true;
                                    t.usingPalette = i;
                                    break;
                                }
                    });

                    //check if all tiles found a palette match
                    foreach (tile t in tiles)
                        if (!t.remappedPalette)
                        {
                            status = patnStatus.fixedPaletteMismatch;
                            break;
                        }
                }
            }
        }

        public void rasterize(ref byte[] buffer, ref int index, ref List<int> skippedTiles, bool makeTexture = false)
        {
            //rasterize
            Parallel.ForEach(tiles, (t) =>
            {
                if (t.duplicateOf < 0)
                    t.rasterize(fixedPalettes.Count > 0 ? fixedPalettes[t.usingPalette] : tiles[t.usingPalette].pal, makeTexture);
            });

            //align for auto8
            while (((index >> 7) % 8) != 0) { skippedTiles.Add(index); index += 128; }

            //auto8 tiles first
            foreach (tile t in tiles.FindAll(findAuto8))
            {
                //forced blank skip ?
                if (((index >> 7) & 0xffff) == 0)
                {
                    index += 128;
                    //auto8 re-align after skip
                    while (((index >> 7) % 8) != 0) { skippedTiles.Add(index); index += 128; }
                }

                t.location = index >> 7;
                t.tileData.CopyTo(buffer, index);
                index += 128 * 8;
            }

            //auto4 tiles second (coming from auto8, we are aligned)
            foreach (tile t in tiles.FindAll(findAuto4))
            {
                //forced blank skip ?
                if (((index >> 7) & 0xffff) == 0)
                {
                    index += 128;
                    //auto4 re-align after skip
                    while (((index >> 7) % 4) != 0) { skippedTiles.Add(index); index += 128; }
                }

                t.location = index >> 7;
                t.tileData.CopyTo(buffer, index);
                index += 128 * 4;
            }

            //singles
            foreach (tile t in tiles.FindAll(findAuto1))
            {
                if (skippedTiles.Count > 0)
                {
                    //reuse a skipped slot
                    int x = skippedTiles[0];
                    skippedTiles.Remove(skippedTiles[0]);

                    t.location = x >> 7;
                    t.tileData.CopyTo(buffer, x);
                }
                else
                {
                    //forced blank skip ?
                    if (((index >> 7) & 0xffff) == 0)
                        index += 128;

                    t.location = index >> 7;
                    t.tileData.CopyTo(buffer, index);
                    index += 128;
                }
            }

            //set palettes indexes
            int px = 0;
            for (int i = 0; i < tiles.Count; i++)
                if (!tiles[i].remappedPalette)
                    tiles[i].pal.location = px++;
        }

        private int compareTiles(tile a, tile b)
        {
            if (a.autoAnim != b.autoAnim) return 0;
            if (a.hash.SequenceEqual(b.hash)) return 1; //same
            if (a.hash.SequenceEqual(b.hashFlipX)) return 2; //same, flipX
            if (a.hash.SequenceEqual(b.hashFlipY)) return 3; //same, flipY
            if (a.hash.SequenceEqual(b.hashFlipXY)) return 4; //same, flipXY
            return 0;
        }

        public string palData()
        {
            string data = "";
            int count = 0;

            if (fixedPalettes.Count > 0)
            {
                //palettes from file
                for (int i = 0; i < fixedPalettes.Count; i++)
                    data += fixedPalettes[i].toAsm() + Environment.NewLine;
                count = fixedPalettes.Count;
            }
            else
            {
                //computed palettes
                for (int i = 0; i < tiles.Count; i++)
                    if (!tiles[i].remappedPalette)
                    {
                        data += tiles[i].pal.toAsm() + Environment.NewLine;
                        count++;
                    }
            }
            return string.Format(".globl {0}_Palettes" + Environment.NewLine + "{0}_Palettes:" + Environment.NewLine + "\t.word\t0x{1:x4} ;* {1} palettes" + Environment.NewLine, objID, count) + data;
        }

        public string scrollerMapData()
        {
            string data = string.Format(".globl {0}" + Environment.NewLine + "{0}:" + Environment.NewLine, objID);

            data += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;* {0} bytes per strip / {1} tiles spr size" + Environment.NewLine, tileHeight * 4, tileHeight > 32 ? 32 : tileHeight);
            data += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;* {0} strips of {1} tiles" + Environment.NewLine, tileWidth, tileHeight);
            data += string.Format("\t.long\t{0}_Palettes" + Environment.NewLine, objID);
            data += string.Format("\t.long\t{0}", isColorStream ? string.Format("{0}_colorStream", objID) : "0x00000000");

            for (int i = 0; i < tileWidth; i++)
            {
                if (i % 16 == 0) data += Environment.NewLine + "\t.long\t";
                data += string.Format("{0}_strip{1:x4}{2}", objID, i, i % 16 != 15 ? (i == tileWidth - 1 ? "" : ", ") : "");
            }
            data += Environment.NewLine + Environment.NewLine;

            for (int i = 0; i < tileWidth; i++)
            {
                data += string.Format("{0}_strip{1:x4}:" + Environment.NewLine + "\t.word\t", objID, i);
                for (int j = 0; j < tileHeight; j++)
                    data += tileAsm((i * tileHeight) + j) + (j != (tileHeight - 1) ? ", " : "");
                data += Environment.NewLine;
            }
            return data + Environment.NewLine;
        }

        public string pictureMapData(bool flipX, bool flipY, bool flipXY)
        {
            string data = string.Format(".globl {0}" + Environment.NewLine + "{0}:" + Environment.NewLine, objID);

            data += string.Format("\t.word\t0x{0:x4}\t;*{0} bytes per strip" + Environment.NewLine, tileHeight * 4);
            data += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} strips of {1} tiles" + Environment.NewLine, tileWidth, tileHeight);
            data += string.Format("\t.long\t{0}_Palettes" + Environment.NewLine, objID);
            data += string.Format("\t.long\t{0}_Map, {0}_Map{1}, {0}_Map{2}, {0}_Map{3}" + Environment.NewLine, objID, flipX ? "_FlipX" : "", flipY ? "_FlipY" : "", flipXY ? "_FlipXY" : "");

            //base map
            data += objID + "_Map:" + Environment.NewLine;
            for (int i = 0; i < tileWidth; i++)
            {
                data += "\t.word\t";
                for (int j = 0; j < tileHeight; j++)
                    data += tileAsm((i * tileHeight) + j, 0) + (j != (tileHeight - 1) ? ", " : "");
                data += Environment.NewLine;
            }

            if (flipX)
            {
                data += objID + "_Map_FlipX:" + Environment.NewLine;
                for (int i = tileWidth - 1; i >= 0; i--)
                {
                    data += "\t.word\t";
                    for (int j = 0; j < tileHeight; j++)
                        data += tileAsm((i * tileHeight) + j, 1) + (j != (tileHeight - 1) ? ", " : "");
                    data += Environment.NewLine;
                }
            }

            if (flipY)
            {
                data += objID + "_Map_FlipY:" + Environment.NewLine;
                for (int i = 0; i < tileWidth; i++)
                {
                    data += "\t.word\t";
                    for (int j = tileHeight - 1; j >= 0; j--)
                        data += tileAsm((i * tileHeight) + j, 2) + (j != 0 ? ", " : "");
                    data += Environment.NewLine;
                }
            }

            if (flipXY)
            {
                data += objID + "_Map_FlipXY:" + Environment.NewLine;
                for (int i = tileWidth - 1; i >= 0; i--)
                {
                    data += "\t.word\t";
                    for (int j = tileHeight - 1; j >= 0; j--)
                        data += tileAsm((i * tileHeight) + j, 3) + (j != 0 ? ", " : "");
                    data += Environment.NewLine;
                }
            }
            return data;
        }

        public string framesMapData(ref List<frameInfo> frames, bool flipX, bool flipY, bool flipXY)
        {
            int x = 0;
            int maxWidth = 0;
            string topData = "";
            string botData = "";

            foreach (frameInfo frame in frames) if (frame.w > maxWidth) maxWidth = frame.w;

            topData = string.Format(".globl {0}" + Environment.NewLine + "{0}:" + Environment.NewLine, objID);
            topData += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} frames, {1} max width" + Environment.NewLine, frames.Count, maxWidth);
            topData += string.Format("\t.long\t{0}_Palettes" + Environment.NewLine, objID);
            topData += string.Format("\t.long\t{0}_animations\t;*ptr to anim data" + Environment.NewLine, objID);

            foreach (frameInfo frame in frames)
            {
                topData += string.Format("{0}_{1:x4}:" + Environment.NewLine, objID, x);
                topData += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}\t;*{0} cols of {1} tiles, {2} bytes per strip" + Environment.NewLine, frame.w, frame.h, frame.h * 4);
                topData += string.Format("\t.long\t{0}_{1:x4}_Map, {0}_{1:x4}_Map{2}, {0}_{1:x4}_Map{3}, {0}_{1:x4}_Map{4}" + Environment.NewLine, objID, x, flipX ? "_FlipX" : "", flipY ? "_FlipY" : "", flipXY ? "_FlipXY" : "");

                //base map
                botData += string.Format("{0}_{1:x4}_Map:" + Environment.NewLine, objID, x);
                for (int i = frame.x; i < frame.x + frame.w; i++)
                {
                    botData += "\t.word\t";
                    for (int j = frame.y; j < frame.y + frame.h; j++)
                        botData += tileAsm((i * tileHeight) + j, 0) + (j != (frame.y + frame.h - 1) ? ", " : "");
                    botData += Environment.NewLine;
                }

                if (flipX)
                {
                    botData += string.Format("{0}_{1:x4}_Map_FlipX:" + Environment.NewLine, objID, x);
                    for (int i = frame.x + frame.w - 1; i >= frame.x; i--)
                    {
                        botData += "\t.word\t";
                        for (int j = frame.y; j < frame.y + frame.h; j++)
                            botData += tileAsm((i * tileHeight) + j, 1) + (j != (frame.y + frame.h - 1) ? ", " : "");
                        botData += Environment.NewLine;
                    }
                }

                if (flipY)
                {
                    botData += string.Format("{0}_{1:x4}_Map_FlipY:" + Environment.NewLine, objID, x);
                    for (int i = frame.x; i < frame.x + frame.w; i++)
                    {
                        botData += "\t.word\t";
                        for (int j = frame.y + frame.h - 1; j >= frame.y; j--)
                            botData += tileAsm((i * tileHeight) + j, 2) + (j != frame.y ? ", " : "");
                        botData += Environment.NewLine;
                    }
                }

                if (flipXY)
                {
                    botData += string.Format("{0}_{1:x4}_Map_FlipXY:" + Environment.NewLine, objID, x);
                    for (int i = frame.x + frame.w - 1; i >= frame.x; i--)
                    {
                        botData += "\t.word\t";
                        for (int j = frame.y + frame.h - 1; j >= frame.y; j--)
                            botData += tileAsm((i * tileHeight) + j, 3) + (j != frame.y ? ", " : "");
                        botData += Environment.NewLine;
                    }
                }
                x++;
            }
            return topData + botData;
        }

        public string tileAsm(int x, int flipMod = 0)
        {
            tile t = tiles[x];
            if (t.duplicateOf >= 0) t = tiles[t.duplicateOf];

            return string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}",
                t.location & 0xffff,       //tile lsb, 16b
                isColorStream ? tiles[x].csPalette : t.remappedPalette ? tiles[t.usingPalette].pal.location : t.pal.location,    //palette, 8b
                (t.location >> 16) & 0xf,  //tile msb, 4b
                (t.autoAnim == 1 ? 0 : t.autoAnim) | (tiles[x].duplicateFlip > 0 ? (tiles[x].duplicateFlip - 1) ^ flipMod : tiles[x].duplicateFlip ^ flipMod)     //autoanim, 2b | flip, 2b
            );
        }

        private static bool findAuto8(tile t) { return (t.duplicateOf < 0) && (t.autoAnim == 8); }
        private static bool findAuto4(tile t) { return (t.duplicateOf < 0) && (t.autoAnim == 4); }
        private static bool findAuto1(tile t) { return (t.duplicateOf < 0) && (t.autoAnim == 1); }

        public int palsCount()
        {
            int pals = 0;
            foreach (tile t in tiles)
                if (t.duplicateOf < 0 && !t.remappedPalette)
                    pals++;
            return pals;
        }

        public void printPals()
        {
            Console.WriteLine();
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].duplicateOf >= 0)
                    Console.WriteLine(string.Format("tile[{0}] XXX", i));
                else if (tiles[i].remappedPalette)
                    Console.WriteLine(string.Format("tile[{0}].usingpalette=({1})", i, tiles[i].usingPalette));
                else Console.WriteLine(string.Format("tile[{0}].usingpalette={1}", i, tiles[i].usingPalette));
            }

            string data = "";
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].duplicateOf >= 0)
                    data += string.Format("tile[{0}] XXX" + Environment.NewLine, i);
                else if (tiles[i].remappedPalette)
                    data += string.Format("tile[{0}].usingpalette=({1})" + Environment.NewLine, i, tiles[i].usingPalette);
                else data += string.Format("tile[{0}].usingpalette={1}" + Environment.NewLine, i, tiles[i].usingPalette);
            }
            File.WriteAllText(@"Pals.txt", data);
        }

        public int tileCount()
        {
            int tls = 0;
            foreach (tile t in tiles)
                if (t.duplicateOf < 0)
                    tls += t.autoAnim;
            return tls;
        }

        public int palLocation(int x)
        {
            tile t = tiles[x];
            if (t.duplicateOf >= 0)
                t = tiles[t.duplicateOf];
            return t.remappedPalette ? tiles[t.usingPalette].pal.location : t.pal.location;
        }

        #region [=== colorStreams ===]
        //===== colorstreams =====
        private int addPalette(int palnum, ref int[] allocation, ref int[] allocationAge, ref int[] usage, ref int[] changeList)
        {
            int maxAge = 0;
            bool overWrite = false;

            //check if already allocated
            for (int x = 0; x < allocation.Length; x++)
                if (allocation[x] == palnum)
                {
                    allocationAge[x] = 0;
                    usage[palnum]++;
                    return 0;
                }

            //need to allocate into oldest slot
            maxAge = allocationAge.Max();
            for (int x = 0; x < allocation.Length; x++)
                if (allocationAge[x] == maxAge)
                {
                    allocation[x] = palnum;
                    if (allocationAge[x] < 1000000) overWrite = true;
                    allocationAge[x] = 0;
                    usage[palnum]++;

                    int y = -1;
                    while (changeList[++y] != -1) ;
                    changeList[y++] = palnum;
                    changeList[y++] = x;
                    changeList[y] = -1;
                    return overWrite ? 1 : 0;
                }
            return 0;
        }

        private void addPaletteReverse(int tileNum, ref int[] allocation, ref int[] changeList)
        {
            //already allocated?
            if (allocation[tiles[tileNum].csPalette] == palLocation(tileNum))
                return;

            //nope, check required position from tile
            allocation[tiles[tileNum].csPalette] = palLocation(tileNum);
            int y = -1;
            while (changeList[++y] != -1) ;
            changeList[y++] = palLocation(tileNum);
            changeList[y++] = tiles[tileNum].csPalette;
            changeList[y] = -1;
        }

        private void updateAge(ref int[] allocation, ref int[] allocationAge, ref int[] usage)
        {
            for (int x = 0; x < allocation.Length; x++)
                if (allocation[x] != -1)    //used slot ?
                    if (usage[allocation[x]] == 0)
                        allocationAge[x]++;
        }

        private void remapPalettes(ref int[] allocation, int startX, int endX)
        {
            int pal;

            for (int x = startX; x <= endX; x++)
                for (int y = 0; y < tileHeight; y++)
                {
                    pal = palLocation((x * tileHeight) + y);
                    for (int z = 0; z < allocation.Length; z++)
                        if (allocation[z] == pal)
                        {
                            tiles[(x * tileHeight) + y].csPalette = z;
                            break;
                        }
                }
        }

        private void remapPalettesY(ref int[] allocation, int startY, int endY)
        {
            int pal;

            for (int x = 0; x < tileWidth; x++)
                for (int y = startY; y <= endY; y++)
                {
                    pal = palLocation((x * tileHeight) + y);
                    for (int z = 0; z < allocation.Length; z++)
                        if (allocation[z] == pal)
                        {
                            tiles[(x * tileHeight) + y].csPalette = z;
                            break;
                        }
                }
        }

        private string colorStreamConfig(ref int[] allocation)
        {
            string data = "";

            for (int x = 0; x < allocation.Length; x++)
                if (allocation[x] != -1)
                    data += string.Format("\t.word\t0x{0:x4}" + Environment.NewLine + "\t.long\t{1}_Palettes+{2}" + Environment.NewLine, x << 5, objID, 2 + (allocation[x] << 5));

            return data + "\t.word\t0xffff" + Environment.NewLine;
        }

        public string doColorStream_Horizontal(ref int slotCount)
        {
            int[] allocation = null, allocationAge = null, changeList = null, usage = new int[palsCount()];
            string fwData = "", fwData2 = "", bwData = "", bwData2 = "", startData = "", endData = "", descriptor = "";
            int useCount = 0, maxUsage = 0, overWrites = 0;

            if (tileWidth <= 21) return "";    //nothing to stream
            isColorStream = true;

            //determine max usage
            for (int i = 0; i < (tileWidth - 21); i++)
            {
                Array.Clear(usage, 0, usage.Length);       //reset usage

                for (int x = i; x < i + 21; x++)           //for screen width
                    for (int y = 0; y < tileHeight; y++)   //for pattern height
                        usage[palLocation((x * tileHeight) + y)]++;

                useCount = 0;
                for (int j = 0; j < usage.Length; j++) if (usage[j] > 0) useCount++;
                if (useCount > maxUsage) maxUsage = useCount;
            }

            //maxUsage has value
            Array.Clear(usage, 0, usage.Length);    //all slots unused

            allocation = new int[maxUsage];     //hold palette #
            allocationAge = new int[maxUsage];
            for (int x = 0; x < allocationAge.Length; x++)
            {
                allocation[x] = -1;
                allocationAge[x] = 1000000;
            }
            changeList = new int[(maxUsage + 2) * 2];
            changeList[0] = -1;

            //startup palettes used
            for (int x = 0; x < 21; x++)
                for (int y = 0; y < tileHeight; y++)
                    addPalette(palLocation((x * tileHeight) + y), ref allocation, ref allocationAge, ref usage, ref changeList);

            //update tilemap palettes
            remapPalettes(ref allocation, 0, 20);

            //do start config
            startData = objID + "_colorStream_startConfig:" + Environment.NewLine + colorStreamConfig(ref allocation);

            //foreach step forward, check changes
            for (int z = 0; z < (tileWidth - 21); z++)  //22
            {
                //remove column z, add z+21
                changeList[0] = -1; //clear changes

                for (int y = 0; y < tileHeight; y++) //exiting palettes
                    usage[palLocation((z * tileHeight) + y)]--;

                //update ages
                updateAge(ref allocation, ref allocationAge, ref usage);

                //add z+21 palettes
                for (int y = 0; y < tileHeight; y++)
                    //entering palettes
                    overWrites += addPalette(palLocation(((z + 21) * tileHeight) + y), ref allocation, ref allocationAge, ref usage, ref changeList);

                //remap entering column
                remapPalettes(ref allocation, z + 21, z + 21);

                if (overWrites == 0)
                    //redo start config
                    startData = objID + "_colorStream_startConfig:" + Environment.NewLine + colorStreamConfig(ref allocation);
                else if (changeList[0] != -1) //changes occured?
                {
                    fwData += string.Format("\t.word\t0x{0:x4}" + Environment.NewLine + "\t.long\t{1}" + Environment.NewLine, (z + 1) * 16, string.Format("{0}_cStream_f{1}", objID, (z + 1) * 16));
                    fwData2 += string.Format("{0}_cStream_f{1}:" + Environment.NewLine, objID, (z + 1) * 16);
                    int y = 0;
                    while (changeList[y] != -1)
                    {
                        fwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}" + Environment.NewLine + "\t.long\t{2}_Palettes+{3}\t;* pal #{4}" + Environment.NewLine, changeList[y + 1] << 5, changeList[y + 1], objID, 2 + (changeList[y] << 5), changeList[y]);
                        y += 2;
                    }
                    fwData2 += "\t.word\t0xffff" + Environment.NewLine;
                }
            }
            fwData += objID + "_colorStream_fwDataEnd:" + Environment.NewLine + "\t.word\t0xffff" + Environment.NewLine + "\t.long\t0x00000000" + Environment.NewLine + Environment.NewLine;

            //end config
            endData = objID + "_colorStream_endConfig:" + Environment.NewLine + colorStreamConfig(ref allocation);

            //== backward scan ==
            for (int z = (tileWidth - 21); z >= 0; z--) //22
            {
                changeList[0] = -1;
                for (int y = 0; y < tileHeight; y++)
                    //check availability
                    addPaletteReverse((z * tileHeight) + y, ref allocation, ref changeList);

                //(z+1)*16
                if (changeList[0] != -1)
                {
                    //changes occured
                    bwData += string.Format("\t.word\t0x{0:x4}" + Environment.NewLine + "\t.long\t{1}" + Environment.NewLine, (z + 1) * 16, string.Format("{0}_cStream_b{1}", objID, (z + 1) * 16));
                    bwData2 += string.Format("{0}_cStream_b{1}:" + Environment.NewLine, objID, (z + 1) * 16);
                    int y = 0;
                    while (changeList[y] != -1)
                    {
                        bwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}" + Environment.NewLine + "\t.long\t{2}_Palettes+{3}\t;* pal #{4}" + Environment.NewLine, changeList[y + 1] << 5, changeList[y + 1], objID, 2 + (changeList[y] << 5), changeList[y]);
                        y += 2;
                    }
                    bwData2 += "\t.word\t0xffff" + Environment.NewLine;
                }
            }
            bwData += string.Format("{0}_colorStream_bwDataEnd:" + Environment.NewLine + "\t.word\t0x0000" + Environment.NewLine + "\t.long\t0x00000000" + Environment.NewLine + Environment.NewLine, objID);

            descriptor = string.Format(".globl {0}_colorStream" + Environment.NewLine + "{0}_colorStream:" + Environment.NewLine + "\t.word\t0x{1:x4} ;* {1} pal slots" + Environment.NewLine + "\t.long\t{0}_colorStream_startConfig" + Environment.NewLine + "\t.long\t{0}_colorStream_endConfig" + Environment.NewLine + "\t.long\t{0}_colorStream_fwData" + Environment.NewLine + "\t.long\t{0}_colorStream_fwDataEnd" + Environment.NewLine + "\t.long\t{0}_colorStream_bwData" + Environment.NewLine + "\t.long\t{0}_colorStream_bwDataEnd" + Environment.NewLine + Environment.NewLine, objID, maxUsage);
            fwData = objID + "_colorStream_fwData:" + Environment.NewLine + fwData;
            bwData = "\t.long\t0xffffffff" + Environment.NewLine + objID + "_colorStream_bwData:" + Environment.NewLine + bwData;

            slotCount = maxUsage;
            return descriptor + startData + Environment.NewLine + endData + Environment.NewLine + bwData + fwData + bwData2 + Environment.NewLine + fwData2 + Environment.NewLine;  // /!\ important order /!\
        }

        public string doColorStream_Vertical(ref int slotCount)
        {
            int[] allocation = null, allocationAge = null, changeList = null, usage = new int[palsCount()];
            string fwData = "", fwData2 = "", bwData = "", bwData2 = "", startData = "", endData = "", descriptor = "";
            int useCount = 0, maxUsage = 0, overWrites = 0;
            int scanSize = 15; //15 for 224 resolution

            if (tileHeight <= scanSize) return "";    //nothing to stream
            isColorStream = true;

            //determine max usage
            for (int i = 0; i < (tileHeight - scanSize); i++)
            {
                Array.Clear(usage, 0, usage.Length);       //reset usage

                for (int x = 0; x < tileWidth; x++)         //for map width
                    for (int y = i; y < i + scanSize; y++)    //for screen height
                        //usage[data[(y * tileWidth) + x].paletteNumber]++;
                        usage[palLocation((x * tileHeight) + y)]++;

                useCount = 0;
                for (int j = 0; j < usage.Length; j++) if (usage[j] > 0) useCount++;
                if (useCount > maxUsage) maxUsage = useCount;
            }

            //maxUsage has value
            Array.Clear(usage, 0, usage.Length);    //all slots unused

            allocation = new int[maxUsage];     //hold palette #
            allocationAge = new int[maxUsage];
            for (int x = 0; x < allocationAge.Length; x++)
            {
                allocation[x] = -1;
                allocationAge[x] = 1000000;
            }
            changeList = new int[(maxUsage + 2) * 2];
            changeList[0] = -1;

            //startup palettes used
            for (int x = 0; x < tileWidth; x++)
                for (int y = 0; y < scanSize; y++)
                    addPalette(palLocation((x * tileHeight) + y), ref allocation, ref allocationAge, ref usage, ref changeList);

            //update tilemap palettes
            remapPalettesY(ref allocation, 0, 15); //20

            //do start config
            startData = objID + "_colorStream_startConfig:" + Environment.NewLine + colorStreamConfig(ref allocation);

            //foreach step forward, check changes
            for (int z = 0; z < (tileHeight - scanSize); z++)
            {
                //remove row z, add z+scanSize
                changeList[0] = -1; //clear changes

                for (int y = 0; y < tileWidth; y++) //exiting palettes
                    usage[palLocation((y * tileHeight) + z)]--;

                //update ages
                updateAge(ref allocation, ref allocationAge, ref usage);

                //add entering row palettes (z+scanSize)
                for (int y = 0; y < tileWidth; y++)
                    overWrites += addPalette(palLocation((y * tileHeight) + (z + scanSize)), ref allocation, ref allocationAge, ref usage, ref changeList);

                //remap entering row
                remapPalettesY(ref allocation, z + scanSize, z + scanSize);

                if (overWrites == 0)
                    //redo start config
                    startData = objID + "_colorStream_startConfig:" + Environment.NewLine + colorStreamConfig(ref allocation);
                else if (changeList[0] != -1) //changes occured
                {
                    fwData += string.Format("\t.word\t0x{0:x4}" + Environment.NewLine + "\t.long\t{1}" + Environment.NewLine, (z + 1) * 16, string.Format("{0}_cStream_f{1}", objID, (z + 1) * 16));
                    fwData2 += string.Format("{0}_cStream_f{1}:" + Environment.NewLine, objID, (z + 1) * 16);
                    int y = 0;
                    while (changeList[y] != -1)
                    {
                        fwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}" + Environment.NewLine + "\t.long\t{2}_Palettes+{3}\t;* pal #{4}" + Environment.NewLine, changeList[y + 1] << 5, changeList[y + 1], objID, 2 + (changeList[y] << 5), changeList[y]);
                        y += 2;
                    }
                    fwData2 += "\t.word\t0xffff" + Environment.NewLine;
                }
            }
            fwData += objID + "_colorStream_fwDataEnd:" + Environment.NewLine + "\t.word\t0xffff" + Environment.NewLine + "\t.long\t0x00000000" + Environment.NewLine + Environment.NewLine;

            //end config
            endData = objID + "_colorStream_endConfig:" + Environment.NewLine + colorStreamConfig(ref allocation);

            //== backward scan ==
            for (int z = (tileHeight - scanSize) - 1; z >= 0; z--)
            {
                changeList[0] = -1;

                //add z
                for (int y = 0; y < tileWidth; y++)     //check availability
                    addPaletteReverse((y * tileHeight) + z, ref allocation, ref changeList);

                //(z+1)*16
                if (changeList[0] != -1)
                {
                    //changes occured
                    bwData += string.Format("\t.word\t0x{0:x4}" + Environment.NewLine + "\t.long\t{1}" + Environment.NewLine, (z + 1) * 16, string.Format("{0}_cStream_b{1}", objID, (z + 1) * 16));
                    bwData2 += string.Format("{0}_cStream_b{1}:" + Environment.NewLine, objID, (z + 1) * 16);
                    int y = 0;
                    while (changeList[y] != -1)
                    {
                        bwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}" + Environment.NewLine + "\t.long\t{2}_Palettes+{3}\t;* pal #{4}" + Environment.NewLine, changeList[y + 1] << 5, changeList[y + 1], objID, 2 + (changeList[y] << 5), changeList[y]);
                        y += 2;
                    }
                    bwData2 += "\t.word\t0xffff" + Environment.NewLine;
                }
            }
            bwData += objID + "_colorStream_bwDataEnd:" + Environment.NewLine + "\t.word\t0x0000" + Environment.NewLine + "\t.long\t0x00000000" + Environment.NewLine + Environment.NewLine;

            descriptor = string.Format(".globl {0}_colorStream" + Environment.NewLine + "{0}_colorStream:" + Environment.NewLine + "\t.word\t0x{1:x4} ;* {1} pal slots" + Environment.NewLine + "\t.long\t{0}_colorStream_startConfig" + Environment.NewLine + "\t.long\t{0}_colorStream_endConfig" + Environment.NewLine + "\t.long\t{0}_colorStream_fwData" + Environment.NewLine + "\t.long\t{0}_colorStream_fwDataEnd" + Environment.NewLine + "\t.long\t{0}_colorStream_bwData" + Environment.NewLine + "\t.long\t{0}_colorStream_bwDataEnd" + Environment.NewLine + Environment.NewLine, objID, maxUsage);
            fwData = objID + "_colorStream_fwData:" + Environment.NewLine + fwData;
            bwData = "\t.long\t0xffffffff" + Environment.NewLine + objID + "_colorStream_bwData:" + Environment.NewLine + bwData;

            slotCount = maxUsage;
            return descriptor + startData + Environment.NewLine + endData + Environment.NewLine + bwData + fwData + bwData2 + Environment.NewLine + fwData2 + Environment.NewLine;      // /!\ important order /!\
        }
        #endregion
    }

    public struct frameInfo
    {
        public int x, y, w, h;
    }

    class Program
    {
        static string charFile = "char.bin", mapFile = "maps.s", palFile = "palettes.s", incFile = "externs.h", incPrefix = "";
        static int textureSize = 0, tilesPerTexture = 0, textureTileWidth = 0, textureIdx = 0;
        static Bitmap texturePage = null;
        static string textureFolder = "";

        static void showUsage(bool showApp = true)
        {
            if (showApp)
                Console.WriteLine("BuildChar " +/*v1.0*/"- libNG / Hpman" + Environment.NewLine + "Process pictures into tiles, maps and palettes data." + Environment.NewLine);
            Console.WriteLine("Usage: BuildChar [input_file]");
#if DEBUG
            Console.Read();
#endif
        }

        static bool loadBitmap(ref Bitmap bmp, string file, ref List<palette> pals, bool parsePals = false)
        {
            try
            {
                bmp = (Bitmap)new Bitmap(file);
                if ((bmp.PixelFormat == PixelFormat.Format8bppIndexed) && parsePals)
                {
                    //parse palettes
                    int i, c = 0;
                    do
                    {
                        palette p = new palette();
                        c++;
                        for (i = 0; i < 15; i++)
                        {
                            if (c >= bmp.Palette.Entries.Length) break;

                            p.addRGBColor(bmp.Palette.Entries[c].R, bmp.Palette.Entries[c].G, bmp.Palette.Entries[c].B);
                            c++;
                        }
                        if (i > 0)
                            pals.Add(p);
                    } while (i == 15);
                }
                bmp.MakeTransparent(Color.Fuchsia); //maketransparent() turns bitmap into 32bppArgb
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            if ((bmp.Width % 16 != 0) || (bmp.Height % 16 != 0))
            {
                Console.WriteLine("Invalid dimensions (" + bmp.Width + "x" + bmp.Height + ")");
                return false;
            }
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                Console.WriteLine("Invalid format");
                return false;
            }
            return true;
        }

        static void ptnTexture(ref pattern ptn)
        {
            Graphics g;
            foreach (tile T in ptn.tiles)
            {
                if (T.duplicateOf >= 0) continue;
                for (int i = 0; i < T.autoAnim; i++)
                {
                    if (textureIdx % tilesPerTexture == 0)
                    {
                        //save page if non null
                        texturePage?.Save(textureFolder + string.Format("texture{0:d4}.png", (textureIdx - 1) / tilesPerTexture));
                    }
                    if ((textureIdx % tilesPerTexture == 0) || (texturePage == null))
                    {
                        texturePage = new Bitmap(textureSize, textureSize, PixelFormat.Format32bppArgb);
                        g = Graphics.FromImage(texturePage);
                        g.Clear(Color.Black);
                    }
                    else g = Graphics.FromImage(texturePage);

                    //copy tile
                    int pageIdx = textureIdx % tilesPerTexture;
                    int x = 20 * (pageIdx % textureTileWidth);
                    int y = 20 * (pageIdx / textureTileWidth);
                    g.DrawImage(T.texture, x, y, new Rectangle(i * 20, 0, 20, 20), GraphicsUnit.Pixel);
                    g.Dispose();
                    textureIdx++;
                }
            }
        }

        static Bitmap load32bppImage(string fName, Bitmap mask = null)
        {
            Graphics gfx;
            try
            {
                Bitmap bmp = new Bitmap(fName);
                if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    Bitmap tmpBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
                    gfx = Graphics.FromImage(tmpBmp);
                    gfx.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                    gfx.Dispose();
                    bmp.Dispose();
                    bmp = tmpBmp;
                }
                if (mask != null)
                {
                    gfx = Graphics.FromImage(bmp);
                    gfx.DrawImage(mask, 0, 0);
                    gfx.Dispose();
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to open file " + fName + Environment.NewLine + ex.Message + Environment.NewLine);
                Environment.Exit(1);
            }
            return null;
        }

        static void Main(string[] args)
        {
            //debug flags
            bool processScrolls = true, processPics = true, processSprites = true;
            //settings
            bool isFix = false, dummyFill = true;
            int baseTile = 0, startBaseTile = 0;

            //data
            string externs = "", mapData = "", palData = "", section = "";
            byte[] charData = null;
            int charDataIdx = 0;
            List<int> skippedTiles = new List<int>(0);
            List<palette> fixedPalettes = null;

            //misc vars
            XmlDocument doc = new XmlDocument();
            XmlNode node1, node2, node3, node4, node5, node6, node7, tmpNode;
            XmlNodeList tmpNodeList;
            int autoAnim, itemNum, slotCount = 0;
            Bitmap workBmp = null, workBmp1, workBmp2, workBmp3, workBmp4, workBmp5, workBmp6, workBmp7;
            pattern ptn = null;
            string str, fName, id, colorStreamType;
            bool flipX, flipY, flipXY, isColorStream;
            List<frameInfo> frames = null;
            bool makeTexture = false;

            if (args.Length != 1)
            {
                showUsage();
                Environment.Exit(1);
            }

            string xmlFile = args[0];
            Console.WriteLine("Opening " + xmlFile + "..." + Environment.NewLine);
            try
            {
                doc.Load(xmlFile);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
#if DEBUG
                Console.Read();
#endif
                Environment.Exit(1);
            }

            XmlNode node = doc.DocumentElement;
            XmlNodeList ndlist = node.SelectNodes("setup");

            //fix file data?
            if (ndlist[0].Attributes["fileType"] != null)
                if (ndlist[0].Attributes["fileType"].Value.ToUpper() == "FIX")
                    isFix = true;

            if (ndlist[0].SelectNodes("starting_tile").Item(0) != null)
            {
                startBaseTile = baseTile = int.Parse(ndlist[0].SelectNodes("starting_tile").Item(0).InnerText);

                if (ndlist[0].SelectNodes("starting_tile").Item(0).Attributes["fillmode"] != null)
                    if (ndlist[0].SelectNodes("starting_tile").Item(0).Attributes["fillmode"].Value.ToUpper() == "NONE")
                        dummyFill = false;
            }

            if (ndlist[0].SelectNodes("charfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("charfile").Item(0).InnerText;
                if (str != "") charFile = str;
            }
            if (ndlist[0].SelectNodes("mapfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("mapfile").Item(0).InnerText;
                if (str != "") mapFile = str;
            }
            if (ndlist[0].SelectNodes("palfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("palfile").Item(0).InnerText;
                if (str != "") palFile = str;
            }
            if (ndlist[0].SelectNodes("incfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("incfile").Item(0).InnerText;
                if (str != "") incFile = str;
            }
            if (ndlist[0].SelectNodes("incprefix").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("incprefix").Item(0).InnerText;
                if (str != "") incPrefix = str;
            }
            //undocumented
            if (ndlist[0].SelectNodes("textureSize").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("textureSize").Item(0).InnerText;
                if (str != "") textureSize = int.Parse(str);
            }

            if (isFix)
            {
                bool _fixSuccess = fixProcess(node);
#if DEBUG
                Console.Read();
#endif
                Environment.Exit(_fixSuccess ? 0 : 1);
            }

            //not fix data

            //allocate space for binary data
            charData = new byte[128 * 1024 * 1024]; //I has RAM

            //basetile handling
            charDataIdx = baseTile * 128;

            //imports handling
            XmlNodeList importList = node.SelectNodes("import");
            if (importList.Count > 0)
            {
                byte[] buffer;
                Console.WriteLine("Importing " + importList.Count.ToString() + " files...");

                foreach (XmlNode import in importList)
                {
                    str = "";
                    try
                    {
                        str = import.SelectSingleNode("file").InnerText;
                        Console.WriteLine("Importing " + str + "...");
                        buffer = File.ReadAllBytes(str);
                        if (buffer.Length % 128 != 0)
                        {
                            Console.WriteLine("Import file isn't 128 bytes multiple.");
                            Environment.Exit(1);
                        }
                        baseTile += buffer.Length / 128;
                        buffer.CopyTo(charData, charDataIdx);
                        charDataIdx += buffer.Length;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to import file " + str + Environment.NewLine + e.Message);
                        Environment.Exit(1);
                    }
                }
                Console.WriteLine();
            }

            if (textureSize > 0)
            {
                makeTexture = true;
                textureTileWidth = textureSize / 20;
                tilesPerTexture = textureTileWidth * textureTileWidth;
                textureIdx = baseTile;
            }

            if (processScrolls)
            {
                #region [scrollers processing]

                itemNum = 0;
                ndlist = node.SelectNodes("scrl");
                if (ndlist.Count > 0) externs += "//scrollers" + Environment.NewLine;

                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Scrollers - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode scrl in ndlist)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    //get file name & ID
                    try
                    {
                        fName = scrl.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = scrl.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //section
                    if (scrl.Attributes["section"] == null)
                        section = ".text";
                    else section = scrl.Attributes["section"].Value;
                    palData += ".section " + section + Environment.NewLine;
                    mapData += ".section " + section + Environment.NewLine;

                    //check colorStream infos
                    colorStreamType = "";
                    if (scrl.Attributes["colorStream"] != null)
                        colorStreamType = scrl.Attributes["colorStream"].Value.ToUpper();
                    switch (colorStreamType)
                    {
                        case "HORIZONTAL":
                        case "VERTICAL":
                            isColorStream = true;
                            break;
                        default:
                            isColorStream = false;
                            break;
                    }

                    // check for autofiles
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = scrl.SelectSingleNode("auto1"); node2 = scrl.SelectSingleNode("auto2"); node3 = scrl.SelectSingleNode("auto3"); node4 = scrl.SelectSingleNode("auto4"); node5 = scrl.SelectSingleNode("auto5"); node6 = scrl.SelectSingleNode("auto6"); node7 = scrl.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    fixedPalettes = new List<palette>(0);
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");
                    if (!loadBitmap(ref workBmp, fName, ref fixedPalettes, true)) return;
                    if (autoAnim >= 4)
                    {
                        if (!loadBitmap(ref workBmp1, node1.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp2, node2.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp3, node3.InnerText, ref fixedPalettes)) return;
                    }
                    if (autoAnim == 8)
                    {
                        if (!loadBitmap(ref workBmp4, node4.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp5, node5.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp6, node6.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp7, node7.InnerText, ref fixedPalettes)) return;
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.Width != workBmp1.Width) || (workBmp.Width != workBmp2.Width) || (workBmp.Width != workBmp3.Width) ||
                            (workBmp.Height != workBmp1.Height) || (workBmp.Height != workBmp2.Height) || (workBmp.Height != workBmp3.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    if (autoAnim == 8)
                        if ((workBmp.Width != workBmp4.Width) || (workBmp.Width != workBmp5.Width) || (workBmp.Width != workBmp6.Width) || (workBmp.Width != workBmp7.Width) ||
                            (workBmp.Height != workBmp4.Height) || (workBmp.Height != workBmp5.Height) || (workBmp.Height != workBmp6.Height) || (workBmp.Height != workBmp7.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    Console.WriteLine("OK");

                    Console.Write("Parsing data ");
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0} tiles{1}]", (workBmp.Height * workBmp.Width) / 256, autoAnim == 1 ? "" : string.Format(" x{0}", autoAnim)));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("... ");

                    //parse tiledata / palettes
                    ptn = new pattern(id, ref workBmp, ref workBmp1, ref workBmp2, ref workBmp3, ref workBmp4, ref workBmp5, ref workBmp6, ref workBmp7, ref fixedPalettes);

                    //"rasterize" tiles
                    ptn.rasterize(ref charData, ref charDataIdx, ref skippedTiles, makeTexture);
                    if (makeTexture)
                        ptnTexture(ref ptn);

                    if (isColorStream)
                        switch (colorStreamType)
                        {
                            case "HORIZONTAL":
                                palData += ptn.doColorStream_Horizontal(ref slotCount);
                                break;
                            case "VERTICAL":
                                palData += ptn.doColorStream_Vertical(ref slotCount);
                                break;
                            default:
                                break;
                        }

                    externs += string.Format("extern const scrollerInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine + "{1}", id, isColorStream ? string.Format("extern const colorStreamInfo {0}_colorStream;" + Environment.NewLine, id) : "");
                    palData += ptn.palData() + Environment.NewLine;
                    mapData += ptn.scrollerMapData() + Environment.NewLine;

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes{2}]", ptn.tileCount(), fixedPalettes.Count > 0 ? fixedPalettes.Count : ptn.palsCount(), isColorStream ? string.Format(" (colorStream: {0} slots)", slotCount) : ""));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                }
                #endregion
            }

            if (processPics)
            {
                #region [pictures processing]

                itemNum = 0;
                ndlist = node.SelectNodes("pict");
                if (ndlist.Count > 0) externs += Environment.NewLine + "//pictures" + Environment.NewLine;

                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Pictures - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode pict in ndlist)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    //get file name & ID
                    try
                    {
                        fName = pict.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = pict.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //section
                    if (pict.Attributes["section"] == null)
                        section = ".text";
                    else section = pict.Attributes["section"].Value;
                    palData += ".section " + section + Environment.NewLine;
                    mapData += ".section " + section + Environment.NewLine;

                    //get flip modes
                    string flipModes = "";
                    if (pict.Attributes["flips"] != null)
                        flipModes = pict.Attributes["flips"].Value.ToUpper();
                    else
                        if ((tmpNode = pict.SelectSingleNode("flips")) != null)
                        flipModes = tmpNode.InnerText.ToUpper();
                    flipX = flipModes.Contains('X');
                    flipY = flipModes.Contains('Y');
                    flipXY = flipModes.Contains('Z');



                    // check for autofiles
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = pict.SelectSingleNode("auto1"); node2 = pict.SelectSingleNode("auto2"); node3 = pict.SelectSingleNode("auto3"); node4 = pict.SelectSingleNode("auto4"); node5 = pict.SelectSingleNode("auto5"); node6 = pict.SelectSingleNode("auto6"); node7 = pict.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    fixedPalettes = new List<palette>(0);
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");
                    if (!loadBitmap(ref workBmp, fName, ref fixedPalettes, true)) return;
                    if (autoAnim >= 4)
                    {
                        if (!loadBitmap(ref workBmp1, node1.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp2, node2.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp3, node3.InnerText, ref fixedPalettes)) return;
                    }
                    if (autoAnim == 8)
                    {
                        if (!loadBitmap(ref workBmp4, node4.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp5, node5.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp6, node6.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp7, node7.InnerText, ref fixedPalettes)) return;
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.Width != workBmp1.Width) || (workBmp.Width != workBmp2.Width) || (workBmp.Width != workBmp3.Width) ||
                            (workBmp.Height != workBmp1.Height) || (workBmp.Height != workBmp2.Height) || (workBmp.Height != workBmp3.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    if (autoAnim == 8)
                        if ((workBmp.Width != workBmp4.Width) || (workBmp.Width != workBmp5.Width) || (workBmp.Width != workBmp6.Width) || (workBmp.Width != workBmp7.Width) ||
                            (workBmp.Height != workBmp4.Height) || (workBmp.Height != workBmp5.Height) || (workBmp.Height != workBmp6.Height) || (workBmp.Height != workBmp7.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    Console.WriteLine("OK");

                    Console.Write("Parsing data ");
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0} tiles{1}]", (workBmp.Height * workBmp.Width) / 256, autoAnim == 1 ? "" : string.Format(" x{0}", autoAnim)));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("... ");

                    ptn = new pattern(id, ref workBmp, ref workBmp1, ref workBmp2, ref workBmp3, ref workBmp4, ref workBmp5, ref workBmp6, ref workBmp7, ref fixedPalettes);
                    ptn.rasterize(ref charData, ref charDataIdx, ref skippedTiles, makeTexture);
                    if (makeTexture) ptnTexture(ref ptn);

                    externs += string.Format("extern const pictureInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine, id);
                    palData += ptn.palData() + Environment.NewLine;
                    mapData += ptn.pictureMapData(flipX, flipY, flipXY) + Environment.NewLine;

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes]", ptn.tileCount(), fixedPalettes.Count > 0 ? fixedPalettes.Count : ptn.palsCount()));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                } //foreach pict
                #endregion
            }

            if (false)
            {
                #region [sprites processing]

                itemNum = 0;
                ndlist = node.SelectNodes("sprt");
                if (ndlist.Count > 0) externs += Environment.NewLine + "//animated sprites" + Environment.NewLine;

                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Sprites - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode sprt in ndlist)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    //get file name & ID
                    try
                    {
                        fName = sprt.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = sprt.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //get flip modes
                    flipX = flipY = flipXY = false;
                    tmpNode = sprt.SelectSingleNode("flips");
                    if (tmpNode != null)
                    {
                        string modes = tmpNode.InnerText.ToUpper();
                        if (modes.Contains('X')) flipX = true;
                        if (modes.Contains('Y')) flipY = true;
                        if (modes.Contains('Z')) flipXY = true;
                    }

                    // check for autofiles
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = sprt.SelectSingleNode("auto1"); node2 = sprt.SelectSingleNode("auto2"); node3 = sprt.SelectSingleNode("auto3"); node4 = sprt.SelectSingleNode("auto4"); node5 = sprt.SelectSingleNode("auto5"); node6 = sprt.SelectSingleNode("auto6"); node7 = sprt.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    fixedPalettes = new List<palette>(0);
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");
                    if (!loadBitmap(ref workBmp, fName, ref fixedPalettes, true)) return;
                    if (autoAnim >= 4)
                    {
                        if (!loadBitmap(ref workBmp1, node1.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp2, node2.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp3, node3.InnerText, ref fixedPalettes)) return;
                    }
                    if (autoAnim == 8)
                    {
                        if (!loadBitmap(ref workBmp4, node4.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp5, node5.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp6, node6.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp7, node7.InnerText, ref fixedPalettes)) return;
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.Width != workBmp1.Width) || (workBmp.Width != workBmp2.Width) || (workBmp.Width != workBmp3.Width) ||
                            (workBmp.Height != workBmp1.Height) || (workBmp.Height != workBmp2.Height) || (workBmp.Height != workBmp3.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    if (autoAnim == 8)
                        if ((workBmp.Width != workBmp4.Width) || (workBmp.Width != workBmp5.Width) || (workBmp.Width != workBmp6.Width) || (workBmp.Width != workBmp7.Width) ||
                            (workBmp.Height != workBmp4.Height) || (workBmp.Height != workBmp5.Height) || (workBmp.Height != workBmp6.Height) || (workBmp.Height != workBmp7.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    Console.WriteLine("OK");

                    //parse frames & build mask
                    Bitmap mask = new Bitmap(workBmp.Width, workBmp.Height);
                    Graphics gfx = Graphics.FromImage(mask);
                    gfx.FillRectangle(Brushes.Fuchsia, 0, 0, mask.Width, mask.Height);

                    frames = new List<frameInfo>(0);
                    tmpNodeList = sprt.SelectNodes("frame");
                    foreach (XmlNode f in tmpNodeList)
                    {
                        frameInfo fi = new frameInfo();
                        string s = f.InnerText;

                        fi.x = int.Parse(s.Substring(0, s.IndexOf(',')));
                        s = s.Substring(s.IndexOf(',') + 1);
                        fi.y = int.Parse(s.Substring(0, s.IndexOf(':')));
                        s = s.Substring(s.IndexOf(':') + 1);
                        fi.w = int.Parse(s.Substring(0, s.IndexOf(',')));
                        s = s.Substring(s.IndexOf(',') + 1);
                        fi.h = int.Parse(s);
                        //TODO: check bounds
                        gfx.FillRectangle(Brushes.Cyan, fi.x << 4, fi.y << 4, fi.w << 4, fi.h << 4);
                        frames.Add(fi);
                    }
                    gfx.Dispose();
                    mask.MakeTransparent(Color.Cyan);

                    //apply mask
                    gfx = Graphics.FromImage(workBmp);
                    gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height);
                    gfx.Dispose();
                    workBmp.MakeTransparent(Color.Fuchsia);
                    if (workBmp1 != null) { gfx = Graphics.FromImage(workBmp1); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp2 != null) { gfx = Graphics.FromImage(workBmp2); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp3 != null) { gfx = Graphics.FromImage(workBmp3); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp4 != null) { gfx = Graphics.FromImage(workBmp4); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp5 != null) { gfx = Graphics.FromImage(workBmp5); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp6 != null) { gfx = Graphics.FromImage(workBmp6); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp7 != null) { gfx = Graphics.FromImage(workBmp7); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }

                    Console.Write("Parsing data ");
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0} tiles{1}]", (workBmp.Height * workBmp.Width) / 256, autoAnim == 1 ? "" : string.Format(" x{0}", autoAnim)));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("... ");

                    //process
                    ptn = new pattern(id, ref workBmp, ref workBmp1, ref workBmp2, ref workBmp3, ref workBmp4, ref workBmp5, ref workBmp6, ref workBmp7, ref fixedPalettes);
                    ptn.rasterize(ref charData, ref charDataIdx, ref skippedTiles, makeTexture);
                    if (makeTexture) ptnTexture(ref ptn);

                    externs += string.Format("extern const spriteInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine + "#include \"" + incPrefix + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + ".h\"" + Environment.NewLine, id);
                    palData += ptn.palData() + Environment.NewLine;
                    mapData += ptn.framesMapData(ref frames, flipX, flipY, flipXY) + Environment.NewLine;
                    mapData += ".include \"" + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + "_anims.s" + "\"" + Environment.NewLine + Environment.NewLine;

                    //cut frames for animator
                    if (frames.Count > 0)
                    {
                        int fCount = 0;
                        Directory.CreateDirectory(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id);
                        foreach (frameInfo fi in frames)
                        {
                            try
                            {
                                workBmp.Clone(new Rectangle(fi.x * 16, fi.y * 16, fi.w * 16, fi.h * 16), PixelFormat.Format32bppArgb).Save(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id + string.Format("\\({0:d4}) ", fCount) + id + string.Format("_{0:x4}.png", fCount));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(Environment.NewLine + Environment.NewLine + "Error saving frame: " + e.Message);
                                return;
                            }
                            fCount++;
                        }
                    }

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes]", ptn.tileCount(), fixedPalettes.Count > 0 ? fixedPalettes.Count : ptn.palsCount()));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                }
                #endregion
            }

            if (processSprites)
            {
                #region [new sprites processing]
                tileBank bank = new tileBank(charDataIdx >> 7);
                List<NGpal> sprPals = null;
                itemNum = 0;
                string palSource = "", keepID = "";
                bool keepLastPal;
                ndlist = node.SelectNodes("sprt");
                if (ndlist.Count > 0) externs += Environment.NewLine + "//animated sprites" + Environment.NewLine;

                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Sprites - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode sprt in ndlist)
                {
                    List<NGFrame> NGFrames = new List<NGFrame>(0);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    int tileCount = bank.tiles.Count;

                    //get file name & ID
                    try
                    {
                        fName = sprt.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = sprt.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //section
                    if (sprt.Attributes["section"] == null)
                        section = ".text";
                    else section = sprt.Attributes["section"].Value;
                    palData += ".section " + section + Environment.NewLine;
                    mapData += ".section " + section + Environment.NewLine;

                    //source of color data
                    if (sprt.Attributes["pal"] == null)
                        palSource = "";
                    else palSource = sprt.Attributes["pal"].Value.ToUpper();

                    //get flip modes
                    string flipModes = "";
                    if (sprt.Attributes["flips"] != null)
                        flipModes = sprt.Attributes["flips"].Value.ToUpper();
                    else
                        if ((tmpNode = sprt.SelectSingleNode("flips")) != null)
                            flipModes = tmpNode.InnerText.ToUpper();
                    flipX = flipModes.Contains('X');
                    flipY = flipModes.Contains('Y');
                    flipXY = flipModes.Contains('Z');

                    //parse options
                    bool optNoFlips = false, optNoAnim = false, optGlobalFrames = false, optForceSplit = false; ;
                    if (sprt.Attributes["opts"] != null)
                        foreach (string o in sprt.Attributes["opts"].Value.ToUpper().Split(','))
                            switch (o.Trim())
                            {
                                //don't generate pointers for flips
                                case "NOFLIPS":
                                    if (flipX == false && flipY == false && flipXY == false)
                                        optNoFlips = true;
                                    break;
                                //don't generate anim pointer / file include
                                case "NOANIM":
                                    optNoAnim = true;
                                    break;
                                //generate globals for each frame
                                case "GLOBALFRAMES":
                                    optGlobalFrames = true;
                                    break;
                                //force sprite splitting
                                case "FORCESPLIT":
                                    optForceSplit = true;
                                    break;
                            }

                    // check for autofiles
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = sprt.SelectSingleNode("auto1"); node2 = sprt.SelectSingleNode("auto2"); node3 = sprt.SelectSingleNode("auto3"); node4 = sprt.SelectSingleNode("auto4"); node5 = sprt.SelectSingleNode("auto5"); node6 = sprt.SelectSingleNode("auto6"); node7 = sprt.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    //fixedPalettes = new List<palette>(0);
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");

                    //load main file
                    Bitmap mask = null;
                    workBmp = load32bppImage(fName);
                    Color chromaKey = Color.Fuchsia;

                    switch (palSource)
                    {
                        //from pal file
                        case "FILE":
                            keepID = id;
                            keepLastPal = false;
                            //TODO

                            //work out the palette name = "palette_" + spr_XXX
                            string pal_path = fName + "palette_" + id + ".png";
                            Bitmap palBmp = new Bitmap(pal_path);
                            palBmp.MakeTransparent(chromaKey = palBmp.GetPixel(0, 0));

                            //need to apply this chroma key here
                            workBmp.MakeTransparent(chromaKey);

                            //read color data
                            sprPals = new List<NGpal>(0);
                            for (int y = 0; y < palBmp.Height; y++)
                            {
                                sprPals.Add(new NGpal());
                                sprPals[y].addColor(Color.FromArgb(0, 0, 0, 0), true);
                                for (int x = 1; x < 16; x++)
                                {
                                    sprPals[y].addColor(palBmp.GetPixel(x, y));
                                }
                            }
                            break;

                        //keep previous pal data
                        case "KEEP":
                            keepLastPal = true;
                            //tmp quick fix
                            workBmp.MakeTransparent(Color.Fuchsia);
                            break;

                        //read from pixel strips
                        case "STRIPS":
                            keepID = id;
                            keepLastPal = false;
                            sprPals = new List<NGpal>(0);
                            workBmp.MakeTransparent(chromaKey = workBmp.GetPixel(0, 0));
                            sprPals.Add(new NGpal());
                            //pal 0
                            sprPals[0].addColor(Color.FromArgb(0, 0, 0, 0), true);
                            for (int x = 1; x < 16; x++)
                                sprPals[0].addColor(workBmp.GetPixel(x, 0));
                            //extra pals?
                            for (int y = 1; y < workBmp.Height; y++)
                            {
                                if (workBmp.GetPixel(0, y) == workBmp.GetPixel(1, y)) break;
                                sprPals.Add(new NGpal());
                                sprPals[y].addColor(Color.FromArgb(0, 0, 0, 0), true);
                                for (int x = 1; x < 16; x++)
                                    sprPals[y].addColor(workBmp.GetPixel(x, y));
                            }
                            break;

                        //default scan
                        case "SCAN":
                        default:
                            keepID = id;
                            keepLastPal = false;
                            sprPals = new List<NGpal>(0);
                            //need to mask out unused areas beforehand
                            mask = new Bitmap(workBmp.Width, workBmp.Height);
                            Graphics gfx = Graphics.FromImage(mask);
                            gfx.Clear(Color.Fuchsia);
                            tmpNodeList = sprt.SelectNodes("frame");
                            foreach (XmlNode frameNode in sprt.SelectNodes("frame"))
                            {
                                if (frameNode.Attributes["outline"] != null)
                                {//new framer file
                                    foreach (XmlNode sprNode in frameNode.SelectNodes("hwspr"))
                                    {
                                        string outline = sprNode.InnerText.Trim();
                                        gfx.FillRectangle(Brushes.Black,
                                            int.Parse(outline.Split(':')[0].Split(',')[0].Trim()),
                                            int.Parse(outline.Split(':')[0].Split(',')[1].Trim()),
                                            int.Parse(outline.Split(':')[1].Split(',')[0].Trim()),
                                            int.Parse(outline.Split(':')[1].Split(',')[1].Trim())
                                        );
                                    }
                                }
                                else
                                {//old framer file, single frames only
                                    string outline = frameNode.InnerText;
                                    gfx.FillRectangle(Brushes.Black,
                                            16 * int.Parse(outline.Split(':')[0].Split(',')[0].Trim()),
                                            16 * int.Parse(outline.Split(':')[0].Split(',')[1].Trim()),
                                            16 * int.Parse(outline.Split(':')[1].Split(',')[0].Trim()),
                                            16 * int.Parse(outline.Split(':')[1].Split(',')[1].Trim())
                                        );
                                }
                            }
                            gfx.Dispose();
                            mask.MakeTransparent(Color.Black);
                            gfx = Graphics.FromImage(workBmp);
                            //gfx.DrawImage(mask, 0, 0);
                            gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height);
                            gfx.Dispose();
                            workBmp.MakeTransparent(Color.Fuchsia);
                            //mask.Save("mask.png");
                            //workBmp.Save("masked.png");

                            //scan palette
                            sprPals.Add(new NGpal());
                            sprPals[0].addColor(Color.FromArgb(0, 0, 0, 0), true);
                            workBmp.MakeTransparent(chromaKey);
                            for (int x = 0; x < workBmp.Width; x++)
                                for (int y = 0; y < workBmp.Height; y++)
                                    if (!sprPals[0].addColor(workBmp.GetPixel(x, y)))
                                    {
                                        Console.WriteLine(Environment.NewLine + "Color scan error: does not fit 1 palette.");
                                        Environment.Exit(1);
                                    }
                            break;
                    }

                    if (autoAnim >= 4)
                    {
                        (workBmp1 = load32bppImage(node1.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp2 = load32bppImage(node2.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp3 = load32bppImage(node3.InnerText, mask)).MakeTransparent(chromaKey);
                    }
                    if (autoAnim == 8)
                    {
                        (workBmp4 = load32bppImage(node4.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp5 = load32bppImage(node5.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp6 = load32bppImage(node6.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp7 = load32bppImage(node7.InnerText, mask)).MakeTransparent(chromaKey);
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.PhysicalDimension != workBmp1.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp2.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp3.PhysicalDimension))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            Environment.Exit(1);
                        }
                    if (autoAnim == 8)
                        if ((workBmp.PhysicalDimension != workBmp4.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp5.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp6.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp7.PhysicalDimension))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            Environment.Exit(1);
                        }
                    Console.WriteLine("OK");

                    //parse frames
                    tmpNodeList = sprt.SelectNodes("frame");
                    foreach (XmlNode f in tmpNodeList)
                    {
                        NGFrame frm = null;
                        string outline;
                        int factor = 1;
                        if (f.Attributes["outline"] != null)
                        {//new framer file
                            outline = f.Attributes["outline"].Value;
                        }
                        else
                        {//old framer file, single frames only
                            outline = f.InnerText;
                            factor = 16;
                        }
                        frm = new NGFrame(
                            factor * int.Parse(outline.Split(':')[0].Split(',')[0].Trim()),
                            factor * int.Parse(outline.Split(':')[0].Split(',')[1].Trim()),
                            factor * int.Parse(outline.Split(':')[1].Split(',')[0].Trim()),
                            factor * int.Parse(outline.Split(':')[1].Split(',')[1].Trim())
                        );
                        XmlNodeList _sprs = f.SelectNodes("hwspr");
                        List<XmlNode> sprs = new List<XmlNode>(0);

                        //Undocumented, look for axis data and remove it from list
                        for (int i = 0; i < _sprs.Count; i++)
                        {
                            if ((int.Parse(_sprs[i].InnerText.Split(':')[1].Split(',')[0].Trim()) == 1) && (int.Parse(_sprs[i].InnerText.Split(':')[1].Split(',')[1].Trim()) == 1))
                            {
                                frm.offsetX = int.Parse(_sprs[i].InnerText.Split(':')[0].Split(',')[0].Trim());
                                frm.offsetY = int.Parse(_sprs[i].InnerText.Split(':')[0].Split(',')[1].Trim());
                            }
                            else sprs.Add(_sprs[i]);
                        }

                        switch (sprs.Count)
                        {
                            case 0: //copy outline
                                frm.sprites.Add(new NGspr(frm.outline.X, frm.outline.Y, frm.outline.Width, frm.outline.Height));
                                break;
                            case 1:
                                if (optForceSplit) goto _forcedSplit;
                                frm.sprites.Add(new NGspr(
                                    int.Parse(sprs[0].InnerText.Split(':')[0].Split(',')[0].Trim()),
                                    int.Parse(sprs[0].InnerText.Split(':')[0].Split(',')[1].Trim()),
                                    int.Parse(sprs[0].InnerText.Split(':')[1].Split(',')[0].Trim()),
                                    int.Parse(sprs[0].InnerText.Split(':')[1].Split(',')[1].Trim())
                                    ));
                                break;
                            default:
                                _forcedSplit:
                                foreach (XmlNode s in sprs)
                                {
                                    int sx = int.Parse(s.InnerText.Split(':')[0].Split(',')[0].Trim());
                                    int sy = int.Parse(s.InnerText.Split(':')[0].Split(',')[1].Trim());
                                    int sw = int.Parse(s.InnerText.Split(':')[1].Split(',')[0].Trim());
                                    int sh = int.Parse(s.InnerText.Split(':')[1].Split(',')[1].Trim());
                                    for (int w = 0; w < sw; w += 16)
                                    {
                                        frm.sprites.Add(new NGspr(sx + w, sy, 16, sh));
                                    }
                                }
                                break;
                        }
                        NGFrames.Add(frm);
                    } //foreach frame

                    //Process tiles
                    int maxWidth = 0;
                    foreach (NGFrame frame in NGFrames)
                    {
                        frame.pickTiles(ref workBmp, ref sprPals, ref bank);
                        //compute maxWidth
                        if (frame.sprites.Count == 1)
                            maxWidth = frame.sprites[0].outline.Width / 16;
                        else maxWidth = frame.sprites.Count;
                    }

                    //process
                    if (makeTexture) ptnTexture(ref ptn);

                    externs += string.Format("extern const spriteInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine, id);
                    if (!optNoAnim)
                        externs += string.Format("#include \"" + incPrefix + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + ".h\"" + Environment.NewLine);

                    //export palettes data
                    if (!keepLastPal)
                    {
                        palData += string.Format(".globl {0}_Palettes" + Environment.NewLine + "{0}_Palettes:" + Environment.NewLine + "\t.word\t0x{1:x4} ;* {1} palettes" + Environment.NewLine, id, sprPals.Count);
                        foreach (NGpal p in sprPals)
                            palData += p.toAsm() + Environment.NewLine;
                        palData += Environment.NewLine;
                    }

                    //export map data
                    mapData += string.Format(".globl {0}" + Environment.NewLine + "{0}:" + Environment.NewLine, id);
                    mapData += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} frames, {1} max width" + Environment.NewLine, NGFrames.Count, maxWidth);
                    mapData += string.Format("\t.long\t{0}_Palettes" + Environment.NewLine, keepLastPal ? keepID : id);
                    if (!optNoAnim)
                        mapData += string.Format("\t.long\t{0}_animations\t;*ptr to anim data" + Environment.NewLine, id);
                    int fNum = 0;
                    string fInfo = "", fMap = "", globals = "";
                    foreach (NGFrame frame in NGFrames)
                    {
                        if (optGlobalFrames)
                        {
                            globals += string.Format(".globl\t{0}_{1:x4}" + Environment.NewLine, id, fNum);
                            externs+= string.Format("extern const sprFrame {0}_{1:x4};" + Environment.NewLine, id, fNum);
                        }
                        frame.toAsm(ref fInfo, ref fMap, id, fNum++, flipX, flipY, flipXY, optNoFlips);
                    }
                    mapData += globals + fInfo + fMap;
                    if (optNoAnim)
                        mapData += Environment.NewLine;
                    else mapData += Environment.NewLine + ".include \"" + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + "_anims.s" + "\"" + Environment.NewLine + Environment.NewLine;


                    //cut frames for animator
                    if (NGFrames.Count > 0)
                    {
                        int fCount = 0;
                        Directory.CreateDirectory(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id);
                        foreach (NGFrame f in NGFrames)
                        {
                            try
                            {
                                workBmp.Clone(f.outline, PixelFormat.Format32bppArgb).Save(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id + string.Format("\\({0:d4}) ", fCount) + id + string.Format("_{0:x4}.png", fCount));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(Environment.NewLine + Environment.NewLine + "Error saving frame: " + e.Message);
                                return;
                            }
                            fCount++;
                        }
                    }

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes]", bank.tiles.Count - tileCount, sprPals.Count));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                } //foreach sprt

                //rasterize & save tile data
                Parallel.ForEach(bank.tiles, tl =>
                {
                    tl.raster();
                });

                foreach (NGTile tl in bank.tiles)
                {
                    tl.rasterized.CopyTo(charData, charDataIdx);
                    charDataIdx += 128;
                }
                #endregion
            }

            Console.WriteLine(Environment.NewLine + "Processed tiles: " + (charDataIdx - (startBaseTile * 128)) / 128);
            Console.WriteLine("Next free tile: " + charDataIdx / 128);
            Console.Write("Saving...");

            //save text files
            File.WriteAllText(incFile, externs);
            File.WriteAllText(mapFile, mapData);
            File.WriteAllText(palFile, palData);

            //save char binary
            FileStream fs = File.Open(charFile, FileMode.Create);
            if (dummyFill)
                fs.Write(charData, 0, charDataIdx);
            else fs.Write(charData, startBaseTile * 128, charDataIdx - (startBaseTile * 128));
            fs.Close();
            Console.WriteLine(" OK.");

            //save texture
            if (makeTexture && (texturePage != null))
                texturePage.Save(textureFolder + string.Format("texture{0:d4}.png", (textureIdx - 1) / tilesPerTexture));

#if DEBUG
            Console.Read();
#endif
        }

        static bool fixProcess(XmlNode node)
        {
            //data
            string externs = "", palData = "", section = "";
            byte[] fixData = new byte[0x20000];
            List<palette> fixedPalettes = null;
            fixPattern ptn;

            //misc
            int bank, tileCount, bankCount;
            string fName, id;
            Bitmap fixBmp = null;

            //XmlNodeList importList = node.SelectNodes("import");
            foreach (XmlNode import in node.SelectNodes("import"))
            {
                try
                {
                    bank = int.Parse(import.Attributes["bank"].Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "File \"bank\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                try
                {
                    fName = import.SelectSingleNode("file").InnerText;
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                Console.WriteLine("Importing " + fName + "...");

                byte[] buffer = File.ReadAllBytes(fName);
                int idx = bank * 8192;

                if (buffer.Length + idx > 0x20000)
                {
                    Console.WriteLine("Import exceeds fix capacity");
                    return false;
                }
                if (buffer.Length % 32 != 0)
                {
                    Console.WriteLine("Import file isn't 32 bytes multiple.");
                    return false;
                }

                buffer.CopyTo(fixData, idx);
            }

            XmlNodeList fixList = node.SelectNodes("fix");

            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("## Fix data - " + fixList.Count.ToString() + " items");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            int itemNum = 0;
            foreach (XmlNode fix in fixList)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + fixList.Count + ": ");
                Console.ForegroundColor = ConsoleColor.Gray;

                try
                {
                    bank = int.Parse(fix.Attributes["bank"].Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "File \"bank\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                try
                {
                    fName = fix.SelectSingleNode("file").InnerText;
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                try
                {
                    id = fix.Attributes["id"].Value;
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                Console.WriteLine(string.Format("{0} ({1})", id, fName));

                //section
                if (fix.Attributes["section"] == null)
                    section = ".text";
                else section = fix.Attributes["section"].Value;
                palData += ".section " + section + Environment.NewLine;

                //load & check bitmap
                Console.Write("Loading file... ");
                fixedPalettes = new List<palette>(0);
                if (!loadBitmap(ref fixBmp, fName, ref fixedPalettes, true)) return false;
                if ((fixBmp.Width % 128 != 0) || (fixBmp.Height != 128))
                {
                    Console.WriteLine("Invalid image dimensions.");
                    return false;
                }
                tileCount = fixBmp.Width * 2;
                bankCount = tileCount / 256;

                if (bankCount + bank > 15)
                {
                    Console.WriteLine("Exceeding fix capacity.");
                    return false;
                }
                Console.WriteLine("OK");

                //process fix file
                ptn = new fixPattern(id, bank, ref fixBmp, ref fixedPalettes);
                ptn.rasterize(ref fixData);

                externs += string.Format("extern const paletteInfo {0}_Palettes;" + Environment.NewLine, id);
                palData += ptn.palData() + Environment.NewLine;
            }

            Console.Write("Saving...");

            //save text files
            File.WriteAllText(incFile, externs);
            File.WriteAllText(palFile, palData);
            //save binary
            File.WriteAllBytes(charFile, fixData);

            Console.WriteLine(" OK.");
#if DEBUG
            Console.Read();
#endif
            return true;
        }

    }

    public class fixTile
    {
        public Bitmap tile = null;
        public int location, usingPalette = -1;
        public bool remappedPalette = false;

        public byte[] tileData = null;
        public palette pal = null;

        public fixTile(ref Bitmap bmp, int x, int y)
        {
            tile = new Bitmap(8, 8, PixelFormat.Format32bppArgb);
            Graphics gfx = Graphics.FromImage(tile);
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.DrawImage(bmp, new Rectangle(0, 0, 8, 8), x, y, 8, 8, GraphicsUnit.Pixel);
            gfx.Dispose();
        }

        public bool makePalette()
        {
            byte r, g, b;

            BitmapData bmpData = tile.LockBits(new Rectangle(0, 0, 8, 8), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            pal = new palette();
            unsafe
            {
                byte* ptr;
                ptr = (byte*)bmpData.Scan0;
                for (int x = 0; x < 64; x++)
                {
                    b = *ptr++;
                    g = *ptr++;
                    r = *ptr++;
                    if (*ptr++ != 0) //alpha not 0, pixel is visible
                        if (!pal.addRGBColor(r, g, b))
                        {
                            tile.UnlockBits(bmpData);
                            return false;
                        }
                }
            }
            tile.UnlockBits(bmpData);
            return true;
        }

        public void rasterize(palette pal)
        {
            int idx = 0;
            byte r, g, b;
            byte[] data = new byte[64];
            tileData = new byte[32];

            //build px index table
            BitmapData bmpData = tile.LockBits(new Rectangle(0, 0, 8, 8), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* ptr;
                ptr = (byte*)bmpData.Scan0;

                for (int x = 0; x < 64; x++)
                {
                    b = *ptr++;
                    g = *ptr++;
                    r = *ptr++;

                    data[idx++] = (byte)((*ptr++ != 0) ? (pal.colors.IndexOf(ColorTool.NeoColor(r, g, b)) + 1) : 0);
                }
            }
            tile.UnlockBits(bmpData);

            for (int y = 0; y < 8; y++)
            {
                tileData[y] = (byte)((data[y * 8 + 5] << 4) | data[y * 8 + 4]);
                tileData[y + 8] = (byte)((data[y * 8 + 7] << 4) | data[y * 8 + 6]);
                tileData[y + 16] = (byte)((data[y * 8 + 1] << 4) | data[y * 8 + 0]);
                tileData[y + 24] = (byte)((data[y * 8 + 3] << 4) | data[y * 8 + 2]);
            }
            data = null;
        }
    }

    public class fixPattern
    {
        int tileWidth, tileHeight, bank;
        string objID;
        List<fixTile> tiles;
        List<palette> fixedPalettes;
        public patnStatus status = patnStatus.OK;

        public fixPattern(string id, int bk, ref Bitmap sourceBmp, ref List<palette> fixedPals)
        {
            objID = id;
            fixedPalettes = fixedPals;
            bank = bk;

            tileWidth = sourceBmp.Width >> 3;
            tileHeight = sourceBmp.Height >> 3;

            tiles = new List<fixTile>(0);
            for (int b = 0; b < sourceBmp.Width; b += 128)
                for (int h = 0; h < 128; h += 8)
                    for (int w = 0; w < 128; w += 8)
                        tiles.Add(new fixTile(ref sourceBmp, w + b, h));

            //each tile starts with its own palette
            for (int i = 0; i < tiles.Count; i++)
                tiles[i].usingPalette = i;

            //build palette for each tile
            Parallel.ForEach(tiles, (t) =>
            {
                t.makePalette();
            });

            //check color overload
            foreach (fixTile t in tiles)
                if (t.pal.colors.Count > 15)
                {
                    status = patnStatus.colorOverload;
                    break;
                }

            if (status == patnStatus.OK)
            {
                if (fixedPalettes.Count == 0)
                {
                    //optimize palettes
                    for (int i = 0; i < tiles.Count - 1; i++)
                    {
                        if (tiles[i].remappedPalette) continue;

                        for (int j = i + 1; j < tiles.Count; j++)
                        {
                            if (!tiles[j].remappedPalette)
                            {
                                //both palettes used
                                if (tiles[j].pal.colors.Count <= tiles[i].pal.colors.Count)
                                {
                                    if (tiles[i].pal.contains(tiles[j].pal))
                                    {
                                        tiles[j].usingPalette = i;
                                        tiles[j].remappedPalette = true;
                                    }
                                }
                                else
                                {
                                    if (tiles[j].pal.contains(tiles[i].pal))
                                    {
                                        tiles[i].usingPalette = j;
                                        tiles[i].remappedPalette = true;
                                        Parallel.ForEach(tiles, (t) =>
                                        {
                                            if (t.remappedPalette && t.usingPalette == i)
                                                t.usingPalette = j;
                                        });
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //merge pass
                    int mergeTarget = 16;
                    while (--mergeTarget > 1)
                    {
                        bool merged = false;
                        for (int i = 0; i < tiles.Count - 1; i++)
                        {
                            if (tiles[i].remappedPalette) continue;

                            for (int j = i + 1; j < tiles.Count; j++)
                            {
                                if (tiles[j].remappedPalette) continue;

                                if (tiles[i].pal.checkMerge(tiles[j].pal) == mergeTarget)
                                {
                                    //merge pals
                                    tiles[i].pal.merge(tiles[j].pal);
                                    merged = true;
                                    //replace indexes
                                    Parallel.ForEach(tiles, (t) =>
                                    {
                                        if (/*!t.remappedPalette &&*/ t.usingPalette == j)
                                        {
                                            t.remappedPalette = true;
                                            t.usingPalette = i;
                                        }
                                    });

                                    //check if we can merge pals in newly created one
                                    for (int a = 0; a < tiles.Count; a++)
                                    {
                                        if ((a == i) || tiles[a].remappedPalette) continue;

                                        if (tiles[i].pal.contains(tiles[a].pal))
                                        {
                                            tiles[a].remappedPalette = true;
                                            Parallel.ForEach(tiles, (t) =>
                                            {
                                                if (t.usingPalette == a)
                                                    t.usingPalette = i;
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        if (merged) mergeTarget = 16;
                    }
                }
                else
                {
                    //using provided palettes
                    Parallel.ForEach(tiles, (t) =>
                    {
                        for (int i = 0; i < fixedPalettes.Count; i++)
                            if (fixedPalettes[i].contains(t.pal))
                            {
                                t.remappedPalette = true;
                                t.usingPalette = i;
                                break;
                            }
                    });

                    //check if all tiles found a palette match
                    foreach (fixTile t in tiles)
                        if (!t.remappedPalette)
                        {
                            status = patnStatus.fixedPaletteMismatch;
                            break;
                        }
                }
            }

        }

        public void rasterize(ref byte[] buffer)
        {
            int idx = bank * 0x2000;
            byte b;

            //rasterize
            Parallel.ForEach(tiles, (t) =>
            {
                t.rasterize(fixedPalettes.Count > 0 ? fixedPalettes[t.usingPalette] : tiles[t.usingPalette].pal);
            });

            //write in buffer
            for (int i = 0; i < tiles.Count; i++)
                for (int x = 0; x < 32; idx++, x++)
                    if ((b = tiles[i].tileData[x]) != 0)
                        buffer[idx] = b;
        }

        public string palData()
        {
            string data = "";
            int count = 0;

            if (fixedPalettes.Count > 0)
            {
                //palettes from file
                for (int i = 0; i < fixedPalettes.Count; i++)
                    data += fixedPalettes[i].toAsm() + Environment.NewLine;
                count = fixedPalettes.Count;
            }
            else
            {
                //computed palettes
                for (int i = 0; i < tiles.Count; i++)
                    if (!tiles[i].remappedPalette)
                    {
                        data += tiles[i].pal.toAsm() + Environment.NewLine;
                        count++;
                    }
            }
            return string.Format(".globl {0}_Palettes" + Environment.NewLine + "{0}_Palettes:" + Environment.NewLine + "\t.word\t0x{1:x4} ;* {1} palettes" + Environment.NewLine, objID, count) + data;
        }

        public void printPals()
        {
            Console.WriteLine();
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].remappedPalette)
                    Console.WriteLine(string.Format("tile[{0}].usingpalette=({1})", i, tiles[i].usingPalette));
                else Console.WriteLine(string.Format("tile[{0}].usingpalette={1}", i, tiles[i].usingPalette));
            }

            string data = "";
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].remappedPalette)
                    data += string.Format("tile[{0}].usingpalette=({1})" + Environment.NewLine, i, tiles[i].usingPalette);
                else data += string.Format("tile[{0}].usingpalette={1}" + Environment.NewLine, i, tiles[i].usingPalette);
            }
            File.WriteAllText(@"Pals.txt", data);
        }

    }
}

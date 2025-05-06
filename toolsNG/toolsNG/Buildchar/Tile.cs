using NeoTools;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Buildchar
{

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
}

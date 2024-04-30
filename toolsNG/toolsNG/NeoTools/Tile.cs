using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Linq;

namespace NeoTools
{
    //public static byte[] auto4hash = { 46, 56, 132, 98, 9, 247, 191, 112, 54, 98, 161, 213, 69, 241, 152, 122 };

    public class Tile
    {
        private Bitmap bmp = null, bmp1 = null, bmp2 = null, bmp3 = null, bmp4 = null, bmp5 = null, bmp6 = null, bmp7 = null;
        private Bitmap bmpFlipX = null;
        private Bitmap bmpFlipY = null;
        private Bitmap bmpFlipXY = null;

        public byte[] hash, hash1, hash2, hash3, hash4, hash5, hash6, hash7, hashFlipX, hashFlipY, hashFlipXY;
        private byte[] auto4hash = { 46, 56, 132, 98, 9, 247, 191, 112, 54, 98, 161, 213, 69, 241, 152, 122 };
        public int autoAnim;
        public Bitmap autoAnimStrip = null;
        //private Bitmap autoAnimStripFlip = null;
        public byte[] stripHash, stripHashFlipX, stripHashFlipY, stripHashFlipXY;
        public bool isDummy = false;
        public int realIndex = 0;

        public Tile(bool dmy = false)
        {
            bmp = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmpFlipX = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmpFlipY = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmpFlipXY = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            autoAnim = 1;
            isDummy = dmy;
        }

        public void tileGet(ref Bitmap src, int srcX, int srcY)
        {
            byte[] data = new byte[16 * 16 * 4];
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            BitmapData bdata;
            Graphics g = null;
            
            g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata = bmp.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            bmp.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            g = Graphics.FromImage(bmpFlipX);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bmpFlipX.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bdata = bmpFlipX.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            bmpFlipX.UnlockBits(bdata);
            hashFlipX = md5.ComputeHash(data);

            g = Graphics.FromImage(bmpFlipY);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bmpFlipY.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bdata = bmpFlipY.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            bmpFlipY.UnlockBits(bdata);
            hashFlipY = md5.ComputeHash(data);

            g = Graphics.FromImage(bmpFlipXY);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bmpFlipXY.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            bdata = bmpFlipXY.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            bmpFlipXY.UnlockBits(bdata);
            hashFlipXY = md5.ComputeHash(data);

            bmpFlipX = null;
            bmpFlipY = null;
            bmpFlipXY = null;
        }

        public void tileGet(Bitmap src, Bitmap src1, Bitmap src2, Bitmap src3, int srcX, int srcY)
        {
            byte[] data = new byte[16 * 16 * 4];
            byte[] data1 = new byte[16 * 16 * 4];
            byte[] data2 = new byte[16 * 16 * 4];
            byte[] data3 = new byte[16 * 16 * 4];

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            BitmapData bdata, bdata1, bdata2, bdata3;
            Graphics g = null;

            bmp1 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp2 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp3 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);

            g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata = bmp.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            bmp.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            g = Graphics.FromImage(bmp1);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src1, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata1 = bmp1.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata1.Scan0, data1, 0, 16 * 16 * 4);
            bmp1.UnlockBits(bdata1);
            hash1 = md5.ComputeHash(data1);

            g = Graphics.FromImage(bmp2);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src2, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata2 = bmp2.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata2.Scan0, data2, 0, 16 * 16 * 4);
            bmp2.UnlockBits(bdata2);
            hash2 = md5.ComputeHash(data2);

            g = Graphics.FromImage(bmp3);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src3, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata3 = bmp3.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata3.Scan0, data3, 0, 16 * 16 * 4);
            bmp3.UnlockBits(bdata3);
            hash3 = md5.ComputeHash(data3);

            if (hash.SequenceEqual(hash1) && hash.SequenceEqual(hash2) && hash.SequenceEqual(hash3))
            {
                //all 4 are identical
                autoAnim = 1;

                //original*******************************
                g = Graphics.FromImage(bmp);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bdata = bmp.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmp.UnlockBits(bdata);
                hash = md5.ComputeHash(data);

                g = Graphics.FromImage(bmpFlipX);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bmpFlipX.RotateFlip(RotateFlipType.RotateNoneFlipX);
                bdata = bmpFlipX.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmpFlipX.UnlockBits(bdata);
                hashFlipX = md5.ComputeHash(data);

                g = Graphics.FromImage(bmpFlipY);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bmpFlipY.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bdata = bmpFlipY.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmpFlipY.UnlockBits(bdata);
                hashFlipY = md5.ComputeHash(data);

                g = Graphics.FromImage(bmpFlipXY);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bmpFlipXY.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                bdata = bmpFlipXY.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmpFlipXY.UnlockBits(bdata);
                hashFlipXY = md5.ComputeHash(data);

                bmpFlipX.Dispose();// = null;
                bmpFlipY.Dispose();// = null;
                bmpFlipXY.Dispose();// = null;
            }
            else
            {
                //different tiles, need autoanim
                Bitmap reverseStrip = new Bitmap(64, 16, PixelFormat.Format32bppArgb);
                autoAnim = 4;
                data = new byte[16 * 16 * 4 * autoAnim];

                autoAnimStrip = new Bitmap(64, 16, PixelFormat.Format32bppArgb);
                //build tile strip
                g = Graphics.FromImage(autoAnimStrip);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src1, new Rectangle(16, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src2, new Rectangle(32, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src3, new Rectangle(48, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();

                g = Graphics.FromImage(reverseStrip);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(48, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src1, new Rectangle(32, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src2, new Rectangle(16, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src3, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();

                bdata = autoAnimStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                autoAnimStrip.UnlockBits(bdata);
                stripHash = md5.ComputeHash(data);
                //autoAnimStrip.Save("strip0.png");

                autoAnimStrip.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bdata = autoAnimStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                autoAnimStrip.UnlockBits(bdata);
                stripHashFlipY = md5.ComputeHash(data); //flip Y
                //autoAnimStrip.Save("stripY.png");
                autoAnimStrip.RotateFlip(RotateFlipType.RotateNoneFlipY);

                reverseStrip.RotateFlip(RotateFlipType.RotateNoneFlipX);
                bdata = reverseStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                reverseStrip.UnlockBits(bdata);
                stripHashFlipX = md5.ComputeHash(data); //flip X
                //reverseStrip.Save("stripX.png");

                reverseStrip.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bdata = reverseStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                reverseStrip.UnlockBits(bdata);
                stripHashFlipXY = md5.ComputeHash(data); //flip XY
                //reverseStrip.Save("stripZ.png");

                //autoAnimStripFlip = null;
            }
        }

        public void tileGet(Bitmap src, Bitmap src1, Bitmap src2, Bitmap src3, Bitmap src4, Bitmap src5, Bitmap src6, Bitmap src7, int srcX, int srcY)
        {
            byte[] data = new byte[16 * 16 * 4];
            byte[] data1 = new byte[16 * 16 * 4];
            byte[] data2 = new byte[16 * 16 * 4];
            byte[] data3 = new byte[16 * 16 * 4];
            byte[] data4 = new byte[16 * 16 * 4];
            byte[] data5 = new byte[16 * 16 * 4];
            byte[] data6 = new byte[16 * 16 * 4];
            byte[] data7 = new byte[16 * 16 * 4];

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            BitmapData bdata, bdata1, bdata2, bdata3, bdata4, bdata5, bdata6, bdata7;
            Graphics g = null;

            bmp1 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp2 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp3 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp4 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp5 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp6 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            bmp7 = new Bitmap(16, 16, PixelFormat.Format32bppArgb);

            g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata = bmp.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
            bmp.UnlockBits(bdata);
            hash = md5.ComputeHash(data);

            g = Graphics.FromImage(bmp1);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src1, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata1 = bmp1.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata1.Scan0, data1, 0, 16 * 16 * 4);
            bmp1.UnlockBits(bdata1);
            hash1 = md5.ComputeHash(data1);

            g = Graphics.FromImage(bmp2);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src2, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata2 = bmp2.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata2.Scan0, data2, 0, 16 * 16 * 4);
            bmp2.UnlockBits(bdata2);
            hash2 = md5.ComputeHash(data2);

            g = Graphics.FromImage(bmp3);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src3, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata3 = bmp3.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata3.Scan0, data3, 0, 16 * 16 * 4);
            bmp3.UnlockBits(bdata3);
            hash3 = md5.ComputeHash(data3);

            g = Graphics.FromImage(bmp4);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src4, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata4 = bmp4.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata4.Scan0, data4, 0, 16 * 16 * 4);
            bmp4.UnlockBits(bdata4);
            hash4 = md5.ComputeHash(data4);

            g = Graphics.FromImage(bmp5);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src5, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata5 = bmp5.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata5.Scan0, data5, 0, 16 * 16 * 4);
            bmp5.UnlockBits(bdata5);
            hash5 = md5.ComputeHash(data5);

            g = Graphics.FromImage(bmp6);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src6, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata6 = bmp6.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata6.Scan0, data6, 0, 16 * 16 * 4);
            bmp6.UnlockBits(bdata6);
            hash6 = md5.ComputeHash(data6);

            g = Graphics.FromImage(bmp7);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src7, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
            g.Dispose();
            bdata7 = bmp7.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bdata7.Scan0, data7, 0, 16 * 16 * 4);
            bmp7.UnlockBits(bdata7);
            hash7 = md5.ComputeHash(data7);

            if ((hash.SequenceEqual(hash1) && hash.SequenceEqual(hash2) && hash.SequenceEqual(hash3) && hash.SequenceEqual(hash4) && hash.SequenceEqual(hash5) && hash.SequenceEqual(hash6) && hash.SequenceEqual(hash7))
                || ((hash.SequenceEqual(hash1) && hash.SequenceEqual(hash2) && hash.SequenceEqual(hash3)) && hash4.SequenceEqual(auto4hash))
                )
            {
                //all 8 are identical
                autoAnim = 1;

                //original*******************************
                g = Graphics.FromImage(bmp);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bdata = bmp.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmp.UnlockBits(bdata);
                hash = md5.ComputeHash(data);

                g = Graphics.FromImage(bmpFlipX);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bmpFlipX.RotateFlip(RotateFlipType.RotateNoneFlipX);
                bdata = bmpFlipX.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmpFlipX.UnlockBits(bdata);
                hashFlipX = md5.ComputeHash(data);

                g = Graphics.FromImage(bmpFlipY);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bmpFlipY.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bdata = bmpFlipY.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmpFlipY.UnlockBits(bdata);
                hashFlipY = md5.ComputeHash(data);

                g = Graphics.FromImage(bmpFlipXY);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.Dispose();
                bmpFlipXY.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                bdata = bmpFlipXY.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4);
                bmpFlipXY.UnlockBits(bdata);
                hashFlipXY = md5.ComputeHash(data);

                bmpFlipX.Dispose();// = null;
                bmpFlipY.Dispose();// = null;
                bmpFlipXY.Dispose();// = null;
            }
            else
            {
                //different tiles, need autoanim
                autoAnim = 8;
                if (hash4.SequenceEqual(auto4hash)) autoAnim = 4; //auto 4 stopper tile found
                Bitmap reverseStrip = new Bitmap(16 * autoAnim, 16, PixelFormat.Format32bppArgb);

                data = new byte[16 * 16 * 4 * autoAnim];

                autoAnimStrip = new Bitmap(16 * autoAnim, 16, PixelFormat.Format32bppArgb);
                //build tile strip
                g = Graphics.FromImage(autoAnimStrip);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src1, new Rectangle(16, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src2, new Rectangle(32, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                g.DrawImage(src3, new Rectangle(48, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                if (autoAnim == 8)
                {
                    g.DrawImage(src4, new Rectangle(64, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src5, new Rectangle(80, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src6, new Rectangle(96, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src7, new Rectangle(112, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                }
                g.Dispose();

                //build reverse strip
                g = Graphics.FromImage(reverseStrip);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                if (autoAnim == 8)
                {
                    g.DrawImage(src, new Rectangle(112, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src1, new Rectangle(96, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src2, new Rectangle(80, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src3, new Rectangle(64, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src4, new Rectangle(48, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src5, new Rectangle(32, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src6, new Rectangle(16, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src7, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(src, new Rectangle(48, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src1, new Rectangle(32, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src2, new Rectangle(16, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                    g.DrawImage(src3, new Rectangle(0, 0, 16, 16), new Rectangle(srcX, srcY, 16, 16), GraphicsUnit.Pixel);
                }
                g.Dispose();


                bdata = autoAnimStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                autoAnimStrip.UnlockBits(bdata);
                stripHash = md5.ComputeHash(data);
                //autoAnimStrip.Save("strip0.png");

                autoAnimStrip.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bdata = autoAnimStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                autoAnimStrip.UnlockBits(bdata);
                stripHashFlipY = md5.ComputeHash(data); //flip Y
                //autoAnimStrip.Save("stripY.png");
                autoAnimStrip.RotateFlip(RotateFlipType.RotateNoneFlipY);

                reverseStrip.RotateFlip(RotateFlipType.RotateNoneFlipX);
                bdata = reverseStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                reverseStrip.UnlockBits(bdata);
                stripHashFlipX = md5.ComputeHash(data); //flip X
                //reverseStrip.Save("stripX.png");

                reverseStrip.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bdata = reverseStrip.LockBits(new Rectangle(0, 0, 16 * autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bdata.Scan0, data, 0, 16 * 16 * 4 * autoAnim);
                reverseStrip.UnlockBits(bdata);
                stripHashFlipXY = md5.ComputeHash(data); //flip XY
                                                         //reverseStrip.Save("stripZ.png");

                //autoAnimStripFlip = null;
                bmpFlipX.Dispose();// = null;
                bmpFlipY.Dispose();// = null;
                bmpFlipXY.Dispose();// = null;
            }
        }

        public Bitmap getBitmap(int x = 0)
        {
            switch (x)
            {
                case 0:
                    return bmp;
                case 1:
                    return bmpFlipX;
                case 2:
                    return bmpFlipY;
                case 3:
                    return bmpFlipXY;
                default:
                    return bmp;
            }
        }

        //public Bitmap getautoAnimStrip(int x = 0)
        //{
        //    return (autoAnimStrip);
        //}

        public byte GetColorIndex(int pixel, Palette pal, int subtile = -1)
        {
            BitmapData data;
            Bitmap tmpBmp = null;
            int r, g, b, a;

            switch (subtile)
            {
                case 1:
                    tmpBmp = bmp1;
                    break;
                case 2:
                    tmpBmp = bmp2;
                    break;
                case 3:
                    tmpBmp = bmp3;
                    break;
                case 4:
                    tmpBmp = bmp4;
                    break;
                case 5:
                    tmpBmp = bmp5;
                    break;
                case 6:
                    tmpBmp = bmp6;
                    break;
                case 7:
                    tmpBmp = bmp7;
                    break;
                default:
                    tmpBmp = bmp;
                    break;
            }

            data = tmpBmp.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr;
                ptr = (byte*)data.Scan0;
                ptr += (pixel * 4);
                b = *ptr;
                ptr++;
                g = *ptr;
                ptr++;
                r = *ptr;
                ptr++;
                a = *ptr;
            }

            tmpBmp.UnlockBits(data);

            if (a == 0) return (0); //transparent pixel
            else return pal.GetColorIndex((byte)r, (byte)g, (byte)b);
        }
    }
}

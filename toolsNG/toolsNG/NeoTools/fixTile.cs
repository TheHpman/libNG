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
    public class fixTile
    {
        public Bitmap bmp = null;

        public fixTile()
        {
            bmp = new Bitmap(8, 8, PixelFormat.Format32bppArgb);
        }

        public void tileGet(Bitmap src, int srcX, int srcY)
        {
            Graphics g = null;

            g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, 8, 8), new Rectangle(srcX, srcY, 8, 8), GraphicsUnit.Pixel);
            g.Dispose();
        }

        public byte GetColorIndex(int pixel, Palette pal)
        {
            BitmapData data;
            Bitmap tmpBmp = null;
            int r, g, b, a;

            tmpBmp = bmp;

            data = tmpBmp.LockBits(new Rectangle(0, 0, 8, 8), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

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

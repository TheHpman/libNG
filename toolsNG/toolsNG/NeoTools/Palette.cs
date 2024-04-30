using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace NeoTools
{
    public class Palette
    {
        public ushort[] NGColor;
        public int colorCount;

        public Palette()
        {
            NGColor = new ushort[16];
            for (int i = 0; i < 16; i++)
            {
                NGColor[i] = 0x8000;
            }
            colorCount = 0;
        }
        /*
        public Palette(int red, int green, int blue)
        {
            R = new int[16];
            G = new int[16];
            B = new int[16];
            for (int i = 0; i < 16; i++)
            {
                R[i] = 0;
                G[i] = 0;
                B[i] = 0;
            }
            R[0] = red;
            G[0] = green;
            B[0] = blue;
            colorCount = 0;
        }
        */
        //public int Count()
        //{
        //    return colorCount;
        //}

        public bool Contain(ushort neoColor)
        {
            for (int i = 1; i <= colorCount; i++)
            {
                if (NGColor[i] == neoColor)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Add(byte red, byte green, byte blue)
        {
            ushort ngc = ColorTool.NeoColor(red, green, blue);

            if (Contain(ngc)) return true;
            if (colorCount == 15) return false;
            NGColor[++colorCount] = ngc;
            return true;
        }

        public bool Add(ushort ngc)
        {
            if (Contain(ngc)) return true;
            if (colorCount == 15) return false;
            NGColor[++colorCount] = ngc;
            return true;
        }

        public bool FromTile(Tile t)
        {
            BitmapData bmpdata = null;
            int x, r, g, b;

            if (t.autoAnim > 1)
            {
                bmpdata = t.autoAnimStrip.LockBits(new Rectangle(0, 0, 16 * t.autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            }
            else
            {
                bmpdata = t.getBitmap().LockBits(new Rectangle(0, 0, 16 * t.autoAnim, 16), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            }
            unsafe
            {
                byte* ptr;
                ptr = (byte*)bmpdata.Scan0;

                for (x = 0; x < (256 * t.autoAnim); x++) //256 pixels * autoanim count
                {
                    b = *ptr++;
                    g = *ptr++;
                    r = *ptr++;
                    if (*ptr++ != 0) //alpha not 0, visible pixel
                    {
                        if (!Add((byte)r, (byte)g, (byte)b)) //more than 15 colors
                        {
                            if (t.autoAnim > 1)
                            {
                                t.autoAnimStrip.UnlockBits(bmpdata);
                            }
                            else
                            {
                                t.getBitmap().UnlockBits(bmpdata);
                            }
                            return false;
                        }
                    }
                    //else Console.WriteLine("alpha on" + r.ToString() + "/" + g.ToString() + "/" + b.ToString());
                }
            }
            if (t.autoAnim > 1)
            {
                t.autoAnimStrip.UnlockBits(bmpdata);
            }
            else
            {
                t.getBitmap().UnlockBits(bmpdata);
            }
            return true;
        }

        public bool FromFixTile(fixTile t)
        {
            BitmapData bmpdata = null;
            int x, r, g, b;

            bmpdata = t.bmp.LockBits(new Rectangle(0, 0, 8,8), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr;
                ptr = (byte*)bmpdata.Scan0;

                for (x = 0; x < 64; x++) //64px
                {
                    b = *ptr++;
                    g = *ptr++;
                    r = *ptr++;
                    if (*ptr++ != 0) //alpha not 0, visible pixel
                    {
                        if (!Add((byte)r, (byte)g, (byte)b)) //more than 15 colors
                        {
                            t.bmp.UnlockBits(bmpdata);
                            return false;
                        }
                    }
                    //else Console.WriteLine("alpha on" + r.ToString() + "/" + g.ToString() + "/" + b.ToString());
                }
            }
            t.bmp.UnlockBits(bmpdata);
            return true;
        }

        public bool IsIncludedIn(Palette p)
        {
            for (int x = 1; x <= colorCount; x++)
            {
                if (!p.Contain(NGColor[x]))
                {
                    return false;
                }
            }
            return true;
        }
        
        //public ushort GetNeoColor(int x)
        //{
        //    return NGColor[x];
        //}

        public byte GetColorIndex(byte red, byte green, byte blue)
        {
            ushort ngc = ColorTool.NeoColor(red, green, blue);

            for (int i = 1; i <= colorCount; i++)
            {
                if (NGColor[i] == ngc)
                {
                    return (byte)i;
                }
            }
            return 0;
        }

        public int mergeResult(Palette pal)
        {
            int nbcolors = 0;

            nbcolors = colorCount + pal.colorCount;
            for (int i = 1; i <= colorCount; i++)
            {
                if (pal.Contain(NGColor[i]))
                {
                    nbcolors--;
                }
            }
            return nbcolors;
        }

        //public ushort getNGColor(int index)
        //{
        //    return NGColor[index];
        //}

        public void Merge(Palette pal)
        {
            for (int i = 1; i <= pal.colorCount; i++)
            {
                Add(pal.NGColor[i]);
            }
        }
    }
}

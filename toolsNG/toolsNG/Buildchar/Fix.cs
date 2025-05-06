using NeoTools;
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildchar
{
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

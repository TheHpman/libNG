using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace NeoTools
{
    public class tileBank
    {
        public int baseTile;
        public List<NGTile> tiles;
        public List<int> dummies;
        public tileBank(int _base)
        {
            baseTile = _base;
            dummies = new List<int>(0);
            tiles = new List<NGTile>(0);
        }

        //adds new tile if no occurences found
        //returns double short vram data
        public uint addTile(NGTile tile)
        {
            //check for 65536 multiples
            if (((baseTile + tiles.Count) % 65536) == 0)
            {
                NGTile t = new NGTile();
                t.indexData = new byte[256];
                for (int i = 0; i < 256; i++)
                    t.indexData[i] = 0;
                t.isReserved = true;
                tiles.Add(t);
            }

            int tNum, tFlips;
            /*
            tNum = tile.findOccurence(tiles, ref tFlips);
            if (tNum == -1)
            {//new tile
                tiles.Add(tile);
                tNum = tiles.Count - 1;
            }
            else
            {//found in bank

            }
            */

            if (!tile.findOccurence(tiles, out tNum, out tFlips))
                tiles.Add(tile);

            tNum += baseTile;
            return (uint)((tNum << 16) + ((tNum & 0xf0000) >> 12) + tFlips);
        }
    }

    public class NGTile
    {
        public byte[] indexData = null;
        public byte[] rasterized = null;

        public byte[] indexDataFlipX = null;
        public byte[] indexDataFlipY = null;
        public byte[] indexDataFlipXY = null;

        public bool isDummy = false;
        public bool isReserved = false; //reserved blanks (65536 multiples)

        public NGTile() { }

        public NGTile(ref Bitmap srcBmp, int posX, int posY, ref List<Color> pal)
        {
            build(ref srcBmp, posX, posY, ref pal);
        }

        public bool build(ref Bitmap srcBmp, int posX, int posY, ref List<Color> pal)
        {
            indexData = new byte[256];
            indexDataFlipX = new byte[256];
            indexDataFlipY = new byte[256];
            indexDataFlipXY = new byte[256];

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    /*byte index = 0;
                    if (((posX + x) >= 0) && ((posY + y) >= 0) && ((posX + x) < srcBmp.Width) && ((posY + y) < srcBmp.Height))
                        index = (byte)pal.IndexOf(srcBmp.GetPixel(posX + x, posY + y));
                    */
                    byte index;
                    try
                    {
                        index = (byte)pal.IndexOf(srcBmp.GetPixel(posX + x, posY + y));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        index = 0;
                    }

                    if (index == 0xff)
                    {
                        return false;
                        Console.WriteLine("Color not found");
                        Environment.Exit(0);
                    }
                    indexData[y * 16 + x] = index;
                    indexDataFlipX[y * 16 + (15 - x)] = index;
                    indexDataFlipY[(15 - y) * 16 + x] = index;
                    indexDataFlipXY[(15 - y) * 16 + (15 - x)] = index;
                }
            return true;
        }

        public void raster()
        {
            byte b1, b2, b3, b4;
            int dataIdx = 0;

            rasterized = new byte[128];
            for (int p = 0; p < 256; dataIdx += 4, p += 16)
            {
                b4 = (byte)((indexData[p + 15] << 4) & 0x80);
                b2 = (byte)((indexData[p + 15] << 5) & 0x80);
                b3 = (byte)((indexData[p + 15] << 6) & 0x80);
                b1 = (byte)((indexData[p + 15] << 7) & 0x80);
                b4 |= (byte)((indexData[p + 14] << 3) & 0x40);
                b2 |= (byte)((indexData[p + 14] << 4) & 0x40);
                b3 |= (byte)((indexData[p + 14] << 5) & 0x40);
                b1 |= (byte)((indexData[p + 14] << 6) & 0x40);
                b4 |= (byte)((indexData[p + 13] << 2) & 0x20);
                b2 |= (byte)((indexData[p + 13] << 3) & 0x20);
                b3 |= (byte)((indexData[p + 13] << 4) & 0x20);
                b1 |= (byte)((indexData[p + 13] << 5) & 0x20);
                b4 |= (byte)((indexData[p + 12] << 1) & 0x10);
                b2 |= (byte)((indexData[p + 12] << 2) & 0x10);
                b3 |= (byte)((indexData[p + 12] << 3) & 0x10);
                b1 |= (byte)((indexData[p + 12] << 4) & 0x10);
                b4 |= (byte)((indexData[p + 11] << 0) & 0x08);
                b2 |= (byte)((indexData[p + 11] << 1) & 0x08);
                b3 |= (byte)((indexData[p + 11] << 2) & 0x08);
                b1 |= (byte)((indexData[p + 11] << 3) & 0x08);
                b4 |= (byte)((indexData[p + 10] >> 1) & 0x04);
                b2 |= (byte)((indexData[p + 10] << 0) & 0x04);
                b3 |= (byte)((indexData[p + 10] << 1) & 0x04);
                b1 |= (byte)((indexData[p + 10] << 2) & 0x04);
                b4 |= (byte)((indexData[p + 9] >> 2) & 0x02);
                b2 |= (byte)((indexData[p + 9] >> 1) & 0x02);
                b3 |= (byte)((indexData[p + 9] << 0) & 0x02);
                b1 |= (byte)((indexData[p + 9] << 1) & 0x02);
                b4 |= (byte)((indexData[p + 8] >> 3) & 0x01);
                b2 |= (byte)((indexData[p + 8] >> 2) & 0x01);
                b3 |= (byte)((indexData[p + 8] >> 1) & 0x01);
                b1 |= (byte)((indexData[p + 8] << 0) & 0x01);
                rasterized[dataIdx] = b1;
                rasterized[dataIdx + 1] = b2;
                rasterized[dataIdx + 2] = b3;
                rasterized[dataIdx + 3] = b4;

                b4 = (byte)((indexData[p + 7] << 4) & 0x80);
                b2 = (byte)((indexData[p + 7] << 5) & 0x80);
                b3 = (byte)((indexData[p + 7] << 6) & 0x80);
                b1 = (byte)((indexData[p + 7] << 7) & 0x80);
                b4 |= (byte)((indexData[p + 6] << 3) & 0x40);
                b2 |= (byte)((indexData[p + 6] << 4) & 0x40);
                b3 |= (byte)((indexData[p + 6] << 5) & 0x40);
                b1 |= (byte)((indexData[p + 6] << 6) & 0x40);
                b4 |= (byte)((indexData[p + 5] << 2) & 0x20);
                b2 |= (byte)((indexData[p + 5] << 3) & 0x20);
                b3 |= (byte)((indexData[p + 5] << 4) & 0x20);
                b1 |= (byte)((indexData[p + 5] << 5) & 0x20);
                b4 |= (byte)((indexData[p + 4] << 1) & 0x10);
                b2 |= (byte)((indexData[p + 4] << 2) & 0x10);
                b3 |= (byte)((indexData[p + 4] << 3) & 0x10);
                b1 |= (byte)((indexData[p + 4] << 4) & 0x10);
                b4 |= (byte)((indexData[p + 3] << 0) & 0x08);
                b2 |= (byte)((indexData[p + 3] << 1) & 0x08);
                b3 |= (byte)((indexData[p + 3] << 2) & 0x08);
                b1 |= (byte)((indexData[p + 3] << 3) & 0x08);
                b4 |= (byte)((indexData[p + 2] >> 1) & 0x04);
                b2 |= (byte)((indexData[p + 2] << 0) & 0x04);
                b3 |= (byte)((indexData[p + 2] << 1) & 0x04);
                b1 |= (byte)((indexData[p + 2] << 2) & 0x04);
                b4 |= (byte)((indexData[p + 1] >> 2) & 0x02);
                b2 |= (byte)((indexData[p + 1] >> 1) & 0x02);
                b3 |= (byte)((indexData[p + 1] << 0) & 0x02);
                b1 |= (byte)((indexData[p + 1] << 1) & 0x02);
                b4 |= (byte)((indexData[p + 0] >> 3) & 0x01);
                b2 |= (byte)((indexData[p + 0] >> 2) & 0x01);
                b3 |= (byte)((indexData[p + 0] >> 1) & 0x01);
                b1 |= (byte)((indexData[p + 0] << 0) & 0x01);
                rasterized[dataIdx + 64] = b1;
                rasterized[dataIdx + 65] = b2;
                rasterized[dataIdx + 66] = b3;
                rasterized[dataIdx + 67] = b4;
            }
        }

        public int findOccurence(List<NGTile> tiles, ref int flips)
        {
            int num = -1;
            int _flips = 0;

            Parallel.ForEach(tiles, (tile, state) =>
            {
                bool found = false;
                if (!tile.isReserved)
                {
                    if (indexData.SequenceEqual(tile.indexData))
                    {
                        num = tiles.IndexOf(tile);
                        _flips = 0;
                        found = true;
                    }
                    else if (indexData.SequenceEqual(tile.indexDataFlipX))
                    {
                        num = tiles.IndexOf(tile);
                        _flips = 1;
                        found = true;
                    }
                    else if (indexData.SequenceEqual(tile.indexDataFlipY))
                    {
                        num = tiles.IndexOf(tile);
                        _flips = 2;
                        found = true;
                    }
                    else if (indexData.SequenceEqual(tile.indexDataFlipXY))
                    {
                        num = tiles.IndexOf(tile);
                        _flips = 3;
                        found = true;
                    }
                }
                if (found)
                    state.Break();
            });

            flips = _flips;
            return num;
        }

        public bool findOccurence(List<NGTile> tiles, out int num, out int flips)
        {
            int tileNum = -1;
            int _flips = 0;

            Parallel.ForEach(tiles, (tile, state) =>
            {
                bool found = false;
                if (!tile.isReserved)
                {
                    if (indexData.SequenceEqual(tile.indexData))
                    {
                        tileNum = tiles.IndexOf(tile);
                        _flips = 0;
                        found = true;
                    }
                    else if (indexData.SequenceEqual(tile.indexDataFlipX))
                    {
                        tileNum = tiles.IndexOf(tile);
                        _flips = 1;
                        found = true;
                    }
                    else if (indexData.SequenceEqual(tile.indexDataFlipY))
                    {
                        tileNum = tiles.IndexOf(tile);
                        _flips = 2;
                        found = true;
                    }
                    else if (indexData.SequenceEqual(tile.indexDataFlipXY))
                    {
                        tileNum = tiles.IndexOf(tile);
                        _flips = 3;
                        found = true;
                    }
                }
                if (found)
                    state.Break();
            });

            flips = _flips;
            if (tileNum == -1)
            {   //not found
                num = tiles.Count;
                return false;
            }
            else
            {   //found
                num = tileNum;
                return true;
            }
            //num = tileNum;
            //return tileNum == -1 ? false : true;
        }
    }

}

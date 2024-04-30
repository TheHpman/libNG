using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Security.Cryptography;
using System.IO.Ports;
using System.Linq;

namespace NeoTools
{
    public class fixTileList
    {
        public List<fixTile> tiles = null;

        public fixTileList()
        {
            tiles = new List<fixTile>(0);
        }

        public fixTileList(int size)
        {
            int i;

            tiles = new List<fixTile>(size);

            for (i = 0; i < size; i++)
            {
                tiles.Add(new fixTile());
            }
        }

        public fixTileList(ref Bitmap bmp)
        {
            int i, x, y, b = 0;

            tiles = new List<fixTile>(0);
            i = 0;
            for (b = 0; b < bmp.Width / 128; b++)
            {
                for (y = 0; y < 16; y++)
                {
                    for (x = 0; x < 16; x++)
                    {
                        tiles.Add(new fixTile());
                        tiles[i++].tileGet(bmp, (x * 8) + (128 * b), y * 8);
                    }
                }
            }


        }

        /*public int GetCount()
        {
            //number of tiles, not reflecting auto anim;
            return tiles.Count;
        }*/
        /*
        public fixTile GetTile(int x)
        {
            return (tiles[x]);
        }*/
        /*
        public void setRealIndexes(int starting)
        {
            int i;
            int idx;

            idx = starting;
            for (i = 0; i < tiles.Count; i++)
            {
                tiles[i].realIndex = idx;
                idx += tiles[i].autoAnim;
            }
        }

        public int getRealIndex(int i)
        {
            return (tiles[i].realIndex);
        }
        */
    }
}

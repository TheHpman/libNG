using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
//using System.Drawing.Drawing2D;
using System.Drawing;
using System.Security.Cryptography;
using System.IO.Ports;
using System.Linq;
using System.IO;

namespace NeoTools
{
    public class TileList
    {
        private List<Tile> tiles = null;
        private List<int> dummies = null;

        public TileList()
        {
            tiles = new List<Tile>(0);
        }

        public void debugAutoAnimInfo()
        {
            int i;
            int single = 0;
            int auto4 = 0;
            int auto8 = 0;

            for (i = 0; i < tiles.Count; i++)
            {
                switch (tiles[i].autoAnim)
                {
                    case 1:
                        single++;
                        break;
                    case 4 :
                        auto4++;
                        break;
                    case 8:
                        auto8++;
                        break;
                    default:
                        break;
                }
            }

            Console.WriteLine("\n{0} tiles: {1} single / {2} auto4 / {3} auto8", tiles.Count, single, auto4, auto8);
        }

        public TileList(int size)
        {
            int i;

            tiles = new List<Tile>(size);

            for (i = 0; i < size; i++)
            {
                tiles.Add(new Tile());
            }
        }

        public TileList(ref Bitmap bmp)
        {
            int i, j, x = 0;

            j = (bmp.Width / 16) * (bmp.Height / 16);
            tiles = new List<Tile>(j);
            for (i = 0; i < j; i++)
            {
                tiles.Add(new Tile());
            }

            for (i = 0; i < bmp.Height; i += 16)
            {
                for (j = 0; j < bmp.Width; j += 16)
                {
                    tiles[x++].tileGet(ref bmp, j, i);
                }
            }
        }

        public TileList(ref Bitmap bmp, ref Bitmap bmp1, ref Bitmap bmp2, ref Bitmap bmp3)
        {
            int i, j, x = 0;

            j = (bmp.Width / 16) * (bmp.Height / 16);
            tiles = new List<Tile>(j);
            for (i = 0; i < j; i++)
            {
                tiles.Add(new Tile());
            }

            for (i = 0; i < bmp.Height; i += 16)
            {
                for (j = 0; j < bmp.Width; j += 16)
                {
                    tiles[x++].tileGet(bmp, bmp1, bmp2, bmp3, j, i);
                }
            }
        }


        public TileList(ref Bitmap bmp, ref Bitmap bmp1, ref Bitmap bmp2, ref Bitmap bmp3, ref Bitmap bmp4, ref Bitmap bmp5, ref Bitmap bmp6, ref Bitmap bmp7)
        {
            int i, j, x = 0;

            j = (bmp.Width / 16) * (bmp.Height / 16);
            tiles = new List<Tile>(j);
            for (i = 0; i < j; i++)
            {
                tiles.Add(new Tile());
            }

            for (i = 0; i < bmp.Height; i += 16)
            {
                for (j = 0; j < bmp.Width; j += 16)
                {
                    tiles[x++].tileGet(bmp, bmp1, bmp2, bmp3, bmp4, bmp5, bmp6, bmp7, j, i);
                }
            }
        }

        public int GetCount()
        {
            //number of tiles, not reflecting auto anim;
            return tiles.Count;
        }

        public int GetPhysicalCount()
        {
            //total number of physical tiles
            int i;
            int count = 0;

            for (i = 0; i < tiles.Count; i++)
            {
                count += tiles[i].autoAnim;
            }
            return (count);
        }

        public void insertDummy(int pos)
        {
            if(dummies==null) dummies=new List<int>(0);

            tiles.Insert(pos, new Tile(true));
            dummies.Add(pos);
        }

        public int getDummyIndex()
        {
            int x;

            if (dummies == null) return (-1);
            if (dummies.Count > 0)
            {
                //dummies available
                x = dummies[0];
                dummies.Remove(dummies[0]);
                return (x);
            }
            else
            {
                return (-1);
            }
        }

        public void moveToDummy(int from, int to)
        {
            Tile t = null;

            t = tiles[from];
            tiles.Remove(t);
            tiles.Remove(tiles[to]);
            tiles.Insert(to, t);

        }

        public int sameTiles_hash(int tileA, int tileB)
        {
            if (tiles[tileA].autoAnim != tiles[tileB].autoAnim)
            {
                //different autoanim, different tiles
                //TODO: optimize checking single tile vs each strip tile
                return (-1);
            }

            if (tiles[tileA].autoAnim > 1)
            {
                //dealing with autoanim strips
                if (tiles[tileA].stripHash.SequenceEqual(tiles[tileB].stripHash))
                    return (0);
                if (tiles[tileA].stripHash.SequenceEqual(tiles[tileB].stripHashFlipX))
                    return (1);
                if (tiles[tileA].stripHash.SequenceEqual(tiles[tileB].stripHashFlipY))
                    return (2);
                if (tiles[tileA].stripHash.SequenceEqual(tiles[tileB].stripHashFlipXY))
                    return (3);
                return (-1);
            }

            //standrard, single tiles
            if (tiles[tileA].hash.SequenceEqual(tiles[tileB].hash))
                return (0);
            if (tiles[tileA].hash.SequenceEqual(tiles[tileB].hashFlipX))
                return (1);
            if (tiles[tileA].hash.SequenceEqual(tiles[tileB].hashFlipY))
                return (2);
            if (tiles[tileA].hash.SequenceEqual(tiles[tileB].hashFlipXY))
                return (3);
            return (-1);
        }

        public void removeTile(int x)
        {
            tiles.Remove(tiles[x]);
        }

        public void CleanUp(TileMap tm)
        {
            int i, j, flipType;

            for (i = 0; i < tiles.Count; i++)
            {
                for (j = i + 1; j < tiles.Count; j++)
                {
                    flipType = sameTiles_hash(i, j);
                    if (flipType >= 0) //égal
                    {
                        tm.replaceTileData(j, i, flipType);
                        tm.decreaseTile(j);
                        tiles.Remove(tiles[j]);
                        j--;
                    }
                }
            }
        }

        public Tile GetTile(int x)
        {
            return (tiles[x]);
        }

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

        public void debugDump()
        {
            int i;

            for (i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].autoAnim == 1) tiles[i].getBitmap().Save(string.Format("dbgdat" + Path.DirectorySeparatorChar + "{0:d4}.png", i));
                else tiles[i].autoAnimStrip.Save(string.Format("dbgdat" + Path.DirectorySeparatorChar + "{0:d4}.png", i));
            }
        }

    }
}

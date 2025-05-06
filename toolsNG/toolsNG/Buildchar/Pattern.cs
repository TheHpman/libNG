using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildchar
{
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
}

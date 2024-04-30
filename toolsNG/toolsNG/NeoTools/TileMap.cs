using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

//data stored horizontally:
// 0 1 2 3 4
// 5 6 7 8 9

namespace NeoTools
{
    public class TileMap
    {
        private int sizeX;
        private int sizeY;
        private List<TileData> data = null;

        public TileMap()
        {
            sizeX = 0;
            sizeY = 0;
            data = new List<TileData>(0);
        }

        public TileMap(int tilesX, int tilesY)
        {
            sizeX = tilesX;
            sizeY = tilesY;
            data = new List<TileData>(tilesX * tilesY);
            for (int i = 0; i < (sizeX * sizeY); i++)
            {
                data.Add(new TileData(i, i));
            }
        }
        /*
        public TileMap(Bitmap bmp)
        {
            sizeX = bmp.Width / 16;
            sizeY = bmp.Height / 16;

            data = new List<TileData>(sizeX * sizeY);
            for (int i = 0; i < (sizeX * sizeY); i++)
            {
                data.Add(new TileData(i, i));
            }
        }
        */
        public TileMap(Bitmap bmp, TileList list)
        {
            //possible autoanim
            sizeX = bmp.Width / 16;
            sizeY = bmp.Height / 16;

            data = new List<TileData>(sizeX * sizeY);
            for (int i = 0; i < (sizeX * sizeY); i++)
            {
                data.Add(new TileData(i, i, list.GetTile(i).autoAnim));
            }
        }

        public int getSizeX() { return (sizeX); }
        public int getSizeY() { return (sizeY); }

        public void replaceTileData(int oldTile, int newTile, int flip = 0, int autoAnim = 1)
        {
            //used before palettes optimisation, so tile=palette
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].tileNumber == oldTile)
                {
                    data[i].tileNumber = (uint)newTile;
                    data[i].paletteNumber = (uint)newTile;
                    if ((flip == 1) || (flip == 3))
                        //if (!data[oldTile].FlipX)
                        data[i].FlipX = true;
                    if ((flip == 2) || (flip == 3))
                        //if (!data[oldTile].FlipY)
                        data[i].FlipY = true;
                    data[i].autoAnim = autoAnim;
                }
            }
        }

        public void replacePalette(int oldPalette, int newPalette)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].paletteNumber == oldPalette)
                {
                    data[i].paletteNumber = (uint)newPalette;
                }
            }
        }

        public void decreasePalette(int above)
        {
            for (int i = 0; i < data.Count; i++)
            {
                //check
                //if (data[i].paletteNumber > above)
                if (data[i].paletteNumber >= above)
                {
                    data[i].paletteNumber--;
                }
            }
        }

        public void decreaseTile(int above)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].tileNumber > above)
                {
                    data[i].tileNumber--;
                }
            }
        }

        public void changeTile(int from, int to)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].tileNumber == from)
                {
                    data[i].tileNumber = (uint)to;
                }
            }
        }

        public void shiftTiles(int idx, int shift)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].tileNumber >= (idx/* + shift*/))
                {
                    //data[i].tileNumber += shift;      //MEH
                    data[i].tileNumber = (uint)(data[i].tileNumber + shift);
                }
            }
        }

        public int getMaxPalette()
        {
            uint x = 0;

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].paletteNumber > x)
                {
                    x = data[i].paletteNumber;
                }
            }
            return (int)x;
        }

        //remove duplicate tiles
        public void cleanUp(TileList tl, PaletteList pl)
        {
            int i, j, flipType;

            for (i = 0; i < tl.GetCount(); i++)
            {
                for (j = i + 1; j < tl.GetCount(); j++)
                {
                    flipType = tl.sameTiles_hash(i, j);
                    if (flipType >= 0) //égal
                    {
                        replaceTileData(j, i, flipType, tl.GetTile(i).autoAnim);
                        decreaseTile(j);
                        decreasePalette(j);
                        tl.removeTile(j);
                        pl.removePalette(j);
                        j--;
                    }
                }
            }
        }

        public void alignAutoAnim(TileList tl, int tileShift)
        {
            int x = 0;
            int idx, shift, dmy;
            int displ = tileShift;

            //tl.debugDump();
            do
            {
                idx = x;
                shift = 0;
                switch (tl.GetTile(x).autoAnim)
                {
                    case 4:
                        while ((x + displ) % 4 != 0)
                        {
                            tl.insertDummy(x++);
                            shift++;
                        }
                        shiftTiles(idx, shift);
                        //shiftTiles(idx + shift + 1, 3);
                        x++;
                        displ += 3;
                        break;
                    case 8:
                        while ((x + displ) % 8 != 0)
                        {
                            tl.insertDummy(x++);
                            shift++;
                        }
                        shiftTiles(idx, shift);
                        //shiftTiles(idx + shift + 1, 7);
                        x++;
                        displ += 7;
                        break;
                    default:
                        dmy = tl.getDummyIndex();
                        if (dmy != -1)
                        {
                            //dummy is available
                            tl.moveToDummy(x, dmy);
                            changeTile(x, dmy);
                            decreaseTile(x);
                        }
                        else
                        {
                            x++;
                            //displ++;
                        }
                        break;
                }
            } while (x < tl.GetCount());
        }

        public string doColorStream_Horizontal(TileList tl, PaletteList pl, ref int slotCount, string label = "debug")
        {
            int[] usage = new int[pl.GetCount()];
            int[] allocation = null;
            int[] allocationAge = null;
            int[] changeList = null;
            string fwData = "";
            string fwData2 = "";
            string bwData = "";
            string bwData2 = "";
            string startData = "";
            string endData = "";
            string descriptor = "";
            int useCount;
            int maxUsage = 0;
            int i, x, y, z;
            int overWrites = 0;

            if (sizeX <= 21) return "";    //nothing to stream

            useCount = 0;
            for (i = 0; i < usage.Length; i++)
                usage[i] = 0;

            //determine max usage
            for (i = 0; i < (sizeX - 21); i++)
            {
                for (z = 0; z < usage.Length; z++) usage[z] = 0;    //reset usage

                for (x = i; x < i + 21; x++)           //for screen width
                    for (y = 0; y < sizeY; y++)         //for map height
                        usage[data[(y * sizeX) + x].paletteNumber]++;

                useCount = 0;
                for (z = 0; z < usage.Length; z++) if (usage[z] > 0) useCount++;
                if (useCount > maxUsage) maxUsage = useCount;
            }

            //maxUsage has value
            usage = new int[pl.GetCount()];
            for (z = 0; z < usage.Length; z++) usage[z] = 0;    //all slots unused

            allocation = new int[maxUsage];     //hold palette #
            allocationAge = new int[maxUsage];
            for (z = 0; z < allocationAge.Length; z++)
            {
                allocation[z] = -1;
                allocationAge[z] = 100000;
            }
            changeList = new int[(maxUsage + 2) * 2];
            changeList[0] = -1;

            //startup palettes used
            //0-21
            for (x = 0; x < 21; x++)
                for (y = 0; y < sizeY; y++)
                    addPalette(data[(y * sizeX) + x].paletteNumber, ref allocation, ref allocationAge, ref usage, ref changeList);

            //update tilemap palettes
            remapPalettes(ref allocation, 0, 20);

            //do start config
            startData = label + "_colorStream_startConfig:\n" + colorStreamConfig(label, ref allocation);

            //foreach step forward, check changes
            for (z = 0; z < (sizeX - 21); z++)  //22
            {
                //remove column z, add z+21

                changeList[0] = -1; //clear changes

                for (y = 0; y < sizeY; y++) //exiting palettes
                    usage[data[(y * sizeX) + z].paletteNumber]--;

                //update ages
                updateAge(ref allocation, ref allocationAge, ref usage);

                //add z+21 palettes
                //Console.WriteLine(String.Format("{0}:", (z + 1) * 16));
                for (y = 0; y < sizeY; y++)
                {
                    //entering palettes
                    overWrites += addPalette(data[(y * sizeX) + (z + 21)].paletteNumber, ref allocation, ref allocationAge, ref usage, ref changeList);
                }
                //remap entering column
                remapPalettes(ref allocation, z + 21, z + 21);

                if (overWrites == 0)
                {
                    //redo start config
                    startData = label + "_colorStream_startConfig:\n" + colorStreamConfig(label, ref allocation);
                }
                else
                {
                    if (changeList[0] != -1) //changes occured
                    {
                        fwData += string.Format("\t.word\t0x{0:x4}\n\t.long\t{1}\n", (z + 1) * 16, string.Format("{0}_cStream_f{1}", label, (z + 1) * 16));

                        fwData2 += string.Format("{0}_cStream_f{1}:\n", label, (z + 1) * 16);
                        y = 0;
                        while (changeList[y] != -1)
                        {
                            fwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}\n\t.long\t{2}_Palettes+{3}\t;* pal #{4}\n", changeList[y + 1] << 5, changeList[y + 1], label, 2 + (changeList[y] << 5), changeList[y]);
                            y += 2;
                        }
                        fwData2 += "\t.word\t0xffff\n";
                    }
                }
            }
            fwData += string.Format("{0}_colorStream_fwDataEnd:\n\t.word\t0xffff\n\t.long\t0x00000000\n\n", label);

            //end config
            endData = label + "_colorStream_endConfig:\n" + colorStreamConfig(label, ref allocation);

            //backward scan
            for (z = (sizeX - 21); z >= 0; z--) //22
            {
                changeList[0] = -1;
                for (y = 0; y < sizeY; y++)
                {
                    //check availability
                    addPaletteReverse(data[(y * sizeX) + z], ref allocation, ref changeList);
                }
                //(z+1)*16
                if (changeList[0] != -1)
                {
                    //changes occured
                    bwData += string.Format("\t.word\t0x{0:x4}\n\t.long\t{1}\n", (z + 1) * 16, string.Format("{0}_cStream_b{1}", label, (z + 1) * 16));

                    bwData2 += string.Format("{0}_cStream_b{1}:\n", label, (z + 1) * 16);
                    y = 0;
                    while (changeList[y] != -1)
                    {
                        bwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}\n\t.long\t{2}_Palettes+{3}\t;* pal #{4}\n", changeList[y + 1] << 5, changeList[y + 1], label, 2 + (changeList[y] << 5), changeList[y]);
                        y += 2;
                    }
                    bwData2 += "\t.word\t0xffff\n";
                }
            }
            bwData += string.Format("{0}_colorStream_bwDataEnd:\n\t.word\t0x0000\n\t.long\t0x00000000\n\n", label);

            descriptor = string.Format(".globl {0}_colorStream\n{0}_colorStream:\n\t.word\t0x{1:x4} ;* {1} pal slots\n\t.long\t{0}_colorStream_startConfig\n\t.long\t{0}_colorStream_endConfig\n\t.long\t{0}_colorStream_fwData\n\t.long\t{0}_colorStream_fwDataEnd\n\t.long\t{0}_colorStream_bwData\n\t.long\t{0}_colorStream_bwDataEnd\n\n", label, maxUsage);

            startData += "\n";
            endData += "\n";
            bwData2 += "\n";
            fwData = string.Format("{0}_colorStream_fwData:\n", label) + fwData;
            bwData = string.Format("\t.long\t0xffffffff\n{0}_colorStream_bwData:\n", label) + bwData;

            slotCount = maxUsage;
            return descriptor + startData + endData + bwData + fwData + bwData2 + fwData2;      // /!\ important order /!\
        }

        private string colorStreamConfig(string label, ref int[] allocation)
        {
            int x;
            string data = "";

            for (x = 0; x < allocation.Length; x++)
            {
                if (allocation[x] != -1)
                {
                    //data += string.Format("\t.long\t0x0000{0:x4}, {1}_Palettes+{2}\n", x << 5, label, 2 + (allocation[x] << 5));
                    data += string.Format("\t.word\t0x{0:x4}\n\t.long\t{1}_Palettes+{2}\n", x << 5, label, 2 + (allocation[x] << 5));
                }
            }

            //return data + "\t.long\t0xffffffff\n";
            return data + "\t.word\t0xffff\n";
        }

        public string doColorStream_Vertical(TileList tl, PaletteList pl, ref int slotCount, string label = "debug")
        {
            int[] usage = new int[pl.GetCount()];
            int[] allocation = null;
            int[] allocationAge = null;
            int[] changeList = null;
            string fwData = "";
            string fwData2 = "";
            string bwData = "";
            string bwData2 = "";
            string startData = "";
            string endData = "";
            string descriptor = "";
            int useCount;
            int maxUsage = 0;
            int i, x, y, z;
            int overWrites = 0;
            int scanSize = 15; //15 for 224 resolution

            if (sizeY <= scanSize) return "";    //nothing to stream

            useCount = 0;
            for (i = 0; i < usage.Length; i++)
                usage[i] = 0;

            //determine max usage
            for (i = 0; i < (sizeY - scanSize); i++)
            {
                for (z = 0; z < usage.Length; z++) usage[z] = 0;    //reset usage

                for (x = 0; x < sizeX; x++)         //for map width
                    for (y = i; y < i + scanSize; y++)    //for screen height
                        usage[data[(y * sizeX) + x].paletteNumber]++;

                useCount = 0;
                for (z = 0; z < usage.Length; z++) if (usage[z] > 0) useCount++;
                if (useCount > maxUsage) maxUsage = useCount;
            }

            //maxUsage has value
            usage = new int[pl.GetCount()];
            for (z = 0; z < usage.Length; z++) usage[z] = 0;    //all slots unused

            allocation = new int[maxUsage];     //hold palette #
            allocationAge = new int[maxUsage];
            for (z = 0; z < allocationAge.Length; z++)
            {
                allocation[z] = -1;
                allocationAge[z] = 100000;
            }
            changeList = new int[(maxUsage + 2) * 2];
            changeList[0] = -1;

            //startup palettes used
            for (x = 0; x < sizeX; x++)
                for (y = 0; y < scanSize; y++)
                    addPalette(data[(y * sizeX) + x].paletteNumber, ref allocation, ref allocationAge, ref usage, ref changeList);

            //update tilemap palettes
            remapPalettesY(ref allocation, 0, 15); //20

            //do start config
            startData = label + "_colorStream_startConfig:\n" + colorStreamConfig(label, ref allocation);

            //foreach step forward, check changes
            for (z = 0; z < (sizeY - scanSize); z++)
            {
                //remove rom z, add z+scanSize

                changeList[0] = -1; //clear changes

                for (y = 0; y < sizeX; y++) //exiting palettes
                    usage[data[(z * sizeX) + y].paletteNumber]--;

                //update ages
                updateAge(ref allocation, ref allocationAge, ref usage);

                //add entering row palettes (z+scanSize)
                for (y = 0; y < sizeX; y++)
                {
                    //entering palettes
                    overWrites += addPalette(data[((z + scanSize) * sizeX) + y].paletteNumber, ref allocation, ref allocationAge, ref usage, ref changeList);
                }
                //remap entering row
                remapPalettesY(ref allocation, z + scanSize, z + scanSize);

                if (overWrites == 0)
                {
                    //redo start config
                    startData = label + "_colorStream_startConfig:\n" + colorStreamConfig(label, ref allocation);
                }
                else
                {
                    if (changeList[0] != -1) //changes occured
                    {
                        fwData += string.Format("\t.word\t0x{0:x4}\n\t.long\t{1}\n", (z + 1) * 16, string.Format("{0}_cStream_f{1}", label, (z + 1) * 16));

                        fwData2 += string.Format("{0}_cStream_f{1}:\n", label, (z + 1) * 16);
                        y = 0;
                        while (changeList[y] != -1)
                        {
                            fwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}\n\t.long\t{2}_Palettes+{3}\t;* pal #{4}\n", changeList[y + 1] << 5, changeList[y + 1], label, 2 + (changeList[y] << 5), changeList[y]);
                            y += 2;
                        }
                        fwData2 += "\t.word\t0xffff\n";
                    }
                }
            }
            fwData += string.Format("{0}_colorStream_fwDataEnd:\n\t.word\t0xffff\n\t.long\t0x00000000\n\n", label);

            //end config
            endData = label + "_colorStream_endConfig:\n" + colorStreamConfig(label, ref allocation);
            //******

            //string dbg = "";
            //for (y = 0; y < sizeY; y++)
            //{
            //    dbg += string.Format("{0:d5}:\t", y * 16);
            //    for (x = 0; x < sizeX; x++)
            //    {
            //        dbg += string.Format("{0:d3} ", data[(y * sizeX) + x].streamPaletteNumber);
            //    }
            //    dbg += Environment.NewLine;
            //}
            //File.WriteAllText("dbgPals.txt", dbg);

            //backward scan
            for (z = (sizeY - scanSize) - 1; z >= 0; z--)
            {
                changeList[0] = -1;

                //add z
                for (y = 0; y < sizeX; y++)     //check availability
                    addPaletteReverse(data[(z * sizeX) + y], ref allocation, ref changeList);
                //addPalette(data[(z * sizeX) + y].paletteNumber, ref allocation, ref allocationAge, ref usage, ref changeList);

                //(z+1)*16
                if (changeList[0] != -1)
                {
                    //changes occured
                    bwData += string.Format("\t.word\t0x{0:x4}\n\t.long\t{1}\n", (z + 1) * 16, string.Format("{0}_cStream_b{1}", label, (z + 1) * 16));

                    bwData2 += string.Format("{0}_cStream_b{1}:\n", label, (z + 1) * 16);
                    y = 0;
                    while (changeList[y] != -1)
                    {
                        bwData2 += string.Format("\t.word\t0x{0:x4}\t;* slot #{1}\n\t.long\t{2}_Palettes+{3}\t;* pal #{4}\n", changeList[y + 1] << 5, changeList[y + 1], label, 2 + (changeList[y] << 5), changeList[y]);
                        y += 2;
                    }
                    bwData2 += "\t.word\t0xffff\n";
                }
            }
            bwData += string.Format("{0}_colorStream_bwDataEnd:\n\t.word\t0x0000\n\t.long\t0x00000000\n\n", label);

            descriptor = string.Format(".globl {0}_colorStream\n{0}_colorStream:\n\t.word\t0x{1:x4} ;* {1} pal slots\n\t.long\t{0}_colorStream_startConfig\n\t.long\t{0}_colorStream_endConfig\n\t.long\t{0}_colorStream_fwData\n\t.long\t{0}_colorStream_fwDataEnd\n\t.long\t{0}_colorStream_bwData\n\t.long\t{0}_colorStream_bwDataEnd\n\n", label, maxUsage);

            startData += "\n";
            endData += "\n";
            bwData2 += "\n";
            fwData = string.Format("{0}_colorStream_fwData:\n", label) + fwData;
            bwData = string.Format("\t.long\t0xffffffff\n{0}_colorStream_bwData:\n", label) + bwData;

            slotCount = maxUsage;
            return descriptor + startData + endData + bwData + fwData + bwData2 + fwData2 + "\n";      // /!\ important order /!\
        }

        //on the way back we look at where the streamPalette was mapped
        private void addPaletteReverse(TileData tile, ref int[] allocation, ref int[] changeList)
        {
            int y;

            //already allocated?
            if (allocation[tile.streamPaletteNumber] == (int)tile.paletteNumber)
                return;

            //nope, check required position from tile
            allocation[tile.streamPaletteNumber] = (int)tile.paletteNumber;
            y = -1;
            while (changeList[++y] != -1) ;
            changeList[y++] = (int)tile.paletteNumber;
            changeList[y++] = (int)tile.streamPaletteNumber;
            changeList[y] = -1;
        }

        private int addPalette(uint palnum, ref int[] allocation, ref int[] allocationAge, ref int[] usage, ref int[] changeList)
        {
            int x, y;
            int maxAge = 0;
            bool overWrite = false;

            //check if already allocated
            for (x = 0; x < allocation.Length; x++)
                if (allocation[x] == palnum)
                {
                    allocationAge[x] = 0;
                    usage[palnum]++;
                    return 0;
                }

            //need to allocate

            //find oldest slot
            for (x = 0; x < allocation.Length; x++)
                if (allocationAge[x] > maxAge)
                    maxAge = allocationAge[x];

            //allocate into oldest
            for (x = 0; x < allocation.Length; x++)
                if (allocationAge[x] == maxAge)
                {
                    allocation[x] = (int)palnum;
                    if (allocationAge[x] < 100000) overWrite = true;
                    allocationAge[x] = 0;
                    usage[palnum]++;

                    //Console.WriteLine(string.Format("pal#{0} @ {1}", palnum, x));

                    y = -1;
                    while (changeList[++y] != -1) ;
                    changeList[y++] = (int)palnum;
                    changeList[y++] = x;
                    changeList[y] = -1;
                    if (overWrite)
                        return 1;
                    else return 0;
                }

            return 0;
        }

        private void updateAge(ref int[] allocation, ref int[] allocationAge, ref int[] usage)
        {
            int x;

            for (x = 0; x < allocation.Length; x++)
            {
                if (allocation[x] != -1)
                {
                    //used slot
                    if (usage[allocation[x]] == 0)
                        allocationAge[x]++;
                }
            }
        }

        private void remapPalettes(ref int[] allocation, int startX, int endX)
        {
            int x, y, z, pal;

            for (z = startX; z <= endX; z++)
            {
                for (y = 0; y < sizeY; y++)
                {
                    pal = (int)data[(y * sizeX) + z].paletteNumber;
                    for (x = 0; x < allocation.Length; x++)
                    {
                        if (allocation[x] == pal)
                        {
                            data[(y * sizeX) + z].streamPaletteNumber = (uint)x;
                            break;
                        }
                    }
                }
            }
        }

        private void remapPalettesY(ref int[] allocation, int startY, int endY)
        {
            int x, y, z, pal;

            for (y = startY; y <= endY; y++)
            {
                for (x = 0; x < sizeX; x++)
                {
                    pal = (int)data[(y * sizeX) + x].paletteNumber;
                    for (z = 0; z < allocation.Length; z++)
                    {
                        if (allocation[z] == pal)
                        {
                            data[(y * sizeX) + x].streamPaletteNumber = (uint)z;
                            break;
                        }
                    }
                }
            }
        }


        /* old file ops*/
        public void PictureToFile(TileList tl, string fName, bool append = false, string objName = "NS", int tileShift = 0, bool flipX = false, bool flipY = false, bool flipXY = false)
        {
            int i, j, flip;
            int tileNumber;
            FileStream fs = null;
            byte autoAnimFlag;
            //string tmp;

            tl.setRealIndexes(tileShift);

            if (append) fs = File.Open(fName, FileMode.Append);
            else fs = File.Create(fName);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(".globl " + objName);
            sw.WriteLine(objName + ":");
            sw.WriteLine(string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} words per column / {1} tiles height", sizeY * 2, sizeY));
            sw.WriteLine(string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} cols of {1} tiles", sizeX, sizeY));
            sw.WriteLine(string.Format("\t.long\t{0}_Map, {0}_Map{1}, {0}_Map{2}, {0}_Map{3}", objName, flipX == true ? "_FlipX" : "", flipY == true ? "_FlipY" : "", flipXY == true ? "_FlipXY" : ""));

            //base map
            sw.WriteLine(objName + "_Map:");
            for (i = 0; i < sizeX; i++)
            {
                sw.Write("\t.word\t");
                for (j = 0; j < sizeY; j++)
                {
                    flip = 0;
                    if (data[(j * sizeX) + i].FlipX) flip += 0x1;
                    if (data[(j * sizeX) + i].FlipY) flip += 0x2;

                    autoAnimFlag = 0x00;
                    if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                    if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                    tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                    sw.Write(string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                        tileNumber & 0xffff,                    //tile lsb, 16b
                        data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                        (tileNumber >> 16) & 0xf,               //tile msb, 4b
                        autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                    ));
                }
                sw.WriteLine();
            }

            if (flipX)
            {
                sw.WriteLine(objName + "_Map_FlipX:");
                for (i = sizeX - 1; i >= 0; i--)
                {
                    sw.Write("\t.word\t");
                    for (j = 0; j < sizeY; j++)
                    {
                        flip = 0;
                        if (!(data[(j * sizeX) + i].FlipX)) flip += 0x1;
                        if (data[(j * sizeX) + i].FlipY) flip += 0x2;

                        autoAnimFlag = 0x00;
                        if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                        if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                        tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                        sw.Write(string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        ));
                    }
                    sw.WriteLine();
                }
            }

            if (flipY)
            {
                sw.WriteLine(objName + "_Map_FlipY:");
                for (i = 0; i < sizeX; i++)
                {
                    sw.Write("\t.word\t");
                    for (j = sizeY - 1; j >= 0; j--)
                    {
                        flip = 0;
                        if (data[(j * sizeX) + i].FlipX) flip += 0x1;
                        if (!(data[(j * sizeX) + i].FlipY)) flip += 0x2;

                        autoAnimFlag = 0x00;
                        if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                        if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                        tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                        sw.Write(string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != 0 ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        ));
                    }
                    sw.WriteLine();
                }
            }

            if (flipXY)
            {
                sw.WriteLine(objName + "_Map_FlipXY:");
                for (i = sizeX - 1; i >= 0; i--)
                {
                    sw.Write("\t.word\t");
                    for (j = sizeY - 1; j >= 0; j--)
                    {
                        flip = 0;
                        if (!(data[(j * sizeX) + i].FlipX)) flip += 0x1;
                        if (!(data[(j * sizeX) + i].FlipY)) flip += 0x2;

                        autoAnimFlag = 0x00;
                        if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                        if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                        tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                        sw.Write(string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != 0 ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        ));
                    }
                    sw.WriteLine();
                }
            }

            sw.WriteLine();
            sw.Close();
            fs.Close();
        }/**/
        //tmp
        public bool ScrollerToFile(TileList tl, string fName, bool append = false, string objName = "NS", int tileShift = 0/*, bool useColorStreamPalettes = false*/)
        {
            bool useColorStreamPalettes = false;
            int i, j, flip;
            int tileNumber;
            FileStream fs = null;
            byte autoAnimFlag;

            tl.setRealIndexes(tileShift);

            try
            {
                if (append) fs = File.Open(fName, FileMode.Append);
                else fs = File.Create(fName);
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine(".globl " + objName);
                sw.WriteLine(objName + ":");
                sw.WriteLine(string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;* {0} bytes per strip / {1} tiles spr size", sizeY * 4, sizeY > 32 ? 32 : sizeY));
                sw.WriteLine(string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;* {0} strips of {1} tiles", sizeX, sizeY));

                for (i = 0; i < sizeX; i++)
                {
                    if (i % 16 == 0) sw.Write("\n\t.long\t");
                    sw.Write(string.Format("{0}_strip{1:x4}{2}", objName, i, i % 16 != 15 ? (i == sizeX - 1 ? "" : ", ") : ""));
                }
                sw.Write("\n\n");

                for (i = 0; i < sizeX; i++)
                {
                    sw.Write(string.Format("{0}_strip{1:x4}:\n\t.word\t", objName, i));
                    for (j = 0; j < sizeY; j++)
                    {
                        flip = 0;
                        if (data[(j * sizeX) + i].FlipX) flip += 0x1;
                        if (data[(j * sizeX) + i].FlipY) flip += 0x2;
                        autoAnimFlag = 0x00;
                        if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                        if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;
                        tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);
                        if (!useColorStreamPalettes)
                            sw.Write(string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                                tileNumber & 0xffff,                    //tile lsb, 16b
                                data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                                (tileNumber >> 16) & 0xf,               //tile msb, 4b
                                autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                            ));
                        else
                            sw.Write(string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                                tileNumber & 0xffff,                    //tile lsb, 16b
                                data[(j * sizeX) + i].streamPaletteNumber,//palette, 8b
                                (tileNumber >> 16) & 0xf,               //tile msb, 4b
                                autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                            ));
                    }
                    sw.WriteLine();
                }
                //sw.WriteLine(objName + "_Map_end:");
                sw.WriteLine();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception /*ex*/)
            {
                return false;
            }
        }
        /**/

        public string PictureToAsmString(TileList tl, string objName = "NS", int tileShift = 0, bool flipX = false, bool flipY = false, bool flipXY = false)
        {
            int i, j, flip;
            int tileNumber;
            byte autoAnimFlag;
            string strData = "";

            tl.setRealIndexes(tileShift);

            strData += ".globl " + objName + "\n";
            strData += objName + ":\n";
            strData += string.Format("\t.word\t0x{0:x4}\t;*{0} bytes per strip\n", sizeY * 4, sizeY);
            strData += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} strips of {1} tiles\n", sizeX, sizeY);
            strData += string.Format("\t.long\t{0}_Palettes\n", objName);
            strData += string.Format("\t.long\t{0}_Map, {0}_Map{1}, {0}_Map{2}, {0}_Map{3}\n", objName, flipX == true ? "_FlipX" : "", flipY == true ? "_FlipY" : "", flipXY == true ? "_FlipXY" : "");

            //base map
            strData += objName + "_Map:\n";
            for (i = 0; i < sizeX; i++)
            {
                strData += "\t.word\t";
                for (j = 0; j < sizeY; j++)
                {
                    flip = 0;
                    if (data[(j * sizeX) + i].FlipX) flip += 0x1;
                    if (data[(j * sizeX) + i].FlipY) flip += 0x2;

                    autoAnimFlag = 0x00;
                    if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                    if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                    tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                    strData += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                        tileNumber & 0xffff,                    //tile lsb, 16b
                        data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                        (tileNumber >> 16) & 0xf,               //tile msb, 4b
                        autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                    );
                }
                strData += "\n";
            }

            if (flipX)
            {
                strData += objName + "_Map_FlipX:\n";
                for (i = sizeX - 1; i >= 0; i--)
                {
                    strData += "\t.word\t";
                    for (j = 0; j < sizeY; j++)
                    {
                        flip = 0;
                        if (!(data[(j * sizeX) + i].FlipX)) flip += 0x1;
                        if (data[(j * sizeX) + i].FlipY) flip += 0x2;

                        autoAnimFlag = 0x00;
                        if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                        if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                        tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                        strData += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        );
                    }
                    strData += "\n";
                }
            }

            if (flipY)
            {
                strData += objName + "_Map_FlipY:\n";
                for (i = 0; i < sizeX; i++)
                {
                    strData += "\t.word\t";
                    for (j = sizeY - 1; j >= 0; j--)
                    {
                        flip = 0;
                        if (data[(j * sizeX) + i].FlipX) flip += 0x1;
                        if (!(data[(j * sizeX) + i].FlipY)) flip += 0x2;

                        autoAnimFlag = 0x00;
                        if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                        if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                        tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                        strData += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != 0 ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        );
                    }
                    strData += "\n";
                }
            }

            if (flipXY)
            {
                strData += objName + "_Map_FlipXY:\n";
                for (i = sizeX - 1; i >= 0; i--)
                {
                    strData += "\t.word\t";
                    for (j = sizeY - 1; j >= 0; j--)
                    {
                        flip = 0;
                        if (!(data[(j * sizeX) + i].FlipX)) flip += 0x1;
                        if (!(data[(j * sizeX) + i].FlipY)) flip += 0x2;

                        autoAnimFlag = 0x00;
                        if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                        if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                        tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                        strData += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != 0 ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        );
                    }
                    strData += "\n";
                }
            }
            return strData + "\n";
        }

        public string ScrollerToAsmString(TileList tl, string objName = "NS", int tileShift = 0, bool isColorStream = false)
        {
            int i, j, flip;
            int tileNumber;
            byte autoAnimFlag;
            string strData = "";

            tl.setRealIndexes(tileShift);

            strData += ".globl " + objName + "\n";
            strData += objName + ":\n";
            strData += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;* {0} bytes per strip / {1} tiles spr size\n", sizeY * 4, sizeY > 32 ? 32 : sizeY);
            strData += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;* {0} strips of {1} tiles\n", sizeX, sizeY);
            strData += string.Format("\t.long\t{0}_Palettes\n", objName);
            strData += string.Format("\t.long\t{0}", isColorStream ? string.Format("{0}_colorStream", objName) : "0x00000000");

            for (i = 0; i < sizeX; i++)
            {
                if (i % 16 == 0) strData += "\n\t.long\t";
                strData += string.Format("{0}_strip{1:x4}{2}", objName, i, i % 16 != 15 ? (i == sizeX - 1 ? "" : ", ") : "");
            }
            strData += "\n\n";

            for (i = 0; i < sizeX; i++)
            {
                strData += string.Format("{0}_strip{1:x4}:\n\t.word\t", objName, i);
                for (j = 0; j < sizeY; j++)
                {
                    flip = 0;
                    if (data[(j * sizeX) + i].FlipX) flip += 0x1;
                    if (data[(j * sizeX) + i].FlipY) flip += 0x2;
                    autoAnimFlag = 0x00;
                    if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                    if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;
                    tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);
                    if (!isColorStream)
                        strData += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        );
                    else
                        strData += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                            tileNumber & 0xffff,                    //tile lsb, 16b
                            data[(j * sizeX) + i].streamPaletteNumber,//palette, 8b
                            (tileNumber >> 16) & 0xf,               //tile msb, 4b
                            autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                        );
                }
                strData += "\n";
            }
            //sw.WriteLine(objName + "_Map_end:");
            strData += "\n";

            return strData;
        }

        /*
        public void ToFile(TileList tl, string fName, bool append = false, string objName = "NS", int tileShift = 0)
        {
            int i, j, flip;
            int tileNumber;
            FileStream fs = null;
            byte autoAnimFlag;

            tl.setRealIndexes(tileShift);

            if (append) fs = File.Open(fName, FileMode.Append);
            else fs = File.Create(fName);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(".globl " + objName + "_Map");
            sw.WriteLine(string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} words per column / {1} tiles height", sizeY * 2, sizeY > 32 ? 32 : sizeY));
            sw.WriteLine(string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} cols of {1} tiles", sizeX, sizeY));
            sw.WriteLine(objName + "_Map:");
            //sw.WriteLine(string.Format("\t.byte\t0x{0:x2}, 0x{1:x2}\t|;{0} cols of {1} tiles", sizeX, sizeY));

            for (i = 0; i < sizeX; i++)
            {
                sw.Write("\t.word\t");
                for (j = 0; j < sizeY; j++)
                {
                    flip = 0;
                    if (data[(j * sizeX) + i].FlipX) flip += 0x1;
                    if (data[(j * sizeX) + i].FlipY) flip += 0x2;

                    autoAnimFlag = 0x00;
                    if (data[(j * sizeX) + i].autoAnim == 4) autoAnimFlag = 0x4;
                    if (data[(j * sizeX) + i].autoAnim == 8) autoAnimFlag = 0x8;

                    //tileNumber = data[(j * sizeX) + i].tileNumber + tileShift;
                    //tileNumber = data[(j * sizeX) + i].tileNumber + tileShift;
                    tileNumber = tl.getRealIndex((int)data[(j * sizeX) + i].tileNumber);

                    sw.Write(string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (sizeY - 1) ? ", " : ""),
                        tileNumber & 0xffff,                    //tile lsb, 16b
                        data[(j * sizeX) + i].paletteNumber,    //palette, 8b
                        (tileNumber >> 16) & 0xf,               //tile msb, 4b
                        autoAnimFlag | flip                     //autoanim, 2b | flip, 2b
                    ));
                }
                sw.WriteLine();
            }

            sw.WriteLine(objName + "_Map_end:");
            sw.WriteLine();
            sw.Close();
            fs.Close();
        }*/

        public string getFrameMaps(string id, int x, int y, int w, int h, int tileShift = 0, bool flipX = false, bool flipY = false, bool flipXY = false)
        {
            string str = "";
            int i, j, flip;
            uint tileNumber;

            str += id + "_Map:\n";
            for (i = 0; i < w; i++) //x
            {
                str += "\t.word\t";
                for (j = 0; j < h; j++) //y
                {
                    flip = 0;
                    if (data[((j + y) * sizeX) + (i + x)].FlipX) flip += 0x1;
                    if (data[((j + y) * sizeX) + (i + x)].FlipY) flip += 0x2;

                    tileNumber = data[((j + y) * sizeX) + (i + x)].tileNumber + (uint)tileShift;
                    str += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (h - 1) ? ", " : "\n"),
                        tileNumber & 0xffff,
                        data[((j + y) * sizeX) + (i + x)].paletteNumber,
                        (tileNumber >> 16) & 0xf,
                        flip
                    );
                }
            }

            if (flipX)
            {
                str += id + "_Map_FlipX:\n";
                for (i = w - 1; i >= 0; i--) //x
                {
                    str += "\t.word\t";
                    for (j = 0; j < h; j++) //y
                    {
                        flip = 0;
                        if (!(data[((j + y) * sizeX) + (i + x)].FlipX)) flip += 0x1;
                        if (data[((j + y) * sizeX) + (i + x)].FlipY) flip += 0x2;

                        tileNumber = data[((j + y) * sizeX) + (i + x)].tileNumber + (uint)tileShift;
                        str += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (h - 1) ? ", " : "\n"),
                            tileNumber & 0xffff,
                            data[((j + y) * sizeX) + (i + x)].paletteNumber,
                            (tileNumber >> 16) & 0xf,
                            flip
                        );
                    }
                }
            }

            if (flipY)
            {
                str += id + "_Map_FlipY:\n";
                for (i = 0; i < w; i++) //x
                {
                    str += "\t.word\t";
                    for (j = h - 1; j >= 0; j--) //y
                    {
                        flip = 0;
                        if (data[((j + y) * sizeX) + (i + x)].FlipX) flip += 0x1;
                        if (!(data[((j + y) * sizeX) + (i + x)].FlipY)) flip += 0x2;

                        tileNumber = data[((j + y) * sizeX) + (i + x)].tileNumber + (uint)tileShift;
                        str += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j > 0 ? ", " : "\n"),
                            tileNumber & 0xffff,
                            data[((j + y) * sizeX) + (i + x)].paletteNumber,
                            (tileNumber >> 16) & 0xf,
                            flip
                        );
                    }
                }
            }

            if (flipXY)
            {
                str += id + "_Map_FlipXY:\n";
                for (i = w - 1; i >= 0; i--) //x
                {
                    str += "\t.word\t";
                    for (j = h - 1; j >= 0; j--) //y
                    {
                        flip = 0;
                        if (!(data[((j + y) * sizeX) + (i + x)].FlipX)) flip += 0x1;
                        if (!(data[((j + y) * sizeX) + (i + x)].FlipY)) flip += 0x2;

                        tileNumber = data[((j + y) * sizeX) + (i + x)].tileNumber + (uint)tileShift;
                        str += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j > 0 ? ", " : "\n"),
                            tileNumber & 0xffff,
                            data[((j + y) * sizeX) + (i + x)].paletteNumber,
                            (tileNumber >> 16) & 0xf,
                            flip
                        );
                    }
                }
            }

            return (str);
        }

        public string getPartialMap(int x, int y, int w, int h, int tileShift = 0)
        {
            string str = "";
            int i, j, flip;
            uint tileNumber;

            for (i = 0; i < w; i++) //x
            {
                str += "\t.word\t";
                for (j = 0; j < h; j++) //y
                {
                    flip = 0;
                    if (data[((j + y) * sizeX) + (i + x)].FlipX) flip += 0x1;
                    if (data[((j + y) * sizeX) + (i + x)].FlipY) flip += 0x2;

                    tileNumber = data[((j + y) * sizeX) + (i + x)].tileNumber + (uint)tileShift;
                    str += string.Format("0x{0:x4},0x{1:x2}{2:x1}{3:x1}" + (j != (h - 1) ? ", " : "\n"),
                        tileNumber & 0xffff,
                        data[((j + y) * sizeX) + (i + x)].paletteNumber,
                        (tileNumber >> 16) & 0xf,
                        flip
                    );
                }
            }
            return (str);
        }

        private int GetTilePalette(int tNum)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].tileNumber == tNum)
                {
                    return (int)data[i].paletteNumber;
                }
            }
            return 0;
        }

        public void fixTileDataToBuffer(ref byte[] buffer, int bank, fixTileList ftl, PaletteList pl)
        {
            int b, w, x, y, z;
            int idx;
            Palette pal;
            fixTile tile;
            byte i1, i2, data;

            //copy over fix binary
            w = 0;
            for (b = 0; b < ftl.tiles.Count / 256; b++)
            {
                for (y = 0; y < 16; y++)
                {
                    for (x = 0; x < 16; x++)
                    {
                        //base tile addr
                        idx = (x * 32) + (y * 512) + (b * 8192) + (bank * 8192);
                        tile = ftl.tiles[w];
                        pal = pl.Palettes[GetTilePalette(w++)];

                        for (z = 0; z < 8; z++)
                        {
                            data = buffer[idx + z];
                            i1 = tile.GetColorIndex(z * 8 + 4, pal);
                            i2 = tile.GetColorIndex(z * 8 + 5, pal);
                            if (i1 != 0) data = (byte)((data & 0xf0) | i1);
                            if (i2 != 0) data = (byte)((data & 0xf) | (i2 << 4));
                            buffer[idx + z] = data;
                        }
                        for (z = 0; z < 8; z++)
                        {
                            data = buffer[idx + 8 + z];
                            i1 = tile.GetColorIndex(z * 8 + 6, pal);
                            i2 = tile.GetColorIndex(z * 8 + 7, pal);
                            if (i1 != 0) data = (byte)((data & 0xf0) | i1);
                            if (i2 != 0) data = (byte)((data & 0xf) | (i2 << 4));
                            buffer[idx + 8 + z] = data;
                        }
                        for (z = 0; z < 8; z++)
                        {
                            data = buffer[idx + 16 + z];
                            i1 = tile.GetColorIndex(z * 8 + 0, pal);
                            i2 = tile.GetColorIndex(z * 8 + 1, pal);
                            if (i1 != 0) data = (byte)((data & 0xf0) | i1);
                            if (i2 != 0) data = (byte)((data & 0xf) | (i2 << 4));
                            buffer[idx + 16 + z] = data;
                        }
                        for (z = 0; z < 8; z++)
                        {
                            data = buffer[idx + 24 + z];
                            i1 = tile.GetColorIndex(z * 8 + 2, pal);
                            i2 = tile.GetColorIndex(z * 8 + 3, pal);
                            if (i1 != 0) data = (byte)((data & 0xf0) | i1);
                            if (i2 != 0) data = (byte)((data & 0xf) | (i2 << 4));
                            buffer[idx + 24 + z] = data;
                        }
                    }
                }
            }
        }

        /*tmp*/
        public void TileDataToFile(string fName, TileList tl, PaletteList pl, bool append = false)
        {
            int i, j, x, p, auto;
            byte[] data = new byte[256];
            byte b1, b2, b3, b4;
            Palette pal;
            Tile tile;
            FileStream fs = null;

            //dummy tile
            byte[] dummy = {   0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xb9, 0x00, 0x00, 0x00, 0x92, 0x00, 0x00, 0x00,
                               0x93, 0x00, 0x00, 0x00, 0x92, 0x00, 0x00, 0x00, 0x92, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00,
                               0x80, 0x00, 0x00, 0x00, 0x82, 0x00, 0x00, 0x00, 0x82, 0x00, 0x00, 0x00, 0x86, 0x00, 0x00, 0x00,
                               0x8a, 0x00, 0x00, 0x00, 0x86, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00,
                               0xff, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0xa9, 0x00, 0x00, 0x00,
                               0xa9, 0x00, 0x00, 0x00, 0xa9, 0x00, 0x00, 0x00, 0x99, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                               0x01, 0x00, 0x00, 0x00, 0x91, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x91, 0x00, 0x00, 0x00,
                               0x91, 0x00, 0x00, 0x00, 0xa1, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00 };

            if (append) fs = File.Open(fName, FileMode.Append);
            else fs = File.Create(fName);
            BinaryWriter bw = new BinaryWriter(fs);

            for (i = 0; i < tl.GetCount(); i++)
            {
                tile = tl.GetTile(i);

                if (tile.autoAnim != 1)
                {
                    //tile.getautoAnimStrip().Save(string.Format("tmpdat\\dat{0:d4}.png", i));
                    b1 = 0;
                    for (auto = 0; auto < tile.autoAnim; auto++)
                    {
                        x = GetTilePalette(i); //x = tile palette #
                        pal = pl.GetPalette(x);
                        tile = tl.GetTile(i);
                        for (j = 0; j < 256; j++) //256 pixels
                        {
                            data[j] = tile.GetColorIndex(j, pal, auto);
                        }
                        for (p = 0; p < 256; p += 16)
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
                            bw.Write(b1);
                            bw.Write(b2);
                            bw.Write(b3);
                            bw.Write(b4);
                        }
                        for (p = 0; p < 256; p += 16)
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
                            bw.Write(b1);
                            bw.Write(b2);
                            bw.Write(b3);
                            bw.Write(b4);
                        }
                    }
                }
                else
                {
                    if (tile.isDummy)
                    {
                        bw.Write(dummy);
                    }
                    else
                    {
                        //tile.getBitmap().Save(string.Format("tmpdat\\dat{0:d4}.png", i));
                        x = GetTilePalette(i); //x = tile palette #
                        pal = pl.GetPalette(x);
                        tile = tl.GetTile(i);
                        for (j = 0; j < 256; j++) //256 pixels
                        {
                            data[j] = tile.GetColorIndex(j, pal);
                        }
                        for (p = 0; p < 256; p += 16)
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
                            bw.Write(b1);
                            bw.Write(b2);
                            bw.Write(b3);
                            bw.Write(b4);
                        }
                        for (p = 0; p < 256; p += 16)
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
                            bw.Write(b1);
                            bw.Write(b2);
                            bw.Write(b3);
                            bw.Write(b4);
                        }
                    }
                }
            }
            bw.Close();
            fs.Close();
        }
        /**/
        public void TileDataToBuffer(TileList tl, PaletteList pl, ref byte[] buffer, ref int index)
        {
            int i, j, x, p, auto;
            byte[] data = new byte[256];
            byte b1, b2, b3, b4;
            Palette pal;
            Tile tile;

            //dummy tile
            byte[] dummy = {   0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xb9, 0x00, 0x00, 0x00, 0x92, 0x00, 0x00, 0x00,
                               0x93, 0x00, 0x00, 0x00, 0x92, 0x00, 0x00, 0x00, 0x92, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00,
                               0x80, 0x00, 0x00, 0x00, 0x82, 0x00, 0x00, 0x00, 0x82, 0x00, 0x00, 0x00, 0x86, 0x00, 0x00, 0x00,
                               0x8a, 0x00, 0x00, 0x00, 0x86, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00,
                               0xff, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0xa9, 0x00, 0x00, 0x00,
                               0xa9, 0x00, 0x00, 0x00, 0xa9, 0x00, 0x00, 0x00, 0x99, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                               0x01, 0x00, 0x00, 0x00, 0x91, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x91, 0x00, 0x00, 0x00,
                               0x91, 0x00, 0x00, 0x00, 0xa1, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00 };

            for (i = 0; i < tl.GetCount(); i++)
            {
                tile = tl.GetTile(i);

                if (tile.isDummy)
                {
                    for (x = 0; x < 128; x++)
                    {
                        buffer[index++] = dummy[x];
                    }
                }
                else
                {
                    for (auto = 0; auto < tile.autoAnim; auto++)
                    {
                        x = GetTilePalette(i); //x = tile palette #
                        pal = pl.GetPalette(x);
                        tile = tl.GetTile(i);
                        for (j = 0; j < 256; j++) //256 pixels
                        {
                            data[j] = tile.GetColorIndex(j, pal, auto);
                        }
                        for (p = 0; p < 256; p += 16)
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
                            buffer[index++] = b1;
                            buffer[index++] = b2;
                            buffer[index++] = b3;
                            buffer[index++] = b4;
                        }
                        for (p = 0; p < 256; p += 16)
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
                            buffer[index++] = b1;
                            buffer[index++] = b2;
                            buffer[index++] = b3;
                            buffer[index++] = b4;
                        }
                    }
                }
            }
        }

        public int paletteIterations(int palette)
        {
            int i, x = 0;

            for (i = 0; i < sizeX * sizeY; i++)
            {
                if (data[i].paletteNumber == palette) x++;
            }

            return (x);
        }

        public void paletteMask(int palette, Bitmap bmp, string file)
        {
            int x, y;

            Bitmap tempBmp = new Bitmap(bmp);
            Graphics g = Graphics.FromImage(tempBmp);

            for (x = 0; x < sizeX; x++)
            {
                for (y = 0; y < sizeY; y++)
                {
                    if (data[(y * sizeX) + x].paletteNumber != palette)
                    {
                        g.FillRectangle(Brushes.Fuchsia, x * 16, y * 16, 16, 16);
                    }
                }
            }
            tempBmp.MakeTransparent(Color.Fuchsia);

            tempBmp.Save(file);
            g.Dispose();
        }

    }
}

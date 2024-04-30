using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace NeoTools
{
    public class PaletteList
    {
        public List<Palette> Palettes = null;

        public PaletteList()
        {
            Palettes = new List<Palette>(0);
        }

        public PaletteList(int count)
        {
            Palettes = new List<Palette>(count);
            for (int i = 0; i < count; i++)
            {
                Palettes.Add(new Palette());
            }
        }

        public PaletteList(ref TileList tiles, ref string errmsg)
        {
            Palettes = new List<Palette>(tiles.GetCount());
            for (int i = 0; i < tiles.GetCount(); i++)
            {
                Palettes.Add(new Palette());
                if (!Palettes[i].FromTile(tiles.GetTile(i)))
                {
                    errmsg += "Tile #" + i.ToString() + " color overload\r\n";
                }
            }
        }

        public PaletteList(ref fixTileList ftl, ref string errmsg)
        {
            Palettes = new List<Palette>(ftl.tiles.Count);
            for (int i = 0; i < ftl.tiles.Count; i++)
            {
                Palettes.Add(new Palette());
                if (!Palettes[i].FromFixTile(ftl.tiles[i]))
                {
                    errmsg += "Tile #" + i.ToString() + " color overload\r\n";
                }
            }
        }


        public int GetCount()
        {
            return Palettes.Count;
        }

        public void removePalette(int x)
        {
            Palettes.Remove(Palettes[x]);
        }

        //remove duplicate palettes
        public void CleanUp(TileMap tm)
        {
            int i, j, aim;
            bool opt;

            for (i = 0; i < Palettes.Count; i++)
            {
                for (j = i + 1; j < Palettes.Count; j++)
                {
                    if (Palettes[j].colorCount <= Palettes[i].colorCount)
                    {
                        if (Palettes[j].IsIncludedIn(Palettes[i]))
                        {
                            tm.replacePalette(j, i);
                            tm.decreasePalette(j);
                            Palettes.Remove(Palettes[j]);
                            j--;
                        }
                    }
                    else
                    {
                        if (Palettes[i].IsIncludedIn(Palettes[j]))
                        {
                            tm.replacePalette(i, j);
                            tm.decreasePalette(i);
                            Palettes.Remove(Palettes[i]);
                            j = i;
                        }
                    }
                }
            }
            
            //palettes combine
            aim = 15;
            do
            {
                opt = false;
                for (i = 0; i < Palettes.Count; i++)
                {
                    for (j = i + 1; j < Palettes.Count; j++)
                    {
                        if (Palettes[i].mergeResult(Palettes[j]) == aim)
                        {
                            Palettes[i].Merge(Palettes[j]);
                            tm.replacePalette(j, i);
                            tm.decreasePalette(j);
                            Palettes.Remove(Palettes[j]);
                            j--;
                            opt = true;
                            break;
                        }
                    }
                }
                aim--;
                if (opt) aim = 15;
            } while (aim > 0);
        }

        public Palette GetPalette(int palNum)
        {
            return Palettes[palNum];
        }
        
        public void ToFile(string fName, bool append = false, string objName = "NS")
        {
            int i;
            FileStream fs = null;

            if (append) fs = File.Open(fName, FileMode.Append);
            else fs = File.Create(fName);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(".globl " + objName + "_Palettes");
            sw.WriteLine(objName + "_Palettes:");
            sw.WriteLine(string.Format("\t.word\t0x{0:x4} ;* {0} palettes", Palettes.Count));
            //sw.WriteLine(objName + "_Palettes:\t");

            for (i = 0; i < Palettes.Count; i++)
            {
                //sw.WriteLine(string.Format("\tdc.w\t${0:x4}, ${1:x4}, ${2:x4}, ${3:x4}, ${4:x4}, ${5:x4}, ${6:x4}, ${7:x4}, ${8:x4}, ${9:x4}, ${10:x4}, ${11:x4}, ${12:x4}, ${13:x4}, ${14:x4}, ${15:x4};", Palettes[i].GetNeoColor(0), Palettes[i].GetNeoColor(1), Palettes[i].GetNeoColor(2), Palettes[i].GetNeoColor(3), Palettes[i].GetNeoColor(4), Palettes[i].GetNeoColor(5), Palettes[i].GetNeoColor(6), Palettes[i].GetNeoColor(7), Palettes[i].GetNeoColor(8), Palettes[i].GetNeoColor(9), Palettes[i].GetNeoColor(10), Palettes[i].GetNeoColor(11), Palettes[i].GetNeoColor(12), Palettes[i].GetNeoColor(13), Palettes[i].GetNeoColor(14), Palettes[i].GetNeoColor(15)));
                sw.WriteLine(string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}, 0x{3:x4}, 0x{4:x4}, 0x{5:x4}, 0x{6:x4}, 0x{7:x4}, 0x{8:x4}, 0x{9:x4}, 0x{10:x4}, 0x{11:x4}, 0x{12:x4}, 0x{13:x4}, 0x{14:x4}, 0x{15:x4}", Palettes[i].NGColor[0], Palettes[i].NGColor[1], Palettes[i].NGColor[2], Palettes[i].NGColor[3], Palettes[i].NGColor[4], Palettes[i].NGColor[5], Palettes[i].NGColor[6], Palettes[i].NGColor[7], Palettes[i].NGColor[8], Palettes[i].NGColor[9], Palettes[i].NGColor[10], Palettes[i].NGColor[11], Palettes[i].NGColor[12], Palettes[i].NGColor[13], Palettes[i].NGColor[14], Palettes[i].NGColor[15]));
            }
            //sw.WriteLine(objName + "_Palettes_end:");
            sw.WriteLine();

            sw.Close();
            fs.Close();
        }
        
        public string ToAsmString(string objName="NS")
        {
            string data = "";
            int i;

            data += ".globl " + objName + "_Palettes\n";
            data += objName + "_Palettes:\n";
            data += string.Format("\t.word\t0x{0:x4} ;* {0} palettes\n", Palettes.Count);

            for (i = 0; i < Palettes.Count; i++)
            {
                data += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}, 0x{2:x4}, 0x{3:x4}, 0x{4:x4}, 0x{5:x4}, 0x{6:x4}, 0x{7:x4}, 0x{8:x4}, 0x{9:x4}, 0x{10:x4}, 0x{11:x4}, 0x{12:x4}, 0x{13:x4}, 0x{14:x4}, 0x{15:x4}\n",
                    Palettes[i].NGColor[0], Palettes[i].NGColor[1], Palettes[i].NGColor[2], Palettes[i].NGColor[3],
                    Palettes[i].NGColor[4], Palettes[i].NGColor[5], Palettes[i].NGColor[6], Palettes[i].NGColor[7],
                    Palettes[i].NGColor[8], Palettes[i].NGColor[9], Palettes[i].NGColor[10], Palettes[i].NGColor[11],
                    Palettes[i].NGColor[12], Palettes[i].NGColor[13], Palettes[i].NGColor[14], Palettes[i].NGColor[15]);
            }
            data += "\n";

            return data;
        }

    }
}

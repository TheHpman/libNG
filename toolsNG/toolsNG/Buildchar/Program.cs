using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using NeoTools;

//still is midway through switching to color index instead of pixels but will work for now

namespace Buildchar
{
    public struct frameInfo
    {
        public int x, y, w, h;
    }

    class Program
    {
        static string charFile = "char.bin", mapFile = "maps.s", palFile = "palettes.s", incFile = "externs.h", incPrefix = "";
        static int textureSize = 0, tilesPerTexture = 0, textureTileWidth = 0, textureIdx = 0;
        static Bitmap texturePage = null;
        static string textureFolder = "";

        static void showUsage(bool showApp = true)
        {
            if (showApp)
                Console.WriteLine("BuildChar " +/*v1.0*/"- libNG / Hpman" + Environment.NewLine + "Process pictures into tiles, maps and palettes data." + Environment.NewLine);
            Console.WriteLine("Usage: BuildChar [input_file]");
#if DEBUG
            Console.Read();
#endif
        }

        static bool loadBitmap(ref Bitmap bmp, string file, ref List<palette> pals, bool parsePals = false)
        {
            try
            {
                bmp = (Bitmap)new Bitmap(file);
                if ((bmp.PixelFormat == PixelFormat.Format8bppIndexed) && parsePals)
                {
                    //parse palettes
                    int i, c = 0;
                    do
                    {
                        palette p = new palette();
                        c++;
                        for (i = 0; i < 15; i++)
                        {
                            if (c >= bmp.Palette.Entries.Length) break;

                            p.addRGBColor(bmp.Palette.Entries[c].R, bmp.Palette.Entries[c].G, bmp.Palette.Entries[c].B);
                            c++;
                        }
                        if (i > 0)
                            pals.Add(p);
                    } while (i == 15);
                }
                bmp.MakeTransparent(Color.Fuchsia); //maketransparent() turns bitmap into 32bppArgb
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            if ((bmp.Width % 16 != 0) || (bmp.Height % 16 != 0))
            {
                Console.WriteLine("Invalid dimensions (" + bmp.Width + "x" + bmp.Height + ")");
                return false;
            }
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                Console.WriteLine("Invalid format");
                return false;
            }
            return true;
        }

        static bool loadPalFile(string file, ref List<palette> pals)
        {
            string ext = file.Substring(file.LastIndexOf('.'));


            switch (file.Substring(file.LastIndexOf('.')).ToUpper())
            {
                case ".PNG":
                    Bitmap bmp = new Bitmap(file);
                    bmp.MakeTransparent(Color.Fuchsia);
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        palette p = new palette();
                        for (int x = 1; x < 16; x++)
                        {
                            Color c = bmp.GetPixel(x, y);
                            if (c.A == 255)
                                p.addRGBColor(c.R, c.G, c.B);
                        }
                        pals.Add(p);
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        static void ptnTexture(ref pattern ptn)
        {
            Graphics g;
            foreach (tile T in ptn.tiles)
            {
                if (T.duplicateOf >= 0) continue;
                for (int i = 0; i < T.autoAnim; i++)
                {
                    if (textureIdx % tilesPerTexture == 0)
                    {
                        //save page if non null
                        texturePage?.Save(textureFolder + string.Format("texture{0:d4}.png", (textureIdx - 1) / tilesPerTexture));
                    }
                    if ((textureIdx % tilesPerTexture == 0) || (texturePage == null))
                    {
                        texturePage = new Bitmap(textureSize, textureSize, PixelFormat.Format32bppArgb);
                        g = Graphics.FromImage(texturePage);
                        g.Clear(Color.Black);
                    }
                    else g = Graphics.FromImage(texturePage);

                    //copy tile
                    int pageIdx = textureIdx % tilesPerTexture;
                    int x = 20 * (pageIdx % textureTileWidth);
                    int y = 20 * (pageIdx / textureTileWidth);
                    g.DrawImage(T.texture, x, y, new Rectangle(i * 20, 0, 20, 20), GraphicsUnit.Pixel);
                    g.Dispose();
                    textureIdx++;
                }
            }
        }

        static Bitmap load32bppImage(string fName, Bitmap mask = null)
        {
            Graphics gfx;
            try
            {
                Bitmap bmp = new Bitmap(fName);
                if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    Bitmap tmpBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
                    gfx = Graphics.FromImage(tmpBmp);
                    gfx.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                    gfx.Dispose();
                    bmp.Dispose();
                    bmp = tmpBmp;
                }
                if (mask != null)
                {
                    gfx = Graphics.FromImage(bmp);
                    gfx.DrawImage(mask, 0, 0);
                    gfx.Dispose();
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to open file " + fName + Environment.NewLine + ex.Message + Environment.NewLine);
                Environment.Exit(1);
            }
            return null;
        }

        static void Main(string[] args)
        {
            //debug flags
            bool processScrolls = true, processPics = true, processSprites = true;
            //settings
            bool isFix = false, dummyFill = true;
            int baseTile = 0, startBaseTile = 0;

            //data
            string externs = "", mapData = "", palData = "", section = "";
            byte[] charData = null;
            int charDataIdx = 0;
            List<int> skippedTiles = new List<int>(0);
            List<palette> fixedPalettes = null;

            //misc vars
            XmlDocument doc = new XmlDocument();
            XmlNode node1, node2, node3, node4, node5, node6, node7, tmpNode;
            XmlNodeList tmpNodeList;
            int autoAnim, itemNum, slotCount = 0;
            Bitmap workBmp = null, workBmp1, workBmp2, workBmp3, workBmp4, workBmp5, workBmp6, workBmp7;
            pattern ptn = null;
            string str, fName, id, colorStreamType;
            bool flipX, flipY, flipXY, isColorStream;
            List<frameInfo> frames = null;
            bool makeTexture = false;

            // (define animation macros)
            mapData = string.Format("\t.macro\t_ANM_END{0}\t.word\t0xc000{0}\t.endm{0}{0}" +
                            "\t.macro\t_ANM_REPEAT _count{0}\t.word\t0x8000, \\_count{0}\t.endm{0}{0}" +
                            "\t.macro\t_ANM_LINK _id _ptr{0}\t.word\t0xa000, \\_id{0}\t.long\t\\_ptr{0}\t.endm{0}{0}", Environment.NewLine);

#if DEBUG
            processScrolls = true;
            processPics = false;
#endif

            if (args.Length != 1)
            {
                showUsage();
                Environment.Exit(1);
            }

            string xmlFile = args[0];
            Console.WriteLine("Opening " + xmlFile + "..." + Environment.NewLine);
            try
            {
                doc.Load(xmlFile);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
#if DEBUG
                Console.Read();
#endif
                Environment.Exit(1);
            }

            XmlNode node = doc.DocumentElement;
            XmlNodeList ndlist = node.SelectNodes("setup");

            //fix file data?
            if (ndlist[0].Attributes["fileType"] != null)
                if (ndlist[0].Attributes["fileType"].Value.ToUpper() == "FIX")
                    isFix = true;

            if (ndlist[0].SelectNodes("starting_tile").Item(0) != null)
            {
                startBaseTile = baseTile = int.Parse(ndlist[0].SelectNodes("starting_tile").Item(0).InnerText);

                if (ndlist[0].SelectNodes("starting_tile").Item(0).Attributes["fillmode"] != null)
                    if (ndlist[0].SelectNodes("starting_tile").Item(0).Attributes["fillmode"].Value.ToUpper() == "NONE")
                        dummyFill = false;
            }

            if (ndlist[0].SelectNodes("charfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("charfile").Item(0).InnerText;
                if (str != "") charFile = str;
            }
            if (ndlist[0].SelectNodes("mapfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("mapfile").Item(0).InnerText;
                if (str != "") mapFile = str;
            }
            if (ndlist[0].SelectNodes("palfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("palfile").Item(0).InnerText;
                if (str != "") palFile = str;
            }
            if (ndlist[0].SelectNodes("incfile").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("incfile").Item(0).InnerText;
                if (str != "") incFile = str;
            }
            if (ndlist[0].SelectNodes("incprefix").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("incprefix").Item(0).InnerText;
                if (str != "") incPrefix = str;
            }
            //undocumented
            if (ndlist[0].SelectNodes("textureSize").Item(0) != null)
            {
                str = ndlist[0].SelectNodes("textureSize").Item(0).InnerText;
                if (str != "") textureSize = int.Parse(str);
            }

            if (isFix)
            {
                bool _fixSuccess = fixProcess(node);
#if DEBUG
                Console.Read();
#endif
                Environment.Exit(_fixSuccess ? 0 : 1);
            }

            //not fix data

            //allocate space for binary data
            charData = new byte[128 * 1024 * 1024]; //I has RAM

            //basetile handling
            charDataIdx = baseTile * 128;

            //imports handling
            XmlNodeList importList = node.SelectNodes("import");
            if (importList.Count > 0)
            {
                byte[] buffer;
                Console.WriteLine("Importing " + importList.Count.ToString() + " files...");

                foreach (XmlNode import in importList)
                {
                    str = "";
                    try
                    {
                        str = import.SelectSingleNode("file").InnerText;
                        Console.WriteLine("Importing " + str + "...");
                        buffer = File.ReadAllBytes(str);
                        if (buffer.Length % 128 != 0)
                        {
                            Console.WriteLine("Import file isn't 128 bytes multiple.");
                            Environment.Exit(1);
                        }
                        baseTile += buffer.Length / 128;
                        buffer.CopyTo(charData, charDataIdx);
                        charDataIdx += buffer.Length;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to import file " + str + Environment.NewLine + e.Message);
                        Environment.Exit(1);
                    }
                }
                Console.WriteLine();
            }

            if (textureSize > 0)
            {
                makeTexture = true;
                textureTileWidth = textureSize / 20;
                tilesPerTexture = textureTileWidth * textureTileWidth;
                textureIdx = baseTile;
            }

            if (processScrolls)
            {
                #region [scrollers processing]

                itemNum = 0;
                ndlist = node.SelectNodes("scrl");
                if (ndlist.Count > 0) externs += "//scrollers" + Environment.NewLine;

                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Scrollers - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode scrl in ndlist)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    //get file name & ID
                    try
                    {
                        fName = scrl.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = scrl.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //section
                    if (scrl.Attributes["section"] == null)
                        section = ".text";
                    else section = scrl.Attributes["section"].Value;
                    palData += ".section " + section + Environment.NewLine;
                    mapData += ".section " + section + Environment.NewLine;

                    //check colorStream infos
                    colorStreamType = "";
                    if (scrl.Attributes["colorStream"] != null)
                        colorStreamType = scrl.Attributes["colorStream"].Value.ToUpper();
                    switch (colorStreamType)
                    {
                        case "HORIZONTAL":
                        case "VERTICAL":
                            isColorStream = true;
                            break;
                        default:
                            isColorStream = false;
                            break;
                    }

                    //check preset palette file
                    fixedPalettes = new List<palette>(0);
                    if (scrl.Attributes["palFile"] != null)
                        loadPalFile(scrl.Attributes["palFile"].Value, ref fixedPalettes);

                    // check for autofiles
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = scrl.SelectSingleNode("auto1"); node2 = scrl.SelectSingleNode("auto2"); node3 = scrl.SelectSingleNode("auto3"); node4 = scrl.SelectSingleNode("auto4"); node5 = scrl.SelectSingleNode("auto5"); node6 = scrl.SelectSingleNode("auto6"); node7 = scrl.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");
                    if (!loadBitmap(ref workBmp, fName, ref fixedPalettes, true)) return;
                    if (autoAnim >= 4)
                    {
                        if (!loadBitmap(ref workBmp1, node1.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp2, node2.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp3, node3.InnerText, ref fixedPalettes)) return;
                    }
                    if (autoAnim == 8)
                    {
                        if (!loadBitmap(ref workBmp4, node4.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp5, node5.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp6, node6.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp7, node7.InnerText, ref fixedPalettes)) return;
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.Width != workBmp1.Width) || (workBmp.Width != workBmp2.Width) || (workBmp.Width != workBmp3.Width) ||
                            (workBmp.Height != workBmp1.Height) || (workBmp.Height != workBmp2.Height) || (workBmp.Height != workBmp3.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    if (autoAnim == 8)
                        if ((workBmp.Width != workBmp4.Width) || (workBmp.Width != workBmp5.Width) || (workBmp.Width != workBmp6.Width) || (workBmp.Width != workBmp7.Width) ||
                            (workBmp.Height != workBmp4.Height) || (workBmp.Height != workBmp5.Height) || (workBmp.Height != workBmp6.Height) || (workBmp.Height != workBmp7.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    Console.WriteLine("OK");

                    Console.Write("Parsing data ");
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0} tiles{1}]", (workBmp.Height * workBmp.Width) / 256, autoAnim == 1 ? "" : string.Format(" x{0}", autoAnim)));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("... ");

                    //parse tiledata / palettes
                    ptn = new pattern(id, ref workBmp, ref workBmp1, ref workBmp2, ref workBmp3, ref workBmp4, ref workBmp5, ref workBmp6, ref workBmp7, ref fixedPalettes);

                    //"rasterize" tiles
                    ptn.rasterize(ref charData, ref charDataIdx, ref skippedTiles, makeTexture);
                    if (makeTexture)
                        ptnTexture(ref ptn);

                    if (isColorStream)
                        switch (colorStreamType)
                        {
                            case "HORIZONTAL":
                                palData += ptn.doColorStream_Horizontal(ref slotCount);
                                break;
                            case "VERTICAL":
                                palData += ptn.doColorStream_Vertical(ref slotCount);
                                break;
                            default:
                                break;
                        }

                    externs += string.Format("extern const scrollerInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine + "{1}", id, isColorStream ? string.Format("extern const colorStreamInfo {0}_colorStream;" + Environment.NewLine, id) : "");
                    palData += ptn.palData() + Environment.NewLine;
                    mapData += ptn.scrollerMapData() + Environment.NewLine;

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes{2}]", ptn.tileCount(), fixedPalettes.Count > 0 ? fixedPalettes.Count : ptn.palsCount(), isColorStream ? string.Format(" (colorStream: {0} slots)", slotCount) : ""));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                }
                #endregion
            }

            if (processPics)
            {
                #region [pictures processing]

                itemNum = 0;
                ndlist = node.SelectNodes("pict");
                if (ndlist.Count > 0) externs += Environment.NewLine + "//pictures" + Environment.NewLine;

                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Pictures - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode pict in ndlist)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    //get file name & ID
                    try
                    {
                        fName = pict.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = pict.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //section
                    if (pict.Attributes["section"] == null)
                        section = ".text";
                    else section = pict.Attributes["section"].Value;
                    palData += ".section " + section + Environment.NewLine;
                    mapData += ".section " + section + Environment.NewLine;

                    //get flip modes
                    string flipModes = "";
                    if (pict.Attributes["flips"] != null)
                        flipModes = pict.Attributes["flips"].Value.ToUpper();
                    else
                        if ((tmpNode = pict.SelectSingleNode("flips")) != null)
                        flipModes = tmpNode.InnerText.ToUpper();
                    flipX = flipModes.Contains('X');
                    flipY = flipModes.Contains('Y');
                    flipXY = flipModes.Contains('Z');

                    // check for autofiles
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = pict.SelectSingleNode("auto1"); node2 = pict.SelectSingleNode("auto2"); node3 = pict.SelectSingleNode("auto3"); node4 = pict.SelectSingleNode("auto4"); node5 = pict.SelectSingleNode("auto5"); node6 = pict.SelectSingleNode("auto6"); node7 = pict.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    fixedPalettes = new List<palette>(0);
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");
                    if (!loadBitmap(ref workBmp, fName, ref fixedPalettes, true)) return;
                    if (autoAnim >= 4)
                    {
                        if (!loadBitmap(ref workBmp1, node1.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp2, node2.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp3, node3.InnerText, ref fixedPalettes)) return;
                    }
                    if (autoAnim == 8)
                    {
                        if (!loadBitmap(ref workBmp4, node4.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp5, node5.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp6, node6.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp7, node7.InnerText, ref fixedPalettes)) return;
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.Width != workBmp1.Width) || (workBmp.Width != workBmp2.Width) || (workBmp.Width != workBmp3.Width) ||
                            (workBmp.Height != workBmp1.Height) || (workBmp.Height != workBmp2.Height) || (workBmp.Height != workBmp3.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    if (autoAnim == 8)
                        if ((workBmp.Width != workBmp4.Width) || (workBmp.Width != workBmp5.Width) || (workBmp.Width != workBmp6.Width) || (workBmp.Width != workBmp7.Width) ||
                            (workBmp.Height != workBmp4.Height) || (workBmp.Height != workBmp5.Height) || (workBmp.Height != workBmp6.Height) || (workBmp.Height != workBmp7.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    Console.WriteLine("OK");

                    Console.Write("Parsing data ");
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0} tiles{1}]", (workBmp.Height * workBmp.Width) / 256, autoAnim == 1 ? "" : string.Format(" x{0}", autoAnim)));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("... ");

                    ptn = new pattern(id, ref workBmp, ref workBmp1, ref workBmp2, ref workBmp3, ref workBmp4, ref workBmp5, ref workBmp6, ref workBmp7, ref fixedPalettes);
                    ptn.rasterize(ref charData, ref charDataIdx, ref skippedTiles, makeTexture);
                    if (makeTexture) ptnTexture(ref ptn);

                    externs += string.Format("extern const pictureInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine, id);
                    palData += ptn.palData() + Environment.NewLine;
                    mapData += ptn.pictureMapData(flipX, flipY, flipXY) + Environment.NewLine;

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes]", ptn.tileCount(), fixedPalettes.Count > 0 ? fixedPalettes.Count : ptn.palsCount()));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                } //foreach pict
                #endregion
            }

            if (false)
            {
                #region [sprites processing]

                itemNum = 0;
                ndlist = node.SelectNodes("sprt");
                if (ndlist.Count > 0) externs += Environment.NewLine + "//animated sprites" + Environment.NewLine;

                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Sprites - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode sprt in ndlist)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    //get file name & ID
                    try
                    {
                        fName = sprt.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = sprt.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //get flip modes
                    flipX = flipY = flipXY = false;
                    tmpNode = sprt.SelectSingleNode("flips");
                    if (tmpNode != null)
                    {
                        string modes = tmpNode.InnerText.ToUpper();
                        if (modes.Contains('X')) flipX = true;
                        if (modes.Contains('Y')) flipY = true;
                        if (modes.Contains('Z')) flipXY = true;
                    }

                    // check for autofiles
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = sprt.SelectSingleNode("auto1"); node2 = sprt.SelectSingleNode("auto2"); node3 = sprt.SelectSingleNode("auto3"); node4 = sprt.SelectSingleNode("auto4"); node5 = sprt.SelectSingleNode("auto5"); node6 = sprt.SelectSingleNode("auto6"); node7 = sprt.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    fixedPalettes = new List<palette>(0);
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");
                    if (!loadBitmap(ref workBmp, fName, ref fixedPalettes, true)) return;
                    if (autoAnim >= 4)
                    {
                        if (!loadBitmap(ref workBmp1, node1.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp2, node2.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp3, node3.InnerText, ref fixedPalettes)) return;
                    }
                    if (autoAnim == 8)
                    {
                        if (!loadBitmap(ref workBmp4, node4.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp5, node5.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp6, node6.InnerText, ref fixedPalettes)) return;
                        if (!loadBitmap(ref workBmp7, node7.InnerText, ref fixedPalettes)) return;
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.Width != workBmp1.Width) || (workBmp.Width != workBmp2.Width) || (workBmp.Width != workBmp3.Width) ||
                            (workBmp.Height != workBmp1.Height) || (workBmp.Height != workBmp2.Height) || (workBmp.Height != workBmp3.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    if (autoAnim == 8)
                        if ((workBmp.Width != workBmp4.Width) || (workBmp.Width != workBmp5.Width) || (workBmp.Width != workBmp6.Width) || (workBmp.Width != workBmp7.Width) ||
                            (workBmp.Height != workBmp4.Height) || (workBmp.Height != workBmp5.Height) || (workBmp.Height != workBmp6.Height) || (workBmp.Height != workBmp7.Height))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            return;
                        }
                    Console.WriteLine("OK");

                    //parse frames & build mask
                    Bitmap mask = new Bitmap(workBmp.Width, workBmp.Height);
                    Graphics gfx = Graphics.FromImage(mask);
                    gfx.FillRectangle(Brushes.Fuchsia, 0, 0, mask.Width, mask.Height);

                    frames = new List<frameInfo>(0);
                    tmpNodeList = sprt.SelectNodes("frame");
                    foreach (XmlNode f in tmpNodeList)
                    {
                        frameInfo fi = new frameInfo();
                        string s = f.InnerText;

                        fi.x = int.Parse(s.Substring(0, s.IndexOf(',')));
                        s = s.Substring(s.IndexOf(',') + 1);
                        fi.y = int.Parse(s.Substring(0, s.IndexOf(':')));
                        s = s.Substring(s.IndexOf(':') + 1);
                        fi.w = int.Parse(s.Substring(0, s.IndexOf(',')));
                        s = s.Substring(s.IndexOf(',') + 1);
                        fi.h = int.Parse(s);
                        //TODO: check bounds
                        gfx.FillRectangle(Brushes.Cyan, fi.x << 4, fi.y << 4, fi.w << 4, fi.h << 4);
                        frames.Add(fi);
                    }
                    gfx.Dispose();
                    mask.MakeTransparent(Color.Cyan);

                    //apply mask
                    gfx = Graphics.FromImage(workBmp);
                    gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height);
                    gfx.Dispose();
                    workBmp.MakeTransparent(Color.Fuchsia);
                    if (workBmp1 != null) { gfx = Graphics.FromImage(workBmp1); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp2 != null) { gfx = Graphics.FromImage(workBmp2); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp3 != null) { gfx = Graphics.FromImage(workBmp3); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp4 != null) { gfx = Graphics.FromImage(workBmp4); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp5 != null) { gfx = Graphics.FromImage(workBmp5); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp6 != null) { gfx = Graphics.FromImage(workBmp6); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }
                    if (workBmp7 != null) { gfx = Graphics.FromImage(workBmp7); gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height); gfx.Dispose(); workBmp.MakeTransparent(Color.Fuchsia); }

                    Console.Write("Parsing data ");
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0} tiles{1}]", (workBmp.Height * workBmp.Width) / 256, autoAnim == 1 ? "" : string.Format(" x{0}", autoAnim)));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("... ");

                    //process
                    ptn = new pattern(id, ref workBmp, ref workBmp1, ref workBmp2, ref workBmp3, ref workBmp4, ref workBmp5, ref workBmp6, ref workBmp7, ref fixedPalettes);
                    ptn.rasterize(ref charData, ref charDataIdx, ref skippedTiles, makeTexture);
                    if (makeTexture) ptnTexture(ref ptn);

                    externs += string.Format("extern const spriteInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine + "#include \"" + incPrefix + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + ".h\"" + Environment.NewLine, id);
                    palData += ptn.palData() + Environment.NewLine;
                    mapData += ptn.framesMapData(ref frames, flipX, flipY, flipXY) + Environment.NewLine;
                    mapData += ".include \"" + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + "_anims.s" + "\"" + Environment.NewLine + Environment.NewLine;

                    //cut frames for animator
                    if (frames.Count > 0)
                    {
                        int fCount = 0;
                        Directory.CreateDirectory(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id);
                        foreach (frameInfo fi in frames)
                        {
                            try
                            {
                                workBmp.Clone(new Rectangle(fi.x * 16, fi.y * 16, fi.w * 16, fi.h * 16), PixelFormat.Format32bppArgb).Save(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id + string.Format("\\({0:d4}) ", fCount) + id + string.Format("_{0:x4}.png", fCount));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(Environment.NewLine + Environment.NewLine + "Error saving frame: " + e.Message);
                                return;
                            }
                            fCount++;
                        }
                    }

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes]", ptn.tileCount(), fixedPalettes.Count > 0 ? fixedPalettes.Count : ptn.palsCount()));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                }
                #endregion
            }

            if (processSprites)
            {
                #region [new sprites processing]
                tileBank bank = new tileBank(charDataIdx >> 7);
                List<NGpal> sprPals = null;
                itemNum = 0;
                string palSource = "", keepID = "";
                bool keepLastPal;
                ndlist = node.SelectNodes("sprt");
                if (ndlist.Count > 0) externs += Environment.NewLine + "//animated sprites" + Environment.NewLine;

                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("## Sprites - " + ndlist.Count.ToString() + " items");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                foreach (XmlNode sprt in ndlist)
                {
                    List<NGFrame> NGFrames = new List<NGFrame>(0);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + ndlist.Count + ": ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    int tileCount = bank.tiles.Count;

                    //get file name & ID
                    try
                    {
                        fName = sprt.SelectSingleNode("file").InnerText;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    try
                    {
                        id = sprt.Attributes["id"].Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                        return;
                    }
                    Console.WriteLine(string.Format("{0} ({1})", id, fName));

                    //section
                    if (sprt.Attributes["section"] == null)
                        section = ".text";
                    else section = sprt.Attributes["section"].Value;
                    palData += ".section " + section + Environment.NewLine;
                    mapData += ".section " + section + Environment.NewLine;

                    //source of color data
                    if (sprt.Attributes["pal"] == null)
                        palSource = "";
                    else palSource = sprt.Attributes["pal"].Value.ToUpper();

                    //get flip modes
                    string flipModes = "";
                    if (sprt.Attributes["flips"] != null)
                        flipModes = sprt.Attributes["flips"].Value.ToUpper();
                    else
                        if ((tmpNode = sprt.SelectSingleNode("flips")) != null)
                        flipModes = tmpNode.InnerText.ToUpper();
                    flipX = flipModes.Contains('X');
                    flipY = flipModes.Contains('Y');
                    flipXY = flipModes.Contains('Z');

                    //check palette range settings
                    paletteRange palRange = new paletteRange();
                    try
                    {
                        string range = sprt.Attributes["palrange"].Value;
                        palRange.start = int.Parse(range.Split(',')[0].Trim());
                        palRange.size = int.Parse(range.Split(',')[1].Trim());
                        try
                        {
                            palRange.step = int.Parse(range.Split(',')[2].Trim());
                        }
                        catch (Exception)
                        {
                            palRange.step = 0;
                        }
                    }
                    catch (Exception)
                    {
                        palRange.start = 0;
                        palRange.size = 0x10000; // that's a lot of palettes
                        palRange.step = 0;
                    }

                    //parse options
                    bool optNoFlips = false, optNoAnim = false, optGlobalFrames = false, optForceSplit = false; ;
                    if (sprt.Attributes["opts"] != null)
                        foreach (string o in sprt.Attributes["opts"].Value.ToUpper().Split(','))
                            switch (o.Trim())
                            {
                                //don't generate pointers for flips
                                case "NOFLIPS":
                                    if (flipX == false && flipY == false && flipXY == false)
                                        optNoFlips = true;
                                    break;
                                //don't generate anim pointer / file include
                                case "NOANIM":
                                    optNoAnim = true;
                                    break;
                                //generate globals for each frame
                                case "GLOBALFRAMES":
                                    optGlobalFrames = true;
                                    break;
                                //force sprite splitting
                                case "FORCESPLIT":
                                    optForceSplit = true;
                                    break;
                            }

                    // check for autofiles
                    // (no support atm for sprites)
                    node1 = node2 = node3 = node4 = node5 = node6 = node7 = null;
                    autoAnim = 1;
                    node1 = sprt.SelectSingleNode("auto1"); node2 = sprt.SelectSingleNode("auto2"); node3 = sprt.SelectSingleNode("auto3"); node4 = sprt.SelectSingleNode("auto4"); node5 = sprt.SelectSingleNode("auto5"); node6 = sprt.SelectSingleNode("auto6"); node7 = sprt.SelectSingleNode("auto7");
                    if ((node1 != null) && (node2 != null) && (node3 != null))
                    {
                        if ((node4 != null) && (node5 != null) && (node6 != null) && (node7 != null))
                            autoAnim = 8;   //auto8
                        else autoAnim = 4;  //auto4
                    }

                    //load file(s)
                    //fixedPalettes = new List<palette>(0);
                    workBmp1 = workBmp2 = workBmp3 = workBmp4 = workBmp5 = workBmp6 = workBmp7 = null;
                    Console.Write("Loading file(s)... ");

                    //load main file
                    Bitmap mask = null;
                    workBmp = load32bppImage(fName);
                    Color chromaKey = Color.Fuchsia;

                    switch (palSource)
                    {
                        //from pal file
                        case "FILE":
                            keepID = id;
                            keepLastPal = false;
                            //TODO

                            //work out the palette name = "palette_" + spr_XXX
                            string pal_path = fName + "palette_" + id + ".png";
                            Bitmap palBmp = new Bitmap(pal_path);
                            palBmp.MakeTransparent(chromaKey = palBmp.GetPixel(0, 0));

                            //need to apply this chroma key here
                            workBmp.MakeTransparent(chromaKey);

                            //read color data
                            sprPals = new List<NGpal>(0);
                            for (int y = 0; y < palBmp.Height; y++)
                            {
                                sprPals.Add(new NGpal());
                                sprPals[y].addColor(Color.FromArgb(0, 0, 0, 0), true);
                                for (int x = 1; x < 16; x++)
                                {
                                    sprPals[y].addColor(palBmp.GetPixel(x, y));
                                }
                            }
                            break;

                        //keep previous pal data
                        case "KEEP":
                            keepLastPal = true;
                            //tmp quick fix
                            workBmp.MakeTransparent(Color.Fuchsia);
                            break;

                        //read from pixel strips
                        case "STRIPS":
                            keepID = id;
                            keepLastPal = false;
                            sprPals = new List<NGpal>(0);
                            workBmp.MakeTransparent(chromaKey = workBmp.GetPixel(0, 0));
                            sprPals.Add(new NGpal());
                            //pal 0
                            sprPals[0].addColor(Color.FromArgb(0, 0, 0, 0), true);
                            for (int x = 1; x < 16; x++)
                                sprPals[0].addColor(workBmp.GetPixel(x, 0));
                            //extra pals?
                            for (int y = 1; y < workBmp.Height; y++)
                            {
                                if (workBmp.GetPixel(0, y) == workBmp.GetPixel(1, y)) break;
                                sprPals.Add(new NGpal());
                                sprPals[y].addColor(Color.FromArgb(0, 0, 0, 0), true);
                                for (int x = 1; x < 16; x++)
                                    sprPals[y].addColor(workBmp.GetPixel(x, y));
                            }
                            break;

                        //default scan
                        case "SCAN":
                        default:
                            keepID = id;
                            keepLastPal = false;
                            sprPals = new List<NGpal>(0);
                            //need to mask out unused areas beforehand
                            mask = new Bitmap(workBmp.Width, workBmp.Height);
                            Graphics gfx = Graphics.FromImage(mask);
                            gfx.Clear(Color.Fuchsia);
                            tmpNodeList = sprt.SelectNodes("frame");
                            foreach (XmlNode frameNode in sprt.SelectNodes("frame"))
                            {
                                if (frameNode.Attributes["outline"] != null)
                                {//new framer file
                                    foreach (XmlNode sprNode in frameNode.SelectNodes("hwspr"))
                                    {
                                        string outline = sprNode.InnerText.Trim();
                                        gfx.FillRectangle(Brushes.Black,
                                            int.Parse(outline.Split(':')[0].Split(',')[0].Trim()),
                                            int.Parse(outline.Split(':')[0].Split(',')[1].Trim()),
                                            int.Parse(outline.Split(':')[1].Split(',')[0].Trim()),
                                            int.Parse(outline.Split(':')[1].Split(',')[1].Trim())
                                        );
                                    }
                                }
                                else
                                {//old framer file, single frames only
                                    string outline = frameNode.InnerText;
                                    gfx.FillRectangle(Brushes.Black,
                                            16 * int.Parse(outline.Split(':')[0].Split(',')[0].Trim()),
                                            16 * int.Parse(outline.Split(':')[0].Split(',')[1].Trim()),
                                            16 * int.Parse(outline.Split(':')[1].Split(',')[0].Trim()),
                                            16 * int.Parse(outline.Split(':')[1].Split(',')[1].Trim())
                                        );
                                }
                            }
                            gfx.Dispose();
                            mask.MakeTransparent(Color.Black);
                            gfx = Graphics.FromImage(workBmp);
                            //gfx.DrawImage(mask, 0, 0);
                            gfx.DrawImage(mask, 0, 0, mask.Width, mask.Height);
                            gfx.Dispose();
                            workBmp.MakeTransparent(Color.Fuchsia);
                            //mask.Save("mask.png");
                            //workBmp.Save("masked.png");

                            //scan palette
                            sprPals.Add(new NGpal());
                            sprPals[0].addColor(Color.FromArgb(0, 0, 0, 0), true);
                            workBmp.MakeTransparent(chromaKey);
                            for (int x = 0; x < workBmp.Width; x++)
                                for (int y = 0; y < workBmp.Height; y++)
                                    if (!sprPals[0].addColor(workBmp.GetPixel(x, y)))
                                    {
                                        Console.WriteLine(Environment.NewLine + "Color scan error: does not fit 1 palette.");
                                        Environment.Exit(1);
                                    }
                            break;
                    }

                    if (autoAnim >= 4)
                    {
                        (workBmp1 = load32bppImage(node1.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp2 = load32bppImage(node2.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp3 = load32bppImage(node3.InnerText, mask)).MakeTransparent(chromaKey);
                    }
                    if (autoAnim == 8)
                    {
                        (workBmp4 = load32bppImage(node4.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp5 = load32bppImage(node5.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp6 = load32bppImage(node6.InnerText, mask)).MakeTransparent(chromaKey);
                        (workBmp7 = load32bppImage(node7.InnerText, mask)).MakeTransparent(chromaKey);
                    }

                    //check dimensions
                    if (autoAnim >= 4)
                        if ((workBmp.PhysicalDimension != workBmp1.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp2.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp3.PhysicalDimension))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            Environment.Exit(1);
                        }
                    if (autoAnim == 8)
                        if ((workBmp.PhysicalDimension != workBmp4.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp5.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp6.PhysicalDimension) || (workBmp.PhysicalDimension != workBmp7.PhysicalDimension))
                        {
                            Console.WriteLine(Environment.NewLine + "Dimensions mismatch across files.");
                            Environment.Exit(1);
                        }
                    Console.WriteLine("OK");

                    //parse frames
                    tmpNodeList = sprt.SelectNodes("frame");
                    foreach (XmlNode f in tmpNodeList)
                    {
                        NGFrame frm = null;
                        string outline;
                        int factor = 1;
                        if (f.Attributes["outline"] != null)
                        {//new framer file
                            outline = f.Attributes["outline"].Value;
                            if (f.Attributes["customOutline"] != null)
                                outline = f.Attributes["customOutline"].Value;
                        }
                        else
                        {//old framer file, single frames only
                            outline = f.InnerText;
                            factor = 16;
                        }
                        frm = new NGFrame(
                            factor * int.Parse(outline.Split(':')[0].Split(',')[0].Trim()),
                            factor * int.Parse(outline.Split(':')[0].Split(',')[1].Trim()),
                            factor * int.Parse(outline.Split(':')[1].Split(',')[0].Trim()),
                            factor * int.Parse(outline.Split(':')[1].Split(',')[1].Trim())
                        );
                        XmlNodeList _sprs = f.SelectNodes("hwspr");
                        List<XmlNode> sprs = new List<XmlNode>(0);

                        //Undocumented, look for axis data and remove it from list
                        for (int i = 0; i < _sprs.Count; i++)
                        {
                            if ((int.Parse(_sprs[i].InnerText.Split(':')[1].Split(',')[0].Trim()) == 1) && (int.Parse(_sprs[i].InnerText.Split(':')[1].Split(',')[1].Trim()) == 1))
                            {
                                frm.offsetX = int.Parse(_sprs[i].InnerText.Split(':')[0].Split(',')[0].Trim());
                                frm.offsetY = int.Parse(_sprs[i].InnerText.Split(':')[0].Split(',')[1].Trim());
                            }
                            else sprs.Add(_sprs[i]);
                        }

                        switch (sprs.Count)
                        {
                            case 0: //copy outline
                                frm.sprites.Add(new NGspr(frm.outline.X, frm.outline.Y, frm.outline.Width, frm.outline.Height));
                                break;
                            case 1:
                                if (optForceSplit) goto _forcedSplit;
                                frm.sprites.Add(new NGspr(
                                    int.Parse(sprs[0].InnerText.Split(':')[0].Split(',')[0].Trim()),
                                    int.Parse(sprs[0].InnerText.Split(':')[0].Split(',')[1].Trim()),
                                    int.Parse(sprs[0].InnerText.Split(':')[1].Split(',')[0].Trim()),
                                    int.Parse(sprs[0].InnerText.Split(':')[1].Split(',')[1].Trim())
                                    ));
                                break;
                            default:
                            _forcedSplit:
                                foreach (XmlNode s in sprs)
                                {
                                    int sx = int.Parse(s.InnerText.Split(':')[0].Split(',')[0].Trim());
                                    int sy = int.Parse(s.InnerText.Split(':')[0].Split(',')[1].Trim());
                                    int sw = int.Parse(s.InnerText.Split(':')[1].Split(',')[0].Trim());
                                    int sh = int.Parse(s.InnerText.Split(':')[1].Split(',')[1].Trim());

                                    if (s.Attributes["relocate"] != null)
                                    {
                                        int rx = int.Parse(s.Attributes["relocate"].Value.Split(',')[0]);
                                        int ry = int.Parse(s.Attributes["relocate"].Value.Split(',')[1]);
                                        for (int w = 0; w < sw; w += 16)
                                            frm.sprites.Add(new NGspr(sx + w, sy, 16, sh, rx + w, ry));
                                    }
                                    else
                                    {
                                        for (int w = 0; w < sw; w += 16)
                                            frm.sprites.Add(new NGspr(sx + w, sy, 16, sh));
                                    }
                                }
                                break;
                        }
                        NGFrames.Add(frm);
                    } //foreach frame

                    //Process tiles
                    int maxWidth = 0;
                    foreach (NGFrame frame in NGFrames)
                    {
                        frame.pickTiles(ref workBmp, ref sprPals, ref palRange, ref bank);
                        palRange.start += palRange.step;
                        //compute maxWidth
                        if (frame.sprites.Count == 1)
                            maxWidth = frame.sprites[0].outline.Width / 16;
                        else maxWidth = frame.sprites.Count;    //this is wrong in some cases
                    }

                    //process
                    if (makeTexture) ptnTexture(ref ptn);

                    externs += string.Format("extern const spriteInfo {0};" + Environment.NewLine + "extern const paletteInfo {0}_Palettes;" + Environment.NewLine, id);
                    if (!optNoAnim)
                        externs += string.Format("#include \"" + incPrefix + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + ".h\"" + Environment.NewLine);

                    //export palettes data
                    if (!keepLastPal)
                    {
                        palData += string.Format(".globl {0}_Palettes" + Environment.NewLine + "{0}_Palettes:" + Environment.NewLine + "\t.word\t0x{1:x4} ;* {1} palettes" + Environment.NewLine, id, sprPals.Count);
                        foreach (NGpal p in sprPals)
                            palData += p.toAsm() + Environment.NewLine;
                        palData += Environment.NewLine;
                    }

                    //export map data
                    mapData += string.Format(".globl {0}" + Environment.NewLine + "{0}:" + Environment.NewLine, id);
                    mapData += string.Format("\t.word\t0x{0:x4}, 0x{1:x4}\t;*{0} frames, {1} max width" + Environment.NewLine, NGFrames.Count, maxWidth);
                    mapData += string.Format("\t.long\t{0}_Palettes" + Environment.NewLine, keepLastPal ? keepID : id);
                    if (!optNoAnim)
                        mapData += string.Format("\t.long\t{0}_animations\t;*ptr to anim data" + Environment.NewLine, id);
                    int fNum = 0;
                    string fInfo = "", fMap = "", globals = "";
                    foreach (NGFrame frame in NGFrames)
                    {
                        if (optGlobalFrames)
                        {
                            globals += string.Format(".globl\t{0}_{1:x4}" + Environment.NewLine, id, fNum);
                            externs += string.Format("extern const sprFrame {0}_{1:x4};" + Environment.NewLine, id, fNum);
                        }
                        frame.toAsm(ref fInfo, ref fMap, id, fNum++, flipX, flipY, flipXY, optNoFlips);
                    }
                    mapData += globals + fInfo + fMap;
                    if (optNoAnim)
                        mapData += Environment.NewLine;
                    else mapData += Environment.NewLine + ".include \"" + fName.Substring(0, fName.LastIndexOf("\\") + 1).Replace("\\", "/") + id + "/" + id + "_anims.s" + "\"" + Environment.NewLine + Environment.NewLine;

                    //cut frames for animator
                    if (NGFrames.Count > 0)
                    {
                        int fCount = 0;
                        Directory.CreateDirectory(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id);
                        foreach (NGFrame f in NGFrames)
                        {
                            try
                            {
                                if (!f.sprites[0].relocated)
                                    workBmp.Clone(f.outline, PixelFormat.Format32bppArgb).Save(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id + string.Format("\\({0:d4}) ", fCount) + id + string.Format("_{0:x4}.png", fCount));
                                else
                                {
                                    Bitmap bmpFrame = new Bitmap(f.outline.Width, f.outline.Height, PixelFormat.Format32bppArgb);
                                    Graphics g = Graphics.FromImage(bmpFrame);
                                    foreach (NGspr s in f.sprites)
                                    {
                                        g.DrawImage(workBmp, new Rectangle(s.relocateX, s.relocateY, s.outline.Width, s.outline.Height), new Rectangle(s.outline.X, s.outline.Y, s.outline.Width, s.outline.Height), GraphicsUnit.Pixel);
                                    }
                                    bmpFrame.Save(fName.Substring(0, fName.LastIndexOf("\\") + 1) + id + string.Format("\\({0:d4}) ", fCount) + id + string.Format("_{0:x4}.png", fCount));
                                    g.Dispose();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(Environment.NewLine + Environment.NewLine + "Error saving frame: " + e.Message);
                                return;
                            }
                            fCount++;
                        }
                    }

                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("DONE - [{0} tiles / {1} palettes]", bank.tiles.Count - tileCount, sprPals.Count));
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("");
                } //foreach sprt

                //rasterize & save tile data
                Parallel.ForEach(bank.tiles, tl =>
                {
                    tl.raster();
                });

                foreach (NGTile tl in bank.tiles)
                {
                    tl.rasterized.CopyTo(charData, charDataIdx);
                    charDataIdx += 128;
                }
                #endregion
            }

            Console.WriteLine(Environment.NewLine + "Processed tiles: " + (charDataIdx - (startBaseTile * 128)) / 128);
            Console.WriteLine("Next free tile: " + charDataIdx / 128);
            Console.Write("Saving...");

            //save text files
            File.WriteAllText(incFile, externs);
            File.WriteAllText(mapFile, mapData);
            File.WriteAllText(palFile, palData);

            //save char binary
            FileStream fs = File.Open(charFile, FileMode.Create);
            if (dummyFill)
                fs.Write(charData, 0, charDataIdx);
            else fs.Write(charData, startBaseTile * 128, charDataIdx - (startBaseTile * 128));
            fs.Close();
            Console.WriteLine(" OK.");

            //save texture
            if (makeTexture && (texturePage != null))
                texturePage.Save(textureFolder + string.Format("texture{0:d4}.png", (textureIdx - 1) / tilesPerTexture));

#if DEBUG
            Console.Read();
#endif
        }

        static bool fixProcess(XmlNode node)
        {
            //data
            string externs = "", palData = "", section = "";
            byte[] fixData = new byte[0x20000];
            List<palette> fixedPalettes = null;
            fixPattern ptn;

            //misc
            int bank, tileCount, bankCount;
            string fName, id;
            Bitmap fixBmp = null;

            //XmlNodeList importList = node.SelectNodes("import");
            foreach (XmlNode import in node.SelectNodes("import"))
            {
                try
                {
                    bank = int.Parse(import.Attributes["bank"].Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "File \"bank\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                try
                {
                    fName = import.SelectSingleNode("file").InnerText;
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                Console.WriteLine("Importing " + fName + "...");

                byte[] buffer = File.ReadAllBytes(fName);
                int idx = bank * 8192;

                if (buffer.Length + idx > 0x20000)
                {
                    Console.WriteLine("Import exceeds fix capacity");
                    return false;
                }
                if (buffer.Length % 32 != 0)
                {
                    Console.WriteLine("Import file isn't 32 bytes multiple.");
                    return false;
                }

                buffer.CopyTo(fixData, idx);
            }

            XmlNodeList fixList = node.SelectNodes("fix");

            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("## Fix data - " + fixList.Count.ToString() + " items");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            int itemNum = 0;
            foreach (XmlNode fix in fixList)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Environment.NewLine + "Processing " + ++itemNum + " of " + fixList.Count + ": ");
                Console.ForegroundColor = ConsoleColor.Gray;

                try
                {
                    bank = int.Parse(fix.Attributes["bank"].Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "File \"bank\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                try
                {
                    fName = fix.SelectSingleNode("file").InnerText;
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "\"<file>\" node not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                try
                {
                    id = fix.Attributes["id"].Value;
                }
                catch (Exception e)
                {
                    Console.WriteLine(Environment.NewLine + "File \"id\" attribute not found." + Environment.NewLine + "Exception: " + e.Message);
                    return false;
                }
                Console.WriteLine(string.Format("{0} ({1})", id, fName));

                //section
                if (fix.Attributes["section"] == null)
                    section = ".text";
                else section = fix.Attributes["section"].Value;
                palData += ".section " + section + Environment.NewLine;

                //load & check bitmap
                Console.Write("Loading file... ");
                fixedPalettes = new List<palette>(0);
                if (!loadBitmap(ref fixBmp, fName, ref fixedPalettes, true)) return false;
                if ((fixBmp.Width % 128 != 0) || (fixBmp.Height != 128))
                {
                    Console.WriteLine("Invalid image dimensions.");
                    return false;
                }
                tileCount = fixBmp.Width * 2;
                bankCount = tileCount / 256;

                if (bankCount + bank > 15)
                {
                    Console.WriteLine("Exceeding fix capacity.");
                    return false;
                }
                Console.WriteLine("OK");

                //process fix file
                ptn = new fixPattern(id, bank, ref fixBmp, ref fixedPalettes);
                ptn.rasterize(ref fixData);

                externs += string.Format("extern const paletteInfo {0}_Palettes;" + Environment.NewLine, id);
                palData += ptn.palData() + Environment.NewLine;
            }

            Console.Write("Saving...");

            //save text files
            File.WriteAllText(incFile, externs);
            File.WriteAllText(palFile, palData);
            //save binary
            File.WriteAllBytes(charFile, fixData);

            Console.WriteLine(" OK.");
#if DEBUG
            Console.Read();
#endif
            return true;
        }
    }
}

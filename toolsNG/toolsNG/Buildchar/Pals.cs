using NeoTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildchar
{
    public class NGpal
    {
        public List<Color> colors;
        public NGpal() { colors = new List<Color>(0); }

        public bool addColor(Color c, bool forceAdd = false)
        {
            if (forceAdd)
            {
                colors.Add(c);
                return true;
            }
            if (c.A != 255) return true;
            if (colors.Contains(c)) return true;
            if (colors.Count >= 16) return false;
            colors.Add(c);
            return true;
        }
        public string toAsm()
        {
            string data = "\t.word\t";

            for (int i = 0; i < colors.Count; i++)
                data += string.Format("0x{0:x4}{1}", ColorTool.NeoColor(colors[i].R, colors[i].G, colors[i].B), i != 15 ? ", " : "");
            for (int i = colors.Count; i < 16; i++)
                data += i != 15 ? "0x8000, " : "0x8000";
            return data;
        }
    }

    public class palette
    {
        public List<ushort> colors;
        public int location;

        public palette() { colors = new List<ushort>(0); }

        public bool addRGBColor(byte r, byte g, byte b)
        {
            ushort color = ColorTool.NeoColor(r, g, b);

            if (colors.Contains(color)) return true;
            if (colors.Count >= 15) return false;
            colors.Add(color);
            return true;
        }

        public bool contains(palette p)
        {
            if (p.colors.Count > colors.Count)
                return false;

            foreach (ushort c in p.colors)
                if (!colors.Contains(c))
                    return false;

            return true;
        }

        public int checkMerge(palette p)
        {
            int x = colors.Count;

            foreach (ushort c in p.colors)
                if (!colors.Contains(c))
                    x++;

            return x;
        }

        public void merge(palette p)
        {
            foreach (ushort c in p.colors)
                if (!colors.Contains(c))
                    colors.Add(c);
        }

        public string toAsm()
        {
            string data = "\t.word\t0x8000";

            for (int i = 0; i < colors.Count; i++)
                data += string.Format(", 0x{0:x4}", colors[i]);

            for (int i = colors.Count; i < 15; i++)
                data += ", 0x8000";

            return data;
        }
    }

    public class paletteRange
    {
        public int start;
        public int size;
        public int step;

        public paletteRange() { }
    }
}

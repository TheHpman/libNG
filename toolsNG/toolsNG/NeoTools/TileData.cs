using System;
using System.Collections.Generic;
using System.Text;

namespace NeoTools
{
    class TileData
    {
        public uint tileNumber;
        public uint paletteNumber;
        public uint streamPaletteNumber;
        public bool FlipX;
        public bool FlipY;
        public int autoAnim;

        public TileData()
        {
            tileNumber = 0;
            paletteNumber = 0;
            streamPaletteNumber = 0;
            FlipX = false;
            FlipY = false;
            autoAnim = 1;
        }

        public TileData(int tile)
        {
            tileNumber = (uint)tile;
            paletteNumber = 0;
            streamPaletteNumber = 0;
            FlipX = false;
            FlipY = false;
            autoAnim = 1;
        }

        public TileData(int tile, int palette, int auto = 1)
        {
            tileNumber = (uint)tile;
            paletteNumber = (uint)palette;
            streamPaletteNumber = 0;
            FlipX = false;
            FlipY = false;
            autoAnim = auto;
        }
              
    }
}

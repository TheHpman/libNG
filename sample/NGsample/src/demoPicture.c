/*
Picture demo program.

pictureDemo()
-------------
Demonstrates the use of picture object.

This is kinda a legacy object, in most cases it can be replaced with a
single frame animated sprite.
*/

#include <neogeo.h>
#include "externs.h"

const u8 sinTable[] = {32, 34, 35, 37, 38, 40, 41, 43, 44, 46, 47, 48, 50, 51, 52, 53,
		       55, 56, 57, 58, 59, 59, 60, 61, 62, 62, 63, 63, 63, 64, 64, 64,
		       64, 64, 64, 64, 63, 63, 63, 62, 62, 61, 60, 59, 59, 58, 57, 56,
		       55, 53, 52, 51, 50, 48, 47, 46, 44, 43, 41, 40, 38, 37, 35, 34,
		       32, 30, 29, 27, 26, 24, 23, 21, 20, 18, 17, 16, 14, 13, 12, 11,
		       9, 8, 7, 6, 5, 5, 4, 3, 2, 2, 1, 1, 1, 0, 0, 0,
		       0, 0, 0, 0, 1, 1, 1, 2, 2, 3, 4, 5, 5, 6, 7, 8,
		       9, 11, 12, 13, 14, 16, 17, 18, 20, 21, 23, 24, 26, 27, 29, 30};

void pictureDemo()
{
	s32 x = 94 + 48;
	s32 y = 54;
	s32 i, j;
	picture testPict;
	u16 raster = FALSE;
	u16 tableShift = 0;
	u16 rasterData0[512];
	u16 rasterData1[512];
	u16 rasterAddr;
	u16 *dataPtr;
	s16 displayedRasters;
	u16 flipMode = 0;

	clearFixLayer();
	backgroundColor = 0x7bbb;
	initGfx();
	jobMeterSetup(TRUE);

	loadTIirq(TI_MODE_SINGLE_DATA);

	pictureInit(&testPict, &terrypict, 1, 16, x, y, FLIP_NONE);
	palJobPut(16, terrypict.palInfo->count, terrypict.palInfo->data);

	rasterAddr = VRAM_POSX_ADDR(testPict.baseSprite);

	extern u16 mess_pictureDemo[];
	addMessage(mess_pictureDemo);

	while (SCClose(), waitVBlank(), 1)
	{
		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, terrypict.tileWidth);
			clearSprites(64, gradient.tileWidth);
			TInextTable = 0;
			SCClose();
			waitVBlank();
			unloadTIirq();
			return;
		}

		while (LSPC.MODE.lineCounter != 0x120)
			; // wait raster line 16
		jobMeterColor = NGCOLOR_BLUE;

		if (BIOS.P1.CURRENT.A)
		{
			if (BIOS.P1.EDGE.UP)
				flipMode |= FLIP_Y;
			else if (BIOS.P1.EDGE.DOWN)
				flipMode &= ~FLIP_Y;
			if (BIOS.P1.EDGE.RIGHT)
				flipMode |= FLIP_X;
			else if (BIOS.P1.EDGE.LEFT)
				flipMode &= ~FLIP_X;
		}
		else
		{
			if (BIOS.P1.CURRENT.UP)
				y--;
			else if (BIOS.P1.CURRENT.DOWN)
				y++;
			if (BIOS.P1.CURRENT.LEFT)
				x--;
			else if (BIOS.P1.CURRENT.RIGHT)
				x++;
		}
		if (BIOS.P1.EDGE.B)
			raster ^= 1;

		pictureSetFlip(&testPict, flipMode);
		pictureSetPos(&testPict, x, y);

		if (raster)
		{
			TInextTable = (TInextTable == rasterData0) ? rasterData1 : rasterData0;
			dataPtr = TInextTable;
			rasterAddr = VRAM_POSX_ADDR(testPict.baseSprite);

			if (BIOS.P1.CURRENT.C)
				for (vu16 x = 0; x < 5000; x++)
					; // induce frameskipping

			TIbase = TI_ZERO + (y > 0 ? 384 * y : 0); // timing to first line

			displayedRasters = (testPict.info->tileHeight << 4) - (y >= 0 ? 0 : 0 - y);
			if (displayedRasters + y > 224)
				displayedRasters = 224 - y;
			if (displayedRasters < 0)
				displayedRasters = 0;

			i = (y >= 0) ? 0 : 0 - y;
			for (j = 0; j < displayedRasters; i++, j++)
			{
				*dataPtr++ = rasterAddr;
				if (!(j & 0x1))
					*dataPtr++ = VRAM_POSX(x + (sinTable[(i + tableShift) & 0x3f] - 32));
				else
					*dataPtr++ = VRAM_POSX(x - (sinTable[(i + 1 + tableShift) & 0x3f] - 32));
			}
			SC234Put(rasterAddr, VRAM_POSX(x)); // restore pos
			*dataPtr++ = 0x0000;
			*dataPtr++ = 0x0000; // end
		}
		else
		{
			SC234Put(rasterAddr, VRAM_POSX(x)); // restore position
			TInextTable = 0;
		}

		tableShift++;
		jobMeterColor = NGCOLOR_GREEN;
	}
}

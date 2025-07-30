/*
Raster effect demonstration program.

Note:
- Emulation is typically 3 raster lines off from real hardware, adjust timers
depending on your test device. (timers here are for MAME)

rasterScrollDemo()
------------------
This demonstration uses two 512px width pixtures to simulate planes, using 
X position shifting raster effect.

desertRaster()
------------------
This demonstration simulates a heat effect, using Y position shifting raster
effect.
*/

#include <neogeo.h>
#include "externs.h"

// from messData.s
extern u16 mess_rasterScrollDemo[];

#define SCROLLSPEED 1.06

void rasterScrollDemo()
{
	pictureInfo frontLayerInfo, backLayerInfo;
	picture frontLayer, backLayer;
	s16 posY = -192;
	u16 rasterData0[256], rasterData1[256];
	u16 *rasterData;
	float scrollAcc;
	s32 scrollPos[34];
	s32 scrollValues[34];
	u16 backAddr = 0x8401, frontAddr = 0x8421;
	s32 x, y;
	s16 frontPosX[13], backPosX[13];
	u16 skipY;
	u16 firstLine;
	u16 zeroval;

	// layers were merged to save up tiles/palettes
	frontLayerInfo.stripSize = tf4layers.stripSize;
	backLayerInfo.stripSize = tf4layers.stripSize;
	frontLayerInfo.tileWidth = 32;
	backLayerInfo.tileWidth = 32;
	frontLayerInfo.tileHeight = tf4layers.tileHeight;
	backLayerInfo.tileHeight = tf4layers.tileHeight;
	// only using first map
	frontLayerInfo.maps[0] = tf4layers.maps[0];
	backLayerInfo.maps[0] = tf4layers.maps[0] + (tf4layers.stripSize * (32 / 2)); // bytesize but u16* declaration, so /2

	clearFixLayer();
	initGfx();
	jobMeterSetup(TRUE);
	loadTIirq(TI_MODE_DUAL_DATA);
	TInextTable = 0;

	scrollValues[0] = 1024;
	scrollPos[0] = 0;
	scrollAcc = 1024;
	for (x = 1; x < 34; x++)
	{
		scrollAcc *= SCROLLSPEED;
		scrollValues[x] = (s32)(scrollAcc + 0.5);
		scrollPos[x] = 0;
	}

	pictureInit(&backLayer, &backLayerInfo, 1, 16, 0, 0, FLIP_NONE);
	pictureInit(&frontLayer, &frontLayerInfo, 33, 16, 0, 0, FLIP_NONE);
	palJobPut(16, tf4layers.palInfo->count, tf4layers.palInfo->data);

	backgroundColor = 0x38db;
	zeroval = TI_ZERO;

	addMessage(mess_rasterScrollDemo);
	while (SCClose(), waitVBlank(), 1)
	{
		while (LSPC.MODE.lineCounter != 0x120)
			; // (debug) wait line 16
		jobMeterColor = NGCOLOR_BLUE;

		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, 64);
			TInextTable = 0;
			SCClose(), waitVBlank();
			unloadTIirq();
			return;
		}

		fixPrintf2(PRINTINFO(2, 7, 5, 3), "TIbase: %d (0x%04x)   ", zeroval, zeroval);

		if (BIOS.P1.CURRENT.A)
		{
			if (BIOS.P1.EDGE.UP)
				zeroval -= 384;
			else if (BIOS.P1.EDGE.DOWN)
				zeroval += 384;
			if (BIOS.P1.CURRENT.RIGHT)
				zeroval++;
			else if (BIOS.P1.CURRENT.LEFT)
				zeroval--;
		}
		else
		{
			if (BIOS.P1.CURRENT.UP)
			{
				if (posY < 0)
					posY++;
			}
			else if (BIOS.P1.CURRENT.DOWN)
			{
				if (posY > -288)
					posY--;
			}
		}

		// update scroll values
		for (x = 0; x < 34; x++)
			scrollPos[x] += scrollValues[x];
		frontPosX[0] = (s16)(0 - (scrollPos[32] >> 3));
		frontPosX[1] = frontPosX[2] = (s16)(0 - (scrollPos[24] >> 3));
		frontPosX[3] = frontPosX[4] = (s16)(0 - (scrollPos[16] >> 3));
		frontPosX[5] = (s16)(0 - (scrollPos[8] >> 3));
		frontPosX[6] = frontPosX[7] = frontPosX[8] = (s16)(0 - (scrollPos[0] >> 3));
		frontPosX[9] = frontPosX[10] = frontPosX[11] = (s16)(0 - (scrollPos[1] >> 3));
		frontPosX[12] = (s16)(0 - (scrollPos[32] >> 3));

		backPosX[0] = (s16)(0 - (scrollPos[24] >> 3));
		backPosX[1] = backPosX[2] = (s16)(0 - (scrollPos[16] >> 3));
		backPosX[3] = backPosX[4] = (s16)(0 - (scrollPos[8] >> 3));
		backPosX[5] = (s16)(0 - (scrollPos[0] >> 3));
		backPosX[6] = backPosX[7] = backPosX[8] = (s16)(0 - (scrollPos[0] >> 4));
		backPosX[9] = backPosX[10] = backPosX[11] = (s16)(0 - (scrollPos[0] >> 3));
		backPosX[12] = (s16)(0 - (scrollPos[1] >> 3));

		skipY = 0 - posY;
		x = skipY >> 5;
		firstLine = 32 - (skipY & 0x1f);

		// TIbase = TI_ZERO + (384 * firstLine); // timing to first raster line
		TIbase = zeroval + (384 * firstLine); // timing to first raster line
		TInextTable = (TInextTable == rasterData0) ? rasterData1 : rasterData0;
		rasterData = TInextTable;

		pictureSetPos(&frontLayer, frontPosX[x] >> 7, posY);
		pictureSetPos(&backLayer, backPosX[x] >> 7, posY);
		// might need to force the update if base scroll position didn't change
		SC234Put(frontAddr, frontPosX[x]);
		SC234Put(backAddr, backPosX[x]);

		if (skipY < 164)
		{			     // can we see water?
			TIreload = 384 * 32; // nope, 32px chunks
			for (x++; x < 13; x++)
			{
				*rasterData++ = frontAddr;
				*rasterData++ = frontPosX[x];
				*rasterData++ = backAddr;
				*rasterData++ = backPosX[x];
				firstLine += 32;
				if (firstLine >= 224)
					break;
			}
		}
		else
		{
			TIreload = 384 * 4; // yup, 4px chunks
			for (x++; x < 12; x++)
			{
				for (y = 0; y < 8; y++)
				{
					*rasterData++ = frontAddr;
					*rasterData++ = frontPosX[x];
					*rasterData++ = backAddr;
					*rasterData++ = backPosX[x];
				}
				firstLine += 32;
			}
			x = 1;
			while (firstLine < 224)
			{
				*rasterData++ = frontAddr;
				*rasterData++ = frontPosX[12];
				*rasterData++ = backAddr;
				*rasterData++ = 0 - (scrollPos[x++] >> 3);
				firstLine += 4;
			}
		}
		*rasterData++ = 0x0000;
		*rasterData++ = 0x0000;
		jobMeterColor = NGCOLOR_GREEN;
	}
}

#define DESERT_POSY 68
static const s16 heatTable[16] = {0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, -1, -1, -1, -1, -1};

void desertRaster(void)
{
	u32 fc, ticks = 0;
	u16 *dataPtr;
	u16 heatStartIndex = 0, heatIndex = 0;
	u16 rasterAddr, rasterData0[64], rasterData1[64];
	picture desertHandler;

	clearFixLayer();
	backgroundColor = 0x6bef;

	pictureInit(&desertHandler, &desert, 1, 16, 8, DESERT_POSY, FLIP_NONE);
	palJobPut(16, desert.palInfo->count, desert.palInfo->data);

	TIbase = TI_ZERO + DESERT_POSY * 384; // timing to first line
	TIreload = 384 * 4;		      // 4 lines intervals
	rasterAddr = VRAM_POSY_ADDR(desertHandler.baseSprite);

	loadTIirq(TI_MODE_SINGLE_DATA);

	while (fc = libNG_frameCounter, SCClose(), waitVBlank(), 1)
	{
		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, desert.tileWidth);
			TInextTable = 0;
			SCClose(), waitVBlank();
			unloadTIirq();
			return;
		}

		ticks = (fc ^ libNG_frameCounter) & libNG_frameCounter;
		if (ticks & 0x04)
		{
			heatIndex = heatStartIndex++;
			heatStartIndex &= 0xf;
			dataPtr = (TInextTable = (TInextTable == rasterData0) ? rasterData1 : rasterData0);

			for (u16 x = 0; x < 20; x++)
			{ // 80px pic/4px intervals = 20 iterations
				*dataPtr++ = rasterAddr;
				*dataPtr++ = VRAM_POSY(DESERT_POSY + heatTable[heatIndex++], SPR_UNSTICK, desertHandler.info->tileHeight);
				heatIndex &= 0xf;
			}
			// end marker
			*dataPtr++ = 0;
			*dataPtr++ = 0;
		}
	}
}
/*
Animated sprites demo program.

aSpriteDemo()
-------------
Demonstrates the use of multiple animated sprites and sprite pool rendering.

Retained option to use lagacy rendering for test/demo purposes, but pools are
the way to go in most sccenarios.

void portraitsDemo()
--------------------
Demonstrates use of hi-color objects.
*/

#include <neogeo.h>
#include "externs.h"

#define POOL_MODE
#define LOTS
#define SHADOW

void sortSprites(aSprite *list[], s32 count)
{
	// insertion sort
	s32 x, y;
	aSprite *tmp;

	for (x = 1; x < count; x++)
	{
		y = x;
		while (y > 0 && (list[y]->posY < list[y - 1]->posY))
		{
			tmp = list[y];
			list[y] = list[y - 1];
			list[y - 1] = tmp;
			y--;
		}
	}
}

void aSpriteDemo()
{
	s32 x = 32;
	s32 y = 160;
	s32 relX, relY;
	s16 showdebug = FALSE;
	aSprite demoSpr, demoSpr2, demoSpr3;
	u16 *data;
	u16 flipMode = 0, anim = 0;
#ifdef POOL_MODE
	spritePool testPool;
	u32 *drawTable[16];
	u32 *drawTablePtr;
	s32 sortSize;
#ifdef LOTS
	aSprite demoSpr4, demoSpr5, demoSpr6, demoSpr7, demoSpr8, demoSpr9, demoSprA;
#endif
#endif
	s16 way1 = JOY_UP, way2 = JOY_UP;
	u8 colorToggle = 0;

#ifdef SHADOW
	aSprite shadow;
	aSpriteInit(&shadow, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 + 32, 146, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	shadow.flag_noAnim = 1; // disable animation
	shadow.basePalette = 100;
	// clear palette #100
	u32 *clr = (u32 *)&PALRAM.pals[100].color[0];
	for (u16 x = 0; x < 8; x++)
		*clr++ = 0x80008000;

#endif

	clearFixLayer();
	backgroundColor = 0x7bbb;
	jobMeterSetup(TRUE);

	extern u16 mess_aspriteDemo[];
	addMessage(mess_aspriteDemo);

#ifdef POOL_MODE
	aSpriteInit(&demoSpr, &bmary_spr, AS_USE_SPRITEPOOL, 16, x, y, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	// aSpriteInit(&demoSpr, &bmary_spr, AS_USE_SPRITEPOOL, 16, x, y, BMARY_SPR_ANIM_NEWFMT, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr2, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 - 16, y, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr3, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 + 16, y, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
#ifdef LOTS
	aSpriteInit(&demoSpr4, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 + 32, 146, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr5, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 - 32, 156, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr6, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 + 48, 166, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr7, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 - 48, 176, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr8, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 + 10, 186, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr9, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 - 10, 196, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSprA, &bmary_spr, AS_USE_SPRITEPOOL, 16, 87, 206, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
#endif
#else
	aSpriteInit(&demoSpr, &bmary_spr, 1, 16, x, y, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr2, &bmary_spr, 5, 16, 160 - 16, y, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr3, &bmary_spr, 9, 16, 160 + 16, y, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
#endif

	palJobPut(16, bmary_spr.palInfo->count, &bmary_spr.palInfo->data);

	data = dbgTags.maps[0];
	palJobPut(200, dbgTags.palInfo->count, &dbgTags.palInfo->data);
	SC234Put(VRAM_POSX_ADDR(200), VRAM_POSX(0));
	SC234Put(VRAM_POSY_ADDR(200), VRAM_POSY(224, SPR_UNSTICK, 0));
	SC234Put(VRAM_SPR_ADDR(200), data[4 << 1]);
	SC234Put(VRAM_SPR_ADDR(200) + 1, 200 << 8);
	SC234Put(VRAM_POSX_ADDR(201), VRAM_POSX(0));
	SC234Put(VRAM_POSY_ADDR(201), VRAM_POSY(224, SPR_UNSTICK, 0));
	SC234Put(VRAM_SPR_ADDR(201), data[40 << 1]);
	SC234Put(VRAM_SPR_ADDR(201) + 1, 200 << 8);
	SC234Put(VRAM_POSX_ADDR(202), VRAM_POSX(0));
	SC234Put(VRAM_POSY_ADDR(202), VRAM_POSY(224, SPR_UNSTICK, 0));
	SC234Put(VRAM_SPR_ADDR(202), data[40 << 1]);
	SC234Put(VRAM_SPR_ADDR(202) + 1, (200 << 8) | FLIP_X);
	SC234Put(VRAM_POSX_ADDR(203), VRAM_POSX(0));
	SC234Put(VRAM_POSY_ADDR(203), VRAM_POSY(224, SPR_UNSTICK, 0));
	SC234Put(VRAM_SPR_ADDR(203), data[40 << 1]);
	SC234Put(VRAM_SPR_ADDR(203) + 1, (200 << 8) | FLIP_Y);
	SC234Put(VRAM_POSX_ADDR(204), VRAM_POSX(0));
	SC234Put(VRAM_POSY_ADDR(204), VRAM_POSY(224, SPR_UNSTICK, 0));
	SC234Put(VRAM_SPR_ADDR(204), data[40 << 1]);
	SC234Put(VRAM_SPR_ADDR(204) + 1, (200 << 8) | FLIP_XY);

#ifdef POOL_MODE
	// spritePoolInit(&testPool, 10, 60 /*50*/ /*80*/, TRUE);
	spritePoolInit(&testPool, 10, 180, TRUE);
	drawTablePtr = (u32 *)drawTable;
	*drawTablePtr++ = 0;
#ifdef SHADOW
	*drawTablePtr++ = (u32)&shadow;
#endif
	*drawTablePtr++ = (u32)&demoSpr;
	demoSpr.Xbig = 0x78;
	demoSpr.Ybig = 0x78;
	// demoSpr.XYbig = 0xffff;
	*drawTablePtr++ = (u32)&demoSpr2;
	*drawTablePtr++ = (u32)&demoSpr3;
	sortSize = 3;
#ifdef LOTS
	*drawTablePtr++ = (u32)&demoSpr4;
	*drawTablePtr++ = (u32)&demoSpr5;
	*drawTablePtr++ = (u32)&demoSpr6;
	*drawTablePtr++ = (u32)&demoSpr7;
	*drawTablePtr++ = (u32)&demoSpr8;
	*drawTablePtr++ = (u32)&demoSpr9;
	*drawTablePtr++ = (u32)&demoSprA;
	sortSize = 10;
#endif
	*drawTablePtr = 0;

#ifdef SHADOW
	sortSize++;
#endif

	// sortSprites((aSprite **)&drawTable[1], sortSize);
	// spritePoolDrawList(&testPool, &drawTable[1]);
	// spritePoolClose(&testPool);
#else
	aSpriteAnimate(&demoSpr);
	aSpriteAnimate(&demoSpr2);
	aSpriteAnimate(&demoSpr3);
#endif

	// color math
	// scratchpad just has enough room
	cMathPalEffect((u16 *)bmary_spr.palInfo->data, (u16 *)libNG_scratchpad64, CMATH_EFFECT(CMATH_EFFECT_ADD_HALF, 1), CMATH_COLOR(31, 0, 0));
	// cMathPalEffect((u16 *)bmary_spr.palInfo->data, (u16 *)libNG_scratchpad64, CMATH_EFFECT(CMATH_EFFECT_XOR, 1), CMATH_COLOR(0x1f, 0x1f, 0x1f));
	cMathPalEffect((u16 *)bmary_spr.palInfo->data, (u16 *)&libNG_scratchpad64[32], CMATH_EFFECT(CMATH_EFFECT_ADD_HALF, 1), CMATH_COLOR(0, 31, 0));
	palJobPut(101, 2, libNG_scratchpad64);

	while (SCClose(), waitVBlank(), 1)
	{
		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, 210);
			// restoring shrink values
			for (u16 x = 0x8000 + 1; x <= 0x8000 + 384; x++)
			{
				SC234Put(x, 0xfff);
			}
			SCClose();
			waitVBlank();
			return;
		}

		while (LSPC.MODE.lineCounter != 0x120)
			; // (debug) wait raster line 16
		jobMeterColor = NGCOLOR_BLUE;

		if (BIOS.P1.CURRENT.A)
		{
			if (BIOS.P1.EDGE.DOWN)
				flipMode |= FLIP_Y;
			else if (BIOS.P1.EDGE.UP)
				flipMode &= ~FLIP_Y;
			if (BIOS.P1.EDGE.LEFT)
				flipMode |= FLIP_X;
			else if (BIOS.P1.EDGE.RIGHT)
				flipMode &= ~FLIP_X;
#ifdef POOL_MODE
			demoSpr.currentFlip = flipMode;
#else
			aSpriteSetFlip(&demoSpr, flipMode);
#endif
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

			if (BIOS.P1.EDGE.B)
				aSpriteSetAnim(&demoSpr, anim ^= 1);
			if (BIOS.P1.EDGE.C)
			{
				demoSpr.flags ^= AS_FLAG_STRICT_COORDS;
				demoSpr.flags |= AS_FLAG_FLIPPED;
			}
			// if (BIOS.P1.EDGE.D)
			//	aSpriteSetAnim(&demoSpr, 2);

			if (BIOS.P1.EDGE.D)
			{
				if (showdebug)
				{
					// move debug stuff offscreen
					SC234Put(VRAM_POSY_ADDR(200), VRAM_POSY(224, SPR_UNSTICK, 0));
					SC234Put(VRAM_POSY_ADDR(201), VRAM_POSY(224, SPR_UNSTICK, 0));
					SC234Put(VRAM_POSY_ADDR(202), VRAM_POSY(224, SPR_UNSTICK, 0));
					SC234Put(VRAM_POSY_ADDR(203), VRAM_POSY(224, SPR_UNSTICK, 0));
					SC234Put(VRAM_POSY_ADDR(204), VRAM_POSY(224, SPR_UNSTICK, 0));
					fixJobPut(0, 25, FIX_LINE_WRITE, 0, _fixBlankLine);
					fixJobPut(0, 26, FIX_LINE_WRITE, 0, _fixBlankLine);
					fixJobPut(0, 27, FIX_LINE_WRITE, 0, _fixBlankLine);
					fixJobPut(0, 28, FIX_LINE_WRITE, 0, _fixBlankLine);
					fixJobPut(0, 29, FIX_LINE_WRITE, 0, _fixBlankLine);
				}
				showdebug ^= 1;
			}
		}

#ifdef POOL_MODE
		demoSpr.posX = x;
		demoSpr.posY = y;
#else
		aSpriteSetPos(&demoSpr, x, y);
#endif

		if (way1 == JOY_UP)
		{
#ifdef POOL_MODE
			demoSpr2.posY += 2;
#else
			aSpriteMove(&demoSpr2, 0, 2);
#endif
			if (demoSpr2.posY > 220)
				way1 = JOY_DOWN;
		}
		else
		{
#ifdef POOL_MODE
			demoSpr2.posY -= 2;
#else
			aSpriteMove(&demoSpr2, 0, -2);
#endif
			if (demoSpr2.posY < 90)
				way1 = JOY_UP;
		}
		if (way2 == JOY_UP)
		{
#ifdef POOL_MODE
			demoSpr3.posY++;
#else
			aSpriteMove(&demoSpr3, 0, 1);
#endif
			if (demoSpr3.posY > 220)
				way2 = JOY_DOWN;
		}
		else
		{
#ifdef POOL_MODE
			demoSpr3.posY--;
#else
			aSpriteMove(&demoSpr3, 0, -1);
#endif
			if (demoSpr3.posY < 90)
				way2 = JOY_UP;
		}

		// cMath effect
		if (BIOS.P2.EDGE.A)
			colorToggle ^= 1;
		demoSpr2.basePalette = libNG_frameCounter & colorToggle ? 101 : 16;
		demoSpr3.basePalette = libNG_frameCounter & colorToggle ? 102 : 16;

#ifdef POOL_MODE
		sortSprites((aSprite **)&drawTable[1], sortSize);
		jobMeterColor = NGCOLOR_PINK;

		if (BIOS.P2.CURRENT.UP)
			demoSpr.Ybig++;
		else if (BIOS.P2.CURRENT.DOWN)
			demoSpr.Ybig--;
		if (BIOS.P2.CURRENT.LEFT)
			demoSpr.Xbig--;
		else if (BIOS.P2.CURRENT.RIGHT)
			demoSpr.Xbig++;

#ifdef SHADOW
		// need to get next step & frame for shadow to always match
		shadow.currentFrame = (shadow.currentStep = aSpriteGetNextStep(&demoSpr))->frame;
		shadow.posX = demoSpr.posX;
		shadow.posY = demoSpr.posY;
		shadow.currentFlip = demoSpr.currentFlip ^ FLIP_Y;
		shadow.Xbig = demoSpr.Xbig;
		shadow.Ybig = demoSpr.Ybig >> 2;
#endif

		spritePoolDrawList3(&testPool, testPool.way == WAY_UP ? (void *)&drawTable[1] : (void *)drawTablePtr);

#else
		aSpriteAnimate(&demoSpr);
		aSpriteAnimate(&demoSpr2);
		aSpriteAnimate(&demoSpr3);
#endif

		// aSprite debug info
		if (showdebug)
		{
			jobMeterColor = NGCOLOR_BLACK;
			if (!(demoSpr.flags & AS_FLAG_STRICT_COORDS))
			{
				if (demoSpr.currentFlip & FLIP_X)
					relX = x - ((demoSpr.currentFrame->tileWidth << 4) + demoSpr.currentStep->shiftX) + 1;
				else
					relX = x + demoSpr.currentStep->shiftX;
				if (demoSpr.currentFlip & FLIP_Y)
					relY = y - ((demoSpr.currentFrame->tileHeight << 4) + demoSpr.currentStep->shiftY) + 1;
				else
					relY = y + demoSpr.currentStep->shiftY;
			}
			else
			{
				relX = demoSpr.posX;
				relY = demoSpr.posY;
			}
			SC234Put(VRAM_POSX_ADDR(200), VRAM_POSX(x - 3));
			SC234Put(VRAM_POSY_ADDR(200), VRAM_POSY(y - 3, SPR_UNSTICK, 1));
			SC234Put(VRAM_POSX_ADDR(201), VRAM_POSX(relX));
			SC234Put(VRAM_POSY_ADDR(201), VRAM_POSY(relY, SPR_UNSTICK, 1));
			SC234Put(VRAM_POSX_ADDR(202), VRAM_POSX(relX + ((demoSpr.currentFrame->tileWidth - 1) << 4)));
			SC234Put(VRAM_POSY_ADDR(202), VRAM_POSY(relY, SPR_UNSTICK, 1));
			SC234Put(VRAM_POSX_ADDR(203), VRAM_POSX(relX));
			SC234Put(VRAM_POSY_ADDR(203), VRAM_POSY(relY + ((demoSpr.currentFrame->tileHeight - 1) << 4), SPR_UNSTICK, 1));
			SC234Put(VRAM_POSX_ADDR(204), VRAM_POSX(relX + ((demoSpr.currentFrame->tileWidth - 1) << 4)));
			SC234Put(VRAM_POSY_ADDR(204), VRAM_POSY(relY + ((demoSpr.currentFrame->tileHeight - 1) << 4), SPR_UNSTICK, 1));

			// debug live update prints = 1 frame ahead. meh.
			jobMeterColor = NGCOLOR_GREY;
			fixPrintf1(PRINTINFO(3, 25, 2, 3), "Anim data: A:%02d S:%02d R:%02d   ", demoSpr.currentAnim, demoSpr.stepNum, demoSpr.repeats);
			fixPrintf1(PRINTINFO(3, 26, 2, 3), "Step data: Frame:0x%06x", bmary_spr.anims[demoSpr.currentAnim][demoSpr.stepNum].frame);
			fixPrintf1(PRINTINFO(14, 27, 2, 3), "SX:%04d SY:%04d D:%02d",
				   bmary_spr.anims[demoSpr.currentAnim][demoSpr.stepNum].shiftX,
				   bmary_spr.anims[demoSpr.currentAnim][demoSpr.stepNum].shiftY,
				   bmary_spr.anims[demoSpr.currentAnim][demoSpr.stepNum].duration);
			fixPrintf1(PRINTINFO(2, 28, 2, 3), "Frame data: W:%02d H:%02d TMAP:0x%06x",
				   ((sprFrame *)(bmary_spr.anims[demoSpr.currentAnim][demoSpr.stepNum].frame))->tileWidth,
				   ((sprFrame *)(bmary_spr.anims[demoSpr.currentAnim][demoSpr.stepNum].frame))->tileHeight,
				   ((sprFrame *)(bmary_spr.anims[demoSpr.currentAnim][demoSpr.stepNum].frame))->maps[demoSpr.currentFlip]);
			fixPrintf1(PRINTINFO(5, 29, 2, 3), "Scaling: Xbig:%02x Ybig:%02x", demoSpr.Xbig, demoSpr.Ybig);
		}

#ifdef POOL_MODE
		jobMeterColor = NGCOLOR_YELLOW;
		spritePoolClose(&testPool);
#endif

		jobMeterColor = NGCOLOR_GREEN;
		fixPrintf1(PRINTINFO(0, 1, 0, 0), "%d\xff\xff\xff", demoSpr.baseSprite);
	}
}

// ################################################################

static const u16 portraitPositions[] = {80, 160, 240};

void portraitsDemo()
{
	aSprite portrait;
	spritePool pool;

	// draw list setup
	u32 *drawTable[4];
	u32 *drawTablePtr = (u32 *)drawTable;
	*drawTablePtr++ = 0;
	*drawTablePtr++ = (u32)&portrait;
	*drawTablePtr++ = 0;

	clearFixLayer();
	backgroundColor = NGCOLOR_BLACK;
	jobMeterSetup(TRUE);

	// pool & spr setup
	spritePoolInit(&pool, 10, 100, TRUE);
	aSpriteInit(&portrait, &portraits_spr, AS_USE_SPRITEPOOL, 16, 160, 200, PORTRAITS_SPR_ANIM_KULA, FLIP_NONE, AS_FLAGS_DEFAULT);

	u16 demoStep = 0;
	u16 demoWait = 0;
	u16 demoPosIdx = 0;
	u16 demoChar = 0;

	while (SCClose(), waitVBlank(), 1)
	{
		while (LSPC.MODE.lineCounter != 0x120)
			; // (debug) wait raster line 16
		jobMeterColor = NGCOLOR_BLUE;

		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(10, 100);
			return;
		}

		/*
		if(BIOS.P1.EDGE.A)
		{
			aSpriteSetAnim(&portrait, PORTRAITS_SPR_ANIM_KULA);
			cMathLoadPalette(16, 2, FADE_RESET, &portraits_spr.palInfo->data[0]);
		}
		else if(BIOS.P1.EDGE.B)
		{
			aSpriteSetAnim(&portrait, PORTRAITS_SPR_ANIM_LEONA);
			cMathLoadPalette(16, 2, FADE_RESET, &portraits_spr.palInfo->data[2]);
		}
		if(BIOS.P1.EDGE.C)
			portrait.currentFlip = (portrait.currentFlip + 1) & 3;
		*/

		if (demoWait)
			demoWait--;
		else
			switch (demoStep)
			{
			case 0:
				portrait.posX = portraitPositions[demoPosIdx];
				aSpriteSetAnim(&portrait, demoChar);
				cMathLoadPalette(16, 2, FADE_FILL | FADE_WHITE, &portraits_spr.palInfo->data[demoChar << 1]);
				demoStep++;
				if (++demoPosIdx == 3)
					demoPosIdx = 0;
				demoChar ^= 1;
				break;
			case 1:
				cMathSetCommand(16, 2, FADE_RESET);
				demoWait = 4 - 1;
				demoStep++;
				break;
			case 2:
				cMathSetCommand(16, 2, FADE_TO | FADE_BLACK | FADE_SPEED3);
				demoStep++;
				break;
			case 3:
				if (!palCommandPending)
					demoStep = 0;
				break;
			}

		jobMeterColor = NGCOLOR_GREY;
		spritePoolDrawList3(&pool, pool.way == WAY_UP ? (void *)&drawTable[1] : (void *)&drawTable[2]);
		jobMeterColor = NGCOLOR_LIGHTGREY;
		spritePoolClose(&pool);
		jobMeterColor = NGCOLOR_GREEN;
	}
}

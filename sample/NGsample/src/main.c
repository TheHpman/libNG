#include <neogeo.h>
#include "externs.h"

#define FRONT_START_X 157
#define FRONT_START_Y 24
#define FRONT_MIN_X 8
#define FRONT_MAX_X 307
#define FRONT_MIN_Y 16
#define FRONT_MAX_Y 24

#define BACK_MIN_X 8
#define BACK_MAX_X 149
#define BACK_MIN_Y 5
#define BACK_MAX_Y 8


typedef struct bkp_ram_info
{
	union
	{
		u16 debugDips;
		struct
		{
			u8 debugDip1;
			u8 debugDip2;
		};
		struct
		{
			u8 debugDip1_1 : 1;
			u8 debugDip1_2 : 1;
			u8 debugDip1_3 : 1;
			u8 debugDip1_4 : 1;
			u8 debugDip1_5 : 1;
			u8 debugDip1_6 : 1;
			u8 debugDip1_7 : 1;
			u8 debugDip1_8 : 1;

			u8 debugDip2_1 : 1;
			u8 debugDip2_2 : 1;
			u8 debugDip2_3 : 1;
			u8 debugDip2_4 : 1;
			u8 debugDip2_5 : 1;
			u8 debugDip2_6 : 1;
			u8 debugDip2_7 : 1;
			u8 debugDip2_8 : 1;
		};
	};
	// first 2 bytes are always reserved for debug dips
	u8 stuff[254];
	// 256 bytes
} bkp_ram_info;

extern bkp_ram_info bkp_data;	// defined in linker file
extern u32 _bend;

u32 callBackCounter;

// fix palettes for text
static const u16 fixPalettes[] = {
    0x8000, 0xefb8, 0x0222, 0x5fa7, 0xde85, 0x2c74, 0x2a52, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xebea, 0x0041, 0xa9d8, 0x57c7, 0xf6b5, 0x43a4, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0x014c, 0x9113, 0xb15e, 0x317f, 0x119f, 0x11af, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xeb21, 0x0111, 0xee21, 0x6f31, 0x6f51, 0x6f61, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xed31, 0xc311, 0xee51, 0x4f81, 0x4fa1, 0x4fc1, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xbad3, 0x0111, 0x09c0, 0xe7b0, 0xc580, 0xe250, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xefb8, 0x0111, 0xde96, 0x3c75, 0x2950, 0x4720, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0x8444, 0x0111, 0xf555, 0xf666, 0x7777, 0x8888, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000};

#define CLIPPING 5

void scrollerInitClipped(scroller *s, scrollerInfo *si, u16 baseSprite, u16 basePalette, s16 posX, s16 posY, u16 clipping)
{
	u16 i, addr, pos;

	scrollerInit(s, si, baseSprite, basePalette, posX, posY);
	addr = VRAM_POSY_ADDR(baseSprite);
	pos = VRAM_POSY(-posY, SPR_UNSTICK, clipping);
	for (i = 0; i < 21; i++)
		SC234Put(addr++, pos);
}

void scrollerSetPosClipped(scroller *s, s16 toX, s16 toY, u16 clipping)
{
	u16 i, addr, pos;

	if (s->scrlPosY != toY)
	{ // Y moved ?
		scrollerSetPos(s, toX, toY);

		addr = VRAM_POSY_ADDR(s->baseSprite);
		pos = VRAM_POSY(-toY, SPR_UNSTICK, clipping);
		for (i = 0; i < 21; i++)
			SC234Put(addr++, pos);
	}
	else
		scrollerSetPos(s, toX, toY);
}

void scrollerDemo()
{
	s32 x = FRONT_START_X;
	s32 y = FRONT_START_Y;
	s32 carx = 320;
	s32 cary = 106;
	s32 backX;
	s32 backY;
	u16 palIdx = 16;

	scroller backScroll, frontScroll;
	picture car;

	backgroundColor = 0x7bbb;
	LSPCmode = 0x1c00; // autoanim speed
	clearFixLayer();
	jobMeterSetup(TRUE);

	scrollerInit(&backScroll, &ffbg_a, 1, palIdx, (((x - 8) * 141) / 299) + BACK_MIN_X, (((y - 16) * 3) / 8) + BACK_MIN_Y);
	// setBank(&ffbg_a);
	// this section valid because scrollerInit did the banking for us
	palJobPut(palIdx, ffbg_a.palInfo->count, ffbg_a.palInfo->data);
	palIdx += ffbg_a.palInfo->count;

	scrollerInit(&frontScroll, &ffbg_b, 22, palIdx, x, y);
	// scrollerInitClipped(&frontScroll, &ffbg_b, 22, palIdx, x, y, CLIPPING);
	// setBank(&ffbg_b);
	palJobPut(palIdx, ffbg_b.palInfo->count, ffbg_b.palInfo->data);
	palIdx += ffbg_b.palInfo->count;

	pictureInit(&car, &ffbg_c, 43, palIdx, carx, cary, FLIP_NONE);
	// setBank(&ffbg_c);
	palJobPut(palIdx, ffbg_c.palInfo->count, ffbg_c.palInfo->data);

	fixPrint(PRINTINFO(2, 3, 4, 3), "1P \x12\x13\x10\x11: scroll");

	SCClose();
	while (1)
	{
		SCClose();
		waitVBlank();

		while ((LSPC.MODE >> 7) != 0x120)
			; // (debug) wait raster line 16
		jobMeterColor = NGCOLOR_PURPLE;

		if (BIOS.START_EDGE.P1_START)
		{
			clearSprites(1, 42 + ffbg_c.tileWidth);
			SCClose();
			waitVBlank();
			return;
		}

		if (BIOS.P1.CURRENT.UP)
			y--;
		else if (BIOS.P1.CURRENT.DOWN)
			y++;
		if (BIOS.P1.CURRENT.LEFT)
			x--;
		else if (BIOS.P1.CURRENT.RIGHT)
			x++;

		if (x < FRONT_MIN_X)
			x = FRONT_MIN_X;
		else if (x > FRONT_MAX_X)
			x = FRONT_MAX_X;
		if (y < FRONT_MIN_Y)
			y = FRONT_MIN_Y;
		else if (y > FRONT_MAX_Y)
			y = FRONT_MAX_Y;

		if (x > 161)
		{
			cary = 106 + (24 - y);
			pictureSetPos(&car, 320 - (x - 161), cary);
		}
		else
			pictureSetPos(&car, 320, cary);

		backX = (((x - 8) * 141) / 299) + BACK_MIN_X;
		backY = (((y - 16) * 3) / 8) + BACK_MIN_Y;

		jobMeterColor = NGCOLOR_BLUE;
		scrollerSetPos(&frontScroll, x, y);
		// scrollerSetPosClipped(&frontScroll, x, y, CLIPPING);
		scrollerSetPos(&backScroll, backX, backY);
		jobMeterColor = NGCOLOR_GREEN;
	}
}

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

#define POOL_MODE
#define LOTS
#define SHADOW

void aSpriteDemo()
{
	s32 x = 87;
	s32 y = 136;
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

#ifdef SHADOW
	aSprite shadow;
	aSpriteInit(&shadow, &bmary_spr, AS_USE_SPRITEPOOL, 16, 160 + 32, 146, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	shadow.flag_noAnim = 1;	//disable animation
	shadow.basePalette = 100;
#endif

	clearFixLayer();
	backgroundColor = 0x7bbb;
	jobMeterSetup(TRUE);

#ifdef POOL_MODE
	// aSpriteInit(&demoSpr, &bmary_spr, AS_USE_SPRITEPOOL, 16, x, y, BMARY_SPR_ANIM_IDLE, FLIP_NONE, AS_FLAGS_DEFAULT);
	aSpriteInit(&demoSpr, &bmary_spr, AS_USE_SPRITEPOOL, 16, x, y, BMARY_SPR_ANIM_NEWFMT, FLIP_NONE, AS_FLAGS_DEFAULT);
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

	fixPrint(PRINTINFO(2, 3, 4, 3), "1P \x12\x13\x10\x11: move sprite");
	fixPrint(PRINTINFO(2, 4, 4, 3), "1P A+\x12\x13\x10\x11: flip mode");
	fixPrint(PRINTINFO(2, 5, 4, 3), "1P B/C/D: toggle animation");
	fixPrint(PRINTINFO(12, 6, 4, 3), "/coords mode/debug");

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

	while (1)
	{
		SCClose();
		waitVBlank();

		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, 150);
			clearSprites(200, 5);
			// restoring shrink values
			for (u16 x = 0x8000 + 1; x <= 0x8000 + 384; x++)
			{
				SC234Put(x, 0xfff);
			}
			SCClose();
			waitVBlank();
			return;
		}

		while ((LSPC.MODE >> 7) != 0x120)
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
			aSpriteSetFlip(&demoSpr, flipMode);
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

		// if (BIOS_P2_CURRENT.A)
		//	aSpriteSetStep(&demoSpr, 3);
		// if (BIOS_P2_CURRENT.B)
		//	aSpriteSetStep2(&demoSpr, 3);
		// if (BIOS_P2_CURRENT.C)
		//	aSpriteSetAnimStep(&demoSpr, 0, 3);
		// if (BIOS_P2_CURRENT.D)
		//	aSpriteSetAnimStep2(&demoSpr, 0, 3);

		aSpriteSetPos(&demoSpr, x, y);

		if (way1 == JOY_UP)
		{
			aSpriteMove(&demoSpr2, 0, 2);
			if (demoSpr2.posY > 220)
				way1 = JOY_DOWN;
		}
		else
		{
			aSpriteMove(&demoSpr2, 0, -2);
			if (demoSpr2.posY < 90)
				way1 = JOY_UP;
		}
		if (way2 == JOY_UP)
		{
			aSpriteMove(&demoSpr3, 0, 1);
			if (demoSpr3.posY > 220)
				way2 = JOY_DOWN;
		}
		else
		{
			aSpriteMove(&demoSpr3, 0, -1);
			if (demoSpr3.posY < 90)
				way2 = JOY_UP;
		}

		// demoSpr2.Xbig = demoSpr2.Ybig = 0xff - (220 - (demoSpr2.posY > 220 ? 220 : demoSpr2.posY));
		// demoSpr3.Xbig = demoSpr3.Ybig = 0xff - (220 - (demoSpr3.posY > 220 ? 220 : demoSpr3.posY));

#ifdef POOL_MODE
		sortSprites((aSprite **)&drawTable[1], sortSize);
		// sortSprites((aSprite **)&drawTable[2], sortSize);
		jobMeterColor = NGCOLOR_PINK;

		if (BIOS.P2.CURRENT.UP)
			demoSpr.Ybig++;
		else if (BIOS.P2.CURRENT.DOWN)
			demoSpr.Ybig--;
		if (BIOS.P2.CURRENT.LEFT)
			demoSpr.Xbig--;
		else if (BIOS.P2.CURRENT.RIGHT)
			demoSpr.Xbig++;
		// fixPrintf1(24, 2, 2, 3, "Xbig:%02x Ybig:%02x", demoSpr.Xbig, demoSpr.Ybig);

#ifdef SHADOW
		// need to get next step & frame for shadow to always match
		shadow.currentFrame = (shadow.currentStep = aSpriteGetNextStep(&demoSpr))->frame;
		shadow.posX = demoSpr.posX;
		shadow.posY = demoSpr.posY;
		shadow.currentFlip = demoSpr.currentFlip ^ FLIP_Y; 
		shadow.Xbig = demoSpr.Xbig;
		shadow.Ybig = demoSpr.Ybig >> 2;
#endif


		// if ((BIOS_P2_CURRENT.A) && (testPool.way == WAY_UP))
		if (1 /*BIOS_P2_CURRENT.A*/)
		{
			// if (testPool.way == WAY_UP)
			// 	testPool.currentDown = testPool.poolStart + 2;
			// if (testPool.way == WAY_DOWN)
			// 	testPool.currentUp = testPool.poolEnd - 2;
			// spritePoolDrawList3(&testPool, testPool.way == WAY_UP ? (void *)&drawTable[1] : (void *)drawTablePtr);
			spritePoolDrawList3(&testPool, testPool.way == WAY_UP ? (void *)&drawTable[1] : (void *)drawTablePtr);
		}
		else
			spritePoolDrawList(&testPool, testPool.way == WAY_UP ? (void *)&drawTable[1] : (void *)drawTablePtr);

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

const char sinTable[] = {32, 34, 35, 37, 38, 40, 41, 43, 44, 46, 47, 48, 50, 51, 52, 53,
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

	LSPCmode = 0x1c00;
	loadTIirq(TI_MODE_SINGLE_DATA);

	pictureInit(&testPict, &terrypict, 1, 16, x, y, FLIP_NONE);
	palJobPut(16, terrypict.palInfo->count, terrypict.palInfo->data);

	rasterAddr = 0x8400 + testPict.baseSprite;

	extern u16 mess_pictureDemo[];
	addMessage(mess_pictureDemo);

	while (1)
	{
		SCClose();
		waitVBlank();

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

		while ((LSPC.MODE >> 7) != 0x120)
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

#define SCROLLSPEED 1.06
extern u16 mess_rasterScrollDemo[];
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
	while (1)
	{
		SCClose();
		waitVBlank();

		while ((LSPC.MODE >> 7) != 0x120)
			; // (debug) wait line 16
		jobMeterColor = NGCOLOR_BLUE;

		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, 64);
			TInextTable = 0;
			SCClose();
			waitVBlank();
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

	LSPCmode = 0x0000;
	TIbase = TI_ZERO + DESERT_POSY * 384; // timing to first line
	TIreload = 384 * 4;		      // 4 lines intervals
	rasterAddr = VRAM_POSY_ADDR(desertHandler.baseSprite);

	loadTIirq(TI_MODE_SINGLE_DATA);

	while (1)
	{
		fc = libNG_frameCounter;
		SCClose();
		waitVBlank();

		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, desert.tileWidth);
			TInextTable = 0;
			SCClose();
			waitVBlank();
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

// misc fix maps
static const u16 fadeData0[15] = {0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x03f0, 0x0000};
static const u16 fadeData1[15] = {0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x03f1, 0x0000};
static const u16 fadeData2[15] = {0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x03f2, 0x0000};
static const u16 fadeData3[15] = {0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x03f3, 0x0000};
static const u16 fadeData4[15] = {0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x03f4, 0x0000};
static const u16 fadeData5[15] = {0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x03f5, 0x0000};
static const u16 fadeData6[15] = {0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x03f6, 0x0000};
static const u16 fadeData7[15] = {0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x03f7, 0x0000};
static const u16 fadeData8[15] = {0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x03f8, 0x0000};
static const u16 fadeData9[15] = {0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x03f9, 0x0000};
static const u16 fadeDataA[15] = {0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x03fa, 0x0000};
static const u16 *const fadeData[11] = {fadeData0, fadeData1, fadeData2, fadeData3, fadeData4, fadeData5, fadeData6, fadeData7, fadeData8, fadeData9, fadeDataA};
static const paletteInfo *const logoPals[] = {&logo95_Palettes, &logo96_Palettes, &logo97_Palettes, &logo98_Palettes};

#define MAX_HEALTH 192
#define MAX_POWER 64

void fixDemo()
{
	u16 a, b;
	s16 i;
	u32 fc, ticks = 0;
	u16 power = 0, logo = 99;
	s16 time = 99, health = MAX_HEALTH, fadeIndex = -1, fadeType = 1;
	u16 powerString[11] = {0x3e9, 0x3e0, 0x3e0, 0x3e0, 0x3e0, 0x3e0, 0x3e0, 0x3e0, 0x3e0, 0x3ea, 0};
	char healthTmp[18] = "\xc9                ";
	u16 healthTopString[20], healthBotString[20], counterString[20];

	u8 fadeDensity[40];

	clearFixLayer();
	jobMeterSetup(TRUE);

	palJobPut(14, 1, &fix_bars_Palettes.data);
	for (i = 0; i < 40; i++)
		fadeDensity[i] = 0;

	while (1)
	{
		fc = libNG_frameCounter;
		SCClose();
		waitVBlank();

		while ((LSPC.MODE >> 7) != 0x120)
			; // (debug) wait raster line 16
		jobMeterColor = NGCOLOR_BLUE;

		if (BIOS.START_CURRENT.P1_START)
		{
			SCClose();
			waitVBlank();
			return;
		}

		ticks = (fc ^ libNG_frameCounter) & libNG_frameCounter;
		if (ticks & 0x2)
			if (--health < 0)
				health = MAX_HEALTH;
		if (ticks & 0x4)
			if (++power > MAX_POWER)
				power = 0;
		if (ticks & 0x20)
			if (--time < 0)
				time = 99;

		fixPrintf3(PRINTINFO(2, 8, (libNG_frameCounter >> 5) & 0x7, 3), counterString, "Frame #0x%08x", libNG_frameCounter);

		// print power bar
		u16 *ps = &powerString[1];
		s16 pwr = power;
		for (u16 x = 0; x < 8; pwr -= 8, x++)
			*ps++ = 0x3e0 + (pwr > 7 ? 8 : (pwr <= 0 ? 0 : (pwr & 0x7)));
		fixJobPut(2, 14, FIX_LINE_WRITE, 14, powerString);

		// print health bar & timer
		char *cp = &healthTmp[1];
		s16 hlth = health >= 96 ? health - 96 : health;
		a = health >= 96 ? 0xd0 : 0xc0;
		for (u16 x = 0; x < 12; hlth -= 8, x++)
			*cp++ = a + (hlth > 7 ? 8 : (hlth <= 0 ? 0 : (hlth & 7)));
		a = time / 10;
		*cp++ = 0xa0 + a;
		*cp++ = 0xb0 + a;
		a = time % 10;
		*cp++ = 0xa0 + a;
		*cp++ = 0xb0 + a;

		fixPrintf3(PRINTINFO(2, 4, 14, 3), healthTopString, "%s", healthTmp);
		fixPrintf3(PRINTINFO(2, 5, 14, 4), healthBotString, "%s", healthTmp); // could also copy data from topString and +1 bank #

		// do fade in / fade out
		if (libNG_frameCounter & 0x1)
		{
			if (fadeIndex < 39 + 27)
				fadeIndex++;
			i = fadeIndex;
			b = 0;
			if (fadeType)
			{
				a = 1;
				do
				{
					if (i >= 39)
						goto _skip;
					if (fadeDensity[i] != a)
					{
						fadeDensity[i] = a;
						fixJobPut(i, 16, FIX_COLUMN_WRITE, 14, fadeData[a]);
					}
					if (a == 10)
						break;
				_skip:
					if (++b >= 3)
					{
						a = a < 10 ? a + 1 : a;
						b = 0;
					}
				} while (--i >= 0);
			}
			else
			{
				a = 9;
				do
				{
					if (i >= 39)
						goto _skip2;
					if (fadeDensity[i] != a)
					{
						fadeDensity[i] = a;
						fixJobPut(i, 16, FIX_COLUMN_WRITE, 14, fadeData[a]);
					}
					if (a == 0)
						break;
				_skip2:
					if (++b >= 3)
					{
						a = a > 0 ? a - 1 : a;
						b = 0;
					}
				} while (--i >= 0);
			}
			if (i == 38)
			{
				fadeType ^= 1;
				fadeIndex = -1;
			}
		}

		// display logo
		extern u16 *mess_logos[];
		if ((a = (libNG_frameCounter >> 6) & 0x3) != logo)
		{
			logo = a;
			addMessage(mess_logos[logo]);
			palJobPut(13, 1, &logoPals[logo]->data);
		}
		jobMeterColor = NGCOLOR_GREEN;
	}
}

extern u16 mess_colorStreamDemoA[];
void colorStreamDemoA()
{
	scroller sc;
	colorStream stream;
	s16 posX = 0, posY = 0;

	clearFixLayer();
	scrollerInit(&sc, &streamScroll, 1, 16, 0, 0);
	colorStreamInit(&stream, &streamScroll_colorStream, 16, COLORSTREAM_STARTCONFIG);

	addMessage(mess_colorStreamDemoA);

	while (1)
	{
		SCClose();
		// check palJobs load
		if (0)
		{
			u32 *plj;
			u16 lastJobs = 0, jobs = 0;
			if (jobs != 0)
				lastJobs = jobs;
			jobs = 0;
			plj = PALJOBS;
			while (*plj != 0xffffffff)
			{
				jobs += ((*plj++) >> 16) + 1;
				plj++;
			}
			fixPrintf1(PRINTINFO(0, 2, 3, 3), "Jobs:%d (last:%d)   ", jobs, lastJobs);
		}
		waitVBlank();

		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, 21);
			SCClose();
			waitVBlank();
			return;
		}

		if (BIOS.P1.CURRENT.B)
		{
			if (BIOS.P1.EDGE.LEFT)
				posX -= 64;
			else if (BIOS.P1.EDGE.RIGHT)
				posX += 64;
		}
		else
		{
			if (BIOS.P1.CURRENT.LEFT)
				posX -= BIOS.P1.CURRENT.A ? 8 : 1;
			else if (BIOS.P1.CURRENT.RIGHT)
				posX += BIOS.P1.CURRENT.A ? 8 : 1;
		}
		if (posX < 0)
			posX = 0;
		else if (posX > (streamScroll.mapWidth - 20) << 4)
			posX = (streamScroll.mapWidth - 20) << 4;

		scrollerSetPos(&sc, posX, posY);
		colorStreamSetPos(&stream, posX);
	}
}

extern u16 mess_colorStreamDemoB[];
void colorStreamDemoB()
{
	scroller sc;
	colorStream stream;
	s16 posX = 0, posY = 0;

	clearFixLayer();
	scrollerInit(&sc, &SNKLogoStrip, 1, 16, 0, 0);
	colorStreamInit(&stream, &SNKLogoStrip_colorStream, 16, COLORSTREAM_STARTCONFIG);

	addMessage(mess_colorStreamDemoB);

	while (1)
	{
		SCClose();

		// check palJobs load
		if (0)
		{
			u32 *plj;
			u16 lastJobs = 0, jobs = 0;
			if (jobs != 0)
				lastJobs = jobs;
			jobs = 0;
			plj = PALJOBS;
			while (*plj != 0xffffffff)
			{
				jobs += ((*plj++) >> 16) + 1;
				plj++;
			}
			fixPrintf1(PRINTINFO(0, 2, 3, 3), "Jobs:%d (last:%d)   ", jobs, lastJobs);
		}
		waitVBlank();

		if (BIOS.START_CURRENT.P1_START)
		{
			clearSprites(1, 21);
			SCClose();
			waitVBlank();
			return;
		}

		if (BIOS.P1.CURRENT.B)
		{
			if (BIOS.P1.EDGE.UP)
				posY -= 224;
			else if (BIOS.P1.EDGE.DOWN)
				posY += 224;
		}
		else
		{
			if (BIOS.P1.CURRENT.UP)
				posY -= BIOS.P1.CURRENT.A ? 224 : 1;
			else if (BIOS.P1.CURRENT.DOWN)
				posY += BIOS.P1.CURRENT.A ? 224 : 1;
		}
		if (posY < 0)
			posY = 0;
		if (posY > (SNKLogoStrip.mapHeight - 14) << 4)
			posY = (SNKLogoStrip.mapHeight - 14) << 4;

		scrollerSetPos(&sc, posX, posY);
		colorStreamSetPos(&stream, posY);
	}
}

void testCallBack()
{
	if (BIOS.P1.EDGE.A)
		callBackCounter++;
}

#define CURSOR_MAX 7
void (*const demos[])() = {pictureDemo, scrollerDemo, aSpriteDemo, fixDemo, rasterScrollDemo, desertRaster, colorStreamDemoA, colorStreamDemoB};

void USER()
{
	u16 cursor = 0;

	clearFixLayer();
	initGfx();

	palJobPut(0, 8, fixPalettes);
	backgroundColor = 0x7bbb;

	// using VBL callbacks to count global A button presses
	VBL_callBack = testCallBack;
	VBL_skipCallBack = testCallBack;

	fixPrintf1(PRINTINFO(0, 2, 1, 3), "RAM usage: 0x%04x (%d)", ((u32)&_bend) - 0x100000, ((u32)&_bend) - 0x100000);
	sprintf2((char*)MAME_PRINT, "RAM usage: 0x%04x (%d)\n", ((u32)&_bend) - 0x100000, ((u32)&_bend) - 0x100000);
	sprintf2((char*)MAME_LOG, "RAM usage: 0x%04x (%d)\n", ((u32)&_bend) - 0x100000, ((u32)&_bend) - 0x100000);

	while (1)
	{
		SCClose();
		waitVBlank();

		if (BIOS.P1.EDGE.UP)
			cursor = cursor > 0 ? cursor - 1 : CURSOR_MAX;
		else if (BIOS.P1.EDGE.DOWN)
			cursor = cursor < CURSOR_MAX ? cursor + 1 : 0;
		if (BIOS.P1.EDGE.A)
		{
			demos[cursor]();
			clearFixLayer();
			backgroundColor = 0x7bbb; // restore BG color
		}

		fixPrintf1(PRINTINFO(0, 3, 1, 3), "CB counter:%d", callBackCounter);
		fixPrint(PRINTINFO(8, 10, cursor == 0 ? 2 : 4, 3), "Picture demo");
		fixPrint(PRINTINFO(8, 11, cursor == 1 ? 2 : 4, 3), "Scroller demo");
		fixPrint(PRINTINFO(8, 12, cursor == 2 ? 2 : 4, 3), "Animated sprite demo");
		fixPrint(PRINTINFO(8, 13, cursor == 3 ? 2 : 4, 3), "Fix layer demo");
		fixPrint(PRINTINFO(8, 14, cursor == 4 ? 2 : 4, 3), "Raster demo A");
		fixPrint(PRINTINFO(8, 15, cursor == 5 ? 2 : 4, 3), "Raster demo B");
		fixPrint(PRINTINFO(8, 16, cursor == 6 ? 2 : 4, 3), "Color stream demo A");
		fixPrint(PRINTINFO(8, 17, cursor == 7 ? 2 : 4, 3), "Color stream demo B");

		fixPrint(PRINTINFO(8, 20, 4, 3), "(P1 START - Menu return)");
		fixPrint(PRINTINFO(8, 28, 5, 3), "libNG tests - @2024 Hpman");
	}
}


void COIN_SOUND()
{
}

void DEMO_END()
{
}

void PLAYER_START()
{
	// no proper start support for this sample
}

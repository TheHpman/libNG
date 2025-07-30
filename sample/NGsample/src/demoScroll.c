/*
Scroller demo program.

scrollerDemo()
-------------
Demonstrates the use of scroller plaaes to make a background.

Also included color math demonstration on player 2 controls.
*/

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

#define CLIPPING 5

// from messData.s
extern u16 mess_scrollDemo[];

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
	LSPCmode.raw = 0x1c00; // 0x1c autoanim speed, enabled
	clearFixLayer();
	jobMeterSetup(TRUE);

	scrollerInit(&backScroll, &ffbg_a, 1, palIdx, (((x - 8) * 141) / 299) + BACK_MIN_X, (((y - 16) * 3) / 8) + BACK_MIN_Y);
	// setBank(&ffbg_a);
	// this section valid because scrollerInit did the banking for us
#if COLORMATH_ENABLE
	cMathLoadPalette(palIdx, ffbg_a.palInfo->count, FADE_RESET, ffbg_a.palInfo->data);
#else
	palJobPut(palIdx, ffbg_a.palInfo->count, ffbg_a.palInfo->data);
#endif
	palIdx += ffbg_a.palInfo->count;

	scrollerInit(&frontScroll, &ffbg_b, 22, palIdx, x, y);
	// scrollerInitClipped(&frontScroll, &ffbg_b, 22, palIdx, x, y, CLIPPING);
	// setBank(&ffbg_b);
#if COLORMATH_ENABLE
	cMathLoadPalette(palIdx, ffbg_b.palInfo->count, FADE_RESET, ffbg_b.palInfo->data);
#else
	palJobPut(palIdx, ffbg_b.palInfo->count, ffbg_b.palInfo->data);
#endif
	palIdx += ffbg_b.palInfo->count;

	pictureInit(&car, &ffbg_c, 43, palIdx, carx, cary, FLIP_NONE);
	// setBank(&ffbg_c);
#if COLORMATH_ENABLE
	cMathLoadPalette(palIdx, ffbg_c.palInfo->count, FADE_RESET, ffbg_c.palInfo->data);
	u16 palcount = palIdx - 16 + 1;
#else
	palJobPut(palIdx, ffbg_c.palInfo->count, ffbg_c.palInfo->data);
#endif

	addMessage(mess_scrollDemo);

	while (SCClose(), waitVBlank(), 1)
	{
		while (LSPC.MODE.lineCounter != 0x120)
			; // (debug) wait raster line 16
		jobMeterColor = NGCOLOR_PURPLE;

#if COLORMATH_ENABLE
		u16 palComps = (BIOS.P2.CURRENT.A ? FADE_RED : 0) |
			       (BIOS.P2.CURRENT.B ? FADE_GREEN : 0) |
			       (BIOS.P2.CURRENT.C ? FADE_BLUE : 0);
		if (BIOS.P2.EDGE.UP)
			cMathSetCommand(16, palcount, (FADE_TO | palComps | FADE_SPEED2));
		else if (BIOS.P2.EDGE.DOWN)
			cMathSetCommand(16, palcount, (FADE_FROM | palComps | FADE_SPEED2));
		else if (BIOS.P2.EDGE.LEFT)
			cMathSetCommand(16, palcount, (FADE_FILL | palComps));
		else if (BIOS.P2.EDGE.RIGHT)
			cMathSetCommand(16, palcount, FADE_RESET);

		if (BIOS.P2.EDGE.D)
			cMathSetCommand(16, palcount, FADE_RESET);
#endif // COLORMATH_ENABLE

		if (BIOS.START_EDGE.P1_START)
		{
			setBank(&ffbg_c);
			clearSprites(1, 42 + ffbg_c.tileWidth);
			SCClose(), waitVBlank();
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

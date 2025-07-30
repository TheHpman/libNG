#include <neogeo.h>
#include "externs.h"

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
			u8 debugDip1_8 : 1;
			u8 debugDip1_7 : 1;
			u8 debugDip1_6 : 1;
			u8 debugDip1_5 : 1;
			u8 debugDip1_4 : 1;
			u8 debugDip1_3 : 1;
			u8 debugDip1_2 : 1;
			u8 debugDip1_1 : 1;

			u8 debugDip2_8 : 1;
			u8 debugDip2_7 : 1;
			u8 debugDip2_6 : 1;
			u8 debugDip2_5 : 1;
			u8 debugDip2_4 : 1;
			u8 debugDip2_3 : 1;
			u8 debugDip2_2 : 1;
			u8 debugDip2_1 : 1;
		};
	};
	// first 2 bytes are always reserved for debug dips
	u8 stuff[254];
	// 256 bytes
} bkp_ram_info;

extern bkp_ram_info bkp_data; // defined in linker file
extern u32 _bend;

volatile u32 callBackCounter;

// fix palettes for text
static const u16 fixPalettes[] = {
    0x8000, 0xefb8, 0x0222, 0x5fa7, 0xde85, 0x2c74, 0x2a52, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xebea, 0x0041, 0xa9d8, 0x57c7, 0xf6b5, 0x43a4, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0x014c, 0x9113, 0xb15e, 0x317f, 0x119f, 0x11af, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xeb21, 0x0111, 0xee21, 0x6f31, 0x6f51, 0x6f61, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xed31, 0xc311, 0xee51, 0x4f81, 0x4fa1, 0x4fc1, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xbad3, 0x0111, 0x09c0, 0xe7b0, 0xc580, 0xe250, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xefb8, 0x0111, 0xde96, 0x3c75, 0x2950, 0x4720, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0x8444, 0x0111, 0xf555, 0xf666, 0x7777, 0x8888, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0x7fff, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000};

    
void testCallBack()
{
	if (BIOS.P1.EDGE.B)
		callBackCounter++;
}

#define CURSOR_MAX 9
extern void pictureDemo();
extern void aSpriteDemo();
extern void portraitsDemo();
extern void scrollerDemo();
extern void fixDemo(), textTyperDemo();
extern void rasterScrollDemo(), desertRaster();
extern void colorStreamDemoA(), colorStreamDemoB();
void (*const demos[])() = {pictureDemo, scrollerDemo, aSpriteDemo, portraitsDemo, fixDemo, textTyperDemo, rasterScrollDemo, desertRaster, colorStreamDemoA, colorStreamDemoB};
extern u16 mess_menu[], *mess_menuMsgs[];

// Improper USER usage in this simple test program,
//  see template project for correct usage.
void USER()
{
	u16 cursor = 0;

	clearFixLayer();
	initGfx();

	palJobPut(0, 9, fixPalettes);
	backgroundColor = 0x7bbb;

	// using VBL callbacks to count global B button presses
	VBL_callBack = testCallBack;
	VBL_skipCallBack = testCallBack;

	fixPrintf1(PRINTINFO(0, 2, 1, 3), "RAM usage: 0x%04x (%d)", ((u32)&_bend) - 0x100000, ((u32)&_bend) - 0x100000);
	// debug print to mame
	sprintf2(MAME_PRINT_BUFFER, "RAM usage: 0x%04x (%d)\n", ((u32)&_bend) - 0x100000, ((u32)&_bend) - 0x100000);

	addMessage(mess_menu);
	while (SCClose(), waitVBlank(), 1)
	{
		if (BIOS.P1.EDGE.UP)
			cursor = cursor > 0 ? cursor - 1 : CURSOR_MAX;
		else if (BIOS.P1.EDGE.DOWN)
			cursor = cursor < CURSOR_MAX ? cursor + 1 : 0;
		if (BIOS.P1.EDGE.A)
		{
			demos[cursor]();
			clearFixLayer();
			backgroundColor = 0x7bbb; // restore BG color
			addMessage(mess_menu);
		}

		fixPrintf1(PRINTINFO(0, 3, 1, 3), "CB counter:%d", callBackCounter);
		addMessage(mess_menuMsgs[cursor]);
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

/*
FIX layer demo program.

fixDemo()
---------
Demonstrates importing and using FIX data for various text/meters or logos.

This demo is kinda sloppy and need a redo but hey it works =)
*/

#include <neogeo.h>
#include "externs.h"

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
		 SCClose(), waitVBlank();

		while (LSPC.MODE.lineCounter != 0x120)
			; // (debug) wait raster line 16
		jobMeterColor = NGCOLOR_BLUE;

		if (BIOS.START_CURRENT.P1_START)
			return;

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
					if (i < 39)
					{
						if (fadeDensity[i] != a)
						{
							fadeDensity[i] = a;
							fixJobPut(i, 16, FIX_COLUMN_WRITE, 14, fadeData[a]);
						}
						if (a == 10)
							break;
					}
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
					if (i < 39)
					{
						if (fadeDensity[i] != a)
						{
							fadeDensity[i] = a;
							fixJobPut(i, 16, FIX_COLUMN_WRITE, 14, fadeData[a]);
						}
						if (a == 0)
							break;
					}
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
		extern u16 *mess_logos[]; // from messData.s
		if ((a = (libNG_frameCounter >> 6) & 0x3) != logo)
		{
			logo = a;
			addMessage(mess_logos[logo]);
			palJobPut(13, 1, &logoPals[logo]->data);
		}
		jobMeterColor = NGCOLOR_GREEN;
	}
}

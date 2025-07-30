/*
Color stream demo program demo program.

Notes:
- colorStream will generate a lot of data, it is recommended to optimize
assets palettes to avoid unnecessary usage.
- colorStream currently only uses direct palettes load, and is therefore
not compatible with color math at the moment.

colorStreamDemoA()
------------------
Demonstration of colorStream on horizontal plane background.

colorStreamDemoB();
------------------
Demonstration of animation sequence, placed on vertical plane.
*/

#include <neogeo.h>
#include "externs.h"

// from messData.s
extern u16 mess_colorStreamDemoA[], mess_colorStreamDemoB[];

void colorStreamDemoA()
{
	scroller sc;
	colorStream stream;
	s16 posX = 0, posY = 0;

	clearFixLayer();
	scrollerInit(&sc, &streamScroll, 1, 16, 0, 0);
	colorStreamInit(&stream, streamScroll.csInfo, 16, COLORSTREAM_STARTCONFIG);

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
			SCClose(), waitVBlank();
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

void colorStreamDemoB()
{
	scroller sc;
	colorStream stream;
	s16 posX = 0, posY = 0;

	clearFixLayer();
	scrollerInit(&sc, &SNKLogoStrip, 1, 16, 0, 0);
	colorStreamInit(&stream, SNKLogoStrip.csInfo, 16, COLORSTREAM_STARTCONFIG);

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
			SCClose(), waitVBlank();
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

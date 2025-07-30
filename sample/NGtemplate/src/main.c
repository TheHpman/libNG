/*
Template NEO-GEO program
------------------------

This template demonstrates how to properly handle bios interactions
for proper sequencing, coinage and bookkeeping.

You can copy this folder when starting a new project.
*/

#include <neogeo.h>

typedef struct __attribute__((packed, aligned(2))) bkp_ram_info
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

	// check each frame, send code if > 0 and decrement
	u8 coinSound;

	u8 stuff[253];
	// 256 bytes total default size, adjust linker file if changing
} bkp_ram_info;

extern bkp_ram_info bkp_data; // reserved in linker file

// fix palettes for text
//  8 palettes just for text, take that MD =)
static const u16 fixPalettes[] = {
    0x8000, 0xefb8, 0x0222, 0x5fa7, 0xde85, 0x2c74, 0x2a52, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xebea, 0x0041, 0xa9d8, 0x57c7, 0xf6b5, 0x43a4, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0x014c, 0x9113, 0xb15e, 0x317f, 0x119f, 0x11af, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xeb21, 0x0111, 0xee21, 0x6f31, 0x6f51, 0x6f61, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xed31, 0xc311, 0xee51, 0x4f81, 0x4fa1, 0x4fc1, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xbad3, 0x0111, 0x09c0, 0xe7b0, 0xc580, 0xe250, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0xefb8, 0x0111, 0xde96, 0x3c75, 0x2950, 0x4720, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000,
    0x8000, 0x8444, 0x0111, 0xf555, 0xf666, 0x7777, 0x8888, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000, 0x8000};

// local game settings
#define LEVEL_1	0
#define LEVEL_2	1
#define LEVEL_3	2
#define LEVEL_4	3
#define LEVEL_5	4
#define LEVEL_6	5
#define LEVEL_7	6
#define LEVEL_8	7

#define LNG_ENGLISH	0
#define LNG_SPANISH	1
#define LNG_FRENCH	2
#define LNG_GERMAN	3
#define LNG_JAPANESE	8

#define SOFTDIP_DIFFICULTY	6
#define SOFTDIP_DEMOSOUND	7
#define SOFTDIP_LANGUAGE	8

u8 localSetting_difficulty;
u8 localSetting_language;
bool localSetting_demoSound;
u8 P1HomeCredits, P2HomeCredits;
vu8 *creditsP1, *creditsP2;

void doSettings()
{
	// setup credits pointers for faster access
	// (system/region is only checked once here)
	if (BIOS.MVS_FLAG)
	{
		// MVS
		creditsP1 = BIOS.DEVMODE[0] ? &BIOS.CREDITS.P1 : &CAB_CREDITS_P1;
		creditsP2 = (BIOS.COUNTRY_CODE != COUNTRY_USA) ? creditsP1 : creditsP1 + 1; // USA is 2 chutes
	}
	else
	{
		// NEOGEO
		creditsP1 = &P1HomeCredits;
		creditsP2 = &P2HomeCredits;
	}

	// setup game settings
	if (BIOS.MVS_FLAG)
	{
		// MVS, pull settings from soft dips
		localSetting_difficulty = BIOS.GAME_DIP[SOFTDIP_DIFFICULTY];
		localSetting_demoSound = CAB_SOUND_STOP ? FALSE : BIOS.GAME_DIP[SOFTDIP_DEMOSOUND];
		localSetting_language = BIOS.COUNTRY_CODE ? BIOS.GAME_DIP[SOFTDIP_LANGUAGE] : LNG_JAPANESE;
	}
	else
	{
		// NEOGEO, set local settings to default home values
		// can also pull from soft dips if using default MVS settings in home mode
		localSetting_difficulty = LEVEL_4;
		localSetting_demoSound = TRUE;
		localSetting_language = BIOS.COUNTRY_CODE ? LNG_ENGLISH : LNG_JAPANESE;
		// gibs credits
		P1HomeCredits = 5;
		P2HomeCredits = 5;
	}
}

void printCredits()
{
	//	/!\ all credits values are BCD format

	// left side for two chutes only
	if (creditsP1 != creditsP2)
		fixPrintf1(PRINTINFO(3, 29, 4, 3), "CR %02x", *creditsP1);
	fixPrintf1(PRINTINFO(28, 29, 4, 3), "CR %02x", *creditsP2);

	if (BIOS.MVS_FLAG)
		fixPrintf1(PRINTINFO(18, 29, 4, 3), "LV-%x", localSetting_difficulty + 1);

	// (there's usually a credit/lvl soft dip setting to turn on/off display of each item)
}

void initBackupRAM()
{
	// 1st boot bkp_data init
	// Home: bios will do an USER REQUEST_INIT call to setup default values on each power on
	// MVS: this is called only once, when the game is first booted.
	// MVS: bios automatically handles bkp_data loading/saving, making data non-volatile across multiple power cycles.
	//		(this is why you don't want a full ram clear on startup, this would wipe this data)

	// clear debug dips
	bkp_data.debugDips = 0;

	// clear coin sound counter
	bkp_data.coinSound = 0;

	// set default hi-score data etc.
	// ...
}

void customSplash()
{
	// custom splash screen animation, if not using the bios one (NEO-GEO spinning logo)
}

const char joinMessages[][21] = {
    "    INSERT COIN     \0",
    "   PRESS 1P START   \0",
    "   PRESS 2P START   \0",
    "PRESS 1P OR 2P START\0",
    "                    \0"};

void printJoinMsg()
{
	char *joinMsg;

	if (BIOS.FRAME_COUNTER & 0x20)
		joinMsg = (char *)joinMessages[4];
	else if (FREE_PLAY)
		joinMsg = (char *)joinMessages[3];
	else
	{
		u8 idx = 0;

		if (*creditsP1)
			idx += 1;
		if (*creditsP2 > ((creditsP1 != creditsP2) ? 0 : 1))
			idx += 2;
		joinMsg = (char *)joinMessages[idx];
	}
	// non VBL synced print will do for this test program
	fixPrint(PRINTINFO(10, 21, 4, 3), joinMsg);
}

void titleDisplay()
{
	// title + compulsion timer display

	palJobPut(0, 8, fixPalettes);
	backgroundColor = NGCOLOR_LIGHTRED;
	fixPrint(PRINTINFO(14, 7, 4, 3), "TITLE SCREEN");
	fixPrint(PRINTINFO(18, 24, 4, 3), "TIME");

	while (SCClose(), waitVBlank(), 1)
	{
		// no need to check for timeout, BIOS will handle that

		printJoinMsg();
		fixPrintf1(PRINTINFO(19, 25, 4, 3), "%02x", BIOS.COMPULSION_TIMER);
		if ((BIOS.PLAYER_MODE.P1 == PLAYER_MODE_PLAYING) || (BIOS.PLAYER_MODE.P2 == PLAYER_MODE_PLAYING))
			return;
	}
}

void attractDemo()
{
	// attract sequence / demo game
	// if you have multiple attracts/demos, use a bkp_data variable to keep track of cycle
	u16 timer = 15 * 60;

	palJobPut(0, 8, fixPalettes);
	backgroundColor = NGCOLOR_LIGHTBLUE;

	while (timer--)
	{
		SCClose(), waitVBlank();

		printJoinMsg();
		fixPrintf1(PRINTINFO(2, 4, 4, 3), "Attract demo timer:%d ", timer);
		if ((BIOS.PLAYER_MODE.P1 == PLAYER_MODE_PLAYING) || (BIOS.PLAYER_MODE.P2 == PLAYER_MODE_PLAYING))
			return;
	}
}

void game()
{
	u16 timer1, timer2;
	clearFixLayer();
	backgroundColor = NGCOLOR_LIGHTGREEN;

	while (SCClose(), waitVBlank(), 1)
	{
		if (((BIOS.PLAYER_MODE.P1 == PLAYER_MODE_NEVER_PLAYED) || (BIOS.PLAYER_MODE.P1 == PLAYER_MODE_GAMEOVER)) &&
		    ((BIOS.PLAYER_MODE.P2 == PLAYER_MODE_NEVER_PLAYED) || (BIOS.PLAYER_MODE.P2 == PLAYER_MODE_GAMEOVER)))
			return;

		switch (BIOS.PLAYER_MODE.P1)
		{
		case PLAYER_MODE_NEVER_PLAYED:
		case PLAYER_MODE_GAMEOVER:
			fixPrint(PRINTINFO(3, 8, 4, 3), "P1: not playing");
			if (*creditsP1 || FREE_PLAY)
			{
				BIOS.PLAYER_MODE.P1 = PLAYER_MODE_NEVER_PLAYED; // home fix
				fixPrint(PRINTINFO(3, 9, 4, 3), "PRESS START");
			}
			else
				fixPrint(PRINTINFO(3, 9, 4, 3), "INSERT COIN");
			break;
		case PLAYER_MODE_PLAYING:
			fixPrint(PRINTINFO(3, 8, 4, 3), "P1: playing    ");
			fixPrint(PRINTINFO(3, 9, 4, 3), " A to kill ");
			if (BIOS.P1.EDGE.A)
			{
				BIOS.PLAYER_MODE.P1 = PLAYER_MODE_CONTINUE;
				timer1 = 10 * 60;
			}
			break;
		case PLAYER_MODE_CONTINUE:
			fixPrintf1(PRINTINFO(3, 8, 4, 3), "P1: cont. ? %03d", timer1);
			if (*creditsP1 || FREE_PLAY)
				fixPrint(PRINTINFO(3, 9, 4, 3), "PRESS START");
			else
				fixPrint(PRINTINFO(3, 9, 4, 3), "INSERT COIN");
			if (!timer1--)
				BIOS.PLAYER_MODE.P1 = PLAYER_MODE_GAMEOVER;
			break;
		}

		switch (BIOS.PLAYER_MODE.P2)
		{
		case PLAYER_MODE_NEVER_PLAYED:
		case PLAYER_MODE_GAMEOVER:
			fixPrint(PRINTINFO(23, 8, 4, 3), "P2: not playing");
			if (*creditsP2 || FREE_PLAY)
			{
				BIOS.PLAYER_MODE.P2 = PLAYER_MODE_NEVER_PLAYED; // home fix
				fixPrint(PRINTINFO(23, 9, 4, 3), "PRESS START");
			}
			else
				fixPrint(PRINTINFO(23, 9, 4, 3), "INSERT COIN");
			break;
		case PLAYER_MODE_PLAYING:
			fixPrint(PRINTINFO(23, 8, 4, 3), "P2: playing    ");
			fixPrint(PRINTINFO(23, 9, 4, 3), " A to kill ");
			if (BIOS.P2.EDGE.A)
			{
				BIOS.PLAYER_MODE.P2 = PLAYER_MODE_CONTINUE;
				timer2 = 10 * 60;
			}
			break;
		case PLAYER_MODE_CONTINUE:
			fixPrintf1(PRINTINFO(23, 8, 4, 3), "P2: cont. ? %03d", timer2);
			if (*creditsP2 || FREE_PLAY)
				fixPrint(PRINTINFO(23, 9, 4, 3), "PRESS START");
			else
				fixPrint(PRINTINFO(23, 9, 4, 3), "INSERT COIN");
			if (!timer2--)
				BIOS.PLAYER_MODE.P2 = PLAYER_MODE_GAMEOVER;
			break;
		}
	}
}

void USER()
{
	// main entry point, handles USER requests from BIOS
	// this sub MUST return to give back control to BIOS

	if (!BIOS.DEVMODE)
		bkp_data.debugDips = 0;
	initGfx();
	clearFixLayer();
	// using callback as demonstration purpose only,
	// this is in no way an optimal place to print credits
	VBL_callBack = VBL_skipCallBack = printCredits;

	// check what the BIOS wants
	switch (BIOS.USER_REQUEST)
	{
	case USER_REQUEST_INIT:
		initBackupRAM();
		return; // not break
	case USER_REQUEST_SPLASH:
		customSplash();
		return; // not break
	case USER_REQUEST_DEMO:
		doSettings();
		attractDemo();
		break;
	case USER_REQUEST_TITLE:
		doSettings();
		titleDisplay();
		break;
	}
	// coming from demo/title, was a game started?
	if ((BIOS.PLAYER_MODE.P1 == PLAYER_MODE_PLAYING) || (BIOS.PLAYER_MODE.P2 == PLAYER_MODE_PLAYING))
	{
		game(); // start main game loop
		// -- show game over screen here --
		BIOS.USER_MODE = USER_MODE_TITLE_DEMO; // re-enable game switching/join-in after game over screen
	}
	// -- show high scores screen here --
}

void PLAYER_START()
{
	// handles start requests
	// Note: probably want to add an additional condition checks to disallow join-in during some transitions/cutscenes/etc
	u8 flags;

	if (BIOS.USER_MODE == USER_MODE_INIT_BOOT)
	{
		// cannot start during init/boot phase
		flags = 0;
	}
	else
	{
		flags = BIOS.START_FLAG;
		flags &= START_FLAG_P1 | START_FLAG_P2; // disable P3 & P4 for this example

		if (flags & START_FLAG_P1)
		{
			if (BIOS.PLAYER_MODE.P1 & 1) // already playing or game over?
				flags ^= START_FLAG_P1;
			else if (BIOS.MVS_FLAG)
				BIOS.PLAYER_MODE.P1 = PLAYER_MODE_PLAYING;
			else if (P1HomeCredits)
			{
				BIOS.PLAYER_MODE.P1 = PLAYER_MODE_PLAYING;
				P1HomeCredits--;
			}
		}

		if (flags & START_FLAG_P2)
		{
			if (BIOS.PLAYER_MODE.P2 & 1) // already playing or game over?
				flags ^= START_FLAG_P2;
			else if (BIOS.MVS_FLAG)
				BIOS.PLAYER_MODE.P2 = PLAYER_MODE_PLAYING;
			else if (P2HomeCredits)
			{
				BIOS.PLAYER_MODE.P2 = PLAYER_MODE_PLAYING;
				P2HomeCredits--;
			}
		}
	}

	BIOS.START_FLAG = flags; // return start flags
	if (flags)		 // someone is starting / continuing, switch to game mode
		BIOS.USER_MODE = USER_MODE_GAME;
}

void DEMO_END()
{
	// demonstration stop event
	// last chance to do something before bios switches games

	// typically empty
}

void COIN_SOUND()
{
	// coin sound event
	// send appropriate sound code to Z80
	// no need to check demo sound etc flags, coin sound should NEVER be muted

	// using backup ram instead of sending code directly fixes issue with coin sound
	// being lost when bios switches to tile screen
	bkp_data.coinSound++;
}

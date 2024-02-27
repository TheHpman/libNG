/**
 *  \file neogeo.h
 *  \brief main NeoGeo system defines
 *
 *  Provides declarations for hardware registers as well
 *  as BIOS RAM reserved area
 */

#ifndef __NEOGEO_H__
#define __NEOGEO_H__

#include <types.h>
#include <configNG.h>
#include <libNG.h>
#include <libNG_spr.h>
#include <libNG_scrl.h>
#include <libNG_pict.h>

#ifdef _MSC_VER
#define __attribute__(a) /* Visual Studio fix */
#endif

#ifdef VSCODE
#define __attribute__(att) /* VS Code fix */
#endif


// few basic colors for job meter
#define	NGCOLOR_BLACK		0x8000
#define	NGCOLOR_WHITE		0x7fff

#define	NGCOLOR_LIGHTRED	0xcf88
#define	NGCOLOR_RED		0x4f00
#define	NGCOLOR_DARKRED		0x8800

#define	NGCOLOR_LIGHTGREEN	0xa8f8
#define	NGCOLOR_GREEN		0x20f0
#define	NGCOLOR_DARKGREEN	0x8080

#define	NGCOLOR_LIGHTBLUE	0x988f
#define	NGCOLOR_BLUE		0x100f
#define	NGCOLOR_DARKBLUE	0x8008

#define	NGCOLOR_LIGHTPURPLE	0x7f6f
#define	NGCOLOR_PURPLE		0x5f0f
#define	NGCOLOR_DARKPURPLE	0xd909

#define	NGCOLOR_LIGHTCYAN	0x7bff
#define	NGCOLOR_CYAN		0x30ff
#define	NGCOLOR_DARKCYAN	0x8088

#define	NGCOLOR_LIGHTYELLOW	0x7ffb
#define	NGCOLOR_YELLOW		0x6ff0
#define	NGCOLOR_DARKYELLOW	0x8880

#define	NGCOLOR_LIGHTORANGE	0x6fb0
#define	NGCOLOR_ORANGE		0x4f80
#define	NGCOLOR_DARKORANGE	0x8840

#define	NGCOLOR_LIGHTPINK	0x5fcf
#define	NGCOLOR_PINK		0x5f8f
#define	NGCOLOR_DARKPINK	0x8808

#define	NGCOLOR_LIGHTGREY	0x7bbb
#define	NGCOLOR_GREY		0x8888
#define	NGCOLOR_DARKGREY	0x8444


// base bios calls
#define BIOS_SUB_CREDIT_CHECK	0xc00450
#define BIOS_SUB_CREDIT_DOWN	0xc00456
#define BIOS_SUB_READ_CALENDAR	0xc0045c
#define BIOS_SUB_SETUP_CALENDAR	0xc00462
#define BIOS_SUB_CARD		0xc00468
#define BIOS_SUB_CARD_ERROR	0xc0046e
#define BIOS_SUB_HOW_TO_PLAY	0xc00474
#define BIOS_SUB_FIX_CLEAR	0xc004c2
#define BIOS_SUB_LSP_1ST	0xc004c8
#define BIOS_SUB_MESS_OUT	0xc004ce
#define BIOS_SUB_CONTROLLER_SETUP 0xc004d4
// CD systems calls
#define CDBIOS_SUB_UPLOAD	0xc00546
#define CDBIOS_SUB_LOADFILE	0xc00552
#define CDBIOS_SUB_CDPLAYER	0xc0055e
#define CDBIOS_SUB_LOADFILEX	0xc00564
#define CDBIOS_SUB_CDDACMD	0xc0056a
#define CDBIOS_SUB_VIDEOEN	0xc00570
#define CDBIOS_SUB_PUSHCDOP	0xc00576
#define CDBIOS_SUB_SETCDDMODE	0xc0057c
#define CDBIOS_SUB_RESETGAME	0xc00582


// misc hardware I/O
#define REG_P1CNT		(*(vu8*)0x300000)	/**< Controller 1 input (active low) */
#define REG_DIPSW		(*(vu8*)0x300001)	/**< Hardware dip switches (active low) */
#define REG_SYSTYPE		(*(vu8*)0x300081)	/**< System ID */
#define REG_SOUND		(*(vu8*)0x320000)	/**< Z80 communication (R/W) */
#define REG_STATUS_A		(*(vu8*)0x320001)	/**< System switches (MVS, active low) */
#define REG_P2CNT		(*(vu8*)0x340000)	/**< Controller 2 input (active low) */
#define REG_STATUS_B		(*(vu8*)0x380000)	/**< Auxiliary inputs */
#define REG_POUTPUT		(*(vu8*)0x380001)	/**< Controller ports output */
#define REG_CRDBANK		(*(vu8*)0x380011)	/**< Set memory card bank */
#define REG_SLOT		(*(vu8*)0x380021)	/**< Set active slot (MVS) */
#define REG_LEDLATCHES		(*(vu8*)0x380031)	/**< LED latches (MVS) */
#define REG_LEDDATA		(*(vu8*)0x380041)	/**< LED data (MVS) */
#define REG_RTCCTRL		(*(vu8*)0x380051)	/**< RTC control (MVS) */
#define REG_RESETCC1		(*(vu8*)0x380061)	/**< Reset coin counter 1 (MVS) */
#define REG_RESETCC2		(*(vu8*)0x380063)	/**< Reset coin counter 2 (MVS) */
#define REG_RESETCL1		(*(vu8*)0x380065)	/**< Reset coin lockout 1 (MVS) */
#define REG_RESETCL2		(*(vu8*)0x380067)	/**< Reset coin lockout 2 (MVS) */
#define REG_SETCC1		(*(vu8*)0x3800e1)	/**< Set coin counter 1 (MVS) */
#define REG_SETCC2		(*(vu8*)0x3800e3)	/**< Set coin counter 2 (MVS) */
#define REG_SETCL1		(*(vu8*)0x3800e5)	/**< Set coin lockout 1 (MVS) */
#define REG_SETCL2		(*(vu8*)0x3800e7)	/**< Set coin lockout 2 (MVS) */

// system
#define REG_NOSHADOW		(*(vu8*)0x3a0001)	/**< Disable shadow */
#define REG_SHADOW		(*(vu8*)0x3a0011)	/**< Enable shadow */
#define REG_SWPBIOS		(*(vu8*)0x3a0003)	/**< Set BIOS vector table */
#define REG_SWPROM		(*(vu8*)0x3a0013)	/**< Set cart vector table */
#define REG_CRDUNLOCK1		(*(vu8*)0x3a0005)	/**< Enable memory card writes (1 of 2) */
#define REG_CRDLOCK1		(*(vu8*)0x3a0015)	/**< Disable memory card writes (1 of 2) */
#define REG_CRDLOCK2		(*(vu8*)0x3a0007)	/**< Disable memory card writes (2 of 2) */
#define REG_CRDUNLOCK2		(*(vu8*)0x3a0017)	/**< Enable memory card writes (2 of 2) */
#define REG_CRDREGSEL		(*(vu8*)0x3a0009)	/**< Enable memory card register select */
#define REG_CRDNORMAL		(*(vu8*)0x3a0019)	/**< Disable memory card register select */
#define REG_BRDFIX		(*(vu8*)0x3a000b)	/**< Use onboard FIX ROM (MVS only) */
#define REG_CRTFIX		(*(vu8*)0x3a001b)	/**< Use cart FIX ROM */
#define REG_SRAMLOCK		(*(vu8*)0x3a000d)	/**< Disable SRAM writes (MVS) */
#define REG_SRAMUNLOCK		(*(vu8*)0x3a001d)	/**< Enable SRAM writes (MVS) */
#define REG_PALBANK1		(*(vu8*)0x3a000f)	/**< Use palette bank 1 */
#define REG_PALBANK0		(*(vu8*)0x3a001f)	/**< Use palette bank 0 */

// CD systems only
#define REG_DISBLSPR		(*(vu8*)0xff0111)	/**< Disable sprite layer (CD) */
#define REG_DISBLFIX		(*(vu8*)0xff0115)	/**< Disable FIX layer (CD) */
#define REG_ENVIDEO		(*(vu8*)0xff0119)	/**< Enable display (CD) */
#define REG_SPRBANK		(*(vu8*)0xff01a1)	/**< SPR upload bank (CD) */
#define REG_PCMBANK		(*(vu8*)0xff01a3)	/**< PCM upload bank (CD) */
#define REG_UPMAPSPR		(*(vu8*)0xff0121)	/**< Map upload zone to SPR (CD) */
#define REG_UPMAPPCM		(*(vu8*)0xff0123)	/**< Map upload zone to PCM (CD) */
#define REG_UPMAPZ80		(*(vu8*)0xff0127)	/**< Map upload zone to Z80 (CD) */
#define REG_UPMAPFIX		(*(vu8*)0xff0129)	/**< Map upload zone to FIX (CD) */
#define REG_UPUNMAPSPR		(*(vu8*)0xff0141)	/**< Unmap upload zone to SPR (CD) */
#define REG_UPUNMAPPCM		(*(vu8*)0xff0143)	/**< Unmap upload zone to PCM (CD) */
#define REG_UPUNMAPZ80		(*(vu8*)0xff0147)	/**< Unmap upload zone to Z80 (CD) */
#define REG_UPUNMAPFIX		(*(vu8*)0xff0149)	/**< Unmap upload zone to FIX (CD) */

// LSPC chip
typedef struct _LSPC {
	union {
		u32 ADDR_DATA;		/**< LSPC VRAM address & data register, 32 bit access */
		struct {
			u16 ADDR;	/**< LSPC VRAM address register */
			u16 DATA;	/**< LSPC VRAM data register */
		};
	};
	u16 MODULO;			/**< LSPC VRAM write modulo register */
	u16 MODE;			/**< LSPC mode register */
	union {
		u32 TIMER;		/**< LSPC timer value register, 32bits access */
		struct {
			u16 TIMERHIGH;	/**< LSPC timer value register, high */
			u16 TIMERLOW;	/**< LSPC timer value register, low */
		};
	};
	u16 IRQACK;			/**< LSPC IRQ acknowledge register */
	u16 TIMERSTOP;			/**< LSPC PAL timer stop register */
} _LSPC;

#define LSPC (*((volatile _LSPC *)(0x3c0000)))

// BIOS
// COUNTRY_CODE
#define	COUNTRY_JAPAN		0
#define	COUNTRY_USA		1
#define	COUNTRY_EUROPE		2
// controllers
#define	JOY_UP			0x01
#define	JOY_DOWN		0x02
#define	JOY_LEFT		0x04
#define	JOY_RIGHT		0x08
#define	JOY_A			0x10
#define	JOY_B			0x20
#define	JOY_C			0x40
#define	JOY_D			0x80
#define	CTRL_NO_CONNECTION	0
#define	CTRL_NORMAL		1
#define	CTRL_EXPANDED		2
#define	CTRL_MAHJONG		3
#define	CTRL_KEYBOARD		4
#define	JOY_P1START		0x01
#define	JOY_P1SELECT		0x02
#define	JOY_P2START		0x04
#define	JOY_P2SELECT		0x08
#define	JOY_P3START		0x10
#define	JOY_P3SELECT		0x20
#define	JOY_P4START		0x40
#define	JOY_P4SELECT		0x80
// USER
#define	USER_REQUEST_INIT	0
#define	USER_REQUEST_SPLASH	1
#define	USER_REQUEST_DEMO	2
#define	USER_REQUEST_TITLE	3
#define	USER_MODE_INIT_BOOT	0
#define	USER_MODE_TITLE_DEMO	1
#define	USER_MODE_GAME		2

#define	START_FLAG_P1		0x01
#define	START_FLAG_P2		0x02
#define	START_FLAG_P3		0x04
#define	START_FLAG_P4		0x08

#define	PLAYER_MODE_NEVER_PLAYED 0
#define	PLAYER_MODE_PLAYING	1
#define	PLAYER_MODE_CONTINUE	2
#define	PLAYER_MODE_GAMEOVER	3
//CARD
#define	CARDCMD_FORMAT		0
#define	CARDCMD_SEARCH		1
#define	CARDCMD_DATA_LOAD	2
#define	CARDCMD_DATA_SAVE	3
#define	CARDCMD_DATA_DELETE	4
#define	CARDCMD_DATA_READ_TITLE	5
#define	CARDCMD_USER_SAVE	6
#define	CARDCMD_USER_LOAD	7
#define CARDANSWR_OK		0x00
#define CARDANSWR_NOCARD	0x80
#define CARDANSWR_UNFORMATTED	0x81
#define CARDANSWR_NO_DATA	0x82
#define CARDANSWR_FAT_ERROR	0x83
#define CARDANSWR_CARD_FULL	0x84
#define CARDANSWR_READ_ONLY	0x85

typedef struct __attribute__((packed, aligned(1))) ctrlData {
	union {
		u8 raw;
		struct {
			u8 D : 1;
			u8 C : 1;
			u8 B : 1;
			u8 A : 1;
			u8 RIGHT : 1;
			u8 LEFT : 1;
			u8 DOWN : 1;
			u8 UP : 1;
		};
	};
} ctrlData;

typedef struct __attribute__((packed, aligned(1))) ctrlDataS {
	union {
		u8 raw;
		struct {
			u8 P4_SELECT : 1;
			u8 P4_START : 1;
			u8 P3_SELECT : 1;
			u8 P3_START : 1;
			u8 P2_SELECT : 1;
			u8 P2_START : 1;
			u8 P1_SELECT : 1;
			u8 P1_START : 1;
		};
	};
} ctrlDataS;

typedef struct NGCTRL {
	u8 STATUS;			/**< Status */
	ctrlData PREVIOUS;		/**< Previous data, positive logic */
	ctrlData CURRENT;		/**< Current data, positive logic */
	ctrlData EDGE;			/**< Positive edge data, positive logic */
	ctrlData REPEAT;		/**< Repeat data, positive logic */
	u8 TIMER;			/**< Repeat timer */
} NGCTRL;

typedef struct PQUAD {
	union {
		u32 ALL;
		struct {
			u8 P1;
			u8 P2;
			u8 P3;
			u8 P4;
		};
	};
} PQUAD;

typedef struct NG_BIOS {
	u8 SSP[0xa80];			/**< INTERNAL, SSP data */
	u8 SYSTEM_MODE;			/**< System mode (0=bios wants VBlank) */
	u8 SYSRET_STATUS;		/**< INTERNAL */
	u8 MVS_FLAG;			/**< System type (0:NEOGEO 1:MVS) */
	u8 COUNTRY_CODE;		/**< System country code (COUNTRY_JAPAN/COUNTRY_USA/COUNTRY_EUROPE) */
	u8 GAME_DIP[16];		/**< Game specific soft dip settings */
	NGCTRL P1;			/**< P1 controller data */
	NGCTRL P2;			/**< P2 controller data */
	NGCTRL P3;			/**< P3 controller data */
	NGCTRL P4;			/**< P4 controller data */
	ctrlDataS START_CURRENT;	/**< Select/start inputs for controllers 1-4, positive logic */
	ctrlDataS START_EDGE;		/**< Select/start positive edge inputs for controllers 1-4, positive logic */
	u8 USER_REQUEST;		/**< Request code for USER sub (USER_REQUEST_INIT/USER_REQUEST_SPLASH/USER_REQUEST_DEMO/USER_REQUEST_TITLE) */
	u8 USER_MODE;			/**< Current user mode (USER_MODE_INIT_BOOT/USER_MODE_TITLE_DEMO/USER_MODE_GAME) */
	PQUAD CREDIT_DEC;		/**< Credit decrement values for CREDIT_DOWN, BCD format */
	u8 START_FLAG;			/**< Start flags for PLAYER_START */
	struct __padding_10fdb5 {
		u8 __pad_10fdb5;
	};
	PQUAD PLAYER_MODE;		/**< Player 1-4 statuses (PLAYER_MODE_NEVER_PLAYED/PLAYER_MODE_PLAYING/PLAYER_MODE_CONTINUE/PLAYER_MODE_GAMEOVER) */
	struct __padding_10fdba {
		u8 __pad_10fdba[4];	// INTERNAL, last frame PLAYER_MODE
	};
	u32 *MESS_POINT;
	u8 MESS_BUSY;
	struct __padding_10fdc3 {
		u8 __pad_10fdc3;
	};
	struct {
		u8 COMMAND;		/**< CARD command */
		u8 MODE;		/**< INTERNAL ? */
		u8 ANSWER;		/**< CARD return code */
		struct _paddingCard {
			u8 _padCard;
		};
		u32 START;		/**< CARD data pointer */
		u16 SIZE;		/**< CARD data size */
		u16 FCB;		/**< CARD game NGH */
		union {
			u8 SUB_8;	/**< CARD game sub number (byte) */
			u16 SUB_16;	/**< CARD game sub number (word) */
		};
	} CARD;				/**< CARD command arguments */
	struct {
		u8 YEAR;		/**< Current year (MVS) */
		u8 MONTH;		/**< Current month (MVS) */
		u8 DAY;			/**< Current day (MVS) */
		u8 WEEKDAY;		/**< Current weekday (MVS) */
		u8 HOUR;		/**< Current hour(MVS) */
		u8 MINUTE;		/**< Current minute (MVS) */
		u8 SECOND;		/**< Current second (MVS) */
		struct _paddingDate {
			u8 _padDate;
		};
	} DATE;				/**< Current date (MVS) */
	u8 COMPULSION_TIMER;		/**< Compultion timer (BCD format) */
	u8 COMPULSION_FRAME;		/**< INTERNAL, compultion timer frame counter */
	struct __padding_10fddc {
		u8 __pad_10fddc[0x24];
	};
	PQUAD CREDITS;			/**< Debug credits */
	struct __padding_10fe04 {
		u8 __pad_10fe04[0x7c];
	};
	u8 DEVMODE[8];			/**< Developer mode (8 bytes, 0=normal / '1streset'=dev mode) */
	u32 FRAME_COUNTER;		/**< Bios frame counter */
	u8 SYS_STOPPER;			/**< INTERNAL? */
	struct __padding_10fe8d {
		u8 __pad_10fe8d[0x38];
	};
	u8 TITLE_MODE;
	u32 MESS_STACK[5];		/**< INTERNAL, MESS stack */
	struct __padding_10feda {
		u8 __pad_10feda[2];
	};
	ctrlDataS STATCURNT_RAW;	/**< Raw select/start inputs for controllers 1-4, positive logic */
	ctrlDataS STATEDGE_RAW;		/**< Raw select/start positive edge inputs for controllers 1-4, positive logic */
	struct __padding_10fede {
		u8 __pad_10fede[0xa];
	};
	NGCTRL P5;			/**< P5 controller data */
	NGCTRL P6;			/**< P6 controller data */
	struct __padding_10fef4 {
		u8 __pad_10fef4[4];
	};
	struct {
		u8 REQUESTED;		/**< INTERNAL, 4P mode requested flag */
		struct __padding_10fef9 {
			u8 __pad_10fef9;
		};
		u8 ENABLED;		/**< 4P mode enabled flag */
		u8 FOUND_BOARD;		/**< 4P board found flag */
	} MODE_4P;			/**< 4P mode data */
	struct __padding_10fefc {
		u8 __pad_10fefc[4];
	};
	u32 MESS_BUFFER[64];		/**< MESS buffer pointers */
} NG_BIOS;

#define BIOS (*((volatile NG_BIOS *)(0x10f300)))


// palette RAM
typedef struct palette {
	u16 color[16];
} palette;

typedef struct _PALRAM {
	union {
		palette pals[256];
		u16 colors[256 * 16];
	};
} _PALRAM;

#define PALRAM (*((volatile _PALRAM *)(0x400000)))

// MVS SRAM
#define	CAB_CREDITS_P1		(*(vu8*)0xd00034)	/**< Cab credits count, P1, BCD format */
#define	CAB_CREDITS_P2		(*(vu8*)0xd00035)	/**< Cab credits count, P2, BCD format */
#define	CAB_SOUND_STOP		(*(vu8*)0xd00046)	/**< Cab sound stop flag */

#endif // __NEOGEO_H__

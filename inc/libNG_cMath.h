/**
 *  \file libNG_cMath.h
 *  \brief libNG color math defines
 *
 *  Provides defines, typedefs & prototypes for color math engine.
  */

#ifndef __LIBNG_CMATH_H__
#define __LIBNG_CMATH_H__

#include <types.h>
#include <configNG.h>
#include <neogeo.h>

#ifdef COLORMATH_ENABLE

#define FADE_TO		0x00
#define FADE_FROM	0x04
#define FADE_FILL	0x08
#define FADE_RESET	0x0c

#define FADE_BLACK	0x00
#define FADE_RED	0x40
#define FADE_GREEN	0x20
#define FADE_BLUE	0x10
#define FADE_YELLOW	(FADE_RED|FADE_GREEN)
#define FADE_PURPLE	(FADE_RED|FADE_BLUE)
#define FADE_CYAN	(FADE_GREEN|FADE_BLUE)
#define FADE_WHITE	(FADE_RED|FADE_GREEN|FADE_BLUE)

#define	FADE_SPEED0	0x00
#define	FADE_SPEED1	0x01
#define	FADE_SPEED2	0x02
#define	FADE_SPEED3	0x03


typedef struct __attribute__((packed, aligned(2))) palHandle
{
	union
	{
		struct
		{
			u8 acc;		/**< update counter, internal */
			u8 command;	/**< command ID */
		};
		u16 acc_cmd;
	};
	palette* ptrPalette;		/**< pointer to color data */
} palHandle;



extern palette palBuffer[256];		/**< color data buffer */
extern palHandle palHandles[256];	/**< hanldes for palettes commands & data */
extern u8 palTransferPending;		/**< flag for palette transfer pending (couldn't transfer all in one VBlank) */
extern u8 palCommandPending;		/**< flag for color math commands pending (0 when all commands finished) */
extern u8 palFlushOnFrameSkip;		/**< allow palette data transfer on skipped frames flag */

/**
*  \brief
*      Load palette(s) data to color buffer
*  \param slot
*      Palette # (0-255) to load
*  \param count
*      Count of palettes to load, from slot to slot+count-1
*  \param cmd
*      Command to apply to loaded data
*      Use command FADE_RESET for standard load to color RAM
*  \param pal
*      Pointer to color data
*/
void cMathLoadPalette(u16 slot, u16 count, u16 cmd, palette *pal);

/**
*  \brief
*      Issue a color math command to a palettes range
*  \param slot
*      Palette # (0-255) to issue command to
*  \param count
*      Count of palettes to issue command to, from slot to slot+count-1
*  \param cmd
*      Command to apply to selected palette(s)
*/
void cMathSetCommand(u16 slot, u16 count, u16 cmd);


#endif	// COLORMATH_ENABLE

#endif	// __LIBNG_CMATH_H__
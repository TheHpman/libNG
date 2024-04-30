/**
 *  \file configNG.h
 *  \brief libNG configuration file
 *
 *  Sets basic options/parameters to compile library with.
 */

#ifndef _CONFIG_NG_
#define _CONFIG_NG_

/**
 *  \brief
 *      Enable watchdog refresh during frame skips
 * 	Not recommended, watchdog must trigger on softlocks
 */
#define FRAMESKIP_KICK_DOG 0

/**
 *  \brief
 *      Flag for bankswitching support
 */
#define BANKING_ENABLE 1

/**
 *  \brief
 *      Bankswitching register address (write only, byte access)
 */
#define REG_BANKING 0x2ffff1

/**
 *  \brief
 *      Flag for color math support
 */
#define COLORMATH_ENABLE 1

#if COLORMATH_ENABLE
/**
 *  \brief
 *      Palette transfer block size (palettes count per block)
 */
#define	PAL_BLOCK_SIZE 16
#define	PAL_BLOCK_COUNT (256/PAL_BLOCK_SIZE)

/**
 *  \brief
 *      Palette transfer limit per vblank (palettes count)
 */
#define	PAL_MAX_VBLXFER 128
#define	PAL_MAX_VBLBLKS (PAL_MAX_VBLXFER/PAL_BLOCK_SIZE)

/**
 *  \brief
 *      Color math fading speeds (256/value)
 */
#define	CM_SPEED0 128
#define	CM_SPEED1 64
#define	CM_SPEED2 32
#define	CM_SPEED3 16
#endif	// COLORMATH_ENABLE

/**
 *  \brief
 *      Enable sound command ring buffer & handling
 */
#define SOUNDBUFFER_ENABLE 1

#if SOUNDBUFFER_ENABLE
/**
 *  \brief
 *      Sound command ring buffer size (power of 2, max 256)
 */
#define SOUNDBUFFER_SIZE 64

/**
 *  \brief
 *      Send an additional dispatch command during waitVBlank
 * 	Set to 1 if using 2 bytes sound codes or having overflow issues
 */
#define SOUNDBUFFER_DISPATCH_TWICE 0
#endif

/**
 *  \brief
 *      Sprite Control block 1 command buffer size
 */
#define SC1_BUFFER_SIZE 760

/**
 *  \brief
 *      Sprite Control block 2, 3 & 4 command buffer size
 */
#define SC234_BUFFER_SIZE 2280

/**
 *  \brief
 *      Palette jobs command buffer size
 */
#define PAL_BUFFER_SIZE	514

/**
 *  \brief
 *      Fix jobs command buffer size
 */
#define FIX_BUFFER_SIZE	128

/**
 *  \brief
 *      Character used to clear fix layer (12bits)
 */
#define FIX_CLEAR_CHARACTER 0x0ff

/**
 *  \brief
 *      Character to use for job meter (12bits)
 */
#define JOB_METER_CHARACTER 0x000

/**
 *  \brief
 *      Palette index to use for job meter (4bits)
 */
#define JOB_METER_PALETTE 0xf

/**
 *  \brief
 *      Color index to use for job meter
 */
#define JOB_METER_COLOR 0x01

/**
 *  \brief
 *      Mame debug print area
 */
#define MAME_PRINT_ADDR 0xc00000
#define MAME_PRINT_BUFFER ((char*)MAME_PRINT_ADDR)

/**
 *  \brief
 *      Mame debug log area
 */
#define MAME_LOG_ADDR 0xc10000
#define MAME_LOG_BUFFER ((char*)MAME_LOG_ADDR)

#endif // _CONFIG_NG_

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

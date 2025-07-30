OUTPUT_ARCH(m68k)
SEARCH_DIR(.)
/*GROUP(-lbcc -lc -lgcc)*/
__DYNAMIC	= 0;
bkp_data	= 0x100000;	/* placing backup data at RAM start */
_SIZEOF_BCKPRAM	= 0x100;	/* report value in RAM section below - Even only, max size is 0x1000 */

/*
 * Setup the memory map of the NEOGEO.
 * stack grows down from high memory.
 *
 * The memory map look like this:
 * +--------------------+ <- low memory
 * | .text              | 1MB ROM region (main cart region)
 * |        _tend       |
 * |        _tsize      |
 * |        ctor list   | the ctor and dtor lists are for
 * |        dtor list   | C++ support
 * +--------------------+ <- 0x100000 (RAM start)
 * |  Game backup       |
 * |    ram reserved    |
 * |        bkp_data    | ptr to reserved area
 * +--------------------+ <- 0x100000 + _SIZEOF_BCKPRAM
 * | .data              | Initialized data
 * |        _dstart     | start of .data
 * |        _dend       | end of .data
 * |        _dsize      | size of .data
 * +--------------------+
 * | .bss               | Uninitialized data
 * |        _bstart     | start of .bss
 * |        _bend       | end of .bss
 * |        _bsize      | size of .bss
 * +--------------------+
 * .                    .
 * .                    .
 * .                    .
 * |        __stack     | top of stack
 * +--------------------+ <- 0x0010f300 (BIOS area start)
 * |  BIOS RAM          |
 * |     reserved area  |
 * |                    |
 * +--------------------+ <- 0x110000 (top of RAM)
 * .                    .
 * .   (Mirrored RAM)   .
 * .                    .
 * +--------------------+ <- 0x200000
 * |  PORT area  (1MB)  |
 * |                    | secondary cart region:
 * |                    | typically extra rom storage
 * |                    | but can map to ram, 
 * |                    | extra hardware etc...
 * |                    |
 * +--------------------+ <- 0x300000
 * .  I/O & LSPC        .
 * .    registers       .
 * .                    .
 * +--------------------+ <- 0x400000
 * |  Palettes RAM      |
 * |                    |
 * +--------------------+ <- 0x402000
 * .                    .
 * . (Mirrored  PALRAM) .
 * .                    .
 * +--------------------+ <- 0x800000
 * |  CARD area         |
 * |                    |
 * +--------------------+ <- 0x810000
 * .                    .
 * . (Mirrored  CARD)   .
 * .                    .
 * +--------------------+ <- 0xc00000
 * |  BIOS ROM          |
 * |                    |
 * +--------------------+ <- 0xc20000
 * .                    .
 * . (Mirrored  BIOS)   .
 * .                    .
 * +--------------------+ <- 0xd00000
 * |  SRAM area         |
 * |    (MVS only)      |
 * +--------------------+ <- 0xd10000
 * .                    .
 * . (Mirrored  SRAM)   .
 * .                    .
 * +--------------------+ <- 0xe00000
*/

MEMORY
{
	/* main rom region */
	rom (rx) : ORIGIN = 0x0, LENGTH = 0x100000
	/* report _SIZEOF_BCKPRAM value below:				*/
	/*                            _____                    _____	*/
	ram (rwx): ORIGIN = (0x100000+0x100), LENGTH = (0xF300-0x100)
	/* banks region */
	rom00 (rx) : ORIGIN = 0x00200000, LENGTH = 0x100000
	rom01 (rx) : ORIGIN = 0x01200000, LENGTH = 0x100000
	rom02 (rx) : ORIGIN = 0x02200000, LENGTH = 0x100000
	rom03 (rx) : ORIGIN = 0x03200000, LENGTH = 0x100000
	rom04 (rx) : ORIGIN = 0x04200000, LENGTH = 0x100000
	rom05 (rx) : ORIGIN = 0x05200000, LENGTH = 0x100000
	rom06 (rx) : ORIGIN = 0x06200000, LENGTH = 0x100000
	rom07 (rx) : ORIGIN = 0x07200000, LENGTH = 0x100000
}

/*
 * stack grows down from BIOS reserved area
 */
PROVIDE (__stack = 0x10F300);


SECTIONS
{
	.text 0x00000000:
	{
		KEEP(*(.text.keepboot)) *(.text.*) *(.text)
		. = ALIGN(0x2);
		__CTOR_LIST__ = .;
		LONG((__CTOR_END__ - __CTOR_LIST__) / 4 - 2)
		*(.ctors)
		LONG(0)
		__CTOR_END__ = .;
		__DTOR_LIST__ = .;
		LONG((__DTOR_END__ - __DTOR_LIST__) / 4 - 2)
		*(.dtors)
		LONG(0)
		__DTOR_END__ = .;
		*(.rodata .rodata.*)
		*(.gcc_except_table .gcc_except_table.*)
		
		. = ALIGN(0x2);
		__INIT_SECTION__ = . ;
		*(.init)
		SHORT (0x4E75)	/* rts */
		
		__FINI_SECTION__ = . ;
		*(.fini)
		SHORT (0x4E75)	/* rts */
		
		_tend = .;
		*(.lit)
	} > rom
	_tsize = SIZEOF (.text);

	/* Initialized data */
	.data (0x100000 + _SIZEOF_BCKPRAM) :
	AT ( ADDR (.text) + SIZEOF (.text) )
	{
		_dstart = .;
		*(.got.plt) *(.got)
		*(.shdata)
		*(.data .data.*)
		_dend = .;
	} > ram
	_dsize = SIZEOF (.data);

	/* Uninitialized data */
	.bss (0x100000 + _SIZEOF_BCKPRAM ) + SIZEOF (.data) :
	{
		_bstart = . ;
		*(.shbss)
		*(.bss .bss.*)
		*(COMMON)
		_bend = . ;
	} > ram
	_bsize = SIZEOF (.bss);

	/* padding main rom region */
	.romfill ( _tsize + _dsize ):
	{
		FILL(0xff);
		. = ORIGIN(rom) + LENGTH(rom) - 1;
		BYTE(0xff)
	} > rom

	.stab 0 (NOLOAD) :
	{
		*(.stab)
	}

	.stabstr 0 (NOLOAD) :
	{
		*(.stabstr)
	}

	.eh_frame 0 (NOLOAD) :
	{
		*(.eh_frame)
	}
 
	.bank00 :
	{
		FILL(0xff);
		SHORT(0x4200)	/* bank ID 00 */
		KEEP(*(.bank00))
		. = ORIGIN(rom00) + LENGTH(rom00) - 0;
	} > rom00
  
	.bank01 :
	{
		FILL(0xff);
		SHORT(0x4201)	/* bank ID 01 */
		KEEP(*(.bank01))
		. = ORIGIN(rom01) + LENGTH(rom01) - 0;
	} > rom01

/*
	.bank02 :
	{
		FILL(0xff);
		SHORT(0x4202)
		KEEP(*(.bank02))
		. = ORIGIN(rom02) + LENGTH(rom02) - 0;
	} > rom02
/*
	.bank03 :
	{
		FILL(0xff);
		SHORT(0x4203)
		KEEP(*(.bank03))
		. = ORIGIN(rom03) + LENGTH(rom03) - 0;
	} > rom03
/*
	.bank04 :
	{
		FILL(0xff);
		SHORT(0x4204)
		KEEP(*(.bank04))
		. = ORIGIN(rom04) + LENGTH(rom04) - 0;
	} > rom04
/*
	.bank05 :
	{
		FILL(0xff);
		SHORT(0x4205)
		KEEP(*(.bank05))
		. = ORIGIN(rom05) + LENGTH(rom05) - 0;
	} > rom05
/*
	.bank06 :
	{
		FILL(0xff);
		SHORT(0x4206)
		KEEP(*(.bank06))
		. = ORIGIN(rom06) + LENGTH(rom06) - 0;
	} > rom06
/*
	.bank07 :
	{
		FILL(0xff);
		SHORT(0x4207)
		KEEP(*(.bank07))
		. = ORIGIN(rom07) + LENGTH(rom07) - 0;
	} > rom07
/*  */
}

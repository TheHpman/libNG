/**
 *  \file libNG.h
 *  \brief libNG base defines
 *
 *  Provides base defines, typedefs & prototypes
 */

#ifndef __LIBNG_H__
#define __LIBNG_H__


#ifdef _MSC_VER
#define __attribute__(a) /* Visual Studio fix */
#endif

#ifdef VSCODE
#define __attribute__(att) /* VS Code fix */
#endif

/*******************************
	INCLUDES
*******************************/
#include <types.h>
#include <configNG.h>

/*******************************
	DEFINES
*******************************/
#define YSHIFT	496


//timer interrupt consts
//384*40 - delay = pixel 0 timing
//emulation pixel 0 is typically off by 3 lines
#define TI_VBL_DELAY		188		// 376/2
#define TI_IRQ_DELAY		21		// 42/2
#define TI_ZERO			(15360-TI_VBL_DELAY-TI_IRQ_DELAY)
#define TI_RELOAD		384
#define TI_MODE_SINGLE_DATA	0
#define TI_MODE_DUAL_DATA	1

//fix display
#define	FIX_LINE_WRITE		0x20	// Horizontal write VRAM modulo
#define	FIX_COLUMN_WRITE	0x01	// Vertical write VRAM modulo


/*******************************
	MACROS
*******************************/
//** memory/hardware registers access macros
#define MEMBYTE(addr)		(*((u8 *)(addr)))
#define MEMWORD(addr)		(*((u16 *)(addr)))
#define MEMDWORD(addr)		(*((u32 *)(addr)))
//** volatile defines
//#define volMEMBYTE(addr)	(*((vu8 *)(addr)))
#define volMEMBYTE(addr)	(*((volatile unsigned char *)(addr)))
#define volMEMWORD(addr)	(*((vu16 *)(addr)))
#define volMEMDWORD(addr)	(*((vu32 *)(addr)))

// ========== job meter ==========
/**
 *  \brief
 *      Address of job meter color
 */
#define JOB_COLOR		(0x400000+(((JOB_METER_PALETTE*16)+JOB_METER_COLOR)*2))

/**
 *  \brief
 *      Set color of job meter (last color of palette #0xF)
 *
 *  \param color
 *      NEOGEO color value
 */
#define jobMeterColor		(*(vu16*)JOB_COLOR)

/**
 *  \brief
 *      Address of background color
 */
#define BACKGROUND_COLOR	0x401ffe

/**
 *  \brief
 *      Set background color (last color of palette #0xFF)
 *
 *  \param color
 *      NEOGEO color value
 */
#define backgroundColor		(*(vu16*)BACKGROUND_COLOR)

//** SC macros
/**
 *  \brief
 *      Add a sprite control block 1 job to buffer
 *
 *  \param addr
 *      VRAM address
 *  \param size
 *      Tile size
 *  \param pal
 *      Base palette
 *  \param data
 *      Pointer to data
 */
#define SC1Put(addr,size,pal,data)	{*SC1ptr++=((pal)<<24)|((size)<<17)|(addr);*SC1ptr++=(u32)(data);}

/**
 *  \brief
 *      Add a sprite control block 1 job to buffer, pre formatted command
 *
 *  \param cmd
 *      Formatted command
 *  \param data
 *      Pointer to data
 */
#define SC1PutQuick(cmd,data)		{*SC1ptr++=(u32)(cmd);*SC1ptr++=(u32)(data);}

/**
 *  \brief
 *      Add a sprite control block 2/3/4 job to buffer
 *
 *  \param addr
 *      VRAM address
 *  \param data
 *      VRAM data
 */
#define SC234Put(addr,data)		{*SC234ptr++=(addr);*SC234ptr++=(data);}

/**
 *  \brief
 *      Add a palette job to buffer
 *
 *  \param num
 *      Palette #
 *  \param count
 *      # of palettes to copy
 *  \param data
 *      Pointer to color data
 */
#define palJobPut(num,count,data)	{*palJobsPtr++=(((count)-1)<<16)|((num)<<5);*palJobsPtr++=(u32)(data);}

/**
 *  \brief
 *      Add a FIX layer print job to buffer
 *
 *  \param x
 *      FIX layer X position
 *  \param y
 *      FIX layer Y position
 *  \param mod
 *      VRAM modulo (FIX_LINE_WRITE/FIX_COLUMN_WRITE)
 *  \param pal
 *      Base palette
 *  \param addr
 *      Pointer to FIX data
 */
#define fixJobPut(x,y,mod,pal,addr)	{*fixJobsPtr++=(((0x7000+((x)<<5)+(y))<<16)|(((pal)<<12)|(mod)));*fixJobsPtr++=(u32)(addr);}


//** misc VRAM macros
#define	SPR_STICK		0x0040
#define SPR_UNSTICK		0x0000

/**
 *  \brief
 *      Compute VRAM address of sprite (pattern data)
 *
 *  \param sprite
 *      Sprite number
 */
#define	VRAM_SPR_ADDR(sprite)		((sprite)<<6)

/**
 *  \brief
 *      Compute VRAM address of sprite shrink attribute
 *
 *  \param sprite
 *      Sprite number
 */
#define	VRAM_SHRINK_ADDR(sprite)	(0x8000|(sprite))

/**
 *  \brief
 *      Format VRAM sprite shrink attributes value
 *
 *  \param h
 *      Horizontal shrink (0-15)
 *  \param v
 *      Vertical shrink (0-255)
 */
#define	VRAM_SHRINK(h,v)		(((h)<<8)|(v))

/**
 *  \brief
 *      Compute VRAM address of sprite Y coordinate attribute
 *
 *  \param sprite
 *      Sprite number
 */
#define	VRAM_POSY_ADDR(sprite)		(0x8200|(sprite))

/**
 *  \brief
 *      Format VRAM sprite Y coordinate attributes value
 *
 *  \param posY
 *      Y coordinate
 *  \param stick
 *      Sticky bit (SPR_STICK/SPR_UNSTICK)
 *  \param size
 *      Sprite size (1-33)
 */
#define	VRAM_POSY(posY,stick,size)	(((YSHIFT-(posY))<<7)|(stick)|(size))

 /**
  *  \brief
  *      Compute VRAM address of sprite X coordinate attribute
  *
  *  \param sprite
  *      Sprite number
  */
#define	VRAM_POSX_ADDR(sprite)		(0x8400|(sprite))

/**
 *  \brief
 *      Format VRAM sprite X coordinate attribute value
 *
 *  \param posX
 *      X coordinate
 */
#define	VRAM_POSX(posX)			((posX)<<7)

/**
*  \brief
*      Compute VRAM address of FIX plane character
*
*  \param posX
*      X coordinate
*  \param posY
*      Y coordinate
*/
#define	VRAM_FIX_ADDR(posX,posY)	(0x7000+(((posX)<<5)+(posY)))
 
/**
*  \brief
*      Print information macro - byte strings
*
*  \param posX
*      FIX plane X coordinate (0-39)
*  \param posY
*      FIX plane Y coordinate (0-31)
*  \param pal
*      Palette number (0-15)
*  \param bank
*      FIX character bank number (0-15)
*/
#define	PRINTINFO(posX,posY,pal,bank)	(((0x7000+((posX)<<5)+(posY))<<16)+((pal)<<12)+((bank)<<8))

/**
*  \brief
*      Print information macro - word strings
*
*  \param posX
*      FIX plane X coordinate (0-39)
*  \param posY
*      FIX plane Y coordinate (0-31)
*  \param pal
*      Palette number (0-15)
*/
#define	PRINTINFO_W(posX,posY,pal)	(((0x7000+((posX)<<5)+(posY))<<16)+((pal)<<12))

/*******************************
	TYPEDEFS
*******************************/

/**
*  \brief
*      Palettes ressource structure
*/
typedef struct paletteInfo {
	u16		count;			/**< # of exported palettes */
	u16		data[0];		/**< Palette data */
} paletteInfo;


/*******************************
	VARIABLES EXPORTS
*******************************/
extern vu32	libNG_frameCounter;		/**< "real" frame counter. (dropped frames won't be added) */
extern vu32	libNG_droppedFrames;		/**< dropped frames counter */
extern void	*VBL_callBack;			/**< VBlank callback function pointer */
extern void	*VBL_skipCallBack;		/**< VBlank callback function pointer (dropped frame) */

// ========== draw lists ==========
extern u32	SC1[SC1_BUFFER_SIZE];
extern u32	*SC1ptr;
extern u16	SC234[SC234_BUFFER_SIZE];
extern u16	*SC234ptr;
extern u32	PALJOBS[PAL_BUFFER_SIZE];
extern u32	*palJobsPtr;
extern u32	FIXJOBS[FIX_BUFFER_SIZE];
extern u32	*fixJobsPtr;
extern u16	libNG_drawListReady;

// ========== timer interrupt related ==========
extern u16	LSPCmode;			/**< LSPC default mode for timer interrupt mode */
extern u32	TIbase;				/**< Start value for timer interrupt counter */
extern u32	TIreload;			/**< Reload value for timer interrupt counter */
extern u16	*TInextTable;			/**< Pointer to next frame data table for timer interrupt mode */

// ========== scratchpads ==========
extern char	libNG_scratchpad64[64];		/**< 64 bytes scratch pad */
extern char	libNG_scratchpad16[16];		/**< 16 bytes scratch pad */

// ========== data ==========
extern const	u16 _fixBlankLine[41];		/**< A 16bit formatted blank string */
extern const	char libID[];			/**< Library version & build datetime */


/*******************************
	FUNCTIONS PROTOTYPES
*******************************/
//** base stuff

/**
*  \brief
*      Bank to defined object address
*/
#define setBank(addr)		(*(vu8*)REG_BANKING)=(((u32)(addr))>>24)

/**
*  \brief
*      Enables system IRQs
*/
void enableIRQ();

/**
*  \brief
*      Disables system IRQs
*/
void disableIRQ();

/**
*  \brief
*      Initializes/reset libNG graphics systems
*/
void initGfx();

/**
*  \brief
*      Close Sprite Control buffers.
*      Sets end markers & prepare for VBlank.
*/
void SCClose();

/**
*  \brief
*      Wait for VBlank interrupt to occur & process
*/
void waitVBlank();

//timer interrupt

/**
*  \brief
*      Load timer IRQ processing sub into reserved RAM area
*  \param mode
*	   Select single or dual data mode (TI_MODE_SINGLE_DATA/TI_MODE_DUAL_DATA)
*/
void loadTIirq(u16 mode);

/**
*  \brief
*      Unload timer IRQ processing sub into reserved RAM area
*/
void unloadTIirq();

//** misc
/**
*  \brief
*      Call a BIOS function
* \param sub
*	BIOS sub to call
*/
void biosCall(u32 sub);

/**
*  \brief
*      Call a BIOS function with parameter, 
*	used for calls requiring a value placed in A0 or D0
* \param sub
*	BIOS sub to call
* \param arg
*	argument to place in D0/A0
*/
void  biosCall2(u32 sub, u32 arg);

/**
*  \brief
*      Check for & setup 4P adapter
* \return
*	   true if adapter was found, false otherwise
*/
bool setup4P();

/**
*  \brief
*      Setup debug job meter
* \param setDip
*	   set additional debug soft dips
*/
void jobMeterSetup(bool setDip);

/**
*  \brief
*      Setup debug job meter - IRQ safe version
* \param setDip
*	   set additional debug soft dips
*/
void jobMeterSetup2(bool setDip);

/**
*  \brief
*      Clear sprites (set offscreen with size 0)
* \param spr
*	   starting sprite #
* \param count
*	   # of sprites to clear (1+)
*/
void clearSprites(u16 spr, u16 count);

//** text/fix

/**
*  \brief
*      Add a message to the BIOS MESS buffer
*  \param message
*      pointer to message data
*/
void addMessage(u16 *message);

/**
*  \brief
*      Clear FIX layer - immediate clear
*/
void clearFixLayer();

/**
*  \brief
*      Clear FIX layer - immediate clear, IRQ safe
*/
void clearFixLayer2();

/**
*  \brief
*      Clear FIX layer - VBlank synced
*/
void clearFixLayer3();

/**
*  \brief
*      Print a 8bit formatted string to FIX layer - immediate print
*  \param printInfo
*      PRINTINFO data
*  \param buf
*      string to be printed
*/
void fixPrint(u32 printInfo, char *buf);

/**
*  \brief
*      Print a 8bit formatted string to FIX layer - immediate print, IRQ safe
*  \param printInfo
*      PRINTINFO data
*  \param buf
*      string to be printed
*/
void fixPrint2(u32 printInfo, char *buf);

/**
*  \brief
*      Print a 16bit formatted string to FIX layer - immediate print
*  \param printInfo
*      PRINTINFO data
*  \param buf
*      string to be printed
*/
void fixPrint3(u32 printInfo, const u16 *buf);

/**
*  \brief
*      Print a 16bit formatted string to FIX layer - immediate print, IRQ safe
*  \param printInfo
*      PRINTINFO data
*  \param buf
*      string to be printed
*/
void fixPrint4(u32 printInfo, const u16 *buf);

/**
*  \brief
*      Format a 8bit string
*  \param dst
*      Destination buffer
*  \param fmt
*      Format string (8bit)
*  \param ...
*      Additional args
*/
u16 sprintf2(char *dst, char *fmt, ...);

/**
*  \brief
*      Format a 16bit string
*  \param printInfo
*      PRINTINFO data (x/y will be ignored)
*  \param dst
*      Destination buffer
*  \param fmt
*      Format string (8bit)
*  \param ...
*      Additional args
*/
u16 sprintf3(u32 printInfo, u16 *dst, char *fmt, ...);

/**
*  \brief
*      Format & print a string to FIX layer - immediate print
*  \param printInfo
*      PRINTINFO data
*  \param ...
*      Format string & args
*/
#define fixPrintf1(printInfo,...)	do{sprintf2(libNG_scratchpad64,__VA_ARGS__);fixPrint((printInfo),libNG_scratchpad64);}while(0)

/**
*  \brief
*      Format & print a string to FIX layer - immediate print, IRQ safe
*  \param printInfo
*      PRINTINFO data
*  \param ...
*      Format string & args
*/
#define fixPrintf2(printInfo,...)	do{sprintf2(libNG_scratchpad64,__VA_ARGS__);fixPrint2((printInfo),libNG_scratchpad64);}while(0)

/**
*  \brief
*      Format & print a string to FIX layer - VSynced print
*  \param printInfo
*      PRINTINFO data
*  \param buffer
*      String buffer to store result until next VBlank 
*  \param ...
*      Format string & args
*/
#define fixPrintf3(printInfo,buffer,...)	do{sprintf3(((printInfo)&0xffff0f00),(u16*)buffer,__VA_ARGS__);*fixJobsPtr++=(((printInfo)&0xfffff000)|FIX_LINE_WRITE);*fixJobsPtr++=(u32)(buffer);}while(0)


#endif // __LIBNG_H__

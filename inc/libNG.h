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

//** flip modes - VALUES ARE CODE BOUND, DO NOT CHANGE
#define FLIP_NONE	0
#define FLIP_X		1
#define FLIP_Y		2
#define FLIP_XY		3
#define FLIP_BOTH	3

//palettes consts

//scrollers consts

//pictures consts

//aSprites consts
#define	AS_FLAGS_DEFAULT	0x0000
#define	AS_FLAG_MOVED		0x0001
#define	AS_FLAG_FLIPPED		0x0002
#define	AS_FLAG_STD_COORDS	0x0000
#define	AS_FLAG_STRICT_COORDS	0x0040
#define	AS_FLAG_DISPLAY		0x0000
#define	AS_FLAG_NODISPLAY	0x0080

#define	AS_MASK_MOVED		0xfffe
#define	AS_MASK_FLIPPED		0xfffd
#define	AS_MASK_MOVED_FLIPPED	0xfffc
#define	AS_MASK_STRICT_COORDS	0xffbf
#define	AS_MASK_NODISPLAY	0xff7f

#define	AS_USE_SPRITEPOOL	0x8000
#define	AS_NOSPRITECLEAR	0x8000

//timer interrupt consts
//384*40 - delay = pixel 0 timing
//emulation pixel 0 is typically off by 3 lines
#define TI_VBL_DELAY		188		// 376/2
#define TI_IRQ_DELAY		21		// 42/2
#define TI_ZERO			(15360-TI_VBL_DELAY-TI_IRQ_DELAY)
#define TI_RELOAD		384
#define TI_MODE_SINGLE_DATA	0
#define TI_MODE_DUAL_DATA	1

//sprite pools consts
#define WAY_UP			0
#define WAY_DOWN		1

//fix display
#define	FIX_LINE_WRITE		0x20	// Horizontal write VRAM modulo
#define	FIX_COLUMN_WRITE	0x01	// Vertical write VRAM modulo

//color streams
#define COLORSTREAM_STARTCONFIG	0
#define COLORSTREAM_ENDCONFIG	1

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

//** aSprite macros
#define aSpriteHide(as)		{(as)->flags|=AS_FLAG_NODISPLAY;}
#define aSpriteShow(as)		{(as)->flags&=AS_MASK_NODISPLAY;(as)->flags|=AS_FLAG_FLIPPED;}

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

/**
 *  \brief
 *      Color stream ressource data
 */
typedef struct colorStreamInfo {
	u16		palSlots;		/**< # of required palettes */
	void		*startConfig;		/**< Pointer to start configuration data */
	void		*endConfig;		/**< Pointer to end configuration data */
	void		*fwData;		/**< Pointer to forward data */
	void		*fwDataEnd;		/**< Pointer to forward data end */
	void		*bwData;		/**< Pointer to backward data */
	void		*bwDataEnd;		/**< Pointer to backward data end */
} colorStreamInfo;

/**
 *  \brief
 *      Color stream job data
 */
typedef struct colorStreamJob {
	u16		coord;			/**< Job coordinate */
	void		*data;			/**< Pointer to job data */
} colorStreamJob;

/**
 *  \brief
 *      colorStream object handle
 */
typedef struct colorStream {
	u16		palMod;			/**< Base palette offset (pal*32) */
	u16		position;		/**< Current position in stream */
	colorStreamInfo	*info;			/**< Pointer to colorStreamInfo ressource data */
	colorStreamJob	*fwJob;			/**< Pointer to current forward data */
	colorStreamJob	*bwJob;			/**< Pointer to current backward data end */
} colorStream;


// ========== scrollers ==========
/**
 *  \brief
 *      Scroller ressource data
 */
typedef struct scrollerInfo {
	u16		stripSize;		/**< Byte size of each strip tiledata (scroller plane tile height * 4) */
	u16		sprHeight;		/**< Required sprite height (1-32) */
	u16		mapWidth;		/**< Scroller plane tile width */
	u16		mapHeight;		/**< Scroller plane tile height */
	paletteInfo	*palInfo;		/**< Pointer to related paletteInfo ressource */
	colorStreamInfo	*csInfo;		/**< Pointer to related colorStreamInfo ressource */
	u16		*strips[0];		/**< Pointer array to tilemap strips data */
} scrollerInfo;

/**
 *  \brief
 *      scroller object handle
 */
typedef struct scroller {
	u16		baseSprite;		/**< Sprite # of first sprite */
	u16		basePalette;		/**< Base palette # (0-255) */
	s16		scrlPosX;		/**< Scroller current X position */
	s16		scrlPosY;		/**< Scroller current Y position */
	scrollerInfo 	*info;			/**< Pointer to related scrollerInfo ressource */
	u16		config[32];		/**< Internal use data */
} scroller;


// ========== pictures ==========
/**
 *  \brief
 *      Picture ressource data
 */
typedef struct pictureInfo {
	u16		stripSize;		/**< Byte size of each sprite tiledata (picture tile height * 4) */
	u16		tileWidth;		/**< Tile width of picture */
	u16		tileHeight;		/**< Tile height of picture */
	paletteInfo	*palInfo;		/**< Pointer to related paletteInfo ressource */
	u16		*maps[4];		/**< Pointer array to tilemap data (normal/flipX/flipY/flipXY) */
} pictureInfo;

/**
 *  \brief
 *      picture object handle
 */
typedef struct __attribute__((packed,aligned(2))) picture {
	u16		baseSprite;		/**< Sprite # of first sprite */
	u8		basePalette;		/**< Base palette # (0-255) */
	u8		RFU;			/**< Reserved future use / padding */
	s16		posX;			/**< X coordinate of picture */
	s16		posY;			/**< Y coordinate of picture */
	u16		currentFlip;		/**< Current orientation of picture (FLIP_NONE/FLIP_X/FLIP_Y/FLIP_XY) */
	pictureInfo	*info;			/**< Pointer to related pictureInfo ressource data */
} picture;


//========== animated sprites ==========

/**
 *  \brief
 *      Sprite frame data (plain style)
 */
typedef struct sprFrame {
	u16		tileWidth;		/**< Frame tile width */
	u16		tileHeight;		/**< Frame tile height */
	u16		stripSize;		/**< Strip byte size (tileHeight *4) */
	u16		*maps[4];		/**< Pointer array to tilemap data (FLIP_NONE/FLIP_X/FLIP_Y/FLIP_XY) */
} sprFrame;

/**
 *  \brief
 *      Sprite frame data (split style)
 */
typedef struct sprFrame2 {
	u16		key;			/**< Key to signal split format (always 0) */
	u16		sprCount;		/**< Sprite count used in frame */
	u16		*maps[4];		/**< Pointer array to tilemap data (FLIP_NONE/FLIP_X/FLIP_Y/FLIP_XY) */
} sprFrame2;

/**
 *  \brief
 *      Animation step data
 */
typedef struct animStep {
	sprFrame	*frame;			/**< Pointer to frame data */
	s16		flipShiftX;		/**< Frame X offset (X flipped) */
	s16		shiftX;			/**< Frame X offset */
	s16		flipShiftY;		/**< Frame Y offset (Y flipped) */
	s16		shiftY;			/**< Frame Y offset */
	u16		duration;		/**< Display frame time of step */
} animStep;
// /!\ size is code bound

/**
 *  \brief
 *      Animated sprite ressource data
 */
typedef struct spriteInfo {
	u16		frameCount;		/**< # of frames */
	u16		maxWidth;		/**< Tile width of largest frame */
	paletteInfo	*palInfo;		/**< Pointer to related palette ressource data */
	animStep	**anims;		/**< Pointer to animation data */
	sprFrame	frames[0];		/**< Array of frames */
} spriteInfo;

/**
 *  \brief
 *      aSprite object handle
 */
typedef struct __attribute__((packed,aligned(2))) aSprite {
	u16		baseSprite;		/**< Sprite # of first sprite */
	u8		basePalette;		/**< Base palette # (0-255) */
	u8		bank;			/**< Bank # */
	s16		posX;			/**< X coordinate of sprite */
	s16		posY;			/**< Y coordinate of sprite */
	u16		animID;			/**< Animation ID */
	u16		currentAnim;		/**< Current animation ID */
	u16		stepNum;		/**< Current animation step # */
	animStep	*anims;			/**< Pointer to animations data */
	animStep	*steps;			/**< Pointer to steps data for current animation */
	animStep	*currentStep;		/**< Pointer to current step data */
	sprFrame	*currentFrame;		/**< Pointer to current frame data */
	u16		counter;		/**< Frame counter for current step */
	u16		repeats;		/**< # of played repeats */
	u16		tileWidth;		/**< (internal, for fixed allocation) */
	u16		currentFlip;		/**< Current orientation of sprite */
	union
	{
		u16		flags;			/**< Current flags of sprite */
		struct
		{
			u16	flag_padding : 8;
			u16	flag_noDisplay : 1;
			u16	flag_strictCoords : 1;
			u16	flag_none : 4;
			u16	flag_flipped : 1;
			u16	flag_moved : 1;
		};
	};
	union
	{
		struct
		{
			u8	Xbig;		/**< Horizontal scale (0-255) */
			u8	Ybig;		/**< Vertical scale (0-255) */
		};
		u16	XYbig;			/**< Horizontal & Vertical scale */
	};
} aSprite;

// ========== sprite pools ==========
/**
 *  \brief
 *      Sprite pool handle
 */
typedef struct spritePool {
	u16		poolStart;		/**< Starting sprite # of pool */
	u16		poolEnd;		/**< Last sprite # of pool */
	u16		poolSize;		/**< Pool size (sprite count) */
	u16		way;			/**< Current render way (WAY_UP/WAY_DOWN) */
	u16		currentUp;		/**< Current sprite # for up render */
	u16		currentDown;		/**< Current sprite # for down render */
} spritePool;


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


/*******************************
	FUNCTIONS EXPORTS
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


// ========== pictures ==========
/**
*  \brief
*      Initializes a picture handle
*  \param p
*      picture handle
*  \param pi
*      Picture info resource data
*  \param baseSprite
*      Starting sprite
*  \param basePalette
*      Starting palette
*  \param posX
*      X start position
*  \param posY
*      Y start position
*  \param flip
*      Flip mode
*/
void pictureInit(picture *p, const pictureInfo *pi, u16 baseSprite, u8 basePalette, s16 posX, s16 posY, u16 flip);

/**
*  \brief
*      Set picture screen position (top-left corner)
*  \param p
*      picture handle
*  \param toX
*      Picture X position
*  \param toY
*      Starting Y position
*/
void pictureSetPos(picture *p, s16 toX, s16 toY);

/**
*  \brief
*      Set picture flip mode
*  \param p
*      picture handle
*  \param flip
*      Flip mode
*/
void pictureSetFlip(picture *p, u16 flip);

/**
*  \brief
*      Move picture, relative from current position
*  \param p
*      picture handle
*  \param shiftX
*      X shift from current position
*  \param shiftY
*      Y shift from current position
*/
void pictureMove(picture *p, s16 shiftX, s16 shiftY);

/**
*  \brief
*      Disable picture display
*  \param p
*      picture handle
*/
void pictureHide(picture *p);

/**
*  \brief
*      Enable picture display
*  \param p
*      picture handle
*/
void pictureShow(picture *p);

// ========== scrollers ==========
/**
*  \brief
*      Initialize a scroller handle
*  \param s
*      scroller handle
*  \param si
*      Scroller info resource data
*  \param baseSprite
*      Starting sprite
*  \param basePalette
*      Starting palette
*  \param posX
*      X start position
*  \param posY
*      Y start position
*/
void scrollerInit(scroller *s, const scrollerInfo *si, u16 baseSprite, u8 basePalette, s16 posX, s16 posY);

/**
*  \brief
*      Shift a scroller to set position
*  \param s
*      scroller handle
*  \param toX
*      X position to scroll to
*  \param toY
*      Y position to scroll to
*/
void scrollerSetPos(scroller *s, s16 toX, s16 toY);

// ========== animated sprites ==========

/**
*  \brief
*      Initialize an aSprite handle
*  \param as
*      aSprite handle
*  \param si
*      Sprite info resource data
*  \param baseSprite
*      Starting sprite
*  \param basePalette
*      Starting palette
*  \param posX
*      X start position
*  \param posY
*      Y start position
*  \param anim
*      Start animation
*  \param flip
*      Flip mode to initialize with
*  \param flags
*      Flags to initialize with
*/
void aSpriteInit(aSprite *as, const spriteInfo *si, u16 baseSprite, u8 basePalette, s16 posX, s16 posY, u16 anim, u16 flip, u16 flags);

/**
*  \brief
*      Animate & update display of fixed allocation aSprite 
*  \param as
*      aSprite handle
*/
void aSpriteAnimate(aSprite *as);

/**
*  \brief
*      Set aSprite animation
*	No action if same anim is already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*/
void aSpriteSetAnim(aSprite *as, u16 anim);

/**
*  \brief
*      Set aSprite animation
*	Will retrigger if same anim is already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*/
void aSpriteSetAnim2(aSprite *as, u16 anim);

/**
*  \brief
*      Set aSprite step for current animation
*	No action if same step is already playing
*  \param as
*      aSprite handle
*  \param step
*      Step number
*/
void aSpriteSetStep(aSprite *as, u16 step);

/**
*  \brief
*      Set aSprite step for current animation
*	Will retrigger if same step is already playing
*  \param as
*      aSprite handle
*  \param step
*      Step number
*/
void aSpriteSetStep2(aSprite *as, u16 step);

/**
*  \brief
*      Set aSprite animation and step
*	No action if same parameters already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*  \param step
*      Step number
*/
void aSpriteSetAnimStep(aSprite *as, u16 anim, u16 step);

/**
*  \brief
*      Set aSprite animation and step
*	Will retrigger if same parameters already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*  \param step
*      Step number
*/
void aSpriteSetAnimStep2(aSprite *as, u16 anim, u16 step);

/**
*  \brief
*      Set aSprite position
*  \param as
*      aSprite handle
*  \param newX
*      X position to set
*  \param newY
*      Y position to set
*/
void aSpriteSetPos(aSprite *as, s16 newX, s16 newY);

/**
*  \brief
*      Shift aSprite position
*  \param as
*      aSprite handle
*  \param shiftX
*      X axis position shift
*  \param newY
*      Y axis position shift
*/
void aSpriteMove(aSprite *as, s16 shiftX, s16 shiftY);

/**
*  \brief
*      Set aSprite flip mode
*  \param as
*      aSprite handle
*  \param flip
*      new flip setting (FLIP_xxx)
*/
void aSpriteSetFlip(aSprite *as, u16 flip);

/**
*  \brief
*      Update animation of a single aSprite item
*  \param as
*      aSprite handle
*/
void aSpriteAnimateSingle(aSprite *as);

/**
*  \brief
*      Update animation of a list of aSprite items
*  \param list
*      pointer to aSprite list
*/
void aSpriteAnimateList(void *list);


// ========== sprite pools ==========
/**
*  \brief
*      Initialize a sprite pool handle
*  \param sp
*      spritePool handle
*  \param baseSprite
*      Base sprite #
*  \param size
*      Pool total sprite count
*  \param clearSprites
*      Pool sprites will be cleared if true.
*      (Sprites will be moved offscreen, tilemap is not cleared)
*/
void spritePoolInit(spritePool *sp, u16 baseSprite, u16 size, bool clearSprites);

/**
*  \brief
*      Render an object list into sprite pool
*  \param sp
*      spritePool handle
*  \param list
*      aSprite pointer list
*/
void spritePoolDrawList(spritePool *sp,void *list);

/**
*  \brief
*      Render an object list into sprite pool - IRQ safe version
*  \param sp
*      spritePool handle
*  \param list
*      aSprite pointer list
*/
void spritePoolDrawList2(spritePool *sp,void *list);

/**
*  \brief
*      Render an object list into sprite pool
*      Support scaling and new sprite format
*  \param sp
*      spritePool handle
*  \param list
*      aSprite pointer list
*/
void spritePoolDrawList3(spritePool *sp,void *list);

/**
*  \brief
*      Terminate operations on sprite pool for current frame
*  \param sp
*      spritePool handle
*/
void spritePoolClose(spritePool *sp);

// ========== color streams ==========
/**
*  \brief
*      Initialize a colorStream handle
*  \param cs
*      colorStream handle
*  \param basePalette
*      Starting palette
*  \param config
*      Start configuration (COLORSTREAM_STARTCONFIG/COLORSTREAM_ENDCONFIG)
*/
void colorStreamInit(colorStream *cs, const colorStreamInfo *csi, u16 basePalette, u16 config);

/**
*  \brief
*      Update a colorStream to set position
*  \param cs
*      colorStream handle
*  \param pos
*      New stream position
*/
void colorStreamSetPos(colorStream *cs, u16 pos);

#endif // __LIBNG_H__

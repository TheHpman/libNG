#include <neogeo.h>


/******************************************************************/
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-const-variable"
    const char libID[] = "libNG Ver 1.1 - Build:"__DATE__" "__TIME__;
#pragma GCC diagnostic pop


/**************************************
	VARIABLES
**************************************/
// vblank / frame counters
u8	libNG_vbl_flag;		// internal
vu32	libNG_droppedFrames;	// dropped frames
vu32	libNG_frameCounter;	//"real" frame count
void	*VBL_callBack;
void	*VBL_skipCallBack;
// 18 bytes

// draw lists
u32	SC1[SC1_BUFFER_SIZE];
u32	*SC1ptr;
u16	SC234[SC234_BUFFER_SIZE];
u16	*SC234ptr;
u32	PALJOBS[PAL_BUFFER_SIZE];
u32	*palJobsPtr;
u32	FIXJOBS[FIX_BUFFER_SIZE];
u32	*fixJobsPtr;
u16	libNG_drawListReady;
// default values will take about 10KB RAM

// timer interrupt stuff
_LSPCMODE_W	LSPCmode;	// LSPC mode (autoanim speed)
u16	libNG_TIfunc[34];	// irq code space, internal
u32	TIbase;			// timer base value
u32	TIreload;		// timer reload value
u16	*TInextTable;		// table for next frame
u32	TIcurrentBase;		// current frame base timer
u32	*TIcurrentData;		// current frame data
// 90 bytes

// scratchpads
char	libNG_scratchpad64[64];
char	libNG_scratchpad16[16];
// 80 bytes

// buffers for color math
#if COLORMATH_ENABLE
palette palBuffer[256];
palHandle palHandles[256];
u8 palTransferPending;
u8 palCommandPending;
u8 palFlushOnFrameSkip;
#endif
// ~9.5KB

#if SOUNDBUFFER_ENABLE
u8 sndBuffer[SOUNDBUFFER_SIZE];	// ring buffer
u16 sndBufferIndexRW;		// R & W indexes packed as u16
#endif

/**************************************
	FUNCTIONS
**************************************/

/****** base video stuff ******/
void disableIRQ()
{
	__asm__(
	    "ori.w #0x0700, %%sr \n"
	    :::"cc");
}

void enableIRQ()
{
	__asm__(
	    "andi.w #0xf8ff, %%sr \n"
	    :::"cc");
}

/***************************
	DRAW LISTS
*****************************/
void SCReset()
{
	SC1ptr = SC1;
	SC234ptr = SC234;
	palJobsPtr = PALJOBS;
	fixJobsPtr = FIXJOBS;
}

void SCClose()
{
	*SC1ptr = 0x00000000;
	// SC324 has no end marker
	*palJobsPtr = 0xffffffff;
	*fixJobsPtr = 0x00000000;
	libNG_drawListReady = 1;
}

void initGfx()
{
	libNG_drawListReady = 0;
	libNG_droppedFrames = 0;
	libNG_frameCounter = 0;
	SCReset();
	TIreload = TI_RELOAD;
	TInextTable = 0;
	unloadTIirq();
	// jobMeterSetup(true);
}

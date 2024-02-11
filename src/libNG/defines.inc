BIOS_SYSTEM_MODE	= 0x10fd80
BIOS_MESS_POINT		= 0x10fdbe
BIOS_MESS_BUSY		= 0x10fdc2
BIOS_DEVMODE		= 0x10fe80

REG_P1CNT		= 0x300000	;* P1 controller port
REG_DIPSW		= 0x300001	;* DSW / WDog
REG_WATCHDOG		= 0x300001	;* DSW / WDog
REG_P2CNT		= 0x340000	;* P2 controller port
REG_POUTPUT		= 0x380001	;* controllers outputs
REG_VRAM_ADDR		= 0x3c0000	;* VRAM address port
REG_VRAM_RW		= 0x3c0002	;* VRAM data port
REG_VRAM_MOD		= 0x3c0004	;* VRAM auto increment port
REG_LSPCMODE		= 0x3c0006
REG_TIMERHIGH		= 0x3c0008
REG_TIMERLOW		= 0x3c000a
REG_IRQACK		= 0x3c000c
REG_TIMERSTOP		= 0x3c000e

PAL_RAM			= 0x400000

SYSTEM_INT1		= 0xc00438
SYSTEM_IO		= 0xc0044a
MESS_OUT		= 0xc004ce

MAX_SPRITES		= 0x180		;* 384 sprites
YSHIFT 			= 496

BANKING_CMD		= (65*2)

;****************** picture ******************

;*typedef struct __attribute__((packed,aligned(2))) picture {
;*	u16		baseSprite;
;*	u8		basePalette;
;*	u8		RFU;
;*	s16		posX, posY;
;*	u16		currentFlip;
;*	pictureInfo *info;
;*	//14 bytes
;*} picture;

P_BASESPRITE		= 0
P_BASEPALETTE		= 2
P_RFU			= 3
P_POSX			= 4
P_POSY			= 6
P_CURRENTFLIP		= 8
P_PICTUREINFO		= 10


;*typedef struct pictureInfo {
;*	u16		stripSize;		//size of each strip (bytes)
;*	u16		tileWidth;
;*	u16		tileHeight;
;*	paletteInfo	*palInfo;
;*	u16		*maps[4];		//ptrs to maps (std/flipX/flipY/flipXY)
;*} pictureInfo;

PI_COLSIZE		= 0
PI_TILEWIDTH		= 2
PI_TILEHEIGHT		= 4
PI_PALINFO		= 6
PI_MAPS			= 10


;****************** animated sprite ******************

;*typedef struct sprFrame {
;*	u16		tileWidth;
;*	u16		tileHeight;
;*	u16		stripSize;
;*	u16		*maps[4];
;*} sprFrame;

|; code bound
FRAME_TILEWIDTH		= 0		;* code bound - .l read
FRAME_TILEHEIGHT	= 2		;* code bound - .l read
FRAME_COLSIZE		= 4
FRAME_MAPS		= 6
;* new format
FRAME2_SPRCOUNT		= 2


;*typedef struct animStep {
;*	sprFrame	*frame;
;*	s16		flipShiftX;
;*	s16		shiftX;
;*	s16		flipShiftY;
;*	s16		shiftY;
;*	u16		duration;
;*} animStep;

STEP_FRAME		= 0
STEP_FLIPSHIFTX		= 4
STEP_SHIFTX		= 6
STEP_FLIPSHIFTY		= 8
STEP_SHIFTY		= 10
STEP_DURATION		= 12

STEP_BYTESIZE		= 14

	.macro	_COMPUTE_STEP_ rd ra
		add.w	\rd, \rd
		sub.w	\rd, \ra
		lsl.w	#3, \rd
		add.w	\rd, \ra	;* -2 +16 = 14
	.endm

;*typedef struct spriteInfo {
;*	u16		frameCount;
;*	u16		maxWidth;
;*	paletteInfo	*palInfo;
;*	animStep	**anims;
;*	sprFrame	frames[0];
;*} spriteInfo;

;*SI_PALCOUNT		= 0
SI_FRAMECOUNT		= 0
SI_MAXWIDTH		= 2
SI_PALINFO		= 4
SI_ANIMS		= 8
SI_FRAMES		= 12

;*typedef struct __attribute__((packed,aligned(2))) aSprite {
;*	u16		baseSprite;		//
;*	u8		basePalette;
;*	u8		RFU;
;*	s16		posX, posY;		//
;*	u16		animID;
;*	u16		currentAnim;
;*	u16		stepNum;		//current step number
;*	animStep	*anims;			// anims bank
;*	animStep	*steps;			// steps bank of current anim (variable)
;*	animStep	*currentStep;		// current step
;*	sprFrame	*currentFrame;		// current frame
;*	u32		counter;		// frame update counter
;*	u16		repeats;		// repeats played
;*	u16		tileWidth;		//	remove?
;*	u16		currentFlip;		//
;*	u16		flags;			//flags (moved/flipped etc)
;*	//42 bytes
;*	u8		Xbig, Ybig;
;*	//44 bytes
;*} aSprite;

AS_BASESPRITE		= 0	;* expected to be 0
AS_BASEPALETTE		= 2
AS_BANK			= 3
AS_POSX			= 4
AS_POSY			= 6
AS_ANIMID		= 8
AS_CURRENTANIMID	= 10
AS_CURRENTSTEPNUM	= 12
AS_ANIMS		= 14
AS_STEPS		= 18
AS_CURRENTSTEP		= 22
AS_CURRENTFRAME		= 26
AS_COUNTER		= 30
AS_REPEATS		= 32
AS_TILEWIDTH		= 34		;* for fixed allocation
AS_CURRENTFLIP		= 36
AS_FLAGS		= 38
AS_XBIG			= 40
AS_YBIG			= 41


;*	//flags & masks

;* !! BYTE USAGE !!
AS_FLAG_MOVED		= 0x01
AS_FLAG_FLIPPED		= 0x02
AS_FLAG_STRICTCOORDS	= 0x40
AS_FLAG_NODISPLAY	= 0x80

AS_MASK_MOVED		= 0xfe
AS_MASK_FLIPPED		= 0xfd
AS_MASK_MOVED_FLIPPED	= 0xfc
AS_MASK_STRICTCOORDS	= 0xbf
AS_MASK_NODISPLAY	= 0x7f

;* flags byte 0
B_AS_ANIMSTOP		= 0x07

;* flags byte1
B_AS_MOVED		= 0x00
B_AS_FLIPPED		= 0x01
B_AS_STRICTCOORDS	= 0x06
B_AS_NODISPLAY		= 0x07



;****************** sprite pools ******************

;*typedef struct spritePool {
;*	u16		poolStart;
;*	u16		poolEnd;
;*	u16		poolSize;
;*	u16		way;
;*	u16		currentUp;
;*	u16		currentDown;
;*} spritePool;

SPOOL_POOLSTART		= 0
SPOOL_POOLEND		= 2
SPOOL_POOLSIZE		= 4
SPOOL_WAY		= 6
SPOOL_CURRENTUP		= 8
SPOOL_CURRENTDOWN	= 10

WAY_UP			= 0
WAY_DOWN		= 1


;****************** color streams ******************

;*typedef struct colorStreamInfo {
;*	u16		palSlots;
;*	void		*startConfig;
;*	void		*endConfig;
;*	void		*fwData;
;*	void		*fwDataEnd;
;*	void		*bwData;
;*	void		*bwDataEnd;
;*} colorStreamInfo;

CSI_PALSLOTS		= 0
CSI_STARTCONFIG		= 2
CSI_ENDCONFIG		= 6
CSI_FWDATA		= 10
CSI_FWDATAEND		= 14
CSI_BWDATA		= 18
CSI_BWDATAEND		= 22

;*typedef struct colorStreamJob {
;*	u16		coord;
;*	void		*data;
;*} colorStreamJob;

CSJOB_COORD		= 0
CSJOB_DATA		= 2
SIZEOF_CSJOB		= 6

;*//color stream handler
;*typedef struct colorStream {
;*	u16		basePalette;		//palmod: pal*32
;*	u16		position;
;*	colorStreamInfo	*info;
;*	colorStreamJob	*fwJob;
;*	colorStreamJob	*bwJob;
;*} colorStream;

CS_PALMOD		= 0
CS_POSITION		= 2
CS_INFO			= 4
CS_FWJOB		= 8
CS_BWJOB		= 12

;* animation macro
;* rd0, rd1, ra1: scratch data/addr registers
;* ra0:	addr register, holds AS ptr
	.macro	___AS_ANIMATION_BLOCK___ rd0 rd1 ra0 ra1
		tst.b	AS_FLAGS(\ra0)						;* test anim stop flag		12
		bmi.s	700f							;*				8/10
		subq.w	#1, AS_COUNTER(\ra0)					;*				16
		bne.s	700f	;* anim_done					;*(unsigned compare)		8 not taken, 10 taken

		;*must move to next step/repeat/link
		move.l	AS_CURRENTSTEP(\ra0), \ra1				;*				16
		lea	STEP_BYTESIZE(\ra1), \ra1				;*a0=next step			8

100:		move.b	(\ra1), \rd1						;*				8
		bpl.s	200f	;* anim_do_next_step
		add.b	\rd1, \rd1
		beq.s	300f	;* anim_do_repeat
		bmi.s	400f	;* anim_ended

 		;* anim_do_link:
		addq.w	#2, \ra1						;*				4
		move.w	(\ra1)+, AS_CURRENTANIMID(\ra0)				;* update current anim id	16
		move.l	(\ra1), \ra1						;*				12
		move.l	\ra1, AS_STEPS(\ra0)					;*				16
		move.l	\ra1, AS_CURRENTSTEP(\ra0)				;*				16
		moveq	#0, \rd1						;*always 0			4
		move.w	\rd1, AS_REPEATS(\ra0)					;*				12
		move.w	\rd1, AS_CURRENTSTEPNUM(\ra0)				;*				12
		bra.s	600f	;* anim_common_block				;*				10

300:		;* anim_do_repeat:
		move.w	AS_REPEATS(\ra0), \rd1					;*d1=as->repeats		12
		addq.w	#2, \ra1
		cmp.w	(\ra1)+, \rd1						;*				12
		bhs.s	100b	;* repeats done
		;* repeating
		addq.w	#1, \rd1						;*				4
		move.w	\rd1, AS_REPEATS(\ra0)					;*				12
		move.l	AS_STEPS(\ra0), \ra1					;*				16
		move.l	\ra1, AS_CURRENTSTEP(\ra0)				;*				16
		clr.w	AS_CURRENTSTEPNUM(\ra0)					;*	saves 2 bytes		16
		bra.s	600f	;* anim_common_block				;*				10

400:		;* anim_ended:
		bset.b	#B_AS_ANIMSTOP, AS_FLAGS(\ra0)				;*				20
		bra.s	700f	;* anim_done					;*step didn't change

200:		;* anim_do_next_step:
		move.l	\ra1, AS_CURRENTSTEP(\ra0)				;*				16
		addq.w	#1, AS_CURRENTSTEPNUM(\ra0)				;*				16
	;*	bra.s	600f	;* anim_common_block				;*				10

		;*animating section
600:		;* anim_common_block:						;*a0=as a1=as->currentanim d0=libNG_frameCounter
		move.w	STEP_DURATION(\ra1), AS_COUNTER(\ra0)			;*				20

		;* STEP_FRAME is 0, hence (\ra1)
		move.l	(\ra1), AS_CURRENTFRAME(\ra0)				;*update frame ptr
;*		move.w	FRAME_TILEWIDTH(\ra1), AS_TILEWIDTH(\ra0)		;*update tilewidth

700:
	.endm

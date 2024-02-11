	.include "src/libNG/defines.inc"
	#include <configNG.h>
	.align	2

	.set	_METER_COLOR, (PAL_RAM+(((JOB_METER_PALETTE*16)+JOB_METER_COLOR)*2))

;*VRAM format:
;*
;* 	0x0000
;* 	fedcba9876543210
;* 	tile # LSB
;*
;* 	fedcba98	7654		3	2	1	0
;* 	palette		tile MSB	Anim8	Anim4	flipY	flipX
;* 
;*	0x7000
;*	fedc	ba98 76543210
;*	pal	character #
;* 
;* 	0x8000
;* 	fedc	ba98		76543210
;* 	xxxx	h shrink	v shrink
;* 
;* 	0x8200
;* 	fedcba987	6	543210
;* 	posY		link	nb tiles
;* 
;* 	0x8400
;* 	fedcba987	6543210
;* 	posX		xxxxxxx


;* ******************************************************************************
;* 				biosCall(u32 sub)
;* 				biosCall2(u32 sub, u32 arg)
;* ******************************************************************************
.globl	biosCall
.globl	biosCall2
	.set	_ARGS, 4
	.set	_sub, _ARGS
	.set	_arg, _ARGS+4
biosCall2:
		move.l	_arg(sp), d0
		move.l	d0, a0			;* place arg in both a0 & d0
		;* falls through
biosCall:
		move.l	_sub(sp), a1
		movem.l	d2-d7/a2-a6, -(sp)
		jsr	(a1)
		movem.l	(sp)+, d2-d7/a2-a6
		rts


;* ******************************************************************************
;* 				addMessage(u16 *message)
;* ******************************************************************************
.globl	addMessage
	.set	_ARGS, 4
	.set	_msg, _ARGS
addMessage:
	;*	addq.b	#1, BIOS_MESS_BUSY

	;*	move.l	BIOS_MESS_POINT, a0
	;*	move.l	_msg(sp), (a0)+
	;*	move.l	a0, BIOS_MESS_POINT

	;*	subq.b	#1, BIOS_MESS_BUSY
	;*	rts

		;* 18cy. faster
		lea	BIOS_MESS_BUSY, a1
		addq.b	#1, (a1)		;* busy++
		move.l	-(a1), a0		;* read buffer ptr
		move.l	_msg(sp), (a0)+		;* store data addr
		move.l	a0, (a1)+		;* write back buffer ptr
		subq.b	#1, (a1)		;* busy--
		rts



;* ******************************************************************************
;* 				jobMeterSetup(bool setdebug)
;* ******************************************************************************
.globl jobMeterSetup
.globl jobMeterSetup2
	.set	_ARGS, 4
	.set	_debug, _ARGS+2		;* word

jobMeterSetup2:
		lea	REG_VRAM_ADDR, a0
		move.l	#0x74e00000+((JOB_METER_PALETTE&0xf)<<12)+JOB_METER_CHARACTER, d1	;* fix last column 74e0, character
		moveq	#32-1, d0					;* 32 lines (-1)

0:		move.l	d1, (a0)					;* write plain tile / palette 0xf
		add.l	#0x10000, d1
		dbra	d0, 0b						;* loop
		bra.s	5f

jobMeterSetup:
		lea	REG_VRAM_RW, a0
		move.w	#1, 2(a0)					;* +1 increments
		move.w	#0x74e0, -2(a0)					;* fix last column 74e0
		moveq	#32-1, d0					;* 32 lines

0:		move.w	#((JOB_METER_PALETTE&0xf)<<12)+JOB_METER_CHARACTER, (a0)	;* write character
		dbra	d0, 0b						;* loop

5:		tst.w	_debug(sp)
		beq.s	9f						;* false? => end

		bset.b	#0, 0x10fe35					;* set unibios devmode flag.
		move.w	0x108.w, 0x10fe38				;* set game id for current game (needed to detect game switch on multislot).
		bset.b	#0, 0x10fe37					;* set unibios debug dip 2-1

		move.l	#0x31737472, 0x10fe80				;* sets dev mode string "1streset"
		move.l	#0x65736574, 0x10fe84				;* sets dev mode string "1streset"

		move.l	0x10e.w, a0
		bset.b	#0, 1(a0)					;* sets dip 2-1

9:		rts


;* ******************************************************************************
;* 				clearSprites
;* ******************************************************************************

.globl clearSprites
clearSprites:
	.set	_ARGS, 4
	.set	_spr, _ARGS+2
	.set	_count, _ARGS+4+2
	
		move.w	_count(sp), d0					;* d1=count			12
		beq.s	9f						;* count==0?			8 not taken, 10 taken

		movea.l	SC234ptr, a0					;* a0=queue ptr			20
		subq.w	#1, d0						;* d0=loop			4
		move.l	#0x10000, a1					;* a1=next spr mod		12	swap+12

		move.l	#0x88008200, d1					;* data, addr			12
		add.w	_spr(sp), d1					;* addr+=spr#			12
		swap	d1						;* d1=addr/data			4

		move.l	d1, (a0)+					;* queue			12
		dbra	d0, 0f						;* loop				10 taken, 14 not
		move.l	a0, SC234ptr					;* update ptr			20
		rts

0:		add.l	a1, d1						;* next spr			8	swap+4
		move.l	d1, (a0)+					;* queue			12
		dbra	d0, 0b						;* loop				10 taken, 14 not
		move.l	a0, SC234ptr					;* update ptr			20
9:		rts


;* ******************************************************************************
;* 				SETUP 4P
;* ******************************************************************************
.globl setup4P

delay_out:	;* as in bios
		bsr.s	delay_6
		bsr.s	delay_6
		bsr.w	delay_6
delay_6:
	.rept	6
		nop
	.endr
		rts

setup4P:
		moveq	#0, d0
		moveq	#0, d1
		lea	REG_POUTPUT, a0					;* a0=OUTs
		move.b	#0x04, (a0)					;* outs= 000 100
		bsr.s	delay_out
		move.b	REG_P1CNT, d0
		not.w	d0
		move.b	REG_P2CNT, d1
		not.w	d1
		and.w	d1, d0
		and.w	#0x10, d0					;* A pressed on both ?
		beq.s	9f						;* 0= not pressed =bye
	
		move.b	#0x05, (a0)					;* outs= 000 101
		bsr.s	delay_out
		move.b	REG_P1CNT, d0
		not.w	d0
		move.b	REG_P2CNT, d1
		not.w	d1
		and.w	d1, d0
		and.w	#0x20, d0					;* B pressed on both ?
		beq.s	9f						;* 0= not pressed =bye

		move.b	#0xff, 0x10fefa					;* sets bios 4p flag (need to? which byte?)
		move.b	#2, 0x10fda0					;* p3=expanded
		move.b	#2, 0x10fda6					;* p4=expanded
		moveq	#1, d0						;* return 1

0:		move.b	#0x00, (a0)					;* outs=0
		rts

9:		moveq	#0, d0						;* return 0
		bra.s	0b


;* ******************************************************************************
;* 				CLEAR FIX LAYER
;* ******************************************************************************
.globl clearFixLayer
.globl clearFixLayer2
.globl clearFixLayer3

clearFixLayer:
		lea	REG_VRAM_RW, a0
		move.w	#0x0001, 2(a0)					;* +1 increments
		move.w	#0x7000, -2(a0)					;* fix start
		move.w	#FIX_CLEAR_CHARACTER, d1
		move.w	#0x4f, d0					;* 4f  80*16 = 1280 chars

0:		move.w	d1, (a0)					;*				8
	.rept	15
		nop							;*				4
		move.w	d1, (a0)					;*				8
	.endr
		dbra	d0, 0b
		rts

;* IRQ safe version
clearFixLayer2:
		lea	REG_VRAM_ADDR, a0				;*				12
		move.l	#0x70000000+FIX_CLEAR_CHARACTER, d1		;* vram addr & data		12
		lea	0x10000, a1					;* vram increment		12
		moveq	#0x4f, d0					;* 80*16 = 1280 chars		8

0:	
	.rept	16
		move.l	d1, (a0)					;*				12
		add.l	a1, d1						;*				8
	.endr
		dbra	d0, 0b
		rts

;* VSynced version
clearFixLayer3:
		movea.l	fixJobsPtr, a0
		move.l	#0x70010020, d0
		moveq	#1, d1
		swap	d1
		lea	_fixBlankLine(pc), a1

	.rept	29
		move.l	d0, (a0)+
		move.l	a1, (a0)+
		add.l	d1, d0
	.endr
		move.l	d0, (a0)+
		move.l	a1, (a0)+
		move.l	a0, fixJobsPtr
		rts

.globl	_fixBlankLine
_fixBlankLine:
	.rept	40
		.word	FIX_CLEAR_CHARACTER
	.endr
		.word	0x0000


;* ******************************************************************************
;* 				unloadTIirq
;* ******************************************************************************

.globl unloadTIirq
unloadTIirq:
		lea	libNG_TIfunc, a0
		move.w	sr, d0
		move.w	#0x2700, sr
		move.l	#0x33fc0002, (a0)+				;* move.w #0x0002,
		move.l	#REG_IRQACK, (a0)+				;*		REG_IRQACK
		move.w	#0x4e73, (a0)+					;* rte
		move.w	d0, sr
		rts


;* ******************************************************************************
;* 				loadTIirq
;* ******************************************************************************

;* void loadTIirq(ushort mode);
.globl loadTIirq
loadTIirq:
	.set	_ARGS, 4
	.set	_mode, _ARGS+2

		lea	libNG_TIfunc, a0				;*				12
		move.w	_mode(sp), d1					;* d1=mode			12
		add.w	d1, d1						;*				4
		move.w	TI_SIZES(d1.w, pc), d0				;* d0=count			14

		add.w	d1, d1						;*				4
		move.l	TI_DATA(d1.w, pc), a1				;* a1=func data			18

		move.w	sr, d1
		move.w	#0x2700, sr
0:		move.w	(a1)+, (a0)+					;* copy data			12
		dbra	d0, 0b						;*				10
		move.w	d1, sr
		rts							;*				16

TI_SIZES:
	.word	((TI_SINGLE_DATA_END - TI_SINGLE_DATA)/2)-1, ((TI_DUAL_DATA_END - TI_DUAL_DATA)/2)-1
TI_DATA:
	.long	TI_SINGLE_DATA, TI_DUAL_DATA

TI_SINGLE_DATA:	;* 30 words
;* (230)							;*				26-44 for irq
		move.l	a0, -(sp)					;*				12
		move.l	USP, a0						;*				4		//pre=42/21
		move.l	(a0)+, REG_VRAM_ADDR				;*				28
		tst.l	(a0)						;*				12
		beq.s	9f						;*				8 not taken, 10 taken
		move.l	a0, USP						;*				4
		;* next data ok
		move.w	#2, REG_IRQACK					;*				20
		move.l	(sp)+, a0					;*				12
		rte							;*				20		//158/79	120+irq
9:		;* next data was empty
		exg	d0, a0						;*				6
		move.w	LSPCmode, d0					;*				16
		andi.w	#0xff0f, d0					;*				8
		move.w	d0, REG_LSPCMODE				;*				16
		exg	d0, a0						;*				6
		move.w	#2, REG_IRQACK					;*				20
		move.l	(sp)+, a0					;*				12
		rte							;*				20
TI_SINGLE_DATA_END:

TI_DUAL_DATA:	;* 33 words					;*				26-44 for irq
		move.l	a0, -(sp)					;*				12
		move.l	USP, a0						;*				4
		move.l	(a0)+, REG_VRAM_ADDR				;*				28
		move.l	(a0)+, REG_VRAM_ADDR				;*				28
		tst.l	(a0)						;*				12
		beq.s	9f						;*				8 not taken, 10 taken
		move.l	a0, USP						;*				4
		;* next data ok             				;*			
		move.w	#2, REG_IRQACK					;*				20
		move.l	(sp)+, a0					;*				12
		rte							;*				20		//186/93
9:		;* next data was empty      				;*			
		exg	d0, a0						;*				6
		move.w	LSPCmode, d0					;*				16
		andi.w	#0xff0f, d0					;*				8
		move.w	d0, REG_LSPCMODE				;*				16
		exg	d0, a0						;*				6
		move.w	#2, REG_IRQACK					;*				20
		move.l	(sp)+, a0					;*				12
		rte							;*				20
TI_DUAL_DATA_END:



;* ******************************************************************************
;* 				libNG_vblank subs
;* ******************************************************************************
;* SC1 processing jumptable (6 bits = 64 entries)

SC1table:
	.word	SC1writeEnd-SC1table	;* 00
	.word	SC1_01tile-SC1table, SC1_02tile-SC1table, SC1_03tile-SC1table, SC1_04tile-SC1table		;* 01-04
	.word	SC1_05tile-SC1table, SC1_06tile-SC1table, SC1_07tile-SC1table, SC1_08tile-SC1table		;* 05-08
	.word	SC1_09tile-SC1table, SC1_10tile-SC1table, SC1_11tile-SC1table, SC1_12tile-SC1table		;* 09-12
	.word	SC1_13tile-SC1table, SC1_14tile-SC1table, SC1_15tile-SC1table, SC1_16tile-SC1table		;* 13-16
	.word	SC1_17tile-SC1table, SC1_18tile-SC1table, SC1_19tile-SC1table, SC1_20tile-SC1table		;* 17-20
	.word	SC1_21tile-SC1table, SC1_22tile-SC1table, SC1_23tile-SC1table, SC1_24tile-SC1table		;* 21-24
	.word	SC1_25tile-SC1table, SC1_26tile-SC1table, SC1_27tile-SC1table, SC1_28tile-SC1table		;* 25-28
	.word	SC1_29tile-SC1table, SC1_30tile-SC1table, SC1_31tile-SC1table, SC1_32tile-SC1table		;* 29-32
	.word	SC1_01tile_S-SC1table, SC1_02tile_S-SC1table, SC1_03tile_S-SC1table, SC1_04tile_S-SC1table	;* 33-36
	.word	SC1_05tile_S-SC1table, SC1_06tile_S-SC1table, SC1_07tile_S-SC1table, SC1_08tile_S-SC1table	;* 37-40
	.word	SC1_09tile_S-SC1table, SC1_10tile_S-SC1table, SC1_11tile_S-SC1table, SC1_12tile_S-SC1table	;* 41-44
	.word	SC1_13tile_S-SC1table, SC1_14tile_S-SC1table, SC1_15tile_S-SC1table, SC1_16tile_S-SC1table	;* 45-48
	.word	SC1_17tile_S-SC1table, SC1_18tile_S-SC1table, SC1_19tile_S-SC1table, SC1_20tile_S-SC1table	;* 49-52
	.word	SC1_21tile_S-SC1table, SC1_22tile_S-SC1table, SC1_23tile_S-SC1table, SC1_24tile_S-SC1table	;* 53-56
	.word	SC1_25tile_S-SC1table, SC1_26tile_S-SC1table, SC1_27tile_S-SC1table, SC1_28tile_S-SC1table	;* 57-60
	.word	SC1_29tile_S-SC1table, SC1_30tile_S-SC1table, SC1_31tile_S-SC1table, SC1_32tile-SC1table	;* 61-64 (fall back to regular for 32 sized)
	.word	SC1_setBank-SC1table	;* 65


	.macro	_SC1_PROCESS_BLOCK_
		move.l	(a2)+, d0					;* d0=pal/size/addr		12
		_SC1_PROCESS_SUB_BLOCK_
	.endm

	.macro	_SC1_PROCESS_SUB_BLOCK_					;* sub block for data rdy in d0
	#if	BANKING_ENABLE
		move.b	(a2), (a4)
	#endif
		move.l	(a2)+, a3					;* a3=spr data addr		12
		move.w	d0, (a5)					;* set VRAM addr		8
		swap	d0						;*				4
		move.b	d0, d1						;* d1=tile size*4		4
		clr.b	d0						;* d0=pal mod (upper byte)	4
		move.w	(a0, d1.w), d2					;* compute jump			14
		jmp	(a0, d2.w)					;*				14
	.endm

	.macro	_WRITE_TILE_
		move.w	(a3)+, (a6)					;* write tile lsb		12
		move.w	(a3)+, d2					;* 				8
		add.w	d0, d2						;* add palette mod		4
		move.w	d2, (a6)					;* write pal/msb/anim/flip	8	32
	.endm

;*SC1_32tile_S:	_WRITE_TILE_
SC1_31tile_S:	_WRITE_TILE_
SC1_30tile_S:	_WRITE_TILE_
SC1_29tile_S:	_WRITE_TILE_
SC1_28tile_S:	_WRITE_TILE_
SC1_27tile_S:	_WRITE_TILE_
SC1_26tile_S:	_WRITE_TILE_
SC1_25tile_S:	_WRITE_TILE_
SC1_24tile_S:	_WRITE_TILE_
SC1_23tile_S:	_WRITE_TILE_
SC1_22tile_S:	_WRITE_TILE_
SC1_21tile_S:	_WRITE_TILE_
SC1_20tile_S:	_WRITE_TILE_
SC1_19tile_S:	_WRITE_TILE_
SC1_18tile_S:	_WRITE_TILE_
SC1_17tile_S:	_WRITE_TILE_
SC1_16tile_S:	_WRITE_TILE_
SC1_15tile_S:	_WRITE_TILE_
SC1_14tile_S:	_WRITE_TILE_
SC1_13tile_S:	_WRITE_TILE_
SC1_12tile_S:	_WRITE_TILE_
SC1_11tile_S:	_WRITE_TILE_
SC1_10tile_S:	_WRITE_TILE_
SC1_09tile_S:	_WRITE_TILE_
SC1_08tile_S:	_WRITE_TILE_
SC1_07tile_S:	_WRITE_TILE_
SC1_06tile_S:	_WRITE_TILE_
SC1_05tile_S:	_WRITE_TILE_
SC1_04tile_S:	_WRITE_TILE_
SC1_03tile_S:	_WRITE_TILE_
SC1_02tile_S:	_WRITE_TILE_
SC1_01tile_S:	_WRITE_TILE_
	;* blank checks
		move.w	#2, 2(a6)					;* VRAM modulo			16
0:		tst.w	(a6)						;*				8
		beq.s	1f						;*				8 not taken / 10 taken
		move.w	d3, (a6)					;*				8
		nop							;*need 16 cycles?		4
		nop							;*				4
		bra.s	0b						;*				10
1:		move.w	#1, 2(a6)					;* VRAM modulo			16

		_SC1_PROCESS_BLOCK_


SC1_32tile:	_WRITE_TILE_
SC1_31tile:	_WRITE_TILE_
SC1_30tile:	_WRITE_TILE_
SC1_29tile:	_WRITE_TILE_
SC1_28tile:	_WRITE_TILE_
SC1_27tile:	_WRITE_TILE_
SC1_26tile:	_WRITE_TILE_
SC1_25tile:	_WRITE_TILE_
SC1_24tile:	_WRITE_TILE_
SC1_23tile:	_WRITE_TILE_
SC1_22tile:	_WRITE_TILE_
SC1_21tile:	_WRITE_TILE_
SC1_20tile:	_WRITE_TILE_
SC1_19tile:	_WRITE_TILE_
SC1_18tile:	_WRITE_TILE_
SC1_17tile:	_WRITE_TILE_
SC1_16tile:	_WRITE_TILE_
SC1_15tile:	_WRITE_TILE_
SC1_14tile:	_WRITE_TILE_
SC1_13tile:	_WRITE_TILE_
SC1_12tile:	_WRITE_TILE_
SC1_11tile:	_WRITE_TILE_
SC1_10tile:	_WRITE_TILE_
SC1_09tile:	_WRITE_TILE_
SC1_08tile:	_WRITE_TILE_
SC1_07tile:	_WRITE_TILE_
SC1_06tile:	_WRITE_TILE_
SC1_05tile:	_WRITE_TILE_
SC1_04tile:	_WRITE_TILE_
SC1_03tile:	_WRITE_TILE_
SC1_02tile:	_WRITE_TILE_
SC1_01tile:	_WRITE_TILE_
		;*  target
		_SC1_PROCESS_BLOCK_


SC1_setBank:
		swap	d0						;* restore lower word
		move.b	d0, REG_BANKING					;* set bank
		move.l	a3, d0						;* d0=pal/size/addr (prefetched in a3)
		_SC1_PROCESS_SUB_BLOCK_


;* /******************************************************************************
;* 				libNG_vblankTI
;* ******************************************************************************/

TI_splash_screen:
		jmp	SYSTEM_INT1					;* bios splash screen
	
;* *** main vblank function, sets up LSPC & data for IRQ then branch to regular vblank
.globl libNG_vblankTI
libNG_vblankTI:								;*		irq rising =	26~44
		tst.b	BIOS_SYSTEM_MODE				;*				16
		bpl.s	TI_splash_screen				;*				8 not taken, 10 taken
		movem.l	d0-d7/a0-a6, -(sp)				;*				128

		tst.w	libNG_drawListReady				;* draw list rdy?		16
		bne.s	newData						;* nope? use old data		10 taken, 8 if not

oldData:	;* retain current data - this will preserve raster when frame skipping
		nop							;*				4
		nop							;*				4
		nop							;*				4
		move.l	TIcurrentData, a2				;*				20
		move.l	TIcurrentBase, d2				;*				20
		bra.s	dataReady					;*				10	//70
	
newData:	;* load new data, update current
		move.l	TInextTable, a2					;*				20
		move.l	TIbase, d2					;*				20
		move.l	a2, TIcurrentData				;*				20	//70
;* !BOTH PATH TO HERE MUST BE SAME CYCLES COUNT!
dataReady:
		move.l	d2, TIcurrentBase				;*				20

;* d0=lspc mode
;* d2=basetimer
;* a1=lspc
;* a2=data table

		move.w	LSPCmode, d0					;*				16	keep before branch
		lea	REG_VRAM_ADDR, a1				;*				12	keep before branch
		move.l	a2, d1						;*				4
		beq.s	9f						;* check ptr value		8 not taken, 10 taken		//0x00 ptr, disable

		ori.w	#0x00b0, d0					;*				8	//load when lowwrite, load when 0
		move.w	d0, (6, a1)					;* sets LSPC mode		12

		swap	d2						;*				4
		move.w	d2, (8, a1)					;* MSB				12
		swap	d2						;*				4
		move.w	d2, (0xa, a1)					;* LSB				12	//timer loaded & running <-- couting from here (376~394)

		andi.w	#0xff9f,d0					;*				8	//load when 0
		move.w	d0, (6,a1)					;* sets mode			12
		move.w	TIreload, (8, a1)				;* MSB				24
		move.w	TIreload+2, (0xa, a1)				;* LSB				24	//reload value

		;* setup data ptr in USP
		tst.l	(a2)						;* 1st data is 0?		12
		beq.s	9f						;*				8 not taken, 10 taken
		move.l	a2, USP						;*				4
		bra.s	libNG_vblank_fromTI				;*				10

		;* null ptr or empty data, disable timer
9:		andi.w	#0xff0f, d0					;* disable timer		8
		move.w	d0, (6, a1)					;* sets mode			12
		bra.s	libNG_vblank_fromTI				;*				10


;* /******************************************************************************
;* 				libNG_vblank
;* ******************************************************************************/
splash_screen:
		jmp	SYSTEM_INT1					;* bios splash screen
frameskip:
		add.l	#1, libNG_droppedFrames
		jsr	SYSTEM_IO
		move.l	VBL_skipCallBack, a0
		bra.w	VBL_end						;*				10

.globl libNG_vblank
libNG_vblank:
		tst.b	BIOS_SYSTEM_MODE				;*				16
		bpl.s	splash_screen					;*				8 not taken, 10 taken
		movem.l	d0-d7/a0-a6, -(sp)				;*				128

		;* user control
libNG_vblank_fromTI:
		tst.w	libNG_drawListReady				;* draw list rdy?		16
		beq.s	frameskip					;* is 0, nope, not ready to draw!! frame skipping!! ohnoez!!	//10 taken, 8 if not

		tst.b	BIOS_DEVMODE					;* dev mode?			16
		beq.s	0f						;*				8 not taken, 10 taken
		move.l	0x10e.w, a4					;* backup ram ptr
		btst	#0, (1, a4)					;* dip 2-1 ?
		beq.s	0f
		move.w	#0x4f00, _METER_COLOR				;* JOB_RED			20

0:		lea	REG_VRAM_ADDR, a5				;* a0=vram addr			12
		lea	2(a5), a6					;* a6=vram RW			8
		move.w	#1, 2(a6) 					;* vram modulo			16


		;*** process SC1 jobs *********************
		lea	SC1table, a0					;* a0=offset table		12
		lea	SC1, a2						;* a2=buffer			12
	#if	BANKING_ENABLE
		lea	REG_BANKING, a4
	#endif
		moveq	#0, d1						;* must clear d1		4
		moveq	#0, d3						;* 0 for scaled blanks		4

		_SC1_PROCESS_BLOCK_
SC1writeEnd:
		;******************************************



		;*** process SC234 jobs *********************
SC234jobs:
		move.l	SC234ptr, d1					;* d1=ptr			20
		lea	SC234, a2					;* a2=data table		12
		sub.l	a2, d1						;* d1-=table start		8
		lsr.w	#2, d1						;* d1=data count ($/4)		8
		beq.s	SC234jobs_end					;* 0 data=> end			8 not taken, 10 taken

		moveq	#32, d3
		moveq	#16, d2
	;*	cmpi.w	#32, d1						;*				8
		cmp.w	d3, d1						;*				4
		bge.s	SC234_32					;*				10 taken, 8 if not
	;*	cmpi.w	#16, d1						;*				8
		cmp.w	d2, d1						;*				4
		bge.s	SC234_16					;*				10 taken, 8 if not
		bra.s	SC234_finish					;*				10

SC234_32:
		sub.w	d2, d1						;*				4
	.rept	16
		move.l	(a2)+, (a5)					;* write addr+data		20
	.endr
SC234_16:
		sub.w	d2, d1						;*				4
	.rept	16
		move.l	(a2)+, (a5)					;* write addr+data		20
	.endr
	;*	cmpi.w	#32, d1						;*				8
		cmp.w	d3, d1						;*				4
		bge.s	SC234_32					;*				10 taken, 8 if not
	;*	cmpi.w	#16, d1						;*				8
		cmp.w	d2, d1						;*				4
		bge.s	SC234_16					;*				10 taken, 8 if not

SC234_finish:
		subq.w	#1, d1						;*				4
		bmi.s	SC234jobs_end					;* 				10 if taken, 8 if not
0:		move.l	(a2)+, (a5)					;* write addr+data		20
		dbra	d1, 0b						;*				10 taken 14 not / total 42
		;* 60 + x*42
SC234jobs_end:
		;******************************************



		;*** process palette jobs ******************
PALjobs:
	#if	BANKING_ENABLE
		lea	REG_BANKING, a0
	#endif
		lea	PALJOBS, a4					;* a4=buffer			12
		move.l	#PAL_RAM, d1					;* pal ram base			12

0:		move.l	(a4)+, d0					;* d0=count/ pal#		12
		bmi.s	PALjobs_end					;* ==ffffffffff? => end		8 not taken, 10 taken
1:
	#if	BANKING_ENABLE
		move.b	(a4), (a0)					;* bankswitch			12
	#endif
		move.l	(a4)+, a2					;* a2=data ptr			12
		move.w	d0, d1						;*				4
		move.l	d1, a3						;* a3=pal addr			4
		swap	d0						;* d0=pal count-1		4
3:
	.rept	8
		move.l	(a2)+, (a3)+					;* copy palette			20*8=160
	.endr
		dbra	d0, 3b						;* loop				10 taken, 14 if not
		move.l	(a4)+, d0					;* d0 = next count/pal#		12
		bpl.s	1b						;* ==ffffffffff? => end		8 not taken, 10 taken
PALjobs_end:
		;******************************************



		;*** process fix jobs *********************
FIXjobs:
		;*a5 vram addr
		;*a6 vram RW
		lea	FIXJOBS, a2					;* a2=jobs
0:		move.w	(a2)+, (a5)					;* set vram addr		12
		beq.s	FIXjobs_end					;* end if 0x0000		8 not taken, 10 taken

		move.w	(a2)+, d0					;* d0=pal/mod			8
		move.w	d0, d1						;*				4
		ext.w	d1						;* d1=mod			4
		move.w	d1, 2(a6)					;* set vram mod			12

		clr.b	d0						;* keep basepal			4
		move.l	(a2)+, a3					;* a3=data			12

1:
	.rept	8
		move.w	(a3)+, d1					;* read fixmap			8
		beq.s	0b						;* end if 0x0000		8 not taken, 10 taken
		add.w	d0, d1						;* add basepalette		4
		move.w	d1, (a6)					;* write vram			8
	.endr
		bra.s	1b						;* loop				10
FIXjobs_end:
		;******************************************



		;**** processed all jobs ******************
		tst.b	BIOS_DEVMODE					;* dev mode?			16
		beq.s	0f						;* 				8 not taken, 10 taken
		move.l	0x10e.w, a4					;* backup ram ptr
		btst	#0, 1(a4)					;* dip 2-1 ?
		beq.s	0f
		move.w	#0x4f80, _METER_COLOR				;* JOB_ORANGE			20

0:		tst.b	BIOS_DEVMODE					;*				16
		bne.w	libNG_debug					;*				10 taken, 12 not taken

debug_return:	;* we done, resetting drawlists
		move.l	#SC1, SC1ptr					;* SC1ptr=SC1			28
		move.l	#SC234, SC234ptr				;* SC234ptr=SC234		28
		move.l	#PALJOBS, palJobsPtr				;* jobPtr=JOB			28
		move.l	#FIXJOBS, fixJobsPtr				;* fixPtr=FIX			28
		clr.w	libNG_drawListReady				;*				20
		addq.l	#1, libNG_frameCounter				;*				28

		;* BIOS calls
		jsr	MESS_OUT					;*
		jsr	SYSTEM_IO					;*

		tst.b	BIOS_DEVMODE					;* dev mode?			16
		beq.s	0f						;*				8 not taken, 10 taken
		move.l	0x10e.w, a4					;* backup ram ptr
		btst	#0, (1,a4)					;* dip 2-1 ?
		beq.s	0f
		move.w	#0x20f0, _METER_COLOR				;* JOB_GREEN			20

0:		move.l	VBL_callBack, a0
VBL_end:
		move.w	#1, libNG_vbl_flag				;* Vblank flag			20
		move.w	#4, REG_IRQACK					;* IRQ ack			20
		move.b	d0, REG_WATCHDOG				;*				16

		move.l	a0, d0
		beq.s	0f						;* valid callback ?
		jsr	(a0)
0:		movem.l	(sp)+, d0-d7/a0-a6				;*				132
		rte


;* ******************************************************************************
;* 				DEBUG dips
;* ******************************************************************************
;* a4=dips
;* a5=3c0000
;* a6=R/W
libNG_debug:
		btst	#1, 1(a4)					;* dip 2-2 ?
		bne.s	DIP_22						;* print line counter
DIP_22_END:
		btst	#2, 1(a4)
		bne.s	DIP_23						;* print buffers load
DIP_23_END:

DIPS_END:
		bra.w	debug_return

DIP_22:									;* display raster line #
	.globl	_hexTable						;* from strings.s
		;*moveq	#0, d0
		move.w	6(a5), d0
		lsr.w	#7, d0						;* d0=line #
		lea	_hexTable, a3
		moveq	#0, d1						;* d1=base data
		move.w	#0x7483, (a5)					;* set addr, modulo
		move.w	#-32, 4(a5)
		moveq	#0, d2

		move.b	d0, d2
		andi.b	#0x0f, d2
		move.b	(d2.w, a3), d1
		move.w	d1,  (a6)

		lsr.w	#4, d0
		move.b	d0, d2
		andi.b	#0x0f, d2
		move.b	(d2.w, a3), d1
		move.w	d1,  (a6)

		lsr.w	#4, d0
		move.b	d0, d2
		andi.b	#0x0f, d2
		move.b	(d2.w, a3), d1
		move.w	d1, (a6)
	
		bra.s	DIP_22_END

DIP_23:
	;* ▲▼
	;* buffer sizes /10
	SC1_10 	= (SC1_BUFFER_SIZE*4)/10
	SC234_10= (SC234_BUFFER_SIZE*2)/10
	PAL_10	= (PAL_BUFFER_SIZE*4)/10
	FIX_10	= (FIX_BUFFER_SIZE*4)/10

		move.w	#1, 4(a5)					;* 1 modulo
		moveq	#0, d2
		moveq	#0, d3
		subq.b	#1, d3

		;* SC1 buffer
		move.w	#0x7445, (a5)					;* addr
		move.w	#0x0031, (a6)					;* '1'
		move.w	#0x0013, (a6)					;* '▼'
		moveq	#9, d0
		move.l	SC1ptr, d1
		sub.l	#SC1+4, d1					;* +4: end marker adj.
0:		ble.s	5f						;* empty
		move.w	d2, (a6)					;* '■'
		sub.w	#SC1_10, d1
		dbra	d0, 0b
		bra.s	9f
5:		move.w	d3, (a6)					;* ' '
		dbra	d0, 5b
9:		move.w	#0x0012, (a6)					;* '▲'

		;* SC234 buffer
		move.w	#0x7465, (a5)					;* addr
		move.w	#0x0032, (a6)					;* '2'
		move.w	#0x0013, (a6)					;* '▼'
		moveq	#9, d0
		move.l	SC234ptr, d1
		sub.l	#SC234, d1					;* no end marker
0:		ble.s	5f						;* empty
		move.w	d2, (a6)					;* '■'
		sub.w	#SC234_10, d1
		dbra	d0, 0b
		bra.s	9f
5:		move.w	d3, (a6)					;* ' '
		dbra	d0, 5b
9:		move.w	#0x0012, (a6)					;* '▲'

		;* pal buffer
		move.w	#0x7485, (a5)					;* addr
		move.w	#0x0050, (a6)					;* 'P'
		move.w	#0x0013, (a6)					;* '▼'
		moveq	#9, d0
		move.l	palJobsPtr, d1
		sub.l	#PALJOBS+4, d1					;* +4: end marker adj.
0:		ble.s	5f						;* empty
		move.w	d2, (a6)					;* '■'
		sub.w	#PAL_10, d1
		dbra	d0, 0b
		bra.s	9f
5:		move.w	d3, (a6)					;* ' '
		dbra	d0, 5b
9:		move.w	#0x0012, (a6)					;* '▲'

		;* fix buffer
		move.w	#0x74a5, (a5)					;* addr
		move.w	#0x0046, (a6)					;* 'F'
		move.w	#0x0013, (a6)					;* '▼'
		moveq	#9, d0
		move.l	fixJobsPtr, d1
		sub.l	#FIXJOBS+4, d1					;* +4: end marker adj.
0:		ble.s	5f						;* empty
		move.w	d2, (a6)					;* '■'
		sub.w	#FIX_10, d1
		dbra	d0, 0b
		bra.s	9f
5:		move.w	d3, (a6)					;* ' '
		dbra	d0, 5b
9:		move.w	#0x0012, (a6)					;* '▲'

		bra.w	DIP_23_END


;* 	NEO-GEO main 68k vectors table, system header & startup code

REG_WATCHDOG		=	0x300001
REG_VRAMRW		=	0x3c0002
REG_LSPCMODE		=	0x3c0006
REG_IRQACK		=	0x3c000c
REG_TIMERSTOP		=	0x3c000e

SYSTEM_RETURN		=	0xc00444
BIOS_SYSTEM_MODE	=	0x10fd80

	.org	0x0000

	.long	__stack		;* reset stack ptr
	.long	0xc00402	;* reset entry point
	.long	0xc00408	;* bus error
	.long	0xc0040e	;* address error
	.long	0xc00414	;* illegal instruction
	.long	0xc00426	;* division by 0
	.long	0xc00426	;* CHK command
	.long	0xc00426	;* TRAPV command
	.long	0xc0041a	;* illegal privilege
	.long	0xc00420	;* trace exception handling
	.long	0xc00426	;* no package command (1010)
	.long	0xc00426	;* no package command (1111)
	.long	0xc00426, 0xc00426, 0xc00426	;* unused
	.long	0xc0042c	;* uninitialized interrupt
.if	_SYSTEM_NCD
	.long	0xc00522, 0xc00528, 0xc0052e, 0xc00534	;* unused
	.long	0xc0053a, 0xc004f2, 0xc004ec, 0xc004e6	;* unused
	.long	0xc004e0	;* virtual interrupt
.else
	.long	0xc00426, 0xc00426, 0xc00426, 0xc00426	;* unused
	.long	0xc00426, 0xc00426, 0xc00426, 0xc00426	;* unused
	.long	0xc00432	;* virtual interrupt
.endif
		;* 0x64 - IRQs
.if	_SYSTEM_NCD
	.long	_IRQ_TIMER, _IRQ_VBLANK, _IRQ3
.else
	.long	_IRQ_VBLANK, _IRQ_TIMER, _IRQ3
.endif
	.long	0xc00426, 0xc00426, 0xc00426, 0xc00426	;* 4-7 unused
		;* 0x80 - TRAPS
	.long	_TRAP_0, _TRAP_1, _TRAP_2, _TRAP_3	;* 0-3
	.long	_TRAP_4, _TRAP_5, _TRAP_6, _TRAP_7	;* 4-7
	.long	_TRAP_8, _TRAP_9, _TRAP_A, _TRAP_B	;* 8-b
	.long	_TRAP_C, _TRAP_D, _TRAP_E, _TRAP_F	;* c-f
		;* 0xc0 - 0xff: 68K reserved area
	.long	-1, -1, -1, -1, -1, -1, -1, -1
	.long	-1, -1, -1, -1, -1, -1, -1, -1

		;* 0x100: neo geo header
	.org	0x100
	.ascii	"NEO-GEO"
.if	_SYSTEM_NCD
	.byte	_CDDA_FLAG
.else 	
	.byte	0
.endif
	.word	_NGH
	.long	_PROGRAM_SIZE
	.long	_WRK_BCKP_AREA
	.word	_WRK_BCKP_AREA_SIZE
	.byte	_EYE_CATCHER, _EYE_CATCHER_TILES

		;* soft dips (defined in neogeo.s)
	.long	JPconfig	;* JP soft dips
	.long	NAconfig	;* NA soft dips
	.long	EUconfig	;* EU soft dips

		;* entry points
		jmp	PRE_USER.l
		jmp	PLAYER_START.l
		jmp	DEMO_END.l
		jmp	COIN_SOUND.l

		;* 0x13a - 0x181 empty
.if	_SYSTEM_NCD
	.word	_CDDA_ADDR, -1, -1, -1
.else
	.word	-1, -1, -1, -1
.endif
	.long	-1, -1, -1, -1, -1, -1, -1, -1
	.long	-1, -1, -1, -1, -1, -1, -1, -1

	.org	0x182
	.long	__security_code
	.long	0x0, 0x1, SPconfig	;* SP soft dips

__security_code:
	.long	0x76004a6d, 0x0a146600, 0x003c206d, 0x0a043e2d
	.long	0x0a0813c0, 0x00300001, 0x32100c01, 0x00ff671a
	.long	0x30280002, 0xb02d0ace, 0x66103028, 0x0004b02d
	.long	0x0acf6606, 0xb22d0ad0, 0x67085088, 0x51cfffd4
	.long	0x36074e75, 0x206d0a04, 0x3e2d0a08, 0x3210e049
	.long	0x0c0100ff, 0x671a3010, 0xb02d0ace, 0x66123028
	.long	0x0002e048, 0xb02d0acf, 0x6606b22d, 0x0ad06708
	.long	0x588851cf, 0xffd83607
	.word	0x4e75

_IRQ3:
		move.b	#1, REG_IRQACK
		rte

_dummyTIdata:
	.word	0x0000


;* setup basics & calls user sub
PRE_USER:
		;* Reset watchdog
		move.b	d0, REG_WATCHDOG

		;* copy .data section
		move.w	#_dsize, d1	;* data section size
		beq.s	9f
		lea	_tend, a0	;* data placed after .text / always even aligned by GCC (?)
		lea	_dstart, a1	;* .data start addr, expected even aligned

		move.w	d1, d2
		lsr.w	#2, d1
		beq.s	5f
		subq.w	#1, d1

0:		move.l	(a0)+, (a1)+
		dbra	d1, 0b
		;* extra bytes ?
5:		and.w	#3, d2
		beq.s	9f
		subq.w	#1, d2
0:		move.b	(a0)+, (a1)+
		dbra	d2, 0b
9:
		;* Reset watchdog
		move.b	d0, REG_WATCHDOG

		;* clear remaining ram
		moveq	#0, d0
		lea	_bstart, a0	;* bss start
		move.l	#__stack, d1
		sub.l	a0, d1		;* clear size
		beq.s	9f		;* this won't end well...

		;* odd start?
		move.l	a0, d2
		roxr.w	#1, d2
		bcc.s	1f
		move.b	d0, (a0)+
		subq.w	#1, d1
		beq.s	9f

1:		move.w	d1, d2
		lsr.w	#2, d1
		beq.s	5f
		subq.w	#1, d1
0:		move.l	d0, (a0)+
		dbra	d1, 0b
		;* extra bytes ?
5:		and.w	#3, d2
		beq.s	9f
		subq.w	#1, d2
0:		move.b	d0, (a0)+
		dbra	d2, 0b
9:
		;* Reset watchdog
		move.b	d0, REG_WATCHDOG

		;* must clear sprites tilemap LSB, some
		;* older unibios versions only clear partially
		lea	REG_VRAMRW, a0
		move.w	#64, -2(a0)	;* set addr
		move.w	#2, 2(a0)	;* set modulo
		move.w	#384, d1
0:	.rept	32
		clr.w	(a0)
	.endr
		dbra	d1, 0b

		;* Reset watchdog
		move.b	d0, REG_WATCHDOG

		;* clear TI data & callbacks
		moveq	#0, d0
		move.l	d0, VBL_callBack
		move.l	d0, VBL_skipCallBack
		move.l	d0, TInextTable
		move.l	#_dummyTIdata, TIcurrentData	;* sets up dummy data
		;* init local LSPCmode value
		move.w	#0x4000, LSPCmode
		;* 50Hz timer disable
		btst.b	#3, REG_LSPCMODE+1
		beq.s	0f
		move.w	#1, REG_TIMERSTOP

0:		;* for CD only
	.if	_SYSTEM_NCD
		bset	#7, BIOS_SYSTEM_MODE
	.endif

		;* Flush interrupts
		move.b	#7, REG_IRQACK

		;* Enable interrupts
		move.w	#0x2000, sr

		;* call USER sub, then return to BIOS
		jsr	USER
		jmp	SYSTEM_RETURN

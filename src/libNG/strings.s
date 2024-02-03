	.include "src/libNG/defines.inc"
	#include <configNG.h>
	.align 2

.globl	libNG_scratchpad16

.globl	_hexTable
_hexTable:
	.ascii	"0123456789ABCDEF"


;* /******************************************************************************
;* 				FIX PRINT
;* ******************************************************************************/
;* **** void fixPrint(printInfo, *text) ******************** fedc|ba98.76543210 => palette|tile
.globl fixPrint
.globl fixPrint2
	.set	_ARGS, 4
fixPrint:
		lea	REG_VRAM_RW, a1					;* a1=vram			12
		move.w	#0x20, 2(a1)					;* 0x20 increments (line write)	16
		lea	_ARGS(sp), a0
		move.w	(a0)+, -2(a1)					;* set addr
		move.w	(a0)+, d0					;* pal & bank in upper byte
		move.l	(a0), a0					;* a0=text

0:
	.rept	8
		move.b	(a0)+, d0					;* d0=char			8
		beq.s	1f						;* 0x0 => end			8 not taken, 10 taken
		move.w	d0, (a1)					;* write vram			8
	.endr
		bra.s	0b						;* loop				10
1:		rts

;* IRQ safe version
	.set	_ARGS, 4
fixPrint2:
		lea	REG_VRAM_ADDR, a1				;* a1=vram			12
		move.l	_ARGS(sp), d0					;* d0=addr,pal,bank
		move.l	_ARGS+4(sp), a0					;* a0=text			16

		moveq	#0x20, d1
		swap	d1						;* d1=address increment value

0:
	.rept	8
		move.b	(a0)+, d0					;* d0=char			8
		beq.s	1f						;* 0x0 => end			8 not taken, 10 taken
		move.l	d0, (a1)					;* write vram	addr+data	12	
		add.l	d1, d0						;* step addr			8
	.endr
		bra.s	0b						;* loop				10
1:		rts

;* 16b strings
;* *** void fixPrint3(printInfo, *text) *****************
.globl fixPrint3
.globl fixPrint4
	.set	_ARGS, 4
fixPrint3:
		lea	REG_VRAM_RW, a1					;* a1=vram			12
		move.w	#0x20, 2(a1)					;* set VRAM mod
		lea	_ARGS(sp), a0
		move.w	(a0)+, -2(a1)
		move.w	(a0)+, d1					;* d1=pal
		move.l	(a0), a0					;* a0=text

0:
	.rept	8
		move.w	(a0)+, d0					;* get character
		beq.s	1f
		add.w	d1, d0						;* add pal mod
		move.w	d0, (a1)					;* write VRAM
	.endr
		bra.s	0b
1:		rts

;* IRQ safe version
	.set	_ARGS, 4
fixPrint4:
		lea	REG_VRAM_ADDR, a1				;* a1=vram			12
		move.l	_ARGS(sp), d0					;* d0 upper word = addr
		move.l	_ARGS+4(sp), a0					;* a0=text			16
		moveq	#0x20, d1
		swap	d1						;* d1=addr mod

		move.w	d2, -(sp)
		move.w	d0, d2						;* d2=pal

0:
	.rept	8
		move.w	(a0)+, d0					;* get character
		beq.s	1f
		add.w	d2, d0						;* add pal mod
		move.l	d0, (a1)					;* write VRAM
		add.l	d1, d0						;* step addr
	.endr
		bra.s	0b
1:		move.w	(sp)+, d2
		rts


;*/******************************************************************************
;*				sprintf2
;*******************************************************************************/
;* ushort sprintf2(char *dest, char *format, args....)
.globl sprintf2

;*d0 = 
;*d1 = 
;*d2 = 

;*a0 = format string
;*a1 = args
;*a2 = dest string
;*a3 = scratchpad
;*a4 = hex table/copy string

_printFormat:
		move.b	(a0)+, d2					;*	%0
		beq.w	endChar ;*9f					;* "%0\0"	braW
		cmp.b	#'2', d2
		blt.s	specChar
		cmp.b	#'<', d2					;* '<' =12
		bhi.s	specChar
		sub.b	#'0', d2
		bra.s	0f						;* specChar 0:
	
_printCharArg:	;* %c
		addq.w	#3, a1
		move.b	(a1)+, (a2)+
		bra.s	nextChar

_printStringArg:	;* %s
		move.l	(a1)+, a4					;* a4= string to copy
3:		move.b	(a4)+, (a2)+					;* best for 2+ size		12
		bne.s	3b						;* was \0 ?			10 taken / 8 not taken		//22 ea char
		subq.w	#1, a2						;*				8
		bra.s	nextChar					;*				10

	.set	_ARGS, 4
sprintf2:
		lea	_ARGS(sp), a1
		movem.l	d2/a2-a4, -(sp)					;* push regs			40
		move.l	(a1)+, a2					;* dest string
		move.l	(a1)+, a0					;* format string
		;* a1 now points to args

		lea	libNG_scratchpad16, a3				;*				12
		clr.b	(a3)+						;* scratchpad16[0]='\0'		12

nextChar:
		move.b	(a0)+, d0
		beq.w	endChar						;*
		cmp.b	#'%', d0
		beq.s	specChar
		move.b	d0, (a2)+
		bra.s	nextChar

specChar:	;*was '%'
		moveq	#1, d2
0:		swap	d2
		move.b	(a0)+, d0
		cmp.b	#'d', d0
		beq.s	_printDecimalArg
		cmp.b	#'x', d0
		beq.s	_printHexArg
		cmp.b	#'u', d0
		beq.s	_printUnsignedDecimalArg
		cmp.b	#'c', d0
		beq.s	_printCharArg
		cmp.b	#'s', d0
		beq.s	_printStringArg
		cmp.b	#'0', d0
		beq.s	_printFormat
		move.b	d0, (a2)+
		beq.w	9f						;* "%\0"
		bra.s	nextChar

_printHexArg:	;* %h
		lea	_hexTable(pc), a4
	;*	moveq	#0, d1						;* d1=0
		move.l	(a1)+, d0					;* d0=arg value
0:		move.b	d0, d1
		andi.w	#0x0f, d1					;* d1=nibble
		move.b	(d1.w,a4), (a3)+				;* print hexChar[nibble]
		lsr.l	#4, d0						;* shift to next nibble
		bne.s	0b						;* loop if not 0

	;*copy scratch12 (backwards)
_scratchCopy:
		move.l	a3, d0
		sub.l	#libNG_scratchpad16+1, d0				;* size
		swap	d2
		sub.w	d0, d2
		ble.s	1f	;*ble /blt
		subq.w	#1, d2						;* loop fix
0:		move.b	#'0', (a2)+
		dbra	d2, 0b
1:		move.b	-(a3), (a2)+					;*				14
		bne.s	1b						;*				8 not taken	10 taken
		subq	#1, a2						;*				8
		addq	#1, a3						;*				8
		bra.s	nextChar					;*				10


_printUnsignedDecimalArg:	;* %u
		move.l	(a1)+, d0					;* d0=arg value
		bra.s	0f

_printDecimalArg:		;* %d
		move.l	(a1)+, d0					;* d0=arg value
		bpl.s	0f
		swap	d2
		subq.w	#1, d2						;* dec format lenght
		swap	d2
		move.b	#'-', (a2)+
		neg.l	d0

0:		cmpi.l	#0x10000, d0					;* long?
		bcs.s	_printDecimal_short

		move.w	d0, d2						;* d2=#
		move.l	d0, d1
		clr.w	d1
		swap	d1
		divu.w	#10, d1						;* div upper word		140
		move.w	d1, d0
		swap	d0
		move.w	d2, d1
		divu.w	#10, d1						;* div lower word
		move.w	d1, d0						;* d0 has long result
	
		swap	d1						;* d1 has remainder
		add.b	#'0', d1					;*				8
		move.b	d1, (a3)+					;*				8
		bra.s	0b

_printDecimal_short:
		divu.w	#10, d0						;*				144
		swap	d0						;*				4
		add.b	#'0', d0					;*				8
		move.b	d0, (a3)+					;*				8
		clr.w	d0						;*				4
		swap	d0						;*				4
		bne.s	_printDecimal_short				;* something left
		bra.s	_scratchCopy

endChar:
		clr.b	(a2)+

9:		move.l	a2, d0						;*				4
		movem.l	(sp)+, d2/a2-a4					;* pop regs
		sub.l	_ARGS(sp), d0					;* - dest string		18
		subq.w	#1, d0						;*				4
		rts


;*/******************************************************************************
;*				sprintf3
;*******************************************************************************/
;* ushort sprintf3(printInfo, char *dest, char *format, args...);
.globl sprintf3

_printFormat3:
		move.b	(a0)+, d2					;*	%0
		beq.w	endChar3					;* "%0\0"	braW
		cmp.b	#'2', d2
		blt.s	specChar3
		cmp.b	#'<', d2					;* '<' =12
		bhi.s	specChar3
		sub.b	#'0', d2
		bra.s	0f						;* specChar 0:

_printWord3:	;* %w
		addq.l	#2, a1						;* 				4
		move.w	(a1)+, (a2)+					;*				12
		bra.s	nextChar3					;*				10
	
_printCharArg3:	;* %c
		addq.w	#3, a1						;*				4
		move.b	(a1)+, d3					;*				8
		move.w	d3, (a2)+					;*				8
		bra.s	nextChar3

_printStringArg3:	;* %s
		move.l	(a1)+, a4					;* a4= string to copy
3:		move.b	(a4)+, d3
		beq.s	nextChar3
		move.w	d3, (a2)+
		bra.s	3b
	
	.set	_ARGS, 4
sprintf3:
		lea	_ARGS+2(sp), a1
		movem.l	d2-d3/a2-a4, -(sp)				;* push regs
		move.w	(a1)+, d3					;* d3= pal/bk byte
		move.l	(a1)+, a2					;* dest string			16
		move.l	(a1)+, a0					;* format string		16
		;* a1 now points to args

		lea	libNG_scratchpad16, a3				;*				12
		clr.b	(a3)+						;* scratchpad16[0]='\0'		12

nextChar3:
		move.b	(a0)+, d3
		beq.w	endChar3					;*
		cmp.b	#'%', d3
		beq.s	specChar3
		move.w	d3, (a2)+
		bra.s	nextChar3

specChar3:	;*was '%'
		moveq	#1, d2
0:		swap	d2
		move.b	(a0)+, d0
		cmp.b	#'d', d0
		beq.s	_printDecimalArg3
		cmp.b	#'x', d0
		beq.s	_printHexArg3
		cmp.b	#'u', d0
		beq.s	_printUnsignedDecimalArg3
		cmp.b	#'c', d0
		beq.s	_printCharArg3
		cmp.b	#'s', d0
		beq.s	_printStringArg3				;*
		cmp.b	#'0', d0
		beq.w	_printFormat3					;*
		cmp.b	#'w', d0
		beq.s	_printWord3					;*
		move.b	d0, d3
		beq.w	endChar3					;*	%\0
		move.w	d3, (a2)+
		bra.s	nextChar3

_printHexArg3:	;* %h
		lea	_hexTable(pc), a4
	;*	moveq	#0, d1						;*d1=0
		move.l	(a1)+, d0					;*d0=arg value
0:		move.b	d0, d1
		andi.w	#0x0f, d1					;* d1=nibble
		move.b	(d1.w,a4), (a3)+				;* print hexChar[nibble]
		lsr.l	#4, d0						;* shift to next nibble
		bne.s	0b						;* loop if not 0

	;*copy scratch16 (backwards)
_scratchCopy3:
		move.l	a3, d0
		sub.l	#libNG_scratchpad16+1, d0			;* size
		swap	d2
		sub.w	d0, d2
		ble.s	1f	;*ble /blt
		subq.w	#1, d2						;* loop fix
		move.b	#'0', d3
0:		move.w	d3, (a2)+
		dbra	d2, 0b
1:		move.b	-(a3), d3
		beq.s	1f						;* was \0
		move.w	d3, (a2)+					;*
		bra.s	1b ;*_scratchCopy				;*				8 not taken	10 taken

1:		addq	#1, a3						;*				8
		bra.w	nextChar3					;*				10


_printUnsignedDecimalArg3:	;* %u
		move.l	(a1)+, d0					;* d0=arg value
		bra.s	0f

_printDecimalArg3:		;* %d
		move.l	(a1)+, d0					;* d0=arg value
		bpl.s	0f
		swap	d2
		subq.w	#1, d2						;* dec format lenght
		swap	d2
		move.b	#'-', d3
		move.w	d3, (a2)+
		neg.l	d0

0:		cmpi.l	#0x10000, d0					;* long?
		bcs.s	_printDecimal_short3

		move.w	d0, d2						;* d2=#
		move.l	d0, d1
		clr.w	d1
		swap	d1
		divu.w	#10, d1						;* div upper word		144
		move.w	d1, d0
		swap	d0
		move.w	d2, d1
		divu.w	#10, d1						;* div lower word
		move.w	d1, d0						;* d0 has long result
	
		swap	d1						;* d1 has remainder
		add.b	#'0', d1					;*				8
		move.b	d1, (a3)+					;*				8
		bra.s	0b

_printDecimal_short3:
		divu.w	#10, d0						;*				144
		swap	d0						;*				4
		add.b	#'0', d0					;*				8
		move.b	d0, (a3)+					;*				8
		clr.w	d0						;*				4
		swap	d0						;*				4
		bne.s	_printDecimal_short3				;* something left
		bra.s	_scratchCopy3

endChar3:
		clr.w	(a2)+
9:		move.l	a2, d0						;*				4
		movem.l	(sp)+, d2-d3/a2-a4				;* pop regs
		sub.l	_ARGS+4(sp), d0					;* dest string			18
		lsr.w	#1, d0
		subq.w	#1, d0						;*				4
		rts
		

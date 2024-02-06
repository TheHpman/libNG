	.include "src/libNG/defines.inc"
	#include <configNG.h>
	.align	2

;* BUG: pools will glitch if trying to render a 0 sized frame
;*	it's not possible for tools to produce such a frame atm tho...


;*/******************************************************************************
;*				spritePoolInit
;*******************************************************************************/

;* void spritePoolInit(spritePool *sp, ushort baseSprite, ushort size, bool clearSprites)

.globl spritePoolInit
spritePoolInit:
	.set	_ARGS, 4
		move.l	_ARGS(sp), a0					;*a0=sp				16
		move.w	_ARGS+4+2(sp), d0				;*d0=baseSprite
		move.w	_ARGS+8+2(sp), d1				;*d1=poolSize

		move.w	d0, SPOOL_POOLSTART(a0)				;*sp->poolStart=baseSprite
		move.w	d0, SPOOL_CURRENTUP(a0)				;*sp->currentUp=baseSprite
		move.w	d1, SPOOL_POOLSIZE(a0)				;*sp->poolSize=poolSize
		move.w	#WAY_UP, SPOOL_WAY(a0)				;*sp->way=WAY_UP
		add.w	d1, d0						;*baseSprite+poolSize
		subq.w	#1, d0						;*-1
		move.w	d0, SPOOL_POOLEND(a0)				;*sp->poolEnd=
		move.w	d0, SPOOL_CURRENTDOWN(a0)			;*sp->currentDown=

		;*  clear sprites
		tst.w	_ARGS+12+2(sp)					;* clear sprites?
		beq.s	9f						;*
		move.l	#0x88008200, d0					;*data, addr			12
		add.w	_ARGS+4+2(sp), d0				;*
		swap	d0						;*d0=addr/data

		movea.l	SC234ptr, a0					;*a0=queue ptr			20
		move.l	#0x10000, a1					;*a1=next spr mod		12		swap+12
		subq.w	#1, d1						;*d1=loop			4

		move.l	d0, (a0)+					;*queue				12
		dbra	d1, 0f						;*loop				10 taken, 14 not
		move.l	a0, SC234ptr					;*update ptr			20
		rts

0:		add.l	a1, d0						;*next spr			8		swap+4
		move.l	d0, (a0)+					;*queue				12
		dbra	d1, 0b						;*loop				10 taken, 14 not
		move.l	a0, SC234ptr					;*update ptr			20
9:		rts


;*/******************************************************************************
;*				spritePoolClose
;*******************************************************************************/

;* spritePoolClose_(spritePool *sp)

.globl spritePoolClose
spritePoolClose:
	.set	_ARGS, 4
		move.l	_ARGS(sp), a0					;*a0=sp				16
		tst.w	SPOOL_WAY(a0)					;*
		bne.s	5f						;*

		;* WAY_UP
		move.w	#WAY_DOWN, SPOOL_WAY(a0)			;* sp->way=WAY_DOWN
		move.w	SPOOL_POOLEND(a0), d1
		move.w	d1, SPOOL_CURRENTDOWN(a0)			;* sp->currentDown=sp->poolEnd;

		move.w	SPOOL_CURRENTUP(a0), d0
		sub.w	d0, d1						;* d0.w=spr, d1.w=loop
		bge.s	7f
1:		rts

		;* WAY_DOWN
5:		move.w	#WAY_UP, SPOOL_WAY(a0)				;* sp->way=WAY_UP
		move.w	SPOOL_POOLSTART(a0), d0
		move.w	d0, SPOOL_CURRENTUP(a0)				;* sp->currentUp=sp->poolStart;

		move.w	d0, d1
		neg.w	d1
		add.w	SPOOL_CURRENTDOWN(a0), d1			;* d0.w=spr, d1.w=loop
		bmi.s	1b

		;* clear unused sprites
7:		movea.l	SC234ptr, a0					;* a0=queue ptr			20
		move.l	#0x10000, a1					;* a1=next spr mod		12		swap+12

		ext.l	d0
		or.l	#0x88008200, d0
		swap	d0

		move.l	d0, (a0)+					;* queue			12
		dbra	d1, 0f						;* loop				10 taken, 14 not
		move.l	a0, SC234ptr					;* update ptr			20
		rts							;*

0:		add.l	a1, d0						;* next spr			8		swap+4
		move.l	d0, (a0)+					;* queue			12
		dbra	d1, 0b						;* loop				10
		move.l	a0, SC234ptr					;* update ptr			20
		rts


;*/******************************************************************************
;*				spritePoolDrawList2
;*******************************************************************************/

;* spritePoolDrawList2(spritePool* sp,void* list)

.globl spritePoolDrawList2
spritePoolDrawList2:
	.set	_ARGS, 4
		move.l	_ARGS(sp), a0					;* a0=SP
		move.l	_ARGS+4(sp), a1					;* a1=list
		movem.l	d2-d7/a2-a6, -(sp)				;* push				96

		movea.l	SC234ptr, a5					;* a5=SC234ptr			20
		move.l	#REG_VRAM_ADDR, a6				;* a3=VRAM_ADDR			12

		move.w	SPOOL_WAY(a0), d0				;* d0=way
		bne.w	spritePoolDrawList_2_downStart			;* not 0 = going down

;*	/*****************************
;*			GOING UP - start
;*	*****************************/
spritePoolDrawList_2_upStart:
		move.w	SPOOL_CURRENTUP(a0), d0				;* d0=currentUp
		move.w	d0, d7
		swap	d7						;* d7=1st spr to use
		;* get gap in d7
		move.w	SPOOL_CURRENTDOWN(a0), d7
		sub.w	d0, d7						;* d7=currentDown-currentUp
		addq.w	#1, d7						;* d7=1st|gap

		lea	spritePoolDrawList_2_upReturn(pc), a4
		bra.s	spritePoolDrawList_2_upNext

spritePoolDrawList_2_upReturn:
		swap	d4
		add.w	d4, d7
		swap	d7						;* spr|gap
spritePoolDrawList_2_upNext:
		move.l	(a1)+, d0					;*				12
		beq.w	spritePoolDrawList_1_upListEnd			;*				12 not taken, 10 taken
		move.l	d0, a3						;* a3=AS/Pict			4

		;*render up
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)			;* no display flag?		16
		bne.w	spritePoolDrawList_2_upNext			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d5
		sub.w	d5, d7
		bmi.w	spritePoolDrawList_1_upOOSprites 		;* neg result
		move.w	d5, d4
		swap	d4
		swap	d7						;* gap|spr
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d5
		bra.w	drawUnscaledPattern_IRQsafe

;*	/*****************************
;*			GOING DOWN - start
;*	*****************************/
spritePoolDrawList_2_downStart:
		move.w	SPOOL_CURRENTDOWN(a0), d0			;* d0=currentDown
		move.w	d0, d7
		addq.w	#1, d7						;* fix for -width
		swap	d7						;* d7=1st spr to use
		;* get gap in d7
		move.w	d0, d7						;* d7=currentdown
		sub.w	SPOOL_CURRENTUP(a0), d7				;* d7=currentDown-currentUp
		addq.w	#1, d7						;* d7=1st|gap

		lea	spritePoolDrawList_2_downReturn(pc), a4
	;*	bra.s	spritePoolDrawList_2_downNext
		swap	d7

spritePoolDrawList_2_downReturn:
		swap	d7						;* spr|gap			4
spritePoolDrawList_2_downNext:
		move.l	-(a1), d0					;*				12
		beq.w	spritePoolDrawList_1_downListEnd		;*				12 not taken, 10 taken
		move.l	d0, a3						;* a3=AS/Pict			4

		;*render down
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)			;* no display flag?		16
		bne.w	spritePoolDrawList_2_downNext			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d5
		sub.w	d5, d7
		bmi.w	spritePoolDrawList_1_downOOSprites 		;* neg result
		move.w	d5, d4
		swap	d4
		swap	d7
		sub.w	d5, d7						;* predec sprite to use #
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d5
		bra.w	drawUnscaledPattern_IRQsafe


;*/******************************************************************************
;*				spritePoolDrawList
;*******************************************************************************/

;* spritePoolDrawList(spritePool* sp,void* list)

.globl spritePoolDrawList
spritePoolDrawList:
	.set	_ARGS, 4
		move.l	_ARGS(sp), a0					;* a0=SP
		move.l	_ARGS+4(sp), a1					;* a1=list
		movem.l	d2-d7/a2-a6, -(sp)				;* push				96

		movea.l	SC234ptr, a5					;* a5=SC234ptr			20
		move.l	#REG_VRAM_RW, a6				;* a3=VRAM_R/W			12
		move.w	#1, 2(a6)					;* modulo=1			16

		move.w	SPOOL_WAY(a0), d0				;* d0=way
		bne.w	spritePoolDrawList_1_downStart			;* not 0 = going down

;*	/*****************************
;*			GOING UP - start
;*	*****************************/
spritePoolDrawList_1_upStart:
		move.w	SPOOL_CURRENTUP(a0), d0				;* d0=currentUp
		move.w	d0, d7
		swap	d7						;* d7=1st spr to use
		;* get gap in d7
		move.w	SPOOL_CURRENTDOWN(a0), d7
		sub.w	d0, d7						;* d7=currentDown-currentUp
		addq.w	#1, d7						;* d7=1st|gap

		lea	spritePoolDrawList_1_upReturn(pc), a4
		bra.s	spritePoolDrawList_1_upNext

spritePoolDrawList_1_upReturn:
		swap	d4
		add.w	d4, d7
		swap	d7						;* spr|gap
spritePoolDrawList_1_upNext:
		move.l	(a1)+, d0					;*				12
		beq.w	spritePoolDrawList_1_upListEnd			;*				12 not taken, 10 taken
		move.l	d0, a3						;* a3=AS/Pict			4

		;*render up
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)			;* no display flag?		16
		bne.w	spritePoolDrawList_1_upNext			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d5
		sub.w	d5, d7
		bmi.s	spritePoolDrawList_1_upOOSprites 		;* neg result
		move.w	d5, d4
		swap	d4
		swap	d7						;* gap|spr
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d5
		bra.w	drawUnscaledPattern

spritePoolDrawList_1_upListEnd2:
		move.l	a6, SC1ptr
spritePoolDrawList_1_upListEnd:
		move.l	48(sp), a0
		swap	d7
		move.w	d7, SPOOL_CURRENTUP(a0)
		move.l	a5, SC234ptr
		movem.l	(sp)+, d2-d7/a2-a6
		rts

;*=== switch to buffer only ===
spritePoolDrawList_1_upOOSprites:
		;* d0 has tilewidth
		;* update ptrs
		movea.l	SC1ptr, a6					;* a6=SC1ptr			20
		lea	spritePoolDrawList_1_upReturn2(pc), a4		;* return spot
		;* compute gap to pool end
		move.l	d7, d1
		swap	d1						;* d1=current up
		move.l	48(sp), a0
		move.w	SPOOL_POOLEND(a0), d7
		sub.w	d1, d7
		addq.w	#1, d7						;* updated gap
		bra.w	5f						;* we know we must render it

spritePoolDrawList_1_upReturn2:
		swap	d4
		add.w	d4, d7
		swap	d7						;* spr|gap
spritePoolDrawList_1_upNext2:
		move.l	(a1)+, d0					;*				12
		beq.s	spritePoolDrawList_1_upListEnd2			;*				12 not taken, 10 taken
		move.l	d0, a3						;*a3=AS/Pict			4

		;*render up, buffer only
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)			;* no display flag?		16
		bne.w	spritePoolDrawList_1_upNext2			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d5
5:		sub.w	d5, d7
		bmi.s	spritePoolDrawList_1_upSkip 			;* neg result
		move.w	d5, d4
		swap	d4
		swap	d7						;* gap|spr
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d5
		bra.w	drawUnscaledPatternBuffered

spritePoolDrawList_1_upSkip:
		;* out of options, sprite must be skipped
		clr.w	AS_BASESPRITE(a3)				;* set spr num			12
		add.w	d5, d7						;* restore gap
		bra.w	spritePoolDrawList_1_upNext2			;* try next


;*	/*****************************
;*			GOING DOWN - start
;*	*****************************/
spritePoolDrawList_1_downStart:
		move.w	SPOOL_CURRENTDOWN(a0), d0			;* d0=currentDown
		move.w	d0, d7
		addq.w	#1, d7						;* fix for -width
		swap	d7						;* d7=1st spr to use
		;* get gap in d7
		move.w	d0, d7						;* d7=currentdown
		sub.w	SPOOL_CURRENTUP(a0), d7				;* d7=currentDown-currentUp
		addq.w	#1, d7						;* d7=1st|gap

		lea	spritePoolDrawList_1_downReturn(pc), a4
	;*	bra.s	spritePoolDrawList_1_downNext
		swap	d7

spritePoolDrawList_1_downReturn:
		swap	d7						;*spr|gap
spritePoolDrawList_1_downNext:
		move.l	-(a1), d0					;*				12
		beq.w	spritePoolDrawList_1_downListEnd		;*				12 not taken, 10 taken
		move.l	d0, a3						;*a3=AS/Pict			4

		;*render down
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)		;* no display flag?		16
		bne.w	spritePoolDrawList_1_downNext			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d5
		sub.w	d5, d7
		bmi.s	spritePoolDrawList_1_downOOSprites 		;* neg result
		move.w	d5, d4
		swap	d4
		swap	d7
		sub.w	d5, d7						;* predec sprite to use #
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d5
		bra.w	drawUnscaledPattern

spritePoolDrawList_1_downListEnd2:
		move.l	a6, SC1ptr
spritePoolDrawList_1_downListEnd:
		move.l	48(sp), a0
		swap	d7
		subq.w	#1, d7
		move.w	d7, SPOOL_CURRENTDOWN(a0)	;*CHECK
		move.l	a5, SC234ptr
		movem.l	(sp)+, d2-d7/a2-a6
		rts

		;* === switch to buffer only (down) ===
spritePoolDrawList_1_downOOSprites:
		;* d0 has tileWidth
		;* update ptrs
		movea.l	SC1ptr, a6					;* a6=SC1ptr			20
		lea	spritePoolDrawList_1_downReturn2(pc), a4	;* return spot
		;* compute gap to pool start
		move.l	d7, d1
		swap	d1						;* d1=current down
		move.l	48(sp), a0
		move.w	d1, d7
		sub.w	SPOOL_POOLSTART(a0), d7				;* updated gap
		bra.w	5f						;* we know we must render it

spritePoolDrawList_1_downReturn2:
		swap	d7						;* spr|gap
spritePoolDrawList_1_downNext2:
		move.l	-(a1), d0					;*				12
		beq.w	spritePoolDrawList_1_downListEnd2		;*				12 not taken, 10 taken
		move.l	d0, a3

		;*render down, buffer only
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)		;* no display flag?		16
		bne.w	spritePoolDrawList_1_downNext2			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)	;* 					20

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d5
5:		sub.w	d5, d7
		bmi.s	spritePoolDrawList_1_downSkip 			;* neg result
		move.w	d5, d4
		swap	d4
		swap	d7
		sub.w	d5, d7						;* predec sprite to use #
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d5
		bra.w	drawUnscaledPatternBuffered

spritePoolDrawList_1_downSkip:
		;* out of options, sprite must be skipped
		clr.w	AS_BASESPRITE(a3)				;* set spr num			12
		add.w	d5, d7						;* restore gap
		bra.w	spritePoolDrawList_1_downNext2			;* try next



;*******************************************************************************************
drawUnscaledPattern_IRQsafe:
		move.w	#0x8400, d6
		add.w	d7, d6
		swap	d6

		;*setup coords
		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a3)		;* strict coordinates?
		beq.s	0f
		;* set posX
		move.w	AS_POSX(a3), d1					;*d1=posX			12
		;* set posY
		move.w	#YSHIFT, d2					;*				8
		sub.w	AS_POSY(a3), d2					;*d2=posY			12
		bra.s	5f						;*				10

		;*std coords, compute shifts
		move.l	AS_CURRENTSTEP(a3), a0				;* a0=step
		addq.w	#4, a0
		move.w	AS_CURRENTFLIP(a3), d0
		;* prep shiftX
		move.l	(a0)+, d1
		ror.b	#1, d0
		bcc.s	0f
		swap	d1
0:		;* prep shiftY
		move.l	(a0)+, d2
		ror.b	#1, d0
		bcc.s	0f
		swap	d2

0:		;* compute final posX
		add.w	AS_POSX(a3), d1					;* d1= final posX
		;*compute final posY
		add.w	AS_POSY(a3), d2
		neg.w	d2
		add.w	#YSHIFT, d2					;* d2= final posY

		;* d1=posX d2=posY ok
5:		move.w	d1, d6
		lsl.w	#7, d6
		move.l	d6, (a6)					;* VRAM set posX

		sub.l	#0x2000000, d6
		move.w	d2, d6
		lsl.w	#7, d6
		add.w	FRAME_TILEHEIGHT(a2), d6
		move.l	d6, (a5)+					;* buffer posY

		;* link following sprs
		move.w	d5, d0
		beq.s	1f						;* single spr
		subq.w	#1, d0
		move.w	#0x40, d6					;* link flag			8
		moveq	#1, d2
		swap	d2						;*d2=spr++
0:		add.l	d2, d6
		move.l	d6, (a5)+					;* buffer spr link
		dbra	d0, 0b

		;*draw ptn
1:		move.w	d7, d6						;* not needed? (d6=0x8000+spr)
		lsl.w	#6, d6						;* d6= spr addr
;*		move.w	d6, -2(a6)					;* set LSPC addr

		move.w	AS_CURRENTFLIP(a3), d0
		add.w	d0, d0
		add.w	d0, d0
		move.l	FRAME_MAPS(d0.w, a2), a0			;*				18

		move.w	AS_BASEPALETTE(a3), d4
		clr.b	d4						;* d4=base palette		4

		move.w	FRAME_TILEHEIGHT(a2), d0 			;* tileH
		add.w	d0, d0						;* code to write 1 tile = 14bytes atm
		move.w	d0, d1  ;* d1=x2
		lsl.w	#3, d0	;* d0=x16
		sub.w	d1, d0	;* d0=x14

		lea	writeBaseUnscaledIRQsafe(pc), a2
		sub.w	d0, a2

	;*	move.w	FRAME_COLSIZE(a2), d0 				;* tileH*4
	;*	move.l	LW_tble-4(pc,d0.w), a2

		;* d0: free
		;* d1: free?
		;* d2: free?
		;* d3: free?
		;* d4: tilewidth | pal
		;* d5: loop
		;* d6: spr addr
		;* d7: gap/spr#
		moveq	#1, d1
		swap	d1
		moveq	#64, d2
		move.w	d6, d0
		swap	d0

		jmp	(a2)						;* write sprite

_LONGWRITE_SIZE	=	14
LW_tble:
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*1), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*2), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*3), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*4)
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*5), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*6), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*7), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*8)
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*9), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*10), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*11), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*12)
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*13), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*14), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*15), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*16)
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*17), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*18), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*19), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*20)
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*21), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*22), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*23), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*24)
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*25), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*26), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*27), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*28)
	.long	writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*29), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*30), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*31), writeBaseUnscaledIRQsafe-(_LONGWRITE_SIZE*32)

;* 14 bytes / 60 cy
	.macro	_LONGWRITE_TILE_
		move.w	(a0)+, d0					;* read word1			8
		move.l	d0, (a6)					;* longwrite			12
		add.l	d1, d0						;* addr++			8

		move.w	(a0)+, d0					;* read word2			8
		add.w	d4, d0						;* add basePal			4
		move.l	d0, (a6)					;* longwrite			12
		add.l	d1, d0						;* addr++			8	//60
	.endm

;* 8 bytes / 32 cy
	.macro	_WRITE_TILE_
		move.w	(a0)+, (a6)					;* write tile word1		12
		move.w	(a0)+, d0					;* read tile2			8
		add.w	d4, d0						;* add basepalette		4
		move.w	d0, (a6)					;* write tile word2		8
	.endm

;* ==unscaled==
		_WRITE_TILE_	;* 32
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 25

		_WRITE_TILE_	;* 24
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 17

		_WRITE_TILE_	;* 16
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 9

		_WRITE_TILE_	;* 8
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 1
writeBaseUnscaled:
		dbra	d5, 2f						;* next sprite
		;*arrange for loop
		jmp	(a4)						;* return

2:		add.w	#64, d6						;* spr addr++
		move.w	d6, -2(a6)
		jmp	(a2)


;* ==unscaled IRQ safe==
		_LONGWRITE_TILE_	;* 32
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_	;* 25

		_LONGWRITE_TILE_	;* 24
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_	;* 17

		_LONGWRITE_TILE_	;* 16
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_	;* 9

		_LONGWRITE_TILE_	;* 8
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_
		_LONGWRITE_TILE_	;* 1
writeBaseUnscaledIRQsafe:
		dbra	d5, 2f		;* next sprite
		;*arrange for loop
		jmp	(a4)		;* return

2:		;*add.w	#64, d6		;* spr addr++
		add.w	d2, d6
		move	d6, d0
		swap	d0
		jmp	(a2)


;*********************************************************************************************
drawUnscaledPattern:
		move.w	#0x8400, d6
		add.w	d7, d6
		swap	d6

		;*setup coords
		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a3)		;* strict coordinates?
		beq.s	0f
		;* set posX
		move.w	AS_POSX(a3), d1					;*d1=posX			12
		;* set posY
		move.w	#YSHIFT, d2					;*				8
		sub.w	AS_POSY(a3), d2					;*d2=posY			12
		bra.s	5f						;*				10

		;*std coords, compute shifts
		move.l	AS_CURRENTSTEP(a3), a0				;* a0=step
		addq.w	#4, a0						;* skip ptn ptr
		move.w	AS_CURRENTFLIP(a3), d0				;* d0= flips
		;* prep shiftX
		move.l	(a0)+, d1
		ror.b	#1, d0
		bcc.s	0f
		swap	d1
0:		;* prep shiftY
		move.l	(a0)+, d2
		ror.b	#1, d0
		bcc.s	0f
		swap	d2

		;*d1=shiftX, d2=shiftY, now scale shifts
0:		;* compute final posX
		add.w	AS_POSX(a3), d1					;* d1= final posX

		;*compute final posY
		add.w	AS_POSY(a3), d2
		neg.w	d2
		add.w	#YSHIFT, d2					;* d2= final posY

		;* d1=posX d2=posY ok
5:		move.w	d1, d6
		lsl.w	#7, d6
		move.l	d6, -2(a6)					;* VRAM set posX

		sub.l	#0x2000000, d6
		move.w	d2, d6
		lsl.w	#7, d6
		add.w	FRAME_TILEHEIGHT(a2), d6
		move.l	d6, (a5)+					;* buffer posY

		;* link following sprs
		move.w	d5, d0
		beq.s	1f						;* single spr
		subq.w	#1, d0
		move.w	#0x40, d6					;* link flag	8
		moveq	#1, d2
		swap	d2						;*d2=spr++
0:		add.l	d2, d6
		move.l	d6, (a5)+					;* buffer spr link
		dbra	d0, 0b

		;*draw ptn
1:		move.w	d7, d6						;* not needed? (d6=0x8000+spr)
		lsl.w	#6, d6						;* d6= spr addr
		move.w	d6, -2(a6)					;* set LSPC addr

		move.w	AS_CURRENTFLIP(a3), d0
		add.w	d0, d0
		add.w	d0, d0
		move.l	FRAME_MAPS(d0.w, a2), a0			;*				18

		move.w	AS_BASEPALETTE(a3), d4
		clr.b	d4						;* d4=base palette		4

		move.w	FRAME_COLSIZE(a2), d0 				;* tileH*4
		add.w	d0, d0						;* code to write 1 tile = 8bytes

		lea	writeBaseUnscaled(pc), a2
		sub.w	d0, a2
		jmp	(a2)						;* write sprite


;*******************************************************************************************
drawUnscaledPatternBuffered:
;*	#if	BANKING_ENABLE
;*		move.w	#BANKING_CMD, (a6)+
;*		move.w	AS_BANK-1(a3), (a6)+
;*	#endif

		move.w	#0x8400, d6
		add.w	d7, d6
		swap	d6

		;*setup coords
		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a3)		;* strict coordinates?
		beq.s	0f
		;* set posX
		move.w	AS_POSX(a3), d1					;*d1=posX			12
		;* set posY
		move.w	#YSHIFT, d2					;*				8
		sub.w	AS_POSY(a3), d2					;*d2=posY			12
		bra.s	5f						;*				10

		;*std coords, compute shifts
		move.l	AS_CURRENTSTEP(a3), a0				;* a0=step
		addq.w	#4, a0						;* skip ptn ptr
		move.w	AS_CURRENTFLIP(a3), d0				;* d0= flips
		;* prep shiftX
		move.l	(a0)+, d1
		ror.b	#1, d0
		bcc.s	0f
		swap	d1
0:		;* prep shiftY
		move.l	(a0)+, d2
		ror.b	#1, d0
		bcc.s	0f
		swap	d2

0:		;* compute final posX
		add.w	AS_POSX(a3), d1					;* d1= final posX
		;* compute final posY
		add.w	AS_POSY(a3), d2
		neg.w	d2
		add.w	#YSHIFT, d2					;* d2= final posY

		;* d1=posX d2=posY ok
5:		move.w	d1, d6
		lsl.w	#7, d6
		move.l	d6, (a5)+					;* buffer posX

		sub.l	#0x2000000, d6
		move.w	d2, d6
		lsl.w	#7, d6
		add.w	FRAME_TILEHEIGHT(a2), d6
		move.l	d6, (a5)+					;* buffer posY

		;* link following sprs
		move.w	d5, d0
		beq.s	1f						;* single spr
		subq.w	#1, d0
		move.w	#0x40, d6					;* link flag			8
		moveq	#1, d2
		swap	d2						;*d2=spr++
0:		add.l	d2, d6
		move.l	d6, (a5)+					;* buffer spr link
		dbra	d0, 0b
		;*d2 = 0x0001 0000

		;* buffer ptn
1:		move.w	AS_CURRENTFLIP(a3), d0
		add.w	d0, d0
		add.w	d0, d0
		move.l	FRAME_MAPS(d0.w, a2), a0			;* a0=ptn data			18

		move.w	FRAME_TILEHEIGHT(a2), d0
		add.w	d0, d0						;* d0=tileH*2

		move.w	AS_BASEPALETTE(a3), d6
		move.b	d0, d6						;* d6=palmod|H*2
		swap	d6
	
		move.w	d7, d6
		lsl.w	#6, d6						;* d6=palmod|H*4|vram addr
		moveq	#64, d1
		add.w	d0, d0						;* d0=tileH*4

0:		move.l	d6, (a6)+
		move.l	a0, (a6)+
		add.w	d1, d6						;* next spr
		add.w	d0, a0						;* data ptr increment
		dbra	d5, 0b

		jmp	(a4)						;* return





;*/******************************************************************************
;*				spritePoolDrawList3
;*******************************************************************************/
;* spritePoolDrawList3(spritePool* sp,void* list)
.globl spritePoolDrawList3
spritePoolDrawList3:
	.set	_ARGS, 4
		move.l	_ARGS(sp), a0					;* a0=SP
		move.l	_ARGS+4(sp), a1					;* a1=list
		movem.l	d2-d7/a2-a6, -(sp)				;* push				96

		move.l	SC234ptr, a5					;* a5=SC234ptr			20
		move.l	#REG_VRAM_RW, a6				;* a3=VRAM_R/W			12
		move.w	#1, 2(a6)					;* modulo=1			16

		move.w	SPOOL_WAY(a0), d0				;*d0=way
		bne.w	spritePoolDrawList3_downStart			;*not 0 = going down

;*	/*****************************
;*			GOING UP - start
;*	*****************************/
;*spritePoolDrawList3_upStart:
		move.w	SPOOL_CURRENTUP(a0), d0				;*d0=currentUp
		move.w	d0, d7
		swap	d7						;*d7=1st spr to use
		;* get gap in d7
		move.w	SPOOL_CURRENTDOWN(a0), d7
		sub.w	d0, d7						;* d7=currentDown-currentUp
		addq.w	#1, d7						;* d7=1st|gap

		lea	spritePoolDrawList3_upReturn(pc), a4
		bra.s	spritePoolDrawList3_upNext

spritePoolDrawList3_upReturn:
		swap	d4
		add.w	d4, d7
		swap	d7						;* spr|gap
spritePoolDrawList3_upNext:
		move.l	(a1)+, d0					;*				12
		beq.w	spritePoolDrawList3_upListEnd			;*				12 not taken, 10 taken
		move.l	d0, a3						;*a3=AS/Pict			4

;*render up
		;* === animate if requested ===
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)			;* no display flag?
		bne.w	spritePoolDrawList3_upNext			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;*

		cmp.w	#0x1000, AS_XBIG(a3)				;* skip when bigX <0x10
		blo.w	spritePoolDrawList3_upNext

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d0
		beq.w	_renderNewFrameUp
		sub.w	d0, d7
		bmi.s	spritePoolDrawList3_upOOSprites 		;* neg result
		move.w	d0, d4
		swap	d4
		swap	d7						;* gap|spr
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d0
		move.w	d0, d5
		swap	d5
		move.w	d0, d5
		bra.w	drawScaledPattern

spritePoolDrawList3_upListEnd2:
		move.l	a6, SC1ptr
spritePoolDrawList3_upListEnd:
		move.l	48(sp), a0
		swap	d7
		move.w	d7, SPOOL_CURRENTUP(a0)
		move.l	a5, SC234ptr
		movem.l	(sp)+, d2-d7/a2-a6
		rts

;*=== switch to buffer only ===
spritePoolDrawList3_upOOSprites:
		;* d0 has tilewidth
		;* update ptrs
		movea.l	SC1ptr, a6					;* a6=SC1ptr			20
		lea	spritePoolDrawList3_upReturn2(pc), a4		;* return spot
		;* compute gap to pool end
		move.l	d7, d1
		swap	d1						;* d1=current up
		move.l	48(sp), a0
		move.w	SPOOL_POOLEND(a0), d7
		sub.w	d1, d7
		addq.w	#1, d7						;* updated gap
		bra.w	5f						;* we know we must render it

spritePoolDrawList3_upReturn2:
		swap	d4
		add.w	d4, d7
		swap	d7						;* spr|gap
spritePoolDrawList3_upNext2:
		move.l	(a1)+, d0					;*				12
		beq.s	spritePoolDrawList3_upListEnd2			;*				12 not taken, 10 taken
		move.l	d0, a3						;*a3=AS/Pict			4

;*render up, buffer only
		;* === animate if requested ===
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)		;* no display flag?		16
		bne.w	spritePoolDrawList3_upNext2			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		cmp.w	#0x1000, AS_XBIG(a3)				;* skip when bigX <0x10
		blo.w	spritePoolDrawList3_upNext2

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d0
		beq.w	_bfrNewFrmUp2
5:		sub.w	d0, d7
		bmi.s	spritePoolDrawList3_upSkip 			;* neg result
		move.w	d0, d4
		swap	d4
		swap	d7						;* gap|spr
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d0
		move.w	d0, d5
		swap	d5
		move.w	d0, d5
		bra.w	drawScaledPatternBuffered

spritePoolDrawList3_upSkip:
		;* out of options, sprite must be skipped
		clr.w	AS_BASESPRITE(a3)				;* set spr num			12
		add.w	d0, d7						;* restore gap
		bra.w	spritePoolDrawList3_upNext2			;* try next


;*	/*****************************
;*			GOING DOWN - start
;*	*****************************/
spritePoolDrawList3_downStart:
		move.w	SPOOL_CURRENTDOWN(a0), d0			;* d0=currentDown
		move.w	d0, d7
		addq.w	#1, d7						;* fix for -width
		swap	d7						;* d7=1st spr to use
		;* get gap in d7
		move.w	d0, d7						;* d7=currentdown
		sub.w	SPOOL_CURRENTUP(a0), d7				;* d7=currentDown-currentUp
		addq.w	#1, d7						;* d7=1st|gap

		lea	spritePoolDrawList3_downReturn(pc), a4
	;*	bra.s	spritePoolDrawList3_downNext
		swap	d7

spritePoolDrawList3_downReturn:
		swap	d7						;*spr|gap
spritePoolDrawList3_downNext:
		move.l	-(a1), d0					;*				12
		beq.w	spritePoolDrawList3_downListEnd			;*				12 not taken, 10 taken
		move.l	d0, a3						;*a3=AS/Pict			4


;*render down
		;* === animate if requested ===
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)		;* no display flag?		16
		bne.w	spritePoolDrawList3_downNext			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		cmp.w	#0x1000, AS_XBIG(a3)				;* skip when bigX <0x10
		blo.w	spritePoolDrawList3_downNext

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d0
		beq.w	_renderNewFrameDown
		sub.w	d0, d7
		bmi.s	spritePoolDrawList3_downOOSprites 		;* neg result
		move.w	d0, d4
		swap	d4
		swap	d7
		sub.w	d0, d7						;* predec sprite to use #
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d0
		move.w	d0, d5
		swap	d5
		move.w	d0, d5
		bra.w	drawScaledPattern

spritePoolDrawList3_downListEnd2:
		move.l	a6, SC1ptr
spritePoolDrawList3_downListEnd:
		move.l	48(sp), a0
		swap	d7
		subq.w	#1, d7
		move.w	d7, SPOOL_CURRENTDOWN(a0)	;*CHECK
		move.l	a5, SC234ptr
		movem.l	(sp)+, d2-d7/a2-a6
		rts

		;* === switch to buffer only (down) ===
spritePoolDrawList3_downOOSprites:
		;* d0 has tileWidth
		;* update ptrs
		movea.l	SC1ptr, a6					;* a6=SC1ptr			20
		lea	spritePoolDrawList3_downReturn2(pc), a4		;* return spot
		;* compute gap to pool start
		move.l	d7, d1
		swap	d1						;* d1=current down
		move.l	48(sp), a0
		move.w	d1, d7
		sub.w	SPOOL_POOLSTART(a0), d7				;* updated gap
		bra.w	5f						;* we know we must render it

spritePoolDrawList3_downReturn2:
		swap	d7						;* spr|gap
spritePoolDrawList3_downNext2:
		move.l	-(a1), d0					;*				12
		beq.w	spritePoolDrawList3_downListEnd2		;*				12 not taken, 10 taken
		move.l	d0, a3

;*render down, buffer only
		;* === animate if requested ===
	#if	BANKING_ENABLE
		move.b	AS_BANK(a3), REG_BANKING			;* bankswitch			24
	#endif
		___AS_ANIMATION_BLOCK___ d0 d1 a3 a0

		;* render
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a3)		;* no display flag?		16
		bne.w	spritePoolDrawList3_downNext2			;* skip
;*		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a3)		;* 				20

		cmp.w	#0x1000, AS_XBIG(a3)				;* skip when bigX <0x10
		blo.w	spritePoolDrawList3_downNext2

		move.l	AS_CURRENTFRAME(a3), a2				;* a2=frame
		move.w	FRAME_TILEWIDTH(a2), d0
		beq.w	_bfrNewFrmDn2
5:		sub.w	d0, d7
		bmi.s	spritePoolDrawList3_downSkip 			;* neg result
		move.w	d0, d4
		swap	d4
		swap	d7
		sub.w	d0, d7						;* predec sprite to use #
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d0
		move.w	d0, d5
		swap	d5
		move.w	d0, d5
		bra.w	drawScaledPatternBuffered

spritePoolDrawList3_downSkip:
		;* out of options, sprite must be skipped
		clr.w	AS_BASESPRITE(a3)				;* set spr num			12
		add.w	d0, d7						;* restore gap
		bra.w	spritePoolDrawList3_downNext2			;* try next


;*******************************************************************************************
drawScaledPattern:
		move.w	#0x8400, d6
		add.w	d7, d6
		swap	d6

		;*setup coords
		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a3)		;* strict coordinates?
		beq.s	0f
		;* setup posX
		move.w	AS_POSX(a3), d1					;*d1=posX			12
		;* setup posY
		move.w	#YSHIFT, d2					;*				8
		sub.w	AS_POSY(a3), d2					;*d2=posY			12
		bra.s	5f						;*				10

		;* coord = scaled shift + pos
0:		;*std coords, compute shifts
		move.l	AS_CURRENTSTEP(a3), a0				;* a0=step
		addq.w	#4, a0						;* skip ptn ptr
		move.w	AS_CURRENTFLIP(a3), d0				;* d0= flips
		;* prep shiftX
		move.l	(a0)+, d1
		ror.b	#1, d0
		bcc.s	0f
		swap	d1
0:		;* prep shiftY
		move.l	(a0)+, d2
		ror.b	#1, d0
		bcc.s	0f
		swap	d2

0:		;*d1=shiftX, d2=shiftY, now scale shifts
		;*shiftX scale
		moveq	#0, d0
		move.b	AS_XBIG(a3), d0
		addq.b	#1, d0
		beq.s	0f
		;* scale shiftX
		muls.w	d0, d1
		asr.l	#8, d1						;*d1=shiftX*scale >> 8
		;* compute final posX
0:		add.w	AS_POSX(a3), d1					;* d1= final posX

		;*shiftY scale
		move.b	AS_YBIG(a3), d0
		addq.b	#1, d0
		beq.s	0f
		;* scale shiftY
		muls.w	d0, d2
		asr.l	#8, d2						;*d2=shiftY*scale >> 8
		;*compute final posY
0:		add.w	AS_POSY(a3), d2
		neg.w	d2
		add.w	#YSHIFT, d2					;* d2= final posY

		;* d1=posX d2=posY ok
5:		move.w	d1, d6
		lsl.w	#7, d6
		move.l	d6, -2(a6)					;* VRAM set posX

		sub.l	#0x2000000, d6					;* 0x8200+ (posY)
		move.w	d2, d6
		lsl.w	#7, d6
		add.w	FRAME_TILEHEIGHT(a2), d6
		move.l	d6, (a5)+					;* buffer posY

		;* link following sprs
		move.w	d5, d0
		beq.s	1f						;* single spr
		subq.w	#1, d0
		moveq	#0x40, d1
		add.l	d6, d1						;* d1=spr addr | data with link
		moveq	#1, d2
		swap	d2						;*d2=spr++
0:		add.l	d2, d1
		move.l	d1, (a5)+					;* buffer spr link
		dbra	d0, 0b

		;* set shrink data
1:		swap	d6
		sub.w	#0x200, d6					;* 0x8000+ (shrink)
		move.w	d6, -2(a6)					;* set shrink addr

		moveq	#0, d1
		move.b	AS_XBIG(a3), d0
		lsl.w	#4, d0						;*d0= bigX remainder

		move.w	d0, d4
		move.b	AS_YBIG(a3), d4					;* d4= carry shrink data
		add.b	#0x10, d0
		bcs.s	2f
		move.w	d4, d3
		sub.w	#0x100, d3					;* d3=shrink data

0:		add.b	d0, d1
		bcs.s	1f
		move.w	d3, (a6)					;* write shrink data
		dbra	d5, 0b
		bra.s	3f
1:		move.w	d4, (a6)					;* write carry shrink data
		dbra	d5, 0b
		bra.s	3f
2:		move.w	d4, (a6)					;* write carry shrink data
		dbra	d5, 2b

		;*shrink data done, draw ptn
3:		swap	d5						;* tileW-1
	;*	move.w	d7, d6						;* not needed (d6=0x8000+spr)
		lsl.w	#6, d6						;* d6= spr addr
		move.w	d6, -2(a6)					;* set LSPC addr

		move.w	AS_CURRENTFLIP(a3), d0
		add.w	d0, d0
		add.w	d0, d0
		move.l	FRAME_MAPS(d0.w, a2), a0			;* get pattern addr

		move.w	AS_BASEPALETTE(a3), d4
		clr.b	d4						;* d4=base palette		4

		move.w	FRAME_COLSIZE(a2), d0 				;* tileH*4
		add.w	d0, d0						;* code to write 1 tile = 8bytes
		cmp.b	#0xff, AS_YBIG(a3)
		beq.s	0f
		;* vertical shrink
		lea	writeBaseScaled(pc), a2
		sub.w	d0, a2
		jmp	(a2)						;* write sprite

0:		;* no vertical shrink
		lea	writeBaseUnscaled(pc), a2
		sub.w	d0, a2
		jmp	(a2)						;* write sprite

writeScaledTop:
		_WRITE_TILE_	;* 32
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 25

		_WRITE_TILE_	;* 24
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 17

		_WRITE_TILE_	;* 16
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 9

		_WRITE_TILE_	;* 8
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_
		_WRITE_TILE_	;* 1
writeBaseScaled:
		move.w	#2, 2(a6)					;* modulo 2
0:		tst.w	(a6)						;*				8
		beq.s	1f						;*				8 not taken, 10 taken
		clr.w	(a6)						;* 				12
		nop							;* need 16 cycles?
		nop							;*
		bra.s	0b						;*				10

1:		move.w	#1, 2(a6)					;* modulo 1
		dbra	d5, 2f						;* next sprite
		;* draw next sprite
		jmp	(a4)						;* return

2:		;* arrange for loop
		add.w	#64, d6						;* spr addr++
		move.w	d6, -2(a6)					;* set spr addr
		jmp	(a2)



;*******************************************************************************************
drawScaledPatternBuffered:
;*	#if	BANKING_ENABLE
;*		move.w	#BANKING_CMD, (a6)+
;*		move.w	AS_BANK-1(a3), (a6)+
;*	#endif

		move.w	#0x8400, d6
		add.w	d7, d6
		swap	d6

		;*setup coords
		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a3)		;* strict coordinates?
		beq.s	0f
		;* set posX
		move.w	AS_POSX(a3), d1					;*d1=posX			12
		;* set posY
		move.w	#YSHIFT, d2					;*				8
		sub.w	AS_POSY(a3), d2					;*d2=posY			12
		bra.s	5f						;*				10

		;* coord = scaled shift + pos
0:		;*std coords, compute shifts
		move.l	AS_CURRENTSTEP(a3), a0				;* a0=step
		addq.w	#4, a0						;* skip ptn ptr
		move.w	AS_CURRENTFLIP(a3), d0				;* d0= flips
		;* prep shiftX
		move.l	(a0)+, d1
		ror.b	#1, d0
		bcc.s	0f
		swap	d1
0:		;* prep shiftY
		move.l	(a0)+, d2
		ror.b	#1, d0
		bcc.s	0f
		swap	d2

		;*d1=shiftX, d1=shiftX, now scale shifts
		;*shiftX scale
0:		moveq	#0, d0
		move.b	AS_XBIG(a3), d0
		addq.b	#1, d0
		beq.s	0f
		;* scale shiftX
		muls.w	d0, d1
		asr.l	#8, d1						;*d1=shiftX*scale >> 8
		;* compute final posX
0:		add.w	AS_POSX(a3), d1					;* d1= final posX

		;*shiftY scale
		move.b	AS_YBIG(a3), d0
		addq.b	#1, d0
		beq.s	0f
		;* scale shiftY
		muls.w	d0, d2
		asr.l	#8, d2						;*d2=shiftY*scale >> 8
		;*compute final posY
0:		add.w	AS_POSY(a3), d2
		neg.w	d2
		add.w	#YSHIFT, d2					;* d2= final posY

		;* d1=posX d2=posY ok
5:		move.w	d1, d6
		lsl.w	#7, d6
		move.l	d6, (a5)+					;* buffer posX

		sub.l	#0x2000000, d6
		move.w	d2, d6
		lsl.w	#7, d6
		add.w	FRAME_TILEHEIGHT(a2), d6
		move.l	d6, (a5)+					;* buffer posY

		;* link following sprs
		move.w	d5, d0
		beq.s	1f						;* single spr
		subq.w	#1, d0
		moveq	#0x40, d1
		add.l	d6, d1						;* d1=spr addr | data with link
		moveq	#1, d2
		swap	d2						;*d2=spr++
0:		add.l	d2, d1
		move.l	d1, (a5)+					;* buffer spr link
		dbra	d0, 0b
		;*d2 = 0x0001 0000

		;* set shrink data
1:		swap	d6
		sub.w	#0x200, d6

		moveq	#0, d1						;*d1 = accu
		move.b	AS_XBIG(a3), d0
		lsl.w	#4, d0						;*d0= bigX remainder

		move.w	d0, d4
		move.b	AS_YBIG(a3), d4					;* d4= carry shrink data
		add.b	#0x10, d0
		bcs.s	2f
		move.w	d4, d3
		sub.w	#0x100, d3					;* d3=shrink data

0:		move.w	d6, (a5)+
		addq.w	#1, d6						;* buffer & inc spr addr
		add.b	d0, d1
		bcs.s	1f						;* accu overflow?
		move.w	d3, (a5)+					;* buffer shrink data
		dbra	d5, 0b
		bra.s	3f
1:		move.w	d4, (a5)+					;* buffer carry shrink data
		dbra	d5, 0b
		bra.s	3f
		;* F remainder
2:		move.w	d6, (a5)+
		addq.w	#1, d6						;* buffer & inc spr addr
		move.w	d4, (a5)+					;* buffer carry shrink data
		dbra	d5, 2b

		;*shrink data done
3:		swap	d5						;* loop

		move.w	AS_CURRENTFLIP(a3), d0
		add.w	d0, d0
		add.w	d0, d0
		move.l	FRAME_MAPS(d0.w, a2), a0			;* a0=ptn data			18

;*		move.w	FRAME_COLSIZE(a2), d0	;* tileH*4
		move.w	FRAME_TILEHEIGHT(a2), d0
		add.w	d0, d0						;* d0=tileH*2

		move.w	AS_BASEPALETTE(a3), d6
		move.b	d0, d6						;* d6=palmod|tileH*2
	
		;* special tag for non-ff Ybig
		cmp.b	#0xff, AS_YBIG(a3)				;* Ybig is 0xff?		16
		beq.s	0f						;*				8 not taken, 10 taken
		add.b	#64, d6						;*				8

0:		swap	d6
		move.w	d7, d6
		lsl.w	#6, d6						;* d6=palmod|H*2|vram addr

		moveq	#64, d1
		add.w	d0, d0						;* d0=tileH*4

0:		move.l	d6, (a6)+
		move.l	a0, (a6)+
		add.w	d1, d6						;* next spr
		add.w	d0, a0						;* data ptr increment
		dbra	d5, 0b

		jmp	(a4)						;* return


;*##########################################################################################

HWSPR_POSX	=	0
HWSPR_POSY	=	2
HWSPR_TILESIZE	=	4
HWSPR_DATA	=	6

_renderNewFrameUp:
		;* spr count is FRAME_TILEHEIGHT on new format
		move.w	FRAME2_SPRCOUNT(a2), d0			;* d0 = spr count
		sub.w	d0, d7
		bmi.w	_bufferNewFrameUp

		move.w	d0, d4
		swap	d4
		move.w	AS_BASEPALETTE(a3), d4
		clr.b	d4						;* d4 = spr count, basepal

		swap	d7
		move.w	d7, AS_BASESPRITE(a3)
		subq.w	#1, d0
		move.w	d0, d5						;* d5 = loop counter
		bra.s	_renderNewFrame

_renderNewFrameDown:
		move.w	FRAME2_SPRCOUNT(a2), d0			;* d0 = spr count
		sub.w	d0, d7
		bmi.w	_bufferNewFrameDown		 		;* neg result

		move.w	d0, d4
		swap	d4
		move.w	AS_BASEPALETTE(a3), d4
		clr.b	d4						;* d4 = spr count, basepal

		swap	d7
		sub.w	d0, d7						;* predec sprite to use #
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num
		subq.w	#1, d0
		move.w	d0, d5
	;*	bra.w	_renderNewFrame

_renderNewFrame:
		;* get data for current flip
		move.w	AS_CURRENTFLIP(a3), d1
		move.w	d1, d0
		add.w	d1, d1
		add.w	d1, d1
		move.l	4(a2, d1.w), a2					;* a2=data

		;* compute base posX/Y
		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a3)		;* strict coordinates?
		beq.s	0f
		;* setup base shiftX/shiftY
		move.w	AS_POSX(a3), d1					;*d1=posX			12
		move.w	#YSHIFT, d2					;*				8
		sub.w	AS_POSY(a3), d2					;*d2=posY			12
		bra.s	4f

0:		;* anchor shift posX / posY
		move.l	AS_CURRENTSTEP(a3), a0				;* a0=step
		addq.w	#4, a0						;* skip ptn ptr
		;* prep shiftX
		move.l	(a0)+, d1
		ror.b	#1, d0
		bcc.s	0f
		swap	d1
0:		;* prep shiftY
		move.l	(a0)+, d2
		ror.b	#1, d0
		bcc.s	0f
		swap	d2
0:		;* compute final shiftX/shiftY
		add.w	AS_POSX(a3), d1
		add.w	AS_POSY(a3), d2
		neg.w	d2
		add.w	#YSHIFT, d2

4:		move.w	d7, d3						;* d3= spr#
		move.w	d7, d0
		lsl.w	#6, d0
		move.w	d0, a3						;* a3= spr vram addr
		move.l	#0x2000000, a0
5:		;* set final attributes for this chunk	<==== spr loop
		move.w	#0x8400, d6
		add.w	d3, d6
		swap	d6
		;* posX
		move.w	(a2)+, d6					;* posX
		add.w	d1, d6
		lsl.w	#7, d6
		move.l	d6, -2(a6)					;* write posX
		;* posY
		sub.l	a0, d6
		move.w	d2, d6
		sub.w	(a2)+, d6					;* posY
		lsl.w	#7, d6
		add.w	(a2), d6					;* add tileHeight
		move.l	d6, (a5)+					;* buffer posY
		;* shrink
		sub.l	a0, d6
		move.w	#0x0fff, d6
		move.l	d6, -2(a6)					;* write shrink value

		;* set spr address
	;*	move.w	d3, d0
	;*	lsl.w	#6, d0
	;*	move.w	d0, -2(a6)
		move.w	a3, -2(a6)
		;* write pattern
		moveq	#32, d0
		sub.w	(a2)+, d0					;* sub tileHeight
		lsl.w	#3, d0
		jmp	_newWriteTop(pc, d0.w)

_newWriteTop:
	.rept	32
		;* 8 bytes / 32 cy
		move.w	(a2)+, (a6)					;* write tile word1		12
		move.w	(a2)+, d0					;* read tile2			8
		add.w	d4, d0						;* add basepalette		4
		move.w	d0, (a6)					;* write tile word2		8
	.endr
_newWriteBase:
		addq.w	#1, d3
		lea	64(a3), a3
		dbra	d5, 5b						;* iterate strip **********
		;* wrote all HW sprites
		jmp	(a4)


;* from _renderNewFrameUp/_renderNewFrameDown - already determined frame is new format
_bufferNewFrameUp:
		;* d0 has tilewidth
		;* update ptrs
		move.l	SC1ptr, a6					;* a6=SC1ptr
		lea	spritePoolDrawList3_upReturn2(pc), a4		;* return spot
		;* compute gap to pool end
		move.l	d7, d1
		swap	d1						;* d1=current up
		move.l	48(sp), a0
		move.w	SPOOL_POOLEND(a0), d7
		sub.w	d1, d7
		addq.w	#1, d7						;* updated gap
		bra.s	3f						;* we know we must render it

_bfrNewFrmUp2:	move.w	FRAME2_SPRCOUNT(a2), d0
3:		sub.w	d0, d7
		bmi.w	spritePoolDrawList3_upSkip
		move.w	d0, d4
		swap	d4

		move.w	AS_BASEPALETTE(a3), d4
		clr.b	d4

		swap	d7
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num
		subq.w	#1, d0
		move.w	d0, d5
	;*	swap	d5
	;*	move.w	d0, d5
		bra.s	_bufferNewFrame

_bufferNewFrameDown:
		;* d0 has tilewidth
		;* update ptrs
		move.l	SC1ptr, a6					;* a6=SC1ptr
		lea	spritePoolDrawList3_downReturn2(pc), a4		;* return spot
		;* compute gap to pool start
		move.l	d7, d1
		swap	d1						;* d1=current down
		move.l	48(sp), a0
		move.w	d1, d7
		sub.w	SPOOL_POOLSTART(a0), d7				;* updated gap
		bra.s	3f						;* we know we must render it

_bfrNewFrmDn2:	move.w	FRAME2_SPRCOUNT(a2), d0
3:		sub.w	d0, d7
		bmi.w	spritePoolDrawList3_downSkip 			;* neg result
		move.w	d0, d4
		swap	d4

		move.w	AS_BASEPALETTE(a3), d4
		clr.b	d4

		swap	d7
		sub.w	d0, d7						;* predec sprite to use #
		move.w	d7, AS_BASESPRITE(a3)				;* set spr num			8
		subq.w	#1, d0
		move.w	d0, d5
	;*	swap	d5
	;*	move.w	d0, d5
	;*	bra.s	_bufferNewFrame


_bufferNewFrame:
;*	#if	BANKING_ENABLE
;*		move.w	#BANKING_CMD, (a6)+
;*		move.w	AS_BANK-1(a3), (a6)+
;*	#endif
	
		;* get data for current flip
		move.w	AS_CURRENTFLIP(a3), d1
		move.w	d1, d0
		add.w	d1, d1
		add.w	d1, d1
		move.l	4(a2, d1.w), a2					;* a2=data

		;* compute base posX/Y
		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a3)		;* strict coordinates?
		beq.s	0f
		;* setup base shiftX/shiftY
		move.w	AS_POSX(a3), d1					;*d1=posX			12
		move.w	#YSHIFT, d2					;*				8
		sub.w	AS_POSY(a3), d2					;*d2=posY			12
		bra.s	4f

0:		;* anchor shift posX / posY
		move.l	AS_CURRENTSTEP(a3), a0				;* a0=step
		addq.w	#4, a0						;* skip ptn ptr
		;* prep shiftX
		move.l	(a0)+, d1
		ror.b	#1, d0
		bcc.s	0f
		swap	d1
0:		;* prep shiftY
		move.l	(a0)+, d2
		ror.b	#1, d0
		bcc.s	0f
		swap	d2
0:		;* compute final shiftX/shiftY
		add.w	AS_POSX(a3), d1
		add.w	AS_POSY(a3), d2
		neg.w	d2
		add.w	#YSHIFT, d2

4:		move.w	d7, d3						;* d3= spr#
		move.l	#0x2000000, a0
5:		;* set final attributes for this chunk	<==== spr loop
		move.w	#0x8400, d6
		add.w	d3, d6
		swap	d6
		;* posX
		move.w	(a2)+, d6					;* posX
		add.w	d1, d6
		lsl.w	#7, d6
		move.l	d6, (a5)+					;* buffer posX
		;* posY
		sub.l	a0, d6
		move.w	d2, d6
		sub.w	(a2)+, d6					;* posY
		lsl.w	#7, d6
	;*	add.w	(a2), d6					;* add tileHeight
		move.w	(a2)+, d0
		add.w	d0, d6
		move.l	d6, (a5)+					;* buffer posY
		;* shrink
		sub.l	a0, d6
		move.w	#0x0fff, d6
		move.l	d6, (a5)+					;* buffer shrink

		;* buffer sprite pattern
	;*	add.w	d4, d0		;* pal,Hsize
		add.w	d0, d0
		add.w	d4, d0		;* pal,Hsize*2

		move.w	d0, (a6)+
		;* set spr address
		move.w	d3, d6
		lsl.w	#6, d6
		move.w	d6, (a6)+
		;* pattern address 
		move.l	a2, (a6)+
		;* advance pointer
		ext.w	d0
	;*	add.w	d0, d0		;* removed, d0 has Hsize*2
		add.w	d0, d0		;* Hsize*4
		add.w	d0, a2

		;* loop to next spr
		addq.w	#1, d3
		dbra	d5, 5b						;* iterate strip
		;* wrote all HW sprites
		jmp	(a4)

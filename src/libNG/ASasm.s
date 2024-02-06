	.include "src/libNG/defines.inc"
	#include <configNG.h>
	.align	2

;* /******************************************************************************
;* 				aSpriteInit
;* ******************************************************************************/

;* void aSpriteInit(aSprite *as, const spriteInfo *si, ushort baseSprite, uchar basePalette, short posX, short posY, ushort anim, ushort flip, ushort flags);

.globl aSpriteInit
aSpriteInit:
	.set	_ARGS, 8
	.set	_aSprite, _ARGS
	.set	_sprInfo, _ARGS+4
	.set	_baseSpr, _ARGS+8+2	;* word
	.set	_basePal, _ARGS+12+3	;* byte
	.set	_posX, _ARGS+16+2	;* word
	.set	_posY, _ARGS+20+2	;* word
	.set	_anim, _ARGS+24+2	;* word
	.set	_flip, _ARGS+28+2	;* word
	.set	_flags, _ARGS+32+2	;* word
		move.l	d2, -(sp)					;* push				12

		move.l	_aSprite(sp), a0				;* a0=as			16
	#if	BANKING_ENABLE
		move.b	_sprInfo(sp), d0
		move.b	d0, REG_BANKING					;* bankswitch
		move.b	d0, AS_BANK(a0)
	#endif
		move.l	_sprInfo(sp), a1				;* a1=si			16
		move.w	#1, AS_COUNTER(a0)
		moveq	#0, d0						;* 				4
	;*	move.l	d0, AS_COUNTER(a0)				;* as->counter=0		16
		move.l	d0, AS_CURRENTFRAME(a0)				;* as->currentFrame=0		16		//prevents same frame skipping glitch
		move.w	d0, AS_REPEATS(a0)				;* as->repeats=0		12
		moveq	#-1, d0
		move.w	d0, AS_CURRENTSTEPNUM(a0)			;* as->currentStepNum=-1 	16
		move.w	d0, AS_XBIG(a0)					;* Xbig/Ybig=0xff		16
		move.b	_basePal(sp), AS_BASEPALETTE(a0)		;* as->basePalette=palette 	20
		move.w	_posX(sp), AS_POSX(a0)				;* as->posX=posX		20
		move.w	_posY(sp), AS_POSY(a0)				;* as->posY=posY		20
		move.w	_flip(sp), AS_CURRENTFLIP(a0)			;* as->currentFlip=flip		20
		move.w	_flags(sp), AS_FLAGS(a0)			;* as->flags=flags		20
		move.l	SI_ANIMS(a1), AS_ANIMS(a0)			;* as->anims=si->anims		28

		move.w	_anim(sp), d1					;* d0=anim			12
		move.w	d1, AS_ANIMID(a0)				;* as->animID=anim		12
		move.w	d1, AS_CURRENTANIMID(a0)			;* currentAnimID=anim		12
		add.w	d1, d1						;* animation=4b each		4
		add.w	d1, d1						;* d1=anim*4			4
		move.w	SI_MAXWIDTH(a1), d2				;* d2=maxwidth			12

		move.l	SI_ANIMS(a1), a1				;* a1=si->anims			16
		move.l	(d1.w, a1), a1					;*				18

		move.l	a1, AS_STEPS(a0)				;* as->steps=(animStep*)as->anims[anim].data	16
		move.l	a1, d0						;* 				4
		sub.l	#STEP_BYTESIZE, d0				;* sets ptr to step-1		16
		move.l	d0, AS_CURRENTSTEP(a0)				;* as->currentStep		16

		move.l	STEP_FRAME(a1), a1				;* a1=frame			16
		move.w	FRAME_TILEWIDTH(a1), d0				;* d0=as->steps->frame->tileWidth	12
		move.w	d0, AS_TILEWIDTH(a0)				;* as->tileWidth=d0		12
		move.w	_baseSpr(sp), d1				;* d1=sprite			12
		bmi.s	9f						;* no init / pool mode flag

		;* regular init
		move.w	d1, AS_BASESPRITE(a0)				;* as->baseSprite=d1	12

		;* unlink/shrinkreset sprites
		movea.l	SC234ptr, a0					;* a0=SC234ptr			20
		;* a0 has buffer ptr
		swap	d1						;*				4
		;*move.w SI_MAXWIDTH(a1), d2				;* d2=maxwidth			12
		move.w	d2, d1						;* d1=basespite,maxwidth   d0=tilewidth	4

		neg.w	d0 						;* d0=-tilewidth		4
		add.w	d1, d0						;* d0=maxwidth-tilewidth	4
		beq.s	1f						;* 0 loop? skip			8 not taken, 10 taken
		subq.w	#1, d0						;* d0=loop count		4
		move.l	#0x88008200, d2 				;* d2=data, addr		12
		add.w	d1, d2						;* +=maxwidth			4
		swap	d1						;* 				4
		add.w	d1, d2						;* +=basesprite			4
		move.l	#0x10000, a1					;* 				12		+12
		swap	d2						;* 				4		+4
0:
;*		subq.w #1, d2						;* 				4
		sub.l	a1, d2						;* 				8		+4
		move.l	d2, (a0)+					;* queue spr unlink		12
		dbra	d0, 0b						;* 				10 taken, 14 not taken
;*		bra.s 2f
1:
;*		swap d1							;*				4

2:
;*		move.l #0x10000, a1					;*				12		+12
;*		move.l #0x0fff8000, d2					;* d2=data, addr		12
;*		add.w d1, d2						;* +=basesprite			4
;*		swap d1							;* 				4
;*		subq.w #1, d1						;* d1=loop			4
;*		swap d2							;*				4		+4
;*0:
;*		move.l d2, (a0)+					;* queue shrink reset		12
;*		add.l a1, d2						;*				8
;*		dbra d1, 0b						;*				10 taken, 14 not taken
		move.l	a0, SC234ptr					;* save ptr			20
		move.l	(sp)+, d2					;* pop				12
		rts

9:		;* was sprite 0x8--- (pool / no init)
		and.w	#0x7fff, d1
		move.w	d1, AS_BASESPRITE(a0)				;* as->baseSprite=d1		12
		move.l	(sp)+, d2					;* pop				12
		rts


;* /******************************************************************************
;* 				aSpriteSetAnim
;* ******************************************************************************/

	.set _ARGS, 4
	.set _aSprite, _ARGS
	.set _anim, _ARGS+4+2	;* word

.globl aSpriteSetAnim2
aSpriteSetAnim2:
		move.l	_aSprite(sp), a0				;* a0=as			16
		move.w	_anim(sp), d0					;* d0=anim			12
		bra.s	0f

.globl aSpriteSetAnim
aSpriteSetAnim:
		move.l	_aSprite(sp), a0				;* a0=as			16
		move.w	_anim(sp), d0					;* d0=anim			12
		cmp.w	AS_ANIMID(a0), d0				;* same ID? return		12
		beq.s	9f						;*				8 not taken, 10 taken
	
0:		move.w	d0, AS_ANIMID(a0)				;* update ID			12
		move.w	d0, AS_CURRENTANIMID(a0)			;* update current ID		12
		move.w	#-1, AS_CURRENTSTEPNUM(a0)			;* as->currentStepNum=-1	 16
	;*	moveq	#0, d1						;*				4
	;*	move.l	d1, AS_COUNTER(a0)				;* as->counter=0		16
	;*	move.w	d1, AS_REPEATS(a0)				;* as-> repeats=0		12
		move.w	#1, AS_COUNTER(a0)				;* as->counter=1		16
		move.w	#0, AS_REPEATS(a0)				;* as-> repeats=0		16

		add.w	d0, d0						;* animation			4
		add.w	d0, d0						;* =4B each			4

	#if	BANKING_ENABLE
		move.b	AS_BANK(a0), REG_BANKING			;* bankswitch			24
	#endif

		move.l	AS_ANIMS(a0), a1				;*				16
		move.l	(d0.w, a1), a1					;*				18
		move.l	a1, AS_STEPS(a0)				;* as->steps=(animStep*)as->anims[anim].data	16
		sub.w	#STEP_BYTESIZE, a1				;* sets ptr to step-1		12
		move.l	a1, AS_CURRENTSTEP(a0)				;* as->currentStep		16
9:		rts


;* /******************************************************************************
;* 				aSpriteSetAnimStep
;* ******************************************************************************/

	.set _ARGS, 4
	.set _aSprite, _ARGS
	.set _anim, _ARGS+4+2	;* word
	.set _step, _ARGS+8+2	;* word

.globl aSpriteSetAnimStep2
aSpriteSetAnimStep2:
		move.w	_step(sp), d1					;* d1=step			12
		beq.s	aSpriteSetAnim2					;* just load anim if step is 0
		move.l	_aSprite(sp), a0				;* a0=as			16
		move.w	_anim(sp), d0					;* d0=anim
		bra.s	0f

.globl aSpriteSetAnimStep
aSpriteSetAnimStep:
		move.w	_step(sp), d1					;* d1=step			12
		beq.w	aSpriteSetAnim					;* just load anim if step is 0
		move.l	_aSprite(sp), a0				;* a0=as			16
		move.w	_anim(sp), d0					;* d0=anim
		cmp.w	AS_ANIMID(a0), d0				;* same ID ?
		beq.s	4f
		;*set anim & step
	
		;* ==== step is > 0 ====
0:		move.w	d0, AS_ANIMID(a0)				;* update ID			12
		move.w	d0, AS_CURRENTANIMID(a0)			;* update current ID		12

		add.w	d0, d0						;* animation			4
		add.w	d0, d0						;* =4B each			4

	#if	BANKING_ENABLE
		move.b	AS_BANK(a0), REG_BANKING			;* bankswitch			24
	#endif

		move.l	AS_ANIMS(a0), a1				;*				16
		move.l	(d0.w, a1), a1					;*				18
		move.l	a1, AS_STEPS(a0)				;* as->steps=(animStep*)as->anims[anim].data	16

		subq.w	#1, d1
		move.w	d1, AS_CURRENTSTEPNUM(a0)			;* as->currentStepNum=-1 12?
		;* step*=size
		_COMPUTE_STEP_	d1, a1					;* a1+= stepsize * step#
		move.l	a1, AS_CURRENTSTEP(a0)				;* as->currentStep		16
	
	;*	moveq	#0, d1						;*				4
	;*	move.l	d1, AS_COUNTER(a0)				;* as->counter=0		16
	;*	move.w	d1, AS_REPEATS(a0)				;* as-> repeats=0		12
		move.w	#1, AS_COUNTER(a0)				;* as->counter=1		16
		clr.w	AS_REPEATS(a0)					;* as-> repeats=0		16

		rts


;* /******************************************************************************
;* 				aSpriteSetStep
;* ******************************************************************************/

	.set _ARGS, 4
	.set _aSprite, _ARGS
	.set _step, _ARGS+4+2	;* word

.globl aSpriteSetStep2
aSpriteSetStep2:
		move.l	_aSprite(sp), a0				;* a0=as			16
		move.w	_step(sp), d1					;* d1=step			12
		bra.s	5f

.globl aSpriteSetStep
aSpriteSetStep:
		move.l	_aSprite(sp), a0				;* a0=as			16
		move.w	_step(sp), d1					;* d1=step			12
4:		cmp.w	AS_CURRENTSTEPNUM(a0), d1			;* same step? return		12
		beq.s	9f

5:		move.w	d1, d0
		subq.w	#1, d0
		move.w	d0, AS_CURRENTSTEPNUM(a0)			;* as->currentStepNum=-1	12?
		move.l	AS_STEPS(a0), a1
		_COMPUTE_STEP_	d1, a1					;* a1+= stepsize * step#
		sub.w	#STEP_BYTESIZE, a1				;*				12
		move.l	a1, AS_CURRENTSTEP(a0)				;* as->currentStep		16
	
	;*	moveq	#0, d1						;*				4
	;*	move.l	d1, AS_COUNTER(a0)				;* as->counter=0		16
;*	;*	move.w	d1, AS_REPEATS(a0)				;* as-> repeats=0		12
		move.w	#1, AS_COUNTER(a0)				;* as->counter=1		16
;*		clr.w	AS_REPEATS(a0)					;* as-> repeats=0		16

9:		rts


;* /******************************************************************************
;* 				aSpriteSetPos
;* ******************************************************************************/

.globl aSpriteSetPos
aSpriteSetPos:
	.set	_ARGS, 4

		move.l	_ARGS(sp), a0					;* a0=as			16	
		move.l	_ARGS+4+2(sp), d0				;* d0=newX|0000			16
		move.w	_ARGS+8+2(sp), d0				;* d0=newX|newY			12
		cmp.l	AS_POSX(a0), d0					;*				18
		beq.s	9f						;*				8 not taken, 10 taken

		move.l	d0, AS_POSX(a0)					;*				16
		ori.b	#AS_FLAG_MOVED, AS_FLAGS+1(a0)			;*				20
9:		rts


;* /******************************************************************************
;* 				aSpriteMove
;* ******************************************************************************/

.globl aSpriteMove
aSpriteMove:
	.set	_ARGS, 4

		move.l	_ARGS(sp), a0					;* a0=as			16
		move.w	_ARGS+6(sp), d0					;* d0=shiftX			12
		beq.s	0f						;* ==0? next			8 not taken, 10 taken
		add.w	d0, AS_POSX(a0)					;*				16
		ori.b	#AS_FLAG_MOVED, AS_FLAGS+1(a0)			;* flag as moved		20

			move.w	_ARGS+10(sp), d0			;* d0=shiftY			12
			;* slower on avg.
			;*	beq.s	9f				;*				8 not taken, 10 taken
			add.w	d0, AS_POSY(a0)				;*				16
			rts

0:		move.w	_ARGS+10(sp), d0				;* d0=shiftY			12
		beq.s	9f						;* 				8 not taken, 10 taken
		add.w	d0, AS_POSY(a0)					;* 				16
		ori.b	#AS_FLAG_MOVED, AS_FLAGS+1(a0)			;* flag as moved		20

9:		rts


;* /******************************************************************************
;* 				aSpriteSetFlip
;* ******************************************************************************/

.globl aSpriteSetFlip
aSpriteSetFlip:
	.set	_ARGS, 4
	
		move.l	_ARGS(sp), a0					;* a0=as			nah need16
		move.w	_ARGS+6(sp), d0					;* d0=flip			12
		cmp.w	AS_CURRENTFLIP(a0), d0				;* same flip? end		12
		beq.s	0f						;*				8 not taken, 10 taken
		move.w	d0, AS_CURRENTFLIP(a0)				;* sets new flip		12
		bset.b	#B_AS_FLIPPED, AS_FLAGS+1(a0)			;* flag as flipped		20

0:		rts


;* /******************************************************************************
;* 				aSpriteSetScale
;* ******************************************************************************/

;* void aSpriteScale(aSprite *as, byte Xbig, byte Ybig) {

.globl aSpriteSetScale
aSpriteSetScale:
	.set	_ARGS, 4
	
		move.l	_ARGS(sp), a0					;* a0=as			16
		move.b	_ARGS+4+3(sp), -(sp)				;* d0=Xbig			12
		move.w	(sp)+, d0
		move.b	_ARGS+8+3(sp), d0				;* d0=Xbig|Ybig			12
	
		cmp.w	AS_XBIG(a0), d0					;* same scale? end		12
		beq.s	0f						;*				8 not taken, 10 taken
		move.w	d0, AS_XBIG(a0)					;* sets new flip		12
		bset.b	#B_AS_MOVED, AS_FLAGS+1(a0)			;* flag as moved		20

0:		rts


;* /******************************************************************************
;* 				aSpriteAnimateSingle
;* ******************************************************************************/

;* void aSpriteAnimateSingle(aSprite *as) 
.globl aSpriteAnimateSingle
aSpriteAnimateSingle:
	.set	_ARGS, 4
	
		move.l	_ARGS(sp), a0					;* a0=as			16
	
	#if	BANKING_ENABLE
		move.b	AS_BANK(a0), REG_BANKING			;* bankswitch			24
	#endif
	
		;* animation macro
		;* rd0, rd1, ra1: scratch data/addr registers
		;* ra0:	addr register, holds AS ptr
		___AS_ANIMATION_BLOCK___ d0 d1 a0 a1

9:		rts

;* /******************************************************************************
;* 				aSpriteAnimateList
;* ******************************************************************************/

;* void aSpriteAnimateList(aSprite *as) 
.globl aSpriteAnimateList
aSpriteAnimateList:
	.set	_ARGS, 8
	
		move.l	a2, -(sp)
		move.l	_ARGS(sp), a2					;* a2=list			16
	
		move.l	libNG_frameCounter, d0
		move.l	(a2)+, d1
		beq.w	9f
0:		move.l	d1, a0

	#if	BANKING_ENABLE
		move.b	AS_BANK(a0), REG_BANKING			;* bankswitch			24
	#endif

		;* animation macro
		;* rd0, rd1, ra1: scratch data/addr registers
		;* ra0:	addr register, holds AS ptr
		___AS_ANIMATION_BLOCK___ d0 d1 a0 a1

		move.l	(a2)+, d1
		bne.w	0b

9:		move.l	(sp)+, a2
		rts



;* ##############################################################################
;*		Old style fixed allocation sprite subs, deprecated
;* ##############################################################################


;*/******************************************************************************
;*				aSpritePush - internal, print pattern
;*******************************************************************************/

;*	.globl aSpritePush_int
aSpritePush_int:
	;*a0=as
	;*	move.l a2, -(sp)					;*push				12
		movem.l	d2-d4/a2, -(sp)					;*				40
	
		and.b	#AS_MASK_MOVED_FLIPPED, AS_FLAGS+1(a0)		;*clear flags			20
	
		move.l	AS_CURRENTFRAME(a0), a2				;*as->a1=currentframe		16

;*			move.w	AS_BASEPALETTE(a0), d0			;*				12
;*		;*	lsl.w #6, d0					;*				18
;*		;*	or.w FRAME_TILEHEIGHT(a2), d0			;*				12
;*		;*	add.w d0, d0					;*				4
;*		;*	add.w d0, d0					;*				4		38
;*			lsl.w	#8, d0					;*				22
;*			or.w	FRAME_COLSIZE(a2), d0			;*				12		34

;*		move.w	AS_BASEPALETTE(a0), d0				;*				12
;*		lsl.w	#8, d0						;*				22
;*		or.w	FRAME_TILEHEIGHT(a2), d0			;*				12
;*		add.b	d0, d0						;*				4		38 (50)
		move.w	AS_BASEPALETTE(a0), d0				;*				12
		move.b	FRAME_TILEHEIGHT+1(a2), d0			;*				12
		add.b	d0, d0						;*				4		28

		swap	d0						;*				4
		move.w	AS_BASESPRITE(a0), d0				;*AS_BASESPRITE			8
		lsl.w	#6, d0						;*				18
		;*d0 has base cmd

;*		move.w FRAME_COLSIZE(a2), d2				;*				12
;*		swap d2							;*				4
;*		move.w FRAME_TILEWIDTH(a2), d2				;*d2=colsize,tilewidth		12
		;*move.l	FRAME_TILEWIDTH(a2), d2			;*				16
		move.l	(a2), d2					;*				12
		add.w	d2, d2						;*				4
		swap	d2						;*d2=colsize,tilewidth		4

		move.w	AS_CURRENTFLIP(a0), d4				;*				12
		add.w	d4, d4						;*				4
		add.w	d4, d4						;*flip*=4			4
		move.l	(FRAME_MAPS,d4.w,a2), d1			;*				18
		;*d1 has tilempap addr

		move.w	d2, d4						;*d4=tilewidth			4

		movea.l	SC1ptr, a2					;*a2=SC1ptr			20
		movea.l	SC234ptr, a1					;*a1=SC234ptr			20	

		subq.w	#1, d4						;*				4
		;*d4 has loop, a2 has SC1, a1 has SC234
	
		moveq	#0, d3						;*				4
		swap	d2						;*				4
		move.w	d2, d3						;*				4
		add.w	d3, d3						;*d3.w=colsize(bytes)		4
	
		move.l	#0x00408200, d2					;*stick bit+addr		12
		add.w	AS_BASESPRITE(a0), d2				;*				12
		swap	d2						;*				4			+4
		;*d2= SC234 data, addr

;*write
		move.l	d0, (a2)+					;*queue cmd			12
		move.l	d1, (a2)+					;*queue addr			12
		dbra	d4, 0f						;*
		move.l	a2, SC1ptr					;**save ptr			20
		movem.l	(sp)+, d2/d3/d4					;*				36
		bra.s	altPushPosEntry					;*				10

0:		add.w	#64, d0						;*cmd+=64
		add.l	d3, d1						;*addr+=col bytesize
		;*addq.w #1, d2						;*next sprite			4
		add.l	#0x10000, d2					;*				16			+12
		move.l	d0, (a2)+					;*queue cmd			12
		move.l	d1, (a2)+					;*queue addr			12
		move.l	d2, (a1)+					;*queue sprite link		12							//could be optimized
		dbra	d4, 0b						;*

		move.l	a2, SC1ptr					;*save ptr			20
		movem.l	(sp)+, d2/d3/d4					;*				36
		bra.s	altPushPosEntry					;*				10


;*/******************************************************************************
;*				aSpritePushPos // internal, update position
;*******************************************************************************/

aSpritePushPos_int:
		;*a0=as

		move.l	a2, -(sp)					;*push				12
		movea.l	SC234ptr, a1					;*a1=SC234ptr			20
		and.b	#AS_MASK_MOVED, AS_FLAGS+1(a0)			;*CLEAR FLAG

altPushPosEntry:							;*a1=SC234, ptr updated flags cleared
		move.l	AS_CURRENTSTEP(a0), a2				;*a2=as->currentstep		16
	;*	move.l	STEP_SHIFTX(a2), d1				;*d1=shiftX/shiftY		16
		move.w	STEP_SHIFTX(a2), d1
		swap	d1
		move.w	STEP_SHIFTY(a2), d1
		move.l	AS_CURRENTFRAME(a0), a2				;*a2=as->currentFrame		16

		btst.b	#B_AS_STRICTCOORDS, AS_FLAGS+1(a0)		;* strict coordinates?
		beq.s	0f
		;* strict posY
		move.w	#0x8200, d0
		add.w	AS_BASESPRITE(a0), d0				;* AS_BASESPRITE(a0)
		swap	d0
		move.w	#YSHIFT, d0
		sub.w	AS_POSY(a0), d0					;*				12
		lsl.w	#7, d0
		or.w	FRAME_TILEHEIGHT(a2), d0
		move.l	d0, (a1)+
		;* strict posX
		add.l	#0x2000000, d0					;* best cycles/size		16
		move.w	AS_POSX(a0), d0
		bra.s	2f

0:		;* POSY set
		move.w	#0x8200, d0					;*				8
		add.w	AS_BASESPRITE(a0), d0				;*AS_BASESPRITE(a0)		8?
		swap	d0						;*				4

		btst.b	#1, AS_CURRENTFLIP+1(a0)			;* Y flipped?			16
		beq.s	0f						;*				8 not taken, 10 taken
		;*is Y flipped
		move.w	FRAME_TILEHEIGHT(a2), d0			;*				12
		lsl.w	#4, d0						;*				14
		add.w	d0, d1						;*				4
		neg.w	d1						;*				4
		add.w	AS_POSY(a0), d1					;*				12
		move.w	#YSHIFT-1, d0					;*				8
		sub.w	d1, d0						;*				4
		bra.s	1f						;*				10
	
		;*isn't Y flipped
0:		move.w	#YSHIFT, d0					;*				8
		add.w	AS_POSY(a0), d1					;*				12
		sub.w	d1, d0						;*				4

1:		lsl.w	#7, d0						;*				20
		or.w	FRAME_TILEHEIGHT(a2), d0			;*				12
		move.l	d0, (a1)+					;*queue posY update		12

		swap	d1						;*d1=shiftY/shiftX		4
	
		;*POSX set
		add.l	#0x2000000, d0					;* best cycles/size		16

		btst.b	#0, AS_CURRENTFLIP+1(a0)			;* X flipped?			16
		beq.s	0f						;*				8 not taken, 10 taken
		;*is X flipped
		move.w	FRAME_TILEWIDTH(a2), d0				;*				12
		lsl.w	#4, d0						;*d0=tileWidth<<4		14
		add.w	d1, d0						;*d0=(tileWidth<<4)+shiftX	4
		subq.w	#1, d0						;*flipped posX fix		4
		neg.w	d0						;*d0=-(tileWidth<<4)+shiftX	4
		add.w	AS_POSX(a0), d0					;*d0=posX-(tileWidth<<4)+shiftX	12
		bra.s	2f						;*				10
	
		;*isn't X flipped
0:		move.w	AS_POSX(a0), d0					;*d0=as->posX			12
		add.w	d1, d0						;* +=as->currentStep->shiftX	4

2:		lsl.w	#7, d0						;*d0<<7				20
		move.l	d0, (a1)+					;*queue posX update		12

		move.l	a1, SC234ptr					;*save ptr			20
		move.l	(sp)+, a2					;*pop				12
		rts
	
;*/******************************************************************************
;*				aSpriteAnimate
;*******************************************************************************/
	
.globl aSpriteAnimate
aSpriteAnimate:
	.set	_ARGS, 4

		move.l	_ARGS(sp), a0					;*a0=as				16

	#if	BANKING_ENABLE
		move.b	AS_BANK(a0), REG_BANKING			;* bankswitch			24
	#endif

	;*	move.l	libNG_frameCounter, d0				;*				20
	;*	cmp.l	AS_COUNTER(a0), d0				;*				18
	;*	blo.s	700f						;*(unsigned compare)		8 not taken, 10 taken
		tst.b	AS_FLAGS(a0)					;* test anim stop flag		12
		bmi.s	700f						;*				8/10
		subq.w	#1, AS_COUNTER(a0)				;*				16
		bne.s	700f	;* anim_done	

		;*must move to next step/repeat/link
		move.l	AS_CURRENTSTEP(a0), a1				;*				16
		lea	STEP_BYTESIZE(a1), a1				;* next step			8

100:		move.b	(a1), d1
		bpl.s	200f	;* anim_do_next_step
		add.b	d1, d1
		beq.s	300f	;* anim_do_repeat
		bmi.s	400f	;* anim_ended

 		;* anim_do_link:
		addq.w	#2, a1						;*				4
		move.w	(a1)+, AS_CURRENTANIMID(a0)			;* update current anim id	16
		move.l	(a1), a1					;*				12
		move.l	a1, AS_STEPS(a0)				;*				16
		move.l	a1, AS_CURRENTSTEP(a0)				;*				16
		moveq	#0, d1						;*always 0			4
		move.w	d1, AS_REPEATS(a0)				;*				12
		move.w	d1, AS_CURRENTSTEPNUM(a0)			;*				12
		bra.s	600f	;* anim_common_block	

300:		;* anim_do_repeat:
		move.w	AS_REPEATS(a0), d1				;*d1=as->repeats		12
		addq.w	#2, a1
		cmp.w	(a1)+, d1					;*				12
		bhs.s	100b	;* repeats done
		;* repeating
		addq.w	#1, d1						;*				4
		move.w	d1, AS_REPEATS(a0)				;*				12
		move.l	AS_STEPS(a0), a1				;*				16
		move.l	a1, AS_CURRENTSTEP(a0)				;*				16
		clr.w	AS_CURRENTSTEPNUM(a0)				;*	saves 2 bytes		16
		bra.s	600f	;* anim_common_block	

400:		;* anim_ended:
	;*	moveq	#-1, d1						;*				4
	;*	move.l	d1, AS_COUNTER(a0)				;*display forever		16
		bset.b	#B_AS_ANIMSTOP, AS_FLAGS(a0)			;*				20
		bra.s	700f	;* anim_done	

200:		;* anim_do_next_step:
		move.l	a1, AS_CURRENTSTEP(a0)				;*				16
		addq.w	#1, AS_CURRENTSTEPNUM(a0)			;*				16
	;*	bra.s	600f	;* anim_common_block			;*				10

		;*animating section
600:		;* anim_common_block:					;*a0=as a1=as->currentanim d0=libNG_frameCounter
		bra.w	aSpriteAnimate_common_block2

700:
	;*	bra.w	aSpriteAnimate_same_step2

aSpriteAnimate_same_step2:					;* step didn't change, check if moved or flipped
		;*check no display flag
		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a0)			;*no display flag?		16
		bne.s	9f						;*is set>rts			8 not taken, 10 taken

		btst.b	#B_AS_FLIPPED, AS_FLAGS+1(a0)			;*flipped flag?			16
		bne.w	aSpritePush_int					;*is set > jmp			8 not taken, 10 taken

		btst.b	#B_AS_MOVED, AS_FLAGS+1(a0)			;*moved flag?			16
		bne.w	aSpritePushPos_int				;*is set > jmp			8 not taken, 10 taken

9:		rts

;*animating section
aSpriteAnimate_common_block2:					;*a0=as a1=as->currentanim d0=libNG_frameCounter
	;*	moveq	#0, d1						;* //
	;*	move.w	STEP_DURATION(a1), d1				;*				12
	;*	add.l	d1, d0						;*				8
	;*	move.l	d0, AS_COUNTER(a0)				;*update counter		16
		move.w	STEP_DURATION(a1), AS_COUNTER(a0)		;*				20

		move.l	AS_CURRENTFRAME(a0), d0				;*d0=old frame			16
		move.l	STEP_FRAME(a1), AS_CURRENTFRAME(a0)		;*update frame ptr		28

		btst.b	#B_AS_NODISPLAY, AS_FLAGS+1(a0)			;*no display flag?		16
		bne.s	9b						;*1? exit			8 not taken, 10 taken
	
		;*	if(prevFrame==as->currentFrame) aSpritePushPos(as); else aSpritePush2(as);
		cmp.l	AS_CURRENTFRAME(a0), d0				;* frame changed?		18
		bne.s	0f						;*				8 not taken, 10 taken
		;*same frame //check if flipped
		btst.b	#B_AS_FLIPPED, AS_FLAGS+1(a0)			;*				16
		bne.s	0f						;*				8 not taken, 10 taken
		pea	aSpritePushPos_int				;*				20
		bra.s	9f						;*				10
0:		;*diff frame or flip bit set
		pea	aSpritePush_int					;*				20																		<=

9:		move.w	AS_TILEWIDTH(a0), d0				;*d0=old width			12
		move.l	AS_CURRENTFRAME(a0), a1				;*a1=as->currentFrame		12
		move.w	FRAME_TILEWIDTH(a1), d1				;*d1=as->currentFrame->tilewidth (new width)	12
		move.w	d1, AS_TILEWIDTH(a0)				;*as->tileWidth=as->currentFrame->tileWidth	12	

		sub.w	d1, d0						;*old-=new			4
		subq.w	#1, d0						;*d0=clear loop			4
		bmi.s	0f						;*loop<0? end			8 not taken, 10 taken
		;*if(as->currentFrame->tileWidth<as->tileWidth)	SC234Put(0x8200+as->baseSprite+as->currentFrame->tileWidth, 0x8800); //clear sprite

		;*d1=new width
		add.w	#0x8200, d1					;*addr+=width			8
		add.w	AS_BASESPRITE(a0), d1				;*+=basesprite			12
		swap	d1						;*				4
		move.w	#0x8800, d1					;* clear data			8
	
		movea.l	SC234ptr, a1					;*a1=SC234ptr			20
		move.l	d1, (a1)+					;*queue sprite clear		12
		dbra	d0, 1f						;*				10 taken, 14 not
		move.l	a1, SC234ptr					;*save ptr			20
		rts							;*returns to pushed address

1:		swap	d1						;*				4
		addq.w	#1, d1						;*				4
		swap	d1						;*inc vram addr			4
		move.l	d1, (a1)+					;*queue sprite clear		12
		dbra	d0, 1b						;*				10 taken, 14 not
		move.l	a1, SC234ptr					;*save ptr			20

0:		rts							;*returns to pushed address

;*	cases:
;*	- no updates
;*	- update pattern, positions
;*	- update positions

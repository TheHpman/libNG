	.include "srcNG/defines.inc"
	#include <configNG.h>
	.align	2

;* /******************************************************************************
;* 				pictureInit
;* ******************************************************************************/

;* void pictureInit(Picture *p, pictureInfo *pi, WORD sprite, BYTE palette, short posX, short posY, WORD flip)

.globl pictureInit
pictureInit:
	;* +a2 +a3 +d2 +d6
	.set	_ARGS, 20
	.set	_picture, _ARGS		;* long
	.set	_pictInfo, _ARGS+4	;* long
	.set	_baseSpr, _ARGS+8+2	;* word
	.set	_basePal, _ARGS+12+3	;* byte
	.set	_posX, _ARGS+16+2	;* word
	.set	_posY, _ARGS+20+2	;* word
	.set	_flip, _ARGS+24+2	;* word
		movem.l	d2/d6/a2-a3, -(sp)				;* push regs			
		move.l	_picture(sp), a0				;* a0=p				16
		move.l	_pictInfo(sp), a1				;* a1=pi			16
		move.l	a1, P_PICTUREINFO(a0) 				;* p->info=pi			16

		movea.l	SC1ptr, a2					;* a2=SC1ptr			20
		movea.l	SC234ptr, a3					;* a3=SC234ptr			20

	#if	BANKING_ENABLE
		moveq	#0, d0
		move.b	P_PICTUREINFO(a0), d0
		move.b	d0, REG_BANKING					;* bankswitch
		move.w	#BANKING_CMD, (a2)+				;* setup banking cmd
		move.w	d0, (a2)+
	#endif
	
		move.w	PI_TILEWIDTH(a1), d0				;* d0=tilewidth			12
		subq.w	#1, d0						;* width loop			4
		;* d0 has tilewidth loop (-1)
	
		move.w	_baseSpr(sp), d1				;* d1=basesprite		12
		move.w	d1, P_BASESPRITE(a0)				;* set p->basesprite12

		move.w	#0x8400, d6					;* X bank addr			8
		add.w	d1, d6						;*  add basesprite		4
		swap	d6						;*				4
		move.w	_posX(sp), d6					;* d6=posX			12
		move.w	d6, P_POSX(a0)					;* set p->posX			12
		lsl.w	#7, d6						;*				20
		move.l	d6, (a3)+					;* q. lead spr posX		12

		;*	y=((YSHIFT-posY)<<7)|p->info->tileHeight;
		sub.l	#0x2000000, d6					;*				16
		move.w	_posY(sp), d6					;* d2=posY			12
		move.w	d6, P_POSY(a0)					;* set p->posY			12	
		neg.w	d6						;*				4
		add.w	#YSHIFT, d6					;* YSHIFT			8
		lsl.w	#7, d6						;*  d6<<=7			20
		or.w	PI_TILEHEIGHT(a1), d6				;*  d6|=tileheight		12
		;* d6=addr/Ydata
	
		lsl.w	#6, d1						;* basesprite*=64		18
		swap	d1						;*				4

;*		;* d1 has base cmd
		move.b	_basePal(sp), d1				;* d1=basePalette		12
		move.b	d1, P_BASEPALETTE(a0)				;* set basepalette		12
		lsl.w	#7, d1						;*				18
		or.b	PI_TILEHEIGHT+1(a1), d1				;* height			12
		add.w	d1, d1						;* d1*=2			4
		swap	d1						;*				4
		;* d1 has base cmd

		move.w	_flip(sp), d2					;* d2=flip			12	-4
		move.w	d2, P_CURRENTFLIP(a0)				;* set currentFlip		12
	
		move.w	PI_COLSIZE(a1), a0				;* a0=strip bytesize12
	
		add.w	d2, d2						;*				4
		add.w	d2, d2						;* flip*=4			4
		move.l	PI_MAPS(d2.w, a1), a1				;* a1=col#0 addr		18

		;* write
		move.l	d1, (a2)+					;* queue SC1 cmd		12
		move.l	a1, (a2)+					;* queue SC1 addr		12
		move.l	d6, (a3)+					;* queue Y info			12
		dbra	d0, 0f						;*				10 taken
		move.l	a2, SC1ptr					;* save ptr			20
		move.l	a3, SC234ptr					;* save ptr			20
		movem.l	(sp)+, d2/d6/a2-a3				;* pop regs			44
		rts

0:		bset	#6, d6						;* add Y link			12
		moveq	#1, d2						;*				4
		swap	d2						;* d2=0x10000			4

1:		add.w	#64, d1						;* cmd next sprite		8
		add.l	a0, a1						;* map next column		8
		add.l	d2, d6						;* next Y spr			8	+4

		move.l	d1, (a2)+					;* queue SC1 cmd		12
		move.l	a1, (a2)+					;* queue SC1 addr		12
		move.l	d6, (a3)+					;* queue Y info			12
		dbra	d0, 1b						;*				10 taken
		move.l	a2, SC1ptr					;* save ptr			20
		move.l	a3, SC234ptr					;* save ptr			20
		movem.l	(sp)+, d2/d6/a2-a3				;* pop regs			44
		rts


;* /******************************************************************************
;* 				pictureSetFlip
;* ******************************************************************************/

;* void pictureSetFlip(picture *p, u16 flip);

.globl pictureSetFlip
pictureSetFlip:
	.set	_ARGS, 4
	.set	_picture, _ARGS		;* long
	.set	_flip, _ARGS+4+2	;* word
		move.l	_picture(sp), a0				;* a0=p				16
		move.w	_flip(sp), d1					;* d1=flip			12	-4
		cmp.w	P_CURRENTFLIP(a0), d1				;* changed?			12
		beq.s	1f						;* 				10 taken, 8 if not
		movem.l	d2-d3, -(sp)					;* save regs			24
		move.w	d1, P_CURRENTFLIP(a0)				;* update value			12	**
		;* d1.w has flip

	#if	BANKING_ENABLE
		move.b	P_PICTUREINFO(a0), REG_BANKING			;* bankswitch			24
	#endif
		move.l	P_PICTUREINFO(a0), a1				;* a1=p->info			16

		move.w	P_BASEPALETTE(a0), d0				;* palette			12
		move.b	PI_TILEHEIGHT+1(a1), d0				;* height			12
		add.b	d0, d0						;* height*=2			4

		swap	d0						;*				4
		move.w	P_BASESPRITE(a0), d0				;* spr addr			12
		lsl.w	#6, d0						;*				18
		;* d0.l has base cmd

		moveq	#0, d3						;*				4
		move.w	PI_COLSIZE(a1), d3				;* d3=				12
		;*add.w	d3, d3						;* colsize (bytes)		4

		add.w	d1, d1						;*				4
		add.w	d1, d1						;* flip*=4			4
		move.l	PI_MAPS(d1.w, a1), d2				;* d2=col#0 addr		18

		movea.l	SC1ptr, a0					;* load ptr			20

		move.w	PI_TILEWIDTH(a1), d1				;* d1=width			12
		subq.w	#1, d1						;* d1=tilewidth loop counter	4

		move.l	d0, (a0)+					;* queue cmd			12
		move.l	d2, (a0)+					;* queue addr			12
		dbra	d1, 2f						;* loop				10 if loop, 14 else
		move.l	a0, SC1ptr					;* save ptr			20
		move.l	(sp)+, d2					;*				12
		move.l	(sp)+, d3					;*				12
1:		rts							;*				16

2:		add.w	#64, d0						;* move to next sprite addr in base cmd		8
		add.l	d3, d2						;* next data column		8
		move.l	d0, (a0)+					;* queue cmd			12
		move.l	d2, (a0)+					;* queue addr			12
		dbra	d1, 2b						;* loop				10 if loop, 14 else
		move.l	a0, SC1ptr					;* save ptr			20
		move.l	(sp)+, d2					;*				12
		move.l	(sp)+, d3					;*				12
		rts							;*				16


;* /******************************************************************************
;* 				pictureSetPos
;* ******************************************************************************/

;* void pictureSetPos(Picture *p, short toX, short toY)

.globl pictureSetPos
pictureSetPos:
	.set	_ARGS, 4
	.set	_picture, _ARGS		;* long
	.set	_toX, _ARGS+4+2		;* word
	.set	_toY, _ARGS+8+2		;* word
		move.l	_picture(sp), a0				;* a0=p				16
		movea.l	SC234ptr, a1					;* a1=ptr			20

		move.w	_toX(sp), d1					;* d1=toX			12
		cmp.w	P_POSX(a0), d1					;* changed?			12
		beq.s	0f						;*				10 taken, 8 not
		move.w	d1, P_POSX(a0)					;* update value			12
		lsl.w	#7, d1						;* shift value			20
		move.w	#0x8400, d0					;* base addr			8
		add.w	P_BASESPRITE(a0), d0				;* add spr			12
		move.w	d0, (a1)+					;* write addr			8
		move.w	d1, (a1)+					;* write data			8

0:		move.w	_toY(sp), d1					;* d1=toY			12
		cmp.w	P_POSY(a0), d1					;* changed?			12
		beq.s	0f						;*				10 taken, 8 not
		move.w	d1, P_POSY(a0)					;* update value			12
		neg.w	d1						;*				4
		add.w	#YSHIFT, d1					;*				8
		lsl.w	#7, d1						;* <<=7				20
		move.w	#0x8200, d0					;* base addr			8
		add.w	P_BASESPRITE(a0), d0				;* add spr			12
	#if	BANKING_ENABLE
		move.b	P_PICTUREINFO(a0), REG_BANKING			;* bankswitch			24
	#endif
		move.l	P_PICTUREINFO(a0), a0				;* p->info			16
		or.w	PI_TILEHEIGHT(a0), d1				;* |=height			12
		move.w	d0, (a1)+					;* addr to queue		8
		move.w	d1, (a1)+					;* data to queue		8

0:		move.l	a1, SC234ptr					;* save ptr			20
		rts


;* /******************************************************************************
;* 				pictureMove
;* ******************************************************************************/

;* void pictureMove(Picture *p, short shiftX, short shiftY)

.globl pictureMove
pictureMove:
	.set	_ARGS, 4
	.set	_picture, _ARGS		;* long
	.set	_shiftX, _ARGS+4+2	;* word
	.set	_shiftY, _ARGS+8+2	;* word

		move.l	_picture(sp), a0					;* a0=p				16
		movea.l	SC234ptr, a1					;* a1=ptr			20

		move.w	_shiftX(sp), d1					;* d1=shiftX			12
		beq.s	0f						;* 0=no move			10 taken, 8 not
		move.w	#0x8400, d0					;* base addr			8
		add.w	P_BASESPRITE(a0), d0				;* add spr			12
		move.w	d0, (a1)+					;* write addr			8
		add.w	P_POSX(a0), d1					;* add shiftX			12
		move.w	d1, P_POSX(a0)					;* update value			12
		lsl.w	#7, d1						;* shift value			20
		move.w	d1, (a1)+					;* write data			8				

0:		move.w	_shiftY(sp), d1					;* d0=shiftY			12
		beq.s	0f						;* 0=no move			10 taken, 8 not
		move.w	#0x8200, d0					;* base addr			8
		add.w	P_BASESPRITE(a0), d0				;* add spr			12
		move.w	d0, (a1)+					;* write addr			8
		add.w	P_POSY(a0), d1					;* add posY			12
		move.w	d1, P_POSY(a0)					;* update value			12
		neg.w	d1						;*				4
		add.w	#YSHIFT, d1					;*				8
		lsl.w	#7, d1						;* <<=7				20
	#if	BANKING_ENABLE
		move.b	P_PICTUREINFO(a0), REG_BANKING			;* bankswitch			24
	#endif
		move.l	P_PICTUREINFO(a0), a0				;* p->info			16
		or.w	PI_TILEHEIGHT(a0), d1				;* |=height			12
		move.w	d1, (a1)+					;* write data			8

0:		move.l	a1, SC234ptr					;* save ptr			20
		rts


;* /******************************************************************************
;* 				pictureHide
;* ******************************************************************************/

;* void pictureHide(Picture *p)

.globl pictureHide
pictureHide:
	.set	_ARGS, 4
	.set	_picture, _ARGS		;* long

		move.l	_picture(sp), a0				;* a0=p				16
		movea.l	SC234ptr, a1					;* a1=ptr			20

		move.l	#0x88008200, d0					;* data, addr			12
		add.w	P_BASESPRITE(a0), d0				;* addr+=spr#			12
		swap	d0						;*				4	+4

		move.l	d0, (a1)+					;* queue addr/data		12
		move.l	a1, SC234ptr					;* save ptr			20
		rts


;* /******************************************************************************
;* 				pictureShow
;* ******************************************************************************/

;* void pictureShow(Picture *p)

.globl pictureShow
pictureShow:
	.set	_ARGS, 4
	.set	_picture, _ARGS		;* long
	
		move.l	_picture(sp), a0				;* a0=p				16
		movea.l	SC234ptr, a1					;* a1=ptr			20

		move.w	#0x8200, d0					;*				8
		add.w	P_BASESPRITE(a0), d0				;* d0=addr			12
		swap	d0 						;*				4

		move.w	P_POSY(a0), d0					;*				12
		neg.w	d0						;*				4
		add.w	#YSHIFT, d0					;*				8
		lsl.w	#7, d0						;*				20
	#if	BANKING_ENABLE
		move.b	P_PICTUREINFO(a0), REG_BANKING			;* bankswitch			24
	#endif
		move.l	P_PICTUREINFO(a0), a0				;*				16
		or.w	PI_TILEHEIGHT(a0), d0				;* d0=addr/posY			12

		move.l	d0, (a1)+					;* queue addr/data		12
		move.l	a1, SC234ptr					;* save ptr			20
		rts

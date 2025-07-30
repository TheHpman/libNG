	.include "srcNG/defines.inc"
	#include <configNG.h>
	.align	2

;* === good luck bro ===


;* IMPORTS / EXPORTS
.globl scrollerInit
.globl scrollerSetPos

;*	//** scrollers
;*typedef struct scrollerInfo {
;*	u16		stripSize;		//column size (bytes)
;*	u16		sprHeight;		//sprite tile height
;*	u16		mapWidth;		//map tile width
;*	u16		mapHeight;		//map tile height
;*	paletteInfo	*palInfo;
;*	colorStreamInfo	*csInfo;
;*	u16		*strips[0];		//ptr to strips data
;*} scrollerInfo;

SCI_STRIPSIZE		=	0
SCI_SPRHEIGHT		=	2
SCI_MAPWIDTH		=	4
SCI_MAPHEIGHT		=	6
SCI_PALINFO		=	8
SCI_CSINFO		=	12
SCI_STRIPS		=	16

;*typedef struct scroller {
;*	u16		baseSprite;
;*	u16		basePalette;
;*	s16		scrlPosX, scrlPosY;
;*	scrollerInfo 	*info;
;*	u16		config[32];
;*} scroller;	//76 bytes

SC_BASESPRITE		=	0
SC_BASEPALETTE		=	2
SC_POSX			=	4
SC_POSY			=	6
SC_INFO			=	8
SC_COLINDEX		=	12
SC_RANGEMIN		=	54
SC_RANGEMAX		=	56
SC_CONFIG		=	58

CFG_DATAINDEX2		=	SC_CONFIG + 0
CFG_DATAINDEX		=	SC_CONFIG + 2			;* data tile index
CFG_DATALENGTH2		=	SC_CONFIG + 4			;* same as TILEINDEX
CFG_TILEINDEX		=	SC_CONFIG + 4			;*index of top tile (0-31)
CFG_DATALENGTH		=	SC_CONFIG + 6			;* section length
;*$CFG_TILEINDEX2	=	8

CFG_REFILL_TILEINDEX	=	SC_CONFIG + 8
CFG_REFILL_DATAINDEX	=	SC_CONFIG + 10
CFG_REFILL_DATALENGTH	=	SC_CONFIG + 12
;*$CFG_REFILL_TILEINDEX2	=	16
CFG_REFILL_DATAINDEX2	=	SC_CONFIG + 14
CFG_REFILL_DATALENGTH2	=	SC_CONFIG + 16


;*/******************************************************************************
;*				scrollerInit
;*******************************************************************************/

;*	void scrollerInit (scroller *s, scrollerInfo *si, ushort sprite, ushort palette, short posX, short posY) {
scrollerInit:
	.set	_ARGS, 4+44
	.set	_scrlr, _ARGS		;* long
	.set	_scrlInfo, _ARGS+4	;* long
	.set	_baseSpr, _ARGS+8+2	;* word
	.set	_basePal, _ARGS+12+2	;* word
	.set	_posX, _ARGS+16+2	;* word
	.set	_posY, _ARGS+20+2	;* word

		movem.l	d2-d7/a2-a6, -(sp)				;* push
	
		move.l	SC1ptr, a4					;* a4=SC1ptr
		move.l	SC234ptr, a5					;*a5=SC234ptr			20
		move.l	_scrlr(sp), a0					;* a0=scroller

	#if	BANKING_ENABLE
		move.b	_scrlInfo(sp), REG_BANKING			;* bankswitch			24
	#endif

		move.l	_scrlInfo(sp), a1				;* a1=scrlInfo
		move.l	a1, SC_INFO(a0)					;* set scrollerInfo

		move.w	_baseSpr(sp), d2
		move.w	d2, SC_BASESPRITE(a0)				;* set baseSprite (d2)

		move.w	_posY(sp), d0					;*d0=posY

		moveq	#1, d4
		swap	d4						;* d4=spr increment
		move.w	#0x8200, d3
		add.w	d2, d3
		swap	d3						;* d3=sprAddr|xxxx

		move.w	d0, d3
		add.w	#YSHIFT, d3					;* set spr pos to -posY
		lsl.w	#7, d3
		or.w	SCI_SPRHEIGHT(a1), d3				;* d3=sprAddr|posY,tileSize

	.rept	20
		move.l	d3, (a5)+					;* write sprites posY		12
		add.l	d4, d3						;* next spr			8
	.endr
		move.l	d3, (a5)+					;* write last sprite posY	12

		;* setup Y & strip config
		move.w	d0, SC_POSY(a0)					;*write posY
		bpl.s	0f
		moveq	#0, d0						;* map index 0 if posY <0
		;* d0 has Y pos
0:		move.w	SCI_MAPHEIGHT(a1), d5
		cmp.w	#32, d5						;* map size<=32 ?
		bls.s	_shortStrip
		lsr.w	#4, d0						;*d0=Ypos>>4
		move.w	d0, d1

		;*set index & min range for >32 tiles scroller	(common code for _slitted & _nosplit)
		move.w	d0, CFG_DATAINDEX(a0)				;* top data index
		move.w	d0, SC_RANGEMIN(a0)				;* min range
		add.w	#18, d0						;* 17??
		move.w	d0, SC_RANGEMAX(a0)				;* max range

		and.w	#0x1f, d1					;* d1=splitSize
		beq.s	_noSplit

_splitted:	;* splitted strip
		move.w	d1, CFG_TILEINDEX(a0)				;* top tile index
		add.w	#14, d0
		and.w	#0xffe0, d0
		move.w	d0, CFG_DATAINDEX2(a0)				;* bottom data index
		sub.w	#32, d1
		neg.w	d1
		move.w	d1, CFG_DATALENGTH(a0)				;* top data length
		bra.s	9f

_noSplit:	;* straight strip, 32 tiles
		move.w	#0, CFG_TILEINDEX(a0)
		moveq	#32, d0
		move.w	d0, CFG_DATALENGTH(a0)
		add.w	CFG_DATAINDEX(a0), d0
		move.w	d0, CFG_DATAINDEX2(a0)
		bra.s	9f

_shortStrip:	;* strip is < 32 tiles, no refills needed
		move.w	d5, CFG_DATALENGTH(a0)				;*  12
		moveq	#0, d0
		move.w	d0, CFG_TILEINDEX(a0)
		move.w	d0, CFG_DATAINDEX(a0)
		move.w	d0, SC_RANGEMIN(a0)
		move.w	#-1, SC_RANGEMAX(a0)

	;* d0 d1 d3 d6 d7
	;* d2= baseSprite
	;* d5= strip length
	;* a4=SC1
	;* a5=SC234
	
	;*	needed: posX, spr# SC2_data SC1_data SC1_addr
	;*
	;*	d0 d1 d2 d3 d4 d5 d6 d7
	;*	a0:SC a1:SCI a2:colIndex a3
	
9:		move.l	_ARGS+16(sp), d1				;* d1=posX
		move.w	d1, SC_POSX(a0)					;* save posX

		;* lead sprite posX
		move.w	d1, d3
		and.w	#0xf, d3
		neg.w	d3						;* lead spr pos
		lsl.w	#7, d3
		swap	d3						;* d3=posX|xxxx

		lsr.w	#4, d1						;*posX/=4		(strip index)
		move.l	d1, d6
		divu.w	#21, d6						;*
		swap	d6						;* d6 has remainder (front sprite index)
	
		lea	SC_COLINDEX(a0), a2				;* a2=colIndex ptr
		add.w	d6, a2
		add.w	d6, a2						;* set colIndex[frontSpr] ptr

		move.w	_basePal(sp), d4
		move.w	d4, SC_BASEPALETTE(a0)				;* set basePalette
		move.b	d4, -(sp)
		move.w	(sp)+, d4
	
_fillStripA:
		moveq	#20, d7
		move.b	CFG_DATALENGTH+1(a0), d4
		add.b	d4, d4
		swap	d4						;* d4=SC1 pal,size*4|xxxx
		move.w	d2, d4						;* baseSpr
		add.w	d6, d4						;* + current index
		move.w	d4, d3						;* (posX spr set)
		lsl.w	#5, d4						;* *64 => base spr addr (*32 *2)
		add.w	CFG_TILEINDEX(a0), d4				;* add index *2
		add.w	d4, d4						;* d4=SC1 pal,size*4|addr

		add.w	#0x8400, d3
		swap	d3						;* d3=addr|posX

		move.l	CFG_DATAINDEX2(a0), d5				;* d5=dataIndex2|dataIndex1
		lsl.l	#2, d5						;* d5=data displacement

		add.w	d1, d1
		add.w	d1, d1						;* strip index *=4
		lea	SCI_STRIPS(a1,d1.w), a6				;* a6=ptr strip addr

		move.l	a6, a1						;* save base strip addr
	
		lsr.w	#2, d1						;* back to strip index

0:		;* posX set
		move.l	d3, (a5)+
		add.w	#16<<7, d3
	
		;* SC1 pal, size & addr set
		move.l	d4, (a4)+

		;* SC1 data ptr set
		move.l	(a6)+, d0
		and.w	#~1, d0						;* overscroll even addr fix
		move.l	d0, a3
		add.w	d5, a3						;* add displacement
		move.l	a3, (a4)+					;* write ptr
	
		;* set colNum[index]
		move.w	d1, (a2)+
		addq.w	#1, d1

		;* == to next strip ==
		cmp.w	#20, d6
		beq.s	1f
		;* next spr
		addq.w	#1, d6						;* update current index
		add.w	#64, d4						;* next spr addr
		swap	d3
		addq.w	#1, d3
		swap	d3						;* next spr posX	4+4+4
		dbra	d7, 0b
		bra.s	_fillStripB
		;* back to spr #0
1:		moveq	#0, d6						;* update current index
		sub.w	#64*20, d4					;* spr 0 addr
		sub.l	#0x10000*20, d3					;* spr 0 posX			16
		lea	-42(a2), a2
		dbra	d7, 0b

;*-------------------------------------------
_fillStripB:
	;*a1 has base strip add
		swap	d4
		move.b	CFG_DATALENGTH2+1(a0), d4
		beq.s	9f						;* no second section when lenght2=0
		add.b	d4, d4
		swap	d4						;* d4=SC1 pal,size*4|xxxx
		and.w	#0xffc0, d4					;* 2nd index is always 0
	
		swap	d5						;* swap to data index 2
	;*	add.w	d5, a1						;* a1=strip addr + displacement
		move.l	a1, a6

		sub.w	#21*4, d1					;* back to leading sprite

	;* (freed d3)
		moveq	#20, d7
		;* SC1 pal, size & addr set
0:		move.l	d4, (a4)+

		;* SC1 data ptr set
		move.l	(a6)+, d0
		and.w	#~1, d0						;* overscroll even addr fix
		move.l	d0, a3
		add.w	d5, a3						;* add displacement
		move.l	a3, (a4)+					;* write ptr
	
		;* == to next strip ==
		cmp.w	#20, d6
		beq.s	1f
		;* next spr
		addq.w	#1, d6						;* update current index
		add.w	#64, d4						;* next spr addr
		dbra	d7, 0b
		bra.s	9f
		;* back to spr #0
1:		moveq	#0, d6						;* update current index
		sub.w	#64*20, d4					;* spr 0 addr
		dbra	d7, 0b

9:		move.l	a5, SC234ptr					;* save SC234ptr
		move.l	a4, SC1ptr					;* save SC1ptr
		movem.l (sp)+, d2-d7/a2-a6				;* pull
9:		rts


;*/******************************************************************************
;*				scrollerSetPos
;*******************************************************************************/

;*	void scrollerSetPos (scroller *s, short posX, short posY)

scrollerSetPos:
	.set	_ARGS, 4
	.set	_scrlr, _ARGS		;* long
	.set	_posX, _ARGS+4+2	;* word
	.set	_posY, _ARGS+8+2	;* word
		move.l	_scrlr(sp), a0					;* a0 = scroller handler
		move.l	_posX(sp), d0					;* .l read to place in upper word
		move.w	_posY(sp), d0					;* d0=targetX|targetY
		cmp.l	SC_POSX(a0), d0					;* X or Y changed?
		beq.s	9b						;* return if unchanged

		;* X or Y did change
		movem.l	d2-d7/a2-a5, -(sp)				;* push				80

		;* some preparations
	#if	BANKING_ENABLE
		move.b	SC_INFO(a0), REG_BANKING			;* bankswitch			24
	#endif
	
		move.l	SC_INFO(a0), a1					;* a1= scrollInfo
		move.l	SC234ptr, a5					;* a5=SC234ptr			20
		move.w	SC_BASESPRITE(a0), d2				;* d2=baseSprite

		;* handle sprites posX & posY
		;* posY setup
		moveq	#1, d4
		swap	d4						;* d4=spr increment
		move.w	#0x8200, d3
		add.w	d2, d3						;* d3=0x8200+baseSprite
		swap	d3						;* d3=sprAddr|xxxx
		move.w	d0, d3
		add.w	#YSHIFT, d3					;* set spr pos to -posY
		lsl.w	#7, d3
		or.w	SCI_SPRHEIGHT(a1), d3				;* d3=sprAddr|posY,tileSize

		;* posX setup
		swap	d0						;* posX

		move.w	#0x8400, d5
		add.w	d2, d5
		moveq	#0, d6
		move.w	d0, d6
		lsr.w	#4, d6
		divu.w	#21, d6
		swap	d6						;* d6=lead spr index (remainder)
		add.w	d6, d5
		swap	d5
	
		move.w	d0, d5
		and.w	#0xf, d5
		neg.w	d5						;* lead spr pos
		lsl.w	#7, d5						;* d5=addr|posX
	
		move.w	d6, d7
		add.w	d6, d6
		add.w	d6, d6						;* lead spr index*4
		lea	_sprIncrementTable(pc), a2
		add.w	d6, a2

	.rept	20
		move.l	d3, (a5)+					;* write sprites posY		12
		add.l	d4, d3						;* next spr			8
		move.l	d5, (a5)+					;* write sprites posX		12
		add.l	(a2)+, d5					;* next spr
		add.w	#16<<7, d5					;* +16 px
	.endr
		move.l	d3, (a5)+					;* write last sprite posY	12
		move.l	d5, (a5)+					;* write sprites posX		12

		move.l	a5, SC234ptr					;* save SC234ptr
		move.l	SC1ptr, a5					;* a5=SC1ptr

		swap	d0  						;* posY

_checkRefills:
		move.w	d0, SC_POSY(a0)					;* update posY
		bpl.s	0f
		clr.w	d0						;* map index 0 if y<0
0:		lsr.w	#4, d0						;* d0= top data index ======

		cmp.w	SC_RANGEMIN(a0), d0
		blo.s	_refillUp					;* unsigned compares
		cmp.w	SC_RANGEMAX(a0), d0
		bhs.w	_refillDown					;* bhs?
	;* d0= top data index
	;* d1= refill size

	;* staying in same range ******************
_noRefill:
		moveq	#0, d1
		move.w	d1, CFG_REFILL_DATALENGTH(a0)
		move.w	d1, CFG_REFILL_DATALENGTH2(a0)
		bra.w	_refillConfigDone

	;* upward partial refill ******************
_refillUp:
		move.w	SC_RANGEMIN(a0), d1
		sub.w	d0, d1						;* d1 = refill size
		cmp.w	#31, d1
		bpl.w	_fullRefill
		sub.w	d1, SC_RANGEMIN(a0)
		sub.w	d1, SC_RANGEMAX(a0)
	
		move.w	CFG_TILEINDEX(a0), d2
		sub.w	d1, d2
		bmi.s	0f	;* ****adjust value?
		move.w	#0, CFG_REFILL_DATALENGTH2(a0)
		move.w	d2, CFG_REFILL_TILEINDEX(a0)
		move.w	d0, CFG_REFILL_DATAINDEX(a0)
		move.w	d1, CFG_REFILL_DATALENGTH(a0)
	
		;* update raw config
		sub.w	d1, CFG_DATAINDEX(a0)
		add.w	d1, CFG_DATALENGTH(a0)
		sub.w	d1, CFG_TILEINDEX(a0)
		bra.w	_refillConfigDone
	
0:		;*split section
		move.w	d0, CFG_DATAINDEX(a0)
		move.w	d0, CFG_REFILL_DATAINDEX(a0)
		moveq	#32, d3
		add.w	d2, d3
		move.w	d3, CFG_TILEINDEX(a0)				;* also is CFG_DATALENGTH2
		move.w	d3, CFG_REFILL_TILEINDEX(a0)
		neg.w	d2
		move.w	d2, CFG_DATALENGTH(a0)
		move.w	d2, CFG_REFILL_DATALENGTH(a0)
		sub.w	d2, d1
		move.w	d1, CFG_REFILL_DATALENGTH2(a0)
		add.w	d0, d2
		move.w	d2, CFG_DATAINDEX2(a0)
		move.w	d2, CFG_REFILL_DATAINDEX2(a0)
		bra.w	_refillConfigDone


	;* downward partial refill ******************
_refillDown:
		move.w	SC_RANGEMAX(a0), d1
		sub.w	d0, d1
		neg.w	d1
		addq.w	#1, d1						;* d1= refill size
		cmp.w	#31, d1
		bpl.w	_fullRefill
		add.w	d1, SC_RANGEMIN(a0)
		add.w	d1, SC_RANGEMAX(a0)

		move.w	CFG_TILEINDEX(a0), d2
		add.w	d1, d2						;* bottom section size
		cmp.w	#31, d2
		bhi.s	0f
		move.w	#0, CFG_REFILL_DATALENGTH2(a0)
		move.w	CFG_TILEINDEX(a0), CFG_REFILL_TILEINDEX(a0)
		move.w	d1, CFG_REFILL_DATALENGTH(a0)
	
		move.w	CFG_DATAINDEX(a0), d0
		add.w	#32, d0
		move.w	d0, CFG_REFILL_DATAINDEX(a0)

		;* update raw config
		add.w	d1, CFG_DATAINDEX(a0)
		sub.w	d1, CFG_DATALENGTH(a0)
		add.w	d1, CFG_TILEINDEX(a0)
		bra.w	_refillConfigDone

0:		;* new lower section	- d2 has tileIndex + refill
		move.w	CFG_TILEINDEX(a0), CFG_REFILL_TILEINDEX(a0)
		and.w	#0x1f, d2
		move.w	d2, CFG_TILEINDEX(a0)

		move.w	CFG_DATAINDEX(a0), d2
		move.w	d2, d3
		add.w	#32, d3
		move.w	d3, CFG_REFILL_DATAINDEX(a0)
		add.w	d1, d2
		move.w	d2, CFG_DATAINDEX(a0)

		move.w	CFG_DATALENGTH(a0), d3
		move.w	d3, CFG_REFILL_DATALENGTH(a0)
		move.w	d3, d4
		sub.w	d1, d3
		add.w	#32, d3
		move.w	d3, CFG_DATALENGTH(a0)
		add.w	d2, d3
		move.w	d3, CFG_DATAINDEX2(a0)
		move.w	d3, CFG_REFILL_DATAINDEX2(a0)

		sub.w	d4, d1						;* breaks refill info
		move.w	d1, CFG_REFILL_DATALENGTH2(a0)
		bra.s	_refillConfigDone

		;* complete refill ******************
_fullRefill:
		move.w	d0, d1
		;*set index & min range for >32 tiles scroller	(common code for _slitted & _nosplit)
		move.w	d0, CFG_DATAINDEX(a0)				;* top data index
		move.w	d0, CFG_REFILL_DATAINDEX(a0)
		move.w	d0, SC_RANGEMIN(a0)				;* min range
		add.w	#18, d0						;* 17??
		move.w	d0, SC_RANGEMAX(a0)				;* max range
		and.w	#0x1f, d1					;* d1=splitSize

_fullRefill_splitted:		;* splitted strip
		move.w	d1, CFG_TILEINDEX(a0)				;* top tile index
		move.w	d1, CFG_REFILL_TILEINDEX(a0)
		move.w	d1, CFG_REFILL_DATALENGTH2(a0)
		add.w	#14, d0
		and.w	#0xffe0, d0
		move.w	d0, CFG_DATAINDEX2(a0)				;* bottom data index
		move.w	d0, CFG_REFILL_DATAINDEX2(a0)
		sub.w	#32, d1
		neg.w	d1
		move.w	d1, CFG_DATALENGTH(a0)				;* top data length
		move.w	d1, CFG_REFILL_DATALENGTH(a0)

;* d0	posx			strip index
;* d1	-			scratch?
;* d2	-
;* d3	-
;* d4	-			lead index
;* d5	-			spr pal/size|addr (work reg)
;* d6	lead idx*4		spr pal/size|addr (base)
;* d7	lead spr index	loop

;* a0	sc_handler
;* a1	SC_info
;* a2	colIndex[] ptr
;* a3	map ptrs
;* a4				strip_addr
;* a5	SC1ptr

_refillConfigDone:
;* setup: base strip addr base vram addr
		swap	d0						;* posX
		move.w	d0, SC_POSX(a0)					;* update posX & posY
		bpl.s	0f
		moveq	#0, d0
0:		lsr.w	#4, d0						;* d0= leftmost strip index

		move.l	SC_BASESPRITE(a0), d6				;* d6=spr|pal
		lsl.w	#2, d6
		swap	d6
		add.w	d7, d6						;* d6=basespr+ lead index
		lsl.l	#6, d6			
		swap	d6						;* d6=lead spr addr|pal/xx

		lea	SC_COLINDEX(a0), a2
		add.w	d7, a2
		add.w	d7, a2						;* a2=colIndex[leadSpr]

		move.w	d0, d4
		add.w	d4, d4
		add.w	d4, d4
		lea	SCI_STRIPS(a1,d4.w), a1
		move.l	(a1)+, d1
		and.w	#~1, d1						;* safety for odd addr on overscroll
		move.l	d1, a4						;* a4= front strip data addr

		move.w	d7, d4						;* d4=lead index
		move.w	#20, d7
_stripLoop:
		cmp.w	(a2), d0
		bne.s	_loadNewStrip
		;* same colIndex, partial refill if needed
_updateStrip:
		addq.w	#2, a2
		;*section1
		move.l	d6, d5
		move.b	CFG_REFILL_DATALENGTH+1(a0), d5
		beq.w	_nextStrip
		add.b	d5, d5						;* add datalen*2
		swap	d5
		add.w	CFG_REFILL_TILEINDEX(a0), d5
		add.w	CFG_REFILL_TILEINDEX(a0), d5			;********

		move.l	a4, a3						;* base strip addr
		move.w	CFG_REFILL_DATAINDEX(a0), d1
		add.w	d1, d1
		add.w	d1, d1						;* index*=4
		add.w	d1, a3						;* a3=data ptr

		move.l	d5, (a5)+
		move.l	a3, (a5)+
		;*section 2
		move.l	d6, d5
		move.b	CFG_REFILL_DATALENGTH2+1(a0), d5
		beq.s	_nextStrip
		add.b	d5, d5						;* add datalen*2
		swap	d5						;* INDEX2 always 0

		move.l	a4, a3						;* base strip addr
		move.w	CFG_REFILL_DATAINDEX2(a0), d1
		add.w	d1, d1
		add.w	d1, d1						;* index*=4
		add.w	d1, a3						;* a3=data ptr

		move.l	d5, (a5)+
		move.l	a3, (a5)+
		bra.s	_nextStrip

		;* colIndex was different, full reload
_loadNewStrip:
		move.w	d0, (a2)+					;* update colIndex
		;* section 1
		move.l	d6, d5
		move.b	CFG_DATALENGTH+1(a0), d5
		add.b	d5, d5						;* add datalen*2
		swap	d5
		add.w	CFG_TILEINDEX(a0), d5
		add.w	CFG_TILEINDEX(a0), d5				;*********

		move.l	a4, a3						;* base strip addr
		move.w	CFG_DATAINDEX(a0), d1
		add.w	d1, d1
		add.w	d1, d1						;* index*=4
		add.w	d1, a3						;* a3=data ptr

		move.l	d5, (a5)+
		move.l	a3, (a5)+

		;* section 2	
		move.l	d6, d5
		move.b	CFG_DATALENGTH2+1(a0), d5
		beq.s	_nextStrip
		add.b	d5, d5						;* add datalen*2
		swap	d5						;* INDEX2 always 0

		move.l	a4, a3						;* base strip addr
		move.w	CFG_DATAINDEX2(a0), d1
		add.w	d1, d1
		add.w	d1, d1						;* index*=4
		add.w	d1, a3						;* a3=data ptr

		move.l	d5, (a5)+
		move.l	a3, (a5)+

_nextStrip:
		move.l	(a1)+, d1					;* read to next strip ptr
		and.w	#~1, d1						;* overscroll odd addr protect
		move.l	d1, a4

		addq.w	#1, d0						;* next data strip index

		cmp.w	#20, d4
		beq.s	1f
		;* next sprite
		addq.w	#1, d4
		add.l	#0x400000, d6
		dbra	d7, _stripLoop
		bra.s	9f
		;* return to sprite #0
1:		moveq	#0, d4						;* back to spr index 0
		sub.l	#0x5000000, d6					;* 64*20
		lea	SC_COLINDEX(a0), a2				;* back to index 0
		dbra	d7, _stripLoop

9:		move.l	a5, SC1ptr					;* save SC1ptr
		movem.l (sp)+, d2-d7/a2-a5				;* pull
		rts


_sprIncrementTable:
	.long	0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000
	.long	0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000
	.long	0xffec0000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000
	.long	0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000, 0x10000
	.long	0x10000 ;* ?

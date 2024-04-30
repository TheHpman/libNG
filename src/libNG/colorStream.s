	.include "src/libNG/defines.inc"
	#include <configNG.h>
	.align	2

;*******************************************************************************
;*				colorStreamInit
;*******************************************************************************
;*void colorStreamInit(colorStream *cs, colorStreamInfo *csi, u16 basePalette, u16 config) {

.globl colorStreamInit
colorStreamInit:
	.set	_ARGS, 4
	.set	_cs, _ARGS		;* long
	.set	_csInfo, _ARGS+4	;* long
	.set	_basePal, _ARGS+8+2	;* word
	.set	_config, _ARGS+12+2	;* word

		move.l	_cs(sp), a0					;* a0=cs			16
		move.l	_csInfo(sp), a1					;* a1=csi			16
	#if	BANKING_ENABLE
		move.b	_csInfo(sp), REG_BANKING			;* bankswitch			24
	#endif
		move.l	a1, CS_INFO(a0)					;* cs->info=csi

		move.w	_basePal(sp), d0				;* d0=basePalette
		lsl.w	#5, d0						;* d0<<=5
		move.w	d0, CS_PALMOD(a0)				;* cs->palMod=basePalette<<5

		tst.w	_config(sp)					;* config==0?
		beq.s	0f						;* ==0?
		;* ENDCONFIG
		move.w	#0xffff, CS_POSITION(a0)			;* cs->position=0xffff
		move.l	CSI_FWDATAEND(a1), CS_FWJOB(a0)			;* cs->fwJob=csi->fwDataEnd
		move.l	CSI_BWDATA(a1), CS_BWJOB(a0)			;* cs->bwJob=csi->bwData
		move.l	CSI_ENDCONFIG(a1), a0				;* a0=endConfig
		bra.s	5f
0:		;* STARTCONFIG
		move.w	#0, CS_POSITION(a0)				;* cs->position=0
		move.l	CSI_FWDATA(a1), CS_FWJOB(a0)			;* cs->fwJob=csi->fwData
		move.l	CSI_BWDATAEND(a1), CS_BWJOB(a0)			;* cs->bwJob=csi->bwDataEnd
		move.l	CSI_STARTCONFIG(a1), a0				;* a0=startConfig

5:		;* write config (in a0)
		movea.l	palJobsPtr, a1					;* a1=palJobsPtr
		moveq	#0, d1						;* clear d1

6:		move.w	(a0)+, d1					;* d1=slot
		bmi.s	9f						;* 0xffff? end
		add.w	d0, d1						;* add palmod
		move.l	d1, (a1)+					;* write palJob palette info
		move.l	(a0)+, (a1)+					;* write palJob palette address
		bra.s	6b

9:		move.l	a1, palJobsPtr					;* save palJobsPtr
		rts


;*******************************************************************************
;*				colorStreamSetPos
;*******************************************************************************	
;*void colorStreamSetPos(colorStream *cs, u16 pos) {

.globl colorStreamSetPos
colorStreamSetPos:
	.set	_ARGS, 4
	.set	_cs, _ARGS	;* long
	.set	_pos, _ARGS+4+2	;* word

		move.l	_cs(sp), a0					;* a0=cs			16
		move.w	_pos(sp), d0					;* d0=pos			
	#if	BANKING_ENABLE
		move.b	CS_INFO(a0), REG_BANKING			;* bankswitch			24
	#endif
		cmp.w	CS_POSITION(a0), d0				;* d0-position
		beq.w	9f
		bhi.s	5f						;* d0>position

		;*going backward
		move.l	CS_BWJOB(a0), a1				;* a1=cs->bwJob
		cmp.w	CSJOB_COORD(a1), d0				;* while(pos<cs->bwJob->coord)
		bhs.w	8f

		;*process bw data
		movem.l	d2/a2-a3, -(sp)					;* push regs			32
		moveq	#0, d2
		movea.l	palJobsPtr, a3					;* a3=palJobsPtr
		move.w	CS_PALMOD(a0), d1				;* d1=palmod

0:		move.l	CSJOB_DATA(a1), a2				;* a2=data	data=cs->bwJob->data;
1:		move.w	(a2)+, d2					;* d2=slot
		bmi.s	2f						;* 0xffff? end
		add.w	d1, d2						;* add palMod
		move.l	d2, (a3)+					;* write to palJobs
		move.l	(a2)+, (a3)+					;* write data add in palJobs
		bra.s	1b

2:		addq.w	#SIZEOF_CSJOB, a1				;* cs->fwJob++	next job
		cmp.w	CSJOB_COORD(a1), d0				;* while(pos<cs->bwJob->coord)
		blo.s	0b						;* loop
		move.l	a1, CS_BWJOB(a0)				;* save cs->bwJob++

		;*adj fw data
		move.l	CS_FWJOB(a0), a1				;* a1=cs->fwJob
0:		cmp.w	CSJOB_COORD(a1), d0				;* while(pos<cs->fwJob->coord)	
		bhs.s	1f						;* end on (cs->fwJob->coord<=pos)
		subq.w	#SIZEOF_CSJOB, a1				;* cs->fwJob--
		bra.s	0b
1:		addq.w	#SIZEOF_CSJOB, a1				;* cs->fwJob++
		move.l	a1, CS_FWJOB(a0)				;* store value
		bra.s	7f

		;*********************************
5:		;*going forward
		move.l	CS_FWJOB(a0), a1				;* a1=cs->fwJob
		cmp.w	CSJOB_COORD(a1), d0				;* while(pos>=cs->fwJob->coord)
		blo.s	8f						;* new pos < csjob pos? exit

		;*process fw data
		movem.l	d2/a2-a3, -(sp)					;* push regs
		moveq	#0, d2
		movea.l	palJobsPtr, a3					;* a3=palJobsPtr
		move.w	CS_PALMOD(a0), d1				;* d1=palmod

0:		move.l	CSJOB_DATA(a1), a2				;* a2=data	data=cs->fwJob->data;
1:		move.w	(a2)+, d2					;* d2=pal slot
		bmi.s	2f						;* 0xffff? job end
		add.w	d1, d2						;* add palMod
		move.l	d2, (a3)+					;* write to palJobs
		move.l	(a2)+, (a3)+					;* write data add in palJobs
		bra.s	1b

2:		addq.w	#SIZEOF_CSJOB, a1				;* cs->fwJob++	next job
		cmp.w	CSJOB_COORD(a1), d0				;* while(pos>=cs->fwJob->coord)
		bhs.s	0b						;* loop
		move.l	a1, CS_FWJOB(a0)				;* save cs->fwJob++

		;*adjust bw data
		move.l	CS_BWJOB(a0), a1				;* a1=cs->bwJob
0:		cmp.w	CSJOB_COORD(a1), d0				;*
		bls.s	1f						;* while(pos>cs->bwJob->coord)	exit on (cs->bwJob->coord>=pos)
		subq.w	#SIZEOF_CSJOB, a1				;* cs->bwJob--
		bra.s	0b
1:		addq.w	#SIZEOF_CSJOB, a1				;* cs->bwJob++
		move.l	a1, CS_BWJOB(a0)				;* store value


7:		move.l	a3, palJobsPtr					;* save ptr
		movem.l	(sp)+, d2/a2-a3					;* pop regs
8:		move.w	d0, CS_POSITION(a0)				;* save new pos
9:		rts

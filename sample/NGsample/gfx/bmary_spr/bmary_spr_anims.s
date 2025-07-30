bmary_spr_animations:
	.long	bmary_spr_anim_IDLE_steps	;* steplist
	.long	bmary_spr_anim_WALK_steps	;* steplist
	.long	bmary_spr_anim_TEST_steps	;* steplist
	.long	bmary_spr_anim_TEST2_steps	;* steplist
	.long	bmary_spr_anim_PUNCH_steps	;* steplist
	.long	bmary_spr_anim_NEWFMT_steps	;* steplist

bmary_spr_anim_IDLE_steps:
	.long	bmary_spr_0000	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0001	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0002	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0003	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0004	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0005	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0006	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0007	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0008	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0009	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_000a	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_000b	;* frame ptr
	.word	0xffda, 0xffe7, 0xfffd, 0xff94, 0x0006	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_LINK 0x0000 bmary_spr_anim_IDLE_steps

bmary_spr_anim_WALK_steps:
	.long	bmary_spr_000c	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_000d	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_000e	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_000f	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0010	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0011	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0012	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0013	;* frame ptr
	.word	0xffe1, 0xffe0, 0x0000, 0xff91, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_LINK 0x0001 bmary_spr_anim_WALK_steps

bmary_spr_anim_TEST_steps:
	.long	bmary_spr_0000	;* frame ptr
	.word	0xffc1, 0x0000, 0xff91, 0x0000, 0x003c	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0001	;* frame ptr
	.word	0xffc1, 0x0000, 0xff91, 0x0000, 0x003c	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0002	;* frame ptr
	.word	0xffc1, 0x0000, 0xff91, 0x0000, 0x003c	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_REPEAT 0x0002
	_ANM_LINK 0x0003 bmary_spr_anim_TEST2_steps

bmary_spr_anim_TEST2_steps:
	.long	bmary_spr_0011	;* frame ptr
	.word	0xffc9, 0xfff8, 0xff99, 0xfff8, 0x0014	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0012	;* frame ptr
	.word	0xffc9, 0xfff8, 0xff99, 0xfff8, 0x0014	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0013	;* frame ptr
	.word	0xffc9, 0xfff8, 0xff99, 0xfff8, 0x0014	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_REPEAT 0x0003
	_ANM_LINK 0x0002 bmary_spr_anim_TEST_steps

bmary_spr_anim_PUNCH_steps:
	.long	bmary_spr_0014	;* frame ptr
	.word	0xffe6, 0xffdb, 0xfffd, 0xff94, 0x0004	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0015	;* frame ptr
	.word	0xffe2, 0xffdf, 0xfffd, 0xff94, 0x0004	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0016	;* frame ptr
	.word	0xffcd, 0xffe4, 0xfffa, 0xff97, 0x0004	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0017	;* frame ptr
	.word	0xffcc, 0xffe5, 0xfffb, 0xff96, 0x0004	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0015	;* frame ptr
	.word	0xffe2, 0xffdf, 0xfffd, 0xff94, 0x0004	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_0014	;* frame ptr
	.word	0xffe6, 0xffdb, 0xfffd, 0xff94, 0x0004	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_LINK 0x0000 bmary_spr_anim_IDLE_steps

bmary_spr_anim_NEWFMT_steps:
	.long	bmary_spr_0018	;* frame ptr
	.word	0xffe0, 0xffe1, 0xfffe, 0xff93, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	.long	bmary_spr_000f	;* frame ptr
	.word	0xffe0, 0xffe1, 0xfffe, 0xff93, 0x0005	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_LINK 0x0005 bmary_spr_anim_NEWFMT_steps



portraits_spr_animations:
	.long	portraits_spr_anim_KULA_steps	;* steplist
	.long	portraits_spr_anim_LEONA_steps	;* steplist

portraits_spr_anim_KULA_steps:
	.long	portraits_spr_0000	;* frame ptr
	.word	0xffb4, 0xffbd, 0x0000, 0xff81, 0x7d00	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_LINK 0x0000 portraits_spr_anim_KULA_steps

portraits_spr_anim_LEONA_steps:
	.long	portraits_spr_0001	;* frame ptr
	.word	0xffd4, 0xffb9, 0x0000, 0xff71, 0x7d00	;* flipShiftX, shiftX, flipShiftY, shiftY, duration
	_ANM_LINK 0x0001 portraits_spr_anim_LEONA_steps



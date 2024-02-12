	_MESS_ENDCODE = 0xff
	
	.macro	MESS_END
	.word	0
	.endm

	_FMT_ENDCODE	= 0
	_FMT_SETSIZE	= 1

	_FMT_BYTE_DATA	= 0
	_FMT_WORD_DATA	= 2

	.macro	MESS_FORMAT fmt data
	.byte	\fmt, 1
	.word	\data
	.endm

	.macro	MESS_AUTOINC amount
	.byte	\amount, 2
	.endm

	.macro	MESS_ADDR addr
	.word	3, \addr
	.endm

	.macro	MESS_POS posX posY
	.word	3, 0x7000+\posY+(\posX<<5)
	.endm

	.macro	MESS_ADDR_INC amount
	.word	5, \amount
	.endm

	.macro	MESS_RESUME
	.word	6
	.endm

	.macro	MESS_PRINT data endcode=0xff
	.word	7
	.ascii	"\data"
	.byte	\endcode
	.align	2
	.endm

	;*8

	;*9

	.macro	MESS_CALL addr
	.word	0xa
	.long	\addr
	.endm

	.macro	MESS_RETURN
	.word	0xb
	.endm

	.macro	MESS_REPEAT data count
	.byte	\count, 0xc
	.word	\data
	.endm

	.macro	MESS_INC_REPEAT data count
	.byte	\count, 0xd
	.word	\data
	.endm


	;* mess data
	.globl	mess_pictureDemo
mess_pictureDemo:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 2, 3
		MESS_PRINT "1P \x12\x13\x10\x11: move picture"
		MESS_POS 2, 4
		MESS_PRINT "1P A+\x12\x13\x10\x11: flip mode"
		MESS_POS 2, 5
		MESS_PRINT "1P B: toggle rasters"
		MESS_END

	.globl	mess_colorStreamDemoA
mess_colorStreamDemoA:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 2, 3
		MESS_PRINT "1P \x10\x11: scroll"
		MESS_END

	.globl	mess_colorStreamDemoB
mess_colorStreamDemoB:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 2, 3
		MESS_PRINT "1P \x12\x13: scroll"
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x63ff
		MESS_POS 2, 29
		MESS_PRINT "(Sequence formatted by MegaShocked)"
		MESS_END

	.globl	mess_rasterScrollDemo
mess_rasterScrollDemo:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x00ff
		MESS_AUTOINC 0x20
		MESS_POS 0, 1
		MESS_CALL _submess_blankLine
		MESS_POS 0, 30
		MESS_CALL _submess_blankLine
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_POS 2, 3
		MESS_PRINT "1P \x12\x13: Scroll Up/Down"
		MESS_POS 2, 4
		MESS_PRINT "1P A+\x12\x13: Adjust timer (line)"
		MESS_POS 2, 5
		MESS_PRINT "1P A+\x10\x11: Adjust timer (unit)"
		MESS_END

_submess_blankLine:
		MESS_PRINT "                                       "	;* minus 1 to keep job meter
		MESS_RETURN


	.globl	mess_logos
mess_logos:
	.long	mess_logo95, mess_logo96, mess_logo97, mess_logo98

.macro	PRINTLOGO	tileNum, w, h
	.set	__tile, \tileNum
	.rept	\h-1
		MESS_INC_REPEAT __tile, \w
		MESS_ADDR_INC 1
	.set	__tile, __tile+0x10
	.endr
		MESS_INC_REPEAT __tile, \w
.endm

mess_logo95:
		MESS_AUTOINC 0x20
		MESS_POS 23, 6
		PRINTLOGO 0xd500, 12, 6
		MESS_END

mess_logo96:
		MESS_AUTOINC 0x20
		MESS_POS 23, 6
		PRINTLOGO 0xd560, 12, 6
		MESS_END

mess_logo97:
		MESS_AUTOINC 0x20
		MESS_POS 23, 6
		PRINTLOGO 0xd600, 12, 6
		MESS_END

mess_logo98:
		MESS_AUTOINC 0x20
		MESS_POS 23, 6
		PRINTLOGO 0xd660, 12, 6
		MESS_END



;* main menu messages
.globl mess_menu
mess_menu:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 8, 20
		MESS_PRINT "(P1 START - Menu return)"
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x53ff
		MESS_POS 8, 28
		MESS_PRINT "libNG tests - @2024 Hpman"
		MESS_END

mess_menuItem0_msg:
		MESS_POS 8, 10
		MESS_PRINT "Picture demo"
		MESS_RETURN
mess_menuItem1_msg:
		MESS_POS 8, 11
		MESS_PRINT "Scroller demo"
		MESS_RETURN
mess_menuItem2_msg:
		MESS_POS 8, 12
		MESS_PRINT "Animated sprite demo"
		MESS_RETURN
mess_menuItem3_msg:
		MESS_POS 8, 13
		MESS_PRINT "Fix layer demo"
		MESS_RETURN
mess_menuItem4_msg:
		MESS_POS 8, 14
		MESS_PRINT "Raster demo A"
		MESS_RETURN
mess_menuItem5_msg:
		MESS_POS 8, 15
		MESS_PRINT "Raster demo B"
		MESS_RETURN
mess_menuItem6_msg:
		MESS_POS 8, 16
		MESS_PRINT "Color stream demo A"
		MESS_RETURN
mess_menuItem7_msg:
		MESS_POS 8, 17
		MESS_PRINT "Color stream demo B"
		MESS_RETURN

mess_menuBase:
		MESS_AUTOINC 0x20
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_CALL mess_menuItem0_msg
		MESS_CALL mess_menuItem1_msg
		MESS_CALL mess_menuItem2_msg
		MESS_CALL mess_menuItem3_msg
		MESS_CALL mess_menuItem4_msg
		MESS_CALL mess_menuItem5_msg
		MESS_CALL mess_menuItem6_msg
		MESS_CALL mess_menuItem7_msg
		MESS_RETURN

.globl mess_menuMsgs
mess_menuMsgs:
	.long	mess_index0, mess_index1, mess_index2, mess_index3, mess_index4, mess_index5, mess_index6, mess_index7

mess_index0:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem0_msg
		MESS_END
mess_index1:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem1_msg
		MESS_END
mess_index2:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem2_msg
		MESS_END
mess_index3:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem3_msg
		MESS_END
mess_index4:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem4_msg
		MESS_END
mess_index5:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem5_msg
		MESS_END
mess_index6:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem6_msg
		MESS_END
mess_index7:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem7_msg
		MESS_END

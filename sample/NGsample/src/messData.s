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

	.macro	MESS_PRINT_ADDR addr
	.word	4
	.long	\addr
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

	.macro	MESS_PRINT16 palBank, data endcode=0xff
	.byte	\palBank, 8
	.ascii	"\data"
	.byte	\endcode
	.align	2
	.endm

	.macro	MESS_PRINT16_JP palBank, data endcode=0xff
	.byte	\palBank, 9
	.ascii	"\data"
	.byte	\endcode
	.align	2
	.endm

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

;* ====================================
;* misc control info messages for demos
;* ====================================
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
		MESS_PRINT "1P \x10\x11: scroll (hold A: faster)"
		MESS_END

	.globl	mess_colorStreamDemoB
mess_colorStreamDemoB:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 2, 3
		MESS_PRINT "1P \x12\x13: scroll (hold A: frame scroll)"
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

	.globl	mess_aspriteDemo
mess_aspriteDemo:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 2, 3
		MESS_PRINT "1P \x12\x13\x10\x11: move sprite"
		MESS_POS 2, 4
		MESS_PRINT "1P A+\x12\x13\x10\x11: flip mode"
		MESS_POS 2, 5
		MESS_PRINT "1P B/C/D: toggle anim/anchor/debug"
		MESS_POS 2, 6
		MESS_PRINT "2P \x12\x13\x10\x11: scale sprite"
		MESS_POS 2, 7
		MESS_PRINT "2P A: toggle color blink"
		MESS_END

	.globl	mess_scrollDemo
mess_scrollDemo:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 2, 3
		MESS_PRINT "1P \x12\x13\x10\x11: scroll"
		MESS_POS 20, 3
		MESS_PRINT "2P: color math"
		MESS_POS 20, 4
		MESS_PRINT "A/B/C: RGB comp."
		MESS_POS 20, 5
		MESS_PRINT "\x12/\x13: fade out/in"
		MESS_POS 20, 6
		MESS_PRINT "\x10/\x11: set/reset"
		MESS_END

_submess_blankLine:
		MESS_PRINT "                                       "	;* minus 1 to keep job meter in this demo program
		MESS_RETURN

;* =======================
;* fix layer demo messages
;* =======================
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


;* ========================
;* typewriter demo messages
;* ========================
	.globl	mess_typerWindow
mess_typerWindow:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x81ff	;* 0x81 hi byte, 0xff end code
		MESS_AUTOINC 0x20
		MESS_POS 7, 20
		MESS_CALL _windowLine
		MESS_CALL _windowLine
		MESS_CALL _windowLine
		MESS_CALL _windowLine
		MESS_END

_windowLine:	MESS_PRINT16	0x81, "                          "
		MESS_ADDR_INC 1
		MESS_RETURN

	.globl	typerHandle
	.globl	mess_typerData
mess_typerData:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_SETSIZE), 0x8118	;* 0x81 hi byte, 24 bytes size
		MESS_AUTOINC 0x20
		MESS_POS 8, 21
		MESS_CALL _mess_typerPrint
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_SETSIZE), 0x8218
		MESS_POS 8, 22
		MESS_CALL _mess_typerPrint
		MESS_END

_mess_typerPrint:
		MESS_PRINT_ADDR typerHandle
		MESS_ADDR_INC 2
		MESS_RESUME
		MESS_ADDR_INC 2
		MESS_RESUME
		MESS_RETURN

;* ==================
;* main menu messages
;* ==================
.globl mess_menu
mess_menu:
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x43ff
		MESS_AUTOINC 0x20
		MESS_POS 8, 22
		MESS_PRINT "(P1 START - Menu return)"
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x53ff
		MESS_POS 8, 28
		MESS_PRINT "libNG tests - @2025 Hpman"
		MESS_END

	_menuLine = 10

mess_menuItem0_msg:
		MESS_POS 8, _menuLine+0
		MESS_PRINT "Picture demo"
		MESS_RETURN
mess_menuItem1_msg:
		MESS_POS 8, _menuLine+1
		MESS_PRINT "Scroller demo"
		MESS_RETURN
mess_menuItem2_msg:
		MESS_POS 8, _menuLine+2
		MESS_PRINT "Animated sprite demo"
		MESS_RETURN
mess_menuItem3_msg:
		MESS_POS 8, _menuLine+3
		MESS_PRINT "Hicolor sprite demo"
		MESS_RETURN
mess_menuItem4_msg:
		MESS_POS 8, _menuLine+4
		MESS_PRINT "FIX layer demo"
		MESS_RETURN
mess_menuItem5_msg:
		MESS_POS 8, _menuLine+5
		MESS_PRINT "Typewriter demo"
		MESS_RETURN
mess_menuItem6_msg:
		MESS_POS 8, _menuLine+6
		MESS_PRINT "Raster demo A"
		MESS_RETURN
mess_menuItem7_msg:
		MESS_POS 8, _menuLine+7
		MESS_PRINT "Raster demo B"
		MESS_RETURN
mess_menuItem8_msg:
		MESS_POS 8, _menuLine+8
		MESS_PRINT "Color stream demo A"
		MESS_RETURN
mess_menuItem9_msg:
		MESS_POS 8, _menuLine+9
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
		MESS_CALL mess_menuItem8_msg
		MESS_CALL mess_menuItem9_msg
		MESS_RETURN

.globl mess_menuMsgs
mess_menuMsgs:
	.long	mess_index0, mess_index1, mess_index2, mess_index3, mess_index4, mess_index5, mess_index6, mess_index7, mess_index8, mess_index9

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
mess_index8:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem8_msg
		MESS_END
mess_index9:
		MESS_CALL mess_menuBase
		MESS_FORMAT (_FMT_BYTE_DATA+_FMT_ENDCODE), 0x23ff
		MESS_CALL mess_menuItem9_msg
		MESS_END

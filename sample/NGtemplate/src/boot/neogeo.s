.section .text.keepboot
	.org	0

********************* Vectors & game configuration definitions  *********************
_IRQ_TIMER		=	libNG_TIfunc
;* _IRQ_VBLANK		= 	libNG_vblank	;* without timer IRQ handling
_IRQ_VBLANK		= 	libNG_vblankTI	;* with timer IRQ handling

_TRAP_0			=	0xc00426	;* dummy handler, will reset system
_TRAP_1			=	0xc00426	;* dummy handler, will reset system
_TRAP_2			=	0xc00426	;* dummy handler, will reset system
_TRAP_3			=	0xc00426	;* dummy handler, will reset system
_TRAP_4			=	0xc00426	;* dummy handler, will reset system
_TRAP_5			=	0xc00426	;* dummy handler, will reset system
_TRAP_6			=	0xc00426	;* dummy handler, will reset system
_TRAP_7			=	0xc00426	;* dummy handler, will reset system
_TRAP_8			=	0xc00426	;* dummy handler, will reset system
_TRAP_9			=	0xc00426	;* dummy handler, will reset system
_TRAP_A			=	0xc00426	;* dummy handler, will reset system
_TRAP_B			=	0xc00426	;* dummy handler, will reset system
_TRAP_C			=	0xc00426	;* dummy handler, will reset system
_TRAP_D			=	0xc00426	;* dummy handler, will reset system
_TRAP_E			=	0xc00426	;* dummy handler, will reset system
_TRAP_F			=	0xc00426	;* dummy handler, will reset system

_NGH			=	0x7777		;* unique game code
_PROGRAM_SIZE		=	0x100000	;* 
_WRK_BCKP_AREA		=	bkp_data	;* bckp area address - from linker file
_WRK_BCKP_AREA_SIZE	=	0x100		;* bckp area size
_EYE_CATCHER		=	0x02		;* eye catcher mode (0-default 1-custom 2-off)
_EYE_CATCHER_TILES	=	0x01		;* eye catcher start tiles (upper bits, 0x01 => 0x0100)

;* NEO-GEO CD
_SYSTEM_NCD		=	0		;* 0-cart 1-CD
_CDDA_FLAG		=	0x00		;* for CD builds
_CDDA_ADDR		=	0xffff		;* for CD builds, Z80 CDDA addr

;* insert main 68K header file @0x0000
	.include	"src/boot/neogeo_boot.s"


;* Game specific soft dips
;* Game title must be 16 characters width
;* Options labels must be 12 characters width

JPconfig:
	.ascii	"JP GAME TITLE   "
	.word	0xffff		;* unused special timer #1 setting
	.word	0xffff		;* unused special timer #2 setting
	.byte	0xff, 0xff	;* unused special counter #1 & #2
	.byte	0x38		;* difficulty 3 of 8 settings
	.byte	0x12		;* demo sound on
	.byte	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 ;* unused settings

	.ascii	"DIFFICULTY  "
 	.ascii	"LEVEL 1     "
 	.ascii	"LEVEL 2     "
 	.ascii	"LEVEL 3     "
 	.ascii	"LEVEL 4     "
 	.ascii	"LEVEL 5     "
 	.ascii	"LEVEL 6     "
 	.ascii	"LEVEL 7     "
 	.ascii	"LEVEL 8     "
 	
 	.ascii	"DEMO SOUND  "
 	.ascii	"WITHOUT     "
 	.ascii	"WITH        "
	;* JP config: no language choice

NAconfig:
	.ascii	"NA GAME TITLE   "
	.word	0xffff		;* unused special timer #1 setting
	.word	0xffff		;* unused special timer #2 setting
	.byte	0xff, 0xff	;* unused special counter #1 & #2
	.byte	0x38		;* difficulty 3 of 8 settings
	.byte	0x12		;* demo sound on
	.byte	0x02		;* languages
	.byte	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 ;* unused settings

	.ascii	"DIFFICULTY  "
 	.ascii	"LEVEL 1     "
 	.ascii	"LEVEL 2     "
 	.ascii	"LEVEL 3     "
 	.ascii	"LEVEL 4     "
 	.ascii	"LEVEL 5     "
 	.ascii	"LEVEL 6     "
 	.ascii	"LEVEL 7     "
 	.ascii	"LEVEL 8     "
 	
 	.ascii	"DEMO SOUND  "
 	.ascii	"WITHOUT     "
 	.ascii	"WITH        "
	
	.ascii	"LANGUAGE    "
	.ascii	"ENGLISH     "
	.ascii	"SPANISH     "

SPconfig:
EUconfig:
	.ascii	"EU GAME TITLE   "
	.word	0xffff		;* unused special timer #1 setting
	.word	0xffff		;* unused special timer #2 setting
	.byte	0xff, 0xff	;* unused special counter #1 & #2
	.byte	0x38		;* difficulty 3 of 8 settings
	.byte	0x12		;* demo sound on
	.byte	0x04		;* languages
	.byte	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 ;* unused settings

	.ascii	"DIFFICULTY  "
 	.ascii	"LEVEL 1     "
 	.ascii	"LEVEL 2     "
 	.ascii	"LEVEL 3     "
 	.ascii	"LEVEL 4     "
 	.ascii	"LEVEL 5     "
 	.ascii	"LEVEL 6     "
 	.ascii	"LEVEL 7     "
 	.ascii	"LEVEL 8     "
 	
 	.ascii	"DEMO SOUND  "
 	.ascii	"WITHOUT     "
 	.ascii	"WITH        "
	
	.ascii	"LANGUAGE    "
	.ascii	"ENGLISH     "
	.ascii	"SPANISH     "
	.ascii	"FRENCH      "	;* EU settings have extra languages
	.ascii	"GERMAN      "


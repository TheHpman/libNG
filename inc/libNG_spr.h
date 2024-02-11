/**
 *  \file libNG.h
 *  \brief libNG aSprite defines
 *
 *  Provides defines, typedefs & prototypes for animated sprites (aSprite type), 
 *  as well as sprite pools.
 */

#ifndef __LIBNG_SPR_H__
#define __LIBNG_SPR_H__

#include <types.h>

//** flip modes - VALUES ARE CODE BOUND, DO NOT CHANGE
#define FLIP_NONE	0
#define FLIP_X		1
#define FLIP_Y		2
#define FLIP_XY		3
#define FLIP_BOTH	3

//aSprites consts
#define	AS_FLAGS_DEFAULT	0x0000
#define	AS_FLAG_MOVED		0x0001
#define	AS_FLAG_FLIPPED		0x0002
#define	AS_FLAG_STD_COORDS	0x0000
#define	AS_FLAG_STRICT_COORDS	0x0040
#define	AS_FLAG_DISPLAY		0x0000
#define	AS_FLAG_NODISPLAY	0x0080
#define	AS_FLAG_NOANIM		0x8000

#define	AS_MASK_MOVED		0xfffe
#define	AS_MASK_FLIPPED		0xfffd
#define	AS_MASK_MOVED_FLIPPED	0xfffc
#define	AS_MASK_STRICT_COORDS	0xffbf
#define	AS_MASK_NODISPLAY	0xff7f
#define	AS_MASK_NOANIM		0x7fff

#define	AS_USE_SPRITEPOOL	0x8000
#define	AS_NOSPRITECLEAR	0x8000


//** typedefs

/**
 *  \brief
 *      Sprite frame data (plain style)
 */
typedef struct sprFrame {
	u16		tileWidth;		/**< Frame tile width */
	u16		tileHeight;		/**< Frame tile height */
	u16		stripSize;		/**< Strip byte size (tileHeight *4) */
	u16		*maps[4];		/**< Pointer array to tilemap data (FLIP_NONE/FLIP_X/FLIP_Y/FLIP_XY) */
} sprFrame;

/**
 *  \brief
 *      Sprite frame data (split style)
 */
typedef struct sprFrame2 {
	u16		key;			/**< Key to signal split format (always 0) */
	u16		sprCount;		/**< Sprite count used in frame */
	u16		*maps[4];		/**< Pointer array to tilemap data (FLIP_NONE/FLIP_X/FLIP_Y/FLIP_XY) */
} sprFrame2;

/**
 *  \brief
 *      Animation step data
 */
typedef struct animStep {
	sprFrame	*frame;			/**< Pointer to frame data */
	s16		flipShiftX;		/**< Frame X offset (X flipped) */
	s16		shiftX;			/**< Frame X offset */
	s16		flipShiftY;		/**< Frame Y offset (Y flipped) */
	s16		shiftY;			/**< Frame Y offset */
	u16		duration;		/**< Display frame time of step */
} animStep;
// /!\ size is code bound

/**
 *  \brief
 *      Animated sprite ressource data
 */
typedef struct spriteInfo {
	u16		frameCount;		/**< # of frames */
	u16		maxWidth;		/**< Tile width of largest frame */
	paletteInfo	*palInfo;		/**< Pointer to related palette ressource data */
	animStep	**anims;		/**< Pointer to animation data */
	sprFrame	frames[0];		/**< Array of frames */
} spriteInfo;

/**
 *  \brief
 *      aSprite object handle
 */
typedef struct __attribute__((packed,aligned(2))) aSprite {
	u16		baseSprite;		/**< Sprite # of first sprite */
	u8		basePalette;		/**< Base palette # (0-255) */
	u8		bank;			/**< Bank # */
	s16		posX;			/**< X coordinate of sprite */
	s16		posY;			/**< Y coordinate of sprite */
	u16		animID;			/**< Animation ID */
	u16		currentAnim;		/**< Current animation ID */
	u16		stepNum;		/**< Current animation step # */
	animStep	*anims;			/**< Pointer to animations data */
	animStep	*steps;			/**< Pointer to steps data for current animation */
	animStep	*currentStep;		/**< Pointer to current step data */
	sprFrame	*currentFrame;		/**< Pointer to current frame data */
	u16		counter;		/**< Frame counter for current step */
	u16		repeats;		/**< # of played repeats */
	u16		tileWidth;		/**< (internal, for fixed allocation) */
	u16		currentFlip;		/**< Current orientation of sprite */
	union
	{
		u16		flags;			/**< Current flags of sprite */
		struct
		{
			u16	flag_noAnim : 1;
			u16	flag_padding : 7;
			u16	flag_noDisplay : 1;
			u16	flag_strictCoords : 1;
			u16	flag_none : 4;
			u16	flag_flipped : 1;
			u16	flag_moved : 1;
		};
	};
	union
	{
		struct
		{
			u8	Xbig;		/**< Horizontal scale (0-255) */
			u8	Ybig;		/**< Vertical scale (0-255) */
		};
		u16	XYbig;			/**< Horizontal & Vertical scale */
	};
} aSprite;

//** macros

/**
 *  \brief
 *      Disable display of animated sprite
 *
 *  \param as
 *      pointer to aSprite handle
  */
#define aSpriteHide(as)		{(as)->flags|=AS_FLAG_NODISPLAY;}

/**
 *  \brief
 *      Re-enable display of animated sprite
 *
 *  \param as
 *      pointer to aSprite handle
  */
#define aSpriteShow(as)		{(as)->flags&=AS_MASK_NODISPLAY;(as)->flags|=AS_FLAG_FLIPPED;}


//** functions

/**
*  \brief
*      Initialize an aSprite handle
*  \param as
*      aSprite handle
*  \param si
*      Sprite info resource data
*  \param baseSprite
*      Starting sprite
*  \param basePalette
*      Starting palette
*  \param posX
*      X start position
*  \param posY
*      Y start position
*  \param anim
*      Start animation
*  \param flip
*      Flip mode to initialize with
*  \param flags
*      Flags to initialize with
*/
void aSpriteInit(aSprite *as, const spriteInfo *si, u16 baseSprite, u8 basePalette, s16 posX, s16 posY, u16 anim, u16 flip, u16 flags);

/**
*  \brief
*      Animate & update display of fixed allocation aSprite 
*  \param as
*      aSprite handle
*/
void aSpriteAnimate(aSprite *as);

/**
*  \brief
*      Set aSprite animation
*	No action if same anim is already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*/
void aSpriteSetAnim(aSprite *as, u16 anim);

/**
*  \brief
*      Set aSprite animation
*	Will retrigger if same anim is already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*/
void aSpriteSetAnim2(aSprite *as, u16 anim);

/**
*  \brief
*      Set aSprite step for current animation
*	No action if same step is already playing
*  \param as
*      aSprite handle
*  \param step
*      Step number
*/
void aSpriteSetStep(aSprite *as, u16 step);

/**
*  \brief
*      Set aSprite step for current animation
*	Will retrigger if same step is already playing
*  \param as
*      aSprite handle
*  \param step
*      Step number
*/
void aSpriteSetStep2(aSprite *as, u16 step);

/**
*  \brief
*      Set aSprite animation and step
*	No action if same parameters already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*  \param step
*      Step number
*/
void aSpriteSetAnimStep(aSprite *as, u16 anim, u16 step);

/**
*  \brief
*      Set aSprite animation and step
*	Will retrigger if same parameters already playing
*  \param as
*      aSprite handle
*  \param anim
*      Animation ID
*  \param step
*      Step number
*/
void aSpriteSetAnimStep2(aSprite *as, u16 anim, u16 step);

/**
*  \brief
*      Set aSprite position
*  \param as
*      aSprite handle
*  \param newX
*      X position to set
*  \param newY
*      Y position to set
*/
void aSpriteSetPos(aSprite *as, s16 newX, s16 newY);

/**
*  \brief
*      Shift aSprite position
*  \param as
*      aSprite handle
*  \param shiftX
*      X axis position shift
*  \param newY
*      Y axis position shift
*/
void aSpriteMove(aSprite *as, s16 shiftX, s16 shiftY);

/**
*  \brief
*      Set aSprite flip mode
*  \param as
*      aSprite handle
*  \param flip
*      new flip setting (FLIP_xxx)
*/
void aSpriteSetFlip(aSprite *as, u16 flip);

/**
*  \brief
*      Update animation of a single aSprite item
*  \param as
*      aSprite handle
*/
void aSpriteAnimateSingle(aSprite *as);

/**
*  \brief
*      Update animation of a list of aSprite items
*  \param list
*      pointer to aSprite list
*/
void aSpriteAnimateList(void *list);

/**
*  \brief
*      Get next step to be displayed (does not tick animation)
*  \param list
*      pointer to aSprite handle
*/
animStep* aSpriteGetNextStep(aSprite *as);


// ========== sprite pools ==========

//sprite pools consts
#define WAY_UP			0
#define WAY_DOWN		1

/**
 *  \brief
 *      Sprite pool object handle
 */
typedef struct spritePool {
	u16		poolStart;		/**< Starting sprite # of pool */
	u16		poolEnd;		/**< Last sprite # of pool */
	u16		poolSize;		/**< Pool size (sprite count) */
	u16		way;			/**< Current render way (WAY_UP/WAY_DOWN) */
	u16		currentUp;		/**< Current sprite # for up render */
	u16		currentDown;		/**< Current sprite # for down render */
} spritePool;

/**
*  \brief
*      Initialize a sprite pool handle
*  \param sp
*      spritePool handle
*  \param baseSprite
*      Base sprite #
*  \param size
*      Pool total sprite count
*  \param clearSprites
*      Pool sprites will be cleared if true.
*      (Sprites will be moved offscreen, tilemap is not cleared)
*/
void spritePoolInit(spritePool *sp, u16 baseSprite, u16 size, bool clearSprites);

/**
*  \brief
*      Render an object list into sprite pool
*  \param sp
*      spritePool handle
*  \param list
*      aSprite pointer list
*/
void spritePoolDrawList(spritePool *sp,void *list);

/**
*  \brief
*      Render an object list into sprite pool - IRQ safe version
*  \param sp
*      spritePool handle
*  \param list
*      aSprite pointer list
*/
void spritePoolDrawList2(spritePool *sp,void *list);

/**
*  \brief
*      Render an object list into sprite pool
*      Support scaling and new sprite format
*  \param sp
*      spritePool handle
*  \param list
*      aSprite pointer list
*/
void spritePoolDrawList3(spritePool *sp,void *list);

/**
*  \brief
*      Terminate operations on sprite pool for current frame
*  \param sp
*      spritePool handle
*/
void spritePoolClose(spritePool *sp);

#endif //__LIBNG_SPR_H__
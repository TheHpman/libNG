/**
 *  \file libNG.h
 *  \brief libNG picture defines
 *
 *  Provides defines, typedefs & prototypes for picture objects. 
  */

#ifndef __LIBNG_PICT_H__
#define __LIBNG_PICT_H__

#include <types.h>


/**
 *  \brief
 *      Picture ressource data
 */
typedef struct pictureInfo {
	u16		stripSize;		/**< Byte size of each sprite tiledata (picture tile height * 4) */
	u16		tileWidth;		/**< Tile width of picture */
	u16		tileHeight;		/**< Tile height of picture */
	paletteInfo	*palInfo;		/**< Pointer to related paletteInfo ressource */
	u16		*maps[4];		/**< Pointer array to tilemap data (normal/flipX/flipY/flipXY) */
} pictureInfo;

/**
 *  \brief
 *      picture object handle
 */
typedef struct __attribute__((packed,aligned(2))) picture {
	u16		baseSprite;		/**< Sprite # of first sprite */
	u8		basePalette;		/**< Base palette # (0-255) */
	u8		RFU;			/**< Reserved future use / padding */
	s16		posX;			/**< X coordinate of picture */
	s16		posY;			/**< Y coordinate of picture */
	u16		currentFlip;		/**< Current orientation of picture (FLIP_NONE/FLIP_X/FLIP_Y/FLIP_XY) */
	pictureInfo	*info;			/**< Pointer to related pictureInfo ressource data */
} picture;


// ========== pictures ==========
/**
*  \brief
*      Initializes a picture handle
*  \param p
*      picture handle
*  \param pi
*      Picture info resource data
*  \param baseSprite
*      Starting sprite
*  \param basePalette
*      Starting palette
*  \param posX
*      X start position
*  \param posY
*      Y start position
*  \param flip
*      Flip mode
*/
void pictureInit(picture *p, const pictureInfo *pi, u16 baseSprite, u8 basePalette, s16 posX, s16 posY, u16 flip);

/**
*  \brief
*      Set picture screen position (top-left corner)
*  \param p
*      picture handle
*  \param toX
*      Picture X position
*  \param toY
*      Starting Y position
*/
void pictureSetPos(picture *p, s16 toX, s16 toY);

/**
*  \brief
*      Set picture flip mode
*  \param p
*      picture handle
*  \param flip
*      Flip mode
*/
void pictureSetFlip(picture *p, u16 flip);

/**
*  \brief
*      Move picture, relative from current position
*  \param p
*      picture handle
*  \param shiftX
*      X shift from current position
*  \param shiftY
*      Y shift from current position
*/
void pictureMove(picture *p, s16 shiftX, s16 shiftY);

/**
*  \brief
*      Disable picture display
*  \param p
*      picture handle
*/
void pictureHide(picture *p);

/**
*  \brief
*      Enable picture display
*  \param p
*      picture handle
*/
void pictureShow(picture *p);


#endif // __LIBNG_PICT_H__
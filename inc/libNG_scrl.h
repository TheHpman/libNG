/**
 *  \file libNG.h
 *  \brief libNG scroller defines
 *
 *  Provides defines, typedefs & prototypes for scrollers (scroller type), 
 *  as well as color streams (colorStream type).
 */
#ifndef __LIBNG_SCRL_H__
#define __LIBNG_SCRL_H__

#include <types.h>


// ========== color streams ==========

//color streams
#define COLORSTREAM_STARTCONFIG	0
#define COLORSTREAM_ENDCONFIG	1


/**
 *  \brief
 *      Color stream ressource data
 */
typedef struct colorStreamInfo {
	u16		palSlots;		/**< # of required palettes */
	void		*startConfig;		/**< Pointer to start configuration data */
	void		*endConfig;		/**< Pointer to end configuration data */
	void		*fwData;		/**< Pointer to forward data */
	void		*fwDataEnd;		/**< Pointer to forward data end */
	void		*bwData;		/**< Pointer to backward data */
	void		*bwDataEnd;		/**< Pointer to backward data end */
} colorStreamInfo;

/**
 *  \brief
 *      Color stream job data
 */
typedef struct colorStreamJob {
	u16		coord;			/**< Job coordinate */
	void		*data;			/**< Pointer to job data */
} colorStreamJob;

/**
 *  \brief
 *      colorStream object handle
 */
typedef struct colorStream {
	u16		palMod;			/**< Base palette offset (pal*32) */
	u16		position;		/**< Current position in stream */
	colorStreamInfo	*info;			/**< Pointer to colorStreamInfo ressource data */
	colorStreamJob	*fwJob;			/**< Pointer to current forward data */
	colorStreamJob	*bwJob;			/**< Pointer to current backward data end */
} colorStream;


/**
*  \brief
*      Initialize a colorStream handle
*  \param cs
*      colorStream handle
*  \param basePalette
*      Starting palette
*  \param config
*      Start configuration (COLORSTREAM_STARTCONFIG/COLORSTREAM_ENDCONFIG)
*/
void colorStreamInit(colorStream *cs, const colorStreamInfo *csi, u16 basePalette, u16 config);

/**
*  \brief
*      Update a colorStream to set position
*  \param cs
*      colorStream handle
*  \param pos
*      New stream position
*/
void colorStreamSetPos(colorStream *cs, u16 pos);


// ========== scrollers ==========

/**
 *  \brief
 *      Scroller ressource data
 */
typedef struct scrollerInfo {
	u16		stripSize;		/**< Byte size of each strip tiledata (scroller plane tile height * 4) */
	u16		sprHeight;		/**< Required sprite height (1-32) */
	u16		mapWidth;		/**< Scroller plane tile width */
	u16		mapHeight;		/**< Scroller plane tile height */
	paletteInfo	*palInfo;		/**< Pointer to related paletteInfo ressource */
	colorStreamInfo	*csInfo;		/**< Pointer to related colorStreamInfo ressource */
	u16		*strips[0];		/**< Pointer array to tilemap strips data */
} scrollerInfo;

/**
 *  \brief
 *      scroller object handle
 */
typedef struct scroller {
	u16		baseSprite;		/**< Sprite # of first sprite */
	u16		basePalette;		/**< Base palette # (0-255) */
	s16		scrlPosX;		/**< Scroller current X position */
	s16		scrlPosY;		/**< Scroller current Y position */
	scrollerInfo 	*info;			/**< Pointer to related scrollerInfo ressource */
	u16		config[32];		/**< Internal use data */
} scroller;


/**
*  \brief
*      Initialize a scroller handle
*  \param s
*      scroller handle
*  \param si
*      Scroller info resource data
*  \param baseSprite
*      Starting sprite
*  \param basePalette
*      Starting palette
*  \param posX
*      X start position
*  \param posY
*      Y start position
*/
void scrollerInit(scroller *s, const scrollerInfo *si, u16 baseSprite, u8 basePalette, s16 posX, s16 posY);

/**
*  \brief
*      Shift a scroller to set position
*  \param s
*      scroller handle
*  \param toX
*      X position to scroll to
*  \param toY
*      Y position to scroll to
*/
void scrollerSetPos(scroller *s, s16 toX, s16 toY);

#endif //__LIBNG_SCRL_H__

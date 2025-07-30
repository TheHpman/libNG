/*
Typewriter effect message box demo program.

textTyperDemo()
---------------
Demonstrates how to map a dynamic text buffer to a fixed message command
to allow typewriter effects more easily.

This demo uses byte string data, if you want to implement palette/fix bank
change commands it must be tweaked to use word string data.
*/

#include <neogeo.h>

#define TYPER_LINE_WIDTH 24 // must be multiple of 4 for this demo
#define TYPER_LINE_COUNT 3

enum typerStates
{
	TYPE_TYPING,
	TYPE_BTNWAIT,
	TYPE_TIMEWAIT,
	TYPE_OVER
};

typedef struct t_typerHandle
{
	char typeBuffer[TYPER_LINE_WIDTH * TYPER_LINE_COUNT];
	char *ptrBuffer, *ptrText;
	u8 characterDelay, counter;
	u8 state;
} t_typerHandle;

t_typerHandle typerHandle;

void typerInit(char *text)
{
	t_typerHandle *th = &typerHandle;

	// clear buffer
	// use memset() if available
	u32 *ptrC = (u32 *)&th->typeBuffer;
	for (u16 x = 0; x < sizeof(typerHandle.typeBuffer) / 4; x++)
		*ptrC++ = 0x20202020;

	th->ptrBuffer = (char *)&th->typeBuffer;
	th->ptrText = text;
	th->characterDelay = 20;
	th->counter = 1;
	th->state = TYPE_TYPING;
}

u8 tickTyper()
{
	t_typerHandle *th = &typerHandle;
	char c;

	switch (th->state)
	{
	case TYPE_TYPING:
		if (!--th->counter)
		{
			th->counter = th->characterDelay;
		_nextChar:
			switch ((c = *th->ptrText++))
			{
			case 0:
				th->state = TYPE_OVER;
				break;
			case '\n': // line feed (0x0A)
				if (th->ptrBuffer >= &th->typeBuffer[TYPER_LINE_WIDTH * (TYPER_LINE_COUNT - 1)])
				{
					// was last line, line feed all
					u32 *c0 = (u32 *)&th->typeBuffer[0];
					u32 *c1 = (u32 *)&th->typeBuffer[TYPER_LINE_WIDTH];
					u32 *c2 = (u32 *)&th->typeBuffer[TYPER_LINE_WIDTH * (TYPER_LINE_COUNT - 1)];

					for (u16 x = 0; x < TYPER_LINE_WIDTH / 4; x++)
					{
						*c0++ = *c1;
						*c1++ = *c2;
						*c2++ = 0x20202020;
					}
					th->ptrBuffer = &th->typeBuffer[TYPER_LINE_WIDTH * (TYPER_LINE_COUNT - 1)];
				}
				else
				{
					if (th->ptrBuffer < &th->typeBuffer[TYPER_LINE_WIDTH])
						th->ptrBuffer = &th->typeBuffer[TYPER_LINE_WIDTH]; // was line #0
					else
						th->ptrBuffer = &th->typeBuffer[TYPER_LINE_WIDTH * 2]; // was line 1#
				}
				goto _nextChar;
			case '\r': // windows clear, return to position 0 (0x0D)
				for (u32 *c0 = (u32 *)&th->typeBuffer[0]; c0 < (u32 *)&th->typeBuffer[TYPER_LINE_WIDTH * TYPER_LINE_COUNT];)
					*c0++ = 0x20202020;
				th->ptrBuffer = (char *)&th->typeBuffer;
				goto _nextChar;
			case '\t': // set timing (0x09)
				th->counter = (th->characterDelay = *th->ptrText++);
				goto _nextChar;
			case 0xe: // wait command
				th->state = TYPE_TIMEWAIT;
				th->counter = *th->ptrText++;
				break;
			case '\b': // backspace (0x08)
				*--th->ptrBuffer = ' ';
				break;
			case 0xf: // btn press prompt
				th->state = TYPE_BTNWAIT;
				th->counter = 0;
				break;
			default:
				*th->ptrBuffer++ = c;
				if (!th->characterDelay /*th->counter*/)
					goto _nextChar;
			}
		}
		break;
	// wait player input
	case TYPE_BTNWAIT:
		if (BIOS.P1.EDGE.A)
		{
			th->state = TYPE_TYPING;
			th->counter = 1;
			th->typeBuffer[TYPER_LINE_WIDTH * TYPER_LINE_COUNT - 1] = ' ';
		}
		else
			th->typeBuffer[TYPER_LINE_WIDTH * TYPER_LINE_COUNT - 1] = (th->counter++ & 0x20) ? ' ' : 0x13;
		break;
	// timed delay
	case TYPE_TIMEWAIT:
		if (!--th->counter)
		{
			th->state = TYPE_TYPING;
			th->counter = 1;
		}
		break;
	}
	return th->state;
}

// from messData.s
extern u16 mess_typerWindow[], mess_typerData[];

static const char typerDemoMessage[] = "\t\x07TYPEWRITER DEMONSTRATION\n"
				       "This little demo is here\n"
				       "to show how to build\n"
				       "nice message boxes for\n"
				       "dialogues or cut scenes\n"
				       "using a single message\n"
				       "command and a dynamic\n"
				       "buffer.\n"
				       "Press A to continue!\x0f\r"
				       "Special codes are used\n"
				       "for special things like\n"
				       "speed change, wait or\n"
				       "button prompt.\x0f\r"
				       "\t\x02Text can be faaaaaaast,\n"
				       "\t\x28"
				       "can be slow,\n"
				       "\t\0or can be instant!!!!!\x0f\r"
				       "\t\x07It can go backwards...\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b"
				       "Be creative and add your"
				       "own special codes.\n"
				       "Enjoy!\x0e\xb4"; // 3s delay at end

void textTyperDemo()
{
	clearFixLayer();
	addMessage(mess_typerWindow);
	typerInit((char *)typerDemoMessage);

	while (SCClose(), waitVBlank(), 1)
	{
		if (BIOS.START_EDGE.P1_START)
			break;

		if (tickTyper() != TYPE_OVER)
			addMessage(mess_typerData);
		else
			break;
	}
}

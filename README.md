**libNG** is a NEO•GEO library/toolkit designed to ease development on the system in C. It is built as a library for SGDK, as it widely used and documented.


## Where to begin
You need to be familiar with the C language, and specifics of coding for low speed / low ressource systems. ASM warriors are welcome too!<br>
libNG tools are done with C#, so make sure to have a .NET runtime.<br>
Being familiar with SGDK is a plus, as we will use it as a baseline.


### NEO•GEO Reference & tools
* Stef's SGDK. It's for the silly Megadrive but we will use the included 68K GCC toolchain and more from this excellent kit<br>
https://github.com/Stephane-D/SGDK
* Everything NEO•GEO (and more) on the NEO•GEO dev wiki<br>
https://wiki.neogeodev.org/
* Official documentation and guidelines from SNK<br>
http://www.neogeodev.org/NG.pdf
* Universe bios 4.0 from Razoola, recommended for region switching, proper 68K exception messages and for being awesome as a whole.<br>
http://unibios.free.fr
* MAME, your go-to emulator, excellent debugger<br>
https://github.com/mamedev/mame


### Installation

Firstly, install and configure SGDK. You likely already have tried this kit if you are dropping by here.

Copy files from this repo into your SGDK base folder. We are only adding new files, your setup will remain unchanged.

Build the library. From your SGDK folder use:
```
    make -f makelib.neo
```
> [!NOTE]
> libNG comes barebones from any SGDK components, if you are used to and want to use some system abstract features (memory, pools, etc...) copy the related files into src/libNG folder and rebuild the library.

You can now build projects targetting the NEO•GEO, simply use the `makefile.neo` script from your project folder instead of the usual `makefile.gen` one. If you are using an IDE integration, simply change the make script.


## FAQ
* How do I use Memory card / NEO•GEO feature xxx ?<br>
Many system features are handled by the system bios and/or bios calls, check SNK documentation.
* Does it work on real hardware?<br>
No explosions so far.
* Generated data is huge, how comes?<br>
NEO•GEO sprites require more data than other classic systems. Also libNG is speed-optimized, meaning it sacrifices rom space for faster execution. With today memory sizes and costs, this shouldn't be an issue.
* What about sound?<br>
Sound is not currently in the scope of this library, but you can use tools like Blastar's NGFX Soundbuilder https://blastar.citavia.de/

using System;

namespace FamiMan.Core
{

    // Picture processing unit
    public class Ppu
    {
        private Bus _b;
        private Ram _r;
        public Ppu(Bus b)
        {
            _b = b;
            _r = new Ram(16 * 1024);
            /* The PPU addresses a 16kB space, $0000-3FFF, 
             * completely separate from the CPU's address bus. 
             * It is either directly accessed by the PPU itself, 
             * or via the CPU with memory mapped registers at $2006 and $2007.
             * The NES has 2kB of RAM dedicated to the PPU, 
             * normally mapped to the nametable address space from $2000-2FFF, 
             * but this can be rerouted through custom cartridge wiring. */
            Registers = 0;
        }

        public byte Registers;
    }
}
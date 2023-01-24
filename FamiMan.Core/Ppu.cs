using System;
using System.Net.Http.Headers;

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
            Registers = new PPURegister();
            /* The PPU addresses a 16kB space, $0000-3FFF, 
             * completely separate from the CPU's address bus. 
             * It is either directly accessed by the PPU itself, 
             * or via the CPU with memory mapped registers at $2006 and $2007.
             * The NES has 2kB of RAM dedicated to the PPU, 
             * normally mapped to the nametable address space from $2000-2FFF, 
             * but this can be rerouted through custom cartridge wiring. */
        }

        /*
         * PPUCTRL	    $2000	VPHB SINN	NMI enable (V), PPU master/slave (P), sprite height (H), background tile select (B), sprite tile select (S), increment mode (I), nametable select (NN)
         * PPUMASK	    $2001	BGRs bMmG	color emphasis (BGR), sprite enable (s), background enable (b), sprite left column enable (M), background left column enable (m), greyscale (G)
         * PPUSTATUS	$2002	VSO- ----	vblank (V), sprite 0 hit (S), sprite overflow (O); read resets write pair for $2005/$2006
         * OAMADDR	    $2003	aaaa aaaa	OAM read/write address
         * OAMDATA	    $2004	dddd dddd	OAM data read/write
         * PPUSCROLL    $2005	xxxx xxxx	fine scroll position (two writes: X scroll, Y scroll)
         * PPUADDR	    $2006	aaaa aaaa	PPU read/write address (two writes: most significant byte, least significant byte)
         * PPUDATA	    $2007	dddd dddd	PPU data read/write
         * OAMDMA	    $4014	aaaa aaaa	OAM DMA high address
        */
        public PPURegister Registers;
    }

    public class PPURegister
    {
        public PPURegister()
        {
            _registers = new byte[9];
        }

        private byte[] _registers { get; set; }
        public byte PPUCTRL
        {
            get => _registers[0];
            set => _registers[0] = value;
        }

        public byte PPUMASK
        {
            get => _registers[1];
            set => _registers[1] = value;
        }

        public byte PPUSTATUS
        {
            get => _registers[2]; set => _registers[2] = value;
        }

        public byte OAMADDR
        {
            get => _registers[3]; set => _registers[3] = value;
        }

        public byte OAMDATA
        {
            get => _registers[4]; set => _registers[4] = value;
        }

        public byte PPUSCROLL
        {
            get => _registers[5]; set => _registers[5] = value;
        }

        public byte PPUADDR
        {
            get => _registers[6]; set => _registers[6] = value;
        }

        public byte PPUDATA
        {
            get => _registers[7]; set => _registers[7] = value;
        }

        public byte OAMDMA
        {
            get => _registers[8]; set => _registers[8] = value;
        }

        public ref byte this[ushort index]
        {
            get
            {
                if (index >= 0x2000 && index < 0x2008)
                    return ref _registers[index - 0x2000];
                else if (index == 0x4012)
                    return ref _registers[8];
                else throw new InvalidOperationException("Memory access violation");
            }
        }
    }
}
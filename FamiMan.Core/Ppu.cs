using FamiMan.Core.Interfaces;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;

namespace FamiMan.Core
{

    // Picture processing unit
    public class Ppu
    {
        private Bus _b;
        private Ram _r;

        private readonly byte[] _nametableRam = new byte[0x800];
        private readonly byte[] _paletteRam = new byte[0x20];

        public const ushort PPUCTRL     = 0x2000;
        private const ushort PPUMASK    = 0x2001;
        private const ushort PPUSTATUS  = 0x2002;
        private const ushort PPUADDR    = 0x2006;
        private const ushort PPUDATA    = 0x2007;

        public const ushort NAMETABLE_START = 0x2000;

        public Ppu(Bus b)
        {
            _b = b;
            _r = new Ram(16 * 1024);
            Register = new PPURegister
            {
                // Vblank always on
                PPUSTATUS = 0b10000000 //128
            };

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
        public PPURegister Register;

        public NametableMirroring Mirroring { get; set; }

        // Exposed so timing behavior can be learned and tested directly.
        public int Cycle { get; private set; }
        public int Scanline { get; private set; }
        public bool FrameComplete { get; private set; }

        /// <summary>
        /// Reads the PPU's separate $0000-$3FFF address space.
        /// This is intentionally separate from the CPU-visible registers at
        /// $2000-$2007 handled by ReadCpuRegister().
        /// </summary>
        public byte ReadPpuMemory(ushort address)
        {
            // We only have a 14bit address bus so $0000–$3FFF
            // After that it just wraps around to $0000 again. So we can just mask the address to 14 bits.
            address &= 0x3FFF;
            if (address <= 0x1FFF)
            {
                return _b.IO.CHRROM[address];
            }
            else if (address <= 0x3EFF)
            {
                int nametableAddress = GetNametableIndex(address);
                return _nametableRam[nametableAddress];
            }
            else if (address <= 0x3FFF)
            {
                int paletteAddress = (address - 0x3F00) % 0x20;
                // Special aliases for the background color and sprite color
                if (paletteAddress is 0x10 or 0x14 or 0x18 or 0x1C)
                    paletteAddress -= 0x10;
                return _paletteRam[paletteAddress];
            }
            else
            {
                throw new InvalidOperationException("Invalid memory address access in PPU");
            }
        }

        /// <summary>
        /// Writes the PPU's separate $0000-$3FFF address space.
        /// </summary>
        public void WritePpuMemory(ushort address, byte value)
        {
            /*
             *  $0000–$1FFF  CHR-ROM or CHR-RAM
                $2000–$3EFF  nametable memory and mirrors
                $3F00–$3FFF  palette memory and mirrors
            */
            address &= 0x3FFF;
            address = (ushort)(address % 0x4000);
            if (address <= 0x1FFF)
            {
                _b.IO.CHRROM[address] = value;
                return;
            }
            else if (address <= 0x3EFF)
            {
                int nametableAddress = GetNametableIndex(address);
                
                _nametableRam[nametableAddress] = value;
                return;
            }
            else if (address <= 0x3FFF)
            {
                int paletteAddress = (address - 0x3F00) % 0x20;
                // Special aliases for the background color and sprite color
                if (paletteAddress is 0x10 or 0x14 or 0x18 or 0x1C)
                    paletteAddress -= 0x10;
                _paletteRam[paletteAddress] = value;
                return;
            }
            else
            {
                throw new InvalidOperationException("Invalid memory address access in PPU");
            }
        }

        private int GetNametableIndex(ushort address)
        {
            // First collapse $3000-$3EFF onto $2000-$2EFF.
            int logicalOffset = (address - 0x2000) % 0x1000;

            // Each logical nametable occupies $400 bytes.
            // So which table and what offset within that table?
            int table = logicalOffset / 0x400;
            int offsetWithinTable = logicalOffset % 0x400;

            // We have two actual tables but four logical tables
            int physicalTable = Mirroring switch
            {
                NametableMirroring.Vertical => table % 2,   // 0 1 0 1
                NametableMirroring.Horizontal => table / 2, // 0 0 1 1
                _ => throw new InvalidOperationException()
            };

            // table 0 or 1 + the offset within that table gives us the final index into the nametable RAM.
            return physicalTable * 0x400 + offsetWithinTable;
        }

        private byte _ppuDataReadBuffer;

        public byte ReadCpuRegister(ushort index)
        {
            if (index == PPUSTATUS) 
            {
                _expectingAddressHighByte = true;
                byte result = Register.PPUSTATUS;
                Register.PPUSTATUS &= 0x7F;
                return result;
            }
            else if (index == PPUCTRL)
            {
                return Register.PPUCTRL;
            }
            else if (index == PPUMASK)
            {
                return Register.PPUMASK;
            }
            else if (index == PPUDATA)
            {
                byte result;
                if (_ppuAddress <= 0x3EFF)
                {
                     result = _ppuDataReadBuffer;
                    _ppuDataReadBuffer = ReadPpuMemory(_ppuAddress);
                }
                else
                {
                    result = ReadPpuMemory(_ppuAddress);
                }
                IncrementPpuAddress();
                return result;
            }
            else
            {
                throw new InvalidOperationException("Invalid memory address access in PPU");
            }
        }

        ushort _ppuAddress;
        bool _expectingAddressHighByte = true;

        public void WriteCpuRegister(ushort index, byte value)
        {
            if (index == PPUSTATUS) 
            { 
                Register.PPUSTATUS = value;
            }
            else if (index == PPUCTRL)
            {
                Register.PPUCTRL = value;
            }
            else if (index == PPUMASK)
            {
                Register.PPUMASK = value;
            }
            else if (index == PPUADDR)
            {
                WritePpuAddress(value);
            }
            else if (index == PPUDATA)
            {
                WritePpuMemory(_ppuAddress, value);
                IncrementPpuAddress();

            }
            else
            {
                throw new InvalidOperationException("Invalid memory address access in PPU");

            }
        }

        private void IncrementPpuAddress()
        {
            if ((Register.PPUCTRL & 0x04) != 0)
                _ppuAddress += 32;  // Write down column
            else
                _ppuAddress++;      // Write across row
        }

        private void WritePpuAddress(byte value)
        {
            if (_expectingAddressHighByte)
            {
                // Only six address bits are meaningful because PPU addresses are 14-bit.
                // We got high bytes so move the value into the high byte of the 16-bit address and clear the low byte.
                _ppuAddress = (ushort)((value & 0x3F) << 8);
                _expectingAddressHighByte = false;
            }
            else
            {
                // Bitwise OR to combine the high byte we already have with the low byte we just got.
                _ppuAddress |= value;
                _expectingAddressHighByte = true;
            }
        }

        /// <summary>
        /// Returns the NES palette value for one background pixel. This keeps
        /// tile decoding separate from the eventual framebuffer/SDL work.
        /// </summary>
        public byte GetBackgroundPixel(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the start of the CHR pattern table selected for background
        /// tiles by bit 4 of PPUCTRL.
        /// </summary>
        public ushort GetBackgroundPatternTableBase()
        {
            bool bit4IsSet = (Register.PPUCTRL & 0x10) != 0;
            return (ushort)((bit4IsSet ? 0x1000 : 0));
        }

        /// <summary>
        /// Returns the first CHR address belonging to a background tile.
        /// </summary>
        public ushort GetBackgroundTileAddress(byte tileNumber)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the tile number selected by the nametable for a screen pixel.
        /// </summary>
        public byte GetNametableTileNumber(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decodes the two CHR bitplanes into a color index from 0 through 3.
        /// The supplied coordinates are positions within an 8x8 tile.
        /// </summary>
        public byte GetTilePixelColorIndex(byte tileNumber, int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the background palette number selected by the attribute table.
        /// </summary>
        public byte GetBackgroundPaletteNumber(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resolves a background palette number and color index to an NES palette value.
        /// </summary>
        public byte GetBackgroundPaletteValue(byte paletteNumber, byte colorIndex)
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            Cycle++;
            if (Cycle > 340)
            {
                Cycle = 0;
                Scanline++;
                if (Scanline > 261)
                {
                    Scanline = 0;
                    FrameComplete = true;
                }
            }


            if (Scanline == 241 && Cycle == 1)
                Register.PPUSTATUS |= 0x80; // Set bit 7

            if (Scanline == 261 && Cycle == 1)
                Register.PPUSTATUS &= 0x7F; // Clear bit 7
        }
    }

    public enum NametableMirroring
    {
        Horizontal,
        Vertical
    }

    public class PPURegister
    {
        public PPURegister()
        {
            _registers = new byte[9];
        }

        private byte[] _registers { get; set; }


        public byte[] Registers
        {
            get
            {
                return _registers;
            }
        }

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

    }
}

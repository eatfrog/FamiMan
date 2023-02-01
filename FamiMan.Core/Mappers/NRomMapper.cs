using FamiMan.Core.Interfaces;
using System;

namespace FamiMan.Core.Mappers
{
    /*
     * This is the mapper used with the first NES games. It has not bank-switching capabilities so the game using it are pretty simple. 
     * It can have either one or two PRG-ROM of 16kb, that are mapped at ranges 0x8000-0xBFFF and 0xC000-0xFFFF of the CPU memory. 
     * It also has CHR-ROM which contains the tile and sprite data. This CHR-ROM is mapped to the PPU memory at addresses 0x0-0x2000. 
     * You can still have fun with games using this mapper: 
     * Donkey Kong
     * Mario Bros
     * Super Mario Bros
     * Arkanoid
     * The CPU cannot write to the PRG-ROM for this mapper. 
     */
    internal class NRomMapper : IMapper
    {
        private readonly Bus _b;
        private Ram Ram => _b.Ram;

        private Ppu PPU => _b.Ppu;

        private IO IO => _b.IO;

        private Apu APU => _b.Apu;
        
        private Ram _sram;
        public NRomMapper (Bus b)
        {
            _b = b;
            _sram = new Ram(8192);
        }

        // Maybe we should have separate Read and Write methods instead to limit access to read only addresses
        public ref byte GetByteAtAddress(ushort index)
        {
            // Ram = $0000 -$07FF
            // 8 blocks, 256 values each ex XX00 -> XXFF
            // 0000 to 00ff is zero page, faster ram
            // 0100 to 01ff is stack
            // 0200 to 07ff is actual ram
            // 08xx, 10xx, 18xx are mirrors
            if (index >= 0 && index <= 0x07FF)
                return ref Ram.AsSpan()[index];
            else if (index >= 0x800 && index <= 0xFFF)
                return ref Ram.AsSpan()[index - 0x800];
            else if (index >= 0x1000 && index <= 0x17FF)
                return ref Ram.AsSpan()[index - 0x1000];
            else if (index >= 0x1800 && index <= 0x1FFF)
                return ref Ram.AsSpan()[index - 0x1800];
            else if (index >= 0x2000 && index <= 0x2007)
                return ref GetPPUByteAtAddress(index);
            else if (index >= 0x4000 && index <= 0x4014)
                return ref APU.Registers[index];
            else if (index >= 0x6000 && index <= 0x7FFF)
                return ref _sram.AsSpan()[index - 0x6000];
            else if (index >= 0x8000 && index <= 0xFFFF)
            {
                if (IO.PRGROM.Length < index)
                {
                    int timesMirrored = index / IO.PRGROM.Length;
                    int realIndex = index - (IO.PRGROM.Length * timesMirrored);
                    return ref IO.PRGROM[realIndex + 1]; // FIXME: why +1?
                }

                return ref IO.PRGROM[index];
            }
            else
            {
                Console.WriteLine("Access to not implemented memory area: " + index.ToString("X"));
                return ref Ram.AsSpan()[0];
                //throw new NotImplementedException("Not done");
            }


            /*  $2000 - $2007         8 bytes       Input / Output registers
                $4000 - $401F         32 bytes      NES PPU Input / Output registers
                $6000 - $7FFF         8192 bytes    SRAM - Save Ram used to save data between game plays or Work ram, depending on mapper?
                $8000 - $BFFF         16384 bytes   PRG-ROM lower bank - executable code
                $C000 - $FFFF         16384 bytes   PRG-ROM upper bank - executable code
                $FFFA - $FFFB         2 bytes       Address of Non Maskable Interrupt (NMI) handler routine
                $FFFC - $FFFD         2 bytes       Address of Power on reset handler routine
                $FFFE - $FFFF         2 bytes       Address of Break (BRK instruction) handler routine
            */
        }

        public ref byte[] GetBytesAtAddress(ushort[] address)
        {
            throw new System.NotImplementedException();
        }

        public ref byte GetPPUByteAtAddress(ushort index)
        {

            if (index < 0x2000) // CHR-ROM
                return ref _b.IO.CHRROM[index];
            else if (index >= 0x2000 && index < 0x2008)
                return ref PPU.Register.Registers[index - 0x2000];
            else if (index == 0x4014) // OAMDMA
                return ref PPU.Register.Registers[8];

            else throw new InvalidOperationException("Memory access violation");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public class Bus
    {
        public Bus()
        {
            Ram = new Ram(2 * 1024);
            Cpu = new Cpu(this);
            Ppu = new Ppu(this);
        }

        public Ram Ram { get; set; }
        public Cpu Cpu { get; set; }
        public Ppu Ppu { get; set; }

        public ref byte this[ushort index]
        {
            get
            {
                // Ram = $0000 -$07FF
                // 8 blocks, 256 values each ex XX00 -> XXFF
                // 0000 to 00ff is zero page, faster ram
                // 0100 to 01ff is stack
                // 0200 to 07ff is actual ram
                // 08xx, 10xx, 18xx are mirrors
                if (index >= 0 && index < 0x7FF)
                    return ref Ram.AsSpan()[index];
                else
                    throw new NotImplementedException("Not done");

                /*  $2000 - $2007                 8 bytes                 Input / Output registers
                    $4000 - $401F                 32 bytes Input / Output registers
                    $6000 - $7FFF                 8192 bytes         SRAM - Save Ram used to save data between game plays.
                    $8000 - $BFFF                 16384 bytes         PRG-ROM lower bank - executable code
                    $C000 - $FFFF                 16384 bytes         PRG-ROM upper bank - executable code
                    $FFFA - $FFFB         2 bytes                 Address of Non Maskable Interrupt (NMI) handler routine
                    $FFFC - $FFFD         2 bytes                 Address of Power on reset handler routine
                    $FFFE - $FFFF                 2 bytes                 Address of Break (BRK instruction) handler routine
                */
            }
            //set
            //{
            //    if (index >= 0 && index < 0x7FF)
            //        Ram[index] = value;
            //    else
            //        throw new NotImplementedException("Not done");
            //}
        }
    }
}

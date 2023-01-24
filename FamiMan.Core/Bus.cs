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
            IO = new IO(this);
        }

        public void Clock()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            Cpu.Reset();

            // TODO
            //this[0x4017] = 0; // frame irq enabled
            //this[0x4015] = 0; // all channels disabled

            //for (ushort i = 0x4000; i < 0x4014; i++)
            //{
            //    this[i] = 0;
            //}

            var ram = Ram.AsSpan();
            for (int i = 0; i < ram.Length; i++)
                ram[i] = i % 2 == 0 ? (byte)0x00 : (byte)0xFF;

            // TODO: reset memory and interrupts etc
        }

        public Ram Ram { get; set; }
        public Cpu Cpu { get; set; }
        public Ppu Ppu { get; set; }

        public IO IO { get; set; }

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
                else if (index >= 0x800 && index <= 0xFFF)
                    return ref Ram.AsSpan()[index - 0x800];
                else if (index >= 0x1000 && index <= 0x17FF)
                    return ref Ram.AsSpan()[index - 0x1000];
                else if (index >= 0x1800 && index <= 0x1FFF)
                    return ref Ram.AsSpan()[index - 0x1800];
                else if (index == 0x2000)
                    return ref Ppu.Registers;
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
                    Console.WriteLine("Access to not implemented memory area");
                    return ref Ram.AsSpan()[0];
                    //throw new NotImplementedException("Not done");
                }


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

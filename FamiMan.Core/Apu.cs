using System;
using System.Net.Http.Headers;

namespace FamiMan.Core
{

    // Audio processing unit
    public class Apu
    {
        private Bus _b;
        private Ram _r;
        public Apu(Bus b)
        {
            _b = b;
            _r = new Ram(16 * 1024);
            Registers = new APURegister();
            /* The PPU addresses a 16kB space, $0000-3FFF, 
             * completely separate from the CPU's address bus. 
             * It is either directly accessed by the PPU itself, 
             * or via the CPU with memory mapped registers at $2006 and $2007.
             * The NES has 2kB of RAM dedicated to the PPU, 
             * normally mapped to the nametable address space from $2000-2FFF, 
             * but this can be rerouted through custom cartridge wiring. */
        }

        /*
         * $4000–$4003	Pulse 1	Timer, length counter, envelope, sweep
         * $4004–$4007	Pulse 2	Timer, length counter, envelope, sweep
         * $4008–$400B	Triangle	Timer, length counter, linear counter
         * $400C–$400F	Noise	Timer, length counter, envelope, linear feedback shift register
         * $4010–$4013	DMC	Timer, memory reader, sample buffer, output unit
         * $4015	    All	Channel enable and length counter status
         * $4017	    All	Frame counter
        */
        public APURegister Registers;
    }

    public class APURegister
    {
        public APURegister()
        {
            _registers = new byte[9];
        }

        private byte[] _registers { get; set; }
        public byte[] Pulse1Channel
        {
            set
            {
                _registers[0] = value[0]; // $4000
                _registers[1] = value[1]; // $4001
                _registers[2] = value[2]; // $4002
                _registers[3] = value[3]; // $4003
            }
        }

        public byte[] Pulse2Channel
        {
            set
            {
                _registers[4] = value[0]; // $4004
                _registers[5] = value[1]; // $4005
                _registers[6] = value[2]; // $4006
                _registers[7] = value[3]; // $4007
            }
        }

        public byte[] TriangleChannel
        {
            set
            {
                _registers[8]  = value[0]; // $4008
                // No $4009
                _registers[9] = value[1]; // $400A
                _registers[10] = value[2]; // $400B
            }
        }

        public byte[] NoiseChannel
        {
            set
            {
                _registers[11] = value[0]; // $400C
                // No $400D
                _registers[12] = value[1]; // $400E
                _registers[13] = value[2]; // $400F
            }
        }

        public byte[] DmcChannel
        {
            set
            {
                _registers[14] = value[0]; // $4010
                _registers[15] = value[1]; // $4011
                _registers[16] = value[2]; // $4012
                _registers[17] = value[3]; // $4013
            }
        }

        public byte DmcStatus
        {
            /* $4015	---D NT21	Control: DMC enable, length counter enables: noise, triangle, pulse 2, pulse 1 (write)
               $4015	IF-D NT21	Status: DMC interrupt, frame interrupt, length counter status: noise, triangle, pulse 2, pulse 1 (read)
             */
            get => _registers[18]; set => _registers[18] = value;
        }

        public byte FrameCounter
        {
            get => _registers[19]; set => _registers[19] = value;
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
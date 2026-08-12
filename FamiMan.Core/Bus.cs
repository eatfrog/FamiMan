using System;
using System.Collections.Generic;
using System.Text;
using FamiMan.Core.Interfaces;
using FamiMan.Core.Mappers;

namespace FamiMan.Core
{
    public class Bus
    {
        public Bus()
        {
            Ram = new Ram(2 * 1024);
            Cpu = new Cpu(this);
            Mapper = new NRomMapper(this);
            Ppu = new Ppu(this);
            IO = new IO(this);
            Apu = new Apu(this);
        }

        public void Clock()
        {
            Cpu.Tick();
            Ppu.Tick();
        }

        public void Reset()
        {
            Cpu.Reset();

            var ram = Ram.AsSpan();
            for (int i = 0; i < ram.Length; i++)
                ram[i] = i % 2 == 0 ? (byte)0x00 : (byte)0xFF;
            
        }

        public Ram Ram { get; set; }
        public Cpu Cpu { get; set; }
        public Ppu Ppu { get; set; }

        public Apu Apu { get; set; }

        public IO IO { get; set; }

        public IMapper Mapper { get; }

        public ref byte this[ushort index]
        {
            get => ref Mapper.GetByteAtAddress(index);
        }
    }
}

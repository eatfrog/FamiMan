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
            Ppu = new Ppu(this, Mapper);
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

        public Apu Apu { get; set; }

        public IO IO { get; set; }

        public IMapper Mapper { get; }

        public ref byte this[ushort index]
        {
            get => ref Mapper.GetByteAtAddress(index);
        }
    }
}

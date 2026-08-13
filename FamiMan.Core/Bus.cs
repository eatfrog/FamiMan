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

        /// <summary>
        /// Reads one byte from the CPU address space. Device-specific read side
        /// effects can now be implemented here instead of being hidden behind a ref.
        /// </summary>
        public byte Read(ushort address)
        {
            return this[address];
        }

        /// <summary>
        /// Writes one byte to the CPU address space. Device-specific write side
        /// effects can now be implemented here instead of being hidden behind a ref.
        /// </summary>
        public void Write(ushort address, byte value)
        {
            this[address] = value;
        }

        // Kept temporarily for test setup and code that has not yet moved to the
        // explicit CPU-bus API. The CPU itself no longer uses this ref indexer.
        public ref byte this[ushort index]
        {
            get => ref Mapper.GetByteAtAddress(index);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
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
            Controller1 = new NesController();
        }

        public void Clock()
        {
            Cpu.Tick();
            Ppu.Tick();
            Ppu.Tick();
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

        public NesController Controller1 { get; }

        public IO IO { get; set; }

        public IMapper Mapper { get; }


        private bool _controllerLatch;
        /// <summary>
        /// Reads one byte from the CPU address space. 
        /// </summary>
        public byte Read(ushort address)
        {
            if (address <= 0x1FFF)
                return Ram.AsSpan()[address % 0x0800];

            if (address <= 0x3FFF)
            {
                ushort registerAddress =
                    (ushort)(0x2000 + ((address - 0x2000) % 8));

                return Ppu.ReadCpuRegister(registerAddress);
            }

            // APU, controller, DMA...
            else if (address == 0x4016)
            {
                return Controller1.Read();
            }
            else if (address >= 0x6000)
                return Mapper.ReadCpu(address);

            return 0;
        }

        /// <summary>
        /// Writes one byte to the CPU address space. 
        /// </summary>
        public void Write(ushort address, byte value)
        {
            if (address <= 0x1FFF)
            {
                Ram.AsSpan()[address % 0x0800] = value;
                return;
            }

            else if (address <= 0x3FFF)
            {
                ushort registerAddress =
                    (ushort)(0x2000 + ((address - 0x2000) % 8));

                Ppu.WriteCpuRegister(registerAddress, value);
                return;
            }
            else if (address == Ppu.OAMADDR_ADDR)
            {
                // select destination address in OAM ie where to copy to
                Ppu.Register.OAMADDR = value;
                return;
            }
            else if (address == Ppu.OAMDMA_ADDR)
            {
                // Select source page in CPU ram ie where to copy from
                // AND start the DMA copy
                Ppu.Register.OAMDMA = value;

                ushort sourceStart = (ushort)(value << 8);
                byte oamStart = Ppu.Register.OAMADDR;
                for (int offset = 0; offset < 0x100; offset++)
                {
                    byte data = Read((ushort)(sourceStart + offset));
                    byte destination = unchecked((byte)(oamStart + offset));

                    Ppu.SetOamByte(destination, data);
                }
                return;
            }
            else if (address == 0x4016)
            {
                var previousValue = _controllerLatch;
                _controllerLatch = value == 1;
                if (previousValue && !_controllerLatch)
                {
                    Controller1.Latch();
                }
            }
            else if (address >= 0x6000)
            {
                Mapper.WriteCpu(address, value);
                return;
            }
        }

        /// <summary>
        /// Initializes a byte in the currently loaded PRG-ROM image. This is
        /// intended for test/debug setup; it is not a simulated CPU write.
        /// </summary>
        public void SetPrgRomByte(ushort cpuAddress, byte value)
        {
            if (cpuAddress < 0x8000)
                throw new ArgumentOutOfRangeException(
                    nameof(cpuAddress),
                    "PRG-ROM is mapped at CPU addresses $8000-$FFFF.");

            if (IO.PRGROM is null || IO.PRGROM.Length == 0)
                throw new InvalidOperationException(
                    "A PRG-ROM image must be loaded or allocated first.");

            int index = (cpuAddress - 0x8000) % IO.PRGROM.Length;
            IO.PRGROM[index] = value;
        }

        public byte this[ushort address]
        {
            get => Read(address);
            set => Write(address, value);
        }
    }
}

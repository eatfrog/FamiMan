using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// CPU-bus accesses to PPU registers are operations, not ordinary byte
    /// storage. These tests are the reason the CPU no longer uses the ref indexer.
    /// </summary>
    public class CpuBusPpuRegisterTests
    {
        [Fact]
        public void CpuBusWritesThroughPpuAddrAndPpuDataToPpuMemory()
        {
            var bus = CreateBusWithChr();

            // The CPU supplies PPU address $2000 as a high/low pair, then writes
            // the value through PPUDATA. Bus.Write must call the PPU register API.
            bus.Write(0x2006, 0x20);
            bus.Write(0x2006, 0x00);
            bus.Write(0x2007, 0x42);

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x2000));
        }

        [Fact]
        public void CpuBusReadOfPpuStatusClearsVblank()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.Register.PPUSTATUS = 0x80;

            byte status = bus.Read(0x2002);

            Assert.Equal(0x80, status & 0x80);
            Assert.Equal(0x00, bus.Ppu.Register.PPUSTATUS & 0x80);
        }

        [Fact]
        public void MirroredCpuPpuRegistersKeepTheirSideEffects()
        {
            var bus = CreateBusWithChr();

            // $200E and $200F mirror PPUADDR ($2006) and PPUDATA ($2007).
            bus.Write(0x200E, 0x20);
            bus.Write(0x200E, 0x00);
            bus.Write(0x200F, 0x37);

            Assert.Equal(0x37, bus.Ppu.ReadPpuMemory(0x2000));
        }

        [Fact]
        public void CpuProgramCanUploadBackgroundTileAndPaletteThroughPpuRegisters()
        {
            var bus = CreateBusWithChr();
            bus.Cpu.PC = 0x0200;

            // This is the same kind of CPU code a game uses: select a PPU
            // address with $2006, then upload a value through $2007.
            byte[] program =
            {
                LDA.Immediate.Opcode, 0x20,
                STA.Absolute.Opcode, 0x06, 0x20,
                LDA.Immediate.Opcode, 0x00,
                STA.Absolute.Opcode, 0x06, 0x20,
                LDA.Immediate.Opcode, 0x01,
                STA.Absolute.Opcode, 0x07, 0x20,

                LDA.Immediate.Opcode, 0x3F,
                STA.Absolute.Opcode, 0x06, 0x20,
                LDA.Immediate.Opcode, 0x01,
                STA.Absolute.Opcode, 0x06, 0x20,
                LDA.Immediate.Opcode, 0x21,
                STA.Absolute.Opcode, 0x07, 0x20
            };

            for (int i = 0; i < program.Length; i++)
                bus.Write((ushort)(0x0200 + i), program[i]);

            bus.Cpu.Tick(36);

            Assert.Equal(0x01, bus.Ppu.ReadPpuMemory(0x2000));
            Assert.Equal(0x21, bus.Ppu.ReadPpuMemory(0x3F01));
        }

        private static Bus CreateBusWithChr()
        {
            var bus = new Bus();
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }
    }
}

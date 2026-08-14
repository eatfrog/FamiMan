using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// Timing needed for a running game to produce and update colored frames.
    /// </summary>
    public class CpuPpuSynchronizationTests
    {
        [Fact]
        public void BusClocksPpuThreeTimesPerCpuCycle()
        {
            var bus = CreateBusWithPrgAndChr();
            bus.Cpu.PC = 0x0200;
            bus[0x0200] = NOP.NOP_EA.Opcode;

            bus.Clock();

            Assert.Equal(3, bus.Ppu.Cycle);
            Assert.Equal(1, bus.Cpu.Ticks);
        }

        [Fact]
        public void VblankRequestsNmiWhenEnabledInPpuCtrl()
        {
            var bus = CreateBusWithPrgAndChr();
            bus.Cpu.PC = 0x0200;
            bus.Cpu.SP = 0xFD;
            bus[0x0200] = NOP.NOP_EA.Opcode;
            bus.SetPrgRomByte(0xFFFA, 0x00);
            bus.SetPrgRomByte(0xFFFB, 0x90);
            bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, 0x80);

            for (int i = 0; i < 241 * 341 + 1; i++)
                bus.Ppu.Tick();

            bus.Cpu.Tick(7);

            Assert.Equal(0x9000, bus.Cpu.PC);
        }

        [Fact]
        public void CompletedFrameCanBeConsumedOnlyOnce()
        {
            var bus = CreateBusWithPrgAndChr();

            for (int i = 0; i < 262 * 341; i++)
                bus.Ppu.Tick();

            Assert.True(bus.Ppu.ConsumeFrameComplete());
            Assert.False(bus.Ppu.ConsumeFrameComplete());
        }

        private static Bus CreateBusWithPrgAndChr()
        {
            var bus = new Bus();
            bus.IO.PRGROM = new byte[32_768];
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }
    }
}

using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class LSRTests
    {
        [Theory]
        [InlineData(LSR.ZeroPage.Opcode, LSR.ZeroPage.Cycles, 0x0010, 0)]
        [InlineData(LSR.ZeroPage_X.Opcode, LSR.ZeroPage_X.Cycles, 0x0011, 1)]
        [InlineData(LSR.Absolute.Opcode, LSR.Absolute.Cycles, 0x0310, 0)]
        [InlineData(LSR.Absolute_X.Opcode, LSR.Absolute_X.Cycles, 0x0311, 1)]
        public void LSRMemoryShiftsMemoryAndLeavesAccumulatorAlone(byte opcode, int cycles, int targetAddress, byte x)
        {
            var bus = new Bus();
            var cpu = bus.Cpu;
            cpu.A = 0x55;
            cpu.X = x;
            bus.Ram[0] = opcode;
            bus.Ram[1] = 0x10;
            bus.Ram[2] = 0x03;
            bus.Ram[(ushort)targetAddress] = 0x6D;

            cpu.Tick(cycles);

            Assert.Equal(0x36, bus.Ram[(ushort)targetAddress]);
            Assert.Equal(0x55, cpu.A);
            Assert.True(cpu.P.Carry);
            Assert.False(cpu.P.Negative);
            Assert.False(cpu.P.Zero);
        }

        [Theory]
        [InlineData(0x6D, 0x36, true, false)]
        [InlineData(0x80, 0x40, false, false)]
        [InlineData(0x01, 0x00, true, true)]
        public void LSRAccumulatorShiftsAccumulator(byte value, byte expected, bool carry, bool zero)
        {
            var bus = new Bus();
            var cpu = bus.Cpu;
            bus.Ram[0] = LSR.Accumulator.Opcode;
            cpu.A = value;

            cpu.Tick(LSR.Accumulator.Cycles);

            Assert.Equal(expected, cpu.A);
            Assert.Equal(carry, cpu.P.Carry);
            Assert.False(cpu.P.Negative);
            Assert.Equal(zero, cpu.P.Zero);
        }
    }
}

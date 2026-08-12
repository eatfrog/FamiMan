using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class ASLTests
    {
        [Theory]
        [InlineData(ASL.ZeroPage.Opcode, ASL.ZeroPage.Cycles, 0x0010, 0)]
        [InlineData(ASL.ZeroPage_X.Opcode, ASL.ZeroPage_X.Cycles, 0x0011, 1)]
        [InlineData(ASL.Absolute.Opcode, ASL.Absolute.Cycles, 0x0310, 0)]
        [InlineData(ASL.Absolute_X.Opcode, ASL.Absolute_X.Cycles, 0x0311, 1)]
        public void ASLMemoryShiftsMemoryAndLeavesAccumulatorAlone(byte opcode, int cycles, int targetAddress, byte x)
        {
            var bus = new Bus();
            var cpu = bus.Cpu;
            cpu.A = 0x55;
            cpu.X = x;
            bus.Ram[0] = opcode;
            bus.Ram[1] = 0x10;
            bus.Ram[2] = 0x03;
            bus.Ram[(ushort)targetAddress] = 0x81;

            cpu.Tick(cycles);

            Assert.Equal(0x02, bus.Ram[(ushort)targetAddress]);
            Assert.Equal(0x55, cpu.A);
            Assert.True(cpu.P.Carry);
            Assert.False(cpu.P.Negative);
            Assert.False(cpu.P.Zero);
        }

        [Theory]
        [InlineData(0x6D, 0xDA, false, true, false)]
        [InlineData(0x80, 0x00, true, false, true)]
        public void ASLAccumulatorShiftsAccumulator(byte value, byte expected, bool carry, bool negative, bool zero)
        {
            var bus = new Bus();
            var cpu = bus.Cpu;
            bus.Ram[0] = ASL.Accumulator.Opcode;
            cpu.A = value;

            cpu.Tick(ASL.Accumulator.Cycles);

            Assert.Equal(expected, cpu.A);
            Assert.Equal(carry, cpu.P.Carry);
            Assert.Equal(negative, cpu.P.Negative);
            Assert.Equal(zero, cpu.P.Zero);
        }
    }
}

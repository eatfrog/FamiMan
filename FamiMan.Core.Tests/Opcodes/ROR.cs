using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class RORTests
    {
        [Theory]
        [InlineData(ROR.ZeroPage.Opcode, ROR.ZeroPage.Cycles, 0x0010, 0)]
        [InlineData(ROR.ZeroPage_X.Opcode, ROR.ZeroPage_X.Cycles, 0x0011, 1)]
        [InlineData(ROR.Absolute.Opcode, ROR.Absolute.Cycles, 0x0310, 0)]
        [InlineData(ROR.Absolute_X.Opcode, ROR.Absolute_X.Cycles, 0x0311, 1)]
        public void RORMemoryRotatesMemoryAndLeavesAccumulatorAlone(byte opcode, int cycles, int targetAddress, byte x)
        {
            var bus = new Bus();
            var cpu = bus.Cpu;
            cpu.A = 0x55;
            cpu.X = x;
            cpu.P.Carry = true;
            bus.Ram[0] = opcode;
            bus.Ram[1] = 0x10;
            bus.Ram[2] = 0x03;
            bus.Ram[(ushort)targetAddress] = 0x6D;

            cpu.Tick(cycles);

            Assert.Equal(0xB6, bus.Ram[(ushort)targetAddress]);
            Assert.Equal(0x55, cpu.A);
            Assert.True(cpu.P.Carry);
            Assert.True(cpu.P.Negative);
            Assert.False(cpu.P.Zero);
        }

        [Theory]
        [InlineData(0x6D, true, 0xB6, true, true, false)]
        [InlineData(0x80, false, 0x40, false, false, false)]
        [InlineData(0x01, false, 0x00, true, false, true)]
        public void RORAccumulatorRotatesThroughCarry(byte value, bool carryIn, byte expected, bool carryOut, bool negative, bool zero)
        {
            var bus = new Bus();
            var cpu = bus.Cpu;
            bus.Ram[0] = ROR.Accumulator.Opcode;
            cpu.A = value;
            cpu.P.Carry = carryIn;

            cpu.Tick(ROR.Accumulator.Cycles);

            Assert.Equal(expected, cpu.A);
            Assert.Equal(carryOut, cpu.P.Carry);
            Assert.Equal(negative, cpu.P.Negative);
            Assert.Equal(zero, cpu.P.Zero);
        }
    }
}

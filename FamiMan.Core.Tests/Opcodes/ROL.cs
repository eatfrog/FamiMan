using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class ROLTests
    {
        [Theory]
        [InlineData(ROL.ZeroPage.Opcode, ROL.ZeroPage.Cycles, 0x0010, 0)]
        [InlineData(ROL.ZeroPage_X.Opcode, ROL.ZeroPage_X.Cycles, 0x0011, 1)]
        [InlineData(ROL.Absolute.Opcode, ROL.Absolute.Cycles, 0x0310, 0)]
        [InlineData(ROL.Absolute_X.Opcode, ROL.Absolute_X.Cycles, 0x0311, 1)]
        public void ROLMemoryRotatesMemoryAndLeavesAccumulatorAlone(byte opcode, int cycles, int targetAddress, byte x)
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

            Assert.Equal(0xDB, bus.Ram[(ushort)targetAddress]);
            Assert.Equal(0x55, cpu.A);
            Assert.False(cpu.P.Carry);
            Assert.True(cpu.P.Negative);
            Assert.False(cpu.P.Zero);
        }

        [Theory]
        [InlineData(0x6D, true, 0xDB, false, true, false)]
        [InlineData(0x80, false, 0x00, true, false, true)]
        public void ROLAccumulatorRotatesThroughCarry(byte value, bool carryIn, byte expected, bool carryOut, bool negative, bool zero)
        {
            var bus = new Bus();
            var cpu = bus.Cpu;
            bus.Ram[0] = ROL.Accumulator.Opcode;
            cpu.A = value;
            cpu.P.Carry = carryIn;

            cpu.Tick(ROL.Accumulator.Cycles);

            Assert.Equal(expected, cpu.A);
            Assert.Equal(carryOut, cpu.P.Carry);
            Assert.Equal(negative, cpu.P.Negative);
            Assert.Equal(zero, cpu.P.Zero);
        }
    }
}

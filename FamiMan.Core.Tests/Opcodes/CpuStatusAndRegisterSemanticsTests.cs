using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class CpuStatusAndRegisterSemanticsTests
    {
        [Theory]
        [InlineData(AND.Immediate.Opcode, 0x01, true)]
        [InlineData(ORA.Immediate.Opcode, 0x01, true)]
        [InlineData(EOR.Immediate.Opcode, 0x01, true)]
        [InlineData(AND.Immediate.Opcode, 0x40, false)]
        [InlineData(ORA.Immediate.Opcode, 0x40, false)]
        [InlineData(EOR.Immediate.Opcode, 0x40, false)]
        public void LogicalInstructionsLeaveOverflowUnchanged(
            byte opcode,
            byte operand,
            bool initialOverflow)
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            cpu.A = 0xFF;
            cpu.P.Overflow = initialOverflow;
            bus[0] = opcode;
            bus[1] = operand;

            cpu.Tick(2);

            Assert.Equal(initialOverflow, cpu.P.Overflow);
        }

        [Theory]
        [InlineData(Registers.INX.Opcode, 0x7F, 0x80)]
        [InlineData(Registers.INY.Opcode, 0x7F, 0x80)]
        [InlineData(Registers.DEX.Opcode, 0x00, 0xFF)]
        [InlineData(Registers.DEY.Opcode, 0x81, 0x80)]
        public void IndexIncrementAndDecrementSetNegativeFromResult(
            byte opcode,
            byte initialValue,
            byte expectedValue)
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            bool usesX = opcode is Registers.INX.Opcode or Registers.DEX.Opcode;

            if (usesX)
                cpu.X = initialValue;
            else
                cpu.Y = initialValue;

            cpu.P.Negative = false;
            bus[0] = opcode;

            cpu.Tick(2);

            Assert.Equal(expectedValue, usesX ? cpu.X : cpu.Y);
            Assert.True(cpu.P.Negative);
            Assert.False(cpu.P.Zero);
        }

        [Theory]
        [InlineData(Registers.INX.Opcode)]
        [InlineData(Registers.INY.Opcode)]
        public void IndexIncrementClearsNegativeWhenResultWrapsToZero(byte opcode)
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            bool usesX = opcode == Registers.INX.Opcode;

            if (usesX)
                cpu.X = 0xFF;
            else
                cpu.Y = 0xFF;

            cpu.P.Negative = true;
            bus[0] = opcode;

            cpu.Tick(2);

            Assert.Equal(0, usesX ? cpu.X : cpu.Y);
            Assert.True(cpu.P.Zero);
            Assert.False(cpu.P.Negative);
        }

        [Theory]
        [InlineData(0xFF, 0xEF)]
        [InlineData(0x04, 0x24)]
        public void PLPIgnoresBreakAndKeepsUnusedStatusBitSet(byte stackedStatus, byte expectedStatus)
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            cpu.SP = 0xFC;
            bus[0x01FD] = stackedStatus;
            bus[0] = Stack.PLP.Opcode;

            cpu.Tick(Stack.PLP.Cycles);

            Assert.Equal(expectedStatus, cpu.P.AsByte());
        }

        [Fact]
        public void RTIKeepsUnusedStatusBitSet()
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            cpu.SP = 0xFC;
            bus[0x01FD] = 0x04;
            bus[0x01FE] = 0x34;
            bus[0x01FF] = 0x12;
            bus[0] = RTI.Implied.Opcode;

            cpu.Tick(RTI.Implied.Cycles);

            Assert.Equal(0x24, cpu.P.AsByte());
            Assert.Equal(0x1234, cpu.PC);
        }
    }
}

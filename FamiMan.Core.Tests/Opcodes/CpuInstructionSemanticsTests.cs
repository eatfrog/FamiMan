using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    /// <summary>
    /// Regression tests for CPU behavior that is currently known to be wrong.
    /// Each test describes one independent rule so it can be fixed and checked
    /// without having to diagnose a full ROM trace.
    /// </summary>
    public class CpuInstructionSemanticsTests
    {
        private readonly Bus _bus;
        private readonly Cpu _cpu;

        public CpuInstructionSemanticsTests()
        {
            _bus = new Bus();
            _cpu = _bus.Cpu;
        }

        [Fact]
        public void ReadingCarryFlagDoesNotClearIt()
        {
            _cpu.P.Carry = true;

            Assert.True(_cpu.P.Carry);
            Assert.True(_cpu.P.Carry);
        }

        [Fact]
        public void PHAWritesToStackPageAndThenDecrementsStackPointer()
        {
            _cpu.PC = 0x0200;
            _cpu.SP = 0xFD;
            _cpu.A = 0x42;
            _bus[0x0200] = Stack.PHA.Opcode;

            _cpu.Tick(Stack.PHA.Cycles);

            Assert.Equal(0x42, _bus[0x01FD]);
            Assert.Equal(0xFC, _cpu.SP);
        }

        [Fact]
        public void PLAPullsFromNextStackSlotInStackPage()
        {
            _cpu.PC = 0x0200;
            _cpu.SP = 0xFC;
            _bus[0x0200] = Stack.PLA.Opcode;
            _bus[0x01FD] = 0x80;

            _cpu.Tick(Stack.PLA.Cycles);

            Assert.Equal(0x80, _cpu.A);
            Assert.Equal(0xFD, _cpu.SP);
            Assert.True(_cpu.P.Negative);
            Assert.False(_cpu.P.Zero);
        }

        [Fact]
        public void ASLZeroPageWritesResultBackToMemoryAndLeavesAccumulatorAlone()
        {
            _cpu.PC = 0x0200;
            _cpu.A = 0x55;
            _bus[0x0200] = ASL.ZeroPage.Opcode;
            _bus[0x0201] = 0x10;
            _bus[0x0010] = 0x81;

            _cpu.Tick(ASL.ZeroPage.Cycles);

            Assert.Equal(0x02, _bus[0x0010]);
            Assert.Equal(0x55, _cpu.A);
            Assert.True(_cpu.P.Carry);
            Assert.False(_cpu.P.Negative);
            Assert.False(_cpu.P.Zero);
        }

        [Fact]
        public void BITTakesNegativeAndOverflowFromMemoryOperand()
        {
            _cpu.PC = 0x0200;
            _cpu.A = 0x00;
            _bus[0x0200] = BIT.ZeroPage.Opcode;
            _bus[0x0201] = 0x10;
            _bus[0x0010] = 0xC0;

            _cpu.Tick(BIT.ZeroPage.Cycles);

            Assert.True(_cpu.P.Zero);
            Assert.True(_cpu.P.Negative);
            Assert.True(_cpu.P.Overflow);
            Assert.Equal(0x00, _cpu.A);
        }

        [Fact]
        public void TAXUpdatesNegativeAndZeroFlags()
        {
            _cpu.PC = 0x0200;
            _cpu.A = 0x80;
            _cpu.P.Zero = true;
            _bus[0x0200] = Registers.TAX.Opcode;

            _cpu.Tick(Registers.TAX.Cycles);

            Assert.Equal(0x80, _cpu.X);
            Assert.True(_cpu.P.Negative);
            Assert.False(_cpu.P.Zero);
        }

        [Fact]
        public void INCUpdatesNegativeAndZeroFlags()
        {
            _cpu.PC = 0x0200;
            _cpu.P.Negative = true;
            _cpu.P.Zero = false;
            _bus[0x0200] = INC.ZeroPage.Opcode;
            _bus[0x0201] = 0x10;
            _bus[0x0010] = 0xFF;

            _cpu.Tick(INC.ZeroPage.Cycles);

            Assert.Equal(0x00, _bus[0x0010]);
            Assert.True(_cpu.P.Zero);
            Assert.False(_cpu.P.Negative);
        }

        [Fact]
        public void LDAZeroPageXWrapsWithinZeroPage()
        {
            _cpu.PC = 0x0200;
            _cpu.X = 0x01;
            _bus[0x0200] = LDA.ZeroPage_X.Opcode;
            _bus[0x0201] = 0xFF;
            _bus[0x0000] = 0x42;
            _bus[0x0100] = 0x99;

            _cpu.Tick(LDA.ZeroPage_X.Cycles);

            Assert.Equal(0x42, _cpu.A);
        }

        [Fact]
        public void LDXSetsNegativeFromLoadedXValue()
        {
            _cpu.PC = 0x0200;
            _cpu.Y = 0x00;
            _bus[0x0200] = LDX.Immediate.Opcode;
            _bus[0x0201] = 0x80;

            _cpu.Tick(LDX.Immediate.Cycles);

            Assert.Equal(0x80, _cpu.X);
            Assert.True(_cpu.P.Negative);
            Assert.False(_cpu.P.Zero);
        }
    }
}

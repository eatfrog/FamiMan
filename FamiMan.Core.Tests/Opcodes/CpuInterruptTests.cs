using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    /// <summary>
    /// Focused tests for CPU interrupt behavior.
    /// BRK, NMI, and IRQ all enter an interrupt handler, but BRK differs in the
    /// return address and in the copy of the status byte pushed to the stack.
    /// </summary>
    public class CpuInterruptTests
    {
        [Fact]
        public void BRKUsesSevenCycles()
        {
            Assert.Equal(7, Find(BRK.BRK_00.Opcode).Cycles);
        }

        [Fact]
        public void BRKPushesReturnStateAndLoadsIrqVector()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            cpu.P.Carry = true;
            bus[0x0200] = BRK.BRK_00.Opcode;
            bus.SetPrgRomByte(0xFFFE, 0x00);
            bus.SetPrgRomByte(0xFFFF, 0x90);

            cpu.Tick(7);

            Assert.Equal(0x9000, cpu.PC);
            Assert.Equal(0xFA, cpu.SP);
            Assert.Equal(0x02, bus[0x01FD]); // Return address high byte.
            Assert.Equal(0x02, bus[0x01FC]); // BRK returns to PC + 2.
            Assert.Equal(0x31, bus[0x01FB]); // N V 1 B D I Z C: unused, B, and C set.
            Assert.True(cpu.P.InterruptsDisabled);
        }

        [Fact]
        public void BRKDoesNotExecuteHandlerInstructionBeforeSevenCyclesFinish()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            bus[0x0200] = BRK.BRK_00.Opcode;
            bus.SetPrgRomByte(0xFFFE, 0x00);
            bus.SetPrgRomByte(0xFFFF, 0x90);
            bus.SetPrgRomByte(0x9000, LDA.Immediate.Opcode);
            bus.SetPrgRomByte(0x9001, 0x42);

            cpu.Tick(7);

            Assert.Equal(0x9000, cpu.PC);
            Assert.Equal(0x00, cpu.A);

            cpu.Tick(LDA.Immediate.Cycles);

            Assert.Equal(0x42, cpu.A);
            Assert.Equal(0x9002, cpu.PC);
        }

        [Fact]
        public void BRKSetsBreakOnlyInStackedStatusCopy()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            bus[0x0200] = BRK.BRK_00.Opcode;
            bus.SetPrgRomByte(0xFFFE, 0x00);
            bus.SetPrgRomByte(0xFFFF, 0x90);

            cpu.Tick(7);

            Assert.NotEqual(0, bus[0x01FB] & 0x10);
            Assert.False(cpu.P.Break);
        }

        [Fact]
        public void BRKCanBeExecutedAgainAfterRti()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            bus[0x0200] = BRK.BRK_00.Opcode;
            bus[0x0202] = BRK.BRK_00.Opcode;
            bus.SetPrgRomByte(0xFFFE, 0x00);
            bus.SetPrgRomByte(0xFFFF, 0x90);
            bus.SetPrgRomByte(0x9000, RTI.Implied.Opcode);

            cpu.Tick(7 + RTI.Implied.Cycles + 7);

            Assert.Equal(0x9000, cpu.PC);
            Assert.Equal(0xFA, cpu.SP);
            Assert.Equal(0x02, bus[0x01FD]);
            Assert.Equal(0x04, bus[0x01FC]);
        }

        [Fact]
        public void NmiPushesCurrentStateAndLoadsNmiVector()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            cpu.P.Carry = true;
            cpu.P.InterruptsDisabled = true; // NMI cannot be masked by the I flag.
            bus[0x0200] = NOP.NOP_EA.Opcode;
            bus.SetPrgRomByte(0xFFFA, 0x00);
            bus.SetPrgRomByte(0xFFFB, 0x90);

            cpu.RequestInterrupt(InterruptType.NMI);
            cpu.Tick(7);

            Assert.Equal(0x9000, cpu.PC);
            Assert.Equal(0xFA, cpu.SP);
            Assert.Equal(0x02, bus[0x01FD]);
            Assert.Equal(0x00, bus[0x01FC]); // Hardware interrupts resume at the current PC.
            Assert.Equal(0x25, bus[0x01FB]); // Unused, I, and C set; B clear.
        }

        [Fact]
        public void IrqPushesCurrentStateAndLoadsIrqVectorWhenEnabled()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            cpu.P.Carry = true;
            cpu.P.InterruptsDisabled = false;
            bus[0x0200] = NOP.NOP_EA.Opcode;
            bus.SetPrgRomByte(0xFFFE, 0x34);
            bus.SetPrgRomByte(0xFFFF, 0x12);

            cpu.RequestInterrupt(InterruptType.IRQ);
            cpu.Tick(7);

            Assert.Equal(0x1234, cpu.PC);
            Assert.Equal(0xFA, cpu.SP);
            Assert.Equal(0x02, bus[0x01FD]);
            Assert.Equal(0x00, bus[0x01FC]);
            Assert.Equal(0x21, bus[0x01FB]); // Unused and C set; B clear.
            Assert.True(cpu.P.InterruptsDisabled);
        }

        [Fact]
        public void IrqDoesNotInterruptWhenInterruptDisableFlagIsSet()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            cpu.P.InterruptsDisabled = true;
            bus[0x0200] = LDA.Immediate.Opcode;
            bus[0x0201] = 0x42;
            bus.SetPrgRomByte(0xFFFE, 0x34);
            bus.SetPrgRomByte(0xFFFF, 0x12);

            cpu.RequestInterrupt(InterruptType.IRQ);
            cpu.Tick(LDA.Immediate.Cycles);

            Assert.Equal(0x42, cpu.A);
            Assert.Equal(0x0202, cpu.PC);
            Assert.Equal(0xFD, cpu.SP);
        }

        [Fact]
        public void PendingNmiWaitsUntilCurrentInstructionFinishes()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            bus[0x0200] = LDA.Immediate.Opcode;
            bus[0x0201] = 0x42;
            bus[0x0202] = NOP.NOP_EA.Opcode;
            bus.SetPrgRomByte(0xFFFA, 0x00);
            bus.SetPrgRomByte(0xFFFB, 0x90);

            cpu.Tick();
            cpu.RequestInterrupt(InterruptType.NMI);
            cpu.Tick();

            Assert.Equal(0x42, cpu.A);
            Assert.Equal(0x0202, cpu.PC);

            cpu.Tick(7);

            Assert.Equal(0x9000, cpu.PC);
            Assert.Equal(0x02, bus[0x01FD]);
            Assert.Equal(0x02, bus[0x01FC]);
        }

        [Fact]
        public void NmiDoesNotDiscardAnIrqThatWasAlsoPending()
        {
            var bus = CreateBusWithPrgRom();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x0200;
            cpu.SP = 0xFD;
            cpu.P.InterruptsDisabled = false;
            // Keep normal execution at $0200. This prevents an accidental BRK
            // in zero-filled memory from reaching the shared IRQ/BRK vector.
            bus[0x0200] = JMP.Absolute.Opcode;
            bus[0x0201] = 0x00;
            bus[0x0202] = 0x02;
            bus.SetPrgRomByte(0xFFFA, 0x00);
            bus.SetPrgRomByte(0xFFFB, 0x90);
            bus.SetPrgRomByte(0xFFFE, 0x00);
            bus.SetPrgRomByte(0xFFFF, 0xA0);
            bus.SetPrgRomByte(0x9000, RTI.Implied.Opcode);

            cpu.RequestInterrupt(InterruptType.IRQ);
            cpu.RequestInterrupt(InterruptType.NMI);

            // NMI has priority, then RTI restores the original status and PC.
            cpu.Tick(7 + RTI.Implied.Cycles);

            Assert.Equal(0x0200, cpu.PC);
            Assert.False(cpu.P.InterruptsDisabled);

            // The IRQ request must still be pending and is accepted next.
            cpu.Tick(7);

            Assert.Equal(0xA000, cpu.PC);
            Assert.Equal(0xFA, cpu.SP);
        }

        private static Bus CreateBusWithPrgRom()
        {
            var bus = new Bus();
            bus.IO.PRGROM = new byte[32_768];
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }

    }
}

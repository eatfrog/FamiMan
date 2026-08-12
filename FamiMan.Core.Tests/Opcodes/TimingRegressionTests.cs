using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class TimingRegressionTests
    {
        [Fact]
        public void OfficialNopTakesTwoCycles()
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            bus[0] = NOP.NOP_EA.Opcode;

            cpu.Tick();

            Assert.Equal(0, cpu.PC);

            cpu.Tick();

            Assert.Equal(1, cpu.PC);
        }

        [Theory]
        [InlineData(Branches.BCC.Opcode)]
        [InlineData(Branches.BCS.Opcode)]
        [InlineData(Branches.BEQ.Opcode)]
        [InlineData(Branches.BNE.Opcode)]
        [InlineData(Branches.BMI.Opcode)]
        [InlineData(Branches.BPL.Opcode)]
        [InlineData(Branches.BVC.Opcode)]
        [InlineData(Branches.BVS.Opcode)]
        public void TakenBranchTakesThreeCycles(byte opcode)
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            SetConditionForTakenBranch(cpu, opcode);
            bus[0] = opcode;
            bus[1] = 0x02;

            cpu.Tick(2);

            Assert.Equal(0, cpu.PC);

            cpu.Tick();

            Assert.Equal(4, cpu.PC);
        }

        [Fact]
        public void TakenBranchAcrossPageBoundaryTakesFourCycles()
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            cpu.PC = 0x00FD;
            cpu.P.Zero = false;
            bus[0x00FD] = Branches.BNE.Opcode;
            bus[0x00FE] = 0x01;

            cpu.Tick(3);

            Assert.Equal(0x00FD, cpu.PC);

            cpu.Tick();

            Assert.Equal(0x0100, cpu.PC);
        }

        [Theory]
        [InlineData(STA.Absolute_X.Opcode, 5)]
        [InlineData(STA.Absolute_Y.Opcode, 5)]
        [InlineData(STA.IndirectIndexed.Opcode, 6)]
        public void IndexedStoresUseTheirFixed6502CycleCounts(byte opcode, int expectedCycles)
        {
            Assert.Equal(expectedCycles, Find(opcode).Cycles);
        }

        [Theory]
        [InlineData(ORA.ZeroPage_X.Opcode)]
        [InlineData(AND.ZeroPage_X.Opcode)]
        [InlineData(EOR.ZeroPage_X.Opcode)]
        public void ZeroPageIndexedLogicalInstructionTakesFourCycles(byte opcode)
        {
            Assert.Equal(4, Find(opcode).Cycles);
        }

        [Theory]
        [InlineData(LDA.IndirectIndexed.Opcode, 6)]
        [InlineData(LDA.Absolute_Y.Opcode, 5)]
        [InlineData(LDY.Absolute_X.Opcode, 5)]
        [InlineData(LDA.Absolute_X.Opcode, 5)]
        [InlineData(LDX.Absolute_Y.Opcode, 5)]
        public void IndexedReadAddsOneCycleWhenPageBoundaryIsCrossed(
            byte opcode,
            int expectedCycles)
        {
            var bus = new Bus();
            Cpu cpu = bus.Cpu;
            bus[0] = opcode;

            if (opcode == LDA.IndirectIndexed.Opcode)
            {
                cpu.Y = 1;
                bus[1] = 0x10;
                bus[0x0010] = 0xFF;
                bus[0x0011] = 0x00;
            }
            else
            {
                if (opcode is LDY.Absolute_X.Opcode or LDA.Absolute_X.Opcode)
                    cpu.X = 1;
                else
                    cpu.Y = 1;

                bus[1] = 0xFF;
                bus[2] = 0x00;
            }

            bus[0x0100] = 0x42;

            cpu.Tick(expectedCycles - 1);

            Assert.Equal(0, cpu.PC);
            Assert.Equal(0, GetDestinationRegister(cpu, opcode));

            cpu.Tick();

            Assert.Equal(0x42, GetDestinationRegister(cpu, opcode));
        }

        private static void SetConditionForTakenBranch(Cpu cpu, byte opcode)
        {
            if (opcode == Branches.BCS.Opcode)
                cpu.P.Carry = true;
            else if (opcode == Branches.BEQ.Opcode)
                cpu.P.Zero = true;
            else if (opcode == Branches.BMI.Opcode)
                cpu.P.Negative = true;
            else if (opcode == Branches.BVS.Opcode)
                cpu.P.Overflow = true;
        }

        private static byte GetDestinationRegister(Cpu cpu, byte opcode)
        {
            if (opcode == LDY.Absolute_X.Opcode)
                return cpu.Y;

            if (opcode == LDX.Absolute_Y.Opcode)
                return cpu.X;

            return cpu.A;
        }
    }
}

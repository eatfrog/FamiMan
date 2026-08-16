using FamiMan.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class OpcodeTests
    {
        private Bus _b;
        private Cpu _c;
        public OpcodeTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void OpcodePopulatesLookupTable()
        {
            Opcode opcode = Find(0x61);
            Assert.Equal(Instruction.ADC, opcode.Instruction);
            Assert.Equal(AddressingMode.IndexedIndirect, opcode.AddressingMode);
            Assert.Equal(2, opcode.Length);
            Assert.Equal(typeof(ADC.IndexedIndirect), opcode.BackingType);
            Assert.Equal(0x61, opcode.BackingType.GetOpcode());
            Assert.Equal(2, opcode.BackingType.GetLength());
            Assert.Equal(6, opcode.BackingType.GetCycles());
        }

        [Theory]
        [InlineData(LDA.ZeroPage_X.Opcode, AddressingMode.ZeroPageX)]
        [InlineData(LDA.Absolute_X.Opcode, AddressingMode.AbsoluteX)]
        [InlineData(LDX.ZeroPage_Y.Opcode, AddressingMode.ZeroPageY)]
        [InlineData(LDA.Absolute_Y.Opcode, AddressingMode.AbsoluteY)]
        [InlineData(JMP.Indirect.Opcode, AddressingMode.Indirect)]
        [InlineData(Branches.BNE.Opcode, AddressingMode.Relative)]
        [InlineData(Registers.TAX.Opcode, AddressingMode.Implied)]
        public void OpcodeHasTypeSafeAddressingMode(byte opcodeValue, AddressingMode expected)
        {
            Assert.Equal(expected, Find(opcodeValue).AddressingMode);
        }

        [Fact]
        public void KilInstructionThrows()
        {
            byte i = 0;
            _b.Ram[i++] = KIL.KIL_02.Opcode;
            Assert.Throws<CpuException>(() => _c.Tick());  // Tick should throw exception
        }

        [Fact]
        public void NopDoesNothing()
        {
            byte i = 0;
            _b.Ram[i++] = NOP.NOP_04.Opcode;
            var pc = _c.PC;
            _c.Tick();
            Assert.Equal(pc + Find(NOP.NOP_04.Opcode).Length, _c.PC);

        }
    }
}

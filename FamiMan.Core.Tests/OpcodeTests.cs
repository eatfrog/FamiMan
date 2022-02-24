using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
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
            Type opcode = Opcodes.Find(0x61);
            Assert.Equal(typeof(Opcodes.ADC.IndexedIndirect), opcode);
            Assert.Equal(0x61, opcode.GetOpcode());
            Assert.Equal(2, opcode.GetLength());
            Assert.Equal(6, opcode.GetCycles());
        }

        [Fact]
        public void KilInstructionThrows()
        {
            byte i = 0;
            _b.Ram[i++] = Opcodes.KIL.KIL_02.Opcode;
            Assert.Throws<CpuException>(() => _c.Tick());  // Tick should throw exception
        }

        [Fact]
        public void NopDoesNothing()
        {
            byte i = 0;
            _b.Ram[i++] = Opcodes.NOP.NOP_04.Opcode;
            var pc = _c.PC;
            _c.Tick();
            Assert.Equal(pc + 1, _c.PC);

        }
    }
}

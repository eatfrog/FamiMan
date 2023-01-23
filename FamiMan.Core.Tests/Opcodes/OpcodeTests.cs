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
            Assert.Equal(typeof(ADC.IndexedIndirect), opcode.BackingType);
            Assert.Equal(0x61, opcode.BackingType.GetOpcode());
            Assert.Equal(2, opcode.BackingType.GetLength());
            Assert.Equal(6, opcode.BackingType.GetCycles());
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
            Assert.Equal(pc + 1, _c.PC);

        }
    }
}

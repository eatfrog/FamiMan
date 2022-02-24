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
        [Fact]
        public void OpcodePopulatesLookupTable()
        {
            Type opcode = Opcodes.Find(0x61);
            Assert.Equal(typeof(Opcodes.ADC.Indirect_X), opcode);
            Assert.Equal(0x61, opcode.GetOpcode());
            Assert.Equal(2, opcode.GetLength());
            Assert.Equal(6, opcode.GetCycles());
        }
    }
}

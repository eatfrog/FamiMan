using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class FlagsTests
    {
        private Bus _b;
        private Cpu _c;
        public FlagsTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void CLC_0x18()
        {
            _c.P.Carry = true;
            byte i = 0;
            _b.Ram[i++] = Flags.CLC.Opcode;
            _c.Tick(Flags.CLC.Cycles);
            Assert.False(_c.P.Carry);
        }

        [Fact]
        public void SED_0xF8()
        {
            _c.P.Carry = true;
            byte i = 0;
            _b.Ram[i++] = Flags.SED.Opcode;
            _c.Tick(Flags.SED.Cycles);
            Assert.True(_c.P.Decimal);
        }

        // TODO: the rest maybe..
    }
}

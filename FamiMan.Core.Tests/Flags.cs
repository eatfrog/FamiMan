using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class Flags
    {
        private Bus _b;
        private Cpu _c;
        public Flags()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void CLC_0x18()
        {
            _c.P.Carry = true;
            byte i = 0;
            _b.Ram[i++] = Opcodes.Flags.CLC.Opcode;
            _c.Tick(Opcodes.Flags.CLC.Cycles);
            Assert.False(_c.P.Carry);
        }

        // TODO: the rest maybe..
    }
}

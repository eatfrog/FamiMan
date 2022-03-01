using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class BIT
    {
        private Bus _b;
        private Cpu _c;
        public BIT()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void BIT_0x24_ZeroPage()
        {
            byte i = 0;
            _c.A = 0xA0;
            _b.Ram[i++] = Opcodes.BIT.ZeroPage.Opcode;    // BIT
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 0xA0;
            _c.Tick(Opcodes.AND.ZeroPage.Cycles);

            Assert.False(_c.P.Zero);
            Assert.True(_c.P.Overflow);
            Assert.True(_c.P.Negative);
            Assert.Equal(160, _c.A);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests
{
    public class CPX_CPY
    {
        private Bus _b;
        private Cpu _c;

        public CPX_CPY()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void CPX_0xE0_Immediate()
        {
            byte i = 0;
            _b.Ram[i++] = CPX.Immediate.Opcode;
            _b.Ram[i++] = 0x10;
            _c.A = 6;
            _c.X = 0x10;
            _c.Tick(CPX.Immediate.Cycles);
            Assert.True(_c.P.Zero);
            Assert.Equal(6, _c.A);
        }

        [Fact]
        public void CPY_0xC0_Immediate()
        {
            byte i = 0;
            _b.Ram[i++] = CPY.Immediate.Opcode;
            _b.Ram[i++] = 0x10;
            _c.A = 6;
            _c.Y = 0x10;
            _c.X = 0x09;
            _c.Tick(CPY.Immediate.Cycles);
            Assert.True(_c.P.Zero);
            Assert.Equal(6, _c.A);
        }

    }
}

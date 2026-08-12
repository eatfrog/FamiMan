using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class Programs
    {
        private Bus _b;
        private Cpu _c;
        private IO _io;

        public Programs()
        {
            _b = new Bus();
            _c = new Cpu(_b);
            _io = new IO(_b);
        }

        [Fact]
        public void TestProgram1()
        {
            _io.LoadProgramFromHexString("A9448544E64400", 0);
            _c.Tick(10);
            Assert.Equal(0x45, _b[0x044]);
        }

        [Fact]
        public void TestProgram2()
        {
            _c.SP = 0x20;
            // LDA #$44
            // STA $44
            // INC $44
            // JSR $0E
            // NOP NOP NOP NOP
            // BRK
            // LDX #45
            // RTS
            _io.LoadProgramFromHexString("A9 44 85 44 E6 44 20 0E 00 EA EA EA EA 00 A2 2D 60", 0);
            _c.Tick(32);
            Assert.Equal(45, _c.X);
            Assert.Equal(13, _c.PC);
        }
    }
}

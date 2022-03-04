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
            _c.Tick(1000);
            Assert.Equal(0x45, _b[0x044]);
        }

        [Fact]
        public void TestProgram2()
        {
            _c.S = 0x20;
            _io.LoadProgramFromHexString("A9448544E644200E00EAEAEAEA00A22D60", 0);
            _c.Tick(1000);
            Assert.Equal(45, _c.X);
            Assert.Equal(13, _c.PC);
           
        }
    }
}

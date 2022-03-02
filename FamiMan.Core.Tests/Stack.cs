using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class Stack
    {
        private Bus _b;
        private Cpu _c;
        public Stack()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void TXS_0x9A()
        {
            byte i = 0;
            _c.X = 0x05;
            _b.Ram[i++] = Opcodes.Stack.TXS.Opcode;
            _c.Tick(Opcodes.Stack.TXS.Cycles);
            Assert.Equal(0x05, _c.S);
        }

        [Fact]
        public void TSX_0xBA()
        {
            byte i = 0;
            _c.S = 0x05;
            _b.Ram[i++] = Opcodes.Stack.TSX.Opcode;
            _c.Tick(Opcodes.Stack.TSX.Cycles);
            Assert.Equal(0x05, _c.X);
        }
    }
}

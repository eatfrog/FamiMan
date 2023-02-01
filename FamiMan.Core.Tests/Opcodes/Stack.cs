using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class StackTests
    {
        private Bus _b;
        private Cpu _c;
        public StackTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void TXS_0x9A()
        {
            byte i = 0;
            _c.X = 0x05;
            _b.Ram[i++] = Stack.TXS.Opcode;
            _c.Tick(Stack.TXS.Cycles);
            Assert.Equal(0x05, _c.SP);
        }

        [Fact]
        public void TSX_0xBA()
        {
            byte i = 0;
            _c.SP = 0x05;
            _b.Ram[i++] = Stack.TSX.Opcode;
            _c.Tick(Stack.TSX.Cycles);
            Assert.Equal(0x05, _c.X);
        }

        [Fact]
        public void PHA_0x48()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.SP = 0x04;
            _b.Ram[i++] = Stack.PHA.Opcode;
            _c.Tick(Stack.PHA.Cycles);
            Assert.Equal(0x05, _b.Ram[0x04]);
            Assert.Equal(0x03, _c.SP);
        }

        [Fact]
        public void PLA_0x68()
        {
            byte i = 0;
            _c.SP = 0x10;
            _b.Ram[_c.SP] = 0xBC;
            _b.Ram[i++] = Stack.PLA.Opcode;
            _c.Tick(Stack.PLA.Cycles);
            Assert.Equal(0xBC, _c.A);
            Assert.True(_c.P.Negative);
            Assert.Equal(0x11, _c.SP);
        }

        [Fact]
        public void PHP_0x08()
        {
            byte i = 0;
            _c.P.Negative = true;
            _c.P.Zero = true;
            _c.SP = 0x04;
            _b.Ram[i++] = Stack.PHP.Opcode;
            _c.Tick(Stack.PHP.Cycles);
            Assert.Equal(0x82, _b.Ram[0x04]);
            Assert.Equal(0x03, _c.SP);
        }

        [Fact]
        public void PLP_0x28()
        {
            byte i = 0;
            _c.SP = 0x10;
            _b.Ram[_c.SP] = 0x82;
            _b.Ram[i++] = Stack.PLP.Opcode;
            _c.Tick(Stack.PLP.Cycles);
            Assert.True(_c.P.Negative);
            Assert.True(_c.P.Zero);
            Assert.Equal(0x11, _c.SP);
        }
    }
}

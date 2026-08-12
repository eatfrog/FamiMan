using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class RegistersTests
    {
        private Bus _b;
        private Cpu _c;
        public RegistersTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void TAX_0xAA()
        {
            byte i = 0;
            _c.A = 0x80;
            _b.Ram[i++] = Registers.TAX.Opcode;
            _c.Tick(Registers.TAX.Cycles);

            Assert.Equal(0x80, _c.X);
            Assert.True(_c.P.Negative);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void TXA_0x8A()
        {
            byte i = 0;
            _c.X = 0;
            _b.Ram[i++] = Registers.TXA.Opcode;
            _c.Tick(Registers.TXA.Cycles);

            Assert.Equal(0, _c.A);
            Assert.True(_c.P.Zero);
            Assert.False(_c.P.Negative);
        }

        [Fact]
        public void INX_0xE8()
        {
            byte i = 0;
            _c.X = 0x14;
            _b.Ram[i++] = Registers.INX.Opcode;
            _c.Tick(Registers.INX.Cycles);

            Assert.Equal(0x15, _c.X);

            _c.X = 0xFF;
            _b.Ram[i++] = Registers.INX.Opcode;
            _c.Tick(Registers.INX.Cycles);

            Assert.Equal(0x00, _c.X);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void DEX_0xCA()
        {
            byte i = 0;
            _c.X = 0x14;
            _b.Ram[i++] = Registers.DEX.Opcode;
            _c.Tick(Registers.DEX.Cycles);

            Assert.Equal(0x13, _c.X);

            _c.X = 0x01;
            _b.Ram[i++] = Registers.DEX.Opcode;
            _c.Tick(Registers.DEX.Cycles);

            Assert.Equal(0x00, _c.X);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void TAY_0xA8()
        {
            byte i = 0;
            _c.A = 0;
            _b.Ram[i++] = Registers.TAY.Opcode;
            _c.Tick(Registers.TAY.Cycles);

            Assert.Equal(0, _c.Y);
            Assert.True(_c.P.Zero);
            Assert.False(_c.P.Negative);
        }

        [Fact]
        public void TYA_0x98()
        {
            byte i = 0;
            _c.Y = 0x80;
            _b.Ram[i++] = Registers.TYA.Opcode;
            _c.Tick(Registers.TYA.Cycles);

            Assert.Equal(0x80, _c.A);
            Assert.True(_c.P.Negative);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void INY_0xC8()
        {
            byte i = 0;
            _c.Y = 0xFF;
            _b.Ram[i++] = Registers.INY.Opcode;
            _c.Tick(Registers.INY.Cycles);

            Assert.Equal(0, _c.Y);
            Assert.True(_c.P.Zero);
            Assert.False(_c.P.Negative);
        }

        [Fact]
        public void DEY_0x88()
        {
            byte i = 0;
            _c.Y = 0;
            _b.Ram[i++] = Registers.DEY.Opcode;
            _c.Tick(Registers.DEY.Cycles);

            Assert.Equal(0xFF, _c.Y);
            Assert.True(_c.P.Negative);
            Assert.False(_c.P.Zero);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class Registers
    {
        private Bus _b;
        private Cpu _c;
        public Registers()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void TAX_0xAA()
        {
            byte i = 0;
            _c.A = 0x14;
            _b.Ram[i++] = Opcodes.Registers.TAX.Opcode;
            _c.Tick(Opcodes.Registers.TAX.Cycles);

            Assert.Equal(0x14, _c.X);
        }

        [Fact]
        public void TXA_0x8A()
        {
            byte i = 0;
            _c.X = 0x14;
            _b.Ram[i++] = Opcodes.Registers.TXA.Opcode;
            _c.Tick(Opcodes.Registers.TXA.Cycles);

            Assert.Equal(0x14, _c.A);
        }

        [Fact]
        public void INX_0xE8()
        {
            byte i = 0;
            _c.X = 0x14;
            _b.Ram[i++] = Opcodes.Registers.INX.Opcode;
            _c.Tick(Opcodes.Registers.INX.Cycles);

            Assert.Equal(0x15, _c.X);
        }

        [Fact]
        public void DEX_0xCA()
        {
            byte i = 0;
            _c.X = 0x14;
            _b.Ram[i++] = Opcodes.Registers.DEX.Opcode;
            _c.Tick(Opcodes.Registers.DEX.Cycles);

            Assert.Equal(0x13, _c.X);
        }

        [Fact]
        public void TAY_0xA8()
        {
            byte i = 0;
            _c.A = 0x14;
            _b.Ram[i++] = Opcodes.Registers.TAY.Opcode;
            _c.Tick(Opcodes.Registers.TAY.Cycles);

            Assert.Equal(0x14, _c.Y);
        }

        [Fact]
        public void TYA_0x98()
        {
            byte i = 0;
            _c.Y = 0x14;
            _b.Ram[i++] = Opcodes.Registers.TYA.Opcode;
            _c.Tick(Opcodes.Registers.TYA.Cycles);

            Assert.Equal(0x14, _c.A);
        }

        [Fact]
        public void INY_0xC8()
        {
            byte i = 0;
            _c.Y = 0x14;
            _b.Ram[i++] = Opcodes.Registers.INY.Opcode;
            _c.Tick(Opcodes.Registers.INY.Cycles);

            Assert.Equal(0x15, _c.Y);
        }

        [Fact]
        public void DEY_0x88()
        {
            byte i = 0;
            _c.Y = 0x14;
            _b.Ram[i++] = Opcodes.Registers.DEY.Opcode;
            _c.Tick(Opcodes.Registers.DEY.Cycles);

            Assert.Equal(0x13, _c.Y);
        }
    }
}

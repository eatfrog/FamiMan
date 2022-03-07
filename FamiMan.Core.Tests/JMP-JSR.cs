using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class JMP
    {
        private Bus _b;
        private Cpu _c;

        public JMP()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void JMP_0x4c_Absolute()
        {
            byte i = 0;
            _b.Ram[i++] = Opcodes.JMP.Absolute.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01; // Jump to 0x110
            _c.Tick(Opcodes.JMP.Absolute.Cycles);
            Assert.Equal(0x110, _c.PC);
        }

        [Fact]
        public void JMP_0x6C_Indirect()
        {
            byte i = 0;
            _b.Ram[i++] = Opcodes.JMP.Indirect.Opcode;
            _b.Ram[i++] = 0x0F;
            _b.Ram[i++] = 0x01; // Mem at 0x10F-0x110 tells the jump addr
            _b.Ram[0x10F] = 0x12;
            _b.Ram[0x110] = 0x01; // Jump to 0x112
            _c.Tick(Opcodes.JMP.Indirect.Cycles);
            Assert.Equal(0x112, _c.PC);
        }

        [Fact]
        public void JSR_0x20_Absolute()
        {
            byte i = 0;
            _c.S = 0x10;
            _b.Ram[i++] = Opcodes.NOP.NOP_14.Opcode;
            _c.Tick(1);
            _b.Ram[i++] = Opcodes.JSR.Absolute.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01; // Jump to 0x110
            _c.Tick(Opcodes.JSR.Absolute.Cycles);
            Assert.Equal(0x110, _c.PC);
            Assert.Equal(4, _b.Ram[(byte)(_c.S + 2)]);
        }

    }
}


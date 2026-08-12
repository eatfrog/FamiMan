using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class JMPTests
    {
        private Bus _b;
        private Cpu _c;

        public JMPTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void JMP_0x4c_Absolute()
        {
            byte i = 0;
            _b.Ram[i++] = JMP.Absolute.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01; // Jump to 0x110
            _c.Tick(JMP.Absolute.Cycles);
            Assert.Equal(0x110, _c.PC);
        }

        [Fact]
        public void JMP_0x6C_Indirect()
        {
            byte i = 0;
            _b.Ram[i++] = JMP.Indirect.Opcode;
            _b.Ram[i++] = 0x0F;
            _b.Ram[i++] = 0x01; // Mem at 0x10F-0x110 tells the jump addr
            _b.Ram[0x10F] = 0x12;
            _b.Ram[0x110] = 0x01; // Jump to 0x112
            _c.Tick(JMP.Indirect.Cycles);
            Assert.Equal(0x112, _c.PC);
        }

        [Fact]
        public void JSR_0x20_Absolute()
        {
            _c.PC = 0x0200;
            _c.SP = 0xFD;
            _b[0x0200] = JSR.Absolute.Opcode;
            _b[0x0201] = 0x10;
            _b[0x0202] = 0x01; // Jump to 0x0110

            _c.Tick(JSR.Absolute.Cycles);

            Assert.Equal(0x110, _c.PC);
            Assert.Equal(0x02, _b[0x01FD]); // high byte of return address $0202
            Assert.Equal(0x02, _b[0x01FC]); // low byte of return address $0202
            Assert.Equal(0xFB, _c.SP);
        }

    }
}


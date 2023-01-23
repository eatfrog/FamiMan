using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class DECTests
    {
        private Bus _b;
        private Cpu _c;
        public DECTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void DEC_0xC6_ZeroPage()
        {
            byte i = 0;

            _b.Ram[i++] = DEC.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // 14            
            _b.Ram[0x0E] = 10;
            _c.Tick(DEC.ZeroPage.Cycles);
            Assert.Equal(9, _b.Ram[0x0E]);
        }

        [Fact]
        public void DEC_0xD6_ZeroPage_X()
        {
            byte i = 0;

            _b.Ram[i++] = DEC.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // 14
            _c.X = 1;
            _b.Ram[0x0F] = 10;
            _c.Tick(DEC.ZeroPage_X.Cycles);
            Assert.Equal(9, _b.Ram[0x0F]);
        }

        [Fact]
        public void DEC_0xCE_Absolute()
        {
            byte i = 0;

            _b.Ram[i++] = DEC.Absolute.Opcode;
            _b.Ram[i++] = 0x0E; // 14
            _b.Ram[i++] = 0x01; // 0x10E
            _b.Ram[0x10E] = 10;
            _c.Tick(DEC.Absolute.Cycles);
            Assert.Equal(9, _b.Ram[0x10E]);
        }

        [Fact]
        public void DEC_0xDE_Absolute_X()
        {
            byte i = 0;

            _b.Ram[i++] = DEC.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E; // 14
            _b.Ram[i++] = 0x01; // 0x10E
            _c.X = 1;
            _b.Ram[0x10F] = 10;
            _c.Tick(DEC.Absolute_X.Cycles);
            Assert.Equal(9, _b.Ram[0x10F]);
        }
    }
}

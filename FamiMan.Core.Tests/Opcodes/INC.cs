using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class INCTests
    {
        private Bus _b;
        private Cpu _c;
        public INCTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void INC_0xC6_ZeroPage()
        {
            byte i = 0;

            _b.Ram[i++] = INC.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // 14            
            _b.Ram[0x0E] = 0xFF;
            _c.Tick(INC.ZeroPage.Cycles);
            Assert.Equal(0, _b.Ram[0x0E]);
            Assert.True(_c.P.Zero);
            Assert.False(_c.P.Negative);
        }

        [Fact]
        public void INC_0xD6_ZeroPage_X()
        {
            byte i = 0;

            _b.Ram[i++] = INC.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // 14
            _c.X = 1;
            _b.Ram[0x0F] = 10;
            _c.Tick(INC.ZeroPage_X.Cycles);
            Assert.Equal(11, _b.Ram[0x0F]);
        }

        [Fact]
        public void INC_0xCE_Absolute()
        {
            byte i = 0;

            _b.Ram[i++] = INC.Absolute.Opcode;
            _b.Ram[i++] = 0x0E; // 14
            _b.Ram[i++] = 0x01; // 0x10E
            _b.Ram[0x10E] = 10;
            _c.Tick(INC.Absolute.Cycles);
            Assert.Equal(11, _b.Ram[0x10E]);
        }

        [Fact]
        public void INC_0xDE_Absolute_X()
        {
            byte i = 0;

            _b.Ram[i++] = INC.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E; // 14
            _b.Ram[i++] = 0x01; // 0x10E
            _c.X = 1;
            _b.Ram[0x10F] = 10;
            _c.Tick(INC.Absolute_X.Cycles);
            Assert.Equal(11, _b.Ram[0x10F]);
        }
    }
}

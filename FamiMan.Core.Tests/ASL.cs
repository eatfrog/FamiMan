using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class ASL
    {
        private Bus _b;
        private Cpu _c;
        public ASL()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void ASL_0x0E_Absolute()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = Opcodes.ASL.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 109; // 01101101
            _c.Tick(Opcodes.ASL.Absolute.Cycles);

            // 01101101 <- 0 
            // 11011010 
            Assert.Equal(218, _c.A);

            _b.Ram[i++] = Opcodes.ASL.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 128; // 10000000
            _c.Tick(Opcodes.ASL.Absolute.Cycles);

            // 10000000 <- 0 
            // 00000000 
            Assert.Equal(0, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

    }
}

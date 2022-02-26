using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class LDX
    {
        private Bus _b;
        private Cpu _c;
        public LDX()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void LDX_0xA2_Immediate()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDX.Immediate.Opcode;  
            _b.Ram[i++] = 0x0E;         

            _c.Tick(Opcodes.LDX.Immediate.Cycles);

            Assert.Equal(0x0E, _c.X);
        }

        [Fact]
        public void LDX_0xA6_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDX.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 14;
            _c.Tick(Opcodes.LDX.ZeroPage.Cycles);

            Assert.Equal(14, _c.X);
        }

        [Fact]
        public void LDX_0xB6_ZeroPage_Y()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDX.ZeroPage_Y.Opcode;
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 14;
            _c.Tick(Opcodes.LDX.ZeroPage_Y.Cycles);

            Assert.Equal(14, _c.X);
        }

        [Fact]
        public void LDX_0xAE_Absolute()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDX.Absolute.Opcode; 

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.

            _b.Ram[0x03E8] = 14;
            _c.Tick(Opcodes.LDX.Absolute.Cycles);

            Assert.Equal(14, _c.X);
        }

        [Fact]
        public void LDX_0xBE_Absolute_Y()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDX.Absolute_Y.Opcode;

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Y = 1;               // Add 1 to the memory address
            _b.Ram[0x03E9] = 14;
            _c.Tick(Opcodes.LDX.Absolute.Cycles);

            Assert.Equal(14, _c.X);
        }
    }
}

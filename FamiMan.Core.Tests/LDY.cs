using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class LDY
    {
        private Bus _b;
        private Cpu _c;
        public LDY()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void LDY_0xA0_Immediate()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDY.Immediate.Opcode;  
            _b.Ram[i++] = 0x0E;         

            _c.Tick(Opcodes.LDY.Immediate.Cycles);

            Assert.Equal(0x0E, _c.Y);
        }

        [Fact]
        public void LDY_0xA4_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDY.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 14;
            _c.Tick(Opcodes.LDY.ZeroPage.Cycles);

            Assert.Equal(14, _c.Y);
        }

        [Fact]
        public void LDY_0xB4_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDY.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _c.X = 1; // Add one
            _b.Ram[0x0B] = 14;
            _c.Tick(Opcodes.LDY.ZeroPage_X.Cycles);

            Assert.Equal(14, _c.Y);
        }

        [Fact]
        public void LDY_0xAC_Absolute()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDY.Absolute.Opcode; 

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.

            _b.Ram[0x03E8] = 14;
            _c.Tick(Opcodes.LDY.Absolute.Cycles);

            Assert.Equal(14, _c.Y);
        }

        [Fact]
        public void LDY_0xBC_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDY.Absolute_X.Opcode;

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;               // Add 1 to the memory address
            _b.Ram[0x03E9] = 14;
            _c.Tick(Opcodes.LDY.Absolute_X.Cycles);

            Assert.Equal(14, _c.Y);
        }
    }
}

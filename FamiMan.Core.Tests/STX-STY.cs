using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class STX_STY
    {
        private Bus _b;
        private Cpu _c;
        public STX_STY()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void STX_0x86_ZeroPage()
        {
            byte i = 0;
            _c.X = 123;
            _b.Ram[i++] = Opcodes.STX.ZeroPage.Opcode; // Store X into memory location..
            _b.Ram[i++] = 0x64;                        // ..0x64

            _c.Tick();
            Assert.Equal(0, _c.PC); // Program counter should not yet have moved

            _c.Tick(Opcodes.STX.ZeroPage.Cycles - 1);

            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(Opcodes.STX.ZeroPage.Length, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _b.Ram[0x64]); // Value is now in memory
        }

        [Fact]
        public void STX_0x96_ZeroPageY()
        {
            byte i = 0;
            _c.X = 123;
            _b.Ram[i++] = Opcodes.STX.ZeroPage_Y.Opcode;     // Store to X that is in memory location
            _b.Ram[i++] = 0x64;     // 0x64
            _c.Y = 0x01;            // Memloc + Y = 0x65

            _c.Tick(Opcodes.STX.ZeroPage_Y.Cycles);              // Tick

            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _b.Ram[0x65]); // Value is now in memory
        }

        [Fact]
        public void STX_0x8E_Absolute()
        {
            byte i = 0;
            _c.X = 123;

            _b.Ram[i++] = Opcodes.STX.Absolute.Opcode;
            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Tick(Opcodes.STX.Absolute.Cycles);              // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(123, _b.Ram[0x3E8]); // Value is now in memory

        }

        [Fact]
        public void STY_0x8C_Absolute()
        {
            byte i = 0;
            _c.Y = 123;

            _b.Ram[i++] = Opcodes.STY.Absolute.Opcode;
            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Tick(Opcodes.STY.Absolute.Cycles);              // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(3, Opcodes.STY.Absolute.Length);
            Assert.Equal(123, _b.Ram[0x3E8]); // Value is now in memory
        }

        [Fact]
        public void STY_0x84_ZeroPage()
        {
            byte i = 0;
            _c.Y = 123;

            _b.Ram[i++] = Opcodes.STY.ZeroPage.Opcode;
            _b.Ram[i++] = 0x64;     // 0x64
            _c.Tick(Opcodes.STY.ZeroPage.Cycles);   // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _b.Ram[0x64]); // Value is now in memory
        }

        [Fact]
        public void STY_0x94_ZeroPageX()
        {
            byte i = 0;
            _c.Y = 123;

            _b.Ram[i++] = Opcodes.STY.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x64;     // 0x64
            _c.X = 0x01;            // Memloc + X = 0x65
            _c.Tick(Opcodes.STY.ZeroPage_X.Cycles);              // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _b.Ram[0x65]); // Value is now in memory
        }
    }
}

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
            byte opcode = 0x86;
            byte i = 0;
            _b.Ram[i++] = opcode;     // Store to X that is in memory location
            _b.Ram[i++] = 0x64;     // 0x64
            _b[0x64] = 123;         // Value to store in X register

            _c.Tick();

            // TODO: timing with ticks?
            //Assert.Equal(0, _c.PC); // Program counter should not yet have moved
            //for (int t = 0; t < Constants.STXSTY.Cycles[opcode]; t++)
            //    _c.Tick();              // Tick

            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _c.X);
        }

        [Fact]
        public void STX_0x96_ZeroPageY()
        {
            byte opcode = 0x96;
            byte i = 0;
            _b.Ram[i++] = opcode;     // Store to X that is in memory location
            _b.Ram[i++] = 0x64;     // 0x64
            _c.Y = 0x01;            // Memloc + Y = 0x65
            _b[0x65] = 123;         // Value to store in X register

            for (int t = 0; t <= Constants.STXSTY.Cycles[opcode]; t++)
                _c.Tick();              // Tick

            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _c.X);
        }

        [Fact]
        public void STX_0x8E_Absolute()
        {
            byte i = 0;
            byte opcode = 0x8E;
            _b.Ram[i++] = 0x8E;     // Store to X that is in absolute memory location
            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _b[0x03E8] = 0x02;      // 2 at memory location 0x3E8/1000d
            _c.Ticks(Constants.STXSTY.Cycles[opcode]);              // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(3, Constants.STXSTY.Length[0x8E]);
            Assert.Equal(2, _c.X);
        }

        [Fact]
        public void STY_0x8C_Absolute()
        {
            byte i = 0;
            byte opcode = 0x8C;

            _b.Ram[i++] = 0x8C;     // Store to Y that is in memory location
            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _b[0x03E8] = 0x02;      // 2 at memory location 0x3E8/1000d
            _c.Ticks(Constants.STXSTY.Cycles[opcode]);              // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(3, Constants.STXSTY.Length[0x8C]);
            Assert.Equal(2, _c.Y);
        }

        [Fact]
        public void STY_0x84_ZeroPage()
        {
            byte i = 0;
            byte opcode = 0x84;

            _b.Ram[i++] = 0x84;     // Store to X that is in memory location
            _b.Ram[i++] = 0x64;     // 0x64
            _b[0x64] = 123;         // Value to store in Y register
            _c.Ticks(Constants.STXSTY.Cycles[opcode]);              // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _c.Y);
        }

        [Fact]
        public void STY_0x94_ZeroPageX()
        {
            byte i = 0, opcode = 0x94;

            _b.Ram[i++] = 0x94;     // Store to Y that is in memory location
            _b.Ram[i++] = 0x64;     // 0x64
            _c.X = 0x01;            // Memloc + X = 0x65
            _b[0x65] = 123;         // Value to store in Y register
            _c.Ticks(Constants.STXSTY.Cycles[opcode]);              // Tick
            Assert.Equal(0, _c.A);  // Accumulator should be 0
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            Assert.Equal(123, _c.Y);
        }
    }
}

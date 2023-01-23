using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class STATests
    {
        private Bus _b;
        private Cpu _c;
        public STATests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void STA_0x85_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x14;
            _b.Ram[i++] = STA.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _c.Tick(STA.ZeroPage.Cycles);

            Assert.Equal(0x14, _b.Ram[0x0A]);
        }

        [Fact]
        public void STA_0x95_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x14;
            _b.Ram[i++] = STA.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0A;                             // Memory location 0x0A
            _c.X = 2;

            _c.Tick(STA.ZeroPage_X.Cycles);

            Assert.Equal(0x14, _b.Ram[0x0C]);
        }

        [Fact]
        public void STA_0x8D_Absolute()
        {
            byte i = 0;
            _c.A = 0x14;
            _b.Ram[i++] = STA.Absolute.Opcode;

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.

            _c.Tick(STA.Absolute.Cycles);

            Assert.Equal(0x14, _b.Ram[0x03E8]);
        }

        [Fact]
        public void STA_0x9D_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x14;
            _b.Ram[i++] = STA.Absolute_X.Opcode;

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;               // add 1 to the memory address

            _c.Tick(STA.Absolute.Cycles);

            Assert.Equal(0x14, _b.Ram[0x03E9]);
        }

        [Fact]
        public void STA_0x99_Absolute_Y()
        {
            byte i = 0;
            _c.A = 0x14;
            _b.Ram[i++] = STA.Absolute_Y.Opcode;

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Y = 1;               // Add 1 to the memory address
            _c.Tick(STA.Absolute.Cycles);

            Assert.Equal(0x14, _b.Ram[0x03E9]);
        }

        [Fact]
        public void STA_0x81_IndirectY()
        {
            _c.A = 0x14;
            byte i = 0;
            _b.Ram[i++] = STA.IndirectIndexed.Opcode;   // Add Indirect_X
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                    // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                           // + 2 so 0x05
            _c.Tick(STA.IndirectIndexed.Cycles);        // Tick

            Assert.Equal(0x14, _b.Ram[0x05]);
            Assert.Equal(STA.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }

        [Fact]
        public void STA_0x91_IndirectX()
        {
            _c.A = 0x14;
            byte i = 0;
            _b.Ram[i++] = STA.IndexedIndirect.Opcode;   // Add Indirect_Y
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                           // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                    // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                                // 0x0703
            _c.Tick(STA.IndexedIndirect.Cycles);        // Tick


            Assert.Equal(0x14, _b.Ram[0x0703]);
            Assert.Equal(STA.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }

    }
}

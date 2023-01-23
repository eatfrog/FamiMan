using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    /*
        MODE           SYNTAX       HEX LEN TIM
        Immediate     ORA #$44      $09  2   2
        Zero Page     ORA $44       $05  2   3
        Zero Page,X   ORA $44,X     $15  2   4
        Absolute      ORA $4400     $0D  3   4
        Absolute,X    ORA $4400,X   $1D  3   4+
        Absolute,Y    ORA $4400,Y   $19  3   4+
        Indirect,X    ORA ($44,X)   $01  2   6
        Indirect,Y    ORA ($44),Y   $11  2   5+
     */
    public class ORATests
    {
        private Bus _b;
        private Cpu _c;
        public ORATests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void ORA_0x09_Immediate()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ORA.Immediate.Opcode;    // ORA
            _b.Ram[i++] = 0x0E;                            // 14            

            _c.Tick(ORA.Immediate.Cycles);

            Assert.Equal(5 | 14, _c.A);
        }

        [Fact]
        public void ORA_0x05_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ORA.ZeroPage.Opcode;    // ORA
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 14;
            _c.Tick(ORA.ZeroPage.Cycles);

            Assert.Equal(5 | 14, _c.A);
        }

        [Fact]
        public void ORA_0x15_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ORA.ZeroPage_X.Opcode;    // ORA
            _b.Ram[i++] = 0x0A;                             // Memory location 0x0A
            _c.X = 2;
            _b.Ram[0x0C] = 14;
            _c.Tick(ORA.ZeroPage_X.Cycles);

            Assert.Equal(5 | 14, _c.A);
        }

        [Fact]
        public void ORA_0x0D_Absolute()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ORA.Absolute.Opcode;    // ORA

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.

            _b.Ram[0x03E8] = 14;
            _c.Tick(ORA.Absolute.Cycles);


            Assert.Equal(5 | 14, _c.A);
        }

        [Fact]
        public void ORA_0x1D_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ORA.Absolute_X.Opcode;    // ORA

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;               // add 1 to the memory address
            _b.Ram[0x03E9] = 14;

            _c.Tick(ORA.Absolute.Cycles);


            Assert.Equal(5 | 14, _c.A);
        }

        [Fact]
        public void ORA_0x19_Absolute_Y()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ORA.Absolute_Y.Opcode;    // ORA

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Y = 1;               // Add 1 to the memory address
            _b.Ram[0x03E9] = 14;
            _c.Tick(ORA.Absolute.Cycles);


            Assert.Equal(5 | 14, _c.A);
        }

        [Fact]
        public void ORA_0x11_IndirectY()
        {
            _c.A = 5;
            byte i = 0;
            _b.Ram[i++] = ORA.IndirectIndexed.Opcode;   // Add Indirect_X
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                    // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                           // + 2 so 0x05
            _b[0x05] = 14;                                      // which has value 14
            _c.Tick(ORA.IndirectIndexed.Cycles);        // Tick


            Assert.Equal(5 | 14, _c.A);
            Assert.Equal(ORA.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }

        [Fact]
        public void ORA_0x01_IndirectX()
        {
            _c.A = 5;
            byte i = 0;
            _b.Ram[i++] = ORA.IndexedIndirect.Opcode;   // Add Indirect_Y
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                           // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                    // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                                // 0x0703
            _b[0x0703] = 14;                                    // which has value 14
            _c.Tick(ORA.IndexedIndirect.Cycles);        // Tick


            Assert.Equal(5 | 14, _c.A);
            Assert.Equal(ORA.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }
    }
}

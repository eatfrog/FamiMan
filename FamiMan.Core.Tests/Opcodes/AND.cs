using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    /*
     *  MODE           SYNTAX       HEX LEN TIM
        Immediate     AND #$44      $29  2   2
        Zero Page     AND $44       $25  2   3
        Zero Page,X   AND $44,X     $35  2   4
        Absolute      AND $4400     $2D  3   4
        Absolute,X    AND $4400,X   $3D  3   4+
        Absolute,Y    AND $4400,Y   $39  3   4+
        Indirect,X    AND ($44,X)   $21  2   6
        Indirect,Y    AND ($44),Y   $31  2   5+     
     */
    public class ANDTests
    {
        private Bus _b;
        private Cpu _c;
        public ANDTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void AND_0x29_Immediate()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = AND.Immediate.Opcode;    // AND
            _b.Ram[i++] = 0x0E;                            // 14            

            _c.Tick(AND.Immediate.Cycles);

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
        }

        [Fact]
        public void AND_0x25_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = AND.ZeroPage.Opcode;    // AND
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 14;
            _c.Tick(AND.ZeroPage.Cycles);

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
        }

        [Fact]
        public void AND_0x35_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = AND.ZeroPage_X.Opcode;    // AND
            _b.Ram[i++] = 0x0A;                             // Memory location 0x0A
            _c.X = 2;
            _b.Ram[0x0C] = 14;
            _c.Tick(AND.ZeroPage_X.Cycles);

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
        }

        [Fact]
        public void AND_0x2D_Absolute()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = AND.Absolute.Opcode;    // AND

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.

            _b.Ram[0x03E8] = 14;
            _c.Tick(AND.Absolute.Cycles);

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
        }

        [Fact]
        public void AND_0x3D_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = AND.Absolute_X.Opcode;    // AND

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;               // add 1 to the memory address
            _b.Ram[0x03E9] = 14;

            _c.Tick(AND.Absolute_X.Cycles);

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
        }

        [Fact]
        public void AND_0x39_Absolute_Y()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = AND.Absolute_Y.Opcode;    // AND

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Y = 1;               // Add 1 to the memory address
            _b.Ram[0x03E9] = 14;
            _c.Tick(AND.Absolute_Y.Cycles);

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
        }

        [Fact]
        public void AND_0x31_IndirectY()
        {
            _c.A = 5;
            byte i = 0;
            _b.Ram[i++] = AND.IndirectIndexed.Opcode;
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                    // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                           // + 2 so 0x05
            _b[0x05] = 14;                                      // which has value 14
            _c.Tick(AND.IndirectIndexed.Cycles);        // Tick

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
            Assert.Equal(AND.IndirectIndexed.Length, _c.PC);
        }

        [Fact]
        public void AND_0x21_IndirectX()
        {
            _c.A = 5;
            byte i = 0;
            _b.Ram[i++] = AND.IndexedIndirect.Opcode;
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                           // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                    // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                                // 0x0703
            _b[0x0703] = 14;                                    // which has value 14
            _c.Tick(AND.IndexedIndirect.Cycles);        // Tick

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
            Assert.Equal(AND.IndexedIndirect.Length, _c.PC);
        }
    }
}

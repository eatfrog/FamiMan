using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    /*
        MODE           SYNTAX       HEX LEN TIM
        Immediate     EOR #$44      $49  2   2
        Zero Page     EOR $44       $45  2   3
        Zero Page,X   EOR $44,X     $55  2   4
        Absolute      EOR $4400     $4D  3   4
        Absolute,X    EOR $4400,X   $5D  3   4+
        Absolute,Y    EOR $4400,Y   $59  3   4+
        Indirect,X    EOR ($44,X)   $41  2   6
        Indirect,Y    EOR ($44),Y   $51  2   5+  
     */
    public class EORTests
    {
        private Bus _b;
        private Cpu _c;
        public EORTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void EOR_0x49_Immediate()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = EOR.Immediate.Opcode;    // EOR
            _b.Ram[i++] = 0x0E;                            // 14            

            _c.Tick(EOR.Immediate.Cycles);

            Assert.Equal(5 ^ 14, _c.A);
        }

        [Fact]
        public void EOR_0x45_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = EOR.ZeroPage.Opcode;    // EOR
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 14;
            _c.Tick(EOR.ZeroPage.Cycles);

            Assert.Equal(5 ^ 14, _c.A);
        }

        [Fact]
        public void EOR_0x55_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = EOR.ZeroPage_X.Opcode;    // EOR
            _b.Ram[i++] = 0x0A;                             // Memory location 0x0A
            _c.X = 2;
            _b.Ram[0x0C] = 14;
            _c.Tick(EOR.ZeroPage_X.Cycles);

            Assert.Equal(5 ^ 14, _c.A);
        }

        [Fact]
        public void EOR_0x4D_Absolute()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = EOR.Absolute.Opcode;    // EOR

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.

            _b.Ram[0x03E8] = 14;
            _c.Tick(EOR.Absolute.Cycles);


            Assert.Equal(5 ^ 14, _c.A);
        }

        [Fact]
        public void EOR_0x5D_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = EOR.Absolute_X.Opcode;    // EOR

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;               // add 1 to the memory address
            _b.Ram[0x03E9] = 14;

            _c.Tick(EOR.Absolute.Cycles);


            Assert.Equal(5 ^ 14, _c.A);
        }

        [Fact]
        public void EOR_0x59_Absolute_Y()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = EOR.Absolute_Y.Opcode;    // EOR

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Y = 1;               // Add 1 to the memory address
            _b.Ram[0x03E9] = 14;
            _c.Tick(EOR.Absolute.Cycles);


            Assert.Equal(5 ^ 14, _c.A);
        }

        [Fact]
        public void EOR_0x51_IndirectY()
        {
            _c.A = 5;
            byte i = 0;
            _b.Ram[i++] = EOR.IndirectIndexed.Opcode;   // Add Indirect_X
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                    // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                           // + 2 so 0x05
            _b[0x05] = 14;                                      // which has value 14
            _c.Tick(EOR.IndirectIndexed.Cycles);        // Tick


            Assert.Equal(5 ^ 14, _c.A);
            Assert.Equal(EOR.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }

        [Fact]
        public void EOR_0x41_IndirectX()
        {
            _c.A = 5;
            byte i = 0;
            _b.Ram[i++] = EOR.IndexedIndirect.Opcode;   // Add Indirect_Y
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                           // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                    // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                                // 0x0703
            _b[0x0703] = 14;                                    // which has value 14
            _c.Tick(EOR.IndexedIndirect.Cycles);        // Tick


            Assert.Equal(5 ^ 14, _c.A);
            Assert.Equal(EOR.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }
    }
}

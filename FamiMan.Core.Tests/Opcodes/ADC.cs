using System;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{

    /*
     *  MODE           SYNTAX       HEX LEN TIM
        Immediate     ADC #$44      $69  2   2
        Zero Page     ADC $44       $65  2   3
        Zero Page,X   ADC $44,X     $75  2   4
        Absolute      ADC $4400     $6D  3   4
        Absolute,X    ADC $4400,X   $7D  3   4+
        Absolute,Y    ADC $4400,Y   $79  3   4+
        Indirect,X    ADC ($44,X)   $61  2   6
        Indirect,Y    ADC ($44),Y   $71  2   5+
     */
    public class ADCTests
    {
        private Bus _b;
        private Cpu _c;
        public ADCTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void ADC_0x69_Immediate()
        {
            byte i = 0;
            _b.Ram[i++] = ADC.Immediate.Opcode;     // Add
            _b.Ram[i++] = 0x01;                             // 1
            _b.Ram[i++] = ADC.Immediate.Opcode;     // Add again
            _b.Ram[i++] = 0x02;                             // 2
            _c.Tick(ADC.Immediate.Cycles);          // Tick
            Assert.Equal(1, _c.A);                          // Accumulator should be 1
            Assert.Equal(2, _c.PC);                         // Program counter should have moved to 2
            _c.Tick(ADC.Immediate.Cycles);          // Tick
            Assert.Equal(3, _c.A);                          // Accumulator should have 2 more now = 3

            _c.P.Carry = true;                              // Set carry flag since we want to add one more
            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0x01;                             // So value is 1, we expect 2 more
            _c.Tick(ADC.Immediate.Cycles);          // Tick
            Assert.Equal(5, _c.A);                          // From 3 to 5

            _c.A = 0x50;                                    // Set to 80
            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0x50;                             // Lets add 80
            _c.Tick(ADC.Immediate.Cycles);          // Tick

            Assert.Equal(160, _c.A);         // 160 in Acc
            Assert.False(_c.P.Carry);        // 0 in carry

            // ______ OVERFLOW FLAG ________
            // We moved from 0-128 <-> 129-255 range
            Assert.True(_c.P.Overflow);

            _c.A = 0xd0;
            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0x90;
            _c.Tick(ADC.Immediate.Cycles);  // Tick
            Assert.Equal(96, _c.A);
            Assert.True(_c.P.Overflow);     // 1 in overflow
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0x01;             // Lets add 1
            _c.Tick(ADC.Immediate.Cycles);  // Tick
            Assert.Equal(0x61, _c.A);
            Assert.False(_c.P.Overflow);    // 0 in overflow
            Assert.False(_c.P.Negative);

            _c.A = 1;                        // Reset
            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0x01;              // Lets add 1
            _c.Tick(ADC.Immediate.Cycles);  // Tick
            Assert.Equal(2, _c.A);           // 2 in A
            Assert.False(_c.P.Negative);     // 0 in negative

            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0x80;              // Lets add 128
            _c.Tick(ADC.Immediate.Cycles);  // Tick
            Assert.Equal(130, _c.A);         // 130 in A, 2 + 128
            Assert.True(_c.P.Negative);      // 1 in negative

            // ________ ZERO FLAG _______
            // Result is 0
            Assert.False(_c.P.Zero);

            _c.A = 0;                        // Reset
            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0x00;              // Lets add 0
            _c.Tick(ADC.Immediate.Cycles);  // Tick
            Assert.Equal(0, _c.A);           // 0 in A
            Assert.True(_c.P.Zero);          // 1 in zero

            _c.A = 1;                        // Reset
            _b.Ram[i++] = ADC.Immediate.Opcode;
            _b.Ram[i++] = 0xFF;              // Lets add 255
            _c.Tick(ADC.Immediate.Cycles);  // Tick

            Assert.Equal(0, _c.A);           // 0 in A
            Assert.True(_c.P.Zero);          // 1 in zero
        }

        [Fact]
        public void ADC_0x6D_Absolute()
        {
            byte i = 0;
            _b.Ram[i++] = ADC.Absolute.Opcode;     // Add Absolute
            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _b[0x03E8] = 0x02;     // 2 at memory location 0x3E8/1000d
            _c.Tick(ADC.Absolute.Cycles);              // Tick
            Assert.Equal(2, _c.A);  // Accumulator should be 2
            Assert.Equal(3, _c.PC); // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x65_ZeroPage()
        {
            byte i = 0;
            _b.Ram[i++] = ADC.ZeroPage.Opcode;     // Add Zero page
            _b.Ram[i++] = 0xE8;     // Memory location: 0x00E8/232d
            _b[0x00E8] = 0x02;     // 2 at memory location 0x00E8/232d
            _c.Tick(ADC.ZeroPage.Cycles);              // Tick
            Assert.Equal(2, _c.A);  // Accumulator should be 2
            Assert.Equal(2, _c.PC); // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x75_ZeroPageX()
        {
            byte i = 0;
            _b.Ram[i++] = ADC.ZeroPage_X.Opcode;     // Add Zero page X
            _b.Ram[i++] = 0xE8;     // Memory location: 0x00E8/232d
            _c.X = 1;
            _b[0x00E9] = 0x02;      // 2 at memory location 0x00E9
            _c.Tick(ADC.ZeroPage_X.Cycles);              // Tick
            Assert.Equal(2, _c.A);  // Accumulator should be 2
            Assert.Equal(2, _c.PC); // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x7D_AbsoluteX()
        {
            byte i = 0;
            _b.Ram[i++] = ADC.Absolute_X.Opcode;    // Add Absolute X
            _b.Ram[i++] = 0xE8;                             // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;                             // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;
            _b[0x03E9] = 0x04;                              // 4 at memory location 0x3E9/1001d
            _c.Tick(ADC.Absolute_X.Cycles);         // Tick
            Assert.Equal(4, _c.A);                          // Accumulator should be 4
            Assert.Equal(3, _c.PC);                         // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x79_AbsoluteY()
        {
            byte i = 0;
            _b.Ram[i++] = ADC.Absolute_Y.Opcode;    // Add Absolute Y
            _b.Ram[i++] = 0xE8;                             // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;                             // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 5;
            _c.Y = 1;
            _b[0x03E9] = 0x12;                              // 18 at memory location 0x3E9/1001d
            _c.Tick(ADC.Absolute_Y.Cycles);         // Tick
            Assert.Equal(18, _c.A);                         // Accumulator should be 18
            Assert.Equal(3, _c.PC);                         // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x61_IndirectX()
        {
            _c.A = 2;
            byte i = 0;
            _b.Ram[i++] = ADC.IndexedIndirect.Opcode;    // Add Indirect_X
            _b.Ram[i++] = 0xE8;                             // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                       // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                            // 0x0703
            _b[0x0703] = 0xfd;                              // which has value fd
            _c.Tick(ADC.IndexedIndirect.Cycles);    // Tick
            Assert.Equal(0xff, _c.A);                       // Accumulator should be ff
            Assert.Equal(ADC.IndexedIndirect.Length, _c.PC); // Program counter should have moved to correct value
        }

        [Fact]
        public void ADC_0x71_IndirectY()
        {
            _c.A = 2;
            byte i = 0;
            _b.Ram[i++] = ADC.IndirectIndexed.Opcode;    // Add Indirect_Y
            _b.Ram[i++] = 0xE8;                             // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                       // + 2 so 0x05
            _b[0x05] = 0xea;                                // which has value ea
            _c.Tick(ADC.IndirectIndexed.Cycles);         // Tick
            Assert.Equal(0xea + 2, _c.A);                         // Accumulator should be ec
            Assert.Equal(ADC.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }
    }
}

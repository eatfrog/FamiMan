using System;
using Xunit;

namespace FamiMan.Core.Tests
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
    public class ADC
    {
        private Bus _b;
        private Cpu _c;
        public ADC()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void ADC_0x69_Immediate()
        {
            byte i = 0;
            _b.Ram[i++] = 0x69;     // Add
            _b.Ram[i++] = 0x01;     // 1
            _b.Ram[i++] = 0x69;     // Add again
            _b.Ram[i++] = 0x02;     // 1
            _c.Tick();              // Tick
            Assert.Equal(1, _c.A);  // Accumulator should be 1
            Assert.Equal(2, _c.PC); // Program counter should have moved to 2
            _c.Tick();              // Another cpu tick
            Assert.Equal(3, _c.A);  // Accumulator should have 2 more now

            _c.P.Carry = true;      // If carry flag is set we want to add one more
            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0x01;     // So value is 1, we expect 2 more
            _c.Tick();
            Assert.Equal(5, _c.A);  // From 3 to 5

            _c.A = 0;               // Reset
            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0xF0;     // Lets add 240
            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0x10;     // And 16
            _c.Tick();
            _c.Tick();
            Assert.Equal(0, _c.A);          // 0 in Acc
            Assert.True(_c.P.Carry);        // 1 in carry

            // ______ OVERFLOW FLAG ________
            // We moved from 0-128 <-> 129-255 range
            Assert.True(_c.P.Overflow);    // 1 in overflow, 240 -> 1

            _c.A = 1;                       // Reset
            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0x80;             // Lets add 128
            _c.Tick();
            Assert.Equal(129, _c.A);        // 129 in A
            Assert.True(_c.P.Overflow);     // 1 in overflow


            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0x01;             // Lets add 1
            _c.Tick();
            Assert.Equal(130, _c.A);        // 129 in A
            Assert.False(_c.P.Overflow);    // 0 in overflow

            // ______ NEGATIVE FLAG _______
            // Result is more than 127
            Assert.True(_c.P.Negative);

            _c.A = 1;                        // Reset
            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0x01;              // Lets add 1
            _c.Tick();
            Assert.Equal(2, _c.A);           // 2 in A
            Assert.False(_c.P.Negative);     // 0 in negative

            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0x80;              // Lets add 128
            _c.Tick();
            Assert.Equal(130, _c.A);         // 130 in A, 2 + 128
            Assert.True(_c.P.Negative);      // 1 in negative

            // ________ ZERO FLAG _______
            // Result is 0
            Assert.False(_c.P.Zero);

            _c.A = 0;                        // Reset
            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0x00;              // Lets add 0
            _c.Tick();
            Assert.Equal(0, _c.A);           // 0 in A
            Assert.True(_c.P.Zero);          // 1 in zero

            _c.A = 1;                        // Reset
            _b.Ram[i++] = 0x69;
            _b.Ram[i++] = 0xFF;              // Lets add 255
            _c.Tick();

            Assert.Equal(0, _c.A);           // 0 in A
            Assert.True(_c.P.Zero);          // 1 in zero
        }

        [Fact]
        public void ADC_0x6D_Absolute()
        {
            byte i = 0;
            _b.Ram[i++] = 0x6D;     // Add Absolute
            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _b[0x03E8]  = 0x02;     // 2 at memory location 0x3E8/1000d
            _c.Tick();              // Tick
            Assert.Equal(2, _c.A);  // Accumulator should be 2
            Assert.Equal(3, _c.PC); // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x65_ZeroPage()
        {
            byte i = 0;
            _b.Ram[i++] = 0x65;     // Add Zero page
            _b.Ram[i++] = 0xE8;     // Memory location: 0x00E8/232d
            _b[0x00E8]  = 0x02;     // 2 at memory location 0x00E8/232d
            _c.Tick();              // Tick
            Assert.Equal(2, _c.A);  // Accumulator should be 2
            Assert.Equal(2, _c.PC); // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x75_ZeroPageX()
        {
            byte i = 0;
            _b.Ram[i++] = 0x75;     // Add Zero page
            _b.Ram[i++] = 0xE8;     // Memory location: 0x00E8/232d
            _c.X = 1;
            _b[0x00E9] = 0x02;      // 2 at memory location 0x00E9
            _c.Tick();              // Tick
            Assert.Equal(2, _c.A);  // Accumulator should be 2
            Assert.Equal(2, _c.PC); // Program counter should have moved to 3
        }

        [Fact]
        public void ADC_0x7D_AbsoluteX()
        {
            byte i = 0;
            _b.Ram[i++] = 0x7D;     // Add Absolute
            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;
            _b[0x03E9] = 0x02;      // 2 at memory location 0x3E9/1000d
            _c.Tick();              // Tick
            Assert.Equal(2, _c.A);  // Accumulator should be 2
            Assert.Equal(3, _c.PC); // Program counter should have moved to 3
        }
    }
}

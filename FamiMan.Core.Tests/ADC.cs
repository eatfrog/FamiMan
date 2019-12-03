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
        public void ADC_0x69()
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

            Assert.Equal(1, _c.A);      // 1 in A
            Assert.True(_c.P.Carry);    // 1 in carry

        }
    }
}

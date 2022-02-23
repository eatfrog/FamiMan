using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FamiMan.Core.Tests
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
    public class AND
    {
        private Bus _b;
        private Cpu _c;
        public AND()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void AND_0x29_Immediate() // TODO: implementation missing in cpu
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = Opcodes.AND.Immediate.Opcode;    // AND
            _b.Ram[i++] = 0x0E;                            // 14            

            _c.Tick(Opcodes.AND.Immediate.Cycles);

            // 00000101 - 5
            // 00001110 - 14
            // ________ AND
            // 00000100 - 4
            Assert.Equal(4, _c.A);
        }
    }
}

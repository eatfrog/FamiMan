using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class RTI
        {
            /*
             * MODE       SYNTAX    HEX     LEN TIM
               Implied    RTI       $40     1   6
            */

            public static class Implied
            {
                public const byte Opcode = 0x40;
                public const int Length = 1;
                public const int Cycles = 6;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public static class Opcodes
    {

        public static class ADC
        {
            public static class Immediate
            {
                public const int    Cycles  = 2;
                public const int    Length  = 2;
                public const byte   Opcode  = 0x69;
            }
        }
    }
}

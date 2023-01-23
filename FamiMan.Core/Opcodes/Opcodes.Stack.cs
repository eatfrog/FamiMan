using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public partial class Opcodes
    {
        /*
         *  MNEMONIC                        HEX TIM
            TXS (Transfer X to Stack ptr)   $9A  2
            TSX (Transfer Stack ptr to X)   $BA  2
            PHA (PusH Accumulator)          $48  3
            PLA (PuLl Accumulator)          $68  4
            PHP (PusH Processor status)     $08  3
            PLP (PuLl Processor status)     $28  4
        */
        public static class Stack
        {
            public static class TXS
            {
                public const byte Opcode = 0x9A;
                public const int Length = 1;
                public const int Cycles = 2;
            }
            public static class TSX
            {
                public const byte Opcode = 0xBA;
                public const int Length = 1;
                public const int Cycles = 2;
            }
            public static class PHA
            {
                public const byte Opcode = 0x48;
                public const int Length = 1;
                public const int Cycles = 3;
            }
            public static class PLA
            {
                public const byte Opcode = 0x68;
                public const int Length = 1;
                public const int Cycles = 4;
            }
            public static class PHP
            {
                public const byte Opcode = 0x08;
                public const int Length = 1;
                public const int Cycles = 3;
            }
            public static class PLP
            {
                public const byte Opcode = 0x28;
                public const int Length = 1;
                public const int Cycles = 4;
            }
        }
    }
}

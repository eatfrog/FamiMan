using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public partial class Opcodes
    {
        /*
         *  MNEMONIC                 HEX
            TAX (Transfer A to X)    $AA
            TXA (Transfer X to A)    $8A
            DEX (DEcrement X)        $CA
            INX (INcrement X)        $E8
            TAY (Transfer A to Y)    $A8
            TYA (Transfer Y to A)    $98
            DEY (DEcrement Y)        $88
            INY (INcrement Y)        $C8
         */
        public static class Registers
        {
            public static class TAX
            {
                public const byte Opcode = 0xAA;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class TXA
            {
                public const byte Opcode = 0x8A;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class DEX
            {
                public const byte Opcode = 0xCA;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class INX
            {
                public const byte Opcode = 0xE8;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class TAY
            {
                public const byte Opcode = 0xA8;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class TYA
            {
                public const byte Opcode = 0x98;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class DEY
            {
                public const byte Opcode = 0x88;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class INY
            {
                public const byte Opcode = 0xC8;
                public const int Length = 1;
                public const int Cycles = 2;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public partial class Opcodes
    {
        /*
            MNEMONIC                       HEX
            CLC (CLear Carry)              $18
            SEC (SEt Carry)                $38
            CLI (CLear Interrupt)          $58
            SEI (SEt Interrupt)            $78
            CLV (CLear oVerflow)           $B8
            CLD (CLear Decimal)            $D8 <- ?
            SED (SEt Decimal)              $F8 <- ?
         */
        public static class Flags
        {
         
            public static class CLC
            {
                public const byte Opcode = 0x18;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class SEC
            {
                public const byte Opcode = 0x38;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class CLI
            {
                public const byte Opcode = 0x58;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class SEI
            {
                public const byte Opcode = 0x78;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class CLV
            {
                public const byte Opcode = 0xB8;
                public const int Length = 1;
                public const int Cycles = 2;
            }

            public static class CLD
            {
                public const byte Opcode = 0xD8;
                public const int Length = 1;
                public const int Cycles = 2;
            }
        }
    }
}

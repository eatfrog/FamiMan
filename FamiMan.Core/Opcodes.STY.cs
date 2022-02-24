using System.Collections.Generic;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {

        /*
         *  MODE           SYNTAX       HEX LEN TIM
            Zero Page     STY $44       $84  2   3
            Zero Page,X   STY $44,X     $94  2   4
            Absolute      STY $4400     $8C  3   4
        */
        public static class STY
        {
            public static class ZeroPage
            {
                public const byte Opcode = 0x84;
                public const int Length = 2;
                public const int Cycles = 3;
            }

            public static class ZeroPage_Y
            {
                public const byte Opcode = 0x94;
                public const int Length = 2;
                public const int Cycles = 4;
            }
            public static class Absolute
            {
                public const byte Opcode = 0x8C;
                public const int Length = 3;
                public const int Cycles = 4;
            }

            public static Dictionary<int, byte> Lengths = new Dictionary<int, byte>() {
                { ZeroPage.Opcode, ZeroPage.Length },
                { ZeroPage_Y.Opcode, ZeroPage_Y.Length },
                { Absolute.Opcode, Absolute.Length },

            };

            public static Dictionary<int, byte> Cycles = new Dictionary<int, byte>() {
                { ZeroPage.Opcode, ZeroPage.Cycles },
                { ZeroPage_Y.Opcode, ZeroPage_Y.Cycles },
                { Absolute.Opcode, Absolute.Cycles },
            };
        }
    }
}

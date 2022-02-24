using System.Collections.Generic;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class STX
        {
            public static class ZeroPage
            {
                public const byte Opcode = 0x86;
                public const int Cycles = 3;
                public const int Length = 2;
            }

            public static class ZeroPage_Y
            {
                public const byte Opcode = 0x96;
                public const int Cycles = 4;
                public const int Length = 2;
            }
            public static class Absolute
            {
                public const byte Opcode = 0x8E;
                public const int Cycles = 4;
                public const int Length = 3;
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

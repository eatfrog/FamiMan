using System.Collections.Generic;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class ADC
        {
            public static class Immediate
            {
                public const byte Opcode = 0x69;
                public const int Cycles = 2;
                public const int Length = 2;
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0x65;
                public const int Cycles = 3;
                public const int Length = 2;
            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0x75;
                public const int Cycles = 4;
                public const int Length = 2;
            }

            public static class Absolute
            {
                public const byte Opcode = 0x6D;
                public const int Cycles = 4;
                public const int Length = 3;
            }

            public static class Absolute_X
            {
                public const byte Opcode = 0x7D;
                public const int Length = 3;
                public const int Cycles = 4;
            }

            public static class Absolute_Y
            {
                public const byte Opcode = 0x79;
                public const int Length = 3;
                public const int Cycles = 4;
            }

            public static class Indirect_X
            {
                public const byte Opcode = 0x61;
                public const int Length = 2;
                public const int Cycles = 6;
            }

            public static class Indirect_Y
            {
                public const byte Opcode = 0x71;
                public const int Length = 2;
                public const int Cycles = 5; // Add cycle if page boundary is crossed
            }

            public static Dictionary<int, byte> Lengths = new Dictionary<int, byte>() {
                { Immediate.Opcode, Immediate.Length },
                { ZeroPage.Opcode, ZeroPage.Length },
                { ZeroPage_X.Opcode, ZeroPage_X.Length },
                { Absolute.Opcode, Absolute.Length },
                { Absolute_X.Opcode, Absolute_X.Length },
                { Absolute_Y.Opcode, Absolute_Y.Length },
                { Indirect_X.Opcode, Indirect_X.Length },
                { Indirect_Y.Opcode, Indirect_Y.Length },
            };

            public static Dictionary<int, byte> Cycles = new Dictionary<int, byte>() {
                { Immediate.Opcode, Immediate.Cycles },
                { ZeroPage.Opcode, ZeroPage.Cycles },
                { ZeroPage_X.Opcode, ZeroPage_X.Cycles },
                { Absolute.Opcode, Absolute.Cycles },
                { Absolute_X.Opcode, Absolute_X.Cycles },
                { Absolute_Y.Opcode, Absolute_Y.Cycles },
                { Indirect_X.Opcode, Indirect_X.Cycles },
                { Indirect_Y.Opcode, Indirect_Y.Cycles },
            };
        }
    }
}

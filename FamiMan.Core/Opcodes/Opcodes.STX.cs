using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class STX
        {

            public static Dictionary<byte, int> Cycles;

            static STX()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0x86;
                public const int Cycles = 3;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;

            }

            public static class ZeroPage_Y
            {
                public const byte Opcode = 0x96;
                public const int Cycles = 4;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }
            public static class Absolute
            {
                public const byte Opcode = 0x8E;
                public const int Cycles = 4;
                public const int Length = 3;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }
        }
    }
}

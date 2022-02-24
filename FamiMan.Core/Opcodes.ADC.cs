using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class ADC
        {

            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static ADC()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const byte Opcode = 0x69;
                public const int Cycles = 2;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0x65;
                public const int Cycles = 3;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;

            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0x75;
                public const int Cycles = 4;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;

            }

            public static class Absolute
            {
                public const byte Opcode = 0x6D;
                public const int Cycles = 4;
                public const int Length = 3;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;

            }

            public static class Absolute_X
            {
                public const byte Opcode = 0x7D;
                public const int Length = 3;
                public const int Cycles = 4;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;

            }

            public static class Absolute_Y
            {
                public const byte Opcode = 0x79;
                public const int Length = 3;
                public const int Cycles = 4;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;

            }

            public static class IndexedIndirect
            {
                public const byte Opcode = 0x61;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndexedIndirect;

            }

            public static class IndirectIndexed
            {
                public const byte Opcode = 0x71;
                public const int Length = 2;
                public const int Cycles = 5; // Add cycle if page boundary is crossed
                public const MemoryMappingMode Mode = MemoryMappingMode.IndirectIndexed;

            }
        }
    }
}

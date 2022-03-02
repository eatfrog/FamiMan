using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class DEC
        {
            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static DEC()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            /*
                MODE           SYNTAX       HEX LEN TIM
                Zero Page     DEC $44       $C6  2   5
                Zero Page,X   DEC $44,X     $D6  2   6
                Absolute      DEC $4400     $CE  3   6
                Absolute,X    DEC $4400,X   $DE  3   7
            */

            public static class Absolute
            {
                public const byte Opcode = 0xCE;
                public const int Length = 3;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const byte Opcode = 0xDE;
                public const int Length = 3;
                public const int Cycles = 7;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0xC6;
                public const int Length = 2;
                public const int Cycles = 5;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0xD6;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class LSR
        {
            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static LSR()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            /*
                MODE           SYNTAX       HEX LEN TIM
                Accumulator   LSR A         $4A  1   2
                Zero Page     LSR $44       $46  2   5
                Zero Page,X   LSR $44,X     $56  2   6
                Absolute      LSR $4400     $4E  3   6
                Absolute,X    LSR $4400,X   $5E  3   7
            */

            public static class Accumulator
            {
                public const byte Opcode = 0x4A;
                public const int Length = 1;
                public const int Cycles = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class Absolute
            {
                public const byte Opcode = 0x4E;
                public const int Length = 3;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const byte Opcode = 0x5E;
                public const int Length = 3;
                public const int Cycles = 7;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0x46;
                public const int Length = 2;
                public const int Cycles = 5;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0x56;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }
        }
    }
}
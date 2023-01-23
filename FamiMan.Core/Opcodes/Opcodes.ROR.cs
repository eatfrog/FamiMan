using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class ROR
        {
            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static ROR()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            /*
                MODE           SYNTAX       HEX LEN TIM
                Accumulator   ROR A         $6A  1   2
                Zero Page     ROR $44       $66  2   5
                Zero Page,X   ROR $44,X     $76  2   6
                Absolute      ROR $4400     $6E  3   6
                Absolute,X    ROR $4400,X   $7E  3   7
            */

            public static class Accumulator
            {
                public const byte Opcode = 0x6A;
                public const int Length = 1;
                public const int Cycles = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }
            public static class ZeroPage
            {
                public const byte Opcode = 0x66;
                public const int Length = 2;
                public const int Cycles = 5;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0x76;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const byte Opcode = 0x6E;
                public const int Length = 3;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const byte Opcode = 0x7E;
                public const int Length = 3;
                public const int Cycles = 7;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }
        }
    }
}
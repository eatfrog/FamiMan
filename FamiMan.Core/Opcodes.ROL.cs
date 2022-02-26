using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class ROL
        {
            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static ROL()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            /*
                MODE           SYNTAX       HEX LEN TIM
                Accumulator   ROL A         $2A  1   2
                Zero Page     ROL $44       $26  2   5
                Zero Page,X   ROL $44,X     $36  2   6
                Absolute      ROL $4400     $2E  3   6
                Absolute,X    ROL $4400,X   $3E  3   7
            */

            public static class Accumulator
            {
                public const byte Opcode = 0x2A;
                public const int Length = 1;
                public const int Cycles = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }
            public static class ZeroPage
            {
                public const byte Opcode = 0x26;
                public const int Length = 2;
                public const int Cycles = 5;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0x36;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const byte Opcode = 0x2E;
                public const int Length = 3;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const byte Opcode = 0x3E;
                public const int Length = 3;
                public const int Cycles = 7;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }
        }
    }
}
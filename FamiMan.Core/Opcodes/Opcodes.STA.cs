using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {


        /*
            MODE           SYNTAX       HEX LEN TIM
            Zero Page     STA $44       $85  2   3
            Zero Page,X   STA $44,X     $95  2   4
            Absolute      STA $4400     $8D  3   4
            Absolute,X    STA $4400,X   $9D  3   5
            Absolute,Y    STA $4400,Y   $99  3   5
            Indirect,X    STA ($44,X)   $81  2   6
            Indirect,Y    STA ($44),Y   $91  2   6
        */

        public static class STA
        {

            public static Dictionary<byte, int> Cycles;

            static STA()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class ZeroPage
            {
                public const int Length = 2;
                public const int Cycles = 3;
                public const byte Opcode = 0x85;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const int Length = 2;
                public const int Cycles = 4;
                public const byte Opcode = 0x95;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const int Length = 3;
                public const int Cycles = 4;
                public const byte Opcode = 0x8D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const int Length = 3;
                public const int Cycles = 5;
                public const byte Opcode = 0x9D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_Y
            {
                public const int Length = 3;
                public const int Cycles = 5;
                public const byte Opcode = 0x99;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class IndexedIndirect
            {
                public const byte Opcode = 0x81;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndexedIndirect;
            }

            public static class IndirectIndexed
            {
                public const byte Opcode = 0x91;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndirectIndexed;
            }


        }
    }
}

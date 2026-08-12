using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {


        /*
            MODE           SYNTAX       HEX LEN TIM
            Immediate     LDA #$44      $A9  2   2
            Zero Page     LDA $44       $A5  2   3
            Zero Page,X   LDA $44,X     $B5  2   4
            Absolute      LDA $4400     $AD  3   4
            Absolute,X    LDA $4400,X   $BD  3   4+
            Absolute,Y    LDA $4400,Y   $B9  3   4+
            Indirect,X    LDA ($44,X)   $A1  2   6
            Indirect,Y    LDA ($44),Y   $B1  2   5+
        */

        public static class LDA
        {

            public static Dictionary<byte, int> Cycles;

            static LDA()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const int Length = 2;
                public const int Cycles = 2;
                public const byte Opcode = 0xA9;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const int Length = 2;
                public const int Cycles = 3;
                public const byte Opcode = 0xA5;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const int Length = 2;
                public const int Cycles = 4;
                public const byte Opcode = 0xB5;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const int Length = 3;
                public const int Cycles = 4;
                public const byte Opcode = 0xAD;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const int Length = 3;
                public const int Cycles = 4; // +
                public const byte Opcode = 0xBD;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_Y
            {
                public const int Length = 3;
                public const int Cycles = 4; // +
                public const byte Opcode = 0xB9;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class IndexedIndirect
            {
                public const byte Opcode = 0xA1;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndexedIndirect;
            }

            public static class IndirectIndexed
            {
                public const byte Opcode = 0xB1;
                public const int Length = 2;
                public const int Cycles = 5; // +
                public const MemoryMappingMode Mode = MemoryMappingMode.IndirectIndexed;
            }


        }
    }
}

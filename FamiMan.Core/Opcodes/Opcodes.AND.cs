using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {


        /*
         *  MODE           SYNTAX       HEX LEN TIM
            Immediate     AND #$44      $29  2   2
            Zero Page     AND $44       $25  2   3
            Zero Page,X   AND $44,X     $35  2   4
            Absolute      AND $4400     $2D  3   4
            Absolute,X    AND $4400,X   $3D  3   4+
            Absolute,Y    AND $4400,Y   $39  3   4+
            Indirect,X    AND ($44,X)   $21  2   6
            Indirect,Y    AND ($44),Y   $31  2   5+
        */

        public static class AND
        {

            public static Dictionary<byte, int> Cycles;

            static AND()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const int Cycles = 2;
                public const int Length = 2;
                public const byte Opcode = 0x29;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const int Cycles = 3;
                public const int Length = 2;
                public const byte Opcode = 0x25;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const int Cycles = 3;
                public const int Length = 2;
                public const byte Opcode = 0x35;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x2D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x3D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_Y
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x39;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class IndexedIndirect
            {
                public const byte Opcode = 0x21;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndexedIndirect;
            }

            public static class IndirectIndexed
            {
                public const byte Opcode = 0x31;
                public const int Length = 2;
                public const int Cycles = 5; // Add cycle if page boundary is crossed
                public const MemoryMappingMode Mode = MemoryMappingMode.IndirectIndexed;
            }


        }
    }
}

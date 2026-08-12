using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {


        /*
            MODE           SYNTAX       HEX LEN TIM
            Immediate     ORA #$44      $09  2   2
            Zero Page     ORA $44       $05  2   3
            Zero Page,X   ORA $44,X     $15  2   4
            Absolute      ORA $4400     $0D  3   4
            Absolute,X    ORA $4400,X   $1D  3   4+
            Absolute,Y    ORA $4400,Y   $19  3   4+
            Indirect,X    ORA ($44,X)   $01  2   6
            Indirect,Y    ORA ($44),Y   $11  2   5+
        */

        public static class ORA
        {

            public static Dictionary<byte, int> Cycles;

            static ORA()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const int Cycles = 2;
                public const int Length = 2;
                public const byte Opcode = 0x09;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const int Cycles = 3;
                public const int Length = 2;
                public const byte Opcode = 0x05;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const int Cycles = 3;
                public const int Length = 2;
                public const byte Opcode = 0x15;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x0D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x1D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_Y
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x19;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class IndexedIndirect
            {
                public const byte Opcode = 0x01;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndexedIndirect;
            }

            public static class IndirectIndexed
            {
                public const byte Opcode = 0x11;
                public const int Length = 2;
                public const int Cycles = 5; // Add cycle if page boundary is crossed
                public const MemoryMappingMode Mode = MemoryMappingMode.IndirectIndexed;
            }


        }
    }
}

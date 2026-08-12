using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class SBC
        {
            /*
             * MODE           SYNTAX       HEX LEN TIM
                Immediate     SBC #$44      $E9  2   2
                Zero Page     SBC $44       $E5  2   3
                Zero Page,X   SBC $44,X     $F5  2   4
                Absolute      SBC $4400     $ED  3   4
                Absolute,X    SBC $4400,X   $FD  3   4+
                Absolute,Y    SBC $4400,Y   $F9  3   4+
                Indirect,X    SBC ($44,X)   $E1  2   6
                Indirect,Y    SBC ($44),Y   $F1  2   5+

                + add 1 cycle if page boundary crossed
            */

            public static Dictionary<byte, int> Cycles;

            static SBC()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const byte Opcode = 0xE9;
                public const int Cycles = 2;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0xE5;
                public const int Cycles = 3;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;

            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0xF5;
                public const int Cycles = 4;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;

            }

            public static class Absolute
            {
                public const byte Opcode = 0xED;
                public const int Cycles = 4;
                public const int Length = 3;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;

            }

            public static class Absolute_X
            {
                public const byte Opcode = 0xFD;
                public const int Length = 3;
                public const int Cycles = 4; // Add cycle if page boundary is crossed
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;

            }

            public static class Absolute_Y
            {
                public const byte Opcode = 0xF9;
                public const int Length = 3;
                public const int Cycles = 4; // Add cycle if page boundary is crossed
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;

            }

            public static class IndexedIndirect
            {
                public const byte Opcode = 0xE1;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndexedIndirect;

            }

            public static class IndirectIndexed
            {
                public const byte Opcode = 0xF1;
                public const int Length = 2;
                public const int Cycles = 5; // Add cycle if page boundary is crossed
                public const MemoryMappingMode Mode = MemoryMappingMode.IndirectIndexed;

            }
        }
    }
}

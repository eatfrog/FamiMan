using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {


        /*
         *  MODE           SYNTAX       HEX LEN TIM
            Immediate     EOR #$44      $49  2   2
            Zero Page     EOR $44       $45  2   3
            Zero Page,X   EOR $44,X     $55  2   4
            Absolute      EOR $4400     $4D  3   4
            Absolute,X    EOR $4400,X   $5D  3   4+
            Absolute,Y    EOR $4400,Y   $59  3   4+
            Indirect,X    EOR ($44,X)   $41  2   6
            Indirect,Y    EOR ($44),Y   $51  2   5+
        */

        public static class EOR // aka XOR
        {

            public static Dictionary<byte, int> Cycles;

            static EOR()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const int Cycles = 2;
                public const int Length = 2;
                public const byte Opcode = 0x49;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const int Cycles = 3;
                public const int Length = 2;
                public const byte Opcode = 0x45;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const int Cycles = 4;
                public const int Length = 2;
                public const byte Opcode = 0x55;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x4D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x5D;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_Y
            {
                public const int Cycles = 4;
                public const int Length = 3;
                public const byte Opcode = 0x59;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class IndexedIndirect
            {
                public const byte Opcode = 0x41;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.IndexedIndirect;
            }

            public static class IndirectIndexed
            {
                public const byte Opcode = 0x51;
                public const int Length = 2;
                public const int Cycles = 5; // Add cycle if page boundary is crossed
                public const MemoryMappingMode Mode = MemoryMappingMode.IndirectIndexed;
            }


        }
    }
}

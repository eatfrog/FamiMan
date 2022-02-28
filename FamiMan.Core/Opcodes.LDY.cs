using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {


        /*
            MODE           SYNTAX       HEX LEN TIM
            Immediate     LDY #$44      $A0  2   2
            Zero Page     LDY $44       $A4  2   3
            Zero Page,X   LDY $44,X     $B4  2   4
            Absolute      LDY $4400     $AC  3   4
            Absolute,X    LDY $4400,X   $BC  3   4+
        */

        public static class LDY
        {

            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static LDY()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const int Length = 2;
                public const int Cycles = 2;
                public const byte Opcode = 0xA0;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const int Length = 2;
                public const int Cycles = 3;
                public const byte Opcode = 0xA4;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const int Length = 2;
                public const int Cycles = 4;
                public const byte Opcode = 0xB4;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const int Length = 3;
                public const int Cycles = 4;
                public const byte Opcode = 0xAC;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const int Length = 3;
                public const int Cycles = 4; // +
                public const byte Opcode = 0xBC;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

        }
    }
}

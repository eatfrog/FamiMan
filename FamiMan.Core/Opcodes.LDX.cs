using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {


        /*
            Immediate     LDX #$44      $A2  2   2
            Zero Page     LDX $44       $A6  2   3
            Zero Page,Y   LDX $44,Y     $B6  2   4
            Absolute      LDX $4400     $AE  3   4
            Absolute,Y    LDX $4400,Y   $BE  3   4+
        */

        public static class LDX
        {

            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static LDX()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const int Length = 2;
                public const int Cycles = 2;
                public const byte Opcode = 0xA2;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const int Length = 2;
                public const int Cycles = 3;
                public const byte Opcode = 0xA6;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }
            public static class ZeroPage_Y
            {
                public const int Length = 2;
                public const int Cycles = 4;
                public const byte Opcode = 0xB6;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const int Length = 3;
                public const int Cycles = 4;
                public const byte Opcode = 0xAE;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_Y
            {
                public const int Length = 3;
                public const int Cycles = 4; // +
                public const byte Opcode = 0xBE;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }
        }
    }
}

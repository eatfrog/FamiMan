using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class INC
        {

            public static Dictionary<byte, int> Cycles;

            static INC()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            /*
                MODE           SYNTAX       HEX LEN TIM
                Zero Page     INC $44       $E6  2   5
                Zero Page,X   INC $44,X     $F6  2   6
                Absolute      INC $4400     $EE  3   6
                Absolute,X    INC $4400,X   $FE  3   7
            */

            public static class Absolute
            {
                public const byte Opcode = 0xEE;
                public const int Length = 3;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Absolute_X
            {
                public const byte Opcode = 0xFE;
                public const int Length = 3;
                public const int Cycles = 7;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0xE6;
                public const int Length = 2;
                public const int Cycles = 5;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0xF6;
                public const int Length = 2;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }
        }
    }
}
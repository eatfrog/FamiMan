using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {

        /*
         *  MODE           SYNTAX       HEX LEN TIM
            Zero Page     STY $44       $84  2   3
            Zero Page,X   STY $44,X     $94  2   4
            Absolute      STY $4400     $8C  3   4
        */
        public static class STY
        {
            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static STY()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0x84;
                public const int Length = 2;
                public const int Cycles = 3;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class ZeroPage_X
            {
                public const byte Opcode = 0x94;
                public const int Length = 2;
                public const int Cycles = 4;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }
            public static class Absolute
            {
                public const byte Opcode = 0x8C;
                public const int Length = 3;
                public const int Cycles = 4;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

        }
    }
}

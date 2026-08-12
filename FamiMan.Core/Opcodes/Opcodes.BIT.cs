using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {

        /*
            MODE           SYNTAX       HEX LEN TIM
            Zero Page     BIT $44       $24  2   3
            Absolute      BIT $4400     $2C  3   4
        */
        public static class BIT
        {

            public static Dictionary<byte, int> Cycles;

            static BIT()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0x24;
                public const int Length = 2;
                public const int Cycles = 3;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;
            }

            public static class Absolute
            {
                public const byte Opcode = 0x2C;
                public const int Length = 3;
                public const int Cycles = 4;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

        }
    }
}

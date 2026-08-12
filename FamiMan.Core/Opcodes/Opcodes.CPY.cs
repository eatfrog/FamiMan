using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class CPY
        {
            /*
                MODE           SYNTAX       HEX LEN TIM
                Immediate     CPY #$44      $C0  2   2
                Zero Page     CPY $44       $C4  2   3
                Absolute      CPY $4400     $CC  3   4
            */

            public static Dictionary<byte, int> Cycles;

            static CPY()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Immediate
            {
                public const byte Opcode = 0xC0;
                public const int Cycles = 2;
                public const int Length = 2;
                public const MemoryMappingMode Mode = MemoryMappingMode.Immediate;
            }

            public static class ZeroPage
            {
                public const byte Opcode = 0xC4;
                public const int Length = 2;
                public const int Cycles = 3;
                public const MemoryMappingMode Mode = MemoryMappingMode.ZeroPage;

            }

            public static class Absolute
            {
                public const byte Opcode = 0xCC;
                public const int Length = 3;
                public const int Cycles = 4;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;

            }
        }
    }
}

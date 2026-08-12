using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class JMP
        {
            /*
                MODE           SYNTAX       HEX LEN TIM
                Absolute      JMP $5597     $4C  3   3
                Indirect      JMP ($5597)   $6C  3   5
            */

            public static Dictionary<byte, int> Cycles;

            static JMP()
            {
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            public static class Absolute
            {
                public const byte Opcode = 0x4C;
                public const int Length = 3;
                public const int Cycles = 3;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }

            public static class Indirect
            {
                public const byte Opcode = 0x6C;
                public const int Length = 3;
                public const int Cycles = 5;
            }
        }

        public static class JSR
        {
            public static class Absolute
            {
                public const byte Opcode = 0x20;
                public const int Length = 3;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }
        }

        public static class RTS
        {
            /*
             *  MODE           SYNTAX       HEX LEN TIM
                Implied       RTS           $60  1   6
            */

            public static class Absolute
            {
                public const byte Opcode = 0x60;
                public const int Length = 1;
                public const int Cycles = 6;
            }
        }
    }
}

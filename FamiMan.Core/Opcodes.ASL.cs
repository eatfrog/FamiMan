using System;
using System.Collections.Generic;
using System.Linq;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        public static class ASL
        {
            public static Dictionary<byte, int> Lengths;

            public static Dictionary<byte, int> Cycles;

            static ASL()
            {
                Lengths = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Length").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
                Cycles = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, int>((byte)t.GetField("Opcode").GetValue(t), (int)t.GetField("Cycles").GetValue(t))).ToDictionary(x => x.Item1, x => x.Item2);
            }

            /*
             *  MODE           SYNTAX       HEX LEN TIM
                Accumulator   ASL A         $0A  1   2
                Zero Page     ASL $44       $06  2   5
                Zero Page,X   ASL $44,X     $16  2   6
                Absolute      ASL $4400     $0E  3   6
                Absolute,X    ASL $4400,X   $1E  3   7
            */
            public static class Absolute
            {
                public const byte Opcode = 0x0E;
                public const int Length = 3;
                public const int Cycles = 6;
                public const MemoryMappingMode Mode = MemoryMappingMode.Absolute;
            }
        }
    }
}
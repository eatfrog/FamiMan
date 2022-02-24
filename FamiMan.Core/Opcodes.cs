using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        private static Dictionary<byte, Type> _opcodes;
        static Opcodes()
        {
            _opcodes = typeof(Opcodes).GetNestedTypes().SelectMany(x => x.GetNestedTypes()).Select(t => new Tuple<byte, Type>((byte)t.GetField("Opcode").GetValue(t), t.UnderlyingSystemType)).ToDictionary(x => x.Item1, x => x.Item2);
        }

        public static class STX
        {
            public static class ZeroPage
            {
                public const byte Opcode = 0x86;
                public const int Cycles = 2;
                public const int Length = 2;
            }

            public static Dictionary<int, byte> Lengths = new Dictionary<int, byte>() {
                { ZeroPage.Opcode, ZeroPage.Length },

            };

            public static Dictionary<int, byte> Cycles = new Dictionary<int, byte>() {
                { ZeroPage.Opcode, ZeroPage.Cycles },
            };
        }

        public static Type Find(byte v)
        {
            return _opcodes[v];
        }
    }
}

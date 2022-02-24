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

        public static Type Find(byte v)
        {
            return _opcodes[v];
        }
    }
    public enum MemoryMappingMode
    {
        Immediate,
        ZeroPage,
        Absolute,
        IndexedIndirect, // Addr + X
        IndirectIndexed  // Ptr at addr + offset Y
    }
}
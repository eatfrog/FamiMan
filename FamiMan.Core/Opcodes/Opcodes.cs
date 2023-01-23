using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        private static Dictionary<byte, Opcode> _opcodes;
        static Opcodes()
        {
            _opcodes = typeof(Opcodes).GetNestedTypes()
                .SelectMany(x => x.GetNestedTypes())
                .Select(t => new Tuple<byte, Opcode>((byte)t.GetField("Opcode").GetValue(t), new Opcode 
                    { 
                        BackingType = t.UnderlyingSystemType,
                        MemoryMappingMode = t.GetMemoryMappingMode(),
                        OpcodeVersionName = t.Name,
                        OpcodeName = t.Name.Length == 3 ? t.Name : t.UnderlyingSystemType.ReflectedType.Name,
                        Cycles = t.GetCycles()
                    }))
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        public static Opcode Find(byte v)
        {
            return _opcodes[v];
        }

        public static class BRK
        {
            public static class BRK_00
            {
                public const byte Opcode = 0x00;
                public const int Length = 1;
                public const int Cycles = 1;
            }
        }
    }

    public class Opcode
    {
        public Type BackingType { get; set; }

        public string OpcodeName { get; set; }

        public string OpcodeVersionName { get; set; }

        public MemoryMappingMode MemoryMappingMode { get; set; }

        public int Cycles { get; set; }
    }
    public enum MemoryMappingMode
    {
        Immediate,
        ZeroPage,
        Absolute,
        IndexedIndirect, // Addr + X
        IndirectIndexed,  // Ptr at addr + offset Y
        None
    }

    
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FamiMan.Core
{
    public static class ExtensionMethods
    {
        public static byte GetOpcode(this Type t)
        {
            return (byte)t.GetField("Opcode").GetValue(t);
        }

        public static int GetLength(this Type t)
        {
            return (int)t.GetField("Length").GetValue(t);
        }

        public static int GetCycles(this Type t)
        {
            return (int)t.GetField("Cycles").GetValue(t);
        }

        public static bool IsKil(this Opcode t) => t.OpcodeName == "KIL";

        public static bool IsNop(this Opcode t) => t.OpcodeName == "NOP";

        public static bool IsBrk(this Opcode t) => t.OpcodeName == "BRK" || t.OpcodeName == "BRK_00";

        public static MemoryMappingMode GetMemoryMappingMode(this Type t)
        {
            var temp = t.GetField("Mode");
            if (temp == null) return MemoryMappingMode.None;
            return (MemoryMappingMode) temp.GetValue(t);
        }
    }
}

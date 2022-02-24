using System;
using System.Collections.Generic;
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
    }
}

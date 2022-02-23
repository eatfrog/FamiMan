using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public static class Constants
    {
        public const int KB = 1024;

        public static class STXSTY
        {
            public static readonly Dictionary<byte, byte> Length = new Dictionary<byte, byte>()
            {
                { 0x86, 2 },
                { 0x96, 2 },
                { 0x84, 2 },
                { 0x94, 2 },
                { 0x8E, 3 },
                { 0x8C, 3 },
            };

            public static readonly Dictionary<byte, byte> Cycles = new Dictionary<byte, byte>()
            {
                { 0x86, 3 },
                { 0x96, 4 },
                { 0x8E, 4 },
                { 0x84, 3 },
                { 0x94, 4 },
                { 0x8C, 4 },
            };
        }
    }
}

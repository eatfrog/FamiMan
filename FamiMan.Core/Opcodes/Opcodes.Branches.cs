using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public partial class Opcodes
    {
        // A branch not taken requires two machine cycles.
        // Add one if the branch is taken and add one more if the branch crosses a page boundary.
        public static class Branches
        {
            public static class BCC
            {
                public const byte Opcode = 0x90;
                public const int Length = 2;
                public const int Cycles = 2;
            }

            public static class BCS
            {
                public const byte Opcode = 0xB0;
                public const int Length = 2;
                public const int Cycles = 2;
            }

            public static class BEQ
            {
                public const byte Opcode = 0xF0;
                public const int Length = 2;
                public const int Cycles = 2;
            }

            public static class BNE
            {
                public const byte Opcode = 0xD0;
                public const int Length = 2;
                public const int Cycles = 2;
            }

            public static class BMI
            {
                public const byte Opcode = 0x30;
                public const int Length = 2;
                public const int Cycles = 2;
            }

            public static class BPL
            {
                public const byte Opcode = 0x10;
                public const int Length = 2;
                public const int Cycles = 2;
            }

            public static class BVC
            {
                public const byte Opcode = 0x50;
                public const int Length = 2;
                public const int Cycles = 2;
            }

            public static class BVS
            {
                public const byte Opcode = 0x70;
                public const int Length = 2;
                public const int Cycles = 2;
            }
        }
    }
}

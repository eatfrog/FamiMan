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
            public static byte Length(byte instruction)
            {

                if (instruction == 0x86 || // STX
                    instruction == 0x96 ||
                    instruction == 0x84 || // STY
                    instruction == 0x94)
                    return 2;

                if (instruction == 0x8E || // STX
                    instruction == 0x8C)   // STY
                    return 3;

                throw new NotImplementedException("Got instruction: " + instruction);
            }

        }

        public static class ADC
        {
            public static byte Length(byte instruction)
            {

                if (instruction == 0x69 ||
                    instruction == 0x65 ||
                    instruction == 0x75 ||
                    instruction == 0x61 ||
                    instruction == 0x71)
                    return 2;

                if (instruction == 0x6D ||
                    instruction == 0x7D ||
                    instruction == 0x79)
                    return 3;

                throw new NotImplementedException("Got instruction: " + instruction);
            }
        }

    }
}

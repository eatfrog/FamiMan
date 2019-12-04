using System;

namespace FamiMan.Core
{
    public class Cpu : ICpu
    {
        public Cpu(Bus b)
        {
            _bus = b;
            b.Cpu = this;
        }

        private Bus _bus;

        public ushort PC = new ushort();
        public byte A = new byte();
        public byte X = new byte();
        public byte Y = new byte();
        public byte S = new byte();

        /// <summary>
        /// Status registers
        /// </summary>                                      //  0   1   2   3   4   5   6
        public StatusRegisters P = new StatusRegisters();   // 	N	V	B	D	I	Z	C

        private const byte NEGATIVE = 0;
        private const byte OVERFLOW = 1;
        private const byte Z = 5;
        private const byte CARRY = 6;

        private const byte ONE = 1;
        private const byte ZERO = 0;

        public void Tick()
        {
            byte length = ExecuteNextInstruction();
            PC += length;
        }

        private byte ExecuteNextInstruction()
        {
            var instruction = _bus[PC];
            byte len = 1;
            switch (instruction)
            {
                case 0x69: // ADC #$44
                case 0x6D: // ADC $4400
                    byte val = 0;
                    ushort addr = 0;
                    if (instruction == 0x69)
                    {
                        len = 2; addr = PC; addr++;
                        val = _bus[addr];
                    }
                    if (instruction == 0x6D)
                    {
                        addr = PC; addr++;
                        addr = (ushort)(_bus[addr] + (_bus[(byte)(addr + 1)] << 8));
                        val = _bus[addr];
                        len = 3;
                    }

                    ADC(addr, val);

                    break;
                default:
                    break;
            }

            return len;
        }

        private void ADC(ushort addr, byte val)
        {
            int temp = A;
            A += P.Carry ? ONE : ZERO;
            if (A + val > 255)
            {
                A += (byte)(_bus[addr] - 256);
                P.Carry = true;
            }
            else
            {
                A += val;
                P.Carry = false;
            }

            P.Overflow = !(temp >> 7 == A >> 7);
            P.Negative = A >> 7 != 0;
            P.Zero = A == 0;
        }

        public class StatusRegisters
        {
            private readonly bool[] _s = new bool[7];

            public bool Negative
            {
                get => _s[NEGATIVE];
                set => _s[NEGATIVE] = value;
            }

            public bool Carry
            {
                get
                {
                    var ret = _s[CARRY];
                    _s[CARRY] = false;
                    return ret;
                }
                set => _s[CARRY] = value;
            }

            public bool Overflow
            {
                get => _s[OVERFLOW];
                set => _s[OVERFLOW] = value;
            }

            public bool Zero
            {
                get => _s[Z];
                set => _s[Z] = value;
            }
        }
    }
}

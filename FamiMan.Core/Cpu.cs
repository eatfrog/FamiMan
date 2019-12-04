using System;

namespace FamiMan.Core
{
    /// <summary>
    /// The Ricoh 2A03 Cpu
    /// </summary>
    public class Cpu : ICpu
    {
        public Cpu(Bus b)
        {
            _bus = b;
            b.Cpu = this;
        }

        private Bus _bus;

        public ushort PC = new ushort();
        public byte A = new byte(); // Accumulator
        public byte X = new byte(); // Gen purp reg X
        public byte Y = new byte(); // Gen purp reg Y
        public byte S = new byte(); // Stack pointer

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
            byte len = 0;
            ushort addr = PC; addr++;

            switch (instruction)
            {
                case 0x69: // ADC #$44  - Immediate
                case 0x6D: // ADC $4400 - Absolute
                case 0x65: // ADC $44   - Zero page
                    len = Constants.ADC.Length(instruction);
                    if (instruction == 0x6D)
                        addr = GetAbsolute(addr);
                    else if (instruction == 0x65)
                        addr = _bus[addr];

                    byte val = _bus[addr];
                    ADC(addr, val);
                    break;
                case 0x86: // STX $44       - ZP
                case 0x84: // STY $44       - ZP
                case 0x96: // STX $44, Y    - ZP + Y
                case 0x94: // STY $44, X    - ZP + X
                case 0x8E: // STX $4400     - Abs
                case 0x8C: // STY $4400     - ABS
                    len = Constants.STXSTY.Length(instruction);
                    if (len == 2)
                        addr = _bus[addr];

                    if (instruction == 0x86) // ZP
                        X = _bus[addr];
                    else if (instruction == 0x84)
                        Y = _bus[addr];
                    else if (instruction == 0x96) // + Y
                    { 
                        addr += Y;
                        X = _bus[addr];
                    }
                    else if (instruction == 0x94) // + X
                    {
                        addr += X;
                        Y = _bus[addr];
                    }
                    else if (instruction == 0x8E) // Abs
                    {
                        addr = GetAbsolute(addr);
                        X = _bus[addr];
                    }
                    else if (instruction == 0x8C) // Abs
                    {
                        addr = GetAbsolute(addr);
                        Y = _bus[addr];
                    }
                    break;
                default:
                    break;
            }

            return len;
        }

        private void ADC(ushort addr, byte val)
        {
            A += P.Carry ? ONE : ZERO;

            int temp = A;
            if (A + val > 255)
                A += (byte)(_bus[addr] - 256);
            else
                A += val;

            P.Carry = A < val;
            P.Overflow = !(temp >> 7 == A >> 7);
            P.Negative = A >> 7 != 0;
            P.Zero = A == 0;
        }

        private ushort GetAbsolute(ushort addr) => (ushort)(_bus[addr] + (_bus[(byte)(addr + 1)] << 8));

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

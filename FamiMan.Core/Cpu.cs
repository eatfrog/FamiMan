using System;
using static FamiMan.Core.Constants;

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

        private long _ticks = 0;

        public void Tick()
        {
            _ticks++;
            ExecuteNextInstruction();
        }

        public void Tick(int ticks)
        {
            for (int i = 0; i < ticks; i++)
                Tick();
        }

        public void Ticks(int num)
        {
            for (int i = 0; i <= num; i++)
                Tick();
        }

        private void ExecuteNextInstruction()
        {
            // TODO: take into consideration the cycles needed for a particular opcode
            var i = _bus[PC];
            byte len = 0;
            ushort addr = PC; addr++;
            switch (i)
            {
                case Opcodes.ADC.Immediate.Opcode:  // ADC #$44  - Immediate
                case Opcodes.ADC.Absolute.Opcode:   // ADC $4400 - Absolute
                case Opcodes.ADC.ZeroPage.Opcode:   // ADC $44   - Zero page
                case Opcodes.ADC.ZeroPage_X.Opcode: // ADC $44, X
                case Opcodes.ADC.Absolute_X.Opcode: // ADC $4400, X
                case Opcodes.ADC.Absolute_Y.Opcode: // ADC $4400, Y
                case Opcodes.ADC.Indirect_X.Opcode: // ADC($F6, X) - $F6 + X = ptr
                case Opcodes.ADC.Indirect_Y.Opcode: // ADC ($44),Y - $F6 = ptr + Y
                    len = Opcodes.ADC.Lengths[i];
                    if (i == Opcodes.ADC.Absolute.Opcode || i == Opcodes.ADC.Absolute_X.Opcode || i == Opcodes.ADC.Absolute_Y.Opcode)
                        addr = Get16bitAbsolute(addr);
                    else if (i == Opcodes.ADC.ZeroPage.Opcode || i == Opcodes.ADC.ZeroPage_X.Opcode)
                        addr = _bus[addr];
                    else if (i == Opcodes.ADC.Indirect_X.Opcode)
                    {
                        addr = Get16bitAbsolute((ushort)(_bus[addr] + X));
                    }
                    else if (i == Opcodes.ADC.Indirect_Y.Opcode)
                    {
                        addr = (ushort)(Get16bitAbsolute(_bus[addr]) + Y);
                    }
                    if (i == Opcodes.ADC.ZeroPage_X.Opcode || i == Opcodes.ADC.Absolute_X.Opcode) // ZP + X, ABS + X
                        addr += X;
                    if (i == Opcodes.ADC.Absolute_Y.Opcode)
                        addr += Y;
                    byte val = _bus[addr];
                    CalculateADC(addr, val);
                    break;
                case 0x86: // STX $44       - ZP
                case 0x84: // STY $44       - ZP
                case 0x96: // STX $44, Y    - ZP + Y
                case 0x94: // STY $44, X    - ZP + X
                case 0x8E: // STX $4400     - Abs
                case 0x8C: // STY $4400     - ABS
                    _ticks += STXSTY.Cycles[i];
                    len = STXSTY.Length[i];

                    if (len == 2)
                        addr = _bus[addr];
                    else
                        addr = Get16bitAbsolute(addr);

                    if (i == 0x86) // ZP
                        X = _bus[addr];
                    else if (i == 0x84)
                        Y = _bus[addr];
                    else if (i == 0x96) // + Y
                    { 
                        addr += Y;
                        X = _bus[addr];
                    }
                    else if (i == 0x94) // + X
                    {
                        addr += X;
                        Y = _bus[addr];
                    }
                    else if (i == 0x8E)
                        X = _bus[addr];
                    else if (i == 0x8C)
                        Y = _bus[addr];
                    break;
                default:
                    break;
            }

            PC += len;
        }

        private void CalculateADC(ushort addr, byte val)
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

        private ushort Get16bitAbsolute(ushort addr) => (ushort)(_bus[addr] + (_bus[(byte)(addr + 1)] << 8));

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

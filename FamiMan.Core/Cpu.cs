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
        /// </summary>
        public StatusRegisters P = new StatusRegisters(); // 	N	V	B	D	I	Z	C

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
                    len = 2;
                    ushort addr = PC; addr++;
                    if (A + _bus[addr] > 255)
                    {
                        A += (byte)(_bus[addr] - 255);
                        P.Carry = true;
                    }
                    else
                    {
                        A += _bus[addr];
                        A += P.Carry ? ONE : ZERO;
                        P.Carry = false;
                    }
                    break;
                default:
                    break;
            }

            // Parse opcode
            // Execute operation

            return len;
        }

        public class StatusRegisters
        {
            private bool[] _s = new bool[7];
            public bool Carry
            {
                get => _s[CARRY];
                set => _s[CARRY] = value;
            }
        }
    }
}

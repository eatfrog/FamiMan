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
        private long _nextInstruction = 0;
        private bool _waiting = false;

        public void Tick()
        {
            if (!_waiting)
            {
                var opcode = Opcodes.Find(_bus[PC]);
                var cycles = opcode.GetCycles();
                _nextInstruction = cycles - 1;
                _waiting = true;
            }
            else
                _nextInstruction--;

            // TODO: take into consideration the cycles needed for a particular opcode
            // We need to know current instruction here and not in ExecuteNextInstruction()
            if (_nextInstruction == 0)
            {
                _waiting = false;
                ExecuteNextInstruction();
            }

            _ticks++;
        }

        public void Tick(int ticks)
        {
            for (int i = 0; i < ticks; i++)
                Tick();
        }

        private void ExecuteNextInstruction()
        {
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
                case Opcodes.ADC.IndexedIndirect.Opcode: // ADC($F6, X) - $F6 + X = ptr
                case Opcodes.ADC.IndirectIndexed.Opcode: // ADC ($44),Y - $F6 = ptr + Y
                    len = ADC(i, ref addr);
                    break;
                case Opcodes.STX.ZeroPage.Opcode:   // STX $44       - ZP
                case Opcodes.STX.ZeroPage_Y.Opcode: // STX $44, Y    - ZP + Y
                case Opcodes.STX.Absolute.Opcode:   // STX $4400     - Abs
                case Opcodes.STY.ZeroPage.Opcode:   // STY $44       - ZP
                case Opcodes.STY.ZeroPage_Y.Opcode: // STY $44, X    - ZP + X
                case Opcodes.STY.Absolute.Opcode:   // STY $4400     - ABS
                    len = STXSTY(i, ref addr);
                    break;
                case Opcodes.AND.Immediate.Opcode:
                case Opcodes.AND.ZeroPage.Opcode:
                case Opcodes.AND.ZeroPage_X.Opcode:
                case Opcodes.AND.Absolute_X.Opcode:
                case Opcodes.AND.Absolute_Y.Opcode:
                case Opcodes.AND.Absolute.Opcode:
                case Opcodes.AND.IndexedIndirect.Opcode:
                case Opcodes.AND.IndirectIndexed.Opcode:
                    len = Opcodes.AND.Lengths[i];
                    var opcode = Opcodes.Find(_bus[PC]);
                    addr = ManageMemoryMapMode(addr, opcode);

                    if (i == Opcodes.AND.Absolute_X.Opcode || i == Opcodes.AND.ZeroPage_X.Opcode) addr += X;
                    if (i == Opcodes.AND.Absolute_Y.Opcode) addr += Y;

                    A &= _bus[addr];
                    P.Zero = A == 0;
                    P.Negative = A >> 7 != 0;
                    break;
                default:
                    throw new NotImplementedException("Opcode not implemented");
            }

            PC += len;
        }

        private ushort ManageMemoryMapMode(ushort addr, Type opcode)
        {
            MemoryMappingMode memorymap = opcode.GetMemoryMappingMode();

            switch (memorymap)
            {
                case MemoryMappingMode.Immediate:
                    break;
                case MemoryMappingMode.ZeroPage:
                    addr = _bus[addr];
                    break;
                case MemoryMappingMode.Absolute:
                    addr = Get16bitAbsoluteAdress(addr);
                    break;
                case MemoryMappingMode.IndexedIndirect:
                    addr = Get16bitAbsoluteAdress((ushort)(_bus[addr] + X));
                    break;
                case MemoryMappingMode.IndirectIndexed:
                    addr = (ushort)(Get16bitAbsoluteAdress(_bus[addr]) + Y);
                    break;
                default:
                    break;
            }

            return addr;
        }

        private byte STXSTY(byte i, ref ushort addr)
        {
            byte len;
            if (Opcodes.STX.Lengths.ContainsKey(i))
                len = Opcodes.STX.Lengths[i];
            else
                len = Opcodes.STY.Lengths[i];

            if (len == 2)
                addr = _bus[addr];
            else
                addr = Get16bitAbsoluteAdress(addr);

            if (i == Opcodes.STX.ZeroPage.Opcode) // ZP
                X = _bus[addr];
            else if (i == Opcodes.STY.ZeroPage.Opcode)
                Y = _bus[addr];
            else if (i == Opcodes.STX.ZeroPage_Y.Opcode) // + Y
            {
                addr += Y;
                X = _bus[addr];
            }
            else if (i == Opcodes.STY.ZeroPage_Y.Opcode)
            {
                addr += X;
                Y = _bus[addr];
            }
            else if (i == 0x8E)
                X = _bus[addr];
            else if (i == 0x8C)
                Y = _bus[addr];
            return len;
        }

        private byte ADC(byte i, ref ushort addr)
        {
            byte len = Opcodes.ADC.Lengths[i];
            if (i != Opcodes.ADC.Immediate.Opcode)
            {
                if (i == Opcodes.ADC.Absolute.Opcode || i == Opcodes.ADC.Absolute_X.Opcode || i == Opcodes.ADC.Absolute_Y.Opcode)
                    addr = Get16bitAbsoluteAdress(addr);
                else if (i == Opcodes.ADC.ZeroPage.Opcode || i == Opcodes.ADC.ZeroPage_X.Opcode)
                    addr = _bus[addr];
                else if (i == Opcodes.ADC.IndexedIndirect.Opcode)
                    addr = Get16bitAbsoluteAdress((ushort)(_bus[addr] + X));
                else if (i == Opcodes.ADC.IndirectIndexed.Opcode)
                    addr = (ushort)(Get16bitAbsoluteAdress(_bus[addr]) + Y);

                if (i == Opcodes.ADC.ZeroPage_X.Opcode || i == Opcodes.ADC.Absolute_X.Opcode) // ZP + X, ABS + X
                    addr += X;
                if (i == Opcodes.ADC.Absolute_Y.Opcode)
                    addr += Y;
            }

            byte val = _bus[addr];
            CalculateADC(addr, val);
            return len;
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

        private ushort Get16bitAbsoluteAdress(ushort addr) => (ushort)(_bus[addr] + (_bus[(byte)(addr + 1)] << 8));

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
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
            int len = 0;
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
                    len = Opcodes.ADC.Lengths[i];
                    addr = ManageMemoryMapMode(addr, Opcodes.Find(_bus[PC]));
                    if (i == Opcodes.ADC.Absolute_X.Opcode || i == Opcodes.ADC.ZeroPage_X.Opcode) addr += X;
                    if (i == Opcodes.ADC.Absolute_Y.Opcode) addr += Y;

                    byte val = _bus[addr];
                    AddToAccumulator(val);
                    break;
                case Opcodes.STX.ZeroPage.Opcode:   // STX $44       - ZP
                case Opcodes.STX.ZeroPage_Y.Opcode: // STX $44, Y    - ZP + Y
                case Opcodes.STX.Absolute.Opcode:   // STX $4400     - Abs
                case Opcodes.STY.ZeroPage.Opcode:   // STY $44       - ZP
                case Opcodes.STY.ZeroPage_X.Opcode: // STY $44, X    - ZP + X
                case Opcodes.STY.Absolute.Opcode:   // STY $4400     - ABS
                    if (Opcodes.STX.Lengths.ContainsKey(i))
                        len = Opcodes.STX.Lengths[i];
                    else
                        len = Opcodes.STY.Lengths[i];

                    addr = ManageMemoryMapMode(addr, Opcodes.Find(_bus[PC]));

                    if (i == Opcodes.STX.ZeroPage_Y.Opcode) addr += Y;
                    else if (i == Opcodes.STY.ZeroPage_X.Opcode) addr += X;

                    if (i == Opcodes.STX.ZeroPage.Opcode ||
                        i == Opcodes.STX.ZeroPage_Y.Opcode || 
                        i == Opcodes.STX.Absolute.Opcode)
                        X = _bus[addr];
                    else if (i == Opcodes.STY.ZeroPage.Opcode ||
                        i == Opcodes.STY.ZeroPage_X.Opcode ||
                        i == Opcodes.STY.Absolute.Opcode)
                        Y = _bus[addr];
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
                    addr = ManageMemoryMapMode(addr, Opcodes.Find(_bus[PC]));

                    if (i == Opcodes.AND.Absolute_X.Opcode || i == Opcodes.AND.ZeroPage_X.Opcode) addr += X;
                    if (i == Opcodes.AND.Absolute_Y.Opcode) addr += Y;

                    SetAccumulatorAndRegisters(A & _bus[addr]);
                    P.Zero = A == 0;
                    P.Negative = A >> 7 != 0;
                    break;
                case Opcodes.ASL.Absolute.Opcode:
                    len = Opcodes.ASL.Lengths[i];
                    addr = ManageMemoryMapMode(addr, Opcodes.Find(_bus[PC]));
                    var temp = _bus[addr] << 1;
                    SetAccumulatorAndRegisters(temp);
                    break;
                default:
                    throw new NotImplementedException("Opcode not implemented");
            }

            PC += (ushort)len;
        }

        private void SetAccumulatorAndRegisters(int val)
        {
            var previousValue = A;
            if (val > 255)
                A = (byte)(val - 256);
            else
                A = (byte)val;
            SetRegisters(previousValue, val);
        }

        private void AddToAccumulator(int val)
        {
            var previousValue = A;
            A += P.Carry ? ONE : ZERO;

            if (A + val > 255)
                A += (byte)(val - 256);
            else
                A += (byte)val;

            SetRegisters(previousValue, val);
        }

        private void SetRegisters(int previousValue, int val)
        {
            P.Carry = A < val;
            P.Overflow = !(A >> 7 == previousValue >> 7);
            P.Negative = A >> 7 != 0;
            P.Zero = A == 0;
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
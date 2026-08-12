using System;
using System.Net;

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
        /// <summary>
        /// Stack pointer
        /// </summary>
        public byte SP = new byte(); // Stack pointer

        /// <summary>
        /// Status registers
        /// </summary>                                      //  0   1   2   3   4   5   6   7
        public StatusRegisters P = new StatusRegisters();   // 	N	V  _	B	D	I	Z	C

        private const byte NEGATIVE = 0;
        private const byte OVERFLOW = 1;
        private const byte BREAK = 3;
        private const byte DECIMAL = 4;
        private const byte INTERRUPTS = 5;
        private const byte Z = 6;
        private const byte CARRY = 7;

        private const byte ONE = 1;
        private const byte ZERO = 0;

        private long _ticks = 0;
        private long _nextInstruction = 0;
        public bool Waiting = false;
        private bool _breaked = false;

        public long Ticks => _ticks;

        public void Reset()
        {
            P.InterruptsDisabled = true;
            A = 0;
            X = 0;
            Y = 0;

            // Stack pointer
            SP = 0xFD;

            // $fffc-$fffd	Start of reset handler
            PC = (ushort)(_bus[0xfffc] + (_bus[0xfffd] << 8));
        }

        public Instruction CurrentInstruction
        {
            get => Opcodes.Find(_bus[PC]).Instruction;
        }

        public void Tick()
        {
            _ticks++;
            if (_breaked) return;
            Opcode opcode = Opcodes.Find(_bus[PC]);
            if (!Waiting)
            {
                if (opcode.IsKil()) throw new CpuException("Kil instruction. System halted.");
                if (opcode.IsNop())
                {
                    PC += (ushort)opcode.Length;
                    return;
                }
                if (opcode.IsBrk())
                {
                    _breaked = true;
                    P.Break = true;
                    return;
                }
                _nextInstruction = opcode.Cycles - 1;
                Waiting = true;
            }
            else
                _nextInstruction--;


            if (_nextInstruction == 0)
            {
                Waiting = false;
                ExecuteNextInstruction(opcode);
            }

        }

        public void Tick(int ticks)
        {
            for (int i = 0; i < ticks; i++)
                Tick();
        }

        private void ExecuteNextInstruction(Opcode opcode)
        {
            var i = _bus[PC];
            bool advanceProgramCounter = true;
            ushort addr = PC; addr++;
            switch (opcode.Instruction)
            {
                case Instruction.ADC:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        if (i == Opcodes.ADC.Absolute_X.Opcode || i == Opcodes.ADC.ZeroPage_X.Opcode) addr += X;
                        if (i == Opcodes.ADC.Absolute_Y.Opcode) addr += Y;

                        byte val = _bus[addr];

                        PerformADC(val);
                        break;
                    }
                case Instruction.SBC:
                case Instruction.CMP:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        if (opcode.AddressingMode == AddressingMode.AbsoluteX || opcode.AddressingMode == AddressingMode.ZeroPageX) addr += X;
                        else if (opcode.AddressingMode == AddressingMode.AbsoluteY) addr += Y;
                        byte val = _bus[addr];
                        SetNegativeFlag((byte)(A - val));
                        P.Carry = A >= val;
                        P.Zero = A == val;
                        break;
                    }
                case Instruction.CPX:
                case Instruction.CPY:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        if (i == Opcodes.SBC.Absolute_X.Opcode || i == Opcodes.SBC.ZeroPage_X.Opcode) addr += X;
                        if (i == Opcodes.SBC.Absolute_Y.Opcode) addr += Y;
                        var tempAcc = A;
                        if (opcode.Instruction == Instruction.CPX)
                            A = X;
                        if (opcode.Instruction == Instruction.CPY)
                            A = Y;
                        byte val = _bus[addr];
                        bool temp = P.Carry;
                        P.Carry = true;
                        PerformADC((byte)(~val));
                        P.Carry = temp;
                        A = tempAcc;
                        break;
                    }

                case Instruction.AND:
                case Instruction.BIT:
                case Instruction.EOR:
                case Instruction.ORA:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);

                        if (i == Opcodes.AND.Absolute_X.Opcode ||
                            i == Opcodes.AND.ZeroPage_X.Opcode ||
                            i == Opcodes.EOR.ZeroPage_X.Opcode ||
                            i == Opcodes.EOR.Absolute_X.Opcode ||
                            i == Opcodes.ORA.Absolute_X.Opcode ||
                            i == Opcodes.ORA.ZeroPage_X.Opcode) addr += X;
                        if (i == Opcodes.AND.Absolute_Y.Opcode ||
                            i == Opcodes.EOR.Absolute_Y.Opcode ||
                            i == Opcodes.ORA.Absolute_Y.Opcode) addr += Y;



                        byte val = _bus[addr];
                        byte result = 0;
                        if (opcode.Instruction is Instruction.AND or Instruction.BIT)
                            result = (byte)(A & val);
                        else if (opcode.Instruction == Instruction.EOR)
                            result = (byte)(A ^ val);
                        else if (opcode.Instruction == Instruction.ORA)
                            result = (byte)(A | val);

                        //P.Overflow = ((A ^ val) & 0x80) == 0
                        //            && ((A ^ setValue) & 0x80) != 0;
                        if (opcode.Instruction == Instruction.BIT)
                        {
                            SetNegativeFlag(val);
                            P.Overflow = (val & (1 << 6)) != 0;
                            SetZeroFlag(result);
                        }
                        else
                        {
                            P.Overflow = (val & (1 << 6)) != 0;
                            SetNegativeFlag(result);
                            SetZeroFlag(result);
                        }

                    if (opcode.Instruction is Instruction.AND or Instruction.EOR or Instruction.ORA)
                            A = result;

                        break;
                    }
                case Instruction.ASL:
                case Instruction.LSR:
                case Instruction.ROL:
                case Instruction.ROR:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);

                        bool useAccumulator = opcode.AddressingMode == AddressingMode.Accumulator;
                        if (opcode.AddressingMode is AddressingMode.ZeroPageX or AddressingMode.AbsoluteX) addr += X;
                        byte value = useAccumulator ? A : _bus[addr];

                        bool carryIn = P.Carry;
                        var lsb = (value & ~(value - 1));

                        byte result;
                        switch (opcode.Instruction)
                        {
                            case Instruction.ASL:
                                // Shift left and set carry flag to bit 7 of value
                                P.Carry = (value & 0x80) != 0;
                                result = (byte)(value << 1);
                                break;
                            case Instruction.LSR:
                                // Shift right and set carry flag to bit 0 of value
                                P.Carry = (value & 0x01) != 0;
                                result = (byte)(value >> 1);
                                break;
                            case Instruction.ROL:
                                // Shift left and add carry in to bit 0
                                result = (byte)((value << 1) | (carryIn ? 1 : 0));
                                P.Carry = (value & 0x80) != 0;
                                break;
                            case Instruction.ROR:
                                // Shift right and add carry in to bit 7
                                result = (byte)((value >> 1) | (carryIn ? 0x80 : 0));
                                P.Carry = (value & 0x01) != 0;
                                break;
                            default:
                                throw new InvalidOperationException();
                        }

                        if (useAccumulator)
                            A = result;
                        else
                            _bus[addr] = result;

                        SetZeroFlag(result);
                        SetNegativeFlag(result);

                        break;
                    }
                case Instruction.BCC:
                case Instruction.BCS:
                case Instruction.BEQ:
                case Instruction.BNE:
                case Instruction.BMI:
                case Instruction.BPL:
                case Instruction.BVC:
                case Instruction.BVS:
                    {
                        bool branchTaken = opcode.Instruction switch
                        {
                            Instruction.BCC => !P.Carry,
                            Instruction.BCS => P.Carry,
                            Instruction.BEQ => P.Zero,
                            Instruction.BNE => !P.Zero,
                            Instruction.BMI => P.Negative,
                            Instruction.BPL => !P.Negative,
                            Instruction.BVC => !P.Overflow,
                            Instruction.BVS => P.Overflow,
                            _ => false
                        };

                        if (branchTaken)
                        {
                            sbyte offset = unchecked((sbyte)_bus[addr]);
                            PC = (ushort)(PC + opcode.Length + offset);
                            advanceProgramCounter = false;
                        }

                        break;
                    }

                case Instruction.LDA:
                case Instruction.LDX:
                case Instruction.LDY:
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                    if (i == Opcodes.LDA.Absolute_X.Opcode || i == Opcodes.LDA.ZeroPage_X.Opcode || i == Opcodes.LDY.ZeroPage_X.Opcode || i == Opcodes.LDY.Absolute_X.Opcode) addr += X;
                    if (i == Opcodes.LDA.Absolute_Y.Opcode || i == Opcodes.LDX.Absolute_Y.Opcode) addr += Y;

                    if (opcode.Instruction == Instruction.LDA)
                    {
                        A = _bus[addr];
                        SetZeroFlag(A);
                        SetNegativeFlag(A);
                    }
                    else if (opcode.Instruction == Instruction.LDX)
                    {
                        X = _bus[addr];
                        SetZeroFlag(X);
                        SetNegativeFlag(X);
                    }
                    else if (opcode.Instruction == Instruction.LDY)
                    {
                        Y = _bus[addr];
                        SetZeroFlag(Y);
                        SetNegativeFlag(Y);
                    }
                    break;
                case Instruction.STA:
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                    if (addr == SP) throw new InvalidOperationException("Attempting to write over stack pointer");

                    if (i == Opcodes.STA.Absolute_X.Opcode || i == Opcodes.STA.ZeroPage_X.Opcode) addr += X;
                    if (i == Opcodes.STA.Absolute_Y.Opcode ) addr += Y;

                    _bus[addr] = A;
                    break;
                case Instruction.STX:
                case Instruction.STY:
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);

                    if (addr == SP) throw new InvalidOperationException("Attempting to write over stack pointer");

                    if (i == Opcodes.STX.ZeroPage_Y.Opcode) addr += Y;
                    else if (i == Opcodes.STY.ZeroPage_X.Opcode) addr += X;

                    if (i == Opcodes.STX.ZeroPage.Opcode ||
                        i == Opcodes.STX.ZeroPage_Y.Opcode ||
                        i == Opcodes.STX.Absolute.Opcode)
                        _bus[addr] = X;
                    else if (i == Opcodes.STY.ZeroPage.Opcode ||
                        i == Opcodes.STY.ZeroPage_X.Opcode ||
                        i == Opcodes.STY.Absolute.Opcode)
                        _bus[addr] = Y;
                    break;
                case Instruction.TAX:
                    X = A;
                    SetNegativeFlag(X);
                    SetZeroFlag(X);
                    break;
                case Instruction.TXA:
                    A = X;
                    SetNegativeFlag(A);
                    SetZeroFlag(A);
                    break;
                case Instruction.DEX:
                    X--;
                    if (X == 0)
                        P.Zero = true;
                    else P.Zero = false;
                    break;
                case Instruction.INX:
                    X++;
                    if (X == 0)
                        P.Zero = true;
                    else P.Zero = false;
                    break;
                case Instruction.TAY:
                    Y = A;
                    P.Zero = Y == 0;
                    SetNegativeFlag(Y);
                    break;
                case Instruction.TYA:
                    A = Y;
                    P.Zero = A == 0;
                    SetNegativeFlag(A);
                    break;
                case Instruction.DEY:
                    if (Y == 0) P.Negative = true;
                    else P.Negative = false;
                    Y--;
                    if (Y == 0)
                        P.Zero = true;
                    else P.Zero = false;
                    break;
                case Instruction.INY:
                    Y++;
                    if (Y == 0)
                        P.Zero = true;
                    else P.Zero = false;
                    break;
                case Instruction.CLC:
                    P.Carry = false;
                    break;
                case Instruction.SEC:
                    P.Carry = true;
                    break;
                case Instruction.CLI:
                    P.InterruptsDisabled = false;
                    break;
                case Instruction.SEI:
                    P.InterruptsDisabled = true;
                    break;
                case Instruction.CLV:
                    P.Overflow = false;
                    break;
                case Instruction.CLD:
                    P.Decimal = false;
                    // NOP
                    break;
                case Instruction.DEC:
                case Instruction.INC:
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                    if (i == Opcodes.DEC.Absolute_X.Opcode || 
                        i == Opcodes.DEC.ZeroPage_X.Opcode || 
                        i == Opcodes.INC.Absolute_X.Opcode || 
                        i == Opcodes.INC.ZeroPage_X.Opcode) addr += X;

                    var op = 1;
                    if (opcode.Instruction == Instruction.DEC)
                        op = -1;
                    _bus[addr] = (byte)(_bus[addr] + op);
                    break;
                case Instruction.TXS:
                    SP = X;
                    break;
                case Instruction.TSX:
                    X = SP;
                    SetZeroFlag(X);
                    SetNegativeFlag(X);
                    break;
                case Instruction.PHA:
                    PushByte(A);
                    break;
                case Instruction.PLA:
                    A = PopByte();
                    SetNegativeFlag(A);
                    SetZeroFlag(A);
                    break;
                case Instruction.PHP:
                    PushByte((byte)(P.AsByte() | 0x30));
                    break;
                case Instruction.PLP:
                    P.FromByte(PopByte());
                    break;
                case Instruction.JMP:
                case Instruction.JSR: // jump to subroutine
                    {
                        addr = GetWord(addr);
                        ushort returnAddress = (ushort)(PC + opcode.Length - 1);
                        if (opcode.AddressingMode == AddressingMode.Indirect)
                        {                           
                            ushort hi = (ushort) ((addr & 0xFF) == 0xFF ? addr - 0xFF : addr + 1);
                            uint oldPC = PC;
                            PC = (ushort)(_bus[addr] | (ushort)(_bus[hi] << 8));

                            if ((oldPC & 0xFF00) != (PC & 0xFF00)) _nextInstruction += 2;
                        }
                        else
                        {
                            PC = addr;
                        }
                        if (opcode.Instruction == Instruction.JSR)
                        {
                            // store old program counter on stack
                            PushWord(returnAddress);
                        }
                        advanceProgramCounter = false;
                        break;
                    }
                case Instruction.RTI: // return from interrupt
                    // RTI retrieves the Processor Status Word (flags) and the Program Counter from the stack in that order (interrupts push the PC first and then the PSW).
                    // Note that unlike RTS, the return address on the stack is the actual address rather than the address - 1.
                    P.FromByte(PopByte());                    
                    PC = PopWord();
                    advanceProgramCounter = false;
                    break;
                case Instruction.RTS:
                    {
                        // get old PC back from stack
                        PC = (ushort)(PopWord() + 1);
                        advanceProgramCounter = false;
                        break;
                    }
                case Instruction.SED:
                    {
                        P.Decimal = true;
                        break;
                    }
                default:
                    throw new NotImplementedException("Opcode not implemented");
            }

            if (advanceProgramCounter)
                PC += (ushort)opcode.Length;
        }

        private void SetZeroFlag(byte value)
        {
            P.Zero = value == 0;
        }

        private void SetNegativeFlag(byte value)
        {
            if ((value & 0x80) != 0)
                P.Negative = true;
            else
                P.Negative = false;
        }

        private void PushWord(ushort word)
        {
            if (SP < 2) throw new InvalidOperationException("Stack pointer underflow");

            ushort high = SP;
            ushort low = (ushort)(SP - 1);
            //ushort pc = (ushort) (PC - 1);
            _bus[low] = (byte)word;
            _bus[high] = (byte)(word >> 8);
            SP -= 2;
        }

        private ushort PopWord()
        {
            ushort low, high;
            low = (ushort)(SP + 1);
            high = (ushort)(SP + 2);
            SP += 2;
            return (ushort)(_bus[low] | (_bus[high] << 8));
        }

        private void PushByte(byte value)
        {
            _bus[(ushort)(0x0100 | SP)] = value;
            SP--;
        }

        private byte PopByte()
        {
            SP++;
            return _bus[(ushort)(0x0100 | SP)];
        }

        private void PerformADC(byte val)
        {
            byte setValue = (byte)(A + val + (P.Carry ? (byte)1 : (byte)0));
            P.Overflow = ((A ^ val) & 0x80) == 0
                        && ((A ^ setValue) & 0x80) != 0;

            P.Negative = (setValue & 0x80) != 0; // bit 7
            P.Carry = (A + val + (P.Carry ? 1 : 0)) > 0xFF;
            P.Zero = setValue == 0;
            A = setValue;
        }



        private ushort ManageMemoryMapMode(ushort addr, MemoryMappingMode memorymap)
        {
            switch (memorymap)
            {
                case MemoryMappingMode.Immediate:
                    break;
                case MemoryMappingMode.ZeroPage:
                    addr = _bus[addr];
                    break;
                case MemoryMappingMode.Absolute:
                    addr = GetWord(addr);
                    break;
                case MemoryMappingMode.IndexedIndirect:
                    addr = GetWord((ushort)(_bus[addr] + X));
                    break;
                case MemoryMappingMode.IndirectIndexed:
                    addr = (ushort)(GetWord(_bus[addr]) + Y);
                    break;
                default:
                    break;
            }

            return addr;
        }

        private ushort GetWord(ushort addr) => (ushort)(_bus[addr] + (_bus[(ushort)(addr + 1)] << 8));

        public class StatusRegisters
        {
            private readonly bool[] _s = new bool[8];

            public bool Negative
            {
                get => _s[NEGATIVE];
                set => _s[NEGATIVE] = value;
            }

            public bool Decimal
            {
                get => _s[DECIMAL];
                set => _s[DECIMAL] = value;
            }

            public bool Carry
            {
                get
                {
                    return _s[CARRY];
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
            public bool InterruptsDisabled
            {
                get => _s[INTERRUPTS];
                set => _s[INTERRUPTS] = value;
            }

            public bool Break
            {
                get => _s[BREAK];
                set => _s[BREAK] = value;
            }

            public byte AsByte()
            {
                byte result = 0;
                int index = 8 - _s.Length;

                foreach (bool b in _s)
                {
                    // if the element is 'true' set the bit at that position
                    if (b)
                        result |= (byte)(1 << (7 - index));

                    index++;
                }
                return result;
            }

            public void FromByte(byte input)
            {
                // check each bit in the byte. if 1 set to true, if 0 set to false
                for (int i = 0; i < 8; i++)
                    _s[i] = (input & (1 << i)) != 0;

                // reverse the array
                Array.Reverse(_s);
            }

            public void InterruptTriggered(InterruptType type)
            {
                if (type == InterruptType.NMI || !InterruptsDisabled)
                {
                    // Todo: Trigger interrupt
                    return;
                }

            }
        }
    }
}

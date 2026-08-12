using System;
using System.Net;
using System.Reflection.Emit;

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
        private long _cyclesRemaining = 0;
        public bool Waiting = false;
        private Opcode _currentOpcode;
        private bool _servicingInterrupt;
        private ushort _activeVector;

        private bool NmiPending { get; set; }
        private bool IrqPending { get; set; }

        public long Ticks => _ticks;

        public void RequestInterrupt(InterruptType type)
        {
            if (type == InterruptType.NMI)
                NmiPending = true;
            else
                IrqPending = true;
        }

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

            if (_servicingInterrupt)
            {
                // Interrupts take some cycles before we move on
                _cyclesRemaining--;
                if (_cyclesRemaining == 0)
                {
                    _servicingInterrupt = false;
                    PC = GetWord((ushort)_activeVector);
                }
                return;
            }

            // Hardware interrupts can only begin between instructions.
            bool acceptNmi = !Waiting && NmiPending;
            bool acceptIrq =
                !Waiting &&
                IrqPending &&
                !P.InterruptsDisabled;

            if (acceptNmi || acceptIrq)
            {
                _activeVector = acceptNmi
                    ? (ushort)0xFFFA
                    : (ushort)0xFFFE;

                if (!_servicingInterrupt)
                {
                    _servicingInterrupt = true;

                    // Clear only the interrupt being accepted.
                    if (acceptNmi)
                        NmiPending = false;
                    else
                        IrqPending = false;

                    PushWord(PC);

                    // $20 is unused and is always set
                    // The B bit, $10, must be clear because this is a hardware interrupt, not BRK:
                    byte stackedStatus = (byte)((P.AsByte() | 0x20) & ~0x10);
                    PushByte(stackedStatus);
                    P.InterruptsDisabled = true;
                    _cyclesRemaining = 6;
                    return;
                }
            }

            // Get next opcode
            _currentOpcode = Opcodes.Find(_bus[PC]);

            if (_currentOpcode.IsBrk())
            {
                if (!_servicingInterrupt)
                {
                    _servicingInterrupt = true;
                    _activeVector = 0xFFFE;

                    PushWord((ushort)(PC + 2));
                    PushByte((byte)(P.AsByte() | 0x30)); // 0x30 sets both bit 5 and the B bit
                    P.InterruptsDisabled = true;
                    _cyclesRemaining = Opcodes.BRK.BRK_00.Cycles - 1;
                }
                else
                    _cyclesRemaining--;

                if (_cyclesRemaining == 0)
                {
                    PC = (ushort)(_bus[0xfffe] + (_bus[0xffff] << 8));
                    _servicingInterrupt = false;
                }
                return;
            }


            // If we are not waiting for the current instruction to finish, we need to fetch the next opcode and start executing it
            if (!Waiting)
            {
                if (_currentOpcode.IsKil()) throw new CpuException("Kil instruction. System halted.");


                // Calculate the number of cycles needed for this instruction, including any extra cycles due to page boundary crossings or other factors
                int extraCycles = CalculateExtraCycles(_currentOpcode);
                _cyclesRemaining = _currentOpcode.Cycles + extraCycles - 1;
                Waiting = true;
            }
            else
            {
                _cyclesRemaining--;
            }


            if (_cyclesRemaining == 0)
            {
                Waiting = false;

                ExecuteNextInstruction(_currentOpcode);
            }

        }

        private int CalculateExtraCycles(Opcode currentOpcode)
        {
            if (IsBranch(currentOpcode))
                return CalculateBranchExtraCycles(currentOpcode);

            if (IsIndexedRead(currentOpcode))
                return IndexedAddressCrossesPage(currentOpcode) ? 1 : 0;

            return 0;
        }

        private bool IndexedAddressCrossesPage(Opcode opcode)
        {
            // The instruction's operand starts immediately after the opcode.
            // We first calculate the unindexed address encoded by that operand,
            // then add X or Y separately so the two addresses can be compared.
            ushort baseAddress;
            ushort indexedAddress;

            switch (opcode.AddressingMode)
            {
                case AddressingMode.AbsoluteX:
                    // Example: LDA $20FF,X. The two operand bytes encode $20FF.
                    baseAddress = GetWord((ushort)(PC + 1));
                    indexedAddress = (ushort)(baseAddress + X);
                    break;

                case AddressingMode.AbsoluteY:
                    // Absolute,Y works the same way, but uses the Y register.
                    baseAddress = GetWord((ushort)(PC + 1));
                    indexedAddress = (ushort)(baseAddress + Y);
                    break;

                case AddressingMode.IndirectIndexed:
                    // Example: LDA ($10),Y. The operand is a zero-page pointer.
                    // Follow the pointer to get the base address before adding Y.
                    byte pointerAddress = _bus[(ushort)(PC + 1)];
                    baseAddress = GetZeroPageWord(pointerAddress);
                    indexedAddress = (ushort)(baseAddress + Y);
                    break;

                default:
                    return false;
            }

            // Each page is $100 bytes. Therefore, a changed upper byte means
            // indexing moved the effective address into a different page.
            return CrossesPageBoundary(baseAddress, indexedAddress);
        }

        private int CalculateBranchExtraCycles(Opcode opcode)
        {
            // Determine whether the branch is taken based on the current status flags.
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

            // A branch that is not taken uses only its normal two cycles.
            if (!branchTaken)
                return 0;

            // Branch offsets are relative to the instruction after the branch.
            ushort nextInstruction = (ushort)(PC + opcode.Length);

            // The operand is a signed 8-bit offset, allowing backward branches.
            sbyte offset = unchecked((sbyte)_bus[(ushort)(PC + 1)]);
            ushort targetAddress = (ushort)(nextInstruction + offset);

            // Taking the branch adds one cycle. Crossing a page adds one more.
            return CrossesPageBoundary(nextInstruction, targetAddress) ? 2 : 1;
        }

        private bool IsIndexedRead(Opcode opcode)
        {
            bool isReadInstruction = opcode.Instruction is
                Instruction.ADC or
                Instruction.AND or
                Instruction.CMP or
                Instruction.EOR or
                Instruction.LDA or
                Instruction.LDX or
                Instruction.LDY or
                Instruction.ORA or
                Instruction.SBC;

            bool canCrossPage = opcode.AddressingMode is
                AddressingMode.AbsoluteX or
                AddressingMode.AbsoluteY or
                AddressingMode.IndirectIndexed;

            return isReadInstruction && canCrossPage;
        }

        private bool IsBranch(Opcode opcode)
        {
            return opcode is { Instruction: Instruction.BCC or Instruction.BCS or Instruction.BEQ or Instruction.BNE or Instruction.BMI or Instruction.BPL or Instruction.BVC or Instruction.BVS };
        }

        private static bool CrossesPageBoundary(ushort original, ushort result)
        {
            return (original & 0xFF00) != (result & 0xFF00);
        }

        public void Tick(int ticks)
        {
            for (int i = 0; i < ticks; i++)
                Tick();
        }

        private void ExecuteNextInstruction(Opcode opcode)
        {
            bool advanceProgramCounter = true;
            ushort addr = PC; addr++;
            switch (opcode.Instruction)
            {
                case Instruction.ADC:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        addr = ApplyIndex(addr, opcode.AddressingMode);

                        byte val = _bus[addr];

                        PerformADC(val);
                        break;
                    }
                case Instruction.SBC:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        addr = ApplyIndex(addr, opcode.AddressingMode);

                        // NOT on the value to subtract it from the accumulator
                        PerformADC((byte)~_bus[addr]);
                        break;
                    }
                case Instruction.CMP:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        addr = ApplyIndex(addr, opcode.AddressingMode);
                        Compare(addr, A);
                        break;
                    }
                case Instruction.CPX:
                case Instruction.CPY:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        addr = ApplyIndex(addr, opcode.AddressingMode);
                        if (opcode.Instruction == Instruction.CPX)
                            Compare(addr, X);
                        else if (opcode.Instruction == Instruction.CPY) Compare(addr, Y);
                        break;
                    }

                case Instruction.AND:
                case Instruction.BIT:
                case Instruction.EOR:
                case Instruction.ORA:
                    {
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        addr = ApplyIndex(addr, opcode.AddressingMode);

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
                        addr = ApplyIndex(addr, opcode.AddressingMode);

                        bool useAccumulator = opcode.AddressingMode == AddressingMode.Accumulator;
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
                    addr = ApplyIndex(addr, opcode.AddressingMode);

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
                    addr = ApplyIndex(addr, opcode.AddressingMode);
                    if (addr == SP) throw new InvalidOperationException("Attempting to write over stack pointer");

                    _bus[addr] = A;
                    break;
                case Instruction.STX:
                case Instruction.STY:
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                    addr = ApplyIndex(addr, opcode.AddressingMode);

                    if (addr == SP) throw new InvalidOperationException("Attempting to write over stack pointer");

                    if (opcode.Instruction == Instruction.STX)
                        _bus[addr] = X;
                    else
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
                    SetZeroFlag(X);
                    SetNegativeFlag(X);
                    break;
                case Instruction.INX:
                    X++;
                    SetZeroFlag(X);
                    SetNegativeFlag(X);
                    break;
                case Instruction.TAY:
                    Y = A;
                    SetZeroFlag(Y);
                    SetNegativeFlag(Y);
                    break;
                case Instruction.TYA:
                    A = Y;
                    SetZeroFlag(A);
                    SetNegativeFlag(A);
                    break;
                case Instruction.DEY:
                    Y--;
                    SetNegativeFlag(Y);
                    SetZeroFlag(Y);
                    break;
                case Instruction.INY:
                    Y++;
                    SetZeroFlag(Y);
                    SetNegativeFlag(Y);
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
                    addr = ApplyIndex(addr, opcode.AddressingMode);

                    var op = 1;
                    if (opcode.Instruction == Instruction.DEC)
                        op = -1;
                    _bus[addr] = (byte)(_bus[addr] + op);
                    SetZeroFlag(_bus[addr]);
                    SetNegativeFlag(_bus[addr]);
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

                            if ((oldPC & 0xFF00) != (PC & 0xFF00)) _cyclesRemaining += 2;
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
                case Instruction.NOP:
                    break;
                default:
                    throw new NotImplementedException("Opcode not implemented");
            }

            if (advanceProgramCounter)
                PC += (ushort)opcode.Length;
        }

        private void Compare(ushort addr, byte regValue)
        {
            byte val = _bus[addr];
            SetNegativeFlag((byte)(regValue - val));
            P.Carry = regValue >= val;
            P.Zero = regValue == val;
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
            PushByte((byte)(word >> 8)); // high byte first
            PushByte((byte)word);        // low byte second
        }

        private ushort PopWord()
        {
            byte low = PopByte();
            byte high = PopByte();

            return (ushort)(low | (high << 8));
        }

        private void PushByte(byte value)
        {
            // Stack is located in page 1 ($0100-$01FF)
            // 0x0100 is the fixed starting address of the 6502’s hardware stack page so we add that to the pointer value
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

        private ushort ApplyIndex(ushort address, AddressingMode mode)
        {
            return mode switch
            {
                AddressingMode.ZeroPageX => (byte)(address + X),
                AddressingMode.ZeroPageY => (byte)(address + Y),
                AddressingMode.AbsoluteX => (ushort)(address + X),
                AddressingMode.AbsoluteY => (ushort)(address + Y),
                _ => address
            };
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
                    addr = GetZeroPageWord((byte)(_bus[addr] + X));
                    break;
                case MemoryMappingMode.IndirectIndexed:
                    addr = (ushort)(GetZeroPageWord(_bus[addr]) + Y);
                    break;
                default:
                    break;
            }

            return addr;
        }

        private ushort GetWord(ushort addr) => (ushort)(_bus[addr] + (_bus[(ushort)(addr + 1)] << 8));

        private ushort GetZeroPageWord(byte addr) =>
            (ushort)(_bus[addr] | (_bus[(byte)(addr + 1)] << 8));

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
                input |= 0x20;  // bit 5 is always represented as set
                input &= 0xEF;  // bit 4/B is not a persistent CPU flag

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

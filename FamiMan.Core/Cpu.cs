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
        private bool _waiting = false;
        private bool _breaked = false;

        public void Tick()
        {
            _ticks++;
            if (_breaked) return;
            Opcode opcode = Opcodes.Find(_bus[PC]);
            if (!_waiting)
            {
                if (opcode.IsKil()) throw new CpuException("Kil instruction. System halted.");
                if (opcode.IsNop())
                {
                    PC++;
                    return;
                }
                if (opcode.IsBrk())
                {
                    _breaked = true;
                    return;
                }
                _nextInstruction = opcode.Cycles - 1;
                _waiting = true;
            }
            else
                _nextInstruction--;


            if (_nextInstruction == 0)
            {
                _waiting = false;
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
            int len;
            ushort addr = PC; addr++;
            switch (opcode.OpcodeName)
            {
                case "ADC":
                    {
                        len = Opcodes.ADC.Lengths[i];
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        if (i == Opcodes.ADC.Absolute_X.Opcode || i == Opcodes.ADC.ZeroPage_X.Opcode) addr += X;
                        if (i == Opcodes.ADC.Absolute_Y.Opcode) addr += Y;

                        byte val = _bus[addr];

                        PerformADC(val);
                        break;
                    }
                case "SBC":
                case "CMP":
                    {
                        len = Opcodes.SBC.Lengths[i];
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        if (i == Opcodes.SBC.Absolute_X.Opcode || i == Opcodes.SBC.ZeroPage_X.Opcode) addr += X;
                        if (i == Opcodes.SBC.Absolute_Y.Opcode) addr += Y;
                        var tempAcc = A;
                        byte val = _bus[addr];
                        PerformADC((byte)(~val));
                        if (opcode.OpcodeName == "CMP")
                            A = tempAcc;
                        break;
                    }
                case "CPX":
                case "CPY":
                    {
                        len = Opcodes.SBC.Lengths[i];
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                        if (i == Opcodes.SBC.Absolute_X.Opcode || i == Opcodes.SBC.ZeroPage_X.Opcode) addr += X;
                        if (i == Opcodes.SBC.Absolute_Y.Opcode) addr += Y;
                        var tempAcc = A;
                        if (opcode.OpcodeName == "CPX")
                            A = X;
                        if (opcode.OpcodeName == "CPY")
                            A = Y;
                        byte val = _bus[addr];
                        bool temp = P.Carry;
                        P.Carry = true;
                        PerformADC((byte)(~val));
                        P.Carry = temp;
                        A = tempAcc;
                        break;
                    }

                case "AND":
                case "BIT":
                case "EOR":
                case "ORA":
                {
                    len = 1;
                    if (Opcodes.AND.Lengths.ContainsKey(i))
                        len = Opcodes.AND.Lengths[i];
                    else if (Opcodes.EOR.Lengths.ContainsKey(i))
                        len = Opcodes.EOR.Lengths[i];
                    else if (Opcodes.BIT.Lengths.ContainsKey(i))
                        len = Opcodes.BIT.Lengths[i];
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
                    byte setValue = 0;
                    if (opcode.OpcodeName == "AND" || opcode.OpcodeName == "BIT")
                        setValue = (byte)(A & val);
                    else if (opcode.OpcodeName == "EOR")
                        setValue = (byte)(A ^ val);
                    else if (opcode.OpcodeName == "ORA")
                        setValue = (byte)(A | val);

                    //P.Overflow = ((A ^ val) & 0x80) == 0
                    //            && ((A ^ setValue) & 0x80) != 0;

                    P.Negative = (setValue & 0x80) != 0; // bit 7

                    if (opcode.OpcodeName == "AND" || opcode.OpcodeName == "EOR" || opcode.OpcodeName == "ORA")
                    {
                        P.Zero = setValue == 0;
                        A = setValue;
                    }
                    else if (opcode.OpcodeName == "BIT")
                        P.Overflow = (setValue & (1 << 5)) != 0;

                    break;
                }
                case "ASL":
                case "LSR":
                case "ROL":
                case "ROR":
                    {
                        len = Opcodes.LSR.Lengths[i];
                        addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);

                        if (opcode.OpcodeVersionName == "ZeroPage_X" || opcode.OpcodeVersionName == "Absolute_X") addr += X;
                        bool carryValue = P.Carry;
                        var lsb = (A & ~(A - 1));
                        if (i == Opcodes.ASL.Accumulator.Opcode || i == Opcodes.ROL.Accumulator.Opcode)
                            SetAccumulatorAndRegisters(A << 1);
                        else if (i == Opcodes.LSR.Accumulator.Opcode || i == Opcodes.ROR.Accumulator.Opcode)
                            SetAccumulatorAndRegisters(A >> 1);
                        else if (opcode.OpcodeName == "LSR" || opcode.OpcodeName == "ROR")
                            SetAccumulatorAndRegisters(_bus[addr] >> 1);
                        else if (opcode.OpcodeName == "ASL" || opcode.OpcodeName == "ROL")
                            SetAccumulatorAndRegisters(_bus[addr] << 1);                    
                        if (opcode.OpcodeName == "LSR")
                            P.Carry = lsb == 1;

                        if (opcode.OpcodeName == "ROL" && carryValue) A |= 1;
                        if (opcode.OpcodeName == "ROR")
                        {
                            if (carryValue) A |= 128;
                            P.Carry = lsb == 1;
                        }
                        break;
                    }
                case "BCC":
                case "BCS":
                case "BEQ":
                case "BNE":
                case "BMI":
                case "BPL":
                case "BVC":
                case "BVS":
                    {
                        len = Opcodes.Branches.BCC.Length;
                        var temp = P.Carry;
                        if ((i == Opcodes.Branches.BCC.Opcode && !temp) ||
                            (i == Opcodes.Branches.BCS.Opcode && temp)) PC += _bus[addr];

                        if (i == Opcodes.Branches.BEQ.Opcode && P.Zero)
                            PC += _bus[addr];
                        else if (i == Opcodes.Branches.BNE.Opcode && !P.Zero)
                            PC += _bus[addr];
                        else if (i == Opcodes.Branches.BMI.Opcode && P.Negative)
                            PC += _bus[addr];
                        else if (i == Opcodes.Branches.BPL.Opcode && !P.Negative)
                            PC += _bus[addr];
                        else if (i == Opcodes.Branches.BVC.Opcode && !P.Overflow)
                            PC += _bus[addr];
                        else if (i == Opcodes.Branches.BVS.Opcode && P.Overflow)
                            PC += _bus[addr];
                        P.Carry = temp;
                        break;
                    }

                case "LDA":
                case "LDX":
                case "LDY":
                    len = Opcodes.LDA.Lengths[i];
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                    if (i == Opcodes.LDA.Absolute_X.Opcode || i == Opcodes.LDA.ZeroPage_X.Opcode || i == Opcodes.LDY.ZeroPage_X.Opcode || i == Opcodes.LDY.Absolute_X.Opcode) addr += X;
                    if (i == Opcodes.LDA.Absolute_Y.Opcode || i == Opcodes.LDX.Absolute_Y.Opcode) addr += Y;

                    if (opcode.OpcodeName == "LDA")
                        A = _bus[addr];
                    else if (opcode.OpcodeName == "LDX")
                        X = _bus[addr];
                    else if (opcode.OpcodeName == "LDY")
                        Y = _bus[addr];
                    break;
                case "STA":
                    len = Opcodes.STA.Lengths[i];
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                    if (i == Opcodes.STA.Absolute_X.Opcode || i == Opcodes.STA.ZeroPage_X.Opcode) addr += X;
                    if (i == Opcodes.STA.Absolute_Y.Opcode ) addr += Y;

                    _bus[addr] = A;
                    break;
                case "STX":
                case "STY":
                    if (Opcodes.STX.Lengths.ContainsKey(i))
                        len = Opcodes.STX.Lengths[i];
                    else
                        len = Opcodes.STY.Lengths[i];

                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);

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
                case "TAX":
                    len = Opcodes.Registers.TAX.Length;
                    X = A;
                    break;
                case "TXA":
                    len = Opcodes.Registers.TXA.Length;
                    A = X;
                    break;
                case "DEX":
                    len = Opcodes.Registers.DEX.Length;
                    X--;
                    break;
                case "INX":
                    len = Opcodes.Registers.INX.Length;
                    X++;
                    break;
                case "TAY":
                    len = Opcodes.Registers.TAY.Length;
                    Y = A;
                    break;
                case "TYA":
                    len = Opcodes.Registers.TYA.Length;
                    A = Y;
                    break;
                case "DEY":
                    len = Opcodes.Registers.DEY.Length;
                    Y--;
                    break;
                case "INY":
                    len = Opcodes.Registers.INY.Length;
                    Y++;
                    break;
                case "CLC":
                    len = Opcodes.Flags.CLC.Length;
                    P.Carry = false;
                    break;
                case "SEC":
                    len = Opcodes.Flags.CLC.Length;
                    P.Carry = true;
                    break;
                case "CLI":
                    len = Opcodes.Flags.CLI.Length;
                    P.InterruptsDisabled = false;
                    break;
                case "SEI":
                    len = Opcodes.Flags.CLI.Length;
                    P.InterruptsDisabled = true;
                    break;
                case "CLV":
                    len = Opcodes.Flags.CLV.Length;
                    P.Overflow = false;
                    break;
                case "DEC":
                case "INC":
                    len = Opcodes.DEC.Lengths[i];
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);
                    if (i == Opcodes.DEC.Absolute_X.Opcode || 
                        i == Opcodes.DEC.ZeroPage_X.Opcode || 
                        i == Opcodes.INC.Absolute_X.Opcode || 
                        i == Opcodes.INC.ZeroPage_X.Opcode) addr += X;

                    var op = 1;
                    if (opcode.OpcodeName == "DEC")
                        op = -1;
                    _bus[addr] = (byte)(_bus[addr] + op);
                    break;
                case "TXS":
                    len = 1;
                    S = X;
                    break;
                case "TSX":
                    len = 1;
                    X = S;
                    break;
                case "PHA":
                    len = 1;
                    _bus[S] = A;
                    S--;
                    break;
                case "PLA":
                    len = 1;
                    A = _bus[S];
                    P.Negative = (A & 0x80) != 0; // bit 7
                    P.Zero = A == 0;
                    S++;
                    break;
                case "PHP":
                    len = 1;
                    _bus[S] = P.AsByte();
                    S--;
                    break;
                case "PLP":
                    len = 1;
                    P.FromByte(_bus[S]);
                    S++;
                    break;
                case "JMP":
                case "JSR":
                    {
                        len = 0;
                        addr = Get16bitAbsoluteAdress(addr);
                        if (opcode.OpcodeVersionName == "Indirect")
                            addr = Get16bitAbsoluteAdress(addr);
                        if (opcode.OpcodeName == "JSR")
                            _bus[S--] = (byte)(PC + 3);
                        PC = addr;
                        break;
                    }
                case "RTS":
                    {
                        len = 0;
                        PC = _bus[++S];
                        break;
                    }
                default:
                    throw new NotImplementedException("Opcode not implemented");
            }

            PC += (ushort)len;
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

        private void SetAccumulatorAndRegisters(int val)
        {
            var previousValue = A;

            if (val > 255)
                A = (byte)(val - 256);
            else
                A = (byte)val;
            P.Carry = A < val;
            P.Negative = A >> 7 != 0;
            P.Zero = A == 0;
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

        private ushort Get16bitAbsoluteAdress(ushort addr) => (ushort)(_bus[addr] + (_bus[(ushort)(addr + 1)] << 8));

        public class StatusRegisters
        {
            private readonly bool[] _s = new bool[8];

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
            public bool InterruptsDisabled
            {
                get => _s[INTERRUPTS];
                set => _s[INTERRUPTS] = value;
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
        }
    }
}

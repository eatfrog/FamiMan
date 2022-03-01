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

        private long _ticks = 0;
        private long _nextInstruction = 0;
        private bool _waiting = false;

        public void Tick()
        {
            _ticks++;
            Opcode opcode = Opcodes.Find(_bus[PC]);
            if (!_waiting)
            {
                if (opcode.IsKil()) throw new CpuException("Kil instruction. System halted.");
                if (opcode.IsNop())
                {
                    PC++;
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

                    byte setValue = (byte)(A + val + (P.Carry ? (byte)1 : (byte)0));
                    P.Overflow = ((A ^ val) & 0x80) == 0
                                && ((A ^ setValue) & 0x80) != 0;

                    P.Negative = (setValue & 0x80) != 0; // bit 7
                    P.Carry = (A + val + (P.Carry ? 1 : 0)) > 0xFF;
                    P.Zero = setValue == 0;
                    A = setValue;
                    break;
                }
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
                case "AND":
                case "BIT":
                {
                    len = Opcodes.AND.Lengths[i];
                    addr = ManageMemoryMapMode(addr, opcode.MemoryMappingMode);

                    if (i == Opcodes.AND.Absolute_X.Opcode || i == Opcodes.AND.ZeroPage_X.Opcode) addr += X;
                    if (i == Opcodes.AND.Absolute_Y.Opcode) addr += Y;



                    byte val = _bus[addr];
                    byte setValue = (byte)(A & val);

                    //P.Overflow = ((A ^ val) & 0x80) == 0
                    //        && ((A ^ setValue) & 0x80) != 0;

                    P.Negative = (setValue & 0x80) != 0; // bit 7
                    P.Zero = setValue == 0;

                    if (opcode.OpcodeName == "AND")
                        A = setValue;
                    else
                        P.Overflow = (setValue & (1 << 5)) != 0;


                    break;
                }
                case "ASL":
                case "LSR":
                case "ROL":
                case "ROR":
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
                case "BCC":
                case "BCS":
                case "BEQ":
                case "BNE":
                case "BMI":
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
                    P.Carry = temp;
                    break;

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

        private void SetRegisters(int previousValue, int val)
        {
            P.Carry = A < val;
            // previousValue = Accumulator before the addition
            // val = the value adding to the accumulator
            // A = the sum of the addition (A+val+C)

            //if (((previousValue ^ A) & (val ^ A) & 0x80) == 1)
            //    P.Overflow = true;
            //else
            //    P.Overflow = false;

            //P.Overflow = !(A >> 7 == previousValue >> 7);

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

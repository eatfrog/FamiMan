using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FamiMan.Core
{
    public static partial class Opcodes
    {
        private static Dictionary<byte, Opcode> _opcodes;
        static Opcodes()
        {
            _opcodes = typeof(Opcodes).GetNestedTypes()
                .SelectMany(x => x.GetNestedTypes())
                .Select(t =>
                {
                    string instructionName = t.Name.Length == 3
                        ? t.Name
                        : t.UnderlyingSystemType.ReflectedType.Name;

                    return new Tuple<byte, Opcode>((byte)t.GetField("Opcode").GetValue(t), new Opcode
                    {
                        BackingType = t.UnderlyingSystemType,
                        MemoryMappingMode = t.GetMemoryMappingMode(),
                        AddressingMode = t.GetAddressingMode(),
                        Instruction = Enum.Parse<Instruction>(instructionName),
                        Length = t.GetLength(),
                        Cycles = t.GetCycles()
                    });
                })
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        public static Opcode Find(byte v)
        {
            return _opcodes[v];
        }

        public static class BRK
        {
            public static class BRK_00
            {
                public const byte Opcode = 0x00;
                public const int Length = 1;
                public const int Cycles = 7;
            }
        }
    }

    public class Opcode
    {
        public Type BackingType { get; set; }

        public Instruction Instruction { get; set; }

        public AddressingMode AddressingMode { get; set; }

        public MemoryMappingMode MemoryMappingMode { get; set; }

        public int Length { get; set; }

        public int Cycles { get; set; }
    }

    public enum AddressingMode
    {
        Unknown,
        Implied,
        Accumulator,
        Immediate,
        ZeroPage,
        ZeroPageX,
        ZeroPageY,
        Relative,
        Absolute,
        AbsoluteX,
        AbsoluteY,
        Indirect,
        IndexedIndirect,
        IndirectIndexed
    }

    public enum Instruction
    {
        ADC,
        AND,
        ASL,
        BCC,
        BCS,
        BEQ,
        BIT,
        BMI,
        BNE,
        BPL,
        BRK,
        BVC,
        BVS,
        CLC,
        CLD,
        CLI,
        CLV,
        CMP,
        CPX,
        CPY,
        DEC,
        DEX,
        DEY,
        EOR,
        INC,
        INX,
        INY,
        JMP,
        JSR,
        KIL,
        LDA,
        LDX,
        LDY,
        LSR,
        NOP,
        ORA,
        PHA,
        PHP,
        PLA,
        PLP,
        ROL,
        ROR,
        RTI,
        RTS,
        SBC,
        SEC,
        SED,
        SEI,
        STA,
        STX,
        STY,
        TAX,
        TAY,
        TSX,
        TXA,
        TXS,
        TYA
    }

    public enum MemoryMappingMode
    {
        Immediate,
        ZeroPage,
        Absolute,
        IndexedIndirect, // Addr + X
        IndirectIndexed,  // Ptr at addr + offset Y
        None
    }

    
}

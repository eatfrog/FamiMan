using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class CMP
    {
        private Bus _b;
        private Cpu _c;

        public CMP()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void CMP_0xE9_Immediate()
        {
            byte i = 0;
            _c.P.Carry = true;

            _c.A = 8;
            _b.Ram[i++] = Opcodes.CMP.Immediate.Opcode;
            _b.Ram[i++] = 6;
            _c.Tick(Opcodes.CMP.Immediate.Cycles);
            Assert.Equal(8, _c.A);

            _c.A = 24;
            _c.P.Carry = true;
            _b.Ram[i++] = Opcodes.CMP.Immediate.Opcode;
            _b.Ram[i++] = 24;

            _c.Tick(Opcodes.CMP.Immediate.Cycles);
            Assert.Equal(24, _c.A);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void CMP_0xE5_ZeroPage()
        {
            byte i = 0;
            _c.P.Carry = true;

            _c.A = 8;
            _b.Ram[i++] = Opcodes.CMP.ZeroPage.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[0x10] = 6;
            _c.Tick(Opcodes.CMP.ZeroPage.Cycles);
            Assert.Equal(8, _c.A);
        }

        [Fact]
        public void CMP_0xF5_ZeroPage_X()
        {
            byte i = 0;
            _c.P.Carry = true;

            _c.A = 8;
            _c.X = 1;
            _b.Ram[i++] = Opcodes.CMP.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[0x11] = 6;
            _c.Tick(Opcodes.CMP.ZeroPage_X.Cycles);
            Assert.Equal(8, _c.A);
        }

        [Fact]
        public void CMP_0xED_Absolute()
        {
            byte i = 0;
            _c.P.Carry = true;

            _c.A = 8;
            _b.Ram[i++] = Opcodes.CMP.Absolute.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01;
            _b.Ram[0x110] = 6;
            _c.Tick(Opcodes.CMP.Absolute.Cycles);
            Assert.Equal(8, _c.A);
        }

        [Fact]
        public void CMP_0xFD_Absolute_X()
        {
            byte i = 0;
            _c.P.Carry = true;

            _c.A = 8;
            _c.X = 1;
            _b.Ram[i++] = Opcodes.CMP.Absolute_X.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01;
            _b.Ram[0x111] = 6;
            _c.Tick(Opcodes.CMP.Absolute_X.Cycles);
            Assert.Equal(8, _c.A);
        }

        [Fact]
        public void CMP_0xF9_Absolute_Y()
        {
            byte i = 0;
            _c.P.Carry = true;

            _c.A = 8;
            _c.X = 1;
            _c.Y = 2;
            _b.Ram[i++] = Opcodes.CMP.Absolute_Y.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01;
            _b.Ram[0x112] = 6;
            _c.Tick(Opcodes.CMP.Absolute_Y.Cycles);
            Assert.Equal(8, _c.A);
        }

        [Fact]
        public void CMP_0xE1_IndirectX()
        {
            _c.A = 0xfd;
            _c.P.Carry = true;
            byte i = 0;
            _b.Ram[i++] = Opcodes.CMP.IndexedIndirect.Opcode; // Add Indirect_X
            _b.Ram[i++] = 0xE8;                             // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                       // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                            // 0x0703
            _b[0x0703] = 0x10;                              // which has value 10
            _c.Tick(Opcodes.CMP.IndexedIndirect.Cycles);    // Tick
            Assert.Equal(0xfd, _c.A);
            Assert.Equal(Opcodes.CMP.IndexedIndirect.Length, _c.PC); // Program counter should have moved to correct value
        }

        [Fact]
        public void CMP_0xF1_IndirectY()
        {
            _c.A = 0xea;
            _c.P.Carry = true;

            byte i = 0;
            _b.Ram[i++] = Opcodes.CMP.IndirectIndexed.Opcode;    // Add Indirect_Y
            _b.Ram[i++] = 0xE8;                             // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                       // + 2 so 0x05
            _b[0x05] = 2;                                // which has value 2
            _c.Tick(Opcodes.CMP.IndirectIndexed.Cycles);         // Tick
            Assert.Equal(0xea, _c.A);                 
            Assert.Equal(Opcodes.CMP.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }
    }
}
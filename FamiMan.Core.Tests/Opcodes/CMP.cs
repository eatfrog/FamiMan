using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class CMPTests
    {
        private Bus _b;
        private Cpu _c;

        public CMPTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Theory]
        [InlineData(8, 6, true, false, false)]
        [InlineData(24, 24, true, true, false)]
        [InlineData(6, 8, false, false, true)]
        public void CMP_0xC9_Immediate(
            byte accumulator,
            byte operand,
            bool expectedCarry,
            bool expectedZero,
            bool expectedNegative)
        {
            _c.A = accumulator;
            _c.P.Carry = !expectedCarry; // CMP must not depend on the old carry value.
            _b.Ram[0] = CMP.Immediate.Opcode;
            _b.Ram[1] = operand;

            _c.Tick(CMP.Immediate.Cycles);

            Assert.Equal(accumulator, _c.A);
            Assert.Equal(expectedCarry, _c.P.Carry);
            Assert.Equal(expectedZero, _c.P.Zero);
            Assert.Equal(expectedNegative, _c.P.Negative);
        }

        [Fact]
        public void CMP_0xE5_ZeroPage()
        {
            byte i = 0;
            _c.P.Carry = false;

            _c.A = 8;
            _b.Ram[i++] = CMP.ZeroPage.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[0x10] = 8;
            _c.Tick(CMP.ZeroPage.Cycles);
            Assert.Equal(8, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void CMP_0xF5_ZeroPage_X()
        {
            byte i = 0;
            _c.P.Carry = false;

            _c.A = 8;
            _c.X = 1;
            _b.Ram[i++] = CMP.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[0x11] = 8;
            _c.Tick(CMP.ZeroPage_X.Cycles);
            Assert.Equal(8, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void CMP_0xED_Absolute()
        {
            byte i = 0;
            _c.P.Carry = false;

            _c.A = 8;
            _b.Ram[i++] = CMP.Absolute.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01;
            _b.Ram[0x110] = 8;
            _c.Tick(CMP.Absolute.Cycles);
            Assert.Equal(8, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void CMP_0xFD_Absolute_X()
        {
            byte i = 0;
            _c.P.Carry = false;

            _c.A = 8;
            _c.X = 1;
            _b.Ram[i++] = CMP.Absolute_X.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01;
            _b.Ram[0x111] = 8;
            _c.Tick(CMP.Absolute_X.Cycles);
            Assert.Equal(8, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void CMP_0xF9_Absolute_Y()
        {
            byte i = 0;
            _c.P.Carry = false;

            _c.A = 8;
            _c.X = 1;
            _c.Y = 2;
            _b.Ram[i++] = CMP.Absolute_Y.Opcode;
            _b.Ram[i++] = 0x10;
            _b.Ram[i++] = 0x01;
            _b.Ram[0x112] = 8;
            _c.Tick(CMP.Absolute_Y.Cycles);
            Assert.Equal(8, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void CMP_0xE1_IndirectX()
        {
            _c.A = 0xfd;
            _c.P.Carry = false;
            byte i = 0;
            _b.Ram[i++] = CMP.IndexedIndirect.Opcode; // Add Indirect_X
            _b.Ram[i++] = 0xE8;                             // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                       // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                            // 0x0703
            _b[0x0703] = 0xfd;
            _c.Tick(CMP.IndexedIndirect.Cycles);    // Tick
            Assert.Equal(0xfd, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
            Assert.Equal(CMP.IndexedIndirect.Length, _c.PC); // Program counter should have moved to correct value
        }

        [Fact]
        public void CMP_0xF1_IndirectY()
        {
            _c.A = 0xea;
            _c.P.Carry = false;

            byte i = 0;
            _b.Ram[i++] = CMP.IndirectIndexed.Opcode;    // Add Indirect_Y
            _b.Ram[i++] = 0xE8;                             // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                       // + 2 so 0x05
            _b[0x05] = 0xea;
            _c.Tick(CMP.IndirectIndexed.Cycles);         // Tick
            Assert.Equal(0xea, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
            Assert.Equal(CMP.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }
    }
}

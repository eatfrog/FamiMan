using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class BranchesTests
    {
        private Bus _b;
        private Cpu _c;
        public BranchesTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void BCC_0x90()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Carry = false;
            _b.Ram[i++] = Branches.BCC.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BCC.Cycles);
            Assert.Equal(2 + 14, _c.PC);
        }

        [Fact]
        public void BCS_0xB0()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Carry = true;
            _b.Ram[i++] = Branches.BCS.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BCS.Cycles);
            Assert.Equal(2 + 14, _c.PC);
        }

        [Fact]
        public void BEQ_0xF0()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Zero = true;
            _b.Ram[i++] = Branches.BEQ.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BCS.Cycles);
            Assert.Equal(2 + 14, _c.PC);

            i = (byte)_c.PC;
            _c.P.Zero = false;
            _b.Ram[i++] = Branches.BEQ.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BCS.Cycles);
            Assert.Equal(i, _c.PC);
        }

        [Fact]
        public void BNE_0xD0()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Zero = false;
            _b.Ram[i++] = Branches.BNE.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BCS.Cycles);
            Assert.Equal(2 + 14, _c.PC);

            i = (byte)_c.PC;
            _c.P.Zero = true;
            _b.Ram[i++] = Branches.BNE.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BCS.Cycles);
            Assert.Equal(i, _c.PC);

            i = (byte)_c.PC;
            _c.P.Zero = false;
            _b.Ram[i++] = Branches.BNE.Opcode;
            _b.Ram[i++] = 253; // Move PC -2
            _c.Tick(Branches.BCS.Cycles);
            Assert.Equal(i - 2, _c.PC);
        }

        [Fact]
        public void BMI_0x30()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Negative = true;
            _b.Ram[i++] = Branches.BMI.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BMI.Cycles);
            Assert.Equal(2 + 14, _c.PC);

            i = (byte)_c.PC;
            _c.P.Negative = false;
            _b.Ram[i++] = Branches.BMI.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BMI.Cycles);
            Assert.Equal(i, _c.PC);
        }

        [Fact]
        public void BPL_0x10()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Negative = false;
            _b.Ram[i++] = Branches.BPL.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BPL.Cycles);
            Assert.Equal(2 + 14, _c.PC);

            i = (byte)_c.PC;
            _c.P.Negative = true;
            _b.Ram[i++] = Branches.BPL.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BPL.Cycles);
            Assert.Equal(i, _c.PC);
        }

        [Fact]
        public void BVC_0x50()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Overflow = false;
            _b.Ram[i++] = Branches.BVC.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BVC.Cycles);
            Assert.Equal(2 + 14, _c.PC);

            i = (byte)_c.PC;
            _c.P.Overflow = true;
            _b.Ram[i++] = Branches.BVC.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BVC.Cycles);
            Assert.Equal(i, _c.PC);
        }

        [Fact]
        public void BVS_0x70()
        {
            byte i = 0;
            _c.A = 0x05;
            _c.P.Overflow = true;
            _b.Ram[i++] = Branches.BVS.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BVS.Cycles);
            Assert.Equal(2 + 14, _c.PC);

            i = (byte)_c.PC;
            _c.P.Overflow = false;
            _b.Ram[i++] = Branches.BVS.Opcode;
            _b.Ram[i++] = 0x0E; // Move PC +14
            _c.Tick(Branches.BVS.Cycles);
            Assert.Equal(i, _c.PC);
        }
    }
}

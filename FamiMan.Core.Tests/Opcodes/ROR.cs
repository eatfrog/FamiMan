using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class RORTests
    {
        private Bus _b;
        private Cpu _c;
        public RORTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void ROR_0x6E_Absolute()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ROR.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 109; // 01101101
            _c.P.Carry = true;
            _c.Tick(ROR.Absolute.Cycles);

            // 1 -> 01101101  
            //      10110110 1 -> Carry
            Assert.Equal(182, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = ROR.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 128; // 10000000
            _c.Tick(ROR.Absolute.Cycles);

            _c.P.Carry = false;
            // 0 -> 10000000  
            //      01000000 0 -> Carry
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void ROR_0x7E_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ROR.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _c.X = 1;           // plus one is 15
            _b.Ram[0x0F] = 109; // 01101101
            _c.P.Carry = true;
            _c.Tick(ROR.Absolute_X.Cycles);

            // 1 -> 01101101  
            //      10110110 1 -> Carry
            Assert.Equal(182, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = ROR.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _c.X = 1;           // plus one is 15
            _b.Ram[0x0F] = 128; // 10000000
            _c.Tick(ROR.Absolute_X.Cycles);

            _c.P.Carry = false;
            // 0 -> 10000000  
            //      01000000 0 -> Carry
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void ROR_0x66_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ROR.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 109; // 01101101
            _c.P.Carry = true;
            _c.Tick(ROR.ZeroPage.Cycles);

            // 1 -> 01101101  
            //      10110110 1 -> Carry
            Assert.Equal(182, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = ROR.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 128; // 10000000
            _c.Tick(ROR.ZeroPage.Cycles);

            _c.P.Carry = false;
            // 0 -> 10000000  
            //      01000000 0 -> Carry
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void ROR_0x76_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ROR.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _c.X = 1;           // plux one = 15
            _b.Ram[0x0F] = 109; // 01101101
            _c.P.Carry = true;
            _c.Tick(ROR.ZeroPage_X.Cycles);

            // 1 -> 01101101  
            //      10110110 1 -> Carry
            Assert.Equal(182, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = ROR.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _c.X = 1;
            _b.Ram[0x0f] = 128; // 10000000
            _c.Tick(ROR.ZeroPage_X.Cycles);

            _c.P.Carry = false;
            // 0 -> 10000000  
            //      01000000 0 -> Carry
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void ROR_0x6A_Accumulator()
        {
            byte i = 0;
            _b.Ram[i++] = ROR.Accumulator.Opcode;
            _c.A = 109;           // 01101101
            _c.P.Carry = true;
            _c.Tick(ROR.Accumulator.Cycles);

            // 1 -> 01101101  
            //      10110110 1 -> Carry
            Assert.Equal(182, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = ROR.Accumulator.Opcode;
            _c.A = 128; // 10000000
            _c.Tick(ROR.Accumulator.Cycles);

            _c.P.Carry = false;
            // 0 -> 10000000  
            //      01000000 0 -> Carry
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

    }
}

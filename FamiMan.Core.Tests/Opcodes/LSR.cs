using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class LSRTests
    {
        private Bus _b;
        private Cpu _c;
        public LSRTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void LSR_0x4E_Absolute()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = LSR.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 109; // 01101101
            _c.Tick(LSR.Absolute.Cycles);

            // 0 -> 01101101
            //      00110110
            Assert.Equal(54, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = LSR.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 128; // 10000000
            _c.Tick(LSR.Absolute.Cycles);

            // 0 -> 10000000 
            //      01000000 
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void LSR_0x5E_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = LSR.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _c.X = 1;           // plus one is 15
            _b.Ram[0x0F] = 109; // 01101101
            _c.Tick(ASL.Absolute_X.Cycles);

            // 0 -> 01101101
            //      00110110 
            Assert.Equal(54, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = LSR.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _c.X = 1;           // plus one is 15
            _b.Ram[0x0F] = 128; // 10000000
            _c.Tick(ASL.Absolute_X.Cycles);

            // 0 -> 10000000 
            //      01000000 
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void LSR_0x46_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = LSR.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 109; // 01101101
            _c.Tick(LSR.ZeroPage.Cycles);

            // 0 -> 01101101
            //      00110110
            Assert.Equal(54, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = LSR.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 128; // 10000000
            _c.Tick(ASL.ZeroPage.Cycles);

            // 0 -> 10000000 
            //      01000000 
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void LSR_0x56_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = LSR.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _c.X = 1;           // plux one = 15
            _b.Ram[0x0F] = 109; // 01101101
            _c.Tick(LSR.ZeroPage_X.Cycles);

            // 0 -> 01101101
            //      00110110
            Assert.Equal(54, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = LSR.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _c.X = 1;
            _b.Ram[0x0f] = 128; // 10000000
            _c.Tick(LSR.ZeroPage_X.Cycles);

            // 0 -> 10000000 
            //      01000000 
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

        [Fact]
        public void LSR_0x4A_Accumulator()
        {
            byte i = 0;
            _b.Ram[i++] = LSR.Accumulator.Opcode;
            _c.A = 109;           // 01101101
            _c.Tick(LSR.Accumulator.Cycles);

            // 0 -> 01101101
            //      00110110
            Assert.Equal(54, _c.A);
            Assert.True(_c.P.Carry);

            _b.Ram[i++] = LSR.Accumulator.Opcode;
            _c.A = 128; // 10000000
            _c.Tick(LSR.Accumulator.Cycles);

            // 0 -> 10000000 
            //      01000000 
            Assert.Equal(64, _c.A);
            Assert.False(_c.P.Carry);
            Assert.False(_c.P.Zero);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class ASLTests
    {
        private Bus _b;
        private Cpu _c;
        public ASLTests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void ASL_0x0E_Absolute()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ASL.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 109; // 01101101
            _c.Tick(ASL.Absolute.Cycles);

            // 01101101 <- 0 
            // 11011010 
            Assert.Equal(218, _c.A);

            _b.Ram[i++] = ASL.Absolute.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 128; // 10000000
            _c.Tick(ASL.Absolute.Cycles);

            // 10000000 <- 0 
            // 00000000 
            Assert.Equal(0, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void ASL_0x1E_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ASL.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _c.X = 1;           // plus one is 15
            _b.Ram[0x0F] = 109; // 01101101
            _c.Tick(ASL.Absolute_X.Cycles);

            // 01101101 <- 0 
            // 11011010 
            Assert.Equal(218, _c.A);

            _b.Ram[i++] = ASL.Absolute_X.Opcode;
            _b.Ram[i++] = 0x0E;
            _b.Ram[i++] = 0x00; // Memory location 0x000E = 14
            _c.X = 1;           // plus one is 15
            _b.Ram[0x0F] = 128; // 10000000
            _c.Tick(ASL.Absolute_X.Cycles);

            // 10000000 <- 0 
            // 00000000 
            Assert.Equal(0, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void ASL_0x06_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ASL.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 109; // 01101101
            _c.Tick(ASL.ZeroPage.Cycles);

            // 01101101 <- 0 
            // 11011010 
            Assert.Equal(218, _c.A);

            _b.Ram[i++] = ASL.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _b.Ram[0x0E] = 128; // 10000000
            _c.Tick(ASL.ZeroPage.Cycles);

            // 10000000 <- 0 
            // 00000000 
            Assert.Equal(0, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void ASL_0x16_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x05;
            _b.Ram[i++] = ASL.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _c.X = 1;           // plux one = 15
            _b.Ram[0x0F] = 109; // 01101101
            _c.Tick(ASL.ZeroPage_X.Cycles);

            // 01101101 <- 0 
            // 11011010 
            Assert.Equal(218, _c.A);

            _b.Ram[i++] = ASL.ZeroPage_X.Opcode;
            _b.Ram[i++] = 0x0E; // Memory location 0x000E = 14
            _c.X = 1;
            _b.Ram[0x0f] = 128; // 10000000
            _c.Tick(ASL.ZeroPage_X.Cycles);

            // 10000000 <- 0 
            // 00000000 
            Assert.Equal(0, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

        [Fact]
        public void ASL_0x0A_Accumulator()
        {
            byte i = 0;
            _b.Ram[i++] = ASL.Accumulator.Opcode;
            _c.A = 109;           // 01101101
            _c.Tick(ASL.Accumulator.Cycles);

            // 01101101 <- 0 
            // 11011010 
            Assert.Equal(218, _c.A);

            _b.Ram[i++] = ASL.Accumulator.Opcode;
            _c.A = 128; // 10000000
            _c.Tick(ASL.Accumulator.Cycles);

            // 10000000 <- 0 
            // 00000000 
            Assert.Equal(0, _c.A);
            Assert.True(_c.P.Carry);
            Assert.True(_c.P.Zero);
        }

    }
}

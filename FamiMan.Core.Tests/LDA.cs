using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class LDA
    {
        private Bus _b;
        private Cpu _c;
        public LDA()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void LDA_0xA9_Immediate()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDA.Immediate.Opcode;  
            _b.Ram[i++] = 0x0E;         

            _c.Tick(Opcodes.LDA.Immediate.Cycles);

            Assert.Equal(0x0E, _c.A);
        }

        [Fact]
        public void LDA_0xA5_ZeroPage()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDA.ZeroPage.Opcode;
            _b.Ram[i++] = 0x0A;                           // Memory location 0x0A
            _b.Ram[0x0A] = 14;
            _c.Tick(Opcodes.LDA.ZeroPage.Cycles);

            Assert.Equal(14, _c.A);
        }

        [Fact]
        public void LDA_0xB5_ZeroPage_X()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDA.ZeroPage_X.Opcode;  
            _b.Ram[i++] = 0x0A;                             // Memory location 0x0A
            _c.X = 2;
            _b.Ram[0x0C] = 14;
            _c.Tick(Opcodes.LDA.ZeroPage_X.Cycles);


            Assert.Equal(14, _c.A);
        }

        [Fact]
        public void LDA_0xAD_Absolute()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDA.Absolute.Opcode; 

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.

            _b.Ram[0x03E8] = 14;
            _c.Tick(Opcodes.LDA.Absolute.Cycles);

            Assert.Equal(14, _c.A);
        }

        [Fact]
        public void LDA_0xBD_Absolute_X()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDA.Absolute_X.Opcode; 

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.X = 1;               // add 1 to the memory address
            _b.Ram[0x03E9] = 14;

            _c.Tick(Opcodes.LDA.Absolute.Cycles);

            Assert.Equal(14, _c.A);
        }

        [Fact]
        public void LDA_0xB9_Absolute_Y()
        {
            byte i = 0;
            _c.A = 0x00;
            _b.Ram[i++] = Opcodes.LDA.Absolute_Y.Opcode; 

            _b.Ram[i++] = 0xE8;     // Memory location: 0x03E8/1000d
            _b.Ram[i++] = 0x03;     // Little endian, The least significant byte (LSB) value, is at the lowest address.
            _c.Y = 1;               // Add 1 to the memory address
            _b.Ram[0x03E9] = 14;
            _c.Tick(Opcodes.LDA.Absolute.Cycles);

            Assert.Equal(14, _c.A);
        }

        [Fact]
        public void LDA_0xA1_IndirectY()
        {
            _c.A = 0;
            byte i = 0;
            _b.Ram[i++] = Opcodes.LDA.IndirectIndexed.Opcode;   // Add Indirect_X
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _b[0xE8] = 0x03;                                    // Ptr at memory location 0x00EA/232d points to 0x03
            _c.Y = 2;                                           // + 2 so 0x05
            _b[0x05] = 14;                                      // which has value 14
            _c.Tick(Opcodes.LDA.IndirectIndexed.Cycles);        // Tick

            Assert.Equal(14, _c.A);
            Assert.Equal(Opcodes.LDA.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }

        [Fact]
        public void LDA_0xB1_IndirectX()
        {
            _c.A = 0;
            byte i = 0;
            _b.Ram[i++] = Opcodes.LDA.IndexedIndirect.Opcode;   // Add Indirect_Y
            _b.Ram[i++] = 0xE8;                                 // Memory location: ZP 0x00E8/232d
            _c.X = 2;                                           // + 2 so 0x00EA
            _b[0xEA] = 0x03;                                    // Ptr at memory location 0x00EA/234d points to 
            _b[0xEA + 1] = 0x07;                                // 0x0703
            _b[0x0703] = 14;                                    // which has value 14
            _c.Tick(Opcodes.LDA.IndexedIndirect.Cycles);        // Tick


            Assert.Equal(14, _c.A);
            Assert.Equal(Opcodes.LDA.IndirectIndexed.Length, _c.PC); // Program counter should have moved to correct value
        }

    }
}

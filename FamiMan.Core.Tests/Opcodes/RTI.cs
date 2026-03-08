using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests.Opcodes
{
    public class RTITests
    {
        private readonly Bus _b;
        private readonly Cpu _c;

        public RTITests()
        {
            _b = new Bus();
            _c = new Cpu(_b);
        }

        [Fact]
        public void RTI_0x40_PullsStatusAndProgramCounterFromStack()
        {
            byte i = 0;
            _b.Ram[i++] = RTI.Implied.Opcode;

            _c.SP = 0x0D;
            _b.Ram[0x0E] = 0x82; // status (N + Z)
            _b.Ram[0x0F] = 0x34; // PC low
            _b.Ram[0x10] = 0x12; // PC high

            _c.Tick(RTI.Implied.Cycles);

            Assert.True(_c.P.Negative);
            Assert.True(_c.P.Zero);
            Assert.Equal(0x1234, _c.PC);
            Assert.Equal(0x10, _c.SP);
        }
    }
}

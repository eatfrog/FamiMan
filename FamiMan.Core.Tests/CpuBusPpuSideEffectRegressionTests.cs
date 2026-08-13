using Xunit;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// CPU-bus accesses to PPU registers are operations, not ordinary byte
    /// storage. These tests are the reason the CPU no longer uses the ref indexer.
    /// </summary>
    public class CpuBusPpuSideEffectRegressionTests
    {
        [Fact]
        public void CpuBusWritesThroughPpuAddrAndPpuDataToPpuMemory()
        {
            var bus = CreateBusWithChr();

            // The CPU supplies PPU address $2000 as a high/low pair, then writes
            // the value through PPUDATA. Bus.Write must call the PPU register API.
            bus.Write(0x2006, 0x20);
            bus.Write(0x2006, 0x00);
            bus.Write(0x2007, 0x42);

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x2000));
        }

        [Fact]
        public void CpuBusReadOfPpuStatusClearsVblank()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.Register.PPUSTATUS = 0x80;

            byte status = bus.Read(0x2002);

            Assert.Equal(0x80, status & 0x80);
            Assert.Equal(0x00, bus.Ppu.Register.PPUSTATUS & 0x80);
        }

        [Fact]
        public void MirroredCpuPpuRegistersKeepTheirSideEffects()
        {
            var bus = CreateBusWithChr();

            // $200E and $200F mirror PPUADDR ($2006) and PPUDATA ($2007).
            bus.Write(0x200E, 0x20);
            bus.Write(0x200E, 0x00);
            bus.Write(0x200F, 0x37);

            Assert.Equal(0x37, bus.Ppu.ReadPpuMemory(0x2000));
        }

        private static Bus CreateBusWithChr()
        {
            var bus = new Bus();
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }
    }
}

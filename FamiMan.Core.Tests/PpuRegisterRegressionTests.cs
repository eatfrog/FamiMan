using Xunit;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// CPU-visible PPU register behavior needed for a game to upload its
    /// pattern, nametable, and palette data before the first rendered frame.
    /// </summary>
    public class PpuRegisterRegressionTests
    {
        [Fact]
        public void PpuCtrlAndPpuMaskWritesStoreTheirValues()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WriteCpuRegister(0x2000, 0x84);
            bus.Ppu.WriteCpuRegister(0x2001, 0x1E);

            Assert.Equal(0x84, bus.Ppu.Register.PPUCTRL);
            Assert.Equal(0x1E, bus.Ppu.Register.PPUMASK);
        }

        [Fact]
        public void PpuAddrAndPpuDataCanWriteNametableMemory()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WriteCpuRegister(0x2006, 0x20); // Address high byte.
            bus.Ppu.WriteCpuRegister(0x2006, 0x00); // Address low byte: $2000.
            bus.Ppu.WriteCpuRegister(0x2007, 0x42);

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x2000));
        }

        [Fact]
        public void PpuDataIncrementsAddressByOneByDefault()
        {
            var bus = CreateBusWithChr();

            // The test checks that every access through PPUDATA ($2007) automatically advances the internal PPU address.
            bus.Ppu.WriteCpuRegister(0x2006, 0x20);
            bus.Ppu.WriteCpuRegister(0x2006, 0x00);
            bus.Ppu.WriteCpuRegister(0x2007, 0x11);
            bus.Ppu.WriteCpuRegister(0x2007, 0x22);

            Assert.Equal(0x11, bus.Ppu.ReadPpuMemory(0x2000)); // First write to $2007 should go to $2000.
            Assert.Equal(0x22, bus.Ppu.ReadPpuMemory(0x2001)); // Second write to $2007 should go to $2001, confirming the increment by 1.
        }

        [Fact]
        public void PpuDataIncrementsAddressBy32WhenPpuCtrlBit2IsSet()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.WriteCpuRegister(0x2000, 0x04);

            bus.Ppu.WriteCpuRegister(0x2006, 0x20);
            bus.Ppu.WriteCpuRegister(0x2006, 0x00);
            bus.Ppu.WriteCpuRegister(0x2007, 0x11);
            bus.Ppu.WriteCpuRegister(0x2007, 0x22);

            Assert.Equal(0x11, bus.Ppu.ReadPpuMemory(0x2000));
            Assert.Equal(0x22, bus.Ppu.ReadPpuMemory(0x2020));
        }

        [Fact]
        public void ReadingPpuStatusReturnsThenClearsVblank()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.Register.PPUSTATUS = 0x80;

            byte firstRead = bus.Ppu.ReadCpuRegister(0x2002);
            byte secondRead = bus.Ppu.ReadCpuRegister(0x2002);

            Assert.Equal(0x80, firstRead & 0x80);
            Assert.Equal(0x00, secondRead & 0x80);
        }

        [Fact]
        public void ReadingPpuStatusResetsPpuAddrWriteLatch()
        {
            var bus = CreateBusWithChr();

            // Leave the address latch halfway through an address.
            bus.Ppu.WriteCpuRegister(0x2006, 0x3F);
            bus.Ppu.ReadCpuRegister(0x2002);

            // The status read means these are a fresh high/low pair.
            bus.Ppu.WriteCpuRegister(0x2006, 0x20);
            bus.Ppu.WriteCpuRegister(0x2006, 0x00);
            bus.Ppu.WriteCpuRegister(0x2007, 0x42);

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x2000));
        }

        [Fact]
        public void PpuDataReadsOutsidePaletteAreDelayedByOneRead()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.WritePpuMemory(0x2000, 0x42);

            bus.Ppu.WriteCpuRegister(0x2006, 0x20);
            bus.Ppu.WriteCpuRegister(0x2006, 0x00);

            byte bufferedValue = bus.Ppu.ReadCpuRegister(0x2007);
            byte actualValue = bus.Ppu.ReadCpuRegister(0x2007);

            Assert.Equal(0x00, bufferedValue);
            Assert.Equal(0x42, actualValue);
        }

        [Fact]
        public void PpuDataReadsPaletteWithoutDelay()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.WritePpuMemory(0x3F00, 0x0F);

            bus.Ppu.WriteCpuRegister(0x2006, 0x3F);
            bus.Ppu.WriteCpuRegister(0x2006, 0x00);

            Assert.Equal(0x0F, bus.Ppu.ReadCpuRegister(0x2007));
        }

        private static Bus CreateBusWithChr()
        {
            var bus = new Bus();
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }
    }
}

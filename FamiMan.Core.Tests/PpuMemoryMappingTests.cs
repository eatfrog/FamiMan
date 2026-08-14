using System;
using System.IO;
using Xunit;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// The smallest PPU memory behaviors needed before background pixels can
    /// be rendered. These deliberately exclude sprites, scrolling, and timing.
    /// </summary>
    public class PpuMemoryMappingTests
    {
        [Fact]
        public void PatternTableReadsComeFromCartridgeChr()
        {
            var bus = new Bus();
            bus.IO.CHRROM = new byte[8_192];
            bus.IO.CHRROM[0x0123] = 0x42;

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x0123));
        }

        [Fact]
        public void NametableMemoryCanBeWrittenAndRead()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WritePpuMemory(0x2000, 0x42);

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x2000));
        }

        [Fact]
        public void VerticalMirroringSupportsHorizontalScrollingUsedBySuperMarioBros()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.Mirroring = NametableMirroring.Vertical;

            // With vertical mirroring:
            // $2000 mirrors $2800
            // $2400 mirrors $2C00
            bus.Ppu.WritePpuMemory(0x2000, 0x11);
            bus.Ppu.WritePpuMemory(0x2400, 0x22);

            // Verify that the mirrored addresses return the same values.
            Assert.Equal(0x11, bus.Ppu.ReadPpuMemory(0x2800));
            Assert.Equal(0x22, bus.Ppu.ReadPpuMemory(0x2C00));
        }

        [Fact]
        public void NametableRangeAt3000Mirrors2000Range()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WritePpuMemory(0x2000, 0x42);

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x3000));
        }

        [Fact]
        public void PaletteRamRepeatsEvery32Bytes()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WritePpuMemory(0x3F01, 0x2A);

            Assert.Equal(0x2A, bus.Ppu.ReadPpuMemory(0x3F21));
        }

        [Theory]
        [InlineData(0x3F10)]
        [InlineData(0x3F14)]
        [InlineData(0x3F18)]
        [InlineData(0x3F1C)]
        public void SpriteBackdropEntriesMirrorBackgroundBackdropEntries(
            ushort mirrorAddress)
        {
            var bus = CreateBusWithChr();
            ushort backgroundAddress = (ushort)(mirrorAddress - 0x10);

            bus.Ppu.WritePpuMemory(backgroundAddress, 0x0F);

            Assert.Equal(0x0F, bus.Ppu.ReadPpuMemory(mirrorAddress));
        }

        [Fact]
        public void PpuAddressesAbove3FFFWrapToFourteenBits()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WritePpuMemory(0x2000, 0x42);

            Assert.Equal(0x42, bus.Ppu.ReadPpuMemory(0x6000));
        }

        [Fact]
        public void INesVerticalMirroringFlagConfiguresPpuForSuperMarioBros()
        {
            byte[] rom = new byte[16 + 16_384 + 8_192];
            rom[0] = (byte)'N';
            rom[1] = (byte)'E';
            rom[2] = (byte)'S';
            rom[3] = 0x1A;
            rom[4] = 1;
            rom[5] = 1;
            rom[6] = 0x01; // Bit 0 selects vertical nametable mirroring.

            string path = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(path, rom);
                var bus = new Bus();

                bus.IO.LoadINesRomFile(path);

                Assert.Equal(NametableMirroring.Vertical, bus.Ppu.Mirroring);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static Bus CreateBusWithChr()
        {
            var bus = new Bus();
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }
    }
}

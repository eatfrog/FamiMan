using Xunit;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// The remaining steps between decoded CHR pixels and a colored host
    /// framebuffer. These deliberately do not involve SDL.
    /// </summary>
    public class PpuColorOutputTests
    {
        [Theory]
        // Bits 1-0 select the top-left 16x16-pixel quadrant.
        [InlineData(0, 0, 0b0000_0010, 2)]
        // Bits 3-2 select the top-right quadrant.
        [InlineData(16, 0, 0b0000_1100, 3)]
        // Bits 5-4 select the bottom-left quadrant.
        [InlineData(0, 16, 0b0001_0000, 1)]
        // Bits 7-6 select the bottom-right quadrant.
        [InlineData(16, 16, 0b1000_0000, 2)]
        public void AttributeByteSelectsPaletteForEachQuadrant(
            int x,
            int y,
            byte attribute,
            byte expectedPalette)
        {
            var bus = CreateBusWithChr();
            bus.Ppu.WritePpuMemory(Ppu.NAMETABLE_ATTR_START, attribute);

            Assert.Equal(expectedPalette, bus.Ppu.GetBackgroundPaletteNumber(x, y));
        }

        [Fact]
        public void BackgroundColorZeroUsesUniversalBackdropColor()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.WritePpuMemory(0x3F00, 0x0F);
            bus.Ppu.WritePpuMemory(0x3F08, 0x2A);

            // A bitplane color index of zero uses $3F00 regardless of which
            // background palette the attribute table selected.
            Assert.Equal(0x0F, bus.Ppu.GetBackgroundPaletteValue(2, 0));
        }

        [Fact]
        public void BackgroundFrameContainsOnePaletteIndexPerNesPixel()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WritePpuMemory(Ppu.NAMETABLE_START, 0x01);
            bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
            bus.Ppu.WritePpuMemory(0x3F01, 0x21);

            byte[] frame = bus.Ppu.RenderBackgroundFrame();

            Assert.Equal(256 * 240, frame.Length);
            Assert.Equal(0x21, frame[0]);
        }

        [Fact]
        public void NesSystemPaletteConvertsSixBitColorValuesToOpaqueArgb()
        {
            uint darkColor = NesSystemPalette.ToArgb(0x0F);
            uint blueColor = NesSystemPalette.ToArgb(0x21);

            Assert.Equal(0xFF000000u, darkColor & 0xFF000000u);
            Assert.Equal(0xFF000000u, blueColor & 0xFF000000u);
            Assert.NotEqual(darkColor, blueColor);

            // The PPU color value is six bits, so higher bits are ignored.
            Assert.Equal(blueColor, NesSystemPalette.ToArgb(0x61));
        }

        private static Bus CreateBusWithChr()
        {
            var bus = new Bus();
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }
    }
}

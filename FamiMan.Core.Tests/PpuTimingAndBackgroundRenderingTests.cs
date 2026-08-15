using Xunit;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// The next PPU milestone: establish the NTSC frame clock, then prove that
    /// nametable and CHR data can be turned into background pixels. Sprites and
    /// fine scrolling deliberately come later.
    /// </summary>
    public class PpuTimingAndBackgroundRenderingTests
    {
        [Fact]
        public void PpuAdvancesToNextScanlineAfter341Cycles()
        {
            var bus = CreateBusWithChr();

            Tick(bus.Ppu, 340);
            Assert.Equal(0, bus.Ppu.Scanline);
            Assert.Equal(340, bus.Ppu.Cycle);

            bus.Ppu.Tick();
            Assert.Equal(1, bus.Ppu.Scanline);
            Assert.Equal(0, bus.Ppu.Cycle);
        }

        [Fact]
        public void VblankStartsAtScanline241Cycle1()
        {
            var bus = CreateBusWithChr();

            Tick(bus.Ppu, 241 * 341 + 1);

            Assert.Equal(241, bus.Ppu.Scanline);
            Assert.Equal(1, bus.Ppu.Cycle);
            Assert.NotEqual(0, bus.Ppu.Register.PPUSTATUS & 0x80);
        }

        [Fact]
        public void VblankEndsAtPreRenderScanlineCycle1()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.Register.PPUSTATUS |= 0x80;

            Tick(bus.Ppu, 261 * 341 + 1);

            Assert.Equal(261, bus.Ppu.Scanline);
            Assert.Equal(1, bus.Ppu.Cycle);
            Assert.Equal(0, bus.Ppu.Register.PPUSTATUS & 0x80);
        }

        [Fact]
        public void PpuMarksFrameCompleteAfter262Scanlines()
        {
            var bus = CreateBusWithChr();

            Tick(bus.Ppu, 262 * 341);

            Assert.True(bus.Ppu.FrameComplete);
            Assert.Equal(0, bus.Ppu.Scanline);
            Assert.Equal(0, bus.Ppu.Cycle);
        }

        [Fact]
        public void BackgroundPixelCombinesBothChrBitplanes()
        {
            var bus = CreateBusWithChr();

            // The top-left nametable entry selects tile 1. A tile occupies 16
            // CHR bytes: eight low-plane rows followed by eight high-plane rows.
            bus.Ppu.WritePpuMemory(0x2000, 0x01);
            bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000); // Pixel 0 low bit = 1.
            bus.Ppu.WritePpuMemory(0x0018, 0b1000_0000); // Pixel 0 high bit = 1.
            bus.Ppu.WritePpuMemory(0x3F03, 0x2A);         // Combined pixel value is 3.

            Assert.Equal(0x2A, bus.Ppu.GetBackgroundPixel(0, 0));
        }

        [Fact]
        public void BackgroundPixelUsesAttributeTableToSelectPalette()
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WritePpuMemory(0x2000, 0x01);
            bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000); // Pixel value 1.
            bus.Ppu.WritePpuMemory(0x23C0, 0b0000_0010); // Top-left quadrant uses palette 2.
            bus.Ppu.WritePpuMemory(0x3F09, 0x30);         // Palette 2, color 1.

            Assert.Equal(0x30, bus.Ppu.GetBackgroundPixel(0, 0));
        }

        [Theory]
        [InlineData(0x00, 0x0000)]
        [InlineData(0x10, 0x1000)]
        public void BackgroundPatternTableBaseComesFromPpuCtrlBit4(
            byte ppuCtrl,
            ushort expectedAddress)
        {
            var bus = CreateBusWithChr();

            bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, ppuCtrl);

            Assert.Equal(
                expectedAddress,
                bus.Ppu.GetBackgroundPatternTableBase());
        }

        [Theory]
        // PPUCTRL bit 4 is clear, so tile 1 starts at $0000 + (1 * 16) = $0010.
        [InlineData(0x00, 0x01, 0x0010)]
        // PPUCTRL bit 4 is set, moving the same tile 1 to $1000 + (1 * 16) = $1010.
        [InlineData(0x10, 0x01, 0x1010)]
        // The $1000 table is still selected, but tile 2 starts one 16-byte tile later.
        [InlineData(0x10, 0x02, 0x1020)]
        public void TileAddressUsesSixteenBytesPerTile(
            byte ppuCtrl,
            byte tileNumber,
            ushort expectedAddress)
        {
            var bus = CreateBusWithChr();
            bus.Ppu.WriteCpuRegister(0x2000, ppuCtrl);

            Assert.Equal(
                expectedAddress,
                bus.Ppu.GetBackgroundTileAddress(tileNumber));
        }

        [Fact]
        public void NametableEntrySelectsTileForScreenPosition()
        {
            var bus = CreateBusWithChr();

            // Pixel (8, 16) is in tile column 1, row 2. Nametables contain
            // 32 tile numbers per row, so this entry is $2000 + (2 * 32) + 1.
            bus.Ppu.WritePpuMemory(0x2041, 0x2A);

            Assert.Equal(0x2A, bus.Ppu.GetNametableTileNumber(8, 16, 0));
        }

        [Fact]
        public void PpuCtrlNametableSelectChoosesBackgroundNametable()
        {
            var bus = CreateBusWithChr();
            bus.Ppu.Mirroring = NametableMirroring.Vertical;

            bus.Ppu.WritePpuMemory(0x2000, 0x11);
            bus.Ppu.WritePpuMemory(0x2400, 0x22);

            // PPUCTRL bits 1-0 select which nametable appears at the top-left
            // of the screen. Value 1 selects the nametable beginning at $2400.
            bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, 0x01);

            Assert.Equal(0x22, bus.Ppu.GetNametableTileNumber(0, 0));
        }

        [Fact]
        public void TilePixelReadsLowBitplane()
        {
            var bus = CreateBusWithChr();

            // Tile 1 starts at $0010. Bit 7 is its leftmost pixel; the high
            // bitplane remains zero, so the resulting color index is 1.
            bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);

            Assert.Equal(1, bus.Ppu.GetTilePixelColorIndex(1, 0, 0));
        }

        [Fact]
        public void TilePixelCombinesBothBitplanes()
        {
            var bus = CreateBusWithChr();

            // For tile 1, row 0 is at $0010 in the low plane and $0018 in
            // the high plane. Binary color bits 11 produce color index 3.
            bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
            bus.Ppu.WritePpuMemory(0x0018, 0b1000_0000);

            Assert.Equal(3, bus.Ppu.GetTilePixelColorIndex(1, 0, 0));
        }

        [Fact]
        public void BackgroundAttributeSelectsPalette()
        {
            var bus = CreateBusWithChr();

            // $23C0 is the first attribute byte. Its lowest two bits select
            // the palette used by the top-left 2x2-tile quadrant.
            bus.Ppu.WritePpuMemory(Ppu.NAMETABLE_ATTR_START, 0b0000_0010);

            Assert.Equal(2, bus.Ppu.GetBackgroundPaletteNumber(0, 0, 0));
        }

        [Fact]
        public void BackgroundColorIndexReadsPaletteValue()
        {
            var bus = CreateBusWithChr();

            // Each background palette has four entries. Palette 2, color 1
            // is therefore $3F00 + (2 * 4) + 1 = $3F09.
            bus.Ppu.WritePpuMemory(0x3F09, 0x30);

            Assert.Equal(0x30, bus.Ppu.GetBackgroundPaletteValue(2, 1));
        }

        [Fact]
        public void PpuCtrlSelectsBackgroundPatternTableAt1000()
        {
            var bus = CreateBusWithChr();

            // PPUCTRL bit 4 chooses where background tile graphics begin:
            // 0 means pattern table $0000 and 1 means pattern table $1000.
            bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, 0b0001_0000);

            // The first nametable byte describes the tile at screen position
            // (0, 0). A value of 1 tells the PPU to draw CHR tile number 1.
            bus.Ppu.WritePpuMemory(Ppu.NAMETABLE_START, 0x01);

            // Each CHR tile occupies 16 bytes. With pattern table $1000 selected,
            // tile 1 begins at $1000 + (1 * 16) = $1010.
            // Bit 7 represents the tile's leftmost pixel. Setting it in the low
            // bitplane gives that pixel the two-bit color index 1 (binary 01).
            bus.Ppu.WritePpuMemory(0x1010, 0b1000_0000);

            // With no attribute byte selecting another palette, the tile uses
            // background palette 0. Color index 1 is stored at $3F00 + 1.
            bus.Ppu.WritePpuMemory(0x3F01, 0x21);

            // Therefore screen pixel (0, 0) resolves to NES palette value $21.
            Assert.Equal(0x21, bus.Ppu.GetBackgroundPixel(0, 0));
        }

        private static void Tick(Ppu ppu, int count)
        {
            for (int i = 0; i < count; i++)
                ppu.Tick();
        }

        private static Bus CreateBusWithChr()
        {
            var bus = new Bus();
            bus.IO.CHRROM = new byte[8_192];
            return bus;
        }
    }
}

using Xunit;

namespace FamiMan.Core.Tests;

/// <summary>
/// Small sprite-decoding steps that lead up to compositing and sprite-zero hit.
/// These tests use 8x8 sprites, which is the mode Super Mario Bros. uses.
/// </summary>
public class PpuSpriteDecodingTests
{
    [Theory]
    [InlineData(0b0000_0000, 0x0000)]
    [InlineData(0b0000_1000, 0x1000)]
    [InlineData(0b0001_0000, 0x0000)]
    public void SpritePatternTableBaseComesFromPpuCtrlBit3(
        byte ppuCtrl,
        ushort expectedBase)
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, ppuCtrl);

        // Bit 3 selects the sprite table. Bit 4 selects the background table
        // and must not affect 8x8 sprite decoding.
        Assert.Equal(expectedBase, bus.Ppu.GetSpritePatternTableBase());
    }

    [Theory]
    [InlineData(0b0000_0000, 2, 0x0020)]
    [InlineData(0b0000_1000, 2, 0x1020)]
    public void SpriteTileAddressUsesSixteenBytesPerTile(
        byte ppuCtrl,
        byte tileNumber,
        ushort expectedAddress)
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, ppuCtrl);

        // Like background tiles, an 8x8 sprite tile has eight low-plane bytes
        // followed by eight high-plane bytes: 16 bytes total.
        Assert.Equal(expectedAddress, bus.Ppu.GetSpriteTileAddress(tileNumber));
    }

    [Fact]
    public void SpriteTilePixelCombinesBothBitplanes()
    {
        var bus = CreateBusWithChr();

        // Tile 2 begins at $0020. Setting bit 7 in both row-0 bitplanes makes
        // the tile's top-left pixel binary 11, or color index 3.
        bus.Ppu.WritePpuMemory(0x0020, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x0028, 0b1000_0000);

        Assert.Equal(3, bus.Ppu.GetSpriteTilePixelColorIndex(2, 0, 0));
    }

    [Fact]
    public void OamPositionPlacesSpriteTilePixelsAtScreenCoordinates()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WritePpuMemory(0x0020, 0b1000_0000);

        // Sprite 0 occupies OAM bytes 0-3: stored Y, tile, attributes, X.
        // Stored Y is one less than the first visible screen row.
        bus.Ppu.SetOamByte(0, 7);
        bus.Ppu.SetOamByte(1, 2);
        bus.Ppu.SetOamByte(2, 0);
        bus.Ppu.SetOamByte(3, 8);

        Assert.Equal(1, bus.Ppu.GetSpritePixelColorIndex(0, 8, 8));
        Assert.Equal(0, bus.Ppu.GetSpritePixelColorIndex(0, 7, 8));
    }

    [Fact]
    public void BackgroundOpacityForSpriteHitUsesPatternColorIndex()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WritePpuMemory(0x2000, 0x01);
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);

        // No palette value is required: sprite-zero hit cares that the CHR
        // index is nonzero, not which final color that index selects.
        Assert.Equal(1, bus.Ppu.GetBackgroundColorIndex(0, 0));
    }

    private static Bus CreateBusWithChr()
    {
        var bus = new Bus();
        bus.IO.CHRROM = new byte[8_192];
        return bus;
    }
}

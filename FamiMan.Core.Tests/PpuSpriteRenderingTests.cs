using Xunit;

namespace FamiMan.Core.Tests;

/// <summary>
/// Focused steps for turning OAM entries into sprite pixels and compositing
/// them with the existing background frame.
/// </summary>
public class PpuSpriteRenderingTests
{
    [Fact]
    public void OneOpaqueSpritePixelAppearsInTheCompositedFrame()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x10); // Show sprites.

        // Sprite tile 1, row 0, leftmost pixel has color index 1.
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F11, 0x21); // Sprite palette 0, color 1.

        // OAM stores Y minus one, tile number, attributes, then X.
        SetSprite(bus.Ppu, spriteIndex: 0, x: 20, y: 10, tile: 1, attributes: 0);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x21, frame[10 * 256 + 20]);
    }

    [Fact]
    public void CompositedFrameIncludesSpritesBeyondSpriteZero()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x10); // Show sprites.

        // Sprite tile 1 has one opaque pixel at its top-left corner.
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F11, 0x21); // Sprite palette 0, color 1.

        // Sprite 0 is reserved for special timing in games such as SMB. Mario
        // and other visible objects occupy later OAM entries, so the completed
        // frame must consider more than sprite 0.
        SetSprite(bus.Ppu, spriteIndex: 1, x: 20, y: 10, tile: 1, attributes: 0);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x21, frame[10 * 256 + 20]);
    }

    [Fact]
    public void LowerOamIndexWinsWhenSpritesOverlap()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x10); // Show sprites.

        // Tile 1 produces color index 1; tile 2 produces color index 2.
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x0028, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F11, 0x21);
        bus.Ppu.WritePpuMemory(0x3F12, 0x16);

        // Both sprites cover the same pixel. The NES gives the lower OAM
        // index priority, so sprite 0 must remain visible over sprite 1.
        SetSprite(bus.Ppu, spriteIndex: 0, x: 20, y: 10, tile: 1, attributes: 0);
        SetSprite(bus.Ppu, spriteIndex: 1, x: 20, y: 10, tile: 2, attributes: 0);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x21, frame[10 * 256 + 20]);
    }

    [Fact]
    public void SpriteAttributeBitsZeroAndOneSelectSpritePalette()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x10);
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F11, 0x11); // Palette 0, color 1.
        bus.Ppu.WritePpuMemory(0x3F19, 0x30); // Palette 2, color 1.
        SetSprite(bus.Ppu, 0, x: 20, y: 10, tile: 1, attributes: 0b0000_0010);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x30, frame[10 * 256 + 20]);
    }

    [Fact]
    public void HorizontalFlipMovesTheLeftmostTilePixelToTheRightEdge()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x10);
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F11, 0x21);
        bus.Ppu.WritePpuMemory(0x3F00, 0x0F);
        SetSprite(bus.Ppu, 0, x: 20, y: 10, tile: 1, attributes: 0b0100_0000);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x0F, frame[10 * 256 + 20]);
        Assert.Equal(0x21, frame[10 * 256 + 27]);
    }

    [Fact]
    public void VerticalFlipMovesTheTopTilePixelToTheBottomEdge()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x10);
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F11, 0x21);
        bus.Ppu.WritePpuMemory(0x3F00, 0x0F);
        SetSprite(bus.Ppu, 0, x: 20, y: 10, tile: 1, attributes: 0b1000_0000);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x0F, frame[10 * 256 + 20]);
        Assert.Equal(0x21, frame[17 * 256 + 20]);
    }

    [Fact]
    public void SpriteColorZeroIsTransparentAndLeavesBackgroundVisible()
    {
        var bus = CreateBusWithChr();
        // Show background and sprites, including both in the leftmost 8 pixels.
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x1E);

        // Background tile 1 is opaque at screen pixel (0, 16).
        bus.Ppu.WritePpuMemory(0x2040, 0x01);
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        // Sprite tile 2 has no pattern bits, so its pixel color is zero.
        SetSprite(bus.Ppu, 0, x: 0, y: 16, tile: 2, attributes: 0);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x21, frame[16 * 256]);
    }

    [Fact]
    public void SpritePriorityBitPlacesSpriteBehindOpaqueBackground()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x1E);

        bus.Ppu.WritePpuMemory(0x2040, 0x01);
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x0020, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21); // Background color.
        bus.Ppu.WritePpuMemory(0x3F11, 0x16); // Sprite color.

        // Attribute bit 5 means the sprite goes behind opaque background pixels.
        SetSprite(bus.Ppu, 0, x: 0, y: 16, tile: 2, attributes: 0b0010_0000);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x21, frame[16 * 256]);
    }

    [Fact]
    public void OpaqueSpriteZeroOverOpaqueBackgroundSetsSpriteZeroHit()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x1E);

        // $2021 is nametable tile row 1, column 1, whose top-left screen
        // coordinate is (8,8). It selects background tile 1.
        bus.Ppu.WritePpuMemory(0x2021, 0x01);

        // Bit 7 of each tile's first low-bitplane row makes its top-left pixel
        // color index 1: non-transparent for both background and sprite 0.
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);
        bus.Ppu.WritePpuMemory(0x0020, 0b1000_0000);
        SetSprite(bus.Ppu, 0, x: 8, y: 8, tile: 2, attributes: 0);

        // Visible cycle 1 is X=0, so scanline 8/cycle 9 reaches pixel (8,8).
        for (int i = 0; i < 8 * 341 + 9; i++)
            bus.Ppu.Tick();

        // PPUSTATUS bit 6 records the opaque sprite-0/background overlap.
        Assert.NotEqual(
            0,
            bus.Ppu.Register.Registers[PPURegister.PPUSTATUS_IDX] & 0x40);
    }

    [Fact]
    public void SpriteZeroHitUsesHorizontallyScrolledBackgroundPosition()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x1E);

        // Sprite 0 is on tile row 1 (screen Y=8). Column 0 stays transparent;
        // column 1 selects background tile 1, whose top-left pixel is opaque.
        // $2021 = $2000 + (tile row 1 * 32) + tile column 1.
        bus.Ppu.WritePpuMemory(0x2021, 0x01);
        bus.Ppu.WritePpuMemory(0x0010, 0b1000_0000);

        // Sprite 0 has an opaque top-left pixel at screen position (0, 8).
        bus.Ppu.WritePpuMemory(0x0020, 0b1000_0000);
        SetSprite(bus.Ppu, spriteIndex: 0, x: 0, y: 8, tile: 2, attributes: 0);

        // Scrolling eight pixels makes nametable column 1 appear at screen
        // X=0. Sprite-zero detection must use that same scrolled position when
        // deciding whether the background underneath sprite 0 is opaque.
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 8);
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0);

        // Visible cycle 1 is screen X=0, so reach pixel (0, 8).
        for (int i = 0; i < 8 * 341 + 1; i++)
            bus.Ppu.Tick();

        Assert.NotEqual(
            0,
            bus.Ppu.Register.Registers[PPURegister.PPUSTATUS_IDX] & 0x40);
    }

    private static void SetSprite(
        Ppu ppu,
        int spriteIndex,
        byte x,
        byte y,
        byte tile,
        byte attributes)
    {
        byte start = (byte)(spriteIndex * 4);
        ppu.SetOamByte(start, (byte)(y - 1));
        ppu.SetOamByte((byte)(start + 1), tile);
        ppu.SetOamByte((byte)(start + 2), attributes);
        ppu.SetOamByte((byte)(start + 3), x);
    }

    private static Bus CreateBusWithChr()
    {
        var bus = new Bus();
        bus.IO.CHRROM = new byte[8_192];
        return bus;
    }
}

using Xunit;

namespace FamiMan.Core.Tests;

/// <summary>
/// Visibility rules controlled by PPUMASK. Each test isolates one enable or
/// left-edge-clipping bit from sprite decoding and palette selection.
/// </summary>
public class PpuMaskRenderingTests
{
    [Fact]
    public void DisabledBackgroundUsesUniversalBackdropColor()
    {
        var bus = CreateBusWithChr();
        MakeOpaqueBackgroundPixel(bus.Ppu, x: 8, y: 8);
        bus.Ppu.WritePpuMemory(0x3F00, 0x0F);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x00);

        Assert.Equal(0x0F, bus.Ppu.RenderFrame()[8 * 256 + 8]);
    }

    [Fact]
    public void DisabledSpritesLeaveBackgroundVisible()
    {
        var bus = CreateBusWithChr();
        MakeOpaqueBackgroundPixel(bus.Ppu, x: 8, y: 8);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        MakeOpaqueSpriteTile(bus.Ppu, tile: 2);
        bus.Ppu.WritePpuMemory(0x3F11, 0x16);
        SetSprite(bus.Ppu, spriteIndex: 0, x: 8, y: 8, tile: 2);

        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x08); // Background only.

        Assert.Equal(0x21, bus.Ppu.RenderFrame()[8 * 256 + 8]);
    }

    [Fact]
    public void BackgroundLeftEdgeClippingHidesPixelsInFirstEightColumns()
    {
        var bus = CreateBusWithChr();
        MakeOpaqueBackgroundPixel(bus.Ppu, x: 0, y: 0);
        bus.Ppu.WritePpuMemory(0x3F00, 0x0F);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        // Bit 3 enables the background. Bit 1 remains clear, clipping its
        // pixels at screen X=0 through X=7.
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x08);

        Assert.Equal(0x0F, bus.Ppu.RenderFrame()[0]);
    }

    [Fact]
    public void SpriteLeftEdgeClippingHidesPixelsInFirstEightColumns()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WritePpuMemory(0x3F00, 0x0F);
        MakeOpaqueSpriteTile(bus.Ppu, tile: 2);
        bus.Ppu.WritePpuMemory(0x3F11, 0x21);
        SetSprite(bus.Ppu, spriteIndex: 0, x: 0, y: 1, tile: 2);

        // Bit 4 enables sprites. Bit 2 remains clear, clipping their pixels at
        // screen X=0 through X=7.
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x10);

        Assert.Equal(0x0F, bus.Ppu.RenderFrame()[1 * 256]);
    }

    private static void MakeOpaqueBackgroundPixel(Ppu ppu, byte x, byte y)
    {
        int tileColumn = x / 8;
        int tileRow = y / 8;
        ppu.WritePpuMemory(
            (ushort)(Ppu.NAMETABLE_START + tileRow * 32 + tileColumn),
            0x01);
        ppu.WritePpuMemory((ushort)(0x0010 + y % 8), (byte)(0x80 >> (x % 8)));
    }

    private static void MakeOpaqueSpriteTile(Ppu ppu, byte tile)
    {
        ppu.WritePpuMemory((ushort)(tile * 16), 0b1000_0000);
    }

    private static void SetSprite(Ppu ppu, int spriteIndex, byte x, byte y, byte tile)
    {
        byte start = (byte)(spriteIndex * 4);
        ppu.SetOamByte(start, (byte)(y - 1));
        ppu.SetOamByte((byte)(start + 1), tile);
        ppu.SetOamByte((byte)(start + 2), 0);
        ppu.SetOamByte((byte)(start + 3), x);
    }

    private static Bus CreateBusWithChr()
    {
        var bus = new Bus();
        bus.IO.CHRROM = new byte[8_192];
        return bus;
    }
}

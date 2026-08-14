using Xunit;

namespace FamiMan.Core.Tests;

public class PpuScrollingTests
{
    [Fact]
    public void HorizontalScrollChangesWhichBackgroundTileAppearsAtScreenOrigin()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WritePpuMemory(0x2000, 0x01); // Tile at world X 0.
        bus.Ppu.WritePpuMemory(0x2001, 0x02); // Tile at world X 8.
        MakeTileTopLeftPixelColorOne(bus.Ppu, tile: 2);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 8); // X scroll.
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0); // Y scroll.

        Assert.Equal(0x21, bus.Ppu.GetBackgroundPixel(0, 0));
    }

    [Fact]
    public void VerticalScrollChangesWhichBackgroundTileAppearsAtScreenOrigin()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.WritePpuMemory(0x2000, 0x01); // Tile at world Y 0.
        bus.Ppu.WritePpuMemory(0x2020, 0x02); // Tile at world Y 8.
        MakeTileTopLeftPixelColorOne(bus.Ppu, tile: 2);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0); // X scroll.
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 8); // Y scroll.

        Assert.Equal(0x21, bus.Ppu.GetBackgroundPixel(0, 0));
    }

    [Fact]
    public void HorizontalScrollCrossesIntoTheAdjacentNametable()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.Mirroring = NametableMirroring.Vertical;

        // With vertical mirroring, $2000 and $2400 are distinct physical
        // nametables and sit next to one another for horizontal scrolling.
        bus.Ppu.WritePpuMemory(0x2000, 0x01);
        bus.Ppu.WritePpuMemory(0x2400, 0x02);
        MakeTileTopLeftPixelColorOne(bus.Ppu, tile: 2);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 255);
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0);

        // Screen X 1 plus scroll 255 is world X 256: the first pixel of the
        // adjacent horizontal nametable.
        Assert.Equal(0x21, bus.Ppu.GetBackgroundPixel(1, 0));
    }

    [Fact]
    public void VerticalScrollCrossesIntoTheNametableBelow()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.Mirroring = NametableMirroring.Horizontal;

        // With horizontal mirroring, $2000 and $2800 are distinct physical
        // nametables and sit above and below one another for vertical scrolling.
        bus.Ppu.WritePpuMemory(0x2000, 0x01);
        bus.Ppu.WritePpuMemory(0x2800, 0x02);
        MakeTileTopLeftPixelColorOne(bus.Ppu, tile: 2);
        bus.Ppu.WritePpuMemory(0x3F01, 0x21);

        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0);
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 239);

        // Screen Y 1 plus scroll 239 is world Y 240: the first pixel of the
        // nametable immediately below the starting nametable.
        Assert.Equal(0x21, bus.Ppu.GetBackgroundPixel(0, 1));
    }

    private static void MakeTileTopLeftPixelColorOne(Ppu ppu, byte tile)
    {
        ppu.WritePpuMemory((ushort)(tile * 16), 0b1000_0000);
    }

    private static Bus CreateBusWithChr()
    {
        var bus = new Bus();
        bus.IO.CHRROM = new byte[8_192];
        return bus;
    }
}

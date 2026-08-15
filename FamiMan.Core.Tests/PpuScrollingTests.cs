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

    [Fact]
    public void BackgroundAttributePaletteFollowsHorizontalScrollIntoAdjacentNametable()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.Mirroring = NametableMirroring.Vertical;

        // The first tile in the right-hand nametable uses color index 1.
        bus.Ppu.WritePpuMemory(0x2400, 0x01);
        MakeTileTopLeftPixelColorOne(bus.Ppu, tile: 1);

        // $27C0 is the attribute table belonging to the nametable at $2400.
        // Its top-left quadrant selects background palette 2.
        bus.Ppu.WritePpuMemory(0x27C0, 0b0000_0010);
        bus.Ppu.WritePpuMemory(0x3F01, 0x11); // Palette 0, color 1.
        bus.Ppu.WritePpuMemory(0x3F09, 0x30); // Palette 2, color 1.

        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 255);
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0);

        // Screen X=1 lands at world X=256, so both the tile and its palette
        // attributes must come from the adjacent nametable.
        Assert.Equal(0x30, bus.Ppu.GetBackgroundPixel(1, 0));
    }

    [Fact]
    public void ScrollChangeDuringFrameOnlyAffectsFollowingScanlines()
    {
        var bus = CreateBusWithChr();
        // Show the background, including columns 0-7 where these assertions sample X=0.
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x0A);

        bus.Ppu.WritePpuMemory(0x2000, 0x01); // Tile visible before scrolling.
        bus.Ppu.WritePpuMemory(0x2001, 0x02); // Tile visible after scrolling 8px.

        // Tile 1 uses color index 1 and tile 2 uses color index 2 for every row.
        for (int row = 0; row < 8; row++)
        {
            bus.Ppu.WritePpuMemory((ushort)(0x0010 + row), 0xFF);
            bus.Ppu.WritePpuMemory((ushort)(0x0028 + row), 0xFF);
        }
        bus.Ppu.WritePpuMemory(0x3F01, 0x11);
        bus.Ppu.WritePpuMemory(0x3F02, 0x22);

        // Finish scanline 0 with scroll X=0, then change scroll before
        // scanline 1. A later write must not rewrite pixels already produced.
        for (int i = 0; i < 341; i++)
            bus.Ppu.Tick();

        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 8);
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0);

        byte[] frame = bus.Ppu.RenderFrame();

        Assert.Equal(0x11, frame[0 * 256]);
        Assert.Equal(0x22, frame[1 * 256]);
    }

    [Fact]
    public void NametableChangeDuringFrameOnlyAffectsFollowingScanlines()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.Mirroring = NametableMirroring.Vertical;
        // Show the background, including columns 0-7 where these assertions sample X=0.
        bus.Ppu.WriteCpuRegister(Ppu.PPUMASK_ADDR, 0x0A);

        // Vertical mirroring keeps logical nametables 0 and 1 in distinct RAM.
        // Their top-left entries select tiles with different color indices.
        bus.Ppu.WritePpuMemory(0x2000, 0x01); // Nametable 0 uses tile 1.
        bus.Ppu.WritePpuMemory(0x2400, 0x02); // Nametable 1 uses tile 2.

        // Tile 1 produces color index 1 and tile 2 produces color index 2 on
        // every row, so scanline Y does not affect which color is expected.
        for (int row = 0; row < 8; row++)
        {
            bus.Ppu.WritePpuMemory((ushort)(0x0010 + row), 0xFF);
            bus.Ppu.WritePpuMemory((ushort)(0x0028 + row), 0xFF);
        }
        bus.Ppu.WritePpuMemory(0x3F01, 0x11); // Color index 1.
        bus.Ppu.WritePpuMemory(0x3F02, 0x22); // Color index 2.

        // Scanline 0 begins while nametable 0 is active.
        bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, 0x00);
        for (int i = 0; i < 341; i++)
            bus.Ppu.Tick();

        // Switch to nametable 1 before scanline 1 begins and captures its
        // background state. This must not retroactively alter scanline 0.
        bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, 0x01);
        bus.Ppu.Tick();

        byte[] frame = bus.Ppu.RenderBackgroundFrame();

        Assert.Equal(0x11, frame[0 * 256]);
        Assert.Equal(0x22, frame[1 * 256]);
    }

    [Fact]
    public void SecondPpuAddrWriteChangesBackgroundCoarseXOrigin()
    {
        var bus = CreateBusWithChr();

        // With coarse X=0, screen X=8 reads nametable column 1 and tile 1.
        bus.Ppu.WritePpuMemory(0x2001, 0x01);

        // $2000 also produces fine Y=2 when interpreted as the PPU's active
        // rendering address. Make every row opaque so this test isolates only
        // the horizontal/coarse-X part of that address.
        for (int row = 0; row < 8; row++)
            bus.Ppu.WritePpuMemory((ushort)(0x0010 + row), 0b1000_0000);

        // Model the previous playfield state as one tile of horizontal scroll.
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 8);
        bus.Ppu.WriteCpuRegister(Ppu.PPUSCROLL_ADDR, 0);

        // The second PPUADDR write copies $2000 into the active VRAM address.
        // Its coarse-X field is zero, so rendering should no longer use the
        // previous one-tile horizontal offset.
        bus.Ppu.WriteCpuRegister(Ppu.PPUADDR_ADDR, 0x20);
        bus.Ppu.WriteCpuRegister(Ppu.PPUADDR_ADDR, 0x00);

        Assert.Equal(1, bus.Ppu.GetBackgroundColorIndex(8, 0));
    }

    [Fact]
    public void SecondPpuAddrWriteChangesBackgroundNametableOrigin()
    {
        var bus = CreateBusWithChr();
        bus.Ppu.Mirroring = NametableMirroring.Vertical;

        // PPUCTRL currently selects logical nametable 1 ($2400).
        bus.Ppu.WriteCpuRegister(Ppu.PPUCTRL_ADDR, 0x01);

        // Only logical nametable 0 ($2000) contains opaque background tile 1.
        // Nametable 1 remains transparent, making the selected origin visible.
        bus.Ppu.WritePpuMemory(0x2000, 0x01);
        for (int row = 0; row < 8; row++)
            bus.Ppu.WritePpuMemory((ushort)(0x0010 + row), 0b1000_0000);

        // A complete PPUADDR write copies $2000 into V. Its nametable field is
        // zero, so background lookup should now begin in logical nametable 0
        // even though the stored PPUCTRL value still has nametable bits 01.
        bus.Ppu.WriteCpuRegister(Ppu.PPUADDR_ADDR, 0x20);
        bus.Ppu.WriteCpuRegister(Ppu.PPUADDR_ADDR, 0x00);

        Assert.Equal(1, bus.Ppu.GetBackgroundColorIndex(0, 0));
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

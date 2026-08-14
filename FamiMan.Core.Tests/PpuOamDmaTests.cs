using Xunit;

namespace FamiMan.Core.Tests;

public class PpuOamDmaTests
{
    [Fact]
    public void OamDmaCopiesOneCpuMemoryPageIntoSpriteMemory()
    {
        var bus = new Bus();

        // $4014 receives the high byte of a CPU address. Writing $02 means
        // copy all 256 bytes from CPU addresses $0200-$02FF into PPU OAM.
        for (int i = 0; i < 256; i++)
            bus.Write((ushort)(0x0200 + i), (byte)(i ^ 0x5A));

        bus.Write(0x4014, 0x02);

        Assert.Equal(0x5A, bus.Ppu.ReadOamByte(0x00));
        Assert.Equal((byte)(0x7F ^ 0x5A), bus.Ppu.ReadOamByte(0x7F));
        Assert.Equal((byte)(0xFF ^ 0x5A), bus.Ppu.ReadOamByte(0xFF));
    }
}

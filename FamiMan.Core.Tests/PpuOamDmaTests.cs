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

    [Fact]
    public void OamDmaStallsCpuForAtLeast513Cycles()
    {
        var bus = new Bus();
        bus.Cpu.PC = 0x0200;

        // Fill RAM with NOPs so an unstalled CPU can safely keep executing and
        // visibly move PC instead of reaching an unrelated BRK instruction.
        bus.Ram.AsSpan().Fill(0xEA);

        bus.Write(0x4014, 0x04);

        // OAM DMA owns the CPU bus for 513 or 514 CPU cycles. The PPU continues
        // running, but the CPU cannot complete an instruction during this time.
        for (int i = 0; i < 513; i++)
            bus.Clock();

        Assert.Equal(0x0200, bus.Cpu.PC);
    }
}

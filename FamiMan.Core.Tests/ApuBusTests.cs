using Xunit;
using static FamiMan.Core.Opcodes;

namespace FamiMan.Core.Tests;

/// <summary>
/// First wiring steps for connecting the CPU-visible APU registers and clock.
/// Channel behavior belongs in later, smaller tests.
/// </summary>
public class ApuBusTests
{
    [Fact]
    public void CpuBusRoutes4000WriteToApu()
    {
        var bus = new Bus();
        var apu = new RecordingApu(bus);
        bus.Apu = apu;

        // $4000 is pulse channel 1's duty/envelope/volume control register.
        bus.Write(0x4000, 0b1011_0101);

        Assert.Equal((Address: (ushort)0x4000, Value: (byte)0b1011_0101), apu.LastWrite);
    }

    [Fact]
    public void CpuBusReadOf4015ReturnsApuStatus()
    {
        var bus = new Bus();
        var apu = new RecordingApu(bus)
        {
            Status = 0b0000_0001
        };
        bus.Apu = apu;

        // On a real APU, bit 0 reports whether pulse channel 1's length
        // counter is nonzero. This test only checks that the bus asks the APU.
        Assert.Equal(0b0000_0001, bus.Read(0x4015));
    }

    [Fact]
    public void BusClocksApuOncePerCpuCycle()
    {
        var bus = new Bus();
        var apu = new RecordingApu(bus);
        bus.Apu = apu;

        // Give the CPU a harmless instruction while one system clock elapses.
        bus.Cpu.PC = 0x0200;
        bus.Write(0x0200, NOP.NOP_EA.Opcode);

        bus.Clock();

        Assert.Equal(1, apu.TickCalls);
    }

    private sealed class RecordingApu : Apu
    {
        public RecordingApu(Bus bus) : base(bus)
        {
        }

        public (ushort Address, byte Value)? LastWrite { get; private set; }
        public byte Status { get; init; }
        public int TickCalls { get; private set; }

        public override void WriteRegister(ushort address, byte value)
        {
            LastWrite = (address, value);
        }

        public override byte ReadStatus()
        {
            return Status;
        }

        public override void Tick()
        {
            TickCalls++;
        }
    }
}

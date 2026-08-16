using Xunit;

namespace FamiMan.Core.Tests;

/// <summary>
/// First behavioral steps for pulse channel 1. These tests deliberately stop
/// before timer, duty-cycle, envelope, and audio-sample generation.
/// </summary>
public class ApuPulseChannelTests
{
    private const ushort Pulse1TimerHighAndLengthAddress = 0x4003;
    private const ushort StatusAddress = 0x4015;
    private const byte Pulse1StatusBit = 0x01;

    [Fact]
    public void NewApuReportsNoActiveChannels()
    {
        var apu = CreateApu();

        Assert.Equal(0, apu.ReadStatus() & Pulse1StatusBit);
    }

    [Fact]
    public void EnabledPulseOneLengthLoadSetsItsStatusBit()
    {
        var apu = CreateApu();

        // $4015 bit 0 allows pulse channel 1's length counter to be loaded.
        // Enabling the channel alone does not make it active: reads of $4015
        // report a nonzero length counter, not the enable latch itself.
        apu.WriteRegister(StatusAddress, Pulse1StatusBit);
        Assert.Equal(0, apu.ReadStatus() & Pulse1StatusBit);

        // Writing $4003 loads a length value selected by bits 7-3. The exact
        // value is tested later; here we only care that it is now nonzero.
        apu.WriteRegister(Pulse1TimerHighAndLengthAddress, 0x00);

        Assert.NotEqual(0, apu.ReadStatus() & Pulse1StatusBit);
    }

    [Fact]
    public void DisabledPulseOneIgnoresLengthLoad()
    {
        var apu = CreateApu();

        // Pulse 1 starts disabled. A $4003 write must therefore leave its
        // length counter at zero rather than remembering the load.
        apu.WriteRegister(Pulse1TimerHighAndLengthAddress, 0x00);

        // Enabling it afterward must not resurrect the ignored length load.
        apu.WriteRegister(StatusAddress, Pulse1StatusBit);

        Assert.Equal(0, apu.ReadStatus() & Pulse1StatusBit);
    }

    [Fact]
    public void DisablingPulseOneClearsItsStatusBit()
    {
        var apu = CreateApu();
        apu.WriteRegister(StatusAddress, Pulse1StatusBit);

        // The enable latch itself must not appear in readable status.
        Assert.Equal(0, apu.ReadStatus() & Pulse1StatusBit);

        apu.WriteRegister(Pulse1TimerHighAndLengthAddress, 0x00);
        Assert.NotEqual(0, apu.ReadStatus() & Pulse1StatusBit);

        // Clearing $4015 bit 0 immediately clears pulse 1's length counter.
        apu.WriteRegister(StatusAddress, 0x00);

        Assert.Equal(0, apu.ReadStatus() & Pulse1StatusBit);
    }

    private static Apu CreateApu()
    {
        var bus = new Bus();
        return bus.Apu;
    }
}

using Xunit;

namespace FamiMan.Core.Tests;

/// <summary>
/// NES controller behavior needed for Super Mario Bros. to observe Start and
/// directional input. SDL key mapping is deliberately kept out of these tests.
/// </summary>
public class ControllerInputTests
{
    [Fact]
    public void ControllerReturnsLatchedButtonsInNesSerialOrder()
    {
        var bus = new Bus();
        bus.Controller1.SetButton(ControllerButton.A, true);
        bus.Controller1.SetButton(ControllerButton.B, false);
        bus.Controller1.SetButton(ControllerButton.Select, true);
        bus.Controller1.SetButton(ControllerButton.Start, false);
        bus.Controller1.SetButton(ControllerButton.Up, true);
        bus.Controller1.SetButton(ControllerButton.Down, false);
        bus.Controller1.SetButton(ControllerButton.Left, true);
        bus.Controller1.SetButton(ControllerButton.Right, false);

        // A 1-to-0 transition on the strobe snapshots the eight buttons.
        bus.Write(0x4016, 1);
        bus.Write(0x4016, 0);

        byte[] actual = new byte[8];
        for (int i = 0; i < actual.Length; i++)
            actual[i] = (byte)(bus.Read(0x4016) & 1);

        Assert.Equal(new byte[] { 1, 0, 1, 0, 1, 0, 1, 0 }, actual);
    }

    [Fact]
    public void ControllerStrobeLatchesButtonsUntilTheNextStrobe()
    {
        var bus = new Bus();
        bus.Controller1.SetButton(ControllerButton.A, true);

        bus.Write(0x4016, 1);
        bus.Write(0x4016, 0);

        // Host input changes after the latch, but this serial packet must
        // continue reporting the old snapshot.
        bus.Controller1.SetButton(ControllerButton.A, false);
        Assert.Equal(1, bus.Read(0x4016) & 1);

        // A new strobe takes a new snapshot.
        bus.Write(0x4016, 1);
        bus.Write(0x4016, 0);
        Assert.Equal(0, bus.Read(0x4016) & 1);
    }
}

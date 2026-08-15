using System;

namespace FamiMan.Core;

/// <summary>
/// Emulates the NES audio processing unit. The implementation is intentionally
/// left empty for now so its behavior can be added through focused tests.
/// </summary>
public class Apu
{
    protected readonly Bus Bus;

    public Apu(Bus bus)
    {
        Bus = bus;
    }

    /// <summary>
    /// Handles a CPU write to an APU register in $4000-$4013, $4015, or $4017.
    /// </summary>
    public virtual void WriteRegister(ushort address, byte value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads APU channel and interrupt status as exposed through $4015.
    /// </summary>
    public virtual byte ReadStatus()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Advances the APU by one CPU clock.
    /// </summary>
    public virtual void Tick()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns audio samples generated since the previous call.
    /// </summary>
    public virtual float[] TakePendingSamples()
    {
        throw new NotImplementedException();
    }
}

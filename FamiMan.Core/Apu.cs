using FamiMan.Core.Exceptions;
using System;

namespace FamiMan.Core;

/// <summary>
/// Emulates the NES audio processing unit. The implementation is intentionally
/// left empty for now so its behavior can be added through focused tests.
/// </summary>
public class Apu
{
    private static readonly byte[] LengthTable =
    [
            10, 254,  20,   2,  40,   4,  80,   6,
        160,   8,  60,  10,  14,  12,  26,  14,
            12,  16,  24,  18,  48,  20,  96,  22,
        192,  24,  72,  26,  16,  28,  32,  30
    ];

    protected readonly Bus Bus;
    private long _ticks = 0;

    private byte _frameCounter = 0;
    private byte[] _channelData = new byte[0x14];

    private bool[] _pulseEnabled = new bool[2];
    private int[] _pulseLengthCounter = new int[2];

    private bool _triangleEnabled = false;
    private int _triangleLengthCounter = 0;
    private bool _noiseEnabled = false;
    private int _noiseLengthCounter = 0;
    private bool _dmcEnabled = false;
    private int _dmcBytesRemaining = 0;
    private byte _status;

    public Apu(Bus bus)
    {
        Bus = bus;
    }

    /// <summary>
    /// Handles a CPU write to an APU register in $4000-$4013, $4015, or $4017.
    /// </summary>
    public virtual void WriteRegister(ushort address, byte value)
    {
        if (address == 0x4003)
        {
            if (_pulseEnabled[0])
            {
                int lengthIndex = (value >> 3) & 0x1F;
                _pulseLengthCounter[0] = LengthTable[lengthIndex];
            }
        }
        else if (address == 0x4007)
        {
            if (_pulseEnabled[1])
            {
                int lengthIndex = (value >> 3) & 0x1F;
                _pulseLengthCounter[1] = LengthTable[lengthIndex];
            }
        }
        else if (address == 0x4015)
        {
            _pulseEnabled[0] = (value & 0x01) != 0;
            if (!_pulseEnabled[0])
                _pulseLengthCounter[0] = 0;
            _pulseEnabled[1] = (value & 0x02) != 0;
            if (!_pulseEnabled[0])
                _pulseLengthCounter[1] = 0;
            _triangleEnabled = (value & 0x04) != 0;
            _noiseEnabled = (value & 0x08) != 0;
            _dmcEnabled = (value & 0x10) != 0;
            _status = value;
        }
        else if (address == 0x4017)
        {
            _frameCounter = value;
        }
        else if (address >= 0x4000 && address <= 0x4013)
        {
            _channelData[address - 0x4000] = value;
        }
        else
            throw new ApuException("Invalid address range");
    }

    /// <summary>
    /// Reads APU channel and interrupt status as exposed through $4015.
    /// </summary>
    public virtual byte ReadStatus()
    {
        byte status = 0;

        if (_pulseLengthCounter[0] > 0)
            status |= 0x01;

        if (_pulseLengthCounter[1] > 0)
            status |= 0x02;

        if (_triangleLengthCounter > 0)
            status |= 0x04;

        if (_noiseLengthCounter > 0)
            status |= 0x08;

        if (_dmcBytesRemaining > 0)
            status |= 0x10;

        return status;
    }

    /// <summary>
    /// Advances the APU by one CPU clock.
    /// </summary>
    public virtual void Tick()
    {
        _ticks++;
    }

    /// <summary>
    /// Returns audio samples generated since the previous call.
    /// </summary>
    public virtual float[] TakePendingSamples()
    {
        throw new NotImplementedException();
    }
}

using System;

namespace FamiMan.Core;

/// <summary>
/// Host-visible state for one NES controller. The bus is responsible for
/// exposing this state through the NES serial protocol at $4016/$4017.
/// </summary>
public sealed class NesController
{
    private readonly bool[] _pressed = new bool[8];
    private bool[] _latchedButtons;

    private int _latchIdx;

    public void SetButton(ControllerButton button, bool pressed)
    {
        _pressed[(int)button] = pressed;
    }

    public bool IsPressed(ControllerButton button)
    {
        return _pressed[(int)button];
    }

    public void Latch()
    {
        bool[] snapshot = new bool[8];
        Array.Copy(_pressed, snapshot, 8);
        _latchedButtons = snapshot;
        _latchIdx = 0;
    }

    public byte Read()
    {
        if (_latchedButtons == null)
            return 0;
        if (_latchIdx >= _latchedButtons.Length)
            return 1; // NES returns 1 after all buttons have been read.
        bool pressed = _latchedButtons[_latchIdx];
        _latchIdx++;
        return (byte)(pressed ? 1 : 0);
    }
}

public enum ControllerButton
{
    A,
    B,
    Select,
    Start,
    Up,
    Down,
    Left,
    Right
}

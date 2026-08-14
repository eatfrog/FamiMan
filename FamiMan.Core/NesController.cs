namespace FamiMan.Core;

/// <summary>
/// Host-visible state for one NES controller. The bus is responsible for
/// exposing this state through the NES serial protocol at $4016/$4017.
/// </summary>
public sealed class NesController
{
    private readonly bool[] _pressed = new bool[8];

    public void SetButton(ControllerButton button, bool pressed)
    {
        _pressed[(int)button] = pressed;
    }

    public bool IsPressed(ControllerButton button)
    {
        return _pressed[(int)button];
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

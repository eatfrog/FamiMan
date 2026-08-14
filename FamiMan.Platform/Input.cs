namespace FamiMan.Platform;

public enum Key
{
    Unknown,
    D,
    Q,
    Escape,
    Up,
    Down,
    Left,
    Right,
    Z,
    X,
    Enter,
    RightShift
}

public enum WindowEventType
{
    None,
    Quit,
    KeyDown,
    KeyUp
}

public readonly struct WindowEvent
{
    internal WindowEvent(WindowEventType type, Key key = Key.Unknown)
    {
        Type = type;
        Key = key;
    }

    public WindowEventType Type { get; }
    public Key Key { get; }
}
